using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for FileResolveRecordTest and is intended
    ///to contain all FileResolveRecordTest Unit Tests
    ///</summary>
	[TestClass()]
	public class FileResolveRecordTest
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
		///A test for Action
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void ActionTest()
		{
			//FileAction expected = FileAction.COPY_FROM; // TODO: Initialize to an appropriate value
			//FileResolveRecord target = new FileResolveRecord(expected); // TODO: Initialize to an appropriate value
			//FileAction actual;
			//target.Action = expected;
			//actual = target.Action;
			//Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for BaseFileSpec
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void BaseFileSpecTest()
		{
			//FileSpec expected = new FileSpec(new PathSpec(PathType.LOCAL_PATH, "local_prefix", "c:\foolocal"),
			//    (new VersionSpec(new LabelNameSingleVersionSpec("my_label"), new LabelNameSingleVersionSpec("my_old_label"))));
			//FileResolveRecord target = new FileResolveRecord(expected); // TODO: Initialize to an appropriate value
			//FileSpec actual;
			//actual = target.BaseFileSpec;
			//Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for FromFileSpec
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void FromFileSpecTest()
		{
			//FileResolveRecord target = new FileResolveRecord(); // TODO: Initialize to an appropriate value
			//FileSpec expected = new FileSpec(new PathSpec(PathType.LOCAL_PATH, "local_prefix", "c:\foolocal"),
			//    (new VersionSpec(new LabelNameSingleVersionSpec("my_label"), new LabelNameSingleVersionSpec("my_old_label"))));
			//FileSpec actual;
			//target.FromFileSpec = expected;
			//actual = target.FromFileSpec;
			//Assert.AreEqual(expected, actual);
		}
	}
}
