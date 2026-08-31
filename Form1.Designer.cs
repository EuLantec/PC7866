namespace PC7866
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            statusStrip1 = new StatusStrip();
            menuStrip1 = new MenuStrip();
            archivoToolStripMenuItem = new ToolStripMenuItem();
            configuraciónToolStripMenuItem = new ToolStripMenuItem();
            salirToolStripMenuItem = new ToolStripMenuItem();
            modoToolStripMenuItem = new ToolStripMenuItem();
            mToolStripMenuItem = new ToolStripMenuItem();
            automáticoToolStripMenuItem = new ToolStripMenuItem();
            parámetrosToolStripMenuItem = new ToolStripMenuItem();
            informesToolStripMenuItem = new ToolStripMenuItem();
            acercaDeToolStripMenuItem = new ToolStripMenuItem();
            panelContent = new Panel();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // statusStrip1
            // 
            statusStrip1.Location = new Point(0, 579);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new Size(804, 22);
            statusStrip1.TabIndex = 0;
            statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { archivoToolStripMenuItem, modoToolStripMenuItem, parámetrosToolStripMenuItem, informesToolStripMenuItem, acercaDeToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(804, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // archivoToolStripMenuItem
            // 
            archivoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { configuraciónToolStripMenuItem, salirToolStripMenuItem });
            archivoToolStripMenuItem.Name = "archivoToolStripMenuItem";
            archivoToolStripMenuItem.Size = new Size(60, 20);
            archivoToolStripMenuItem.Text = "Archivo";
            // 
            // configuraciónToolStripMenuItem
            // 
            configuraciónToolStripMenuItem.Name = "configuraciónToolStripMenuItem";
            configuraciónToolStripMenuItem.Size = new Size(180, 22);
            configuraciónToolStripMenuItem.Text = "Configuración";
            // 
            // salirToolStripMenuItem
            // 
            salirToolStripMenuItem.Name = "salirToolStripMenuItem";
            salirToolStripMenuItem.Size = new Size(180, 22);
            salirToolStripMenuItem.Text = "Salir";
            // 
            // modoToolStripMenuItem
            // 
            modoToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { mToolStripMenuItem, automáticoToolStripMenuItem });
            modoToolStripMenuItem.Name = "modoToolStripMenuItem";
            modoToolStripMenuItem.Size = new Size(51, 20);
            modoToolStripMenuItem.Text = "Modo";
            // 
            // mToolStripMenuItem
            // 
            mToolStripMenuItem.Name = "mToolStripMenuItem";
            mToolStripMenuItem.Size = new Size(180, 22);
            mToolStripMenuItem.Text = "Manual";
            // 
            // automáticoToolStripMenuItem
            // 
            automáticoToolStripMenuItem.Name = "automáticoToolStripMenuItem";
            automáticoToolStripMenuItem.Size = new Size(180, 22);
            automáticoToolStripMenuItem.Text = "Automático";
            // 
            // parámetrosToolStripMenuItem
            // 
            parámetrosToolStripMenuItem.Name = "parámetrosToolStripMenuItem";
            parámetrosToolStripMenuItem.Size = new Size(79, 20);
            parámetrosToolStripMenuItem.Text = "Parámetros";
            // 
            // informesToolStripMenuItem
            // 
            informesToolStripMenuItem.Name = "informesToolStripMenuItem";
            informesToolStripMenuItem.Size = new Size(66, 20);
            informesToolStripMenuItem.Text = "Informes";
            // 
            // acercaDeToolStripMenuItem
            // 
            acercaDeToolStripMenuItem.Name = "acercaDeToolStripMenuItem";
            acercaDeToolStripMenuItem.Size = new Size(72, 20);
            acercaDeToolStripMenuItem.Text = "Acerca De";
            // 
            // panelContent
            // 
            panelContent.Dock = DockStyle.Fill;
            panelContent.Location = new Point(0, 24);
            panelContent.Name = "panelContent";
            panelContent.Size = new Size(804, 555);
            panelContent.TabIndex = 0;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(96F, 96F);
            AutoScaleMode = AutoScaleMode.Dpi;
            ClientSize = new Size(804, 601);
            Controls.Add(panelContent);
            Controls.Add(statusStrip1);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(820, 640);
            Name = "Form1";
            Text = "PC7866 – Test Resistivo Embega";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private StatusStrip statusStrip1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem archivoToolStripMenuItem;
        private ToolStripMenuItem salirToolStripMenuItem;
        private ToolStripMenuItem modoToolStripMenuItem;
        private ToolStripMenuItem parámetrosToolStripMenuItem;
        private ToolStripMenuItem informesToolStripMenuItem;
        private ToolStripMenuItem acercaDeToolStripMenuItem;
        private ToolStripMenuItem mToolStripMenuItem;
        private ToolStripMenuItem automáticoToolStripMenuItem;
        private ToolStripMenuItem configuraciónToolStripMenuItem;
        private System.Windows.Forms.Panel panelContent;
    }
}
