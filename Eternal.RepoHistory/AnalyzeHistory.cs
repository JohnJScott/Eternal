// Copyright Eternal Developments, LLC. All Rights Reserved.

using Eternal.PerforceUtilities;
using Perforce.P4;
using Eternal.ConsoleUtilities;
using File = Perforce.P4.File;

namespace RepoHistory
{
	internal class AnalyzeHistory
	{
		public static readonly HashSet<string> FoundRepos = new HashSet<string>();

		private static int LastCount = 500;

		private static void RecurseGetDepotFolders( Repository repository, string parentDirectory, ref IDictionary<string, int> directoryStructure )
		{
			directoryStructure.Add( parentDirectory, 0 );

			IList<string> sub_directories = repository.GetDepotDirs( null, parentDirectory + "/*" );
			foreach( string sub_directory in sub_directories )
			{
				RecurseGetDepotFolders( repository, sub_directory, ref directoryStructure );
			}

			if( directoryStructure.Count > LastCount )
			{
				ConsoleLogger.Log( $" .... found {directoryStructure.Count} directories" );
				LastCount = directoryStructure.Count + 500;
			}
		}

		private static IDictionary<string, int> GetDepotDirectoryStructure( PerforceConnectionInfo connectionInfo )
		{
			IDictionary<string, int> directory_structure = new Dictionary<string, int>();

			Repository repository = connectionInfo.PerforceRepository!;
			LastCount = 500;
			RecurseGetDepotFolders( repository, connectionInfo.WorkspaceRoot, ref directory_structure );

			return directory_structure;
		}

		private static IList<File> GetFilesInFolder( PerforceConnectionInfo connectionInfo, string folder )
		{
			Repository repository = connectionInfo.PerforceRepository!;

			GetDepotFilesCmdOptions opts = new GetDepotFilesCmdOptions( GetDepotFilesCmdFlags.NotDeleted, 0 );
			FileSpec local_file_spec = new FileSpec( new DepotPath( folder + "/*" ), null );
			IList<File> local_files = repository.GetFiles( opts, local_file_spec );

			return local_files;
		}

		private static string UnescapeDepotPath( string escapedPath )
		{
			string unescaped_path = escapedPath
				.Replace( "%40", "@" )
				.Replace( "%23", "#" )
				.Replace( "%2A", "*" )
				.Replace( "%25", "%" );

			return unescaped_path;
		}

		public static void AnalyzeFileHistory( PerforceConnectionInfo connectionInfo )
		{
			int total_file_count = 0;
			Repository repository = connectionInfo.PerforceRepository!;

			DateTime start = DateTime.UtcNow;
			IDictionary<string, int> directory_structure = GetDepotDirectoryStructure( connectionInfo );
			ConsoleLogger.Log( $" .. found {directory_structure.Count} directories in {ConsoleLogger.TimeString( DateTime.UtcNow - start )}" );

			start = DateTime.UtcNow;

			foreach( string directory_name in directory_structure.Keys )
			{
				IList<File>? files = GetFilesInFolder( connectionInfo, directory_name );
				if( files?.Count > 0 )
				{
					total_file_count += files.Count;
					ConsoleLogger.Log( $" .. processing {files.Count} files in {directory_name}" );

					foreach( File file in files )
					{
						ConsoleLogger.Verbose( $" .... processing {file.DepotPath}" );

						GetFileHistoryCmdOptions opts = new GetFileHistoryCmdOptions( GetFileHistoryCmdFlags.IncludeInherited, 0, 0 );
						FileSpec history_file_spec = new FileSpec( new DepotPath( UnescapeDepotPath( file.DepotPath.Path ) ), null );
						IList<FileHistory> file_histories = repository.GetFileHistory( opts, history_file_spec );
						if( file_histories != null )
						{
							string last_repo = "";
							string full_repo = "";
							foreach( FileHistory history in file_histories.Reverse() )
							{
								string depot_path = history.DepotPath.Path;
								string repo = depot_path.Substring( 2, depot_path.IndexOf( '/', 2 ) - 2 );

								if( last_repo != repo )
								{
									if( last_repo.Length > 0 )
									{
										full_repo += " -> ";
									}

									full_repo += repo;
									last_repo = repo;
								}
							}

							if( FoundRepos.Add( full_repo ) )
							{
								ConsoleLogger.Log( $"Found a new repo called {full_repo}" );
							}
						}
						else
						{
							ConsoleLogger.Warning( $"No history for file '{file}'" );
						}
					}
				}
			}

			ConsoleLogger.Log( $"Checked {total_file_count:n0} files in {directory_structure.Count} directories in {ConsoleLogger.TimeString( DateTime.UtcNow - start )}" );
		}
	}
}
