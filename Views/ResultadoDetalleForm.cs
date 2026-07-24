using PC7866.Models;
using System.Text;

namespace PC7866.Views;

/// <summary>
/// Ventana modal que muestra los detalles paso a paso de un resultado de ensayo.
/// </summary>
public sealed class ResultadoDetalleForm : Form
{
    private readonly int                    _resultadoId;
    private readonly List<ResultadoDetalle> _detalles;

    public ResultadoDetalleForm(int resultadoId, List<ResultadoDetalle> detalles)
    {
        _resultadoId = resultadoId;
        _detalles    = detalles;
        BuildUi();
    }

    private void BuildUi()
    {
        Text            = $"Detalle de ensayo – ID {_resultadoId}";
        Size            = new Size(860, 520);
        StartPosition   = FormStartPosition.CenterParent;
        MinimumSize     = new Size(700, 400);
        FormBorderStyle = FormBorderStyle.Sizable;

        var grid = new DataGridView
        {
            Dock                  = DockStyle.Fill,
            AllowUserToAddRows    = false,
            AllowUserToDeleteRows = false,
            ReadOnly              = true,
            SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
            RowHeadersVisible     = false,
            AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill,
            Font                  = new Font("Consolas", 9f)
        };

        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Paso",       FillWeight = 8  });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Contacto",   FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "R medida (Ω)", FillWeight = 18 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RAW Vain",   FillWeight = 14 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "RAW Ve",     FillWeight = 14 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Resultado",  FillWeight = 14 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "Timestamp",  FillWeight = 20 });

        int ok = 0;
        foreach (var d in _detalles)
        {
            string res = d.Estado switch
            {
                EstadoMedicion.Ok            => "✅ OK",
                EstadoMedicion.Cortocircuito => "⚡ CORTOCIRCUITO",
                EstadoMedicion.Abierto       => "🔵 ABIERTO",
                _                             => "❌ NOK"
            };
            string rStr = d.ResistenciaMedida < 0 ? "∞" : $"{d.ResistenciaMedida:F3}";
            int idx = grid.Rows.Add(
                d.NPasoEnsayo,
                d.NombreContacto,
                rStr,
                d.ValorRawVain,
                d.ValorRawVe,
                res,
                d.Timestamp.ToString("HH:mm:ss.fff"));

            grid.Rows[idx].DefaultCellStyle.BackColor = d.Estado switch
            {
                EstadoMedicion.Ok            => Color.FromArgb(220, 255, 220),
                EstadoMedicion.Cortocircuito => Color.FromArgb(255, 235, 200),
                EstadoMedicion.Abierto       => Color.FromArgb(215, 230, 255),
                _                             => Color.FromArgb(255, 220, 220)
            };
            if (d.Resultado) ok++;
        }

        // Barra inferior
        var pnl = new Panel { Dock = DockStyle.Bottom, Height = 44,
                              BackColor = Color.FromArgb(235, 238, 245) };

        var lblResumen = new Label
        {
            AutoSize = true,
            Location = new Point(10, 14),
            Font     = new Font("Segoe UI", 9.5f, FontStyle.Bold),
            Text     = $"Pasos OK: {ok} / {_detalles.Count}   " +
                       (_detalles.All(d => d.Resultado) ? "✅ BUENO" : "❌ MALO")
        };

        var btnCsv = new Button
        {
            Text      = "💾 Exportar CSV",
            Location  = new Point(600, 8),
            Size      = new Size(140, 28),
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 153, 76),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnCsv.Click += (_, _) => ExportCsv();

        var btnCerrar = new Button
        {
            Text     = "Cerrar",
            Location = new Point(748, 8),
            Size     = new Size(80, 28),
            Font     = new Font("Segoe UI", 9f)
        };
        btnCerrar.Click += (_, _) => Close();

        pnl.Controls.AddRange(new Control[] { lblResumen, btnCsv, btnCerrar });

        Controls.Add(grid);
        Controls.Add(pnl);
    }

    private void ExportCsv()
    {
        using var dlg = new SaveFileDialog
        {
            Filter   = "CSV|*.csv",
            FileName = $"Detalle_{_resultadoId}_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        var sb = new StringBuilder();
        sb.AppendLine("Paso;Contacto;R_medida_Ohm;RAW_Vain;RAW_Ve;Resultado;Timestamp");
        foreach (var d in _detalles)
        {
            string rValue = d.ResistenciaMedida < 0 ? "∞" : d.ResistenciaMedida.ToString("F3", System.Globalization.CultureInfo.InvariantCulture);
            sb.AppendLine(string.Join(";",
                d.NPasoEnsayo,
                d.NombreContacto,
                rValue,
                d.ValorRawVain,
                d.ValorRawVe,
                d.Resultado ? "OK" : "NOK",
                d.Timestamp.ToString("dd/MM/yyyy HH:mm:ss.fff")));
        }

        File.WriteAllText(dlg.FileName, sb.ToString(), System.Text.Encoding.UTF8);
        MessageBox.Show($"Exportado:\n{dlg.FileName}", "OK",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}
