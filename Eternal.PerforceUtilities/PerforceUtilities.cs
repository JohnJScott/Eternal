// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Perforce.P4;

namespace Eternal.PerforceUtilities
{
	/// <summary>
	/// A class to store the current Perforce connection information.
	/// </summary>
	public class PerforceConnectionInfo
	{
		/// <summary>The Perforce repository.</summary>
		public Repository? PerforceRepository = null;

		/// <summary>The port retrieved when retrieving the default connection.</summary>
		public string Port = "";

		/// <summary>The user retrieved when retrieving the default connection.</summary>
		public string User = "";

		/// <summary>The workspace for the current connection.</summary>
		public string Workspace = "";

		/// <summary>The root of the workspace for the current connection.</summary>
		public string WorkspaceRoot = "";

		/// <summary>
		/// Create a default repository and connect to it. This is to acquire the port and user.
		/// </summary>
		public void SimpleConnect()
		{
			PerforceRepository = new Repository( new Server( new ServerAddress( "" ) ) );
			PerforceRepository.Connection.Connect( null );

			Port = PerforceRepository.Connection.Server.Address.Uri;
			User = PerforceRepository.Connection.UserName;
		}

		/// <summary>
		/// Disconnect from the repository.
		/// </summary>
		public void Disconnect()
		{
			PerforceRepository?.Connection.Disconnect();
			PerforceRepository = null;
		}

		/// <summary>
		/// Connect to a specific Perforce port with a user and workspace.
		/// </summary>
		public void Connect()
		{
			PerforceRepository = new Repository( new Server( new ServerAddress( Port ) ) );
			PerforceRepository.Connection.UserName = User;
			PerforceRepository.Connection.SetClient( Workspace );
			PerforceRepository.Connection.Connect( null );
		}

		/// <summary>
		/// Find the first current workspace on the current port with the current user that contains the passed in directory name.
		/// </summary>
		/// <param name="currentDirectory">The directory the workspace must contain.</param>
		/// <returns></returns>
		public bool FindWorkspace( string currentDirectory )
		{
	        string host_name = Environment.GetEnvironmentVariable( "COMPUTERNAME" ) ?? String.Empty;
			
	        ClientsCmdOptions opts = new ClientsCmdOptions( ClientsCmdFlags.None, null, null, 0, "" );
			IList<Client> clients = PerforceRepository?.GetClients( opts ) ?? new List<Client>();

			foreach( Client client in clients )
			{
				ConsoleLogger.Verbose( $" .... checking workspace: '{client.Name}' on host: '{client.Host}' with owner: '{client.OwnerName}' and root: '{client.Root}'" );

				if( !currentDirectory.ToLower().StartsWith( client.Root.ToLower() ) )
				{
					continue;
				}

				if( client.OwnerName.ToLower() != User.ToLower() )
				{
					continue;
				}

				if( client.Host.ToLower() != host_name.ToLower() )
				{
					continue;
				}

				Workspace = client.Name;
				WorkspaceRoot = client.Root;
				return true;
			}

			return false;
		}

		/// <summary>
		/// Gets the connection
		/// </summary>
		/// <returns>The Perforce connection info.</returns>
		public Connection? GetConnection()
		{
			return PerforceRepository?.Connection;
		}

		/// <summary>
		/// Returns the current workspace.
		/// </summary>
		/// <returns>The Perforce workspace.</returns>
		public Client? GetWorkspace()
		{
			return PerforceRepository?.Connection.Client;
		}

		/// <summary>
		/// Returns true if there is an active valid connection.
		/// </summary>
		/// <returns>Returns true if there is an active valid connection.</returns>
		public bool IsValid()
		{
			return PerforceRepository?.Connection.Status == ConnectionStatus.Connected;
		}

		/// <summary>
		/// Reset the connection info to force a fresh grab of the info.
		/// </summary>
		public void Invalidate()
		{
			PerforceRepository = null;
		}

		/// <summary>
		/// Gets a human readable version of the current connection.
		/// </summary>
		/// <returns>A human readable string of the current connection.</returns>
		public override string? ToString()
		{
			return $"Port: '{Port}' User: '{User}' Workspace: '{Workspace}'";
		}
	}

