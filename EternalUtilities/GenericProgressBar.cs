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

namespace Eternal.EternalUtilities
{
	/// <summary>
	/// A simple progress bar dialog
	/// </summary>
	public partial class GenericProgressBar : Form
	{
		/// <summary>
		/// Create a new simple progress bar dialog.
		/// </summary>
		/// <param name="Title">The title of the dialog.</param>
		/// <param name="Explanation">The detailed text to appear in the dialog.</param>
		/// <param name="MaxCount">The maximum value allowed such that 0 is 0%, and MaxCount is 100%.</param>
		public GenericProgressBar( string Title, string Explanation, int MaxCount )
		{
			InitializeComponent();

			Text = Title;
			GenericProgressBarExplanation.Text = Explanation;

			ProgressBar.Minimum = 0;
			ProgressBar.Maximum = MaxCount;

			Show();
		}

		/// <summary>
		/// Increment the progress bar one element to the right
		/// </summary>
		public void Bump()
		{
			Update( ProgressBar.Value + 1 );
		}

		/// <summary>
		/// Update the progress bar to a new value.
		/// </summary>
		/// <param name="CurrentCount">The new value to set. It is bounnds checked to be valid.</param>
		public void Update( int CurrentCount )
		{
			if( CurrentCount <= ProgressBar.Minimum )
			{
				ProgressBar.Value = ProgressBar.Minimum;
			}
			else if( CurrentCount >= ProgressBar.Maximum )
			{
				ProgressBar.Value = ProgressBar.Maximum;
			}
			else
			{
				ProgressBar.Value = CurrentCount;
			}

			Application.DoEvents();
		}
	}
}
