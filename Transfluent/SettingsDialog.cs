// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System.Windows.Forms;

namespace Transfluent
{
	public partial class SettingsDialog : Form
	{
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Owner"></param>
		public SettingsDialog( Transfluent Owner )
		{
			InitializeComponent();

			ConfigurationPropertyGrid.SelectedObject = Owner.Config;
		}
	}
}