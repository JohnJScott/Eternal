using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.IO;
using System.Diagnostics;

namespace p4api.net.unit.test
{
	
	
	/// <summary>
	///This is a test class for P4ServerTest and is intended
	///to contain all P4ServerTest Unit Tests
	///</summary>
	[TestClass()]
	public class P4ServerTest
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
		//
		#endregion

		//Use TestInitialize to run code before running each test
		//[TestInitialize()]
		//public void MyTestInitialize()
		//{
		//}


		//Use TestCleanup to run code after each test has run
		//[TestCleanup()]
		//public void MyTestCleanup()
		//{
		//}

		/// <summary>
		///A test for P4Server Constructor. Connect to a server check if it supports Unicode and disconnect
		///</summary>
		[TestMethod()]
		public void P4ServerConstructorTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );

				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "dirs", cmdId, false, new String[] { "//depot/*" }, 1 ),
							"\"dirs\" command failed" );

						target.ReleaseConnection(cmdId);
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for RunCommand
		///</summary>
		[TestMethod()]
		public void RunCommandTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						uint cmdId = 7;
						Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new String[] { "//depot/*" }, 1),
							"\"dirs\" command failed");

						target.ReleaseConnection(cmdId);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}
#if DEBUG_TIMEOUT
		/// <summary>
		///A test for RunCommand timeout
		///</summary>
		[TestMethod()]
		public void RunCommandTimeOutTest()
		{

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			Process p4d = Utilities.DeployP4TestServer(TestDir, false);

			try
			{
				using (P4Server target = new P4Server(server, user, pass, ws_client))
				{
					try
					{
						Assert.IsTrue(target.RunCommand("TimeOutTest", false, new String[] { "-1" }, 1));
					}
					catch (P4CommandTimeOutException)
					{
						Assert.Fail("Should not have timed out");
					}
					catch (Exception)
					{
						Assert.Fail("Wrong exception thrown for timeout");
					}

					try
					{
						Assert.IsFalse(target.RunCommand("TimeOutTest", false, new String[] { "1" }, 1));
						Assert.Fail("Didn't timeout");
					}
					catch (P4CommandTimeOutException)
					{
					}
					catch (Exception)
					{
						Assert.Fail("Wrong exception thrown for timeout");
					}
				}
			}
			finally
			{
				Utilities.RemoveTestServer(p4d, TestDir);
			}
		}

		/// <summary>
		///A test for RunCommand timeout
		///</summary>
		[TestMethod()]
		public void RunCommandLongTimeOutTest()
		{

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			Process p4d = Utilities.DeployP4TestServer(TestDir, false);

			try
			{
				using (P4Server target = new P4Server(server, user, pass, ws_client))
				{
					try
					{
						Assert.IsTrue(target.RunCommand("LongTimeOutTest", false, new String[] { "10" }, 1));
					}
					catch (P4CommandTimeOutException)
					{
						Assert.Fail("Should not have timed out");
					}
					catch (Exception)
					{
						Assert.Fail("Wrong exception thrown for timeout");
					}
				}
			}
			finally
			{
				Utilities.RemoveTestServer(p4d, TestDir);
			}
		}
