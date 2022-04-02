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
	public partial class MakeConsistentDialog : Form
	{
		private List<string> CorruptFiles;
		private List<string> MissingFiles;
		private List<string> ExtraFiles;
		private List<string> WritableFiles;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InCorruptFiles"></param>
		/// <param name="InMissingFiles"></param>
		/// <param name="InExtraFiles"></param>
		/// <param name="InWritableFiles"></param>
		public MakeConsistentDialog( List<string> InCorruptFiles, List<string> InMissingFiles, List<string> InExtraFiles, List<string> InWritableFiles )
		{
			InitializeComponent();

			CorruptFiles = InCorruptFiles;
			MissingFiles = InMissingFiles;
			ExtraFiles = InExtraFiles;
			WritableFiles = InWritableFiles;

			BadChecksumGroupBox.Enabled = ( CorruptFiles.Count > 0 );
			MissingFilesGroupBox.Enabled = ( MissingFiles.Count > 0 );
			ExtraFilesGroupBox.Enabled = ( ExtraFiles.Count > 0 );
			WritableFilesGroupBox.Enabled = ( WritableFiles.Count > 0 );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileNames"></param>
		/// <returns></returns>
		private string GenerateToolTip( List<string> FileNames )
		{
			string Result = String.Join( Environment.NewLine, FileNames.Take( 50 ) );
			if( FileNames.Count > 100 )
			{
				Result += Environment.NewLine + " ... more";
			}

			return Result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="eventArgs"></param>
		private void MissingFilesHover( object Sender, EventArgs EventArgs )
		{
			GroupBoxToolTip.ToolTipTitle = MissingFiles.Count + " missing files";
			GroupBoxToolTip.SetToolTip( ( Control )Sender, GenerateToolTip( MissingFiles ) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="eventArgs"></param>
		private void ExtraFilesHover( object Sender, EventArgs EventArgs )
		{
			GroupBoxToolTip.ToolTipTitle = ExtraFiles.Count + " extra files";
			GroupBoxToolTip.SetToolTip( ( Control )Sender, GenerateToolTip( ExtraFiles ) );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void WritableFilesHover( object Sender, EventArgs EventArgs )
		{
			GroupBoxToolTip.ToolTipTitle = WritableFiles.Count + " locally writable files.";
			GroupBoxToolTip.SetToolTip( ( Control )Sender, GenerateToolTip( WritableFiles ) );
		}
	}
}
