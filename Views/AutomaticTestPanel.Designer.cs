namespace PC7866.Views;

partial class AutomaticTestPanel
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null)) components.Dispose();
        base.Dispose(disposing);
    }

    #region Component Designer generated code

    private void InitializeComponent()
    {
        grpConnection = new GroupBox();
        lblPort = new Label();
        cmbPort = new ComboBox();
        lblBaudRate = new Label();
        cmbBaudRate = new ComboBox();
        btnRefreshPorts = new Button();
        btnConnect = new Button();
        btnDisconnect = new Button();
        lblConnectionStatus = new Label();
        grpEnsayo = new GroupBox();
        lblReferencia = new Label();
        cmbReferencia = new ComboBox();
        btnRefreshRefs = new Button();
        lblOperario = new Label();
        txtOperario = new TextBox();
        lblLote = new Label();
        txtLote = new TextBox();
        btnStartTest = new Button();
        btnAbortTest = new Button();
        grpImagen = new GroupBox();
        picReferencia = new PictureBox();
        grpResultados = new GroupBox();
        gridResultados = new DataGridView();
        colPaso = new DataGridViewTextBoxColumn();
        colContacto = new DataGridViewTextBoxColumn();
        colMedido = new DataGridViewTextBoxColumn();
        colNominal = new DataGridViewTextBoxColumn();
        colResultado = new DataGridViewTextBoxColumn();
        grpProgress = new GroupBox();
        progressBar = new ProgressBar();
        lblCurrentStep = new Label();
        lblMachineState = new Label();
        grpLog = new GroupBox();
        txtLog = new TextBox();
        grpConnection.SuspendLayout();
        grpEnsayo.SuspendLayout();
        grpImagen.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picReferencia).BeginInit();
        grpResultados.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridResultados).BeginInit();
        grpProgress.SuspendLayout();
        grpLog.SuspendLayout();
        SuspendLayout();
        // 
        // grpConnection
        // 
        grpConnection.Controls.Add(lblPort);
        grpConnection.Controls.Add(cmbPort);
        grpConnection.Controls.Add(lblBaudRate);
        grpConnection.Controls.Add(cmbBaudRate);
        grpConnection.Controls.Add(btnRefreshPorts);
        grpConnection.Controls.Add(btnConnect);
        grpConnection.Controls.Add(btnDisconnect);
        grpConnection.Controls.Add(lblConnectionStatus);
        grpConnection.Location = new Point(3, 3);
        grpConnection.Name = "grpConnection";
        grpConnection.Size = new Size(1250, 60);
        grpConnection.TabIndex = 0;
        grpConnection.TabStop = false;
        grpConnection.Text = "Conexión serie";
        // 
        // lblPort
        // 
        lblPort.AutoSize = true;
        lblPort.Location = new Point(8, 26);
        lblPort.Name = "lblPort";
        lblPort.Size = new Size(45, 15);
        lblPort.TabIndex = 0;
        lblPort.Text = "Puerto:";
        // 
        // cmbPort
        // 
        cmbPort.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbPort.Location = new Point(62, 22);
        cmbPort.Name = "cmbPort";
        cmbPort.Size = new Size(130, 23);
        cmbPort.TabIndex = 1;
        // 
        // lblBaudRate
        // 
        lblBaudRate.AutoSize = true;
        lblBaudRate.Location = new Point(238, 26);
        lblBaudRate.Name = "lblBaudRate";
        lblBaudRate.Size = new Size(52, 15);
        lblBaudRate.TabIndex = 2;
        lblBaudRate.Text = "Baudios:";
        // 
        // cmbBaudRate
        // 
        cmbBaudRate.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbBaudRate.Location = new Point(298, 22);
        cmbBaudRate.Name = "cmbBaudRate";
        cmbBaudRate.Size = new Size(110, 23);
        cmbBaudRate.TabIndex = 3;
        // 
        // btnRefreshPorts
        // 
        btnRefreshPorts.Location = new Point(198, 21);
        btnRefreshPorts.Name = "btnRefreshPorts";
        btnRefreshPorts.Size = new Size(30, 25);
        btnRefreshPorts.TabIndex = 4;
        btnRefreshPorts.Text = "🔄";
        // 
        // btnConnect
        // 
        btnConnect.Location = new Point(420, 21);
        btnConnect.Name = "btnConnect";
        btnConnect.Size = new Size(85, 25);
        btnConnect.TabIndex = 5;
        btnConnect.Text = "Conectar";
        // 
        // btnDisconnect
        // 
        btnDisconnect.Enabled = false;
        btnDisconnect.Location = new Point(511, 21);
        btnDisconnect.Name = "btnDisconnect";
        btnDisconnect.Size = new Size(95, 25);
        btnDisconnect.TabIndex = 6;
        btnDisconnect.Text = "Desconectar";
        // 
        // lblConnectionStatus
        // 
        lblConnectionStatus.AutoSize = true;
        lblConnectionStatus.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        lblConnectionStatus.ForeColor = Color.Red;
        lblConnectionStatus.Location = new Point(620, 25);
        lblConnectionStatus.Name = "lblConnectionStatus";
        lblConnectionStatus.Size = new Size(78, 15);
        lblConnectionStatus.TabIndex = 7;
        lblConnectionStatus.Text = "Sin conexión";
        lblConnectionStatus.Click += lblConnectionStatus_Click;
        // 
        // grpEnsayo
        // 
        grpEnsayo.Controls.Add(lblReferencia);
        grpEnsayo.Controls.Add(cmbReferencia);
        grpEnsayo.Controls.Add(btnRefreshRefs);
        grpEnsayo.Controls.Add(lblOperario);
        grpEnsayo.Controls.Add(txtOperario);
        grpEnsayo.Controls.Add(lblLote);
        grpEnsayo.Controls.Add(txtLote);
        grpEnsayo.Controls.Add(btnStartTest);
        grpEnsayo.Controls.Add(btnAbortTest);
        grpEnsayo.Location = new Point(3, 69);
        grpEnsayo.Name = "grpEnsayo";
        grpEnsayo.Size = new Size(1250, 62);
        grpEnsayo.TabIndex = 1;
        grpEnsayo.TabStop = false;
        grpEnsayo.Text = "Ensayo";
        // 
        // lblReferencia
        // 
        lblReferencia.AutoSize = true;
        lblReferencia.Location = new Point(8, 24);
        lblReferencia.Name = "lblReferencia";
        lblReferencia.Size = new Size(65, 15);
        lblReferencia.TabIndex = 0;
        lblReferencia.Text = "Referencia:";
        // 
        // cmbReferencia
        // 
        cmbReferencia.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbReferencia.Location = new Point(82, 20);
        cmbReferencia.Name = "cmbReferencia";
        cmbReferencia.Size = new Size(260, 23);
        cmbReferencia.TabIndex = 1;
        // 
        // btnRefreshRefs
        // 
        btnRefreshRefs.Location = new Point(348, 19);
        btnRefreshRefs.Name = "btnRefreshRefs";
        btnRefreshRefs.Size = new Size(30, 25);
        btnRefreshRefs.TabIndex = 2;
        btnRefreshRefs.Text = "🔄";
        // 
        // lblOperario
        // 
        lblOperario.AutoSize = true;
        lblOperario.Location = new Point(392, 24);
        lblOperario.Name = "lblOperario";
        lblOperario.Size = new Size(56, 15);
        lblOperario.TabIndex = 3;
        lblOperario.Text = "Operario:";
        // 
        // txtOperario
        // 
        txtOperario.Location = new Point(455, 20);
        txtOperario.Name = "txtOperario";
        txtOperario.Size = new Size(150, 23);
        txtOperario.TabIndex = 4;
        // 
        // lblLote
        // 
        lblLote.AutoSize = true;
        lblLote.Location = new Point(620, 24);
        lblLote.Name = "lblLote";
        lblLote.Size = new Size(33, 15);
        lblLote.TabIndex = 5;
        lblLote.Text = "Lote:";
        // 
        // txtLote
        // 
        txtLote.Location = new Point(655, 20);
        txtLote.Name = "txtLote";
        txtLote.Size = new Size(130, 23);
        txtLote.TabIndex = 6;
        // 
        // btnStartTest
        // 
        btnStartTest.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnStartTest.Location = new Point(1000, 18);
        btnStartTest.Name = "btnStartTest";
        btnStartTest.Size = new Size(115, 28);
        btnStartTest.TabIndex = 7;
        btnStartTest.Text = "▶ Iniciar";
        // 
        // btnAbortTest
        // 
        btnAbortTest.Enabled = false;
        btnAbortTest.Location = new Point(1121, 18);
        btnAbortTest.Name = "btnAbortTest";
        btnAbortTest.Size = new Size(115, 28);
        btnAbortTest.TabIndex = 8;
        btnAbortTest.Text = "⛔ Abortar";
        // 
        // grpImagen
        // 
        grpImagen.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left;
        grpImagen.Controls.Add(picReferencia);
        grpImagen.Location = new Point(3, 195);
        grpImagen.Name = "grpImagen";
        grpImagen.Size = new Size(520, 450);
        grpImagen.TabIndex = 3;
        grpImagen.TabStop = false;
        grpImagen.Text = "Imagen de referencia";
        // 
        // picReferencia
        // 
        picReferencia.BackColor = Color.FromArgb(245, 245, 245);
        picReferencia.Dock = DockStyle.Fill;
        picReferencia.Location = new Point(3, 19);
        picReferencia.Name = "picReferencia";
        picReferencia.Size = new Size(514, 428);
        picReferencia.SizeMode = PictureBoxSizeMode.Zoom;
        picReferencia.TabIndex = 0;
        picReferencia.TabStop = false;
        // 
        // grpResultados
        // 
        grpResultados.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpResultados.Controls.Add(gridResultados);
        grpResultados.Location = new Point(529, 195);
        grpResultados.Name = "grpResultados";
        grpResultados.Size = new Size(724, 450);
        grpResultados.TabIndex = 4;
        grpResultados.TabStop = false;
        grpResultados.Text = "Resultados del ensayo";
        // 
        // gridResultados
        // 
        gridResultados.AllowUserToAddRows = false;
        gridResultados.AllowUserToDeleteRows = false;
        gridResultados.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        gridResultados.Columns.AddRange(new DataGridViewColumn[] { colPaso, colContacto, colMedido, colNominal, colResultado });
        gridResultados.Dock = DockStyle.Fill;
        gridResultados.Font = new Font("Consolas", 9F);
        gridResultados.Location = new Point(3, 19);
        gridResultados.Name = "gridResultados";
        gridResultados.ReadOnly = true;
        gridResultados.RowHeadersVisible = false;
        gridResultados.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridResultados.Size = new Size(718, 428);
        gridResultados.TabIndex = 0;
        // 
        // colPaso
        // 
        colPaso.FillWeight = 8F;
        colPaso.HeaderText = "Paso";
        colPaso.Name = "colPaso";
        colPaso.ReadOnly = true;
        // 
        // colContacto
        // 
        colContacto.FillWeight = 20F;
        colContacto.HeaderText = "Contacto";
        colContacto.Name = "colContacto";
        colContacto.ReadOnly = true;
        // 
        // colMedido
        // 
        colMedido.FillWeight = 20F;
        colMedido.HeaderText = "R medida (Ω)";
        colMedido.Name = "colMedido";
        colMedido.ReadOnly = true;
        // 
        // colNominal
        // 
        colNominal.FillWeight = 25F;
        colNominal.HeaderText = "Nominal ±tol (Ω)";
        colNominal.Name = "colNominal";
        colNominal.ReadOnly = true;
        // 
        // colResultado
        // 
        colResultado.FillWeight = 15F;
        colResultado.HeaderText = "Resultado";
        colResultado.Name = "colResultado";
        colResultado.ReadOnly = true;
        // 
        // grpProgress
        // 
        grpProgress.Controls.Add(progressBar);
        grpProgress.Controls.Add(lblCurrentStep);
        grpProgress.Controls.Add(lblMachineState);
        grpProgress.Location = new Point(3, 137);
        grpProgress.Name = "grpProgress";
        grpProgress.Size = new Size(1250, 52);
        grpProgress.TabIndex = 2;
        grpProgress.TabStop = false;
        grpProgress.Text = "Progreso";
        // 
        // progressBar
        // 
        progressBar.Location = new Point(8, 18);
        progressBar.Name = "progressBar";
        progressBar.Size = new Size(700, 22);
        progressBar.TabIndex = 0;
        // 
        // lblCurrentStep
        // 
        lblCurrentStep.AutoSize = true;
        lblCurrentStep.Location = new Point(720, 22);
        lblCurrentStep.Name = "lblCurrentStep";
        lblCurrentStep.Size = new Size(24, 15);
        lblCurrentStep.TabIndex = 1;
        lblCurrentStep.Text = "—";
        // 
        // lblMachineState
        // 
        lblMachineState.AutoSize = true;
        lblMachineState.ForeColor = Color.Gray;
        lblMachineState.Location = new Point(1050, 22);
        lblMachineState.Name = "lblMachineState";
        lblMachineState.Size = new Size(67, 15);
        lblMachineState.TabIndex = 2;
        lblMachineState.Text = "Estado: Idle";
        // 
        // grpLog
        // 
        grpLog.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpLog.Controls.Add(txtLog);
        grpLog.Location = new Point(3, 651);
        grpLog.Name = "grpLog";
        grpLog.Size = new Size(1250, 200);
        grpLog.TabIndex = 5;
        grpLog.TabStop = false;
        grpLog.Text = "Log de ejecución";
        // 
        // txtLog
        // 
        txtLog.BackColor = Color.Black;
        txtLog.Dock = DockStyle.Fill;
        txtLog.Font = new Font("Consolas", 9F);
        txtLog.ForeColor = Color.LightGreen;
        txtLog.Location = new Point(3, 19);
        txtLog.Multiline = true;
        txtLog.Name = "txtLog";
        txtLog.ReadOnly = true;
        txtLog.ScrollBars = ScrollBars.Vertical;
        txtLog.Size = new Size(1244, 178);
        txtLog.TabIndex = 0;
        // 
        // AutomaticTestPanel
        // 
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.Add(grpConnection);
        Controls.Add(grpEnsayo);
        Controls.Add(grpProgress);
        Controls.Add(grpImagen);
        Controls.Add(grpResultados);
        Controls.Add(grpLog);
        MinimumSize = new Size(900, 600);
        Name = "AutomaticTestPanel";
        Size = new Size(1260, 860);
        grpConnection.ResumeLayout(false);
        grpConnection.PerformLayout();
        grpEnsayo.ResumeLayout(false);
        grpEnsayo.PerformLayout();
        grpImagen.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)picReferencia).EndInit();
        grpResultados.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)gridResultados).EndInit();
        grpProgress.ResumeLayout(false);
        grpProgress.PerformLayout();
        grpLog.ResumeLayout(false);
        grpLog.PerformLayout();
        ResumeLayout(false);
    }

    #endregion

    // Conexión
    private GroupBox   grpConnection;
    private Label      lblPort;
    private ComboBox   cmbPort;
    private Label      lblBaudRate;
    private ComboBox   cmbBaudRate;
    private Button     btnRefreshPorts;
    private Button     btnConnect;
    private Button     btnDisconnect;
    private Label      lblConnectionStatus;

    // Ensayo
    private GroupBox   grpEnsayo;
    private Label      lblReferencia;
    private ComboBox   cmbReferencia;
    private Button     btnRefreshRefs;
    private Label      lblOperario;
    private TextBox    txtOperario;
    private Label      lblLote;
    private TextBox    txtLote;
    private Button     btnStartTest;
    private Button     btnAbortTest;

    // Imagen
    private GroupBox   grpImagen;
    private PictureBox picReferencia;

    // Resultados
    private GroupBox           grpResultados;
    private DataGridView       gridResultados;
    private DataGridViewTextBoxColumn colPaso;
    private DataGridViewTextBoxColumn colContacto;
    private DataGridViewTextBoxColumn colMedido;
    private DataGridViewTextBoxColumn colNominal;
    private DataGridViewTextBoxColumn colResultado;

    // Progreso
    private GroupBox   grpProgress;
    private ProgressBar progressBar;
    private Label      lblCurrentStep;
    private Label      lblMachineState;

    // Log
    private GroupBox   grpLog;
    private TextBox    txtLog;
}
