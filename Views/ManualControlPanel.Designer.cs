using PC7866.Models;

namespace PC7866.Views;

partial class ManualControlPanel
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code
    private void InitializeComponent()
    {
        // ── Connection bar ─────────────────────────────────────────────────
        pnlTopBar       = new System.Windows.Forms.Panel();
        lblPort         = new System.Windows.Forms.Label();
        cmbPort         = new System.Windows.Forms.ComboBox();
        btnRefreshPorts = new System.Windows.Forms.Button();
        lblBaudRate     = new System.Windows.Forms.Label();
        cmbBaudRate     = new System.Windows.Forms.ComboBox();
        btnConnect      = new System.Windows.Forms.Button();
        btnDisconnect   = new System.Windows.Forms.Button();
        lblConnStatus   = new System.Windows.Forms.Label();

        // ── Main layout ────────────────────────────────────────────────────
        tableMain       = new System.Windows.Forms.TableLayoutPanel();

        // ── Col izquierda: Diagnosis + M + P + Salidas ─────────────────────
        grpDiagnosis    = new System.Windows.Forms.GroupBox();
        btnDiagTotal    = new System.Windows.Forms.Button();
        btnDiagAds      = new System.Windows.Forms.Button();
        btnDiagVersion  = new System.Windows.Forms.Button();
        btnDiagReadConfig = new System.Windows.Forms.Button();
        btnDiagTemperature = new System.Windows.Forms.Button();
        _btnDiagMcp     = new System.Windows.Forms.Button[Pc7866Commands.McpChipCount];
        for (int _i = 0; _i < Pc7866Commands.McpChipCount; _i++)
            _btnDiagMcp[_i] = new System.Windows.Forms.Button();

        grpMcpMode      = new System.Windows.Forms.GroupBox();
        lblMcpModeAddr  = new System.Windows.Forms.Label();
        cmbMcpModeAddr  = new System.Windows.Forms.ComboBox();
        rbModeOutput    = new System.Windows.Forms.RadioButton();
        rbModeInput     = new System.Windows.Forms.RadioButton();
        lblMcpModeMask  = new System.Windows.Forms.Label();
        txtMcpModeMask  = new System.Windows.Forms.TextBox();
        btnSendMcpMode  = new System.Windows.Forms.Button();

        grpTrack        = new System.Windows.Forms.GroupBox();
        lblTrack        = new System.Windows.Forms.Label();
        nudTrack        = new System.Windows.Forms.NumericUpDown();
        btnSelectTrack  = new System.Windows.Forms.Button();
        lblTrackHint    = new System.Windows.Forms.Label();

        grpOutputs      = new System.Windows.Forms.GroupBox();
        pnlOutputMatrix = new System.Windows.Forms.Panel();
        lblOutputMask   = new System.Windows.Forms.Label();
        btnOutputsAllOn = new System.Windows.Forms.Button();
        btnOutputsAllOff= new System.Windows.Forms.Button();

        // ── Col derecha: Analógica + Config placa + Reset ──────────────────
        grpAnalog       = new System.Windows.Forms.GroupBox();
        lblAnalogTitle  = new System.Windows.Forms.Label();
        lblChannel      = new System.Windows.Forms.Label();
        cmbChannel      = new System.Windows.Forms.ComboBox();
        btnReadRaw      = new System.Windows.Forms.Button();
        lblRawValue     = new System.Windows.Forms.Label();
        btnReadFiltered = new System.Windows.Forms.Button();
        lblFilteredValue= new System.Windows.Forms.Label();
        btnReadAllFiltered = new System.Windows.Forms.Button();
        // Resultado R
        tableResult     = new System.Windows.Forms.TableLayoutPanel();
        lblVainLbl      = new System.Windows.Forms.Label();
        lblVain         = new System.Windows.Forms.Label();
        lblVeLbl        = new System.Windows.Forms.Label();
        lblVe           = new System.Windows.Forms.Label();
        lblDenomLbl     = new System.Windows.Forms.Label();
        lblDenom        = new System.Windows.Forms.Label();
        lblResistanceLbl= new System.Windows.Forms.Label();
        lblResistance   = new System.Windows.Forms.Label();
        lblFormula      = new System.Windows.Forms.Label();

        grpBoardConfig    = new System.Windows.Forms.GroupBox();
        lblNumMcps        = new System.Windows.Forms.Label();
        nudNumMcps        = new System.Windows.Forms.NumericUpDown();
        lblInh            = new System.Windows.Forms.Label();
        txtInh1           = new System.Windows.Forms.TextBox();
        txtInh2           = new System.Windows.Forms.TextBox();
        txtInh3           = new System.Windows.Forms.TextBox();
        txtInh4           = new System.Windows.Forms.TextBox();
        lblBoardRef       = new System.Windows.Forms.Label();
        txtBoardRef       = new System.Windows.Forms.TextBox();
        lblMuestras       = new System.Windows.Forms.Label();
        nudMuestras       = new System.Windows.Forms.NumericUpDown();
        lblRetardo        = new System.Windows.Forms.Label();
        nudRetardo        = new System.Windows.Forms.NumericUpDown();
        btnSendBoardConfig= new System.Windows.Forms.Button();

        // ── Semiautomático ───────────────────────────────────────────
        grpSemiAuto           = new System.Windows.Forms.GroupBox();
        lblRefManual          = new System.Windows.Forms.Label();
        cmbReferenciaManual   = new System.Windows.Forms.ComboBox();
        btnRefreshRefsManual  = new System.Windows.Forms.Button();
        lblContactoManual     = new System.Windows.Forms.Label();
        cmbContactoManual     = new System.Windows.Forms.ComboBox();
        btnProbarContacto     = new System.Windows.Forms.Button();
        lblSemiAutoResult     = new System.Windows.Forms.Label();

        grpReset        = new System.Windows.Forms.GroupBox();
        btnReset        = new System.Windows.Forms.Button();

        // ── Log ────────────────────────────────────────────────────────────
        grpLog          = new System.Windows.Forms.GroupBox();
        txtLog          = new System.Windows.Forms.TextBox();
        btnClearLog     = new System.Windows.Forms.Button();

        pnlTopBar.SuspendLayout();
        tableMain.SuspendLayout();
        grpDiagnosis.SuspendLayout();
        grpMcpMode.SuspendLayout();
        grpTrack.SuspendLayout();
        grpOutputs.SuspendLayout();
        grpAnalog.SuspendLayout();
        tableResult.SuspendLayout();
        grpBoardConfig.SuspendLayout();
        grpReset.SuspendLayout();
        grpLog.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)nudTrack).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudNumMcps).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMuestras).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudRetardo).BeginInit();
        SuspendLayout();

        // ═══════════════════════════════════════════════════════════════════
        // TOP BAR – conexión serie
        // ═══════════════════════════════════════════════════════════════════
        pnlTopBar.Dock      = System.Windows.Forms.DockStyle.Top;
        pnlTopBar.Height    = 46;
        pnlTopBar.BackColor = System.Drawing.Color.FromArgb(235, 238, 245);

        lblPort.Text     = "Puerto:";
        lblPort.AutoSize = true;
        lblPort.Location = new System.Drawing.Point(10, 14);
        lblPort.Font     = new System.Drawing.Font("Segoe UI", 9f);

        cmbPort.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbPort.Location      = new System.Drawing.Point(62, 11);
        cmbPort.Size          = new System.Drawing.Size(110, 24);

        btnRefreshPorts.Text     = "🔄";
        btnRefreshPorts.Location = new System.Drawing.Point(178, 10);
        btnRefreshPorts.Size     = new System.Drawing.Size(28, 26);
        btnRefreshPorts.UseVisualStyleBackColor = true;
        btnRefreshPorts.Tag      = "native";

        lblBaudRate.Text     = "Baudios:";
        lblBaudRate.AutoSize = true;
        lblBaudRate.Location = new System.Drawing.Point(216, 14);
        lblBaudRate.Font     = new System.Drawing.Font("Segoe UI", 9f);

        cmbBaudRate.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbBaudRate.Location      = new System.Drawing.Point(274, 11);
        cmbBaudRate.Size          = new System.Drawing.Size(100, 24);

        btnConnect.Text      = "Conectar";
        btnConnect.Location  = new System.Drawing.Point(388, 10);
        btnConnect.Size      = new System.Drawing.Size(90, 26);
        btnConnect.BackColor = System.Drawing.Color.FromArgb(0, 153, 76);
        btnConnect.ForeColor = System.Drawing.Color.White;
        btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnConnect.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        btnDisconnect.Text      = "Desconectar";
        btnDisconnect.Enabled   = false;
        btnDisconnect.Location  = new System.Drawing.Point(484, 10);
        btnDisconnect.Size      = new System.Drawing.Size(100, 26);
        btnDisconnect.BackColor = System.Drawing.Color.FromArgb(180, 50, 50);
        btnDisconnect.ForeColor = System.Drawing.Color.White;
        btnDisconnect.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnDisconnect.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblConnStatus.Text      = "○ Desconectado";
        lblConnStatus.ForeColor = System.Drawing.Color.Red;
        lblConnStatus.AutoSize  = true;
        lblConnStatus.Font      = new System.Drawing.Font("Segoe UI", 10f, System.Drawing.FontStyle.Bold);
        lblConnStatus.Location  = new System.Drawing.Point(600, 13);

        pnlTopBar.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblPort, cmbPort, btnRefreshPorts, lblBaudRate, cmbBaudRate,
              btnConnect, btnDisconnect, lblConnStatus });

        // ═══════════════════════════════════════════════════════════════════
        // tableMain  – 2 columnas, 2 filas (comandos | log)
        // ═══════════════════════════════════════════════════════════════════
        tableMain.Dock        = System.Windows.Forms.DockStyle.Fill;
        tableMain.ColumnCount = 2;
        tableMain.RowCount    = 2;
        tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(
            System.Windows.Forms.SizeType.Percent, 58f));
        tableMain.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(
            System.Windows.Forms.SizeType.Percent, 42f));
        tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(
            System.Windows.Forms.SizeType.Percent, 68f));
        tableMain.RowStyles.Add(new System.Windows.Forms.RowStyle(
            System.Windows.Forms.SizeType.Percent, 32f));
        tableMain.Padding = new System.Windows.Forms.Padding(4);

        // ── Panel izquierdo: contenedor vertical de Diagnosis + M + P + Salidas ─
        // AutoScroll evita que los controles queden recortados/invisibles si la ventana no es lo bastante alta.
        var pnlLeft = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Padding = new System.Windows.Forms.Padding(0, 0, 4, 0),
            AutoScroll = true
        };

        // ── Panel derecho: contenedor vertical de Analógica + Config placa + Reset
        var pnlRight = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Padding = new System.Windows.Forms.Padding(4, 0, 0, 0),
            AutoScroll = true
        };

        void StyleBtn(System.Windows.Forms.Button b, string text, int x, int y, int w = 130)
        {
            b.Text      = text;
            b.Location  = new System.Drawing.Point(x, y);
            b.Size      = new System.Drawing.Size(w, 28);
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
        }

        // ═══════════════════════════════════════════════════════════════════
        // grpDiagnosis
        // ═══════════════════════════════════════════════════════════════════
        grpDiagnosis.Text    = "D – Diagnosis";
        grpDiagnosis.Dock    = System.Windows.Forms.DockStyle.Top;
        grpDiagnosis.Height  = 118;
        grpDiagnosis.Enabled = false;
        grpDiagnosis.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        void StyleDiagBtn(System.Windows.Forms.Button b, string text, int x, int y, int w = 130)
        {
            StyleBtn(b, text, x, y, w);
            b.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
            b.ForeColor = System.Drawing.Color.White;
            b.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
        }

        StyleDiagBtn(btnDiagTotal, "DT – Total", 6, 22, 130);
        StyleDiagBtn(btnDiagAds,        "D1 – ADS 0x48", 142, 22, 130);
        StyleDiagBtn(btnDiagVersion,    "DV – Versión",  278, 22, 110);
        StyleDiagBtn(btnDiagReadConfig, "DG – Ver config", 394, 22, 130);
        StyleDiagBtn(btnDiagTemperature, "DC – Temperatura", 534, 22, 130);

        for (int _di = 0; _di < Pc7866Commands.McpChipCount; _di++)
        {
            int addr = 0x20 + _di;
            StyleDiagBtn(_btnDiagMcp[_di], $"D{_di + 2} – MCP 0x{addr:X2}", 6 + _di * 116, 58, 110);
        }

        var diagControls = new System.Windows.Forms.Control[5 + Pc7866Commands.McpChipCount];
        diagControls[0] = btnDiagTotal;
        diagControls[1] = btnDiagAds;
        diagControls[2] = btnDiagVersion;
        diagControls[3] = btnDiagReadConfig;
        diagControls[4] = btnDiagTemperature;
        for (int _di = 0; _di < Pc7866Commands.McpChipCount; _di++)
            diagControls[5 + _di] = _btnDiagMcp[_di];
        grpDiagnosis.Controls.AddRange(diagControls);

        // ═══════════════════════════════════════════════════════════════════
        // grpMcpMode  (M – dirección de pines)
        // ═══════════════════════════════════════════════════════════════════
        grpMcpMode.Text    = "M – Configuración de dirección (E/S)";
        grpMcpMode.Dock    = System.Windows.Forms.DockStyle.Top;
        grpMcpMode.Height  = 116;
        grpMcpMode.Enabled = false;
        grpMcpMode.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblMcpModeAddr.Text     = "Dirección MCP:";
        lblMcpModeAddr.AutoSize = true;
        lblMcpModeAddr.Location = new System.Drawing.Point(6, 26);
        lblMcpModeAddr.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        cmbMcpModeAddr.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbMcpModeAddr.Location      = new System.Drawing.Point(120, 22);
        cmbMcpModeAddr.Size          = new System.Drawing.Size(130, 24);

        rbModeOutput.Text     = "Salida (S)";
        rbModeOutput.Checked  = true;
        rbModeOutput.AutoSize = true;
        rbModeOutput.Location = new System.Drawing.Point(270, 24);
        rbModeOutput.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        rbModeInput.Text     = "Entrada (E)";
        rbModeInput.AutoSize = true;
        rbModeInput.Location = new System.Drawing.Point(390, 24);
        rbModeInput.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        lblMcpModeMask.Text     = "Máscara (hex 4):";
        lblMcpModeMask.AutoSize = true;
        lblMcpModeMask.Location = new System.Drawing.Point(6, 60);
        lblMcpModeMask.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        txtMcpModeMask.Text           = "0000";
        txtMcpModeMask.Location       = new System.Drawing.Point(120, 56);
        txtMcpModeMask.Size           = new System.Drawing.Size(90, 24);
        txtMcpModeMask.MaxLength      = 4;
        txtMcpModeMask.CharacterCasing= System.Windows.Forms.CharacterCasing.Upper;
        txtMcpModeMask.Font           = new System.Drawing.Font("Consolas", 9f);

        StyleBtn(btnSendMcpMode, "Enviar M", 230, 55, 130);

        grpMcpMode.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblMcpModeAddr, cmbMcpModeAddr, rbModeOutput, rbModeInput,
              lblMcpModeMask, txtMcpModeMask, btnSendMcpMode });

        // ═══════════════════════════════════════════════════════════════════
        // grpTrack  (P – selección de pista)
        // ═══════════════════════════════════════════════════════════════════
        grpTrack.Text    = "P – Selección de pista (multiplexores)";
        grpTrack.Dock    = System.Windows.Forms.DockStyle.Top;
        grpTrack.Height  = 78;
        grpTrack.Enabled = false;
        grpTrack.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblTrack.Text     = "Pista (0-48):";
        lblTrack.AutoSize = true;
        lblTrack.Location = new System.Drawing.Point(6, 26);
        lblTrack.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        nudTrack.Location = new System.Drawing.Point(100, 22);
        nudTrack.Size     = new System.Drawing.Size(70, 24);
        nudTrack.Minimum  = 0;
        nudTrack.Maximum  = Pc7866Commands.MaxTrackNumber;

        StyleBtn(btnSelectTrack, "Enviar P", 182, 21, 120);

        lblTrackHint.Text      = "P00 desconecta el mux del ADS";
        lblTrackHint.AutoSize  = true;
        lblTrackHint.Location  = new System.Drawing.Point(312, 26);
        lblTrackHint.Font      = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic);
        lblTrackHint.ForeColor = System.Drawing.Color.Gray;

        grpTrack.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblTrack, nudTrack, btnSelectTrack, lblTrackHint });

        // ═══════════════════════════════════════════════════════════════════
        // grpOutputs  (Salidas S)
        // ═══════════════════════════════════════════════════════════════════
        grpOutputs.Text    = $"S – Activación de salidas  ({Pc7866Commands.OutputCount} canales, 6 × MCP23017)";
        grpOutputs.Dock    = System.Windows.Forms.DockStyle.Fill;
        grpOutputs.Enabled = false;
        grpOutputs.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblOutputMask.Text      = "Trama:  —";
        lblOutputMask.AutoSize  = true;
        lblOutputMask.Location  = new System.Drawing.Point(6, 22);
        lblOutputMask.Font      = new System.Drawing.Font("Consolas", 9.5f, System.Drawing.FontStyle.Bold);
        lblOutputMask.ForeColor = System.Drawing.Color.FromArgb(0, 80, 160);

        btnOutputsAllOn.Text      = "Todo ON";
        btnOutputsAllOn.Location  = new System.Drawing.Point(260, 20);
        btnOutputsAllOn.Size      = new System.Drawing.Size(80, 24);
        btnOutputsAllOn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnOutputsAllOn.Font      = new System.Drawing.Font("Segoe UI", 8.5f);

        btnOutputsAllOff.Text      = "Todo OFF";
        btnOutputsAllOff.Location  = new System.Drawing.Point(346, 20);
        btnOutputsAllOff.Size      = new System.Drawing.Size(80, 24);
        btnOutputsAllOff.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnOutputsAllOff.Font      = new System.Drawing.Font("Segoe UI", 8.5f);

        pnlOutputMatrix.Location  = new System.Drawing.Point(6, 48);
        pnlOutputMatrix.AutoSize  = true;

        grpOutputs.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblOutputMask, btnOutputsAllOn, btnOutputsAllOff, pnlOutputMatrix });

        // Apilar izquierda: Dock=Top se añade después → queda encima
        pnlLeft.Controls.Add(grpOutputs);
        pnlLeft.Controls.Add(grpTrack);
        pnlLeft.Controls.Add(grpMcpMode);
        pnlLeft.Controls.Add(grpDiagnosis);

        // ═══════════════════════════════════════════════════════════════════
        // grpAnalog  (R / F + resultado)
        // ═══════════════════════════════════════════════════════════════════
        grpAnalog.Text    = "R / F – Lecturas analógicas";
        grpAnalog.Dock    = System.Windows.Forms.DockStyle.Top;
        grpAnalog.Height  = 260;
        grpAnalog.Enabled = false;
        grpAnalog.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblAnalogTitle.Text      = "Canales ADS: Ch0 – Ch1 = Vain  |  Ch2 – Ch3 = Ve";
        lblAnalogTitle.AutoSize  = true;
        lblAnalogTitle.Location  = new System.Drawing.Point(6, 22);
        lblAnalogTitle.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
        lblAnalogTitle.ForeColor = System.Drawing.Color.Gray;

        lblChannel.Text     = "Canal (0-3):";
        lblChannel.AutoSize = true;
        lblChannel.Location = new System.Drawing.Point(6, 50);
        lblChannel.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        cmbChannel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbChannel.Location      = new System.Drawing.Point(96, 46);
        cmbChannel.Size          = new System.Drawing.Size(60, 24);

        StyleBtn(btnReadRaw,      "R – Leer RAW",      166, 45, 130);
        StyleBtn(btnReadFiltered, "F – Leer Filtrado", 302, 45, 140);
        btnReadFiltered.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnReadFiltered.ForeColor = System.Drawing.Color.White;
        btnReadFiltered.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblRawValue.Text      = "RAW: —   Filtrado: —";
        lblRawValue.AutoSize  = true;
        lblRawValue.Location  = new System.Drawing.Point(6, 78);
        lblRawValue.Font      = new System.Drawing.Font("Consolas", 9.5f, System.Drawing.FontStyle.Bold);
        lblRawValue.ForeColor = System.Drawing.Color.FromArgb(0, 80, 160);
        lblFilteredValue.Visible = false; // el valor se muestra en lblRawValue

        StyleBtn(btnReadAllFiltered, "F0..F3 → calcular R", 6, 100, 200);
        btnReadAllFiltered.BackColor = System.Drawing.Color.FromArgb(0, 153, 76);
        btnReadAllFiltered.ForeColor = System.Drawing.Color.White;
        btnReadAllFiltered.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        // Tabla de resultado de R
        tableResult.Location     = new System.Drawing.Point(6, 132);
        tableResult.Size         = new System.Drawing.Size(480, 108);
        tableResult.ColumnCount  = 4;
        tableResult.RowCount     = 3;
        tableResult.BackColor    = System.Drawing.Color.FromArgb(245, 247, 252);
        tableResult.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
        tableResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22f));
        tableResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28f));
        tableResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 22f));
        tableResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 28f));

        void LblCaption(System.Windows.Forms.Label l, string text)
        {
            l.Text      = text;
            l.AutoSize  = false;
            l.Dock      = System.Windows.Forms.DockStyle.Fill;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            l.Font      = new System.Drawing.Font("Segoe UI", 9f);
            l.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);
            l.Padding   = new System.Windows.Forms.Padding(4, 0, 4, 0);
        }

        void LblValue(System.Windows.Forms.Label l, string text)
        {
            l.Text      = text;
            l.AutoSize  = false;
            l.Dock      = System.Windows.Forms.DockStyle.Fill;
            l.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            l.Font      = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);
            l.ForeColor = System.Drawing.Color.FromArgb(0, 80, 160);
            l.Padding   = new System.Windows.Forms.Padding(6, 0, 0, 0);
        }

        LblCaption(lblVainLbl,       "Vain (Ch0–Ch1):");
        LblValue  (lblVain,          "—");
        LblCaption(lblVeLbl,         "Ve (Ch2–Ch3):");
        LblValue  (lblVe,            "—");
        LblCaption(lblDenomLbl,      "Ve – Vain:");
        LblValue  (lblDenom,         "—");
        LblCaption(lblResistanceLbl, "R  =  Vain/(Ve–Vain)×390 Ω:");
        LblValue  (lblResistance,    "—");
        lblResistance.Font      = new System.Drawing.Font("Consolas", 12f, System.Drawing.FontStyle.Bold);
        lblResistance.ForeColor = System.Drawing.Color.FromArgb(180, 50, 0);

        lblFormula.Text      = "R = Vain / (Ve – Vain) × 390 Ω";
        lblFormula.AutoSize  = true;
        lblFormula.Location  = new System.Drawing.Point(230, 104);
        lblFormula.Font      = new System.Drawing.Font("Segoe UI", 8.5f, System.Drawing.FontStyle.Italic);
        lblFormula.ForeColor = System.Drawing.Color.Gray;

        tableResult.Controls.Add(lblVainLbl,       0, 0);
        tableResult.Controls.Add(lblVain,          1, 0);
        tableResult.Controls.Add(lblVeLbl,         2, 0);
        tableResult.Controls.Add(lblVe,            3, 0);
        tableResult.Controls.Add(lblDenomLbl,      0, 1);
        tableResult.Controls.Add(lblDenom,         1, 1);
        tableResult.Controls.Add(lblResistanceLbl, 0, 2);
        tableResult.SetColumnSpan(lblResistanceLbl, 1);
        tableResult.Controls.Add(lblResistance,    1, 2);
        tableResult.SetColumnSpan(lblResistance,   3);

        grpAnalog.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblAnalogTitle, lblChannel, cmbChannel, btnReadRaw, btnReadFiltered,
              lblRawValue, lblFilteredValue, btnReadAllFiltered, tableResult, lblFormula });

        // ═══════════════════════════════════════════════════════════════════
        // grpBoardConfig  (I – configuración de placa)
        // ═══════════════════════════════════════════════════════════════════
        grpBoardConfig.Text    = "I – Configuración de placa";
        grpBoardConfig.Dock    = System.Windows.Forms.DockStyle.Top;
        grpBoardConfig.Height  = 226;
        grpBoardConfig.Enabled = false;
        grpBoardConfig.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        int bx = 6, blx = 190, bw = 120, bh = 24;
        void BoardRow(System.Windows.Forms.Label l, string text, System.Windows.Forms.Control c, int y)
        {
            l.Text      = text;
            l.AutoSize  = true;
            l.Location  = new System.Drawing.Point(bx, y + 3);
            l.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
            c.Location  = new System.Drawing.Point(blx, y);
        }

        nudNumMcps.Size    = new System.Drawing.Size(bw, bh);
        nudNumMcps.Minimum = 0;
        nudNumMcps.Maximum = Pc7866Commands.McpChipCount;
        nudNumMcps.Value   = Pc7866Commands.McpChipCount;
        BoardRow(lblNumMcps, "Nº MCPs (0-6):", nudNumMcps, 22);

        lblInh.Text     = "INH1-4 pos (hex/N):";
        lblInh.AutoSize = true;
        lblInh.Location = new System.Drawing.Point(bx, 52);
        lblInh.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        void InhBox(System.Windows.Forms.TextBox t, int x)
        {
            t.Text      = "N";
            t.Location  = new System.Drawing.Point(x, 48);
            t.Size      = new System.Drawing.Size(40, 24);
            t.MaxLength = 1;
            t.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
            t.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            t.Font      = new System.Drawing.Font("Consolas", 9f);
        }
        InhBox(txtInh1, blx);
        InhBox(txtInh2, blx + 48);
        InhBox(txtInh3, blx + 96);
        InhBox(txtInh4, blx + 144);

        txtBoardRef.Location  = new System.Drawing.Point(blx, 78);
        txtBoardRef.Size      = new System.Drawing.Size(bw, bh);
        txtBoardRef.MaxLength = Pc7866Commands.BoardReferenceLength;
        BoardRow(lblBoardRef, "Referencia (7 car.):", txtBoardRef, 82);

        nudMuestras.Size    = new System.Drawing.Size(bw, bh);
        nudMuestras.Minimum = 0;
        nudMuestras.Maximum = 99;
        nudMuestras.Value   = 1;
        BoardRow(lblMuestras, "Muestras (0-99):", nudMuestras, 112);

        nudRetardo.Size    = new System.Drawing.Size(bw, bh);
        nudRetardo.Minimum = 0;
        nudRetardo.Maximum = 999;
        nudRetardo.Value   = 0;
        BoardRow(lblRetardo, "Retardo ms (0-999):", nudRetardo, 142);

        StyleBtn(btnSendBoardConfig, "Enviar I", 6, 178, 160);
        btnSendBoardConfig.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnSendBoardConfig.ForeColor = System.Drawing.Color.White;

        grpBoardConfig.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblNumMcps, nudNumMcps, lblInh, txtInh1, txtInh2, txtInh3, txtInh4,
              lblBoardRef, txtBoardRef, lblMuestras, nudMuestras, lblRetardo, nudRetardo,
              btnSendBoardConfig });

        // ═══════════════════════════════════════════════════════════════════        // grpSemiAuto  (elegir modelo → envía config "I"  +  probar un solo contacto)
        // ═════════════════════════════════════════════════════════════════════════
        grpSemiAuto.Text    = "Semiautomático – probar un contacto";
        grpSemiAuto.Dock    = System.Windows.Forms.DockStyle.Top;
        grpSemiAuto.Height  = 172;
        grpSemiAuto.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblRefManual.Text     = "Modelo:";
        lblRefManual.AutoSize = true;
        lblRefManual.Location = new System.Drawing.Point(6, 26);
        lblRefManual.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        cmbReferenciaManual.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbReferenciaManual.Location      = new System.Drawing.Point(90, 22);
        cmbReferenciaManual.Size          = new System.Drawing.Size(280, 24);

        btnRefreshRefsManual.Text     = "🔄";
        btnRefreshRefsManual.Location = new System.Drawing.Point(378, 21);
        btnRefreshRefsManual.Size     = new System.Drawing.Size(28, 26);
        btnRefreshRefsManual.UseVisualStyleBackColor = true;
        btnRefreshRefsManual.Tag      = "native";

        lblContactoManual.Text     = "Contacto:";
        lblContactoManual.AutoSize = true;
        lblContactoManual.Location = new System.Drawing.Point(6, 58);
        lblContactoManual.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

        cmbContactoManual.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        cmbContactoManual.Location      = new System.Drawing.Point(90, 54);
        cmbContactoManual.Size          = new System.Drawing.Size(180, 24);

        btnProbarContacto.Text      = "▶ Probar contacto (R + Cortocircuito)";
        btnProbarContacto.Location  = new System.Drawing.Point(6, 90);
        btnProbarContacto.Size      = new System.Drawing.Size(300, 30);
        btnProbarContacto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnProbarContacto.BackColor = System.Drawing.Color.FromArgb(0, 153, 76);
        btnProbarContacto.ForeColor = System.Drawing.Color.White;
        btnProbarContacto.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblSemiAutoResult.Text      = "Resultado: —";
        lblSemiAutoResult.AutoSize  = true;
        lblSemiAutoResult.Location  = new System.Drawing.Point(6, 130);
        lblSemiAutoResult.Font      = new System.Drawing.Font("Consolas", 10f, System.Drawing.FontStyle.Bold);
        lblSemiAutoResult.ForeColor = System.Drawing.Color.FromArgb(80, 80, 80);

        grpSemiAuto.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblRefManual, cmbReferenciaManual, btnRefreshRefsManual,
              lblContactoManual, cmbContactoManual, btnProbarContacto, lblSemiAutoResult });

        // ═════════════════════════════════════════════════════════════════════════        // grpReset
        // ═══════════════════════════════════════════════════════════════════
        grpReset.Text    = "Q – Reset";
        grpReset.Dock    = System.Windows.Forms.DockStyle.Top;
        grpReset.Height  = 62;
        grpReset.Enabled = false;
        grpReset.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        btnReset.Text      = "Q – Reiniciar microcontrolador";
        btnReset.Location  = new System.Drawing.Point(6, 22);
        btnReset.Size      = new System.Drawing.Size(220, 28);
        btnReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnReset.ForeColor = System.Drawing.Color.DarkRed;
        btnReset.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        grpReset.Controls.Add(btnReset);

        // Apilar derecha (de abajo hacia arriba con Dock.Top)
        pnlRight.Controls.Add(grpReset);
        pnlRight.Controls.Add(grpBoardConfig);
        pnlRight.Controls.Add(grpAnalog);
        pnlRight.Controls.Add(grpSemiAuto);

        // ═══════════════════════════════════════════════════════════════════
        // grpLog
        // ═══════════════════════════════════════════════════════════════════
        grpLog.Text  = "Log de comunicación";
        grpLog.Dock  = System.Windows.Forms.DockStyle.Fill;
        grpLog.Font  = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        txtLog.Multiline    = true;
        txtLog.ReadOnly     = true;
        txtLog.ScrollBars   = System.Windows.Forms.ScrollBars.Vertical;
        txtLog.Font         = new System.Drawing.Font("Consolas", 9f);
        txtLog.BackColor    = System.Drawing.Color.FromArgb(18, 18, 18);
        txtLog.ForeColor    = System.Drawing.Color.FromArgb(0, 230, 100);
        txtLog.Dock         = System.Windows.Forms.DockStyle.Fill;

        btnClearLog.Text      = "Limpiar";
        btnClearLog.Dock      = System.Windows.Forms.DockStyle.Bottom;
        btnClearLog.Height    = 26;
        btnClearLog.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnClearLog.Font      = new System.Drawing.Font("Segoe UI", 8.5f);

        grpLog.Controls.Add(txtLog);
        grpLog.Controls.Add(btnClearLog);

        // ═══════════════════════════════════════════════════════════════════
        // tableMain – ensamblar
        // ═══════════════════════════════════════════════════════════════════
        tableMain.Controls.Add(pnlLeft,  0, 0);
        tableMain.Controls.Add(pnlRight, 1, 0);
        tableMain.Controls.Add(grpLog,   0, 1);
        tableMain.SetColumnSpan(grpLog, 2);

        // ═══════════════════════════════════════════════════════════════════
        // UserControl
        // ═══════════════════════════════════════════════════════════════════
        AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
        AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Dpi;
        Dock                = System.Windows.Forms.DockStyle.Fill;
        Controls.Add(tableMain);
        Controls.Add(pnlTopBar);   // Top → se coloca encima del Fill

        pnlTopBar.ResumeLayout(false);
        pnlTopBar.PerformLayout();
        tableMain.ResumeLayout(false);
        grpDiagnosis.ResumeLayout(false);
        grpMcpMode.ResumeLayout(false);
        grpMcpMode.PerformLayout();
        grpTrack.ResumeLayout(false);
        grpTrack.PerformLayout();
        grpOutputs.ResumeLayout(false);
        grpAnalog.ResumeLayout(false);
        grpAnalog.PerformLayout();
        tableResult.ResumeLayout(false);
        grpBoardConfig.ResumeLayout(false);
        grpBoardConfig.PerformLayout();
        grpReset.ResumeLayout(false);
        grpLog.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)nudTrack).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudNumMcps).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMuestras).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudRetardo).EndInit();
        ResumeLayout(false);
    }
    #endregion

    // ── Connection bar ────────────────────────────────────────────────────────
    private System.Windows.Forms.Panel      pnlTopBar;
    private System.Windows.Forms.Label      lblPort;
    private System.Windows.Forms.ComboBox   cmbPort;
    private System.Windows.Forms.Button     btnRefreshPorts;
    private System.Windows.Forms.Label      lblBaudRate;
    private System.Windows.Forms.ComboBox   cmbBaudRate;
    private System.Windows.Forms.Button     btnConnect;
    private System.Windows.Forms.Button     btnDisconnect;
    private System.Windows.Forms.Label      lblConnStatus;

    // ── Layout ────────────────────────────────────────────────────────────────
    private System.Windows.Forms.TableLayoutPanel tableMain;

    // ── Diagnosis ─────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpDiagnosis;
    private System.Windows.Forms.Button     btnDiagTotal;
    private System.Windows.Forms.Button     btnDiagAds;
    private System.Windows.Forms.Button     btnDiagVersion;
    private System.Windows.Forms.Button     btnDiagReadConfig;
    private System.Windows.Forms.Button     btnDiagTemperature;
    private System.Windows.Forms.Button[]   _btnDiagMcp;

    // ── M – configuración de dirección ─────────────────────────────────────────
    private System.Windows.Forms.GroupBox      grpMcpMode;
    private System.Windows.Forms.Label         lblMcpModeAddr;
    private System.Windows.Forms.ComboBox      cmbMcpModeAddr;
    private System.Windows.Forms.RadioButton   rbModeOutput;
    private System.Windows.Forms.RadioButton   rbModeInput;
    private System.Windows.Forms.Label         lblMcpModeMask;
    private System.Windows.Forms.TextBox       txtMcpModeMask;
    private System.Windows.Forms.Button        btnSendMcpMode;

    // ── P – selección de pista ───────────────────────────────────────────────
    private System.Windows.Forms.GroupBox        grpTrack;
    private System.Windows.Forms.Label           lblTrack;
    private System.Windows.Forms.NumericUpDown   nudTrack;
    private System.Windows.Forms.Button          btnSelectTrack;
    private System.Windows.Forms.Label           lblTrackHint;

    // ── Outputs ───────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpOutputs;
    private System.Windows.Forms.Panel      pnlOutputMatrix;
    private System.Windows.Forms.Label      lblOutputMask;
    private System.Windows.Forms.Button     btnOutputsAllOn;
    private System.Windows.Forms.Button     btnOutputsAllOff;

    // ── Analog ────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox         grpAnalog;
    private System.Windows.Forms.Label            lblAnalogTitle;
    private System.Windows.Forms.Label            lblChannel;
    private System.Windows.Forms.ComboBox         cmbChannel;
    private System.Windows.Forms.Button           btnReadRaw;
    private System.Windows.Forms.Label            lblRawValue;
    private System.Windows.Forms.Button           btnReadFiltered;
    private System.Windows.Forms.Label            lblFilteredValue;
    private System.Windows.Forms.Button           btnReadAllFiltered;
    private System.Windows.Forms.TableLayoutPanel tableResult;
    private System.Windows.Forms.Label            lblVainLbl;
    private System.Windows.Forms.Label            lblVain;
    private System.Windows.Forms.Label            lblVeLbl;
    private System.Windows.Forms.Label            lblVe;
    private System.Windows.Forms.Label            lblDenomLbl;
    private System.Windows.Forms.Label            lblDenom;
    private System.Windows.Forms.Label            lblResistanceLbl;
    private System.Windows.Forms.Label            lblResistance;
    private System.Windows.Forms.Label            lblFormula;

    // ── I – configuración de placa ───────────────────────────────────────────
    private System.Windows.Forms.GroupBox        grpBoardConfig;
    private System.Windows.Forms.Label           lblNumMcps;
    private System.Windows.Forms.NumericUpDown   nudNumMcps;
    private System.Windows.Forms.Label           lblInh;
    private System.Windows.Forms.TextBox         txtInh1;
    private System.Windows.Forms.TextBox         txtInh2;
    private System.Windows.Forms.TextBox         txtInh3;
    private System.Windows.Forms.TextBox         txtInh4;
    private System.Windows.Forms.Label           lblBoardRef;
    private System.Windows.Forms.TextBox         txtBoardRef;
    private System.Windows.Forms.Label           lblMuestras;
    private System.Windows.Forms.NumericUpDown   nudMuestras;
    private System.Windows.Forms.Label           lblRetardo;
    private System.Windows.Forms.NumericUpDown   nudRetardo;
    private System.Windows.Forms.Button          btnSendBoardConfig;

    // ── Semiautomático ────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpSemiAuto;
    private System.Windows.Forms.Label      lblRefManual;
    private System.Windows.Forms.ComboBox   cmbReferenciaManual;
    private System.Windows.Forms.Button     btnRefreshRefsManual;
    private System.Windows.Forms.Label      lblContactoManual;
    private System.Windows.Forms.ComboBox   cmbContactoManual;
    private System.Windows.Forms.Button     btnProbarContacto;
    private System.Windows.Forms.Label      lblSemiAutoResult;

    // ── Reset ─────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpReset;
    private System.Windows.Forms.Button     btnReset;

    // ── Log ───────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpLog;
    private System.Windows.Forms.TextBox    txtLog;
    private System.Windows.Forms.Button     btnClearLog;
}
