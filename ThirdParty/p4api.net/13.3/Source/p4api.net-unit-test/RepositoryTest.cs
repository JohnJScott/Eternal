using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace p4api.net.unit.test
{
	
	
	/// <summary>
	///This is a test class for RepositoryTest and is intended
	///to contain all RepositoryTest Unit Tests
	///</summary>
	[TestClass()]
	public partial class RepositoryTest
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
		///A test for GetDepotFiles
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void GetDepotFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/..."), null);

						List<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						IList<FileSpec> target = rep.GetDepotFiles(lfs, null);
						Assert.IsNotNull(target);
						bool foundit = false;
						foreach (FileSpec f in target)
						{
							if (f.DepotPath.Path == "//depot/MyCode/ReadMe.txt")
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);
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
		///A test for GetOpenedFiles
		///</summary>
		[TestMethod()]
		public void GetOpenedFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir,12, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/..."), null);

						List<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						IList<File> target = rep.GetOpenedFiles(lfs, null);
						Assert.IsNotNull(target);
						bool foundit = false;
						foreach (File f in target)
						{
							if (f.DepotPath.Path == "//depot/MyCode/NewFile.txt")
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);

						Options opts = new Options();
						opts["-c"] = "default";
						opts["-C"] = con.Client.Name;
						target = rep.GetOpenedFiles(null,opts);
						Assert.IsNotNull(target);
						if (!unicode)
						{
							Assert.AreEqual(target.Count, 3);
						}
						else
						{
							Assert.AreEqual(target.Count, 7);
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
		///A test for GetFileMetadata
		///</summary>
		[TestMethod()]
		public void GetFileMetadataTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/MyCode2/ReadMe.txt"), null);
						List<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						Options ops = new Options();
						ops["-Oa"] = null;

						DepotPath movedfile = null;
						bool ismapped = false;
						bool shelved = false;
						FileAction headaction = FileAction.None;
						int headchange = -1;
						int headrev = -1;
						FileType headtype = new FileType("none");
						DateTime headtime = DateTime.MinValue;
						DateTime headmodtime = DateTime.MinValue;
						int movedrev = -1;
						int haverev = -1;
						string desc = null;
						string digest = null;
						int filesize = -1;
						FileAction action = FileAction.None;
						FileType type = null;
						string actionowner = null;
						int change = -1;
						bool resolved = false;
						bool unresolved = false;
						bool reresolvable = false;
						int otheropen = -1;
						List<string> otheropenuserclients = new List<string>();
						bool otherlock = false;
						List<string> otherlockuserclients = new List<string>();
						List<FileAction> otheractions = new List<FileAction>();
						List<int> otherchanges = new List<int>();
						bool ourlock = false;
						List<FileResolveAction> resolverecords = new List<FileResolveAction>();
						Dictionary<string, object> attributes = null;

						FileMetaData fmd = new FileMetaData
				 (
				 movedfile,
				 ismapped,
				 shelved,
				 headaction,
				 headchange,
				 headrev,
				 headtype,
				 headtime,
				 headmodtime,
				 movedrev,
				 haverev,
				 desc,
				 digest,
				 filesize,
				 action,
				 type,
				 actionowner,
				 change,
				 resolved,
				 unresolved,
				 reresolvable,
				 otheropen,
				 otheropenuserclients,
				 otherlock,
				 otherlockuserclients,
				 otheractions,
				 otherchanges,
				 ourlock,
				 resolverecords,
				 attributes,
                 null
				 );

						IList<FileMetaData> target = rep.GetFileMetaData(lfs, ops);

						int expected = 2;
						int actual = target[0].HaveRev;
						Assert.AreEqual(expected, actual);

						if (unicode == false)
							expected = 11;
						else
							expected = 10;
						actual = target[0].HeadChange;
						Assert.AreEqual(expected, actual);

						bool isresolved = false;
						bool actualres = target[0].Resolved;
						Assert.AreEqual(isresolved, actualres);

						bool foundit = false;
						foreach (FileMetaData f in target)
						{
							if (f.Attributes.ContainsKey("1st")
								&&
								f.Attributes.ContainsValue("9999")
								)
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);

						foundit = false;
						foreach (FileMetaData f in target)
						{
							if (f.OtherOpenUserClients.Contains("Alice@alice_space")
								|
								f.OtherOpenUserClients.Contains("alice@alice_space"))								
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);

						foundit = false;
						foreach (FileMetaData f in target)
						{
							if (f.ActionOwner.Contains("admin"))
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);

						foundit = false;
						foreach (FileMetaData f in target)
						{
							if (f.Action == FileAction.Integrate)
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);
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
		///A test for Getting historical filetypes from tagged output
		///</summary>
		[TestMethod()]
		public void HistoricalFileTypesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);


						if (unicode == false)
						{
							FileSpec fs = new FileSpec(new DepotPath("//depot/Filetypes/ctempobj"), null);
							List<FileSpec> lfs = new List<FileSpec>();
							lfs.Add(fs);
							FileMetaData fmd = new FileMetaData();
							IList<FileMetaData> target = rep.GetFileMetaData(lfs, null);
							FileType ft = new FileType("ctempobj");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/ctext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("ctext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/cxtext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("cxtext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/ktext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("ktext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/kxtext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("kxtext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/ltext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("ltext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/tempobj"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("tempobj");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/ubinary"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("ubinary");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/uresource"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("uresource");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/uxbinary"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("uxbinary");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/xbinary"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("xbinary");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/xltext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("xltext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/xtext"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("xtext");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/xtempobj"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("xtempobj");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);

							fs = new FileSpec(new DepotPath("//depot/Filetypes/xutf16"), null);
							lfs = new List<FileSpec>();
							lfs.Add(fs);
							fmd = new FileMetaData();
							target = rep.GetFileMetaData(lfs, null);
							ft = new FileType("xutf16");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);
						}
						
						if (unicode == true)
						{
							FileSpec fs = new FileSpec(new DepotPath("//depot/Filetypes/xunicode"), null);
							List<FileSpec> lfs = new List<FileSpec>();
							lfs.Add(fs);
							FileMetaData fmd = new FileMetaData();
							IList<FileMetaData> target = rep.GetFileMetaData(lfs, null);
							FileType ft = new FileType("xunicode");
							Assert.AreEqual(target[0].HeadType.BaseType, ft.BaseType);
							Assert.AreEqual(target[0].HeadType.Modifiers, ft.Modifiers);
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
		///A test for GetDepotDirs
		///</summary>
		[TestMethod()]
		public void GetDepotDirsTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<String> dirs = new List<String>() { "//*" };
						string dir = "//*/*";
						dirs.Add(dir);

						IList<String> target = rep.GetDepotDirs(dirs, null );

						bool foundit = false;
						foreach (string line in target)
						{
							if (line == "//depot/MyCode")
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);	
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
		///A test for GetFixes
		///</summary>
		[TestMethod()]
		public void GetFixesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/..."), null);
						List<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						GetFixesCmdFlags flags = GetFixesCmdFlags.None;
						int changelistId = 0;
						string jobId = null;
						int maxFixes = 2;
						Options ops = new Options(flags, changelistId, jobId, maxFixes);
												
						IList<Fix> target = rep.GetFixes(lfs ,ops);

						foreach (Fix fix in target)
						{
							string expected="admin";
							string actual=fix.UserName;
							Assert.AreEqual (expected,actual );

							expected = "admin_space";
							actual = fix.ClientName;
							Assert.AreEqual(expected, actual);

							if (unicode == false)
							{
								string datestr = "12/2/2010 9:13:00 PM";
								DateTime date = DateTime.Parse(datestr);
								Assert.AreEqual(fix.Date, date);
							}
							else
							{
								string udatestr = "12/2/2010 9:19:21 PM";
								DateTime udate = DateTime.Parse(udatestr);
								Assert.AreEqual(fix.Date, udate);
							}
							
							expected = "job000001";
							actual = fix.JobId;
							Assert.AreEqual(expected, actual);

							expected = "closed";
							actual = fix.Status;
							Assert.AreEqual(expected, actual);

							FixAction left = FixAction.Fixed;
							FixAction right = fix.Action;
							Assert.AreEqual(left, right);

							if (unicode == false)
							{
								int cl = 4;
								int clpos = fix.ChangeId;
								Assert.AreEqual(cl, clpos);
							}
							else
							{
								int ucl = 3;
								int uclpos = fix.ChangeId;
								Assert.AreEqual(ucl, uclpos);
							}

							jobId = "job000001";
							maxFixes = 2;
							ops = new Options(flags, changelistId, jobId, maxFixes);

							target = rep.GetFixes(lfs, ops);

							Assert.AreEqual(target.Count, 1);

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
		///A test for GetFileHistory
		///</summary>
		[TestMethod()]
		public void GetFileHistoryTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/MyCode2/ReadMe.txt"), null);
						List<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						Options ops = new Options();
						ops["-m"] = "2";

						IList<FileHistory> target = rep.GetFileHistory(lfs, ops);

						int expected = 2;
						int actual = target[0].Revision;
						Assert.AreEqual(expected, actual);

						if (unicode == false)
							expected = 11;
						else
							expected = 10;
						actual = target[0].ChangelistId;
						Assert.AreEqual(expected, actual);

						expected = 30;
						long actual10 = target[0].FileSize;
						Assert.AreEqual(expected, actual10);

						string expected11 = "C7DECE3DB80A73F3F53AF4BCF6AC0576";
						string actual11 = target[0].Digest;
						Assert.AreEqual(expected11, actual11);

						string expected1 = "admin";
						string actual1 = target[0].UserName;
						Assert.AreEqual(expected1, actual1);

						if (unicode == false)
							expected1 = "submit without changes ";
						else
							expected1 = "submit with no changes ";
						actual1 = target[0].Description;
						Assert.AreEqual(expected1, actual1);

						expected1 = "admin_space";
						actual1 = target[0].ClientName;
						Assert.AreEqual(expected1, actual1);

						FileAction expected2 = FileAction.Edit;
						FileAction actual2 = target[0].Action;
						Assert.AreEqual(expected2, actual2);

						int expected3 = 1;
						int actual3 = target[1].Revision;
						Assert.AreEqual(expected3, actual3);

						if (unicode == false)
							expected3 = 10;
						else
							expected3 = 8;
						actual3 = target[1].ChangelistId;
						Assert.AreEqual(expected3, actual3);

						string expected4 = "admin";
						string actual4 = target[1].UserName;
						Assert.AreEqual(expected4, actual4);

						expected4 = "branch to MyCode2 ";
						actual4 = target[1].Description;
						Assert.AreEqual(expected4, actual4);

						expected4 = "admin_space";
						actual4 = target[1].ClientName;
						Assert.AreEqual(expected4, actual4);

						FileAction expected5 = FileAction.Branch;
						FileAction actual5 = target[1].Action;
						Assert.AreEqual(expected5, actual5);

						
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
		///A test for GetCounters
		///</summary>
		[TestMethod()]
		public void GetCountersTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<Counter> target = rep.GetCounters(null);
						bool foundit = false;
						foreach (Counter counter in target)
						
						{
							if (counter.Name  == "deleteme"
								&&
								counter.Value == "44")
							{
								foundit = true;
								break;
							}
						}
						Assert.IsTrue(foundit);	
							
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
		///A test for GetCounter
		///</summary>
		[TestMethod()]
		public void GetCounterTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Counter target = rep.GetCounter("deleteme", null);
						bool foundit = false;

						if (target.Name == "deleteme"
							&&
							target.Value == "44")
						{
							foundit = true;
							break;
						}
						Assert.IsTrue(foundit);
					
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
		///A test for DeleteCounter
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void DeleteCounterTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						rep.DeleteCounter("deleteme", null);

						IList<Counter> target = rep.GetCounters(null);
						bool foundit = false;
						foreach (Counter counter in target)
						{
							if (counter.Name == "deleteme"
								&&
								counter.Value == "44")
							{
								foundit = true;
								break;
							}
						}
						Assert.IsFalse(foundit);	
					
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
		///A test for GetProtectionTable
		///</summary>
		[TestMethod()]
		public void GetProtectionTableTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<ProtectionEntry> target = new List<ProtectionEntry>();

						Options ops = new Options();


						ops["-o"] = null;
						target = rep.GetProtectionTable(ops);
						Assert.IsNotNull(target);
						bool foundit = false;


						foreach (ProtectionEntry pte in target)
						{
							if (pte.Mode == ProtectionMode.Admin
								&&
								pte.Type == EntryType.User
								&&
								pte.Name == "Alex"
								&&
								pte.Host == "*"
								&&
								pte.Path == "//MyCode2/ReadMe.txt")
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

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
		///A test for GetTriggerTable
		///</summary>
		[TestMethod()]
		public void GetTriggerTableTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<Trigger> target = new List<Trigger>();

						Options ops = new Options();


						ops["-o"] = null;
						target = rep.GetTriggerTable(ops);
						Assert.IsNotNull(target);
						bool foundit = false;


						foreach (Trigger t in target)
						{
							if (t.Name == "change"
								&&
								t.Type  == TriggerType.ChangeSubmit
								&&
								t.Path == "//depot/..."
								&&
								t.Command == "\"cmd %changelist%\""
								&&
								t.Order == 0)
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

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
		///A test for GetTypeMap
		///</summary>
		[TestMethod()]
		public void GetTypeMapTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<TypeMapEntry> target = new List<TypeMapEntry>();

						Options ops = new Options();


						target = rep.GetTypeMap();
						Assert.IsNotNull(target);
						bool foundit = false;


						foreach (TypeMapEntry t in target)
						{
							if (t.FileType.BaseType.Equals(BaseFileType.Binary)
								&&
								t.FileType.Modifiers.HasFlag(FileTypeModifier.ExclusiveOpen)
								&&
								t.Path == "//depot/Modifiers/...")
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

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
		///A test for GetFormSpec
		///</summary>
		[TestMethod()]
		public void GetFormSpecTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FormSpec target = new FormSpec();

						Options ops = new Options();
						ops["-o"] = null;

						target = rep.GetFormSpec(ops, "change");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "branch");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "client");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "depot");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "group");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "job");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "label");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "spec");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "stream");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "triggers");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "typemap");

						Assert.IsNotNull(target);

						target = rep.GetFormSpec(ops, "user");

						Assert.IsNotNull(target);
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
        ///A test for GetSpecFieldDataType
        ///</summary>
        [TestMethod()]
        public void GetFieldDataTypeTest()
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        FormSpec target = new FormSpec();

                        Options ops = new Options();
                        ops["-o"] = null;

                        target = rep.GetFormSpec(ops, "job");

                        Assert.IsNotNull(target);

                        SpecFieldDataType type =target.GetSpecFieldDataType(target, "Job");

                        Assert.AreEqual(type,SpecFieldDataType.Word);

                        type = target.GetSpecFieldDataType(target, "Status");

                        Assert.AreEqual(type, SpecFieldDataType.Select);

                        type = target.GetSpecFieldDataType(target, "NotHere");

                        Assert.AreEqual(type, SpecFieldDataType.None);

                        type = target.GetSpecFieldDataType(target, "User");

                        Assert.AreEqual(type, SpecFieldDataType.Word);

                        type = target.GetSpecFieldDataType(target, "Date");

                        Assert.AreEqual(type, SpecFieldDataType.Date);

                        type = target.GetSpecFieldDataType(target, "Description");

                        Assert.AreEqual(type, SpecFieldDataType.Text);
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
		///A test for GetReviewers
		///</summary>
		[TestMethod()]
		public void GetReviewersTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/..."), null);

						List<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						IList<User> target =  rep.GetReviewers(lfs, null);
						Assert.IsNotNull(target);
						bool foundit = false;

						foreach (User u in target)
						{
							if ((u.Id == "Alex" |u.Id == "Алексей")
								&&
								(u.FullName == "Alexie Dumas" | u.FullName == "Алексей Дюма")
								&&
								(u.EmailAddress == "Alex@p4test.com" | u.EmailAddress == "alex@p4test.com"))
							{
								foundit = true;
								break;
							}
						}

						Assert.IsTrue(foundit);
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
		///A test for GetProtectionEntries
		///</summary>
		[TestMethod()]
		public void GetProtectionEntriesTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<ProtectionEntry> target = new List<ProtectionEntry>();

						Options ops = new Options();


						ops["-u"] = "alex";
						target = rep.GetProtectionEntries(null, ops);
						Assert.IsNotNull(target);
						bool foundit = false;


						foreach (ProtectionEntry pte in target)
						{
							if (pte.Mode == ProtectionMode.Write
								&&
								pte.Type == EntryType.User
								&&
								pte.Name == "*"
								&&
								pte.Host == "*"
								&&
								pte.Path == "//...")
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

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
		///A test for TagFiles
		///</summary>
		[TestMethod()]
		public void TagFilesTest()
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec fs = new FileSpec(new DepotPath("//depot/Modifiers/ReadMe.txt"), null);

						IList<FileSpec> lfs = new List<FileSpec>();
						lfs.Add(fs);

						Options ops = new Options();
						IList<FileSpec> target = rep.TagFiles(lfs, "admin_label", ops);
						Assert.IsNotNull(target);
						bool foundit = false;
						VersionSpec ver = new Revision(2);
						
						foreach (FileSpec f in target)
						{
							if (f.DepotPath.Path == "//depot/Modifiers/ReadMe.txt"
							   &&
							   f.Version.ToString() == "#2")
													
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

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
		///A test for GetFiles
		///</summary>
		[TestMethod()]
		public void GetFilesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec filespec = new FileSpec(new DepotPath("//depot/MyCode2/..."), null);
						FileSpec filespec1 = new FileSpec(new DepotPath("//depot/Modifiers/..."), null);
						FileSpec filespec2 = new FileSpec(new DepotPath("//depot/TestData/..."), null);
						IList<FileSpec> filespecs = new List<FileSpec>();
						filespecs.Add(filespec);
						filespecs.Add(filespec1);
						filespecs.Add(filespec2);
						Options ops = new Options();

						IList<File> target = rep.GetFiles(filespecs, ops);

						bool foundit = false;
						foreach (File f in target)
						{
							if (f.DepotPath.Path == "//depot/TestData/Numbers.txt")
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);


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
		///A test for GetSubmittedIntegrations
		///</summary>
		[TestMethod()]
		public void GetSubmittedIntegrationsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec filespec = new FileSpec(new DepotPath("//depot/MyCode2/..."), null);
						IList<FileSpec> filespecs = new List<FileSpec>();
						filespecs.Add(filespec);
						Options ops = new Options();

						IList<FileIntegrationRecord> target = rep.GetSubmittedIntegrations(filespecs, ops);

						bool foundit = false;
						foreach (FileIntegrationRecord f in target)
						{

							if (f.How == IntegrateAction.BranchFrom)
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

						filespec = null;
						ops = new Options();
						ops["-b"] = "MyCode->MyCode2";

						target = rep.GetSubmittedIntegrations(filespecs, ops);

						foundit = false;
						foreach (FileIntegrationRecord f in target)
						{

							if (f.FromFile.DepotPath.Path == "//depot/MyCode/ReadMe.txt")
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);


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
		///A test for GetFileLineMatches
		///</summary>
		[TestMethod()]
		public void GetFileLineMatchesTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec filespec = new FileSpec(new DepotPath("//depot/TestData/Letters.txt"), null);
						IList<FileSpec> filespecs = new List<FileSpec>();
						filespecs.Add(filespec);
						string pattern = "kjfrj";
						Options ops = new Options((GetFileLineMatchesCmdFlags.IncludeLineNumbers), 0, 0, 0);
												
						IList<FileLineMatch> target = rep.GetFileLineMatches(filespecs, pattern, ops);

						bool foundit = false;
						foreach (FileLineMatch f in target)
						{

							if (f.LineNumber == 27)
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);
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
		///A test for GetFileAnnotations
		///</summary>
		[TestMethod()]
		public void GetFileAnnotationsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        FileSpec filespec = new FileSpec(new DepotPath("//flow/D1/b/the_other.txt"), null);
						IList<FileSpec> filespecs = new List<FileSpec>();
						filespecs.Add(filespec);
						Options ops = new Options();

						ops["-ac"] = null;

						IList<FileAnnotation> target = rep.GetFileAnnotations(filespecs, ops);
					    
                        bool foundit = false;
						foreach (FileAnnotation f in target)
						{
							if (f.Line == "the other\r\n"
								&&
								f.File.Version.ToString().Contains("28")&&
                                f.File.Version.ToString().Contains("49"))
							{
								foundit = true;
								break;
							}
                            if (f.Line == "the other\r\n"
                                &&
                                f.File.Version.ToString().Contains("110") &&
                                f.File.Version.ToString().Contains("131"))
                            {
                                foundit = true;
                                break;
                            }

						}
						Assert.IsTrue(foundit);

                        ops = new Options();

                        ops["-acIi"] = null;

					    try
					    {
                            target = rep.GetFileAnnotations(filespecs, ops);
					    }
					    catch (Exception e)
					    {
                            Assert.IsNotNull(e);
                            Assert.IsTrue(e.Message.Contains("Usage: annotate [ -aciIqt -d<flags> ] files..."));
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
        ///A test for GetFileAnnotations with branches
        ///</summary>
        [TestMethod()]
        public void GetFileAnnotationsBranchesTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        FileSpec filespec = new FileSpec(new DepotPath("//flow/D1/b/the_other.txt"), null);
                        IList<FileSpec> filespecs = new List<FileSpec>();
                        filespecs.Add(filespec);
                        Options ops = new Options();

                        ops["-aci"] = null;

                        IList<FileAnnotation> target = rep.GetFileAnnotations(filespecs, ops);

                        bool foundit = false;
                        foreach (FileAnnotation f in target)
                        {
                            if (f.Line == "the other\r\n"
                                &&
                                f.File.Version.ToString().Contains("17") &&
                                f.File.Version.ToString().Contains("49"))
                            {
                                foundit = true;
                                break;
                            }
                            if (f.Line == "the other\r\n"
                                &&
                                f.File.Version.ToString().Contains("99") &&
                                f.File.Version.ToString().Contains("131"))
                            {
                                foundit = true;
                                break;
                            }

                        }
                        Assert.IsTrue(foundit);
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
        ///A test for GetFileAnnotations with integrations
        ///</summary>
        [TestMethod()]
        public void GetFileAnnotationsIntegrationsTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        FileSpec filespec = new FileSpec(new DepotPath("//flow/D1/b/the_other.txt"), null);
                        IList<FileSpec> filespecs = new List<FileSpec>();
                        filespecs.Add(filespec);
                        Options ops = new Options();

                        ops["-acI"] = null;

                        IList<FileAnnotation> target = rep.GetFileAnnotations(filespecs, ops);

                        bool foundit = false;
                        foreach (FileAnnotation f in target)
                        {
                            if (f.Line == "the other\r\n"
                                &&
                                f.File.Version.ToString().Contains("17") &&
                                f.File.Version.ToString().Contains("49"))
                            {
                                foundit = true;
                                break;
                            }
                            if (f.Line == "the other\r\n"
                                &&
                                f.File.Version.ToString().Contains("99") &&
                                f.File.Version.ToString().Contains("131"))
                            {
                                foundit = true;
                                break;
                            }

                        }
                        Assert.IsTrue(foundit);
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
		///A test for GetFileContents
		///</summary>
		[TestMethod()]
		public void GetFileContentsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						FileSpec filespec = new FileSpec(new DepotPath("//depot/MyCode/README.txt"), null);
						IList<FileSpec> filespecs = new List<FileSpec>();
						filespecs.Add(filespec);
						Options ops = new Options();

						//ops["-acIi"] = null;

						IList<string> target = rep.GetFileContents(filespecs, ops);

						bool foundit = false;
						foreach (string s in target)
							{

								if (s.Contains("Secret"))
								{
									foundit = true;
									break;
								}

							}
						Assert.IsTrue(foundit);
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
		///A test for GetDepotFileDiffs
		///</summary>
		[TestMethod()]
		public void GetDepotFileDiffsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//depot/MyCode2/...", "//depot/TestData/...", null);

						bool foundit = false;
						foreach (DepotFileDiff d in target)
						{

							if (d.LeftFile.DepotPath.Path.Equals("//depot/MyCode2/ReadMe.txt"))
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);

						target = rep.GetDepotFileDiffs("//depot/MyCode2/ReadMe.txt", "//depot/TestData/Numbers.txt", null);

						foundit = false;
						foreach (DepotFileDiff d in target)
						{

							if (d.Diff.Contains("Secret!"))
							{
								foundit = true;
								break;
							}

						}
						Assert.IsTrue(foundit);
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
        ///A test for GetDepotFileDiffsRCS
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsRCSTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.RCS, 0, 100, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);
                                                     if (!(unicode))
                        {
                        Assert.AreEqual(target[0].Diff, "a7 4\nA change... on Fri Jun 24 16:22:05 PDT 2011\n\nA change... on Fri Jun 24 16:22:07 PDT 2011\n\n");
                        }
                        else
                        {
                        Assert.AreEqual(target[0].Diff, "a7 4\nA change... on Mon Jun 27 15:02:42 PDT 2011\n\nA change... on Mon Jun 27 15:02:42 PDT 2011\n\n");
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
        ///A test for GetDepotFileDiffsContext
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsContextTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.Context, 100, 0, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);
                        if (!(unicode))
                        {
                        Assert.AreEqual(target[0].Diff, "***************\n*** 1,7 ****\n--- 1,11 ----\n  that\n  that\n  thatA Change on Mon May 16 16:01:35 PDT 2011\n  \n  A Change on Tue May 24 14:04:57 PDT 2011-changes to a-\n  A Change on Tue May 24 14:06:00 PDT 2011-changes to a, again-\n  A Change on Tue May 24 14:28:53 PDT 2011-changes to a alone (again)-\n+ A change... on Fri Jun 24 16:22:05 PDT 2011\n+ \n+ A change... on Fri Jun 24 16:22:07 PDT 2011\n+ \n");
                        }
                        else
                        {
                        Assert.AreEqual(target[0].Diff, "***************\n*** 1,7 ****\n--- 1,11 ----\n  that\n  that\n  thatA Change on Mon May 16 16:01:35 PDT 2011\n  \n  A Change on Tue May 24 14:04:57 PDT 2011-changes to a-\n  A Change on Tue May 24 14:06:00 PDT 2011-changes to a, again-\n  A Change on Tue May 24 14:28:53 PDT 2011-changes to a alone (again)-\n+ A change... on Mon Jun 27 15:02:42 PDT 2011\n+ \n+ A change... on Mon Jun 27 15:02:42 PDT 2011\n+ \n");
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
        ///A test for GetDepotFileDiffsSummary
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsSummaryTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.Summary, 0, 100, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);

                        Assert.AreEqual(target[0].Diff, "add 1 chunks 4 lines\ndeleted 0 chunks 0 lines\nchanged 0 chunks 0 / 0 lines\n");
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
        ///A test for GetDepotFileDiffsUnified
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsUnifiedTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.Unified, 0, 100, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);
                        if (!(unicode))
                        {
                        Assert.AreEqual(target[0].Diff, "@@ -1,7 +1,11 @@\n that\n that\n thatA Change on Mon May 16 16:01:35 PDT 2011\n \n A Change on Tue May 24 14:04:57 PDT 2011-changes to a-\n A Change on Tue May 24 14:06:00 PDT 2011-changes to a, again-\n A Change on Tue May 24 14:28:53 PDT 2011-changes to a alone (again)-\n+A change... on Fri Jun 24 16:22:05 PDT 2011\n+\n+A change... on Fri Jun 24 16:22:07 PDT 2011\n+\n");
                        }
                        else
                        {
                        Assert.AreEqual(target[0].Diff, "@@ -1,7 +1,11 @@\n that\n that\n thatA Change on Mon May 16 16:01:35 PDT 2011\n \n A Change on Tue May 24 14:04:57 PDT 2011-changes to a-\n A Change on Tue May 24 14:06:00 PDT 2011-changes to a, again-\n A Change on Tue May 24 14:28:53 PDT 2011-changes to a alone (again)-\n+A change... on Mon Jun 27 15:02:42 PDT 2011\n+\n+A change... on Mon Jun 27 15:02:42 PDT 2011\n+\n");
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
        ///A test for GetDepotFileDiffsIgnoreWSChanges
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsIgnoreWSChangesTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.IgnoreWhitespaceChanges, 0, 100, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);

                        if (!(unicode))
                        {
                        Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Fri Jun 24 16:22:05 PDT 2011\n> \n> A change... on Fri Jun 24 16:22:07 PDT 2011\n> \n");
                        }
                        else
                        {
                        Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n");
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
        ///A test for GetDepotFileDiffsIgnoreWS
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsIgnoreWSTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.IgnoreWhitespace, 0, 100, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);

                        if (!(unicode))
                        {
                        Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Fri Jun 24 16:22:05 PDT 2011\n> \n> A change... on Fri Jun 24 16:22:07 PDT 2011\n> \n");
                        }
                        else
                        {
                        Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n");
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
        ///A test for GetDepotFileDiffsIgnoreLE
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsIgnoreLETest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        Options options = new Options(GetDepotFileDiffsCmdFlags.IgnoreLineEndings, 0, 100, null,
                            null, null);
                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", options);
                        if (!(unicode))
                        {
                                                Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Fri Jun 24 16:22:05 PDT 2011\n> \n> A change... on Fri Jun 24 16:22:07 PDT 2011\n> \n");
                        }
                        else
                        {
                                                Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n");
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
        ///A test for GetDepotFileDiffsNoFlags
        ///</summary>
        [TestMethod()]
        public void GetDepotFileDiffsNoFlags()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
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

                        bool connected = con.Connect(null);
                        Assert.IsTrue(connected);

                        Assert.AreEqual(con.Status, ConnectionStatus.Connected);

                        IList<DepotFileDiff> target = rep.GetDepotFileDiffs("//flow/D1/a/that.txt#1",
                            "//flow/D1/a/that.txt#3", null);

                        if (!(unicode))
                        {
                        Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Fri Jun 24 16:22:05 PDT 2011\n> \n> A change... on Fri Jun 24 16:22:07 PDT 2011\n> \n");
                        }
                        else
                        {
                        Assert.AreEqual(target[0].Diff, "7a8,11\n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n> A change... on Mon Jun 27 15:02:42 PDT 2011\n> \n");
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
		///A test for P4APINET-158
		///</summary>
		[TestMethod()]
		public void JanBug_P4APINET_158_Test()
		{
			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			bool unicode = false;

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
				try
				{
					Perforce.P4.Server srv = new Perforce.P4.Server(new ServerAddress(uri));
					Perforce.P4.Repository p4 = new Perforce.P4.Repository(srv);
					p4.Connection.UserName = user;
					p4.Connection.Connect(new Perforce.P4.Options());
					p4.Connection.Login(pass);
					p4.Connection.Disconnect();
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
