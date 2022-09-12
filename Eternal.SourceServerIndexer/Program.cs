// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;
using Microsoft.Win32;

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
	/// <item>Get the have revision for all the files from Perforce.</item>
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

		/// <summary>Validate the required environment for indexing symbol files with source server information</summary>
		/// <returns>True if all the folders and tools are found, false otherwise.</returns>
		/// <remarks>This validates the WindowsSdkDir environment variable points to an existing folder (e.g. '<c>C:/Program Files (x86)/Windows Kits/8.0</c>'), and in that folder
		/// there exists the subfolder '<c>Debuggers/x64/srcsrv</c>'. It then ensures the binary files SrcTool.exe and PdbStr.exe exist there. No version checking is performed.</remarks>
		public static bool ValidateEnvironment()
		{
			RegistryKey? sub_key = Registry.LocalMachine.OpenSubKey( "SOFTWARE\\WOW6432Node\\Microsoft\\Microsoft SDKs\\Windows\\v10.0" );
			string windows_sdk_dir = sub_key?.GetValue( "InstallationFolder" )?.ToString() ?? string.Empty;
			if( windows_sdk_dir.Length == 0 )
			{
				return false;
			}

			string debugging_files_location = Path.Combine( windows_sdk_dir, "Debuggers", "x64", "srcsrv" );
			DirectoryInfo debugging_files_info = new DirectoryInfo( debugging_files_location );
			if( !debugging_files_info.Exists )
			{
				ConsoleLogger.Error( $" ... could not find source server support folder: {debugging_files_info.FullName}" );
				return false;
			}

			SrcToolLocation = Path.Combine( debugging_files_info.FullName, "srctool.exe" );
			FileInfo src_tool_info = new FileInfo( SrcToolLocation );
			if( !src_tool_info.Exists )
			{
				ConsoleLogger.Error( $" ... could not find SrcTool.exe in support folder: {debugging_files_info.FullName}" );
				return false;
			}

			SrcToolLocation = src_tool_info.FullName;

			PdbStrLocation = Path.Combine( debugging_files_info.FullName, "pdbstr.exe" );
			FileInfo pdb_str_info = new FileInfo( PdbStrLocation );
			if( !pdb_str_info.Exists )
			{
				ConsoleLogger.Error( $" ... could not find PdbStr.exe in support folder: {debugging_files_info.FullName}" );
				return false;
			}

			PdbStrLocation = pdb_str_info.FullName;

			return true;
		}

		/// <summary>Handle any command line arguments.</summary>
		/// <param name="arguments">The comment line arguments.</param>
		/// <returns>True to continue execution, false to exit.</returns>
		/// <remarks>The currently supported arguments are '-h' to display command line help, '-v' for verbose logging,
		/// and anything else will be treated as a symbol file name to process.</remarks>
		private static bool ParseArguments( string[] arguments )
		{
			foreach( string argument in arguments )
			{
				switch( argument.ToLower() )
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
						SymbolFileName = argument;
						break;
				}
			}

			return true;
		}

		/// <summary>A generic catch all for all unhandled exceptions.</summary>
		/// <param name="sender">The object that created the exception.</param>
		/// <param name="arguments">Details about the exception.</param>
		private static void GenericExceptionHandler( object sender, UnhandledExceptionEventArgs arguments )
		{
			ConsoleLogger.Error( $"Unhandled exception: {arguments.ExceptionObject}" );
			Environment.Exit( -1 );
		}

		/// <summary>Indexes symbol files with source server information.</summary>
		/// <param name="arguments">The comment line arguments.</param>
		private static void Main( string[] arguments )
		{
			AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler( GenericExceptionHandler );
			DateTime start_time = DateTime.UtcNow;

			ConsoleLogger.Title( "SourceServerIndexer - Copyright 2022 Eternal Developments LLC." );
			ConsoleLogger.Title( "Indexes pdb files to allow source debugging of minidumps." );

			// Handle any command line arguments
			if( !ParseArguments( arguments ) )
			{
				return;
			}

			// Make sure the required folders and tools are installed
			if( !ValidateEnvironment() )
			{
				ConsoleLogger.Error( "... failed to validate environment." );
				return;
			}

			// Get the root of the current branch
			PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( Directory.GetCurrentDirectory() );
			if( !PerforceUtilities.PerforceUtilities.Connect( connection_info ) )
			{
				ConsoleLogger.Error( $"... failed to connect to Perforce server {connection_info}." );
				return;
			}

			if( connection_info.WorkspaceRoot == String.Empty )
			{
				ConsoleLogger.Error( $"... no workspace root containing {Directory.GetCurrentDirectory()} found on {connection_info}." );
				return;
			}

			ConsoleLogger.Log( $"... running from: {Directory.GetCurrentDirectory()} with client root: {connection_info.WorkspaceRoot}" );

			// Recursively find all symbol files
			List<string> symbol_files = Pdb.GetSymbolFiles();
			ConsoleLogger.Log( "" );

			// Iterate over each symbol file and index it
			foreach( string symbol_file in symbol_files )
			{
				// Extract all the non system source files referenced by the symbol files
				List<string> local_source_files = Pdb.GetReferencedSourceFiles( connection_info.WorkspaceRoot, symbol_file );
				if( local_source_files.Count > 0 )
				{
					// Retrieve the current local version of each source file from Perforce
					List<FileSpec> source_files = Perforce.GetHaveRevisions( connection_info, local_source_files );

					// Create the source server stream to inject
					Pdb.CreateSourceServerStream( connection_info, symbol_file, source_files );

					// Inject the source server stream into the symbol file
					Pdb.InjectSourceServerStream( symbol_file );
				}
			}

			PerforceUtilities.PerforceUtilities.Disconnect( connection_info );
			TimeSpan duration = DateTime.UtcNow - start_time;
			ConsoleLogger.Success( $"{SuccessfulIndexings} symbol files successfully indexed in {duration.TotalSeconds.ToString( "F2" )} seconds." );
		}
	}
}