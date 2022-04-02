// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using Eternal.EternalUtilities;
using Perforce.P4;

namespace Eternal.SourceServerIndexer
{
	/// <summary>Class to handle interaction with Perforce.</summary>
	public class Perforce
	{
		/// <summary>The Perforce repository.</summary>
		private static Repository PerforceRepository = null;

		/// <summary>Connect to a Perforce repository.</summary>
		private static void Connect()
		{
			try
			{
				Server PerforceServer = new Server( new ServerAddress(Properties.Settings.Default.PerforceServerName + ":" + Properties.Settings.Default.PerforceServerPort ) );
				PerforceRepository = new Repository( PerforceServer );
				PerforceRepository.Connection.Connect( null );
			}
			catch( Exception Ex )
			{
				PerforceRepository = null;
				ConsoleLogger.Error( "Failed to connect to Perforce server: " + Properties.Settings.Default.PerforceServerName + ":" + Properties.Settings.Default.PerforceServerPort );
				ConsoleLogger.Error( "Failed to connect to Perforce server with exception: " + Ex.Message );
			}
		}

		/// <summary>Disconnect from a Perforce repository.</summary>
		private static void Disconnect()
		{
			if( PerforceRepository != null )
			{
				PerforceRepository.Connection.Disconnect();
				PerforceRepository = null;
			}
		}

		/// <summary>Get the actual #have revisions for a list of source files.</summary>
		/// <param name="SourceFiles">List of local source files referencd by the symbol file.</param>
		/// <returns>A list of file specifications which includes the local path, depot path, and revision number.</returns>
		/// <remarks>'Local source files' are defined as in the current directory or lower. This excludes system source files (such as for the CRT), that are unlikely to be source controlled.</remarks>
		public static List<FileSpec> GetHaveRevisions( List<string> SourceFiles )
		{
			List<FileSpec> VersionedFileSpecs = new List<FileSpec>();

			Connect();

			if( PerforceRepository != null )
			{
				// Number of files to send to send to the Perforce interface at once.
				const int ChunkSize = 100;

				// Set the version to #have for all files
				List<FileSpec> FileSpecs = FileSpec.LocalSpecList( SourceFiles ).ToList();
				List<FileSpec> FullFileSpecs = new List<FileSpec>();

				for( int Chunk = 0; Chunk < FileSpecs.Count; Chunk += ChunkSize )
				{
					// Get the local and depot versions of the path (does not support version)
					IEnumerable<FileSpec> ListChunk = FileSpecs.Skip( Chunk ).Take( ChunkSize );
					FullFileSpecs.AddRange( PerforceRepository.Connection.Client.GetClientFileMappings( ListChunk.ToArray() ) );
				}

				// Convert the #have to an actual revision number (may not find some entries) but lose the local and client paths
				FullFileSpecs.ForEach( x => x.Version = new HaveRevision() );
				List<File> FileDescriptions = new List<File>();
				for( int Chunk = 0; Chunk < FileSpecs.Count; Chunk += ChunkSize )
				{
					IEnumerable<FileSpec> ListChunk = FullFileSpecs.Skip( Chunk ).Take( ChunkSize );
					FileDescriptions.AddRange( PerforceRepository.GetFiles( ListChunk.ToArray(), null ) );
				}

				ConsoleLogger.Log( "... found " + FileDescriptions.Count + " files in Perforce." );

				// Create a dictionary of depot paths for fast lookup
				Dictionary<string, FileSpec> FileSpecDictionary = FullFileSpecs.ToDictionary( x => x.DepotPath.Path.ToLower(), y => y );

				// Combine all the results into the resulting list
				FileDescriptions.ForEach( x => VersionedFileSpecs.Add( new FileSpec( x ) ) );
				VersionedFileSpecs.ForEach( x => x.LocalPath = FileSpecDictionary[x.DepotPath.Path.ToLower()].LocalPath );

				Disconnect();
			}

			return VersionedFileSpecs;
		}
	}
}
