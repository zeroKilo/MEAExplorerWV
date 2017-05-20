namespace PluginTalktableWV
{
    partial class TalkTableEditor
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.tableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportToTXTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importFromTXTToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addModJobToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAndCloseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.listBox1 = new System.Windows.Forms.ListBox();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tableToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(581, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // tableToolStripMenuItem
            // 
            this.tableToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportToTXTToolStripMenuItem,
            this.importFromTXTToolStripMenuItem,
            this.addModJobToolStripMenuItem,
            this.saveAndCloseToolStripMenuItem});
            this.tableToolStripMenuItem.Name = "tableToolStripMenuItem";
            this.tableToolStripMenuItem.Size = new System.Drawing.Size(45, 20);
            this.tableToolStripMenuItem.Text = "Table";
            // 
            // exportToTXTToolStripMenuItem
            // 
            this.exportToTXTToolStripMenuItem.Name = "exportToTXTToolStripMenuItem";
            this.exportToTXTToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.exportToTXTToolStripMenuItem.Text = "Export to TXT...";
            this.exportToTXTToolStripMenuItem.Click += new System.EventHandler(this.exportToTXTToolStripMenuItem_Click);
            // 
            // importFromTXTToolStripMenuItem
            // 
            this.importFromTXTToolStripMenuItem.Name = "importFromTXTToolStripMenuItem";
            this.importFromTXTToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.importFromTXTToolStripMenuItem.Text = "Import from TXT...";
            this.importFromTXTToolStripMenuItem.Click += new System.EventHandler(this.importFromTXTToolStripMenuItem_Click);
            // 
            // addModJobToolStripMenuItem
            // 
            this.addModJobToolStripMenuItem.Name = "addModJobToolStripMenuItem";
            this.addModJobToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.addModJobToolStripMenuItem.Text = "Add Mod Job";
            this.addModJobToolStripMenuItem.Click += new System.EventHandler(this.addModJobToolStripMenuItem_Click);
            // 
            // saveAndCloseToolStripMenuItem
            // 
            this.saveAndCloseToolStripMenuItem.Name = "saveAndCloseToolStripMenuItem";
            this.saveAndCloseToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.saveAndCloseToolStripMenuItem.Text = "Save and Close";
            this.saveAndCloseToolStripMenuItem.Click += new System.EventHandler(this.saveAndCloseToolStripMenuItem_Click);
            // 
            // listBox1
            // 
            this.listBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listBox1.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.listBox1.FormattingEnabled = true;
            this.listBox1.IntegralHeight = false;
            this.listBox1.ItemHeight = 14;
            this.listBox1.Location = new System.Drawing.Point(0, 24);
            this.listBox1.Name = "listBox1";
            this.listBox1.Size = new System.Drawing.Size(581, 351);
            this.listBox1.TabIndex = 1;
            // 
            // TalkTableEditor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(581, 375);
            this.Controls.Add(this.listBox1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "TalkTableEditor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Talktable Editor";
            this.Load += new System.EventHandler(this.TalkTableEditor_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ListBox listBox1;
        private System.Windows.Forms.ToolStripMenuItem tableToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportToTXTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importFromTXTToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAndCloseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addModJobToolStripMenuItem;
    }
}