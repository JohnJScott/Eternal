using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// The protection mode or rights associated with this entry. 
	/// </summary>
	public enum ProtectionMode
	{
		List, Read, Open, Write, Admin, Super, Review, ReadRights,
		BranchRights, OpenRights, WriteRights
	}

	/// <summary>
	/// The type of protection (user or group). 
	/// </summary>
	public enum EntryType
	{ User, Group }

	/// <summary>
	/// Describes a protection entry (line) in a Perforce protection table. 
	/// </summary>
	public class ProtectionEntry
	{
		public ProtectionEntry(ProtectionMode mode, EntryType type, string name, string host, string path)
		{
			Mode = mode;
			Type = type;
			Name = name;
			Host = host;
			Path = path;
		}
		public ProtectionMode Mode { get; set; }
		public EntryType Type { get; set; }
		public string Name { get; set; }
		public string Host { get; set; }
		public string Path { get; set; }
	}
}
