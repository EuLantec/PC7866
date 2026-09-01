using System.Drawing.Drawing2D;

namespace PC7866.Utils;

/// <summary>
/// Paleta y utilidades de estilo centralizadas para dar un aspecto moderno y coherente a la app.
/// Respeta los colores semánticos ya definidos en los paneles (p.ej. conectar=verde, abortar=rojo):
/// solo re-estiliza botones sin color explícito y armoniza rejillas, group boxes y el menú.
/// </summary>
public static class UiTheme
{
    // ── Paleta ────────────────────────────────────────────────────────────────
    public static readonly Color Background   = Color.FromArgb(0xF4, 0xF6, 0xF9);
    public static readonly Color Surface      = Color.White;
    public static readonly Color Primary      = Color.FromArgb(0x0F, 0x76, 0x6E); // teal-700
    public static readonly Color PrimaryDark  = Color.FromArgb(0x11, 0x5E, 0x59);
    public static readonly Color PrimaryHover = Color.FromArgb(0x0D, 0x94, 0x88);
    public static readonly Color SelectionSoft= Color.FromArgb(0xDC, 0xEF, 0xEC);
    public static readonly Color TextPrimary  = Color.FromArgb(0x1F, 0x29, 0x37);
    public static readonly Color TextMuted    = Color.FromArgb(0x6B, 0x72, 0x80);
    public static readonly Color BorderColor  = Color.FromArgb(0xE2, 0xE5, 0xEA);
    public static readonly Color Danger       = Color.FromArgb(0xC0, 0x3A, 0x2B);
    public static readonly Color RowAlt       = Color.FromArgb(0xF7, 0xF9, 0xFB);

    public static readonly Font BaseFont    = new("Segoe UI", 9.75f);
    public static readonly Font HeadingFont = new("Segoe UI Semibold", 9.75f);

    private static readonly string[] DangerWords = { "elimin", "desactiv", "borrar", "abort", "detener", "reset" };

    // ── Shell (Form principal) ─────────────────────────────────────────────────
    public static void ApplyShell(Form form, MenuStrip menu, StatusStrip status, Panel content)
    {
        form.BackColor  = Background;
        content.BackColor = Background;

        menu.Renderer   = new MenuRenderer();
        menu.BackColor  = Primary;
        menu.ForeColor  = Color.White;
        menu.Font       = BaseFont;
        menu.Padding    = new Padding(6, 2, 6, 2);
        foreach (ToolStripItem it in menu.Items) StyleMenuItem(it, topLevel: true);

        status.BackColor  = Surface;
        status.ForeColor  = TextMuted;
        status.Font       = BaseFont;
        status.SizingGrip = false;
    }

    private static void StyleMenuItem(ToolStripItem item, bool topLevel)
    {
        item.ForeColor = topLevel ? Color.White : TextPrimary;
        if (item is ToolStripMenuItem mi)
            foreach (ToolStripItem sub in mi.DropDownItems)
                StyleMenuItem(sub, topLevel: false);
    }

    // ── Paneles ────────────────────────────────────────────────────────────────
    public static void ApplyPanel(Control root)
    {
        if (root is UserControl) root.BackColor = Background;
        ApplyRecursive(root);
    }

    private static void ApplyRecursive(Control root)
    {
        foreach (Control c in root.Controls)
        {
            switch (c)
            {
                case Button b:        StyleButton(b);  break;
                case GroupBox g:      StyleGroup(g);   break;
                case DataGridView dg: StyleGrid(dg);   break;
            }
            if (c.HasChildren) ApplyRecursive(c);
        }
    }

    private static void StyleGroup(GroupBox g)
    {
        g.ForeColor = PrimaryDark;
        g.Font      = HeadingFont;
    }

