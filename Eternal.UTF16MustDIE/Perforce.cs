// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using System.Text;
using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;

using File = Perforce.P4.File;

namespace Eternal.UTF16MustDIE
{
	/// <summary>Class to handle interaction with Perforce.</summary>
	public class Perforce
	{
		/// <summary>
		/// Syncs all files in the list of FileSpecs to #head.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="utf16Files">The container of files to sync.</param>
		public static void SyncUTF16Files( PerforceConnectionInfo connectionInfo, IList<FileSpec> utf16Files )
		{
			Client client = connectionInfo.GetWorkspace()!;

			IList<FileSpec> unversioned = utf16Files.ToList().Select( x => x.StripVersion() ).ToList();
			client.RevertFiles( unversioned, null );
			client.SyncFiles( unversioned, null );
		}

		/// <summary>Find all UTF-16 files in the current workspace.</summary>
		/// <param name="connectionInfo">The information about the current Perforce connection.</param>
		/// <returns>A list of file specifications of all files in the local workspace that have the base type of UTF-16.</returns>
		public static IList<FileSpec> GetUTF16Files( PerforceConnectionInfo connectionInfo )
		{
			string client_root = connectionInfo.GetWorkspace()?.Root ?? String.Empty;

			Repository repository = connectionInfo.PerforceRepository!;

			GetDepotFilesCmdOptions opts = new GetDepotFilesCmdOptions( GetDepotFilesCmdFlags.NotDeleted, 0 );
			FileSpec local_file_spec = new FileSpec( new ClientPath( client_root + "/..." ), null );
			IList<File> local_files = repository.GetFiles( opts, local_file_spec );

			IList<FileSpec> utf16_file_specs = local_files.Where( x => x.Type.BaseType == BaseFileType.UTF16 ).Select( x => ( FileSpec )x ).ToList();

			return utf16_file_specs;
		}

		/// <summary>
		/// Gets all the UTF16 files in pending changelists belonging to the current workspace.
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <returns></returns>
		public static IList<FileSpec> GetUTF16PendingFiles( PerforceConnectionInfo connectionInfo )
		{
			IList<FileSpec> pending_file_specs = new List<FileSpec>();
			string client_root = connectionInfo.GetWorkspace()?.Root ?? String.Empty;

			Repository repository = connectionInfo.PerforceRepository!;
			ChangesCmdOptions changes_cmd_options = new ChangesCmdOptions( ChangesCmdFlags.None, connectionInfo.Workspace, Int32.MaxValue, ChangeListStatus.Pending, connectionInfo.User, 0 );
			FileSpec local_file_spec = new FileSpec( new ClientPath( client_root + "/..." ), null );
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
						if( file_meta_data.Type.BaseType == BaseFileType.UTF16 )
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

		/// <summary>
		/// Uses to GetFileMetaData to obtain the local file path from the FileSpec. Reads in the file character by character
		/// and fixes the Perforce text corruption if any is found. Finally, it saves the file as UTF-8.
		/// </summary>
		/// <param name="repository">The Perforce repository.</param>
		/// <param name="fileSpec">The FileSpec of the single file to process.</param>
		public static void ValidateFixAndUpdate( Repository repository, FileSpec fileSpec )
		{
			StringBuilder result = new StringBuilder();

			FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
			string utf16_file = file_meta_data.ClientPath.Path;
			new FileInfo( utf16_file ).IsReadOnly = false;

			System.IO.Stream bad_stream = new FileStream( utf16_file, FileMode.Open );
			BinaryReader reader = new BinaryReader( bad_stream );

			UInt16 BOM = reader.ReadUInt16();
			if( BOM == 0xbbef )
			{
				ConsoleLogger.Warning( $"File '{utf16_file}' is has UTF-8 BOM - is this file already UTF-8? Skipping" );
				return;
			}
			else if( BOM != 0xfeff )
			{
				ConsoleLogger.Warning( $"File '{utf16_file}' is missing a BOM (0x{BOM.ToString( "X2" )}); adding as regular character" );
				result.Append( ( char )BOM );
			}

			do
			{
				UInt16 character = CheckCharacter( reader );
				if( character != 0 )
				{
					result.Append( ( char )character );
				}
			}
			while( reader.BaseStream.Position != reader.BaseStream.Length );

			reader.Close();

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
		/// This Creates a new changelist and checks out all the
		/// files defined by the FileSpecs. It changes their base file type to UTF-8, but leaves the modifier alone.
		/// It then iterates over all the files and calls the fixup function. 
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="utf16Files">The list of FileSpecs to process.</param>
		/// <returns>The changelist number.</returns>
		public static int ProcessDepotUTF16Files( PerforceConnectionInfo connectionInfo, IList<FileSpec> utf16Files )
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

			return change_id;
		}

		/// <summary>
		/// This Creates a new changelist and checks out all the
		/// files defined by the FileSpecs. It changes their base file type to UTF-8, but leaves the modifier alone.
		/// It then iterates over all the files and calls the fixup function. 
		/// </summary>
		/// <param name="connectionInfo">The Perforce connection info.</param>
		/// <param name="utf16Files">The list of FileSpecs to process.</param>
		/// <returns>The changelist number.</returns>
		public static int ProcessPendingUTF16Files( PerforceConnectionInfo connectionInfo, IList<FileSpec> utf16Files )
		{
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

			return change_id;
		}
	}
}