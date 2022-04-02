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
	public partial class EnterPassword : Form
	{
		/// <summary>
		/// 
		/// </summary>
		public EnterPassword()
		{
			InitializeComponent();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void OKButtonClick( object Sender, EventArgs EventArguments )
		{
			DialogResult = DialogResult.OK;
			Close();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void CancelButtonClick( object Sender, EventArgs EventArguments )
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
