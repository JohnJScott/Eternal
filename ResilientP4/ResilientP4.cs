// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using Eternal.EternalUtilities;
using Perforce.P4;

namespace ResilientP4
{
	/// <summary>
	///     A class to handle to main display form.
	/// </summary>
	public partial class MainForm : Form
	{
		/// <summary></summary>
		public UserConfiguration Config;

		/// <summary></summary>
		public DepotTreeNode Depot = null;

		/// <summary></summary>
		public ChangelistTreeNode Changes = null;

		/// <summary></summary>
		public bool Running = false;

		/// <summary></summary>
		public string SelectedLabel = "";

		/// <summary>
		/// </summary>
		public MainForm()
		{
			InitializeComponent();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static string GetSettingsFolder()
		{
			return Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.MyDocuments ), "ResilientP4" );
		}

		/// <summary>
		/// 
		/// </summary>
		private void ReadConfiguration()
		{
			string ConfigFileName = Path.ChangeExtension( Assembly.GetExecutingAssembly().GetName().Name, "json" );
			Config = JsonHelper.ReadJsonFile<UserConfiguration>( Path.Combine( GetSettingsFolder(), ConfigFileName ) );
		}

		/// <summary>
		/// 
		/// </summary>
		private void WriteConfiguration()
		{
			string ConfigFileName = Path.ChangeExtension( Assembly.GetExecutingAssembly().GetName().Name, "json" );
			JsonHelper.WriteJsonFile( Path.Combine( GetSettingsFolder(), ConfigFileName ), Config );
		}

		/// <summary>
		/// </summary>
		public static void SetWaitMode()
		{
			Application.UseWaitCursor = true;
			Application.DoEvents();
		}

		/// <summary>
		/// </summary>
		public static void ClearWaitMode()
		{
			Application.UseWaitCursor = false;
			Application.DoEvents();
		}

		/// <summary>
		/// </summary>
		/// <returns></returns>
		public static bool IsInWaitMode()
		{
			return Application.UseWaitCursor;
		}

