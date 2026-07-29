// Copyright Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;

// [Flags]
// public enum BaseFileType
// {
// 	Unspecified = 0,
// 	Text = 1,
// 	Binary = 2,
// 	Symlink = 4,
// 	Apple = 8,
// 	Resource = 0x10,
// 	Unicode = 0x20,
// 	UTF16 = 0x40,
// 	UTF8 = 0x80
// }

// [Flags]
// public enum FileTypeModifier
// {
// 	None = 0,
// 	ModTime = 1,
// 	Writable = 2,
// 	Exec = 4,
// 	KeywordsAll = 8,
// 	KeywordsLimited = 0x10,
// 	ExclusiveOpen = 0x20,
// 	CompressedFiles = 0x40,
// 	RCSDeltaFiles = 0x80,
// 	FullRevisions = 0x100,
// 	HeadrevOnly = 0x200,
// 	NRevsOnly = 0x400,
// 	ArchiveTrigger = 0x800
// }

namespace Eternal.FixupTypemap
{
	public class FixupTypemap
	{
		public enum FileTypeChangeSafety
		{
			Unknown = -1,
			Safe,
			Unsafe,
			Warning,
			Invalid
		}

		/// <summary>Whether to check out files with mismatched types.</summary>
		private static bool CheckoutFiles = false;
		/// <summary>Whether to check for files without a type specified in the typemap.</summary>
		private static bool CheckForUntyped = false;

		public static FileTypeChangeSafety CheckChange( FileType source, FileType destination )
		{
			FileTypeChangeSafety result = FileTypeChangeSafety.Unknown;

			// Changing the file type modifier (e.g. +w) is safe
			if( source.BaseType == destination.BaseType )
			{
				result = FileTypeChangeSafety.Safe;
			}
			else
			{
				switch( source.BaseType )
				{
					case BaseFileType.Text:
						{
							switch( destination.BaseType )
							{
								case BaseFileType.Binary:
									result = FileTypeChangeSafety.Safe;
									break;

								case BaseFileType.Symlink:
								case BaseFileType.Apple:
								case BaseFileType.Resource:
									result = FileTypeChangeSafety.Warning;
									break;

								case BaseFileType.UTF16:
									result = FileTypeChangeSafety.Invalid;
									break;

								case BaseFileType.Unicode:
								case BaseFileType.UTF8:
									result = FileTypeChangeSafety.Unsafe;
									break;
							}
						}
					break;

					case BaseFileType.Binary:
						{
							switch( destination.BaseType )
							{
								case BaseFileType.Text:
								case BaseFileType.UTF16:
								case BaseFileType.Unicode:
								case BaseFileType.UTF8:
									result = FileTypeChangeSafety.Invalid;
									break;

								case BaseFileType.Symlink:
								case BaseFileType.Apple:
								case BaseFileType.Resource:
									result = FileTypeChangeSafety.Warning;
									break;
							}
						}
					break;

					case BaseFileType.Symlink:
					case BaseFileType.Apple:
					case BaseFileType.Resource:
						{
							result = FileTypeChangeSafety.Warning;
						}
						break;

					case BaseFileType.UTF16:
						{
							switch( destination.BaseType )
							{
								case BaseFileType.Binary:
									result = FileTypeChangeSafety.Safe;
									break;

								case BaseFileType.Text:
								case BaseFileType.Unicode:
								case BaseFileType.UTF8:
									result = FileTypeChangeSafety.Invalid;
									break;

								case BaseFileType.Symlink:
								case BaseFileType.Apple:
								case BaseFileType.Resource:
									result = FileTypeChangeSafety.Warning;
									break;
							}
						}
						break;

					case BaseFileType.UTF8:
					case BaseFileType.Unicode:
					{
						switch( destination.BaseType )
						{
							case BaseFileType.Binary:
								result = FileTypeChangeSafety.Safe;
								break;

							case BaseFileType.Text:
							case BaseFileType.Unicode:
							case BaseFileType.UTF8:
								result = FileTypeChangeSafety.Unsafe;
								break;

							case BaseFileType.Symlink:
							case BaseFileType.Apple:
							case BaseFileType.Resource:
								result = FileTypeChangeSafety.Warning;
								break;

							case BaseFileType.UTF16:
								result = FileTypeChangeSafety.Invalid;
								break;
						}
						}
					break;
				}
			}

			return result;
		}

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
			IList<FileMetaData> files_meta_data = connectionInfo.PerforceRepository!.GetFileMetaData( opts, depot_path );

