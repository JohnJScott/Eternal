using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for FileTest and is intended
    ///to contain all FileTest Unit Tests
    ///</summary>
	[TestClass()]
	public class FileTest
	{


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
		///A test for File Constructor
		///</summary>
		[TestMethod()]
		public void FileConstructorTest()
		{
			DepotPath path = new DepotPath("//depot/main/photo.jpg");
			Revision rev = new Revision(4);
			int change = 4444;
			FileAction action = FileAction.Branch;
			FileType type = new FileType("binary");
			DateTime submittime = new DateTime(2011, 04, 15);
			File target = new File(path, null, rev, null, change, action, type, submittime,null,null);
			File expected = new File();
			expected.DepotPath = new DepotPath("//depot/main/photo.jpg");
			Assert.AreEqual(expected.DepotPath, target.DepotPath);
		}
	}
}
