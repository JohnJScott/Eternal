// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using System.Windows.Forms;

namespace ResilientP4
{
	/// <summary>
	///     A class to handle the tree nodes in the depot view.
	/// </summary>
	[Serializable]
	public class DepotTreeNode : TreeNode
	{
		/// <summary></summary>
		public Perforce PerforceServer = null;

		/// <summary></summary>
		public string CachedFullPath = "//";

		/// <summary>
		/// 
		/// </summary>
		public DepotTreeNode()
		{
			Text = "<Connect to Perforce server...>";
			ImageKey = "connect-black.ico";
			SelectedImageKey = ImageKey;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InPerforceServer"></param>
		public DepotTreeNode( Perforce InPerforceServer )
		{
			if( InPerforceServer != null )
			{
				PerforceServer = InPerforceServer;
				Text = PerforceServer.SafeServerDisplayName;
				PerforceServer.RootNode = this;

				ImageKey = "server-blue.ico";
				SelectedImageKey = "server-blue.ico";
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InPerforceServer"></param>
		/// <param name="InCachedFullPath"></param>
		/// <param name="InDirectoryName"></param>
		public DepotTreeNode( Perforce InPerforceServer, string InCachedFullPath, string InDirectoryName )
        {
			PerforceServer = InPerforceServer;
			Text = InDirectoryName;
			CachedFullPath = InCachedFullPath + InDirectoryName + "/";

			ImageKey = "branch-grey.ico";
			SelectedImageKey = "branch-green.ico";
		}

		/// <summary>
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected DepotTreeNode( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}

		/// <summary>
		/// </summary>
		/// <param name="InCachedFullPath"></param>
		/// <param name="TopDirectory"></param>
		/// <returns></returns>
		private DepotTreeNode FindOrCreate( string InCachedFullPath, string TopDirectory )
		{
			// Search for existing folder at this level
			foreach( DepotTreeNode FolderNode in Nodes )
			{
				if( TopDirectory.ToUpperInvariant() == FolderNode.Text.ToUpperInvariant() )
				{
					return FolderNode;
				}
			}

			// Not found, so add it
			Nodes.Add( new DepotTreeNode( PerforceServer, InCachedFullPath, TopDirectory ) );
			return ( DepotTreeNode )Nodes[Nodes.Count - 1];
		}

		/// <summary>
		/// </summary>
		/// <param name="DepotDirectory"></param>
		private void InsertDirectory( string DepotDirectory )
		{
			string TopDirectory = DepotDirectory.TrimStart( '/' );
			string SubDirectory = "";
			int SubDirectoryIndex = TopDirectory.IndexOf( '/' );

			if( SubDirectoryIndex > 0 )
			{
				SubDirectory = TopDirectory.Substring( SubDirectoryIndex + 1 );
				TopDirectory = TopDirectory.Substring( 0, SubDirectoryIndex );
			}

			DepotTreeNode FolderNode = FindOrCreate( CachedFullPath, TopDirectory );
			if( SubDirectory.Length > 0 )
			{
				FolderNode.InsertDirectory( SubDirectory );
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="FolderPattern"></param>
		public void InsertDirectories( string FolderPattern )
		{
			IEnumerable<string> DepotDirectories = PerforceServer.GetDepotDirectories( FolderPattern, false );
			if( DepotDirectories != null )
			{
				foreach( string Directory in DepotDirectories )
				{
					InsertDirectory( Directory );
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void ShowDepotContextMenu()
		{
			TreeView.ContextMenuStrip.Tag = this;
		}
	}
}
