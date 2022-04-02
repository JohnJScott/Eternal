namespace ResilientP4
{
	partial class ConnectionDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionDialog));
			this.ServerAddressLabel = new System.Windows.Forms.Label();
			this.ServerAddressComboBox = new System.Windows.Forms.ComboBox();
			this.UserNameLabel = new System.Windows.Forms.Label();
			this.UserNameComboBox = new System.Windows.Forms.ComboBox();
			this.WorkspaceLabel = new System.Windows.Forms.Label();
			this.WorkspaceComboBox = new System.Windows.Forms.ComboBox();
			this.SerDescriptionLabel = new System.Windows.Forms.Label();
			this.UserNameDescription = new System.Windows.Forms.Label();
			this.WorkspaceDescription = new System.Windows.Forms.Label();
			this.ConsoleRichTextBox = new System.Windows.Forms.RichTextBox();
			this.RefreshServerAddressButton = new System.Windows.Forms.Button();
			this.RefreshUserNamesButton = new System.Windows.Forms.Button();
			this.RefreshWorkspaceNamesButton = new System.Windows.Forms.Button();
			this.ConnectButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// ServerAddressLabel
			// 
			this.ServerAddressLabel.AutoSize = true;
			this.ServerAddressLabel.Location = new System.Drawing.Point(12, 9);
			this.ServerAddressLabel.Name = "ServerAddressLabel";
			this.ServerAddressLabel.Size = new System.Drawing.Size(79, 13);
			this.ServerAddressLabel.TabIndex = 0;
			this.ServerAddressLabel.Text = "Server Address";
			// 
			// ServerAddressComboBox
			// 
			this.ServerAddressComboBox.FormattingEnabled = true;
			this.ServerAddressComboBox.Location = new System.Drawing.Point(113, 6);
			this.ServerAddressComboBox.Name = "ServerAddressComboBox";
			this.ServerAddressComboBox.Size = new System.Drawing.Size(256, 21);
			this.ServerAddressComboBox.TabIndex = 1;
			this.ServerAddressComboBox.SelectedValueChanged += new System.EventHandler(this.ServerAddressChanged);
			// 
			// UserNameLabel
			// 
			this.UserNameLabel.AutoSize = true;
			this.UserNameLabel.Enabled = false;
			this.UserNameLabel.Location = new System.Drawing.Point(12, 43);
			this.UserNameLabel.Name = "UserNameLabel";
			this.UserNameLabel.Size = new System.Drawing.Size(60, 13);
			this.UserNameLabel.TabIndex = 2;
			this.UserNameLabel.Text = "User Name";
			// 
			// UserNameComboBox
			// 
			this.UserNameComboBox.Enabled = false;
			this.UserNameComboBox.FormattingEnabled = true;
			this.UserNameComboBox.Location = new System.Drawing.Point(113, 40);
			this.UserNameComboBox.Name = "UserNameComboBox";
			this.UserNameComboBox.Size = new System.Drawing.Size(256, 21);
			this.UserNameComboBox.TabIndex = 3;
			this.UserNameComboBox.SelectedIndexChanged += new System.EventHandler(this.UserNameChanged);
			// 
			// WorkspaceLabel
			// 
			this.WorkspaceLabel.AutoSize = true;
			this.WorkspaceLabel.Enabled = false;
			this.WorkspaceLabel.Location = new System.Drawing.Point(12, 77);
			this.WorkspaceLabel.Name = "WorkspaceLabel";
			this.WorkspaceLabel.Size = new System.Drawing.Size(93, 13);
			this.WorkspaceLabel.TabIndex = 4;
			this.WorkspaceLabel.Text = "Workspace Name";
			// 
			// WorkspaceComboBox
			// 
			this.WorkspaceComboBox.Enabled = false;
			this.WorkspaceComboBox.FormattingEnabled = true;
			this.WorkspaceComboBox.Location = new System.Drawing.Point(113, 74);
			this.WorkspaceComboBox.Name = "WorkspaceComboBox";
			this.WorkspaceComboBox.Size = new System.Drawing.Size(256, 21);
			this.WorkspaceComboBox.TabIndex = 5;
			// 
			// SerDescriptionLabel
			// 
			this.SerDescriptionLabel.AutoSize = true;
			this.SerDescriptionLabel.Location = new System.Drawing.Point(455, 9);
			this.SerDescriptionLabel.Name = "SerDescriptionLabel";
			this.SerDescriptionLabel.Size = new System.Drawing.Size(367, 13);
			this.SerDescriptionLabel.TabIndex = 6;
			this.SerDescriptionLabel.Text = "... the internet address and port of the Perforce server e.g. 12.34.56.78:1666";
			// 
			// UserNameDescription
			// 
			this.UserNameDescription.AutoSize = true;
			this.UserNameDescription.Location = new System.Drawing.Point(455, 43);
			this.UserNameDescription.Name = "UserNameDescription";
			this.UserNameDescription.Size = new System.Drawing.Size(342, 13);
			this.UserNameDescription.TabIndex = 7;
			this.UserNameDescription.Text = "... the name of a valid user on the above Perforce server. e.g. First.Last";
			// 
			// WorkspaceDescription
			// 
			this.WorkspaceDescription.AutoSize = true;
			this.WorkspaceDescription.Location = new System.Drawing.Point(455, 77);
			this.WorkspaceDescription.Name = "WorkspaceDescription";
			this.WorkspaceDescription.Size = new System.Drawing.Size(403, 13);
			this.WorkspaceDescription.TabIndex = 8;
			this.WorkspaceDescription.Text = "... the name of the workspace to use on this host. Formerly known as the clientsp" +
    "ec.";
			// 
			// ConsoleRichTextBox
			// 
			this.ConsoleRichTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsoleRichTextBox.Location = new System.Drawing.Point(2, 164);
			this.ConsoleRichTextBox.Name = "ConsoleRichTextBox";
			this.ConsoleRichTextBox.Size = new System.Drawing.Size(940, 335);
			this.ConsoleRichTextBox.TabIndex = 10;
			this.ConsoleRichTextBox.Text = "";
			// 
			// RefreshServerAddressButton
			// 
			this.RefreshServerAddressButton.Location = new System.Drawing.Point(382, 6);
			this.RefreshServerAddressButton.Name = "RefreshServerAddressButton";
			this.RefreshServerAddressButton.Size = new System.Drawing.Size(57, 23);
			this.RefreshServerAddressButton.TabIndex = 11;
			this.RefreshServerAddressButton.Text = "Refresh";
			this.RefreshServerAddressButton.UseVisualStyleBackColor = true;
			this.RefreshServerAddressButton.Click += new System.EventHandler(this.ServerAddressChanged);
			// 
			// RefreshUserNamesButton
			// 
			this.RefreshUserNamesButton.Location = new System.Drawing.Point(382, 40);
			this.RefreshUserNamesButton.Name = "RefreshUserNamesButton";
			this.RefreshUserNamesButton.Size = new System.Drawing.Size(57, 23);
			this.RefreshUserNamesButton.TabIndex = 12;
			this.RefreshUserNamesButton.Text = "Refresh";
			this.RefreshUserNamesButton.UseVisualStyleBackColor = true;
			this.RefreshUserNamesButton.Click += new System.EventHandler(this.RefreshUserNames);
			// 
			// RefreshWorkspaceNamesButton
			// 
			this.RefreshWorkspaceNamesButton.Location = new System.Drawing.Point(382, 74);
			this.RefreshWorkspaceNamesButton.Name = "RefreshWorkspaceNamesButton";
			this.RefreshWorkspaceNamesButton.Size = new System.Drawing.Size(57, 23);
			this.RefreshWorkspaceNamesButton.TabIndex = 13;
			this.RefreshWorkspaceNamesButton.Text = "Refresh";
			this.RefreshWorkspaceNamesButton.UseVisualStyleBackColor = true;
			// 
			// ConnectButton
			// 
			this.ConnectButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConnectButton.Enabled = false;
			this.ConnectButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.ConnectButton.Location = new System.Drawing.Point(772, 107);
			this.ConnectButton.Name = "ConnectButton";
			this.ConnectButton.Size = new System.Drawing.Size(160, 40);
			this.ConnectButton.TabIndex = 18;
			this.ConnectButton.Text = "Connect";
			this.ConnectButton.UseVisualStyleBackColor = true;
			this.ConnectButton.Click += new System.EventHandler(this.ConnectButtonClick);
			// 
			// ConnectionDialog
			// 
			this.AcceptButton = this.RefreshServerAddressButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(944, 501);
			this.Controls.Add(this.ConnectButton);
			this.Controls.Add(this.RefreshWorkspaceNamesButton);
			this.Controls.Add(this.RefreshUserNamesButton);
			this.Controls.Add(this.RefreshServerAddressButton);
			this.Controls.Add(this.ConsoleRichTextBox);
			this.Controls.Add(this.WorkspaceDescription);
			this.Controls.Add(this.UserNameDescription);
			this.Controls.Add(this.SerDescriptionLabel);
			this.Controls.Add(this.WorkspaceComboBox);
			this.Controls.Add(this.WorkspaceLabel);
			this.Controls.Add(this.UserNameComboBox);
			this.Controls.Add(this.UserNameLabel);
			this.Controls.Add(this.ServerAddressComboBox);
			this.Controls.Add(this.ServerAddressLabel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.Name = "ConnectionDialog";
			this.Text = "Open a connection to a Perforce server";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label ServerAddressLabel;
		private System.Windows.Forms.ComboBox ServerAddressComboBox;
		private System.Windows.Forms.Label UserNameLabel;
		private System.Windows.Forms.ComboBox UserNameComboBox;
		private System.Windows.Forms.Label WorkspaceLabel;
		private System.Windows.Forms.ComboBox WorkspaceComboBox;
		private System.Windows.Forms.Label SerDescriptionLabel;
		private System.Windows.Forms.Label UserNameDescription;
		private System.Windows.Forms.Label WorkspaceDescription;
		private System.Windows.Forms.RichTextBox ConsoleRichTextBox;
		private System.Windows.Forms.Button RefreshServerAddressButton;
		private System.Windows.Forms.Button RefreshUserNamesButton;
		private System.Windows.Forms.Button RefreshWorkspaceNamesButton;
		private System.Windows.Forms.Button ConnectButton;
	}
}