namespace Transfluent
{
	partial class SettingsDialog
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
			this.ConfigurationPropertyGrid = new System.Windows.Forms.PropertyGrid();
			this.SuspendLayout();
			// 
			// ConfigurationPropertyGrid
			// 
			this.ConfigurationPropertyGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ConfigurationPropertyGrid.Location = new System.Drawing.Point(0, 0);
			this.ConfigurationPropertyGrid.Name = "ConfigurationPropertyGrid";
			this.ConfigurationPropertyGrid.Size = new System.Drawing.Size(444, 559);
			this.ConfigurationPropertyGrid.TabIndex = 0;
			// 
			// SettingsDialog
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(444, 559);
			this.Controls.Add(this.ConfigurationPropertyGrid);
			this.Name = "SettingsDialog";
			this.Text = "Transfluent Settings";
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.PropertyGrid ConfigurationPropertyGrid;
	}
}