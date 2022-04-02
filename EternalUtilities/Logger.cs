// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Eternal.EternalUtilities
{
	/// <summary>Class to handle logging to the command prompt or a Windows form.</summary>
	public static class Logger
	{
		/// <summary>Whether to display to the console.</summary>
		public static bool ConsoleLogging
		{
			get;
			set;
		}

		/// <summary>Whether to display to a form.</summary>
		public static bool FormsLogging
		{
			get;
			set;
		}

		/// <summary>Display a prominent message.</summary>
		/// <param name="Line">Line of text to display prominently.</param>
		public static void Title( string Line )
		{
			if( ConsoleLogging )
			{
				ConsoleLogger.Title( Line );
			}

			if( FormsLogging )
			{
				FormsLogger.Title( Line );
			}
		}

		/// <summary>Display a verbose logging message.</summary>
		/// <param name="Line">Line of text to display.</param>
		public static void Verbose( string Line )
		{
			if( ConsoleLogging )
			{
				ConsoleLogger.Verbose( Line );
			}

			if( FormsLogging )
			{
				FormsLogger.Verbose( Line );
			}
		}

		/// <summary>Display a standard log message.</summary>
		/// <param name="Line">Line of text to display.</param>
		public static void Log( string Line )
		{
			if( ConsoleLogging )
			{
				ConsoleLogger.Log( Line );
			}

			if( FormsLogging )
			{
				FormsLogger.Log( Line );
			}
		}

		/// <summary>Display a success message in green.</summary>
		/// <param name="Line">Line of warning text to display.</param>
		public static void Success( string Line )
		{
			if( ConsoleLogging )
			{
				ConsoleLogger.Success( Line );
			}

			if( FormsLogging )
			{
				FormsLogger.Success( Line );
			}
		}

		/// <summary>Display a warning message in yellow.</summary>
		/// <param name="Line">Line of warning text to display.</param>
		public static void Warning( string Line )
		{
			if( ConsoleLogging )
			{
				ConsoleLogger.Warning( Line );
			}

			if( FormsLogging )
			{
				FormsLogger.Warning( Line );
			}
		}

		/// <summary>Display an error message in red.</summary>
		/// <param name="Line">Line of error text to display.</param>
		public static void Error( string Line )
		{
			if( ConsoleLogging )
			{
				ConsoleLogger.Error( Line );
			}

			if( FormsLogging )
			{
				FormsLogger.Error( Line );
			}
		}
	}
}
