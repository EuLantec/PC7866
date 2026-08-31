using PC7866.Configuration;
using PC7866.Models;
using PC7866.Services.Database;
using PC7866.Services.SerialCommunication;
using PC7866.Services.StateMachine;
using PC7866.Utils;

namespace PC7866.Views;

/// <summary>
/// Panel de ejecución automática del ensayo resistivo.
/// Muestra imagen de la referencia con dots de color, lote/operario,
/// resultados por paso y barra de progreso.
/// </summary>
public partial class AutomaticTestPanel : UserControl
{
    // Poner en true para volcar al log los comandos TX/RX y las líneas CALC del ensayo automático
    // (útil para depurar el protocolo). En false el ensayo va más rápido (no bloquea el hilo serie
    // con escrituras al log por cada comando).
    private const bool EnableAutoCommandLog = false;

    private readonly ISerialPortService _serialPort;
    private readonly bool _ownsSerialPort;
    private readonly CommandParser _parser;
    private readonly TestStateMachine _stateMachine;
    private ITestRepository? _repository;
    private CancellationTokenSource? _cts;

    private Referencia? _referenciaActual;
    private List<ParametroEnsayo> _parametros = new();

    // Colores de los dots: gris=no medido, verde=OK, rojo=NOK
    private readonly Dictionary<int, Color> _dotColors = new();

