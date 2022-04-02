namespace Transfluent
{
	partial class Transfluent
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose( bool disposing )
		{
			if( disposing && ( components != null ) )
			{
				components.Dispose();
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Transfluent));
			this.RichTextBoxLog = new System.Windows.Forms.RichTextBox();
			this.MainMenu = new System.Windows.Forms.MenuStrip();
			this.FileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.LoadManifestToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QuitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SettingsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.SpawnUE4CommandletToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.UploadUntranslatedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DownloadTranslatedToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.RemoveTranslationsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GenericOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.MainToolStrip = new System.Windows.Forms.ToolStrip();
			this.UploadToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.DownloadToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.MainMenu.SuspendLayout();
			this.MainToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// RichTextBoxLog
			// 
			this.RichTextBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RichTextBoxLog.Location = new System.Drawing.Point(0, 59);
			this.RichTextBoxLog.Name = "RichTextBoxLog";
			this.RichTextBoxLog.ReadOnly = true;
			this.RichTextBoxLog.Size = new System.Drawing.Size(984, 602);
			this.RichTextBoxLog.TabIndex = 0;
			this.RichTextBoxLog.Text = "";
			// 
			// MainMenu
			// 
			this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileToolStripMenuItem,
            this.ToolsToolStripMenuItem});
			this.MainMenu.Location = new System.Drawing.Point(0, 0);
			this.MainMenu.Name = "MainMenu";
			this.MainMenu.Size = new System.Drawing.Size(984, 24);
			this.MainMenu.TabIndex = 1;
			this.MainMenu.Text = "menuStrip1";
			// 
			// FileToolStripMenuItem
			// 
			this.FileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.LoadManifestToolStripMenuItem,
            this.QuitToolStripMenuItem});
			this.FileToolStripMenuItem.Name = "FileToolStripMenuItem";
			this.FileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
			this.FileToolStripMenuItem.Text = "File";
			// 
			// LoadManifestToolStripMenuItem
			// 
			this.LoadManifestToolStripMenuItem.Name = "LoadManifestToolStripMenuItem";
			this.LoadManifestToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
			this.LoadManifestToolStripMenuItem.Text = "Load Manifest";
			this.LoadManifestToolStripMenuItem.Click += new System.EventHandler(this.LoadManifestClick);
			// 
			// QuitToolStripMenuItem
			// 
			this.QuitToolStripMenuItem.Name = "QuitToolStripMenuItem";
			this.QuitToolStripMenuItem.Size = new System.Drawing.Size(149, 22);
			this.QuitToolStripMenuItem.Text = "Quit";
			this.QuitToolStripMenuItem.Click += new System.EventHandler(this.QuitClick);
			// 
			// ToolsToolStripMenuItem
			// 
			this.ToolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingsToolStripMenuItem,
            this.ToolStripSeparator1,
            this.SpawnUE4CommandletToolStripMenuItem,
            this.UploadUntranslatedToolStripMenuItem,
            this.DownloadTranslatedToolStripMenuItem,
            this.RemoveTranslationsToolStripMenuItem});
			this.ToolsToolStripMenuItem.Name = "ToolsToolStripMenuItem";
			this.ToolsToolStripMenuItem.Size = new System.Drawing.Size(48, 20);
			this.ToolsToolStripMenuItem.Text = "Tools";
			// 
			// SettingsToolStripMenuItem
			// 
			this.SettingsToolStripMenuItem.Name = "SettingsToolStripMenuItem";
			this.SettingsToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			this.SettingsToolStripMenuItem.Text = "Advanced Settings";
			this.SettingsToolStripMenuItem.Click += new System.EventHandler(this.SettingsClick);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			this.ToolStripSeparator1.Size = new System.Drawing.Size(231, 6);
			// 
			// SpawnUE4CommandletToolStripMenuItem
			// 
			this.SpawnUE4CommandletToolStripMenuItem.Name = "SpawnUE4CommandletToolStripMenuItem";
			this.SpawnUE4CommandletToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			this.SpawnUE4CommandletToolStripMenuItem.Text = "Run Localization Commandlet";
			this.SpawnUE4CommandletToolStripMenuItem.Click += new System.EventHandler(this.SpawnUE4Click);
			// 
			// UploadUntranslatedToolStripMenuItem
			// 
			this.UploadUntranslatedToolStripMenuItem.Name = "UploadUntranslatedToolStripMenuItem";
			this.UploadUntranslatedToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			this.UploadUntranslatedToolStripMenuItem.Text = "Upload Untranslated";
			// 
			// DownloadTranslatedToolStripMenuItem
			// 
			this.DownloadTranslatedToolStripMenuItem.Name = "DownloadTranslatedToolStripMenuItem";
			this.DownloadTranslatedToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			this.DownloadTranslatedToolStripMenuItem.Text = "Download Translated";
			// 
			// RemoveTranslationsToolStripMenuItem
			// 
			this.RemoveTranslationsToolStripMenuItem.Name = "RemoveTranslationsToolStripMenuItem";
			this.RemoveTranslationsToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
			this.RemoveTranslationsToolStripMenuItem.Text = "Remove Translations";
			// 
			// GenericOpenFileDialog
			// 
			this.GenericOpenFileDialog.FileName = "*.manifest";
			// 
			// MainToolStrip
			// 
			this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.UploadToolStripButton,
            this.DownloadToolStripButton});
			this.MainToolStrip.Location = new System.Drawing.Point(0, 24);
			this.MainToolStrip.Name = "MainToolStrip";
			this.MainToolStrip.Size = new System.Drawing.Size(984, 35);
			this.MainToolStrip.TabIndex = 2;
			this.MainToolStrip.Text = "toolStrip1";
			// 
			// UploadToolStripButton
			// 
			this.UploadToolStripButton.AutoSize = false;
			this.UploadToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.UploadToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("UploadToolStripButton.Image")));
			this.UploadToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.UploadToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.UploadToolStripButton.Name = "UploadToolStripButton";
			this.UploadToolStripButton.RightToLeftAutoMirrorImage = true;
			this.UploadToolStripButton.Size = new System.Drawing.Size(200, 32);
			this.UploadToolStripButton.ToolTipText = "Upload all untranslated strings to Transfluent";
			this.UploadToolStripButton.Click += new System.EventHandler(this.UploadAllUntranslatedClick);
			// 
			// DownloadToolStripButton
			// 
			this.DownloadToolStripButton.AutoSize = false;
			this.DownloadToolStripButton.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
			this.DownloadToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.DownloadToolStripButton.Image = ((System.Drawing.Image)(resources.GetObject("DownloadToolStripButton.Image")));
			this.DownloadToolStripButton.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
			this.DownloadToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.DownloadToolStripButton.Name = "DownloadToolStripButton";
			this.DownloadToolStripButton.Size = new System.Drawing.Size(200, 32);
			this.DownloadToolStripButton.ToolTipText = "Download all translated text from Transfluent and write to the archive.";
			this.DownloadToolStripButton.Click += new System.EventHandler(this.DownloadAllTranslatedClick);
			// 
			// Transfluent
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(984, 661);
			this.Controls.Add(this.RichTextBoxLog);
			this.Controls.Add(this.MainToolStrip);
			this.Controls.Add(this.MainMenu);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MainMenuStrip = this.MainMenu;
			this.Name = "Transfluent";
			this.Text = "Transfluent Log";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FormClosedClick);
			this.MainMenu.ResumeLayout(false);
			this.MainMenu.PerformLayout();
			this.MainToolStrip.ResumeLayout(false);
			this.MainToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox RichTextBoxLog;
		private System.Windows.Forms.MenuStrip MainMenu;
		private System.Windows.Forms.ToolStripMenuItem FileToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem QuitToolStripMenuItem;
		private System.Windows.Forms.OpenFileDialog GenericOpenFileDialog;
		private System.Windows.Forms.ToolStripMenuItem ToolsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SettingsToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem LoadManifestToolStripMenuItem;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
		private System.Windows.Forms.ToolStripMenuItem RemoveTranslationsToolStripMenuItem;
		private System.Windows.Forms.ToolStrip MainToolStrip;
		private System.Windows.Forms.ToolStripButton UploadToolStripButton;
		private System.Windows.Forms.ToolStripButton DownloadToolStripButton;
		private System.Windows.Forms.ToolStripMenuItem SpawnUE4CommandletToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem UploadUntranslatedToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DownloadTranslatedToolStripMenuItem;
	}
}

