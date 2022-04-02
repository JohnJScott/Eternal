namespace ResilientP4
{
	partial class GenericProgressBarDialog
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GenericProgressBarDialog));
			this.GenericProgressBar = new System.Windows.Forms.ProgressBar();
			this.ProgressBarDetail = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// GenericProgressBar
			// 
			this.GenericProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.GenericProgressBar.Location = new System.Drawing.Point(12, 12);
			this.GenericProgressBar.Name = "GenericProgressBar";
			this.GenericProgressBar.Size = new System.Drawing.Size(616, 23);
			this.GenericProgressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
			this.GenericProgressBar.TabIndex = 0;
			// 
			// ProgressBarDetail
			// 
			this.ProgressBarDetail.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBarDetail.AutoEllipsis = true;
			this.ProgressBarDetail.Location = new System.Drawing.Point(12, 52);
			this.ProgressBarDetail.Name = "ProgressBarDetail";
			this.ProgressBarDetail.Size = new System.Drawing.Size(616, 20);
			this.ProgressBarDetail.TabIndex = 1;
			this.ProgressBarDetail.Text = "Detailed information";
			this.ProgressBarDetail.TextAlign = System.Drawing.ContentAlignment.TopCenter;
			// 
			// GenericProgressBarDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(640, 93);
			this.ControlBox = false;
			this.Controls.Add(this.ProgressBarDetail);
			this.Controls.Add(this.GenericProgressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "GenericProgressBarDialog";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Text = "Progress Bar Title";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar GenericProgressBar;
		private System.Windows.Forms.Label ProgressBarDetail;
	}
}