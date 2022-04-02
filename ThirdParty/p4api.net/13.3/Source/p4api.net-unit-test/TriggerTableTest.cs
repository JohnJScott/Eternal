using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for TriggerTableTest and is intended
    ///to contain all TriggerTableTest Unit Tests
    ///</summary>
	[TestClass()]
	public class TriggerTableTest
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
		///A test for TriggerTable Constructor
		///</summary>
		[TestMethod()]
		public void TriggerTableConstructorTest()
		{
			TriggerTable target = new TriggerTable(new Trigger("Change_Submit", 1, TriggerType.ChangeSubmit, "//depot/main/...", "p4 submit"), null);
			Trigger Entry1 = new Trigger("Add_a_fix", 1, TriggerType.FixAdd, "//depot/main/fixes/...", "p4 fix");
			Trigger Entry2 = new Trigger("Form_In", 1, TriggerType.FormIn, "//depot/main/...", "p4 spec");
			target.Add(Entry1);
			target.Add(Entry2);
			Assert.AreEqual(Entry1, target[0]);
			Assert.AreEqual(Entry2, target[1]);
		}
	}
}
