// Copyright Eternal Developments, LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;

namespace RepoHistory
{
	internal class RepoHistory
	{
		static void Main( string[] args )
		{
			DateTime start_time = DateTime.UtcNow;

			ConsoleLogger.Title( "RepoHistory - Copyright Eternal Developments, LLC. All Rights Reserved." );
			ConsoleLogger.Title( "This iterates over the history of every file to locate the used repos. This takes a long time." );

			PerforceConnectionInfo connection_info = PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
			if( !PerforceUtilities.Connect( connection_info ) )
			{
				ConsoleLogger.Error( $"... failed to connect to Perforce server {connection_info}." );
				return;
			}

			ConsoleLogger.Log( $"... running from: {Directory.GetCurrentDirectory()} with Perforce connection info: {connection_info}" );
			AnalyzeHistory.AnalyzeFileHistory( connection_info );

			PerforceUtilities.Disconnect( connection_info );

			ConsoleLogger.Log( $"Found {AnalyzeHistory.FoundRepos.Count} active repos" );
			foreach( string repo in AnalyzeHistory.FoundRepos )
			{
				ConsoleLogger.Log( $" .. {repo}" );
			}

			ConsoleLogger.Success( $"Completed check in {ConsoleLogger.TimeString( DateTime.UtcNow - start_time )}" );
		}
	}
}
