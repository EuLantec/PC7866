using PC7866.Configuration;
using PC7866.Models;
using PC7866.Services.Database;
using PC7866.Services.SerialCommunication;
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
    private ITestRepository?            _repository;

    public ManualControlPanel(ISerialPortService? serialPort = null)
    {
        InitializeComponent();
        _serialPort = serialPort ?? new SerialPortService();
        _ownsSerialPort = serialPort is null;
        _parser     = new CommandParser();
        InitializeControls();
        AttachEventHandlers();
        _ = TryInitRepositoryAsync();
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
        
        BuildOutputMatrix();
        SetConnectedState(_serialPort.IsOpen);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Base de datos
    // ─────────────────────────────────────────────────────────────────────────

    private async Task TryInitRepositoryAsync()
    {
        try
        {
            _repository = new TestRepository(AppSettings.Instance.GetConnectionString());
            await _repository.TestConnectionAsync();
        }
        catch
        {
            _repository = null;
        }
    }

    private async Task SaveFullTestResultsAsync(List<FullTestRow> rows)
    {
        if (_repository is null) return;
        try
        {
            var resultado = new Resultado
            {
                ReferenciaId    = null,
                FechaPrueba     = DateTime.Now,
                ResultadoGlobal = true,
                Operario        = string.Empty,
                Lote            = string.Empty
            };
            int resultadoId = await _repository.InsertResultadoAsync(resultado);

            foreach (var row in rows)
            {
                float rMedida = double.IsInfinity(row.Resistance) || double.IsNaN(row.Resistance)
                    ? -1f
                    : (float)row.Resistance;

                var detalle = new ResultadoDetalle
                {
                    ResultadoId       = resultadoId,
                    ParametroEnsayoId = null,
                    NombreContacto    = $"S{row.Output:D2}",
                    NPasoEnsayo       = row.Output,
                    ResistenciaMedida = rMedida,
                    ValorRawVain      = row.Ain1Raw,
                    ValorRawVe        = row.Ain3Raw,
                    Resultado         = true,
                    Timestamp         = DateTime.Now
                };
                await _repository.InsertDetalleAsync(detalle);
            }
            AddLog($"💾 Test guardado en BD (id={resultadoId})", LogLevel.Info);
        }
        catch (Exception ex)
        {
            AddLog($"⚠️ No se pudo guardar en BD: {ex.Message}", LogLevel.Warning);
        }
    }

    private void LoadAvailablePorts()
    {        cmbPort.Items.Clear();
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
    /// Crea la cuadrícula 8 × 6 de 48 checkboxes dentro de pnlOutputMatrix.
    /// </summary>
    private void BuildOutputMatrix()
    {
        var tip = new ToolTip();
        const int cols = 8, cw = 86, ch = 24, gapX = 2, gapY = 2, padX = 4, padY = 4;

        for (int i = 0; i < Pc7866Commands.OutputCount; i++)
        {
            int col = i % cols, row = i / cols;
            var chk = new CheckBox
            {
                Text     = $"S{i + 1:D2}",
                Tag      = i,
                Size     = new Size(cw, ch),
                Location = new Point(padX + col * (cw + gapX), padY + row * (ch + gapY)),
                Font     = new Font("Segoe UI", 8.5f)
            };
            tip.SetToolTip(chk, $"Salida {i + 1}  –  bit {i}");
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
        btnDiagTotal.Click += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisTotal());
        btnDiag1.Click     += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.Diag1));
        btnDiag2.Click     += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.Diag2));
        btnDiag3.Click     += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.Diag3));
        btnDiag4.Click     += async (_, _) => await SendAsync(Pc7866Commands.DiagnosisSingle(Pc7866Commands.Diag4));

        // Salidas
        btnOutputsAllOn.Click  += (_, _) => SetAllOutputs(true);
        btnOutputsAllOff.Click += (_, _) => SetAllOutputs(false);
        btnFullTest.Click      += async (_, _) => await RunFullTestAsync();

        // Lectura analógica
        btnReadRaw.Click      += async (_, _) => await SendAsync(Pc7866Commands.ReadRaw());
        btnReadFiltered.Click += BtnReadFiltered_Click;

        // Filtros
        btnFilterFlags.Click += async (_, _) =>
            await SendAsync(Pc7866Commands.SetFilter(Pc7866Commands.FilterFlags, txtFilterFlags.Text.Trim().ToUpper()));
        for (int _fi = 0; _fi < Pc7866Commands.CoefCount; _fi++)
        {
            int idx = _fi; // captura
            _btnCoef[idx].Click += async (_, _) =>
                await SendAsync(Pc7866Commands.SetFilter(Pc7866Commands.CoefSubCmd(idx + 1), CoefInputToHex(_txtCoef[idx].Text)));
        }

        // Guardar – tras cada operación ejecutar GV y rellenar campos
        btnSaveWrite.Click += async (_, _) => { await SendAsync(Pc7866Commands.SaveCommand(Pc7866Commands.SaveWrite)); await SendGvAndPopulateAsync(); };
        btnSaveRead.Click  += async (_, _) => { await SendAsync(Pc7866Commands.SaveCommand(Pc7866Commands.SaveRead));  await SendGvAndPopulateAsync(); };
        btnSaveView.Click  += async (_, _) => await SendGvAndPopulateAsync();

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
        grpDiagnosis.Enabled  = connected;
        grpOutputs.Enabled    = connected;
        btnFullTest.Enabled   = connected;
        grpAnalog.Enabled     = connected;
        grpFilter.Enabled     = connected;
        grpSave.Enabled       = connected;
        grpReset.Enabled      = connected;

        if (connected) AddLog($"✅ Conectado: {_serialPort.CurrentPort}", LogLevel.Info);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Salidas (S)
    // ─────────────────────────────────────────────────────────────────────────

    private async void OutputCheck_Changed(object? sender, EventArgs e) => await SendOutputsAsync();

    private void SetAllOutputs(bool value)
    {
        foreach (var chk in _outputChecks) chk.CheckedChanged -= OutputCheck_Changed;
        foreach (var chk in _outputChecks) chk.Checked = value;
        foreach (var chk in _outputChecks) chk.CheckedChanged += OutputCheck_Changed;
        _ = SendOutputsAsync();
    }

    private async Task SendOutputsAsync()
    {
        bool[] states = _outputChecks.Select(c => c.Checked).ToArray();
        string cmd    = Pc7866Commands.BuildOutputsCommand(states);
        lblOutputMask.Text = $"Trama:  {cmd}";
        await SendAsync(cmd);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lectura analógica filtrada (F) con cálculo de R
    // ─────────────────────────────────────────────────────────────────────────

    private async void BtnReadFiltered_Click(object? sender, EventArgs e)
    {
        string response = await SendAsyncWithResult(Pc7866Commands.ReadFiltered());
        if (string.IsNullOrWhiteSpace(response)) return;

        // Respuesta esperada: "v1 v2 v3 v4"  (4 voltajes separados por espacio)
        double[] v = ParseAnalogResponse(response);
        if (v.Length < 4)
        {
            AddLog("⚠️ Respuesta F: formato inesperado (se esperan 4 valores)", LogLevel.Warning);
            return;
        }

        // Vain = señal diferencial canal 1 – canal 2
        double vain = v[0] - v[1];
        // Ve   = señal diferencial canal 3 – canal 4
        double ve   = v[2] - v[3];

        // R = Vain / (Ve – Vain) × 390 Ω
        double denom = ve - vain;
        double r = CalcResistance(vain, denom);
        string rStr = FormatResistance(r);

        // Actualizar panel de resultado
        lblVain.Text       = $"{vain:F4} V";
        lblVe.Text         = $"{ve:F4} V";
        lblDenom.Text      = $"{denom:F4} V";
        lblResistance.Text = $"{rStr} Ω";

        AddLog($"   Vain={vain:F4}V  Ve={ve:F4}V  R={rStr} Ω", LogLevel.Info);
    }

    /// <summary>
    /// Parsea "32767 49439 43245 63567" o "1.234 2.345 3.456 4.567" → double[]
    /// </summary>
    private static double[] ParseAnalogResponse(string response)
    {
        return response.Trim()
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? d : double.NaN)
            .Where(d => !double.IsNaN(d))
            .ToArray();
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
    // Helper coeficientes
    // ─────────────────────────────────────────────────────────────────────────

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
    // GV – leer parámetros RAM y rellenar campos
    // ─────────────────────────────────────────────────────────────────────────

    private async Task SendGvAndPopulateAsync()
    {
        string resp = await SendAsyncWithResult(Pc7866Commands.SaveCommand(Pc7866Commands.SaveView));
        if (string.IsNullOrWhiteSpace(resp)) return;

        // Respuesta esperada: 4 tokens hex de 4 dígitos (FLAGS Coef1 Coef2 Coef3)
        // Puede venir como "I0XXXX I1XXXX I2XXXX I3XXXX" o como "XXXX XXXX XXXX XXXX"
        string[] tokens = resp.Trim()
            .Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        string[] hexValues = tokens
            .Select(t => t.Length > 4 ? t[^4..].ToUpper() : t.ToUpper().PadLeft(4, '0'))
            .ToArray();

        if (hexValues.Length < 4)
        {
            AddLog($"⚠️ GV: respuesta inesperada: '{resp}'", LogLevel.Warning);
            return;
        }

        // Rellenar campos (en hilo UI)
        Invoke(() =>
        {
            txtFilterFlags.Text = hexValues[0];
            for (int _gi = 0; _gi < Pc7866Commands.CoefCount && _gi + 1 < hexValues.Length; _gi++)
                _txtCoef[_gi].Text = Pc7866Commands.HexToCoef(hexValues[_gi + 1])
                                         .ToString("F5", CultureInfo.InvariantCulture);
        });
        string coefLog = string.Join("  ",
            Enumerable.Range(0, Math.Min(Pc7866Commands.CoefCount, hexValues.Length - 1))
                      .Select(i => $"C{i + 1}={_txtCoef[i].Text}"));
        AddLog($"   FLAGS={hexValues[0]}  {coefLog}", LogLevel.Info);
    }

    private static string CoefInputToHex(string input)
    {
        input = input.Trim();
        if (input.Contains('.') || input.Contains(','))
        {
            if (double.TryParse(input.Replace(',', '.'),
                NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                return Pc7866Commands.CoefToHex(d);
        }
        if (input.Length <= 4) return input.ToUpper().PadLeft(4, '0');
        return input.ToUpper();
    }

    // ─────────────────────────────────────────────────────────────────────────────
    // Test completo manual (48 salidas)
    // ─────────────────────────────────────────────────────────────────────────────

    private async Task RunFullTestAsync()
    {
        if (!_serialPort.IsOpen)
        {
            AddLog("⚠️ Puerto no abierto", LogLevel.Warning);
            return;
        }

        btnFullTest.Enabled = false;
        AddLog("🔍 Iniciando test completo…", LogLevel.Info);

        var results = new List<Models.FullTestRow>(Pc7866Commands.OutputCount);

        for (int i = 0; i < Pc7866Commands.OutputCount; i++)
        {
            // Activar única salida i
            bool[] states = new bool[Pc7866Commands.OutputCount];
            states[i] = true;

            string outCmd = Pc7866Commands.BuildOutputsCommand(states);
            AddLog($"▶️  Salida {i + 1:D2} → {outCmd}", LogLevel.Info);
            await _serialPort.SendCommandAsync(outCmd, AppSettings.Instance.DefaultTimeout);

            await Task.Delay(3);

            var row = new Models.FullTestRow { Output = i + 1 };

            // Leer RAW
            string rawResp = await _serialPort.SendCommandAsync(
                Pc7866Commands.ReadRaw(), AppSettings.Instance.DefaultTimeout);
            int[] rawVals = ParseIntAnalogResponse(rawResp);
            if (rawVals.Length >= 4)
            {
                row.Ain1Raw = rawVals[0];
                row.Ain2Raw = rawVals[1];
                row.Ain3Raw = rawVals[2];
                row.Ain4Raw = rawVals[3];
            }
            else
            {
                row.Error = $"RAW: respuesta inesperada \u201c{rawResp}\u201d";
            }

            // Leer Filtrado
            string filtResp = await _serialPort.SendCommandAsync(
                Pc7866Commands.ReadFiltered(), AppSettings.Instance.DefaultTimeout);
            double[] filtVals = ParseAnalogResponse(filtResp);
            if (filtVals.Length >= 4)
            {
                row.Ain1Filt = (int)filtVals[0];
                row.Ain2Filt = (int)filtVals[1];
                row.Ain3Filt = (int)filtVals[2];
                row.Ain4Filt = (int)filtVals[3];

                row.Vain = filtVals[0] - filtVals[1];
                row.Ve   = filtVals[2] - filtVals[3];
                double denom = row.Ve - row.Vain;
                row.Resistance = CalcResistance(row.Vain, denom);
            }
            else if (string.IsNullOrEmpty(row.Error))
            {
                row.Error = $"FILT: respuesta inesperada \u201c{filtResp}\u201d";
            }

            results.Add(row);
            AddLog($"   S{i + 1:D2}: Vain={row.Vain:F4}V  Ve={row.Ve:F4}V  R={FormatResistance(row.Resistance)} Ω", LogLevel.Info);
        }

        // Apagar todas las salidas al terminar
        await _serialPort.SendCommandAsync(
            Pc7866Commands.BuildOutputsCommand(new bool[Pc7866Commands.OutputCount]),
            AppSettings.Instance.DefaultTimeout);
        foreach (var chk in _outputChecks) { chk.CheckedChanged -= OutputCheck_Changed; chk.Checked = false; chk.CheckedChanged += OutputCheck_Changed; }
        lblOutputMask.Text = "Trama:  S000000000000";

        AddLog($"✅ Test completo finalizado. {results.Count} salidas medidas.", LogLevel.Info);
        await SaveFullTestResultsAsync(results);
        btnFullTest.Enabled = true;

        // Mostrar informe
        using var form = new FullTestReportForm(results);
        form.ShowDialog(ParentForm as Form ?? (IWin32Window)this);
    }

    /// <summary>Parsea respuesta RAW como enteros (ej. "32767 49439 43245 63567").</summary>
    private static int[] ParseIntAnalogResponse(string response)
    {
        return response.Trim()
            .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => int.TryParse(s, out int v) ? v : -1)
            .Where(v => v >= 0)
            .ToArray();
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
        if (_ownsSerialPort)
        {
            if (_serialPort.IsOpen) _serialPort.Close();
            _serialPort.Dispose();
        }
        base.OnHandleDestroyed(e);
    }
}
