// Copyright 2015-2022 Eternal Developments LLC. All Rights Reserved.

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

			JsonTestClass? test_reult = JsonHelper.ReadJson<JsonTestClass>( json_string );

			Assert.IsNotNull( test_reult, "Failed to read json" );
			Assert.IsTrue( test_reult.TestBool, "Failed to read json bool properly" );
			Assert.AreEqual( 456, test_reult.TestInt, "Failed to read json int properly" );
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
		}
	}
}