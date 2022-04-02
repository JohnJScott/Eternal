// Copyright 2015-2021 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Windows.Forms;

namespace Mandelbrot
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault( false );
			Application.Run( new SimpleWindow() );
		}
	}
}