			if( files_meta_data != null )
			{
				ConsoleLogger.Log( $" .. found {files_meta_data.Count} files matching typemap entry: '{typeMapEntry.Path}'." );

				// If any of the files don't match the typemap entry, checkout and update them to match the typemap entry
				int[] change_id = [ -1, -1, -1, -1 ];
				Client client = connectionInfo.GetWorkspace()!;

				foreach( FileMetaData file_meta_data in files_meta_data )
				{
					if( !file_meta_data.Type.Equals( typeMapEntry.FileType ) )
					{
						FileTypeChangeSafety safety = CheckChange( typeMapEntry.FileType, file_meta_data.Type );
						ConsoleLogger.Log( $" .... type mismatch for '{file_meta_data.DepotPath}' - is '{file_meta_data.Type}' should be '{typeMapEntry.FileType}' - conversion '{safety}'." );

						if( CheckoutFiles )
						{
							// If changelist null, create new one
							if( change_id[( int )safety] < 0 )
							{
								change_id[( int )safety] = CreateChangelist( connectionInfo, $"FixupTypemap (Conversion: {safety}) - updating the file types of '{typeMapEntry.Path}' to '{typeMapEntry.FileType}' to match the type map entry" );
							}

							// Add this file with new file type to the changelist
							client.EditFiles( new EditCmdOptions( EditFilesCmdFlags.None, change_id[( int )safety], typeMapEntry.FileType ), file_meta_data.DepotPath );
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
			string temp_folder = Path.GetTempPath();
			string stream = connectionInfo.PerforceRepository!.Connection.Client.Stream;
			string client_name = "temp-exclusion-client";

			Client client = new Client
			{
				Name = client_name,
				OwnerName = "john.scott",
				Root = Path.Combine( temp_folder, client_name ),
				ViewMap = []
			};

			client.ViewMap.Add( new MapEntry(
				MapType.Include,
				new DepotPath( $"{stream}/..." ),
				new ClientPath( $"//{client_name}/..." ) ) );

			foreach( TypeMapEntry type_map_entry in typeMap )
			{
				string depot_path = type_map_entry.Path;
				if( depot_path.StartsWith( "//..." ) )
				{
					depot_path = depot_path.Replace( "//", $"{stream}/" );
					string client_path = type_map_entry.Path.Insert( 2, $"{client_name}/" );

					client.ViewMap.Add( new MapEntry(
						MapType.Exclude,
						new DepotPath( depot_path ),
						new ClientPath( client_path ) ) );
				}
				else
				{
					ConsoleLogger.Warning( $"Unprocessed typemap entry '{type_map_entry}'" );
				}
			}

			connectionInfo.PerforceRepository.CreateClient( client );
			connectionInfo.PerforceRepository.Connection.SetClient( client_name );

			// Must use CLIENT syntax here, not depot syntax, for the exclusion to take effect.
			FileSpec spec = new FileSpec( new ClientPath( $"//{client_name}/..." ), VersionSpec.Head );
			GetDepotFilesCmdOptions options = new GetDepotFilesCmdOptions( GetDepotFilesCmdFlags.NotDeleted, -1 );
			IList<FileSpec>? filtered_files = connectionInfo.PerforceRepository.GetDepotFiles( new List<FileSpec> { spec }, options );

			// Delete the temporary client
			connectionInfo.PerforceRepository.DeleteClient( client, null );

			Dictionary<string, int> extensions = new Dictionary<string, int>();
			ConsoleLogger.Log( $" .. found {filtered_files.Count} file names without a typemap entry in the stream '{stream}'." );

			foreach( FileMetaData file_meta_data in filtered_files )
			{
				ConsoleLogger.Log( $" .... file name with no typemap entry: '{file_meta_data.DepotPath}'" );
				string extension = Path.GetExtension( file_meta_data.DepotPath.Path ).ToLower();
				if( !string.IsNullOrEmpty( extension ) )
				{
					if( !extensions.TryAdd( extension, 1 ) )
					{
						extensions[extension]++;
					}
				}
			}

			ConsoleLogger.Log( $" .. found {extensions.Count} file extensions without a typemap entry in the stream '{stream}'." );

			List<KeyValuePair<string, int>> sorted_descending = extensions
				.OrderByDescending( pair => pair.Value )
				.ToList();

			foreach( KeyValuePair<string, int> pair in sorted_descending )
			{
				ConsoleLogger.Log( $" .... found {pair.Value} files with a file extension not in the typemap for extension: '{pair.Key}'" );
			}
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
				ConsoleLogger.Log( "Usage: Eternal.FixupTypemap [report | fix | untyped] <type to check> [-v]" );
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
					ConsoleLogger.Error( $"No typemap entry found for '{args[1]}'." );
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

		/// <summary>A generic catch-all for all unhandled exceptions.</summary>
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
