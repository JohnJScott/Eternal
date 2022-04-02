// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Diagnostics;
using System.IO;

namespace Eternal.EternalUtilities
{
	/// <summary>A class to encapsulate the spawning of a console process.</summary>
	public class ConsoleProcess : IDisposable
	{
		/// <summary>The delegate type to return stdout and stderr to the owning process.</summary>
		public delegate void CaptureDelegate( string Line );

		/// <summary>The optional delegate to pass output back to the owning process.</summary>
		private readonly CaptureDelegate CaptureOutputDelegate;

		/// <summary>The console process that is spawned.</summary>
		private readonly Process SpawnedProcess;

		/// <summary>The error code of the spawned process.</summary>
		/// <remarks>This returns a negative number on error, 0 on successfully spawned, and a positive return code on completion.</remarks>
		private int ExitCode;

		/// <summary>A constructor to create and spawn a console process.</summary>
		/// <param name="ExecutableName">The name of the executable to spawn.</param>
		/// <param name="WorkingDirectory">The directory to spawn the process in.</param>
		/// <param name="CaptureOutput">The optional delegate to capture stdout and stderr.</param>
		/// <param name="Arguments">The command line to use.</param>
		public ConsoleProcess( string ExecutableName, string WorkingDirectory, CaptureDelegate CaptureOutput, params string[] Arguments )
		{
			ErrorString = "OK";

			FileInfo ExecutableInfo = new FileInfo( ExecutableName );
			if( !ExecutableInfo.Exists )
			{
				ExitCode = -2;
				ErrorString = "Executable file " + ExecutableInfo.FullName + " does not exist!";
				return;
			}

			DirectoryInfo WorkingDirectoryInfo = new DirectoryInfo( WorkingDirectory );
			if( !WorkingDirectoryInfo.Exists )
			{
				ExitCode = -3;
				ErrorString = "Working directory " + WorkingDirectoryInfo.FullName + " does not exist!";
				return;
			}

			try
			{
				SpawnedProcess = new Process();

				SpawnedProcess.StartInfo.FileName = ExecutableInfo.FullName;
				SpawnedProcess.StartInfo.WorkingDirectory = WorkingDirectoryInfo.FullName;
				SpawnedProcess.StartInfo.Arguments = String.Join( " ", Arguments );
#if !DEBUG
				SpawnedProcess.StartInfo.CreateNoWindow = true;
#endif
				if( CaptureOutput != null )
				{
					CaptureOutputDelegate = CaptureOutput;

					SpawnedProcess.StartInfo.RedirectStandardOutput = true;
					SpawnedProcess.StartInfo.RedirectStandardError = true;
					SpawnedProcess.StartInfo.UseShellExecute = false;

					SpawnedProcess.ErrorDataReceived += ProcessOutputHandler;
					SpawnedProcess.OutputDataReceived += ProcessOutputHandler;

					SpawnedProcess.EnableRaisingEvents = true;
				}

				ConsoleLogger.Verbose( "Spawning: " + SpawnedProcess.StartInfo.FileName + " " + SpawnedProcess.StartInfo.Arguments + " (" + SpawnedProcess.StartInfo.WorkingDirectory + ")" );

				SpawnedProcess.Start();

				if( CaptureOutput != null )
				{
					SpawnedProcess.BeginOutputReadLine();
					SpawnedProcess.BeginErrorReadLine();
				}
			}
			catch( Exception Ex )
			{
				ExitCode = -1;
				ErrorString = "Exception while spawning '" + SpawnedProcess.StartInfo.FileName + "' " + Ex;
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
		/// <param name="IsDisposing"></param>
		protected virtual void Dispose( bool IsDisposing )
		{
			SpawnedProcess.Dispose();
		}

		/// <summary>The system callback to process the optional owning process callback.</summary>
		/// <param name="SendingProcess">The process that spawned the data to capture.</param>
		/// <param name="OutLine">The data that is being captured.</param>
		private void ProcessOutputHandler( object SendingProcess, DataReceivedEventArgs OutLine )
		{
			if( CaptureOutputDelegate != null && OutLine.Data != null )
			{
				CaptureOutputDelegate( OutLine.Data );
			}
		}

		/// <summary>Wait for the process to complete, or the timeout to pass.</summary>
		/// <param name="Timeout">The number of milliseconds to wait for completion.</param>
		/// <returns>The exit code of the process.</returns>
		public int Wait( int Timeout )
		{
			SpawnedProcess.WaitForExit( Timeout );
			if( SpawnedProcess.HasExited )
			{
				ExitCode = SpawnedProcess.ExitCode;
			}
			else
			{
				ExitCode = -258;
				ErrorString = "The process did not complete before the timeout expired.";
				SpawnedProcess.Kill();
			}

			// Make sure all output is flushed properly
			if( CaptureOutputDelegate != null )
			{
				SpawnedProcess.CancelErrorRead();
				SpawnedProcess.CancelOutputRead();
			}

			return ExitCode;
		}
	}
}
