// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;

using Eternal.EternalUtilities;
using Perforce.P4;

namespace Eternal.SourceServerIndexer
{
	/// <summary>
	/// The <see cref="Eternal.SourceServerIndexer"/> namespace contains functions to index symbol files with source server data.
	/// </summary>
	[System.Runtime.CompilerServices.CompilerGenerated]
	class NamespaceDoc
	{
	}

	/// <summary>A utility to source index symbol files.</summary>
	/// <remarks>
	/// This utility is designed to replace the source indexing Perl script in 'Debbugging Tools for Windows'. The Perl script is not easy to use, 
	/// tends to fail silently, and does not display helpful error messages. This utility thoroughly validates the enviroment and clearly displays any errors that are found.
	/// The basic workflow is:
	/// <list type="bullet">
	/// <item>Find the symbol file named on the command line, or search recursively for all symbol files.</item>
	/// <item>Run SrcTool.exe on the symbol file, and remove any source files that do not have the same root as the current directory.</item>
	/// <item>Get the #have revision for all the files from Perforce.</item>
	/// <item>Create a source server stream using the local path, depot path, and revision number. <a href="http://msdn.microsoft.com/en-us/library/ms680641(vs.85).aspx">Details</a>.</item>
	/// <item>Inject the stream into the symbol file.</item>
	/// <item>Repeat for all symbol files.</item>
	/// </list>
	/// </remarks>
	public class SourceServerIndexer
	{
		/// <summary>Full validated path of SrcTool.exe</summary>
		public static string SrcToolLocation = "";
		/// <summary>Full validated path of PdbStr.exe</summary>
		public static string PdbStrLocation = "";
		/// <summary>The name of the symbol file to index.</summary>
		public static string SymbolFileName = "";
		/// <summary>The number of symbol files successfully indexed.</summary>
		public static int SuccessfulIndexings = 0;

		/// <summary>Validates an environment variable exists, and the location it points to also exists.</summary>
		/// <param name="EnvironmentVariable">The name of the environment variable to check.</param>
		/// <returns>The full directory name the environment variable is pointing to.</returns>
		private static string GetEnvironmentVariable( string EnvironmentVariable )
		{
			string VariableValue = Environment.GetEnvironmentVariable( EnvironmentVariable );
			if( VariableValue != null )
			{
				DirectoryInfo ValueLocation = new DirectoryInfo( VariableValue );
				if( ValueLocation.Exists )
				{
					return ValueLocation.FullName;
				}
				else
				{
					ConsoleLogger.Error( " ... environment variable location does not exist: " + ValueLocation.FullName );
				}
			}
			else
			{
				ConsoleLogger.Error( " ... could not find environment variable: " + EnvironmentVariable );
			}

			return null;
		}

		/// <summary>Validate the required environment for indexing symbol files with source server information</summary>
		/// <returns>True if all the folders and tools are found, false otherwise.</returns>
		/// <remarks>This validates the WindowsSdkDir environment variable points to an existing folder (e.g. '<c>C:\Program Files (x86)\Windows Kits\8.0</c>'), and in that folder
		/// there exists the subfolder '<c>Debuggers\x64\srcsrv</c>'. It then ensures the binary files SrcTool.exe and PdbStr.exe exist there. No version checking is performed.</remarks>
		private static bool ValidateEnvironment()
		{
			string WindowsSdkDir = GetEnvironmentVariable( "WindowsSdkDir" );
			if( WindowsSdkDir == null )
			{
				return false;
			}

			string DebuggingFilesLocation = Path.Combine( WindowsSdkDir, "Debuggers", "x64", "srcsrv" );
			DirectoryInfo DebuggingFilesInfo = new DirectoryInfo( DebuggingFilesLocation );
			if( !DebuggingFilesInfo.Exists )
			{
				ConsoleLogger.Error( " ... could not find source server support folder: " + DebuggingFilesInfo.FullName );
				return false;
			}

			SrcToolLocation = Path.Combine( DebuggingFilesInfo.FullName, "srctool.exe" );
			FileInfo SrcToolInfo = new FileInfo( SrcToolLocation );
			if( !SrcToolInfo.Exists )
			{
				ConsoleLogger.Error( " ... could not find SrcTool.exe in support folder: " + DebuggingFilesInfo.FullName );
				return false;
			}
			SrcToolLocation = SrcToolInfo.FullName;

			PdbStrLocation = Path.Combine( DebuggingFilesInfo.FullName, "pdbstr.exe" );
			FileInfo PdbStrInfo = new FileInfo( PdbStrLocation );
			if( !PdbStrInfo.Exists )
			{
				ConsoleLogger.Error( " ... could not find PdbStr.exe in support folder: " + DebuggingFilesInfo.FullName );
				return false;
			}
			PdbStrLocation = PdbStrInfo.FullName;

			return true;
		}

