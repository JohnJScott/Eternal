using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Specifies resource access privileges for Perforce users for a specific
	/// Perforce repository. 
	/// </summary>
	public class ProtectionTable : List<ProtectionEntry>
	{
		public ProtectionTable 
			(
			ProtectionEntry entry
			)
		{Entry = entry;}

		public ProtectionEntry Entry { get; set; }
	}
}
