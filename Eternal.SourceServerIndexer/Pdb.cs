// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using Eternal.ConsoleUtilities;
using Eternal.PerforceUtilities;
using Perforce.P4;

namespace Eternal.SourceServerIndexer
{
	/// <summary>Class to handle interaction with symbol files.</summary>
	public class Pdb
	{
		/// <summary>A list of source files returned from the SrcTool process.</summary>
		private static List<string> SourceFiles = new List<string>();

		/// <summary>Get all symbol files from the current folder and any subfolders, or the explicitly named one on the command line.</summary>
		/// <returns>A list of names of discovered symbol files, or the explicitly named symbol file if it was found.</returns>
		public static List<string> GetSymbolFiles()
		{
			List<string> symbol_files = new List<string>();

			if( SourceServerIndexer.SymbolFileName.Length > 0 )
			{
				FileInfo symbol_file_info = new FileInfo( SourceServerIndexer.SymbolFileName );
				if( symbol_file_info.Exists )
				{
					symbol_files.Add( symbol_file_info.FullName );
					ConsoleLogger.Log( $"... found symbol file: {symbol_file_info.FullName}" );
				}
			}
			else
			{
				List<FileInfo> symbol_file_infos = new DirectoryInfo( "." ).GetFiles( "*.pdb", SearchOption.AllDirectories ).ToList();
				symbol_file_infos.ForEach( x => symbol_files.Add( x.FullName) );
				ConsoleLogger.Log( $"... found {symbol_file_infos.Count} symbol files." );
			}

			return symbol_files;
		}

		/// <summary>Callback to capture the output of SrcTool. Each line is the name of a source file.</summary>
		/// <param name="line">line of text printed by the spawned process (which is the name of a source file).</param>
		private static void CaptureConsoleSpew( string line )
		{
			SourceFiles.Add( line );
		}

		/// <summary>Get all local source files referenced in the pdb.</summary>
		/// <param name="sourceControlRoot">The root folder of the current workspace.</param>
		/// <param name="symbolFile">The symbol file to extract source file names from.</param>
		/// <returns>A list of local source files referenced by the symbol file.</returns>
		/// <remarks>A local source file is any source file with the same root as the current directory. This excludes system files that are unlikely to be in Perforce.</remarks>
		public static List<string> GetReferencedSourceFiles( string sourceControlRoot, string symbolFile )
		{
			SourceFiles = new List<string>();
			ConsoleProcess src_tool_process = new ConsoleProcess( SourceServerIndexer.SrcToolLocation, ".", CaptureConsoleSpew, symbolFile, "-r" );

			int exit_code = src_tool_process.Wait( 30 * 1000 );
			if( exit_code <= 0 )
			{
				// No fatal as intermediate pdbs do not have source information
				ConsoleLogger.Warning( $"... SrcTool.exe execution failed on: {symbolFile}" );
				ConsoleLogger.Warning( $"... SrcTool.exe execution failed with exit code: {exit_code}" );

				// Clear out any partial data that may cause errors
				SourceFiles.Clear();
			}
			else
			{
				ConsoleLogger.Log( $"... found {exit_code} source files referenced in {symbolFile} ({SourceFiles.Count} lines captured)" );
				SourceFiles = SourceFiles.Take( exit_code ).ToList();

				// Select the files that are local to this folder i.e. exclude system header and source files
				SourceFiles = SourceFiles.Where( x => x.StartsWith( sourceControlRoot, StringComparison.InvariantCultureIgnoreCase ) ).ToList();
				ConsoleLogger.Log( $"... found {SourceFiles.Count} local source files in {symbolFile}" );
			}

			ConsoleLogger.Log( "" );
			return SourceFiles;
		}

