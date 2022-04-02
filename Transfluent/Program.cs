// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Eternal.EternalUtilities;

namespace Transfluent
{
	internal static class Program
	{
		/// <summary>
		/// Parse the command line arguments into a Parameters class
		/// </summary>
		/// <param name="Arguments">The command line pased in by the user.</param>
		/// <returns>A class instance containing all parsed elements.</returns>
		private static Parameters ParseArguments( string[] Arguments )
		{
			Parameters CommandLineArguments = new Parameters();

			foreach( string Argument in Arguments )
			{
				if( Argument.StartsWith( "/email=", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.UserName = Argument.Substring( "/email=".Length ).Trim();
				}
				else if( Argument.StartsWith( "/password=", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.Password = Argument.Substring( "/password=".Length ).Trim();
				}
				else if( Argument.StartsWith( "/manifest=", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.ManifestFileName = Argument.Substring( "/manifest=".Length ).Trim();
				}
				else if( Argument.StartsWith( "/language=", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.LanguageFolder = Argument.Substring( "/language=".Length ).Trim();
				}
				else if( Argument.StartsWith( "/upload", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.Command = Operation.UploadUntranslated;
				}
				else if( Argument.StartsWith( "/download", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.Command = Operation.DownloadTranslated;
				}
				else if( Argument.StartsWith( "/refresh", StringComparison.OrdinalIgnoreCase ) )
				{
					CommandLineArguments.Command = Operation.RefreshArchives;
				}
			}

			return CommandLineArguments;
		}

		/// <summary>
		/// Validates the parsed arguments to make sure there is enough to attempt a localization operation.
		/// </summary>
		/// <param name="CommandLineArguments">A class instance containing the parsed command line.</param>
		/// <returns>An exit code. 0 for success, less than zero for an error.</returns>
		private static int Validate( Parameters CommandLineArguments )
		{
			if( CommandLineArguments.UserName == null || CommandLineArguments.Password == null )
			{
				Logger.Error( "Credentials are required!" );
				return -100;
			}

			if( CommandLineArguments.ManifestFileName == null )
			{
				Logger.Error( "A manifest file name is required!" );
				return -101;
			}

			if( CommandLineArguments.Command == Operation.None )
			{
				Logger.Error( "An operation is required!" );
				return -102;
			}

			return 0;
		}

		/// <summary>
		/// Print the command line usage to the console.
		/// </summary>
		private static void Usage()
		{
			Logger.Title( "A tool to upload Unreal Engine 4 Localization file to Transfluent - https://www.transfluent.com/en/" );
			Logger.Title( "Copyright Eternal Developments, LLC. All rights reserved." );
			Logger.Log( "Run without parameters to use a GUI, or with command line parameters to use the command line version." );
			Logger.Log( "" );
			Logger.Log( "Usage:" );
			Logger.Log( "Transfluent.exe <credentials> <manifest file name> <operation> [language]" );
			Logger.Log( "Where:" );
			Logger.Log( "<credentials> /email=john@doe.net /password=12345 - these are required." );
			Logger.Log( "<manifest file name> /manifest=<branch path>/Engine/Content/Localization/Editor/Editor.manifest - this is required." );
			Logger.Log( "<operation> /upload - to upload all untranslated strings for all supported languages to Transfluent." );
			Logger.Log( "<operation> /download - to download all translated strings for all supported languages from Transfluent." );
			Logger.Log( "<operation> /refresh - to do both the above operations." );
			Logger.Log( "<language> /language=<folder name of language> - only operate on a single language." );
		}

		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		[STAThread]
		private static int Main( string[] Arguments )
		{
			int ExitCode = 0;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			Application.CurrentCulture = CultureInfo.InvariantCulture;

			if( Arguments.Length > 0 )
			{
				Logger.ConsoleLogging = true;
				NativeMethods.AttachConsole();

				Parameters CommandLineArguments = ParseArguments( Arguments );
				Logger.Title( "" );
				Logger.Title( "Checking parameters" );
				ExitCode = Validate( CommandLineArguments );
				if( ExitCode == 0 )
				{
					using( Transfluent Host = new Transfluent() )
					{
						ExitCode = Host.RunCommandline( CommandLineArguments );
					}
				}
				else
				{
					Usage();
				}
			}
			else
			{
				Logger.FormsLogging = true;

				DialogResult Result;
				string UserName = null;
				string Password = null;

				using( TransfluentLogin Login = new TransfluentLogin() )
				{
					Result = Login.ShowDialog();
					UserName = Login.TextBoxUserName.Text;
					Password = Login.TextBoxPassword.Text;
				}

				if( Result == DialogResult.OK )
				{
					using( Transfluent Host = new Transfluent() )
					{
						Host.Initialize( UserName, Password );

						while( Host.Running )
						{
							Host.Tick();

							Application.DoEvents();
							Thread.Sleep( 10 );
						}

						Host.Shutdown();
					}
				}
				else
				{
					ExitCode = -1;
				}
			}

			return ExitCode;
		}
	}
}