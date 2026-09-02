using PC7866.Configuration;
using PC7866.Models;
using PC7866.Services.Database;
using PC7866.Services.SerialCommunication;
using PC7866.Services.StateMachine;
using PC7866.Utils;
using System.Globalization;

namespace PC7866.Views;

/// <summary>
/// Panel de control manual del PC7866.
/// Layout sin pestañas, optimizado para monitor Full HD.
/// </summary>
public partial class ManualControlPanel : UserControl
{
    private readonly ISerialPortService _serialPort;
    private readonly bool               _ownsSerialPort;
    private readonly CommandParser      _parser;
    private readonly CheckBox[]         _outputChecks = new CheckBox[Pc7866Commands.OutputCount];

    // ── Semiautomático (probar un solo contacto, sin informe) ─────────────────
    private readonly TestStateMachine    _stateMachine = new();
    private ITestRepository?             _repository;
    private Referencia?                  _referenciaActual;
    private List<ParametroEnsayo>        _parametrosManual = new();
    private CancellationTokenSource?     _semiAutoCts;

    public ManualControlPanel(ISerialPortService? serialPort = null)
    {
        InitializeComponent();
        _serialPort = serialPort ?? new SerialPortService();
        _ownsSerialPort = serialPort is null;
        _parser     = new CommandParser();
        InitializeControls();
        AttachEventHandlers();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Inicialización
    // ─────────────────────────────────────────────────────────────────────────

    private void InitializeControls()
    {
        LoadAvailablePorts();
        cmbBaudRate.Items.AddRange(new object[] { 9600, 19200, 38400, 57600, 115200 });
        cmbBaudRate.SelectedItem = AppSettings.Instance.DefaultBaudRate;

        // Solo buscar DefaultPortName si el puerto no está ya abierto
        if (!_serialPort.IsOpen)
        {
            int com4idx = cmbPort.FindStringExact(AppSettings.Instance.DefaultPortName);
            cmbPort.SelectedIndex = com4idx >= 0 ? com4idx : (cmbPort.Items.Count > 0 ? 0 : -1);
        }

        for (int i = 0; i < Pc7866Commands.McpChipCount; i++)
            cmbMcpModeAddr.Items.Add($"{i} (0x{Pc7866Commands.McpI2cAddress(i):X2})");
        cmbMcpModeAddr.SelectedIndex = 0;

        cmbChannel.Items.AddRange(new object[] { 0, 1, 2, 3 });
        cmbChannel.SelectedIndex = 0;

        BuildOutputMatrix();
        SetConnectedState(_serialPort.IsOpen);
        _ = TryInitDatabaseAsync();
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

    /// <summary>
    /// Crea la cuadrícula de <see cref="Pc7866Commands.OutputCount"/> checkboxes (8 × 12)
    /// dentro de pnlOutputMatrix, uno por cada pin de los 6 MCP23017 posibles.
    /// </summary>
    private void BuildOutputMatrix()
    {
        var tip = new ToolTip();
        const int cols = 8, cw = 86, ch = 24, gapX = 2, gapY = 2, padX = 4, padY = 4;

        for (int i = 0; i < Pc7866Commands.OutputCount; i++)
        {
            int chip = i / Pc7866Commands.McpPinCount;
            int pin  = i % Pc7866Commands.McpPinCount;
            int col = i % cols, row = i / cols;
            var chk = new CheckBox
            {
                Text     = $"{chip}.{(pin + 1):D2}",
                Tag      = i,
                Size     = new Size(cw, ch),
                Location = new Point(padX + col * (cw + gapX), padY + row * (ch + gapY)),
                Font     = new Font("Segoe UI", 8.5f)
            };
            tip.SetToolTip(chk, $"MCP {chip} (0x{Pc7866Commands.McpI2cAddress(chip):X2})  pin {pin + 1}  –  bit {i}");
            chk.CheckedChanged += OutputCheck_Changed;
            _outputChecks[i]    = chk;
            pnlOutputMatrix.Controls.Add(chk);
        }

        pnlOutputMatrix.Size = new Size(
            padX * 2 + cols * (cw + gapX),
            padY * 2 + (Pc7866Commands.OutputCount / cols) * (ch + gapY));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Eventos
    // ─────────────────────────────────────────────────────────────────────────

    private void AttachEventHandlers()
    {
        btnConnect.Click      += BtnConnect_Click;
        btnDisconnect.Click   += BtnDisconnect_Click;
        btnRefreshPorts.Click += (_, _) => LoadAvailablePorts();

        // Diagnosis
        btnDiagTotal.Click      += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisTotal());
        btnDiagAds.Click        += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.DiagAds));
        btnDiagVersion.Click    += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.DiagVersion));
        btnDiagReadConfig.Click += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.DiagReadConfig));
        btnDiagTemperature.Click += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.DiagTemperature));
        for (int _mi = 0; _mi < Pc7866Commands.McpChipCount; _mi++)
        {
            int idx = _mi; // captura
            _btnDiagMcp[idx].Click += async (_, _) =>
                await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.DiagMcpSubCmd(idx)));
        }

        // M – configuración de dirección
        btnSendMcpMode.Click += async (_, _) => await SendMcpModeAsync();

        // P – selección de pista
        btnSelectTrack.Click += async (_, _) =>
            await SendAsync(Pc7866Commands.SelectTrack((int)nudTrack.Value));

        // Salidas
        btnOutputsAllOn.Click  += (_, _) => SetAllOutputs(true);
        btnOutputsAllOff.Click += (_, _) => SetAllOutputs(false);

        // Lectura analógica
        btnReadRaw.Click         += BtnReadRaw_Click;
        btnReadFiltered.Click    += BtnReadFilteredSingle_Click;
        btnReadAllFiltered.Click += BtnReadAllFiltered_Click;

        // I – configuración de placa
        btnSendBoardConfig.Click += async (_, _) => await SendBoardConfigAsync();

        // Semiautomático – elegir modelo (envía la config) + probar un solo contacto
        btnRefreshRefsManual.Click += async (_, _) => await LoadReferenciasAsync();
        cmbReferenciaManual.SelectedIndexChanged += async (_, _) => await OnReferenciaManualChangedAsync();
        btnProbarContacto.Click += BtnProbarContacto_Click;

        // Reset
        btnReset.Click += async (_, _) =>
        {
            if (MessageBox.Show("¿Reiniciar el microcontrolador?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
                await SendAsync(Pc7866Commands.Reset());
        };

        btnClearLog.Click += (_, _) => txtLog.Clear();

        _serialPort.PortOpened    += (_, _) => Invoke(() => SetConnectedState(true));
        _serialPort.PortClosed    += (_, _) => Invoke(() => SetConnectedState(false));
        _serialPort.ErrorOccurred += (_, e) => Invoke(() => AddLog($"❌ {e}", LogLevel.Error));
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Conexión
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnConnect_Click(object? sender, EventArgs e)
    {
        if (cmbPort.SelectedItem is null || cmbBaudRate.SelectedItem is null) return;
        string port = cmbPort.SelectedItem.ToString()!;
        int    baud = (int)cmbBaudRate.SelectedItem;
        btnConnect.Enabled = false;
        AddLog($"📡 Conectando {port} @ {baud} bps…", LogLevel.Info);
        bool ok = await Task.Run(() => _serialPort.Open(port, baud));
        if (!ok) { btnConnect.Enabled = true; AddLog($"❌ No se pudo abrir {port}", LogLevel.Error); }
    }

    private void BtnDisconnect_Click(object? sender, EventArgs e)
    {
        _serialPort.Close();
        AddLog("🔌 Desconectado", LogLevel.Info);
    }

    private void SetConnectedState(bool connected)
    {
        lblConnStatus.Text      = connected ? $"● {_serialPort.CurrentPort}" : "○ Desconectado";
        lblConnStatus.ForeColor = connected ? Color.Green : Color.Red;
        btnConnect.Enabled    = !connected;
        btnDisconnect.Enabled =  connected;
        cmbPort.Enabled       = !connected;
        cmbBaudRate.Enabled   = !connected;

        // Habilitar/deshabilitar secciones de comandos
        grpDiagnosis.Enabled    = connected;
        grpMcpMode.Enabled      = connected;
        grpTrack.Enabled        = connected;
        grpOutputs.Enabled      = connected;
        grpAnalog.Enabled       = connected;
        grpBoardConfig.Enabled  = connected;
        grpReset.Enabled        = connected;

        if (connected) AddLog($"✅ Conectado: {_serialPort.CurrentPort}", LogLevel.Info);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // M – Configuración de dirección (E/S)
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SendMcpModeAsync()
    {
        int mcpChip = cmbMcpModeAddr.SelectedIndex;
        if (mcpChip < 0) return;

        if (!ushort.TryParse(txtMcpModeMask.Text.Trim(), NumberStyles.HexNumber,
            CultureInfo.InvariantCulture, out ushort mask))
        {
            AddLog("⚠️ Máscara inválida (se esperan 4 dígitos hex)", LogLevel.Warning);
            return;
        }

        string cmd = Pc7866Commands.BuildMcpModeCommand(mcpChip, rbModeOutput.Checked, mask);
        await SendAsync(cmd);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Salidas (S)
    // ─────────────────────────────────────────────────────────────────────────

    private async void OutputCheck_Changed(object? sender, EventArgs e)
    {
        if (sender is not CheckBox chk || chk.Tag is not int bitIndex) return;
        await SendChipOutputsAsync(bitIndex / Pc7866Commands.McpPinCount);
    }

    private void SetAllOutputs(bool value)
    {
        foreach (var chk in _outputChecks) chk.CheckedChanged -= OutputCheck_Changed;
        foreach (var chk in _outputChecks) chk.Checked = value;
        foreach (var chk in _outputChecks) chk.CheckedChanged += OutputCheck_Changed;
        _ = SendAllChipsOutputsAsync();
    }

    private async Task SendAllChipsOutputsAsync()
    {
        for (int chip = 0; chip < Pc7866Commands.McpChipCount; chip++)
            await SendChipOutputsAsync(chip);
    }

    private async Task SendChipOutputsAsync(int chip)
    {
        ushort word = 0;
        int baseIdx = chip * Pc7866Commands.McpPinCount;
        for (int pin = 0; pin < Pc7866Commands.McpPinCount; pin++)
            if (_outputChecks[baseIdx + pin].Checked) word |= (ushort)(1 << pin);

        string cmd = Pc7866Commands.BuildOutputCommand(chip, word);
        lblOutputMask.Text = $"Última trama:  {cmd}";
        await SendAsync(cmd);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lectura analógica (R / F) de un único canal
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnReadRaw_Click(object? sender, EventArgs e)
    {
        int channel = (int)(cmbChannel.SelectedItem ?? 0);
        string response = await SendAsyncWithResult(Pc7866Commands.ReadRaw(channel));
        int? raw = _parser.ParseRawValue(response);
        lblRawValue.Text = $"RAW(canal {channel}): {(raw?.ToString() ?? "—")}";
    }

    private async void BtnReadFilteredSingle_Click(object? sender, EventArgs e)
    {
        int channel = (int)(cmbChannel.SelectedItem ?? 0);
        string response = await SendAsyncWithResult(Pc7866Commands.ReadFiltered(channel));
        float? filt = _parser.ParseFilteredValue(response);
        lblRawValue.Text = $"Filtrado(canal {channel}): {(filt?.ToString("F4", CultureInfo.InvariantCulture) ?? "—")} V";
    }

    /// <summary>Lee secuencialmente F0..F3 y calcula la resistencia (Vain=Ch0-Ch1, Ve=Ch2-Ch3).</summary>
    private async void BtnReadAllFiltered_Click(object? sender, EventArgs e)
    {
        var v = new double[4];
        for (int ch = 0; ch < 4; ch++)
        {
            string response = await SendAsyncWithResult(Pc7866Commands.ReadFiltered(ch));
            float? val = _parser.ParseFilteredValue(response);
            if (val is null)
            {
                AddLog($"⚠️ F{ch}: respuesta inesperada \u201c{response}\u201d", LogLevel.Warning);
                return;
            }
            v[ch] = val.Value;
        }

        double vain  = v[0] - v[1];
        double ve    = v[2] - v[3];
        double denom = ve - vain;
        double r     = CalcResistance(vain, denom);
        string rStr  = FormatResistance(r);

        lblVain.Text       = $"{vain:F4} V";
        lblVe.Text         = $"{ve:F4} V";
        lblDenom.Text      = $"{denom:F4} V";
        lblResistance.Text = $"{rStr} Ω";

        AddLog($"   Vain={vain:F4}V  Ve={ve:F4}V  R={rStr} Ω", LogLevel.Info);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // I – Configuración de placa
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SendBoardConfigAsync()
    {
        int numMcps = (int)nudNumMcps.Value;
        int? inh1 = ParseInhBox(txtInh1.Text);
        int? inh2 = ParseInhBox(txtInh2.Text);
        int? inh3 = ParseInhBox(txtInh3.Text);
        int? inh4 = ParseInhBox(txtInh4.Text);
        int muestras = (int)nudMuestras.Value;
        int retardo  = (int)nudRetardo.Value;

        string cmd = Pc7866Commands.BuildBoardConfigCommand(
            numMcps, inh1, inh2, inh3, inh4, txtBoardRef.Text, muestras, retardo);
        await SendAsync(cmd);
    }

    private static int? ParseInhBox(string text)
    {
        text = text.Trim().ToUpperInvariant();
        if (string.IsNullOrEmpty(text) || text == "N") return null;
        return int.TryParse(text, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int v) ? v : null;
    }

    private static string FormatInhBox(int? pos) => pos.HasValue ? pos.Value.ToString("X1") : "N";

    // ─────────────────────────────────────────────────────────────────────────
    // Semiautomático – elegir modelo (Referencia) + probar un solo contacto
    // ─────────────────────────────────────────────────────────────────────────

    private async Task TryInitDatabaseAsync()
    {
        try
        {
            _repository = new TestRepository(AppSettings.Instance.GetConnectionString());
            await _repository.TestConnectionAsync();
            await _repository.InitializeDatabaseAsync();
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
            cmbReferenciaManual.Items.Clear();
            foreach (var r in refs) cmbReferenciaManual.Items.Add(r);
            cmbReferenciaManual.DisplayMember = "ReferenciaNombre";
            if (cmbReferenciaManual.Items.Count > 0) cmbReferenciaManual.SelectedIndex = 0;
            AddLog($"📋 {cmbReferenciaManual.Items.Count} modelos cargados", LogLevel.Info);
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error cargando modelos: {ex.Message}", LogLevel.Error);
        }
    }

    /// <summary>Al elegir el modelo: carga sus contactos y envía la configuración de placa (comando I).</summary>
    private async Task OnReferenciaManualChangedAsync()
    {
        if (cmbReferenciaManual.SelectedItem is not Referencia ref_ || _repository is null) return;
        _referenciaActual = ref_;
        _parametrosManual = (await _repository.GetParametrosByReferenciaAsync(ref_.Id))
            .OrderBy(p => p.NPasoEnsayo).ToList();

        cmbContactoManual.Items.Clear();
        foreach (var p in _parametrosManual) cmbContactoManual.Items.Add(p);
        cmbContactoManual.DisplayMember = "NombreContacto";
        if (cmbContactoManual.Items.Count > 0) cmbContactoManual.SelectedIndex = 0;

        nudNumMcps.Value  = Math.Clamp(ref_.NumMcps, nudNumMcps.Minimum, nudNumMcps.Maximum);
        txtInh1.Text      = FormatInhBox(ref_.Inh1Pos);
        txtInh2.Text      = FormatInhBox(ref_.Inh2Pos);
        txtInh3.Text      = FormatInhBox(ref_.Inh3Pos);
        txtInh4.Text      = FormatInhBox(ref_.Inh4Pos);
        txtBoardRef.Text  = ref_.ModeloPlaca;
        nudMuestras.Value = Math.Clamp(ref_.Muestras, nudMuestras.Minimum, nudMuestras.Maximum);
        nudRetardo.Value  = Math.Clamp(ref_.RetardoMs, (int)nudRetardo.Minimum, (int)nudRetardo.Maximum);

        AddLog($"📝 Modelo: {ref_.ReferenciaNombre} — {_parametrosManual.Count} contactos", LogLevel.Info);
        await SendBoardConfigAsync();
    }

    /// <summary>Ejecuta el ensayo (resistencia + cortocircuito) de un único contacto, sin guardar informe en BD.</summary>
    private async void BtnProbarContacto_Click(object? sender, EventArgs e)
    {
        if (_referenciaActual is null || cmbContactoManual.SelectedItem is not ParametroEnsayo paso)
        {
            AddLog("⚠️ Seleccione un modelo y un contacto", LogLevel.Warning);
            return;
        }
        if (!_serialPort.IsOpen)
        {
            AddLog("⚠️ Puerto no abierto", LogLevel.Warning);
            return;
        }

        btnProbarContacto.Enabled  = false;
        lblSemiAutoResult.Text      = $"Probando {paso.NombreContacto}…";
        lblSemiAutoResult.ForeColor = Color.Gray;

        _semiAutoCts = new CancellationTokenSource();
        try
        {
            var resultado = await _stateMachine.RunAsync(
                _referenciaActual,
                new List<ParametroEnsayo> { paso },
                "Manual", string.Empty,
                _serialPort, _parser,
                progress: null,
                stepCompleted: null,
                AppSettings.Instance.DefaultTimeout,
                _semiAutoCts.Token,
                cmd => AddLog(cmd, LogLevel.Debug));

            var detalle = resultado.Detalles.FirstOrDefault();
            if (detalle is null)
            {
                lblSemiAutoResult.Text      = "⚠️ Sin resultado (revisar log)";
                lblSemiAutoResult.ForeColor = Color.Gray;
            }
            else
            {
                string rStr = detalle.ResistenciaMedida < 0f ? "∞" : detalle.ResistenciaMedida.ToString("F2");
                lblSemiAutoResult.Text = $"{paso.NombreContacto}: {detalle.Estado}   R = {rStr} Ω";
                lblSemiAutoResult.ForeColor = detalle.Estado switch
                {
                    EstadoMedicion.Ok            => Color.FromArgb(0, 140, 60),
                    EstadoMedicion.Nok           => Color.FromArgb(200, 40, 40),
                    EstadoMedicion.Cortocircuito => Color.FromArgb(230, 120, 0),
                    EstadoMedicion.Abierto       => Color.FromArgb(0, 90, 200),
                    _ => Color.Black
                };
            }
        }
        catch (OperationCanceledException)
        {
            lblSemiAutoResult.Text      = "⛔ Cancelado";
            lblSemiAutoResult.ForeColor = Color.Gray;
        }
        catch (Exception ex)
        {
            AddLog($"❌ {ex.Message}", LogLevel.Error);
            lblSemiAutoResult.Text      = "❌ Error";
            lblSemiAutoResult.ForeColor = Color.Red;
        }
        finally
        {
            btnProbarContacto.Enabled = true;
            _semiAutoCts?.Dispose();
            _semiAutoCts = null;
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Comunicación genérica
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SendAsync(string command)
    {
        await SendAsyncWithResult(command);
    }

    /// <summary>Envía y devuelve la respuesta en bruto (vacío si error).</summary>
    private async Task<string> SendAsyncWithResult(string command)
    {
        if (!_serialPort.IsOpen)
        {
            AddLog("⚠️ Puerto no abierto", LogLevel.Warning);
            return string.Empty;
        }

        try
        {
            AddLog($"➡️ TX: {command}", LogLevel.Info);
            string response = await _serialPort.SendCommandAsync(
                command, AppSettings.Instance.DefaultTimeout);
            AddLog($"⬅️ RX: {response}", LogLevel.Info);
            InterpretResponse(response);
            return response;
        }
        catch (TimeoutException)
        {
            AddLog("⏱️ Timeout: sin respuesta del dispositivo", LogLevel.Error);
            return string.Empty;
        }
        catch (Exception ex)
        {
            AddLog($"❌ Error: {ex.Message}", LogLevel.Error);
            Logger.Instance.Error($"Error enviando '{command}': {ex}");
            return string.Empty;
        }
    }

    private void InterpretResponse(string response)
    {
        if (string.IsNullOrWhiteSpace(response)) return;
        char first = response.Trim()[0];
        if (first == Pc7866Commands.RespOk)        AddLog("   ✅ OK", LogLevel.Info);
        else if (first == Pc7866Commands.RespNok)  AddLog("   ❌ NOK – fallo en el dispositivo", LogLevel.Warning);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers de resistencia
    // ─────────────────────────────────────────────────────────────────────────

    /// <summary>Calcula R = Vain / denom × 390. Devuelve +∞ si R <= 0 o R > 1000.</summary>
    internal static double CalcResistance(double vain, double denom)
    {
        if (denom <= 1e-9) return double.PositiveInfinity;
        double r = vain / denom * 390.0;
        return (r <= 0 || r > 1000.0) ? double.PositiveInfinity : r;
    }

    internal static string FormatResistance(double r)
        => double.IsInfinity(r) ? "∞" : $"{r:F2}";

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
        _semiAutoCts?.Cancel();
        if (_ownsSerialPort)
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Dispose();
        }
        _repository?.Dispose();
        base.OnHandleDestroyed(e);
    }
}
