using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Flags to define the base file type.
	/// </summary>
	[Flags]
	public enum BaseFileType
	{
		/// <summary>
		/// An unspecified base file type.
		/// </summary>
		Unspecified = 0x0000,
		/// <summary>
		/// Client Use: newlines translated.
		/// Server Storage: deltas in RCS format.
		/// </summary>
		Text = 0x0001,
		/// <summary>
		/// Client Use: raw bytes.
		/// Server Storage: compressed binary.
		/// </summary>
		Binary = 0x0002,
		/// <summary>
		/// Client Use: symbolic link.
		/// Server Storage: deltas in RCS format.
		/// </summary>
		Symlink = 0x0004,
		/// <summary>
		/// Client Use: Mac resource + data.
		/// Server Storage: compressed AppleSingle.
		/// </summary>
		Apple = 0x0008,
		/// <summary>
		/// Client Use: Mac resource fork.
		/// Server Storage: compressed binary.
		/// </summary>
		Resource = 0x0010,
		/// <summary>
		/// Client Use: newlines translated.
		/// Server Storage: deltas in RCS format
		/// stored as UTF-8.
		/// </summary>
		Unicode = 0x0020,
		/// <summary>
		/// Client Use: newlines translated
		/// client file UTF-16
		/// Server Storage: deltas in RCS format
		/// stored as UTF-8
		/// 
		/// in client byte order unless BOM overrides
		/// File is always written with a BOM
		/// A BOM is needed for automatic type detection
		/// </summary>
		UTF16 = 0x0040
	}

	/// <summary>
	/// Flags to specify file type modifiers.
	/// </summary>
	[Flags]
	public enum FileTypeModifier
	{
		/// <summary>
		/// No flags.
		/// </summary>
		None				= 0x0000,
		/// <summary>
		/// +m          always set modtime on client (overrides
		/// client's nomodtime).
		/// </summary>
		ModTime				= 0x0001,
		/// <summary>
		/// +w          always writable on client.
		/// </summary>
		Writable			= 0x0002,
		/// <summary>
		/// +x          exec bit set on client.
		/// </summary>
		Exec				= 0x0004,
		/// <summary>
		/// +k          $Keyword$ expansion of Id, Header, Author
		///             Date, DateTime, Change, File, Revision.
		/// </summary>
		KeywordsAll			= 0x0008,
		/// <summary>
		/// +ko         $Keyword$ expansion of ID, Header only.
		/// </summary>
		KeywordsLimited		= 0x0010,
		/// <summary>
		/// +l          exclusive open: disallow multiple opens.
		/// </summary>
		ExclusiveOpen		= 0x0020,
		/// <summary>
		/// +C          server stores compressed file per revision.
		/// </summary>
		CompressedFiles		= 0x0040,
		/// <summary>
		/// +D          server stores deltas in RCS format.
		/// </summary>
		RCSDeltaFiles		= 0x0080,
		/// <summary>
		/// +F          server stores full file per revision.
		/// </summary>
		FullRevisions		= 0x0100,
		/// <summary>
		/// +S          server stores only single head revision.
		/// </summary>
		HeadrevOnly			= 0x0200,
		/// <summary>
		/// +S<n>       server stores <n> number of revisions, where <n>
		///             is a number 1-10 or 16,32,64,128,256,512.
		/// </summary>
		NRevsOnly			= 0x0400,
		/// <summary>
		/// +X          server runs archive trigger to access files
		/// </summary>
		ArchiveTrigger		= 0x0800
	}

	/// <summary>
	/// Specifies a Perforce file type for a managed file. 
	/// </summary>
	public class FileType
	{
		public FileType (BaseFileType basetype, FileTypeModifier modifiers, int storedrevs)
		{
			if (((modifiers & FileTypeModifier.NRevsOnly) != 0) &&
				( storedrevs <1 | storedrevs >10 ) &&
				 (storedrevs != 16) &&
				 (storedrevs != 32) &&
				 (storedrevs != 64) &&
				 (storedrevs != 128) &&
				 (storedrevs != 256) &&
				 (storedrevs != 512))
			{ throw new ArgumentException("invalid number of revs to store"); }

			BaseType = basetype;
			Modifiers = modifiers;
			StoredRevs = storedrevs;
		}
		public FileType(BaseFileType basetype, FileTypeModifier modifiers)
		{
			if ((modifiers & FileTypeModifier.NRevsOnly) != 0)
			{ throw new ArgumentException("need to specify number of revs to store"); }
			BaseType = basetype;
			Modifiers = modifiers;
			StoredRevs = 0;
		}
		public FileType (string spec)
		{
			Parse(spec);
		}
		private StringEnum<BaseFileType> _baseType;
		public BaseFileType BaseType 
		{
			get { return _baseType; }
			set { _baseType = value; ;} 
		}
		public FileTypeModifier Modifiers { get; set; }
		public int StoredRevs { get; set; }
		internal void Parse(string spec)
        {
            String flags = null;

			// check for historical filetypes
			if (spec == "ctempobj")
			{
				this.BaseType = BaseFileType.Binary;
				this.Modifiers = FileTypeModifier.HeadrevOnly|FileTypeModifier.Writable;
				return;
			}

			if (spec == "ctext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.CompressedFiles;
				return;
			}

			if (spec == "cxtext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.CompressedFiles|FileTypeModifier.Exec;
				return;
			}

			if (spec == "ktext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.KeywordsAll;
				return;
			}

			if (spec == "kxtext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.KeywordsAll|FileTypeModifier.Exec;
				return;
			}

			if (spec == "ltext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.FullRevisions;
				return;
			}

			if (spec == "tempobj")
			{
				this.BaseType = BaseFileType.Binary;
				this.Modifiers = FileTypeModifier.FullRevisions | FileTypeModifier.HeadrevOnly | FileTypeModifier.Writable;
				return;
			}

			if (spec == "ubinary")
			{
				this.BaseType = BaseFileType.Binary;
				this.Modifiers = FileTypeModifier.FullRevisions;
				return;
			}

			if (spec == "uresource")
			{
				this.BaseType = BaseFileType.Resource;
				this.Modifiers = FileTypeModifier.FullRevisions;
				return;
			}

			if (spec == "uxbinary")
			{
				this.BaseType = BaseFileType.Binary;
				this.Modifiers = FileTypeModifier.FullRevisions|FileTypeModifier.Exec;
				return;
			}
			
			if (spec == "xbinary")
			{
				this.BaseType = BaseFileType.Binary;
				this.Modifiers = FileTypeModifier.Exec;
				return;
			}

			if (spec == "xltext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.FullRevisions|FileTypeModifier.Exec;
				return;
			}

			if (spec == "xtext")
			{
				this.BaseType = BaseFileType.Text;
				this.Modifiers = FileTypeModifier.Exec;
				return;
			}

			if (spec == "xtempobj")
			{
				this.BaseType = BaseFileType.Binary;
				this.Modifiers = FileTypeModifier.FullRevisions|FileTypeModifier.HeadrevOnly|FileTypeModifier.Writable|FileTypeModifier.Exec;
				return;
			}

			if (spec == "xutf16")
			{
				this.BaseType = BaseFileType.UTF16;
				this.Modifiers = FileTypeModifier.Exec;
				return;
			}

			if (spec == "xunicode")
			{
				this.BaseType = BaseFileType.Unicode;
				this.Modifiers = FileTypeModifier.Exec;
				return;
			}

			if( spec.StartsWith( "+" ) )
            {
                // no base type specified
                this.BaseType = BaseFileType.Unspecified;

                flags = spec.TrimStart('+');
            }
            else
            {
                string[] parts = spec.Split('+');
                try
                {
                    this._baseType = parts[ 0 ];
                }
                catch
                {
                    this.BaseType = BaseFileType.Unspecified;
                }
                if (parts.Length > 1)
                    flags = parts[1];
                else
                    return; // no flags, so we're done
            }
            for (int idx = 0; idx < flags.Length; idx++)
            {
                if ((idx+1 < flags.Length) && (flags[idx] == 'S'))
                {
                    if (char.IsNumber(flags[idx + 1]))
                    {
                        this.Modifiers |= FileTypeModifier.NRevsOnly;
                        int cnt = 0;
                        
                        while ((idx+cnt+1 < flags.Length) && char.IsNumber(flags[idx + cnt + 1 ]))
                        {
                            cnt++;
                        }
                        int revs = 1;
                        int.TryParse(flags.Substring(idx+1, cnt), out revs);
                        this.StoredRevs = revs;
                        idx += cnt;
                    }
                    else
                    {
                        this.Modifiers |= FileTypeModifier.HeadrevOnly;
                    }
                }
                else if ((idx + 1 < flags.Length) && (flags[idx] == 'k'))
                {
                    if (flags[idx + 1] == 'o')
                    {
                        this.Modifiers |= FileTypeModifier.KeywordsLimited;
                        idx++;
                    }
                    else
                        this.Modifiers |= FileTypeModifier.KeywordsAll;
                }
                else
                {
                    switch (flags[idx])
                    {
                        case 'm':
                            this.Modifiers |= FileTypeModifier.ModTime;
                            break;
                        case 'w':
                            this.Modifiers |= FileTypeModifier.Writable;
                            break;
                        case 'x':
                            this.Modifiers |= FileTypeModifier.Exec;
                            break;
                        case 'k':
                            this.Modifiers |= FileTypeModifier.KeywordsAll;
                            break;
                        case 'l':
                            this.Modifiers |= FileTypeModifier.ExclusiveOpen;
                            break;
                        case 'C':
                            this.Modifiers |= FileTypeModifier.CompressedFiles;
                            break;
                        case 'D':
                            this.Modifiers |= FileTypeModifier.RCSDeltaFiles;
                            break;
                        case 'F':
                            this.Modifiers |= FileTypeModifier.FullRevisions;
                            break;
                        case 'S':
                            this.Modifiers |= FileTypeModifier.HeadrevOnly;
                            break;
                        case 'X':
                            this.Modifiers |= FileTypeModifier.ArchiveTrigger;
                            break;
                        default: //unknown flag
                            break;
                    }
                }
            }
        }

		public override string ToString()
        {
            String v = String.Empty;

            if (BaseType != BaseFileType.Unspecified)
                v = _baseType.ToString(StringEnumCase.Lower);

            if (Modifiers != FileTypeModifier.None)
            {
                v += "+";

                if ((Modifiers & FileTypeModifier.ModTime) != 0)
                    v += "m";
                if ((Modifiers & FileTypeModifier.Writable) != 0)
                    v += "w";
                if ((Modifiers & FileTypeModifier.Exec) != 0)
                    v += "x";
                if ((Modifiers & FileTypeModifier.KeywordsAll) != 0)
                    v += "k";
                if ((Modifiers & FileTypeModifier.KeywordsLimited) != 0)
                    v += "ko";
                if ((Modifiers & FileTypeModifier.ExclusiveOpen) != 0)
                    v += "l";
                if ((Modifiers & FileTypeModifier.CompressedFiles) != 0)
                    v += "C";
                if ((Modifiers & FileTypeModifier.RCSDeltaFiles) != 0)
                    v += "D";
                if ((Modifiers & FileTypeModifier.FullRevisions) != 0)
                    v += "F";
                if ((Modifiers & FileTypeModifier.HeadrevOnly) != 0)
                    v += "S";
                if (((Modifiers & FileTypeModifier.NRevsOnly) != 0) &&
                    (StoredRevs >= 1)) // && (MaxRevisions <= 10))
                    v += String.Format("S{0}", StoredRevs);
                if ((Modifiers & FileTypeModifier.ArchiveTrigger) != 0)
                    v += "X";
            }
            return v;
        }
	}
}
