// Copyright Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Eternal.ConsoleUtilities;

namespace Eternal.ConsoleUtilitiesTest
{
	/// <summary>
	/// A set of tests for the basic utilities
	/// </summary>
	[TestClass]
	public class ConsoleUtilitiesTests
	{
		/// <summary>
		/// A test that calls all the basic logging functions
		/// </summary>
		[TestMethod( "Basic logging functionality" )]
		public void ConsoleLoggerTest()
		{
			ConsoleLogger.VerboseLogs = true;
			Assert.IsTrue( ConsoleLogger.Verbose( "This is a verbose message that should appear" ) );
			ConsoleLogger.VerboseLogs = false;
			Assert.IsFalse( ConsoleLogger.Verbose( "This is a verbose message that should NOT appear" ) );

			Assert.IsTrue( ConsoleLogger.Log( "This is a log" ) );
			ConsoleLogger.SuppressLogs = true;
			Assert.IsFalse( ConsoleLogger.Log( "This is a log that should NOT appear" ) );

			Assert.IsTrue( ConsoleLogger.Warning( "This is a warning" ) );
			ConsoleLogger.SuppressWarnings = true;
			Assert.IsFalse( ConsoleLogger.Warning( "This is a warning that should NOT appear" ) );

			Assert.IsTrue( ConsoleLogger.Error( "This is an error" ) );
			ConsoleLogger.SuppressErrors = true;
			Assert.IsFalse( ConsoleLogger.Error( "This is an error that should not appear" ) );

			Assert.IsTrue( ConsoleLogger.Title( "This is a title message" ) );
			Assert.IsTrue( ConsoleLogger.Success( "This is a success" ) );
		}

