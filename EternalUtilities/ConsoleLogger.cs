// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Eternal.EternalUtilities
{
	/// <summary>
	///     The <see cref="Eternal.EternalUtilities" /> namespace contains helper functions for many common functions, such as logging and Xml parsing.
	/// </summary>
	[CompilerGenerated]
	internal class NamespaceDoc
	{
	}

	/// <summary>Class to handle logging to the command prompt.</summary>
	public static class ConsoleLogger
	{
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

		/// <summary>Returns a timestamp string consistent for all messaging.</summary>
		/// <returns>Returns a timestamp string in local time.</returns>
		private static string GetISOTimeStamp()
		{
			return DateTime.Now.ToString( "HH:mm:ss", CultureInfo.InvariantCulture ) + ": ";
		}

		/// <summary>Display a prominent message.</summary>
		/// <param name="Line">Line of text to display prominently.</param>
		public static void Title( string Line )
		{
			ConsoleColor Foreground = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine( GetISOTimeStamp() + Line );
			Console.ForegroundColor = Foreground;

			Debug.WriteLine( GetISOTimeStamp() + Line );
		}

		/// <summary>Display a verbose logging message.</summary>
		/// <param name="Line">Line of text to display.</param>
		public static void Verbose( string Line )
		{
			if( VerboseLogs )
			{
				Console.WriteLine( GetISOTimeStamp() + Line );
				Debug.WriteLine( GetISOTimeStamp() + Line );
			}
		}

		/// <summary>Display a standard log message.</summary>
		/// <param name="Line">Line of text to display.</param>
		public static void Log( string Line )
		{
			if( !SuppressLogs )
			{
				Console.WriteLine( GetISOTimeStamp() + Line );
			}

			Debug.WriteLine( GetISOTimeStamp() + Line );
		}

		/// <summary>Display a success message in green.</summary>
		/// <param name="Line">Line of warning text to display.</param>
		public static void Success( string Line )
		{
			if( !SuppressWarnings )
			{
				ConsoleColor Foreground = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Green;
				Console.WriteLine( GetISOTimeStamp() + "SUCCESS: " + Line );
				Console.ForegroundColor = Foreground;
			}

			Debug.WriteLine( GetISOTimeStamp() + "SUCCESS: " + Line );
		}

		/// <summary>Display a warning message in yellow.</summary>
		/// <param name="Line">Line of warning text to display.</param>
		public static void Warning( string Line )
		{
			if( !SuppressWarnings )
			{
				ConsoleColor Foreground = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine( GetISOTimeStamp() + "WARNING: " + Line );
				Console.ForegroundColor = Foreground;
			}

			Debug.WriteLine( GetISOTimeStamp() + "WARNING: " + Line );
		}

		/// <summary>Display an error message in red.</summary>
		/// <param name="Line">Line of error text to display.</param>
		public static void Error( string Line )
		{
			if( !SuppressErrors )
			{
				ConsoleColor Foreground = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine( GetISOTimeStamp() + "ERROR: " + Line );
				Console.ForegroundColor = Foreground;
			}

			Debug.WriteLine( GetISOTimeStamp() + "ERROR: " + Line );
		}
	}
}