		/// <summary>Handle any command line arguments.</summary>
		/// <param name="Arguments">The comment line arguments.</param>
		/// <returns>True to continue execution, false to exit.</returns>
		/// <remarks>The currently supported arguments are '-h' to display command line help, '-v' for verbose logging,
		/// and anything else will be treated as a symbol file name to process.</remarks>
		private static bool ParseArguments( string[] Arguments )
		{
			foreach( string Argument in Arguments )
			{
				switch( Argument.ToLower() )
				{
				case "-v":
					ConsoleLogger.VerboseLogs = true;
					break;

				case "-h":
					ConsoleLogger.Log( "" );
					ConsoleLogger.Log( "Usage: SourceServerIndexer.exe [-h] [-v] [SymbolFileName]" );
					ConsoleLogger.Log( "" );
					ConsoleLogger.Log( " -h - displays this help." );
					ConsoleLogger.Log( " -v - displays verbose logging." );
					ConsoleLogger.Log( "" );
					ConsoleLogger.Log( "Indexes the named symbol file, or indexes all symbol files in the current folder or lower if no symbol file is named." );
					ConsoleLogger.Log( "" );
					return false;

				default:
					SymbolFileName = Argument;
					break;
				}
			}

			return true;
		}

		/// <summary>A generic catch all for all unhandled exceptions.</summary>
		/// <param name="Sender">The object that created the exception.</param>
		/// <param name="Arguments">Details about the exception.</param>
		private static void GenericExceptionHandler( object Sender, UnhandledExceptionEventArgs Arguments )
		{
			ConsoleLogger.Error( "Unhandled exception: " + Arguments.ExceptionObject );
			Environment.Exit( -1 );
		}

		/// <summary>Indexes symbol files with source server information.</summary>
		/// <param name="Arguments">The comment line arguments.</param>
		private static void Main( string[] Arguments )
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( GenericExceptionHandler );
			DateTime StartTime = DateTime.UtcNow;

			ConsoleLogger.Title( "SourceServerIndexer - Copyright 2014 Eternal Developments LLC." );
			ConsoleLogger.Title( "Indexes pdb files to allow source debugging of minidumps." );

			// Handle any command line arguments
			if( !ParseArguments( Arguments ) )
			{
				return;
			}

			// Make sure the required folders and tools are installed
			if( !ValidateEnvironment() )
			{
				ConsoleLogger.Error( "... Failed to validate environment." );
				return;
			}

			ConsoleLogger.Log( "... running from: " + Environment.CurrentDirectory );

			// Recursively find all symbol files
			List<string> SymbolFiles = Pdb.GetSymbolFiles();
			ConsoleLogger.Log( "" );

			// Iterate over each symbol file and index it
			foreach( string SymbolFile in SymbolFiles )
			{
				// Extract all the non system source files referenced by the symbol files
				List<string> LocalSourceFiles = Pdb.GetReferencedSourceFiles( SymbolFile );

				if( LocalSourceFiles.Count > 0 )
				{
					// Retrieve the current local version of each source file from Perforce
					List<FileSpec> SourceFiles = Perforce.GetHaveRevisions( LocalSourceFiles );

					// Create the source server stream to inject
					Pdb.CreateSourceServerStream( SymbolFile, SourceFiles );

					// Inject the source server stream into the symbol file
					Pdb.InjectSourceServerStream( SymbolFile );
				}

				ConsoleLogger.Log( "" );
			}

			TimeSpan Duration = DateTime.UtcNow - StartTime;
			ConsoleLogger.Log( SuccessfulIndexings + " symbol files successfully indexed in " + Duration.TotalSeconds.ToString( "F2" ) + " seconds." );
		}
	}
}
