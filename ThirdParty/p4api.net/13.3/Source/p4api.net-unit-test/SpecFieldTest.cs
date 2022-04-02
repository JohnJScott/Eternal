using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for SpecFieldTest and is intended
    ///to contain all SpecFieldTest Unit Tests
    ///</summary>
	[TestClass()]
	public class SpecFieldTest
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
		///A test for Code
		///</summary>
		[TestMethod()]
		public void CodeTest()
		{
			SpecField target = new SpecField(101, "default_name",SpecFieldDataType.Text, 10, SpecFieldFieldType.Always); // TODO: Initialize to an appropriate value
			int expected = 106; 
			int actual;
			target.Code = expected;
			actual = target.Code;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for DataType
		///</summary>
		[TestMethod()]
		public void DataTypeTest()
		{
			SpecField target = new SpecField(101, "default_name", SpecFieldDataType.Text, 10, SpecFieldFieldType.Always); // TODO: Initialize to an appropriate value
			SpecFieldDataType expected = SpecFieldDataType.Select; 
			SpecFieldDataType actual;
			target.DataType = expected;
			actual = target.DataType;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for FieldType
		///</summary>
		[TestMethod()]
		public void FieldTypeTest()
		{
			SpecField target = new SpecField(101, "default_name", SpecFieldDataType.Text, 10, SpecFieldFieldType.Required); // TODO: Initialize to an appropriate value
			SpecFieldFieldType expected = SpecFieldFieldType.Required; // TODO: Initialize to an appropriate value
			SpecFieldFieldType actual;
			target.FieldType = expected;
			actual = target.FieldType;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Name
		///</summary>
		[TestMethod()]
		public void NameTest()
		{
			SpecField target = new SpecField(101, "default_name", SpecFieldDataType.Text, 10, SpecFieldFieldType.Always); // TODO: Initialize to an appropriate value
			string expected = "spec_name"; // TODO: Initialize to an appropriate value
			string actual;
			target.Name = expected;
			actual = target.Name;
			Assert.AreEqual(expected, actual);
		}
	}
}
