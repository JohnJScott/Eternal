// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Perforce.P4;
using Eternal.EternalUtilities;

namespace ResilientP4
{
	public class FileDetail
	{
		public long FileSize = -1;
		public string Action = "";
		public string HeadAction = "";

		public FileDetail( long InFileSize, string InAction, string InHeadAction )
		{
			FileSize = InFileSize;
			Action = InAction;
			HeadAction = InHeadAction;
		}
	}

	/// <summary>
	///     A class to handle the changelist nodes in the changelist view.
	/// </summary>
	[Serializable]
	public class ChangelistTreeNode : TreeNode
	{
		/// <summary></summary>
		public Perforce PerforceServer = null;
		private Dictionary<string, object> ChangelistDefinition;

		/// <summary></summary>
		public string CachedName
		{
			get;
			set;
		}

		/// <summary></summary>
		public int ChangelistId
		{
			get
			{
				if( ChangelistDefinition != null )
				{
					return ( int )ChangelistDefinition["Id"];
				}

				return -1;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InPerforceServer"></param>
		/// <param name="UserName"></param>
		public ChangelistTreeNode( Perforce InPerforceServer, string UserName, string InImageKey )
		{
			PerforceServer = InPerforceServer;
			ChangelistDefinition = null;
			ImageKey = InImageKey;
			SelectedImageKey = InImageKey;

			if( String.IsNullOrEmpty( UserName ) )
			{
				CachedName = PerforceServer.CurrentUserName;
				Text = CachedName;
				int PendingChangelistCount = PerforceServer.GetPendingChangesCount();
				if( PendingChangelistCount > 0 )
				{
					Text += " with " + PendingChangelistCount + " pending change(s).";
				}			
			}
			else
			{
				CachedName = UserName;
				Text = CachedName;
				int SubmittedChangelistCount = PerforceServer.GetUserChangesCount( CachedName );
				if( SubmittedChangelistCount > 0 )
				{
					Text += " with " + SubmittedChangelistCount + " submitted change(s).";
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InPerforceServer"></param>
		/// <param name="InChangelistDefinition"></param>
		public ChangelistTreeNode( Perforce InPerforceServer, Dictionary<string, object> InChangelistDefinition, string InImageKey )
		{
			PerforceServer = InPerforceServer;
			ChangelistDefinition = InChangelistDefinition;
			Text = ToString();
			ImageKey = InImageKey;
			SelectedImageKey = ImageKey;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="InPerforceServer"></param>
		/// <param name="FileName"></param>
		/// <param name="FileSize"></param>
		public ChangelistTreeNode( Perforce InPerforceServer, string FileName, FileDetail Details )
		{
			PerforceServer = InPerforceServer;
			ChangelistDefinition = null;

			if( Details.Action == "None" )
			{
				switch( Details.HeadAction )
				{
				case "Edit":
					Text = FileName + " (edited " + StringHelper.GetMemoryString( Details.FileSize ) + ")";
					break;

				case "Add":
					Text = FileName + " (added " + StringHelper.GetMemoryString( Details.FileSize ) + ")";
					break;

				case "Delete":
					Text = FileName + " (deleted)";
					break;

				default:
					Text = FileName;
					break;
				}
			}
			else
			{
				switch( Details.Action )
				{
				case "Edit":
					Text = FileName + " (opened for edit, " + StringHelper.GetMemoryString( Details.FileSize ) + ")";
					break;

				case "Add":
					Text = FileName + " (marked for add)";
					break;

				case "Delete":
					Text = FileName + " (marked for delete)";
					break;

				default:
					Text = FileName;
					break;
				}
			}

			ImageKey = "document-grey.ico";
			SelectedImageKey = ImageKey;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			if( ChangelistDefinition != null )
			{
				int ChangelistId = ( int )ChangelistDefinition["Id"];
				if( ChangelistId == 0 )
				{
					return "Default changelist";
				}
				else
				{
					return "Change " + ChangelistId + " - " + ChangelistDefinition["Description"];
				}
			}

			return "";
		}

		/// <summary>
		/// </summary>
		/// <param name="info"></param>
		/// <param name="context"></param>
		protected ChangelistTreeNode( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}

		/// <summary>
		/// 
		/// </summary>
		public void InsertChanges()
		{
			// Insert pending changes for current user
			if( PerforceServer.GetPendingChangesCount() > 0 )
			{
				ChangelistTreeNode NewPendingNode = new ChangelistTreeNode( PerforceServer, "", "user-yellow.ico" );
				Collection<Dictionary<string, object>> PendingChangelistsSummary = PerforceServer.PopulatePendingChanges();
				foreach( Dictionary<string, object> Change in PendingChangelistsSummary )
				{
					NewPendingNode.Nodes.Add( new ChangelistTreeNode( PerforceServer, Change, "triangle-red.ico" ) );
				}

				Nodes.Add( NewPendingNode );
			}

			// Insert submitted changes for all users
			foreach( string UserName in PerforceServer.ChangesUsers )
			{
				ChangelistTreeNode NewSubmittedNode = new ChangelistTreeNode( PerforceServer, UserName, "user-yellow.ico" );

				Collection<Dictionary<string, object>> SubmittedChangelistsSummary = PerforceServer.PopulateChanges( UserName );
				foreach( Dictionary<string, object> Change in SubmittedChangelistsSummary )
				{
					NewSubmittedNode.Nodes.Add( new ChangelistTreeNode( PerforceServer, Change, "triangle-grey.ico" ) );
				}

				Nodes.Add( NewSubmittedNode );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public void AugmentChanges()
		{
			// Check to see if this is a change, and not a user name
			if( ChangelistDefinition != null )
			{
				ChangelistTreeNode UserNode = ( ChangelistTreeNode )Parent;
				ChangelistTreeNode BranchNode = ( ChangelistTreeNode )UserNode.Parent;

				if( BranchNode != null && UserNode != null && UserNode.PerforceServer != null )
				{
					UserNode.PerforceServer.GetChangelistDetails( BranchNode.CachedName, UserNode.CachedName, ChangelistId );

					Nodes.Clear();

					Collection<Dictionary<string, object>> ChangelistFiles = UserNode.PerforceServer.PopulateChangeDetails( UserNode.CachedName, ChangelistId );
					foreach( Dictionary<string, object> FileSet in ChangelistFiles )
					{
						foreach( string FileName in FileSet.Keys )
						{
							FileDetail Details = ( FileDetail )FileSet[FileName];
							Nodes.Add( new ChangelistTreeNode( UserNode.PerforceServer, FileName, Details ) );
						}
					}
				}
			}
		}

		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public bool IsPendingChangelistNode()
		{
			return PerforceServer.IsPendingChangelist( ChangelistId );
		}

		/// <summary>
		/// 
		/// </summary>
		public void ShowChangelistContextMenu( ContextMenuStrip InChangelistViewMenuStrip )
		{
			TreeView.ContextMenuStrip = null;
			if( ChangelistDefinition != null && ChangelistId > -1 )
			{
				TreeView.ContextMenuStrip = InChangelistViewMenuStrip;
				TreeView.ContextMenuStrip.Tag = this;
			}	
		}
	}
}
