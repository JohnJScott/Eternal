// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace ResilientP4
{
	/// <summary>
	///     A class to handle the persistent storage of user preferences.
	/// </summary>
	public class UserConfiguration
	{
		/// <summary></summary>
		[CategoryAttribute( "Interrogation Settings" )]
		[DescriptionAttribute( "The number of changes to ask the server for when a new folder in the depot is clicked." )] 
		public int ChangesToReceive
		{
			get;
			set;
		}

        /// <summary></summary>
        [CategoryAttribute("General Settings")]
        [DescriptionAttribute("The number of times each operation is retried if it fails.")]
        public int RetryCount
        {
            get;
            set;
        }

		/// <summary></summary>
		[CategoryAttribute( "Interrogation Settings" )]
		[DescriptionAttribute( "The timeout in seconds of interrogation commands. Sync commands use a longer timeout." )]
		public int CommandTimeoutSeconds
		{
			get;
			set;
		}

		/// <summary></summary>
		[CategoryAttribute( "Interrogation Settings" )]
		[DescriptionAttribute( "The number of folders to recurse into when getting the details of a branch." )]
		public int FolderRecursionDepth
		{
			get;
			set;
		}

		/// <summary></summary>
		[CategoryAttribute( "Changelist Splitting Settings" )]
		[DescriptionAttribute( "Files are added to a set until the cumulative size exceeds this number of kilobytes." )]
		public int MaxFileSizePerChunkKB
		{
			get;
			set;
		}

		/// <summary></summary>
		[CategoryAttribute( "Changelist Splitting Settings" )]
		[DescriptionAttribute( "The maximum number of files that can be added to a change when splitting. "
								+ "The limit here is sum of the lengths of all filenames must not exceed the maximum Perforce command line length (32767 bytes). "
								+ "This equates to 126 names that are all the maximum size for Windows." )]
		public int MaxNumberOfFilesPerChunk
		{
			get;
			set;
		}

		/// <summary></summary>
		public string MostRecentServerAddress = "";

		private Collection<ServerInfo> InternalPerforceServers = new Collection<ServerInfo>();

		/// <summary></summary>
		[Browsable( false )]
		public IEnumerable<string> PerforceServerNames
		{
			get
			{
				return InternalPerforceServers.Select( x => x.DisplayName );
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public UserConfiguration()
		{
			ChangesToReceive = 300;
			CommandTimeoutSeconds = 30;
			FolderRecursionDepth = 2;
			MaxFileSizePerChunkKB = 50000;
			MaxNumberOfFilesPerChunk = 100;
			RetryCount = 3;
			MostRecentServerAddress = "";
		}

		/// <summary>
		///     Parse the p4tickets file to get a list of potential Perforce servers.
		/// </summary>
		public void FindAvailableServers()
		{
			InternalPerforceServers.Clear();

			string TicketsPath = Path.GetFullPath( Path.Combine( Environment.GetFolderPath( Environment.SpecialFolder.UserProfile ), "p4tickets.txt" ) );
			FileInfo TicketInfo = new FileInfo( TicketsPath );
			if( TicketInfo.Exists )
			{
				// Read in the tickets file
				List<string> Tickets = new List<string>();
				StreamReader TicketReader = TicketInfo.OpenText();
				while( !TicketReader.EndOfStream )
				{
					Tickets.Add( TicketReader.ReadLine() );
				}

				TicketReader.Close();

				// Parse out the servers
				foreach( string Line in Tickets )
				{
					if( Line.Contains( "=" ) && Line.Contains( ":" ) && Line.Length > 32 )
					{
						string[] ServerAndUser = Line.Split( '=' );
						if( ServerAndUser.Length == 2 )
						{
							string[] UserNameAndTicket = ServerAndUser[1].Split( ':' );
							if( UserNameAndTicket.Length == 2 )
							{
								AddServer( ServerAndUser[0], ServerAndUser[0], UserNameAndTicket[0], UserNameAndTicket[1] );
							}
						}
					}
				}
			}
		}

		/// <summary>
		///     Add a new server to the available list.
		/// </summary>
		/// <param name="NewServerDisplayName"></param>
		/// <param name="NewServerIPAddress"></param>
		/// <param name="UserWithTicket"></param>
		/// <param name="Ticket"></param>
		/// <returns></returns>
		public bool AddServer( string NewServerDisplayName, string NewServerIPAddress, string UserWithTicket = "", string Ticket = "" )
		{
			bool Updated = false;

			ServerInfo NewServerByName = InternalPerforceServers.Where( x => x.DisplayName.ToUpperInvariant() == NewServerDisplayName.ToUpperInvariant() ).FirstOrDefault();
			ServerInfo NewServerByAddress = InternalPerforceServers.Where( x => x.IPAddress.ToUpperInvariant() == NewServerIPAddress.ToUpperInvariant() ).FirstOrDefault();

			// Not found at all - add a new server
			if( NewServerByName == null && NewServerByAddress == null )
			{
				InternalPerforceServers.Add( new ServerInfo( NewServerDisplayName, NewServerIPAddress, UserWithTicket, Ticket ) );
				Updated = true;
			}
			else
			{
				if( NewServerDisplayName != NewServerIPAddress )
				{
					if( NewServerByName == null && NewServerByAddress != null )
					{
						NewServerByAddress.DisplayName = NewServerDisplayName;
						Updated = true;
					}
					else if( NewServerByName != null && NewServerByAddress == null )
					{
						NewServerByName.IPAddress = NewServerIPAddress;
					}
				}

				if( !String.IsNullOrEmpty( UserWithTicket ) )
				{
					if( NewServerByName != null )
					{
						NewServerByName.UsersWithTickets.Insert( 0, new string[2] { UserWithTicket, Ticket } );
					}
					else
					{
						NewServerByAddress.UsersWithTickets.Insert( 0, new string[2] { UserWithTicket, Ticket } );
					}
				}
			}

			return Updated;
		}

		/// <summary>
		/// </summary>
		/// <param name="ServerAddress"></param>
		/// <param name="UserName"></param>
		/// <returns></returns>
		public string GetUserTicket( string ServerAddress, string UserName )
		{
			string Ticket = null;

			ServerInfo ExistingServer = InternalPerforceServers.Where( x => x.IPAddress.ToUpperInvariant() == ServerAddress.ToUpperInvariant() ).FirstOrDefault();
			if( ExistingServer != null )
			{
				Ticket = ExistingServer.UsersWithTickets.Where( x => x[0].ToUpperInvariant() == UserName.ToUpperInvariant() ).Select( x => x[1] ).FirstOrDefault();
			}

			if( Ticket == null )
			{
				Ticket = "";
			}

			return Ticket;
		}

		/// <summary>
		/// </summary>
		/// <param name="ServerAddress"></param>
		/// <returns></returns>
		public Collection<string[]> GetUsersWithTickets( string ServerAddress )
		{
			Collection<string[]> UserNamesWithTickets = new Collection<string[]>();
			ServerInfo ExistingServer = InternalPerforceServers.Where( x => x.IPAddress.ToUpperInvariant() == ServerAddress.ToUpperInvariant() ).FirstOrDefault();
			if( ExistingServer != null )
			{
				UserNamesWithTickets = ExistingServer.UsersWithTickets;
			}

			return UserNamesWithTickets;
		}

		/// <summary>
		/// </summary>
		/// <param name="ServerAddress"></param>
		/// <returns></returns>
		public string GetDefaultUserWithTicket( string ServerAddress )
		{
			string UserWithTicket = "";
			Collection<string[]> UserNamesWithTickets = GetUsersWithTickets( ServerAddress );
			if( UserNamesWithTickets.Count > 0 )
			{
				UserWithTicket = UserNamesWithTickets[0][0];
			}

			return UserWithTicket;
		}

		/// <summary>
		/// </summary>
		public class ServerInfo
		{
			/// <summary></summary>
			public string DisplayName = "";

			/// <summary></summary>
			public string IPAddress = "";

			/// <summary></summary>
			[JsonIgnore]
			public Collection<string[]> UsersWithTickets = new Collection<string[]>();

			/// <summary>
			/// </summary>
			public ServerInfo()
			{
			}

			/// <summary>
			/// </summary>
			/// <param name="NewServerDisplayName"></param>
			/// <param name="NewServerIPAddress"></param>
			/// <param name="NewUserWithTicket"></param>
			/// <param name="NewTicket"></param>
			public ServerInfo( string NewServerDisplayName, string NewServerIPAddress, string NewUserWithTicket, string NewTicket )
			{
				DisplayName = NewServerDisplayName;
				IPAddress = NewServerIPAddress;
				if( !String.IsNullOrEmpty( NewUserWithTicket ) )
				{
					UsersWithTickets.Insert( 0, new string[2] { NewUserWithTicket, NewTicket } );
				}
			}
		}
	}
}
