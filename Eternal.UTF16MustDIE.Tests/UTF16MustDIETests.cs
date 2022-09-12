// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

global using Microsoft.VisualStudio.TestTools.UnitTesting;

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;

using Perforce.P4;

using File = Perforce.P4.File;

namespace Eternal.UTF16MustDIE.Tests
{
	/// <summary>
	/// A class to test various elements of the UTF16MustDIE utility.
	/// </summary>
    [TestClass]
    public class UTF16MustDIETests
    {
	    private IList<FileSpec> GetCorruptedFiles( PerforceConnectionInfo connectionInfo, int changeId )
	    {
		    string client_root = connectionInfo.GetWorkspace()?.Root ?? String.Empty;
		    Repository repository = connectionInfo.PerforceRepository!;
		    Client client = connectionInfo.GetWorkspace()!;

			GetDepotFilesCmdOptions opts = new GetDepotFilesCmdOptions( GetDepotFilesCmdFlags.NotDeleted, 0 );
		    FileSpec local_file_spec = new FileSpec( new ClientPath( client_root + "/EternalGit/Eternal.UTF16MustDIE.Tests/TestFiles/..." ), null );
		    IList<File> local_files = repository.GetFiles( opts, local_file_spec );

		    IList<FileSpec> binary_file_specs = local_files.Where( x => x.Type.BaseType == BaseFileType.Binary ).Select( x => ( FileSpec )x ).ToList();

		    ConsoleLogger.Log( $" .. adding all UTF-16 files to new pending changelist." );

		    IEnumerable<FileSpec> unversioned = binary_file_specs.ToList().Select( x => x.StripVersion() );
			client.EditFiles( unversioned.ToList(), new EditCmdOptions( EditFilesCmdFlags.None, changeId, new FileType( BaseFileType.UTF16, FileTypeModifier.None ) ) );

			return binary_file_specs;
	    }

	    private void RevertCorruptedFiles( PerforceConnectionInfo connectionInfo, int changeId )
	    {
		    Repository repository = connectionInfo.PerforceRepository!;
		    Client client = connectionInfo.GetWorkspace()!;
		    Changelist change = repository.GetChangelist( changeId, null );

		    List<FileSpec> files = new List<FileSpec>();
		    foreach( FileMetaData file_meta_data in change.Files )
		    {
			    files.Add( file_meta_data );
		    }

		    client.RevertFiles( files, null );
		    repository.DeleteChangelist( change, null );
		}

	    private void CheckUTF16( Repository repository, FileSpec fileSpec )
	    {
		    FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
		    string utf16_file = file_meta_data.ClientPath.Path;

		    System.IO.Stream bad_stream = new FileStream( utf16_file, FileMode.Open );
		    BinaryReader reader = new BinaryReader( bad_stream );

		    UInt16 BOM = reader.ReadUInt16();
		    Assert.IsTrue( BOM == 0xfeff, "BOM not found" );

			// For the test files (based in English) a significant portion of the bytes will be 0
			int null_char_count = 0;
			do
			{
				byte stream_byte = reader.ReadByte();
				if( stream_byte == 0 )
				{
					null_char_count++;
				}
			}
			while( reader.BaseStream.Position != reader.BaseStream.Length );

			Assert.IsTrue( null_char_count > reader.BaseStream.Length / 3, "Not enough null chars; file is not likely UTF16" );
			reader.Close();
	    }

		private void CheckUTF8( Repository repository, FileSpec fileSpec )
	    {
		    FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
		    string utf8_file = file_meta_data.ClientPath.Path;

		    System.IO.Stream good_stream = new FileStream( utf8_file, FileMode.Open );
		    BinaryReader reader = new BinaryReader( good_stream );

			Assert.IsTrue( reader.ReadByte() == 0xef, "First UTF-8 BOM entry incorrect" );
			Assert.IsTrue( reader.ReadByte() == 0xbb, "First UTF-8 BOM entry incorrect" );
			Assert.IsTrue( reader.ReadByte() == 0xbf, "First UTF-8 BOM entry incorrect" );

			int null_char_count = 0;
			do
			{
				byte stream_byte = reader.ReadByte();
				if( stream_byte == 0 )
				{
					null_char_count++;
				}
			}
			while( reader.BaseStream.Position != reader.BaseStream.Length );

			Assert.IsTrue( null_char_count <= 1, "Excess null chars; file is not likely UTF8" );
			reader.Close();
		}

		[TestMethod("Find all UTF-16 files in the local workspace.")]
		public void GetUTF16Files()
        {
	        PerforceUtilities.PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Connect( connection_info ), "Failed to connect" );

			IList<FileSpec> utf16_files = Perforce.GetUTF16Files( connection_info );

			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Disconnect( connection_info ), "Failed to disconnect" );
		}

        [TestMethod( "Validate and fix corrupted UTF-16 files" )]
        public void ValidateFiles()
        {
	        PerforceUtilities.PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
	        Assert.IsTrue( PerforceUtilities.PerforceUtilities.Connect( connection_info ), "Failed to connect" );

		    int change_id = Perforce.CreateChangelist( connection_info, "UTF16MustDIE - Temporary change for unit testing" );
			IList<FileSpec> utf16_files = GetCorruptedFiles( connection_info, change_id );

		    Repository repository = connection_info.PerforceRepository!;
			foreach( FileSpec file_spec in utf16_files )
			{
				CheckUTF16( repository, file_spec );
				Perforce.ValidateFixAndUpdate( repository, file_spec );
				CheckUTF8( repository, file_spec );
			}

			RevertCorruptedFiles( connection_info, change_id );

			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Disconnect( connection_info ), "Failed to disconnect" );
		}
	}
}