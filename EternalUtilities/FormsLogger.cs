// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Windows.Forms;

namespace Eternal.EternalUtilities
{
	/// <summary>
	///     A class to handle logging to a RichTextBox form.
	/// </summary>
	public static class FormsLogger
	{
		private static Form OwningForm;
		private static RichTextBox HostTextBox;

		private static readonly Object LockObject = new Object();

		/// <summary>Whether to display verbose log messages.</summary>
		public static bool VerboseLogs
		{
			get;
			set;
		}

		/// <summary>Whether to suppress log messages.</summary>
		public static bool SuppressLogs
		{
			get;
			set;
		}

		/// <summary>Whether to suppress warning messages.</summary>
		public static bool SuppressWarnings
		{
			get;
			set;
		}

		/// <summary>Whether to suppress error messages.</summary>
		public static bool SuppressErrors
		{
			get;
			set;
		}

		/// <summary>
		/// </summary>
		/// <param name="InForm"></param>
		/// <param name="InTextBox"></param>
		public static void SetRecipient( Form InForm, RichTextBox InTextBox )
		{
			OwningForm = InForm;
			HostTextBox = InTextBox;
		}

		/// <summary></summary>
		/// <param name="Prefix"></param>
		/// <param name="Line"></param>
		/// <param name="SelectionColor"></param>
		private static void SafeAppendText( string Prefix, string Line, Color SelectionColor )
		{
			HostTextBox.SelectionColor = SelectionColor;
			HostTextBox.AppendText( StringHelper.ISOTimestamp + Prefix + Line + Environment.NewLine );

			// Cap the rich text box to 10000 lines to avoid out of memort exceptions
			if( HostTextBox.Lines.Length > 10000 )
			{
				int FirstLine = HostTextBox.Text.IndexOf( "\n", StringComparison.Ordinal ) + 1;
				if( FirstLine > 0 )
				{
					HostTextBox.Text = HostTextBox.Text.Substring( FirstLine );
				}
			}

			HostTextBox.ScrollToCaret();
			Application.DoEvents();
		}

		/// <summary>Display a prominent message.</summary>
		/// <param name="Line">Line of text to display prominently.</param>
		public static void Title( string Line )
		{
			if( OwningForm == null || HostTextBox == null || HostTextBox.IsDisposed )
			{
				return;
			}

			if( OwningForm.InvokeRequired )
			{
				OwningForm.Invoke( new DelegateLog( Title ), new object[] {	Line } );
				return;
			}

			lock( LockObject )
			{
				SafeAppendText( "", Line, Color.Black );
			}

			Debug.WriteLine( StringHelper.ISOTimestamp + Line );
		}

		/// <summary>Display a verbose logging message.</summary>
		/// <param name="Line">Line of text to display.</param>
		public static void Verbose( string Line )
		{
			if( VerboseLogs )
			{
				if( OwningForm == null || HostTextBox == null || HostTextBox.IsDisposed )
				{
					return;
				}

				if( OwningForm.InvokeRequired )
				{
					OwningForm.Invoke( new DelegateLog( Verbose ), new object[] { Line } );
					return;
				}

				lock( LockObject )
				{
					SafeAppendText( "", Line, Color.Blue );
				}

				Debug.WriteLine( StringHelper.ISOTimestamp + Line );
			}
		}

		/// <summary>Display a standard log message.</summary>
		/// <param name="Line">Line of text to display.</param>
		public static void Log( string Line )
		{
			if( !SuppressLogs )
			{
				if( OwningForm == null || HostTextBox == null || HostTextBox.IsDisposed )
				{
					return;
				}

				if( OwningForm.InvokeRequired )
				{
					OwningForm.Invoke( new DelegateLog( Log ), new object[] { Line } );
					return;
				}

				lock( LockObject )
				{
					SafeAppendText( "", Line, Color.Blue );
				}
			}

			Debug.WriteLine( StringHelper.ISOTimestamp + Line );
		}

		/// <summary>Display a success message in green.</summary>
		/// <param name="Line">Line of warning text to display.</param>
		public static void Success( string Line )
		{
			if( !SuppressWarnings )
			{
				if( OwningForm == null || HostTextBox == null || HostTextBox.IsDisposed )
				{
					return;
				}

				if( OwningForm.InvokeRequired )
				{
					OwningForm.Invoke( new DelegateLog( Success ), new object[] { Line } );
					return;
				}

				lock( LockObject )
				{
					SafeAppendText( "SUCCESS: ", Line, Color.Green );
				}
			}

			Debug.WriteLine( StringHelper.ISOTimestamp + "SUCCESS: " + Line );
		}

		/// <summary>Display a warning message in yellow.</summary>
		/// <param name="Line">Line of warning text to display.</param>
		public static void Warning( string Line )
		{
			if( !SuppressWarnings )
			{
				if( OwningForm == null || HostTextBox == null || HostTextBox.IsDisposed )
				{
					return;
				}

				if( OwningForm.InvokeRequired )
				{
					OwningForm.Invoke( new DelegateLog( Warning ), new object[] { Line } );
					return;
				}

				lock( LockObject )
				{
					SafeAppendText( "WARNING: ", Line, Color.Brown );
				}
			}

			Debug.WriteLine( StringHelper.ISOTimestamp + "WARNING: " + Line );
		}

		/// <summary>Display an error message in red.</summary>
		/// <param name="Line">Line of error text to display.</param>
		public static void Error( string Line )
		{
			if( !SuppressErrors )
			{
				if( OwningForm == null || HostTextBox == null || HostTextBox.IsDisposed )
				{
					return;
				}

				if( OwningForm.InvokeRequired )
				{
					OwningForm.Invoke( new DelegateLog( Error ), new object[] { Line } );
					return;
				}

				lock( LockObject )
				{
					SafeAppendText( "ERROR: ", Line, Color.Red );
				}
			}

			Debug.WriteLine( StringHelper.ISOTimestamp + "ERROR: " + Line );
		}

		private delegate void DelegateLog( string Line );
	}
}