    public AutomaticTestPanel(ISerialPortService? serialPort = null)
    {
        InitializeComponent();

        _serialPort   = serialPort ?? new SerialPortService();
        _ownsSerialPort = serialPort is null;
        _parser       = new CommandParser();
        _stateMachine = new TestStateMachine();
        _stateMachine.StateChanged += (_, s) =>
            Invoke(() => lblMachineState.Text = $"Estado: {s}");

        InitializeControls();
        AttachEventHandlers();
        _ = TryInitDatabaseAsync();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeControls()
    {
        LoadAvailablePorts();

        cmbBaudRate.Items.AddRange(new object[] { 9600, 19200, 38400, 57600, 115200 });
        cmbBaudRate.SelectedItem = AppSettings.Instance.DefaultBaudRate;

        SetConnectedState(_serialPort.IsOpen);
        btnStartTest.Enabled    = _serialPort.IsOpen && _referenciaActual is not null && _parametros.Count > 0;
        btnAbortTest.Enabled    = false;
        progressBar.Value       = 0;
        lblCurrentStep.Text     = "—";
        lblMachineState.Text    = "Estado: Idle";
    }

    private void LoadAvailablePorts()
    {
        cmbPort.Items.Clear();
        string[] ports = _serialPort.GetAvailablePorts();
        if (ports.Length > 0)
        {
            cmbPort.Items.AddRange(ports);
            if (_serialPort.IsOpen && !string.IsNullOrWhiteSpace(_serialPort.CurrentPort))
            {
                int currentIdx = cmbPort.FindStringExact(_serialPort.CurrentPort);
                cmbPort.SelectedIndex = currentIdx >= 0 ? currentIdx : 0;
            }
            else
            {
                cmbPort.SelectedIndex = 0;
            }
        }
        else
        {
            AddLog("⚠️ Sin puertos serie disponibles", LogLevel.Warning);
        }
    }

    private static string FormatResistance(float r)
        => r < 0 ? "∞" : $"{r:F2}";

    private void AttachEventHandlers()
    {
        btnConnect.Click        += BtnConnect_Click;
        btnDisconnect.Click     += BtnDisconnect_Click;
        btnStartTest.Click      += BtnStartTest_Click;
        btnAbortTest.Click      += (_, _) => { _cts?.Cancel(); AddLog("⛔ Abortando…", LogLevel.Warning); };
        btnRefreshPorts.Click   += (_, _) => LoadAvailablePorts();
        btnRefreshRefs.Click    += async (_, _) => await LoadReferenciasAsync();

        cmbReferencia.SelectedIndexChanged += async (_, _) => await OnReferenciaChangedAsync();

        _serialPort.PortOpened    += (_, _) => Invoke(() => SetConnectedState(true));
        _serialPort.PortClosed    += (_, _) => Invoke(() => SetConnectedState(false));
        _serialPort.ErrorOccurred += (_, err) => Invoke(() => AddLog($"❌ {err}", LogLevel.Error));

        picReferencia.Paint += PicReferencia_Paint;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Base de datos
    // ─────────────────────────────────────────────────────────────────────────

    private async Task TryInitDatabaseAsync()
    {
        try
        {
            _repository = new TestRepository(AppSettings.Instance.GetConnectionString());
            await _repository.TestConnectionAsync();
            await _repository.InitializeDatabaseAsync();
            AddLog("🗄️ Base de datos conectada", LogLevel.Info);
            await LoadReferenciasAsync();
        }
        catch (Exception ex)
        {
            AddLog($"⚠️ Error BD: {ex.Message}", LogLevel.Warning);
        }
    }

    private async Task LoadReferenciasAsync()
    {
        if (_repository is null) return;
        try
        {
            var refs = await _repository.GetAllReferenciasAsync();
            cmbReferencia.Items.Clear();
            foreach (var r in refs) cmbReferencia.Items.Add(r);
            cmbReferencia.DisplayMember = "ReferenciaNombre";
            if (cmbReferencia.Items.Count > 0) cmbReferencia.SelectedIndex = 0;
            AddLog($"📋 {cmbReferencia.Items.Count} referencias cargadas", LogLevel.Info);
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error cargando referencias: {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>
    /// Recarga las referencias y los parámetros de la referencia actualmente seleccionada.
    /// Se llama cada vez que se muestra el panel automático.
    /// </summary>
    public async Task RefreshAsync()
    {
        if (_repository is null) return;
        try
        {
            string? prevName = _referenciaActual?.ReferenciaNombre;

            var refs = await _repository.GetAllReferenciasAsync();
            cmbReferencia.Items.Clear();
            foreach (var r in refs) cmbReferencia.Items.Add(r);
            cmbReferencia.DisplayMember = "ReferenciaNombre";

            if (cmbReferencia.Items.Count == 0) return;

            // Buscar la referencia que estaba seleccionada, o usar la primera
            int idx = 0;
            if (prevName is not null)
            {
                for (int i = 0; i < cmbReferencia.Items.Count; i++)
                {
                    if (cmbReferencia.Items[i] is Referencia r && r.ReferenciaNombre == prevName)
                    {
                        idx = i;
                        break;
                    }
                }
            }

            if (cmbReferencia.SelectedIndex != idx)
                cmbReferencia.SelectedIndex = idx; // dispara SelectedIndexChanged → OnReferenciaChangedAsync
            else
                await OnReferenciaChangedAsync();  // mismo índice, forzar recarga de parámetros
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error recargando referencias: {ex.Message}", LogLevel.Error);
        }
    }

    private async Task OnReferenciaChangedAsync()
    {
        if (cmbReferencia.SelectedItem is not Referencia ref_ || _repository is null) return;
        _referenciaActual = ref_;
        _parametros = (await _repository.GetParametrosByReferenciaAsync(ref_.Id)).ToList();

        // Resetear dots a gris
        _dotColors.Clear();
        foreach (var p in _parametros) _dotColors[p.Id] = Color.Gray;

        // Mostrar imagen si existe
        if (ref_.Imagen?.Length > 0)
        {
            using var ms = new MemoryStream(ref_.Imagen);
            picReferencia.Image = Image.FromStream(ms);
        }
        else
        {
            picReferencia.Image = null;
        }

        picReferencia.Invalidate();
        gridResultados.Rows.Clear();
        UpdateStartButton();
        AddLog($"📝 Referencia: {ref_.ReferenciaNombre} — {_parametros.Count} pasos", LogLevel.Info);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Conexión serie
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (cmbPort.SelectedItem is null || cmbBaudRate.SelectedItem is null) return;
        btnConnect.Enabled = false;
        string port = cmbPort.SelectedItem.ToString()!;
        int baud    = (int)cmbBaudRate.SelectedItem;
        AddLog($"📡 Conectando {port} @ {baud} bps…", LogLevel.Info);
        bool ok     = await Task.Run(() => _serialPort.Open(port, baud));
        if (!ok) { btnConnect.Enabled = true; AddLog($"❌ No se pudo abrir {port}", LogLevel.Error); }
    }

    private void BtnDisconnect_Click(object? sender, EventArgs e)
    {
        _serialPort.Close();
        AddLog("🔌 Desconectado", LogLevel.Info);
    }

    private void SetConnectedState(bool connected)
    {
        lblConnectionStatus.Text      = connected ? $"● {_serialPort.CurrentPort}" : "○ Desconectado";
        lblConnectionStatus.ForeColor = connected ? Color.Green : Color.Red;
        btnConnect.Enabled            = !connected;
        btnDisconnect.Enabled         = connected;
        cmbPort.Enabled               = !connected;
        cmbBaudRate.Enabled           = !connected;
        UpdateStartButton();
        if (connected) AddLog($"✅ Conectado: {_serialPort.CurrentPort}", LogLevel.Info);
    }

    private void UpdateStartButton() =>
        btnStartTest.Enabled = _serialPort.IsOpen && _referenciaActual is not null && _parametros.Count > 0;

    // ─────────────────────────────────────────────────────────────────────────
    // Ejecución del ensayo
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnStartTest_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null || _parametros.Count == 0)
        {
            MessageBox.Show("Seleccione una referencia con parámetros definidos.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string operario = txtOperario.Text.Trim();
        string lote     = txtLote.Text.Trim();

        if (string.IsNullOrEmpty(operario))
        {
            MessageBox.Show("Introduzca el nombre del operario.", "Aviso",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            txtOperario.Focus();
            return;
        }

        _cts = new CancellationTokenSource();
        btnStartTest.Enabled  = false;
        btnAbortTest.Enabled  = true;
        progressBar.Value     = 0;
        progressBar.Maximum   = _parametros.Count;
        gridResultados.Rows.Clear();

        // Resetear dots a gris
        foreach (var p in _parametros) _dotColors[p.Id] = Color.Gray;
        picReferencia.Invalidate();

        var progress = new Progress<TestProgressReport>(r =>
        {
            progressBar.Value   = Math.Min(r.CurrentStep, progressBar.Maximum);
            lblCurrentStep.Text = r.Message;
        });

        AddLog($"▶️ Ensayo: {_referenciaActual.ReferenciaNombre} | Op: {operario} | Lote: {lote}", LogLevel.Info);

        Resultado? resultado = null;
        try
        {
            resultado = await _stateMachine.RunAsync(
                _referenciaActual,
                _parametros,
                operario,
                lote,
                _serialPort,
                _parser,
                progress,
                OnStepCompleted,
                AppSettings.Instance.DefaultTimeout,
                _cts.Token,
                EnableAutoCommandLog ? cmd => AddLog(cmd, LogLevel.Debug) : null);
        }
        catch (OperationCanceledException)
        {
            AddLog("⛔ Ensayo cancelado", LogLevel.Warning);
        }
        catch (Exception ex)
        {
            AddLog($"❌ {ex.Message}", LogLevel.Error);
            Logger.Instance.Error($"Error ensayo automático: {ex}");
        }
        finally
        {
            btnStartTest.Enabled = true;
            btnAbortTest.Enabled = false;
            _cts?.Dispose();
            _cts = null;
        }

        if (resultado is not null)
        {
            ShowFinalResult(resultado);
            await SaveResultadoAsync(resultado);
        }
    }

    private void OnStepCompleted(ParametroEnsayo paso, ResultadoDetalle detalle)
    {
        if (InvokeRequired) { Invoke(() => OnStepCompleted(paso, detalle)); return; }

        // Actualizar dot (verde=OK, rojo=NOK, naranja=cortocircuito, azul=abierto)
        _dotColors[paso.Id] = DotColorForEstado(detalle.Estado);
        picReferencia.Invalidate();

        // Añadir fila al grid
        string resultado = EtiquetaEstado(detalle.Estado);
        gridResultados.Rows.Add(
            paso.NPasoEnsayo,
            paso.NombreContacto,
            $"{FormatResistance(detalle.ResistenciaMedida)}",
            $"{paso.ResistenciaNominal:F2} ±{paso.Tolerancia:F2}",
            resultado);

        // Color de fila
        var row = gridResultados.Rows[gridResultados.Rows.Count - 1];
        row.DefaultCellStyle.BackColor = FilaColorForEstado(detalle.Estado);
    }

    private static Color DotColorForEstado(EstadoMedicion estado) => estado switch
    {
        EstadoMedicion.Ok            => Color.Green,
        EstadoMedicion.Cortocircuito => Color.Orange,
        EstadoMedicion.Abierto       => Color.RoyalBlue,
        _                             => Color.Red
    };

    private static Color FilaColorForEstado(EstadoMedicion estado) => estado switch
    {
        EstadoMedicion.Ok            => Color.FromArgb(220, 255, 220),
        EstadoMedicion.Cortocircuito => Color.FromArgb(255, 235, 200),
        EstadoMedicion.Abierto       => Color.FromArgb(215, 230, 255),
        _                             => Color.FromArgb(255, 220, 220)
    };

    private static string EtiquetaEstado(EstadoMedicion estado) => estado switch
    {
        EstadoMedicion.Ok            => "✅ OK",
        EstadoMedicion.Cortocircuito => "⚡ CORTOCIRCUITO",
        EstadoMedicion.Abierto       => "🔵 ABIERTO",
        _                             => "❌ NOK"
    };

    private void ShowFinalResult(Resultado resultado)
    {
        string icon = resultado.ResultadoGlobal ? "✅ BUENO" : "❌ MALO";
        int ok      = resultado.Detalles.Count(d => d.Resultado);
        lblCurrentStep.Text = $"{icon}  {ok}/{resultado.Detalles.Count} pasos OK";
        AddLog($"{icon} — {ok}/{resultado.Detalles.Count}", LogLevel.Info);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Imagen y dots
    // ─────────────────────────────────────────────────────────────────────────

    private void PicReferencia_Paint(object? sender, PaintEventArgs e)
    {
        if (_parametros.Count == 0) return;

        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        const int   R    = 10;
        const float MaxC = 4000f;

        RectangleF imgRect = GetImageRect(picReferencia);

        foreach (var p in _parametros)
        {
            if (p.PosX == 0 && p.PosY == 0) continue;

            if (!_dotColors.TryGetValue(p.Id, out var color)) color = Color.Gray;

            // Transformar coordenadas de imagen (0-4000) a píxeles del control
            float cx = imgRect.Left + (p.PosX / MaxC) * imgRect.Width;
            float cy = imgRect.Top  + (p.PosY / MaxC) * imgRect.Height;

            using var brush = new SolidBrush(Color.FromArgb(200, color));
            using var pen   = new Pen(Color.White, 1.5f);
            g.FillEllipse(brush, cx - R, cy - R, R * 2, R * 2);
            g.DrawEllipse(pen,   cx - R, cy - R, R * 2, R * 2);

            // Número de paso dentro del dot
            using var fnt      = new Font("Segoe UI", 7f, FontStyle.Bold);
            using var brushTxt = new SolidBrush(Color.White);
            var label = p.NPasoEnsayo.ToString();
            var sz    = g.MeasureString(label, fnt);
            g.DrawString(label, fnt, brushTxt, cx - sz.Width / 2, cy - sz.Height / 2);
        }
    }

    /// <summary>
    /// Calcula el rectángulo que ocupa la imagen dentro de un PictureBox
    /// con SizeMode = Zoom (mantiene proporción y centra).
    /// </summary>
    private static RectangleF GetImageRect(PictureBox pic)
    {
        if (pic.Image is null)
            return new RectangleF(0, 0, pic.Width, pic.Height);

        float imgW  = pic.Image.Width;
        float imgH  = pic.Image.Height;
        float ctlW  = pic.Width;
        float ctlH  = pic.Height;

        float scale   = Math.Min(ctlW / imgW, ctlH / imgH);
        float drawW   = imgW * scale;
        float drawH   = imgH * scale;
        float offsetX = (ctlW - drawW) / 2f;
        float offsetY = (ctlH - drawH) / 2f;

        return new RectangleF(offsetX, offsetY, drawW, drawH);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Log
    // ─────────────────────────────────────────────────────────────────────────

    private void AddLog(string message, LogLevel level)
    {
        if (InvokeRequired) { Invoke(() => AddLog(message, level)); return; }
        string ts = DateTime.Now.ToString("HH:mm:ss.fff");
        txtLog.AppendText($"[{ts}] {message}{Environment.NewLine}");
        txtLog.SelectionStart = txtLog.Text.Length;
        txtLog.ScrollToCaret();
        Logger.Instance.Log(level, message);
    }

    protected override void OnHandleDestroyed(EventArgs e)
    {
        _cts?.Cancel();
        if (_ownsSerialPort)
            _serialPort.Dispose();
        _repository?.Dispose();
        base.OnHandleDestroyed(e);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Guardado en BD
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SaveResultadoAsync(Resultado resultado)
    {
        if (_repository is null)
        {
            AddLog("⚠️ Sin conexión a BD — resultado no guardado", LogLevel.Warning);
            return;
        }

        try
        {
            resultado.Id = await _repository.InsertResultadoAsync(resultado);
            foreach (var d in resultado.Detalles)
            {
                d.ResultadoId = resultado.Id;
                await _repository.InsertDetalleAsync(d);
            }
            AddLog($"💾 Resultado guardado (ID {resultado.Id}, {resultado.Detalles.Count} detalles)", LogLevel.Info);
        }
        catch (Exception ex)
        {
            AddLog($"⚠️ Error guardando en BD: {ex.Message}", LogLevel.Warning);
            Logger.Instance.Error($"Error guardando resultado automático en BD: {ex}");
        }
    }

    private void lblConnectionStatus_Click(object sender, EventArgs e) { }
}
