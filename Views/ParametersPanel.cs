using PC7866.Configuration;
using PC7866.Models;
using PC7866.Services.Database;
using PC7866.Utils;

namespace PC7866.Views;

/// <summary>
/// Panel de gestión de Referencias y ParametrosEnsayo.
/// Permite crear, editar y eliminar referencias y sus parámetros de ensayo.
/// </summary>
public partial class ParametersPanel : UserControl
{
    private ITestRepository? _repository;
    private Referencia?      _referenciaActual;

    // Indica si la fila seleccionada es una fila nueva (fantasma) aún no guardada en BD.
    private bool _modoNuevoParam = false;

    // ── Arrastre del punto sobre picPreview ───────────────────────────────
    private bool _arrastrando   = false;
    private bool _ignorarNudEvents = false;

    public ParametersPanel()
    {
        InitializeComponent();
        AttachEventHandlers();
        _ = TryInitAsync();
    }

    private void AttachEventHandlers()
    {
        btnNuevaRef.Click                    += BtnNuevaRef_Click;
        btnGuardarRef.Click                  += BtnGuardarRef_Click;
        btnEliminarRef.Click                 += BtnEliminarRef_Click;
        btnCargarImagen.Click                += BtnCargarImagen_Click;
        listReferencias.SelectedIndexChanged += ListReferencias_SelectedIndexChanged;

        btnNuevoParam.Click                  += BtnNuevoParam_Click;
        btnGuardarParam.Click                += BtnGuardarParam_Click;
        btnEliminarParam.Click               += BtnEliminarParam_Click;
        btnExportarParam.Click               += BtnExportarParam_Click;
        btnImportarParam.Click               += BtnImportarParam_Click;
        gridParametros.SelectionChanged      += GridParametros_SelectionChanged;

        // Punto sobre imagen
        picPreview.Paint      += PicPreview_Paint;
        picPreview.MouseDown  += PicPreview_MouseDown;
        picPreview.MouseMove  += PicPreview_MouseMove;
        picPreview.MouseUp    += PicPreview_MouseUp;

        // Redibujar al cambiar las coordenadas manualmente
        nudPosX.ValueChanged += NudPos_ValueChanged;
        nudPosY.ValueChanged += NudPos_ValueChanged;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inicialización BD
    // ─────────────────────────────────────────────────────────────────────────

    private async Task TryInitAsync()
    {
        try
        {
            _repository = new TestRepository(AppSettings.Instance.GetConnectionString());
            if (await _repository.TestConnectionAsync())
            {
                await _repository.InitializeDatabaseAsync();
                await LoadReferenciasAsync();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error BD: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Referencias
    // ─────────────────────────────────────────────────────────────────────────

    private async Task LoadReferenciasAsync()
    {
        if (_repository is null) return;
        var refs = (await _repository.GetAllReferenciasAsync()).ToList();
        listReferencias.Items.Clear();
        foreach (var r in refs) listReferencias.Items.Add(r);
        listReferencias.DisplayMember = "ReferenciaNombre";
    }

    private async void ListReferencias_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (listReferencias.SelectedItem is not Referencia r) return;
        _referenciaActual = r;
        txtRefNombre.Text = r.ReferenciaNombre;
        txtRefDesc.Text   = r.Descripcion;
        txtRefModelo.Text = r.ModeloPlaca;
        chkActiva.Checked = r.BActiva;
        nudRefNumMcps.Value  = Math.Clamp(r.NumMcps, (int)nudRefNumMcps.Minimum, (int)nudRefNumMcps.Maximum);
        nudRefMuestras.Value = Math.Clamp(r.Muestras, (int)nudRefMuestras.Minimum, (int)nudRefMuestras.Maximum);
        nudRefRetardo.Value  = Math.Clamp(r.RetardoMs, (int)nudRefRetardo.Minimum, (int)nudRefRetardo.Maximum);
        txtRefInh1.Text = FormatInh(r.Inh1Pos);
        txtRefInh2.Text = FormatInh(r.Inh2Pos);
        txtRefInh3.Text = FormatInh(r.Inh3Pos);
        txtRefInh4.Text = FormatInh(r.Inh4Pos);

        if (r.Imagen?.Length > 0)
        {
            using var ms = new MemoryStream(r.Imagen);
            picPreview.Image = Image.FromStream(ms);
        }
        else picPreview.Image = null;

        await LoadParametrosAsync(r.Id);
    }

    private async Task LoadParametrosAsync(int referenciaId)
    {
        if (_repository is null) return;

        _modoNuevoParam = false;

        gridParametros.SelectionChanged -= GridParametros_SelectionChanged;
        gridParametros.Rows.Clear();

        var parametros = await _repository.GetParametrosByReferenciaAsync(referenciaId);
        foreach (var p in parametros)
        {
            int idx = gridParametros.Rows.Add(p.Id, p.NPasoEnsayo, p.NombreContacto,
                p.ResistenciaNominal, p.Tolerancia, p.Pendiente, p.Offset, p.ResistenciaMinima, p.PosX, p.PosY);
            gridParametros.Rows[idx].Tag = p;
        }

        gridParametros.SelectionChanged += GridParametros_SelectionChanged;

        if (gridParametros.Rows.Count > 0)
        {
            gridParametros.Rows[0].Selected = true;
            CopiarFilaAlFormulario(gridParametros.Rows[0]);
        }
        else
        {
            ClearParamForm();
        }

        picPreview.Invalidate();
    }

    private async void BtnNuevaRef_Click(object? sender, EventArgs e)
    {
        _referenciaActual = null;
        txtRefNombre.Text = string.Empty;
        txtRefDesc.Text   = string.Empty;
        txtRefModelo.Text = string.Empty;
        chkActiva.Checked = true;
        nudRefNumMcps.Value  = PC7866.Models.Pc7866Commands.McpChipCount;
        nudRefMuestras.Value = 1;
        nudRefRetardo.Value  = 0;
        txtRefInh1.Text = "N"; txtRefInh2.Text = "N"; txtRefInh3.Text = "N"; txtRefInh4.Text = "N";
        picPreview.Image  = null;
        gridParametros.Rows.Clear();
        ClearParamForm();
        txtRefNombre.Focus();
        await Task.CompletedTask;
    }

    private async void BtnGuardarRef_Click(object? sender, EventArgs e)
    {
        if (_repository is null) return;
        if (string.IsNullOrWhiteSpace(txtRefNombre.Text))
        {
            MessageBox.Show("El nombre de la referencia es obligatorio.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        byte[]? imagen = null;
        if (picPreview.Image is not null)
        {
            using var ms = new MemoryStream();
            picPreview.Image.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            imagen = ms.ToArray();
        }

        if (_referenciaActual is null)
        {
            var nueva = new Referencia
            {
                ReferenciaNombre  = txtRefNombre.Text.Trim(),
                Descripcion       = txtRefDesc.Text.Trim(),
                ModeloPlaca       = txtRefModelo.Text.Trim(),
                BActiva           = chkActiva.Checked,
                NumMcps           = (int)nudRefNumMcps.Value,
                Muestras          = (int)nudRefMuestras.Value,
                RetardoMs         = (int)nudRefRetardo.Value,
                Inh1Pos           = ParseInh(txtRefInh1.Text),
                Inh2Pos           = ParseInh(txtRefInh2.Text),
                Inh3Pos           = ParseInh(txtRefInh3.Text),
                Inh4Pos           = ParseInh(txtRefInh4.Text),
                FechaCreacion     = DateTime.Now,
                FechaModificacion = DateTime.Now,
                Imagen            = imagen
            };
            nueva.Id = await _repository.InsertReferenciaAsync(nueva);
            _referenciaActual = nueva;
        }
        else
        {
            _referenciaActual.ReferenciaNombre  = txtRefNombre.Text.Trim();
            _referenciaActual.Descripcion       = txtRefDesc.Text.Trim();
            _referenciaActual.ModeloPlaca       = txtRefModelo.Text.Trim();
            _referenciaActual.BActiva           = chkActiva.Checked;
            _referenciaActual.NumMcps           = (int)nudRefNumMcps.Value;
            _referenciaActual.Muestras          = (int)nudRefMuestras.Value;
            _referenciaActual.RetardoMs         = (int)nudRefRetardo.Value;
            _referenciaActual.Inh1Pos           = ParseInh(txtRefInh1.Text);
            _referenciaActual.Inh2Pos           = ParseInh(txtRefInh2.Text);
            _referenciaActual.Inh3Pos           = ParseInh(txtRefInh3.Text);
            _referenciaActual.Inh4Pos           = ParseInh(txtRefInh4.Text);
            _referenciaActual.FechaModificacion = DateTime.Now;
            _referenciaActual.Imagen            = imagen;
            await _repository.UpdateReferenciaAsync(_referenciaActual);
        }

        await LoadReferenciasAsync();
        MessageBox.Show("Referencia guardada correctamente.", "OK",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private async void BtnEliminarRef_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null || _repository is null) return;
        if (MessageBox.Show($"¿Eliminar referencia '{_referenciaActual.ReferenciaNombre}'?",
            "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        await _repository.DeleteReferenciaAsync(_referenciaActual.Id);
        _referenciaActual = null;
        await LoadReferenciasAsync();
        gridParametros.Rows.Clear();
        ClearParamForm();
        picPreview.Invalidate();

        MessageBox.Show("Referencia borrada correctamente.", "OK",
            MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BtnCargarImagen_Click(object? sender, EventArgs e)
    {
        using var dlg = new OpenFileDialog
        {
            Filter = "Imágenes|*.png;*.jpg;*.bmp;*.gif",
            Title  = "Seleccionar imagen de referencia"
        };
        if (dlg.ShowDialog() == DialogResult.OK)
        {
            picPreview.Image = Image.FromFile(dlg.FileName);
            picPreview.Invalidate();
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // ParametrosEnsayo
    // ─────────────────────────────────────────────────────────────────────────

    private void GridParametros_SelectionChanged(object? sender, EventArgs e)
    {
        if (gridParametros.SelectedRows.Count == 0) return;
        CopiarFilaAlFormulario(gridParametros.SelectedRows[0]);
        picPreview.Invalidate();
    }

    private void CopiarFilaAlFormulario(DataGridViewRow row)
    {
        _ignorarNudEvents = true;
        nudPaso.Value    = Convert.ToDecimal(row.Cells["colP_Paso"].Value);
        txtContacto.Text = row.Cells["colP_Contacto"].Value?.ToString() ?? string.Empty;
        nudNominal.Value = Convert.ToDecimal(row.Cells["colP_Nominal"].Value);
        nudTol.Value     = Convert.ToDecimal(row.Cells["colP_Tol"].Value);
        nudPendiente.Value = Convert.ToDecimal(row.Cells["colP_Pendiente"].Value);
        nudOffset.Value  = Convert.ToDecimal(row.Cells["colP_Offset"].Value);
        nudMinima.Value  = Convert.ToDecimal(row.Cells["colP_Minima"].Value);
        nudPosX.Value    = Convert.ToDecimal(row.Cells["colP_PosX"].Value);
        nudPosY.Value    = Convert.ToDecimal(row.Cells["colP_PosY"].Value);

        if (row.Tag is ParametroEnsayo p)
        {
            nudMcpArribaChip.Value = Math.Max(0, p.McpArribaChip);
            nudMcpArribaPin.Value  = p.McpArribaPin;
            nudMcpAbajoChip.Value  = Math.Max(0, p.McpAbajoChip);
            nudMcpAbajoPin.Value   = p.McpAbajoPin;
            nudCanalMux.Value      = p.CanalMultiplexor;
        }
        else
        {
            nudMcpArribaChip.Value = 0;
            nudMcpArribaPin.Value  = 1;
            nudMcpAbajoChip.Value  = 0;
            nudMcpAbajoPin.Value   = 1;
            nudCanalMux.Value      = 0;
        }
        _ignorarNudEvents = false;
    }

    private void BtnNuevoParam_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null)
        {
            MessageBox.Show("Primero guarde o seleccione una referencia.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_modoNuevoParam)
        {
            var fantasmaExistente = gridParametros.Rows
                .Cast<DataGridViewRow>()
                .FirstOrDefault(r => Convert.ToInt32(r.Cells["colP_Id"].Value) == 0);
            if (fantasmaExistente is not null)
                fantasmaExistente.Selected = true;
            return;
        }

        int siguientePaso = 1;
        if (gridParametros.Rows.Count > 0)
        {
            siguientePaso = gridParametros.Rows
                .Cast<DataGridViewRow>()
                .Max(r => Convert.ToInt32(r.Cells["colP_Paso"].Value)) + 1;
        }

        gridParametros.SelectionChanged -= GridParametros_SelectionChanged;
        gridParametros.ClearSelection();

        int newIndex = gridParametros.Rows.Add(0, siguientePaso, string.Empty, 0, 0, 1, 0, 0, 0, 0);
        DataGridViewRow nuevaFila = gridParametros.Rows[newIndex];
        nuevaFila.DefaultCellStyle.ForeColor = Color.Gray;
        nuevaFila.DefaultCellStyle.Font      = new Font(gridParametros.Font, FontStyle.Italic);

        gridParametros.SelectionChanged += GridParametros_SelectionChanged;

        nuevaFila.Selected = true;
        gridParametros.FirstDisplayedScrollingRowIndex = newIndex;

        _modoNuevoParam = true;

        _ignorarNudEvents = true;
        nudPaso.Value    = siguientePaso;
        txtContacto.Text = string.Empty;
        nudNominal.Value = 0;
        nudTol.Value     = 0;
        nudPendiente.Value = 1;
        nudOffset.Value  = 0;
        nudMinima.Value  = 0;
        nudPosX.Value    = 0;
        nudPosY.Value    = 0;
        nudMcpArribaChip.Value = 0;
        nudMcpArribaPin.Value  = 1;
        nudMcpAbajoChip.Value  = 0;
        nudMcpAbajoPin.Value   = 1;
        nudCanalMux.Value      = 0;
        _ignorarNudEvents = false;

        picPreview.Invalidate();
        txtContacto.Focus();
    }

    private async void BtnGuardarParam_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null || _repository is null)
        {
            MessageBox.Show("Primero guarde o seleccione una referencia.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        int numeroPaso = (int)nudPaso.Value;

        int existingId = 0;
        if (gridParametros.SelectedRows.Count > 0)
            existingId = Convert.ToInt32(gridParametros.SelectedRows[0].Cells["colP_Id"].Value);

        bool pasoYaExiste = gridParametros.Rows
            .Cast<DataGridViewRow>()
            .Any(r =>
                Convert.ToInt32(r.Cells["colP_Paso"].Value) == numeroPaso &&
                Convert.ToInt32(r.Cells["colP_Id"].Value)   != existingId);

        if (pasoYaExiste)
        {
            MessageBox.Show($"El paso {numeroPaso} ya existe en esta referencia.",
                "Paso duplicado", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            nudPaso.Focus();
            return;
        }

        int mcpArribaChip = (int)nudMcpArribaChip.Value;
        int mcpArribaPin  = (int)nudMcpArribaPin.Value; // NUD y modelo usan el mismo pin 1-16
        int mcpAbajoChip  = (int)nudMcpAbajoChip.Value;
        int mcpAbajoPin   = (int)nudMcpAbajoPin.Value;
        int canalMux      = (int)nudCanalMux.Value;

        if (existingId > 0)
        {
            var p = new ParametroEnsayo
            {
                Id                 = existingId,
                ReferenciaId       = _referenciaActual.Id,
                NPasoEnsayo        = numeroPaso,
                NombreContacto     = txtContacto.Text.Trim(),
                ResistenciaNominal = (float)nudNominal.Value,
                Tolerancia         = (float)nudTol.Value,
                Pendiente          = (float)nudPendiente.Value,
                Offset             = (float)nudOffset.Value,
                ResistenciaMinima  = (float)nudMinima.Value,
                McpArribaChip      = mcpArribaChip,
                McpArribaPin       = mcpArribaPin,
                McpAbajoChip       = mcpAbajoChip,
                McpAbajoPin        = mcpAbajoPin,
                CanalMultiplexor   = canalMux,
                PosX               = (int)nudPosX.Value,
                PosY               = (int)nudPosY.Value,
                FechaModificacion  = DateTime.Now
            };
            await _repository.UpdateParametroAsync(p);
        }
        else
        {
            var p = new ParametroEnsayo
            {
                ReferenciaId       = _referenciaActual.Id,
                NPasoEnsayo        = numeroPaso,
                NombreContacto     = txtContacto.Text.Trim(),
                ResistenciaNominal = (float)nudNominal.Value,
                Tolerancia         = (float)nudTol.Value,
                Pendiente          = (float)nudPendiente.Value,
                Offset             = (float)nudOffset.Value,
                ResistenciaMinima  = (float)nudMinima.Value,
                McpArribaChip      = mcpArribaChip,
                McpArribaPin       = mcpArribaPin,
                McpAbajoChip       = mcpAbajoChip,
                McpAbajoPin        = mcpAbajoPin,
                CanalMultiplexor   = canalMux,
                PosX               = (int)nudPosX.Value,
                PosY               = (int)nudPosY.Value,
                FechaCreacion      = DateTime.Now,
                FechaModificacion  = DateTime.Now
            };
            await _repository.InsertParametroAsync(p);
        }

        await LoadParametrosAsync(_referenciaActual.Id);
    }

    private async void BtnEliminarParam_Click(object? sender, EventArgs e)
    {
        if (_repository is null || gridParametros.SelectedRows.Count == 0) return;
        int id = Convert.ToInt32(gridParametros.SelectedRows[0].Cells["colP_Id"].Value);
        if (id <= 0) return;

        if (MessageBox.Show("¿Eliminar este paso?", "Confirmar",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

        await _repository.DeleteParametroAsync(id);
        if (_referenciaActual is not null)
            await LoadParametrosAsync(_referenciaActual.Id);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Importar / exportar parámetros (CSV / JSON)
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnExportarParam_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null || _repository is null)
        {
            MessageBox.Show("Primero seleccione una referencia.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var parametros = (await _repository.GetParametrosByReferenciaAsync(_referenciaActual.Id)).ToList();
        if (parametros.Count == 0)
        {
            MessageBox.Show("Esta referencia no tiene parámetros que exportar.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new SaveFileDialog
        {
            Filter   = "CSV (*.csv)|*.csv|JSON (*.json)|*.json",
            FileName = $"parametros_{_referenciaActual.ReferenciaNombre}",
            Title    = "Exportar parámetros"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        try
        {
            if (dlg.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                ParametroImportExport.ExportJson(dlg.FileName, parametros);
            else
                ParametroImportExport.ExportCsv(dlg.FileName, parametros);

            MessageBox.Show($"Exportados {parametros.Count} parámetros.", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al exportar: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private async void BtnImportarParam_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null || _repository is null)
        {
            MessageBox.Show("Primero seleccione una referencia.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        using var dlg = new OpenFileDialog
        {
            Filter = "Parámetros (*.csv;*.json)|*.csv;*.json|CSV (*.csv)|*.csv|JSON (*.json)|*.json",
            Title  = "Importar parámetros"
        };
        if (dlg.ShowDialog() != DialogResult.OK) return;

        List<ParametroEnsayo> parametros;
        try
        {
            parametros = dlg.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase)
                ? ParametroImportExport.ImportJson(dlg.FileName)
                : ParametroImportExport.ImportCsv(dlg.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al leer el archivo: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (parametros.Count == 0)
        {
            MessageBox.Show("El archivo no contiene parámetros.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var resp = MessageBox.Show(
            $"Se importarán {parametros.Count} parámetros a la referencia '{_referenciaActual.ReferenciaNombre}'.\n\n" +
            "Sí = reemplazar los existentes  |  No = añadir  |  Cancelar",
            "Importar parámetros", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
        if (resp == DialogResult.Cancel) return;

        try
        {
            if (resp == DialogResult.Yes)
            {
                var existentes = await _repository.GetParametrosByReferenciaAsync(_referenciaActual.Id);
                foreach (var ex in existentes)
                    await _repository.DeleteParametroAsync(ex.Id);
            }

            foreach (var p in parametros)
            {
                p.Id           = 0;
                p.ReferenciaId = _referenciaActual.Id;

                p.FechaCreacion     = DateTime.Now;
                p.FechaModificacion = DateTime.Now;
                await _repository.InsertParametroAsync(p);
            }

            await LoadParametrosAsync(_referenciaActual.Id);
            MessageBox.Show($"Importados {parametros.Count} parámetros.", "OK",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error al importar: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Dibujo y arrastre del punto sobre picPreview
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Calcula el rectángulo que ocupa la imagen dentro del PictureBox
    /// teniendo en cuenta el modo Zoom (mantiene proporción y centra).
    /// </summary>
    private RectangleF GetImageRect()
    {
        if (picPreview.Image is null)
            return new RectangleF(0, 0, picPreview.Width, picPreview.Height);

        float imgW = picPreview.Image.Width;
        float imgH = picPreview.Image.Height;
        float ctlW = picPreview.Width;
        float ctlH = picPreview.Height;

        float scale = Math.Min(ctlW / imgW, ctlH / imgH);
        float drawW = imgW * scale;
        float drawH = imgH * scale;
        float offsetX = (ctlW - drawW) / 2f;
        float offsetY = (ctlH - drawH) / 2f;

        return new RectangleF(offsetX, offsetY, drawW, drawH);
    }

    /// <summary>
    /// Convierte coordenadas de imagen (0-MaxX, 0-MaxY según nudPosX.Maximum)
    /// a coordenadas de píxel dentro del PictureBox.
    /// </summary>
    private PointF ImageToControl(float imgX, float imgY)
    {
        var r = GetImageRect();
        float px = r.Left + (imgX / (float)nudPosX.Maximum) * r.Width;
        float py = r.Top  + (imgY / (float)nudPosY.Maximum) * r.Height;
        return new PointF(px, py);
    }

    /// <summary>
    /// Convierte coordenadas de píxel del PictureBox a coordenadas de imagen,
    /// acotando al rango válido.
    /// </summary>
    private PointF ControlToImage(float ctlX, float ctlY)
    {
        var r = GetImageRect();
        float imgX = (ctlX - r.Left) / r.Width  * (float)nudPosX.Maximum;
        float imgY = (ctlY - r.Top)  / r.Height * (float)nudPosY.Maximum;
        imgX = Math.Clamp(imgX, 0, (float)nudPosX.Maximum);
        imgY = Math.Clamp(imgY, 0, (float)nudPosY.Maximum);
        return new PointF(imgX, imgY);
    }

    private void PicPreview_Paint(object? sender, PaintEventArgs e)
    {
        if (picPreview.Image is null) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        const int R       = 7;   // radio del punto seleccionado
        const int R_otros = 5;   // radio de puntos del resto de pasos

        // ── Dibujar todos los puntos en gris ─────────────────────────────
        int selectedId = 0;
        if (gridParametros.SelectedRows.Count > 0)
            selectedId = Convert.ToInt32(gridParametros.SelectedRows[0].Cells["colP_Id"].Value);

        foreach (DataGridViewRow row in gridParametros.Rows)
        {
            int rowId = Convert.ToInt32(row.Cells["colP_Id"].Value);
            if (rowId == selectedId) continue;   // el seleccionado se dibuja después

            int px = Convert.ToInt32(row.Cells["colP_PosX"].Value);
            int py = Convert.ToInt32(row.Cells["colP_PosY"].Value);
            if (px == 0 && py == 0) continue;

            string contacto = row.Cells["colP_Contacto"].Value?.ToString() ?? string.Empty;
            var cp   = ImageToControl(px, py);

            using var brushGris = new SolidBrush(Color.FromArgb(180, Color.DimGray));
            using var penGris   = new Pen(Color.White, 1.5f);
            g.FillEllipse(brushGris, cp.X - R_otros, cp.Y - R_otros, R_otros * 2, R_otros * 2);
            g.DrawEllipse(penGris,   cp.X - R_otros, cp.Y - R_otros, R_otros * 2, R_otros * 2);

            using var fnt  = new Font("Segoe UI", 7f, FontStyle.Bold);
            using var brushTxt = new SolidBrush(Color.White);
            var label = contacto;
            var sz    = g.MeasureString(label, fnt);
            g.FillRectangle(Brushes.DimGray, cp.X + R_otros + 2, cp.Y - sz.Height / 2 - 1,
                sz.Width + 4, sz.Height + 2);
            g.DrawString(label, fnt, brushTxt, cp.X + R_otros + 4, cp.Y - sz.Height / 2);
        }

        // ── Dibujar el punto seleccionado en rojo ─────────────────────────
        int selX = (int)nudPosX.Value;
        int selY = (int)nudPosY.Value;
        if (selX == 0 && selY == 0) return;

        var selPt = ImageToControl(selX, selY);

        using var brushRojo = new SolidBrush(Color.FromArgb(220, Color.Red));
        using var penBlanco = new Pen(Color.White, 2f);
        using var penSombra = new Pen(Color.FromArgb(100, Color.Black), 1f);
        g.FillEllipse(brushRojo, selPt.X - R, selPt.Y - R, R * 2, R * 2);
        g.DrawEllipse(penBlanco, selPt.X - R, selPt.Y - R, R * 2, R * 2);

        // Cruz interior
        using var penCruz = new Pen(Color.White, 1.5f);
        g.DrawLine(penCruz, selPt.X - R + 3, selPt.Y, selPt.X + R - 3, selPt.Y);
        g.DrawLine(penCruz, selPt.X, selPt.Y - R + 3, selPt.X, selPt.Y + R - 3);

        // Nombre del contacto
        using var fntSel    = new Font("Segoe UI", 8f, FontStyle.Bold);
        var labelSel = txtContacto.Text.Trim();
        if (labelSel.Length == 0) return;
        var szSel    = g.MeasureString(labelSel, fntSel);
        // Fondo negro semitransparente detrás del nombre
        g.FillRectangle(new SolidBrush(Color.FromArgb(140, Color.Black)),
            selPt.X + R, selPt.Y - szSel.Height / 2 - 1, szSel.Width + 2, szSel.Height + 2);
        g.DrawString(labelSel, fntSel, Brushes.White,
            selPt.X + R + 1, selPt.Y - szSel.Height / 2);
    }

    private void PicPreview_MouseDown(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Left || picPreview.Image is null) return;

        int selX = (int)nudPosX.Value;
        int selY = (int)nudPosY.Value;
        var selPt = ImageToControl(selX, selY);

        const int hitRadius = 14;
        float dx = e.X - selPt.X;
        float dy = e.Y - selPt.Y;

        // Si el clic está cerca del punto seleccionado, iniciar arrastre.
        // Si el punto es (0,0) (aún no colocado), cualquier clic lo coloca.
        if ((selX == 0 && selY == 0) || (dx * dx + dy * dy) <= hitRadius * hitRadius)
        {
            _arrastrando = true;
            ActualizarPosDesdeRaton(e.X, e.Y);
            picPreview.Cursor = Cursors.Hand;
        }
    }

    private void PicPreview_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_arrastrando) return;
        ActualizarPosDesdeRaton(e.X, e.Y);
    }

    private void PicPreview_MouseUp(object? sender, MouseEventArgs e)
    {
        if (!_arrastrando) return;
        _arrastrando = false;
        picPreview.Cursor = Cursors.Default;
        ActualizarPosDesdeRaton(e.X, e.Y);
    }

    private void ActualizarPosDesdeRaton(int ctlX, int ctlY)
    {
        var img = ControlToImage(ctlX, ctlY);
        _ignorarNudEvents = true;
        nudPosX.Value = (decimal)Math.Round(img.X);
        nudPosY.Value = (decimal)Math.Round(img.Y);
        _ignorarNudEvents = false;
        picPreview.Invalidate();
    }

    private void NudPos_ValueChanged(object? sender, EventArgs e)
    {
        if (_ignorarNudEvents) return;
        picPreview.Invalidate();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Utilidades
    // ─────────────────────────────────────────────────────────────────────────

    private void ClearParamForm()
    {
        _ignorarNudEvents = true;
        nudPaso.Value    = 1;
        txtContacto.Text = string.Empty;
        nudNominal.Value = 0;
        nudTol.Value     = 0;
        nudPendiente.Value = 1;
        nudOffset.Value  = 0;
        nudMinima.Value  = 0;
        nudPosX.Value    = 0;
        nudPosY.Value    = 0;
        nudMcpArribaChip.Value = 0;
        nudMcpArribaPin.Value  = 1;
        nudMcpAbajoChip.Value  = 0;
        nudMcpAbajoPin.Value   = 1;
        nudCanalMux.Value      = 0;
        _modoNuevoParam  = false;
        _ignorarNudEvents = false;
        picPreview.Invalidate();
    }

    /// <summary>Formatea una posición INH (0-15) como dígito hex, o "N" si no está asignada.</summary>
    private static string FormatInh(int? pos)
        => pos is null ? "N" : pos.Value.ToString("X1");

    /// <summary>Parsea el texto de una posición INH ("N" o dígito hex 0-F) a su valor entero o null.</summary>
    private static int? ParseInh(string texto)
    {
        texto = texto.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(texto) || texto == "N") return null;
        return int.TryParse(texto, System.Globalization.NumberStyles.HexNumber,
            System.Globalization.CultureInfo.InvariantCulture, out int v) ? v : null;
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _repository?.Dispose();
        base.OnHandleDestroyed(e);
    }
}
