// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ResilientP4
{
    /// <summary>
    /// 
    /// </summary>
	public partial class SettingsDialog : Form
	{
		public SettingsDialog( MainForm Owner )
		{
			InitializeComponent();

			SettingsPropertyGrid.SelectedObject = Owner.Config;
		}
	}
}
