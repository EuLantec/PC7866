using System.Reflection;
using PC7866.Utils;

namespace PC7866.Views;

/// <summary>Diálogo "Acerca de" con la información de la aplicación.</summary>
public sealed class AboutForm : Form
{
    public AboutForm()
    {
        var asm = Assembly.GetExecutingAssembly();
        string version = asm.GetName().Version?.ToString(3) ?? "1.0.0";

        Text            = "Acerca de PC7866";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition   = FormStartPosition.CenterParent;
        MaximizeBox     = false;
        MinimizeBox     = false;
        ShowInTaskbar   = false;
        ClientSize      = new Size(460, 340);
        BackColor       = UiTheme.Surface;
        Font            = UiTheme.BaseFont;
        AutoScaleMode   = AutoScaleMode.Font;
        AutoScaleDimensions = new SizeF(7F, 15F);

        // Banda superior de color con el nombre de la app.
        var header = new Panel
        {
            Dock      = DockStyle.Top,
            Height    = 88,
            BackColor = UiTheme.Primary
        };
        var lblTitle = new Label
        {
            Text      = "PC7866",
            ForeColor = Color.White,
            Font      = new Font("Segoe UI Semibold", 22F),
            AutoSize  = true,
            Location  = new Point(24, 14),
            BackColor = Color.Transparent
        };
        var lblSubtitle = new Label
        {
            Text      = "Banco de test resistivo Embega",
            ForeColor = Color.FromArgb(220, 235, 233),
            Font      = new Font("Segoe UI", 10F),
            AutoSize  = true,
            Location  = new Point(26, 56),
            BackColor = Color.Transparent
        };
        header.Controls.Add(lblSubtitle);
        header.Controls.Add(lblTitle);

        var lblBody = new Label
        {
            AutoSize  = false,
            Dock      = DockStyle.Fill,
            Padding   = new Padding(24, 20, 24, 12),
            ForeColor = UiTheme.TextPrimary,
            Font      = new Font("Segoe UI", 9.75F),
            Text      =
                $"Versión {version}\n\n" +
                "Software de control y verificación de continuidad/resistencia de placas " +
                "mediante el equipo PC7866 (comunicación serie).\n\n" +
                "Funciones: modo manual, ensayo automático punto a punto, gestión de " +
                "referencias y parámetros, informes y exportación de resultados.\n\n" +
                "Tecnología: .NET 10 · WinForms · MariaDB/MySQL (Dapper) · PdfSharpCore."
        };

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 56, BackColor = UiTheme.Surface };
        var lblCopyright = new Label
        {
            Text      = $"© {DateTime.Now:yyyy} Embega — Todos los derechos reservados",
            ForeColor = UiTheme.TextMuted,
            AutoSize  = true,
            Location  = new Point(24, 20)
        };
        var btnOk = new Button
        {
            Text     = "Aceptar",
            Size     = new Size(96, 30),
            Location = new Point(ClientSize.Width - 96 - 24, 14),
            Anchor   = AnchorStyles.Top | AnchorStyles.Right,
            DialogResult = DialogResult.OK
        };
        UiTheme.StyleButton(btnOk);
        footer.Controls.Add(lblCopyright);
        footer.Controls.Add(btnOk);

        Controls.Add(lblBody);
        Controls.Add(footer);
        Controls.Add(header);

        AcceptButton = btnOk;
        CancelButton = btnOk;
    }
}
