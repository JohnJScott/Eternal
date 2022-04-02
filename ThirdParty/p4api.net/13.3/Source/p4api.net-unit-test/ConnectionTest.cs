using Perforce.P4;
using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

using System.Diagnostics;


namespace p4api.net.unit.test
{
	
	
	/// <summary>
	///This is a test class for ConnectionTest and is intended
	///to contain all ConnectionTest Unit Tests
	///</summary>
	[TestClass()]
	public class ConnectionTest
	{
		String TestDir = "c:\\MyTestDir";

		private TestContext testContextInstance;

		/// <summary>
		///Gets or sets the test context which provides
		///information about and functionality for the current test run.
		///</summary>
		public TestContext TestContext
		{
			get
			{
				return testContextInstance;
			}
			set
			{
				testContextInstance = value;
			}
		}

		#region Additional test attributes
		// 
		//You can use the following additional attributes as you write your tests:
		//
		//Use ClassInitialize to run code before running the first test in the class
		//[ClassInitialize()]
		//public static void MyClassInitialize(TestContext testContext)
		//{
		//}
		//
		//Use ClassCleanup to run code after all tests in a class have run
		//[ClassCleanup()]
		//public static void MyClassCleanup()
		//{
		//}
		//
		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}
		//
		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}
		//
		#endregion


		/// <summary>
		///A test for Connect
		///</summary>
		[TestMethod()]
		public void ConnectTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{

						target.UserName = user;
						target.Client = new Client();
						target.Client.Name = ws_client;

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.IsTrue(target.Connect(null));

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

        /// <summary>
        ///A test for Connect and check server version
        ///</summary>
        [TestMethod()]
        public void ConnectTestCheckServerVersion()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
                Server server = new Server(new ServerAddress(uri));
                try
                {
                    using (Connection target = new Connection(server))
                    {
                        target.Connect(null);
                        if (target._p4server.ApiLevel==28)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2009.2");
                        }
                        target.Connect(null);
                        if (target._p4server.ApiLevel == 29)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2010.1");
                        }
                        target.Connect(null);
                        if (target._p4server.ApiLevel == 30)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2010.2");
                        }
                        target.Connect(null);
                        if (target._p4server.ApiLevel == 31)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2011.1");
                        }
                        target.Connect(null);
                        if (target._p4server.ApiLevel == 32)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2011.2");
                        }
                        target.Connect(null);
                        if (target._p4server.ApiLevel == 33)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2012.1");
                        }
                        target.Connect(null);
                        if (target._p4server.ApiLevel == 34)
                        {
                            Assert.AreEqual(target.Server.Metadata.Version.Major,
                            "2012.2");
                        }
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                unicode = !unicode;
            }
        }

		/// <summary>
		///A test for Connect
		///</summary>
		[TestMethod()]
		public void ContinualConnectTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			Random rdm = new Random();

			int cointoss = rdm.Next(0, 1);
			if (cointoss != 0)
			{
				unicode = true;
			}
			for (int i = 0; i < 1; i++) // run only once for ascii or unicode (randomly), it's a long test
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					DateTime start = DateTime.Now;
					while (true)
					{
						using (Connection target = new Connection(server))
						{
							string[] args = new string[] { "-m", "1", "//depot/*." };

							uint cmdID = 7;
							using (P4Server _P4Server = new P4Server("localhost:6666", null, null, null))
							{
								string val = _P4Server.Get("P4IGNORE");
								int _p4IgnoreSet = string.IsNullOrEmpty(val) ? 0 : 1;

								target.UserName = user;
								target.Client = new Client();
								target.Client.Name = ws_client;

								Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

								Assert.IsTrue(target.Connect(null));

								Assert.AreEqual(target.Status, ConnectionStatus.Connected);

								Assert.IsTrue(target._p4server.RunCommand("fstat", cmdID, false, args, args.Length));
							}

							target._p4server.ReleaseConnection(cmdID);

							Assert.IsTrue(target._p4server.RunCommand("fstat", ++cmdID, false, args, args.Length));

							target._p4server.ReleaseConnection(cmdID);

							Assert.IsTrue(target.ApiLevel > 0);

							int delay = rdm.Next(0, 11);

							if (delay > 0)
							{
								System.Threading.Thread.Sleep(TimeSpan.FromSeconds(delay));
							}
						}

						if ((DateTime.Now - start) > TimeSpan.FromSeconds(158))
							break;
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for Connect
		///</summary>
		[TestMethod()]
		public void ConnectAndRunCommandsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{

						target.UserName = user;
						target.Client = new Client();
						target.Client.Name = ws_client;

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.IsTrue(target.Connect(null));

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						string[] args = new string[] {"-m", "1", "//depot/*."};

						uint cmdID = 7; 
						Assert.IsTrue(target._p4server.RunCommand("fstat", cmdID, false, args, args.Length));

						target._p4server.ReleaseConnection(cmdID);

						Assert.IsTrue(target._p4server.RunCommand("fstat", ++cmdID, false, args, args.Length));

						target._p4server.ReleaseConnection(cmdID);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}


#if _TEST_P4AUTH
		/// <summary>
		///A test for Connect using a bad auth server
		///</summary>
		[TestMethod()]
		public void ConnectWithBadP4AuthTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 3; i++) // run once for ascii, once for unicode
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
				}
				if (i == 2)
				{
					zippedFile = "s3.exe";
					pass = "Password";
				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile, "P4AuthTest.bat");
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{
						target.UserName = user;
						target.Client = new Client();
						target.Client.Name = ws_client;

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						try
						{
							Assert.IsFalse(target.Connect(null));
						}
						catch (Exception ex)
						{
							Assert.IsTrue(ex is P4Exception);
						}
						Assert.AreNotEqual(target.Status, ConnectionStatus.Connected);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
#endif

		/// <summary>
		///A test for Connect
		///</summary>
		[TestMethod()]
		public void ConnectBadTest()
		{
			bool unicode = false;

			string uri = "locadhost:77777";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{
						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						try
						{
							Assert.IsFalse(target.Connect(null));
						}
						catch (AssertFailedException) 
						{ 
							throw; 
						}
						catch (P4Exception ex)
						{
							Trace.WriteLine(string.Format("ConnectBadTest throw an exception: {0}", ex.Message));
							Trace.WriteLine(string.Format("Stacktrace:\r\n{0}", ex.StackTrace));
						}
						catch (Exception ex)
						{
							Trace.WriteLine(string.Format("ConnectBadTest throw an exception: {0}", ex.Message));
							Trace.WriteLine(string.Format("Stacktrace:\r\n{0}", ex.StackTrace));
						}
						
						Assert.AreEqual(target.Server.State, ServerState.Offline);

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for Disconnect
		///</summary>
		[TestMethod()]
		public void DisconnectTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";
			

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{
						target.UserName = user;
						target.Client = new Client();
						target.Client.Name = ws_client;

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						Assert.IsTrue(target.Connect(null));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						Assert.IsTrue(target.Disconnect(null));

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.IsFalse(target.Disconnect(null));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for Client
		///</summary>
		[TestMethod()]
		public void ClientTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						P4Command syncCmd = new P4Command(con._p4server, "sync", false);
						P4CommandResult r = syncCmd.Run();
						Assert.AreEqual(r.ErrorList[0].ErrorMessage, "File(s) up-to-date.\n");
					}
					//rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						ws_client = "ws_bad_client";
						con.Client.Name = ws_client;	
					
						bool failed = false;

						Assert.IsTrue(con.Connect(null));
						
						try
						{
							P4Command syncCmd = new P4Command(con._p4server, "sync", false);
							P4CommandResult r = syncCmd.Run();
						}
						catch
						{
							failed = true;
						}

						Assert.IsTrue(failed);

						ws_client = "admin_space";

					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for Login
		///</summary>
		[TestMethod()]
		public void LoginTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = "pass";
			string ws_client = "admin_space";

			for (int i = 0; i < 3; i++) // run once for ascii, once for unicode, once for the security level 3 server
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
				}
				if (i == 2)
				{
					zippedFile = "s3.exe";
					pass = "Password";
				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						target.UserName = user;
						Options options = new Options();
						options["Password"] = pass;

						Assert.IsTrue(target.Connect(options));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						Credential cred = target.Login(pass, null, null);
						Assert.IsNotNull(cred);

						Assert.AreEqual(user, cred.UserName);

						Assert.IsTrue(target.Logout(null));

						Assert.IsTrue(target.Disconnect(null));

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.IsFalse(target.Disconnect(null));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
		/// <summary>
		///Another test for Login
		///</summary>
		[TestMethod()]
		public void LoginTest2()
		{
			bool unicode = false;

			string uri = "localhost:6666";   
			string user = "admin";
			string pass = "pass";
			string ws_client = "alex_space";

			string user2 = "Alex";
			
			for (int i = 0; i < 3; i++) // run once for ascii, once for unicode, once for the security level 3 server
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
					user2 = "Алексей";
				}
				if (i == 2)
				{
					zippedFile = "s3.exe";
					user2 = "Alex";
					pass = "Password";

				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						target.UserName = user;
						Options options = new Options();
						options["Password"] = pass;

						Assert.IsTrue(target.Connect(options));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						// login as admin
						Credential cred = target.Login(pass, null, null);
						Assert.IsNotNull(cred);

						Assert.AreEqual(user, cred.UserName);

						target.Logout(null);

						target.UserName = user2;
						options = new Options();
						options["Password"] = pass;

						Assert.IsTrue(target.Connect(options));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						// login as alex/alexei
						Credential cred2 = target.Login(pass, null, null);
						Assert.IsNotNull(cred2);

						Assert.AreEqual(user2, cred2.UserName);

						if (zippedFile != "s3.exe")
						{ Assert.IsTrue(target.Logout(null)); }

						Assert.IsTrue(target.Disconnect(null));

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.IsFalse(target.Disconnect(null));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
		/// <summary>
		///Another test for Login
		///</summary>
		[TestMethod()]
		public void TrustTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			//			string user = "admin";
			//			string pass = "pass";
			//			string ws_client = "alex_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{
						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						//						target.UserName = user;

						Assert.IsTrue(target.Connect(null));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						TrustCmdOptions options = new TrustCmdOptions(TrustCmdFlags.AutoAccept);
						Assert.IsTrue(target.Trust(options, null));

						Assert.IsTrue(target.Disconnect(null));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
		/// <summary>
		///Another test for Login
		///</summary>
		[TestMethod()]
		public void CharacterSetNameTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			//			string user = "admin";
			//			string pass = "pass";
			//			string ws_client = "alex_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{
						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						//						target.UserName = user;

						Assert.IsTrue(target.Connect(null));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						string actual = target.CharacterSetName;

                        string p4charset = target.GetP4EnvironmentVar("P4CHARSET");
						if ((p4charset != null) && (p4charset != "none"))
                        {
                            Assert.AreEqual(p4charset, actual);
                        }
                        else if (unicode)
						{
							// should have been automatically detected if the server is 
							// unicode based on this systems codepage
							Assert.IsFalse(string.IsNullOrEmpty(actual) || (actual == "none"));
						}
						else
						{
							// no charset needed on on non unicode servers
							Assert.IsTrue(string.IsNullOrEmpty(actual) || (actual == "none"));
						}
						
						Assert.IsTrue(target.Disconnect(null));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
		/// <summary>
		///A test for SetPassword
		///</summary>
		[TestMethod()]
		public void SetPasswordTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = "pass";
			string ws_client = "admin_space";

			for (int i = 0; i < 3; i++) // run once for ascii, once for unicode, once for the security level 3 server
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
				}
				if (i == 2)
				{
					zippedFile = "s3.exe";
					pass = "Password";
				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					using (Connection target = new Connection(server))
					{

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(target.Server.State, ServerState.Unknown);

						target.UserName = user;
						Options options = new Options();
						options["Password"] = pass;

						Assert.IsTrue(target.Connect(options));

						Assert.AreEqual(target.Server.State, ServerState.Online);

						Assert.AreEqual(target.Status, ConnectionStatus.Connected);

						Credential cred = target.Login(pass, null, null);
						Assert.IsNotNull(cred);

						Assert.AreEqual(user, cred.UserName);

						Assert.IsTrue(target.SetPassword(pass, pass + "2"));

						Assert.IsTrue(target.Logout(null));

						Assert.IsTrue(target.Disconnect(null));

						Assert.AreEqual(target.Status, ConnectionStatus.Disconnected);

						Assert.IsFalse(target.Disconnect(null));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
	}
}
