using System.Security.AccessControl;

namespace ResilientP4
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
			this.components = new System.ComponentModel.Container();
			System.Windows.Forms.ToolStripMenuItem PackageChangelistMenuItem;
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.LogTextBox = new System.Windows.Forms.RichTextBox();
			this.MainTreeView = new System.Windows.Forms.TreeView();
			this.TreeViewMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ReconcileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SyncToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.UnpackageChangelistToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CheckConsistencyQuickMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.CheckConsistencyThoroughMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.DisconnectMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripSeparator = new System.Windows.Forms.ToolStripSeparator();
			this.GetHaveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GetLabelToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TreeViewImageList = new System.Windows.Forms.ImageList(this.components);
			this.ChangelistViewMenuStrip = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.ResilientSubmitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.ToolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
			this.SplitChangelistMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MultiSubmitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.MainSplitContainer = new System.Windows.Forms.SplitContainer();
			this.ChangelistTreeView = new Eternal.EternalUtilities.MultipleSelectionTreeView();
			this.TopSplitContainer = new System.Windows.Forms.SplitContainer();
			this.GenericSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
			this.TopMenuStrip = new System.Windows.Forms.MenuStrip();
			this.FileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.QuitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SettingsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SettingsMenuSubItem = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.HelpMenuSubItem = new System.Windows.Forms.ToolStripMenuItem();
			this.AboutMenuSubItem = new System.Windows.Forms.ToolStripMenuItem();
			this.GenericOpenFileDialog = new System.Windows.Forms.OpenFileDialog();
			this.MainToolStrip = new System.Windows.Forms.ToolStrip();
			this.ConnectToolStripButton = new System.Windows.Forms.ToolStripButton();
			this.UnpackageChangelistButton = new System.Windows.Forms.ToolStripButton();
			this.CleanButton = new System.Windows.Forms.ToolStripButton();
			this.SettingsButton = new System.Windows.Forms.ToolStripButton();
			this.GenericFolderBrowserDialog = new System.Windows.Forms.FolderBrowserDialog();
			PackageChangelistMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.TreeViewMenuStrip.SuspendLayout();
			this.ChangelistViewMenuStrip.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).BeginInit();
			this.MainSplitContainer.Panel1.SuspendLayout();
			this.MainSplitContainer.Panel2.SuspendLayout();
			this.MainSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).BeginInit();
			this.TopSplitContainer.Panel1.SuspendLayout();
			this.TopSplitContainer.Panel2.SuspendLayout();
			this.TopSplitContainer.SuspendLayout();
			this.TopMenuStrip.SuspendLayout();
			this.MainToolStrip.SuspendLayout();
			this.SuspendLayout();
			// 
			// PackageChangelistMenuItem
			// 
			PackageChangelistMenuItem.Name = "PackageChangelistMenuItem";
			PackageChangelistMenuItem.Size = new System.Drawing.Size(177, 22);
			PackageChangelistMenuItem.Text = "Package Changelist";
			PackageChangelistMenuItem.ToolTipText = "Package a changelist into an RP4 file.";
			PackageChangelistMenuItem.Click += new System.EventHandler(this.PackageChangelistClick);
			// 
			// LogTextBox
			// 
			this.LogTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.LogTextBox.Location = new System.Drawing.Point(0, 0);
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.Size = new System.Drawing.Size(1140, 256);
			this.LogTextBox.TabIndex = 0;
			this.LogTextBox.Text = "";
			// 
			// MainTreeView
			// 
			this.MainTreeView.ContextMenuStrip = this.TreeViewMenuStrip;
			this.MainTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainTreeView.HideSelection = false;
			this.MainTreeView.ImageIndex = 0;
			this.MainTreeView.ImageList = this.TreeViewImageList;
			this.MainTreeView.Location = new System.Drawing.Point(0, 0);
			this.MainTreeView.Name = "MainTreeView";
			this.MainTreeView.SelectedImageIndex = 0;
			this.MainTreeView.Size = new System.Drawing.Size(380, 366);
			this.MainTreeView.TabIndex = 1;
			this.MainTreeView.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.OnBeforeSelectTreeNode);
			this.MainTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.DepotTreeViewNodeClick);
			// 
			// TreeViewMenuStrip
			// 
			this.TreeViewMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ReconcileToolStripMenuItem,
            this.SyncToolStripMenuItem,
            this.UnpackageChangelistToolStripMenuItem,
            this.CheckConsistencyQuickMenuItem,
            this.CheckConsistencyThoroughMenuItem,
            this.DisconnectMenuItem,
            this.ToolStripSeparator,
            this.GetHaveToolStripMenuItem,
            this.GetLabelToolStripMenuItem});
			this.TreeViewMenuStrip.Name = "TreeViewMenuStrip";
			this.TreeViewMenuStrip.Size = new System.Drawing.Size(237, 186);
			this.TreeViewMenuStrip.Text = "Operations";
			// 
			// ReconcileToolStripMenuItem
			// 
			this.ReconcileToolStripMenuItem.Name = "ReconcileToolStripMenuItem";
			this.ReconcileToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
			this.ReconcileToolStripMenuItem.Text = "Reconcile with folder";
			this.ReconcileToolStripMenuItem.Click += new System.EventHandler(this.ReconcileFilesClick);
			// 
			// SyncToolStripMenuItem
			// 
			this.SyncToolStripMenuItem.Name = "SyncToolStripMenuItem";
			this.SyncToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
			this.SyncToolStripMenuItem.Text = "Resilient sync to label";
			this.SyncToolStripMenuItem.Click += new System.EventHandler(this.ResilientSyncClick);
			// 
			// UnpackageChangelistToolStripMenuItem
			// 
			this.UnpackageChangelistToolStripMenuItem.Name = "UnpackageChangelistToolStripMenuItem";
			this.UnpackageChangelistToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
			this.UnpackageChangelistToolStripMenuItem.Text = "Unpackage changelist";
			this.UnpackageChangelistToolStripMenuItem.Click += new System.EventHandler(this.UnpackageChangelistClick);
			// 
			// CheckConsistencyQuickMenuItem
			// 
			this.CheckConsistencyQuickMenuItem.Name = "CheckConsistencyQuickMenuItem";
			this.CheckConsistencyQuickMenuItem.Size = new System.Drawing.Size(236, 22);
			this.CheckConsistencyQuickMenuItem.Text = "Check consistency (Quick)";
			this.CheckConsistencyQuickMenuItem.ToolTipText = "Verifies the Perforce server knows of the existence and size of local files.";
			this.CheckConsistencyQuickMenuItem.Click += new System.EventHandler(this.CheckConsistencyMenuItemClick);
			// 
			// CheckConsistencyThoroughMenuItem
			// 
			this.CheckConsistencyThoroughMenuItem.Name = "CheckConsistencyThoroughMenuItem";
			this.CheckConsistencyThoroughMenuItem.Size = new System.Drawing.Size(236, 22);
			this.CheckConsistencyThoroughMenuItem.Text = "Check consistency (Thorough)";
			this.CheckConsistencyThoroughMenuItem.ToolTipText = "Verifies the Perforce server knows of the existence, size, and MD5 checksum of lo" +
    "cal files.";
			this.CheckConsistencyThoroughMenuItem.Click += new System.EventHandler(this.CheckConsistencyThoroughMenuItemClick);
			// 
			// DisconnectMenuItem
			// 
			this.DisconnectMenuItem.Name = "DisconnectMenuItem";
			this.DisconnectMenuItem.Size = new System.Drawing.Size(236, 22);
			this.DisconnectMenuItem.Text = "Disconnect";
			this.DisconnectMenuItem.ToolTipText = "Disconnects from the selected Perforce server.";
			this.DisconnectMenuItem.Click += new System.EventHandler(this.DisconnectMenuItemClick);
			// 
			// ToolStripSeparator
			// 
			this.ToolStripSeparator.Name = "ToolStripSeparator";
			this.ToolStripSeparator.Size = new System.Drawing.Size(233, 6);
			// 
			// GetHaveToolStripMenuItem
			// 
			this.GetHaveToolStripMenuItem.Name = "GetHaveToolStripMenuItem";
			this.GetHaveToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
			this.GetHaveToolStripMenuItem.Text = "Get #have file data";
			this.GetHaveToolStripMenuItem.Click += new System.EventHandler(this.GetHaveFileDataClick);
			// 
			// GetLabelToolStripMenuItem
			// 
			this.GetLabelToolStripMenuItem.Name = "GetLabelToolStripMenuItem";
			this.GetLabelToolStripMenuItem.Size = new System.Drawing.Size(236, 22);
			this.GetLabelToolStripMenuItem.Text = "Get label file data";
			this.GetLabelToolStripMenuItem.Click += new System.EventHandler(this.GetLabelFileDataClick);
			// 
			// TreeViewImageList
			// 
			this.TreeViewImageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("TreeViewImageList.ImageStream")));
			this.TreeViewImageList.TransparentColor = System.Drawing.Color.Transparent;
			this.TreeViewImageList.Images.SetKeyName(0, "unhappy.ico");
			this.TreeViewImageList.Images.SetKeyName(1, "triangle-red.ico");
			this.TreeViewImageList.Images.SetKeyName(2, "branch-green.ico");
			this.TreeViewImageList.Images.SetKeyName(3, "user-yellow.ico");
			this.TreeViewImageList.Images.SetKeyName(4, "document-grey.ico");
			this.TreeViewImageList.Images.SetKeyName(5, "triangle-grey.ico");
			this.TreeViewImageList.Images.SetKeyName(6, "connect-black.ico");
			this.TreeViewImageList.Images.SetKeyName(7, "blank.ico");
			this.TreeViewImageList.Images.SetKeyName(8, "branch-grey.ico");
			this.TreeViewImageList.Images.SetKeyName(9, "server-blue.ico");
			this.TreeViewImageList.Images.SetKeyName(10, "tick-green.ico");
			// 
			// ChangelistViewMenuStrip
			// 
			this.ChangelistViewMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            PackageChangelistMenuItem,
            this.ResilientSubmitMenuItem,
            this.ToolStripSeparator1,
            this.SplitChangelistMenuItem,
            this.MultiSubmitMenuItem});
			this.ChangelistViewMenuStrip.Name = "ChangelistViewMenuStrip";
			this.ChangelistViewMenuStrip.Size = new System.Drawing.Size(178, 98);
			// 
			// ResilientSubmitMenuItem
			// 
			this.ResilientSubmitMenuItem.Name = "ResilientSubmitMenuItem";
			this.ResilientSubmitMenuItem.Size = new System.Drawing.Size(177, 22);
			this.ResilientSubmitMenuItem.Text = "Resilient Submit";
			this.ResilientSubmitMenuItem.ToolTipText = "Perform a resilient submit operation.";
			this.ResilientSubmitMenuItem.Click += new System.EventHandler(this.ResilientSubmitClick);
			// 
			// ToolStripSeparator1
			// 
			this.ToolStripSeparator1.Name = "ToolStripSeparator1";
			this.ToolStripSeparator1.Size = new System.Drawing.Size(174, 6);
			// 
			// SplitChangelistMenuItem
			// 
			this.SplitChangelistMenuItem.Name = "SplitChangelistMenuItem";
			this.SplitChangelistMenuItem.Size = new System.Drawing.Size(177, 22);
			this.SplitChangelistMenuItem.Text = "Split Changelist";
			this.SplitChangelistMenuItem.ToolTipText = "Split a large changelist into smaller chunks according to settings.";
			this.SplitChangelistMenuItem.Click += new System.EventHandler(this.SplitChangelistClick);
			// 
			// MultiSubmitMenuItem
			// 
			this.MultiSubmitMenuItem.Name = "MultiSubmitMenuItem";
			this.MultiSubmitMenuItem.Size = new System.Drawing.Size(177, 22);
			this.MultiSubmitMenuItem.Text = "Multi Submit";
			this.MultiSubmitMenuItem.ToolTipText = "Submit multiple selected changelists.";
			this.MultiSubmitMenuItem.Click += new System.EventHandler(this.MultiSubmitClick);
			// 
			// MainSplitContainer
			// 
			this.MainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainSplitContainer.Location = new System.Drawing.Point(0, 0);
			this.MainSplitContainer.MinimumSize = new System.Drawing.Size(256, 256);
			this.MainSplitContainer.Name = "MainSplitContainer";
			// 
			// MainSplitContainer.Panel1
			// 
			this.MainSplitContainer.Panel1.Controls.Add(this.MainTreeView);
			this.MainSplitContainer.Panel1MinSize = 128;
			// 
			// MainSplitContainer.Panel2
			// 
			this.MainSplitContainer.Panel2.Controls.Add(this.ChangelistTreeView);
			this.MainSplitContainer.Panel2MinSize = 128;
			this.MainSplitContainer.Size = new System.Drawing.Size(1140, 366);
			this.MainSplitContainer.SplitterDistance = 380;
			this.MainSplitContainer.TabIndex = 3;
			// 
			// ChangelistTreeView
			// 
			this.ChangelistTreeView.CausesValidation = false;
			this.ChangelistTreeView.ContextMenuStrip = this.ChangelistViewMenuStrip;
			this.ChangelistTreeView.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ChangelistTreeView.ImageIndex = 0;
			this.ChangelistTreeView.ImageList = this.TreeViewImageList;
			this.ChangelistTreeView.Location = new System.Drawing.Point(0, 0);
			this.ChangelistTreeView.Name = "ChangelistTreeView";
			this.ChangelistTreeView.SelectedImageIndex = 0;
			this.ChangelistTreeView.Size = new System.Drawing.Size(756, 366);
			this.ChangelistTreeView.TabIndex = 2;
			this.ChangelistTreeView.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.ChangelistTreeViewNodeClick);
			// 
			// TopSplitContainer
			// 
			this.TopSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TopSplitContainer.Location = new System.Drawing.Point(0, 49);
			this.TopSplitContainer.Name = "TopSplitContainer";
			this.TopSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// TopSplitContainer.Panel1
			// 
			this.TopSplitContainer.Panel1.Controls.Add(this.MainSplitContainer);
			this.TopSplitContainer.Panel1MinSize = 256;
			// 
			// TopSplitContainer.Panel2
			// 
			this.TopSplitContainer.Panel2.Controls.Add(this.LogTextBox);
			this.TopSplitContainer.Panel2MinSize = 256;
			this.TopSplitContainer.Size = new System.Drawing.Size(1140, 626);
			this.TopSplitContainer.SplitterDistance = 366;
			this.TopSplitContainer.TabIndex = 4;
			// 
			// GenericSaveFileDialog
			// 
			this.GenericSaveFileDialog.DefaultExt = "rp4";
			this.GenericSaveFileDialog.RestoreDirectory = true;
			this.GenericSaveFileDialog.SupportMultiDottedExtensions = true;
			this.GenericSaveFileDialog.Title = "Save Packaged Changelist";
			// 
			// TopMenuStrip
			// 
			this.TopMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenuItem,
            this.SettingsMenuItem,
            this.HelpMenuItem});
			this.TopMenuStrip.Location = new System.Drawing.Point(0, 0);
			this.TopMenuStrip.Name = "TopMenuStrip";
			this.TopMenuStrip.Size = new System.Drawing.Size(1140, 24);
			this.TopMenuStrip.TabIndex = 5;
			this.TopMenuStrip.Text = "File";
			// 
			// FileMenuItem
			// 
			this.FileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.QuitMenuItem});
			this.FileMenuItem.Name = "FileMenuItem";
			this.FileMenuItem.Size = new System.Drawing.Size(37, 20);
			this.FileMenuItem.Text = "File";
			// 
			// QuitMenuItem
			// 
			this.QuitMenuItem.Name = "QuitMenuItem";
			this.QuitMenuItem.Size = new System.Drawing.Size(97, 22);
			this.QuitMenuItem.Text = "Quit";
			this.QuitMenuItem.Click += new System.EventHandler(this.QuitMenuItemClick);
			// 
			// SettingsMenuItem
			// 
			this.SettingsMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingsMenuSubItem});
			this.SettingsMenuItem.Name = "SettingsMenuItem";
			this.SettingsMenuItem.Size = new System.Drawing.Size(61, 20);
			this.SettingsMenuItem.Text = "Settings";
			// 
			// SettingsMenuSubItem
			// 
			this.SettingsMenuSubItem.Name = "SettingsMenuSubItem";
			this.SettingsMenuSubItem.Size = new System.Drawing.Size(116, 22);
			this.SettingsMenuSubItem.Text = "Settings";
			this.SettingsMenuSubItem.Click += new System.EventHandler(this.SettingsMenuItemClick);
			// 
			// HelpMenuItem
			// 
			this.HelpMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.HelpMenuSubItem,
            this.AboutMenuSubItem});
			this.HelpMenuItem.Name = "HelpMenuItem";
			this.HelpMenuItem.Size = new System.Drawing.Size(44, 20);
			this.HelpMenuItem.Text = "Help";
			// 
			// HelpMenuSubItem
			// 
			this.HelpMenuSubItem.Name = "HelpMenuSubItem";
			this.HelpMenuSubItem.Size = new System.Drawing.Size(107, 22);
			this.HelpMenuSubItem.Text = "Help";
			// 
			// AboutMenuSubItem
			// 
			this.AboutMenuSubItem.Name = "AboutMenuSubItem";
			this.AboutMenuSubItem.Size = new System.Drawing.Size(107, 22);
			this.AboutMenuSubItem.Text = "About";
			// 
			// GenericOpenFileDialog
			// 
			this.GenericOpenFileDialog.DefaultExt = "rp4";
			this.GenericOpenFileDialog.Filter = "Resilient P4 Files (*.rp4)|*.rp4|All Files (*.*)|*.*";
			this.GenericOpenFileDialog.RestoreDirectory = true;
			this.GenericOpenFileDialog.SupportMultiDottedExtensions = true;
			// 
			// MainToolStrip
			// 
			this.MainToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ConnectToolStripButton,
            this.UnpackageChangelistButton,
            this.CleanButton,
            this.SettingsButton});
			this.MainToolStrip.Location = new System.Drawing.Point(0, 24);
			this.MainToolStrip.Name = "MainToolStrip";
			this.MainToolStrip.Size = new System.Drawing.Size(1140, 25);
			this.MainToolStrip.TabIndex = 6;
			this.MainToolStrip.Text = "toolStrip1";
			// 
			// ConnectToolStripButton
			// 
			this.ConnectToolStripButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.ConnectToolStripButton.Image = global::ResilientP4.Properties.Resources.connect_black;
			this.ConnectToolStripButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.ConnectToolStripButton.Name = "ConnectToolStripButton";
			this.ConnectToolStripButton.Size = new System.Drawing.Size(23, 22);
			this.ConnectToolStripButton.Text = "Connect to Perforce server";
			this.ConnectToolStripButton.Click += new System.EventHandler(this.ConnectToServerClick);
			// 
			// UnpackageChangelistButton
			// 
			this.UnpackageChangelistButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.UnpackageChangelistButton.Image = global::ResilientP4.Properties.Resources.download_box;
			this.UnpackageChangelistButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.UnpackageChangelistButton.Name = "UnpackageChangelistButton";
			this.UnpackageChangelistButton.Size = new System.Drawing.Size(23, 22);
			this.UnpackageChangelistButton.ToolTipText = "Unpackage a changelist package";
			this.UnpackageChangelistButton.Click += new System.EventHandler(this.UnpackageChangelistClick);
			// 
			// CleanButton
			// 
			this.CleanButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.CleanButton.Image = global::ResilientP4.Properties.Resources.broom_brown;
			this.CleanButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.CleanButton.Name = "CleanButton";
			this.CleanButton.Size = new System.Drawing.Size(23, 22);
			this.CleanButton.Text = "Delete empty changelists.";
			this.CleanButton.ToolTipText = "Delete empty changelists.";
			this.CleanButton.Click += new System.EventHandler(this.CleanButtonClick);
			// 
			// SettingsButton
			// 
			this.SettingsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			this.SettingsButton.Image = global::ResilientP4.Properties.Resources.cog_alt;
			this.SettingsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
			this.SettingsButton.Name = "SettingsButton";
			this.SettingsButton.Size = new System.Drawing.Size(23, 22);
			this.SettingsButton.ToolTipText = "Update user configurable settings.";
			this.SettingsButton.Click += new System.EventHandler(this.SettingsMenuItemClick);
			// 
			// GenericFolderBrowserDialog
			// 
			this.GenericFolderBrowserDialog.ShowNewFolderButton = false;
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(1140, 675);
			this.Controls.Add(this.TopSplitContainer);
			this.Controls.Add(this.MainToolStrip);
			this.Controls.Add(this.TopMenuStrip);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MinimumSize = new System.Drawing.Size(638, 474);
			this.Name = "MainForm";
			this.Text = "ResilientP4";
			this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ResilientP4Closed);
			this.TreeViewMenuStrip.ResumeLayout(false);
			this.ChangelistViewMenuStrip.ResumeLayout(false);
			this.MainSplitContainer.Panel1.ResumeLayout(false);
			this.MainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MainSplitContainer)).EndInit();
			this.MainSplitContainer.ResumeLayout(false);
			this.TopSplitContainer.Panel1.ResumeLayout(false);
			this.TopSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TopSplitContainer)).EndInit();
			this.TopSplitContainer.ResumeLayout(false);
			this.TopMenuStrip.ResumeLayout(false);
			this.TopMenuStrip.PerformLayout();
			this.MainToolStrip.ResumeLayout(false);
			this.MainToolStrip.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.RichTextBox LogTextBox;
		private System.Windows.Forms.TreeView MainTreeView;
		private System.Windows.Forms.ContextMenuStrip TreeViewMenuStrip;
		private System.Windows.Forms.SplitContainer MainSplitContainer;
		private System.Windows.Forms.SplitContainer TopSplitContainer;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator;
		private System.Windows.Forms.ContextMenuStrip ChangelistViewMenuStrip;
		private System.Windows.Forms.SaveFileDialog GenericSaveFileDialog;
		private System.Windows.Forms.MenuStrip TopMenuStrip;
		private System.Windows.Forms.ToolStripMenuItem FileMenuItem;
		private System.Windows.Forms.ToolStripMenuItem QuitMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SettingsMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SettingsMenuSubItem;
		private System.Windows.Forms.ToolStripMenuItem HelpMenuItem;
		private System.Windows.Forms.ToolStripMenuItem HelpMenuSubItem;
		private System.Windows.Forms.ToolStripMenuItem AboutMenuSubItem;
		private System.Windows.Forms.OpenFileDialog GenericOpenFileDialog;
		private System.Windows.Forms.ToolStrip MainToolStrip;
		private System.Windows.Forms.ToolStripButton UnpackageChangelistButton;
		private System.Windows.Forms.ToolStripButton SettingsButton;
        private System.Windows.Forms.ImageList TreeViewImageList;
		private System.Windows.Forms.ToolStripButton ConnectToolStripButton;
		private System.Windows.Forms.ToolStripMenuItem SplitChangelistMenuItem;
		private System.Windows.Forms.ToolStripButton CleanButton;
		private System.Windows.Forms.ToolStripMenuItem ResilientSubmitMenuItem;
		private System.Windows.Forms.ToolStripSeparator ToolStripSeparator1;
		private Eternal.EternalUtilities.MultipleSelectionTreeView ChangelistTreeView;
		private System.Windows.Forms.ToolStripMenuItem MultiSubmitMenuItem;
		private System.Windows.Forms.ToolStripMenuItem GetLabelToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CheckConsistencyQuickMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SyncToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem GetHaveToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem UnpackageChangelistToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem DisconnectMenuItem;
		private System.Windows.Forms.ToolStripMenuItem CheckConsistencyThoroughMenuItem;
		private System.Windows.Forms.ToolStripMenuItem ReconcileToolStripMenuItem;
		private System.Windows.Forms.FolderBrowserDialog GenericFolderBrowserDialog;
	}
}