	/// <summary>
	/// A class to find the local Perforce connection information using the current working directory.
	/// </summary>
    public class PerforceUtilities
    {
		/// <summary>
		/// Disconnect from the current Perforce connection if there is an active and valid connection.
		/// </summary>
		/// <param name="connectionInfo">Current active connection.</param>
		/// <returns>True if the connection successfully disconnected.</returns>
		public static bool Disconnect( PerforceConnectionInfo connectionInfo )
		{
			if( !connectionInfo.IsValid() )
			{
				ConsoleLogger.Error( "Cannot disconnect from a server that hasn't been connected to." );
				return false;
			}

			connectionInfo.Disconnect();
			return !connectionInfo.IsValid();
		}

		private static bool GetDefaultConnection( PerforceConnectionInfo connectionInfo )
	    {
		    try
			{
				connectionInfo.SimpleConnect();
			}
			catch( Exception ex )
		    {
			    connectionInfo.Invalidate();
			    ConsoleLogger.Error( "Failed to connect to default Perforce server with exception: " + ex.Message );
		    }

		    return connectionInfo.IsValid();
	    }

	    private static bool FindLocalWorkspace( PerforceConnectionInfo connectionInfo, string currentDirectory )
	    {
	        string host_name = Environment.GetEnvironmentVariable( "COMPUTERNAME" ) ?? String.Empty;
			ConsoleLogger.Log( $" .. looking for workspace on '{host_name}' owned by '{connectionInfo.User}' which contains the folder '{currentDirectory}'" );

			if( !connectionInfo.FindWorkspace( currentDirectory ) )
			{
				ConsoleLogger.Error( $" .. failed to find workspace for '{host_name}' containing '{currentDirectory}'" );
				return false;
			}

			return true;
	    }

		/// <summary>
		/// Finds the default Perforce connection and the workspace that references the directory.
		/// </summary>
		/// <param name="currentDirectory">The directory the workspace must own.</param>
		/// <returns></returns>
		public static PerforceConnectionInfo GetConnectionInfo( string currentDirectory )
	    {
		    PerforceConnectionInfo connection_info = new PerforceConnectionInfo();

		    try
		    {
			    ConsoleLogger.Log( "Attempting to get default Perforce connection" );
			    if( GetDefaultConnection( connection_info ) )
			    {
				    ConsoleLogger.Log( $" .. found server '{connection_info.Port}' with user '{connection_info.User}'" );
				    if( FindLocalWorkspace( connection_info, currentDirectory ) )
				    {
					    ConsoleLogger.Log( $" .. found workspace '{connection_info.Workspace}' with root '{connection_info.WorkspaceRoot}'" );
				    }

				    Disconnect( connection_info );
			    }
			}
		    catch( Exception ex )
		    {
			    ConsoleLogger.Error( "Failed to get connection info with exception: " + ex.Message );
			}

			return connection_info;
	    }

		/// <summary>
		/// Connect to a Perforce repository given the connection info.
		/// </summary>
		/// <param name="connectionInfo">The Perforce repository, user name, workspace, and port.</param>
		/// <returns>True if the connection was successful.</returns>
	    public static bool Connect( PerforceConnectionInfo connectionInfo )
	    {
		    if( connectionInfo.IsValid() )
		    {
			    ConsoleLogger.Error( "Server already connected. Invalid connection info - " + connectionInfo.ToString() );
			    return false;
		    }

		    ConsoleLogger.Log( $"Attempting connection to '{connectionInfo.Port}' with user '{connectionInfo.User}' using workspace '{connectionInfo.Workspace}'" );
		    try
		    {
			    connectionInfo.Connect();

		    }
		    catch( Exception ex )
		    {
			    connectionInfo.Invalidate();
			    ConsoleLogger.Error( "Failed to connect to to Perforce server with exception: " + ex.Message );
		    }

		    return connectionInfo.IsValid();
	    }

		/// <summary>
		/// Sync all the files for the current workspace to head.
		/// </summary>
		/// <param name="connectionInfo">The Perforce repository, user name, workspace, and port.</param>
		/// <returns>True if the sync was successful.</returns>
		public static bool SyncWorkspace( PerforceConnectionInfo connectionInfo )
		{
			if( !connectionInfo.IsValid() )
			{
				ConsoleLogger.Error( "Please connect to a Perforce server before calling this function." );
				return false;
			}

			ConsoleLogger.Log( $"Syncing '{connectionInfo.Workspace}' to #head" );
			FileSpec all_files = FileSpec.DepotSpec( Path.Combine( connectionInfo.WorkspaceRoot, "..." ) );
			connectionInfo.GetWorkspace()?.SyncFiles( null, all_files );

			return true;
		}
	}
}