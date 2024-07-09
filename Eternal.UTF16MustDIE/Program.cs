// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;

namespace Eternal.Utf16MustDie
{
	/// <summary>The main flow control.</summary>
    public class Program
	{
		private static bool ProcessUtf16 = true;
		private static bool ProcessAnsi= true;

		/// <summary>A generic catch all for all unhandled exceptions.</summary>
		/// <param name="sender">The object that created the exception.</param>
		/// <param name="arguments">Details about the exception.</param>
		private static void GenericExceptionHandler( object sender, UnhandledExceptionEventArgs arguments )
	    {
		    ConsoleLogger.Error( $"Unhandled exception: {arguments.ExceptionObject}" );
		    Environment.Exit( -1 );
	    }

	    private static bool ParseArguments( string[] arguments )
	    {
		    bool result = true;
		    foreach( string argument in arguments )
		    {
			    switch( argument.ToLower() )
			    {
				    case "-v":
					    ConsoleLogger.VerboseLogs = true;
					    break;

				    case "-h":
					    ConsoleLogger.Log( "" );
					    ConsoleLogger.Log( "Usage: UTF16MustDIE.exe [-h] [-v] [-skiputf16] [-skipansi]" );
					    ConsoleLogger.Log( "" );
					    ConsoleLogger.Log( " -h - displays this help." );
					    ConsoleLogger.Log( " -v - displays verbose logging." );
					    ConsoleLogger.Log( " -skipUTF16 - does not process files of type UTF-16 in the depot." );
					    ConsoleLogger.Log( " -skipANSI - does not process files of type Unicode (ANSI files) in the depot." );
					    ConsoleLogger.Log( "" );
					    ConsoleLogger.Log( "Iterates over all files of type UTF-16 and Unicode in the local depot, fixes any broken UTF-16 files," );
					    ConsoleLogger.Log( "and converts all files to UTF-8 with the correct file type" );
					    ConsoleLogger.Log( "" );
					    result = false;
					    break;

				    case "-skiputf16":
					    ProcessUtf16 = false;
					    break;

				    case "-skipansi":
					    ProcessAnsi = false;
					    break;

				    default:
					    break;
			    }
		    }

		    return result;
	    }

		private static void FixUtf16Files( PerforceConnectionInfo connectionInfo )
	    {
		    IList<FileSpec> utf16_files = Perforce.GetFilesOfBaseFileType( connectionInfo, BaseFileType.UTF16 );
		    if( utf16_files.Count > 0 )
		    {
			    ConsoleLogger.Log( $"... found {utf16_files.Count} UTF-16 files in local workspace." );

			    ConsoleLogger.Log( $" .. syncing all UTF-16 files to #head." );
			    Perforce.SyncFiles( connectionInfo, utf16_files );

			    Perforce.ProcessDepotUtf16Files( connectionInfo, utf16_files );
		    }
		    else
		    {
			    ConsoleLogger.Log( $"No UTF-16 files found in depot." );
		    }

		    IList<FileSpec> pending_files = Perforce.GetPendingFilesOfBaseFileType( connectionInfo, BaseFileType.UTF16 );
		    if( pending_files.Count > 0 )
		    {
			    Perforce.ProcessPendingUtf16Files( connectionInfo, pending_files );
		    }
		    else
		    {
			    ConsoleLogger.Log( $"No UTF-16 files found in pending changelists." );
		    }
		}

	    private static void FixAnsiFiles( PerforceConnectionInfo connectionInfo )
	    {
		    IList<FileSpec> unicode_files = Perforce.GetFilesOfBaseFileType( connectionInfo, BaseFileType.Unicode );
		    if( unicode_files.Count > 0 )
		    {
			    ConsoleLogger.Log( $"... found {unicode_files.Count} of file type Unicode (ANSI) files in local workspace." );

			    ConsoleLogger.Log( $" .. syncing all ANSI files to #head." );
			    Perforce.SyncFiles( connectionInfo, unicode_files );

			    Perforce.ProcessDepotAnsiFiles( connectionInfo, unicode_files );
		    }
		    else
		    {
			    ConsoleLogger.Log( $"No ANSI files found in depot." );
		    }

		    IList<FileSpec> pending_files = Perforce.GetPendingFilesOfBaseFileType( connectionInfo, BaseFileType.Unicode );
		    if( pending_files.Count > 0 )
		    {
			    Perforce.ProcessPendingAnsiFiles( connectionInfo, pending_files );
		    }
		    else
		    {
			    ConsoleLogger.Log( $"No ANSI files found in pending changelists." );
		    }
		}

		/// <summary>
		/// The main control loop.
		/// The ReadMe.md file has the operation
		/// </summary>
		public static void Main( string[] arguments )
        {
	        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( GenericExceptionHandler );
	        DateTime start_time = DateTime.UtcNow;

	        ConsoleLogger.Title( "UTF16MustDie - Copyright 2024 Eternal Developments, LLC." );
	        ConsoleLogger.Title( "Hunts down UTF-16 and ANSI files in the local Perforce workspace, fixes them if necessary, then converts and resaves as UTF-8." );

	        if( ParseArguments( arguments ) )
	        {
		        PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
		        if( !PerforceUtilities.PerforceUtilities.Connect( connection_info ) )
		        {
			        ConsoleLogger.Error( $"... failed to connect to Perforce server {connection_info}." );
			        return;
		        }

		        ConsoleLogger.Log( $"... running from: {Directory.GetCurrentDirectory()} with Perforce connection info: {connection_info}" );

				string character_set_name = connection_info.PerforceRepository!.Connection.CharacterSetName;
				if( character_set_name != "utf8" )
		        {
			        ConsoleLogger.Warning( $"Connection character set name is not UTF8; the character encoding is '{character_set_name}' and this may not be able to translate all characters." );
		        }

		        bool unicode_enabled = connection_info.PerforceRepository!.Server.Metadata.UnicodeEnabled;
		        if( unicode_enabled )
		        {
			        ConsoleLogger.Log( "... server is unicode enabled; ANSI files will be processed." );
		        }

		        if( ProcessUtf16 )
		        {
			        FixUtf16Files( connection_info );
		        }

		        if( ProcessAnsi && unicode_enabled )
		        {
			        FixAnsiFiles( connection_info );
		        }

		        PerforceUtilities.PerforceUtilities.Disconnect( connection_info );

		        ConsoleLogger.Success( $"Completed conversion in {ConsoleLogger.TimeString( DateTime.UtcNow - start_time )}" );
	        }
        }
	}
}
