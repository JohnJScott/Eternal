// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Eternal.EternalUtilities;

namespace ResilientP4
{
	/// <summary>
	///     A class to handle the connecting to a Perforce server, and to create a Perforce object with all the pertinent info.
	/// </summary>
	public partial class ConnectionDialog : Form
	{
		private readonly MainForm RootApplication;

		/// <summary></summary>
		public Perforce CurrentPerforceServer = null;

		/// <summary>
		/// </summary>
		/// <param name="Owner"></param>
		public ConnectionDialog( MainForm Owner )
		{
			RootApplication = Owner;

			InitializeComponent();
			FormsLogger.SetRecipient( this, ConsoleRichTextBox );

			RootApplication.Config.FindAvailableServers();

			ServerAddressComboBox.Items.AddRange( RootApplication.Config.PerforceServerNames.ToArray() );
			ServerAddressComboBox.SelectedItem = RootApplication.Config.MostRecentServerAddress;

			AcceptButton = RefreshServerAddressButton;
		}

		/// <summary>
		/// </summary>
		private void RefreshUserNameComboBox()
		{
			Collection<string[]> UsersWithTickets = RootApplication.Config.GetUsersWithTickets( CurrentPerforceServer.SafeServerTicketName );

			UserNameComboBox.Text = "";
			UserNameComboBox.Items.Clear();
			UserNameComboBox.Items.AddRange( CurrentPerforceServer.UserNames.ToArray() );
			if( UsersWithTickets != null )
			{
				IEnumerable<string> NewNames = UsersWithTickets.Where( x => !UserNameComboBox.Items.Contains( x[0] ) ).Select( x => x[0] );
				UserNameComboBox.Items.AddRange( NewNames.ToArray() );
			}

			UserNameComboBox.Enabled = ( UserNameComboBox.Items.Count > 0 );
			UserNameLabel.Enabled = UserNameComboBox.Enabled;

			if( UserNameComboBox.Enabled )
			{
				UserNameComboBox.Text = UserNameComboBox.Items[0].ToString();
				AcceptButton = RefreshUserNamesButton;
			}
		}

		/// <summary>
		/// </summary>
		private void RefreshWorkspaceComboBox( string UserName )
		{
			ConnectButton.Enabled = false;
			WorkspaceComboBox.Items.Clear();

			if( UserName.Length > 0 )
			{
				UserNameComboBox.SelectedItem = UserName;
				CurrentPerforceServer.GetWorkspaces( UserNameComboBox.Text );

				WorkspaceComboBox.Items.AddRange( CurrentPerforceServer.WorkspaceNames.ToArray() );
				if( WorkspaceComboBox.Items.Count > 0 )
				{
					WorkspaceComboBox.SelectedItem = CurrentPerforceServer.GetWorkspaceForHost( Environment.MachineName );
					ConnectButton.Enabled = true;
				}
			}
			else
			{
				WorkspaceComboBox.Text = "";
			}

			WorkspaceComboBox.Enabled = ( WorkspaceComboBox.Items.Count > 0 );
			WorkspaceLabel.Enabled = WorkspaceComboBox.Enabled;
			RefreshWorkspaceNamesButton.Enabled = WorkspaceComboBox.Enabled;
			if( RefreshWorkspaceNamesButton.Enabled )
			{
				AcceptButton = RefreshWorkspaceNamesButton;
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="Arguments"></param>
		private void UserNameChanged( object Sender, EventArgs Arguments )
		{
			if( !MainForm.IsInWaitMode() )
			{
				MainForm.SetWaitMode();

				// Get tickets
				if( CurrentPerforceServer.ReconnectWithCredentials( UserNameComboBox.Text ) )
				{
					CurrentPerforceServer.GetWorkspaces( UserNameComboBox.Text );
					RefreshWorkspaceComboBox( UserNameComboBox.Text );
				}

				MainForm.ClearWaitMode();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="Arguments"></param>
		private void RefreshUserNames( object Sender, EventArgs Arguments )
		{
			if( !MainForm.IsInWaitMode() )
			{
				MainForm.SetWaitMode();

				if( CurrentPerforceServer.GetUsersOnServer( UserNameComboBox.Text ) )
				{
					RefreshUserNameComboBox();

					// Get tickets
					if( CurrentPerforceServer.ReconnectWithCredentials( UserNameComboBox.Text ) )
					{
						RefreshWorkspaceComboBox( UserNameComboBox.Text );
					}
				}
				else
				{
					FormsLogger.Error( "Failed to find any users matching '" + UserNameComboBox.Text + "' on server '" + CurrentPerforceServer.SafeServerDisplayName + "'" );
					RefreshWorkspaceComboBox( "" );
				}

				MainForm.ClearWaitMode();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="Arguments"></param>
		private void ServerAddressChanged( object Sender, EventArgs Arguments )
		{
			if( !MainForm.IsInWaitMode() )
			{
				MainForm.SetWaitMode();

				// Get the server address
				CurrentPerforceServer = new Perforce( RootApplication );
				if( CurrentPerforceServer.ConnectWithoutCredentials( ServerAddressComboBox.Text ) )
				{
					// Server was found, so refresh the UI
					RootApplication.Config.MostRecentServerAddress = CurrentPerforceServer.SafeServerDisplayName;
					if( RootApplication.Config.AddServer( CurrentPerforceServer.SafeServerDisplayName, CurrentPerforceServer.SafeServerTicketName ) )
					{
						ServerAddressComboBox.Items.Clear();
						ServerAddressComboBox.Items.AddRange( RootApplication.Config.PerforceServerNames.ToArray() );
						ServerAddressComboBox.SelectedItem = CurrentPerforceServer.SafeServerDisplayName;
					}
				}

				// Get the users filtered by anything in the tickets file
				string FirstUserName = RootApplication.Config.GetDefaultUserWithTicket( CurrentPerforceServer.SafeServerTicketName );

				// If we have a ticket, try reconnecting with it
				if( FirstUserName.Length > 0 )
				{
					CurrentPerforceServer.ReconnectWithCredentials( FirstUserName );
				}

				if( CurrentPerforceServer.GetUsersOnServer( FirstUserName ) )
				{
					RefreshUserNameComboBox();

					if( FirstUserName.Length > 0 )
					{
						RefreshWorkspaceComboBox( FirstUserName );
					}
				}
				else
				{
					RefreshUserNameComboBox();
					RefreshWorkspaceComboBox( "" );
				}

				MainForm.ClearWaitMode();
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="EventArguments"></param>
		private void ConnectButtonClick( object Sender, EventArgs EventArguments )
		{
			CurrentPerforceServer.SetCurrentWorkspace( WorkspaceComboBox.Text );

			DialogResult = DialogResult.OK;
			Close();
		}
	}
}
