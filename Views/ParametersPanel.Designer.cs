namespace PC7866.Views;

partial class ParametersPanel
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
        // ── Controles Referencias ────────────────────────────────────────────
        grpRefs      = new GroupBox();
        listReferencias = new ListBox();
        pnlRefForm   = new Panel();
        lblRefNombre = new Label();
        txtRefNombre = new TextBox();
        lblRefDesc   = new Label();
        txtRefDesc   = new TextBox();
        chkActiva    = new CheckBox();
        btnCargarImagen = new Button();
        picPreview   = new PictureBox();
        btnNuevaRef  = new Button();
        btnGuardarRef  = new Button();
        btnEliminarRef = new Button();

        // ── Controles Parámetros ─────────────────────────────────────────────
        grpParams      = new GroupBox();
        gridParametros = new DataGridView();
        colP_Id        = new DataGridViewTextBoxColumn();
        colP_Paso      = new DataGridViewTextBoxColumn();
        colP_Contacto  = new DataGridViewTextBoxColumn();
        colP_Nominal   = new DataGridViewTextBoxColumn();
        colP_Tol       = new DataGridViewTextBoxColumn();
        colP_Offset    = new DataGridViewTextBoxColumn();
        colP_Minima    = new DataGridViewTextBoxColumn();
        colP_PosX      = new DataGridViewTextBoxColumn();
        colP_PosY      = new DataGridViewTextBoxColumn();

        pnlParamForm   = new Panel();
        lblPaso        = new Label();
        nudPaso        = new NumericUpDown();
        lblContacto    = new Label();
        txtContacto    = new TextBox();
        lblNominal     = new Label();
        nudNominal     = new NumericUpDown();
        lblTol         = new Label();
        nudTol         = new NumericUpDown();
        lblOffset      = new Label();
        nudOffset      = new NumericUpDown();
        lblMinima      = new Label();
        nudMinima      = new NumericUpDown();
        lblPosX        = new Label();
        nudPosX        = new NumericUpDown();
        lblPosY        = new Label();
        nudPosY        = new NumericUpDown();
        lblSalidas     = new Label();
        txtSalidas     = new TextBox();
        btnNuevoParam    = new Button();
        btnGuardarParam  = new Button();
        btnEliminarParam = new Button();

        grpRefs.SuspendLayout();
        grpParams.SuspendLayout();
        pnlRefForm.SuspendLayout();
        pnlParamForm.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).BeginInit();
        ((System.ComponentModel.ISupportInitialize)gridParametros).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudPaso).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudNominal).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudTol).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudOffset).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudMinima).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudPosX).BeginInit();
        ((System.ComponentModel.ISupportInitialize)nudPosY).BeginInit();
        SuspendLayout();

        // ════════════════════════════════════════════════════════════════════
        // grpRefs
        // ════════════════════════════════════════════════════════════════════
        grpRefs.Text = "Referencias";
        grpRefs.Location = new Point(3, 3);
        grpRefs.Size = new Size(610, 820);
        grpRefs.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom;

        listReferencias.Location = new Point(8, 20);
        listReferencias.Size = new Size(590, 180);
        listReferencias.DisplayMember = "ReferenciaNombre";

        pnlRefForm.Location = new Point(8, 208);
        pnlRefForm.Size = new Size(590, 560);

        lblRefNombre.Text = "Nombre:"; lblRefNombre.AutoSize = true; lblRefNombre.Location = new Point(0, 4);
        txtRefNombre.Location = new Point(75, 0); txtRefNombre.Size = new Size(300, 23);
        chkActiva.Text = "Activa"; chkActiva.Location = new Point(390, 2); chkActiva.AutoSize = true;

        lblRefDesc.Text = "Descripción:"; lblRefDesc.AutoSize = true; lblRefDesc.Location = new Point(0, 34);
        txtRefDesc.Location = new Point(75, 30); txtRefDesc.Size = new Size(500, 80);
        txtRefDesc.Multiline = true; txtRefDesc.ScrollBars = ScrollBars.Vertical;

        btnCargarImagen.Text = "📂 Cargar imagen"; btnCargarImagen.Location = new Point(0, 118); btnCargarImagen.Size = new Size(140, 28);

        picPreview.Location = new Point(0, 152); picPreview.Size = new Size(580, 340);
        picPreview.SizeMode = PictureBoxSizeMode.Zoom;
        picPreview.BackColor = Color.FromArgb(240, 240, 240);
        picPreview.BorderStyle = BorderStyle.FixedSingle;

        btnNuevaRef.Text = "➕ Nueva"; btnNuevaRef.Location = new Point(0, 500); btnNuevaRef.Size = new Size(100, 28);
        btnGuardarRef.Text = "💾 Guardar"; btnGuardarRef.Location = new Point(108, 500); btnGuardarRef.Size = new Size(110, 28);
        btnEliminarRef.Text = "🗑 Desactivar"; btnEliminarRef.Location = new Point(226, 500); btnEliminarRef.Size = new Size(120, 28);

        pnlRefForm.Controls.AddRange(new Control[]
        {
            lblRefNombre, txtRefNombre, chkActiva,
            lblRefDesc, txtRefDesc,
            btnCargarImagen, picPreview,
            btnNuevaRef, btnGuardarRef, btnEliminarRef
        });

        grpRefs.Controls.AddRange(new Control[] { listReferencias, pnlRefForm });

        // ════════════════════════════════════════════════════════════════════
        // grpParams
        // ════════════════════════════════════════════════════════════════════
        grpParams.Text = "Parámetros de ensayo";
        grpParams.Location = new Point(620, 3);
        grpParams.Size = new Size(630, 820);
        grpParams.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;

        gridParametros.Location = new Point(8, 20);
        gridParametros.Size = new Size(610, 340);
        gridParametros.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        gridParametros.AllowUserToAddRows = false;
        gridParametros.AllowUserToDeleteRows = false;
        gridParametros.ReadOnly = true;
        gridParametros.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        gridParametros.RowHeadersVisible = false;
        gridParametros.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        colP_Id.HeaderText = "ID";        colP_Id.FillWeight = 6;  colP_Id.Visible = false;
        colP_Paso.HeaderText = "Paso";    colP_Paso.FillWeight = 8;
        colP_Contacto.HeaderText = "Contacto"; colP_Contacto.FillWeight = 18;
        colP_Nominal.HeaderText = "Nominal Ω"; colP_Nominal.FillWeight = 14;
        colP_Tol.HeaderText = "Tol Ω";   colP_Tol.FillWeight = 12;
        colP_Offset.HeaderText = "Offset"; colP_Offset.FillWeight = 12;
        colP_Minima.HeaderText = "R mín (corto)"; colP_Minima.FillWeight = 14;
        colP_PosX.HeaderText = "PosX";   colP_PosX.FillWeight = 10;
        colP_PosY.HeaderText = "PosY";   colP_PosY.FillWeight = 10;

        colP_Id.Name = "colP_Id"; colP_Paso.Name = "colP_Paso"; colP_Contacto.Name = "colP_Contacto";
        colP_Nominal.Name = "colP_Nominal"; colP_Tol.Name = "colP_Tol"; colP_Offset.Name = "colP_Offset";
        colP_Minima.Name = "colP_Minima";
        colP_PosX.Name = "colP_PosX"; colP_PosY.Name = "colP_PosY";

        gridParametros.Columns.AddRange(colP_Id, colP_Paso, colP_Contacto,
            colP_Nominal, colP_Tol, colP_Offset, colP_Minima, colP_PosX, colP_PosY);

        // Form edición de parámetro
        pnlParamForm.Location = new Point(8, 368);
        pnlParamForm.Size = new Size(610, 400);

        int lx = 0, fx = 100, fy = 0, fh = 28;
        void AddRow(Label l, string lt, Control c, ref int y)
        {
            l.Text = lt; l.AutoSize = true; l.Location = new Point(lx, y + 4);
            c.Location = new Point(fx, y); c.Size = new Size(200, fh);
            pnlParamForm.Controls.AddRange(new Control[] { l, c });
            y += fh + 6;
        }

        int row = 0;
        AddRow(lblPaso,     "Paso nº:",      nudPaso,    ref row);
        AddRow(lblContacto, "Contacto:",     txtContacto, ref row);
        AddRow(lblNominal,  "R nominal (Ω):",nudNominal,  ref row);
        AddRow(lblTol,      "Tolerancia (Ω):",nudTol,     ref row);
        AddRow(lblOffset,   "Offset (Ω):",   nudOffset,  ref row);
        AddRow(lblMinima,   "R mín cortocircuito (Ω):", nudMinima, ref row);
        AddRow(lblPosX,     "Pos X:",         nudPosX,    ref row);
        AddRow(lblPosY,     "Pos Y:",         nudPosY,    ref row);
        AddRow(lblSalidas,  "Salidas activas\n(1-48, coma):", txtSalidas, ref row);

        nudNominal.DecimalPlaces = 2; nudNominal.Maximum = 100000;
        nudTol.DecimalPlaces     = 2; nudTol.Maximum     = 10000;
        nudOffset.DecimalPlaces  = 2; nudOffset.Minimum  = -1000; nudOffset.Maximum = 1000;
        nudMinima.DecimalPlaces  = 2; nudMinima.Minimum  = 0; nudMinima.Maximum = 100000;
        nudPosX.Maximum = 4000; nudPosY.Maximum = 4000;

        btnNuevoParam.Text    = "➕ Nuevo";   btnNuevoParam.Location    = new Point(0, row + 10); btnNuevoParam.Size = new Size(100, 28);
        btnGuardarParam.Text  = "💾 Guardar"; btnGuardarParam.Location  = new Point(108, row + 10); btnGuardarParam.Size = new Size(110, 28);
        btnEliminarParam.Text = "🗑 Eliminar"; btnEliminarParam.Location = new Point(226, row + 10); btnEliminarParam.Size = new Size(110, 28);
        pnlParamForm.Controls.AddRange(new Control[] { btnNuevoParam, btnGuardarParam, btnEliminarParam });

        grpParams.Controls.AddRange(new Control[] { gridParametros, pnlParamForm });

        // ── ParametersPanel ──────────────────────────────────────────────────
        AutoScaleDimensions = new SizeF(7f, 15f);
        AutoScaleMode = AutoScaleMode.Font;
        Controls.AddRange(new Control[] { grpRefs, grpParams });
        Name = "ParametersPanel";
        Size = new Size(1260, 830);
        MinimumSize = new Size(900, 600);

        grpRefs.ResumeLayout(false); grpRefs.PerformLayout();
        grpParams.ResumeLayout(false); grpParams.PerformLayout();
        pnlRefForm.ResumeLayout(false); pnlRefForm.PerformLayout();
        pnlParamForm.ResumeLayout(false); pnlParamForm.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)picPreview).EndInit();
        ((System.ComponentModel.ISupportInitialize)gridParametros).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudPaso).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudNominal).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudTol).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudOffset).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudMinima).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudPosX).EndInit();
        ((System.ComponentModel.ISupportInitialize)nudPosY).EndInit();
        ResumeLayout(false);
    }

    #endregion

    // Referencias
    private GroupBox  grpRefs;
    private ListBox   listReferencias;
    private Panel     pnlRefForm;
    private Label     lblRefNombre;
    private TextBox   txtRefNombre;
    private Label     lblRefDesc;
    private TextBox   txtRefDesc;
    private CheckBox  chkActiva;
    private Button    btnCargarImagen;
    private PictureBox picPreview;
    private Button    btnNuevaRef;
    private Button    btnGuardarRef;
    private Button    btnEliminarRef;

    // Parámetros
    private GroupBox      grpParams;
    private DataGridView  gridParametros;
    private DataGridViewTextBoxColumn colP_Id;
    private DataGridViewTextBoxColumn colP_Paso;
    private DataGridViewTextBoxColumn colP_Contacto;
    private DataGridViewTextBoxColumn colP_Nominal;
    private DataGridViewTextBoxColumn colP_Tol;
    private DataGridViewTextBoxColumn colP_Offset;
    private DataGridViewTextBoxColumn colP_Minima;
    private DataGridViewTextBoxColumn colP_PosX;
    private DataGridViewTextBoxColumn colP_PosY;

    private Panel    pnlParamForm;
    private Label    lblPaso;
    private NumericUpDown nudPaso;
    private Label    lblContacto;
    private TextBox  txtContacto;
    private Label    lblNominal;
    private NumericUpDown nudNominal;
    private Label    lblTol;
    private NumericUpDown nudTol;
    private Label    lblOffset;
    private NumericUpDown nudOffset;
    private Label    lblMinima;
    private NumericUpDown nudMinima;
    private Label    lblPosX;
    private NumericUpDown nudPosX;
    private Label    lblPosY;
    private NumericUpDown nudPosY;
    private Label    lblSalidas;
    private TextBox  txtSalidas;
    private Button   btnNuevoParam;
    private Button   btnGuardarParam;
    private Button   btnEliminarParam;
}
