using PC7866.Configuration;

namespace PC7866.Views;

/// <summary>
/// Diálogo de configuración de parámetros de comunicación y base de datos.
/// Accesible desde Archivo → Configuración.
/// </summary>
public sealed class ConfigurationForm : Form
{
    // ── Comunicación serie ───────────────────────────────────────────────────
    private ComboBox   cmbPuerto      = null!;
    private ComboBox   cmbBaudRate    = null!;
    private NumericUpDown nudTimeout  = null!;

    // ── Base de datos ────────────────────────────────────────────────────────
    private TextBox    txtDbServer    = null!;
    private NumericUpDown nudDbPort   = null!;
    private TextBox    txtDbName      = null!;
    private TextBox    txtDbUser      = null!;
    private TextBox    txtDbPassword  = null!;

    // ── Botones ──────────────────────────────────────────────────────────────
    private Button     btnGuardar     = null!;
    private Button     btnCancelar    = null!;
    private Button     btnProbarBd    = null!;

    public ConfigurationForm()
    {
        BuildUi();
        LoadSettings();
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Construcción de UI
    // ─────────────────────────────────────────────────────────────────────────

    private void BuildUi()
    {
        AutoScaleMode   = AutoScaleMode.Dpi;
        AutoScaleDimensions = new SizeF(96F, 96F);
        Text            = "Configuración";
        Size            = new Size(520, 470);
        StartPosition   = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox     = false;
        MinimizeBox     = false;

        var tab = new TabControl { Dock = DockStyle.Fill };

        // ── Pestaña Comunicación ─────────────────────────────────────────────
        var pgCom = new TabPage("Comunicación serie");
        var tblCom = MakeTable(3);
        pgCom.Controls.Add(tblCom);

        cmbPuerto   = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        cmbPuerto.Items.AddRange(System.IO.Ports.SerialPort.GetPortNames());
        cmbBaudRate = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Width = 200 };
        cmbBaudRate.Items.AddRange(new object[] { 9600, 19200, 38400, 57600, 115200 });
        nudTimeout  = new NumericUpDown { Width = 200, Minimum = 500, Maximum = 60000, Increment = 500 };

        AddRow(tblCom, "Puerto COM:",             cmbPuerto);
        AddRow(tblCom, "Velocidad (baudios):",    cmbBaudRate);
        AddRow(tblCom, "Timeout lectura (ms):",   nudTimeout);

        tab.TabPages.Add(pgCom);

        // ── Pestaña Base de datos ────────────────────────────────────────────
        var pgBd = new TabPage("Base de datos");
        var tblBd = MakeTable(5);
        pgBd.Controls.Add(tblBd);

        txtDbServer   = new TextBox { Width = 200 };
        nudDbPort     = new NumericUpDown { Width = 200, Minimum = 1, Maximum = 65535 };
        txtDbName     = new TextBox { Width = 200 };
        txtDbUser     = new TextBox { Width = 200 };
        txtDbPassword = new TextBox { Width = 200, UseSystemPasswordChar = true };

        AddRow(tblBd, "Servidor:",      txtDbServer);
        AddRow(tblBd, "Puerto:",        nudDbPort);
        AddRow(tblBd, "Base de datos:", txtDbName);
        AddRow(tblBd, "Usuario:",       txtDbUser);
        AddRow(tblBd, "Contraseña:",    txtDbPassword);

        btnProbarBd = new Button
        {
            Text      = "🔌 Probar conexión",
            Size      = new Size(160, 28),
            Margin    = new Padding(4)
        };
        btnProbarBd.Click += BtnProbarBd_Click;

        var pnlProbar = new FlowLayoutPanel
        {
            Dock     = DockStyle.Bottom,
            Height   = 40,
            Padding  = new Padding(4)
        };
        pnlProbar.Controls.Add(btnProbarBd);
        pgBd.Controls.Add(pnlProbar);

        tab.TabPages.Add(pgBd);
        Controls.Add(tab);

        // ── Barra inferior de botones ────────────────────────────────────────
        var pnlBtns = new FlowLayoutPanel
        {
            Dock          = DockStyle.Bottom,
            AutoSize      = true,
            FlowDirection = FlowDirection.RightToLeft,
            BackColor     = Color.FromArgb(235, 238, 245),
            Padding       = new Padding(8)
        };

        btnCancelar = new Button
        {
            Text      = "Cancelar",
            AutoSize  = true,
            MinimumSize = new Size(90, 30),
            Margin    = new Padding(4),
            Font      = new Font("Segoe UI", 9f)
        };
        btnCancelar.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };

