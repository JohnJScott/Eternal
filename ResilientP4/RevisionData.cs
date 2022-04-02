// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Eternal.EternalUtilities;
using Newtonsoft.Json;
using Perforce.P4;

namespace ResilientP4
{
	/// <summary>
	///     A class to contain all the information about a set of files.
	/// </summary>
	public class RevisionData
	{
		/// <summary>The label (prefixed with @) or #head.</summary>
		public string Revision = "";

		/// <summary>The depot name for the branch name we are interested in. All depot file names begin with this, so skipping this path 
		/// in the depot file name will return the relative depot path.</summary>
		public string BranchName = "";

		/// <summary>The timestamp of the sync.</summary>
		public DateTime Timestamp = DateTime.UtcNow;

		/// <summary>The changelist description.</summary>
		public string Description = "";

		/// <summary>A collection of file details indexed by the depot file name.</summary>
		public Dictionary<string, DepotFileData> DepotFiles = new Dictionary<string, DepotFileData>();

		/// <summary>
		/// Default constructor for serialisation purposes.
		/// </summary>
		public RevisionData()
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="InRevision"></param>
		public RevisionData( string InBranchName, string InRevision, string InDescription )
		{
			BranchName = InBranchName;
			Revision = InRevision;
			Description = InDescription;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="NewBranchName"></param>
		/// <returns></returns>
		public IEnumerable<string> RemapFileNames( string NewBranchName )
		{
			IEnumerable<string> NewFileNames = DepotFiles.Keys.Select( x => NewBranchName + x.Substring( BranchName.Length ) );
			return NewFileNames;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public int GetHaveRevision( string FileName )
		{
			return DepotFiles[FileName].HaveRevisionNumber;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public int GetHeadRevision( string FileName )
		{
			return DepotFiles[FileName].HeadRevisionNumber;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public long GetFileSize( string FileName )
		{
			return DepotFiles[FileName].Size;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public string GetDigest( string FileName )
		{
			return DepotFiles[FileName].Digest;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TypedFileType"></param>
		/// <returns></returns>
		static public TextEncoding GetEncodingFromType( FileType TypedFileType )
		{
			TextEncoding Result = TextEncoding.Invalid;
			switch( TypedFileType.BaseType )
			{
			case BaseFileType.Text:
				Result = TextEncoding.TEXT;
				break;
			case BaseFileType.Unicode:
				Result = TextEncoding.UNICODE;
				break;
			case BaseFileType.UTF16:
				Result = TextEncoding.UTF16;
				break;
			}

			return Result;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public TextEncoding GetEncodingForFile( string FileName )
		{
			FileType TypedFileType = new FileType( DepotFiles[FileName].PerforceFileType );
			return GetEncodingFromType( TypedFileType );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public bool IsWritableOnClient( string FileName )
		{
			FileType TypedFileType = new FileType( DepotFiles[FileName].PerforceFileType );
			return TypedFileType.Modifiers.HasFlag( FileTypeModifier.Writable );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotFileName"></param>
		/// <returns></returns>
		public DepotFileData GetDepotFile( string DepotFileName )
		{
			DepotFileData FoundFile = null;
			if( DepotFiles.Keys.Contains( DepotFileName ) )
			{
				FoundFile = DepotFiles[DepotFileName];
			}

			return FoundFile;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileDetail"></param>
		public void AddFile( FileMetaData FileDetail )
		{
			if( FileDetail.DepotPath != null )
			{
				DepotFileData FileDetailInstance = new DepotFileData( FileDetail );

				DepotFiles.Add( FileDetail.DepotPath.Path, FileDetailInstance );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotFile"></param>
		public void AddFile( DepotFileData DepotFile )
		{
			DepotFiles.Add( DepotFile.DepotPath, DepotFile );
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns>A list of files that are new in the label, and the client does not have.</returns>
		public IEnumerable<string> GetNewFiles( RevisionData LabelData )
		{
			IEnumerable<string> NewFiles = LabelData.DepotFiles.Keys.Where( x => !DepotFiles.Keys.Contains( x ) ).ToList();
			return NewFiles;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public int GetNewFileCount( RevisionData LabelData )
		{
			int NewFileCount = LabelData.DepotFiles.Keys.Where( x => !DepotFiles.Keys.Contains( x ) ).Count();
			return NewFileCount;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns>A list of files the client has that are not in the label.</returns>
		public IEnumerable<string> GetDeletedFiles( RevisionData LabelData )
		{
			IEnumerable<string> DeletedFiles = DepotFiles.Keys.Where( x => !LabelData.DepotFiles.Keys.Contains( x ) ).ToList();
			return DeletedFiles;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public int GetDeletedFileCount( RevisionData LabelData )
		{
			int DeletedFileCount = DepotFiles.Keys.Where( x => !LabelData.DepotFiles.Keys.Contains( x ) ).Count();
			return DeletedFileCount;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public IEnumerable<string> GetCommonFiles( RevisionData LabelData )
		{
			IEnumerable<string> CommonFiles = DepotFiles.Keys.Where( x => LabelData.DepotFiles.Keys.Contains( x ) ).ToList();
			return CommonFiles;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public int GetCommonFileCount( RevisionData LabelData )
		{
			int CommonFileCount = DepotFiles.Keys.Where( x => LabelData.DepotFiles.Keys.Contains( x ) ).Count();
			return CommonFileCount;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public IEnumerable<string> GetIdenticalFiles( RevisionData LabelData )
		{
			IEnumerable<string> IdenticalFiles = DepotFiles.Keys.Where( x => ( LabelData.DepotFiles.Keys.Contains( x ) && DepotFiles[x].Equals( LabelData.DepotFiles[x] ) ) ).ToList();
			return IdenticalFiles;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public int GetIdenticalFileCount( RevisionData LabelData )
		{
			int IdenticalFileCount = DepotFiles.Keys.Where( x => ( LabelData.DepotFiles.Keys.Contains( x ) && DepotFiles[x].Equals( LabelData.DepotFiles[x] ) ) ).Count();
			return IdenticalFileCount;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public IEnumerable<string> GetDifferingFiles( RevisionData LabelData )
		{
			IEnumerable<string> DifferingFiles = DepotFiles.Keys.Where( x => ( LabelData.DepotFiles.Keys.Contains( x ) && !DepotFiles[x].Equals( LabelData.DepotFiles[x] ) ) ).ToList();
			return DifferingFiles;
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		public int GetDifferingFileCount( RevisionData LabelData )
		{
			int DifferingFileCount = DepotFiles.Keys.Where( x => ( LabelData.DepotFiles.Keys.Contains( x ) && !DepotFiles[x].Equals( LabelData.DepotFiles[x] ) ) ).Count();
			return DifferingFileCount;
		}

		/// <summary>
		/// </summary>
		public void MatchToLabel()
		{
			FormsLogger.Title( "Matching #have file data to label data" );
			DirectoryInfo DirInfo = new DirectoryInfo( "." );
			if( DirInfo.Exists )
			{
				FileInfo[] RevisionFiles = DirInfo.GetFiles( "*.revisions" );
				FormsLogger.Log( " ... found " + RevisionFiles.Length + " revision files." );
				foreach( FileInfo Info in RevisionFiles )
				{
					if( !Info.Name.Contains( "#have" ) )
					{
						RevisionData LabelData = JsonHelper.ReadJsonFile<RevisionData>( Info.FullName );
						FormsLogger.Log( " ... processing revision file '" + LabelData.Revision + "'" );

						List<string> CommonFiles = GetCommonFiles( LabelData ).ToList();
						if( CommonFiles.Count != DepotFiles.Count )
						{
							// Missing or extra files - display details
							FormsLogger.Log( " ... " + GetDeletedFileCount( LabelData ) + " files missing from the label." );
							FormsLogger.Log( " ... " + GetNewFileCount( LabelData ) + " extra files in the label." );
							FormsLogger.Log( "Rejecting!" );
						}
						else
						{
							int DifferingFileCount = GetDifferingFileCount( LabelData );
							FormsLogger.Log( " ... " + ( DepotFiles.Count - DifferingFileCount ) + " files match, and " + DifferingFileCount + " files differ." );
							if( DifferingFileCount == 0 )
							{
								FormsLogger.Success( "#have files match the contents of label revision '" + LabelData.Revision + "'" );
							}
						}
					}
				}
			}
		}
	}

	/// <summary>
	/// A description of all the files that need updating after local files have been compared to depot files.
	/// </summary>
	public class ReconcileData
	{
		/// <summary>The depot name for the branch name we are interested in. All depot file names begin with this, so skipping this path 
		/// in the depot file name will return the relative depot path.</summary>
		public string BranchName = "";

		/// <summary>The local folder location of the files to be reconciled</summary>
		public string FileFolder = "";

		/// <summary>The timestamp of the sync.</summary>
		public DateTime Timestamp = DateTime.UtcNow;

		/// <summary></summary>
		public List<string> FilesToAdd = new List<string>();

		/// <summary></summary>
		public List<string> FilesToDelete = new List<string>();

		/// <summary></summary>
		public List<string> FilesToEdit = new List<string>();

		/// <summary>
		/// 
		/// </summary>
		public ReconcileData()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InBranchName"></param>
		/// <param name="InFileFolder"></param>
		public ReconcileData( string InBranchName, string InFileFolder )
		{
			BranchName = InBranchName;
			FileFolder = InFileFolder;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TotalCount"></param>
		/// <returns></returns>
		public string GetConfirmationMessage( int TotalCount )
		{
			int GoodFileCount = TotalCount - FilesToDelete.Count - FilesToEdit.Count;
			return "Found " + FilesToAdd.Count + " files to add, " + FilesToDelete.Count + " files to delete, and " + FilesToEdit.Count + " files to edit (" + GoodFileCount + " files were identical). Do you wish to reconcile?";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool HasChanges()
		{
			if( FilesToAdd.Count > 0 )
			{
				return true;
			}

			if( FilesToDelete.Count > 0 )
			{
				return true;
			}

			if( FilesToEdit.Count > 0 )
			{
				return true;
			}

			return false;
		}
	}

	/// <summary>
	///     The known information about a file revision in the depot.
	/// </summary>
	public class DepotFileData
	{
		/// <summary></summary>
		public string DepotPath = "";

		/// <summary></summary>
		[JsonIgnore]
		public string LocalPath = "";

		/// <summary></summary>
		public string Digest = "";

		/// <summary></summary>
		public string PerforceFileType = "binary";

		/// <summary></summary>
		public FileAction HeadAction = FileAction.None;

		/// <summary></summary>
		[JsonIgnore]
		public int HeadRevisionNumber = -1;

		/// <summary></summary>
		public int HaveRevisionNumber = -1;

		/// <summary></summary>
		public long Size = 0;

		// A cached version of the normalised text file
		[JsonIgnore]
		public byte[] EntireTextFile = null;

		/// <summary>
		/// 
		/// </summary>
		public DepotFileData()
		{
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileDetail"></param>
		public DepotFileData( FileMetaData FileDetail )
		{
			DepotPath = FileDetail.DepotPath.Path;
			LocalPath = FileDetail.LocalPath.Path;
			Digest = FileDetail.Digest;
			HeadAction = FileDetail.HeadAction;
			if( FileDetail.Type != null )
			{
				PerforceFileType = FileDetail.Type.ToString();
			}

			HeadRevisionNumber = FileDetail.HeadRev;
			HaveRevisionNumber = FileDetail.HaveRev;
			Size = FileDetail.FileSize;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="TextBuffer"></param>
		public byte[] NormaliseLineEndings( byte[] TextBuffer, Encoding CurrentEncoding )
		{
			string InputString = CurrentEncoding.GetString( TextBuffer );

			// Convert Windows line endings to Unix line endings
			InputString = InputString.Replace( "\r\n", "\n" );

			// Leave Unix (\n) and Mac (\r) line endings as they are

			TextBuffer = CurrentEncoding.GetBytes( InputString );

			return TextBuffer;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Info"></param>
		public void GetNormalisedSize( FileInfo Info )
		{
			// Text files are stored as ANSI
			// Unicode files are stored in Perforce as UTF-8
			// UTF-16 files are stored in Perforce as UTF-8, and stored on the client with a BOM appropriate for the client (LE in this case)

			using( FileStream InputFile = Info.OpenRead() )
			{
				TextEncoding DataEncoding = RevisionData.GetEncodingFromType( new FileType( PerforceFileType ) );

				switch( DataEncoding )
				{
				default:
					Size = Info.Length;
					break;

				case TextEncoding.UNICODE:
					// Just trim out \r characters for SBCS and MBCS strings
					EntireTextFile = new byte[Info.Length];
					if( InputFile.Read( EntireTextFile, 0, ( int )Info.Length ) == Info.Length )
					{
						EntireTextFile = NormaliseLineEndings( EntireTextFile, Encoding.UTF8 );
						Size = EntireTextFile.Length;
					}
					break;

				case TextEncoding.TEXT:
					// Just trim out \r characters for SBCS and MBCS strings
					EntireTextFile = new byte[Info.Length];
					if( InputFile.Read( EntireTextFile, 0, ( int )Info.Length ) == Info.Length )
					{
						EntireTextFile = NormaliseLineEndings( EntireTextFile, Encoding.GetEncoding( 0 ) );
						Size = EntireTextFile.Length;
					}
					break;

				case TextEncoding.UTF16:
					// Convert to UTF-8 before checksumming, and then trim \r characters
					EntireTextFile = new byte[Info.Length];
					if( InputFile.Read( EntireTextFile, 0, ( int )Info.Length ) == Info.Length )
					{
						EntireTextFile = Encoding.Convert( Encoding.Unicode, Encoding.UTF8, EntireTextFile );
						EntireTextFile = NormaliseLineEndings( EntireTextFile, Encoding.UTF8 );
						// Always skip the UTF-8 preamble - even for strings with the DBCS
						EntireTextFile = EntireTextFile.Skip( 3 ).ToArray();
						Size = EntireTextFile.Length;
					}
					break;
				}

				InputFile.Close();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Info"></param>
		public void CalculateMD5Checksum( FileInfo Info )
		{
			Digest = "";

			using( MD5 Checksummer = new MD5CryptoServiceProvider() )
			{
				byte[] Checksum = null;

				TextEncoding DataEncoding = RevisionData.GetEncodingFromType( new FileType( PerforceFileType ) );

				switch( DataEncoding )
				{
					default:
						// Default binary checksum
						using( FileStream InputFile = Info.OpenRead() )
						{
							Checksum = Checksummer.ComputeHash( InputFile );
							InputFile.Close();							
						}
						break;

					case TextEncoding.TEXT:
					case TextEncoding.UNICODE:
					case TextEncoding.UTF16:
						// Checksum the normalised text file
						Checksum = Checksummer.ComputeHash( EntireTextFile );
						EntireTextFile = null;
						break;
				}

				if( Checksum != null )
				{
					foreach( byte Check in Checksum )
					{
						Digest += Check.ToString( "X2", CultureInfo.InvariantCulture );
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Other"></param>
		/// <returns></returns>
		public override bool Equals( object obj )
		{
			if( obj == null )
			{
				return false;
			}

			DepotFileData TypedOther = ( DepotFileData )obj;
			if( Size != TypedOther.Size )
			{
				return false;
			}

			if( PerforceFileType != TypedOther.PerforceFileType )
			{
				return false;
			}

			if( Digest != TypedOther.Digest )
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return base.GetHashCode() ^ HaveRevisionNumber;
		}
	}
}
