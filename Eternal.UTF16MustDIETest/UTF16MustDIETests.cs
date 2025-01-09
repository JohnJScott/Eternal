// Copyright Eternal Developments LLC. All Rights Reserved.

global using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;
using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;

using Perforce.P4;
using UtfUnknown;

using File = Perforce.P4.File;

namespace Eternal.Utf16MustDieTest.Tests
{
	/// <summary>
	/// A class to test various elements of the UTF16MustDIE utility.
	/// </summary>
	[TestClass]
	public class Utf16MustDieTests
	{
		private IList<FileSpec> GetCorruptedFiles( PerforceConnectionInfo connectionInfo, int changeId )
		{
			string client_root = connectionInfo.GetWorkspace()?.Root ?? String.Empty;
			Repository repository = connectionInfo.PerforceRepository!;
			Client client = connectionInfo.GetWorkspace()!;

			GetDepotFilesCmdOptions opts = new GetDepotFilesCmdOptions( GetDepotFilesCmdFlags.NotDeleted, 0 );
			FileSpec local_file_spec = new FileSpec( new ClientPath( client_root + "/EternalGit/Eternal.UTF16MustDIETest/TestFiles/..." ), null );
			IList<File> local_files = repository.GetFiles( opts, local_file_spec );

			IEnumerable<string> binary_file_specs = local_files.Where( x => x.Type.BaseType == BaseFileType.Binary ).Select( x => x.DepotPath.Path );
			IList<FileSpec> unescaped_test_file_specs = binary_file_specs.Select( x => ( FileSpec )( new DepotPath( PathSpec.UnescapePath( x ) ) ) ).ToList();

			ConsoleLogger.Log( $" .. adding all test files to new pending changelist." );

			IEnumerable<FileSpec> unversioned = unescaped_test_file_specs.ToList().Select( x => x.StripVersion() );
			client.EditFiles( unversioned.ToList(), new EditCmdOptions( EditFilesCmdFlags.None, changeId, new FileType( BaseFileType.UTF16, FileTypeModifier.None ) ) );

			return unescaped_test_file_specs;
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

		private void CheckUtf16( Repository repository, FileSpec fileSpec )
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

			Assert.IsTrue( null_char_count > reader.BaseStream.Length / 3, "Not enough null chars; file is not likely UTF-16" );
			reader.Close();
		}

		private void CheckUtf8( Repository repository, FileSpec fileSpec )
		{
			FileMetaData file_meta_data = repository.GetFileMetaData( null, fileSpec ).First();
			string utf8_file = file_meta_data.ClientPath.Path;

			System.IO.Stream good_stream = new FileStream( utf8_file, FileMode.Open );
			BinaryReader reader = new BinaryReader( good_stream );

			Assert.IsTrue( reader.ReadByte() == 0xef, "First UTF-8 BOM entry incorrect" );
			Assert.IsTrue( reader.ReadByte() == 0xbb, "Second UTF-8 BOM entry incorrect" );
			Assert.IsTrue( reader.ReadByte() == 0xbf, "Third UTF-8 BOM entry incorrect" );

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

			Assert.IsTrue( null_char_count <= 1, "Excess null chars; file is not likely UTF-8" );
			reader.Close();
		}

		[TestMethod( "Find all UTF-16 files in the local workspace." )]
		public void GetUtf16Files()
		{
			PerforceUtilities.PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Connect( connection_info ), "Failed to connect" );

			IList<FileSpec> utf16_files = Utf16MustDie.Perforce.GetFilesOfBaseFileType( connection_info, BaseFileType.UTF16 );

			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Disconnect( connection_info ), "Failed to disconnect" );
		}

		[TestMethod( "Validate and fix corrupted UTF-16 files" )]
		public void ValidateFiles()
		{
			PerforceUtilities.PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Connect( connection_info ), "Failed to connect" );

			int change_id = Utf16MustDie.Perforce.CreateChangelist( connection_info, "UTF16MustDIE - Temporary change for unit testing" );
			IList<FileSpec> utf16_files = GetCorruptedFiles( connection_info, change_id );

			Repository repository = connection_info.PerforceRepository!;
			foreach( FileSpec file_spec in utf16_files )
			{
				CheckUtf16( repository, file_spec );
				Utf16MustDie.Perforce.ValidateFixAndUpdate( repository, file_spec );
				CheckUtf8( repository, file_spec );
			}

			RevertCorruptedFiles( connection_info, change_id );

			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Disconnect( connection_info ), "Failed to disconnect" );
		}

		private void TestEncoding( string friendlyName, string encodingName, string sampleString )
		{
			Encoding encoding = Encoding.GetEncoding( encodingName );

			string temp_folder = Path.GetTempPath();
			string file_name = Path.Combine( temp_folder, $"sample_{friendlyName}_string.{encoding.CodePage}" );

			System.IO.File.WriteAllText( file_name, sampleString, encoding );
			System.IO.File.WriteAllText( file_name + ".utf8", sampleString, Encoding.UTF8 );

			DetectionResult result = CharsetDetector.DetectFromFile( file_name );

			string new_utf8_string = System.IO.File.ReadAllText( file_name, result.Detected.Encoding );
			System.IO.File.WriteAllText( file_name + ".utf8.new", new_utf8_string, Encoding.UTF8 );

			byte[] original = System.IO.File.ReadAllBytes( file_name + ".utf8" );
			byte[] updated = System.IO.File.ReadAllBytes( file_name + ".utf8.new" );

			CollectionAssert.AreEqual( original, updated, $"New file is not the same as original for {friendlyName}.\n '{new_utf8_string}' does not match '{sampleString}'." );
		}

		[TestMethod( "ValidateEncodings" )]
		public void ValidateEncodings()
		{
			Encoding.RegisterProvider( CodePagesEncodingProvider.Instance );

			string temp_folder = Path.GetTempPath();

			// Write a file containing random characters
			Random random = new Random();
			byte[] random_bytes = new byte[4096];
			random.NextBytes( random_bytes );

			string random_string = "";
			foreach( byte random_byte in random_bytes )
			{
				random_string += ( char )( random_byte | 0x20 );
			}

			System.IO.File.WriteAllText( Path.Combine( temp_folder, $"sample_random_string.utf8" ), random_string, Encoding.UTF8 );

			TestEncoding( "western_euro", "Windows-1252", "Test characters:á ß Ç Ð" );
			TestEncoding( "shift_jis", "shift_jis", "Test characters:素早い茶色のキツネが怠け者の犬を飛び越えます。" );
			TestEncoding( "central_euro", "windows-1250", "Test characters:Szybki brązowy lis przeskakuje leniwego psa." );
			TestEncoding( "cyrillic", "windows-1251", "Test characters:Быстрая коричневая лиса прыгает через ленивую собаку." );
			TestEncoding( "greek", "windows-1253", "Test characters:Η γρήγορη καφέ αλεπού πηδάει πάνω από το τεμπέλικο σκυλί." );
			TestEncoding( "hebrew", "windows-1255", "Test characters:השועל החום המהיר קופץ מעל הכלב העצלן." );
			TestEncoding( "arabic", "windows-1256", "Test characters:vالثعلب البني السريع يقفز فوق الكلب الكسول." );
			TestEncoding( "korean", "ks_c_5601-1987", "Test characters:날렵한 갈색여우가 게으른 개를 뛰어넘습니다." );
			//TestEncoding( "trad_chinese", "big5", "Test characters:敏捷的棕色狐狸跳過了懶狗。" );
		}
	}
}
