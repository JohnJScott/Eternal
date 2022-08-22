// Copyright 2015-2022 Eternal Developments LLC. All Rights Reserved.

using System.Diagnostics;

namespace Eternal.ConsoleUtilities
{
	/// <summary>A class to encapsulate the spawning of a console process.</summary>
	public class ConsoleProcess : IDisposable
	{
		/// <summary>The delegate type to return stdout and stderr to the owning process.</summary>
		public delegate void CaptureDelegate( string line );

		/// <summary>The optional delegate to pass output back to the owning process.</summary>
		private readonly CaptureDelegate? CaptureOutputDelegate;

		/// <summary>The console process that is spawned.</summary>
		private readonly Process SpawnedProcess;

		/// <summary>The error code of the spawned process.</summary>
		/// <remarks>This returns a negative number on error, 0 on successfully spawned, and a positive return code on completion.</remarks>
		public int ExitCode;

		/// <summary>A constructor to create and spawn a console process.</summary>
		/// <param name="executableName">The name of the executable to spawn.</param>
		/// <param name="workingDirectory">The directory to spawn the process in.</param>
		/// <param name="captureOutput">The optional delegate to capture stdout and stderr.</param>
		/// <param name="arguments">The command line to use.</param>
		public ConsoleProcess( string executableName, string workingDirectory, CaptureDelegate? captureOutput, params string[] arguments )
		{
			ErrorString = "OK";
			SpawnedProcess = new Process();

			FileInfo executable_info = new FileInfo( executableName );
			if( !executable_info.Exists )
			{
				ExitCode = -2;
				ErrorString = "Executable file " + executable_info.FullName + " does not exist!";
				return;
			}

			DirectoryInfo working_directory_info = new DirectoryInfo( workingDirectory );
			if( !working_directory_info.Exists )
			{
				ExitCode = -3;
				ErrorString = "Working directory " + working_directory_info.FullName + " does not exist!";
				return;
			}

			try
			{
				SpawnedProcess.StartInfo.FileName = executable_info.FullName;
				SpawnedProcess.StartInfo.WorkingDirectory = working_directory_info.FullName;
				SpawnedProcess.StartInfo.Arguments = String.Join( " ", arguments );
#if !DEBUG
				SpawnedProcess.StartInfo.CreateNoWindow = true;
#endif
				if( captureOutput != null )
				{
					CaptureOutputDelegate = captureOutput;

					SpawnedProcess.StartInfo.RedirectStandardOutput = true;
					SpawnedProcess.StartInfo.RedirectStandardError = true;
					SpawnedProcess.StartInfo.UseShellExecute = false;

					SpawnedProcess.ErrorDataReceived += ProcessOutputHandler;
					SpawnedProcess.OutputDataReceived += ProcessOutputHandler;

					SpawnedProcess.EnableRaisingEvents = true;
				}

				ConsoleLogger.Verbose( "Spawning: " + SpawnedProcess.StartInfo.FileName + " " + SpawnedProcess.StartInfo.Arguments + " (" + SpawnedProcess.StartInfo.WorkingDirectory + ")" );

				SpawnedProcess.Start();

				if( captureOutput != null )
				{
					SpawnedProcess.BeginOutputReadLine();
					SpawnedProcess.BeginErrorReadLine();
				}
			}
			catch( Exception exception )
			{
				ExitCode = -1;
				ErrorString = "Exception while spawning '" + executable_info.FullName + "' " + exception;
				ConsoleLogger.Error( ErrorString );
			}
		}

		/// <summary>A detailed description of any errors.</summary>
		public string ErrorString
		{
			get;
			set;
		}

		/// <summary>Implementing Dispose as recommended by code analysis.</summary>
		public void Dispose()
		{
			Dispose( true );
			GC.SuppressFinalize( this );
		}

		/// <summary>Implementing Dispose as recommended by code analysis.</summary>
		/// <param name="isDisposing"></param>
		protected virtual void Dispose( bool isDisposing )
		{
			SpawnedProcess?.Dispose();
		}

		/// <summary>The system callback to process the optional owning process callback.</summary>
		/// <param name="sendingProcess">The process that spawned the data to capture.</param>
		/// <param name="outLine">The data that is being captured.</param>
		private void ProcessOutputHandler( object sendingProcess, DataReceivedEventArgs outLine )
		{
			if( CaptureOutputDelegate != null && outLine.Data != null )
			{
				CaptureOutputDelegate( outLine.Data );
			}
		}

		/// <summary>Wait for the process to complete, or the timeout to pass.</summary>
		/// <param name="timeout">The number of milliseconds to wait for completion.</param>
		/// <returns>The exit code of the process.</returns>
		public int Wait( int timeout = Int32.MaxValue )
		{
			SpawnedProcess.WaitForExit( timeout );

			// Make sure all output is flushed properly
			if( CaptureOutputDelegate != null )
			{
				SpawnedProcess.CancelErrorRead();
				SpawnedProcess.CancelOutputRead();
			}

			if( SpawnedProcess.HasExited )
			{
				ExitCode = SpawnedProcess.ExitCode;
			}
			else
			{
				ExitCode = -258;
				ErrorString = "The process did not complete before the timeout expired.";
				SpawnedProcess?.Kill();
			}

			return ExitCode;
		}
	}
}
