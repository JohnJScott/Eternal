// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace Eternal.EternalUtilities
{
	/// <summary>
	/// A treeview control that allows multiple selections.
	/// </summary>
	public partial class MultipleSelectionTreeView : TreeView
	{
		/// <summary></summary>
		/// <param name="Node"></param>
		/// <returns></returns>
		public delegate bool AllowNodeSelectionDelegate( TreeNode Node );

		/// <summary></summary>
		public AllowNodeSelectionDelegate AllowNodeSelection = null;

		private TreeNode FirstNode;
		private TreeNode LastNode;

		/// <summary>
		/// Forms utility constructor.
		/// </summary>
		public MultipleSelectionTreeView()
		{
			InitializeComponent();

			// Default to an empty set of selected nodes
			SelectedNodes = new Collection<TreeNode>();
		}

		/// <summary>A list of currently selected nodes.</summary>
		public Collection<TreeNode> SelectedNodes { get; private set; }

		/// <summary>
		/// Recursively set the hilited or unhilited colors for each node based on SelectedNodes
		/// </summary>
		/// <param name="BaseNode">The node to check the child nodes of.</param>
		public void RefreshSelection( TreeNode BaseNode )
		{
			if( BaseNode != null )
			{
				foreach( TreeNode Node in BaseNode.Nodes )
				{
					if( SelectedNodes.Contains( Node ) )
					{
						Node.BackColor = SystemColors.Highlight;
						Node.ForeColor = SystemColors.HighlightText;
					}
					else
					{
						Node.BackColor = BackColor;
						Node.ForeColor = ForeColor;
					}

					RefreshSelection( Node );
				}
			}
		}

		/// <summary>
		/// Add a unique node to the list of selected nodes.
		/// </summary>
		/// <param name="Node">Node to add.</param>
		private void AddUniqueNode( TreeNode Node )
		{
			if( !SelectedNodes.Contains( Node ) )
			{
				if( AllowNodeSelection == null || AllowNodeSelection( Node ) )
				{
					SelectedNodes.Add( Node );
				}
			}
		}

		/// <summary>
		/// Recursively select all nodes between FirstNode and LastNode, and add them to the list of selected nodes.
		/// </summary>
		/// <param name="Node">The node to check the children of.</param>
		/// <param name="bSelecting">true if we are between FirstNode and LastNode</param>
		private void ShiftSelect( TreeNode Node, ref bool bSelecting )
		{
			if( Nodes != null )
			{
				foreach( TreeNode TestNode in Node.Nodes )
				{
					if( TestNode == FirstNode || TestNode == LastNode )
					{
						bSelecting = !bSelecting;
						if( !bSelecting )
						{
							AddUniqueNode( TestNode );
							break;
						}
					}

					if( bSelecting )
					{
						AddUniqueNode( TestNode );
					}

					ShiftSelect( TestNode, ref bSelecting );
				}
			}
		}

		/// <summary>
		/// Handle the node mouse click event. This handles all selection, multi or not.
		/// </summary>
		/// <param name="e">System event arguments.</param>
		protected override void OnNodeMouseClick( TreeNodeMouseClickEventArgs e )
		{
			// No selection with a right mouse click or lack of nodes
			if( e != null )
			{
				if( e.Button == MouseButtons.Left && e.Node.TreeView.Nodes != null )
				{
					TreeNode RootNode = e.Node.TreeView.Nodes[0];
					TreeNode Node = e.Node;

					if( ModifierKeys == Keys.Control )
					{
						if( !SelectedNodes.Contains( Node ) )
						{
							// Select a previously unselected node 
							AddUniqueNode( Node );
							FirstNode = Node;
						}
						else
						{
							// Deselect a previously selected node
							SelectedNodes.Remove( Node );
						}

						LastNode = null;
					}
					else if( ModifierKeys == Keys.Shift )
					{
						if( FirstNode == null )
						{
							// Set the starting node to select from
							FirstNode = Node;
						}
						else
						{
							// Recursively select all nodes between FirstNode and LastNode
							LastNode = Node;

							bool bSelecting = false;
							ShiftSelect( RootNode, ref bSelecting );
						}
					}
					else
					{
						// Simple selection that removes all previous multi selections
						SelectedNodes.Clear();
						SelectedNodes.Add( Node );

						FirstNode = Node;
						LastNode = null;
					}

					// Set the selected/unselected colours for all nodes
					RefreshSelection( RootNode );
				}
			}

			// Call the base TreeView actions. This includes the TreeView click event.
			base.OnNodeMouseClick( e );
		}

		/// <summary>
		/// Suppress all default selection actions as they are all now handled in OnNodeMouseClick
		/// </summary>
		/// <param name="e">System event arguments.</param>
		protected override void OnBeforeSelect( TreeViewCancelEventArgs e )
		{
			if( e != null )
			{
				e.Cancel = true;
			}
		}
	}
}