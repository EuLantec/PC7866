using PC7866.Configuration;
using PC7866.Models;
using PC7866.Services.Database;
using System.Text;

namespace PC7866.Views;

/// <summary>
/// Panel de informes. Permite explorar resultados históricos con filtros
/// y exportarlos a CSV.
/// </summary>
public partial class ReportsPanel : UserControl
{
    private ITestRepository?  _repository;
    private List<Resultado>   _todos    = new();
    private List<Referencia>  _refsList = new();

    public ReportsPanel()
    {
        InitializeComponent();
        AttachEventHandlers();
        _ = TryInitAsync();
    }

    private void AttachEventHandlers()
    {
        btnBuscar.Click     += async (_, _) => await FilterAsync();
        btnExportCsv.Click  += BtnExportCsv_Click;
        btnVerDetalle.Click += BtnVerDetalle_Click;
        btnRefrescar.Click  += async (_, _) => await LoadResultadosAsync();

        dtpDesde.Value = DateTime.Today.AddMonths(-1);
        dtpHasta.Value = DateTime.Today.AddDays(1);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private async Task TryInitAsync()
    {
        try
        {
            _repository = new TestRepository(AppSettings.Instance.GetConnectionString());
            await _repository.TestConnectionAsync();
            await _repository.InitializeDatabaseAsync();
            await LoadReferenciasAsync();
            await LoadResultadosAsync();
        }
        catch { /* Sin BD – grid vacío */ }
    }

    private async Task LoadReferenciasAsync()
    {
        if (_repository is null) return;
        _refsList = (await _repository.GetAllReferenciasAsync()).ToList();
        cmbFiltroRef.Items.Clear();
        cmbFiltroRef.Items.Add("(Todas)");
        foreach (var r in _refsList) cmbFiltroRef.Items.Add(r);
        cmbFiltroRef.DisplayMember = "ReferenciaNombre";
        cmbFiltroRef.SelectedIndex = 0;
    }

    private async Task LoadResultadosAsync()
    {
        if (_repository is null) return;
        _todos = (await _repository.GetAllResultadosAsync()).ToList();
        await FilterAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Filtrado
    // ─────────────────────────────────────────────────────────────────────────

    private async Task FilterAsync()
    {
        var filtrados = _todos.AsEnumerable();

        if (cmbFiltroRef.SelectedItem is Referencia refFiltro)
            filtrados = filtrados.Where(r => r.ReferenciaId == refFiltro.Id);

        filtrados = filtrados.Where(r =>
            r.FechaPrueba >= dtpDesde.Value && r.FechaPrueba <= dtpHasta.Value);

        if (cmbFiltroResultado.SelectedIndex == 1)
            filtrados = filtrados.Where(r => r.ResultadoGlobal);
        else if (cmbFiltroResultado.SelectedIndex == 2)
            filtrados = filtrados.Where(r => !r.ResultadoGlobal);

        string op = txtFiltroOperario.Text.Trim();
        if (!string.IsNullOrEmpty(op))
            filtrados = filtrados.Where(r =>
                r.Operario.Contains(op, StringComparison.OrdinalIgnoreCase));

        string lote = txtFiltroLote.Text.Trim();
        if (!string.IsNullOrEmpty(lote))
            filtrados = filtrados.Where(r =>
                r.Lote.Contains(lote, StringComparison.OrdinalIgnoreCase));

        var lista = filtrados.OrderByDescending(r => r.FechaPrueba).ToList();
        PopulateGrid(lista);
        lblTotal.Text = $"Total: {lista.Count} resultados";
        await Task.CompletedTask;
    }

    private void PopulateGrid(List<Resultado> lista)
    {
        gridResultados.Rows.Clear();
        foreach (var r in lista)
        {
            string refNombre = _refsList
                .FirstOrDefault(x => x.Id == r.ReferenciaId)?.ReferenciaNombre
                ?? (r.ReferenciaId.HasValue ? r.ReferenciaId.ToString()! : "Manual");
            string resStr = r.ResultadoGlobal ? "✅ BUENO" : "❌ MALO";

            int idx = gridResultados.Rows.Add(
                r.Id,
                r.FechaPrueba.ToString("dd/MM/yyyy HH:mm:ss"),
                refNombre,
                r.Operario,
                r.Lote,
                resStr);

            gridResultados.Rows[idx].DefaultCellStyle.BackColor = r.ResultadoGlobal
                ? Color.FromArgb(220, 255, 220)
                : Color.FromArgb(255, 220, 220);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Exportación CSV
    // ─────────────────────────────────────────────────────────────────────────

    private void BtnExportCsv_Click(object? sender, EventArgs e)
    {
        if (gridResultados.Rows.Count == 0)
        {
            MessageBox.Show("No hay datos para exportar.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter   = "CSV|*.csv",
            FileName = $"Resultados_{DateTime.Now:yyyyMMdd_HHmmss}.csv",
            Title    = "Guardar CSV"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("ID;Fecha;Referencia;Operario;Lote;Resultado");
        foreach (DataGridViewRow row in gridResultados.Rows)
        {
            if (row.IsNewRow) continue;
            sb.AppendLine(string.Join(";",
                row.Cells[0].Value, row.Cells[1].Value,
                row.Cells[2].Value, row.Cells[3].Value,
                row.Cells[4].Value, row.Cells[5].Value));
        }

        File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
        MessageBox.Show($"Exportado:\n{dlg.FileName}", "OK",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Ver detalle
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnVerDetalle_Click(object? sender, EventArgs e)
    {
        if (_repository is null || gridResultados.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(gridResultados.SelectedRows[0].Cells[0].Value);
        var detalles = (await _repository.GetDetallesByResultadoAsync(id)).ToList();

        if (detalles.Count == 0)
        {
            MessageBox.Show("No hay detalles para este resultado.", "Información",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var form = new ResultadoDetalleForm(id, detalles);
        form.ShowDialog(this);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _repository?.Dispose();
        base.OnHandleDestroyed(e);
    }

    public void RefreshData() => _ = LoadResultadosAsync();
}
