using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for FormSpecTest and is intended
    ///to contain all FormSpecTest Unit Tests
    ///</summary>
	[TestClass()]
	public class FormSpecTest
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
		///A test for FormSpec Constructor
		///commented out
		///</summary>
		//[TestMethod()]
		//public void FormSpecConstructorTest()
		//{
		//    List<SpecField> fields = null; // TODO: Initialize to an appropriate value
		//    List<string> words = null; // TODO: Initialize to an appropriate value
		//    List<string> formats = null; // TODO: Initialize to an appropriate value
		//    Dictionary<string, string> values = null; // TODO: Initialize to an appropriate value
		//    Dictionary<string, string> presets = null; // TODO: Initialize to an appropriate value
		//    string comments = string.Empty; // TODO: Initialize to an appropriate value
		//    FormSpec target = new FormSpec(fields, words, formats, values, presets, comments);
		//    Assert.Inconclusive("TODO: Implement code to verify target");
		//}

		/// <summary>
		///A test for Comments
		///</summary>
		[TestMethod()]
		public void CommentsTest()
		{
			List<SpecField> fields = null; // TODO: Initialize to an appropriate value
            Dictionary<string,string> fieldmap=null;
			List<string> words = null; // TODO: Initialize to an appropriate value
			List<string> formats = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> values = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> presets = null; // TODO: Initialize to an appropriate value
			string comments = "here is a comment"; 
			FormSpec target = new FormSpec(fields, fieldmap,words, formats, values, presets, comments); // TODO: Initialize to an appropriate value
			string expected = comments; 
			string actual;
			target.Comments = expected;
			actual = target.Comments;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Fields
		///</summary>
		[TestMethod()]
		public void FieldsTest()
		{
			List<SpecField> fields = new List<SpecField>();
            Dictionary<string, string> fieldmap = null;
			SpecField Field1 = new SpecField(116, "field1", SpecFieldDataType.Date, 10, SpecFieldFieldType.Key);
			SpecField Field2 = new SpecField(118, "field2", SpecFieldDataType.Word, 64, SpecFieldFieldType.Optional);
			List<string> words = null; // TODO: Initialize to an appropriate value
			List<string> formats = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> values = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> presets = null; // TODO: Initialize to an appropriate value
			string comments = string.Empty; // TODO: Initialize to an appropriate value
			FormSpec target = new FormSpec(fields, fieldmap, words, formats, values, presets, comments); // TODO: Initialize to an appropriate value
			List<SpecField> expected = fields;
			target.Fields.Add(Field1);
			target.Fields.Add(Field2);
			Assert.AreEqual(Field1, target.Fields[0]);
			Assert.AreEqual(Field2, target.Fields[1]);
		}

		/// <summary>
		///A test for Formats
		///</summary>
		[TestMethod()]
		public void FormatsTest()
		{
			List<SpecField> fields = null; // TODO: Initialize to an appropriate value
            Dictionary<string, string> fieldmap = null;
			List<string> words = null; // TODO: Initialize to an appropriate value
			List<string> formats = new List<string>();
			string Format1 = "Change 1 L";
			string Format2 = "Date 3 R";
			Dictionary<string, string> values = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> presets = null; // TODO: Initialize to an appropriate value
			string comments = string.Empty; // TODO: Initialize to an appropriate value
            FormSpec target = new FormSpec(fields, fieldmap,words, formats, values, presets, comments); // TODO: Initialize to an appropriate value
			List<string> expected = formats;
			target.Formats.Add(Format1);
			target.Formats.Add(Format2);
			Assert.AreEqual(Format1, target.Formats[0]);
			Assert.AreEqual(Format2, target.Formats[1]);
		}

		/// <summary>
		///A test for Presets
		///</summary>
		[TestMethod()]
		public void PresetsTest()
		{
			List<SpecField> fields = null; // TODO: Initialize to an appropriate value
            Dictionary<string, string> fieldmap = null;
			List<string> words = null; // TODO: Initialize to an appropriate value
			List<string> formats = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> values = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> presets = new Dictionary<string, string>();
			presets.Add("Status", "open");
			string comments = string.Empty; // TODO: Initialize to an appropriate value
            FormSpec target = new FormSpec(fields, fieldmap,words, formats, values, presets, comments); // TODO: Initialize to an appropriate value
			Dictionary<string, string> expected = presets; // TODO: Initialize to an appropriate value
			Dictionary<string, string> actual;
			target.Presets = expected;
			actual = target.Presets;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Values
		///</summary>
		[TestMethod()]
		public void ValuesTest()
		{
			List<SpecField> fields = null; // TODO: Initialize to an appropriate value
            Dictionary<string, string> fieldmap = null;
			List<string> words = null; // TODO: Initialize to an appropriate value
			List<string> formats = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> values = new Dictionary<string, string>();
			values.Add("Severity", "A/B/C");
			Dictionary<string, string> presets = null; // TODO: Initialize to an appropriate value
			string comments = string.Empty; // TODO: Initialize to an appropriate value
            FormSpec target = new FormSpec(fields, fieldmap, words, formats, values, presets, comments); // TODO: Initialize to an appropriate value
			Dictionary<string, string> expected = values; // TODO: Initialize to an appropriate value
			Dictionary<string, string> actual;
			target.Values = expected;
			actual = target.Values;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Words
		///</summary>
		[TestMethod()]
		public void WordsTest()
		{
			List<SpecField> fields = null; // TODO: Initialize to an appropriate value
            Dictionary<string, string> fieldmap = null;
			List<string> words = new List<string>();
			string Word1 = "View 2";
			string Word2 = "View 4";
			List<string> formats = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> values = null; // TODO: Initialize to an appropriate value
			Dictionary<string, string> presets = null; // TODO: Initialize to an appropriate value
			string comments = string.Empty; // TODO: Initialize to an appropriate value
            FormSpec target = new FormSpec(fields, fieldmap,words, formats, values, presets, comments); // TODO: Initialize to an appropriate value
			target.Words.Add(Word1);
			target.Words.Add(Word2);
			Assert.AreEqual(Word1, target.Words[0]);
			Assert.AreEqual(Word2, target.Words[1]);
		}
	}
}
