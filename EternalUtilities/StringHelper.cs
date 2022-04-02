// Copyright 2015 Eternal Developments LLC. All Rights Reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Eternal.EternalUtilities
{
	/// <summary>
	/// 
	/// </summary>
	public static class StringHelper
	{
		/// <summary>Returns a timestamp string consistent for all messaging.</summary>
		/// <returns>Returns a timestamp string in local time.</returns>
		public static string ISOTimestamp
		{
			get
			{
				return DateTime.Now.ToString( "HH:mm:ss", CultureInfo.InvariantCulture ) + ": ";
			}
		}

		/// <summary>Returns a friendly string that represents an amount of memory.</summary>
		/// <param name="MemorySize">The number of bytes to create a string for.</param>
		/// <returns>A string with a human readable representation of a memory size (e.g. '10 kB' or '5.0 MB'</returns>
		public static string GetMemoryString( long MemorySize )
		{
			const long Gigabyte = 1024 * 1024 * 1024;
			const long Megabyte = 1024 * 1024;
			const long Kilobyte = 1024;

			if( MemorySize > Gigabyte )
			{
				float Gigabytes = ( float )MemorySize / Gigabyte;
				return Gigabytes.ToString( "f2", CultureInfo.InvariantCulture ) + " GB";
			}
			else if( MemorySize > Megabyte )
			{
				float Megabytes = ( float )MemorySize / Megabyte;
				return Megabytes.ToString( "f2", CultureInfo.InvariantCulture ) + " MB";
			}
			else if( MemorySize > Kilobyte )
			{
				float Kilobytes = ( float )MemorySize / Kilobyte;
				return Kilobytes.ToString( "f2", CultureInfo.InvariantCulture ) + " kB";
			}
			else
			{
				return MemorySize + " bytes";
			}
		}
	}
}
