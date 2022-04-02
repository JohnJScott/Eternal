using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for ClientMetadataTest and is intended
    ///to contain all ClientMetadataTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ClientMetadataTest
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
		///A test for Address
		///</summary>
		[TestMethod()]
		public void AddressTest()
		{
			ClientMetadata target = new ClientMetadata();
			string expected = "10.0.102.80:18020";
			string actual;
			target.Address = expected;
			actual = target.Address;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for CurrentDirectory
		///</summary>
		[TestMethod()]
		public void CurrentDirectoryTest()
		{
			ClientMetadata target = new ClientMetadata();
			string expected = @"c:\Windows\System32";
			string actual;
			target.CurrentDirectory = expected;
			actual = target.CurrentDirectory;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for HostName
		///</summary>
		[TestMethod()]
		public void HostNameTest()
		{
			ClientMetadata target = new ClientMetadata();
			string expected = "win-perforce";
			string actual;
			target.HostName = expected;
			actual = target.HostName;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Name
		///</summary>
		[TestMethod()]
		public void NameTest()
		{
			ClientMetadata target = new ClientMetadata();
			string expected = @"c:\Perforce";
			string actual;
			target.Name = expected;
			actual = target.Name;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Root
		///</summary>
		[TestMethod()]
		public void RootTest()
		{
			ClientMetadata target = new ClientMetadata();
			string expected = "P4V";
			string actual;
			target.Name = expected;
			actual = target.Name;
			Assert.AreEqual(expected, actual);
		}
	}
}