		/// <summary>Create a source server info stream to inject into the pdb.</summary>
		/// <param name="connectionInfo">Perforce connection information.</param>
		/// <param name="symbolFile">The name of symbol file to index.</param>
		/// <param name="sourceFiles">A list of source files with local path, depot path and revision number.</param>
		public static void CreateSourceServerStream( PerforceConnectionInfo connectionInfo, string symbolFile, List<FileSpec> sourceFiles )
		{
			string source_stream = Path.ChangeExtension( symbolFile, ".SourceServerTemp" );
			ConsoleLogger.Log( $"... creating {source_stream} with {sourceFiles.Count} files" );

			StreamWriter source_server_stream = new StreamWriter( source_stream );

			// Details on these fields http://msdn.microsoft.com/en-us/library/ms680641(vs.85).aspx
			source_server_stream.WriteLine( "SRCSRV: ini ------------------------------------------------" );
			source_server_stream.WriteLine( "VERSION=1" );
			source_server_stream.WriteLine( "INDEXVERSION=2" );
			source_server_stream.WriteLine( "VERCTRL=Perforce" );
			source_server_stream.WriteLine( $"DATETIME={DateTime.Now:ddd MMM dd HH:mm:ss yyyy}" );
			source_server_stream.WriteLine( "SRCSRV: variables ------------------------------------------" );
			source_server_stream.WriteLine( $"REPOSITORY={connectionInfo.Port}" );
			source_server_stream.WriteLine( "SRCSRVTRG=%TARG%\\%VAR2%\\%fnbksl%(%VAR3%)\\%VAR4%\\%fnfile%(%VAR1%)" );
			source_server_stream.WriteLine( "SRCSRVCMD=p4.exe -p %fnvar%(%VAR2%) print -o %SRCSRVTRG% -q \"//%VAR3%#%VAR4%\"" );
			source_server_stream.WriteLine( "SRCSRV: source files ---------------------------------------" );

			foreach( FileSpec SourceFile in sourceFiles )
			{
				// Create an * delimited list of arguments
				// VAR1 = local file path
				// VAR2 = server name
				// VAR3 = depot path (without the leading /)
				// VAR4 = revision number (without the #)
				source_server_stream.WriteLine( $"{SourceFile.LocalPath}*REPOSITORY*{SourceFile.DepotPath.Path.TrimStart( '/' )}*{SourceFile.Version.ToString().TrimStart( '#' )}" );
			}

			source_server_stream.WriteLine( "SRCSRV: end ------------------------------------------------" );
			source_server_stream.Close();
		}

		/// <summary>Callback to capture the output of PdbStr.</summary>
		/// <param name="line">The line of text output by the spawned process.</param>
		private static void CaptureOutput( string line )
		{
			ConsoleLogger.Log( line );
		}

		/// <summary>Inject the stream containing source server information into the pdb.</summary>
		/// <param name="symbolFile">The name of symbol file to index.</param>
		public static void InjectSourceServerStream( string symbolFile )
		{
			string source_stream = Path.ChangeExtension( symbolFile, ".SourceServerTemp" );
			ConsoleLogger.Log( $"... injecting {source_stream} into {symbolFile}" );

			// Spawn the process that injects the source server stream into the pdb.
			string symbol_file_parameter = $"-p:\"{symbolFile}\"";
			string source_stream_parameter = $"-i:\"{source_stream}\"";
			ConsoleProcess pdb_str_process = new ConsoleProcess( SourceServerIndexer.PdbStrLocation, ".", CaptureOutput, "-w", "-s:srcsrv", symbol_file_parameter, source_stream_parameter );
			int exit_code = pdb_str_process.Wait( 30 * 1000 );
			if( exit_code < 0 )
			{
				ConsoleLogger.Error( $"Failed to inject SourceServerStream into pdb for symbol file {symbolFile} with error code {exit_code}" );
			}
			else
			{
				ConsoleLogger.Log( "... source indexing successful!" );
				SourceServerIndexer.SuccessfulIndexings++;
			}

#if !DEBUG
			// Delete the temporary file
			FileInfo SourceStreamInfo = new FileInfo( source_stream );
			if( SourceStreamInfo.Exists )
			{
				SourceStreamInfo.Delete();
			}
#endif
		}
	}
}
