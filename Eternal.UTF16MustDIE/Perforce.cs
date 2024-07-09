// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using System.Text;
using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;
using UtfUnknown;
using File = Perforce.P4.File;

namespace Eternal.Utf16MustDie
{
	/// <summary>Class to handle interaction with Perforce.</summary>
	public class Perforce
	{
		/// <summary>
		/// Syncs all files in the list of FileSpecs to head.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="fileSpecList">The container of files to sync.</param>
		public static void SyncFiles( PerforceConnectionInfo connectionInfo, IList<FileSpec> fileSpecList )
		{
			Client client = connectionInfo.GetWorkspace()!;

			IList<FileSpec> unversioned = fileSpecList.ToList().Select( x => x.StripVersion() ).ToList();
			client.RevertFiles( unversioned, null );
			client.SyncFiles( unversioned, null );
		}

		/// <summary>Find all the files of the file type baseFileType in the current workspace.</summary>
		/// <param name="connectionInfo">The information about the current Perforce connection.</param>
		/// <param name="baseFileType">The Perforce file type we wish to retrieve.</param>
		/// <returns>A list of file specifications of all files in the local workspace that have the base type of baseFileType.</returns>
		public static IList<FileSpec> GetFilesOfBaseFileType( PerforceConnectionInfo connectionInfo, BaseFileType baseFileType )
		{
			Repository repository = connectionInfo.PerforceRepository!;

			GetDepotFilesCmdOptions opts = new GetDepotFilesCmdOptions( GetDepotFilesCmdFlags.NotDeleted, 0 );
			FileSpec local_file_spec = new FileSpec( new ClientPath( connectionInfo.WorkspaceRoot + "/..." ), null );
			IList<File> local_files = repository.GetFiles( opts, local_file_spec );

			IList<FileSpec> utf16_file_specs = local_files.Where( x => x.Type.BaseType == baseFileType ).Select( x => ( FileSpec )x ).ToList();

			return utf16_file_specs;
		}

		/// <summary>
		/// Gets all the files in pending changelists of file type baseFileType belonging to the current workspace.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="baseFileType">The Perforce file type we wish to retrieve.</param>
		/// <returns>A list of files in pending changelists of the file type baseFileType. Does not include any in the default changelist.</returns>
		public static IList<FileSpec> GetPendingFilesOfBaseFileType( PerforceConnectionInfo connectionInfo, BaseFileType baseFileType )
		{
			IList<FileSpec> pending_file_specs = new List<FileSpec>();

			Repository repository = connectionInfo.PerforceRepository!;
			ChangesCmdOptions changes_cmd_options = new ChangesCmdOptions( ChangesCmdFlags.None, connectionInfo.Workspace, Int32.MaxValue, ChangeListStatus.Pending, connectionInfo.User, 0 );
			FileSpec local_file_spec = new FileSpec( new ClientPath( connectionInfo.WorkspaceRoot + "/..." ), null );
			IList<Changelist> changelists = repository.GetChangelists( changes_cmd_options, local_file_spec );
			if( changelists != null )
			{
				ConsoleLogger.Log( $" .. found {changelists.Count} pending changes (not including default)" );

				foreach( Changelist changelist in changelists )
				{
					DescribeCmdOptions describe_options = new DescribeCmdOptions( DescribeChangelistCmdFlags.None, 0, 0 );
					Changelist full_changelist = repository.GetChangelist( changelist.Id, describe_options );
					foreach( FileMetaData file_meta_data in full_changelist.Files )
					{
						if( file_meta_data.Type.BaseType == baseFileType )
						{
							pending_file_specs.Add( file_meta_data );
						}
					}
				}
			}
			else
			{
				ConsoleLogger.Log( $" .. no pending changes found" );
			}

			return pending_file_specs;
		}

		private static bool SkipZeroChar( BinaryReader reader )
		{
			int test = reader.PeekChar();
			if( test == 0x00 )
			{
				if( reader.BaseStream.Position < reader.BaseStream.Length )
				{
					reader.ReadByte();
				}

				return true;
			}

			return false;
		}

		private static UInt16 CheckCharacter( BinaryReader reader )
		{
			UInt16 character = reader.ReadUInt16();
		
			if( character == 0x0a0d )
			{
				if( SkipZeroChar( reader ) )
				{
					character = 0x000a;
				}
			}
			else if( character == 0x0d0d )
			{
				if( SkipZeroChar( reader ) )
				{
					character = 0x0000;
				}
			}

			return character;
		}

