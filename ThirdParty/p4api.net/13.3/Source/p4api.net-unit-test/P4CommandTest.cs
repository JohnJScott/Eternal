//#define _LOG_TO_FILE

using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Threading;

using Perforce.P4;

namespace p4api.net.unit.test
{      
	/// <summary>
	///This is a test class for P4CommandTest and is intended
	///to contain all P4CommandTest Unit Tests
	///</summary>
	[TestClass()]
	public class P4CommandTest
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
		///A test for Args
		///</summary>
		[TestMethod()]
		public void ArgsTest()
		{
			bool unicode = false;

			string serverAddr = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server server = new P4Server( serverAddr, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( server.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( server.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						P4Command target = new P4Command( server );

						StringList expected = new StringList(new string[]{ "a", "b", "c" });
						target.Args = expected;

						StringList actual = target.Args;

						Assert.AreEqual( expected, actual );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
			// reset the exception level
			P4Exception.MinThrowLevel = oldExceptionLevel;
		}

		/// <summary>
		///A test for Run
		///</summary>
		[TestMethod()]
		public void RunTest()
		{
			bool unicode = false;

			string serverAddr = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server server = new P4Server( serverAddr, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( server.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( server.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						P4Command target = new P4Command( server, "help", false, null );

						P4CommandResult results = target.Run();
						Assert.IsTrue( results.Success );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
			// reset the exception level
			P4Exception.MinThrowLevel = oldExceptionLevel;
		}

		/// <summary>
		///A test for Run
		///</summary>
		[TestMethod()]
		public void RunTest1()
		{
			bool unicode = false;

			string serverAddr = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server server = new P4Server( serverAddr, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( server.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( server.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						P4Command target = new P4Command( server, "help", false, null );

						P4CommandResult results = target.Run(new String[] { "print" });
						Assert.IsTrue( results.Success );

						InfoList helpTxt = target.InfoOutput;

						Assert.IsNotNull( helpTxt );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
			// reset the exception level
			P4Exception.MinThrowLevel = oldExceptionLevel;
		}

		///// <summary>
		/////A test for Submit
		/////</summary>
		//[TestMethod()]
		//public void SubmitTest()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 3, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4CommandResult results = target.Submit("Test submit");

		//                Assert.IsTrue(results.Success);

		//                TaggedObject output = target.TaggedOutput[2];

		//                Assert.IsTrue(output.ContainsKey("submittedChange"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		///// <summary>
		/////A test for Submit
		/////</summary>
		//[TestMethod()]
		//public void SubmitTest1()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4Change change = new P4Change(server, "admin" , "admin_space");

		//                change.Description = "On the fly built change list";
		//                change.Files.Add("//depot/TestData/Letters.txt\t#edit");

		//                //change.Save();
		//                //change.Fetch();

		//                //change.Save();

		//                String spec = change.ToString();

		//                P4CommandResult results = target.Submit(false, false, null, change); //change.ChangeNumber);

		//                Assert.IsTrue(results.Success);

		//                TaggedObject output = target.TaggedOutput[2];

		//                Assert.IsTrue(output.ContainsKey("submittedChange"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		///// <summary>
		/////A test for Submit
		/////</summary>
		//[TestMethod()]
		//public void SubmitTest2()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 3, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4CommandResult results = target.Submit(false, false, null, 5);

		//                Assert.IsTrue(results.Success);

		//                TaggedObject output = target.TaggedOutput[2];

		//                Assert.IsTrue(output.ContainsKey("submittedChange"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		///// <summary>
		/////A test for Submit
		/////</summary>
		//[TestMethod()]
		//public void SubmitTest3()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4CommandResult results = target.Submit(false, false, null, "Test submit", "c:\\MyTestDir\\admin_space\\TestData\\Letters.txt");

		//                Assert.IsTrue(results.Success);

		//                TaggedObject output = target.TaggedOutput[2];

		//                Assert.IsTrue(output.ContainsKey("submittedChange"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		///// <summary>
		/////A test for Resolve
		/////</summary>
		//[TestMethod()]
		//public void ResolveTest()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4CommandResult results = target.Submit(false, false, null, "Check It In!",
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsFalse(results.Success);

		//                Dictionary<String,String> responses = new Dictionary<string, string>();
		//                responses["DefaultResponse"] = "s";
		//                responses["Accept(a) Edit(e) Diff(d) Merge (m) Skip(s) Help(?) am: "] = "am";

		//                results = target.Resolve(responses, P4Command.DiffOptions.none,
		//                    false, false, false, false, false, 
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsTrue(results.Success);

		//                InfoList output = results.InfoOutput;

		//                Assert.IsTrue(output[1].Info.Contains("0 conflicting"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		///// <summary>
		/////A test for Resolve
		/////</summary>
		//[TestMethod()]
		//public void ResolveTest1()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4CommandResult results = target.Submit(false, false, null, "Check It In!",
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsFalse(results.Success);

		//                results = target.Resolve(P4Command.AutomaticResolve.AutoMerge, P4Command.DiffOptions.none,
		//                    false, false, false, false, false,
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsTrue(results.Success);

		//                InfoList output = results.InfoOutput;

		//                Assert.IsTrue(output[1].Info.Contains("0 conflicting"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		//private String HandlePrompt(String msg, bool displayText)
		//{
		//    return "am";
		//}

		///// <summary>
		/////A test for Resolve
		/////</summary>
		//[TestMethod()]
		//public void ResolveTest2()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4Server.PromptHandlerDelegate promptHandler = new P4Server.PromptHandlerDelegate(HandlePrompt);

		//                P4CommandResult results = target.Submit(false, false, null, "Check It In!",
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsFalse(results.Success);

		//                results = target.Resolve(promptHandler, P4Command.DiffOptions.none,
		//                    false, false, false, false, false,
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsTrue(results.Success);

		//                InfoList output = results.InfoOutput;

		//                Assert.IsTrue(output[1].Info.Contains("0 conflicting"));
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		///// <summary>
		/////A test for Diff
		/////</summary>
		//[TestMethod()]
		//public void DiffTest()
		//{
		//    bool unicode = false;

		//    string serverAddr = "localhost:6666";
		//    string user = "admin";
		//    string pass = string.Empty;
		//    string ws_client = "admin_space";

		//    // turn off exceptions for this test
		//    ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
		//    P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

		//    for (int i = 0; i < 1; i++) // run once for ascii, once for unicode (so far only ASCII test server works for theses tests)
		//    {
		//        Process p4d = Utilities.DeployP4TestServer(TestDir, 2, unicode);
		//        try
		//        {
		//            using (P4Server server = new P4Server(serverAddr, user, pass, ws_client))
		//            {
		//                if (unicode)
		//                    Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
		//                else
		//                    Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");

		//                P4Command target = new P4Command(server);

		//                P4Server.PromptHandlerDelegate promptHandler = new P4Server.PromptHandlerDelegate(HandlePrompt);

		//                P4CommandResult results = target.Diff(
		//                    P4Command.DiffOptions.IgnoreWhitespace | P4Command.DiffOptions.context, 
		//                    false, 10, P4Command.DiffFileList.none, false,
		//                    "c:\\MyTestDir\\admin_space\\TestData\\Numbers.txt");

		//                Assert.IsTrue(results.Success);

		//                InfoList output = results.InfoOutput;

		//                Assert.IsTrue(output[0].Info.Contains("//depot/TestData/Numbers.txt#1 "));

		//                Assert.IsNotNull(results.TextOutput);
		//            }
		//        }
		//        finally
		//        {
		//            Utilities.RemoveTestServer(p4d, TestDir);
		//        }
		//        unicode = !unicode;
		//    }
		//}

		P4Command cmd1 = null;
		P4Command cmd2 = null;
		P4Command cmd3 = null;
		P4Command cmd4 = null;
		P4Command cmd5 = null;
		P4Command cmd6 = null;

		bool run = true;

		TimeSpan delay = TimeSpan.FromMilliseconds(5);

		private void cmdThreadProc1()
		{
			try
			{
				while (run)
				{
					cmd1 = new P4Command(server, "fstat", false, "//depot/...");

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 1 starting command: {0:X8}, at {1}",
						cmd1.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd1.Run();

					WriteLine(string.Format("Thread 1 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
						cmd1.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput != null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}
					if (result.ErrorList != null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}
					if (result.TextOutput != null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}
					if (result.TaggedOutput != null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}
					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs != null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}

					if (!result.Success)
					{
						WriteLine(string.Format("Thread 1, fstat failed:{0}",(result.ErrorList!=null && result.ErrorList.Count>0)?result.ErrorList[0].ErrorMessage :"<unknown error>"));
					}
					else
					{
						WriteLine(string.Format("Thread 1, fstat Success:{0}", (result.InfoOutput != null && result.InfoOutput.Count>0) ? result.InfoOutput[0].Info : "<no output>"));
					}
					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
				WriteLine("Thread 1 cleanly exited");
				return;
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		private void cmdThreadProc2()
		{
			try
			{
				while (run)
				{
					cmd2 = new P4Command(server, "dirs", false, "//depot/*");

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 2 starting command: {0:X8}, at {1}",
						cmd2.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd2.Run();

					WriteLine(string.Format("Thread 2 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
						cmd2.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput!=null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}
					if (result.ErrorList!=null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}
					if (result.TextOutput!=null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}
					if (result.TaggedOutput!=null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}
					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs!=null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}

					if (!result.Success)
					{
						WriteLine(string.Format("Thread 2, dirs failed:{0}", (result.ErrorList != null && result.ErrorList.Count > 0) ? result.ErrorList[0].ErrorMessage : "<unknown error>"));
					}
					else
					{
						WriteLine(string.Format("Thread 2, dirs Success:{0}", (result.InfoOutput != null && result.InfoOutput.Count>0) ? result.InfoOutput[0].Info : "<no output>"));
					}
					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
				WriteLine("Thread 2 cleanly exited");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		private void cmdThreadProc3()
		{
			try
			{
				while (run)
				{
					cmd3 = new P4Command(server, "edit", false, "-n", "C:\\MyTestDir\\admin_space\\...");

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 3 starting command: {0:X8}, at {1}",
						cmd3.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd3.Run();

					WriteLine(string.Format("Thread 3 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
						cmd3.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput != null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}
					if (result.ErrorList != null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}
					if (result.TextOutput != null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}
					if (result.TaggedOutput != null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}
					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs != null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}

					if (!result.Success)
					{
						WriteLine(string.Format("Thread 3, edit failed:{0}", (result.ErrorList != null && result.ErrorList.Count>0) ? result.ErrorList[0].ErrorMessage : "<unknown error>"));
					}
					else
					{
						WriteLine(string.Format("Thread 3, edit Success:{0}", (result.InfoOutput != null && result.InfoOutput.Count>0) ? result.InfoOutput[0].Info : "<no output>"));
					}
					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
				WriteLine("Thread 3 cleanly exited");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		private void cmdThreadProc4()
		{
			try
			{
				while (run)
				{
					using (P4Server _P4Server = new P4Server("localhost:6666", null, null, null))
					{
						string val = _P4Server.Get("P4IGNORE");
						bool _p4IgnoreSet = !string.IsNullOrEmpty(val);

						if (_p4IgnoreSet)
						{
							WriteLine(string.Format("P4Ignore is set, {0}", val));
						}
						else
						{
							WriteLine("P4Ignore is not set");
						}

						Assert.IsTrue(_P4Server.ApiLevel > 0);
					}
					cmd4 = new P4Command(server, "fstat", false, "//depot/...");

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 4 starting command: {0:X8}, at {1}",
						cmd4.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd4.Run();

					WriteLine(string.Format("Thread 4 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
						cmd4.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput != null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}
					if (result.ErrorList != null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}
					if (result.TextOutput != null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}
					if (result.TaggedOutput != null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}
					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs != null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}
					if (!result.Success)
					{
						WriteLine(string.Format("Thread 4, fstat failed:{0}",(result.ErrorList!=null && result.ErrorList.Count>0)?result.ErrorList[0].ErrorMessage :"<unknown error>"));
					}
					else
					{
						WriteLine(string.Format("Thread 4, fstat Success:{0}", (result.InfoOutput != null && result.InfoOutput.Count>0) ? result.InfoOutput[0].Info : "<no output>"));
					}
					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
				WriteLine("Thread 4 cleanly exited");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		private void cmdThreadProc5()
		{
			try
			{
				while (run)
				{
					cmd5 = new P4Command(server, "dirs", false, "//depot/*");

					DateTime StartedAt = DateTime.Now;

					WriteLine(string.Format("Thread 5 starting command: {0:X8}, at {1}",
						cmd5.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd5.Run();

					WriteLine(string.Format("Thread 5 Finished command: {0:X8}, at {1}, run time {2} Milliseconds",
						cmd5.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now - StartedAt).TotalMilliseconds));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput != null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}
					if (result.ErrorList != null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}
					if (result.TextOutput != null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}
					if (result.TaggedOutput != null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}
					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs != null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}

					if (!result.Success)
					{
						WriteLine(string.Format("Thread 5, dirs failed:{0}", (result.ErrorList != null && result.ErrorList.Count > 0) ? result.ErrorList[0].ErrorMessage : "<unknown error>"));
					}
					else
					{
						WriteLine(string.Format("Thread 5, dirs Success:{0}", (result.InfoOutput != null && result.InfoOutput.Count>0) ? result.InfoOutput[0].Info : "<no output>"));
					}
					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
				WriteLine("Thread 5 cleanly exited");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		private void cmdThreadProc6()
		{
			try
			{
				while (run)
				{
					cmd6 = new P4Command(server, "edit", false, "-n", "C:\\MyTestDir\\admin_space\\...");

					DateTime StartedAt = DateTime.Now;
					WriteLine(string.Format("Thread 6 starting command: {0:X8}, at {1}", 
						cmd6.CommandId, StartedAt.ToLongTimeString()));

					P4CommandResult result = cmd6.Run();

					WriteLine(string.Format("Thread 6 Finished command: {0:X8}, at {1}, run time {2} Milliseconds", 
						cmd6.CommandId, StartedAt.ToLongTimeString(), (DateTime.Now-StartedAt).TotalMilliseconds ));

					P4CommandResult lastResult = server.LastResults;

					Assert.AreEqual(result.Success, lastResult.Success);
					if (result.InfoOutput != null)
					{
						Assert.AreEqual(result.InfoOutput.Count, lastResult.InfoOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.InfoOutput);
					}
					if (result.ErrorList != null)
					{
						Assert.AreEqual(result.ErrorList.Count, lastResult.ErrorList.Count);
					}
					else
					{
						Assert.IsNull(result.ErrorList);
					}
					if (result.TextOutput != null)
					{
						Assert.AreEqual(result.TextOutput, lastResult.TextOutput);
					}
					else
					{
						Assert.IsNull(lastResult.TextOutput);
					}
					if (result.TaggedOutput != null)
					{
						Assert.AreEqual(result.TaggedOutput.Count, lastResult.TaggedOutput.Count);
					}
					else
					{
						Assert.IsNull(lastResult.TaggedOutput);
					}
					Assert.AreEqual(result.Cmd, lastResult.Cmd);
					if (result.CmdArgs != null)
					{
						Assert.AreEqual(result.CmdArgs.Length, lastResult.CmdArgs.Length);
					}
					else
					{
						Assert.IsNull(lastResult.CmdArgs);
					}

					if (!result.Success)
					{
						WriteLine(string.Format("Thread 6, edit failed:{0}", (result.ErrorList != null && result.ErrorList.Count>0) ? result.ErrorList[0].ErrorMessage : "<unknown error>"));
					}
					else
					{
						WriteLine(string.Format("Thread 6, edit Success:{0}", (result.InfoOutput != null && result.InfoOutput.Count>0) ? result.InfoOutput[0].Info : "<no output>"));
					}
					//Assert.IsTrue(result.Success);
					if (delay != TimeSpan.Zero)
					{
						Thread.Sleep(delay);
					}
				}
				WriteLine("Thread 6 cleanly exited");
			}
			catch (ThreadAbortException)
			{
				Thread.ResetAbort();
				return;
			}
			catch (Exception ex)
			{
				Assert.Fail(ex.Message);
			}
		}

		P4Server server = null;

#if _LOG_TO_FILE
		static System.IO.StreamWriter sw = null;

		public static void WriteLine(string msg)
		{
			lock (sw)
			{
				sw.WriteLine(msg);
				sw.Flush();
			}
		}

        public static void LogBridgeMessage( int log_level,String source,String message )
		{
			WriteLine(string.Format("[{0}] {1}:{2}", source, log_level, message));
		}

        private static LogFile.LogMessageDelgate LogFn = new LogFile.LogMessageDelgate(LogBridgeMessage);

#else
		public void WriteLine(string msg)
		{
			Trace.WriteLine(msg);
		}
#endif
		/// <summary>
		///A test for Running multiple command concurrently
		///</summary>
		[TestMethod()]
		public void RunAsyncTest()
		{
#if _LOG_TO_FILE
			using (sw = new System.IO.StreamWriter("C:\\Logs\\RunAsyncTestLog.Txt", true))
			{
				LogFile.SetLoggingFunction(LogFn);
#endif

				bool unicode = false;

				string serverAddr = "localhost:6666";
				string user = "admin";
				string pass = string.Empty;
				string ws_client = "admin_space";

				// turn off exceptions for this test
				ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
				P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

				for (int i = 0; i < 1; i++) // run once for ascii, once for unicode
				{
					Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
					try
					{
						using (server = new P4Server(serverAddr, user, pass, ws_client))
						{
							if (unicode)
								Assert.IsTrue(server.UseUnicode, "Unicode server detected as not supporting Unicode");
							else
								Assert.IsFalse(server.UseUnicode, "Non Unicode server detected as supporting Unicode");


							run = true;

							Thread t1 = new Thread(new ThreadStart(cmdThreadProc1));
							t1.Name = "RunAsyncTest Thread t1";
							Thread t2 = new Thread(new ThreadStart(cmdThreadProc2));
							t2.Name = "RunAsyncTest Thread t2";
							Thread t3 = new Thread(new ThreadStart(cmdThreadProc3));
							t3.Name = "RunAsyncTest Thread t3";

							Thread t4 = new Thread(new ThreadStart(cmdThreadProc4));
							t4.Name = "RunAsyncTest Thread t4";
							Thread t5 = new Thread(new ThreadStart(cmdThreadProc5));
							t5.Name = "RunAsyncTest Thread t5";
							Thread t6 = new Thread(new ThreadStart(cmdThreadProc6));
							t6.Name = "RunAsyncTest Thread t6";

							t1.Start();
							Thread.Sleep(TimeSpan.FromSeconds(5)); // wait to start a 4th thread
							t2.Start();
							t3.Start();
							Thread.Sleep(TimeSpan.FromSeconds(5)); // wait to start a 4th thread

							run = false;

							if (t1.Join(1000) == false)
							{
								WriteLine("Thread 1 did not cleanly exit");
								t1.Abort();
							}
							if (t2.Join(1000) == false)
							{
								WriteLine("Thread 2 did not cleanly exit");
								t2.Abort();
							}
							if (t3.Join(1000) == false)
							{
								WriteLine("Thread 3 did not cleanly exit");
								t3.Abort();
							}

							Thread.Sleep(TimeSpan.FromSeconds(15)); // wait 15 seconds so will disconnect

							run = true; ;

							t1 = new Thread(new ThreadStart(cmdThreadProc1));
							t1.Name = "RunAsyncTest Thread t1b";
							t2 = new Thread(new ThreadStart(cmdThreadProc2));
							t2.Name = "RunAsyncTest Thread t2b";
							t3 = new Thread(new ThreadStart(cmdThreadProc3));
							t3.Name = "RunAsyncTest Thread t3b";

							t1.Start();
							t2.Start();
							t3.Start();
							Thread.Sleep(TimeSpan.FromSeconds(1)); // wait to start a 4th thread

							t4.Start();
							Thread.Sleep(TimeSpan.FromSeconds(2)); // wait to start a 5th thread
							t5.Start();
							Thread.Sleep(TimeSpan.FromSeconds(3)); // wait to start a 6th thread
							t6.Start();

							//Thread.Sleep(TimeSpan.FromMinutes(15)); // run all threads for 15 sseconds
							Thread.Sleep(TimeSpan.FromSeconds(15)); // run all threads for 15 sseconds

							run = false;

							if (t1.Join(1000) == false)
							{
								WriteLine("Thread 1 did not cleanly exit");
								t1.Abort();
							}
							if (t2.Join(1000) == false)
							{
								WriteLine("Thread 2 did not cleanly exit");
								t2.Abort();
							}
							if (t3.Join(1000) == false)
							{
								WriteLine("Thread 3 did not cleanly exit");
								t3.Abort();
							}
							if (t4.Join(1000) == false)
							{
								WriteLine("Thread 4 did not cleanly exit");
								t4.Abort();
							}
							if (t5.Join(1000) == false)
							{
								WriteLine("Thread 5 did not cleanly exit");
								t5.Abort();
							}
							if (t6.Join(1000) == false)
							{
								WriteLine("Thread 6 did not cleanly exit");
								t6.Abort();
							}
						}
					}
					catch (Exception ex)
					{
						Assert.Fail("Test threw an exception: {0}\r\n{1}", ex.Message, ex.StackTrace);
					}
					finally
					{
						Utilities.RemoveTestServer(p4d, TestDir);
					}
					unicode = !unicode;
				}
				// reset the exception level
				P4Exception.MinThrowLevel = oldExceptionLevel;
#if _LOG_TO_FILE
			}
#endif
		}
	}
}
