// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Forms;

namespace ResilientP4
{
	/// <summary>
	///     A class to handle the selection of a label.
	/// </summary>
	public partial class SelectLabelDialog : Form
	{
		/// <summary></summary>
		private readonly MainForm RootApplication;

		/// <summary>
		/// </summary>
		/// <param name="Owner"></param>
		/// <param name="BranchName"></param>
		public SelectLabelDialog( MainForm Owner, Perforce PerforceServer, string BranchName )
		{
			if( Owner != null && PerforceServer != null )
			{
				RootApplication = Owner;
				RootApplication.SelectedLabel = "";

				InitializeComponent();

				LabelFilterLabel.Text = BranchName + "...";

				MainForm.ClearWaitMode();

				PerforceServer.GetLabels( LabelFilterLabel.Text );
				PopulateLabels( PerforceServer.PopulateLabels() );

				SelectLabelGridView.Sort( SelectLabelGridView.Columns[1], ListSortDirection.Descending );

				MainForm.ClearWaitMode();
			}
		}

		/// <summary>
		/// </summary>
		private void PopulateLabels( Collection<Dictionary<string, object>> LabelDetails )
		{
			foreach( Dictionary<string, object> CurrentLabel in LabelDetails )
			{
				using( DataGridViewRow Row = new DataGridViewRow() )
				{
					using( DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell() )
					{
						TextBoxCell.Value = CurrentLabel["Id"];
						Row.Cells.Add( TextBoxCell );
					}

					using( DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell() )
					{
						TextBoxCell.Value = CurrentLabel["Access"];
						Row.Cells.Add( TextBoxCell );
					}

					using( DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell() )
					{
						TextBoxCell.Value = CurrentLabel["Update"];
						Row.Cells.Add( TextBoxCell );
					}

					using( DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell() )
					{
						TextBoxCell.Value = CurrentLabel["Owner"];
						Row.Cells.Add( TextBoxCell );
					}

					using( DataGridViewCheckBoxCell CheckBoxCell = new DataGridViewCheckBoxCell() )
					{
						CheckBoxCell.Value = CurrentLabel["Locked"];
						Row.Cells.Add( CheckBoxCell );
					}

					using( DataGridViewTextBoxCell TextBoxCell = new DataGridViewTextBoxCell() )
					{
						TextBoxCell.Value = CurrentLabel["Description"];
						Row.Cells.Add( TextBoxCell );
					}

					SelectLabelGridView.Rows.Add( Row );
				}
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void SyncToLabelButtonClick( object Sender, EventArgs EventArguments )
		{
			if( SelectLabelGridView.SelectedRows.Count > 0 )
			{
				RootApplication.SelectedLabel = SelectLabelGridView.SelectedRows[0].Cells[0].Value.ToString();

				DialogResult = DialogResult.OK;
				Close();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void DataGridViewSelectionChanged( object Sender, EventArgs EventArguments )
		{
			SyncToLabelButton.Enabled = ( SelectLabelGridView.SelectedRows.Count == 1 );
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void CancelButtonClicked( object Sender, EventArgs EventArguments )
		{
			DialogResult = DialogResult.Cancel;
			Close();
		}
	}
}
