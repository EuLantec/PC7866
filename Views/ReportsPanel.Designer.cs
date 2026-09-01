namespace PC7866.Views;

partial class ReportsPanel
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
        grpFiltros     = new GroupBox();
        lblFiltroRef   = new Label();
        cmbFiltroRef   = new ComboBox();
        lblFiltroRes   = new Label();
        cmbFiltroResultado = new ComboBox();
        lblDesde       = new Label();
        dtpDesde       = new DateTimePicker();
        lblHasta       = new Label();
        dtpHasta       = new DateTimePicker();
        lblFiltroOp    = new Label();
        txtFiltroOperario = new TextBox();
        lblFiltroLote  = new Label();
        txtFiltroLote  = new TextBox();
        btnBuscar      = new Button();
        btnRefrescar   = new Button();

        grpResultados  = new GroupBox();
        gridResultados = new DataGridView();
        colR_Id        = new DataGridViewTextBoxColumn();
        colR_Fecha     = new DataGridViewTextBoxColumn();
        colR_Ref       = new DataGridViewTextBoxColumn();
        colR_Op        = new DataGridViewTextBoxColumn();
        colR_Lote      = new DataGridViewTextBoxColumn();
        colR_Resultado = new DataGridViewTextBoxColumn();

        pnlBottom      = new Panel();
        lblTotal       = new Label();
        btnVerDetalle  = new Button();
        btnExportCsv   = new Button();

        grpFiltros.SuspendLayout();
        grpResultados.SuspendLayout();
        pnlBottom.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)gridResultados).BeginInit();
        SuspendLayout();

        // ── grpFiltros ───────────────────────────────────────────────────────
        grpFiltros.Text = "Filtros";
        grpFiltros.Location = new Point(3, 3);
        grpFiltros.Size = new Size(1250, 62);
        grpFiltros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

        int fx = 0;
        void AddFiltro(Label l, string lt, Control c, int w)
        {
            l.Text = lt; l.AutoSize = true; l.Location = new Point(fx, 24);
            grpFiltros.Controls.Add(l);
            fx += l.PreferredWidth + 9;
            c.Location = new Point(fx, 20); c.Size = new Size(w, 24);
            grpFiltros.Controls.Add(c);
            fx += w + 12;
        }

        AddFiltro(lblFiltroRef,  "Referencia:",  cmbFiltroRef,       160);
        AddFiltro(lblFiltroRes,  "Resultado:",   cmbFiltroResultado,  90);
        AddFiltro(lblDesde,      "Desde:",       dtpDesde,           200);
        AddFiltro(lblHasta,      "Hasta:",       dtpHasta,           200);
        AddFiltro(lblFiltroOp,   "Operario:",    txtFiltroOperario,  110);
        AddFiltro(lblFiltroLote, "Lote:",        txtFiltroLote,       90);

        cmbFiltroRef.DropDownStyle       = ComboBoxStyle.DropDownList;
        cmbFiltroResultado.DropDownStyle = ComboBoxStyle.DropDownList;
        cmbFiltroResultado.Items.AddRange(new object[] { "(Todos)", "✅ Bueno", "❌ Malo" });
        cmbFiltroResultado.SelectedIndex = 0;

        btnBuscar.Text     = "🔍 Buscar";
        btnBuscar.Location = new Point(fx, 19); btnBuscar.Size = new Size(95, 26);
        grpFiltros.Controls.Add(btnBuscar);
        fx += 103;

        btnRefrescar.Text     = "🔄 Recargar";
        btnRefrescar.Location = new Point(fx, 19); btnRefrescar.Size = new Size(100, 26);
        grpFiltros.Controls.Add(btnRefrescar);

        // ── grpResultados ────────────────────────────────────────────────────
        grpResultados.Text = "Resultados";
        grpResultados.Location = new Point(3, 71);
        grpResultados.Size = new Size(1250, 660);
        grpResultados.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
        grpResultados.Controls.Add(gridResultados);

        gridResultados.Dock = DockStyle.Fill;
        gridResultados.AllowUserToAddRows    = false;
        gridResultados.AllowUserToDeleteRows = false;
        gridResultados.ReadOnly              = true;
        gridResultados.SelectionMode         = DataGridViewSelectionMode.FullRowSelect;
        gridResultados.RowHeadersVisible     = false;
        gridResultados.AutoSizeColumnsMode   = DataGridViewAutoSizeColumnsMode.Fill;
        gridResultados.Font = new Font("Segoe UI", 9f);

        colR_Id.HeaderText = "ID";           colR_Id.FillWeight = 6;
        colR_Fecha.HeaderText = "Fecha";     colR_Fecha.FillWeight = 20;
        colR_Ref.HeaderText = "Referencia";  colR_Ref.FillWeight = 22;
        colR_Op.HeaderText = "Operario";     colR_Op.FillWeight = 16;
        colR_Lote.HeaderText = "Lote";       colR_Lote.FillWeight = 14;
        colR_Resultado.HeaderText = "Resultado"; colR_Resultado.FillWeight = 12;
        gridResultados.Columns.AddRange(colR_Id, colR_Fecha, colR_Ref, colR_Op, colR_Lote, colR_Resultado);

        // ── pnlBottom ─────────────────────────────────────────────────────────
        pnlBottom.Dock = DockStyle.Bottom;
        pnlBottom.Height = 44;
        pnlBottom.BackColor = Color.FromArgb(235, 238, 245);

        lblTotal.AutoSize = true; lblTotal.Location = new Point(10, 14);
        lblTotal.Font = new Font("Segoe UI", 9f, FontStyle.Bold);
        lblTotal.Text = "Total: 0 resultados";

        btnVerDetalle.Text     = "🔎 Ver detalle";
        btnVerDetalle.Location = new Point(900, 8); btnVerDetalle.Size = new Size(130, 28);
        btnVerDetalle.Font = new Font("Segoe UI", 9f);

        btnExportCsv.Text      = "💾 Exportar CSV";
        btnExportCsv.Location  = new Point(1038, 8); btnExportCsv.Size = new Size(140, 28);
        btnExportCsv.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
        btnExportCsv.BackColor = Color.FromArgb(0, 153, 76);
        btnExportCsv.ForeColor = Color.White;
        btnExportCsv.FlatStyle = FlatStyle.Flat;

        pnlBottom.Controls.AddRange(new Control[] { lblTotal, btnVerDetalle, btnExportCsv });

        // ── ReportsPanel ──────────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(96f, 96f);
        AutoScaleMode = AutoScaleMode.Dpi;
        Controls.AddRange(new Control[] { grpFiltros, grpResultados, pnlBottom });
        Name = "ReportsPanel";
        Size = new Size(1260, 780);
        MinimumSize = new Size(900, 500);

        grpFiltros.ResumeLayout(false); grpFiltros.PerformLayout();
        grpResultados.ResumeLayout(false);
        pnlBottom.ResumeLayout(false); pnlBottom.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)gridResultados).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private GroupBox   grpFiltros;
    private Label      lblFiltroRef;
    private ComboBox   cmbFiltroRef;
    private Label      lblFiltroRes;
    private ComboBox   cmbFiltroResultado;
    private Label      lblDesde;
    private DateTimePicker dtpDesde;
    private Label      lblHasta;
    private DateTimePicker dtpHasta;
    private Label      lblFiltroOp;
    private TextBox    txtFiltroOperario;
    private Label      lblFiltroLote;
    private TextBox    txtFiltroLote;
    private Button     btnBuscar;
    private Button     btnRefrescar;

    private GroupBox   grpResultados;
    private DataGridView gridResultados;
    private DataGridViewTextBoxColumn colR_Id;
    private DataGridViewTextBoxColumn colR_Fecha;
    private DataGridViewTextBoxColumn colR_Ref;
    private DataGridViewTextBoxColumn colR_Op;
    private DataGridViewTextBoxColumn colR_Lote;
    private DataGridViewTextBoxColumn colR_Resultado;

    private Panel  pnlBottom;
    private Label  lblTotal;
    private Button btnVerDetalle;
    private Button btnExportCsv;
}
