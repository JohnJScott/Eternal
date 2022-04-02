namespace WandHandler
{
	partial class WandReceiver
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
			this.ConsoleSpewText = new System.Windows.Forms.TextBox();
			this.SuspendLayout();
			// 
			// ConsoleSpewText
			// 
			this.ConsoleSpewText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.ConsoleSpewText.Location = new System.Drawing.Point(0, 0);
			this.ConsoleSpewText.Multiline = true;
			this.ConsoleSpewText.Name = "ConsoleSpewText";
			this.ConsoleSpewText.Size = new System.Drawing.Size(851, 563);
			this.ConsoleSpewText.TabIndex = 0;
			// 
			// WandReceiver
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(850, 562);
			this.Controls.Add(this.ConsoleSpewText);
			this.Name = "WandReceiver";
			this.Text = "WandReceiver";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.TextBox ConsoleSpewText;
	}
}

