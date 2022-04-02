// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace ResilientP4
{
	/// <summary>
	///     A class to run the main loop and handle any application level requirements.
	/// </summary>
	public static class Program
	{
		/// <summary>
		/// Load a third party assembly that contains native code.
		/// </summary>
		/// <param name="Assemblyx86Path"></param>
		/// <param name="Assemblyx64Path"></param>
		/// <returns></returns>
		private static Assembly GetThirdPartyNativeAssembly( string Assemblyx86Path, string Assemblyx64Path )
		{
			Assembly ResolvedAssembly = null;
			string AssemblyName = "";
			string ThirdPartyFolder = "..\\..\\..\\ThirdParty";
			if( Environment.Is64BitProcess )
			{
				AssemblyName = Path.GetFullPath( Path.Combine( ThirdPartyFolder, Assemblyx64Path ) );
			}
			else
			{
				AssemblyName = Path.GetFullPath( Path.Combine( ThirdPartyFolder, Assemblyx86Path ) );
			}

			Debug.WriteLine( "Loading assembly: " + AssemblyName );
			if( File.Exists( AssemblyName ) )
			{
				ResolvedAssembly = Assembly.LoadFile( AssemblyName );
			}

			return ResolvedAssembly;
		}

		/// <summary>
		/// Load a third party assembly that is pure managed code.
		/// </summary>
		/// <param name="AssemblyPath"></param>
		/// <returns></returns>
		private static Assembly GetThirdPartyManagedAssembly( string AssemblyPath )
		{
			Assembly ResolvedAssembly = null;
			string AssemblyName = Path.GetFullPath( Path.Combine( "..\\..\\..\\ThirdParty", AssemblyPath ) );

			Debug.WriteLine( "Loading assembly: " + AssemblyName );
			if( File.Exists( AssemblyName ) )
			{
				ResolvedAssembly = Assembly.LoadFile( AssemblyName );
			}

			return ResolvedAssembly;
		}

		/// <summary>
		///     Resolve any unfound assemblies based on the bitness of the application.
		/// </summary>
		/// <param name="Sender">Unused.</param>
		/// <param name="Arguments">Fully qualified name of assembly to resolve.</param>
		/// <returns>Resolved assembly, or null if the assembly was not found.</returns>
		private static Assembly ResolveEventHandler( object Sender, ResolveEventArgs Arguments )
		{
			// Name is the fully qualified assembly name - e.g. "p4api.net, Version=2013.2.69.1914, Culture=neutral, PublicKeyToken=f6b9b9d036c873e1"
			Assembly ResolvedAssembly = null;
			string[] AssemblyInfo = Arguments.Name.Split( ",".ToCharArray() );
			string AssemblyName = AssemblyInfo[0];

			if( AssemblyName.ToUpperInvariant() == "P4API.NET" )
			{
				ResolvedAssembly = GetThirdPartyNativeAssembly( "p4api.net\\13.3\\Win32\\lib\\p4api.net.dll", "p4api.net\\13.3\\Win64\\lib\\p4api.net.dll" );
			}
			else if( AssemblyName.ToUpperInvariant() == "IONIC.ZIP.REDUCED" )
			{
				ResolvedAssembly = GetThirdPartyManagedAssembly( "DotNetZip\\1.9.1.8\\Zip Reduced\\bin\\Release\\Ionic.Zip.Reduced.dll" );
			}

			return ResolvedAssembly;
		}

		/// <summary>
		/// </summary>
		/// <param name="Sender"></param>
		/// <param name="Arguments"></param>
		private static void ApplicationExceptionHandler( object Sender, UnhandledExceptionEventArgs Arguments )
		{
			Exception Ex = ( Exception )Arguments.ExceptionObject;
		}

		/// <summary>
		///     The main entry point for the application.
		/// </summary>
		[STAThread]
		private static void Main()
		{
			AppDomain.CurrentDomain.AssemblyResolve += ResolveEventHandler;
			AppDomain.CurrentDomain.UnhandledException += ApplicationExceptionHandler;

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );

			Application.CurrentCulture = CultureInfo.InvariantCulture;

			using( MainForm Host = new MainForm() )
			{
				Host.Initialize();

				while( Host.Running )
				{
					Host.Tick();

					Application.DoEvents();
					Thread.Sleep( 10 );
				}

				Host.Shutdown();
			}
		}
	}
}
