using PC7866.Views;

namespace PC7866
{
    public partial class Form1 : Form
    {
        private ManualControlPanel? _manualPanel;
        private AutomaticTestPanel? _automaticPanel;
        private ParametersPanel?    _parametersPanel;
        private ReportsPanel?       _reportsPanel;

        public Form1()
        {
            InitializeComponent();
            WindowState = FormWindowState.Maximized;
            AttachMenuHandlers();
            // Arranca en modo automático por defecto
            ShowAutomaticPanel();
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
                _manualPanel = new ManualControlPanel { Dock = DockStyle.Fill };
                panelContent.Controls.Add(_manualPanel);
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
                _automaticPanel = new AutomaticTestPanel { Dock = DockStyle.Fill };
                panelContent.Controls.Add(_automaticPanel);
            }
            _automaticPanel.Show();
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
            }
            _reportsPanel.Show();
            _ = Task.Run(() => _reportsPanel.Invoke(_reportsPanel.RefreshData));
            SetTitle("Informes");
            informesToolStripMenuItem.Checked = true;
        }

        private void ShowConfiguration()
        {
            using var form = new ConfigurationForm();
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
            => Text = $"PC7866 – Test Resistivo Embega  |  {section}";
    }
}
