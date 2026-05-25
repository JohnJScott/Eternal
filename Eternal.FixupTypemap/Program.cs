// Copyright Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;
using System.Collections.Immutable;

namespace Eternal.FixupTypemap
{
	internal class FixupTypemap
	{
		/// <summary>Whether to check out files with mismatched types.</summary>
		private static bool CheckoutFiles = false;
		/// <summary>Whether to check for files without a type specified in the typemap.</summary>
		private static bool CheckForUntyped = false;

		/// <summary>
		/// Creates a new changelist with the description passed in.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="description">The changelist description.</param>
		/// <returns>The id of the changelist.</returns>
		private static int CreateChangelist( PerforceConnectionInfo connectionInfo, string description )
		{
			Repository repository = connectionInfo.PerforceRepository!;

			Changelist change = new Changelist
			{
				Description = description
			};

			change = repository.CreateChangelist( change );
			return change.Id;
		}

		/// <summary>
		/// Ensures that all files matching the specified type map entry in the Perforce repository have the correct file
		/// type, updating them as necessary.
		/// </summary>
		/// <remarks>Files whose types do not match the specified type map entry are checked out and updated to the
		/// correct type if file checkout is enabled. A new changelist is created if required. This method is intended for use
		/// in automated typemap consistency operations.</remarks>
		/// <param name="connectionInfo">The Perforce connection information used to access the repository and workspace.</param>
		/// <param name="typeMapEntry">The type map entry that defines the expected file type and path pattern for matching files.</param>
		private static void UpdateFileTypes( PerforceConnectionInfo connectionInfo, TypeMapEntry typeMapEntry )
		{
			// Convert depot path to stream depot path
			string depot_file_path = typeMapEntry.Path.Replace( "//", $"{connectionInfo.PerforceRepository!.Connection.Client.Stream}/" );

			// Get all the files matching the typemap entry with their file types
			FileSpec depot_path = new FileSpec( new DepotPath( depot_file_path ), null );
			GetFileMetaDataCmdOptions opts = new GetFileMetaDataCmdOptions( GetFileMetadataCmdFlags.None, "^headAction=delete ^headAction=move/delete", null, 0, null, null, null );
			IList<FileMetaData> file_meta_datas = connectionInfo.PerforceRepository!.GetFileMetaData( opts, depot_path );

			if( file_meta_datas != null )
			{
				ConsoleLogger.Log( $" .. found {file_meta_datas.Count} files matching typemap entry: {typeMapEntry.Path}." );

				// If any of the files don't match the typemap entry, checkout and update them to match the typemap entry
				int change_id = -1;
				Client client = connectionInfo.GetWorkspace()!;

				foreach( FileMetaData file_meta_data in file_meta_datas )
				{
					if( !file_meta_data.Type.Equals( typeMapEntry.FileType ) )
					{
						ConsoleLogger.Log( $" .... type mismatch for {file_meta_data.DepotPath} - is '{file_meta_data.Type}' should be '{typeMapEntry.FileType}'" );

						if( CheckoutFiles )
						{
							// If changelist null, create new one
							if( change_id < 0 )
							{
								change_id = CreateChangelist( connectionInfo, $"FixupTypemap - updating the file types of '{typeMapEntry.Path}' to '{typeMapEntry.FileType}' to match the type map entry" );
							}

							// Add this file with new file type to the changelist
							client.EditFiles( new EditCmdOptions( EditFilesCmdFlags.None, change_id, typeMapEntry.FileType ), file_meta_data.DepotPath );
						}
					}
				}
			}
		}

		/// <summary>
		/// Identifies files in the specified Perforce stream that do not match any entry in the provided type map.
		/// </summary>
		/// <remarks>Files that are not matched by any entry in the type map are considered untyped and are logged for
		/// review. Only files that are not deleted or moved/deleted are considered.</remarks>
		/// <param name="connectionInfo">The connection information used to access the Perforce repository and stream.</param>
		/// <param name="typeMap">A list of type map entries that define the file types to match against files in the stream.</param>
		private static void FindUntypedFiles( PerforceConnectionInfo connectionInfo, IList<TypeMapEntry> typeMap )
		{
			string stream = connectionInfo.PerforceRepository!.Connection.Client.Stream;
			GetFileMetaDataCmdOptions opts = new GetFileMetaDataCmdOptions( GetFileMetadataCmdFlags.None, "^headAction=delete ^headAction=move/delete", null, 0, null, null, null );

			List<FileSpec> depot_paths = [ new FileSpec( new DepotPath( $"{stream}/..." ) ) ];

			IEnumerable<FileSpec> exclude_paths = typeMap.Select( x => new FileSpec( new DepotPath( "-" + x.Path ) ) );
			depot_paths.AddRange( exclude_paths );

			IList <FileMetaData> filtered_files = connectionInfo.PerforceRepository!.GetFileMetaData( opts, depot_paths.ToArray() );
			ConsoleLogger.Log( $" .. found {filtered_files.Count} files in the depot." );

			foreach( FileMetaData file_meta_data in filtered_files )
			{
				ConsoleLogger.Log( $" .... untyped file: '{file_meta_data.DepotPath}' with type '{file_meta_data.Type}'" );
			}

			ConsoleLogger.Error( $" .. the filtering code does not seem to respect the exclude patterns." );
		}

