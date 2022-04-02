using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
//using Perforce.P4;
using System.Collections.Generic;

using System.Diagnostics;

namespace p4api.net.unit.test
{
	
	
	/// <summary>
	///This is a test class for ClientTest and is intended
	///to contain all ClientTest Unit Tests
	///</summary>
	[TestClass()]
	public class ClientTest
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
		///A test for Client Constructor
		///</summary>
		[TestMethod()]
		public void ClientConstructorTest()
		{
			Client target = new Client();
			Assert.IsNotNull(target);
		}

		/// <summary>
		///A test for FormatDateTime
		///</summary>
		[TestMethod()]
		public void FormatDateTimeTest()
		{
			DateTime dt = new DateTime(2011,2,3,4,5,6); // 2/3/2011 4:05:06
			string expected = "2011/02/03 04:05:06";
			string actual;
			actual = Client.FormatDateTime(dt);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for FromTaggedOutput
		///</summary>
		[TestMethod()]
		public void FromTaggedOutputTest()
		{
			Client target = new Client(); // TODO: Initialize to an appropriate value
			Perforce.P4.TaggedObject workspaceInfo = new Perforce.P4.TaggedObject();

			workspaceInfo["Client"] = "clientName";

			workspaceInfo["Update"] = "2010/01/02 03:04:05"; // DateTime(2010, 1, 2, 3, 4, 5);

			workspaceInfo["Access"] = "2011/02/03 04:05:06"; // DateTime(2011, 2, 3, 4, 5, 6);

			workspaceInfo["Owner"] = "JoeOwner";

			workspaceInfo["Options"] = "allwrite noclobber compress unlocked modtime normdir"; //new ClientOptionEnum(ClientOption.AllWrite | ClientOption.Compress | ClientOption.ModTime);

			workspaceInfo["SubmitOptions"] = "revertunchanged+reopen";//new ClientSubmitOptions(true, SubmitType.RevertUnchanged);

			workspaceInfo["LineEnd"] = "LOCAL"; // LineEnd.Local;

			workspaceInfo["Root"] = "C:\\clientname";

			workspaceInfo["Host"] = "MissManners";

			workspaceInfo["Description"] = "Miss Manners client";

			workspaceInfo["AltRoots0"] = "C:\\alt0";
			workspaceInfo["AltRoots1"] = "C:\\alt1";

			workspaceInfo["Stream"] = "//Rocket/dev1";

			workspaceInfo["View0"] = "	//depot/main/p4/... //dbarbee_win-dbarbee/main/p4/...";
			// new MapEntry(MapType.Include,
			//		new PathSpec(PathType.DEPOT_PATH, null, "//depot/main/p4/..."),
			//		new PathSpec(PathType.CLIENT_PATH, null, "//dbarbee_win-dbarbee/main/p4/..."));
			workspaceInfo["View1"] = "-//usr/... //dbarbee_win-dbarbee/usr/...";
			//new MapEntry(MapType.Exclude,
			//		new PathSpec(PathType.DEPOT_PATH, null, "//usr/..."),
			//		new PathSpec(PathType.CLIENT_PATH, null, "//dbarbee_win-dbarbee/usr/..."));
			workspaceInfo["View2"] = "+//spec/... //dbarbee_win-dbarbee/spec/...";
			//new MapEntry(MapType.Overlay,
			//		new PathSpec(PathType.DEPOT_PATH, null, "//spec/..."),
			//		new PathSpec(PathType.CLIENT_PATH, null, "//dbarbee_win-dbarbee/spec/..."));

			target.FromClientCmdTaggedOutput(workspaceInfo);

			Assert.AreEqual("clientName", target.Name);

			Assert.AreEqual(new DateTime(2010, 1, 2, 3, 4, 5), target.Updated);

			Assert.AreEqual(new DateTime(2011, 2, 3, 4, 5, 6), target.Accessed);

			Assert.AreEqual("JoeOwner", target.OwnerName);

			Assert.AreEqual((ClientOption.AllWrite | ClientOption.Compress | ClientOption.ModTime),
				target.Options);

			Assert.AreEqual(new ClientSubmitOptions(true, SubmitType.RevertUnchanged), target.SubmitOptions);

			Assert.AreEqual(LineEnd.Local, target.LineEnd);

			Assert.AreEqual("C:\\clientname", target.Root);

			Assert.AreEqual("MissManners", target.Host);

			Assert.AreEqual("Miss Manners client", target.Description);

			Assert.AreEqual("C:\\alt0", target.AltRoots[0]);
			Assert.AreEqual("C:\\alt1", target.AltRoots[1]);

			Assert.AreEqual("//Rocket/dev1", target.Stream);

			Assert.AreEqual(new MapEntry(MapType.Include,
					new DepotPath("//depot/main/p4/..."),
					new ClientPath("//dbarbee_win-dbarbee/main/p4/...")),
					target.ViewMap[0]);
			Assert.AreEqual(new MapEntry(MapType.Exclude,
					new DepotPath("//usr/..."),
					new ClientPath("//dbarbee_win-dbarbee/usr/...")),
					target.ViewMap[1]);
			Assert.AreEqual(new MapEntry(MapType.Overlay,
					new DepotPath("//spec/..."),
					new ClientPath("//dbarbee_win-dbarbee/spec/...")),
					target.ViewMap[2]);
		}

		/// <summary>
		///A test for Parse
		///</summary>
		[TestMethod()]
		public void ParseTest()
		{
			Client target = new Client(); // TODO: Initialize to an appropriate value
			string spec = "Client:\tclientName\r\n\r\nUpdate:\t2010/01/02 03:04:05\r\n\r\nAccess:\t2011/02/03 04:05:06\r\n\r\nOwner:\tJoeOwner\r\n\r\nHost:\tMissManners\r\n\r\nDescription:\r\n\tMiss Manners client\r\n\r\nRoot:\tC:\\clientname\r\n\r\nAltRoots:\r\n\tC:\\alt0\r\n\tC:\\alt1\r\n\r\nOptions:\tallwrite noclobber compress unlocked modtime normdir\r\n\r\nSubmitOptions:\trevertunchanged+reopen\r\n\r\nLineEnd:\tLocal\r\n\r\nView:\r\n\t//depot/main/p4/... //dbarbee_win-dbarbee/main/p4/...\r\n\t-//usr/... //dbarbee_win-dbarbee/usr/...\r\n\t+//spec/... //dbarbee_win-dbarbee/spec/...\r\n";
			bool expected = true; 
			bool actual;
			actual = target.Parse(spec);
			Assert.AreEqual(expected, actual);

			Assert.AreEqual("clientName", target.Name);

			Assert.AreEqual(new DateTime(2010, 1, 2, 3, 4, 5), target.Updated);

			Assert.AreEqual(new DateTime(2011, 2, 3, 4, 5, 6), target.Accessed);

			Assert.AreEqual("JoeOwner", target.OwnerName);

			Assert.AreEqual((ClientOption.AllWrite | ClientOption.Compress | ClientOption.ModTime),
				target.Options);

			Assert.AreEqual(new ClientSubmitOptions(true, SubmitType.RevertUnchanged), target.SubmitOptions);

			Assert.AreEqual(LineEnd.Local, target.LineEnd);

			Assert.AreEqual("C:\\clientname", target.Root);

			Assert.AreEqual("MissManners", target.Host);

			Assert.AreEqual("Miss Manners client", target.Description);

			Assert.AreEqual("C:\\alt0", target.AltRoots[0]);
			Assert.AreEqual("C:\\alt1", target.AltRoots[1]);

			Assert.AreEqual(new MapEntry(MapType.Include,
					new DepotPath("//depot/main/p4/..."),
					new ClientPath("//dbarbee_win-dbarbee/main/p4/...")),
					target.ViewMap[0]);
			Assert.AreEqual(new MapEntry(MapType.Exclude,
					new DepotPath("//usr/..."),
					new ClientPath("//dbarbee_win-dbarbee/usr/...")),
					target.ViewMap[1]);
			Assert.AreEqual(new MapEntry(MapType.Overlay,
					new DepotPath("//spec/..."),
					new ClientPath("//dbarbee_win-dbarbee/spec/...")),
					target.ViewMap[2]);
		}

		/// <summary>
		///A test for ToString
		///</summary>
		[TestMethod()]
		public void ToStringTest()
		{
			Client target = new Client();

			target.Name = "clientName";

			target.Updated = new DateTime(2010, 1, 2, 3, 4, 5);

			target.Accessed =  new DateTime(2011, 2, 3, 4, 5, 6);

			target.OwnerName = "JoeOwner";

			target.Options = (ClientOption.AllWrite | ClientOption.Compress | ClientOption.ModTime);

			target.SubmitOptions = new ClientSubmitOptions(true, SubmitType.RevertUnchanged);

			target.LineEnd =  LineEnd.Local;

			target.Root = "C:\\clientname";

			target.Host = "MissManners";

			target.Description = "Miss Manners client";

			target.AltRoots = new List<string>();
			target.AltRoots.Add("C:\\alt0");
			target.AltRoots.Add("C:\\alt1");

			target.ServerID = "perforce:1666";
			target.Stream = "//Stream/main";
			target.StreamAtChange = "111";

			target.ViewMap = new ViewMap( new string[] {
				"	//depot/main/p4/... //dbarbee_win-dbarbee/main/p4/...",
				"-//usr/... //dbarbee_win-dbarbee/usr/...",
				"+//spec/... //dbarbee_win-dbarbee/spec/..."});

			string expected = "Client:\tclientName\r\n\r\nUpdate:\t2010/01/02 03:04:05\r\n\r\nAccess:\t2011/02/03 04:05:06\r\n\r\nOwner:\tJoeOwner\r\n\r\nHost:\tMissManners\r\n\r\nDescription:\r\n\tMiss Manners client\r\n\r\nRoot:\tC:\\clientname\r\n\r\nAltRoots:\r\n\tC:\\alt0\r\n\tC:\\alt1\r\n\r\nOptions:\tallwrite noclobber compress unlocked modtime normdir\r\n\r\nSubmitOptions:\trevertunchanged+reopen\r\n\r\nLineEnd:\tLocal\r\n\r\nStream:\t//Stream/main\r\n\r\nStreamAtChange:\t111\r\n\r\nServerID:\tperforce:1666\r\n\r\nView:\r\n\t//depot/main/p4/... //dbarbee_win-dbarbee/main/p4/...\r\n\t-//usr/... //dbarbee_win-dbarbee/usr/...\r\n\t+//spec/... //dbarbee_win-dbarbee/spec/...\r\n";
			string actual;
			actual = target.ToString();
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for AltRootsStr
		///</summary>
		[TestMethod()]
		public void AltRootsStrTest()
		{
			Client target = new Client();
			List<string> expected = new List<string>();
			string root = @"C:\depot";
			expected.Add(root);
			target.AltRoots = expected;
			IList<string> actual;
			actual = target.AltRoots;
			Assert.IsTrue(actual.Contains(@"C:\depot"));
		}

		/// <summary>
		///A test for LineEnd
		///</summary>
		[TestMethod()]
		public void LineEndTest()
		{
			Client target = new Client();
			LineEnd expected = LineEnd.Mac;
			LineEnd actual;
			target.LineEnd = expected;
			actual = target.LineEnd;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Options
		///</summary>
		[TestMethod()]
		public void OptionsTest()
		{
			Client target = new Client();
			ClientOption expected = ClientOption.Clobber;
			ClientOption actual;
			target.Options = expected;
			actual = target.Options;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for initialize
		///</summary>
		[TestMethod()]
		public void initializeTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);
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
		///A test for addFiles
		///</summary>
		[TestMethod()]
		public void addFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						System.IO.File.Copy("c:\\MyTestDir\\admin_space\\MyCode\\NewFile.txt", "c:\\MyTestDir\\admin_space\\MyCode\\NewFile2.txt");
						FileSpec toFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\NewFile2.txt"), null);
						Options options = new Options(AddFilesCmdFlags.None, -1, null);
						List<FileSpec> newfiles = con.Client.AddFiles(options, toFile);

						Assert.AreEqual(1, newfiles.Count);

						foreach (var fileSpec in newfiles)
						{
							Assert.IsNotNull(fileSpec.DepotPath.Path);
							Assert.IsNotNull(fileSpec.ClientPath.Path);
							Assert.IsNotNull(fileSpec.LocalPath.Path);
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
		///A test for DeleteFiles
		///</summary>
		[TestMethod()]
		public void DeleteFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec toFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe.txt"), null);
						Options options = new Options(DeleteFilesCmdFlags.None, -1);
						List<FileSpec> oldfiles = con.Client.DeleteFiles(options, toFile);

						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for EditFiles
		///</summary>
		[TestMethod()]
		public void EditFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec toFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe.txt"), null);
						Options options = new Options(EditFilesCmdFlags.None, -1, null);
						List<FileSpec> oldfiles = con.Client.EditFiles(options, toFile);

						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for GetSyncedFiles
		///</summary>
		[TestMethod()]
		public void GetSyncedFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec toFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe.txt"), null);
						Options options = null; 
						List<FileSpec> oldfiles = con.Client.GetSyncedFiles(options, toFile);

						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for IntegrateFiles for the 
		///"p4 integrate [options] fromFile[revRange] toFile"
		///version of integrate
		///</summary>
		[TestMethod()]
		public void IntegrateFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe.txt"), null);
						FileSpec toFile = 
							new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\branchAlpha\\ReadMe.txt"), null);
						Options options = new Options(IntegrateFilesCmdFlags.None,
																					-1,
																					10,
																					null,
																					null,
																					null);
						List<FileSpec> oldfiles = con.Client.IntegrateFiles(fromFile, options, toFile );

						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for IntegrateFiles for the 
		///"p4 integrate [options] -b branch [-r] [toFile[revRange] ...]"
		///version of integrate
		///</summary>
		[TestMethod()]
		public void IntegrateFilesTest1()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);
						string branch = "MyCode->MyCode2";
						List<FileSpec> toFiles = new List<FileSpec>();
						FileSpec toFile = new FileSpec(new DepotPath("//depot/MyCode2/Silly.bmp"), null);
						toFiles.Add(toFile);
						Options options = new Options(IntegrateFilesCmdFlags.Force,
																					-1,
																					10,
																					branch,
																					null,
																					null);
						List<FileSpec> oldfiles = con.Client.IntegrateFiles( toFiles, options);

						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for LabelSync
		///</summary>
		[TestMethod()]
		public void LabelSyncTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 4, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						List<FileSpec> oldfiles = con.Client.LabelSync(null, "admin_label", (FileSpec) null);

						Assert.AreEqual(null, oldfiles);
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
		///A test for LockFiles
		///</summary>
		[TestMethod()]
		public void LockFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 5, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						List<FileSpec> oldfiles = con.Client.LockFiles(null);

						Assert.AreNotEqual(null, oldfiles);

						oldfiles = con.Client.LockFiles(null, (FileSpec) null);

						oldfiles = con.Client.LockFiles(null, (FileSpec)null, (FileSpec)null, (FileSpec)null);

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
		///A test for MoveFiles
		///</summary>
		[TestMethod()]
		public void MoveFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe.txt"), null);

						List<FileSpec> oldfiles = con.Client.EditFiles(null, fromFile);
						Assert.AreEqual(1, oldfiles.Count);

						FileSpec toFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe42.txt"), null);
						Options options = new Options(MoveFileCmdFlags.Preview, -1, null);
						oldfiles = con.Client.MoveFiles(fromFile, toFile, null);

						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for ReopenFiles
		///</summary>
		[TestMethod()]
		public void ReopenFilesTest()
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode\\ReadMe.txt"), null);

						List<FileSpec> oldfiles = con.Client.EditFiles(null, fromFile);
						Assert.AreEqual(1, oldfiles.Count);

						FileType ft = new FileType(BaseFileType.Unicode, FileTypeModifier.ExclusiveOpen);

						Options ops = new Options(-1, ft);
						oldfiles = con.Client.ReopenFiles(ops, fromFile);
						Assert.AreEqual(1, oldfiles.Count);
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
		///A test for ResolveFiles
		///</summary>
		[TestMethod()]
		public void ResolveFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						//
						// NEEDS WORK!
						//

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						List<FileSpec> oldfiles = null;

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Check It In!",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile);
						}
						catch { } // will fail because we need to resolve

						Options rFlags = new Options(
							ResolveFilesCmdFlags.PreviewOnly, -1);
						IList<FileResolveRecord> records = con.Client.ResolveFiles(rFlags, fromFile);
						Assert.IsNotNull(records);

						rFlags = new Options(ResolveFilesCmdFlags.AutomaticForceMergeMode, -1);
						records = con.Client.ResolveFiles(rFlags, fromFile);
						Assert.IsNotNull(records);
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
		///A test for ResolveFiles
		///</summary>
		[TestMethod()]
		public void ResolveFilesTest1()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						//
						// NEEDS WORK!
						//

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						List<FileSpec> oldfiles = null;

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Check It In!",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile);
						}
						catch { } // will fail because we need to resolve


						Dictionary<String, String> responses = new Dictionary<string, string>();
						responses["DefaultResponse"] = "s";
						responses["Accept(a) Edit(e) Diff(d) Merge (m) Skip(s) Help(?) am: "] = "am";

						Options rFlags = new Options(ResolveFilesCmdFlags.IgnoreWhitespace, -1);
						IList<FileResolveRecord> records = con.Client.ResolveFiles(null, null, responses, rFlags, fromFile);
						Assert.IsNotNull(records);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
		private String HandlePrompt(uint cmdId, String msg, bool displayText)
		{
			if (msg == "Accept(a) Edit(e) Diff(d) Merge (m) Skip(s) Help(?) am: ")
				return "am";
			return "s";
		}

		/// <summary>
		///A test for ResolveFiles
		///</summary>
		[TestMethod()]
		public void ResolveFilesTest2()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						//
						// NEEDS WORK!
						//

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Check It In!",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile);
						}
						catch { } // will fail because we need to resolve

