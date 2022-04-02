namespace ResilientP4
{
	partial class EnterPassword
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EnterPassword));
			this.PasswordTextBox = new System.Windows.Forms.TextBox();
			this.EnterPasswordText = new System.Windows.Forms.TextBox();
			this.PasswordLabel = new System.Windows.Forms.Label();
			this.OKButton = new System.Windows.Forms.Button();
			this.AbortButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// PasswordTextBox
			// 
			this.PasswordTextBox.Location = new System.Drawing.Point(250, 83);
			this.PasswordTextBox.Name = "PasswordTextBox";
			this.PasswordTextBox.Size = new System.Drawing.Size(275, 20);
			this.PasswordTextBox.TabIndex = 0;
			this.PasswordTextBox.UseSystemPasswordChar = true;
			// 
			// EnterPasswordText
			// 
			this.EnterPasswordText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.EnterPasswordText.BackColor = System.Drawing.SystemColors.Control;
			this.EnterPasswordText.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.EnterPasswordText.Location = new System.Drawing.Point(12, 12);
			this.EnterPasswordText.Multiline = true;
			this.EnterPasswordText.Name = "EnterPasswordText";
			this.EnterPasswordText.Size = new System.Drawing.Size(579, 65);
			this.EnterPasswordText.TabIndex = 1;
			this.EnterPasswordText.Text = resources.GetString("EnterPasswordText.Text");
			// 
			// PasswordLabel
			// 
			this.PasswordLabel.AutoSize = true;
			this.PasswordLabel.Location = new System.Drawing.Point(78, 86);
			this.PasswordLabel.Name = "PasswordLabel";
			this.PasswordLabel.Size = new System.Drawing.Size(135, 13);
			this.PasswordLabel.TabIndex = 2;
			this.PasswordLabel.Text = "Please enter the password:";
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.Location = new System.Drawing.Point(516, 118);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = new System.Drawing.Size(75, 23);
			this.OKButton.TabIndex = 3;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButtonClick);
			// 
			// AbortButton
			// 
			this.AbortButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AbortButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.AbortButton.Location = new System.Drawing.Point(435, 118);
			this.AbortButton.Name = "AbortButton";
			this.AbortButton.Size = new System.Drawing.Size(75, 23);
			this.AbortButton.TabIndex = 4;
			this.AbortButton.Text = "Cancel";
			this.AbortButton.UseVisualStyleBackColor = true;
			this.AbortButton.Click += new System.EventHandler(this.CancelButtonClick);
			// 
			// EnterPassword
			// 
			this.AcceptButton = this.OKButton;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.Control;
			this.CancelButton = this.AbortButton;
			this.ClientSize = new System.Drawing.Size(603, 153);
			this.Controls.Add(this.AbortButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.PasswordLabel);
			this.Controls.Add(this.EnterPasswordText);
			this.Controls.Add(this.PasswordTextBox);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "EnterPassword";
			this.Text = "Perforce Password Required";
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label PasswordLabel;
		private System.Windows.Forms.Button OKButton;
		public System.Windows.Forms.TextBox PasswordTextBox;
		public System.Windows.Forms.TextBox EnterPasswordText;
		private System.Windows.Forms.Button AbortButton;
	}
}