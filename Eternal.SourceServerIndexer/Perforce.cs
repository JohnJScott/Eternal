// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;

using File = Perforce.P4.File;

namespace Eternal.SourceServerIndexer
{
	/// <summary>Class to handle interaction with Perforce.</summary>
	public class Perforce
	{
		/// <summary>Get the actual have revisions for a list of source files.</summary>
		/// <param name="connectionInfo">The Perforce connection details.</param>
		/// <param name="sourceFiles">List of local source files referenced by the symbol file.</param>
		/// <returns>A list of file specifications which includes the local path, depot path, and revision number.</returns>
		/// <remarks>'Local source files' are defined as in the current directory or lower. This excludes system source files (such as for the CRT), that are unlikely to be source controlled.</remarks>
		public static List<FileSpec> GetHaveRevisions( PerforceConnectionInfo connectionInfo, List<string> sourceFiles )
		{
			List<FileSpec> versioned_file_specs = new List<FileSpec>();

			Repository repository = connectionInfo.PerforceRepository!;
			Connection connection = repository.Connection;

			// Number of files to send to send to the Perforce interface at once.
			const int chunk_size = 100;

			// Set the version to #have for all files
			List<FileSpec> file_specs = FileSpec.LocalSpecList( sourceFiles ).ToList();
			List<FileSpec> full_file_specs = new List<FileSpec>();

			for( int chunk = 0; chunk < file_specs.Count; chunk += chunk_size )
			{
				// Get the local and depot versions of the path (does not support version)
				IEnumerable<FileSpec> list_chunk = file_specs.Skip( chunk ).Take( chunk_size );
				full_file_specs.AddRange( connection.Client.GetClientFileMappings( list_chunk.ToArray() ) );
			}

			// Convert the #have to an actual revision number (may not find some entries) but lose the local and client paths
			full_file_specs.ForEach( x => x.Version = new HaveRevision() );

			List<File> file_descriptions = new List<File>();
			for( int chunk = 0; chunk < file_specs.Count; chunk += chunk_size )
			{
				IEnumerable<FileSpec> list_chunk = full_file_specs.Skip( chunk ).Take( chunk_size );
				IEnumerable<File> file_list_chunks = repository.GetFiles( list_chunk.ToArray(), null );
				if( file_list_chunks != null )
				{
					file_descriptions.AddRange( file_list_chunks );
				}
			}

			ConsoleLogger.Log( $"... found {file_descriptions.Count} files in Perforce." );

			// full_file_specs have the DepotPath and LocalPath but the #have version
			// file_descriptions is a subset of files in full_file_specs have the DepotPath and version but not the LocalPath

			// Create a dictionary of depot paths to container with the LocalPath for fast lookup
			Dictionary<string, FileSpec> file_spec_dictionary = new Dictionary<string, FileSpec>();
			foreach( FileSpec file_spec in full_file_specs )
			{
				file_spec_dictionary.TryAdd( file_spec.DepotPath.Path.ToLower(), file_spec );
			}

			// Combine all the results into the resulting list
			file_descriptions.ForEach( x => versioned_file_specs.Add( new FileSpec( x ) ) );
			versioned_file_specs.ForEach( x => x.LocalPath = file_spec_dictionary[x.DepotPath.Path.ToLower()].LocalPath );

			return versioned_file_specs;
		}
	}
}