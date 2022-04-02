// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Eternal.EternalUtilities;
using Perforce.P4;

namespace Eternal.SourceServerIndexer
{
	/// <summary>Class to handle interaction with symbol files.</summary>
	public class Pdb
	{
		/// <summary>A list of source files returned from the SrcTool process.</summary>
		private static List<string> SourceFiles;

		/// <summary>Get all symbol files from the current folder and any subfolders, or the explicitly named one on the command line.</summary>
		/// <returns>A list of names of discovered symbol files, or the explicitly named symbol file if it was found.</returns>
		public static List<string> GetSymbolFiles()
		{
			List<string> SymbolFiles = new List<string>();

			if( SourceServerIndexer.SymbolFileName.Length > 0 )
			{
				FileInfo SymbolFileInfo = new FileInfo( SourceServerIndexer.SymbolFileName );
				if( SymbolFileInfo.Exists )
				{
					SymbolFiles.Add( SymbolFileInfo.FullName );
					ConsoleLogger.Log( "... found symbol file " + SymbolFileInfo.FullName );
				}
			}
			else
			{
				List<FileInfo> SymbolFileInfos = new DirectoryInfo( "." ).GetFiles( "*.pdb", SearchOption.AllDirectories ).ToList();
				SymbolFileInfos.ForEach( x => SymbolFiles.Add( x.FullName) );
				ConsoleLogger.Log( "... found " + SymbolFileInfos.Count + " symbol files." );
			}

			return SymbolFiles;
		}

		/// <summary>Callback to capture the output of SrcTool. Each line is the name of a source file.</summary>
		/// <param name="Line">Line of text printed by the spawned process (which is the name of a source file).</param>
		private static void CaptureConsoleSpew( string Line )
		{
			if( Line != null )
			{
				SourceFiles.Add( Line );
				ConsoleLogger.Verbose( "... added source file " + Line );
			}
		}

		/// <summary>Get all local source files referenced in the pdb.</summary>
		/// <param name="SymbolFile">The symbol file to extract source file names from.</param>
		/// <returns>A list of local source files referenced by the symbol file.</returns>
		/// <remarks>A local source file is any source file with the same root as the current directory. This excludes system files that are unlikely to be in Perforce.</remarks>
		public static List<string> GetReferencedSourceFiles( string SymbolFile )
		{
			SourceFiles = new List<string>();
			ConsoleProcess SrcToolProcess = new ConsoleProcess( SourceServerIndexer.SrcToolLocation, ".", CaptureConsoleSpew, SymbolFile, "-r" );

			int ExitCode = SrcToolProcess.Wait( 30 * 1000 );
			if( ExitCode <= 0 )
			{
				// No fatal as intermediate pdbs do not have source information
				ConsoleLogger.Warning( "... SrcTool execution failed on: " + SymbolFile );
				ConsoleLogger.Warning( "... SrcTool execution failed with exit code: " + ExitCode );

				// Clear out any partial data that may cause errors
				SourceFiles.Clear();
			}
			else
			{
				ConsoleLogger.Log( "... found " + ExitCode + " source files referenced in " + SymbolFile );
				SourceFiles = SourceFiles.Take( ExitCode ).ToList();

				// Select the files that are local to this folder i.e. exclude system header and source files
				SourceFiles = SourceFiles.Where( x => x.StartsWith( Environment.CurrentDirectory, StringComparison.InvariantCultureIgnoreCase ) ).ToList();
				ConsoleLogger.Log( "... found " + SourceFiles.Count + " local source files in " + SymbolFile );
			}

			return SourceFiles;
		}

		/// <summary>Create a source server info stream to inject into the pdb.</summary>
		/// <param name="SymbolFile">The name of symbol file to index.</param>
		/// <param name="SourceFiles">A list of source files with local path, depot path and revision number.</param>
		public static void CreateSourceServerStream( string SymbolFile, List<FileSpec> SourceFiles )
		{
			string SourceStream = Path.ChangeExtension( SymbolFile, ".SourceServerTemp" );
			ConsoleLogger.Log( "... creating " + SourceStream + " with " + SourceFiles.Count + " files" );

			StreamWriter SourceServerStream = new StreamWriter( SourceStream );

			// Details on these fields http://msdn.microsoft.com/en-us/library/ms680641(vs.85).aspx
			SourceServerStream.WriteLine( "SRCSRV: ini ------------------------------------------------" );
			SourceServerStream.WriteLine( "VERSION=1" );
			SourceServerStream.WriteLine( "INDEXVERSION=2" );
			SourceServerStream.WriteLine( "VERCTRL=Perforce" );
			SourceServerStream.WriteLine( "SRCSRV: variables ------------------------------------------" );
			SourceServerStream.WriteLine( "SRCSRVTRG=%TARG%\\%VAR2%\\%fnbksl%(%VAR3%)\\%VAR4%\\%fnfile%(%VAR1%)" );
			SourceServerStream.WriteLine( "SRCSRVCMD=p4.exe -p %VAR2%:" + Properties.Settings.Default.PerforceServerPort + " print -o %SRCSRVTRG% -q \"//%VAR3%#%VAR4%\"" );
			SourceServerStream.WriteLine( "SRCSRV: source files ---------------------------------------" );

			foreach( FileSpec SourceFile in SourceFiles )
			{
				// Create an * delimited list of arguments
				// VAR1 = local file path
				// VAR2 = server name
				// VAR3 = depot path
				// VAR4 = revision number
				SourceServerStream.WriteLine( SourceFile.LocalPath + "*" + Properties.Settings.Default.PerforceServerName + "*" + SourceFile.DepotPath.Path.TrimStart( '/' ) + "*" + SourceFile.Version.ToString().TrimStart( '#' ) );
			}

			SourceServerStream.WriteLine( "SRCSRV: end ------------------------------------------------" );
			SourceServerStream.Close();
		}

		/// <summary>Callback to capture the output of PdbStr.</summary>
		/// <param name="Line">The line of text output by the spawned process.</param>
		private static void CaptureOutput( string Line )
		{
			if( Line != null )
			{
				ConsoleLogger.Log( Line );
			}
		}

		/// <summary>Inject the stream containing source server information into the pdb.</summary>
		/// <param name="SymbolFile">The name of symbol file to index.</param>
		public static void InjectSourceServerStream( string SymbolFile )
		{
			string SourceStream = Path.ChangeExtension( SymbolFile, ".SourceServerTemp" );
			ConsoleLogger.Log( "... injecting " + SourceStream + " into " + SymbolFile );

			// Spawn the process that injects the source server stream into the pdb.
			string SymbolFileParameter = "-p:\"" + SymbolFile + "\"";
			string SourceStreamParameter = "-i:\"" + SourceStream + "\"";
			ConsoleProcess PdbStrProcess = new ConsoleProcess( SourceServerIndexer.PdbStrLocation, ".", CaptureOutput, "-w", "-s:srcsrv", SymbolFileParameter, SourceStreamParameter );
			int ExitCode = PdbStrProcess.Wait( 30 * 1000 );
			if( ExitCode < 0 )
			{
				ConsoleLogger.Error( "Failed to inject SourceServerStream into pdb for symbol file " + SymbolFile +	" with error code " + ExitCode );
			}
			else
			{
				ConsoleLogger.Log( "... source indexing successful!" );
				SourceServerIndexer.SuccessfulIndexings++;
			}

#if !DEBUG
			// Delete the temporary file
			FileInfo SourceStreamInfo = new FileInfo( SourceStream );
			if( SourceStreamInfo.Exists )
			{
				SourceStreamInfo.Delete();
			}
#endif
		}
	}
}
