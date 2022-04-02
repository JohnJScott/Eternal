using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// A general-purpose counter or named variable in a Perforce repository. 
	/// </summary>
	public class Counter
	{
		public Counter
			(
			string name,
			string value
			)
		{
			Name = name;
			Value = value;
		}

		public string Name { get; set; }

		public string Value { get; set; }
	}
}