		/// <summary>
		/// A test that calls validates the human readable time duration string
		/// </summary>
		[TestMethod( "TimeString functionality" )]
		public void ConsoleLoggerTimeStringTest()
		{
			Assert.AreEqual( "3 days 4 hours", ConsoleLogger.TimeString( new TimeSpan( 3, 4, 5, 6 ) ),  "Error for time string days" );
			Assert.AreEqual( "1 day 12 hours", ConsoleLogger.TimeString( new TimeSpan( 1, 12, 5, 6 ) ),  "Error for time string days" );
			Assert.AreEqual( "3 hours 14 minutes", ConsoleLogger.TimeString( new TimeSpan( 0, 3, 14, 6 ) ),  "Error for time string hours" );
			Assert.AreEqual( "3 minutes 52 seconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 3, 52 ) ),  "Error for time string minutes" );
			Assert.AreEqual( "53 minutes 2 seconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 53, 2 ) ),  "Error for time string minutes" );
			Assert.AreEqual( "52.0 seconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 0, 52 ) ),  "Error for time string seconds" );
			Assert.AreEqual( "123 milliseconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 0, 0, 123 ) ),  "Error for time string milliseconds" );
			Assert.AreEqual( "999 milliseconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 0, 0, 999 ) ),  "Error for time string milliseconds" );
		}

		/// <summary>
		/// A test that calls validates the human readable time duration string
		/// </summary>
		[TestMethod( "TimeString pluralities" )]
		public void ConsoleLoggerTimeStringPluralTest()
		{
			Assert.AreEqual( "1 day 1 hour", ConsoleLogger.TimeString( new TimeSpan( 1, 1, 5, 6 ) ),  "Error for time string days" );
			Assert.AreEqual( "1 minute 1 second", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 1, 1 ) ),  "Error for time string minutes" );
			Assert.AreEqual( "59.0 seconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 0, 59 ) ),  "Error for time string seconds" );
			Assert.AreEqual( "123 milliseconds", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 0, 0, 123 ) ),  "Error for time string milliseconds" );
			Assert.AreEqual( "1 millisecond", ConsoleLogger.TimeString( new TimeSpan( 0, 0, 0, 0, 1 ) ),  "Error for time string millisecond" );
		}

		/// <summary>
		/// A test that calls validates the human readable time duration string
		/// </summary>
		[TestMethod( "MemoryString functionality" )]
		public void ConsoleLoggerMemoryStringTest()
		{
			Assert.AreEqual( "1.201 TB", ConsoleLogger.MemoryString( 1230 * 1024L * 1024L * 1024L ), "Error for memory string TB" );
			Assert.AreEqual( "1.201 GB", ConsoleLogger.MemoryString( 1230 * 1024L * 1024L ), "Error for memory string GB" );
			Assert.AreEqual( "1.201 MB", ConsoleLogger.MemoryString( 1230 * 1024L ), "Error for memory string MB" );
			Assert.AreEqual( "1.201 kB", ConsoleLogger.MemoryString( 1230 ), "Error for memory string kB" );
			Assert.AreEqual( "123 B", ConsoleLogger.MemoryString( 123 ), "Error for memory string B" );
		}

		private List<string> CapturedOuput = new List<string>();

		private void CaptureOutput( string line )
		{
			CapturedOuput.Add( line );
			ConsoleLogger.Log( line );
		}

		/// <summary>
		/// A basic test of process launching and capturing of output
		/// </summary>
		[TestMethod( "Basic process functionality" )]
		public void ConsoleProcessTest()
		{
			string cwd = Environment.GetEnvironmentVariable( "TEMP" ) ?? string.Empty;
			Assert.AreNotEqual( string.Empty, cwd, "Failed to get environment variable TEMP" );

			{
				ConsoleProcess process = new ConsoleProcess( "cmd.exe", cwd, null, "/c", "dir" );
				Assert.AreEqual( -2, process.ExitCode, "Correctly failed to launch ambiguous exe" );
			}

			string com_spec = Environment.GetEnvironmentVariable( "ComSpec" ) ?? string.Empty;
			Assert.AreNotEqual( string.Empty, cwd, "Failed to get environment variable ComSpec" );

			{
				ConsoleProcess process = new ConsoleProcess( com_spec, cwd, null, "/c", "dir" );
				Assert.AreEqual( 0, process.Wait(), "Correctly launched exe" );
			}

			{
				CapturedOuput.Clear();
				ConsoleProcess process = new ConsoleProcess( com_spec, cwd, CaptureOutput, "/c", "dir" );
				Assert.AreEqual( 0, process.Wait(), "Correctly launched exe" );
				Assert.IsTrue( CapturedOuput.Count > 0, "Failed to capture output" );
			}
		}

		private class JsonTestClass
		{
			public bool TestBool = false;
			public int TestInt = 123;
			public List<string> TestList = new List<string>() { "Alpha", "Beta", "Gamma" };
			public Dictionary<int, string> TestDictionary = new Dictionary<int, string>() { { 1, "One" }, { 2, "Two" }, { 3, "Three" } };
		}

		/// <summary>
		/// A test that writes and reads a json file
		/// </summary>
		[TestMethod("Read and verify a json file")]
		public void JsonHelperTest()
		{
			JsonTestClass test_class = new JsonTestClass();
			test_class.TestBool = true;
			test_class.TestInt = 456;
			string json_string = JsonHelper.WriteJson( test_class );

			JsonTestClass? test_result = JsonHelper.ReadJson<JsonTestClass>( json_string );

			Assert.IsNotNull( test_result, "Failed to read json" );
			Assert.IsTrue( test_result.TestBool, "Failed to read json bool properly" );
			Assert.AreEqual( 456, test_result.TestInt, "Failed to read json int properly" );
			Assert.AreEqual( "Beta", test_class.TestList[1], "Failed to read list properly" );
			Assert.AreEqual( "Two", test_class.TestDictionary[2], "Failed to read list properly" );
		}

		/// <summary>
		/// A test class that is required to be public for the XML abstraction to function.
		/// </summary>
		public class XmlTestClass
		{
			/// <summary>
			/// A test bool field.
			/// </summary>
			public bool TestBool = false;
			/// <summary>
			/// A test integer field.
			/// </summary>
			public int TestInt = 123;
			/// <summary>
			/// A test list container
			/// </summary>
			public List<string> TestList = new List<string>() { "Alpha", "Beta", "Gamma" };
		}

		/// <summary>
		/// A test the writes and reads an XML file.
		/// </summary>
		[TestMethod("Read and verify an XML file.")]
		public void XmlHelperTest()
		{
			string cwd = Environment.GetEnvironmentVariable( "TEMP" ) ?? string.Empty;
			Assert.AreNotEqual( string.Empty, cwd, "Failed to get environment variable TEMP" );

			XmlTestClass test_class = new XmlTestClass();
			test_class.TestBool = true;
			test_class.TestInt = 456;

			string file_name = Path.Combine( cwd, "test.xml" );
			Assert.IsTrue( XmlHelper.WriteXmlFile( file_name, test_class ), "Failed to write xml file" );

			XmlTestClass? result = XmlHelper.ReadXmlFile<XmlTestClass>( file_name );
			Assert.IsNotNull( result, "Failed to read xml file" );
			Assert.AreEqual( true, result.TestBool, "Failed to read xml file properly" );
			Assert.AreEqual( 456, result.TestInt, "Failed to read xml file properly" );
			Assert.AreEqual( "Beta", test_class.TestList[1], "Failed to read list properly" );
		}

		/// <summary>
		/// A test class that is required to be public for the YAML abstraction to function.
		/// </summary>
		public class YmlTestClass
		{
			/// <summary>
			/// A test bool field.
			/// </summary>
			public bool TestBool = false;
			/// <summary>
			/// A test integer field.
			/// </summary>
			public int TestInt = 123;
			/// <summary>
			/// A test container
			/// </summary>
			public List<string> TestList = new List<string>() { "Alpha", "Beta", "Gamma" };
			/// <summary>
			/// A Test map container
			/// </summary>
			public Dictionary<int, string> TestDictionary = new Dictionary<int, string>() { { 1, "One" }, { 2, "Two" }, { 3, "Three" } };
		}

		/// <summary>
		/// A test the writes and reads an XML file.
		/// </summary>
		[TestMethod( "Read and verify an YAML file." )]
		public void YmlHelperTest()
		{
			string cwd = Environment.GetEnvironmentVariable( "TEMP" ) ?? string.Empty;
			Assert.AreNotEqual( string.Empty, cwd, "Failed to get environment variable TEMP" );

			YmlTestClass test_class = new YmlTestClass();
			test_class.TestBool = true;
			test_class.TestInt = 456;

			string file_name = Path.Combine( cwd, "test.yaml" );
			Assert.IsTrue( YamlHelper.WriteYamlFile( file_name, test_class ), "Failed to write yaml file" );

			YmlTestClass? result = YamlHelper.ReadYamlFile<YmlTestClass>( file_name );
			Assert.IsNotNull( result, "Failed to read yaml file" );
			Assert.AreEqual( true, result.TestBool, "Failed to read yaml file properly" );
			Assert.AreEqual( 456, result.TestInt, "Failed to read yaml file properly" );
			Assert.AreEqual( "Beta", test_class.TestList[1], "Failed to read list properly" );
			Assert.AreEqual( "Two", test_class.TestDictionary[2], "Failed to read list properly" );
		}
	}
}
