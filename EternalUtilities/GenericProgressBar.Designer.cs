namespace Eternal.EternalUtilities
{
	partial class GenericProgressBar
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
			this.ProgressBar = new System.Windows.Forms.ProgressBar();
			this.GenericProgressBarExplanation = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ProgressBar
			// 
			this.ProgressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ProgressBar.Location = new System.Drawing.Point(12, 65);
			this.ProgressBar.Name = "ProgressBar";
			this.ProgressBar.Size = new System.Drawing.Size(660, 44);
			this.ProgressBar.TabIndex = 0;
			// 
			// GenericProgressBarExplanation
			// 
			this.GenericProgressBarExplanation.Location = new System.Drawing.Point(12, 22);
			this.GenericProgressBarExplanation.Name = "GenericProgressBarExplanation";
			this.GenericProgressBarExplanation.Size = new System.Drawing.Size(660, 23);
			this.GenericProgressBarExplanation.TabIndex = 1;
			this.GenericProgressBarExplanation.Text = "Explanation";
			this.GenericProgressBarExplanation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// GenericProgressBar
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(684, 121);
			this.Controls.Add(this.GenericProgressBarExplanation);
			this.Controls.Add(this.ProgressBar);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "GenericProgressBar";
			this.Text = "Title";
			this.TopMost = true;
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.ProgressBar ProgressBar;
		private System.Windows.Forms.Label GenericProgressBarExplanation;
	}
}