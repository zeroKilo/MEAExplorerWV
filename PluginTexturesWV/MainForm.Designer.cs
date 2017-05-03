namespace PluginTexturesWV
{
    partial class MainForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportResRawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportChunkRawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exportAsDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.tv1 = new System.Windows.Forms.TreeView();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripTextBox1 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton1 = new System.Windows.Forms.ToolStripButton();
            this.tv2 = new System.Windows.Forms.TreeView();
            this.toolStrip2 = new System.Windows.Forms.ToolStrip();
            this.toolStripTextBox2 = new System.Windows.Forms.ToolStripTextBox();
            this.toolStripButton2 = new System.Windows.Forms.ToolStripButton();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer3 = new System.Windows.Forms.SplitContainer();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.rtb1 = new System.Windows.Forms.RichTextBox();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.pic1 = new System.Windows.Forms.PictureBox();
            this.importFromDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.hb1 = new Be.Windows.Forms.HexBox();
            this.hb2 = new Be.Windows.Forms.HexBox();
            this.makeModJobFromDDSToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.toolStrip2.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).BeginInit();
            this.splitContainer3.Panel1.SuspendLayout();
            this.splitContainer3.Panel2.SuspendLayout();
            this.splitContainer3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.tabPage3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).BeginInit();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(683, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exportResRawToolStripMenuItem,
            this.exportChunkRawToolStripMenuItem,
            this.exportAsDDSToolStripMenuItem,
            this.importFromDDSToolStripMenuItem,
            this.makeModJobFromDDSToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // exportResRawToolStripMenuItem
            // 
            this.exportResRawToolStripMenuItem.Enabled = false;
            this.exportResRawToolStripMenuItem.Name = "exportResRawToolStripMenuItem";
            this.exportResRawToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.exportResRawToolStripMenuItem.Text = "Export Res Raw...";
            this.exportResRawToolStripMenuItem.Click += new System.EventHandler(this.exportResRawToolStripMenuItem_Click);
            // 
            // exportChunkRawToolStripMenuItem
            // 
            this.exportChunkRawToolStripMenuItem.Enabled = false;
            this.exportChunkRawToolStripMenuItem.Name = "exportChunkRawToolStripMenuItem";
            this.exportChunkRawToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.exportChunkRawToolStripMenuItem.Text = "Export Chunk Raw...";
            this.exportChunkRawToolStripMenuItem.Click += new System.EventHandler(this.exportChunkRawToolStripMenuItem_Click);
            // 
            // exportAsDDSToolStripMenuItem
            // 
            this.exportAsDDSToolStripMenuItem.Enabled = false;
            this.exportAsDDSToolStripMenuItem.Name = "exportAsDDSToolStripMenuItem";
            this.exportAsDDSToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.exportAsDDSToolStripMenuItem.Text = "Export as DDS...";
            this.exportAsDDSToolStripMenuItem.Click += new System.EventHandler(this.exportAsDDSToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.splitContainer2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(683, 464);
            this.splitContainer1.SplitterDistance = 347;
            this.splitContainer1.TabIndex = 1;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(0, 0);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.tv1);
            this.splitContainer2.Panel1.Controls.Add(this.toolStrip1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tv2);
            this.splitContainer2.Panel2.Controls.Add(this.toolStrip2);
            this.splitContainer2.Size = new System.Drawing.Size(347, 464);
            this.splitContainer2.SplitterDistance = 217;
            this.splitContainer2.TabIndex = 0;
            // 
            // tv1
            // 
            this.tv1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tv1.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.tv1.HideSelection = false;
            this.tv1.Location = new System.Drawing.Point(0, 25);
            this.tv1.Name = "tv1";
            this.tv1.Size = new System.Drawing.Size(347, 192);
            this.tv1.TabIndex = 1;
            this.tv1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv1_AfterSelect);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox1,
            this.toolStripButton1});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(347, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip2";
            // 
            // toolStripTextBox1
            // 
            this.toolStripTextBox1.Name = "toolStripTextBox1";
            this.toolStripTextBox1.Size = new System.Drawing.Size(150, 25);
            // 
            // toolStripButton1
            // 
            this.toolStripButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton1.Image")));
            this.toolStripButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton1.Name = "toolStripButton1";
            this.toolStripButton1.Size = new System.Drawing.Size(57, 22);
            this.toolStripButton1.Text = "Find Next";
            this.toolStripButton1.Click += new System.EventHandler(this.toolStripButton2_Click);
            // 
            // tv2
            // 
            this.tv2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tv2.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.tv2.HideSelection = false;
            this.tv2.Location = new System.Drawing.Point(0, 25);
            this.tv2.Name = "tv2";
            this.tv2.Size = new System.Drawing.Size(347, 218);
            this.tv2.TabIndex = 1;
            this.tv2.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tv2_AfterSelect);
            // 
            // toolStrip2
            // 
            this.toolStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripTextBox2,
            this.toolStripButton2});
            this.toolStrip2.Location = new System.Drawing.Point(0, 0);
            this.toolStrip2.Name = "toolStrip2";
            this.toolStrip2.Size = new System.Drawing.Size(347, 25);
            this.toolStrip2.TabIndex = 4;
            this.toolStrip2.Text = "toolStrip1";
            // 
            // toolStripTextBox2
            // 
            this.toolStripTextBox2.Name = "toolStripTextBox2";
            this.toolStripTextBox2.Size = new System.Drawing.Size(150, 25);
            // 
            // toolStripButton2
            // 
            this.toolStripButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton2.Image")));
            this.toolStripButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton2.Name = "toolStripButton2";
            this.toolStripButton2.Size = new System.Drawing.Size(57, 22);
            this.toolStripButton2.Text = "Find Next";
            this.toolStripButton2.Click += new System.EventHandler(this.toolStripButton1_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Controls.Add(this.tabPage3);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(332, 464);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer3);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(324, 438);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Raw";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer3
            // 
            this.splitContainer3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer3.Location = new System.Drawing.Point(3, 3);
            this.splitContainer3.Name = "splitContainer3";
            this.splitContainer3.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer3.Panel1
            // 
            this.splitContainer3.Panel1.Controls.Add(this.hb1);
            // 
            // splitContainer3.Panel2
            // 
            this.splitContainer3.Panel2.Controls.Add(this.hb2);
            this.splitContainer3.Size = new System.Drawing.Size(318, 432);
            this.splitContainer3.SplitterDistance = 192;
            this.splitContainer3.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.rtb1);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(324, 438);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Info";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // rtb1
            // 
            this.rtb1.DetectUrls = false;
            this.rtb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtb1.Font = new System.Drawing.Font("Courier New", 8.25F);
            this.rtb1.HideSelection = false;
            this.rtb1.Location = new System.Drawing.Point(3, 3);
            this.rtb1.Name = "rtb1";
            this.rtb1.Size = new System.Drawing.Size(318, 432);
            this.rtb1.TabIndex = 2;
            this.rtb1.Text = "";
            this.rtb1.WordWrap = false;
            // 
            // tabPage3
            // 
            this.tabPage3.AutoScroll = true;
            this.tabPage3.Controls.Add(this.pic1);
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(324, 438);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Preview";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // pic1
            // 
            this.pic1.Location = new System.Drawing.Point(3, 3);
            this.pic1.Name = "pic1";
            this.pic1.Size = new System.Drawing.Size(318, 432);
            this.pic1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.AutoSize;
            this.pic1.TabIndex = 1;
            this.pic1.TabStop = false;
            // 
            // importFromDDSToolStripMenuItem
            // 
            this.importFromDDSToolStripMenuItem.Enabled = false;
            this.importFromDDSToolStripMenuItem.Name = "importFromDDSToolStripMenuItem";
            this.importFromDDSToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.importFromDDSToolStripMenuItem.Text = "Import from DDS...";
            this.importFromDDSToolStripMenuItem.Click += new System.EventHandler(this.importFromDDSToolStripMenuItem_Click);
            // 
            // hb1
            // 
            this.hb1.BoldFont = null;
            this.hb1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hb1.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hb1.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hb1.LineInfoVisible = true;
            this.hb1.Location = new System.Drawing.Point(0, 0);
            this.hb1.Name = "hb1";
            this.hb1.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hb1.Size = new System.Drawing.Size(318, 192);
            this.hb1.StringViewVisible = true;
            this.hb1.TabIndex = 0;
            this.hb1.UseFixedBytesPerLine = true;
            this.hb1.VScrollBarVisible = true;
            // 
            // hb2
            // 
            this.hb2.BoldFont = null;
            this.hb2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.hb2.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.hb2.LineInfoForeColor = System.Drawing.Color.Empty;
            this.hb2.LineInfoVisible = true;
            this.hb2.Location = new System.Drawing.Point(0, 0);
            this.hb2.Name = "hb2";
            this.hb2.ShadowSelectionColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(60)))), ((int)(((byte)(188)))), ((int)(((byte)(255)))));
            this.hb2.Size = new System.Drawing.Size(318, 236);
            this.hb2.StringViewVisible = true;
            this.hb2.TabIndex = 1;
            this.hb2.UseFixedBytesPerLine = true;
            this.hb2.VScrollBarVisible = true;
            // 
            // makeModJobFromDDSToolStripMenuItem
            // 
            this.makeModJobFromDDSToolStripMenuItem.Enabled = false;
            this.makeModJobFromDDSToolStripMenuItem.Name = "makeModJobFromDDSToolStripMenuItem";
            this.makeModJobFromDDSToolStripMenuItem.Size = new System.Drawing.Size(202, 22);
            this.makeModJobFromDDSToolStripMenuItem.Text = "Make Mod Job from DDS...";
            this.makeModJobFromDDSToolStripMenuItem.Click += new System.EventHandler(this.makeModJobFromDDSToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 488);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "MainForm";
            this.Text = "Texture Plugin";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel1.PerformLayout();
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.toolStrip2.ResumeLayout(false);
            this.toolStrip2.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer3.Panel1.ResumeLayout(false);
            this.splitContainer3.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer3)).EndInit();
            this.splitContainer3.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.tabPage3.ResumeLayout(false);
            this.tabPage3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pic1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.TreeView tv1;
        private System.Windows.Forms.TreeView tv2;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer3;
        private System.Windows.Forms.TabPage tabPage2;
        private Be.Windows.Forms.HexBox hb1;
        private Be.Windows.Forms.HexBox hb2;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportResRawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportChunkRawToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exportAsDDSToolStripMenuItem;
        private System.Windows.Forms.RichTextBox rtb1;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.PictureBox pic1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox1;
        private System.Windows.Forms.ToolStripButton toolStripButton1;
        private System.Windows.Forms.ToolStrip toolStrip2;
        private System.Windows.Forms.ToolStripTextBox toolStripTextBox2;
        private System.Windows.Forms.ToolStripButton toolStripButton2;
        private System.Windows.Forms.ToolStripMenuItem importFromDDSToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem makeModJobFromDDSToolStripMenuItem;
    }
}