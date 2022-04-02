using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Describes the mapping type.
	/// </summary>
	public enum MapType { None, Include, Exclude, Overlay, Share, Isolate, Import, StreamPathExclude }

	/// <summary>
	/// A single entry in a view map.
	/// </summary>
	public class MapEntry
	{
		public MapEntry(/*int order,*/ MapType type, PathSpec left, PathSpec right)
		{
			//Order = order;
			Type = type;
			Left = left;
			Right = right;
		}

//		public int Order { get; set; }
		public MapType Type { get; set; }
		public PathSpec Left { get; set; }
		public PathSpec Right { get; set; }

		public override bool Equals(object obj)
		{
			if (obj is MapEntry)
			{
				MapEntry o = obj as MapEntry;
				if (o.Right != null)
				{
					return ((this.Type == o.Type) &&
						(this.Left.Equals(o.Left)) &&
						(this.Right.Equals(o.Right)));
				}
				else
				{
					return ((this.Type == o.Type) &&
						(this.Left.Equals(o.Left)));
				}
			}
			return false;
		}
	}

	/// <summary>
	/// Maps one or more Perforce file specs to zero, one, or more
	/// other Perforce file specs.
	/// </summary>
	public class ViewMap : List<MapEntry>
	{
		/// <summary>
		/// Create an empty workspace view
		/// </summary>
		/// <param name="pserver"></param>
		public ViewMap()
		{
		}

		/// <summary>
		/// Create a workspace view
		/// </summary>
		/// <param name="text">List of Left/Right pairs for the map</param>
		public ViewMap(List<string> text)
		{
			foreach (string line in text)
			{
				Add(line);
			}
		}
		/// <summary>
		/// Create a workspace view
		/// </summary>
		/// <param name="text">Array of Left/Right pairs for the map</param>
		public ViewMap(string[] text)
		{
			foreach (string line in text)
			{
				Add(line);
			}
		}
		/// <summary>
		/// Split a single line from a view specification into left and right parts
		/// </summary>
		/// <remarks>
		/// Handles case where the left and/or the right string are enclosed 
		/// in quotes because they contain spaces. I.e "C:\my code\project 1\..."
		/// </remarks>
		/// <param name="line"></param>
		/// <returns></returns>
		public static String[] SplitViewLine(String line)
		{ 
			//Get rid of leading/trailing white spaces
			line = line.Trim(); 
			

			String[] parts = new String[2];
			int idx =0;
			int start=0;
			int cnt=0;

			//Determine if this is a Stream path and remove the path type

			if (line.StartsWith("share") ||
				line.StartsWith("isolate") ||
				line.StartsWith("import") ||
				line.StartsWith("exclude"))
			{
                line=line.Remove(0,line.IndexOf(" ")+1);
				String[] streamLine = line.Split(' ');
				if (streamLine.Length==1)
				{
                    line = streamLine[0];
				}
			}

			if (line.Length > 0)
			{
				if (line[idx] == '"')
				{
					//Left side is quoted, skip to the next quote
					start = ++idx;
                    while (idx < line.Length && line[idx] != '"')
                    {
                        idx++;
                        cnt = idx - start;
                    }
				}
				else
				{
					if (line.Contains(' ') == false)
					{
						parts[0] = line;
						parts[1] = string.Empty;
						return parts;
					}
					{
						// not quoted, so skip to the next white space
						start = idx;
                        while (!Char.IsWhiteSpace(line[idx]))
                        {
                            idx++;
                            cnt = idx - start;
                        }
					}
				}
			}
			parts[0] = line.Substring(start, cnt).TrimStart('-','+');

			if (line.Length > 0)
			{
				// skip the separating white spaces
				while (Char.IsWhiteSpace(line[idx]))
					idx++;

				// rest (trimmed of quotes if any)  of line is the right value
				parts[1] = line.Substring(idx).Trim('"', ' ');
			}

			return parts;
		}

		/// <summary>
		/// Add a line to the end of the view
		/// </summary>
		/// <param name="line">Left/Right pair for the map</param>
		public void Add(String line)
		{
			MapType lineType = MapType.Include;
			if (line.Length > 0)
			{
				if ((line[0] == '-') || (line.StartsWith("\"-")))
				{
					lineType = MapType.Exclude;
				}
				else if ((line[0] == '+') || (line.StartsWith("\"+")))
				{
					lineType = MapType.Overlay;
				}
				else if (line.StartsWith("share"))
				{
					lineType = MapType.Share;
				}
				else if (line.StartsWith("isolate"))
				{
					lineType = MapType.Isolate;
				}
				else if (line.StartsWith("import"))
				{
					lineType = MapType.Import;
				}
				else if (line.StartsWith("exclude"))
				{
					lineType = MapType.StreamPathExclude;
				}
			}
			
			String[] sides = SplitViewLine(line.ToString());
			Add(sides[0], sides[1], lineType);
		}

		/// <summary>
		/// Add a line to the end of the view
		/// </summary>
		/// <param name="left">left side of mapping</param>
		/// <param name="right">right side of mapping</param>
		/// <param name="lineType"></param>
		public void Add(String left, String right, MapType lineType)
		{
			MapEntry entry = new MapEntry(
//				this.Count, 
				lineType,
				new DepotPath(left),
				new ClientPath(right));

			Add(entry);
		}

		/// <summary>
		/// Convert to a Perforce server compatible string for a workspace spec
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{
			String value = String.Empty;
			string right;
			//bool streamPath = true;

			for (int idx = 0; idx < Count; idx++)
			{
				MapEntry entry = this[idx];
				string left = entry.Left.Path;
				
				if (entry.Type == MapType.Exclude)
				{
					if (left.Contains(' '))
					{
						left = String.Format("\"-{0}\"", left);
					}
					else
					left = String.Format("-{0}", left);
				}
				else if (entry.Type == MapType.Overlay)
				{
					if (left.Contains(' '))
					{
						left = String.Format("\"+{0}\"", left);
					}
					else
					left = String.Format("+{0}", left);
				}


				else if (entry.Type == MapType.Share)
				{
					if (left.Contains(' '))
					{
						left = String.Format("share \"{0}\"", left);
					}
					else
					left = String.Format("share {0}", left);
				}
				else if (entry.Type == MapType.Isolate)
				{
					if (left.Contains(' '))
					{
						left = String.Format("isolate \"{0}\"", left);
					}
					else
					left = String.Format("isolate {0}", left);
				}
				else if (entry.Type == MapType.Import)
				{
					if (left.Contains(' '))
					{
						left = String.Format("import \"{0}\"", left);
					}
					else
					left = String.Format("import {0}", left);
				}
				else if (entry.Type == MapType.StreamPathExclude)
				{
					if (left.Contains(' '))
					{
						left = String.Format("exclude \"{0}\"", left);
					}
					else
					left = String.Format("exclude {0}", left);
				}
                else if (entry.Type == MapType.Include||entry.Type==MapType.None)
                {
                    if (left.Contains(' '))
                    {
                        left = String.Format("\"{0}\"", left);
                    }
                    else
                        left = String.Format("{0}", left);
                }

				try
				{
					right = entry.Right.Path;
					if (right.Contains(' '))
					{
						right = String.Format("\"{0}\"", right);
					}
				}
				catch
				{
					right = string.Empty;
				}
				value += String.Format("{0} {1}\r\n", left, right);
			}
			return value;
		}
	}
}
		

