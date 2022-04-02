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
 * Name		: Client.cs
 *
 * Author	: dbb
 *
 * Description	: Class used to abstract a client in Perforce.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Flags to configure the client behavior.
	/// </summary>
	[Flags]
	public enum ClientOption
	{
		/// <summary>
		/// No options.
		/// </summary>
		None		= 0x0000,
		/// <summary>
		/// Leaves all files writable on the client;
		/// by default, only files opened by 'p4 edit'
		/// are writable. If set, files might be clobbered
		/// as a result of ignoring the clobber option.
		/// </summary>
		AllWrite	= 0x0001,
		/// <summary>
		/// Permits 'p4 sync' to overwrite writable
		/// files on the client.  noclobber is ignored if
		/// allwrite is set.
		/// </summary>
		Clobber		= 0x0002,
		/// <summary>
		/// Compresses data sent between the client
		/// and server to speed up slow connections.
		/// </summary>
		Compress	= 0x0004,
		/// <summary>
		/// Allows only the client owner to use or change
		/// the client spec.  Prevents the client spec from
		/// being deleted.
		/// </summary>
		Locked		= 0x0008,
		/// <summary>
		/// Causes 'p4 sync' and 'p4 submit' to preserve
		/// file modification time, as with files with the
		/// +m type modifier. (See 'p4 help filetypes'.)
		/// With nomodtime, file timestamps are updated by
		/// sync and submit operations.
		/// </summary>
		ModTime		= 0x0010,
		/// <summary>
		/// Makes 'p4 sync' attempt to delete a workspace
		/// directory when all files in it are removed.
		/// </summary>
		RmDir		= 0x0020
	};

	internal class ClientOptionEnum : StringEnum<ClientOption>
	{

		public ClientOptionEnum(ClientOption v)
			: base(v)
		{
		}

		public ClientOptionEnum(string spec)
			: base(ClientOption.None)
		{
			Parse(spec);
		}

		public static implicit operator ClientOptionEnum(ClientOption v)
		{
			return new ClientOptionEnum(v);
		}

		public static implicit operator ClientOptionEnum(string s)
		{
			return new ClientOptionEnum(s);
		}

		public static implicit operator string(ClientOptionEnum v)
		{
			return v.ToString();
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType() == typeof(ClientOption))
			{
				return value.Equals((ClientOption)obj);
			}
			if (obj.GetType() == typeof(ClientOptionEnum))
			{
				return value.Equals(((ClientOptionEnum)obj).value);
			}
			return false;
		}

		public static bool operator ==(ClientOptionEnum t1, ClientOptionEnum t2) { return t1.value.Equals(t2.value); }
		public static bool operator !=(ClientOptionEnum t1, ClientOptionEnum t2) { return !t1.value.Equals(t2.value); }

		public static bool operator ==(ClientOption t1, ClientOptionEnum t2) { return t1.Equals(t2.value); }
		public static bool operator !=(ClientOption t1, ClientOptionEnum t2) { return !t1.Equals(t2.value); }

		public static bool operator ==(ClientOptionEnum t1, ClientOption t2) { return t1.value.Equals(t2); }
		public static bool operator !=(ClientOptionEnum t1, ClientOption t2) { return !t1.value.Equals(t2); }

		/// <summary>
		/// Convert to a client spec formatted string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format("{0} {1} {2} {3} {4} {5}",
				((value & ClientOption.AllWrite) != 0) ? "allwrite" : "noallwrite",
				((value & ClientOption.Clobber) != 0) ? "clobber" : "noclobber",
				((value & ClientOption.Compress) != 0) ? "compress" : "nocompress",
				((value & ClientOption.Locked) != 0) ? "locked" : "unlocked",
				((value & ClientOption.ModTime) != 0) ? "modtime" : "nomodtime",
				((value & ClientOption.RmDir) != 0) ? "rmdir" : "normdir"
				);
		}
		/// <summary>
		/// Parse a client spec formatted string
		/// </summary>
		/// <param name="spec"></param>
		public void Parse(String spec)
		{
			value = ClientOption.None;

			if (!spec.Contains("noallwrite"))
				value |= ClientOption.AllWrite;

			if (!spec.Contains("noclobber"))
				value |= ClientOption.Clobber;

			if (!spec.Contains("nocompress"))
				value |= ClientOption.Compress;

			if (!spec.Contains("unlocked"))
				value |= ClientOption.Locked;

			if (!spec.Contains("nomodtime"))
				value |= ClientOption.ModTime;

			if (!spec.Contains("normdir"))
				value |= ClientOption.RmDir;
		}
	}

	/// <summary>
	/// Flags to change submit behavior.
	/// </summary>
	[Flags]
	public enum SubmitType
	{
		/// <summary>
		/// All open files are submitted (default).
		/// </summary>
		SubmitUnchanged = 0x000,
		/// <summary>
		/// Files that have content or type changes
		/// are submitted. Unchanged files are
		/// reverted.
		/// </summary>
		RevertUnchanged = 0x001,
		/// <summary>
		/// Files that have content or type changes
		/// are submitted. Unchanged files are moved
		/// to the default changelist.
		/// </summary>
		LeaveUnchanged = 0x002
	}

	/// <summary>
	/// Client options that define what to do with files upon submit. 
	/// </summary>
	public class ClientSubmitOptions
	{
		/// <summary>
		/// Determines if the files is reopened upon submit.
		/// </summary>
		public bool Reopen { get; set; }

		public ClientSubmitOptions() { }
		public ClientSubmitOptions(string spec)
		{
			Parse(spec);
		}
		public ClientSubmitOptions(bool reopen, SubmitType submitType)
		{
			Reopen = reopen;
			_submitType = submitType;
		}

		public static implicit operator ClientSubmitOptions(string s)
		{
			return new ClientSubmitOptions(s);
		}

		public static implicit operator string(ClientSubmitOptions v)
		{
			return v.ToString();
		}
		public override bool Equals(object obj)
		{
			if (obj is ClientSubmitOptions)
			{
				ClientSubmitOptions o = obj as ClientSubmitOptions;
				return ((this._submitType == o._submitType) && (this.Reopen == o.Reopen));
			}
			return false;
		}
		private StringEnum<SubmitType> _submitType;
		public SubmitType SubmitType 
		{
			get { return _submitType; }
			set { _submitType = value; }
		}
		/// <summary>
		/// Convert to a client spec formatted string
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			String value = _submitType.ToString(StringEnumCase.Lower);

			if (Reopen)
				value += "+reopen";

			return value;
		}
		/// <summary>
		/// Parse a client spec formatted string
		/// </summary>
		/// <param name="spec"></param>
		public void Parse(String spec)
		{
			_submitType = SubmitType.SubmitUnchanged;
			Reopen = false;

			if (spec.Contains("revertunchanged"))
				_submitType = SubmitType.RevertUnchanged;

			if (spec.Contains("leaveunchanged"))
				_submitType = SubmitType.LeaveUnchanged;

			if (spec.Contains("+reopen"))
				Reopen = true;
		}
	}

	/// <summary>
	///  Sets line-ending character(s) for client text files.
	/// </summary>
	[Flags]
	public enum LineEnd
	{ 
		/// <summary>
		/// mode that is native to the client (default).
		/// </summary>
		Local = 0x0000,
		/// <summary>
		/// linefeed: UNIX style.
		/// </summary>
		Unix = 0x0001,
		/// <summary>
		/// carriage return: Macintosh style.
		/// </summary>
		Mac = 0x0002,
		/// <summary>
		/// carriage return-linefeed: Windows style.
		/// </summary>
		Win = 0x0003,
		/// <summary>
		/// hybrid: writes UNIX style but reads UNIX,
		/// Mac or Windows style.
		/// </summary>
		Share = 0x0004
	}

	/// <summary>
	/// A client specification in a Perforce repository. 
	/// </summary>
	public class Client
	{
		// has the actual record been retrieved from the server
		public bool Initialized { get; set; }

		public void Initialize(Connection connection)
		{
			Initialized = false;
			if ((connection == null) || String.IsNullOrEmpty(Name))
			{
				P4Exception.Throw(ErrorSeverity.E_FAILED, "Client cannot be initialized");
				return;
			}
			Connection = connection;

			if (connection._p4server == null)
			{
				// not connected to the server yet
				return;
			}
			P4Command cmd = new P4Command(connection, "client", true, "-o", Name);

			P4CommandResult results = cmd.Run();
			if ((results.Success) && (results.TaggedOutput != null) && (results.TaggedOutput.Count > 0))
			{
				FromClientCmdTaggedOutput(results.TaggedOutput[0]);
				Initialized = true;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}
		}
		internal Connection Connection { get; private set; }
		internal FormBase _baseForm;

		public string Name { get; set; }
		public string OwnerName { get; set; }
		public string Host { get; set; }
		public string Description { get; set; }
		public DateTime Updated { get; set; }
		public DateTime Accessed { get; set; }
		public string Root { get; set; }
		public IList<string> AltRoots { get; set; }

		private ClientOptionEnum _options;
		public ClientOption Options
		{
			get { return _options; }
			set { _options = (ClientOptionEnum) value; }
		}

		public ClientSubmitOptions SubmitOptions { get; set; }

		private StringEnum<LineEnd> _lineEnd;
		public LineEnd LineEnd
		{
			get { return _lineEnd; }
			set { _lineEnd = value; }
		}

		public string Stream { get; set; }
		public string StreamAtChange { get; set; }
		public string ServerID { get; set; }

		public ViewMap ViewMap { get; set; }

		public FormSpec Spec { get; set; }


		#region fromTaggedOutput
		/// <summary>
		/// Parse the tagged output of a 'clients' command
		/// </summary>
		/// <param name="workspaceInfo"></param>
		public void FromClientsCmdTaggedOutput(TaggedObject workspaceInfo)
		{
			Initialized = true;

			_baseForm = new FormBase();
			_baseForm.SetValues(workspaceInfo);

			if (workspaceInfo.ContainsKey("client"))
				Name = workspaceInfo["client"];
			else if (workspaceInfo.ContainsKey("Client"))
				Name = workspaceInfo["Client"];

			if (workspaceInfo.ContainsKey("Update"))
			{
				DateTime d;
				long unixTime = 0;
				if (Int64.TryParse(workspaceInfo["Update"], out unixTime))
				{
					d = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);
					Updated = d;
				}
			}

			if (workspaceInfo.ContainsKey("Access"))
			{
				DateTime d;
				long unixTime = 0;
				if (Int64.TryParse(workspaceInfo["Access"], out unixTime))
				{
					d = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(unixTime);
					Accessed = d;
				}
			}

			if (workspaceInfo.ContainsKey("Owner"))
				OwnerName = workspaceInfo["Owner"];

			if (workspaceInfo.ContainsKey("Options"))
			{
				String optionsStr = workspaceInfo["Options"];
				_options = optionsStr;
			}

			if (workspaceInfo.ContainsKey("SubmitOptions"))
			{
				SubmitOptions = workspaceInfo["SubmitOptions"];
			}

			if (workspaceInfo.ContainsKey("LineEnd"))
			{
				_lineEnd = workspaceInfo["LineEnd"];
			}

			if (workspaceInfo.ContainsKey("Root"))
				Root = workspaceInfo["Root"];

			if (workspaceInfo.ContainsKey("Host"))
				Host = workspaceInfo["Host"];

			if (workspaceInfo.ContainsKey("Description"))
				Description = workspaceInfo["Description"];

			if (workspaceInfo.ContainsKey("Stream"))
				Stream = workspaceInfo["Stream"];

			if (workspaceInfo.ContainsKey("StreamAtChange"))
				StreamAtChange = workspaceInfo["StreamAtChange"];

			if (workspaceInfo.ContainsKey("ServerID"))
				ServerID = workspaceInfo["ServerID"];

			int idx = 0;
			String key = String.Format("AltRoots{0}", idx);
			if (workspaceInfo.ContainsKey(key))
			{
				AltRoots = new List<String>();
				while (workspaceInfo.ContainsKey(key))
				{
					AltRoots.Add(workspaceInfo[key]);
					idx++;
					key = String.Format("AltRoots{0}", idx);
				}
			}

			idx = 0;
			key = String.Format("View{0}", idx);
			if (workspaceInfo.ContainsKey(key))
			{
				ViewMap = new ViewMap();
				while (workspaceInfo.ContainsKey(key))
				{
					ViewMap.Add(workspaceInfo[key]);
					idx++;
					key = String.Format("View{0}", idx);
				}
			}
			else
			{
				ViewMap = null;// new WorkspaceView(pServer, GetView());
			}
		}


		/// <summary>
		/// Parse the tagged output of a 'client' command
		/// </summary>
		/// <param name="workspaceInfo"></param>
		public void FromClientCmdTaggedOutput(TaggedObject workspaceInfo)
		{
			Initialized = true;

			_baseForm = new FormBase();
			_baseForm.SetValues(workspaceInfo);

			if (workspaceInfo.ContainsKey("client"))
				Name = workspaceInfo["client"];
			else if (workspaceInfo.ContainsKey("Client"))
				Name = workspaceInfo["Client"];

			if (workspaceInfo.ContainsKey("Update"))
			{
				DateTime d;
				if (DateTime.TryParse(workspaceInfo["Update"], out d))
				{
					Updated = d;
				}
			}

			if (workspaceInfo.ContainsKey("Access"))
			{
				DateTime d;
				if (DateTime.TryParse(workspaceInfo["Access"], out d))
				{
					Accessed = d;
				}
			}

			if (workspaceInfo.ContainsKey("Owner"))
				OwnerName = workspaceInfo["Owner"];

			if (workspaceInfo.ContainsKey("Options"))
			{
				String optionsStr = workspaceInfo["Options"];
				_options = optionsStr;
			}

			if (workspaceInfo.ContainsKey("SubmitOptions"))
			{
				SubmitOptions = workspaceInfo["SubmitOptions"];
			}

			if (workspaceInfo.ContainsKey("LineEnd"))
			{
				_lineEnd = workspaceInfo["LineEnd"];
			}

			if (workspaceInfo.ContainsKey("Root"))
				Root = workspaceInfo["Root"];

			if (workspaceInfo.ContainsKey("Host"))
				Host = workspaceInfo["Host"];

			if (workspaceInfo.ContainsKey("Description"))
				Description = workspaceInfo["Description"];

			if (workspaceInfo.ContainsKey("Stream"))
				Stream = workspaceInfo["Stream"];

			if (workspaceInfo.ContainsKey("StreamAtChange"))
				StreamAtChange = workspaceInfo["StreamAtChange"];

			if (workspaceInfo.ContainsKey("ServerID"))
				ServerID = workspaceInfo["ServerID"];

			int idx = 0;
			String key = String.Format("AltRoots{0}", idx);
			if (workspaceInfo.ContainsKey(key))
			{
				AltRoots = new List<String>();
				while (workspaceInfo.ContainsKey(key))
				{
					AltRoots.Add(workspaceInfo[key]);
					idx++;
					key = String.Format("AltRoots{0}", idx);
				}
			}

			idx = 0;
			key = String.Format("View{0}", idx);
			if (workspaceInfo.ContainsKey(key))
			{
				ViewMap = new ViewMap();
				while (workspaceInfo.ContainsKey(key))
				{
					ViewMap.Add(workspaceInfo[key]);
					idx++;
					key = String.Format("View{0}", idx);
				}
			}
			else
			{
				ViewMap = null;// new WorkspaceView(pServer, GetView());
			}
		}
		#endregion
		#region client spec support
		/// <summary>
		/// Parse a client spec
		/// </summary>
		/// <param name="spec"></param>
		/// <returns></returns>
		public bool Parse(String spec)
		{
			_baseForm = new FormBase();
			_baseForm.Parse(spec); // parse the values into the underlying dictionary

			if (_baseForm.ContainsKey("Client"))
			{
				Name = _baseForm["Client"] as string;
			}
			if (_baseForm.ContainsKey("Host"))
			{
				Host = _baseForm["Host"] as string;
			}
			if (_baseForm.ContainsKey("Owner"))
			{
				OwnerName = _baseForm["Owner"] as string;
			}
			if (_baseForm.ContainsKey("Root"))
			{
				Root = _baseForm["Root"] as string;
			}
			if (_baseForm.ContainsKey("Description"))
			{
				IList<string> strList = _baseForm["Description"] as IList<string>;
				Description = string.Empty;
				for (int idx = 0; idx < strList.Count; idx++)
				{
					if (idx > 0)
					{
						Description += "\r\n";
					}
					Description += strList[idx];
				}
			}
			if ((_baseForm.ContainsKey("AltRoots")) && (_baseForm["AltRoots"] is IList<string>))
			{
				AltRoots = _baseForm["AltRoots"] as IList<string>;
			}
			if ((_baseForm.ContainsKey("View")) && (_baseForm["View"] is IList<string>))
			{
				IList<string> lines = _baseForm["View"] as IList<string>;
				ViewMap = new ViewMap(lines.ToArray());
			}
			if (_baseForm.ContainsKey("Update"))
			{
				DateTime d;
				if (DateTime.TryParse(_baseForm["Update"] as string, out d))
				{
					Updated = d;
				}
			}
			if (_baseForm.ContainsKey("Access"))
			{
				DateTime d;
				if (DateTime.TryParse(_baseForm["Access"] as string, out d))
				{
					Accessed = d;
				}
			}
			if (_baseForm.ContainsKey("Options"))
			{
				_options = _baseForm["Options"] as string;
			}
			if (_baseForm.ContainsKey("SubmitOptions"))
			{
				SubmitOptions = _baseForm["SubmitOptions"] as string;
			}
			if (_baseForm.ContainsKey("LineEnd"))
			{
				_lineEnd = _baseForm["LineEnd"] as string;
			}
			if (_baseForm.ContainsKey("Stream"))
			{
				Stream = _baseForm["Stream"] as string;
			}
			if (_baseForm.ContainsKey("StreamAtChange"))
			{
				StreamAtChange = _baseForm["StreamAtChange"] as string;
			}
			if (_baseForm.ContainsKey("ServerID"))
			{
				ServerID = _baseForm["ServerID"] as string;
			}
			return true;
		}

		private static String ClientSpecFormat =
													"Client:\t{0}\r\n" +
													"\r\n" +
													"Update:\t{1}\r\n" +
													"\r\n" +
													"Access:\t{2}\r\n" +
													"\r\n" +
													"Owner:\t{3}\r\n" +
													"\r\n" +
													"Host:\t{4}\r\n" +
													"\r\n" +
													"Description:\r\n" +
													"\t{5}\r\n" +
													"\r\n" +
													"Root:\t{6}\r\n" +
													"\r\n" +
													"AltRoots:\r\n" +
													"\t{7}\r\n" +
													"\r\n" +
													"Options:\t{8}\r\n" +
													"\r\n" +
													"SubmitOptions:\t{9}\r\n" +
													"\r\n" +
													"LineEnd:\t{10}\r\n" +
													"\r\n" +
													"{11}"+
													"{12}" +
													"{13}" +
													"View:\r\n" +
													"\t{14}\r\n";
		private String AltRootsStr
		{
			get
			{
				String value = String.Empty;
				if ((AltRoots != null) && (AltRoots.Count > 0))
				{
					for (int idx = 0; idx < AltRoots.Count; idx++)
					{
						value += AltRoots[idx] + "\r\n";
					}
				}
				return value;
			}
		}
		/// <summary>
		/// Utility function to format a DateTime in the format expected in a spec
		/// </summary>
		/// <param name="dt"></param>
		/// <returns></returns>
		public static String FormatDateTime(DateTime dt)
		{
			if ((dt != null) && (DateTime.MinValue != dt))
				return dt.ToString("yyyy/MM/dd HH:mm:ss");
			return string.Empty;
		}

		/// <summary>
		/// Format as a client spec
		/// </summary>
		/// <returns></returns>
		override public String ToString()
		{
			String altRootsStr = String.Empty;
			if (!String.IsNullOrEmpty(AltRootsStr))
			{
				altRootsStr = AltRootsStr.Replace("\r\n", "\n").Trim();
				altRootsStr = AltRootsStr.Replace("\n", "\n\t").Trim();
			}
			String desc = String.Empty;
			if (!String.IsNullOrEmpty(Description))
			{
				desc = Description.Replace("\r\n", "\n").Trim();
				desc = Description.Replace("\n", "\n\t").Trim();
			}
			String viewStr = String.Empty;
			if (ViewMap != null)
			{
				viewStr = ViewMap.ToString().Replace("\r\n", "\n").Trim();
				viewStr = ViewMap.ToString().Replace("\n", "\n\t").Trim();
			}
			String streamStr = String.Empty;
			if (Stream != null)
			{
				streamStr = Stream.ToString().Replace("\r\n", "\n").Trim();
				streamStr = Stream.ToString().Replace("\n", "\n\t").Trim();
				streamStr = "Stream:\t"+streamStr+"\r\n" + "\r\n";
			}
			String streamAtChangeStr = String.Empty;
			if (StreamAtChange != null)
			{
				streamAtChangeStr = StreamAtChange.ToString().Replace("\r\n", "\n").Trim();
				streamAtChangeStr = StreamAtChange.ToString().Replace("\n", "\n\t").Trim();
				streamAtChangeStr = "StreamAtChange:\t" + streamAtChangeStr + "\r\n" + "\r\n";
			}
			String serverIDStr = String.Empty;
			if (ServerID != null)
			{
				serverIDStr = ServerID.ToString().Replace("\r\n", "\n").Trim();
				serverIDStr = ServerID.ToString().Replace("\n", "\n\t").Trim();
				serverIDStr = "ServerID:\t" + serverIDStr + "\r\n" + "\r\n";
			}
			String value = String.Format(ClientSpecFormat, Name,
				FormatDateTime(Updated),
				FormatDateTime(Accessed),
				OwnerName, Host, desc, Root, altRootsStr,
				_options.ToString(),
				SubmitOptions.ToString(),
				_lineEnd.ToString(), streamStr, streamAtChangeStr,
				serverIDStr, viewStr);
			return value;
		}
		#endregion

		#region operations
		internal List<FileSpec> runFileListCmd(string cmdName, Options options, params FileSpec[] files)
		{
			return runFileListCmd(cmdName, options, null, files);
		}
		internal List<FileSpec> runFileListCmd(string cmdName, Options options, string commandData, params FileSpec[] files)
		{
			string[] paths = null;
			P4Command cmd = null;
			if (files != null)
			{
				if (cmdName == "add")
				{
					paths = FileSpec.ToStrings(files);
				}
				else
				{
					paths = FileSpec.ToEscapedStrings(files);
				}

				cmd = new P4Command(Connection, cmdName, true, paths);
			}
			else
			{
				cmd = new P4Command(Connection, cmdName, true);
			}
			if (String.IsNullOrEmpty(commandData) == false)
			{
				cmd.DataSet = commandData;
			}
			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
				{
					return null;
				}
				List<FileSpec> newDepotFiles = new List<FileSpec>();
				foreach (TaggedObject obj in results.TaggedOutput)
				{
					FileSpec spec = null;
					int rev = -1;
					string p;

					DepotPath dp = null;
					ClientPath cp = null;
					LocalPath lp = null;

					if (obj.ContainsKey("workRev"))
					{
						int.TryParse(obj["workRev"], out rev);
					}
					else if (obj.ContainsKey("haveRev"))
					{
						int.TryParse(obj["haveRev"], out rev);
					}
					else if (obj.ContainsKey("rev"))
					{
						int.TryParse(obj["rev"], out rev);
					}
					if (obj.ContainsKey("depotFile"))
					{
						p = obj["depotFile"];
						dp = new DepotPath(PathSpec.UnescapePath(p));
					}
					if (obj.ContainsKey("clientFile"))
					{
						p = obj["clientFile"];
						if (p.StartsWith("//"))
						{
							cp = new ClientPath(PathSpec.UnescapePath(p));
						}
						else
						{
							cp = new ClientPath(PathSpec.UnescapePath(p));
							lp = new LocalPath(PathSpec.UnescapePath(p));
						}
					}
					if (obj.ContainsKey("path"))
					{
						lp = new LocalPath(obj["path"]);
					}
					spec = new FileSpec(dp, cp, lp, new Revision(rev));
					newDepotFiles.Add(spec);
				}
				return newDepotFiles;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}

			return null;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help add</b>
		/// <br/> 
		/// <br/>     add -- Open a new file to add it to the depot
		/// <br/> 
		/// <br/>     p4 add [-c changelist#] [-d -f -n] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Open a file for adding to the depot.  If the file exists on the
		/// <br/> 	client, it is read to determine if it is text or binary. If it does
		/// <br/> 	not exist, it is assumed to be text.  To be added, the file must not
		/// <br/> 	already reside in the depot, or it must be deleted at the current
		/// <br/> 	head revision.  Files can be deleted and re-added.
		/// <br/> 
		/// <br/> 	To associate the open files with a specific pending changelist, use
		/// <br/> 	the -c flag; if you omit the -c flag, the open files are associated
		/// <br/> 	with the default changelist.  If file is already open, it is moved
		/// <br/> 	into the specified pending changelist.  You cannot reopen a file for
		/// <br/> 	add unless it is already open for add.
		/// <br/> 
		/// <br/> 	As a shortcut to reverting and re-adding, you can use the -d
		/// <br/> 	flag to reopen currently-open files for add (downgrade) under
		/// <br/> 	the following circumstances:
		/// <br/> 
		/// <br/> 	    A file that is 'opened for edit' and is synced to the head
		/// <br/> 	    revision, and the head revision has been deleted (or moved).
		/// <br/> 
		/// <br/> 	    A file that is 'opened for move/add' can be downgraded to add,
		/// <br/> 	    which is useful when the source of the move has been deleted
		/// <br/> 	    or moved.  Typically, under these circumstances, your only
		/// <br/> 	    alternative is to revert.  In this case, breaking the move
		/// <br/> 	    connection enables you to preserve any content changes in the
		/// <br/> 	    new file and safely revert the source file (of the move).
		/// <br/> 
		/// <br/> 	To specify file type, use the -t flag.  By default, 'p4 add'
		/// <br/> 	determines file type using the name-to-type mapping table managed
		/// <br/> 	by 'p4 typemap' and by examining the file's contents and execute
		/// <br/> 	permission bit. If the file type specified by -t or configured in
		/// <br/> 	the typemap table is a partial filetype, the resulting modifier is
		/// <br/> 	applied to the file type that is determined by 'p4 add'. For more
		/// <br/> 	details, see 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 	To add files with filenames that contain wildcard characters, specify
		/// <br/> 	the -f flag. Filenames that contain the special characters '@', '#',
		/// <br/> 	'%' or '*' are reformatted to encode the characters using ASCII
		/// <br/> 	hexadecimal representation.  After the files are added, you must
		/// <br/> 	refer to them using the reformatted file name, because Perforce
		/// <br/> 	does not recognize the local filesystem name.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the specified add operation without
		/// <br/> 	changing any files or metadata.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> AddFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("add", options, files);
		}
		public List<FileSpec> AddFiles(IList<FileSpec> toFiles, Options options)
		{
			return AddFiles(options, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help delete</b>
		/// <br/> 
		/// <br/>     delete -- Open an existing file for deletion from the depot
		/// <br/> 
		/// <br/>     p4 delete [-c changelist#] [-n -v -k] file ...
		/// <br/> 
		/// <br/> 	Opens a depot file for deletion.
		/// <br/> 	If the file is synced in the client workspace, it is removed.  If a
		/// <br/> 	pending changelist number is specified using with the -c flag, the
		/// <br/> 	file is opened for delete in that changelist. Otherwise, it is opened
		/// <br/> 	in the default pending changelist.
		/// <br/> 
		/// <br/> 	Files that are deleted generally do not appear on the have list.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the operation without changing any
		/// <br/> 	files or metadata.
		/// <br/> 
		/// <br/> 	The -v flag enables you to delete files that are not synced to the
		/// <br/> 	client workspace.
		/// <br/> 
		/// <br/> 	The -k flag performs the delete on the server without modifying
		/// <br/> 	client files.  Use with caution, as an incorrect delete can cause
		/// <br/> 	discrepancies between the state of the client and the corresponding
		/// <br/> 	server metadata.
		/// <br/> 
		/// </remarks>
		public List<FileSpec> DeleteFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("delete", options, files);
		}
		public List<FileSpec> DeleteFiles(IList<FileSpec> toFiles, Options options)
		{
			return DeleteFiles(options, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help edit</b>
		/// <br/> 
		/// <br/>     edit -- Open an existing file for edit
		/// <br/> 
		/// <br/>     p4 edit [-c changelist#] [-k -n] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Open an existing file for edit.  The server records the fact that
		/// <br/> 	the current user has opened the file in the current workspace, and
		/// <br/> 	changes the file permission from read-only to read/write.
		/// <br/> 
		/// <br/> 	If -c changelist# is included, the file opened in the specified
		/// <br/> 	pending changelist.  If changelist number is omitted, the file is
		/// <br/> 	opened in the 'default' changelist.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that Perforce
		/// <br/> 	filetype. Otherwise, the filetype of the previous revision is reused.
		/// <br/> 	If a partial filetype is specified, it is combined with the current
		/// <br/> 	filetype.For details, see 'p4 help filetypes'.
		/// <br/> 	Using a filetype of 'auto' will cause the filetype to be choosen
		/// <br/> 	as if the file were being added, that is the typemap will be
		/// <br/> 	considered and the file contents may be examined.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without changing any files or
		/// <br/> 	metadata.
		/// <br/> 
		/// <br/> 	The -k flag updates metadata without transferring files to the
		/// <br/> 	workspace. This option can be used to tell the server that files in
		/// <br/> 	a client workspace are already editable, even if they are not in the
		/// <br/> 	client view. Typically this flag is used to correct the Perforce
		/// <br/> 	server when it is wrong about the state of files in the client
		/// <br/> 	workspace, but incorrect use of this option can result in inaccurate
		/// <br/> 	file status information.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> EditFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("edit", options, files);
		}
		public List<FileSpec> EditFiles(IList<FileSpec> toFiles, Options options)
		{
			return EditFiles(options, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help have</b>
		/// <br/> 
		/// <br/>     have -- List the revisions most recently synced to the current workspace
		/// <br/> 
		/// <br/>     p4 have [file ...]
		/// <br/> 
		/// <br/> 	List revision numbers of the currently-synced files. If file name is
		/// <br/> 	omitted, list all files synced to this client workspace.
		/// <br/> 
		/// <br/> 	The format is:  depot-file#revision - client-file
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> GetSyncedFiles(Options options, params FileSpec[] files)
		{
			if (options != null)
			{
				throw new ArgumentException("GetSynchedFiles has no valid options", "options");
			}
			return runFileListCmd("have", options, files);
		}
		public List<FileSpec> GetSyncedFiles(IList<FileSpec> toFiles, Options options)
		{
			return GetSyncedFiles(options, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> IntegrateFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("integrate", options, files);
		}
		public List<FileSpec> IntegrateFiles(IList<FileSpec> toFiles, Options options)
		{
			return IntegrateFiles(options, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="fromFile"></param>
		/// <param name="toFiles"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> IntegrateFiles(FileSpec fromFile, Options options, params FileSpec[] toFiles)
		{
			FileSpec[] newParams = new FileSpec[toFiles.Length + 1];
			newParams[0] = fromFile;
			for (int idx = 0; idx < toFiles.Length; idx++)
			{
				newParams[idx + 1] = toFiles[idx];
			}
			return runFileListCmd("integrate", options, newParams);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="toFiles"></param>
		/// <param name="fromFile"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help integrate</b>
		/// <br/> 
		/// <br/>     integrate -- Integrate one set of files into another
		/// <br/> 
		/// <br/>     p4 integrate [options] fromFile[revRange] toFile
		/// <br/>     p4 integrate [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 integrate [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 integrate [options] -S stream [-r] [-P parent] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -d -f -h -i -o -n -m max -t -v
		/// <br/> 	         -D&lt;flags&gt; -R&lt;flags&gt;
		/// <br/> 
		/// <br/> 	'p4 integrate' integrates one set of files (the 'source') into
		/// <br/> 	another (the 'target'). 
		/// <br/> 
		/// <br/> 	(See also 'p4 merge' and 'p4 copy', variants of 'p4 integrate' that
		/// <br/> 	may be easier and more effective for the task at hand.) 
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 integrate' adds and
		/// <br/> 	deletes target files per changes in the source, and schedules all
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve
		/// <br/> 	filename and filetype changes. 'p4 submit' commits integrated files
		/// <br/> 	to the depot.  Unresolved files may not be submitted.  Integrations
		/// <br/> 	can be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display integration history.
		/// <br/> 
		/// <br/> 	When 'p4 integrate' schedules a workspace file to be resolved, it
		/// <br/> 	leaves it read-only. 'p4 resolve' can operate on a read-only file.
		/// <br/> 	For other pre-submit changes, 'p4 edit' must be used to make the
		/// <br/> 	file writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 integrate'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the
		/// <br/> 	scope of the target file set.
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used on
		/// <br/> 	toFile, it refers to source revisions, not to target revisions.  For
		/// <br/> 	details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 integrate' use a stream's branch view.  (See
		/// <br/> 	'p4 help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit integrated stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 integrate' use a user-defined branch view.
		/// <br/> 	(See 'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 	Note that 'p4 integrate' automatically adusts source-to-target
		/// <br/> 	mappings for moved and renamed files.  (Adjustment occurs only if
		/// <br/> 	the 'p4 move' command was used to move/rename files.) The scope of
		/// <br/> 	source and target file sets must include both the old-named and the
		/// <br/> 	new-named files for mappings to be adjusted.  Moved source files may
		/// <br/> 	cause target files to be scheduled for filename resolves.
		/// <br/> 
		/// <br/> 	The -f flag forces integrate to ignore integration history and treat
		/// <br/> 	all source revisions as unintegrated. It is meant to be used with
		/// <br/> 	revRange to force reintegration of specific, previously integrated
		/// <br/> 	revisions. 
		/// <br/> 
		/// <br/> 	The -i flag enables merging between files that have no prior
		/// <br/> 	integration history.  By default, 'p4 integrate' requires a prior
		/// <br/> 	integration in order to identify a base for merging.  The -i flag
		/// <br/> 	allows the integration, and schedules the target file to be resolved
		/// <br/> 	using the first source revision as the merge base.
		/// <br/> 
		/// <br/> 	The -o flag causes more merge information to be output.  For each
		/// <br/> 	target file scheduled to be resolved, the base file revision and the
		/// <br/> 	source file revision are shown. (After running 'p4 integrate', the
		/// <br/> 	same information is available from 'p4 resolve -o'.)
		/// <br/> 
		/// <br/> 	The -R flags modify the way resolves are scheduled:
		/// <br/> 
		/// <br/> 		-Rb	Schedules 'branch resolves' instead of branching new
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rd	Schedules 'delete resolves' instead of deleting
		/// <br/> 			target files automatically.
		/// <br/> 
		/// <br/> 		-Rs	Skips cherry-picked revisions already integrated.
		/// <br/> 			This can improve merge results, but can also cause
		/// <br/> 			multiple resolves per file to be scheduled.
		/// <br/> 
		/// <br/> 	The -D flags modify the way deleted files are treated:
		/// <br/> 
		/// <br/> 		-Dt     If the target file has been deleted and the source
		/// <br/> 			file has changed, re-branch the source file on top
		/// <br/> 			of the target file instead of scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Ds     If the source file has been deleted and the target
		/// <br/> 			file has changed, delete the target file instead of
		/// <br/> 			scheduling a resolve.
		/// <br/> 
		/// <br/> 		-Di	If the source file has been deleted and re-added,
		/// <br/> 			probe revisions that precede the deletion to find
		/// <br/> 			unintegrated revisions. By default, 'p4 integrate'
		/// <br/> 			starts probing at the last re-added revision.
		/// <br/> 
		/// <br/> 	The -d flag is a shorthand for all -D flags used together.
		/// <br/> 
		/// <br/> 	The -h flag leaves the target files at the revision currently synced
		/// <br/> 	to the client (the '#have' revision). By default, target files are
		/// <br/> 	automatically synced to the head revision by 'p4 integrate'.
		/// <br/> 
		/// <br/> 	The -t flag propagates source filetypes instead of scheduling
		/// <br/> 	filetype conflicts to be resolved.
		/// <br/> 
		/// <br/> 	The -m flag limits integration to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of integration, without actually
		/// <br/> 	doing anything.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the files are opened in the
		/// <br/> 	designated numbered pending changelist instead of the 'default'
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' integration that does not modify
		/// <br/> 	client workspace files unless target files need to be resolved.
		/// <br/> 	After submitting a virtual integration, 'p4 sync' can be used to
		/// <br/> 	update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> IntegrateFiles(IList<FileSpec> toFiles, FileSpec fromFile, Options options)
		{
			return IntegrateFiles(fromFile, options, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// </summary>
		/// <param name="files"></param>
		/// <param name="labelName"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help labelsync</b>
		/// <br/> 
		/// <br/>     labelsync -- Apply the label to the contents of the client workspace
		/// <br/> 
		/// <br/>     p4 labelsync [-a -d -n -q] -l label [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Labelsync causes the specified label to reflect the current contents
		/// <br/> 	of the client.  It records the revision of each file currently synced.
		/// <br/> 	The label's name can subsequently be used in a revision specification
		/// <br/> 	as @label to refer to the revision of a file as stored in the label.
		/// <br/> 
		/// <br/> 	Without a file argument, labelsync causes the label to reflect the
		/// <br/> 	contents of the whole client, by adding, deleting, and updating the
		/// <br/> 	label.  If a file is specified, labelsync updates the specified file.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specification, that revision
		/// <br/> 	is used instead of the revision synced by the client. If the specified
		/// <br/> 	revision is a deleted revision, the label includes that deleted
		/// <br/> 	revision.  See 'p4 help revisions' for details about specifying
		/// <br/> 	revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	The -a flag adds the specified file to the label.
		/// <br/> 
		/// <br/> 	The -d deletes the specified file from the label, regardless of
		/// <br/> 	revision.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering the label.
		/// <br/> 
		/// <br/> 	Only the owner of a label can run labelsync on that label. A label
		/// <br/> 	that has its Options: field set to 'locked' cannot be updated.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are displayed.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> LabelSync(Options options, string labelName, params FileSpec[] files)
		{
			if (String.IsNullOrEmpty(labelName))
			{
				throw new ArgumentNullException("labelName");
			}
			else
			{
				if (options == null)
				{
					options = new Options();
				}
				options["-l"] = labelName;
			}
			return runFileListCmd("labelsync", options, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="toFiles"></param>
		/// <param name="labelName"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help labelsync</b>
		/// <br/> 
		/// <br/>     labelsync -- Apply the label to the contents of the client workspace
		/// <br/> 
		/// <br/>     p4 labelsync [-a -d -n -q] -l label [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Labelsync causes the specified label to reflect the current contents
		/// <br/> 	of the client.  It records the revision of each file currently synced.
		/// <br/> 	The label's name can subsequently be used in a revision specification
		/// <br/> 	as @label to refer to the revision of a file as stored in the label.
		/// <br/> 
		/// <br/> 	Without a file argument, labelsync causes the label to reflect the
		/// <br/> 	contents of the whole client, by adding, deleting, and updating the
		/// <br/> 	label.  If a file is specified, labelsync updates the specified file.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specification, that revision
		/// <br/> 	is used instead of the revision synced by the client. If the specified
		/// <br/> 	revision is a deleted revision, the label includes that deleted
		/// <br/> 	revision.  See 'p4 help revisions' for details about specifying
		/// <br/> 	revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	The -a flag adds the specified file to the label.
		/// <br/> 
		/// <br/> 	The -d deletes the specified file from the label, regardless of
		/// <br/> 	revision.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering the label.
		/// <br/> 
		/// <br/> 	Only the owner of a label can run labelsync on that label. A label
		/// <br/> 	that has its Options: field set to 'locked' cannot be updated.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are displayed.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> LabelSync(IList<FileSpec> toFiles, string labelName, Options options)
		{
			return LabelSync(options, labelName, toFiles.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help lock</b>
		/// <br/> 
		/// <br/>     lock -- Lock an open file to prevent it from being submitted
		/// <br/> 
		/// <br/>     p4 lock [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	The specified files are locked in the depot, preventing any user
		/// <br/> 	other than the current user on the current client from submitting
		/// <br/> 	changes to the files.  If a file is already locked, the lock request
		/// <br/> 	is rejected.  If no file names are specified, all files in the
		/// <br/> 	specified changelist are locked. If changelist number is omitted,
		/// <br/> 	files in the default changelist are locked.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> LockFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("lock", options, files);
		}
		public List<FileSpec> LockFiles(IList<FileSpec> files, Options options)
		{
			return LockFiles(options, files.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help move</b>
		/// <br/> 
		/// <br/>     move -- move file(s) from one location to another
		/// <br/>     rename -- synonym for 'move'
		/// <br/> 
		/// <br/>     p4 move [-c changelist#] [-f -n -k] [-t filetype] fromFile toFile
		/// <br/> 
		/// <br/> 	Move takes an already opened file and moves it from one client
		/// <br/> 	location to another, reopening it as a pending depot move.  When
		/// <br/> 	the file is submitted with 'p4 submit', its depot file is moved
		/// <br/> 	accordingly.
		/// <br/> 
		/// <br/> 	Wildcards in fromFile and toFile must match. The fromFile must be
		/// <br/> 	a file open for add or edit.
		/// <br/> 
		/// <br/> 	'p4 opened' lists pending moves. 'p4 diff' can compare a moved
		/// <br/> 	client file with its depot original, 'p4 sync' can schedule an 
		/// <br/> 	update of a moved file, and 'p4 resolve' can resolve the update.
		/// <br/> 
		/// <br/> 	A client file can be moved many times before it is submitted.
		/// <br/> 	Moving a file back to its original location will undo a pending
		/// <br/> 	move, leaving unsubmitted content intact.  Using 'p4 revert'
		/// <br/> 	undoes the move and reverts the unsubmitted content.
		/// <br/> 
		/// <br/> 	If -c changelist# is specified, the file is reopened in the
		/// <br/> 	specified pending changelist as well as being moved.
		/// <br/> 
		/// <br/> 	The -f flag forces a move to an existing target file. The file
		/// <br/> 	must be synced and not opened.  The originating source file will
		/// <br/> 	no longer be synced to the client.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that filetype.
		/// <br/> 	If the filetype is a partial filetype, the partial filetype is
		/// <br/> 	combined with the current filetype.  See 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without moving files.
		/// <br/> 
		/// <br/> 	The -k flag performs the rename on the server without modifying
		/// <br/> 	client files. Use with caution, as an incorrect move can cause
		/// <br/> 	discrepancies between the state of the client and the corresponding
		/// <br/> 	server metadata.
		/// <br/> 
		/// <br/> 	The 'move' command requires a release 2009.1 or newer client. The
		/// <br/> 	'-f' flag requires a 2010.1 client.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> MoveFiles(FileSpec fromFile, FileSpec toFile, Options options)
		{
			return runFileListCmd("move", options, fromFile, toFile);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help reopen</b>
		/// <br/> 
		/// <br/>     reopen -- Change the filetype of an open file or move it to
		/// <br/>               another changelist
		/// <br/> 
		/// <br/>     p4 reopen [-c changelist#] [-t filetype] file ...
		/// <br/> 
		/// <br/> 	Reopen an open file for the current user in order to move it to a
		/// <br/> 	different changelist or change its filetype.
		/// <br/> 
		/// <br/> 	The target changelist must exist; you cannot create a changelist by
		/// <br/> 	reopening a file. To move a file to the default changelist, use
		/// <br/> 	'p4 reopen -c default'.
		/// <br/> 
		/// <br/> 	If -t filetype is specified, the file is assigned that filetype. If
		/// <br/> 	a partial filetype is specified, it is combined with the current
		/// <br/> 	filetype.  For details, see 'p4 help filetypes'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> ReopenFiles(Options options, params FileSpec[] files)
		{
			FileSpec[] temp = new P4.FileSpec[files.Length];
			for (int idx = 0; idx < files.Length; idx++)
			{
				temp[idx] = new P4.FileSpec(files[idx]);
				temp[idx].Version = null;
			}

			return runFileListCmd("reopen", options, temp);
		}
		public List<FileSpec> ReopenFiles(IList<FileSpec> files, Options options)
		{
			return ReopenFiles(options, files.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"><seealso cref="ResolveFilesOptions"/></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help resolve</b>
		/// <br/> 
		/// <br/>     resolve -- Resolve integrations and updates to workspace files
		/// <br/> 
		/// <br/>     p4 resolve [options] [file ...]
		/// <br/> 
		/// <br/> 	options: -A&lt;flags&gt; -a&lt;flags&gt; -d&lt;flags&gt; -f -n -N -o -t -v
		/// <br/> 		 -c changelist#
		/// <br/> 
		/// <br/> 	'p4 resolve' resolves changes to files in the client workspace.
		/// <br/> 	
		/// <br/> 	'p4 resolve' works only on files that have been scheduled to be 
		/// <br/> 	resolved.  The commands that can schedule resolves are: 'p4 sync',
		/// <br/> 	'p4 update', 'p4 submit', 'p4 merge', and 'p4 integrate'.  Files must
		/// <br/> 	be resolved before they can be submitted.
		/// <br/> 
		/// <br/> 	Resolving involves two sets of files, a source and a target.  The
		/// <br/> 	target is a set of depot files that maps to opened files in the
		/// <br/> 	client workspace.  When resolving an integration, the source is a
		/// <br/> 	different set of depot files than the target.  When resolving an
		/// <br/> 	update, the source is the same set of depot files as the target,
		/// <br/> 	at a different revision.
		/// <br/> 
		/// <br/> 	The 'p4 resolve' file argument specifies the target.  If the file
		/// <br/> 	argument is omitted, all unresolved files are resolved.
		/// <br/> 
		/// <br/> 	Resolving can modify workspace files. To back up files, use 'p4
		/// <br/> 	shelve' before using 'p4 resolve'.
		/// <br/> 
		/// <br/> 	The resolve process is a classic three-way merge. The participating
		/// <br/> 	files are referred to as follows:
		/// <br/> 
		/// <br/> 	  'yours'       The target file open in the client workspace
		/// <br/> 	  'theirs'      The source file in the depot
		/// <br/> 	  'base'        The common ancestor; the highest revision of the
		/// <br/> 	                source file already accounted for in the target.
		/// <br/> 	  'merged'      The merged result.
		/// <br/> 
		/// <br/> 	Filenames, filetypes, and text file content can be resolved by 
		/// <br/> 	accepting 'yours', 'theirs', or 'merged'.  Branching, deletion, and
		/// <br/> 	binary file content can be resolved by accepting either 'yours' or
		/// <br/> 	'theirs'.
		/// <br/> 
		/// <br/> 	When resolving integrated changes, 'p4 resolve' distinguishes among
		/// <br/> 	four results: entirely yours, entirely theirs, a pure merge, or an
		/// <br/> 	edited merge.  The distinction is recorded when resolved files are
		/// <br/> 	submitted, and will be used by future commands to determine whether
		/// <br/> 	integration is needed.
		/// <br/> 
		/// <br/> 	In all cases, accepting 'yours' leaves the target file in its current
		/// <br/> 	state.  The result of accepting 'theirs' is as follows:
		/// <br/> 
		/// <br/> 	   Content:     The target file content is overwritten.
		/// <br/>  	   Branching:	A new target is branched.
		/// <br/>  	   Deletion:    The target file is deleted.
		/// <br/>  	   Filename:	The target file is moved or renamed.
		/// <br/>  	   Filetype:    The target file's type is changed.
		/// <br/> 
		/// <br/> 	For each unresolved change, the user is prompted to accept a result.
		/// <br/> 	Content and non-content changes are resolved separately.  For content,
		/// <br/> 	'p4 resolve' places the merged result into a temporary file in the
		/// <br/> 	client workspace.  If there are any conflicts, the merged file contains
		/// <br/> 	conflict markers that must be removed by the user.
		/// <br/> 
		/// <br/> 	'p4 resolve' displays a count of text diffs and conflicts, and offers
		/// <br/> 	the following prompts:
		/// <br/> 
		/// <br/> 	  Accept:
		/// <br/> 	     at              Keep only changes to their file.
		/// <br/> 	     ay              Keep only changes to your file.
		/// <br/> 	   * am              Keep merged file.
		/// <br/> 	   * ae              Keep merged and edited file.
		/// <br/> 	   * a               Keep autoselected file.
		/// <br/> 
		/// <br/> 	  Diff:
		/// <br/> 	   * dt              See their changes alone.
		/// <br/> 	   * dy              See your changes alone.
		/// <br/> 	   * dm              See merged changes.
		/// <br/> 	     d               Diff your file against merged file.
		/// <br/> 
		/// <br/> 	  Edit:
		/// <br/> 	     et              Edit their file (read only).
		/// <br/> 	     ey              Edit your file (read/write).
		/// <br/> 	   * e               Edit merged file (read/write).
		/// <br/> 
		/// <br/> 	  Misc:
		/// <br/> 	   * m               Run '$P4MERGE base theirs yours merged'.
		/// <br/> 			     (Runs '$P4MERGEUNICODE charset base theirs
		/// <br/> 			      yours merged' if set and the file is a
		/// <br/> 			      unicode file.)
		/// <br/> 	     s               Skip this file.
		/// <br/> 	     h               Print this help message.
		/// <br/> 	     ^C              Quit the resolve operation.
		/// <br/> 
		/// <br/> 	Options marked (*) appear only for text files. The suggested action
		/// <br/> 	will be displayed in brackets. 
		/// <br/> 
		/// <br/> 	The 'merge' (m) option enables you to invoke your own merge program, if
		/// <br/> 	one is configured using the $P4MERGE environment variable.  Four files
		/// <br/> 	are passed to the program: the base, yours, theirs, and the temporary
		/// <br/> 	file. The program is expected to write merge results to the temporary
		/// <br/> 	file.
		/// <br/> 
		/// <br/> 	The -A flag can be used to limit the kind of resolving that will be
		/// <br/> 	attempted; without it, everything is attempted:
		/// <br/> 
		/// <br/> 	    -Ab		Resolve file branching.
		/// <br/> 	    -Ac		Resolve file content changes.
		/// <br/> 	    -Ad		Resolve file deletions.
		/// <br/> 	    -Am		Resolve moved and renamed files.
		/// <br/> 	    -At		Resolve filetype changes.
		/// <br/> 
		/// <br/> 	The -a flag puts 'p4 resolve' into automatic mode. The user is not
		/// <br/> 	prompted, and files that can't be resolved automatically are skipped:
		/// <br/> 
		/// <br/> 	    -as		'Safe' resolve; skip files that need merging.
		/// <br/> 	    -am 	Resolve by merging; skip files with conflicts.
		/// <br/> 	    -af		Force acceptance of merged files with conflicts.
		/// <br/> 	    -at		Force acceptance of theirs; overwrites yours.
		/// <br/> 	    -ay		Force acceptance of yours; ignores theirs.
		/// <br/> 
		/// <br/> 	The -as flag causes the workspace file to be replaced with their file
		/// <br/> 	only if theirs has changed and yours has not.
		/// <br/> 
		/// <br/> 	The -am flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours. If the merge detected conflicts, the
		/// <br/> 	file is left untouched and uresolved.
		/// <br/> 
		/// <br/> 	The -af flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours, even if there were conflicts.  This can
		/// <br/> 	leave conflict markers in workspace files.
		/// <br/> 
		/// <br/> 	The -at flag resolves all files by copying theirs into yours. It 
		/// <br/> 	should be used with care, as it overwrites any changes made to the
		/// <br/> 	file in the client workspace.
		/// <br/> 
		/// <br/> 	The -ay flag resolves all files by accepting yours and ignoring 
		/// <br/> 	theirs. It preserves the content of workspace files.
		/// <br/> 
		/// <br/> 	The -d flags can be used to control handling of whitespace and line
		/// <br/> 	endings when merging files:
		/// <br/> 
		/// <br/> 	    -db		Ingore whitespace changes.
		/// <br/> 	    -dw		Ingore whitespace altogether.
		/// <br/> 	    -dl 	Ignores line endings. 
		/// <br/> 
		/// <br/> 	The -d flags are also passed to the diff options in the 'p4 resolve'
		/// <br/> 	dialog. Additional -d flags that modify the diff output but do not 
		/// <br/> 	modify merge behavior include -dn (RCS), -dc (context), -ds (summary),
		/// <br/> 	and -du (unified). Note that 'p4 resolve' uses text from the client
		/// <br/> 	file if the files differ only in whitespace.
		/// <br/> 
		/// <br/> 	The -f flag enables previously resolved content to be resolved again.
		/// <br/> 	By default, after files have been resolved, 'p4 resolve' does not
		/// <br/> 	process them again.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering files.
		/// <br/> 
		/// <br/> 	The -N flag previews the operation with additional information about
		/// <br/> 	any non-content resolve actions that are scheduled.
		/// <br/> 
		/// <br/> 	The -o flag displays the base file name and revision to be used
		/// <br/> 	during the the merge.
		/// <br/> 
		/// <br/> 	The -t flag forces 'p4 resolve' to attempt a textual merge, even for
		/// <br/> 	files with non-text (binary) types.
		/// <br/> 
		/// <br/> 	The -v flag causes 'p4 resolve' to insert markers for all changes,
		/// <br/> 	not just conflicts.
		/// <br/> 
		/// <br/> 	The -c flag limits 'p4 resolve' to the files in changelist#.
		/// <br/> 
		/// <br/> 
		/// </remarks>

		///       Content:     The target file content is overwritten.
		///       Branching:   A new target is branched.
		///       Deletion:    The target file is deleted.
		///       Filename:    The target file is moved or renamed.
		///       Filetype:    The target file's type is changed.
		///
		///    For each unresolved change, the user is prompted to accept a result.
		///    Content and non-content changes are resolved separately.  For content,
		///    'p4 resolve' places the merged result into a temporary file in the
		///    client workspace.  If there are any conflicts, the merged file contains
		///    conflict markers that must be removed by the user.
		///
		///    'p4 resolve' displays a count of text diffs and conflicts, and offers
		///    the following prompts:
		///
		///      Accept:
		///         at              Keep only changes to their file.
		///         ay              Keep only changes to your file.
		///       * am              Keep merged file.
		///       * ae              Keep merged and edited file.
		///       * a               Keep autoselected file.
		///
		///      Diff:
		///       * dt              See their changes alone.
		///       * dy              See your changes alone.
		///       * dm              See merged changes.
		///         d               Diff your file against merged file.
		///
		///      Edit:
		///         et              Edit their file (read only).
		///         ey              Edit your file (read/write).
		///       * e               Edit merged file (read/write).
		///
		///      Misc:
		///       * m               Run '$P4MERGE base theirs yours merged'.
		///                         (Runs '$P4MERGEUNICODE charset base theirs
		///                          yours merged' if set and the file is a
		///                          unicode file.)
		///         s               Skip this file.
		///         h               Print this help message.
		///         ^C              Quit the resolve operation.

		///    Options marked (*) appear only for text files. The suggested action
		///    will be displayed in brackets.
		///
		///    The 'merge' (m) option enables you to invoke your own merge program, if
		///    one is configured using the $P4MERGE environment variable.  Four files
		///    are passed to the program: the base, yours, theirs, and the temporary
		///    file. The program is expected to write merge results to the temporary
		///    file.
		///
		///    The -A flag can be used to limit the kind of resolving that will be
		///    attempted; without it, everything is attempted:
		///
		///        -Ab         Resolve file branching.
		///        -Ac         Resolve file content changes.
		///        -Ad         Resolve file deletions.
		///        -Am         Resolve moved and renamed files.
		///        -At         Resolve filetype changes.
		///
		///    The -a flag puts 'p4 resolve' into automatic mode. The user is not
		///    prompted, and files that can't be resolved automatically are skipped:
		///
		///        -as         'Safe' resolve; skip files that need merging.
		///        -am         Resolve by merging; skip files with conflicts.
		///        -af         Force acceptance of merged files with conflicts.
		///        -at         Force acceptance of theirs; overwrites yours.
		///        -ay         Force acceptance of yours; ignores theirs.
		///
		///    The -as flag causes the workspace file to be replaced with their file
		///    only if theirs has changed and yours has not.
		///
		///    The -am flag causes the workspace file to be replaced with the result
		///    of merging theirs with yours. If the merge detected conflicts, the
		///    file is left untouched and unresolved.
		///
		///    The -af flag causes the workspace file to be replaced with the result
		///    of merging theirs with yours, even if there were conflicts.  This can
		///    leave conflict markers in workspace files.
		///
		///    The -at flag resolves all files by copying theirs into yours. It
		///    should be used with care, as it overwrites any changes made to the
		///    file in the client workspace.
		///
		///    The -ay flag resolves all files by accepting yours and ignoring
		///    theirs. It preserves the content of workspace files.
		///
		///     The -d flags can be used to control handling of whitespace and line
		///     endings when merging files:
		/// 
		///         -db         Ignore whitespace changes.
		///         -dw         Ignore whitespace altogether.
		///         -dl         Ignores line endings.
		/// 
		///     The -d flags are also passed to the diff options in the 'p4 resolve'
		///     dialog. Additional -d flags that modify the diff output but do not
		///     modify merge behavior include -dn (RCS), -dc (context), -ds (summary),
		///     and -du (unified). Note that 'p4 resolve' uses text from the client
		///     file if the files differ only in whitespace.
		/// 
		///     The -f flag enables previously resolved files to be resolved again.
		///     By default, after files have been resolved, 'p4 resolve' does not
		///     process them again.
		/// 
		///     The -n flag previews the operation without altering files.
		/// 
		///     The -N flag previews the operation with additional information about
		///     any non-content resolve actions that are scheduled.
		/// 
		///     The -o flag displays the base file name and revision to be used
		///     during the the merge.
		/// 
		///     The -t flag forces 'p4 resolve' to attempt a textual merge, even for
		///     files with non-text (binary) types.
		/// 
		///     The -v flag causes 'p4 resolve' to insert markers for all changes,
		///     not just conflicts.
		/// 
		///     The -c flag limits 'p4 resolve' to the files in changelist#.
		/// </remarks>
		public List<FileResolveRecord> ResolveFiles(Options options, params FileSpec[] files)
		{
			return ResolveFiles(null, options, files);
		}

		List<FileResolveRecord> CurrentResolveRecords = null;

		FileResolveRecord CurrentResolveRecord = null;

		public delegate P4.P4ClientMerge.MergeStatus AutoResolveDelegate(P4.P4ClientMerge.MergeForce mergeForce);

		public delegate P4.P4ClientMerge.MergeStatus ResolveFileDelegate(FileResolveRecord resolveRecord,
			AutoResolveDelegate AutoResolve, string sourcePath, string targetPath, string basePath, string resultsPath);

		private ResolveFileDelegate ResolveFileHandler = null;

		P4ClientMerge.MergeForce ForceMerge = P4ClientMerge.MergeForce.CMF_AUTO;

		private P4.P4ClientMerge.MergeStatus HandleResolveFile(uint cmdId, P4.P4ClientMerge cm)
		{
			if (CurrentResolveRecord.Analysis == null)
			{
				CurrentResolveRecord.Analysis = new ResolveAnalysis();
			}
			// this is from a content resolve
			CurrentResolveRecord.Analysis.SetResolveType(ResolveType.Content);

			CurrentResolveRecord.Analysis.SourceDiffCnt = cm.GetYourChunks();
			CurrentResolveRecord.Analysis.TargetDiffCnt = cm.GetTheirChunks();
			CurrentResolveRecord.Analysis.CommonDiffCount = cm.GetBothChunks();
			CurrentResolveRecord.Analysis.ConflictCount = cm.GetConflictChunks();

			CurrentResolveRecord.Analysis.SuggestedAction = cm.AutoResolve(P4ClientMerge.MergeForce.CMF_AUTO);

			if ((ResolveFileHandler != null) && (CurrentResolveRecord != null))
			{
				try
				{
					return ResolveFileHandler(CurrentResolveRecord, new AutoResolveDelegate(cm.AutoResolve),
						cm.GetTheirFile(), cm.GetYourFile(), cm.GetBaseFile(), cm.GetResultFile());
				}
				catch (Exception ex)
				{
					LogFile.LogException("Error", ex);

					return P4ClientMerge.MergeStatus.CMS_SKIP;
				}
			}
			return P4ClientMerge.MergeStatus.CMS_SKIP;
		}

		private P4.P4ClientMerge.MergeStatus HandleResolveAFile(uint cmdId, P4.P4ClientResolve cr)
		{
			string strType = cr.ResolveType;

			if (strType.Contains("resolve"))
			{
				strType = strType.Replace("resolve", string.Empty).Trim();
				if ((strType == "Rename") || (strType == "Filename"))
				{
					strType = "Move";
				}
			}
			if (CurrentResolveRecord.Analysis == null)
			{
				CurrentResolveRecord.Analysis = new ResolveAnalysis();
			}
			// this is likely from an action resolve
			CurrentResolveRecord.Analysis.SetResolveType(strType);

			CurrentResolveRecord.Analysis.Options = ResolveOptions.Skip; // can always skip
			//string strOptions = cr.Prompt;
			//int idx1 = strOptions.IndexOf("Accept(");
			//if (idx1 >= 0)
			//{
			//    idx1 += 7;

			//    int idx2 = strOptions.IndexOf(")", idx1);
			//    if (idx2 > idx1)
			//    {
			//        string str1 = strOptions.Substring(idx1, idx2 - idx1).Trim('(', ')');

			//        string[] parts = str1.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
			//        foreach (string part in parts)
			//        {
			//            if (part == "at")
			//            {
			//                CurrentResolveRecord.Analysis.Options |= ResolveOptions.AccecptTheirs;
			//                continue;
			//            }
			//            if (part == "ay")
			//            {
			//                CurrentResolveRecord.Analysis.Options |= ResolveOptions.AcceptYours;
			//                continue;
			//            }
			//            if (part == "m")
			//            {
			//                CurrentResolveRecord.Analysis.Options |= ResolveOptions.Merge;
			//                continue;
			//            }
			//            if (part == "am")
			//            {
			//                CurrentResolveRecord.Analysis.Options |= ResolveOptions.AcceptMerged;
			//                continue;
			//            }
			//            if (part == "a")
			//            {
			//                CurrentResolveRecord.Analysis.Options |= ResolveOptions.AutoSelect;
			//                continue;
			//            }
			//        }
			//    }
			//}

			if (string.IsNullOrEmpty(cr.MergeAction) == false)
			{
				CurrentResolveRecord.Analysis.Options |= ResolveOptions.Merge;
				CurrentResolveRecord.Analysis.MergeAction = cr.MergeAction;
			}

			if (string.IsNullOrEmpty(cr.TheirAction) == false)
			{
				CurrentResolveRecord.Analysis.Options |= ResolveOptions.AccecptTheirs;
				CurrentResolveRecord.Analysis.TheirsAction = cr.TheirAction;
			}

			if (string.IsNullOrEmpty(cr.YoursAction) == false)
			{
				CurrentResolveRecord.Analysis.Options |= ResolveOptions.AcceptYours;
				CurrentResolveRecord.Analysis.YoursAction = cr.YoursAction;
			}

			// this is likely from an action resolve
			CurrentResolveRecord.Analysis.SetResolveType(strType);

			CurrentResolveRecord.Analysis.SuggestedAction = cr.AutoResolve(P4ClientMerge.MergeForce.CMF_AUTO);

			if ((ResolveFileHandler != null) && (CurrentResolveRecord != null))
			{
				try
				{
					return ResolveFileHandler(CurrentResolveRecord, new AutoResolveDelegate(cr.AutoResolve),
						null, null, null, null);
				}
				catch (Exception ex)
				{
					LogFile.LogException("Error", ex);

					return P4ClientMerge.MergeStatus.CMS_SKIP;
				}
			}
			return P4ClientMerge.MergeStatus.CMS_SKIP;
		}

		private P4.P4Server.TaggedOutputDelegate ResultsTaggedOutputHandler = null;

		private void ResultsTaggedOutputReceived(uint cmdId, int ObjId, TaggedObject Obj)
		{
			if (CurrentResolveRecord != null)
			{
				CurrentResolveRecords.Add(CurrentResolveRecord);
				CurrentResolveRecord = null;
			}
			//Create a record for this file resolve results
			CurrentResolveRecord = FileResolveRecord.FromResolveCmdTaggedOutput(Obj);

			//if (CurrentResolveRecord == null)
			//{
			//    //Create a record for this file resolve results
			//    CurrentResolveRecord = FileResolveRecord.FromResolveCmdTaggedOutput(Obj);
			//}
			//else
			//{
			//    //additional results for this command
			//    P4.FileResolveRecord r2 = FileResolveRecord.FromResolveCmdTaggedOutput(Obj);
			//    P4.FileResolveRecord.MergeRecords(CurrentResolveRecord, r2);
			//}
		}

		/// <summary>
		/// Resolve files
		/// </summary>
		/// <param name="resolveHandler"></param>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// The caller must either 
		/// 1) set an automatic resolution (-as, -am.-af, -at, or -ay), 
		/// 2) provide a callback function of type <see cref="P4Server.PromptHandlerDelegate"/> to
		/// respond to the prompts, or 3) provide a dictionary which contains responses to the prompts.
		/// <br/>
		/// <br/><b>p4 help resolve</b>
		/// <br/> 
		/// <br/>     resolve -- Resolve integrations and updates to workspace files
		/// <br/> 
		/// <br/>     p4 resolve [options] [file ...]
		/// <br/> 
		/// <br/> 	options: -A&lt;flags&gt; -a&lt;flags&gt; -d&lt;flags&gt; -f -n -N -o -t -v
		/// <br/> 		 -c changelist#
		/// <br/> 
		/// <br/> 	'p4 resolve' resolves changes to files in the client workspace.
		/// <br/> 	
		/// <br/> 	'p4 resolve' works only on files that have been scheduled to be 
		/// <br/> 	resolved.  The commands that can schedule resolves are: 'p4 sync',
		/// <br/> 	'p4 update', 'p4 submit', 'p4 merge', and 'p4 integrate'.  Files must
		/// <br/> 	be resolved before they can be submitted.
		/// <br/> 
		/// <br/> 	Resolving involves two sets of files, a source and a target.  The
		/// <br/> 	target is a set of depot files that maps to opened files in the
		/// <br/> 	client workspace.  When resolving an integration, the source is a
		/// <br/> 	different set of depot files than the target.  When resolving an
		/// <br/> 	update, the source is the same set of depot files as the target,
		/// <br/> 	at a different revision.
		/// <br/> 
		/// <br/> 	The 'p4 resolve' file argument specifies the target.  If the file
		/// <br/> 	argument is omitted, all unresolved files are resolved.
		/// <br/> 
		/// <br/> 	Resolving can modify workspace files. To back up files, use 'p4
		/// <br/> 	shelve' before using 'p4 resolve'.
		/// <br/> 
		/// <br/> 	The resolve process is a classic three-way merge. The participating
		/// <br/> 	files are referred to as follows:
		/// <br/> 
		/// <br/> 	  'yours'       The target file open in the client workspace
		/// <br/> 	  'theirs'      The source file in the depot
		/// <br/> 	  'base'        The common ancestor; the highest revision of the
		/// <br/> 	                source file already accounted for in the target.
		/// <br/> 	  'merged'      The merged result.
		/// <br/> 
		/// <br/> 	Filenames, filetypes, and text file content can be resolved by 
		/// <br/> 	accepting 'yours', 'theirs', or 'merged'.  Branching, deletion, and
		/// <br/> 	binary file content can be resolved by accepting either 'yours' or
		/// <br/> 	'theirs'.
		/// <br/> 
		/// <br/> 	When resolving integrated changes, 'p4 resolve' distinguishes among
		/// <br/> 	four results: entirely yours, entirely theirs, a pure merge, or an
		/// <br/> 	edited merge.  The distinction is recorded when resolved files are
		/// <br/> 	submitted, and will be used by future commands to determine whether
		/// <br/> 	integration is needed.
		/// <br/> 
		/// <br/> 	In all cases, accepting 'yours' leaves the target file in its current
		/// <br/> 	state.  The result of accepting 'theirs' is as follows:
		/// <br/> 
		/// <br/> 	   Content:     The target file content is overwritten.
		/// <br/>  	   Branching:	A new target is branched.
		/// <br/>  	   Deletion:    The target file is deleted.
		/// <br/>  	   Filename:	The target file is moved or renamed.
		/// <br/>  	   Filetype:    The target file's type is changed.
		/// <br/> 
		/// <br/> 	For each unresolved change, the user is prompted to accept a result.
		/// <br/> 	Content and non-content changes are resolved separately.  For content,
		/// <br/> 	'p4 resolve' places the merged result into a temporary file in the
		/// <br/> 	client workspace.  If there are any conflicts, the merged file contains
		/// <br/> 	conflict markers that must be removed by the user.
		/// <br/> 
		/// <br/> 	'p4 resolve' displays a count of text diffs and conflicts, and offers
		/// <br/> 	the following prompts:
		/// <br/> 
		/// <br/> 	  Accept:
		/// <br/> 	     at              Keep only changes to their file.
		/// <br/> 	     ay              Keep only changes to your file.
		/// <br/> 	   * am              Keep merged file.
		/// <br/> 	   * ae              Keep merged and edited file.
		/// <br/> 	   * a               Keep autoselected file.
		/// <br/> 
		/// <br/> 	  Diff:
		/// <br/> 	   * dt              See their changes alone.
		/// <br/> 	   * dy              See your changes alone.
		/// <br/> 	   * dm              See merged changes.
		/// <br/> 	     d               Diff your file against merged file.
		/// <br/> 
		/// <br/> 	  Edit:
		/// <br/> 	     et              Edit their file (read only).
		/// <br/> 	     ey              Edit your file (read/write).
		/// <br/> 	   * e               Edit merged file (read/write).
		/// <br/> 
		/// <br/> 	  Misc:
		/// <br/> 	   * m               Run '$P4MERGE base theirs yours merged'.
		/// <br/> 			     (Runs '$P4MERGEUNICODE charset base theirs
		/// <br/> 			      yours merged' if set and the file is a
		/// <br/> 			      unicode file.)
		/// <br/> 	     s               Skip this file.
		/// <br/> 	     h               Print this help message.
		/// <br/> 	     ^C              Quit the resolve operation.
		/// <br/> 
		/// <br/> 	Options marked (*) appear only for text files. The suggested action
		/// <br/> 	will be displayed in brackets. 
		/// <br/> 
		/// <br/> 	The 'merge' (m) option enables you to invoke your own merge program, if
		/// <br/> 	one is configured using the $P4MERGE environment variable.  Four files
		/// <br/> 	are passed to the program: the base, yours, theirs, and the temporary
		/// <br/> 	file. The program is expected to write merge results to the temporary
		/// <br/> 	file.
		/// <br/> 
		/// <br/> 	The -A flag can be used to limit the kind of resolving that will be
		/// <br/> 	attempted; without it, everything is attempted:
		/// <br/> 
		/// <br/> 	    -Ab		Resolve file branching.
		/// <br/> 	    -Ac		Resolve file content changes.
		/// <br/> 	    -Ad		Resolve file deletions.
		/// <br/> 	    -Am		Resolve moved and renamed files.
		/// <br/> 	    -At		Resolve filetype changes.
		/// <br/> 
		/// <br/> 	The -a flag puts 'p4 resolve' into automatic mode. The user is not
		/// <br/> 	prompted, and files that can't be resolved automatically are skipped:
		/// <br/> 
		/// <br/> 	    -as		'Safe' resolve; skip files that need merging.
		/// <br/> 	    -am 	Resolve by merging; skip files with conflicts.
		/// <br/> 	    -af		Force acceptance of merged files with conflicts.
		/// <br/> 	    -at		Force acceptance of theirs; overwrites yours.
		/// <br/> 	    -ay		Force acceptance of yours; ignores theirs.
		/// <br/> 
		/// <br/> 	The -as flag causes the workspace file to be replaced with their file
		/// <br/> 	only if theirs has changed and yours has not.
		/// <br/> 
		/// <br/> 	The -am flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours. If the merge detected conflicts, the
		/// <br/> 	file is left untouched and uresolved.
		/// <br/> 
		/// <br/> 	The -af flag causes the workspace file to be replaced with the result
		/// <br/> 	of merging theirs with yours, even if there were conflicts.  This can
		/// <br/> 	leave conflict markers in workspace files.
		/// <br/> 
		/// <br/> 	The -at flag resolves all files by copying theirs into yours. It 
		/// <br/> 	should be used with care, as it overwrites any changes made to the
		/// <br/> 	file in the client workspace.
		/// <br/> 
		/// <br/> 	The -ay flag resolves all files by accepting yours and ignoring 
		/// <br/> 	theirs. It preserves the content of workspace files.
		/// <br/> 
		/// <br/> 	The -d flags can be used to control handling of whitespace and line
		/// <br/> 	endings when merging files:
		/// <br/> 
		/// <br/> 	    -db		Ingore whitespace changes.
		/// <br/> 	    -dw		Ingore whitespace altogether.
		/// <br/> 	    -dl 	Ignores line endings. 
		/// <br/> 
		/// <br/> 	The -d flags are also passed to the diff options in the 'p4 resolve'
		/// <br/> 	dialog. Additional -d flags that modify the diff output but do not 
		/// <br/> 	modify merge behavior include -dn (RCS), -dc (context), -ds (summary),
		/// <br/> 	and -du (unified). Note that 'p4 resolve' uses text from the client
		/// <br/> 	file if the files differ only in whitespace.
		/// <br/> 
		/// <br/> 	The -f flag enables previously resolved content to be resolved again.
		/// <br/> 	By default, after files have been resolved, 'p4 resolve' does not
		/// <br/> 	process them again.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without altering files.
		/// <br/> 
		/// <br/> 	The -N flag previews the operation with additional information about
		/// <br/> 	any non-content resolve actions that are scheduled.
		/// <br/> 
		/// <br/> 	The -o flag displays the base file name and revision to be used
		/// <br/> 	during the the merge.
		/// <br/> 
		/// <br/> 	The -t flag forces 'p4 resolve' to attempt a textual merge, even for
		/// <br/> 	files with non-text (binary) types.
		/// <br/> 
		/// <br/> 	The -v flag causes 'p4 resolve' to insert markers for all changes,
		/// <br/> 	not just conflicts.
		/// <br/> 
		/// <br/> 	The -c flag limits 'p4 resolve' to the files in changelist#.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileResolveRecord> ResolveFiles(
			ResolveFileDelegate resolveHandler,
			Options options,
			params FileSpec[] files)
		{
			CurrentResolveRecords = new List<FileResolveRecord>();

			try
			{
				string[] paths = null;
				try
				{
					IList<FileSpec> clientFiles = runFileListCmd("files", null, files);

					paths = FileSpec.ToEscapedPaths(clientFiles.ToArray());
				}
				catch
				{
					//try
					//{
					//    IList<FileSpec> clientFiles = runFileListCmd("fstat", null, files);

					//    paths = FileSpec.ToEscapedPaths(clientFiles.ToArray());
					//}
					//catch
					//{
						paths = FileSpec.ToEscapedPaths(files);
					//}
				}
				ResultsTaggedOutputHandler = new P4Server.TaggedOutputDelegate(ResultsTaggedOutputReceived);

				Connection._p4server.TaggedOutputReceived += ResultsTaggedOutputHandler;

				ResolveFileHandler = resolveHandler;

				foreach (string path in paths)
				{
					P4Command cmd = new P4Command(Connection, "resolve", true, path);

					cmd.CmdResolveHandler = new P4Server.ResolveHandlerDelegate(HandleResolveFile);
					cmd.CmdResolveAHandler = new P4Server.ResolveAHandlerDelegate(HandleResolveAFile);

					//if (((options.Flags & ResolveFilesFlags.aFlags) == 0) &&
					//    ( opts.PromptResponses == null) &&
					//    ( opts.PromptHandler == null))
					//{
					//    throw new ArgumentException("Must provide either AutomaticResolve mode, Prompt Response dictionary, or a Prompt Handler");
					//}

					// If auto

					//if (options.ContainsKey("-as"))
					//{
					//    ForceMerge = P4ClientMerge.MergeForce.CMF_SAFE;
					//}
					//if (options.ContainsKey("-af"))
					//{
					//    ForceMerge = P4ClientMerge.MergeForce.CMF_FORCE;
					//}
					//else
					//{
					//    ForceMerge = P4ClientMerge.MergeForce.CMF_AUTO;
					//}
					CurrentResolveRecord = null;

					P4CommandResult results = cmd.Run(options);
					if (results.Success)
					{
						if (CurrentResolveRecord != null)
						{
							CurrentResolveRecords.Add(CurrentResolveRecord);
							CurrentResolveRecord = null;
						}
						else
						{
							if (results.ErrorList[0].ErrorMessage.Contains("no file(s) to resolve"))
							{
								continue;
							}
							// not in interactive mode
							FileResolveRecord  record = null;

							if ((results.TaggedOutput != null) && (results.TaggedOutput.Count > 0))
							{
								foreach (TaggedObject obj in results.TaggedOutput)
								{
									record = new FileResolveRecord(obj);
									if (record != null)
									{
										CurrentResolveRecords.Add(record);
									}
								}
							}
						}
					}
					else
					{
						P4Exception.Throw(results.ErrorList);
					}
					//else
					//{
						//if (CurrentResolveRecord != null)
						//{
						//    if ((results.InfoOutput != null) && (results.InfoOutput.Count > 0))
						//    {
						//        FileResolveRecord record2 = null;

						//        record2 = FileResolveRecord.FromMergeInfo(results.InfoOutput);

						//        if (record2 != null)
						//        {
						//            FileResolveRecord.MergeRecords(CurrentResolveRecord, record2);
						//        }
						//    }
						//    records.Add(CurrentResolveRecord);
						//}
						//else if ((results.InfoOutput != null) && (results.InfoOutput.Count > 0))
						//{
						//    FileResolveRecord record2 = null;

						//    record2 = FileResolveRecord.FromMergeInfo(results.InfoOutput);

						//    if (record2 != null)
						//    {
						//        records.Add(record2);
						//    }
						//}
						//else if ((results.ErrorList != null) && (results.ErrorList.Count > 0))
						//{
						//    FileResolveRecord record2 = new FileResolveRecord();
						//    record2.Result = results.ErrorList[0].ErrorMessage;
						//    records.Add(record2);
						//}
					//}
				}
				if (CurrentResolveRecords.Count > 0)
				{
					return CurrentResolveRecords;
				}
				return null;
			}
			finally
			{
				if (ResultsTaggedOutputHandler != null)
				{
					Connection._p4server.TaggedOutputReceived -= ResultsTaggedOutputHandler;
				}
			}
		}

		[Obsolete("This version of resolve is superseded ")]
		internal List<FileResolveRecord> ResolveFiles(P4Server.ResolveHandlerDelegate resolveHandler,
															P4Server.PromptHandlerDelegate promptHandler,
															Dictionary<String, String> promptResponses,
															Options options,
															params FileSpec[] files)
		{
			string[] paths = FileSpec.ToEscapedPaths(files);
			P4Command cmd = new P4Command(Connection, "resolve", true, paths);

			//if (((options.Flags & ResolveFilesFlags.aFlags) == 0) &&
			//    ( opts.PromptResponses == null) &&
			//    ( opts.PromptHandler == null))
			//{
			//    throw new ArgumentException("Must provide either AutomaticResolve mode, Prompt Response dictionary, or a Prompt Handler");
			//}
			if (resolveHandler != null)
			{
				cmd.CmdResolveHandler = resolveHandler;
			}
			else if (promptHandler != null)
			{
				cmd.CmdPromptHandler = promptHandler;
			}
			else if (promptResponses != null)
			{
				cmd.Responses = promptResponses;
			}

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				Dictionary<string, FileResolveRecord> recordMap = new Dictionary<string, FileResolveRecord>();
				List<FileResolveRecord> records = new List<FileResolveRecord>();
				if ((results.TaggedOutput != null) && (results.TaggedOutput.Count > 0))
				{
					foreach (TaggedObject obj in results.TaggedOutput)
					{
						FileResolveRecord record1 = FileResolveRecord.FromResolveCmdTaggedOutput(obj);
						records.Add(record1);
						if (record1.LocalFilePath != null)
						{
							recordMap[record1.LocalFilePath.Path.ToLower()] = record1;
						}
					}
				}
				if ((results.InfoOutput != null) && (results.InfoOutput.Count > 0))
				{
					string l1 = null;
					string l2 = null;
					string l3 = null;

					FileResolveRecord record2 = null;
					int RecordsPerItem = results.InfoOutput.Count / files.Length;
					for (int idx = 0; idx < results.InfoOutput.Count; idx += RecordsPerItem)
					{
						l1 = results.InfoOutput[idx].Info;
						if (RecordsPerItem == 3)
						{
							l2 = results.InfoOutput[idx + 1].Info;
							l3 = results.InfoOutput[idx + 2].Info;
						}
						if (RecordsPerItem == 2)
						{
							l2 = null;
							l3 = results.InfoOutput[idx + 1].Info;
						}
						record2 = FileResolveRecord.FromMergeInfo(l1, l2, l3);
						if ((record2 != null) && (recordMap.ContainsKey(record2.LocalFilePath.Path.ToLower())))
						{
							FileResolveRecord record1 = recordMap[record2.LocalFilePath.Path.ToLower()];
							FileResolveRecord.MergeRecords(record1, record2);
						}
						else
						{
							records.Add(record2);
						}
					}
				}
				return records;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}

			return null;
		}
		public List<FileResolveRecord> ResolveFiles(IList<FileSpec> files, Options options)
		{
			return ResolveFiles(options, files.ToArray<FileSpec>());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help submit</b>
		/// <br/> 
		/// <br/>     submit -- Submit open files to the depot
		/// <br/> 
		/// <br/>     p4 submit [-r -s -f option]
		/// <br/>     p4 submit [-r -s -f option] file
		/// <br/>     p4 submit [-r -f option] -d description
		/// <br/>     p4 submit [-r -f option] -d description file
		/// <br/>     p4 submit [-r -f option] -c changelist#
		/// <br/>     p4 submit -i [-r -s -f option]
		/// <br/> 
		/// <br/> 	'p4 submit' commits a pending changelist and its files to the depot.
		/// <br/> 
		/// <br/> 	By default, 'p4 submit' attempts to submit all files in the 'default'
		/// <br/> 	changelist.  Submit displays a dialog where you enter a description
		/// <br/> 	of the change and, optionally, delete files from the list of files
		/// <br/> 	to be checked in. 
		/// <br/> 
		/// <br/> 	To add files to a changelist before submitting, use any of the 
		/// <br/> 	commands that open client workspace files: 'p4 add', 'p4 edit',
		/// <br/> 	etc.
		/// <br/> 
		/// <br/> 	If the file parameter is specified, only files in the default
		/// <br/> 	changelist that match the pattern are submitted.
		/// <br/> 
		/// <br/> 	Files in a stream path can be submitted only by client workspaces
		/// <br/> 	dedicated to the stream. See 'p4 help client'.
		/// <br/> 
		/// <br/> 	Before committing a changelist, 'p4 submit' locks all the files being
		/// <br/> 	submitted. If any file cannot be locked or submitted, the files are 
		/// <br/> 	left open in a numbered pending changelist. 'p4 opened' shows
		/// <br/> 	unsubmitted files and their changelists.
		/// <br/> 
		/// <br/> 	Submit is atomic: if the operation succeeds, all files are updated
		/// <br/> 	in the depot. If the submit fails, no depot files are updated.
		/// <br/> 
		/// <br/> 	The -c flag submits the specified pending changelist instead of the
		/// <br/> 	default changelist. Additional changelists can be created manually, 
		/// <br/> 	using the 'p4 change' command, or automatically as the result of a 
		/// <br/> 	failed attempt to submit the default changelist.
		/// <br/> 
		/// <br/> 	The -d flag passes a description into the specified changelist rather
		/// <br/> 	than displaying the changelist dialog for manual editing. This option
		/// <br/> 	is useful for scripting, but does not allow you to add jobs or modify
		/// <br/> 	the default changelist.
		/// <br/> 
		/// <br/> 	The -f flag enables you to override submit options that are configured
		/// <br/> 	for the client that is submitting the changelist.  This flag overrides
		/// <br/> 	the -r (reopen)flag, if it is specified.  See 'p4 help client' for
		/// <br/> 	details about submit options.
		/// <br/> 
		/// <br/> 	The -i flag reads a changelist specification from the standard input.
		/// <br/> 	The user's editor is not invoked.
		/// <br/> 
		/// <br/> 	The -r flag reopens submitted files in the default changelist after
		/// <br/> 	submission.
		/// <br/> 
		/// <br/> 	The -s flag extends the list of jobs to include the fix status
		/// <br/> 	for each job, which becomes the job's status when the changelist
		/// <br/> 	is committed.  See 'p4 help change' for details.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public SubmitResults SubmitFiles(Options options, FileSpec file)
		{
			P4Command cmd = null;
			if (file != null)
			{
				cmd = new P4Command(Connection, "submit", true, file.ToEscapedString());
			}
			else
			{
				cmd = new P4Command(Connection, "submit", true);
			}
			if (options != null&&!options.ContainsKey("-e"))
			{
				//the new Changelist Spec is passed using the command dataset
				if (options.ContainsKey("-i"))
				{
					cmd.DataSet =  options["-i"];
					options["-i"] = null;
				}
			}

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
				{
					return null;
				}
				SubmitResults returnVal = new SubmitResults();
				foreach (TaggedObject obj in results.TaggedOutput)
				{
					if (obj.ContainsKey("submittedChange"))
					{
						int i = -1;
						// the changelist number after the submit
						if (int.TryParse(obj["submittedChange"], out i))
							returnVal.ChangeIdAfterSubmit = i;
					}
					else if (obj.ContainsKey("change"))
					{
						// The changelist use by the submit
						int i = -1;
						if (int.TryParse(obj["change"], out i))
							returnVal.ChangeIdBeforeSubmit = 1;
						if (obj.ContainsKey("locked"))
						{
							if (int.TryParse(obj["locked"], out i))
							{
								returnVal.FilesLockedBySubmit = i;
							}
						}
					}
					else
					{
						// a file in the submit
						StringEnum<FileAction> action = null;
						if (obj.ContainsKey("action"))
						{
							action = obj["action"];
						}
						FileSpec spec = null;
						int rev = -1;
						string p;

						DepotPath dp = null;
						ClientPath cp = null;
						LocalPath lp = null;

						if (obj.ContainsKey("rev"))
						{
							int.TryParse(obj["rev"], out rev);
						}
						if (obj.ContainsKey("depotFile"))
						{
							p = obj["depotFile"];
							dp = new DepotPath(p);
						}
						if (obj.ContainsKey("clientFile"))
						{
							p = obj["clientFile"];
							if (p.StartsWith("//"))
							{
								cp = new ClientPath(p);
							}
							else
							{
								cp = new ClientPath(p);
								lp = new LocalPath(p);
							}
						}
						if (obj.ContainsKey("path"))
						{
							lp = new LocalPath(obj["path"]);
						}
						FileSpec fs = new FileSpec(dp, cp, lp, new Revision(rev));
						returnVal.Files.Add(new FileSubmitRecord(action, fs));
					}
				}
				return returnVal;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}

			return null;
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help resolved</b>
		/// <br/> 
		/// <br/>     resolved -- Show files that have been resolved but not submitted
		/// <br/> 
		/// <br/>     p4 resolved [-o] [file ...]
		/// <br/> 
		/// <br/> 	'p4 resolved' lists file updates and integrations that have been 
		/// <br/> 	resolved but not yet submitted.  To see unresolved integrations, 
		/// <br/> 	use 'p4 resolve -n'.  To see already submitted integrations, use 
		/// <br/> 	'p4 integrated'.
		/// <br/> 
		/// <br/> 	If a depot file path is specified, the output lists resolves for
		/// <br/> 	'theirs' files that match the specified path.  If a client file
		/// <br/> 	path is specified, the output lists resolves for 'yours' files
		/// <br/> 	that match the specified path.
		/// <br/> 
		/// <br/> 	The -o flag reports the revision used as the base during the
		/// <br/> 	resolve.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileResolveRecord> GetResolvedFiles(Options options, params FileSpec[] files)
		{
			P4Command cmd = new P4Command(Connection, "resolved", true, FileSpec.ToStrings(files));

			P4CommandResult results = cmd.Run(options);
			if (results.Success)
			{
				if ((results.TaggedOutput == null) || (results.TaggedOutput.Count <= 0))
				{
					return null;
				}
				List<FileResolveRecord> fileList = new List<FileResolveRecord>();
				foreach (TaggedObject obj in results.TaggedOutput)
				{
					fileList.Add(FileResolveRecord.FromResolvedCmdTaggedOutput(obj));
				}
				return fileList;
			}
			else
			{
				P4Exception.Throw(results.ErrorList);
			}

			return null;

		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public List<FileResolveRecord> GetResolvedFiles(IList<FileSpec> Files, Options options)
		{
			return GetResolvedFiles(options, Files.ToArray());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"><cref>RevertFilesOptions</cref></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help revert</b>
		/// <br/> 
		/// <br/>     revert -- Discard changes from an opened file
		/// <br/> 
		/// <br/>     p4 revert [-a -n -k -c changelist#] file ...
		/// <br/> 
		/// <br/> 	Revert an open file to the revision that was synced from the depot,
		/// <br/> 	discarding any edits or integrations that have been made.  You must
		/// <br/> 	explicitly specify the files to be reverted.  Files are removed from
		/// <br/> 	the changelist in which they are open.  Locked files are unlocked.
		/// <br/> 
		/// <br/> 	The -a flag  reverts only files that are open for edit or integrate
		/// <br/> 	and are unchanged or missing. Files with pending integration records
		/// <br/> 	are left open. The file arguments are optional when -a is specified.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the operation.
		/// <br/> 
		/// <br/> 	The -k flag marks the file as reverted in server metadata without
		/// <br/> 	altering files in the client workspace.
		/// <br/> 
		/// <br/> 	The -c flag reverts files that are open in the specified changelist.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> RevertFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("revert", options, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public List<FileSpec> RevertFiles(IList<FileSpec> Files, Options options)
		{
			return RevertFiles(options, Files.ToArray());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help shelve</b>
		/// <br/> 
		/// <br/>     shelve -- Store files from a pending changelist into the depot
		/// <br/> 
		/// <br/>     p4 shelve [files]
		/// <br/>     p4 shelve -i [-f | -r]
		/// <br/>     p4 shelve -r -c changelist#
		/// <br/>     p4 shelve -c changelist# [-f] [file ...]
		/// <br/>     p4 shelve -d -c changelist# [-f] [file ...]
		/// <br/> 
		/// <br/> 	'p4 shelve' creates, modifies or deletes shelved files in a pending
		/// <br/> 	changelist. Shelved files remain in the depot until they are deleted
		/// <br/> 	(using 'p4 shelve -d') or replaced by subsequent shelve commands.
		/// <br/> 	After 'p4 shelve', the user can revert the files and restore them
		/// <br/> 	later using 'p4 unshelve'.  Other users can 'p4 unshelve' the stored
		/// <br/> 	files into their own workspaces.
		/// <br/> 
		/// <br/> 	Files that have been shelved can be accessed by the 'p4 diff',
		/// <br/> 	'p4 diff2', 'p4 files' and 'p4 print' commands using the revision
		/// <br/> 	specification '@=change', where 'change' is the pending changelist
		/// <br/> 	number.
		/// <br/> 
		/// <br/> 	By default, 'p4 shelve' creates a changelist, adds files from the
		/// <br/> 	user's default changelist, then shelves those files in the depot.
		/// <br/> 	The user is presented with a text changelist form displayed using
		/// <br/> 	the editor configured using the $P4EDITOR environment variable.
		/// <br/> 
		/// <br/> 	If a file pattern is specified, 'p4 shelve' shelves the files that
		/// <br/> 	match the pattern.
		/// <br/> 
		/// <br/> 	The -i flag reads the pending changelist specification with shelved
		/// <br/> 	files from the standard input.  The user's editor is not invoked.
		/// <br/> 	To modify an existing changelist with shelved files, specify the
		/// <br/> 	changelist number using the -c flag.
		/// <br/> 
		/// <br/> 	The -c flag specifies the pending changelist that contains shelved
		/// <br/> 	files to be created, deleted, or modified. Only the user and client
		/// <br/> 	of the pending changelist can add or modify its shelved files.
		/// <br/> 
		/// <br/> 	The -f (force) flag must be used with the -c or -i flag to overwrite
		/// <br/> 	any existing shelved files in a pending changelist.
		/// <br/> 
		/// <br/> 	The -r flag (used with -c or -i) enables you to replace all shelved
		/// <br/> 	files in that changelist with the files opened in your own workspace
		/// <br/> 	at that changelist number. Only the user and client workspace of the
		/// <br/> 	pending changelist can replace its shelved files.
		/// <br/> 
		/// <br/> 	The -d flag (used with -c) deletes the shelved files in the specified
		/// <br/> 	changelist so that they can no longer be unshelved.  By default, only
		/// <br/> 	the user and client of the pending changelist can delete its shelved
		/// <br/> 	files. A user with 'admin' access can delete shelved files by including
		/// <br/> 	the -f flag to force the operation.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> ShelveFiles(Options options, params FileSpec[] files)
		{
			string cmdData = null;
			if (options.ContainsKey("-i"))
			{ 
				cmdData = options["-i"];
				options["-i"] = null;
			}
			return runFileListCmd("shelve", options, cmdData, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public List<FileSpec> ShelveFiles(IList<FileSpec> Files, Options options)
		{
			return ShelveFiles(options, Files!=null?Files.ToArray(): null );
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help sync</b>
		/// <br/> 
		/// <br/>     sync -- Synchronize the client with its view of the depot
		/// <br/>     flush -- synonym for 'sync -k'
		/// <br/>     update -- synonym for 'sync -s'
		/// <br/> 
		/// <br/>     p4 sync [-f -L -n -k -q] [-m max] [file[revRange] ...]
		/// <br/>     p4 sync [-L -n -q -s] [-m max] [file[revRange] ...]
		/// <br/>     p4 sync [-L -n -p -q] [-m max] [file[revRange] ...]
		/// <br/> 
		/// <br/> 	Sync updates the client workspace to reflect its current view (if
		/// <br/> 	it has changed) and the current contents of the depot (if it has
		/// <br/> 	changed). The client view maps client and depot file names and
		/// <br/> 	locations.
		/// <br/> 
		/// <br/> 	Sync adds files that are in the client view and have not been
		/// <br/> 	retrieved before.  Sync deletes previously retrieved files that
		/// <br/> 	are no longer in the client view or have been deleted from the
		/// <br/> 	depot.  Sync updates files that are still in the client view and
		/// <br/> 	have been updated in the depot.
		/// <br/> 
		/// <br/> 	By default, sync affects all files in the client workspace. If file
		/// <br/> 	arguments are given, sync limits its operation to those files.
		/// <br/> 	The file arguments can contain wildcards.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision specifier, then the given
		/// <br/> 	revision is retrieved.  Normally, the head revision is retrieved.
		/// <br/> 	See 'p4 help revisions' for help specifying revisions.
		/// <br/> 
		/// <br/> 	If the file argument includes a revision range specification,
		/// <br/> 	only files selected by the revision range are updated, and the
		/// <br/> 	highest revision in the range is used.
		/// <br/> 
		/// <br/> 	Normally, sync does not overwrite workspace files that the user has
		/// <br/> 	manually made writable.  Setting the 'clobber' option in the
		/// <br/> 	client specification disables this safety check.
		/// <br/> 
		/// <br/> 	The -f flag forces resynchronization even if the client already
		/// <br/> 	has the file, and overwriting any writable files.  This flag doesn't
		/// <br/> 	affect open files.
		/// <br/> 
		/// <br/> 	The -L flag can be used with multiple file arguments that are in
		/// <br/> 	full depot syntax and include a valid revision number. When this
		/// <br/> 	flag is used the arguments are processed together by building an
		/// <br/> 	internal table similar to a label. This file list processing is
		/// <br/> 	significantly faster than having to call the internal query engine
		/// <br/> 	for each individual file argument. However, the file argument syntax
		/// <br/> 	is strict and the command will not run if an error is encountered.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without updating the workspace.
		/// <br/> 
		/// <br/> 	The -k flag updates server metadata without syncing files. It is
		/// <br/> 	intended to enable you to ensure that the server correctly reflects
		/// <br/> 	the state of files in the workspace while avoiding a large data
		/// <br/> 	transfer. Caution: an erroneous update can cause the server to
		/// <br/> 	incorrectly reflect the state of the workspace.
		/// <br/> 
		/// <br/> 	The -p flag populates the client workspace, but does not update the
		/// <br/> 	server to reflect those updates.  Any file that is already synced or
		/// <br/> 	opened will be bypassed with a warning message.  This option is very
		/// <br/> 	useful for build clients or when publishing content without the
		/// <br/> 	need to track the state of the client workspace.
		/// <br/> 
		/// <br/> 	The -q flag suppresses normal output messages. Messages regarding
		/// <br/> 	errors or exceptional conditions are not suppressed.
		/// <br/> 
		/// <br/> 	The -s flag adds a safety check before sending content to the client
		/// <br/> 	workspace.  This check uses MD5 digests to compare the content on the
		/// <br/> 	clients workspace against content that was last synced.  If the file
		/// <br/> 	has been modified outside of Perforce's control then an error message
		/// <br/> 	is displayed and the file is not overwritten.  This check adds some
		/// <br/> 	extra processing which will affect the performance of the operation.
		/// <br/> 
		/// <br/> 	The -m flag limits sync to the first 'max' number of files. This
		/// <br/> 	option is useful in conjunction with tagged output and the '-n'
		/// <br/> 	flag, to preview how many files will be synced without transferring
		/// <br/> 	all the file data.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> SyncFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("sync", options, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public List<FileSpec> SyncFiles(IList<FileSpec> Files, Options options)
		{
			return SyncFiles(options, Files.ToArray());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help unlock</b>
		/// <br/> 
		/// <br/>     unlock -- Release a locked file, leaving it open
		/// <br/> 
		/// <br/>     p4 unlock [-c changelist#] [-f] [file ...]
		/// <br/> 
		/// <br/> 	'p4 unlock' releases locks on the specified files, which must be
		/// <br/> 	open in the specified pending changelist. If you omit the changelist
		/// <br/> 	number, the default changelist is assumed. If you omit the file name,
		/// <br/> 	all locked files are unlocked.
		/// <br/> 
		/// <br/> 	By default, files can be unlocked only by the changelist owner. The
		/// <br/> 	-f flag enables you to unlock files in changelists owned by other
		/// <br/> 	users. The -f flag requires 'admin' access, which is granted by 'p4
		/// <br/> 	protect'.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> UnlockFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("unlock", options, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public List<FileSpec> UnlockFiles(IList<FileSpec> Files, Options options)
		{
			return UnlockFiles(options, Files.ToArray());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help unshelve</b>
		/// <br/> 
		/// <br/>     unshelve -- Restore shelved files from a pending change into a workspace
		/// <br/> 
		/// <br/>     p4 unshelve -s changelist# [-f -n] [-c changelist#] [file ...]
		/// <br/> 
		/// <br/> 	'p4 unshelve' retrieves shelved files from the specified pending
		/// <br/> 	changelist, opens them in a pending changelist and copies them
		/// <br/> 	to the invoking user's workspace.  Unshelving files from a pending
		/// <br/> 	changelist is restricted by the user's permissions on the files.
		/// <br/> 	A successful unshelve operation places the shelved files on the
		/// <br/> 	user's workspace with the same open action and pending integration
		/// <br/> 	history as if it had originated from that user and client.
		/// <br/> 
		/// <br/> 	Unshelving a file over an already opened file is only permitted
		/// <br/> 	if both shelved file and opened file are opened for 'edit'. After
		/// <br/> 	unshelving, the workspace file is flagged as unresolved, and
		/// <br/> 	'p4 resolve' must be run to resolve the differences between the
		/// <br/> 	shelved file and the workspace file.
		/// <br/> 
		/// <br/> 	The -s flag specifies the number of the pending changelist that
		/// <br/> 	contains the shelved files.
		/// <br/> 
		/// <br/> 	If a file pattern is specified, 'p4 unshelve' unshelves files that
		/// <br/> 	match the pattern.
		/// <br/> 
		/// <br/> 	The -c flag specifies the changelist to which files are unshelved.
		/// <br/> 	By default,  'p4 unshelve' opens shelved files in the default
		/// <br/> 	changelist.
		/// <br/> 
		/// <br/> 	The -f flag forces the clobbering of any writeable but unopened files
		/// <br/> 	that are being unshelved.
		/// <br/> 
		/// <br/> 	The -n flag previews the operation without changing any files or
		/// <br/> 	metadata.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> UnshelveFiles(Options options, params FileSpec[] files)
		{
			return runFileListCmd("unshelve", options, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <param name="options"></param>
		/// <returns></returns>
		public List<FileSpec> UnshelveFiles(IList<FileSpec> Files, Options options)
		{
			return UnshelveFiles(options, Files.ToArray());
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help where</b>
		/// <br/> 
		/// <br/>     where -- Show how file names are mapped by the client view
		/// <br/> 
		/// <br/>     p4 where [file ...]
		/// <br/> 
		/// <br/> 	Where shows how the specified files are mapped by the client view.
		/// <br/> 	For each argument, three names are produced: the name in the depot,
		/// <br/> 	the name on the client in Perforce syntax, and the name on the client
		/// <br/> 	in local syntax.
		/// <br/> 
		/// <br/> 	If the file parameter is omitted, the mapping for all files in the
		/// <br/> 	current directory and below) is returned.
		/// <br/> 
		/// <br/> 	Note that 'p4 where' does not determine where any real files reside.
		/// <br/> 	It only displays the locations that are mapped by the client view.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> GetClientFileMappings(params FileSpec[] files)
		{
			return runFileListCmd("where", null, files);
		}
		/// <summary>
		/// 
		/// </summary>
		/// <param name="Files"></param>
		/// <returns></returns>
		public List<FileSpec> GetClientFileMappings(IList<FileSpec> Files)
		{
			return GetClientFileMappings(Files.ToArray());
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="options"></param>
		/// <param name="files"></param>
		/// <returns></returns>
		/// <remarks>
		/// <br/><b>p4 help copy</b>
		/// <br/> 
		/// <br/>     copy -- Copy one set of files to another
		/// <br/> 
		/// <br/>     p4 copy [options] fromFile[rev] toFile
		/// <br/>     p4 copy [options] -b branch [-r] [toFile[rev] ...]
		/// <br/>     p4 copy [options] -b branch -s fromFile[rev] [toFile ...]
		/// <br/>     p4 copy [options] -S stream [-P parent] [-F] [-r] [toFile[rev] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -n -v -m max
		/// <br/> 
		/// <br/> 	'p4 copy' copies one set of files (the 'source') into another (the
		/// <br/> 	'target').
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 copy' makes the
		/// <br/> 	target identical to the source by branching, replacing, or deleting
		/// <br/> 	files.  'p4 submit' submits copied files to the depot. 'p4 revert'
		/// <br/> 	can be used to revert copied files instead of submitting them.  The
		/// <br/> 	history of copied files can be shown with 'p4 filelog' or 'p4
		/// <br/> 	integrated'.
		/// <br/> 
		/// <br/> 	Target files that are already identical to the source, or that are
		/// <br/> 	outside of the client view, are not affected by 'p4 copy'. Opened,
		/// <br/> 	non-identical target files cause 'p4 copy' to exit with a warning. 
		/// <br/> 	When 'p4 copy' creates or modifies files in the workspace, it leaves
		/// <br/> 	them read-only; 'p4 edit' can make them writable.  Files opened by
		/// <br/> 	'p4 copy' do not need to be resolved.
		/// <br/> 
		/// <br/> 	Source and target files (fromFile and toFile) can be specified on
		/// <br/> 	the 'p4 copy' command line or through a branch view. On the command
		/// <br/> 	line, fromFile is the source file set and toFile is the target file
		/// <br/> 	set.  With a branch view, one or more toFile arguments can be given
		/// <br/> 	to limit the scope of the target file set.
		/// <br/> 
		/// <br/> 	A revision specifier can be used to select the revision to copy; by
		/// <br/> 	default, the head revision is copied. The revision specifier can be
		/// <br/> 	used on fromFile, or on toFile, but not on both.  When used on toFile,
		/// <br/> 	it refers to source revisions, not to target revisions.  A range may
		/// <br/> 	not be used as a revision specifier.  For revision syntax, see 'p4
		/// <br/> 	help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 copy' use a stream's branch view.  (See 'p4 help
		/// <br/> 	stream'.) The source is the stream itself, and the target is the
		/// <br/> 	stream's parent. With -r, the direction is reversed.  -P can be used
		/// <br/> 	to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit copied stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -F flag can be used with -S to force copying even though the
		/// <br/> 	stream does not expect a copy to occur in the direction indicated.
		/// <br/> 	Normally 'p4 copy' enforces the expected flow of change dictated
		/// <br/> 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// <br/> 	expected flow of change.
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 copy' use a user-defined branch view.  (See
		/// <br/> 	'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the user-defined branch view to be
		/// <br/> 	treated as the target, per the branch view mapping.  Optional toFile
		/// <br/> 	arguments may be given to further restrict the scope of the target
		/// <br/> 	file set. -r is ignored when -s is used.
		/// <br/> 
		/// <br/> 	The -c changelist# flag opens files in the designated (numbered)
		/// <br/> 	pending changelist instead of the default changelist.
		/// <br/> 
		/// <br/> 	The -n flag displays a preview of the copy, without actually doing
		/// <br/> 	anything.
		/// <br/> 
		/// <br/> 	The -m flag limits the actions to the first 'max' number of files.
		/// <br/> 
		/// <br/> 	The -v flag causes a 'virtual' copy that does not modify client
		/// <br/> 	workspace files.  After submitting a virtual integration, 'p4 sync'
		/// <br/> 	can be used to update the workspace.
		/// <br/> 
		/// <br/> 
		/// </remarks>
		public List<FileSpec> CopyFiles(Options options, FileSpec fromFile, params FileSpec[] toFiles)
		{
			if ((options != null) && (options.ContainsKey("-s")) && (fromFile == null))
			{
				throw new ArgumentNullException("fromFile", 
					"From file cannot be null when the -s  flag is specified");
			}
			IList<FileSpec> Files = null;
			if ((toFiles != null) && (toFiles.Length > 0))
			{
				Files = new List<FileSpec>(toFiles);
			}
			else
			{
				if (fromFile != null)
				{
					Files = new List<FileSpec>();
				}
			}
			if ((Files != null) && (fromFile != null))
			{
				Files.Insert(0, fromFile);
			}
			return runFileListCmd("copy", options, Files.ToArray());
		}
		public List<FileSpec> CopyFiles(FileSpec fromFile, IList<FileSpec> toFiles, Options options)
		{
			return CopyFiles(options, fromFile, toFiles.ToArray());
		}
		public List<FileSpec> CopyFiles(Options options)
		{
			return runFileListCmd("copy", options);
		}

		/// <br/><b>p4 help merge</b>
		/// <br/> 
		/// <br/>     merge -- Merge one set of files into another 
		/// <br/> 
		/// <br/>     p4 merge [options] fromFile[revRange] toFile
		/// <br/>     p4 merge [options] -b branch [-r] [toFile[revRange] ...]
		/// <br/>     p4 merge [options] -b branch -s fromFile[revRange] [toFile ...]
		/// <br/>     p4 merge [options] -S stream [-P parent] [-F] [-r] [toFile[revRange] ...]
		/// <br/> 
		/// <br/> 	options: -c changelist# -n -m max
		/// <br/> 
		/// <br/> 	'p4 merge' merges changes from one set of files (the 'source') into 
		/// <br/> 	another (the 'target'). It is a simplified form of the 'p4 integrate'
		/// <br/> 	command.
		/// <br/> 
		/// <br/> 	Using the client workspace as a staging area, 'p4 merge' branches and
		/// <br/> 	deletes target files per changes in the source, and schedules all 
		/// <br/> 	other affected target files to be resolved.  Target files outside of
		/// <br/> 	the current client view are not affected. Source files need not be
		/// <br/> 	within the client view.
		/// <br/> 
		/// <br/> 	'p4 resolve' must be used to merge file content, and to resolve 
		/// <br/> 	filename and filetype changes. 'p4 submit' commits merged files to
		/// <br/> 	the depot.  Unresolved files may not be submitted.  Merged files can
		/// <br/> 	be shelved with 'p4 shelve' and abandoned with 'p4 revert'.  The 
		/// <br/> 	commands 'p4 integrated' and 'p4 filelog' display merge history.
		/// <br/> 
		/// <br/> 	When 'p4 merge' schedules a workspace file to be resolved, it leaves
		/// <br/> 	it read-only. 'p4 resolve' can operate on a read-only file;  for 
		/// <br/> 	other pre-submit changes, 'p4 edit' must be used to make the file 
		/// <br/> 	writable.
		/// <br/> 
		/// <br/> 	Source and target files can be specified either on the 'p4 merge'
		/// <br/> 	command line or through a branch view. On the command line, fromFile
		/// <br/> 	is the source file set and toFile is the target file set.  With a 
		/// <br/> 	branch view, one or more toFile arguments can be given to limit the 
		/// <br/> 	scope of the target file set. 
		/// <br/> 
		/// <br/> 	Each file in the target is mapped to a file in the source. Mapping 
		/// <br/> 	adjusts automatically for files that have been moved or renamed, as
		/// <br/> 	long as 'p4 move' was used to move/rename files.  The scope of source
		/// <br/> 	and target file sets must include both old-named and new-named files
		/// <br/> 	for mappings to be adjusted.  Moved source files may schedule moves 
		/// <br/> 	to be resolved in target files. 
		/// <br/> 
		/// <br/> 	revRange is a revision or a revision range that limits the span of
		/// <br/> 	source history to be probed for unintegrated revisions.  revRange 
		/// <br/> 	can be used on fromFile, or on toFile, but not on both.  When used
		/// <br/> 	on toFile, it refers to source revisions, not to target revisions.
		/// <br/> 	For details about revision specifiers, see 'p4 help revisions'.
		/// <br/> 
		/// <br/> 	The -S flag makes 'p4 merge' use a stream's branch view.  (See 'p4
		/// <br/> 	help stream'.) The source is the stream itself, and the target is
		/// <br/> 	the stream's parent. With -r, the direction is reversed.  -P can be
		/// <br/> 	used to specify a parent stream other than the stream's actual parent.
		/// <br/> 	Note that to submit merged stream files, the current client must
		/// <br/> 	be dedicated to the target stream. (See 'p4 help client'.)
		/// <br/> 
		/// <br/> 	The -F flag can be used with -S to force merging even though the
		/// <br/> 	stream does not expect a merge to occur in the direction indicated.
		/// <br/> 	Normally 'p4 merge' enforces the expected flow of change dictated
		/// <br/> 	by the stream's spec. The 'p4 istat' command summarizes a stream's
		/// <br/> 	expected flow of change.
		/// <br/> 
		/// <br/> 	The -b flag makes 'p4 merge' use a user-defined branch view.  (See
		/// <br/> 	'p4 help branch'.) The source is the left side of the branch view
		/// <br/> 	and the target is the right side. With -r, the direction is reversed.
		/// <br/> 
		/// <br/> 	The -s flag can be used with -b to cause fromFile to be treated as
		/// <br/> 	the source, and both sides of the branch view to be treated as the
		/// <br/> 	target, per the branch view mapping.  Optional toFile arguments may
		/// <br/> 	be given to further restrict the scope of the target file set.  The
		/// <br/> 	-r flag is ignored when -s is used.
		/// <br/> 
		/// <br/> 
		public List<FileSpec> MergeFiles(Options options, FileSpec fromFile, params FileSpec[] toFiles)
		{
			if ((options != null) && (options.ContainsKey("-s")) && (fromFile == null))
			{
				throw new ArgumentNullException("fromFile",
					"From file cannot be null when the -s  flag is specified");
			}
			IList<FileSpec> Files = null;
			if ((toFiles != null) && (toFiles.Length > 0))
			{
				Files = new List<FileSpec>(toFiles);
			}
			else
			{
				if (fromFile != null)
				{
					Files = new List<FileSpec>();
				}
			}
			if ((Files != null) && (fromFile != null))
			{
				Files.Insert(0, fromFile);
			}
			return runFileListCmd("merge", options, Files.ToArray());
		}
		public List<FileSpec> MergeFiles(FileSpec fromFile, IList<FileSpec> toFiles, Options options)
		{
			return MergeFiles(options, fromFile, toFiles.ToArray());
		}
		public List<FileSpec> MergeFiles(Options options)
		{
			return runFileListCmd("merge", options);
		}

		#endregion
	}
}
