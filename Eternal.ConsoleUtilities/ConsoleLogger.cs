// Copyright Eternal Developments LLC. All Rights Reserved.

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
		/// <param name="line">Line of text to display prominently.</param>
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
		/// <param name="line">Line of text to display.</param>
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
		/// <param name="line">Line of text to display.</param>
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
		/// <param name="line">Line of warning text to display.</param>
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
		/// <param name="line">Line of warning text to display.</param>
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
		/// <param name="line">Line of error text to display.</param>
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

		/// <summary>
		/// Converts a length of time into a human readable string
		/// </summary>
		/// <param name="timeSpan">A duration of time</param>
		/// <returns></returns>
		public static string TimeString( TimeSpan timeSpan )
		{
			if( timeSpan.TotalDays > 1 )
			{
				string plural_days = ( timeSpan.Days > 1 ) ? "s" : "";
				string plural_hours = ( timeSpan.Hours > 1 ) ? "s" : "";
				return $"{timeSpan.Days:n0} day{plural_days} {timeSpan.Hours:n0} hour{plural_hours}";
			}
			else if( timeSpan.TotalHours > 1 )
			{
				string plural_hours = ( timeSpan.Hours > 1 ) ? "s" : "";
				string plural_minutes = ( timeSpan.Minutes > 1 ) ? "s" : "";
				return $"{timeSpan.Hours:n0} hour{plural_hours} {timeSpan.Minutes:n0} minute{plural_minutes}";
			}
			else if( timeSpan.TotalMinutes > 1 )
			{
				string plural_minutes = ( timeSpan.Minutes > 1 ) ? "s" : "";
				string plural_seconds = ( timeSpan.Minutes > 1 ) ? "s" : "";
				return $"{timeSpan.Minutes:n0} minute{plural_minutes} {timeSpan.Seconds:n0} second{plural_seconds}";
			}
			else if( timeSpan.TotalSeconds > 1 )
			{
				return $"{timeSpan.Seconds:n1} seconds";
			}

			string plural_milliseconds = ( timeSpan.TotalMilliseconds > 1 ) ? "s" : "";
			return $"{timeSpan.TotalMilliseconds:n0} millisecond{plural_milliseconds}";
		}

		/// <summary>
		/// Converts a number of bytes into a sensible human readable version
		/// </summary>
		/// <param name="numberOfBytes">A count of bytes from TB to B.</param>
		/// <returns></returns>
		public static string MemoryString( long numberOfBytes )
		{
			const double kilo_byte = 1024L;
			const double mega_byte = 1024L * 1024L;
			const double giga_byte = 1024L * 1024L * 1024L;
			const double tera_byte = 1024L * 1024L * 1024L * 1024L;

			if( numberOfBytes > tera_byte )
			{
				double tb = numberOfBytes / tera_byte;
				return $"{tb:n3} TB";
			}
			else if( numberOfBytes > giga_byte )
			{
				double gb = numberOfBytes / giga_byte;
				return $"{gb:n3} GB";
			}
			else if( numberOfBytes > mega_byte )
			{
				double mb = numberOfBytes / mega_byte;
				return $"{mb:n3} MB";
			}
			else if( numberOfBytes > kilo_byte )
			{
				double kb = numberOfBytes / kilo_byte;
				return $"{kb:n3} kB";
			}
				
			return $"{numberOfBytes} B";
		}
	}
}