    public static void StyleButton(Button b)
    {
        // Tag "native" = dejar el botón con el aspecto estándar de Windows, sin temar.
        if (b.Tag as string == "native") return;

        b.FlatStyle = FlatStyle.Flat;
        b.FlatAppearance.BorderSize = 0;
        b.Cursor = Cursors.Hand;
        b.UseVisualStyleBackColor = false;

        bool hasCustom = b.BackColor != SystemColors.Control
                      && b.BackColor != Color.Empty
                      && b.BackColor != Color.Transparent;
        if (!hasCustom)
        {
            string t = (b.Text ?? string.Empty).ToLowerInvariant();
            bool danger = DangerWords.Any(w => t.Contains(w));
            b.BackColor = danger ? Danger : Primary;
            b.ForeColor = Color.White;
        }

        b.FlatAppearance.MouseOverBackColor = ControlPaint.Light(b.BackColor, 0.15f);
        b.FlatAppearance.MouseDownBackColor = ControlPaint.Dark(b.BackColor, 0.05f);
    }

    // Da forma de píldora a un botón de acción destacado (p.ej. Iniciar/Abortar).
    public static void RoundButton(Button b, int radius = 14)
    {
        void Apply()
        {
            var rect = b.ClientRectangle;
            if (rect.Width <= 0 || rect.Height <= 0) return;
            int d = Math.Min(radius * 2, Math.Min(rect.Width, rect.Height));
            using var path = new GraphicsPath();
            path.StartFigure();
            path.AddArc(rect.X, rect.Y, d, d, 180, 90);
            path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
            path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
            path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            b.Region = new Region(path);
        }

        Apply();
        b.Resize += (_, _) => Apply();
    }

    private static void StyleGrid(DataGridView dg)
    {
        dg.BackgroundColor        = Surface;
        dg.BorderStyle            = BorderStyle.None;
        dg.EnableHeadersVisualStyles = false;
        dg.GridColor              = BorderColor;
        dg.RowHeadersVisible      = false;
        dg.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;
        dg.CellBorderStyle        = DataGridViewCellBorderStyle.SingleHorizontal;

        dg.ColumnHeadersDefaultCellStyle.BackColor = Primary;
        dg.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
        dg.ColumnHeadersDefaultCellStyle.SelectionBackColor = Primary;
        dg.ColumnHeadersDefaultCellStyle.SelectionForeColor = Color.White;
        if (dg.ColumnHeadersDefaultCellStyle.Font is null || dg.ColumnHeadersDefaultCellStyle.Font.Size < 9f)
            dg.ColumnHeadersDefaultCellStyle.Font = HeadingFont;
        dg.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        if (dg.ColumnHeadersHeight < 30) dg.ColumnHeadersHeight = 32;

        dg.DefaultCellStyle.SelectionBackColor = PrimaryHover;
        dg.DefaultCellStyle.SelectionForeColor = Color.White;
        dg.AlternatingRowsDefaultCellStyle.BackColor = RowAlt;
    }

    // ── Renderer del menú ──────────────────────────────────────────────────────
    private sealed class MenuRenderer : ToolStripProfessionalRenderer
    {
        public MenuRenderer() : base(new MenuColors()) { RoundedEdges = false; }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            var g = e.Graphics;
            var item = e.Item;
            var rect = new Rectangle(Point.Empty, item.Size);
            bool topLevel = item.OwnerItem is null;
            bool selected = item.Selected || (item is ToolStripMenuItem mi && (mi.Pressed || mi.DropDown.Visible));
            bool active   = item is ToolStripMenuItem mc && mc.Checked;

            Color? fill = null;
            if (topLevel)
            {
                if (selected)    fill = PrimaryDark;
                else if (active) fill = PrimaryDark;
                else             fill = Primary;
            }
            else if (selected)
            {
                fill = SelectionSoft;
            }
            else if (active)
            {
                fill = SelectionSoft;
            }

            if (fill is { } c)
            {
                using var b = new SolidBrush(c);
                g.FillRectangle(b, rect);
            }
        }
    }

    private sealed class MenuColors : ProfessionalColorTable
    {
        public override Color MenuStripGradientBegin      => Primary;
        public override Color MenuStripGradientEnd        => Primary;
        public override Color ToolStripDropDownBackground  => Surface;
        public override Color ImageMarginGradientBegin     => Surface;
        public override Color ImageMarginGradientMiddle    => Surface;
        public override Color ImageMarginGradientEnd       => Surface;
        public override Color MenuBorder                   => BorderColor;
        public override Color MenuItemBorder               => PrimaryHover;
        public override Color SeparatorDark                => BorderColor;
        public override Color SeparatorLight               => BorderColor;
    }
}
