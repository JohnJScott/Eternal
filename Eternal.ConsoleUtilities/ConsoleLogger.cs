// Copyright 2015-2022 Eternal Developments LLC. All Rights Reserved.

using System.Diagnostics;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Eternal.ConsoleUtilities
{
	/// <summary>
	///     The <see cref="Eternal.ConsoleUtilities" /> namespace contains helper functions for many common functions, such as logging and Xml parsing.
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
		/// <param name="line">line of text to display prominently.</param>
		public static bool Title( string line )
		{
			ConsoleColor foreground = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			Console.WriteLine( GetISOTimeStamp() + line );
			Console.ForegroundColor = foreground;

			Debug.WriteLine( GetISOTimeStamp() + line );
			return true;
		}

		/// <summary>Display a verbose logging message.</summary>
		/// <param name="line">line of text to display.</param>
		public static bool Verbose( string line )
		{
			if( VerboseLogs )
			{
				Console.WriteLine( GetISOTimeStamp() + line );
				Debug.WriteLine( GetISOTimeStamp() + line );
			}

			return VerboseLogs;
		}

		/// <summary>Display a standard log message.</summary>
		/// <param name="line">line of text to display.</param>
		public static bool Log( string line )
		{
			if( !SuppressLogs )
			{
				Console.WriteLine( GetISOTimeStamp() + line );
			}

			Debug.WriteLine( GetISOTimeStamp() + line );
			return !SuppressLogs;
		}

		/// <summary>Display a success message in green.</summary>
		/// <param name="line">line of warning text to display.</param>
		public static bool Success( string line )
		{
			ConsoleColor foreground = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			Console.WriteLine( GetISOTimeStamp() + "SUCCESS: " + line );
			Console.ForegroundColor = foreground;

			Debug.WriteLine( GetISOTimeStamp() + "SUCCESS: " + line );
			return true;
		}

		/// <summary>Display a warning message in yellow.</summary>
		/// <param name="line">line of warning text to display.</param>
		public static bool Warning( string line )
		{
			if( !SuppressWarnings )
			{
				ConsoleColor foreground = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Yellow;
				Console.WriteLine( GetISOTimeStamp() + "WARNING: " + line );
				Console.ForegroundColor = foreground;
			}

			Debug.WriteLine( GetISOTimeStamp() + "WARNING: " + line );
			return !SuppressWarnings;
		}

		/// <summary>Display an error message in red.</summary>
		/// <param name="line">line of error text to display.</param>
		public static bool Error( string line )
		{
			if( !SuppressErrors )
			{
				ConsoleColor foreground = Console.ForegroundColor;
				Console.ForegroundColor = ConsoleColor.Red;
				Console.WriteLine( GetISOTimeStamp() + "ERROR: " + line );
				Console.ForegroundColor = foreground;
			}

			Debug.WriteLine( GetISOTimeStamp() + "ERROR: " + line );
			return !SuppressErrors;
		}
	}
}
