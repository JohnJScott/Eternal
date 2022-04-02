/*******************************************************************************

Copyright (c) 2011-12, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: Connection.cs
 *
 * Author(s)	: dbb
 *
 * Description	: Class used to abstract a server connection.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Perforce.P4;

namespace Perforce.P4
{
	/// <summary>
	/// Flags for the server connection status.
	/// </summary>
	[Flags]
	public enum ConnectionStatus
	{
		/// <summary>
		/// Disconnected from server.
		/// </summary>
		Disconnected = 0x0000,
		/// <summary>
		/// Connected to server.
		/// </summary>
		Connected = 0x0001
	}
	/// <summary>
	/// Represents the logical connection between a specific Perforce
	/// Server instance and a specific client application. 
	/// </summary>
	public class Connection : IDisposable
	{
		public Connection(Server server) { Server = server; }
		public ConnectionStatus Status { get; set; }
		public Server Server { get; private set; }
		public ClientMetadata ClientMetadata { get; private set; }

		public string UserName
		{
			get { return username; }
			set
			{
				if (_p4server != null)
				{
					_p4server.User = value != null ? value : string.Empty;
				}
				username = value;
			}
		}

		public Credential Credential
		{
			get { return credential; }
			set
			{
				if (_p4server != null)
				{
					_p4server.Password = value.Ticket != null ? value.Ticket : string.Empty;
				}
				credential = value;
			}
		}

		public Client Client
		{
			get { return client; }
			set
			{
				client = value;

				if (_p4server != null)
				{
					if ((client != null) && (client.Name != null))
					{
						_p4server.Client = client.Name;
						client.Initialize(this);
					}
					else
					{
						_p4server.Client = string.Empty;
					}
				}
			}
		}

		public string CurrentWorkingDirectory
		{
			get { return _cwd; }
			set
			{
				if (_p4server != null)
				{
					_p4server.CurrentWorkingDirectory = value;
				}
				_cwd = value;
			}
		}

		public void SetClient(string clientId)
		{
			Client c = new Client();
			c.Name = clientId;

			Client = c;
		}

		public string CharacterSetName //{ get; set; }
		{
			get { return _p4server.CharacterSet; }
		}
		internal P4Server _p4server;

		public event P4Server.InfoResultsDelegate InfoResultsReceived
		{
			add { _p4server.InfoResultsReceived += value; }
			remove { _p4server.InfoResultsReceived -= value; }
		}

		public event P4Server.ErrorDelegate ErrorReceived
		{
			add { _p4server.ErrorReceived += value; }
			remove { _p4server.ErrorReceived -= value; }
		}

		public event P4Server.TextResultsDelegate TextResultsReceived
		{
			add { _p4server.TextResultsReceived += value; }
			remove { _p4server.TextResultsReceived -= value; }
		}

		public event P4Server.TaggedOutputDelegate TaggedOutputReceived
		{
			add { _p4server.TaggedOutputReceived += value; }
			remove { _p4server.TaggedOutputReceived -= value; }
		}

		public event P4Server.CommandEchoDelegate CommandEcho
		{
			add { _p4server.CommandEcho += value; }
			remove { _p4server.CommandEcho -= value; }
		}

		private ServerAddress port;
		private Client client;
		private string username;
		private string _cwd;
		private Credential credential;

		/// <summary>
		/// What API level does the server support
		/// </summary>
		public int ApiLevel
		{
			get { return _p4server.ApiLevel; }
		}

		public bool Connect(Options options)
		{
			lock (this)
			{
				if (_p4server != null)
				{
					_p4server.Dispose();
				}

				string password = null;
				string ticket = null;


				if ((options != null) && (options.Keys.Contains("Ticket")))
				{
					ticket = options["Ticket"];
				}
				else
				{
					if ((options != null) && (options.Keys.Contains("Password")))
					{
						password = options["Password"];
					}
				}

				string clientName = null;
				if ((Client != null) && (string.IsNullOrEmpty(Client.Name) == false))
				{
					clientName = Client.Name;
				}
				try
				{
					_p4server = new P4Server(Server.Address.Uri, UserName, password, clientName);
					if (_commandTimeout != TimeSpan.Zero)
					{
						_p4server.RunCmdTimout = _commandTimeout;
					}
					// run a help command
					_p4server.RunCommand("help", 0, false, null, 0);
				}
				catch (Exception)
				{
					Server.SetState(ServerState.Offline);
					throw;
				}
				if ((_p4server != null) && (_p4server.pServer != IntPtr.Zero))
				{
					if (ticket != null)
					{
						_p4server.Password = ticket;
					}
					Status = ConnectionStatus.Connected;
					Server.SetState(ServerState.Online);
                    if (Server.Metadata==null)
                    {
                        ServerMetaData value = new ServerMetaData();
                        if (_p4server.ApiLevel >= 30)
                        {
                            string[] args = new string[1];
                            args[0] = "-s";
                            _p4server.RunCommand("info", 0, true, args, 1);
                        }
                        else
                        {
                            _p4server.RunCommand("info", 0, true, null, 0);
                        }
                        TaggedObjectList results = _p4server.GetTaggedOutput(0);
                        if (results!=null)
                        {
                            value.FromGetServerMetaDataCmdTaggedOutput(results[0]); 
                        }
                        Server.SetMetadata(value);
                    }

					if ((Server.Address == null) || (string.IsNullOrEmpty(Server.Address.Uri)))
					{
						string newUri = _p4server.Port;
						Server.Address = new ServerAddress(newUri);
					}
					if (string.IsNullOrEmpty(UserName))
					{
						UserName = _p4server.User;
					}
					if (string.IsNullOrEmpty(clientName))
					{
						clientName = _p4server.Client;
						if (Client == null)
						{
							Client = new Client();
						}
						Client.Name = clientName;
					}

					if ((Client != null) && (string.IsNullOrEmpty(Client.Name) == false))
					{
						try
						{
							Client.Initialize(this);
						}
						catch (Exception ex)
						{
							LogFile.LogException("P4API.NET", ex);
							if ((_p4server == null) || (_p4server.pServer == IntPtr.Zero))
							{
								// Connection failed and was discarderd, so rethrow the error
								throw;
							}
							// can't initialize yet, probably need to login
							// so ignore this error, we'll init the client later.
						} 

						if (Client.Initialized)
						{
							if ((string.IsNullOrEmpty(Client.Root) == false) && (System.IO.Directory.Exists(Client.Root)))
							{
								_p4server.CurrentWorkingDirectory = Client.Root;
							}
							else
							{
								if (Client.AltRoots != null)
								{
									foreach (string altRoot in Client.AltRoots)
									{
										if ((string.IsNullOrEmpty(altRoot) == false) && (System.IO.Directory.Exists(altRoot)))
										{
											_p4server.CurrentWorkingDirectory = Client.Root;
											return true;
										}
									}
									throw new P4Exception(ErrorSeverity.E_WARN, "The client root and alternate roots do not exist on this system");
								}
							}
						}
					}
					if ((options != null) && (options.Keys.Contains("ProgramName")))
					{
						_p4server.ProgramName = options["ProgramName"];
					}
					if ((options != null) && (options.Keys.Contains("ProgramVersion")))
					{
						_p4server.ProgramVersion = options["ProgramVersion"];
					}
					return true;
				}
				Server.SetState(ServerState.Offline);
				return false;
			}
		}

		public bool TrustAndConnect(Options options, string trustFlag, string fingerprint)
		{
			lock (this)
			{
				if (_p4server != null)
				{
					return true;
				}

				string password = null;
				string ticket = null;


				if ((options != null) && (options.Keys.Contains("Ticket")))
				{
					ticket = options["Ticket"];
				}
				else
				{
					if ((options != null) && (options.Keys.Contains("Password")))
					{
						password = options["Password"];
					}
				}

				string clientName = null;
				if ((Client != null) && (string.IsNullOrEmpty(Client.Name) == false))
				{
					clientName = Client.Name;
				}
				try
				{
					_p4server = new P4Server(Server.Address.Uri, UserName, password, clientName, trustFlag, fingerprint);

					// run a help command
					//_p4server.RunCommand("help", false, null, 0);
					if (_commandTimeout != null)
					{
						_p4server.RunCmdTimout = _commandTimeout;
					}
				}
				catch (Exception)
				{
					Server.SetState(ServerState.Offline);
					throw;
				}
				if ((_p4server != null) && (_p4server.pServer != IntPtr.Zero))
				{
					if (ticket != null)
					{
						_p4server.Password = ticket;
					}
					Status = ConnectionStatus.Connected;
					Server.SetState(ServerState.Online);

					if (string.IsNullOrEmpty(Server.Address.Uri))
					{
						string newUri = _p4server.Port;
						Server.Address = new ServerAddress(newUri);
					}
					if (string.IsNullOrEmpty(UserName))
					{
						UserName = _p4server.User;
					}
					if (string.IsNullOrEmpty(clientName))
					{
						clientName = _p4server.Client;
					}

					if ((Client != null) && (string.IsNullOrEmpty(Client.Name) == false))
					{
						try
						{
							Client.Initialize(this);
						}
						catch { } // can't initialize yet, probably need to login

						if (Client.Initialized)
						{
							if ((string.IsNullOrEmpty(Client.Root) == false) && (System.IO.Directory.Exists(Client.Root)))
							{
								_p4server.CurrentWorkingDirectory = Client.Root;
								return true;
							}
							else
							{
								if (Client.AltRoots != null)
								{
									foreach (string altRoot in Client.AltRoots)
									{
										if ((string.IsNullOrEmpty(altRoot) == false) && (System.IO.Directory.Exists(altRoot)))
										{
											_p4server.CurrentWorkingDirectory = Client.Root;
											return true;
										}
									}
									throw new P4Exception(ErrorSeverity.E_WARN, "The client root and alternate roots do not exist on this system");
								}
							}
						}
					}
					return true;
				}
				Server.SetState(ServerState.Offline);
				return false;
			}
		}

		/// <summary>
		/// Release the connection held by the bridge to the server. This will cause the 
		/// bridge to call init before the next command is run, forcing it to reinitialize 
		/// any cached connection settings.
		/// </summary>
		public void ReleaseConnection()
		{
			_p4server.Disconnect();
		}

		public bool Disconnect()
		{
			return Disconnect(null);
		}

		public bool Disconnect(Options options)
		{
			lock (this)
			{
				if (_p4server == null)
				{
					return false;
				}
				_p4server.Close();
				_p4server.Dispose();
				_p4server = null;
				Status = ConnectionStatus.Disconnected;
				return true;
			}
		}
		/// <summary>
		/// Run a Login on the Perforce Server
		/// </summary>
		/// <param name="password">User' password</param>
		/// <param name="options">Login options (see remarks in help file)</param>
		/// <param name="options">Login as user (see remarks in help file)</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// <br/><b>p4 help login</b>
		/// <br/> 
		/// <br/>     login -- Log in to Perforce by obtaining a session ticket
		/// <br/> 
		/// <br/>     p4 login [-a -p] [-h &lt;host&gt; user]
		/// <br/>     p4 login [-s]
		/// <br/> 
		/// <br/> 	The login command enables a user to access Perforce until the session
		/// <br/> 	expires or the user logs out.
		/// <br/> 
		/// <br/> 	When a user logs in to Perforce, they are prompted for a password
		/// <br/> 	If they enter the correct password, they are issued a ticket.  The
		/// <br/> 	ticket expires when the default timeout value has been reached and
		/// <br/> 	is valid only for the host machine where the 'login' command was
		/// <br/> 	executed (see below for exception).
		/// <br/> 
		/// <br/> 	The ticket can be used anywhere that a password can be used.
		/// <br/> 
		/// <br/> 	Example: p4 -P &lt;ticket value&gt; changes -m1
		/// <br/> 
		/// <br/> 	The -a flag causes the server to issue a ticket that is valid on all
		/// <br/> 	host machines.
		/// <br/> 
		/// <br/> 	The -h flag causes the server to issue a ticket that is valid on the
		/// <br/> 	specified host (IP address).  This flag can only be used when the
		/// <br/> 	login request is for another user.
		/// <br/> 
		/// <br/> 	The -p flag displays the ticket, but does not store it on the client
		/// <br/> 	machine.
		/// <br/> 
		/// <br/> 	The -s flag displays the status of the current ticket (if there is
		/// <br/> 	one).
		/// <br/> 
		/// <br/> 	Specifying a username as an argument to 'p4 login' requires 'super'
		/// <br/> 	access, which is granted by 'p4 protect'.  In this case, 'p4 login'
		/// <br/> 	does not prompt for the password (you must already be logged in).
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Credential Login(string password, Options options, string user)
		{

			if (_p4server.ReqiresLogin)
			{
				string tkt = string.Empty;
				string usr = UserName;
				DateTime exp = DateTime.MaxValue;

				// Login into the server. The login command will prompt
				// for the password. If user does not have a password, 
				// the command will just return with a result saying 
				// that login is not required.
				P4Command login = null;
				if (user == null)
				{
					login = new P4Command(this, "login", true);
				}
				else
				{
					login = new P4Command(this, "login", true, user);
					usr = user;
				}
				login.Responses = new Dictionary<string, string>();
				login.Responses["DefaultResponse"] = password;
				P4CommandResult results;

				try
				{
					//if (options == null)
					//{
					//    options = new Options();
					//}
					//options["-p"] = null;
					results = login.Run(options);
					if (results.Success == false)
					{
						return null;
					}
					if ((results.Success) && (results.InfoOutput != null) && (results.InfoOutput.Count > 0))
					{
						if ((results.InfoOutput[0].Info.Contains("'login' not necessary")) ||
								(results.InfoOutput[0].Info.Contains("logged in")))
						{
							return new Credential(usr, tkt, exp);
						}
						else if (options.ContainsKey("-p"))
						{
							tkt = results.InfoOutput[0].Info;
						}
						else
						{
							tkt = password;
						}
					}
					if ((results.TaggedOutput != null) && (results.TaggedOutput.Count > 0))
					{
						if (results.TaggedOutput[0].ContainsKey("TicketExpiration"))
						{
							string expStr = string.Empty;
							expStr = results.TaggedOutput[0]["TicketExpiration"];
							long seconds = 0;
							long.TryParse(expStr, out seconds);
							exp = DateTime.Now.AddSeconds(seconds);
						}
						if (results.TaggedOutput[0].ContainsKey("User"))
						{
							usr = results.TaggedOutput[0]["User"];
						}
					}
					return new Credential(UserName, tkt, exp);
				}
				catch
				{
					return null;
				}
			}
			_p4server.User = UserName;
			_p4server.Password = password;
			return new Credential(UserName, password);
		}

		/// <summary>
		/// Login to the Perforce Server
		/// </summary>
		/// <param name="password">User' password</param>
		/// <param name="options">Login options (see remarks in help file)</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// <br/><b>p4 help login</b>
		/// <br/> 
		/// <br/>     login -- Log in to Perforce by obtaining a session ticket
		/// <br/> 
		/// <br/>     p4 login [-a -p] [-h &lt;host&gt; user]
		/// <br/>     p4 login [-s]
		/// <br/> 
		/// <br/> 	The login command enables a user to access Perforce until the session
		/// <br/> 	expires or the user logs out.
		/// <br/> 
		/// <br/> 	When a user logs in to Perforce, they are prompted for a password
		/// <br/> 	If they enter the correct password, they are issued a ticket.  The
		/// <br/> 	ticket expires when the default timeout value has been reached and
		/// <br/> 	is valid only for the host machine where the 'login' command was
		/// <br/> 	executed (see below for exception).
		/// <br/> 
		/// <br/> 	The ticket can be used anywhere that a password can be used.
		/// <br/> 
		/// <br/> 	Example: p4 -P &lt;ticket value&gt; changes -m1
		/// <br/> 
		/// <br/> 	The -a flag causes the server to issue a ticket that is valid on all
		/// <br/> 	host machines.
		/// <br/> 
		/// <br/> 	The -h flag causes the server to issue a ticket that is valid on the
		/// <br/> 	specified host (IP address).  This flag can only be used when the
		/// <br/> 	login request is for another user.
		/// <br/> 
		/// <br/> 	The -p flag displays the ticket, but does not store it on the client
		/// <br/> 	machine.
		/// <br/> 
		/// <br/> 	The -s flag displays the status of the current ticket (if there is
		/// <br/> 	one).
		/// <br/> 
		/// <br/> 	Specifying a username as an argument to 'p4 login' requires 'super'
		/// <br/> 	access, which is granted by 'p4 protect'.  In this case, 'p4 login'
		/// <br/> 	does not prompt for the password (you must already be logged in).
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public Credential Login(string password, Options options)
		{
			return Login(password, options, null);
		}

		/// <summary>
		/// Automate the Login to the Perforce Server
		/// </summary>
		/// <param name="password">User' password</param>
		/// <param name="options">Login options (see remarks in help file)</param>
		/// <param name="options">Login as user (see remarks in help file)</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// Runs the login process. If the server is using ticket based 
		/// authentication, actually runs the logn three times. Once to 
		/// login and update the ticket file, once to get the ticket from
		/// the server and finally once to get the ticket expiration data.
		/// </remarks>

		public Credential Login(string password)
		{

			if (_p4server.ReqiresLogin)
			{
				// Login into the server. The login command will prompt
				// for the password. If user does not have a password, 
				// the command will just return with a result saying 
				// that login is not required.
				P4Command login = new P4Command(this, "login", true);

				login.Responses = new Dictionary<string, string>();
				login.Responses["DefaultResponse"] = password;
				P4CommandResult results;

				string tkt = string.Empty;
				string usr = UserName;
				DateTime exp = DateTime.MaxValue;

				//try
				//{
				//    string ssoScript = Environment.GetEnvironmentVariable("P4LOGINSSO");
				//    if (ssoScript != null)
				//    {
				//        // using sso
				//        results = login.Run(null);
				//    }
				//}
				//catch (Exception ex)
				//{
				//    return null;
				//}
				try
				{
					//if (options == null)
					//{
					//    options = new Options();
					//}
					//options["-p"] = null;
					results = login.Run(null);

					if ((results.InfoOutput != null) && (results.InfoOutput.Count > 0) &&
						(results.InfoOutput[0].Info.Contains("'login' not necessary")))
					{
						return new Credential(usr, tkt, exp);
					}
					//else if (results.InfoOutput != null)
					//{
					//    tkt = results.InfoOutput[0].Info;

					//    _p4server.Password = tkt;
					//}
				}
				catch (Exception ex)
				{
					throw;
				}
				if (results.Success)
				{
					P4Server svr = _p4server;

					try
					{
						Options opt = new Options();

						opt["-p"] = null;
						results = login.Run(opt);

						if ((results.InfoOutput != null) && (results.InfoOutput[0].Info.Contains("'login' not necessary")))
						{
							return new Credential(usr, tkt, exp);
						}
						else if (results.InfoOutput != null)
						{
							tkt = results.InfoOutput[results.InfoOutput.Count - 1].Info;

							_p4server.Password = tkt;
						}

						login = new P4Command(svr, "login", true);

						opt = new Options();
						opt["-s"] = null;
						results = login.Run(opt);

						if ((results.TaggedOutput != null) && (results.TaggedOutput.Count > 0))
						{
							if (results.TaggedOutput[0].ContainsKey("TicketExpiration"))
							{
								string expStr = string.Empty;
								expStr = results.TaggedOutput[0]["TicketExpiration"];
								long seconds = 0;
								long.TryParse(expStr, out seconds);
								exp = DateTime.Now.AddSeconds(seconds);
							}
							if (results.TaggedOutput[0].ContainsKey("User"))
							{
								usr = results.TaggedOutput[0]["User"];
							}
						}
						else if (results.InfoOutput != null)
						{
							string line = results.InfoOutput[0].Info;

							int idx = line.IndexOf("ticket");
							if (idx < 0)
								return null;

							// "user " is 5 characters (with space)
							usr = line.Substring(5, idx - 5).Trim();

							string hStr;
							idx = line.IndexOf("expires in ");
							if (idx < 0)
								return null;
							idx += 11;
							int idx2 = line.IndexOf(" hours");
							hStr = line.Substring(idx, idx2 - idx).Trim();

							int hours;
							int.TryParse(hStr, out hours);

							string mStr;
							idx = idx2 + 6; // "hours " is 6 chars
							if (idx < 0)
								return null;
							idx2 = line.IndexOf(" minutes");
							mStr = line.Substring(idx, idx2 - idx).Trim();

							int minutes;
							int.TryParse(mStr, out minutes);

							exp = DateTime.Now.AddHours(hours).AddMinutes(minutes);
						}
						if ((Client.Initialized == false) && (Client != null) && 
							(string.IsNullOrEmpty(Client.Name) == false))
						{
							try
							{
								Client.Initialize(this);
							}
							catch { } // can't initialize yet, probably need to login

							if (Client.Initialized)
							{
								if ((string.IsNullOrEmpty(Client.Root) == false) && (System.IO.Directory.Exists(Client.Root)))
								{
									_p4server.CurrentWorkingDirectory = Client.Root;
								}
								else
								{
									if (Client.AltRoots != null)
									{
										foreach (string altRoot in Client.AltRoots)
										{
											if ((string.IsNullOrEmpty(altRoot) == false) && (System.IO.Directory.Exists(altRoot)))
											{
												_p4server.CurrentWorkingDirectory = Client.Root;
												break;
											}
										}
									}
								}
							}
						}

					}
					catch
					{
						return null;
					}

					return new Credential(usr, tkt, exp);
				}
				return null;
			}
			_p4server.User = UserName;
			_p4server.Password = password;
			return new Credential(UserName, password);
		}

		/// <summary>
		/// Logout of the Perforce server
		/// </summary>
		/// <param name="options">Logout options (see remarks in help file)</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// <br/><b>p4 help logout</b>
		/// <br/> 
		/// <br/>     logout -- Log out from Perforce by removing or invalidating a ticket.
		/// <br/> 
		/// <br/>     p4 logout [-a]
		/// <br/> 
		/// <br/> 	The logout command removes the ticket on the client. To resume using
		/// <br/> 	Perforce, the user must log in again.
		/// <br/> 
		/// <br/> 	The -a flag invalidates the ticket on the server, which will log out
		/// <br/> 	all users of the ticket.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public bool Logout(Options options)
		{
			bool results = false;
			if (_p4server.ReqiresLogin)
			{
				results = _p4server.Logout(options);
			}
			else
			{
				_p4server.User = string.Empty;
				_p4server.Password = string.Empty;
			}
			return results;
		}

		/// <summary>
		/// Run the client side command trust
		/// </summary>
		/// <param name="options">trust options (see remarks in help file)</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// <br/><b>p4 trust -h</b>
		/// <br/> 
		/// <br/>         trust -- Establish trust of an SSL connection
		/// <br/> 
		/// <br/>         p4 trust [ -l -y -n -d -f -r -i &lt;fingerprint&gt; ]
		/// <br/> 
		/// <br/>         Establish trust of an SSL connection.  This client command manages
		/// <br/>         the p4 trust file.  This file contains fingerprints of the keys
		/// <br/>         received on ssl connections.  When an SSL connection is made, this
		/// <br/>         file is examined to determine if the SSL connection has been used
		/// <br/>         before and if the key is the same as a previously seen key for that
		/// <br/>         connection.  Establishing trust with a connection prevents undetected
		/// <br/>         communication interception (man-in-the-middle) attacks.
		/// <br/> 
		/// <br/>         Most options are mutually exclusive.  Only the -r and -f options
		/// <br/>         can be combined with the others.
		/// <br/> 
		/// <br/>         The -l flag lists existing known fingerprints.
		/// <br/> 
		/// <br/>         Without options, this command will make a connection to a server
		/// <br/>         and examine the key if present, if one cannot be found this command
		/// <br/>         will show a fingerprint and ask if this connection should be trusted.
		/// <br/>         If a fingerprint exists and does not match, an error that a possible
		/// <br/>         security problems exists will be displayed.
		/// <br/> 
		/// <br/>         The -y flag will cause prompts to be automatically accepted.
		/// <br/> 
		/// <br/>         The -n flag will cause prompts to be automatically refused.
		/// <br/> 
		/// <br/>         The -d flag will remove an existing trusted fingerprint of a connection.
		/// <br/> 
		/// <br/>         The -f flag will force the replacement of a mismatched fingerprint.
		/// <br/> 
		/// <br/>         The -i flag will allow a specific fingerprint to be installed.
		/// <br/> 
		/// <br/>         The -r flag specifies that a replacement fingerprint is to be
		/// <br/>         affected.  Replacement fingerprints can be used in anticipation
		/// <br/>         of a server replacing its key.  If a replacement fingerprint
		/// <br/>         exists for a connection and the primary fingerprint does not match
		/// <br/>         while the replacement fnigerprint does, the replacement fingerprint
		/// <br/>         will replace the primary.  This flag can be combined with -l, -i,
		/// <br/>         or -d. 
		/// </remarks>
		public bool Trust(Options options, string fingerprint)
		{
			P4.P4Command trustCmd = null;
			if (string.IsNullOrEmpty(fingerprint))
			{
				trustCmd = new P4Command(this, "trust", false);
			}
			else
			{
				trustCmd = new P4Command(this, "trust", false, fingerprint);
			}
			P4.P4CommandResult r = trustCmd.Run(options);
			if (r.Success != true)
			{
				P4Exception.Throw(r.ErrorList);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Set the current user's password on the Perforce server.
		/// </summary>
		/// <param name="OldPassword">User's old password</param>
		/// <param name="NewPassword">User's new password</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// <br/><b>p4 help passwd</b>
		/// <br/> 
		/// <br/>     passwd -- Set the user's password on the server (and Windows client)
		/// <br/> 
		/// <br/>     p4 passwd [-O oldPassword -P newPassword] [user]
		/// <br/> 
		/// <br/> 	'p4 passwd' sets the user's password on the server.
		/// <br/> 
		/// <br/> 	After a password is set for a user, the same password must be set on
		/// <br/> 	the client in the environment variable $P4PASSWD to enable the user
		/// <br/> 	to use all Perforce client applications on that machine. (On Windows,
		/// <br/> 	you can use 'p4 passwd' to configure the password in the environment.)
		/// <br/> 
		/// <br/> 	'p4 passwd' prompts for both the old password and the new password
		/// <br/> 	with character echoing turned off.  To delete the password, set it to
		/// <br/> 	an empty string.
		/// <br/> 
		/// <br/> 	The -O flag provides the old password, avoiding prompting.
		/// <br/> 
		/// <br/> 	The -P flag provides the new password, avoiding prompting.
		/// <br/> 
		/// <br/> 	If you are using ticket-based authentication, changing your password
		/// <br/> 	automatically invalidates all of your tickets and logs you out.
		/// <br/> 
		/// <br/> 	Specifying a username as an argument to 'p4 passwd' requires 'super'
		/// <br/> 	access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public bool SetPassword(string OldPassword, string NewPassword)
		{
			return SetPassword(OldPassword, NewPassword, null);
		}

		/// <summary>
		/// Set the a user's password on the Perforce server.
		/// </summary>
		/// <param name="OldPassword">User's old password</param>
		/// <param name="NewPassword">User's new password</param>
		/// <param name="User">User receiving new password</param>
		/// <returns>Success/Failure</returns>
		/// <remarks>
		/// <br/><b>p4 help passwd</b>
		/// <br/> 
		/// <br/>     passwd -- Set the user's password on the server (and Windows client)
		/// <br/> 
		/// <br/>     p4 passwd [-O oldPassword -P newPassword] [user]
		/// <br/> 
		/// <br/> 	'p4 passwd' sets the user's password on the server.
		/// <br/> 
		/// <br/> 	After a password is set for a user, the same password must be set on
		/// <br/> 	the client in the environment variable $P4PASSWD to enable the user
		/// <br/> 	to use all Perforce client applications on that machine. (On Windows,
		/// <br/> 	you can use 'p4 passwd' to configure the password in the environment.)
		/// <br/> 
		/// <br/> 	'p4 passwd' prompts for both the old password and the new password
		/// <br/> 	with character echoing turned off.  To delete the password, set it to
		/// <br/> 	an empty string.
		/// <br/> 
		/// <br/> 	The -O flag provides the old password, avoiding prompting.
		/// <br/> 
		/// <br/> 	The -P flag provides the new password, avoiding prompting.
		/// <br/> 
		/// <br/> 	If you are using ticket-based authentication, changing your password
		/// <br/> 	automatically invalidates all of your tickets and logs you out.
		/// <br/> 
		/// <br/> 	Specifying a username as an argument to 'p4 passwd' requires 'super'
		/// <br/> 	access granted by 'p4 protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public bool SetPassword(string OldPassword, string NewPassword, string User)
		{
			P4Command passwd = null;
			if (User == null)
			{
				passwd = new P4Command(this, "passwd", true);
			}
			else
			{
				passwd = new P4Command(this, "passwd", true, User);
			}
			//Options options = new Options();
			//options["-O"] = OldPassword;
			//options["-P"] = NewPassword;
			passwd.Responses = new Dictionary<string, string>();
			passwd.Responses["Enter old password: "] = OldPassword;
			passwd.Responses["Enter new password: "] = NewPassword;
			passwd.Responses["Re-enter new password: "] = NewPassword;

			P4CommandResult results = passwd.Run();

			// login using the new password to refresh the credentials used by the connection
			Login(NewPassword);

			return results.Success;
		}

		/// <summary>
		/// The errors  (if any) of the command execution
		/// </summary>
		//public P4ClientErrorList ErrorList
		//{
		//    get
		//    {
		//        if (_p4server != null)
		//        {
		//            return _p4server.ErrorList;
		//        }
		//        return null;
		//    }
		//}

		/// <summary>
		/// The results of the last command executed
		/// </summary>
		public P4CommandResult LastResults
		{
			get
			{
				if (_p4server != null)
				{
					return _p4server.LastResults;
				}
				return null;
			}
		}

		/// <summary>
		/// Create a P4Command that can be run on the connection
		/// </summary>
		/// <param name="cmd">Command name, i.e. 'sync'</param>
		/// <param name="tagged">Flag to create tggged output</param>
		/// <param name="args">The arguments for the command</param>
		/// <returns></returns>
		public P4Command CreateCommand(string cmd, bool tagged, params string[] args)
		{
			return new P4Command(this, cmd, tagged, args);
		}

		/// <summary>
		/// Create a P4.P4MapApi object to be used on the current server connection
		/// </summary>
		/// <returns></returns>
		public P4.P4MapApi GetMapApi()
		{
			if (_p4server != null)
			{
				return new P4MapApi(_p4server);
			}
			return null;
		}

		public IKeepAlive KeepAlive
		{
			get { return _p4server.KeepAlive; }
			set { _p4server.KeepAlive = value; }
		}

		private TimeSpan _commandTimeout = TimeSpan.Zero;

		public TimeSpan CommandTimeout
		{
			get { return _p4server.RunCmdTimout; }
			set 
			{
				_commandTimeout = value;
				_p4server.RunCmdTimout = value; 
			}
		}

		public string GetP4EnvironmentVar(string var)
		{
			if (_p4server == null)
			{
				return null;
			}
			return _p4server.Get(var);
		}

		public void SetP4EnvironmentVar(string var, string val)
		{
			if (_p4server == null)
			{
				return;
			}
			_p4server.Set(var, val);
		}

		#region IDisposable Members

		public void Dispose()
		{
			//LogOut();
			Disconnect(null);
		}

		#endregion
	}
}
