using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace p4api.net.unit.test
{
	
	
	/// <summary>
	///This is a test class for StreamTest and is intended
	///to contain all StreamTest Unit Tests
	///</summary>
	[TestClass()]
	public class StreamTest
	{
		
		private TestContext testContextInstance;
		static string id = "//projectX/dev";
		static DateTime updated = new DateTime(2011, 04, 10);
		static DateTime accessed = new DateTime(2011, 04, 10);
		static string ownername = "John Smith";
		static string name = "ProjectX development";
		static PathSpec parent = new DepotPath("//projectX/main");
		static PathSpec baseparent = null;
		static StreamType type = StreamType.Development;
		static string description = "development stream for experimental work on projectX";
		static string firmerthanparent = "n/a";
		static string changeflowstoparent = "false";
		static string changeflowsfromparent = "false";
		static StreamOption options = new StreamOptionEnum(StreamOption.OwnerSubmit | StreamOption.Locked | StreamOption.NoToParent | StreamOption.NoFromParent);
		static ViewMap paths = null;
		static MapEntry path = new MapEntry(MapType.Share, new DepotPath("//projectX/main"), null);
		static ViewMap remapped = null;
		static MapEntry remap = new MapEntry(MapType.Include, new DepotPath("//projectX/main"), null);
		static ViewMap ignored = null;
        static ViewMap view = null;
		static MapEntry ig = new MapEntry(MapType.Include, new DepotPath("//projectX/extra"), null);
        static FormSpec spec = new FormSpec(new List<SpecField>(), new Dictionary<string, string>(), new List<string>(), new List<string>(), new Dictionary<string, string>(),
			new Dictionary<string,string>(), "here are the comments");
		
		static Stream target = null;
		
		static void setTarget()
		{
			paths = new ViewMap();
			paths.Add(path);
			remapped = new ViewMap();
			remapped.Add(remap);
			ignored = new ViewMap();
			ignored.Add(ig);
			target = new Stream(id, updated, accessed,
			ownername, name, parent, baseparent, type, description,
			options, firmerthanparent, changeflowstoparent,
			changeflowsfromparent,paths, remapped, ignored, view, spec);
		}


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
		///A test for Accessed
		///</summary>
		[TestMethod()]
		public void AccessedTest()
		{
			DateTime expected = new DateTime(2011,03,01);
			setTarget();
			Assert.AreEqual(target.Accessed, new DateTime(2011, 04, 10));
			target.Accessed = expected;
			DateTime actual;
			actual = target.Accessed;
			Assert.AreEqual(expected, actual);
		}
		

		/// <summary>
		///A test for Description
		///</summary>
		[TestMethod()]
		public void DescriptionTest()
		{
			string expected = "wrong string";
			setTarget();
			Assert.AreEqual(target.Description, "development stream for experimental work on projectX");
			target.Description = expected;
			string actual;
			actual = target.Description;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Ignored
		///</summary>
		[TestMethod()]
		public void IgnoredTest()
		{
			MapEntry fs = new MapEntry(MapType.Include, new DepotPath("//projectX/extra"), null);
			setTarget();
			ViewMap actual;
			actual = target.Ignored;
			Assert.IsTrue(actual.Contains(fs));
		}

		/// <summary>
		///A test for Name
		///</summary>
		[TestMethod()]
		public void NameTest()
		{
			string expected = "wrong string";
			setTarget();
			Assert.AreEqual(target.Name, "ProjectX development");
			target.Name = expected;
			string actual;
			actual = target.Name;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Options
		///</summary>
		[TestMethod()]
		public void OptionsTest()
		{
			setTarget();
			StreamOption actual; 
			actual = target.Options;
			Assert.IsTrue(actual.HasFlag(StreamOption.OwnerSubmit));
			Assert.IsTrue(actual.HasFlag(StreamOption.Locked));
			Assert.IsTrue(actual.HasFlag(StreamOption.NoToParent));
			Assert.IsTrue(actual.HasFlag(StreamOption.NoFromParent));
		}

		/// <summary>
		///A test for OwnerName
		///</summary>
		[TestMethod()]
		public void OwnerNameTest()
		{
			string expected = "wrong string";
			setTarget();
			Assert.AreEqual(target.OwnerName, "John Smith");
			target.OwnerName = expected;
			string actual;
			actual = target.OwnerName;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Parent
		///</summary>
		[TestMethod()]
		public void ParentTest()
		{
			PathSpec expected = new DepotPath("//Wrong/main");
			setTarget();
			Assert.AreEqual(target.Parent, new DepotPath("//projectX/main"));
			target.Parent = expected;
			PathSpec actual;
			actual = target.Parent;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Paths
		///</summary>
		[TestMethod()]
		public void PathsTest()
		{
			MapEntry sp = new MapEntry(MapType.Share, new DepotPath("//projectX/main"), null);
			setTarget();
			ViewMap actual;
			actual = target.Paths;
			Assert.IsTrue(actual.Contains(sp));
		}

		/// <summary>
		///A test for Remapped
		///</summary>
		[TestMethod()]
		public void RemappedTest()
		{
			MapEntry re = new MapEntry(MapType.Include, new DepotPath("//projectX/main"), null);
			setTarget();
			ViewMap actual;
			actual = target.Remapped;
			Assert.IsTrue(actual.Contains(re));
		}

		/// <summary>
		///A test for Spec
		///</summary>
		[TestMethod()]
		public void SpecTest()
		{
			setTarget();
			string actual = target.Spec.Comments;
			string expected = "here are the comments";
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for StreamId
		///</summary>
		[TestMethod()]
		public void StreamIdTest()
		{
			string expected = "//projectX/dev";
			string actual;
			setTarget();
			actual = target.Id;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Type
		///</summary>
		[TestMethod()]
		public void TypeTest()
		{
			StreamType expected = StreamType.Development;
			StreamType actual;
			setTarget();
			actual = target.Type;
			Assert.AreEqual(expected, actual);
		}

		/// <summary>
		///A test for Updated
		///</summary>
		[TestMethod()]
		public void UpdatedTest()
		{
			DateTime expected = new DateTime(2011, 03, 01);
			setTarget();
			Assert.AreEqual(target.Updated, new DateTime(2011, 04, 10));
			target.Updated = expected;
			DateTime actual;
			actual = target.Updated;
			Assert.AreEqual(expected, actual);
		}
	}
}
