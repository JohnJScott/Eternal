using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Defines a Perforce repository's default mapping between
	/// file names or locations and file types. 
	/// </summary>
	public class TypeMap : List<TypeMapEntry>
	{
		public TypeMap() { }
		public TypeMap
			(
			TypeMapEntry mapping,
			FormSpec spec
			)
		{
			Mapping = mapping;
			Spec = spec;
		}
		public TypeMapEntry Mapping { get; set; }
		public FormSpec Spec { get; set; }
	}

	/// <summary>
	/// Describes an individual entry in the Perforce repository's typemap.
	/// </summary>
	public class TypeMapEntry
	{
		public TypeMapEntry 
			(
			FileType filetype,
			string path
			)
		{
			FileType = filetype;
			Path = path;
		}
		public TypeMapEntry (string spec)
		{
			Parse(spec);
		}
		public FileType FileType { get; set; }
		public string Path { get; set; }
		public void Parse(string spec)
		{
			int idx = spec.IndexOf(' ');
			string ftstr = spec.Substring(0, idx);
			this.FileType = new FileType(ftstr);
			this.Path = spec.Substring(idx + 1);
		}
		public override string ToString()
		{
			return String.Format("{0} {1}", this.FileType.ToString(), this.Path);
		}
	}

		
}
