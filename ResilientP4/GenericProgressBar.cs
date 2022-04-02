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
	public partial class GenericProgressBarDialog : Form
	{
		private MainForm RootApplication = null;
		private DateTime LastUpdate = DateTime.MinValue;
		private int LocalValue = 0;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InRootApplication"></param>
		/// <param name="Title"></param>
		/// <param name="Min"></param>
		/// <param name="Max"></param>
		public GenericProgressBarDialog( MainForm InRootApplication, string Title, int Min, int Max )
		{
			RootApplication = InRootApplication;

			InitializeComponent();

			Text = Title;

			GenericProgressBar.Minimum = Min;
			GenericProgressBar.Maximum = Max;
			GenericProgressBar.Value = LocalValue;
		}

		/// <summary>
		/// 
		/// </summary>
		public void Bump( string Detail )
		{
			LocalValue++;

			if( LastUpdate.AddMilliseconds( 25 ) < DateTime.UtcNow )
			{
				if( LocalValue >= GenericProgressBar.Maximum )
				{
					GenericProgressBar.Maximum = LocalValue + 1;
				}

				GenericProgressBar.Value = LocalValue;
				ProgressBarDetail.Text = Detail;
				RootApplication.Tick();

				LastUpdate = DateTime.UtcNow;
			}
		}
	}
}