		// private static bool ValidateAsAscii( Repository repository, FileSpec fileSpec )
		// {
		// 	FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
		// 	string file_name = file_meta_data.ClientPath.Path;
		//
		// 	bool result = true;
		// 	using( System.IO.Stream bad_stream = new FileStream( file_name, FileMode.Open ) )
		// 	{
		// 		BinaryReader reader = new BinaryReader( bad_stream );
		// 		do
		// 		{
		// 			byte single_byte = reader.ReadByte();
		// 			if( single_byte >= 128 )
		// 			{
		// 				result = false;
		// 			}
		//
		// 			if( single_byte < 32 )
		// 			{
		// 				if( single_byte != 9 && single_byte != 10 && single_byte != 13 )
		// 				{
		// 					result = false;
		// 				}
		// 			}
		// 		} while( result && ( reader.BaseStream.Position < reader.BaseStream.Length ) );
		//
		// 		bad_stream.Close();
		// 	}
		//
		// 	return result;
		// }

		private static bool ValidateAsUtf8( Repository repository, FileSpec fileSpec )
		{
			FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
			string file_name = file_meta_data.ClientPath.Path;

			bool result = true;
			using( System.IO.Stream bad_stream = new FileStream( file_name, FileMode.Open ) )
			{
				BinaryReader reader = new BinaryReader( bad_stream );

				if( reader.BaseStream.Length < 3 )
				{
					result = false;
				}

				if( reader.ReadByte() != 0xef )
				{ result =  false;
				}

				if( reader.ReadByte() != 0xbb )
				{
					result = false;
				}

				if( reader.ReadByte() != 0xbf )
				{
					result = false;
				}

				bad_stream.Close();
			}

			return result;
		}

		/// <summary>
		/// Uses to GetFileMetaData to obtain the local file path from the FileSpec and resaves the file as UTF-8.
		/// </summary>
		/// <param name="repository">The Perforce repository.</param>
		/// <param name="fileSpec">The FileSpec of the single file to process.</param>
		public static void ConvertToUtf8( Repository repository, FileSpec fileSpec )
		{
			FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
			string file_name = file_meta_data.ClientPath.Path;
			new FileInfo( file_name ).IsReadOnly = false;
			
			// Detect the encoding
			DetectionResult result = CharsetDetector.DetectFromFile( file_name );

			// Read it all in
			ConsoleLogger.Verbose( $" .. processing {file_name} with detected encoding {result.Detected.Encoding.EncodingName}" );
			string text = System.IO.File.ReadAllText( file_name, result.Detected.Encoding );

			// Write it all out as UTF-8
			System.IO.File.WriteAllText( file_name, text, Encoding.UTF8 );
		}

		/// <summary>
		/// Uses to GetFileMetaData to obtain the local file path from the FileSpec. Reads in the file character by character
		/// and fixes the Perforce UTF-16 text corruption if any is found. Finally, it saves the file as UTF-8.
		/// </summary>
		/// <param name="repository">The Perforce repository.</param>
		/// <param name="fileSpec">The FileSpec of the single file to process.</param>
		public static void ValidateFixAndUpdate( Repository repository, FileSpec fileSpec )
		{
			if( ValidateAsUtf8( repository, fileSpec ) )
			{
				ConsoleLogger.Warning( $"File has UTF-8 BOM - is this file already UTF-8? Skipping" );
				return;
			}

			StringBuilder result = new StringBuilder();

			FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
			string utf16_file = file_meta_data.ClientPath.Path;
			ConsoleLogger.Verbose( $" .. processing '{utf16_file}'" );
			new FileInfo( utf16_file ).IsReadOnly = false;

			using( System.IO.Stream bad_stream = new FileStream( utf16_file, FileMode.Open ) )
			{
				BinaryReader reader = new BinaryReader( bad_stream );

				if( reader.BaseStream.Length > 1 )
				{
					UInt16 BOM = reader.ReadUInt16();
					if( BOM != 0xfeff )
					{
						ConsoleLogger.Warning( $"File '{utf16_file}' is missing a BOM (0x{BOM.ToString( "X2" )}); adding as regular character" );
						result.Append( ( char )BOM );
					}
				}

				do
				{
					UInt16 character = CheckCharacter( reader );
					if( character != 0 )
					{
						result.Append( ( char )character );
					}
				}
				while( reader.BaseStream.Length - reader.BaseStream.Position > 1 ) ;

				bad_stream.Close();
			}

			System.IO.File.WriteAllText( utf16_file, result.ToString(), Encoding.UTF8 );
		}

		/// <summary>
		/// Creates a new changelist with the description passed in.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="description">The changelist description.</param>
		/// <returns>The id of the changelist.</returns>
		public static int CreateChangelist( PerforceConnectionInfo connectionInfo, string description )
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
		/// This Creates a new changelist and checks out all the files defined by the FileSpecs.
		/// It changes their base file type to UTF-8, but leaves any modifiers alone.
		/// It then iterates over all the files and calls the fixup function. 
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="utf16Files">The list of FileSpecs to process.</param>
		public static void ProcessDepotUtf16Files( PerforceConnectionInfo connectionInfo, IList<FileSpec> utf16Files )
		{
			int change_id = CreateChangelist( connectionInfo, "UTF16MustDIE - updating and fixing all UTF-16 files in the local workspace and converting to UTF-8" );

			Repository repository = connectionInfo.PerforceRepository!;
			Client client = connectionInfo.GetWorkspace()!;
			
			ConsoleLogger.Log( $" .. adding all UTF-16 files to new pending changelist." );

			IEnumerable<FileSpec> unversioned = utf16Files.ToList().Select( x => x.StripVersion() );
			client.EditFiles( unversioned.ToList(), new EditCmdOptions( EditFilesCmdFlags.None, change_id, new FileType( BaseFileType.UTF8, FileTypeModifier.None ) ) );

			foreach( FileSpec file_spec in utf16Files )
			{
				ValidateFixAndUpdate( repository, file_spec );
			}
		
			ConsoleLogger.Log( $"Decorrupted and updated to UTF-8 {utf16Files.Count} files and added to change {change_id}" );
		}