		/// <summary>
		/// Validates the command-line arguments and updates the type map entry reference if applicable.
		/// </summary>
		/// <remarks>The method sets internal flags based on the operation mode specified in the arguments. If the
		/// arguments are invalid or a required type map entry is not found, error messages are logged and false is
		/// returned.</remarks>
		/// <param name="args">The array of command-line arguments to validate. The first argument specifies the operation mode ('report', 'fix',
		/// or 'untyped'). The second argument, if present, specifies the type map path to check.</param>
		/// <param name="typeMap">The collection of available type map entries used to resolve the specified path argument.</param>
		/// <param name="typeEntry">When the second argument is provided and matches an entry in the type map, this reference is set to the
		/// corresponding entry; otherwise, it is set to null.</param>
		/// <returns>true if the arguments are valid and, if applicable, a matching type map entry is found; otherwise, false.</returns>
		private static bool ValidateArguments( string[] args, IList<TypeMapEntry> typeMap, ref TypeMapEntry? typeEntry )
		{
			if( args.Length < 1 )
			{
				ConsoleLogger.Error( "Not enough arguments provided." );
				ConsoleLogger.Log( "Usage: Eternal.FixupTypemap [report | fix | untyped] <type to check>" );
				ConsoleLogger.Log( " .. 'report' - just reports any mismatches between the typemap and the file types in Perforce." );
				ConsoleLogger.Log( " .. 'fix' - checks out and updates the type of any files that don't match the file type in the typemap" );
				ConsoleLogger.Log( " .. 'untyped' - reports any files that don't have a matching path in the typemap" );
				ConsoleLogger.Log( " .. e.g. Eternal.FixupTypemap report //....cpp" );
				return false;
			}

			switch( args[0].ToLower() )
			{
				case "report":
					CheckoutFiles = false;
					break;

				case "fix":
					CheckoutFiles = true;
					break;

				case "untyped":
					CheckForUntyped = true;
					break;

				default:
					return false;
			}

			if( args.Length > 1 )
			{
				typeEntry = typeMap.FirstOrDefault( x => x.Path.Equals( args[1], StringComparison.OrdinalIgnoreCase ) );
				if( typeEntry == null )
				{
					ConsoleLogger.Error( $"No typemap entry found for {args[1]}." );
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Retrieves the list of type map entries from the specified Perforce connection.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection information used to access the repository and retrieve type map entries. Cannot be null.</param>
		/// <returns>A list of type map entries defined in the Perforce repository. The list will be empty if no entries are defined.</returns>
		private static IList<TypeMapEntry> GetTypeMapEntries( PerforceConnectionInfo connectionInfo )
		{
			IList<TypeMapEntry> type_map = connectionInfo.PerforceRepository!.GetTypeMap();
			ConsoleLogger.Log( $" .. found {type_map.Count} typemap entries." );

			IEnumerable<string> duplicates = type_map
				.GroupBy( x => x.Path )
				.Where( g => g.Count() > 1 )
				.Select( g => g.Key );

			foreach( string type_map_entry in duplicates )
			{
				ConsoleLogger.Warning( $"Depot path '{type_map_entry}' found more than once in the typemap" );
			}

			return type_map;
		}

		/// <summary>A generic catch all for all unhandled exceptions.</summary>
		/// <param name="sender">The object that created the exception.</param>
		/// <param name="arguments">Details about the exception.</param>
		private static void GenericExceptionHandler( object sender, UnhandledExceptionEventArgs arguments )
		{
			ConsoleLogger.Error( $"Unhandled exception: {arguments.ExceptionObject}" );
			Environment.Exit( -1 );
		}

		static void Main( string[] args )
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( GenericExceptionHandler );
			DateTime start_time = DateTime.UtcNow;

			ConsoleLogger.Title( "FixupTypemap - Copyright Eternal Developments LLC." );
			ConsoleLogger.Title( "Checks out and changes the Perforce file type of any file that doesn't match the type in the typemap." );

			// Get the root of the current branch
			PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
			if( !PerforceUtilities.PerforceUtilities.Connect( connection_info ) )
			{
				ConsoleLogger.Error( $"... failed to connect to Perforce server {connection_info}." );
				return;
			}

			if( connection_info.WorkspaceRoot == String.Empty )
			{
				ConsoleLogger.Error( $"... no workspace root containing {Directory.GetCurrentDirectory()} found on {connection_info}." );
				return;
			}

			ConsoleLogger.Log( $" .. running from: {Directory.GetCurrentDirectory()} with client root: {connection_info.WorkspaceRoot}" );

			IList<TypeMapEntry> type_map = GetTypeMapEntries( connection_info );

			TypeMapEntry? type_entry = null;
			if( !ValidateArguments( args, type_map, ref type_entry ) )
			{
				return;
			}

			if( CheckForUntyped )
			{
				// Check for files that don't have a typemap entry
				FindUntypedFiles( connection_info, type_map );
			}
			else
			{
				// Apply to single typemap entry
				if( type_entry != null )
				{
					UpdateFileTypes( connection_info, type_entry );
				}
				else
				{
					// Apply to all typemap entries
					foreach( TypeMapEntry type_map_entry in type_map )
					{
						UpdateFileTypes( connection_info, type_map_entry );
					}
				}
			}

			PerforceUtilities.PerforceUtilities.Disconnect( connection_info );
			ConsoleLogger.Success( $"Completed in {ConsoleLogger.TimeString( DateTime.UtcNow - start_time )}." );
		}
	}
}
