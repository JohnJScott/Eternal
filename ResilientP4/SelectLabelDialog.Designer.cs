namespace ResilientP4
{
	partial class SelectLabelDialog
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
			this.SelectLabelGridView = new System.Windows.Forms.DataGridView();
			this.LabelName = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LabelLastUpdated = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LabelLastAccessed = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LabelOwner = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.LabelLocked = new System.Windows.Forms.DataGridViewCheckBoxColumn();
			this.LabelDescription = new System.Windows.Forms.DataGridViewTextBoxColumn();
			this.SyncToLabelButton = new System.Windows.Forms.Button();
			this.LabelSelectTitle = new System.Windows.Forms.Label();
			this.LabelFilterLabel = new System.Windows.Forms.Label();
			this.SyncCancelButton = new System.Windows.Forms.Button();
			((System.ComponentModel.ISupportInitialize)(this.SelectLabelGridView)).BeginInit();
			this.SuspendLayout();
			// 
			// SelectLabelGridView
			// 
			this.SelectLabelGridView.AllowUserToAddRows = false;
			this.SelectLabelGridView.AllowUserToDeleteRows = false;
			this.SelectLabelGridView.AllowUserToOrderColumns = true;
			this.SelectLabelGridView.AllowUserToResizeRows = false;
			this.SelectLabelGridView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.SelectLabelGridView.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
			this.SelectLabelGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
			this.SelectLabelGridView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.LabelName,
            this.LabelLastUpdated,
            this.LabelLastAccessed,
            this.LabelOwner,
            this.LabelLocked,
            this.LabelDescription});
			this.SelectLabelGridView.Location = new System.Drawing.Point(0, 46);
			this.SelectLabelGridView.MultiSelect = false;
			this.SelectLabelGridView.Name = "SelectLabelGridView";
			this.SelectLabelGridView.ReadOnly = true;
			this.SelectLabelGridView.RowTemplate.ReadOnly = true;
			this.SelectLabelGridView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
			this.SelectLabelGridView.Size = new System.Drawing.Size(967, 436);
			this.SelectLabelGridView.TabIndex = 0;
			this.SelectLabelGridView.SelectionChanged += new System.EventHandler(this.DataGridViewSelectionChanged);
			// 
			// LabelName
			// 
			this.LabelName.FillWeight = 35F;
			this.LabelName.HeaderText = "Name";
			this.LabelName.Name = "LabelName";
			this.LabelName.ReadOnly = true;
			this.LabelName.ToolTipText = "The name of the label.";
			// 
			// LabelLastUpdated
			// 
			this.LabelLastUpdated.FillWeight = 15F;
			this.LabelLastUpdated.HeaderText = "Updated";
			this.LabelLastUpdated.Name = "LabelLastUpdated";
			this.LabelLastUpdated.ReadOnly = true;
			this.LabelLastUpdated.ToolTipText = "The server time the label was last updated.";
			// 
			// LabelLastAccessed
			// 
			this.LabelLastAccessed.FillWeight = 15F;
			this.LabelLastAccessed.HeaderText = "Accessed";
			this.LabelLastAccessed.Name = "LabelLastAccessed";
			this.LabelLastAccessed.ReadOnly = true;
			this.LabelLastAccessed.ToolTipText = "The server time when the label was last accessed.";
			// 
			// LabelOwner
			// 
			this.LabelOwner.FillWeight = 15F;
			this.LabelOwner.HeaderText = "Owner";
			this.LabelOwner.Name = "LabelOwner";
			this.LabelOwner.ReadOnly = true;
			this.LabelOwner.ToolTipText = "The owner of the label.";
			// 
			// LabelLocked
			// 
			this.LabelLocked.FillWeight = 8F;
			this.LabelLocked.HeaderText = "Locked";
			this.LabelLocked.Name = "LabelLocked";
			this.LabelLocked.ReadOnly = true;
			this.LabelLocked.ToolTipText = "Whether the label can be edited or not.";
			// 
			// LabelDescription
			// 
			this.LabelDescription.FillWeight = 60F;
			this.LabelDescription.HeaderText = "Description";
			this.LabelDescription.Name = "LabelDescription";
			this.LabelDescription.ReadOnly = true;
			// 
			// SyncToLabelButton
			// 
			this.SyncToLabelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SyncToLabelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SyncToLabelButton.Location = new System.Drawing.Point(798, 488);
			this.SyncToLabelButton.Name = "SyncToLabelButton";
			this.SyncToLabelButton.Size = new System.Drawing.Size(157, 46);
			this.SyncToLabelButton.TabIndex = 1;
			this.SyncToLabelButton.Text = "Select Label";
			this.SyncToLabelButton.UseVisualStyleBackColor = true;
			this.SyncToLabelButton.Click += new System.EventHandler(this.SyncToLabelButtonClick);
			// 
			// LabelSelectTitle
			// 
			this.LabelSelectTitle.AutoSize = true;
			this.LabelSelectTitle.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelSelectTitle.Location = new System.Drawing.Point(12, 15);
			this.LabelSelectTitle.Name = "LabelSelectTitle";
			this.LabelSelectTitle.Size = new System.Drawing.Size(150, 16);
			this.LabelSelectTitle.TabIndex = 2;
			this.LabelSelectTitle.Text = "All labels filtered by:";
			// 
			// LabelFilterLabel
			// 
			this.LabelFilterLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LabelFilterLabel.Location = new System.Drawing.Point(168, 15);
			this.LabelFilterLabel.Name = "LabelFilterLabel";
			this.LabelFilterLabel.Size = new System.Drawing.Size(250, 23);
			this.LabelFilterLabel.TabIndex = 3;
			// 
			// SyncCancelButton
			// 
			this.SyncCancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SyncCancelButton.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.SyncCancelButton.Location = new System.Drawing.Point(635, 488);
			this.SyncCancelButton.Name = "SyncCancelButton";
			this.SyncCancelButton.Size = new System.Drawing.Size(157, 46);
			this.SyncCancelButton.TabIndex = 4;
			this.SyncCancelButton.Text = "Cancel";
			this.SyncCancelButton.UseVisualStyleBackColor = true;
			this.SyncCancelButton.Click += new System.EventHandler(this.CancelButtonClicked);
			// 
			// SelectLabelDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(967, 546);
			this.Controls.Add(this.SyncCancelButton);
			this.Controls.Add(this.LabelFilterLabel);
			this.Controls.Add(this.LabelSelectTitle);
			this.Controls.Add(this.SyncToLabelButton);
			this.Controls.Add(this.SelectLabelGridView);
			this.MinimumSize = new System.Drawing.Size(640, 480);
			this.Name = "SelectLabelDialog";
			this.Text = "Select Label";
			((System.ComponentModel.ISupportInitialize)(this.SelectLabelGridView)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.DataGridView SelectLabelGridView;
		private System.Windows.Forms.Button SyncToLabelButton;
		private System.Windows.Forms.Label LabelSelectTitle;
		private System.Windows.Forms.Label LabelFilterLabel;
		private System.Windows.Forms.Button SyncCancelButton;
		private System.Windows.Forms.DataGridViewTextBoxColumn LabelName;
		private System.Windows.Forms.DataGridViewTextBoxColumn LabelLastUpdated;
		private System.Windows.Forms.DataGridViewTextBoxColumn LabelLastAccessed;
		private System.Windows.Forms.DataGridViewTextBoxColumn LabelOwner;
		private System.Windows.Forms.DataGridViewCheckBoxColumn LabelLocked;
		private System.Windows.Forms.DataGridViewTextBoxColumn LabelDescription;
	}
}