						Perforce.P4.P4Server.PromptHandlerDelegate promptHandler =
							new Perforce.P4.P4Server.PromptHandlerDelegate(HandlePrompt);

						Options rFlags = new Options(ResolveFilesCmdFlags.IgnoreWhitespace, -1);
						IList<FileResolveRecord> records = con.Client.ResolveFiles(null, promptHandler, null, rFlags, fromFile);
						Assert.IsNotNull(records);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		private Connection resolveCon = null;

		private P4ClientMerge.MergeStatus HandleResolve(uint cmdId, P4ClientMerge merger)
		{
			if (resolveCon != null)
			{
				TaggedObjectList taggedOut = resolveCon._p4server.GetTaggedOutput(cmdId);
				string[] infoOut = resolveCon._p4server.GetInfoResults(cmdId);
			}
			if (merger.AutoResolve(P4ClientMerge.MergeForce.CMF_AUTO) == P4ClientMerge.MergeStatus.CMS_MERGED)
			{
				return P4ClientMerge.MergeStatus.CMS_MERGED;
			}
			return P4ClientMerge.MergeStatus.CMS_SKIP;
		}

		/// <summary>
		///A test for ResolveFiles
		///</summary>
		[TestMethod()]
		public void ResolveFilesTest3()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						//
						// NEEDS WORK!
						//

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Check It In!",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile);
						}
						catch { } // will fail because we need to resolve

						Perforce.P4.P4Server.ResolveHandlerDelegate resolveHandler =
							new Perforce.P4.P4Server.ResolveHandlerDelegate(HandleResolve);

						Options rFlags = new Options(ResolveFilesCmdFlags.IgnoreWhitespace, -1);
						resolveCon = con;
						IList<FileResolveRecord> records = con.Client.ResolveFiles(resolveHandler,null, null, rFlags, fromFile);
						resolveCon = null; 
						Assert.IsNotNull(records);
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
		///A test for ResolveFiles
		///</summary>
		[TestMethod()]
		public void ResolveFilesTest4()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						//
						// NEEDS WORK!
						//

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile1 = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						//FileSpec fromFile2 = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt"), null);
						//FileSpec fromFile3 = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\WingDings.txt"), null);

						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Check It In!",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile1);
						}
						catch { } // will fail because we need to resolve
						//try
						//{
						//    sr = con.Client.SubmitFiles(sFlags, fromFile2);
						//}
						//catch { } // will fail because we need to resolve
						//try
						//{
						//    sr = con.Client.SubmitFiles(sFlags, fromFile3);
						//}
						//catch { } // will fail because we need to resolve

						Options rFlags = new Options(ResolveFilesCmdFlags.AutomaticMergeMode, -1);
						resolveCon = con;
						IList<FileResolveRecord> records = con.Client.ResolveFiles(null, rFlags, fromFile1); //, fromFile2, fromFile3);
						resolveCon = null;
						Assert.IsNotNull(records);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		private bool itWorked = true;

		private P4ClientMerge.MergeStatus ResolveHandler6(FileResolveRecord resolveRecord,
			Client.AutoResolveDelegate AutoResolve, string sourcePath, string targetPath, string basePath, string resultsPath)
		{
			itWorked = true;

			if (sourcePath != null)
				itWorked &= System.IO.File.Exists(sourcePath);
			if (targetPath != null)
				itWorked &= System.IO.File.Exists(targetPath);
			if (basePath != null)
				itWorked &= System.IO.File.Exists(basePath);
			if (resultsPath != null)
				itWorked &= System.IO.File.Exists(resultsPath);

			return P4ClientMerge.MergeStatus.CMS_SKIP;
		}

		/// <summary>
		///A test for ResolveFiles
		///</summary>
		[TestMethod()]
		public void ResolveFilesTest6()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 12, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						//
						// NEEDS WORK!
						//

						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile1 = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode2\\BranchResolve.txt"), null);
						FileSpec fromFile2 = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\MyCode2\\DeleteResolve2.txt"), null);
						FileSpec fromFile3 = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);

						Options rFlags = new Options(ResolveFilesCmdFlags.DisplayBaseFile, -1);
						resolveCon = con;

						Client.ResolveFileDelegate resolver = new Client.ResolveFileDelegate(ResolveHandler6);

						IList<FileResolveRecord> records = con.Client.ResolveFiles(resolver, rFlags, fromFile1, fromFile2, fromFile3);
						resolveCon = null;
						Assert.IsNotNull(records);
						Assert.IsTrue(itWorked);
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
		///A test for SubmitFiles
		///</summary>
		[TestMethod()]
		public void SubmitFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 3, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = null;// new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Submit the default changelist",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile);
						}
						catch { } // will fail because we need to resolve

						Assert.IsNotNull(sr);
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
		///A test for SubmitFiles
		///</summary>
		[TestMethod()]
		public void SubmitFilesTest1()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 3, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						Changelist change = new Changelist();
						change.Description = "On the fly built change list";
						FileMetaData file = new FileMetaData();
						file.DepotPath = new DepotPath("//depot/TestData/Letters.txt");
						change.Files.Add(file);

						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							change,
							null,
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, null);
						}
						catch { } // will fail because we need to resolve

						Assert.IsNotNull(sr);
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
		///A test for SubmitFiles
		///</summary>
		[TestMethod()]
		public void SubmitFilesTest2()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 3, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							5,
							null,
							null,
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, null);
						}
						catch { } // will fail because we need to resolve

						Assert.IsNotNull(sr);
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
		///A test for SubmitFiles
		///</summary>
		[TestMethod()]
		public void SubmitShelvedFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

                        Options sFlags = new Options(
                            ShelveFilesCmdFlags.None,
                            null,
                            5
                        );

                        IList<FileSpec> rFiles = con.Client.ShelveFiles(sFlags);
					    rFiles[0].Version = null;
					    sFlags = new Options(
					        RevertFilesCmdFlags.None, 5);

                        rFiles = con.Client.RevertFiles(rFiles,sFlags);


						sFlags = new Options(
							SubmitFilesCmdFlags.SubmitShelved,
							5,
							null,
							null,
							null
						);
						SubmitResults sr = null;
						try
						{
							//FileSpec fs = FileSpec.LocalSpec("c:\\MyTestDir\\admin_space\\TestData\\Letters.txt");
							sr = con.Client.SubmitFiles(sFlags, null);
						}
						catch { } // will fail because we need to resolve

						Assert.IsNotNull(sr);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

        public void SubmitFilesTest3()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, 3, unicode);
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

                        Assert.AreEqual("admin", con.Client.OwnerName);

                        Options sFlags = new Options(
                            SubmitFilesCmdFlags.None,
                            -1,
                            null,
                            "Test submit",
                            null
                        );
                        SubmitResults sr = null;
                        try
                        {
                            FileSpec fs = FileSpec.LocalSpec("c:\\MyTestDir\\admin_space\\TestData\\Letters.txt");
                            sr = con.Client.SubmitFiles(sFlags, fs);
                        }
                        catch { } // will fail because we need to resolve

                        Assert.IsNotNull(sr);
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
		///A test for GetResolveFiles
		///</summary>
		[TestMethod()]
		public void GetResolvedFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						List<FileSpec> oldfiles = null;

						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							SubmitFilesCmdFlags.None,
							-1,
							null,
							"Check It In!",
							null
						);
						SubmitResults sr = null;
						try
						{
							sr = con.Client.SubmitFiles(sFlags, fromFile);
						}
						catch { } // will fail because we need to resolve

						Options rFlags = new Options(
							ResolveFilesCmdFlags.AutomaticForceMergeMode | ResolveFilesCmdFlags.PreviewOnly, -1);
						IList<FileResolveRecord> records = con.Client.ResolveFiles(rFlags, fromFile);
						Assert.IsNotNull(records);

						rFlags = new Options(
							ResolveFilesCmdFlags.AutomaticForceMergeMode, -1);
						records = con.Client.ResolveFiles(rFlags, fromFile);
						Assert.IsNotNull(records);

						Options opts = new Options(GetResolvedFilesCmdFlags.IncludeBaseRevision);
						IList<FileResolveRecord> rFiles = con.Client.GetResolvedFiles(opts, null);

						Assert.IsNotNull(rFiles);
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
		///A test for RevertFiles
		///</summary>
		[TestMethod()]
		public void RevertFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

                        // test revert against all .txt files in a directory with no changelist specified
						FileSpec fromFile = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*.txt"), null);
						Options sFlags = new Options(
							RevertFilesCmdFlags.Preview,
							-1
						);
						IList<FileSpec> rFiles = con.Client.RevertFiles(sFlags, fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(3, rFiles.Count);

                        // test revert against all files in changelist 5 (1 marked for add)
                        fromFile = new FileSpec(new DepotPath("//..."), null);
                        sFlags = new Options(
                            RevertFilesCmdFlags.Preview,
                            5);
                        rFiles = con.Client.RevertFiles(sFlags, fromFile);

                        Assert.IsNotNull(rFiles);
                        Assert.AreEqual(1, rFiles.Count);

                        // test revert against all files in the default changelist (3 in total)
                        fromFile = new FileSpec(new DepotPath("//..."), null);
                        sFlags = new Options(
                            RevertFilesCmdFlags.Preview,
                            0);
                        rFiles = con.Client.RevertFiles(sFlags, fromFile);

                        Assert.IsNotNull(rFiles);
                        Assert.AreEqual(3, rFiles.Count);
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
		///A test for ShelveFiles
		///</summary>
		[TestMethod()]
		public void ShelveFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						Changelist change = new Changelist();
						change.Description = "On the fly built change list";
						FileMetaData file = new FileMetaData();
						file.DepotPath = new DepotPath("//depot/TestData/Letters.txt");
						change.Files.Add(file);

						Options sFlags = new Options(
							ShelveFilesCmdFlags.None,
							change,
							-1
						);

						IList<FileSpec> rFiles = con.Client.ShelveFiles(sFlags);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);

						FileSpec fromFile = new FileSpec(new DepotPath("//depot/TestData/Numbers.txt"), null);
						Options ops = new Options(9, null);
						rFiles = con.Client.ReopenFiles(ops, fromFile);
						Assert.AreEqual(1, rFiles.Count);

						sFlags = new Options(
							ShelveFilesCmdFlags.None,
							null,
							9   // created by last shelve command
						);
						rFiles = con.Client.ShelveFiles(sFlags, fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);
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
		///A test for SyncFiles
		///</summary>
		[TestMethod()]
		public void SyncFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "Alex";
			string pass = string.Empty;
			string ws_client = "alex_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
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

						Assert.AreEqual("Alex", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new DepotPath("//depot/..."), null);

						Options sFlags = new Options(
							SyncFilesCmdFlags.Preview,
							100
						);

						IList<FileSpec> rFiles = con.Client.SyncFiles(sFlags, fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(6, rFiles.Count);

						fromFile = new FileSpec(new DepotPath("//depot/MyCode2/*"), null);

						sFlags = new Options(
							SyncFilesCmdFlags.Force,
							1
						);

						rFiles = con.Client.SyncFiles(sFlags, fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);
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
		///A test for UnlockFiles
		///</summary>
		[TestMethod()]
		public void UnlockFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 5, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						List<FileSpec> oldfiles = con.Client.LockFiles(null);

						Assert.AreNotEqual(null, oldfiles);

						oldfiles = con.Client.UnlockFiles(null);

						Assert.AreNotEqual(null, oldfiles);
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
		///A test for UnshelveFiles
		///</summary>
		[TestMethod()]
		public void UnshelveFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						Changelist change = new Changelist();
						change.Description = "On the fly built change list";
						FileMetaData file = new FileMetaData();
						file.DepotPath = new DepotPath("//depot/TestData/Letters.txt");
						change.Files.Add(file);

						Options sFlags = new Options(
							ShelveFilesCmdFlags.None,
							change,
							-1
						);

						IList<FileSpec> rFiles = con.Client.ShelveFiles(sFlags);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);

						FileSpec fromFile = new FileSpec(new DepotPath("//depot/TestData/Numbers.txt"), null);
						Options ops = new Options(9, null);
						rFiles = con.Client.ReopenFiles(ops, fromFile);
						Assert.AreEqual(1, rFiles.Count);

						sFlags = new Options(
							ShelveFilesCmdFlags.None,
							null,
							9   // created by last shelve command
						);
						rFiles = con.Client.ShelveFiles(sFlags, fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);

						FileSpec revertFiles = new FileSpec(new LocalPath("c:\\MyTestDir\\admin_space\\TestData\\*"), null);
						Options rFlags = new Options(
							RevertFilesCmdFlags.None,
							9
						);
						rFiles = con.Client.RevertFiles(rFlags, revertFiles);

						Options uFlags =
							new Options(UnshelveFilesCmdFlags.None, 9, -1);

						rFiles = con.Client.UnshelveFiles(uFlags, fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);

						rFiles = con.Client.UnshelveFiles(uFlags);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);
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
		///A test for GetClientFileMappings
		///</summary>
		[TestMethod()]
		public void GetClientFileMappingsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new DepotPath("//depot/TestData/Numbers.txt"), null);
						
						IList<FileSpec> rFiles = con.Client.GetClientFileMappings(fromFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);
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
		///A test for CopyFiles
		///</summary>
		[TestMethod()]
		public void CopyFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new DepotPath("//depot/TestData/Numbers.txt"), null);
						FileSpec toFile = new FileSpec(new DepotPath("//depot/TestData42/Numbers.txt"), null);

						IList<FileSpec> rFiles = con.Client.CopyFiles(null, fromFile, toFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);

						fromFile = new FileSpec(new DepotPath("//depot/TestData/*"), null);
						toFile = new FileSpec(new DepotPath("//depot/TestData44/*"), null);

						Options cFlags = new Options(
							CopyFilesCmdFlags.Virtual, null, null, null, -1, 2
							);
						rFiles = con.Client.CopyFiles(cFlags, fromFile, toFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(2, rFiles.Count);
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
		///A test for MergeFiles
		///</summary>
		[TestMethod()]
		public void MergeFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
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

						Assert.AreEqual("admin", con.Client.OwnerName);

						FileSpec fromFile = new FileSpec(new DepotPath("//depot/TestData/Numbers.txt"), null);
						FileSpec toFile = new FileSpec(new DepotPath("//depot/TestData42/Numbers.txt"), null);

						IList<FileSpec> rFiles = con.Client.MergeFiles(null, fromFile, toFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(1, rFiles.Count);

						fromFile = new FileSpec(new DepotPath("//depot/TestData/*"), null);
						toFile = new FileSpec(new DepotPath("//depot/TestData44/*"), null);

						Options cFlags = new Options(
							MergeFilesCmdFlags.Force, null, null, null, -1, 2
							);
						rFiles = con.Client.MergeFiles(cFlags, fromFile, toFile);

						Assert.IsNotNull(rFiles);
						Assert.AreEqual(2, rFiles.Count);
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