		/// <summary>
		/// This Creates a new changelist and checks out all the files defined by the FileSpecs.
		/// It changes their base file type to UTF-8, but leaves any modifiers alone.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="unicodeFiles">The list of FileSpecs to process.</param>
		public static void ProcessDepotAnsiFiles( PerforceConnectionInfo connectionInfo, IList<FileSpec> unicodeFiles )
		{
			int change_id = CreateChangelist( connectionInfo, "UTF16MustDIE - updating all ANSI files in the local workspace and converting to UTF-8" );

			Repository repository = connectionInfo.PerforceRepository!;
			Client client = connectionInfo.GetWorkspace()!;

			ConsoleLogger.Log( $" .. adding all ANSI files to new pending changelist." );

			IEnumerable<FileSpec> unversioned = unicodeFiles.ToList().Select( x => x.StripVersion() );
			client.EditFiles( unversioned.ToList(), new EditCmdOptions( EditFilesCmdFlags.None, change_id, new FileType( BaseFileType.UTF8, FileTypeModifier.None ) ) );

			foreach( FileSpec file_spec in unicodeFiles )
			{
				ConvertToUtf8( repository, file_spec );
			}
	
			ConsoleLogger.Log( $"Updated to UTF-8 {unicodeFiles.Count} files and added to change {change_id}" );
		}

		/// <summary>
		/// This Creates a new changelist and checks out all the files defined by the FileSpecs.
		/// It changes their base file type to UTF-8, but leaves any modifiers alone.
		/// It then iterates over all the files and calls the fixup function. 
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="utf16Files">The list of FileSpecs to process.</param>
		public static void ProcessPendingUtf16Files( PerforceConnectionInfo connectionInfo, IList<FileSpec> utf16Files )
		{
			ConsoleLogger.Log( $"... found {utf16Files.Count} UTF-16 files in pending change(s)." );
			int change_id = CreateChangelist( connectionInfo, "UTF16MustDIE - updating and fixing all UTF-16 files in pending changes and converting to UTF-8" );

			Repository repository = connectionInfo.PerforceRepository!;
			Client client = connectionInfo.GetWorkspace()!;

			ConsoleLogger.Log( $" .. moving all pending UTF-16 files to new pending changelist." );

			IEnumerable<FileSpec> unversioned = utf16Files.ToList().Select( x => x.StripVersion() );
			client.ReopenFiles( unversioned.ToList(), new EditCmdOptions( EditFilesCmdFlags.None, change_id, new FileType( BaseFileType.UTF8, FileTypeModifier.None ) ) );

			foreach( FileSpec file_spec in utf16Files )
			{
				ValidateFixAndUpdate( repository, file_spec );
			}
		
			ConsoleLogger.Log( $"Decorrupted and updated to UTF-8 {utf16Files.Count} files and added to change {change_id}" );
		}

		/// <summary>
		/// This Creates a new changelist and checks out all the files defined by the FileSpecs.
		/// It changes their base file type to UTF-8, but leaves the modifier alone.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="unicodeFiles">The list of FileSpecs to process.</param>
		public static void ProcessPendingAnsiFiles( PerforceConnectionInfo connectionInfo, IList<FileSpec> unicodeFiles )
		{
			ConsoleLogger.Log( $"... found {unicodeFiles.Count} ANSI files in pending change(s)." );
			int change_id = CreateChangelist( connectionInfo, "UTF16MustDIE - updating ANSI files in pending changes and converting to UTF-8" );

			Repository repository = connectionInfo.PerforceRepository!;
			Client client = connectionInfo.GetWorkspace()!;

			ConsoleLogger.Log( $" .. moving all pending ANSI files to new pending changelist." );

			IEnumerable<FileSpec> unversioned = unicodeFiles.ToList().Select( x => x.StripVersion() );
			client.ReopenFiles( unversioned.ToList(), new EditCmdOptions( EditFilesCmdFlags.None, change_id, new FileType( BaseFileType.UTF8, FileTypeModifier.None ) ) );

			foreach( FileSpec file_spec in unicodeFiles )
			{
				ConvertToUtf8( repository, file_spec );
			}
	
			ConsoleLogger.Log( $"Updated to UTF-8 {unicodeFiles.Count} files and added to pending change" );
		}
	}
}
