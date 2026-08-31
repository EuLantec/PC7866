using PC7866.Views;
using PC7866.Services.SerialCommunication;
using PC7866.Utils;

namespace PC7866
{
    public partial class Form1 : Form
    {
        private readonly ISerialPortService _serialPort;
        private ManualControlPanel? _manualPanel;
        private AutomaticTestPanel? _automaticPanel;
        private ParametersPanel?    _parametersPanel;
        private ReportsPanel?       _reportsPanel;

        private readonly ToolStripStatusLabel _statusSection = new("—") { ForeColor = UiTheme.TextPrimary };
        private readonly ToolStripStatusLabel _statusSpring  = new() { Spring = true };
        private readonly ToolStripStatusLabel _statusBrand   = new("PC7866 · Embega") { ForeColor = UiTheme.TextMuted };

        public Form1()
        {
            InitializeComponent();
            _serialPort = new SerialPortService();
            WindowState = FormWindowState.Maximized;
            ApplyTheme();
            AttachMenuHandlers();
            // Arranca en modo automático por defecto
            ShowAutomaticPanel();
        }

        private void ApplyTheme()
        {
            UiTheme.ApplyShell(this, menuStrip1, statusStrip1, panelContent);
            statusStrip1.Items.AddRange(new ToolStripItem[] { _statusSection, _statusSpring, _statusBrand });
        }

        private void AttachMenuHandlers()
        {
            mToolStripMenuItem.Click              += (_, _) => ShowManualPanel();
            automáticoToolStripMenuItem.Click     += (_, _) => ShowAutomaticPanel();
            parámetrosToolStripMenuItem.Click     += (_, _) => ShowParametersPanel();
            informesToolStripMenuItem.Click       += (_, _) => ShowReportsPanel();
            configuraciónToolStripMenuItem.Click  += (_, _) => ShowConfiguration();
            salirToolStripMenuItem.Click          += (_, _) => Application.Exit();
        }

        // ─────────────────────────────────────────────────────────────────────
        // Paneles
        // ─────────────────────────────────────────────────────────────────────

        private void ShowManualPanel()
        {
            HideAllPanels();
            if (_manualPanel is null)
            {
                _manualPanel = new ManualControlPanel(_serialPort) { Dock = DockStyle.Fill };
                panelContent.Controls.Add(_manualPanel);
                UiTheme.ApplyPanel(_manualPanel);
            }
            _manualPanel.Show();
            SetTitle("Modo Manual");
            mToolStripMenuItem.Checked = true;
        }

        private void ShowAutomaticPanel()
        {
            HideAllPanels();
            if (_automaticPanel is null)
            {
                _automaticPanel = new AutomaticTestPanel(_serialPort) { Dock = DockStyle.Fill };
                panelContent.Controls.Add(_automaticPanel);
                UiTheme.ApplyPanel(_automaticPanel);
            }
            _automaticPanel.Show();
            _ = _automaticPanel.RefreshAsync();
            SetTitle("Modo Automático");
            automáticoToolStripMenuItem.Checked = true;
        }

        private void ShowParametersPanel()
        {
            HideAllPanels();
            if (_parametersPanel is null)
            {
                _parametersPanel = new ParametersPanel { Dock = DockStyle.Fill };
                panelContent.Controls.Add(_parametersPanel);
                UiTheme.ApplyPanel(_parametersPanel);
            }
            _parametersPanel.Show();
            SetTitle("Parámetros");
            parámetrosToolStripMenuItem.Checked = true;
        }

        private void ShowReportsPanel()
        {
            HideAllPanels();
            if (_reportsPanel is null)
            {
                _reportsPanel = new ReportsPanel { Dock = DockStyle.Fill };
                panelContent.Controls.Add(_reportsPanel);
                UiTheme.ApplyPanel(_reportsPanel);
            }
            _reportsPanel.Show();
            _ = Task.Run(() => _reportsPanel.Invoke(_reportsPanel.RefreshData));
            SetTitle("Informes");
            informesToolStripMenuItem.Checked = true;
        }

        private void ShowConfiguration()
        {
            using var form = new ConfigurationForm();
            UiTheme.ApplyPanel(form);
            form.ShowDialog(this);
        }

        private void HideAllPanels()
        {
            _manualPanel?.Hide();
            _automaticPanel?.Hide();
            _parametersPanel?.Hide();
            _reportsPanel?.Hide();

            mToolStripMenuItem.Checked          = false;
            automáticoToolStripMenuItem.Checked = false;
            parámetrosToolStripMenuItem.Checked = false;
            informesToolStripMenuItem.Checked   = false;
        }

        private void SetTitle(string section)
        {
            Text = $"PC7866 – Test Resistivo Embega  |  {section}";
            _statusSection.Text = section;
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            _serialPort.Dispose();
            base.OnFormClosed(e);
        }
    }
}