        btnGuardar = new Button
        {
            Text      = "💾 Guardar",
            AutoSize  = true,
            MinimumSize = new Size(110, 30),
            Margin    = new Padding(4),
            Font      = new Font("Segoe UI", 9f, FontStyle.Bold),
            BackColor = Color.FromArgb(0, 120, 212),
            ForeColor = Color.White,
            FlatStyle = FlatStyle.Flat
        };
        btnGuardar.Click += BtnGuardar_Click;

        pnlBtns.Controls.AddRange(new Control[] { btnCancelar, btnGuardar });
        Controls.Add(pnlBtns);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers UI
    // ─────────────────────────────────────────────────────────────────────────

    private static TableLayoutPanel MakeTable(int rows)
    {
        var t = new TableLayoutPanel
        {
            Dock        = DockStyle.Fill,
            ColumnCount = 2,
            RowCount    = rows,
            Padding     = new Padding(10)
        };
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 210));
        t.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < rows; i++)
            t.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        return t;
    }

    private static void AddRow(TableLayoutPanel t, string label, Control control)
    {
        int row = t.RowCount - (t.RowStyles.Count - t.Controls.Count / 2);
        var lbl = new Label
        {
            Text      = label,
            AutoSize  = false,
            Dock      = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin    = new Padding(3, 6, 6, 6)
        };
        control.Margin = new Padding(3, 6, 3, 6);
        control.Anchor = AnchorStyles.Left;
        t.Controls.Add(lbl);
        t.Controls.Add(control);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Lógica
    // ─────────────────────────────────────────────────────────────────────────

    private void LoadSettings()
    {
        var s = AppSettings.Instance;

        // Comunicación
        int portIdx = cmbPuerto.FindStringExact(s.DefaultPortName);
        cmbPuerto.SelectedIndex = portIdx >= 0 ? portIdx : (cmbPuerto.Items.Count > 0 ? 0 : -1);
        cmbBaudRate.SelectedItem = s.DefaultBaudRate;
        nudTimeout.Value = Math.Max(nudTimeout.Minimum, Math.Min(nudTimeout.Maximum, s.DefaultTimeout));

        // BD
        txtDbServer.Text   = s.DatabaseServer;
        nudDbPort.Value    = s.DatabasePort;
        txtDbName.Text     = s.DatabaseName;
        txtDbUser.Text     = s.DatabaseUser;
        txtDbPassword.Text = s.DatabasePassword;
    }

    private void BtnGuardar_Click(object? sender, EventArgs e)
    {
        var s = AppSettings.Instance;

        if (cmbPuerto.SelectedItem is not null)
            s.DefaultPortName = cmbPuerto.SelectedItem.ToString()!;
        if (cmbBaudRate.SelectedItem is int baud)
            s.DefaultBaudRate = baud;
        s.DefaultTimeout = (int)nudTimeout.Value;

        s.DatabaseServer   = txtDbServer.Text.Trim();
        s.DatabasePort     = (int)nudDbPort.Value;
        s.DatabaseName     = txtDbName.Text.Trim();
        s.DatabaseUser     = txtDbUser.Text.Trim();
        s.DatabasePassword = txtDbPassword.Text;

        s.Save();

        DialogResult = DialogResult.OK;
        Close();
    }

    private async void BtnProbarBd_Click(object? sender, EventArgs e)
    {
        btnProbarBd.Enabled = false;
        btnProbarBd.Text    = "Probando…";
        try
        {
            string cs = $"Server={txtDbServer.Text.Trim()};" +
                        $"Port={nudDbPort.Value};" +
                        $"Database={txtDbName.Text.Trim()};" +
                        $"User={txtDbUser.Text.Trim()};" +
                        $"Password={txtDbPassword.Text};";

            using var repo = new Services.Database.TestRepository(cs);
            bool ok = await repo.TestConnectionAsync();
            MessageBox.Show(ok ? "✅ Conexión correcta." : "❌ No se pudo conectar.",
                "Prueba de conexión", MessageBoxButtons.OK,
                ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"❌ Error: {ex.Message}", "Prueba de conexión",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnProbarBd.Enabled = true;
            btnProbarBd.Text    = "🔌 Probar conexión";
        }
    }
}
