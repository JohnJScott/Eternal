/*******************************************************************************

Copyright (c) 2011, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: Depot.cs
 *
 * Author	: wjb
 *
 * Description	: Class used to abstract a depot specification in Perforce.
 *
 ******************************************************************************/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// The type of the depot.
	/// </summary>
	[Flags]
	public enum DepotType
	{
		/// <summary>
		/// A 'local' depot (the default) is managed directly by
		/// the server and its files reside in the server's root
		/// directory.
		/// </summary>
		Local =0x0000,
		/// <summary>
		/// A 'remote' depot refers to files in another Perforce
		/// server.
		/// </summary>
		Remote = 0x0001,
		/// <summary>
		/// A 'spec' depot automatically archives all edited forms
		/// (branch, change, client, depot, group, job, jobspec,
		/// protect, triggers, typemap, and user) in special,
		/// read-only files.  The files are named:
		/// //depotname/formtype/name[suffix].  Updates to jobs made
		/// by the 'p4 change', 'p4 fix', and 'p4 submit' commands
		/// are also saved, but other automatic updates such as
		/// as access times or opened files (for changes) are not.
		/// A server can contain only one 'spec' depot.
		/// </summary>
		Spec = 0x0002,
		/// <summary>
		/// A 'stream' depot is a local depot dedicated to the
		/// storage of files in a stream.
		/// </summary>
		Stream = 0x0004,
		/// <summary>
		/// An 'archive' depot defines a storage location to which
		/// obsolete revisions may be relocated.
		/// </summary>
		Archive = 0x0008,
	}

		/// <remarks>
		/// <br/><b>p4 help depot</b>
		/// <br/> 
		/// <br/>     depot -- Create or edit a depot specification
		/// <br/> 
		/// <br/>     p4 depot name
		/// <br/>     p4 depot -d name
		/// <br/>     p4 depot -o name
		/// <br/>     p4 depot -i
		/// <br/> 
		/// <br/> 	Create a new depot specification or edit an existing depot
		/// <br/> 	specification. The specification form is put into a temporary file
		/// <br/> 	and the editor (configured by the environment variable $P4EDITOR)
		/// <br/> 	is invoked.
		/// <br/> 
		/// <br/> 	The depot specification contains the following fields:
		/// <br/> 
		/// <br/> 	Depot:       The name of the depot.  This name cannot be the same as
		/// <br/> 		     any branch, client, or label name.
		/// <br/> 
		/// <br/> 	Owner:       The user who created this depot.
		/// <br/> 
		/// <br/> 	Date:        The date that this specification was last modified.
		/// <br/> 
		/// <br/> 	Description: A short description of the depot (optional).
		/// <br/> 
		/// <br/> 	Type:        'local', 'stream', 'remote', 'spec', or 'archive'.
		/// <br/> 
		/// <br/> 		     A 'local' depot (the default) is managed directly by
		/// <br/> 		     the server and its files reside in the server's root
		/// <br/> 		     directory.
		/// <br/> 
		/// <br/> 		     A 'stream' depot is a local depot dedicated to the
		/// <br/> 		     storage of files in a stream.
		/// <br/> 
		/// <br/> 		     A 'remote' depot refers to files in another Perforce
		/// <br/> 		     server.
		/// <br/> 
		/// <br/> 		     A 'spec' depot automatically archives all edited forms
		/// <br/> 		     (branch, change, client, depot, group, job, jobspec,
		/// <br/> 		     protect, triggers, typemap, and user) in special,
		/// <br/> 		     read-only files.  The files are named:
		/// <br/> 		     //depotname/formtype/name[suffix].  Updates to jobs made
		/// <br/> 		     by the 'p4 change', 'p4 fix', and 'p4 submit' commands
		/// <br/> 		     are also saved, but other automatic updates such as
		/// <br/> 		     as access times or opened files (for changes) are not.
		/// <br/> 		     A server can contain only one 'spec' depot.
		/// <br/> 
		/// <br/> 		     A 'archive' depot defines a storage location to which
		/// <br/> 		     obsolete revisions may be relocated.
		/// <br/> 
		/// <br/> 	Address:     For remote depots, the $P4PORT (connection address)
		/// <br/> 		     of the remote server.
		/// <br/> 
		/// <br/> 	Suffix:      For spec depots, the optional suffix to be used
		/// <br/> 		     for generated paths. The default is '.p4s'.
		/// <br/> 
		/// <br/> 	Map:         Path translation information, in the form of a file
		/// <br/> 		     pattern with a single ... in it.  For local depots,
		/// <br/> 		     this path is relative to the server's root directory
		/// <br/> 		     (Example: depot/...).  For remote depots, this path
		/// <br/> 		     refers to the remote server's namespace
		/// <br/> 		     (Example: //depot/...).
		/// <br/> 
		/// <br/> 	The -d flag deletes the specified depot.  If any files reside in the
		/// <br/> 	depot, they must be removed with 'p4 obliterate' before deleting the
		/// <br/> 	depot.
		/// <br/> 
		/// <br/> 	The -o flag writes the depot specification to standard output. The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -i flag reads a depot specification from standard input. The
		/// <br/> 	user's editor is not invoked.
		/// <br/> 
		/// <br/> 
		/// </remarks>
	/// <summary>
	/// A depot specification in a Perforce repository. 
	/// </summary>
	public class Depot
	{
		public Depot()
		{ 
		}
		public Depot(string id,
						DepotType type,
						DateTime modified,
						ServerAddress address,
						string owner,
						string description,
						string suffix,
						string map,
						FormSpec spec
						)
		{
			Id = id;
			Type = type;
			Modified = modified;
			Address = address;
			Owner = owner;
			Description = description;
			Suffix = suffix;
			Map = map;
			Spec = spec;
		}

		private bool _initialized;
		private FormBase _baseForm;

		#region properties

		public string Id { get; set; }

		private StringEnum<DepotType> _type = DepotType.Local;
		public DepotType Type
		{
			get { return _type; }
			set { _type = value; }
		}

	    public DateTime Modified { get; set; }
		public ServerAddress Address{ get; set; }
		public string Owner { get; set; }
		public string Description { get; set; }
		public string Suffix { get; set; }
		public string Map { get; set; }
		public FormSpec Spec { get; set; }

		#endregion

		/// <summary>
		/// Read the fields from the tagged output of a depot command
		/// </summary>
		/// <param name="objectInfo">Tagged output from the 'depot' command</param>
		public void FromDepotCmdTaggedOutput(TaggedObject objectInfo)
		{
			_initialized = true;
			_baseForm = new FormBase();

			_baseForm.SetValues(objectInfo);

			if (objectInfo.ContainsKey("Depot"))
				Id = objectInfo["Depot"];

			if (objectInfo.ContainsKey("Owner"))
				Owner = objectInfo["Owner"];

			if (objectInfo.ContainsKey("Date"))
			{
				DateTime v = DateTime.MinValue;
				DateTime.TryParse(objectInfo["Date"], out v);
				Modified = v;
			}

			if (objectInfo.ContainsKey("Description"))
				Description = objectInfo["Description"];

			if (objectInfo.ContainsKey("Type"))
				_type = objectInfo["Type"];

			if (objectInfo.ContainsKey("Address"))
				Address = new ServerAddress(objectInfo["Address"]);

			if (objectInfo.ContainsKey("Map"))
				Map = objectInfo["Map"];

			if (objectInfo.ContainsKey("Suffix"))
				Suffix = objectInfo["Suffix"];

		}

		/// <summary>
		/// Parse the fields from a depot specification 
		/// </summary>
		/// <param name="spec">Text of the depot specification in server format</param>
		/// <returns></returns>
		public bool Parse(String spec)
		{
			_baseForm = new FormBase();

			_baseForm.Parse(spec); // parse the values into the underlying dictionary

			if (_baseForm.ContainsKey("Depot"))
			{
				Id = _baseForm["Depot"] as string;
			}

			if (_baseForm.ContainsKey("Owner"))
			{
				Owner = _baseForm["Owner"] as string;
			}

			if (_baseForm.ContainsKey("Date"))
			{
				DateTime v = DateTime.MinValue;
				DateTime.TryParse(_baseForm["Date"] as string, out v);
				Modified = v;
			}

			if (_baseForm.ContainsKey("Description"))
			{
				Description = _baseForm["Description"] as string;
			}

			if (_baseForm.ContainsKey("Type"))
			{
				_type = _baseForm["Type"] as string;
			}

			if (_baseForm.ContainsKey("Address"))
			{
				Address = new ServerAddress(_baseForm["Description"] as string);
			}

			if (_baseForm.ContainsKey("Map"))
			{
				Map = _baseForm["Map"] as string;
			}

			if (_baseForm.ContainsKey("Suffix"))
			{
				Suffix = _baseForm["Suffix"] as string;
			}

			return true;

		}

		/// <summary>
		/// Format of a depot specification used to save a depot to the server
		/// </summary>
		private static String DepotSpecFormat =
													"Depot:\t{0}\r\n" +
													"\r\n" +
													"Owner:\t{1}\r\n" +
													"\r\n" +
													"Date:\t{2}\r\n" +
													"\r\n" +
													"Description:\r\n\t{3}\r\n" +
													"\r\n" +
													"Type:\t{4}\r\n" +
													"\r\n" +
													"Address:\t{5}\r\n" +
													"\r\n" +
													"Suffix:\t{6}\r\n" +
													"\r\n" +
													"Map:\t{7}";
													

		/// <summary>
		/// Convert to specification in server format
		/// </summary>
		/// <returns></returns>
		override public String ToString()
		{
			String DepotType = _type.ToString(StringEnumCase.Lower);
			//String Address = ToString();
			
			String value = String.Format(DepotSpecFormat, Id, Owner,
				FormBase.FormatDateTime(Modified), Description, DepotType,
				Address, Suffix, Map);
			return value;
		}

		/// <summary>
		/// Read the fields from the tagged output of a depots command
		/// </summary>
		/// <param name="objectInfo">Tagged output from the 'depots' command</param>
		public void FromDepotsCmdTaggedOutput(TaggedObject objectInfo)
		{
			_initialized = true;
			_baseForm = new FormBase();

			_baseForm.SetValues(objectInfo);

			if (objectInfo.ContainsKey("name"))
				Id = objectInfo["name"];

			if (objectInfo.ContainsKey("Owner"))
				Owner = objectInfo["Owner"];

			if (objectInfo.ContainsKey("time"))
			{
				Modified = FormBase.ConvertUnixTime(objectInfo["time"]);
			}

			if (objectInfo.ContainsKey("desc"))
				Description = objectInfo["desc"];

			if (objectInfo.ContainsKey("type"))
				_type = objectInfo["type"];

			if (objectInfo.ContainsKey("Address"))
				Address = new ServerAddress(objectInfo["Address"]);

			if (objectInfo.ContainsKey("map"))
				Map = objectInfo["map"];

			if (objectInfo.ContainsKey("Suffix"))
				Suffix = objectInfo["Suffix"];

		}


	}
}
