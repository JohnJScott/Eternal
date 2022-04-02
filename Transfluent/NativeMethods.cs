// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Runtime.InteropServices;

namespace Transfluent
{
	public class NativeMethods
	{
		private const UInt32 ATTACH_PARENT_PROCESS = 0xffffffff;

		[DllImport( "kernel32.dll" )]
		private static extern bool AttachConsole( UInt32 dwProcessId );

		public static void AttachConsole()
		{
			AttachConsole( ATTACH_PARENT_PROCESS );
		}
	}
}