// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;

namespace Eternal.UTF16MustDIE
{
	/// <summary>The main flow control.</summary>
    public class Program
	{
	    /// <summary>A generic catch all for all unhandled exceptions.</summary>
	    /// <param name="sender">The object that created the exception.</param>
	    /// <param name="arguments">Details about the exception.</param>
	    private static void GenericExceptionHandler( object sender, UnhandledExceptionEventArgs arguments )
	    {
		    ConsoleLogger.Error( $"Unhandled exception: {arguments.ExceptionObject}" );
		    Environment.Exit( -1 );
	    }

		/// <summary>
		/// The main control loop.
		/// 1. Attempts to find a Perforce workspace using P4PORT and the current working folder
		/// 2. Finds and syncs all UTF-16 files in the workspace
		/// 3. Checks the UTF-16 files for validity and fixes if required.
		/// 4. Converts all the files to UTF-8 and adds to a pending changelist.
		/// 5. Iterates over all pending changelists collecting all the UTF-16 files
		/// 6. Checks the UTF-16 files for validity and fixes if required.
		/// 7. Converts all the files to UTF-8 and adds to a different pending changelist.
		/// </summary>
		public static void Main()
        {
	        AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( GenericExceptionHandler );
	        DateTime start_time = DateTime.UtcNow;

	        ConsoleLogger.Title( "UTF16MustDie - Copyright 2022 Eternal Developments LLC." );
	        ConsoleLogger.Title( "Hunts down UTF-16 files in the local Perforce workspace, fixes them if necessary, then converts and resaves as UTF-8." );

	        PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
	        if( !PerforceUtilities.PerforceUtilities.Connect( connection_info ) )
	        {
		        ConsoleLogger.Error( $"... failed to connect to Perforce server {connection_info}." );
		        return;
	        }

			ConsoleLogger.Log( $"... running from: {Directory.GetCurrentDirectory()} with Perforce connection info: {connection_info}" );

	        IList<FileSpec> utf16_files = Perforce.GetUTF16Files( connection_info );
	        if( utf16_files.Count > 0 )
	        {
		        ConsoleLogger.Log( $"... found {utf16_files.Count} UTF-16 files in local workspace." );

		        ConsoleLogger.Log( $" .. syncing all UTF-16 files to #head." );
		        Perforce.SyncUTF16Files( connection_info, utf16_files );

		        int change_id = Perforce.ProcessDepotUTF16Files( connection_info, utf16_files );
		        ConsoleLogger.Log( $"Decorrupted and updated to UTF-8 {utf16_files.Count} files and added to change {change_id}" );
	        }
	        else
	        {
		        ConsoleLogger.Log( $"No UTF-16 files found in depot." );
			}

			IList<FileSpec> pending_files = Perforce.GetUTF16PendingFiles( connection_info );
	        if( pending_files.Count > 0 )
	        {
		        ConsoleLogger.Log( $"... found {pending_files.Count} UTF-16 files in pending change(s)." );
		        int change_id = Perforce.ProcessPendingUTF16Files( connection_info, pending_files );
		        ConsoleLogger.Log( $"Decorrupted and updated to UTF-8 {pending_files.Count} files and added to change {change_id}" );
	        }
	        else
	        {
		        ConsoleLogger.Log( $"No UTF-16 files found in pending changelists." );
	        }

			PerforceUtilities.PerforceUtilities.Disconnect( connection_info );

			ConsoleLogger.Success( $"Completed conversion in {( DateTime.UtcNow - start_time ).TotalSeconds} seconds" );
        }
	}
}