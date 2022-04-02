// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Eternal.EternalUtilities;
using Perforce.P4;
using Ionic.Zip;
using File = Perforce.P4.File;
using Label = Perforce.P4.Label;

namespace ResilientP4
{
	public enum TextEncoding
	{
		Invalid,
		TEXT,
		UNICODE,
		UTF16
	}

	/// <summary>
	///     A class to interface with Perforce .NET API.
	/// </summary>
	public class Perforce : IDisposable
	{
		private MainForm RootApplication = null;

		private Repository ServerRepository;
		private IList<User> ServerUsers = new List<User>();
		private IList<Client> Workspaces = new List<Client>();
		private IList<Label> Labels = new List<Label>();
		private Dictionary<string, List<Changelist>> SubmittedChanges = new Dictionary<string, List<Changelist>>();
		private IList<Changelist> PendingChanges = new List<Changelist>();

		private string ServerDisplayName = "";
		private string ServerTicketName = "";
		private string CachedUserName = "";
		private string CachedTicket = "";
		private string CachedWorkspace = "";

		/// <summary></summary>
		public DepotTreeNode RootNode = null;

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InRootApplication"></param>
		public Perforce( MainForm InRootApplication )
		{
			RootApplication = InRootApplication;
		}

		protected virtual void Dispose( bool disposing )
		{
			if( ServerRepository != null )
			{
				ServerRepository.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>
		/// 
		/// </summary>
		public string SafeServerDisplayName
		{
			get
			{
				if( ServerRepository != null )
				{
					return ServerDisplayName;
				}

				return "Unknown";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string SafeServerTicketName
		{
			get
			{
				if( ServerRepository != null )
				{
					return ServerTicketName;
				}

				return "Unknown";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public string CurrentUserName
		{
			get { return CachedUserName; }
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> UserNames
		{
			get { return ServerUsers.Select( x => x.Id ); }
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> ChangesUsers
		{
			get { return SubmittedChanges.Keys; }
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="File"></param>
		/// <returns></returns>
		public bool ShouldArchiveFile( FileMetaData File )
		{
			bool bArchive = false;

			if( File.LocalPath == null )
			{
				return bArchive;
			}

			switch( File.HeadAction )
			{
			case FileAction.MoveDelete:
			case FileAction.Delete:
			case FileAction.DeleteFrom:
			case FileAction.DeleteInto:
			case FileAction.Purge:
				bArchive = false;
				FormsLogger.Log( " ... suppressing archival of '" + File.DepotPath + "' with action: " + File.Action );
				break;

			default:
				bArchive = true;
				FormsLogger.Verbose( " ... archiving '" + File.DepotPath + "' with action: " + File.Action );
				break;
			}

			return bArchive;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public int GetPendingChangesCount()
		{
			return PendingChanges.Count;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserName"></param>
		/// <returns></returns>
		public int GetUserChangesCount( string UserName )
		{
			if( SubmittedChanges.ContainsKey( UserName ) )
			{
				return SubmittedChanges[UserName].Count;
			}

			return 0;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> WorkspaceNames
		{
			get { return Workspaces.Select( x => x.Name ); }
		}


		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		private object SetCurrentWorkspaceDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			if( CachedWorkspace.Length > 0 )
			{
				FormsLogger.Log( " ... setting current workspace to '" + CachedWorkspace + "'" );
				ServerRepository.Connection.SetClient( CachedWorkspace );
			}
			else
			{
				FormsLogger.Error( " ... cannot set to a blank workspace." );
			}
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetUsersOnServerDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			ServerUsers = new List<User>();

			string UserName = ( string ) Parameters[0];
			string ServerDisplayName = ( string ) Parameters[1];
			FormsLogger.Log( " ... retrieving users matching '" + UserName + "' from server '" + ServerDisplayName + "'" );

			IList<User> FoundServerUsers = ServerRepository.GetUsers( null, UserName );
			if( FoundServerUsers != null )
			{
				ServerUsers = FoundServerUsers;
			}

			FormsLogger.Log( " ... found " + ServerUsers.Count + " user(s)." );
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetWorkspacesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			Workspaces = new List<Client>();

			string UserName = ( string ) Parameters[0];
			FormsLogger.Log( " ... retrieving workspaces from server for '" + UserName + "'." );

			ClientsCmdOptions Options = new ClientsCmdOptions( ClientsCmdFlags.None, UserName, null, -1, null );
			IList<Client> FoundWorkspaces = ServerRepository.GetClients( Options );
			if( FoundWorkspaces != null )
			{
				Workspaces = FoundWorkspaces;
			}

			FormsLogger.Success( " ... found " + Workspaces.Count + " workspace(s)." );
			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private IList<string> GetDepotDirsDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			string FolderMatch = ( string )Parameters[0];
			bool bIncludeDeletedFolders = ( bool )Parameters[1];

			GetDepotDirsCmdFlags Flags = GetDepotDirsCmdFlags.None;
			if( bIncludeDeletedFolders )
			{
				Flags |= GetDepotDirsCmdFlags.IncludeDeletedFilesDirs;
			}
			GetDepotDirsCmdOptions FolderOptions = new GetDepotDirsCmdOptions( Flags, null );

			FormsLogger.Log( " ... getting subfolders for: " + FolderMatch );
			IList<string> Folders = ServerRepository.GetDepotDirs( new List<string> { FolderMatch }, FolderOptions );
			FormsLogger.Log( " .... found " + Folders.Count + " folders." );

			return Folders;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private IList<File> GetFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			FileSpec FileRevisionSpec = FileSpecs.FirstOrDefault();
			FormsLogger.Log( " ... getting files for: " + FileRevisionSpec );
			IList<File> Files = ServerRepository.GetFiles( null, FileRevisionSpec );

			return Files;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetSubmittedChangelistsDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			SubmittedChanges.Clear();

			int ChangesToReceive = ( int ) Parameters[0];

			FileSpec FolderMatch = FileSpecs.FirstOrDefault();
			FormsLogger.Log( " ... getting " + ChangesToReceive + " submitted changes in: " + FolderMatch + "..." );

			ChangesCmdOptions ChangesOptions = new ChangesCmdOptions( ChangesCmdFlags.FullDescription, null, ChangesToReceive, ChangeListStatus.Submitted, null );
			IList<Changelist> FoundChanges = ServerRepository.GetChangelists( ChangesOptions, FolderMatch );

			if( FoundChanges != null )
			{
				foreach( Changelist Change in FoundChanges )
				{
					if( !SubmittedChanges.ContainsKey( Change.OwnerName ) )
					{
						SubmittedChanges.Add( Change.OwnerName, new List<Changelist>() );
					}

					SubmittedChanges[Change.OwnerName].Add( Change );
				}

				FormsLogger.Log( " .... found " + FoundChanges.Count + " submitted changes." );
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetPendingChangelistsDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			PendingChanges.Clear();

			int ChangesToReceive = ( int ) Parameters[0];

			FileSpec FolderMatch = FileSpecs.FirstOrDefault();
			FormsLogger.Log( " ... getting " + ChangesToReceive + " pending changes in: " + FolderMatch + "..." );

			ChangesCmdOptions ChangesOptions = new ChangesCmdOptions( ChangesCmdFlags.FullDescription, CachedWorkspace, ChangesToReceive, ChangeListStatus.Pending, CachedUserName );
			IList<Changelist> FoundChanges = ServerRepository.GetChangelists( ChangesOptions, FolderMatch );

			if( FoundChanges != null )
			{
				PendingChanges = FoundChanges;
				FormsLogger.Log( " .... found " + FoundChanges.Count + " pending changes." );
			}

			// Get any files in the default changelist
			GetOpenedFilesOptions OpenedOptions = new GetOpenedFilesOptions( GetOpenedFilesCmdFlags.None, null, CachedWorkspace, CachedUserName, 0 );
			IList<File> OpenedFiles = ServerRepository.GetOpenedFiles( FileSpecs, OpenedOptions );

			if( OpenedFiles != null )
			{
				// FIXME: Handle large numbers of files
				List<File> DefaultChangeFiles = OpenedFiles.Where( x => x.ChangeId == 0 ).ToList();
				if( DefaultChangeFiles.Count > 0 )
				{
					Changelist DefaultChangelist = new Changelist( 0, true );
					DefaultChangelist.Files = DefaultChangeFiles.ConvertAll( x => new FileMetaData( x ) );
					PendingChanges.Insert( 0, DefaultChangelist );
				}
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetFileMetaDataForDefaultChangelistDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			// Get any files in the default changelist
			GetOpenedFilesOptions OpenedOptions = new GetOpenedFilesOptions( GetOpenedFilesCmdFlags.None, null, CachedWorkspace, CachedUserName, 0 );
			IList<File> OpenedFiles = ServerRepository.GetOpenedFiles( FileSpecs, OpenedOptions );

			List<File> DefaultFiles = OpenedFiles.ToList().Where( x => x.ChangeId == 0 ).ToList();
			IList<FileMetaData> OpenedFileMetaDatas = DefaultFiles.ConvertAll( x => new FileMetaData( x ) );

			return OpenedFileMetaDatas;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetEmptyPendingChangelistsDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			List<Changelist> EmptyPendingChanges = new List<Changelist>();
			int ChangesToReceive = ( int ) Parameters[0];

			FormsLogger.Log( " ... getting " + ChangesToReceive + " empty pending changes" );

			ChangesCmdOptions ChangesOptions = new ChangesCmdOptions( ChangesCmdFlags.FullDescription, CachedWorkspace, ChangesToReceive, ChangeListStatus.Pending, CachedUserName );
			IList<Changelist> FoundChanges = ServerRepository.GetChangelists( ChangesOptions, null );

			if( FoundChanges != null )
			{
				FormsLogger.Log( " .... found " + FoundChanges.Count + " pending changes." );

				// Get all the pending changes without shelved files
				List<Changelist> PendingChanges = FoundChanges.ToList().Where( x => x.Pending && !x.Shelved ).Select( x => x ).ToList();
				foreach( Changelist Change in PendingChanges )
				{
					string ChangelistParameter = Change.Id.ToString( CultureInfo.InvariantCulture );
					GetFileMetaDataCmdOptions MetaDataOptions = new GetFileMetaDataCmdOptions( GetFileMetadataCmdFlags.Opened, null, null, 0, null, ChangelistParameter );
					IList<FileMetaData> Files = ServerRepository.GetFileMetaData( FileSpecs, MetaDataOptions );
					if( Files == null || Files.Count == 0 )
					{
						EmptyPendingChanges.Add( Change );
					}
				}
			}

			FormsLogger.Log( " .... found " + EmptyPendingChanges.Count + " pending changes with no pending or shelved files." );
			return EmptyPendingChanges;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object DeleteEmptyChangelistDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			Changelist Change = ( Changelist ) Parameters[0];
			ServerRepository.DeleteChangelist( Change, null );

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object GetLabelsDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			Labels = new List<Label>();

			string FileMatch = ( string ) Parameters[0];
			FormsLogger.Log( " ... retrieving labels from server filtered by: " + FileMatch );

			LabelsCmdOptions Options = new LabelsCmdOptions( LabelsCmdFlags.None, null, null, 0, null );
			IList<Label> FoundLabels = ServerRepository.GetLabels( Options, FileSpec.DepotSpec( FileMatch ) );
			if( FoundLabels != null )
			{
				Labels = FoundLabels;
			}

			FormsLogger.Success( " ... found " + Labels.Count + " label(s)." );
			return true;
		}

		/// <summary>
		/// Create a new changelist
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private object CreateChangelistDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			string Description = ( string ) Parameters[0];
			FormsLogger.Log( " ... creating new changelist" );

			Changelist NewChangelist = new Changelist();
			NewChangelist.Description = Description;
			NewChangelist = ServerRepository.CreateChangelist( NewChangelist );

			FormsLogger.Log( " ... created changelist " + NewChangelist.Id );
			return NewChangelist.Id;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object ReopenFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			int ChangelistId = ( int ) Parameters[0];
			FormsLogger.Log( " ... moving " + FileSpecs.Count + " files to changelist " + ChangelistId );

			ReopenCmdOptions ReopenOptions = new ReopenCmdOptions( ChangelistId, null );
			IList<FileSpec> ReopenedFiles = ServerRepository.Connection.Client.ReopenFiles( FileSpecs, ReopenOptions );
			return ReopenedFiles;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object SubmitDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			int ChangelistId = ( int ) Parameters[0];
			FormsLogger.Log( " ... submitting changelist " + ChangelistId );

			SubmitCmdOptions SubmitOptions = new SubmitCmdOptions( SubmitFilesCmdFlags.None, ChangelistId, null, null, null );
			SubmitResults Results = ServerRepository.Connection.Client.SubmitFiles( SubmitOptions, null );
			FormsLogger.Log( " ... submitted " + Results.Files.Count + " files in changelist " + Results.ChangeIdAfterSubmit );

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private object DeleteShelvedFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			int ChangelistId = ( int )Parameters[0];
			FormsLogger.Log( " ... deleting shelved files in changelist " + ChangelistId );

			ShelveFilesCmdOptions Options = new ShelveFilesCmdOptions( ShelveFilesCmdFlags.Delete, null, ChangelistId );
			List<FileSpec> DeletedFiles = ServerRepository.Connection.Client.ShelveFiles( Options, null );

			FormsLogger.Log( " ... deleted " + DeletedFiles.Count + " files" );
			return true;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private IList<FileMetaData> GetFileMetaDataDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			FormsLogger.Log( " ... getting file details for '" + FileSpecs.FirstOrDefault() + "'." );

			GetFileMetaDataCmdOptions MetaDataOptions = new GetFileMetaDataCmdOptions( GetFileMetadataCmdFlags.FileSize, null, null, 0, null, null );
			return ServerRepository.GetFileMetaData( FileSpecs, MetaDataOptions );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private IList<FileMetaData> GetFileMetaDataForChangelistDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			Changelist Change = ( Changelist ) Parameters[0];
			string ChangelistParameter = Change.Id.ToString( CultureInfo.InvariantCulture );
			FormsLogger.Log( " ... getting change details for change '" + ChangelistParameter + "'." );

			GetFileMetaDataCmdOptions MetaDataOptions = null;
			if( Change.Pending )
			{
				MetaDataOptions = new GetFileMetaDataCmdOptions( GetFileMetadataCmdFlags.FileSize | GetFileMetadataCmdFlags.Opened, null, null, 0, null, ChangelistParameter );
			}
			else
			{
				MetaDataOptions = new GetFileMetaDataCmdOptions( GetFileMetadataCmdFlags.FileSize, null, null, 0, null, ChangelistParameter );
			}

			return ServerRepository.GetFileMetaData( FileSpecs, MetaDataOptions );
		}

		/// <summary>
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private IList<FileSpec> SyncFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			SyncFilesCmdOptions Options = new SyncFilesCmdOptions( SyncFilesCmdFlags.None, 0 );
			return ServerRepository.Connection.Client.SyncFiles( FileSpecs, Options );
		}

		/// <summary>
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private IList<FileSpec> ForceSyncFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			SyncFilesCmdOptions Options = new SyncFilesCmdOptions( SyncFilesCmdFlags.Force, 0 );
			return ServerRepository.Connection.Client.SyncFiles( FileSpecs, Options );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private IList<FileSpec> EditFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			FormsLogger.Log( " ... marking " + FileSpecs.Count + " files for edit." );

			int ChangelistId = ( int ) Parameters[0];
			EditCmdOptions Options = new EditCmdOptions( EditFilesCmdFlags.None, ChangelistId, null );
			return ServerRepository.Connection.Client.EditFiles( FileSpecs, Options );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private IList<FileSpec> AddFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			FormsLogger.Log( " ... marking " + FileSpecs.Count + " files for add." );

			int ChangelistId = ( int ) Parameters[0];
			AddFilesCmdOptions Options = new AddFilesCmdOptions( AddFilesCmdFlags.None, ChangelistId, null );
			return ServerRepository.Connection.Client.AddFiles( FileSpecs, Options );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private IList<FileSpec> DeleteFilesDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			FormsLogger.Log( " ... marking " + FileSpecs.Count + " files for delete." );

			int ChangelistId = ( int ) Parameters[0];
			DeleteFilesCmdOptions Options = new DeleteFilesCmdOptions( DeleteFilesCmdFlags.None, ChangelistId );
			return ServerRepository.Connection.Client.DeleteFiles( FileSpecs, Options );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private string MapClientFileDelegate( IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			string LocalPath = "";
			List<FileSpec> MappedFiles = ServerRepository.Connection.Client.GetClientFileMappings( FileSpecs );
			if( MappedFiles != null && MappedFiles.Count == 1 )
			{
				LocalPath = MappedFiles[0].LocalPath.Path;
			}

			return LocalPath;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <typeparam name="Container"></typeparam>
		/// <param name="Operation"></param>
		/// <param name="Timeout"></param>
		/// <param name="FileSpecs"></param>
		/// <param name="Parameters"></param>
		/// <returns></returns>
		private Container SandboxedOperation<Container>( SandboxedOperationDelegate Operation, int Timeout, IList<FileSpec> FileSpecs, params object[] Parameters )
		{
			Container Result = default( Container );

			if( ServerRepository != null )
			{
				int MaxTimeout = Timeout * 5;
				int Increment = Timeout;
				int RetryCount = RootApplication.Config.RetryCount;

				while( RetryCount >= 0 && Timeout < MaxTimeout )
				{
					try
					{
						ServerRepository.Connection.CommandTimeout = new TimeSpan( 0, Timeout, 0 );
						Result = ( Container ) Operation( FileSpecs, Parameters );

						RetryCount = -1;
					}
					catch( P4Exception Ex )
					{
						RetryCount--;
						Timeout += Increment;
						FormsLogger.Error( " ... exception during Perforce operation; retrying with a timeout of " + Timeout + " minutes. Exception: " + Ex.Message );

						RefreshConnection();
					}
				}
			}

			return Result;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		private Changelist FindChangelist( string UserName, int ChangelistId )
		{
			Changelist Change = null;

			Change = PendingChanges.Where( x => x.Id == ChangelistId ).FirstOrDefault();
			if( Change == null )
			{
				if( SubmittedChanges.ContainsKey( UserName ) )
				{
					List<Changelist> Changes = SubmittedChanges[UserName];
					Change = Changes.Where( x => x.Id == ChangelistId ).FirstOrDefault();
				}
			}

			return Change;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		private Changelist FindPendingChangelist( int ChangelistId  )
		{
			return PendingChanges.Where( x => x.Id == ChangelistId ).FirstOrDefault();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		public bool IsChangelistPending( int ChangelistId )
		{
			return PendingChanges.Any( x => x.Id == ChangelistId );
		}

		/// <summary>
		/// </summary>
		public void ResetConnection()
		{
			// Clear out all known data about the server barring its display name and ticket name
			if( ServerRepository != null )
			{
				ServerUsers.Clear();
				Workspaces.Clear();
				Labels.Clear();
				SubmittedChanges.Clear();

				try
				{
					ServerRepository.Connection.Disconnect();
				}
				catch
				{
				}

				ServerRepository = null;

				CachedUserName = "";
				CachedTicket = "";
				CachedWorkspace = "";
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="ServerURL"></param>
		public bool ConnectWithoutCredentials( string ServerURL )
		{
			// Connect to the current server
			if( !String.IsNullOrEmpty( ServerURL ) )
			{
				FormsLogger.Title( "Connecting without credentials to '" + ServerURL + "'" );

				ServerDisplayName = ServerURL;

				// Create a new server instance
				Server PerforceServer = new Server( new ServerAddress( ServerURL ) );
				try
				{
					// Create a new repository instance
					ServerRepository = new Repository( PerforceServer );

					// Attempt to establish a connection to a repository
					ServerRepository.Connection.Connect( null );

					// Get the server Uri to correlate against the ticket
					ServerMetaData MetaData = ServerRepository.GetServerMetaData( null );
					if( MetaData.LicenseIp != null )
					{
						ServerTicketName = MetaData.LicenseIp;
					}
					else
					{
						ServerTicketName = MetaData.Address.Uri;
					}

					FormsLogger.Success( " ... found server '" + ServerURL + "' (" + MetaData.Version.Major + "/" + MetaData.Version.Platform + ")" );
				}
				catch( Exception Ex )
				{
					List<string> Lines = Ex.Message.Split( "\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
					Lines.ForEach( x => FormsLogger.Error( " ... connection threw an exception '" + x + "'" ) );
					ServerRepository = null;
					ServerTicketName = "";
				}
			}
			else
			{
				FormsLogger.Error( "Cannot connect to a blank server!" );
				ServerTicketName = "";
			}

			return !String.IsNullOrEmpty( ServerTicketName );
		}

		/// <summary>
		/// </summary>
		/// <param name="ServerDisplayName"></param>
		/// <param name="UserName"></param>
		/// <param name="Ticket"></param>
		private bool Connect( string UserName, string Ticket )
		{
			FormsLogger.Title( "Connecting with user '" + UserName + "' to '" + ServerDisplayName + "' using a ticket." );

			// Create a new server instance
			Server PerforceServer = new Server( new ServerAddress( ServerDisplayName ) );
			try
			{
				// Create a new repository instance
				ServerRepository = new Repository( PerforceServer );

				// Attempt to establish a connection to a repository
				Options ConnectOptions = new Options();
				ConnectOptions["Ticket"] = Ticket;
				ServerRepository.Connection.UserName = UserName;
				ServerRepository.Connection.Connect( ConnectOptions );

				if( ServerRepository.Connection.LastResults.Success )
				{
					// Set the default timeout
					ServerRepository.Connection.CommandTimeout = new TimeSpan( 0, 0, RootApplication.Config.CommandTimeoutSeconds );

					CachedUserName = UserName;
					CachedTicket = Ticket;

					FormsLogger.Success( " ... connected to '" + ServerDisplayName + "' as '" + UserName + "'" );
					return true;
				}
				else
				{
					ServerRepository.Connection.LastResults.ErrorList.ForEach( x => FormsLogger.Error( " ... connection threw an exception '" + x.ToString().Replace( "\n", "" ) + "'" ) );
				}
			}
			catch( Exception Ex )
			{
				List<string> Lines = Ex.Message.Split( "\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
				Lines.ForEach( x => FormsLogger.Error( " ... connection threw an exception '" + x + "'" ) );
				ServerRepository = null;
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="Password"></param>
		private bool ConnectWithPassword( string UserName, string Password )
		{
			FormsLogger.Title( "Connecting with user '" + UserName + "' to '" + ServerDisplayName + "' using a password." );

			// Create a new server instance
			Server PerforceServer = new Server( new ServerAddress( ServerDisplayName ) );
			try
			{
				// Create a new repository instance
				ServerRepository = new Repository( PerforceServer );

				ServerRepository.Connection.UserName = UserName;
				ServerRepository.Connection.Connect( null );

				ServerRepository.Connection.Login( Password );

				FormsLogger.Success( " ... connected to '" + ServerDisplayName + "' as '" + UserName + "'" );

				ResetConnection();
				return true;
			}
			catch( Exception Ex )
			{
				List<string> Lines = Ex.Message.Split( "\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries ).ToList();
				Lines.ForEach( x => FormsLogger.Error( " ... connection threw an exception '" + x + "'" ) );
				ServerRepository = null;
			}

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="UserName"></param>
		public bool ReconnectWithCredentials( string UserName )
		{
			// Retrieve the ticket
			string Ticket = RootApplication.Config.GetUserTicket( ServerTicketName, UserName );
			if( Ticket.Length > 0 )
			{
				// Cleanup any existing connections
				ResetConnection();

				if( Connect( UserName, Ticket ) )
				{
					return true;
				}
			}

			FormsLogger.Error( "Failed to find a ticket for user '" + UserName + "' on server '" + ServerDisplayName + "'" );

			using( EnterPassword Dialog = new EnterPassword() )
			{
				Dialog.EnterPasswordText.Text =
					"ResilientP4 could not find a valid ticket for user '" + UserName + "' on server '" + ServerDisplayName + "'; " +
					"please enter the password for this user. Alternatively, login with your preferred Perforce client (e.g. P4v) and restart this application " +
					"to use that ticket";

				if( Dialog.ShowDialog() == DialogResult.OK )
				{
					ResetConnection();

					if( ConnectWithPassword( UserName, Dialog.PasswordTextBox.Text ) )
					{
						RootApplication.Config.FindAvailableServers();

						return ReconnectWithCredentials( UserName );
					}
					else
					{
						ConnectWithoutCredentials( ServerDisplayName );
					}
				}
			}

			return false;
		}

		/// <summary>
		/// </summary>
		private void RefreshConnection()
		{
			string Workspace = CachedWorkspace;

			if( ReconnectWithCredentials( CachedUserName ) )
			{
				SetCurrentWorkspace( Workspace );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="SelectedWorkspace"></param>
		public void SetCurrentWorkspace( string SelectedWorkspace )
		{
			CachedWorkspace = SelectedWorkspace;
			SandboxedOperation<object>( SetCurrentWorkspaceDelegate, 2, null );
		}

		/// <summary>
		/// </summary>
		/// <param name="UserName"></param>
		public bool GetUsersOnServer( string UserName )
		{
			// Get all available users from the server
			if( string.IsNullOrEmpty( UserName ) )
			{
				UserName = "*";
			}

			SandboxedOperation<object>( GetUsersOnServerDelegate, 2, null, UserName, ServerDisplayName );

			return ServerUsers.Count > 0;
		}

		/// <summary>
		/// </summary>
		public void GetWorkspaces( string SelectedUserName )
		{
			SandboxedOperation<object>( GetWorkspacesDelegate, 2, null, SelectedUserName );
		}

		/// <summary>
		/// </summary>
		/// <param name="FileMatch"></param>
		public void GetLabels( string FileMatch )
		{
			SandboxedOperation<object>( GetLabelsDelegate, 2, null, FileMatch );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotPath"></param>
		/// <returns></returns>
		public string GetLocalBranchPath( string DepotPath )
		{
			IList<FileSpec> DepotFileSpecs = FileSpec.DepotSpecList( DepotPath.TrimEnd( "/".ToCharArray() ) );
			return SandboxedOperation<string>( MapClientFileDelegate, 2, DepotFileSpecs );		
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <param name="FilesToAdd"></param>
		public void MarkForAdd( int ChangelistId, List<string> FilesToAdd )
		{
			if( FilesToAdd.Count > 0 )
			{
				IList<FileSpec> FileSpecsToAdd = FileSpec.DepotSpecList( FilesToAdd.ToArray() );
				SandboxedOperation<List<FileSpec>>( AddFilesDelegate, 2, FileSpecsToAdd, ChangelistId );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <param name="FilesToDelete"></param>
		public void MarkForDelete( int ChangelistId, List<string> FilesToDelete )
		{
			// Delete files
			if( FilesToDelete.Count > 0 )
			{
				IList<FileSpec> FileSpecsToDelete = FileSpec.DepotSpecList( FilesToDelete.ToArray() );
				SandboxedOperation<List<FileSpec>>( DeleteFilesDelegate, 2, FileSpecsToDelete, ChangelistId );
			}	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <param name="FilesToEdit"></param>
		public void MarkForEdit( int ChangelistId, List<string> FilesToEdit )
		{
			if( FilesToEdit.Count > 0 )
			{
				IList<FileSpec> FileSpecsToEdit = FileSpec.DepotSpecList( FilesToEdit.ToArray() );
				SandboxedOperation<List<FileSpec>>( EditFilesDelegate, 2, FileSpecsToEdit, ChangelistId );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		private bool CheckShelvedFiles( int ChangelistId )
		{
			bool bIsSubmittable = true;
			Changelist Change = FindPendingChangelist( ChangelistId );
			if( Change != null )
			{
				if( Change.Shelved )
				{
					string ConfirmationMessage = "Change " + ChangelistId + " has shelved files. Do you wish to delete the shelved files? (a changelist cannot be submitted if it has shelved files)";
					if( MessageBox.Show( ConfirmationMessage, "Shelved Files", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
					{
						SandboxedOperation<bool>( DeleteShelvedFilesDelegate, 2, null, ChangelistId );
					}
					else
					{
						FormsLogger.Warning( " ... submit aborted" );
						bIsSubmittable = false;
					}
				}
			}

			return bIsSubmittable;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		private int HandleDefaultChangelist()
		{
			int NewChangeId = 0;
			Changelist Change = FindPendingChangelist( 0 );
			if( Change != null )
			{
				string ConfirmationMessage = "Submitting the default changelist requires the files be moved to a named and numbered changelist. Do you wish to continue?";
				if( MessageBox.Show( ConfirmationMessage, "Default Changelist", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
				{
					string Description = "ResilientP4 - submitted default changelist";
					NewChangeId = SandboxedOperation<int>( CreateChangelistDelegate, 2, null, Description );

					if( Change.Files == null || Change.Files.Count == 0 )
					{
						List<FileSpec> EntireDepot = new List<FileSpec>()
						{
							FileSpec.DepotSpec( "//..." )
						};

						Change.Files = SandboxedOperation<IList<FileMetaData>>( GetFileMetaDataForDefaultChangelistDelegate, 5, EntireDepot );
					}

					List<FileSpec> FileSpecs = Change.Files.Select( x => ( FileSpec )x ).ToList();
					FileSpecs.ForEach( x => x.Version = new HaveRevision() );

					List<FileSpec> ReopenedFiles = SandboxedOperation<List<FileSpec>>( ReopenFilesDelegate, 5, FileSpecs, NewChangeId );

					if( ReopenedFiles.Count != FileSpecs.Count )
					{
						FormsLogger.Warning( " ... failed to move files in default changelist; submit aborted" );
						NewChangeId = 0;
					}
				}
				else
				{
					FormsLogger.Warning( " ... default changelist unchanged; submit aborted" );
					NewChangeId = 0;					
				}
			}

			return NewChangeId;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangeslistIds"></param>
		public bool Submit( List<int> ChangeslistIds )
		{
			// Check for shelved files
			int NewChangelistId = 0;
			foreach( int ChangelistId in ChangeslistIds )
			{
				if( ChangelistId == 0 )
				{
					NewChangelistId = HandleDefaultChangelist();
					if( NewChangelistId == 0 )
					{
						return false;
					}
				}

				if( !CheckShelvedFiles( ChangelistId ) )
				{
					return false;
				}
			}

			// Update the list of changes to submit if the files in the default changelist have been moved
			if( NewChangelistId != 0 )
			{
				ChangeslistIds.Remove( 0 );
				ChangeslistIds.Insert( 0, NewChangelistId );
			}

			// Confirm submission
			string ConfirmationMessage = "This will submit " + ChangeslistIds.Count + " changelist(s) to '" + SafeServerDisplayName + "'";
			ConfirmationMessage += Environment.NewLine + Environment.NewLine;
			ConfirmationMessage += "Are you sure?";
			if( MessageBox.Show( ConfirmationMessage, "Confirm Submission", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
			{
				foreach( int ChangelistId in ChangeslistIds )
				{
					SandboxedOperation<bool>( SubmitDelegate, 10, null, ChangelistId );
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// </summary>
		/// <param name="BranchName"></param>
		/// <param name="Version"></param>
		/// <param name="Depth"></param>
		/// <param name="FileRevisions"></param>
		private void RecursivelyGetStructure( string BranchName, VersionSpec Version, bool bIncludeDeletedFolders, int Depth, ref List<string> FileRevisions )
		{
			// Get the subfolders for the current folder
			string FolderMatch = BranchName + "*" + Version;
			IList<string> Folders = GetDepotDirectories( FolderMatch, bIncludeDeletedFolders );

			if( Folders != null )
			{
				// Use the * wildcard if we will be recursing, otherwise ... to get all files in all subfolders
				string Wildcard = "...";
				if( Depth < RootApplication.Config.FolderRecursionDepth )
				{
					Wildcard = "*";
				}

				// Get the files and revisions associated with the label
				List<FileSpec> FileSpecs = new List<FileSpec>()
				{
					FileSpec.DepotSpec( BranchName + Wildcard + Version )
				};

				List<File> Files = SandboxedOperation<List<File>>( GetFilesDelegate, 2, FileSpecs );
				if( Files != null )
				{
					FormsLogger.Log( " .... found " + Files.Count + " files." );
					FileRevisions.AddRange( Files.Select( x => x.DepotPath.Path ) );
				}
				else
				{
					FormsLogger.Log( " .... found no files." );
				}

				// Recurse into any subfolders
				if( Wildcard == "*" )
				{
					foreach( string Folder in Folders )
					{
						RecursivelyGetStructure( Folder + "/", Version, bIncludeDeletedFolders, Depth + 1, ref FileRevisions );
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="FileNames"></param>
		/// <param name="RevisionName"></param>
		/// <returns></returns>
		private RevisionData GetFileDetails( string BranchName, List<string> FileNames, VersionSpec RevisionName, string Description )
		{
			RevisionData FileDetails = new RevisionData( BranchName, RevisionName.ToString(), Description );
			int FileCountToGrab = RootApplication.Config.MaxNumberOfFilesPerChunk;

			for( int Index = 0; Index < FileNames.Count; Index += FileCountToGrab )
			{
				IEnumerable<string> FileNamesSection = FileNames.Skip( Index ).Take( FileCountToGrab );
				FormsLogger.Log( " ... getting " + FileNamesSection.Count() + " files starting at " + Index );
				List<FileSpec> FileSpecs = FileSpec.DepotSpecList( FileNamesSection.ToArray() ).ToList();
				FileSpecs.ForEach( x => x.Version = RevisionName );

				IList<FileMetaData> VersionFileMetaData = SandboxedOperation<IList<FileMetaData>>( GetFileMetaDataDelegate, 2, FileSpecs );

				if( VersionFileMetaData != null )
				{
					FormsLogger.Log( " .... creating data for files at version." );
					foreach( FileMetaData FileDetail in VersionFileMetaData )
					{
						FileDetails.AddFile( FileDetail );
					}
				}
			}

			return FileDetails;
		}

		/// <summary>
		/// </summary>
		/// <param name="UpdateFiles"></param>
		/// <param name="LabelData"></param>
		/// <returns></returns>
		private List<List<string>> SplitFileSets( List<string> UpdateFiles, RevisionData LabelData )
		{
			List<List<string>> FileSets = new List<List<string>>();
			List<string> FileSet = null;
			int CurrentSetCount = 0;
			long CurrentSetSizeKB = 0;

			foreach( string UpdateFile in UpdateFiles )
			{
				if( FileSet == null )
				{
					FileSet = new List<string>();
					CurrentSetCount = 0;
					CurrentSetSizeKB = 0;
				}

				FileSet.Add( UpdateFile );

				CurrentSetCount++;
				if( LabelData != null )
				{
					CurrentSetSizeKB += 1 + ( LabelData.GetFileSize( UpdateFile ) / 1024 );
				}

				if( CurrentSetCount >= RootApplication.Config.MaxNumberOfFilesPerChunk || CurrentSetSizeKB >= RootApplication.Config.MaxFileSizePerChunkKB )
				{
					FileSets.Add( FileSet );
					FileSet = null;

					FormsLogger.Log( " ... generated file set with " + CurrentSetCount + " files with a total size of " + CurrentSetSizeKB + " kB" );
				}
			}

			if( FileSet != null && FileSet.Count > 0 )
			{
				FileSets.Add( FileSet );
			}

			return FileSets;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Change"></param>
		/// <returns></returns>
		private List<List<string>> SplitFileSets( Changelist Change )
		{
			List<List<string>> FileSets = new List<List<string>>();
			List<string> FileSet = null;
			int CurrentSetCount = 0;
			long CurrentSetSizeKB = 0;

			foreach( FileMetaData LocalFile in Change.Files )
			{
				if( LocalFile.LocalPath != null )
				{
					FileInfo Info = new FileInfo( LocalFile.LocalPath.Path );
					if( Info.Exists )
					{
						if( FileSet == null )
						{
							FileSet = new List<string>();
							CurrentSetCount = 0;
							CurrentSetSizeKB = 0;
						}

						FileSet.Add( LocalFile.LocalPath.Path );

						CurrentSetCount++;
						CurrentSetSizeKB += Info.Length / 1024;

						if( CurrentSetCount >= RootApplication.Config.MaxNumberOfFilesPerChunk || CurrentSetSizeKB >= RootApplication.Config.MaxFileSizePerChunkKB )
						{
							FileSets.Add( FileSet );
							FileSet = null;

							FormsLogger.Log( " ... generated file set with " + CurrentSetCount + " files with a total size of " + CurrentSetSizeKB + " kB" );
						}
					}
				}
			}

			if( FileSet != null && FileSet.Count > 0 )
			{
				FileSets.Add( FileSet );
			}

			return FileSets;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileSets"></param>
		/// <param name="LabelData"></param>
		private void SyncFileSets( List<List<string>> FileSets, RevisionData LabelData )
		{
			foreach( List<string> FileSet in FileSets )
			{
				FormsLogger.Log( "Syncing " + FileSet.Count + " files ..." );

				List<FileSpec> FileSpecs = new List<FileSpec>();
				FileSet.ForEach( x => FileSpecs.Add( FileSpec.DepotSpec( x, LabelData.GetHaveRevision( x ) ) ) );

				SandboxedOperation<List<FileSpec>>( SyncFilesDelegate, 12, FileSpecs );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="LabelData"></param>
		private void SyncRevision( RevisionData FileData )
		{
			FormsLogger.Log( "Syncing " + FileData.DepotFiles.Count + " files from changelist " + FileData.Revision + " ..." );
			List<FileSpec> FileSpecs = new List<FileSpec>();
			FileData.DepotFiles.Keys.ToList().ForEach( x => FileSpecs.Add( FileSpec.DepotSpec( x, FileData.GetHeadRevision( x ) ) ) );

            // Sync any files that may not be in sync
			SandboxedOperation<List<FileSpec>>( SyncFilesDelegate, 12, FileSpecs );
		}

		/// <summary>
		/// </summary>
		/// <param name="FileSets"></param>
		private void DeleteFileSets( List<List<string>> FileSets )
		{
			foreach( List<string> FileSet in FileSets )
			{
				FormsLogger.Log( "Deleting " + FileSet.Count + " files ..." );

				List<FileSpec> FileSpecs = new List<FileSpec>();
				FileSet.ForEach( x => FileSpecs.Add( FileSpec.DepotSpec( x ) ) );

				SandboxedOperation<List<FileSpec>>( SyncFilesDelegate, 2, FileSpecs );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="BranchName"></param>
		/// <param name="LabelName"></param>
		public void SyncToLabel( string BranchName, string LabelName )
		{
			RevisionData HaveData = GetFileData( BranchName, new HaveRevision(), false );
			RevisionData LabelData = GetFileData( BranchName, new LabelNameVersion( LabelName.TrimStart( '@' ) ), false );

			FormsLogger.Title( "Summary of files to sync:" );

			List<string> OldFilesToDelete = HaveData.GetDeletedFiles( LabelData ).ToList();
			List<string> NewFilesToAdd = HaveData.GetNewFiles( LabelData ).ToList();
			int UnchangedFiles = HaveData.GetIdenticalFileCount( LabelData );
			List<string> UpdatedFiles = HaveData.GetDifferingFiles( LabelData ).ToList();

			FormsLogger.Title( " ... " + UnchangedFiles + " files are unchanged." );
			FormsLogger.Title( " ... " + NewFilesToAdd.Count + " new files to add." );
			FormsLogger.Title( " ... " + OldFilesToDelete.Count + " old files to delete." );
			FormsLogger.Title( " ... " + UpdatedFiles.Count + " files are updated." );

			if( OldFilesToDelete.Count > 0 )
			{
				FormsLogger.Title( "Splitting " + OldFilesToDelete.Count + " old files to delete into manageable chunks." );
				List<List<string>> FileSets = SplitFileSets( OldFilesToDelete, null );

				FormsLogger.Title( "Deleting " + FileSets.Count + " file sets." );
				DeleteFileSets( FileSets );
				FormsLogger.Title( " ... completed!" );
			}

			if( UpdatedFiles.Count > 0 )
			{
				FormsLogger.Title( "Splitting " + UpdatedFiles.Count + " updated files into manageable chunks." );
				List<List<string>> FileSets = SplitFileSets( UpdatedFiles, LabelData );

				FormsLogger.Title( "Updating " + FileSets.Count + " file sets." );
				SyncFileSets( FileSets, LabelData );
				FormsLogger.Title( " ... completed!" );
			}

			if( NewFilesToAdd.Count > 0 )
			{
				FormsLogger.Title( "Splitting " + NewFilesToAdd.Count + " new files into manageable chunks." );
				List<List<string>> FileSets = SplitFileSets( NewFilesToAdd, LabelData );

				FormsLogger.Title( "Adding " + FileSets.Count + " file sets." );
				SyncFileSets( FileSets, LabelData );
				FormsLogger.Title( " ... completed!" );
			}

			// Validate the sync did the right thing
			List<string> ModifiedFiles = OldFilesToDelete.Concat( UpdatedFiles ).Concat( NewFilesToAdd ).ToList();
			RevisionData NewHaveData = GetFileData( BranchName, new HaveRevision(), ModifiedFiles );
			// FIXME
			//RevisionData LocalData = GetLocalFileData( NewHaveData, BranchName, "?", ModifiedFiles );

			// Make sure the local data matches the depot have data
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FilesToAdd"></param>
		/// <param name="FilesToDelete"></param>
		/// <param name="FilesEdit"></param>
		private void WriteBatFile( string BatFileName, ReconcileData FilesToUpdate )
		{
			FileInfo BatFileInfo = new FileInfo( BatFileName );
			try
			{
				if( BatFileInfo.Exists && BatFileInfo.IsReadOnly )
				{
					BatFileInfo.IsReadOnly = false;
				}

				if( BatFileInfo.Exists )
				{
					BatFileInfo.Delete();
					BatFileInfo.Refresh();
				}

				int BranchNameLength = FilesToUpdate.BranchName.Length;

				using( StreamWriter Writer = new StreamWriter( BatFileInfo.FullName, false, Encoding.ASCII ) )
				{
					Writer.WriteLine( "@echo off" );
					Writer.WriteLine( "" );

					Writer.WriteLine( "p4 set P4PORT=P4Server:1666" );
					Writer.WriteLine( "p4 set P4USER=Authed.User" );
					Writer.WriteLine( "p4 set P4CLIENT=WORKSPACE" );
					Writer.WriteLine( "" );

					Writer.WriteLine( "rem DepotRemotePath is the Perforce location on the depot e.g. //depot/Branch" );
					Writer.WriteLine( "set DepotRemotePath=%1" );
					Writer.WriteLine( "rem LocalDestFilePath is the local file path of the branch e.g. d:/depot/Branch" );
					Writer.WriteLine( "set LocalDestFilePath=%2" );

					Writer.WriteLine( "set DepotRemotePath=%DepotRemotePath:\"=%" );
					Writer.WriteLine( "set LocalDestFilePath=%LocalDestFilePath:\"=%" );

					Writer.WriteLine( "" );

					if( FilesToUpdate.FilesToDelete.Count > 0 )
					{
						foreach( string FileToDelete in FilesToUpdate.FilesToDelete )
						{
							string FileToOperate = FileToDelete.Substring( BranchNameLength );
							Writer.WriteLine( "p4 delete \"%DepotRemotePath%/" + FileToOperate + "\"" );
						}

						Writer.WriteLine( "" );
					}

					if( FilesToUpdate.FilesToAdd.Count > 0 )
					{
						foreach( string FileToAdd in FilesToUpdate.FilesToAdd )
						{
							string PathToOperate = FileToAdd.Substring( BranchNameLength );
							string DirectoryToOperate = Path.GetDirectoryName( PathToOperate );
							Writer.WriteLine( "mkdir \"%LocalDestFilePath%/" + DirectoryToOperate + "\"" );
							Writer.WriteLine( "xcopy /S /F /Y /I \"" + PathToOperate + "\" \"%LocalDestFilePath%/" + DirectoryToOperate + "\"" );
							Writer.WriteLine( "p4 add \"%DepotRemotePath%/" + PathToOperate + "\"" );
							Writer.WriteLine( "" );
						}
					}

					if( FilesToUpdate.FilesToEdit.Count > 0 )
					{
						foreach( string FileToEdit in FilesToUpdate.FilesToEdit )
						{
							string PathToOperate = FileToEdit.Substring( BranchNameLength );
							string DirectoryToOperate = Path.GetDirectoryName( PathToOperate );
							Writer.WriteLine( "mkdir \"%LocalDestFilePath%/" + DirectoryToOperate + "\"" );
							Writer.WriteLine( "p4 edit \"%DepotRemotePath%/" + PathToOperate + "\"" );
							Writer.WriteLine( "xcopy /S /F /Y /I \"" + PathToOperate + "\" \"%LocalDestFilePath%/" + DirectoryToOperate + "\"" );
							Writer.WriteLine( "" );
						}
					}
				}
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during write of " + BatFileInfo.FullName + " with exception " + Ex.Message );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <param name="FilesToAdd"></param>
		/// <param name="FilesToDelete"></param>
		/// <param name="FilesEdit"></param>
		private void WriteBashFile( string BashFileName, ReconcileData FilesToUpdate )
		{
			FileInfo BashFileInfo = new FileInfo( BashFileName );
			try
			{
				if( BashFileInfo.Exists && BashFileInfo.IsReadOnly )
				{
					BashFileInfo.IsReadOnly = false;
				}

				if( BashFileInfo.Exists )
				{
					BashFileInfo.Delete();
					BashFileInfo.Refresh();
				}

				int BranchNameLength = FilesToUpdate.BranchName.Length;
				string MacNewLine = "\n";

				using( StreamWriter Writer = new StreamWriter( BashFileInfo.FullName, false, Encoding.ASCII ) )
				{
					Writer.Write( "#!/bin/bash" + MacNewLine );
					Writer.Write( MacNewLine );
					Writer.Write( "p4 set P4PORT=P4Server:1666" + MacNewLine );
					Writer.Write( "p4 set P4USER=Authed.User" + MacNewLine );
					Writer.Write( "p4 set P4CLIENT=WORKSPACE" + MacNewLine );
					Writer.Write( MacNewLine );
					Writer.Write( "#DepotRemotePath is the Perforce location on the depot e.g. //depot/Branch" + MacNewLine );
					Writer.Write( "DepotRemotePath=$1" + MacNewLine );
					Writer.Write( "#LocalDestFilePath is the local file path of the branch e.g. /Documents/depot/Branch" + MacNewLine );
					Writer.Write( "LocalDestFilePath=$2" + MacNewLine );

					Writer.Write( "DepotRemotePath=${DepotRemotePath//\\\"/}" + MacNewLine );
					Writer.Write( "LocalDestFilePath=${LocalDestFilePath//\\\"/}" + MacNewLine );

					Writer.Write( MacNewLine );

					if( FilesToUpdate.FilesToDelete.Count > 0 )
					{
						foreach( string FileToDelete in FilesToUpdate.FilesToDelete )
						{
							string FileToOperate = FileToDelete.Substring( BranchNameLength ).Replace( "\\", "/" );
							Writer.Write( "p4 delete \"${DepotRemotePath}/" + FileToOperate + "\"" + MacNewLine );
						}

						Writer.Write( MacNewLine );
					}

					if( FilesToUpdate.FilesToAdd.Count > 0 )
					{
						foreach( string FileToAdd in FilesToUpdate.FilesToAdd )
						{
							string PathToOperate = FileToAdd.Substring( BranchNameLength ).Replace( "\\", "/" );
							string DirectoryToOperate = Path.GetDirectoryName( PathToOperate ).Replace( "\\", "/" );
							Writer.Write( "mkdir -p \"${LocalDestFilePath}/" + DirectoryToOperate + "\"" + MacNewLine );
							Writer.Write( "cp \"./" + PathToOperate + "\" \"${LocalDestFilePath}/" + PathToOperate + "\"" + MacNewLine );
							Writer.Write( "p4 add \"${DepotRemotePath}/" + PathToOperate + "\"" + MacNewLine );
							Writer.Write( MacNewLine );
						}
					}

					if( FilesToUpdate.FilesToEdit.Count > 0 )
					{
						foreach( string FileToEdit in FilesToUpdate.FilesToEdit )
						{
							string PathToOperate = FileToEdit.Substring( BranchNameLength ).Replace( "\\", "/" );
							string DirectoryToOperate = Path.GetDirectoryName( PathToOperate ).Replace( "\\", "/" );
							Writer.Write( "mkdir -p \"${LocalDestFilePath}/" + DirectoryToOperate + "\"" + MacNewLine );
							Writer.Write( "p4 edit \"${DepotRemotePath}/" + PathToOperate + "\"" + MacNewLine );
							Writer.Write( "cp \"./" + PathToOperate + "\" \"${LocalDestFilePath}/" + PathToOperate + "\"" + MacNewLine );
							Writer.Write( MacNewLine );
						}
					}
				}
			}
			catch( Exception Ex )
			{
				ConsoleLogger.Error( "Exception during write of " + BashFileInfo.FullName + " with exception " + Ex.Message );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		public void PackageChangelist( string UserName, string BranchName, int ChangelistId, string PackageFileName )
		{
			Changelist Change = FindChangelist( UserName, ChangelistId );
			if( Change != null && Change.Files != null && Change.Files.Count != 0 )
			{
				// Create a RevisionData container with the relevant info
				VersionSpec RevisionName = new ChangelistIdVersion( ChangelistId );
				RevisionData FileData = new RevisionData( BranchName, RevisionName.ToString(), Change.Description );

				foreach( FileMetaData FileDetail in Change.Files )
				{
					FileData.AddFile( FileDetail );
				}

				// Sync to the changelist we wish to package (if it isn't pending)
				if( !Change.Pending )
				{
					SyncRevision( FileData );
				}

				// Work out the local path of the branch name (which is in depotspec format)
				IList<FileSpec> DepotFileSpecs = FileSpec.DepotSpecList( BranchName.TrimEnd( "/".ToCharArray() ) );
				string LocalBranchPath = SandboxedOperation<string>( MapClientFileDelegate, 2, DepotFileSpecs );
				if( !String.IsNullOrEmpty( LocalBranchPath ) )
				{
					int LocalBranchPathLength = LocalBranchPath.Length;

					// Delete any previous packages
					FileInfo PackageInfo = new FileInfo( PackageFileName );
					if( PackageInfo.Exists )
					{
						PackageInfo.Delete();
						PackageInfo.Refresh();
					}

					// Add the files to a zip
					int FileCount = Change.Files.Count - 1;
					FormsLogger.Log( " ... packaging " + FileCount + " files from change " + ChangelistId + "..." );
					ZipFile PackagedChange = new ZipFile( PackageFileName );
					foreach( FileMetaData File in Change.Files )
					{
						if( ShouldArchiveFile( File ) )
						{
							string ArchiveFolder = File.LocalPath.Path.Substring( LocalBranchPathLength );
							PackagedChange.AddFile( File.ClientPath.Path, Path.GetDirectoryName( ArchiveFolder ) );
						}
					}

					// Add the metadata file
					FormsLogger.Log( " ... writing meta data file" );
					string TempFileName = Path.Combine( Path.GetTempPath(), ".metadata" );
					JsonHelper.WriteJsonFile( TempFileName, FileData );
					PackagedChange.AddFile( TempFileName, "." );

					// Construct the bat file to unpackage the change
					ReconcileData FilesToUpdate = EvaluateUnpackageOperations( FileData, BranchName, LocalBranchPath, true );

					FormsLogger.Log( " ... writing bat file" );
					TempFileName = Path.Combine( Path.GetTempPath(), "unpackage.bat" );
					WriteBatFile( TempFileName, FilesToUpdate );
					PackagedChange.AddFile( TempFileName, "." );

					FormsLogger.Log( " ... writing bash file" );
					TempFileName = Path.Combine( Path.GetTempPath(), "unpackage.bash" );
					WriteBashFile( TempFileName, FilesToUpdate );
					PackagedChange.AddFile( TempFileName, "." );

					PackagedChange.Save();

					FormsLogger.Success( "Successfully packaged " + FileCount + " files in package '" + PackageFileName + "'" );
				}
				else
				{
					FormsLogger.Error( "Could not map '" + BranchName + "' to a local path." );
				}
			}
			else
			{
				FormsLogger.Error( "Could not find change to package" );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="BranchName"></param>
		/// <param name="EditCount"></param>
		/// <param name="AddCount"></param>
		/// <param name="DeleteCount"></param>
		/// <returns></returns>
		private string ConstructConfirmationMessage( ReconcileData FilesToUpdate )
		{
			bool bAddComma = false;

			int EditCount = FilesToUpdate.FilesToEdit.Count;
			int AddCount = FilesToUpdate.FilesToAdd.Count;
			int DeleteCount = FilesToUpdate.FilesToDelete.Count;

			string ConfirmationMessage = "This operation will create a new pending changelist, then";
			if( EditCount > 0 )
			{
				ConfirmationMessage += " checkout " + EditCount + " files";
				bAddComma = true;
			}

			if( AddCount > 0 )
			{
				if( bAddComma )
				{
					ConfirmationMessage += ", ";
				}

				ConfirmationMessage += " add " + AddCount + " files";
				bAddComma = true;
			}

			if( DeleteCount > 0 )
			{
				if( bAddComma )
				{
					ConfirmationMessage += ", ";
				}

				ConfirmationMessage += " delete " + DeleteCount + " files";
			}

			ConfirmationMessage += " under '" + FilesToUpdate.BranchName + "...' to that changelist." + Environment.NewLine + Environment.NewLine;
			ConfirmationMessage += "Do you wish to continue?";

			return ConfirmationMessage;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileData"></param>
		/// <param name="BranchName"></param>
		/// <param name="FilesToAdd"></param>
		/// <param name="FilesToDelete"></param>
		/// <param name="FilesToEdit"></param>
		/// <returns></returns>
		private ReconcileData EvaluateUnpackageOperations( RevisionData FileData, string BranchName, string LocalBranchPath, bool bIncludeAll )
		{
			FormsLogger.Log( " ... mapping files from '" + FileData.BranchName + "' to '" + BranchName + "'" );
			List<string> NewFileNames = FileData.RemapFileNames( BranchName ).ToList();
			RevisionData NewFileData = GetFileDetails( BranchName, NewFileNames, new HaveRevision(), FileData.Description );

			ReconcileData FilesToUpdate = new ReconcileData( BranchName, LocalBranchPath );

			foreach( string DepotFileName in FileData.DepotFiles.Keys )
			{
				DepotFileData PackagedFile = FileData.GetDepotFile( DepotFileName );
				DepotFileData DestinationFile = NewFileData.GetDepotFile( DepotFileName );

				string NewName = BranchName + PackagedFile.DepotPath.Substring( FileData.BranchName.Length );

				switch( PackagedFile.HeadAction )
				{
					case FileAction.MoveDelete:
					case FileAction.Delete:
					case FileAction.DeleteFrom:
					case FileAction.DeleteInto:
						if( bIncludeAll || ( DestinationFile != null && DestinationFile.Size >= 0 ) )
						{
							// Mark file for delete
							FilesToUpdate.FilesToDelete.Add( NewName );
						}
						break;

					case FileAction.Add:
					case FileAction.MoveAdd:
					case FileAction.Branch:
						// Calculate the new file name as DestinationFile may be null
						if( bIncludeAll || DestinationFile == null )
						{
							FilesToUpdate.FilesToAdd.Add( NewName );
						}
						break;

					case FileAction.Edit:
					case FileAction.Integrate:
					case FileAction.EditInto:
						FilesToUpdate.FilesToEdit.Add( NewName );
						break;

					default:
						FormsLogger.Warning( " ... unhandled action '" + PackagedFile.HeadAction + "' for file '" + PackagedFile.DepotPath + "'" );
						break;
				}
			}

			return FilesToUpdate;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PackagedChange"></param>
		/// <param name="BranchName"></param>
		/// <param name="ChangelistId"></param>
		/// <param name="FilesToAdd"></param>
		/// <param name="FilesToDelete"></param>
		/// <param name="FilesToEdit"></param>
		private void ExecuteUnpackageOperations( ZipFile PackagedChange, string ChangelistDescription, ReconcileData FilesToUpdate )
		{
			string LocalBranchPath = GetLocalBranchPath( FilesToUpdate.BranchName );
			if( !String.IsNullOrEmpty( LocalBranchPath ) )
			{
				int ChangelistId = SandboxedOperation<int>( CreateChangelistDelegate, 2, null, ChangelistDescription );

				// Delete files
				MarkForDelete( ChangelistId, FilesToUpdate.FilesToDelete );

				// Add files
				foreach( string FileToAdd in FilesToUpdate.FilesToAdd )
				{
					string OldFileName = FileToAdd.Substring( FilesToUpdate.BranchName.Length );
					ZipEntry FileToAddEntry = PackagedChange[OldFileName];
					if( FileToAddEntry != null )
					{
						string ExtractionPath = Path.Combine( LocalBranchPath, OldFileName );
						FileToAddEntry.Extract( Path.GetDirectoryName( ExtractionPath ), ExtractExistingFileAction.OverwriteSilently );
					}

					RootApplication.Tick();
				}

				MarkForAdd( ChangelistId, FilesToUpdate.FilesToAdd );

				// Edit files
				MarkForEdit( ChangelistId, FilesToUpdate.FilesToEdit );

				foreach( string FileToEdit in FilesToUpdate.FilesToEdit )
				{
					string OldFileName = FileToEdit.Substring( FilesToUpdate.BranchName.Length );
					ZipEntry FileToEditEntry = PackagedChange[OldFileName];
					if( FileToEditEntry != null )
					{
						string ExtractionPath = Path.Combine( LocalBranchPath, OldFileName );
						FileToEditEntry.Extract( Path.GetDirectoryName( ExtractionPath ), ExtractExistingFileAction.OverwriteSilently );
					}

					RootApplication.Tick();
				}

				FormsLogger.Success( "Changelist successfully unpackaged!" );
			}
			else
			{
				FormsLogger.Error( "Could not map '" + FilesToUpdate.BranchName + "' to a local path." );
			}	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileFolder"></param>
		/// <param name="DepotBranch"></param>
		/// <param name="FilesToCopy"></param>
		private void CopyFiles( string FileFolder, string DepotBranch, string LocalBranchPath, List<string> FilesToCopy )
		{
			foreach( string FileName in FilesToCopy )
			{
				string SourceFileName = Path.Combine( FileFolder, FileName.Substring( DepotBranch.Length ) );
				FileInfo SourceFileInfo = new FileInfo( SourceFileName );
				if( !SourceFileInfo.Exists )
				{
					FormsLogger.Warning( "Source file '" + SourceFileName + "' does not exist!" );
					continue;
				}

				string DestFileName = Path.Combine( LocalBranchPath, FileName.Substring( DepotBranch.Length ) );
				FileInfo DestFileInfo = new FileInfo( DestFileName );
				if( DestFileInfo.Exists )
				{
					DestFileInfo.IsReadOnly = false;
					DestFileInfo.Refresh();
				}

				Directory.CreateDirectory( DestFileInfo.DirectoryName );
				SourceFileInfo.CopyTo( DestFileInfo.FullName, true );

				RootApplication.Tick();
			}			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileFolder"></param>
		/// <param name="DepotBranch"></param>
		/// <param name="FilesToAdd"></param>
		/// <param name="FilesToDelete"></param>
		/// <param name="FilesToEdit"></param>
		public void ReconcileFiles( string FileFolder, string DepotBranch, ReconcileData Reconcile )
		{
			string LocalBranchPath = GetLocalBranchPath( DepotBranch );
			if( !String.IsNullOrEmpty( LocalBranchPath ) )
			{
				string Description = "Reconciling files from '" + FileFolder + "' to '" + DepotBranch + "'";
				int ChangelistId = SandboxedOperation<int>( CreateChangelistDelegate, 2, null, Description );

				MarkForDelete( ChangelistId, Reconcile.FilesToDelete );

				CopyFiles( FileFolder, DepotBranch, LocalBranchPath, Reconcile.FilesToAdd );
				MarkForAdd( ChangelistId, Reconcile.FilesToAdd );

				MarkForEdit( ChangelistId, Reconcile.FilesToEdit );
				CopyFiles( FileFolder, DepotBranch, LocalBranchPath, Reconcile.FilesToEdit );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="PackageFileName"></param>
		/// <returns></returns>
		public void UnpackageChangelist( string PackageFileName, string BranchName )
		{
			using( ZipFile PackagedChange = ZipFile.Read( PackageFileName ) )
			{
				FormsLogger.Log( " ... reading meta data file" );

				string TempPath = Path.GetTempPath();
				ZipEntry MetaDataEntry = PackagedChange[".metadata"];
				MetaDataEntry.Extract( TempPath, ExtractExistingFileAction.OverwriteSilently );

				RevisionData FileData = JsonHelper.ReadJsonFile<RevisionData>( Path.Combine( TempPath, ".metadata" ) );

				// Work out which files need to be added, deleted, and edited
				ReconcileData FilesToUpdate = EvaluateUnpackageOperations( FileData, BranchName, "", false );
				if( !FilesToUpdate.HasChanges() )
				{
					FormsLogger.Log( " ... no files to add, edit, or delete!" );
				}
				else
				{
					string ConfirmationMessage = ConstructConfirmationMessage( FilesToUpdate );
					if( MessageBox.Show( ConfirmationMessage, "Confirm Unpackage", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
					{
						// Execute the add, delete, and edit operations
						string Description = "Unpackaged changelist from file '" + Path.GetFileName( PackageFileName ) + "'";
						ExecuteUnpackageOperations( PackagedChange, Description, FilesToUpdate );
					}
					else
					{
						FormsLogger.Error( "Unpackaging canceled!" );
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="OriginalChange"></param>
		/// <param name="FileSets"></param>
		private bool CreateSplitChanges( Changelist OriginalChange, List<List<string>> FileSets )
		{
			bool Success = true;

			// Create new pending change
			int Counter = 0;
			foreach( List<string> FileSet in FileSets )
			{
				Counter++;
				string Description = "ResilientP4 - split change " + Counter + "/" + FileSets.Count + Environment.NewLine + Environment.NewLine + OriginalChange.Description;
				int SplitChangeId = SandboxedOperation<int>( CreateChangelistDelegate, 2, null, Description );

				List<FileSpec> FileSpecs = FileSpec.DepotSpecList( FileSet.ToArray() ).ToList();
				FileSpecs.ForEach( x => x.Version = new HaveRevision() );

				List<FileSpec> ReopenedFiles = SandboxedOperation<List<FileSpec>>( ReopenFilesDelegate, 5, FileSpecs, SplitChangeId );

				if( ReopenedFiles.Count != FileSpecs.Count )
				{
					FormsLogger.Error( "Failed to reopen files into change" + SplitChangeId );
					Success = false;
				}
			}

			return Success;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		public void SplitChangelist( string UserName, int ChangelistId )
		{
			Changelist Change = FindChangelist( UserName, ChangelistId );
			if( Change != null && Change.Files != null && Change.Files.Count != 0 )
			{
				List<List<string>> FileSets = SplitFileSets( Change );
				if( FileSets.Count > 1 )
				{
					// Confirmation message
					string ConfirmationMessage = "This will split change " + ChangelistId + " with " + ( Change.Files.Count - 1 ) + " files into " + FileSets.Count + " separate changes.";
					ConfirmationMessage += Environment.NewLine + Environment.NewLine;
					ConfirmationMessage += "Do you wish to continue?";
					if( MessageBox.Show( ConfirmationMessage, "Confirm Split", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
					{
						if( CreateSplitChanges( Change, FileSets ) )
						{
							FormsLogger.Success( "Change split!" );
						}
					}
					else
					{
						FormsLogger.Error( "Change splitting canceled!" );					
					}
				}
				else
				{
					FormsLogger.Warning( "Change would not be split in the current configuration. Please check your settings." );					
				}
			}	
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="BranchName"></param>
		/// <param name="RevisionName"></param>
		/// <param name="FileDetails"></param>
		/// <returns></returns>
		private RevisionData GetFileData( string BranchName, VersionSpec RevisionName, List<string> FileDetails )
		{
			FormsLogger.Title( "Getting file details for " + FileDetails.Count + " files at revision '" + RevisionName + "'." );
			RevisionData FileData = GetFileDetails( BranchName, FileDetails, RevisionName, "Details for " + RevisionName );

			FormsLogger.Title( " ... complete!" );
			return FileData;
		}

		/// <summary>
		/// </summary>
		/// <param name="BranchName"></param>
		/// <param name="RevisionName"></param>
		/// <returns></returns>
		private RevisionData GetFileData( string BranchName, VersionSpec RevisionName, bool bIncludeDeletedFolders )
		{
			FormsLogger.Title( "Getting file details for all '" + RevisionName + "' revisions." );
			List<string> FileDetails = new List<string>();
			RecursivelyGetStructure( BranchName, RevisionName, bIncludeDeletedFolders, 0, ref FileDetails );

			FormsLogger.Title( "Getting file details for " + FileDetails.Count + " files." );
			RevisionData FileData = GetFileDetails( BranchName, FileDetails, RevisionName, "Details for " + RevisionName );

			if( !RevisionName.ToString().Contains( "#head" ) && !RevisionName.ToString().Contains( "#have" ) )
			{
				string RevisionDetailFileName = BranchName.Replace( '/', '-' ) + RevisionName + "-" + DateTime.UtcNow.Ticks + ".revisions";
				FormsLogger.Title( "Writing have file details to '" + RevisionDetailFileName + "'" );
				JsonHelper.WriteJsonFile( Path.Combine( MainForm.GetSettingsFolder(), RevisionDetailFileName ), FileData );
			}

			FormsLogger.Title( " ... complete!" );
			return FileData;
		}

		/// <summary>
		/// </summary>
		/// <param name="BranchName"></param>
		public void GetChangelists( string BranchName )
		{
			List<FileSpec> FolderMatch = new List<FileSpec>()
			{
				FileSpec.DepotSpec( BranchName + "..." )
			};

			SandboxedOperation<bool>( GetSubmittedChangelistsDelegate, 5, FolderMatch, RootApplication.Config.ChangesToReceive );
			SandboxedOperation<bool>( GetPendingChangelistsDelegate, 5, FolderMatch, RootApplication.Config.ChangesToReceive );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		public bool IsPendingChangelist( int ChangelistId )
		{
			Changelist Change = PendingChanges.Where( x => x.Id == ChangelistId ).FirstOrDefault();
			return Change != null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="UserName"></param>
		/// <param name="ChangelistId"></param>
		public void GetChangelistDetails( string BranchName, string UserName, int ChangelistId )
		{
			Changelist Change = FindChangelist( UserName, ChangelistId );
			if( Change != null && Change.Files != null && Change.Files.Count == 0 )
			{
				List<FileSpec> FolderMatch = new List<FileSpec>()
				{
					FileSpec.DepotSpec( BranchName + "..." )
				};

				Change.Files = SandboxedOperation<IList<FileMetaData>>( GetFileMetaDataForChangelistDelegate, 5, FolderMatch, Change );
				if( Change.Files != null )
				{
					FormsLogger.Log( " .... found " + Change.Files.Count + " files in change." );
				}
				else
				{
					FormsLogger.Warning( " .... failed to find any files in change '" + ChangelistId + "' for user '" + UserName + "'." );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void CleanWorkspace()
		{
			List<FileSpec> FolderMatch = new List<FileSpec>()
			{
				FileSpec.DepotSpec( "..." )
			};

			List<Changelist> EmptyPendingChanges = SandboxedOperation<List<Changelist>>( GetEmptyPendingChangelistsDelegate, 5, FolderMatch, RootApplication.Config.ChangesToReceive );

			if( EmptyPendingChanges.Count > 0 )
			{
				string Message = "This will delete " + EmptyPendingChanges.Count + " empty pending changes from workspace '"
								+ CachedWorkspace + "' on Perforce server '" + SafeServerDisplayName + "'." + Environment.NewLine + Environment.NewLine
								+ "Are you sure?";
				if( MessageBox.Show( Message, "Confirm workspace clean", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
				{
					foreach( Changelist Change in EmptyPendingChanges )
					{
						FormsLogger.Log( " ... deleting empty change " + Change.Id );
						SandboxedOperation<bool>( DeleteEmptyChangelistDelegate, 5, null, Change );
					}

					FormsLogger.Success( " ... workspace cleaned!" );
				}
				else
				{
					FormsLogger.Warning( " .... workspace clean operation canceled." );
				}
			}
			else
			{
				FormsLogger.Log( " ... no empty changes found to delete" );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="BranchName"></param>
		public void MatchHaveFileData( string BranchName )
		{
			RevisionData FileData = GetFileData( BranchName, new HaveRevision(), false );
			if( FileData != null )
			{
				FileData.MatchToLabel();
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="BranchName"></param>
		public RevisionData GetDepotFileHeadData( string BranchName )
		{
			RevisionData FileData = GetFileData( BranchName, new HeadRevision(), true );
			if( FileData != null )
			{
				FormsLogger.Log( " ... found " + FileData.DepotFiles.Count + " files."  );
			}

			return FileData;
		}

		/// <summary>
		/// </summary>
		/// <param name="BranchName"></param>
		/// <param name="LabelName"></param>
		public void GetLabelFileData( string BranchName, string LabelName )
		{
			GetFileData( BranchName, new LabelNameVersion( LabelName ), false );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FilesNames"></param>
		/// <param name="DoNothing"></param>
		/// <param name="ForceSync"></param>
		/// <param name="CheckOut"></param>
		public void HandleInconsistentFiles( string Description, List<string> FileSet, bool DoNothing, bool ForceSync, bool CheckOut )
		{
			if( !DoNothing )
			{
				if( ForceSync )
				{
					FormsLogger.Log( "Force syncing " + FileSet.Count + " files ..." );

					// Split file sets

					List<FileSpec> FileSpecs = new List<FileSpec>();
					FileSet.ForEach( x => FileSpecs.Add( FileSpec.DepotSpec( x ) ) );
					FileSpecs.ForEach( x => x.Version = new HaveRevision() );

					SandboxedOperation<List<FileSpec>>( ForceSyncFilesDelegate, 12, FileSpecs );
				}
				else if( CheckOut )
				{
					// Create changelist with description
					// Add files to change
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Host"></param>
		/// <returns></returns>
		public string GetWorkspaceForHost( string Host )
		{
			string WorkspaceName = "";
			Client Workspace = Workspaces.Where( x => x.Host.ToUpperInvariant() == Host.ToUpperInvariant() ).FirstOrDefault();
			if( Workspace != null )
			{
				WorkspaceName = Workspace.Name;
			}

			return WorkspaceName;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileMatch"></param>
		/// <returns></returns>
		public IList<string> GetDepotDirectories( string FileMatch, bool bIncludeDeletedFolders )
		{
			return SandboxedOperation<IList<string>>( GetDepotDirsDelegate, 2, null, FileMatch, bIncludeDeletedFolders );
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public Collection<Dictionary<string, object>> PopulateLabels()
		{
			Collection<Dictionary<string, object>> LabelDetails = new Collection<Dictionary<string, object>>();

			if( Labels != null )
			{
				foreach( Label CurrentLabel in Labels )
				{
					Dictionary<string, object> Map = new Dictionary<string, object>();

					Map["Id"] = CurrentLabel.Id;
					Map["Update"] = CurrentLabel.Update;
					Map["Access"] = CurrentLabel.Access;
					Map["Owner"] = CurrentLabel.Owner;
					Map["Locked"] = CurrentLabel.Locked;
					Map["Description"] = CurrentLabel.Description;

					LabelDetails.Add( Map );
				}
			}

			return LabelDetails;
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public Collection<Dictionary<string, object>> PopulateChanges( string UserName )
		{
			Collection<Dictionary<string, object>> ChangeDetails = new Collection<Dictionary<string, object>>();

			if( SubmittedChanges.ContainsKey( UserName ) )
			{
				foreach( Changelist CurrentChange in SubmittedChanges[UserName] )
				{
					Dictionary<string, object> Map = new Dictionary<string, object>();

					Map["Id"] = CurrentChange.Id;
					Map["OwnerName"] = CurrentChange.OwnerName;
					Map["Description"] = CurrentChange.Description;

					ChangeDetails.Add( Map );
				}
			}

			return ChangeDetails;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public Collection<Dictionary<string, object>> PopulatePendingChanges()
		{
			Collection<Dictionary<string, object>> ChangeDetails = new Collection<Dictionary<string, object>>();

			foreach( Changelist CurrentChange in PendingChanges )
			{
				Dictionary<string, object> Map = new Dictionary<string, object>();

				Map["Id"] = CurrentChange.Id;
				Map["OwnerName"] = CurrentChange.OwnerName;
				Map["Description"] = CurrentChange.Description;

				ChangeDetails.Add( Map );
			}

			return ChangeDetails;		
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public Collection<Dictionary<string, object>> PopulateChangeDetails( string UserName, int ChangelistId )
		{
			Collection<Dictionary<string, object>> ChangeDetails = new Collection<Dictionary<string, object>>();

			Changelist Change = FindChangelist( UserName, ChangelistId );
			if( Change != null && Change.Files != null )
			{
				Dictionary<string, object> Map = new Dictionary<string, object>();

				foreach( FileMetaData MetaData in Change.Files )
				{
					if( MetaData.DepotPath != null )
					{
						if( Change.Pending )
						{
							FileDetail Detail = new FileDetail( MetaData.FileSize, MetaData.Action.ToString(), "None" );
							Map[MetaData.DepotPath.Path] = Detail;
						}
						else
						{
							FileDetail Detail = new FileDetail( MetaData.FileSize, "None", MetaData.HeadAction.ToString() );
							Map[MetaData.DepotPath.Path] = Detail;
						}
					}
				}

				ChangeDetails.Add( Map );
			}

			return ChangeDetails;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileSpecs"></param>
		/// <returns></returns>
		private delegate object SandboxedOperationDelegate( IList<FileSpec> FileSpecs, params object[] Parameters );
	}
}