#endif
		/// <summary>
		///A test for GetBinaryResults
		///</summary>
		[TestMethod()]
		public void GetBinaryResultsTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String[] parms = new String[] { "//depot/MyCode/Silly.bmp" };

						uint cmdId = 7;
						Assert.IsTrue(target.RunCommand("print", cmdId, false, parms, 1),
							"\"print\" command failed" );

						byte[] results = target.GetBinaryResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( results, "GetBinaryResults returned null data" );

						Assert.AreEqual( results.Length, 3126 );

						Assert.AreEqual( results[ 0 ], 0x42 );
						Assert.AreEqual( results[ 1 ], 0x4d );
						Assert.AreEqual( results[ 2 ], 0x36 );

						Assert.AreEqual( results[ 0x10 ], 0x00 );
						Assert.AreEqual( results[ 0xA0 ], 0xC0 );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for GetErrorResults
		///</summary>
		[TestMethod()]
		public void GetErrorResultsTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String[] parms = new String[] { "//depot/MyCode/NoSuchFile.bmp" };

						uint cmdId = 7;
						target.RunCommand("print", cmdId, false, parms, 1);

						P4ClientErrorList results = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( results, "GetErrorResults returned null data" );

						Assert.AreEqual( results.Count, 1 );

						P4ClientError firstError = (P4ClientError)results[ 0 ];
						Assert.AreEqual( firstError.ErrorMessage.TrimEnd(new char [] {'\r','\n'}), 
							"//depot/MyCode/NoSuchFile.bmp - no such file(s)." );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for GetInfoResults
		///</summary>
		[TestMethod()]
		public void GetInfoResultsTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";


			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String[] parms = new String[] { "//depot/mycode/*" };

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "files", cmdId, false, parms, 1 ),
							"\"files\" command failed" );

						String[] results = target.GetInfoResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( results, "GetInfoResults returned null data" );

						if( unicode )
							Assert.AreEqual( 3, results.Length );
						else
							Assert.AreEqual( 3, results.Length );

						String firstResult = results[ 0 ];
						Assert.IsTrue( firstResult.StartsWith( "0://depot/MyCode" ));
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for GetTaggedOutput
		///</summary>
		[TestMethod()]
		public void GetTaggedOutputTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String[] parms = new String[] { "//depot/mycode/*" };

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "files", cmdId, true, parms, 1 ),
							"\"files\" command failed" );

						TaggedObjectList results = target.GetTaggedOutput(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( results, "GetTaggedOutput returned null data" );

						if( unicode )
							Assert.AreEqual( 3, results.Count );
						else
							Assert.AreEqual( 3, results.Count );

						//TaggedObject result = results[ 0 ];
						//String depotFile = result[ "depotFile" ];
						//Assert.AreEqual( depotFile, "//depot/MyCode/ReadMe.txt" );

						//result = (TaggedObject)results[ 1 ];
						//depotFile = result[ "depotFile" ];
						//Assert.AreEqual( depotFile, "//depot/MyCode/Silly.bmp" );

						//if( unicode )
						//{
						//    result = (TaggedObject)results[ 2 ];
						//    depotFile = result[ "depotFile" ];
						//    Assert.AreEqual( depotFile, "//depot/MyCode/Пюп.txt" );
						//}
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for GetTextResults
		///</summary>
		[TestMethod()]
		public void GetTextResultsTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String[] parms = new String[] { "//depot/MyCode/ReadMe.txt" };

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "print", cmdId, false, parms, 1 ),
							"\"print\" command failed" );

						String results = target.GetTextResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( results, "GetErrorResults GetTextResults null data" );

						if( unicode )
							Assert.AreEqual( results.Length, 30 );
						else
							Assert.AreEqual( results.Length, 30 );

						Assert.IsTrue( results.StartsWith("Don't Read This!") );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		private byte[] BinaryCallbackResults;

		private void BinaryResultsCallback(uint cmdId, byte[] data)
		{
			if ((data != null) && (data.Length > 0))
				BinaryCallbackResults = data;
		}

		/// <summary>
		///A test for SetBinaryResultsCallback
		///</summary>
		[TestMethod()]
		public void SetBinaryResultsCallbackTest()
		{
			P4Server.BinaryResultsDelegate cb = 
				new P4Server.BinaryResultsDelegate( BinaryResultsCallback );

			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						target.BinaryResultsReceived += cb;

						String[] parms = new String[] { "//depot/MyCode/Silly.bmp" };

						uint cmdId = 7;
						Assert.IsTrue(target.RunCommand("print", cmdId, false, parms, 1),
							"\"print\" command failed" );

						byte[] results = target.GetBinaryResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( BinaryCallbackResults, "BinaryCallbackResults is null" );
						Assert.IsNotNull( results, "InfoCallbackResults is null" );

						Assert.AreEqual( results.Length, BinaryCallbackResults.Length );
						Assert.AreEqual( BinaryCallbackResults.Length, 3126 );

						for( int idx = 0; idx < BinaryCallbackResults.Length; idx++ )
						{
							Assert.AreEqual( results[ idx ], BinaryCallbackResults[ idx ] );
						}
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		private string ErrorCallbackResultsMessage;
		private uint ErrorCallbackResultsMessageId;
		private int ErrorCallbackResultsErrorNumber;
		private int ErrorCallbackResultsSeverity;

		private void ErrorResultsCallback(uint cmdId, int severity, int errorNumber, String message)
		{
			ErrorCallbackResultsMessageId = cmdId;
			ErrorCallbackResultsMessage = message;
			ErrorCallbackResultsSeverity = severity;
			ErrorCallbackResultsErrorNumber = errorNumber;
		}

		/// <summary>
		///A test for SetErrorCallback
		///</summary>
		[TestMethod()]
		public void SetErrorCallbackTest()
		{
			P4Server.ErrorDelegate cb = 
				new P4Server.ErrorDelegate( ErrorResultsCallback );

			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						target.ErrorReceived += cb;

						String[] parms = new String[] { "//depot/MyCode/NoSuchFile.bmp" };

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "fstat", cmdId, false, parms, 1 ),
							"\"fstat\" command failed" );

						P4ClientErrorList results = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsFalse( String.IsNullOrEmpty( ErrorCallbackResultsMessage ), "ErrorCallbackResultsMessage is null or empty" );
						Assert.IsNotNull( results, "GetErrorResults returned null" );

						Assert.AreEqual( results.Count, 1 );

						P4ClientError firstError = (P4ClientError)results[ 0 ];
						Assert.AreEqual( firstError.ErrorMessage.TrimEnd( new char[] { '\r', '\n' } ),
							ErrorCallbackResultsMessage.TrimEnd( new char[] { '\r', '\n' } ) );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		private class InfoCbData
		{
			public InfoCbData(String m, int l)
			{
				Message = m;
				Level = l;
			}
			public String Message;
			public int Level;

			public override string ToString()
			{
				return String.Format("{0}:{1}", Level, Message);
			}
		}

		private ArrayList InfoCallbackResults;

		private void InfoResultsCallback(uint cmdId, int level, String message)
		{
			InfoCallbackResults.Add( new InfoCbData( message, level ) );
		}

		private void BadInfoResultsCallback(uint cmdId, int level, String message)
		{
			throw new Exception("I'm a bad delegate");
		}

		/// <summary>
		///A test for SetInfoResultsCallback
		///</summary>
		[TestMethod()]
		public void SetInfoResultsCallbackTest()
		{
			P4Server.InfoResultsDelegate cb = 
				new P4Server.InfoResultsDelegate( InfoResultsCallback );

			P4Server.InfoResultsDelegate bcb = 
				new P4Server.InfoResultsDelegate( BadInfoResultsCallback );

			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						target.InfoResultsReceived += cb;

						// add in the bad handler that throws an exception, to 
						// make sure the event broadcaster can handle it.
						target.InfoResultsReceived += bcb;

						String[] parms = new String[] { "//depot/mycode/*" };

						InfoCallbackResults = new ArrayList();

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "files", cmdId, false, parms, 1 ),
							"\"files\" command failed" );

						String[] results = target.GetInfoResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( InfoCallbackResults, "InfoCallbackResults is null" );
						Assert.IsNotNull( results, "GetInfoResults returned null" );

						Assert.AreEqual( results.Length, InfoCallbackResults.Count );

						for( int idx = 0; idx < InfoCallbackResults.Count; idx++ )
						{
							Assert.AreEqual( results[idx], InfoCallbackResults[idx].ToString() );
						}
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		private TaggedObjectList TaggedCallbackResults;

		private void TaggedOutputCallback(uint cmdId, int objId, TaggedObject obj)
		{
			TaggedCallbackResults.Add( obj );
		}

		/// <summary>
		///A test for SetTaggedOutputCallback
		///</summary>
		[TestMethod()]
		public void SetTaggedOutputCallbackTest()
		{
			P4Server.TaggedOutputDelegate cb = 
				new P4Server.TaggedOutputDelegate( TaggedOutputCallback );

			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						target.TaggedOutputReceived += cb;

						StringList parms = new String[] { "//depot/mycode/*" };

						TaggedCallbackResults = new TaggedObjectList();

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "fstat", cmdId, true, parms, 1 ),
							"\"fstat\" command failed" );

						TaggedObjectList results = target.GetTaggedOutput(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull( TaggedCallbackResults, "TaggedCallbackResults is null" );
						Assert.IsNotNull( results, "GetTaggedOutput returned null" );

						Assert.AreEqual( results.Count, TaggedCallbackResults.Count );

						for( int idx = 0; idx < TaggedCallbackResults.Count; idx++ )
						{
							Assert.AreEqual( ( results[ idx ] )[ "depotFile" ],
												( TaggedCallbackResults[ idx ] )[ "depotFile" ] );
						}
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		private String TextCallbackResults;

		private void TextResultsCallback(uint cmdId, String info)
		{
			TextCallbackResults += info;
		}

		/// <summary>
		///A test for SetTextResultsCallback
		///</summary>
		[TestMethod()]
		public void SetTextResultsCallbackTest()
		{
			P4Server.TextResultsDelegate cb = 
				new P4Server.TextResultsDelegate( TextResultsCallback );

			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );
				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						target.TextResultsReceived += cb;

						String[] parms = new String[] { "//depot/mycode/ReadMe.txt" };

						TextCallbackResults = String.Empty;

						uint cmdId = 7;
						Assert.IsTrue( target.RunCommand( "print", cmdId, true, parms, 1 ),
							"\"print\" command failed" );

						String results = target.GetTextResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsFalse( String.IsNullOrEmpty( TextCallbackResults ), "TextCallbackResults is null" );
						Assert.IsFalse( String.IsNullOrEmpty( results ), "GetTextResults is null" );

						Assert.AreEqual( results.Length, TextCallbackResults.Length );

						Assert.AreEqual( TextCallbackResults, results );
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
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

			string server = "localhost:6666";
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
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						uint cmdId = 7; 
						Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new String[] { "//depot/*" }, 1),
							"\"dirs\" command failed" );

						target.ReleaseConnection(cmdId);

						String actual = target.Client;
						Assert.AreEqual( actual, "admin_space" );

						target.Client = "admin_space2";

						// run a command to trigure a reconnect
						Assert.IsTrue( target.RunCommand( "dirs", ++cmdId, false, new String[] { "//admin_space2/*" }, 1 ),
							"\"dirs //admin_space2/*\" command failed");

						target.ReleaseConnection(cmdId);

						/// try a bad value
						target.Client = "admin_space3";

						Assert.IsTrue( target.RunCommand( "dirs", ++cmdId, false, new String[] { "//admin_space3/*" }, 1 ),
											"\"dirs //admin_space3/*\" command failed");

						P4ClientErrorList ErrorList = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(ErrorList);
						Assert.AreEqual(ErrorSeverity.E_WARN, ErrorList[0].SeverityLevel);
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
		///A test for DataSet
		///</summary>
		[TestMethod()]
		public void DataSetTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );

				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String expected = "The quick brown fox jumped over the tall white fence.";
						target.SetDataSet(7,expected);

						String actual = target.GetDataSet(7);
						Assert.AreEqual( actual, expected );

						target.ReleaseConnection(7);
					}
				}
				finally
				{
					Utilities.RemoveTestServer( p4d, TestDir );
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for Password
		///</summary>
		[TestMethod()]
		public void PasswordTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "Alex";
			string pass = "pass";
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for( int i = 0; i < 2; i++ ) // run once for ascii, once for unicode
			{
				if( unicode )
					user = "Алексей";
				Process p4d = Utilities.DeployP4TestServer( TestDir, unicode );

				try
				{
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String actual = target.Password;
						Assert.IsNotNull( actual );

						/// try a bad value
						target.Password = "ssap";

						uint cmdId = 7;
						// command triggers a reconnect
						Assert.IsFalse( target.RunCommand( "dirs", cmdId, false, new String[] { "//depot/*" }, 1 ),
											"\"dirs\" command failed" );

						target.ReleaseConnection(cmdId);

						// try a user with no password
						target.User = "admin";
						target.Password = String.Empty;

						// command triggers a reconnect
						Assert.IsTrue(target.RunCommand("dirs", ++cmdId, false, new String[] { "//depot/*" }, 1),
							"\"dirs\" command failed" );

						target.ReleaseConnection(cmdId);
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
		///A test for Port
		///</summary>
		[TestMethod()]
		public void PortTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
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
					using( P4Server target = new P4Server( server, user, pass, ws_client ) )
					{
						if( unicode )
							Assert.IsTrue( target.UseUnicode, "Unicode server detected as not supporting Unicode" );
						else
							Assert.IsFalse( target.UseUnicode, "Non Unicode server detected as supporting Unicode" );

						String expected = "localhost:6666";
						target.Port = expected;

						String actual = target.Port;
						Assert.AreEqual( actual, expected );

						// try a bad value
						target.Port = "null:0";

						uint cmdId = 7;

						// command triggers a reconnect
						bool reconectSucceeded = true;
						try
						{
							reconectSucceeded = target.RunCommand("dirs", cmdId, false, new String[] { "//depot/*" }, 1);

							target.ReleaseConnection(cmdId);
						}
						catch
						{
							reconectSucceeded = false;
						}

						Assert.IsFalse(reconectSucceeded, "Reconnect to \"null:0\" did not fail");
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
		///A test for User
		///</summary>
		[TestMethod()]
		public void UserTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "Alex";
			string pass = "pass";
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				if (unicode)
					user = "Алексей";
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						String actual = target.User;
						Assert.AreEqual(actual, user);

						// try a bad value
						target.User = "John";

						uint cmdId = 7;
						// command triggers a reconnect
						bool success = target.RunCommand("dirs", cmdId, false, new String[] { "//depot/*" }, 1);
						Assert.IsTrue(success, "\"dirs\" command failed");

						P4ClientErrorList errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						// try a user with no password
						target.User = "admin";
						target.Password = String.Empty;

						// command triggers a reconnect
						Assert.IsTrue(target.RunCommand("dirs", ++cmdId, false, new String[] { "//depot/*" }, 1),
							"\"dirs\" command failed");

						target.ReleaseConnection(cmdId);
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
		///A test for ErrorList
		///</summary>
		[TestMethod()]
		public void ErrorListTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "Alex";
			string pass = "pass";
			string ws_client = "admin_space";

			// turn off exceptions for this test
			ErrorSeverity oldExceptionLevel = P4Exception.MinThrowLevel;
			P4Exception.MinThrowLevel = ErrorSeverity.E_NOEXC;

			for (int i = 0; i < 3; i++) // run once for ascii, once for unicode, once for the security level 3 server
			{
				String zippedFile = "a.exe";
				if (i == 1)
				{
					zippedFile = "u.exe";
					user = "Алексей";
					pass = "pass";
				}
				if (i == 2)
				{
					zippedFile = "s3.exe";
					user = "alex";
					pass = "Password";
				}

				Process p4d = Utilities.DeployP4TestServer(TestDir, 10, zippedFile);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						P4ClientErrorList errors = null;

						uint cmdId = 7;
						// a bad user will not fail on the servers fro a.exe and u.exe, 
						// so only test on s3
						if (i == 2)
						{
							// setting the user name will trigger a disconnect
							target.User = "badboy"; // nonexistent user

							// command triggers an attempt to reconnect
							Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new String[] { "//depot/*" }, 1));

							errors = target.GetErrorResults(cmdId);

							target.ReleaseConnection(cmdId);

							Assert.IsNotNull(errors);
							Assert.IsTrue(errors[0].ErrorMessage.Contains("Password must be set before access can be granted"));

							target.User = user; // back to the good  user
						}

						target.Password = "NoWayThisWillWork"; // bad password
						if (i == 2)
						{
							Assert.IsFalse(target.Login("NoWayThisWillWork", null));
							P4ClientError conError = target.ConnectionError;

							Assert.IsNotNull(conError);
							Assert.IsTrue(conError.ErrorMessage.Contains("Password invalid"));
						}
						// command triggers an attempt to reconnect
						Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new String[] { "//depot/*" }, 1));

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(errors);
						if (i == 2)
						{
							Assert.IsTrue(errors[0].ErrorMessage.Contains("Password invalid") ||
								errors[0].ErrorMessage.Contains("Perforce password (P4PASSWD) invalid or unset"));
						}
						else
						{
							Assert.IsTrue(errors[0].ErrorMessage.Contains("Perforce password (P4PASSWD) invalid or unset"));
						}
						target.Password = pass; // back to the good password
						if (i == 2)
						{
							Assert.IsTrue(target.Login(pass, null));
						}

						target.Client = "AintNoClientNamedLikeThis";

						// command triggers an attempt to reconnect (have requires a client)
						Assert.IsFalse(target.RunCommand("have", ++cmdId, false, null, 0));

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(errors);
						Assert.IsTrue(errors[0].ErrorMessage.Contains("unknown - use 'client' command to create it"));

						target.Client = ws_client; // back to the good client name
						target.Port = "NoServerAtThisAddress:666";

						// command triggers an attempt to reconnect
						Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new String[] { "//depot/*" }, 1));

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						P4ClientError conErr = target.ConnectionError;
						if (errors == null)
						{
							Assert.IsNotNull(conErr);
							Assert.IsTrue(conErr.ErrorMessage.Contains("Connect to server failed"));
						}
						else
						{
							Assert.IsNotNull(errors);
							Assert.IsTrue(errors[0].ErrorMessage.Contains("Connect to server failed"));
						}
						target.Port = server;// back to the good port name

						if (i == 2)
						{
							target.Password = null;
							Assert.IsTrue(target.Login(pass, null));
						}

						// command triggers an attempt to reconnect
						// run a good command to make sure the connection is solid
						bool success = target.RunCommand("dirs", ++cmdId, false, new String[] { "//depot/*" }, 1);

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsTrue(success);
						Assert.IsNull(errors);

						// try a bad command name
						Assert.IsFalse(target.RunCommand("dirrrrrs", ++cmdId, false, new String[] { "//depot/*" }, 1));

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(errors);
						Assert.IsTrue(errors[0].ErrorMessage.Contains("Unknown command.  Try 'p4 help' for info"));

						target.Port = server;// back to the good port name
						if (i == 2)
						{
							target.Password = null;
							Assert.IsTrue(target.Login(pass, null));
						}

						// try a bad command parameter
						Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new String[] { "//freebird/*" }, 1));

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(errors);
						Assert.IsTrue(errors[0].ErrorMessage.Contains("must refer to client"));

						// try a bad command flag
						Assert.IsFalse(target.RunCommand("dirs", ++cmdId, false, new String[] { "-UX", "//depot/*" }, 1));

						errors = target.GetErrorResults(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(errors);
						Assert.IsTrue(errors[0].ErrorMessage.Contains("Invalid option: -UX"));
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
			// reset the exception level
			P4Exception.MinThrowLevel = oldExceptionLevel;
		}
	
		/// <summary>
		///A test for ApiLevel
		///</summary>
		[TestMethod()]
		public void ApiLevelTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						Assert.IsTrue(target.ApiLevel > 0);
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
		///A test for GetTaggedOutput
		///</summary>
		[TestMethod()]
		public void TestTaggedUntaggedOutputTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);
				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						String[] parms = new String[] { "//depot/mycode/*" };

						uint cmdId = 7; 
						Assert.IsTrue(target.RunCommand("files", cmdId, false, parms, 1),
							"\"files\" command failed");

						TaggedObjectList results = target.GetTaggedOutput(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNull(results, "GetTaggedOutput did not return null data");
						
						Assert.IsTrue(target.RunCommand("files", ++cmdId, true, parms, 1),
							"\"files\" command failed");

						results = target.GetTaggedOutput(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNotNull(results, "GetTaggedOutput returned null data");

						if (unicode)
							Assert.AreEqual(3, results.Count);
						else
							Assert.AreEqual(3, results.Count);

						Assert.IsTrue(target.RunCommand("files", ++cmdId, false, parms, 1),
							"\"files\" command failed");

						results = target.GetTaggedOutput(cmdId);

						target.ReleaseConnection(cmdId);

						Assert.IsNull(results, "GetTaggedOutput did not return null data");


						//TaggedObject result = results[ 0 ];
						//String depotFile = result[ "depotFile" ];
						//Assert.AreEqual( depotFile, "//depot/MyCode/ReadMe.txt" );

						//result = (TaggedObject)results[ 1 ];
						//depotFile = result[ "depotFile" ];
						//Assert.AreEqual( depotFile, "//depot/MyCode/Silly.bmp" );

						//if( unicode )
						//{
						//    result = (TaggedObject)results[ 2 ];
						//    depotFile = result[ "depotFile" ];
						//    Assert.AreEqual( depotFile, "//depot/MyCode/Пюп.txt" );
						//}
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		private static bool BeginKeepAliveCalled = false;
		private static bool CommandCompletedCalled = false;

		public class KeepAliveTestClass :IKeepAlive
		{

			#region IKeepAlive Members

			public bool StartQueryCancel(P4Server svr, uint cmdId, string cmdLine)
			{
				BeginKeepAliveCalled = true;

				while (CommandCompletedCalled == false)
				{
					System.Threading.Thread.Sleep(100);
				}
				return false;
			}

			public void CommandCompleted(uint cmdId)
			{
				CommandCompletedCalled = true;
			}

			#endregion
		}

		/// <summary>
		///A test for IKeepAlive
		///</summary>
		[TestMethod()]
		public void KeepAliveTest2()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						BeginKeepAliveCalled = false;
						CommandCompletedCalled = false;
						target.KeepAlive = new KeepAliveTestClass2();

						// Short time out will time out and trigger the keep alive
						target.KeepAliveDelay = TimeSpan.Zero;

						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						uint cmdId = 7;
						bool exThrown = false;
						try
						{
							Assert.IsTrue(target.RunCommand("dirs", cmdId, false, new String[] { "//depot/*" }, 1));
						}
						catch(Exception ex)
						{
							exThrown = true;
							Assert.IsTrue(ex.Message.Contains("Command canceled by client"), 
								string.Format("second command thew the wrong exception, {0}", ex.Message));
						}

						target.ReleaseConnection(cmdId);
						
						Assert.IsTrue(BeginKeepAliveCalled, "First BeginKeepAliveCalled");
						Assert.IsTrue(CommandCompletedCalled, "First CommandCompletedCalled");
						Assert.IsTrue(exThrown, "Second Command threw a \"Command canceled\" exception");
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		public class KeepAliveTestClass2 :IKeepAlive
		{

			#region IKeepAlive Members

			public bool StartQueryCancel(P4Server svr, uint cmdId, string cmdLine)
			{
				BeginKeepAliveCalled = true;

				//cancel the command
				return true;
			}

			public void CommandCompleted(uint cmdId)
			{
				CommandCompletedCalled = true;
			}

			#endregion
		}

		/// <summary>
		///A test for IKeepAlive
		///</summary>
		[TestMethod()]
		public void KeepAliveTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						BeginKeepAliveCalled = false;
						CommandCompletedCalled = false;
						target.KeepAlive = new KeepAliveTestClass();

						// Short time out will time out and trigger the keep alive
						target.KeepAliveDelay = TimeSpan.Zero;

						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");


						P4Command cmd = new P4Command(target, "dirs", false, "//depot/*");
						P4CommandResult result = cmd.Run();
						if (result.Success == false)
						{
							P4ClientErrorList errors = target.GetErrorResults(cmd.CommandId);
							if (errors != null)
							{
								foreach (P4ClientError error in errors)
								{
									System.Diagnostics.Trace.WriteLine(error.ErrorMessage);
								}
							}
						}
						Assert.IsTrue(result.Success, "First \"dirs\" command failed");

						Assert.IsTrue(BeginKeepAliveCalled, "First BeginKeepAliveCalled");
						Assert.IsTrue(CommandCompletedCalled, "First CommandCompletedCalled");

						BeginKeepAliveCalled = false;
						CommandCompletedCalled = false;

						// Long time out won't time out and trigger the keep alive
						target.KeepAliveDelay = TimeSpan.FromHours(1);

						result = cmd.Run();

						Assert.IsTrue(result.Success,"Second \"dirs\" command failed");

						Assert.IsFalse(BeginKeepAliveCalled, "Second BeginKeepAliveCalled");
						Assert.IsTrue(CommandCompletedCalled, "Second CommandCompletedCalled");
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		private string ClearP4VarSetting(string var)
		{
			try
			{
				Process p = new Process();

				ProcessStartInfo ps = new ProcessStartInfo();
				ps.FileName = "P4";
				ps.Arguments = string.Format("set {0}=", var);
				ps.RedirectStandardOutput = true; ;
				ps.UseShellExecute = false; ;

				p.StartInfo = ps;

				p.Start();

				string output = p.StandardOutput.ReadToEnd();

				p.WaitForExit();

				return output;
			}
			catch (Exception ex)
			{
				return ex.Message;
			}
		}

		/// <summary>
		///A test for Get/Set
		///</summary>
		[TestMethod()]
		public void GetSetTest()
		{
			bool unicode = false;

			string server = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, unicode);

				try
				{
					using (P4Server target = new P4Server(server, user, pass, ws_client))
					{
						if (unicode)
							Assert.IsTrue(target.UseUnicode, "Unicode server detected as not supporting Unicode");
						else
							Assert.IsFalse(target.UseUnicode, "Non Unicode server detected as supporting Unicode");

						string value = target.Get("P4CLIENT");

						Assert.IsNotNull(value);

						string expected = "C:\\login.bat";

						target.Set("P4LOGINSSO", expected);

						value = target.Get("P4LOGINSSO");

						Assert.AreEqual(expected, value);

						target.Set("P4LOGINSSO", "D:\\login.bat");

						value = target.Get("P4LOGINSSO");

						target.Set("P4LOGINSSO", null);
						//string outp = ClearP4VarSetting("P4LOGINSSO");

						value = target.Get("P4LOGINSSO");

						Assert.AreEqual(null, value);
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
