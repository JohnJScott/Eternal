using System;
using System.Collections.Generic;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Describes the path and path type in a file spec.
	/// </summary>
	public abstract class PathSpec
	{
		protected PathSpec() { }

		public PathSpec(string path)
		{
			Path = path;
		}
		private string _path;

		public string Path
		{ get; protected set; }

		public override string ToString()
		{
			return Path;
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{ return false; }
			if (obj is PathSpec)
			{
				PathSpec o = obj as PathSpec;
				return ((this.GetType() == o.GetType()) &&
					(this.Path == o.Path));
			}
			return false;
		}

		public override int GetHashCode()
		{
			return Path.GetHashCode();
		}

		protected string NormalizeDepot(string path)
		{
		    string val = "";
            if (path != null)
            {
                val = path.Trim('"');
            }
			return val;
		}

		protected string NormalizeLocal(string path)
		{
			string val = "";
			if (path != null)
			{
				val = path.Trim('"', ' ');
				val = val.Replace('/', '\\');
			}		
			return val;
		}


		private static string GetFileName(string path)
		{
			String name = path;
			if (!String.IsNullOrEmpty(path))
			{
				int idx = path.LastIndexOf('/');
				if (idx >= 0)
				{
					name = path.Substring(idx + 1);
				}
				else
				{
					idx = path.LastIndexOf('\\');
					if (idx >= 0)
					{
						name = path.Substring(idx + 1);
					}
				}
			}
			return name;
		}

		private static string GetDirectoryName(string path)
		{
			String name = path;
			if (!String.IsNullOrEmpty(path))
			{
				int idx = path.LastIndexOf('/');
				if (idx >= 0)
				{
					name = path.Substring(0, idx);
				}
				else
				{
					idx = path.LastIndexOf('\\');
					if (idx >= 0)
					{
						name = path.Substring(0, idx);
					}
				}
			}
			return name;
		}


		public string GetFileName()
		{
			return GetFileName(Path);
		}

		public string GetDirectoryName()
		{
			return GetDirectoryName(Path);
		}

		public static implicit operator FileSpec(PathSpec s) 
		{
			return new FileSpec(s,null);
		}

		/// <summary>
		/// Unescape a string.
		/// </summary>
		/// <param name="Path">Paths to unescape</param>
		public static string UnescapePath(string p)
		{
            if (p==null)
            {
                return p;   
            }
			string v = p.Replace("%2A", "*");
			v = v.Replace("%23", "#");
			v = v.Replace("%40", "@");
			v = v.Replace("%25", "%");
			return v;
		}

		/// <summary>
		/// Escape a string.
		/// </summary>
		/// <param name="Path">Path to escape</param>
		public static string EscapePath(string p)
		{
            if (p == null)
            {
                return p;
            }
			string v = p.Replace("%", "%25");
			v = v.Replace("#", "%23");
			v = v.Replace("@", "%40");
			//v = v.Replace("*", "%2A");
			return v;
		}

		/// <summary>
		/// Escape a list of strings.
		/// </summary>
		/// <param name="Paths">Path to escape</param>
		public static IList<string> EscapePaths(IList<string> Paths)
		{
			List<string> v = new List<string>();
			foreach (string p in Paths)
			{
				v.Add(EscapePath(p));
			}
			return v;
		}

		/// <summary>
		/// Escape an array of strings.
		/// </summary>
		/// <param name="Paths">Paths to escape</param>
		public static string[] EscapePaths(string[] Paths)
		{
			string[] v = new string[Paths.Length];
			for (int idx = 0; idx < Paths.Length; idx++)
			{
				v[idx] = EscapePath(Paths[idx]);
			}
			return v;
		}


		/// <summary>
		/// Unescape a list of strings.
		/// </summary>
		/// <param name="p4Server">Perforce server</param>
		public static IList<string> UnescapePaths(IList<string> Paths)
		{
			List<string> v = new List<string>();
			foreach (string p in Paths)
			{
				v.Add(UnescapePath(p));
			}
			return v;
		}

		/// <summary>
		/// Unescape an array of strings.
		/// </summary>
		/// <param name="Paths">Paths to unescape</param>
		public static string[] UnescapePaths(string[] Paths)
		{
			string[] v = new string[Paths.Length];
			for (int idx = 0; idx < Paths.Length; idx++)
			{
				v[idx] = UnescapePath(Paths[idx]);
			}
			return v;
		}
	}

	/// <summary>
	/// A path spec in depot syntax. 
	/// </summary>
	public class DepotPath : PathSpec
	{
		public DepotPath(string path)
		{
			Path = NormalizeDepot(path);
		}
		public override bool Equals(object obj) { return base.Equals(obj); }
	}

	/// <summary>
	/// A path spec in client syntax. 
	/// </summary>
	public class ClientPath : PathSpec
	{
		public ClientPath(string path)
		{
			Path = NormalizeDepot(path);
		}
		public override bool Equals(object obj) { return base.Equals(obj); }
	}

	/// <summary>
	/// A path spec in local syntax. 
	/// </summary>
	public class LocalPath : PathSpec
	{
		public LocalPath(string path)
		{
			Path = NormalizeLocal(path);
		}
		public override bool Equals(object obj) { return base.Equals(obj); }
	}

}
