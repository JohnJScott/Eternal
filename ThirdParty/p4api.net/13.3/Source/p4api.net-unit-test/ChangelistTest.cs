using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

using Perforce.P4;
namespace p4api.net.unit.test
{
	
	
	/// <summary>
	///This is a test class for ChangelistTest and is intended
	///to contain all ChangelistTest Unit Tests
	///</summary>
	[TestClass()]
	public class ChangelistTest
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
		///A test for FixJobs
		///</summary>
		[TestMethod()]
		public void FixJobsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
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
						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						Changelist change = new Changelist(9, true);
						change.initialize(con);

						Job job = new Job();
						job.Id = "job000001";

						Options opt = new Options(FixJobsCmdFlags.None, -1, null);
						IList<Fix> fixes = change.FixJobs(null, job);

						Assert.IsNotNull(fixes);
						Assert.AreEqual(1, fixes.Count);
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
		///A test for Submit
		///</summary>
		[TestMethod()]
		public void SubmitTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
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
						Assert.AreEqual(con.Status, ConnectionStatus.Disconnected);

						Assert.AreEqual(con.Server.State, ServerState.Unknown);

						Assert.IsTrue(con.Connect(null));

						Assert.AreEqual(con.Server.State, ServerState.Online);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						Assert.AreEqual("admin", con.Client.OwnerName);

						Changelist change = new Changelist(5, true);
						change.initialize(con);

						SubmitResults sr = change.Submit(null);

						Assert.IsNotNull(sr);
						Assert.AreEqual(1, sr.Files.Count);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
		private static string testChangelistSpec =
@"# A Perforce Change Specification.
#
#  Change:      The change number. 'new' on a new changelist.
#  Date:        The date this specification was last modified.
#  Client:      The client on which the changelist was created.  Read-only.
#  User:        The user who created the changelist.
#  Status:      Either 'pending' or 'submitted'. Read-only.
#  Type:        Either 'public' or 'restricted'. Default is 'public'.
#  Description: Comments about the changelist.  Required.
#  Jobs:        What opened jobs are to be closed by this changelist.
#               You may delete jobs from this list.  (New changelists only.)
#  Files:       What opened files from the default changelist are to be added
#               to this changelist.  You may delete files from this list.
#               (New changelists only.)

Change:	new

Date:	2008/10/15 16:42:12

Client:	admin_space

User:	admin

Status:	new

Description:
	Test for changelist.

Jobs:
	job000608	# Test the changelist parser

Files:
	//depot/Sample Solutions/Threading/ThreadPool/ThreadPool.cs	# edit
	//depot/Sample Solutions/Threading/ThreadPool/ThreadPool.csproj	# edit

";

		/// <summary>
		///A test for Parse
		///</summary>
		[TestMethod()]
		public void ParseTest()
		{
			Changelist target = new Changelist(); 
			string spec = testChangelistSpec;
			bool expected = true;
			bool actual;
			actual = target.Parse(spec);
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for ToString
		///</summary>
		[TestMethod()]
		public void ToStringTest()
		{
			Changelist target = new Changelist();
			string spec = testChangelistSpec;
			bool expected = true;
			bool actual;
			actual = target.Parse(spec);
			Assert.AreEqual(expected, actual);

			Changelist target2 = new Changelist();
			string newSpec = target.ToString();
			expected = true;
			actual = target2.Parse(newSpec);
			Assert.AreEqual(expected, actual);

			Assert.AreEqual(target.Id, target2.Id);
			Assert.AreEqual(target.Description, target2.Description);
			Assert.AreEqual(target.ClientId, target2.ClientId);
			Assert.AreEqual(target.Jobs.Count, target2.Jobs.Count);
			Assert.AreEqual(target.Files.Count, target2.Files.Count);
		}
	}
}
