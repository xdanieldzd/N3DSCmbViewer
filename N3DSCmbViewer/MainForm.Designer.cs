namespace N3DSCmbViewer
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.ofdOpenFile = new System.Windows.Forms.OpenFileDialog();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveScreenshotToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.archiveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.extractAllToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpModelInfosToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.dumpSceneActorsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem(); 
            this.dumpCOLLADAFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableHUDToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.resetCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.disableAllShadersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sfdScreenshot = new System.Windows.Forms.SaveFileDialog();
            this.fbdArchiveExtractAll = new System.Windows.Forms.FolderBrowserDialog();
            this.cmsTreeFile = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.extractToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sfdExtractFile = new System.Windows.Forms.SaveFileDialog();
            this.sfdLogFile = new System.Windows.Forms.SaveFileDialog();
            this.glControl1 = new Aglex.GLControl();
            this.sfdColladaDae = new System.Windows.Forms.SaveFileDialog();
            this.treeViewEx1 = new N3DSCmbViewer.TreeViewEx();
            this.toolStripMenuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.enableSkeletalRenderingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.cmsTreeFile.SuspendLayout();
            this.SuspendLayout();
            // 
            // ofdOpenFile
            // 
            this.ofdOpenFile.Filter = "All Supported Files (*.cmb;*.zar;*.zsi;*.gar;*.lzs)|*.cmb;*.zar;*.zsi;*.gar;*.lzs" +
    "|All Files (*.*)|*.*";
            this.ofdOpenFile.Title = "Open Cmb File";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.archiveToolStripMenuItem,
            this.dumpToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(890, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openFileToolStripMenuItem,
            this.saveScreenshotToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openFileToolStripMenuItem
            // 
            this.openFileToolStripMenuItem.Name = "openFileToolStripMenuItem";
            this.openFileToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.openFileToolStripMenuItem.Text = "&Open File...";
            this.openFileToolStripMenuItem.Click += new System.EventHandler(this.openFileToolStripMenuItem_Click);
            // 
            // saveScreenshotToolStripMenuItem
            // 
            this.saveScreenshotToolStripMenuItem.Name = "saveScreenshotToolStripMenuItem";
            this.saveScreenshotToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.saveScreenshotToolStripMenuItem.Text = "Save &Screenshot";
            this.saveScreenshotToolStripMenuItem.Click += new System.EventHandler(this.saveScreenshotToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(155, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(158, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // archiveToolStripMenuItem
            // 
            this.archiveToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractAllToolStripMenuItem});
            this.archiveToolStripMenuItem.Enabled = false;
            this.archiveToolStripMenuItem.Name = "archiveToolStripMenuItem";
            this.archiveToolStripMenuItem.Size = new System.Drawing.Size(57, 20);
            this.archiveToolStripMenuItem.Text = "&Archive";
            // 
            // extractAllToolStripMenuItem
            // 
            this.extractAllToolStripMenuItem.Name = "extractAllToolStripMenuItem";
            this.extractAllToolStripMenuItem.Size = new System.Drawing.Size(125, 22);
            this.extractAllToolStripMenuItem.Text = "&Extract All";
            this.extractAllToolStripMenuItem.Click += new System.EventHandler(this.extractAllToolStripMenuItem_Click);
            // 
            // dumpToolStripMenuItem
            // 
            this.dumpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.dumpModelInfosToolStripMenuItem,
            this.dumpSceneActorsToolStripMenuItem,
            this.dumpCOLLADAFileToolStripMenuItem});
            this.dumpToolStripMenuItem.Enabled = false;
            this.dumpToolStripMenuItem.Name = "dumpToolStripMenuItem";
            this.dumpToolStripMenuItem.Size = new System.Drawing.Size(51, 20);
            this.dumpToolStripMenuItem.Text = "&Dump";
            // 
            // dumpModelInfosToolStripMenuItem
            // 
            this.dumpModelInfosToolStripMenuItem.Name = "dumpModelInfosToolStripMenuItem";
            this.dumpModelInfosToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dumpModelInfosToolStripMenuItem.Text = "&Dump Model Infos";
            this.dumpModelInfosToolStripMenuItem.Click += new System.EventHandler(this.dumpModelInfosToolStripMenuItem_Click);
            // 
            // dumpSceneActorsToolStripMenuItem
            // 
            this.dumpSceneActorsToolStripMenuItem.Name = "dumpSceneActorsToolStripMenuItem";
            this.dumpSceneActorsToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dumpSceneActorsToolStripMenuItem.Text = "&Dump Scene Actors";
            this.dumpSceneActorsToolStripMenuItem.Click += new System.EventHandler(this.dumpSceneActorsToolStripMenuItem_Click);
            // 
            // dumpCOLLADAFileToolStripMenuItem
            // 
            this.dumpCOLLADAFileToolStripMenuItem.Name = "dumpCOLLADAFileToolStripMenuItem";
            this.dumpCOLLADAFileToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.dumpCOLLADAFileToolStripMenuItem.Text = "Dump &COLLADA File";
            this.dumpCOLLADAFileToolStripMenuItem.Click += new System.EventHandler(this.dumpCOLLADAFileToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableHUDToolStripMenuItem,
            this.resetCameraToolStripMenuItem,
            this.toolStripMenuItem3,
            this.enableSkeletalRenderingToolStripMenuItem,
            this.toolStripMenuItem2,
            this.disableAllShadersToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // enableHUDToolStripMenuItem
            // 
            this.enableHUDToolStripMenuItem.CheckOnClick = true;
            this.enableHUDToolStripMenuItem.Name = "enableHUDToolStripMenuItem";
            this.enableHUDToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.enableHUDToolStripMenuItem.Text = "&Enable HUD";
            this.enableHUDToolStripMenuItem.Click += new System.EventHandler(this.enableHUDToolStripMenuItem_Click);
            // 
            // resetCameraToolStripMenuItem
            // 
            this.resetCameraToolStripMenuItem.Name = "resetCameraToolStripMenuItem";
            this.resetCameraToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.resetCameraToolStripMenuItem.Text = "&Reset Camera";
            this.resetCameraToolStripMenuItem.Click += new System.EventHandler(this.resetCameraToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(205, 6);
            // 
            // disableAllShadersToolStripMenuItem
            // 
            this.disableAllShadersToolStripMenuItem.CheckOnClick = true;
            this.disableAllShadersToolStripMenuItem.Name = "disableAllShadersToolStripMenuItem";
            this.disableAllShadersToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.disableAllShadersToolStripMenuItem.Text = "Disable &All Shaders";
            this.disableAllShadersToolStripMenuItem.Click += new System.EventHandler(this.disableAllShadersToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(106, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // sfdScreenshot
            // 
            this.sfdScreenshot.Filter = "PNG Files (*.png)|*.png";
            // 
            // fbdArchiveExtractAll
            // 
            this.fbdArchiveExtractAll.Description = "Select path to save extracted file to.";
            // 
            // cmsTreeFile
            // 
            this.cmsTreeFile.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.extractToolStripMenuItem});
            this.cmsTreeFile.Name = "cmsTreeFile";
            this.cmsTreeFile.Size = new System.Drawing.Size(110, 26);
            // 
            // extractToolStripMenuItem
            // 
            this.extractToolStripMenuItem.Name = "extractToolStripMenuItem";
            this.extractToolStripMenuItem.Size = new System.Drawing.Size(109, 22);
            this.extractToolStripMenuItem.Text = "&Extract";
            this.extractToolStripMenuItem.Click += new System.EventHandler(this.extractToolStripMenuItem_Click);
            // 
            // sfdExtractFile
            // 
            this.sfdExtractFile.Filter = "All Files (*.*)|*.*";
            // 
            // sfdLogFile
            // 
            this.sfdLogFile.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            // 
            // glControl1
            // 
            this.glControl1.BackColor = System.Drawing.Color.SkyBlue;
            this.glControl1.Camera = true;
            this.glControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl1.Lighting = true;
            this.glControl1.Location = new System.Drawing.Point(250, 24);
            this.glControl1.Name = "glControl1";
            this.glControl1.ProjectionType = Aglex.ProjectionTypes.Perspective;
            this.glControl1.Size = new System.Drawing.Size(640, 480);
            this.glControl1.TabIndex = 0;
            this.glControl1.VSync = true;
            this.glControl1.Render += new System.EventHandler<System.EventArgs>(this.glControl1_Render);
            this.glControl1.Load += new System.EventHandler(this.glControl1_Load);
            this.glControl1.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl1_KeyDown);
            // 
            // sfdColladaDae
            // 
            this.sfdColladaDae.Filter = "COLLADA File (*.dae)|*.dae|All Files (*.*)|*.*";
            // 
            // treeViewEx1
            // 
            this.treeViewEx1.Dock = System.Windows.Forms.DockStyle.Left;
            this.treeViewEx1.HideSelection = false;
            this.treeViewEx1.Location = new System.Drawing.Point(0, 24);
            this.treeViewEx1.Name = "treeViewEx1";
            this.treeViewEx1.Size = new System.Drawing.Size(350, 480);
            this.treeViewEx1.TabIndex = 2;
            this.treeViewEx1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeViewEx1_AfterSelect);
            this.treeViewEx1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewEx1_NodeMouseClick);
            // 
            // toolStripMenuItem3
            // 
            this.toolStripMenuItem3.Name = "toolStripMenuItem3";
            this.toolStripMenuItem3.Size = new System.Drawing.Size(205, 6);
            // 
            // enableSkeletalRenderingToolStripMenuItem
            // 
            this.enableSkeletalRenderingToolStripMenuItem.CheckOnClick = true;
            this.enableSkeletalRenderingToolStripMenuItem.Name = "enableSkeletalRenderingToolStripMenuItem";
            this.enableSkeletalRenderingToolStripMenuItem.Size = new System.Drawing.Size(208, 22);
            this.enableSkeletalRenderingToolStripMenuItem.Text = "Enable &Skeletal Rendering";
            this.enableSkeletalRenderingToolStripMenuItem.Click += new System.EventHandler(this.enableSkeletalRenderingToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(890, 504);
            this.Controls.Add(this.glControl1);
            this.Controls.Add(this.treeViewEx1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.cmsTreeFile.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Aglex.GLControl glControl1;
        private System.Windows.Forms.OpenFileDialog ofdOpenFile;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem enableHUDToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog sfdScreenshot;
        private System.Windows.Forms.ToolStripMenuItem saveScreenshotToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private TreeViewEx treeViewEx1;
        private System.Windows.Forms.ToolStripMenuItem archiveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem extractAllToolStripMenuItem;
        private System.Windows.Forms.FolderBrowserDialog fbdArchiveExtractAll;
        private System.Windows.Forms.ContextMenuStrip cmsTreeFile;
        private System.Windows.Forms.ToolStripMenuItem extractToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog sfdExtractFile;
        private System.Windows.Forms.ToolStripMenuItem resetCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem dumpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpModelInfosToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpSceneActorsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog sfdLogFile;
        private System.Windows.Forms.ToolStripMenuItem disableAllShadersToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem dumpCOLLADAFileToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog sfdColladaDae;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem3;
        private System.Windows.Forms.ToolStripMenuItem enableSkeletalRenderingToolStripMenuItem;
    }
}

