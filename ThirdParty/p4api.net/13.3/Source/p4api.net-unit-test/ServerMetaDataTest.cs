using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace p4api.net.unit.test
{
    
    
    /// <summary>
    ///This is a test class for ServerMetaDataTest and is intended
    ///to contain all ServerMetaDataTest Unit Tests
    ///</summary>
	[TestClass()]
	public class ServerMetaDataTest
	{


		private TestContext testContextInstance;

        static string name = "newServer";
        static ServerAddress address = new ServerAddress("perforce:1984");
        static string root = @"C:\TestDepot\";
        static DateTime date = new DateTime(2011, 03, 21);
        private static string datetimeoffset = null;
        static int uptime = 192455;
        static ServerVersion version = new ServerVersion("P4D", "NTX86","2011.1", "12345", new DateTime(2011, 03, 21));
        static ServerLicense license = new ServerLicense(300, new DateTime(2012, 04, 09));
        static string licenseIp = "server.site.com";
        static bool caseSensitive = false;
        static bool unicodeEnabled = false;
		static bool moveEnabled = true;

        static ServerMetaData target = null;
        static void setTarget()
        {
            target = new ServerMetaData(name, address, root, date, datetimeoffset,
                					 uptime, version, license, licenseIp,
									 caseSensitive, unicodeEnabled, moveEnabled);

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
		///A test for Address
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void AddressTest()
		{
			ServerAddress expected = new ServerAddress("perforce:1984");
            setTarget();
            Assert.AreEqual(target.Address, expected);
			
		}

		/// <summary>
		///A test for CaseSensitive
		///</summary>
        [TestMethod()]
        [DeploymentItem("p4api.net.dll")]
        public void CaseSensitiveTest()
        {
            setTarget();
            Assert.AreEqual(target.CaseSensitive, false);
        }

		/// <summary>
		///A test for Date
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void DateTest()
		{
            DateTime expected = new DateTime(2011, 03, 21);
            setTarget();
			Assert.AreEqual(expected, target.Date);
		}

		/// <summary>
		///A test for License
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void LicenseTest()
		{
			int expected = 300;
            setTarget();
			Assert.AreEqual(expected, target.License.Users);
		}

		/// <summary>
		///A test for LicenseIp
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void LicenseIpTest()
		{
			string expected = "server.site.com";
            setTarget();
			Assert.AreEqual(expected, target.LicenseIp);
		}

		/// <summary>
		///A test for Name
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void NameTest()
		{
            string expected = "newServer";
            setTarget();
			Assert.AreEqual(expected, target.Name);
		}

		/// <summary>
		///A test for Root
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void RootTest()
		{
            string expected = @"C:\TestDepot\";
            setTarget();
			Assert.AreEqual(expected, target.Root);
		}

		/// <summary>
		///A test for UnicodeEnabled
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void UnicodeEnabledTest()
		{
            setTarget();
			Assert.AreEqual(target.UnicodeEnabled, false);
		}

		/// <summary>
		///A test for Uptime
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void UptimeTest()
		{
            int expected = 192455;
            setTarget();
			Assert.AreEqual(expected, target.Uptime);
		}

		/// <summary>
		///A test for Version
		///</summary>
		[TestMethod()]
		[DeploymentItem("p4api.net.dll")]
		public void VersionTest()
		{
            setTarget();
			Assert.AreEqual(target.Version.Major, "2011.1");
            Assert.AreEqual(target.Version.Minor, "12345");
            Assert.AreEqual(target.Version.Platform, "NTX86");
            Assert.AreEqual(target.Version.Product, "P4D");
            Assert.AreEqual(target.Version.Date, new DateTime(2011, 03, 21));
		}
	}
}