		/// <summary>
		/// 
		/// </summary>
		private string CreateDepotTreeView( Perforce PerforceServer )
		{
			// Find the depots
			Depot = new DepotTreeNode( PerforceServer );
			Depot.InsertDirectories( "//*" );
			MainTreeView.Nodes.Add( Depot );

			if( Depot.Nodes.Count > 0 )
			{
				// Get the directories in the root of the depot
				DepotTreeNode DepotNode = ( DepotTreeNode )Depot.Nodes[0];
				Depot.InsertDirectories( DepotNode.CachedFullPath + "*" );

				// Set the selected node to the current depot
				MainTreeView.SelectedNode = DepotNode;

				return DepotNode.CachedFullPath;
			}

			return "";
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotPath"></param>
		private void CreateChangelistTreeView( Perforce PerforceServer, string BranchName )
		{
			// Set up the default changes view
			PerforceServer.GetChangelists( BranchName );

			Changes = new ChangelistTreeNode( PerforceServer, BranchName, "branch-green.ico" );
			Changes.InsertChanges();
			ChangelistTreeView.Nodes.Clear();
			ChangelistTreeView.Nodes.Add( Changes );
			Changes.Expand();

			ChangelistTreeView.AllowNodeSelection = AllowNodeSelection;
		}

		/// <summary>
		/// </summary>
		public void Initialize()
		{
			ReadConfiguration();

			// Create a default node with the new connection string
			Depot = new DepotTreeNode();
			MainTreeView.Nodes.Add( Depot );

			Running = true;
			Show();
		}

		/// <summary>
		/// </summary>
		public void Tick()
		{
			Application.DoEvents();
		}

		/// <summary>
		/// </summary>
		public void Shutdown()
		{
			WriteConfiguration();
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="FormClosedEventArguments"></param>
		private void ResilientP4Closed( object Sender, FormClosedEventArgs FormClosedEventArguments )
		{
			Running = false;
		}

		/// <summary>
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		public static bool FileExists( string FileName )
		{
			FileInfo Info = new FileInfo( FileName );
			return Info.Exists;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectToServerClick( object Sender, EventArgs EventArguments )
		{
			using( ConnectionDialog Dialog = new ConnectionDialog( this ) )
			{
				if( Dialog.ShowDialog() == DialogResult.OK )
				{
					// Set up the default view of all depots
					string RootDepotPath = CreateDepotTreeView( Dialog.CurrentPerforceServer );

					// Set up the changelist tree view
					CreateChangelistTreeView( Dialog.CurrentPerforceServer, RootDepotPath );
				}

				FormsLogger.SetRecipient( this, LogTextBox );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Node"></param>
		private void SetDepotContextMenuState( DepotTreeNode Node )
		{
			bool IsServer = ( Node.Parent == null );

			SyncToolStripMenuItem.Enabled = !IsServer;
			UnpackageChangelistToolStripMenuItem.Enabled = !IsServer;
			CheckConsistencyQuickMenuItem.Enabled = !IsServer;
			CheckConsistencyThoroughMenuItem.Enabled = !IsServer;
			DisconnectMenuItem.Enabled = IsServer;
			GetHaveToolStripMenuItem.Enabled = !IsServer;
			GetLabelToolStripMenuItem.Enabled = !IsServer;
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="ClickEventArguments"></param>
		private void DepotTreeViewNodeClick( object Sender, TreeNodeMouseClickEventArgs ClickEventArguments )
		{
			DepotTreeNode Node = ( DepotTreeNode )ClickEventArguments.Node;

			Perforce PerforceServer = Node.PerforceServer;
			if( PerforceServer == null )
			{
				ConnectToServerClick( Sender, null );
			}
			else
			{
				MainTreeView.SelectedNode = Node;

				if( ClickEventArguments.Button == MouseButtons.Right )
				{
					SetDepotContextMenuState( Node );
					Node.ShowDepotContextMenu();
				}

				if( ClickEventArguments.Button == MouseButtons.Left || ClickEventArguments.Button == MouseButtons.Right )
				{
					if( Node.Nodes.Count == 0 )
					{
						PerforceServer.RootNode.InsertDirectories( Node.CachedFullPath + "*" );
					}

					CreateChangelistTreeView( PerforceServer, Node.CachedFullPath );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="ClickEventArguments"></param>
		private void ChangelistTreeViewNodeClick( object Sender, TreeNodeMouseClickEventArgs ClickEventArguments )
		{
			ChangelistTreeNode Node = ( ChangelistTreeNode )ClickEventArguments.Node;

			if( ClickEventArguments.Button == MouseButtons.Right )
			{
				ChangelistViewMenuStrip.Items[1].Enabled = Node.IsPendingChangelistNode();
				Node.ShowChangelistContextMenu( ChangelistViewMenuStrip );
			}
			else if( ClickEventArguments.Button == MouseButtons.Left )
			{
				Node.AugmentChanges();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void ResilientSyncClick( object Sender, EventArgs EventArguments )
		{
			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode BranchNode = ( DepotTreeNode )MenuItem.Owner.Tag;

			using( SelectLabelDialog Dialog = new SelectLabelDialog( this, BranchNode.PerforceServer, BranchNode.CachedFullPath ) )
			{
				if( Dialog.ShowDialog() == DialogResult.OK )
				{
					if( SelectedLabel.Length > 0 )
					{
						SetWaitMode();
						BranchNode.PerforceServer.SyncToLabel( BranchNode.CachedFullPath, SelectedLabel );
						ClearWaitMode();
					}
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void GetHaveFileDataClick( object Sender, EventArgs EventArguments )
		{
			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode BranchNode = ( DepotTreeNode )MenuItem.Owner.Tag;

			SetWaitMode();
			BranchNode.PerforceServer.MatchHaveFileData( BranchNode.CachedFullPath );
			ClearWaitMode();
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void GetLabelFileDataClick( object Sender, EventArgs EventArguments )
		{
			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode BranchNode = ( DepotTreeNode )MenuItem.Owner.Tag;

			using( SelectLabelDialog Dialog = new SelectLabelDialog( this, BranchNode.PerforceServer, BranchNode.CachedFullPath ) )
			{
				if( Dialog.ShowDialog() == DialogResult.OK )
				{
					if( SelectedLabel.Length > 0 )
					{
						SetWaitMode();
						BranchNode.PerforceServer.GetLabelFileData( BranchNode.CachedFullPath, SelectedLabel );
						ClearWaitMode();
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="UserName"></param>
		/// <param name="FullPath"></param>
		/// <param name="ChangelistId"></param>
		/// <returns></returns>
		private bool GetNodeInformation( ChangelistTreeNode ChangelistNode, out string UserName, out string FullPath, out int ChangelistId )
		{
			bool Success = false;
			UserName = "";
			FullPath = "";
			ChangelistId = 0;

			if( ChangelistNode != null )
			{
				DepotTreeNode BranchNode = ( DepotTreeNode )MainTreeView.SelectedNode;
				if( BranchNode != null )
				{
					ChangelistTreeNode UserChangelistNode = ( ChangelistTreeNode )ChangelistNode.Parent;
					if( UserChangelistNode != null )
					{
						if( ChangelistNode.ChangelistId > -1 )
						{
							UserName = UserChangelistNode.CachedName;
							FullPath = BranchNode.CachedFullPath;
							ChangelistId = ChangelistNode.ChangelistId;
							Success = true;
						}
					}
				}
			}

			return Success;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void PackageChangelistClick( object Sender, EventArgs EventArguments )
		{
			FormsLogger.Title( "Packaging change..."  );

			string UserName;
			string FullPath;
			int ChangelistId;

			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			ChangelistTreeNode ChangelistNode = ( ChangelistTreeNode )MenuItem.Owner.Tag;
			if( GetNodeInformation( ChangelistNode, out UserName, out FullPath, out ChangelistId ) )
			{
				GenericSaveFileDialog.FileName = "Change-" + ChangelistId + ".rp4";
				if( GenericSaveFileDialog.ShowDialog() == DialogResult.OK )
				{
					// Delete the old file if it exists
					FileInfo Info = new FileInfo( GenericSaveFileDialog.FileName );
					if( Info.Exists )
					{
						Info.IsReadOnly = false;
						Info.Delete();
					}

					// Save the changelist 
					ChangelistNode.PerforceServer.PackageChangelist( UserName, FullPath, ChangelistId, GenericSaveFileDialog.FileName );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void SplitChangelistClick( object Sender, EventArgs EventArguments )
		{
			FormsLogger.Title( "Splitting change..." );

			string UserName;
			string FullPath;
			int ChangelistId;

			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			ChangelistTreeNode ChangelistNode = ( ChangelistTreeNode )MenuItem.Owner.Tag;
			if( GetNodeInformation( ChangelistNode, out UserName, out FullPath, out ChangelistId ) )
			{
				ChangelistNode.PerforceServer.SplitChangelist( UserName, ChangelistId );
				// FIXME: Refresh changelist view here
			}
			else
			{
				FormsLogger.Error( " ... failed to retrieve enough information." );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void MultiSubmitClick( object Sender, EventArgs EventArguments )
		{
			FormsLogger.Title( "Multi submit..." );

			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			ChangelistTreeNode ChangelistNode = ( ChangelistTreeNode )MenuItem.Owner.Tag;
			if( ChangelistNode != null )
			{
				List<int> ChangelistIds = new List<int>();
				foreach( ChangelistTreeNode Node in ChangelistTreeView.SelectedNodes )
				{
					ChangelistIds.Add( Node.ChangelistId );
				}

				if( ChangelistNode.PerforceServer.Submit( ChangelistIds ) )
				{
					FormsLogger.Success( " ... all changes submitted!" );
					// FIXME: Refresh changelist view here
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void ResilientSubmitClick( object Sender, EventArgs EventArguments )
		{
			FormsLogger.Title( "Resilient submit..." );

			FormsLogger.Warning( " ... not implemented" );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void UnpackageChangelistClick( object Sender, EventArgs EventArguments )
		{
			FormsLogger.Title( "Unpackaging change..." );

			if( GenericOpenFileDialog.ShowDialog() == DialogResult.OK )
			{
				FileInfo Info = new FileInfo( GenericOpenFileDialog.FileName );
				if( Info.Exists )
				{
					DepotTreeNode DepotNode = ( DepotTreeNode )MainTreeView.SelectedNode;
					if( DepotNode != null )
					{
						Perforce PerforceServer = DepotNode.PerforceServer;
						if( PerforceServer != null )
						{
							PerforceServer.UnpackageChangelist( GenericOpenFileDialog.FileName, DepotNode.CachedFullPath );
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArgs"></param>
		private void DisconnectMenuItemClick( object Sender, EventArgs EventArgs )
		{
			FormsLogger.Title( "Disconnect from server..." );

			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode ServerNode = ( DepotTreeNode )MenuItem.Owner.Tag;
			if( ServerNode.PerforceServer != null )
			{
				ServerNode.PerforceServer.ResetConnection();
				ServerNode.PerforceServer = null;

				MainTreeView.Nodes.Remove( ServerNode );
				MainTreeView.SelectedNode = null;

				ChangelistTreeView.SelectedNodes.Clear();
				ChangelistTreeView.Nodes.Clear();

				FormsLogger.Success( "Successfully disconnected from server!" );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FileName"></param>
		/// <returns></returns>
		private bool IsFileNameValid( string FileName )
		{
			if( FileName.Contains( "@" ) )
			{
				return false;
			}
			else if( FileName.Contains( "#" ) )
			{
				return false;
			}
			else if( FileName.Contains( "%" ) )
			{
				return false;
			}
			else if( FileName.Contains( "*" ) )
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="FolderName"></param>
		/// <param name="bValidateChecksums"></param>
		/// <returns></returns>
		// FIXME: Move in Perforce.cs
		private void GetLocalFileData( GenericProgressBarDialog Bar, int RootFolderNameLength, RevisionData DepotHaveData, string FolderName, bool bValidateChecksums, ref RevisionData LocalFileData )
		{
			DirectoryInfo DirInfo = new DirectoryInfo( FolderName );

			foreach( DirectoryInfo SubDirInfo in DirInfo.GetDirectories() )
			{
				GetLocalFileData( Bar, RootFolderNameLength, DepotHaveData, SubDirInfo.FullName, bValidateChecksums, ref LocalFileData );
			}

			foreach( FileInfo Info in DirInfo.GetFiles() )
			{
				if( !IsFileNameValid( Info.FullName ) )
				{
					FormsLogger.Warning( " ... ignoring file '" + Info.FullName + "' with wildcards in the name [@#%*] - please rename." );
				}
				else
				{
					Bar.Bump( "Getting details for " + Info.Name );

					DepotFileData FileData = new DepotFileData();

					// Map local file path to depot path
					FileData.DepotPath = LocalFileData.BranchName + Info.FullName.Substring( RootFolderNameLength + 1 ).Replace( "\\", "/" );
					FileData.LocalPath = Info.FullName;

					DepotFileData DepotData;
					if( DepotHaveData.DepotFiles.TryGetValue( FileData.DepotPath, out DepotData ) )
					{
						FileData.PerforceFileType = DepotData.PerforceFileType;
						FileData.GetNormalisedSize( Info );
					}

					if( bValidateChecksums )
					{
						FileData.CalculateMD5Checksum( Info );
					}

					LocalFileData.AddFile( FileData );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotHaveData"></param>
		/// <param name="LocalFileData"></param>
		/// <param name="Reconcile"></param>
		private void ReconcileFiles( RevisionData DepotHaveData, RevisionData LocalFileData, ReconcileData Reconcile )
		{
			foreach( DepotFileData LocalFile in LocalFileData.DepotFiles.Values )
			{
				DepotFileData DepotFile;
				if( DepotHaveData.DepotFiles.TryGetValue( LocalFile.DepotPath, out DepotFile ) )
				{
					if( !LocalFile.Equals( DepotFile ) )
					{
						// Local file exists in depot, but is different
						if( DepotFile.Size >= 0 )
						{
							// It exists in the depot, but is different -> mark for edit
							Reconcile.FilesToEdit.Add( LocalFile.DepotPath );
						}
						else
						{
							// It exists in the depot, but is deleted -> mark for add
							Reconcile.FilesToAdd.Add( LocalFile.DepotPath );
						}
					}
				}
				else
				{
					// Exists locally, does not exist in depot -> copy & mark for add
					Reconcile.FilesToAdd.Add( LocalFile.DepotPath );
				}
			}

			foreach( DepotFileData DepotFile in DepotHaveData.DepotFiles.Values )
			{
				// If file is already deleted in the depot, the size will be -1
				if( DepotFile.Size > -1 )
				{
					DepotFileData LocalFile;
					if( !LocalFileData.DepotFiles.TryGetValue( DepotFile.DepotPath, out LocalFile ) )
					{
						// Does not exist locally, exists in depot -> mark for delete					
						Reconcile.FilesToDelete.Add( DepotFile.DepotPath );
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotHaveData"></param>
		/// <param name="DepotBranch"></param>
		/// <param name="FolderName"></param>
		/// <param name="ModifiedFiles"></param>
		/// <returns></returns>
		private RevisionData GetLocalFileData( RevisionData DepotHaveData, string DepotBranch, string FolderName, List<string> ModifiedFiles  )
		{
			RevisionData LocalFileData = new RevisionData( DepotBranch, "#have", "Local file data." );

			using( GenericProgressBarDialog Bar = new GenericProgressBarDialog( this, "Getting details about the local files", 0, ModifiedFiles.Count ) )
			{
				Bar.Show();

				// FIXME
				//GetLocalFileData( Bar, FolderName.Length, DepotHaveData, FolderName, ModifiedFiles, ref LocalFileData );

				Bar.Close();
			}

			return LocalFileData;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="DepotBranch"></param>
		/// <param name="FolderName"></param>
		/// <param name="bValidateChecksums"></param>
		/// <returns></returns>
		private RevisionData GetLocalFileData( RevisionData DepotHaveData, string DepotBranch, string FolderName, bool bValidateChecksums )
		{
			RevisionData LocalFileData = new RevisionData( DepotBranch, "#have", "Local file data." );

			using( GenericProgressBarDialog Bar = new GenericProgressBarDialog( this, "Getting details about the local files", 0, DepotHaveData.DepotFiles.Count ) )
			{
				Bar.Show();

				GetLocalFileData( Bar, FolderName.Length, DepotHaveData, FolderName, bValidateChecksums, ref LocalFileData );

				Bar.Close();
			}

			return LocalFileData;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="BranchNode"></param>
		/// <param name="bValidateChecksums"></param>
		private void CheckConsistency( DepotTreeNode BranchNode, bool bValidateChecksums )
		{
			SetWaitMode();

			string DepotBranch = BranchNode.CachedFullPath;

			// Gets the have and head revisions along with the depot and local paths
			RevisionData DepotHaveData = BranchNode.PerforceServer.GetDepotFileHeadData( DepotBranch );

			// Get the local file data (and optionally the checksums) from the local files
			string FileFolder = BranchNode.PerforceServer.GetLocalBranchPath( DepotBranch );
			RevisionData LocalFileData = GetLocalFileData( DepotHaveData, DepotBranch, FileFolder, bValidateChecksums );

			List<string> GoodFiles = new List<string>();
			List<string> ExtraFiles = new List<string>();
			List<string> MissingFiles = new List<string>();
			List<string> CorruptFiles = new List<string>();
			// Writable on client
			List<string> WritableFiles = new List<string>();

			foreach( DepotFileData DepotFile in DepotHaveData.DepotFiles.Values )
			{
				DepotFileData LocalFile;
				if( !LocalFileData.DepotFiles.TryGetValue( DepotFile.DepotPath, out LocalFile ) )
				{
					// Does not exist locally, exists in depot -> mark as missing				
					MissingFiles.Add( DepotFile.DepotPath );
				}
				else
				{
					// Local file has different size or checksum to the depot file -> mark as corrupt
					if( !LocalFile.Equals( DepotFile ) )
					{
						CorruptFiles.Add( DepotFile.DepotPath );
					}
					else
					{
						GoodFiles.Add( DepotFile.DepotPath  );
					}
				}
			}
	
			foreach( DepotFileData LocalFile in LocalFileData.DepotFiles.Values )
			{
				DepotFileData DepotFile;
				if( !DepotHaveData.DepotFiles.TryGetValue( LocalFile.DepotPath, out DepotFile ) )
				{
					// Exists locally, does not exist in depot -> it is extra
					ExtraFiles.Add( LocalFile.DepotPath );
				}
			}

			FormsLogger.Log( " ... found " + WritableFiles.Count + " non text file(s) with a different size locally than to that in the depot, but are marked as *ALWAYS WRITABLE* on the client (presumed good)." );
			FormsLogger.Log( " ... found " + ExtraFiles.Count + " existing local file(s) deleted in the depot." );
			FormsLogger.Log( " ... found " + MissingFiles.Count + " file(s) in the depot that do not exist locally." );
			FormsLogger.Log( " ... found " + CorruptFiles.Count + " non text file(s) with a different size locally than to that in the depot, and are marked as *READ ONLY* on the client (no +w modifier)." );

			int GoodFileCount = GoodFiles.Count + WritableFiles.Count;
			int CorruptFileCount = ExtraFiles.Count + MissingFiles.Count + CorruptFiles.Count;
			FormsLogger.Success( " ... found " + GoodFileCount + " files consistent with the depot, and " + CorruptFileCount + " corrupt files." );

			if( CorruptFileCount > 0 || WritableFiles.Count > 0 )
			{
				ClearWaitMode();

				using( MakeConsistentDialog Dialog = new MakeConsistentDialog( CorruptFiles, MissingFiles, ExtraFiles, WritableFiles ) )
				{
					if( Dialog.ShowDialog() == DialogResult.OK )
					{
						BranchNode.PerforceServer.HandleInconsistentFiles( "Files with a bad checksum", CorruptFiles, false, false, false );
						BranchNode.PerforceServer.HandleInconsistentFiles( "Missing files", MissingFiles, false, false, false );
						BranchNode.PerforceServer.HandleInconsistentFiles( "Extra files", ExtraFiles, false, false, false );
						BranchNode.PerforceServer.HandleInconsistentFiles( "Writable files", WritableFiles, false, false, false );
					}
				}
			}

			ClearWaitMode();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArgs"></param>
		private void CheckConsistencyMenuItemClick( object Sender, EventArgs EventArgs )
		{
			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode BranchNode = ( DepotTreeNode )MenuItem.Owner.Tag;

			CheckConsistency( BranchNode, false );
		}

		private void CheckConsistencyThoroughMenuItemClick( object Sender, EventArgs EventArgs )
		{
			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode BranchNode = ( DepotTreeNode )MenuItem.Owner.Tag;

			CheckConsistency( BranchNode, true );
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="QuitEventArguments"></param>
		private void QuitMenuItemClick( object Sender, EventArgs QuitEventArguments )
		{
			Running = false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void CleanButtonClick( object Sender, EventArgs EventArguments )
		{
			FormsLogger.Title( "Cleaning workspace(s)..." );

			DepotTreeNode DepotNode = ( DepotTreeNode )MainTreeView.SelectedNode;
			if( DepotNode != null )
			{
				Perforce PerforceServer = DepotNode.PerforceServer;
				if( PerforceServer != null )
				{
					PerforceServer.CleanWorkspace();
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="SettingsEventArguments"></param>
		private void SettingsMenuItemClick( object Sender, EventArgs SettingsEventArguments )
		{
			using( SettingsDialog Dialog = new SettingsDialog( this ) )
			{
				Dialog.ShowDialog();
			}			
		}

		/// <summary>
		/// Suppress the spurious selection of the 'connect to new server' node
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="TreeViewCancelEventArguments"></param>
		private void OnBeforeSelectTreeNode( object Sender, TreeViewCancelEventArgs TreeViewCancelEventArguments )
		{
			if( TreeViewCancelEventArguments.Node == MainTreeView.TopNode )
			{
				TreeViewCancelEventArguments.Cancel = true;
			}
		}

		/// <summary>
		/// Callback from MultiSelectTreeView to allow the host to suppress addition of nodes.
		/// </summary>
		/// <param name="Node">Node to check to see if it should be added to the selected list.</param>
		/// <returns></returns>
		private bool AllowNodeSelection( TreeNode Node )
		{
			// Only allow multiselection if it's a changelist node
			ChangelistTreeNode ChangelistNode = ( ChangelistTreeNode )Node;
			if( ChangelistNode.ChangelistId > -1 )
			{
				// ... and a pending changelist node
				DepotTreeNode DepotNode = ( DepotTreeNode )MainTreeView.SelectedNode;
				if( DepotNode != null )
				{
					Perforce PerforceServer = DepotNode.PerforceServer;
					if( PerforceServer != null )
					{
						return PerforceServer.IsChangelistPending( ChangelistNode.ChangelistId );
					}
				}
			}

			return false;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ReconcileFilesClick( object Sender, EventArgs MenuItemArgs )
		{
			SetWaitMode();

			ToolStripMenuItem MenuItem = ( ToolStripMenuItem )Sender;
			DepotTreeNode BranchNode = ( DepotTreeNode )MenuItem.Owner.Tag;

			string DepotBranch = BranchNode.CachedFullPath;
			FormsLogger.Title( "Reconciling files with '" + DepotBranch + "'/..." );

			// Browse to folder
			GenericFolderBrowserDialog.Description = "Select the folder you wish to reconcile with '" + DepotBranch + "...'";
			if( GenericFolderBrowserDialog.ShowDialog() == DialogResult.OK )
			{
				// Get #have data
				RevisionData DepotHaveData = BranchNode.PerforceServer.GetDepotFileHeadData( DepotBranch );
	
				// Get data for folder
				string FileFolder = GenericFolderBrowserDialog.SelectedPath;
				RevisionData LocalFileData = GetLocalFileData( DepotHaveData, DepotBranch, FileFolder, true );

				// Work out differences
				ReconcileData Reconcile = new ReconcileData( DepotBranch, FileFolder );
				ReconcileFiles( DepotHaveData, LocalFileData, Reconcile );

				// Write out a summary
				string ReconcileFileName = DepotBranch.Replace( '/', '-' ) + "-" + DateTime.UtcNow.Ticks + ".reconcile";
				JsonHelper.WriteJsonFile( Path.Combine( MainForm.GetSettingsFolder(), ReconcileFileName ), Reconcile );

				// Are you sure?
				string ConfirmationMessage = Reconcile.GetConfirmationMessage( DepotHaveData.DepotFiles.Count );
				if( MessageBox.Show( ConfirmationMessage, "Reconcile folder with depot", MessageBoxButtons.OKCancel, MessageBoxIcon.Question ) == DialogResult.OK )
				{
					// Reconcile
					BranchNode.PerforceServer.ReconcileFiles( FileFolder, DepotBranch, Reconcile );

					FormsLogger.Log( " ... files reconciled!" );
				}
				else
				{
					FormsLogger.Log( " ... files not reconciled!" );
				}
			}
			else
			{
				FormsLogger.Log( " ... operation canceled." );
			}
			
			ClearWaitMode();
		}
	}
}
