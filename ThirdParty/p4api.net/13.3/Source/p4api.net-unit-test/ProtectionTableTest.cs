using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for ProtectionTableTest and is intended
    ///to contain all ProtectionTableTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ProtectionTableTest
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
		///A test for ProtectionTable Constructor
		///</summary>
		[TestMethod()]
		public void ProtectionTableConstructorTest()
		{
			ProtectionTable target = new ProtectionTable(new ProtectionEntry(ProtectionMode.Super, EntryType.User, " ", " ", " "));
			ProtectionEntry Entry1 = new ProtectionEntry(ProtectionMode.Admin, EntryType.Group, "admin_user", "win-admin-host", "//...");
			ProtectionEntry Entry2 = new ProtectionEntry(ProtectionMode.Read, EntryType.User, "read_user", "win-user-host", "//depot/test/...");
			target.Add(Entry1);
			target.Add(Entry2);
			Assert.AreEqual(Entry1, target[0]);
			Assert.AreEqual(Entry2, target[1]);
		}
	}
}
