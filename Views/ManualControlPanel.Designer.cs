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

        // ── Col izquierda: Diagnosis + Salidas ─────────────────────────────
        grpDiagnosis    = new System.Windows.Forms.GroupBox();
        btnDiagTotal    = new System.Windows.Forms.Button();
        btnDiag1        = new System.Windows.Forms.Button();
        btnDiag2        = new System.Windows.Forms.Button();
        btnDiag3        = new System.Windows.Forms.Button();
        btnDiag4        = new System.Windows.Forms.Button();

        grpOutputs      = new System.Windows.Forms.GroupBox();
        pnlOutputMatrix = new System.Windows.Forms.Panel();
        lblOutputMask   = new System.Windows.Forms.Label();
        btnOutputsAllOn = new System.Windows.Forms.Button();
        btnOutputsAllOff= new System.Windows.Forms.Button();
        btnFullTest     = new System.Windows.Forms.Button();

        // ── Col derecha: Analógica + Filtros + Guardar + Reset ─────────────
        grpAnalog       = new System.Windows.Forms.GroupBox();
        btnReadRaw      = new System.Windows.Forms.Button();
        btnReadFiltered = new System.Windows.Forms.Button();
        lblAnalogTitle  = new System.Windows.Forms.Label();
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

        grpFilter       = new System.Windows.Forms.GroupBox();
        lblCoefHint     = new System.Windows.Forms.Label();
        lblFilterFlags  = new System.Windows.Forms.Label();
        txtFilterFlags  = new System.Windows.Forms.TextBox();
        btnFilterFlags  = new System.Windows.Forms.Button();
        // Arrays de 10 coeficientes
        _lblCoef    = new System.Windows.Forms.Label  [10];
        _txtCoef    = new System.Windows.Forms.TextBox[10];
        _btnCoef    = new System.Windows.Forms.Button [10];
        for (int _i = 0; _i < 10; _i++)
        {
            _lblCoef[_i] = new System.Windows.Forms.Label();
            _txtCoef[_i] = new System.Windows.Forms.TextBox();
            _btnCoef[_i] = new System.Windows.Forms.Button();
        }

        grpSave         = new System.Windows.Forms.GroupBox();
        btnSaveWrite    = new System.Windows.Forms.Button();
        btnSaveRead     = new System.Windows.Forms.Button();
        btnSaveView     = new System.Windows.Forms.Button();


        grpReset        = new System.Windows.Forms.GroupBox();
        btnReset        = new System.Windows.Forms.Button();

        // ── Log ────────────────────────────────────────────────────────────
        grpLog          = new System.Windows.Forms.GroupBox();
        txtLog          = new System.Windows.Forms.TextBox();
        btnClearLog     = new System.Windows.Forms.Button();

        pnlTopBar.SuspendLayout();
        tableMain.SuspendLayout();
        grpDiagnosis.SuspendLayout();
        grpOutputs.SuspendLayout();
        grpAnalog.SuspendLayout();
        tableResult.SuspendLayout();
        grpFilter.SuspendLayout();
        grpSave.SuspendLayout();
        grpReset.SuspendLayout();
        grpLog.SuspendLayout();
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
        btnRefreshPorts.FlatStyle= System.Windows.Forms.FlatStyle.Flat;

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
        tableMain.SetColumnSpan(grpLog, 2);   // el log ocupa las 2 columnas

        // ── Panel izquierdo: contenedor vertical de Diagnosis + Salidas ────
        var pnlLeft = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Padding = new System.Windows.Forms.Padding(0, 0, 4, 0)
        };

        // ── Panel derecho: contenedor vertical de Analógica + Filtros + Guardar/Reset
        var pnlRight = new System.Windows.Forms.Panel
        {
            Dock = System.Windows.Forms.DockStyle.Fill,
            Padding = new System.Windows.Forms.Padding(4, 0, 0, 0)
        };

        // ═══════════════════════════════════════════════════════════════════
        // grpDiagnosis
        // ═══════════════════════════════════════════════════════════════════
        grpDiagnosis.Text    = "D – Diagnosis";
        grpDiagnosis.Dock    = System.Windows.Forms.DockStyle.Top;
        grpDiagnosis.Height  = 82;
        grpDiagnosis.Enabled = false;
        grpDiagnosis.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        void StyleBtn(System.Windows.Forms.Button b, string text, int x, int y, int w = 130)
        {
            b.Text      = text;
            b.Location  = new System.Drawing.Point(x, y);
            b.Size      = new System.Drawing.Size(w, 28);
            b.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            b.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
        }

        StyleBtn(btnDiagTotal, "DT – Total",    6,  22, 148);
        btnDiagTotal.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnDiagTotal.ForeColor = System.Drawing.Color.White;
        btnDiagTotal.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        StyleBtn(btnDiag1, "D1 – MCP 0x20", 162, 22);
        StyleBtn(btnDiag2, "D2 – MCP 0x21", 298, 22);
        StyleBtn(btnDiag3, "D3 – MCP 0x22", 434, 22);
        StyleBtn(btnDiag4, "D4 – 0x48",     570, 22);

        grpDiagnosis.Controls.AddRange(new System.Windows.Forms.Control[]
            { btnDiagTotal, btnDiag1, btnDiag2, btnDiag3, btnDiag4 });

        // ═══════════════════════════════════════════════════════════════════
        // grpOutputs  (Salidas S)
        // ═══════════════════════════════════════════════════════════════════
        grpOutputs.Text    = "S – Activación de salidas  (48 canales)";
        grpOutputs.Dock    = System.Windows.Forms.DockStyle.Fill;
        grpOutputs.Enabled = false;
        grpOutputs.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblOutputMask.Text      = "Trama:  S000000000000";
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

        btnFullTest.Text      = "▶  Test completo (48 salidas)";
        btnFullTest.Location  = new System.Drawing.Point(438, 18);
        btnFullTest.Size      = new System.Drawing.Size(220, 28);
        btnFullTest.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        btnFullTest.BackColor = System.Drawing.Color.FromArgb(180, 100, 0);
        btnFullTest.ForeColor = System.Drawing.Color.White;
        btnFullTest.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
        btnFullTest.Enabled   = false;

        pnlOutputMatrix.Location  = new System.Drawing.Point(6, 48);
        pnlOutputMatrix.AutoSize  = true;

        grpOutputs.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblOutputMask, btnOutputsAllOn, btnOutputsAllOff, btnFullTest, pnlOutputMatrix });

        pnlLeft.Controls.Add(grpOutputs);
        pnlLeft.Controls.Add(grpDiagnosis);   // Top se añade después → queda encima

        // ═══════════════════════════════════════════════════════════════════
        // grpAnalog  (R / F + resultado)
        // ═══════════════════════════════════════════════════════════════════
        grpAnalog.Text    = "R / F – Lecturas analógicas";
        grpAnalog.Dock    = System.Windows.Forms.DockStyle.Top;
        grpAnalog.Height  = 200;
        grpAnalog.Enabled = false;
        grpAnalog.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblAnalogTitle.Text      = "Canales: Ch1 – Ch2 = Vain  |  Ch3 – Ch4 = Ve";
        lblAnalogTitle.AutoSize  = true;
        lblAnalogTitle.Location  = new System.Drawing.Point(6, 22);
        lblAnalogTitle.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
        lblAnalogTitle.ForeColor = System.Drawing.Color.Gray;

        StyleBtn(btnReadRaw,      "R – Leer RAW",      6, 44, 140);
        StyleBtn(btnReadFiltered, "F – Leer Filtrado", 154, 44, 155);
        btnReadFiltered.BackColor = System.Drawing.Color.FromArgb(0, 120, 215);
        btnReadFiltered.ForeColor = System.Drawing.Color.White;
        btnReadFiltered.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        // Tabla de resultado de R
        tableResult.Location     = new System.Drawing.Point(6, 82);
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

        LblCaption(lblVainLbl,       "Vain (Ch1–Ch2):");
        LblValue  (lblVain,          "—");
        LblCaption(lblVeLbl,         "Ve (Ch3–Ch4):");
        LblValue  (lblVe,            "—");
        LblCaption(lblDenomLbl,      "Ve – Vain:");
        LblValue  (lblDenom,         "—");
        LblCaption(lblResistanceLbl, "R  =  Vain/(Ve–Vain)×390 Ω:");
        LblValue  (lblResistance,    "—");
        lblResistance.Font      = new System.Drawing.Font("Consolas", 12f, System.Drawing.FontStyle.Bold);
        lblResistance.ForeColor = System.Drawing.Color.FromArgb(180, 50, 0);

        lblFormula.Text      = "R = Vain / (Ve – Vain) × 390 Ω";
        lblFormula.AutoSize  = true;
        lblFormula.Location  = new System.Drawing.Point(300, 48);
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
            { lblAnalogTitle, btnReadRaw, btnReadFiltered, tableResult, lblFormula });

        // ═══════════════════════════════════════════════════════════════════
        // grpFilter
        // ═══════════════════════════════════════════════════════════════════
        grpFilter.Text    = "I – Filtros";
        grpFilter.Dock    = System.Windows.Forms.DockStyle.Top;
        grpFilter.Height  = 290;
        grpFilter.Enabled = false;
        grpFilter.Visible = false;
        grpFilter.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        lblCoefHint.Text      = "Decimal (0.95553) o hex de 4 dígitos (7373).  Fórmula: (coef + 2.0) × 10000";
        lblCoefHint.AutoSize  = true;
        lblCoefHint.Location  = new System.Drawing.Point(6, 20);
        lblCoefHint.Font      = new System.Drawing.Font("Segoe UI", 8f);
        lblCoefHint.ForeColor = System.Drawing.Color.Gray;

        var pnlFilterScroll = new System.Windows.Forms.Panel
        {
            Location    = new System.Drawing.Point(6, 40),
            Size        = new System.Drawing.Size(490, 264),
            AutoScroll  = true,
            BorderStyle = System.Windows.Forms.BorderStyle.None
        };

        void FilterRowInPanel(System.Windows.Forms.Label lbl, string caption,
                              System.Windows.Forms.TextBox txt, string defVal,
                              System.Windows.Forms.Button btn, string btnText, int y)
        {
            lbl.Text     = caption;
            lbl.AutoSize = true;
            lbl.Location = new System.Drawing.Point(0, y + 3);
            lbl.Font     = new System.Drawing.Font("Segoe UI", 8.5f);

            txt.Text     = defVal;
            txt.Location = new System.Drawing.Point(180, y);
            txt.Size     = new System.Drawing.Size(100, 24);
            txt.Font     = new System.Drawing.Font("Consolas", 9f);

            btn.Text      = btnText;
            btn.Location  = new System.Drawing.Point(286, y);
            btn.Size      = new System.Drawing.Size(88, 24);
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.Font      = new System.Drawing.Font("Segoe UI", 8.5f);
        }

        // FLAGS
        FilterRowInPanel(lblFilterFlags, "I0 – FLAGS (hex):", txtFilterFlags, "0000", btnFilterFlags, "Enviar I0", 2);
        txtFilterFlags.MaxLength       = 4;
        txtFilterFlags.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
        pnlFilterScroll.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblFilterFlags, txtFilterFlags, btnFilterFlags });

        // Coeficientes I1 – I10
        for (int _ci = 0; _ci < 10; _ci++)
        {
            int _y = 28 + _ci * 26;
            FilterRowInPanel(
                _lblCoef[_ci], $"I{_ci + 1} – Coef. {_ci + 1}:",
                _txtCoef[_ci], "0.0",
                _btnCoef[_ci], $"Enviar I{_ci + 1}", _y);
            pnlFilterScroll.Controls.AddRange(new System.Windows.Forms.Control[]
                { _lblCoef[_ci], _txtCoef[_ci], _btnCoef[_ci] });
        }

        grpFilter.Controls.AddRange(new System.Windows.Forms.Control[]
            { lblCoefHint, pnlFilterScroll });

        // ═══════════════════════════════════════════════════════════════════
        // grpSave
        // ═══════════════════════════════════════════════════════════════════
        grpSave.Text    = "G – Parámetros EEPROM";
        grpSave.Dock    = System.Windows.Forms.DockStyle.Top;
        grpSave.Height  = 62;
        grpSave.Enabled = false;
        grpSave.Visible = false;
        grpSave.Font    = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);

        StyleBtn(btnSaveWrite, "GG – Guardar EEPROM",  6,  22, 160);
        StyleBtn(btnSaveRead,  "GL – Leer EEPROM→RAM", 174, 22, 160);
        StyleBtn(btnSaveView,  "GV – Ver RAM",         342, 22, 120);

        grpSave.Controls.AddRange(new System.Windows.Forms.Control[]
            { btnSaveWrite, btnSaveRead, btnSaveView });

        // ═══════════════════════════════════════════════════════════════════
        // grpReset
        // ═══════════════════════════════════════════════════════════════════
        grpReset.Text    = "Q – Reset";
        grpReset.Dock    = System.Windows.Forms.DockStyle.Top;
        grpReset.Height  = 62;
        grpReset.Enabled = false;
        grpReset.Visible = false;
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
        pnlRight.Controls.Add(grpSave);
        pnlRight.Controls.Add(grpFilter);
        pnlRight.Controls.Add(grpAnalog);

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
        AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
        AutoScaleMode       = System.Windows.Forms.AutoScaleMode.Font;
        Dock                = System.Windows.Forms.DockStyle.Fill;
        Controls.Add(tableMain);
        Controls.Add(pnlTopBar);   // Top → se coloca encima del Fill

        pnlTopBar.ResumeLayout(false);
        pnlTopBar.PerformLayout();
        tableMain.ResumeLayout(false);
        grpDiagnosis.ResumeLayout(false);
        grpOutputs.ResumeLayout(false);
        grpAnalog.ResumeLayout(false);
        tableResult.ResumeLayout(false);
        grpFilter.ResumeLayout(false);
        grpFilter.PerformLayout();
        grpSave.ResumeLayout(false);
        grpReset.ResumeLayout(false);
        grpLog.ResumeLayout(false);
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
    private System.Windows.Forms.Button     btnDiag1;
    private System.Windows.Forms.Button     btnDiag2;
    private System.Windows.Forms.Button     btnDiag3;
    private System.Windows.Forms.Button     btnDiag4;

    // ── Outputs ───────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpOutputs;
    private System.Windows.Forms.Panel      pnlOutputMatrix;
    private System.Windows.Forms.Label      lblOutputMask;
    private System.Windows.Forms.Button     btnOutputsAllOn;
    private System.Windows.Forms.Button     btnOutputsAllOff;
    private System.Windows.Forms.Button     btnFullTest;

    // ── Analog ────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox         grpAnalog;
    private System.Windows.Forms.Label            lblAnalogTitle;
    private System.Windows.Forms.Button           btnReadRaw;
    private System.Windows.Forms.Button           btnReadFiltered;
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

    // ── Filter ────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpFilter;
    private System.Windows.Forms.Label      lblCoefHint;
    private System.Windows.Forms.Label      lblFilterFlags;
    private System.Windows.Forms.TextBox    txtFilterFlags;
    private System.Windows.Forms.Button     btnFilterFlags;
    // Coeficientes I1..I10 (arrays, índice 0-based = coef 1-based)
    private System.Windows.Forms.Label  []  _lblCoef;
    private System.Windows.Forms.TextBox[]  _txtCoef;
    private System.Windows.Forms.Button []  _btnCoef;

    // ── Save
    private System.Windows.Forms.GroupBox   grpSave;
    private System.Windows.Forms.Button     btnSaveWrite;
    private System.Windows.Forms.Button     btnSaveRead;
    private System.Windows.Forms.Button     btnSaveView;

    // ── Reset ─────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpReset;
    private System.Windows.Forms.Button     btnReset;

    // ── Log ───────────────────────────────────────────────────────────────────
    private System.Windows.Forms.GroupBox   grpLog;
    private System.Windows.Forms.TextBox    txtLog;
    private System.Windows.Forms.Button     btnClearLog;
}
