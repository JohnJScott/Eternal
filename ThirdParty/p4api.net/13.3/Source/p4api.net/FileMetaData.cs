using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Describes the pending or completed action related to open,
	/// resolve, or integration for a specific file.
	/// </summary>
	public enum FileAction : long
	{
		/// <summary>
		/// None.
		/// </summary>
		None = 0x0000,
		/// <summary>
		/// Opened for add.
		/// </summary>
		Add = 1,
		/// <summary>
		/// Opened for branch.
		/// </summary>
		Branch = 2,
		/// <summary>
		/// Opened for edit.
		/// </summary>
		Edit = 3,
		/// <summary>
		/// Opened for integrate.
		/// </summary>
		Integrate = 4,
		/// <summary>
		/// File has been deleted.
		/// </summary>
		Delete = 5,
		/// <summary>
		/// file was integrated from partner-file, and partner-file
		/// had been previously deleted.
		/// </summary>
		DeleteFrom = 6,
		/// <summary>
		/// file was integrated into partner-file, and file had been
		/// previously deleted.
		/// </summary>
		DeleteInto = 7,
		/// <summary>
		/// File has been synced.
		/// </summary>
		Sync = 8,
		/// <summary>
		/// File has been updated.
		/// </summary>
		Updated = 9,
		/// <summary>
		/// File has been added.
		/// </summary>
		Added = 10,
		/// <summary>
		/// file was integrated into previously nonexistent partner-file,
		/// and partner-file was reopened for add before submission.
		/// </summary>
		AddInto = 11,
		/// <summary>
		/// File has been refreshed.
		/// </summary>
		Refreshed = 12,
		/// <summary>
		/// File was integrated from partner-file, accepting yours.
		/// </summary>
		Ignored = 13,
		/// <summary>
		/// File was integrated into partner-file, accepting yours.
		/// </summary>
		IgnoredBy = 14,
		/// <summary>
		/// File has been abandoned.
		/// </summary>
		Abandoned = 15,
		/// <summary>
		/// None.
		/// </summary>
		EditIgnored = 16,
		/// <summary>
		/// File is opened for move.
		/// </summary>
		Move = 17,
		/// <summary>
		/// File has been added as part of a move.
		/// </summary>
		MoveAdd = 18,
		/// <summary>
		/// File has been deleted as part of a move.
		/// </summary>
		MoveDelete = 19,
		/// <summary>
		/// File was integrated from partner-file, accepting theirs
		/// and deleting the original.
		/// </summary>
		MovedFrom = 20,
		/// <summary>
		/// File was integrated into partner-file, accepting merge.
		/// </summary>
		MovedInto = 21,
		/// <summary>
		/// File has not been resolved.
		/// </summary>
		Unresolved = 22,
		/// <summary>
		/// File was integrated from partner-file, accepting theirs.
		/// </summary>
		CopyFrom = 23,
		/// <summary>
		/// File was integrated into partner-file, accepting theirs.
		/// </summary>
		CopyInto = 24,
		/// <summary>
		/// File was integrated from partner-file, accepting merge.
		/// </summary>
		MergeFrom = 25,
		/// <summary>
		/// File was integrated into partner-file, accepting merge.
		/// </summary>
		MergeInto = 26,
		/// <summary>
		/// file was integrated from partner-file, and file was edited
		/// within the p4 resolve process. This allows you to determine
		/// whether the change should ever be integrated back; automated
		/// changes (merge from) needn't be, but original user edits
		/// (edit from) performed during the resolve should be.
		/// </summary>
		EditFrom = 27,
		/// <summary>
		/// File was integrated into partner-file, and partner-file was
		/// reopened for edit before submission.
		/// </summary>
		EditInto = 28,
		/// <summary>
		/// File was purged.
		/// </summary>
		Purge = 29,
		/// <summary>
		/// File was imported.
		/// </summary>
		Import = 30,
		/// <summary>
		/// File did not previously exist; it was created as a copy of
		/// partner-file.
		/// </summary>
		BranchFrom = 31,
		/// <summary>
		/// Partner-file did not previously exist; it was created as a
		/// copy of file.
		/// </summary>
		BranchInto = 32,
		/// <summary>
		/// File was reverted.
		/// </summary>
		Reverted = 33,
	}

	/// <summary>
	/// Class summarizing the use of this file by another user.
	/// </summary>
	public class OtherFileUser
	{
		private string _client;
		public string Client
		{
			get { return _client; }
			set
			{
				_client = value;
				string[] parts = value.Split('@');
				if (parts.Length > 0)
					UserName = parts[0];
				if (parts.Length > 1)
					ClientName = parts[1];
			}
		}
		public string  UserName {get; set;}
		public FileAction Action { get; set; }
		public bool hasLock { get; set; }
		public string ClientName { get; set; }
		public int ChangelistId { get; set; }
	}
	
	/// <summary>
	/// Specifies other users who have a particular file open.
	/// </summary>
	public class OtherUsers : Dictionary<string, OtherFileUser>
	{
		public OtherFileUser this[string key]
		{
			get
			{
				if (base.ContainsKey(key) == false)
				{
					OtherFileUser newEntry = new OtherFileUser();
					newEntry.Client = key;
					base.Add(key, newEntry);
				}

				return base[key];
			}
			set
			{
				base[key] = value;
			}
		}

	}
	/// <summary>
	/// Metadata for a specific file stored in a Perforce repository.
	/// </summary>
	public class FileMetaData
	{
		public FileMetaData() 
		{
			MovedFile = null;
			IsMapped = false;
			Shelved = false;
			HeadAction = FileAction.None;
			HeadChange = -1;
			HeadRev = -1;
			HeadType = null;
			HeadTime = DateTime.MinValue;
			HeadModTime = DateTime.MinValue;
			MovedRev = -1;
			HaveRev = -1;
			Desc = null;
			Digest = null;
			FileSize = -1;
			Action = FileAction.None;
			Type = null;
			ActionOwner = null;
			Change = -1;
			Resolved = false;
			Unresolved = false;
			Reresolvable = false;
			OtherOpen = 0;
			OtherOpenUserClients = null;
			OtherLock = false;
			OtherLockUserClients = null;
			OtherActions = null;
			OtherChanges = null;
			OurLock = false;
			ResolveRecords = null;
			Attributes = null;
		    Directory = null;
		}

		public FileMetaData(	DepotPath movedfile,
								bool ismapped,
								bool shelved,
								FileAction headaction,
								int headchange,
								int headrev,
								FileType headtype,
								DateTime headtime,
								DateTime headmodtime,
								int movedrev,
								int haverev,
								string desc,
								string digest,
								int filesize,
								FileAction action,
								FileType type,
								string actionowner,
								int change,
								bool resolved,
								bool unresolved,
								bool reresolvable,
								int otheropen,
								List<string> otheropenuserclients,
								bool otherlock,
								List<string> otherlockuserclients,
								List<FileAction> otheractions,
								List<int> otherchanges,
								bool ourlock,
								List<FileResolveAction> resolverecords,
								Dictionary<String, Object> attributes,
            string directory
								)
		{
			MovedFile = movedfile;
			IsMapped = ismapped;
			Shelved = shelved;
			HeadAction = headaction;
			HeadChange = headchange;
			HeadRev = headrev;
			HeadType = headtype;
			HeadTime = headtime;
			HeadModTime = headmodtime;
			MovedRev = movedrev;
			HaveRev = haverev;
			Desc = desc;
			Digest = digest;
			FileSize = filesize;
			Action = action;
			Type = type;
			ActionOwner = actionowner;
			Change = change;
			Resolved = resolved;
			Unresolved = unresolved;
			Reresolvable = reresolvable;
			OtherOpen = otheropen;
			OtherOpenUserClients = otheropenuserclients;
			OtherLock = otherlock;
			OtherLockUserClients = otherlockuserclients;
			OtherActions = otheractions;
			OtherChanges = otherchanges;
			OurLock = ourlock;
			ResolveRecords = resolverecords;
			Attributes = attributes;
		    Directory = directory;
		}

		public FileMetaData(File f)
		{
			MovedFile = null;
			IsMapped = false;
			Shelved = false;
			HeadAction = FileAction.None;
			HeadChange = -1;
			HeadRev = -1;
			HeadType = null;
			HeadTime = DateTime.MinValue;
			HeadModTime = DateTime.MinValue;
			MovedRev = -1;
			Revision rev = f.Version as Revision;
			if (rev != null)
			{
				HaveRev = rev.Rev;
			}
			else
			{
				HaveRev = -1;
			}
			Desc = null;
			Digest = null;
			FileSize = -1;
			Action = f.Action;
			Type = f.Type;
			ActionOwner = f.User;
			Change = f.ChangeId;
			Resolved = false;
			Unresolved = false;
			Reresolvable = false;
			OtherOpen = 0;
			OtherOpenUserClients = null;
			OtherLock = false;
			OtherLockUserClients = null;
			OtherActions = null;
			OtherChanges = null;
			OurLock = false;
			ResolveRecords = null;
			Attributes = null;
			Directory = null;

			DepotPath = f.DepotPath;
			ClientPath = f.ClientPath;
		}

		/// <summary>
		///  The location of the file in the depot
		/// </summary>
		public DepotPath DepotPath { get; set; }
		public bool IsInDepot
		{
			get
			{ 
				return ((DepotPath !=null) && (string.IsNullOrEmpty(DepotPath.Path) == false)); 
			}
		}
		/// <summary>
		/// The location of the file in the client's file system,
		/// may be either a ClientPath or a LocalPath
		/// </summary>
		public LocalPath LocalPath { get; set; }
		public ClientPath ClientPath { get; set; }
		public bool IsInClient
		{
			get
			{
				return ((ClientPath != null) && (string.IsNullOrEmpty(ClientPath.Path) == false));
			}
		}
		public DepotPath MovedFile { get; set; }
		public bool IsMapped { get; set; }
		public bool Shelved { get; set; }
		private StringEnum<FileAction> _headAction = FileAction.None;
		public FileAction HeadAction 
		{
			get { return (_headAction == null)? FileAction.None : (FileAction) _headAction; }
			set {_headAction = value; }
		}
		public int HeadChange { get; set; }
		public int HeadRev { get; set; }
		public FileType HeadType { get; set; }
		public DateTime HeadTime { get; set; }
		public DateTime HeadModTime { get; set; }
		public int MovedRev { get; set; }
		public int HaveRev { get; set; }
		public string Desc { get; set; }
		public string Digest { get; set; }
		public long FileSize { get; set; }
		private StringEnum<FileAction> _action;
		public FileAction Action
		{
			get { return (_action == null)? FileAction.None : (FileAction) _action; }
			set { _action = value; }
		}
		public FileType Type { get; set; }
		public string ActionOwner { get; set; }
		public int Change { get; set; }
		public bool Resolved { get; set; }
		public bool Unresolved { get; set; }
		public bool Reresolvable { get; set; }		
		public int OtherOpen { get; set; }
		public List<String> OtherOpenUserClients { get; set; }
		public bool OtherLock { get; set; }
		public List<String> OtherLockUserClients { get; set; }
		public List<FileAction> OtherActions { get; set; }
		public List<int> OtherChanges { get; set; }
		public bool OurLock { get; set; }
		public List<FileResolveAction> ResolveRecords { get; set; }
		public Dictionary<string, object> Attributes { get; set; }

		public OtherUsers OtherUsers { get; set; }
        public string Directory { get; set; }

		public void FromFstatCmdTaggedData(TaggedObject obj)
		{
			if (obj.ContainsKey("clientFile"))
			{
				string path = obj["clientFile"];
				if (path.StartsWith("//"))
				{
					ClientPath = new ClientPath(obj["clientFile"]);
				}
				else
				{
					ClientPath = new ClientPath(obj["clientFile"]);
					LocalPath = new LocalPath(obj["clientFile"]);
				}
			}

			if (obj.ContainsKey("path"))
			{
				LocalPath = new LocalPath(obj["path"]);
			}

			if (obj.ContainsKey("depotFile"))
			{
				string p = PathSpec.UnescapePath(obj["depotFile"]);
				DepotPath = new DepotPath(p);
			}

			if (obj.ContainsKey("movedFile"))
			{
				MovedFile = new DepotPath(obj["movedFile"]);
				if (obj.ContainsKey("movedRev"))
				{
					int movedrev = -1;
					if (int.TryParse(obj["movedRev"], out movedrev))
					{
						MovedRev = movedrev;
					}
				}
			}

			if (obj.ContainsKey("isMapped"))
			{ IsMapped = true; }

			if (obj.ContainsKey("shelved"))
			{ Shelved = true; }

			if (obj.ContainsKey("headAction"))
			{ 
				_headAction = obj["headAction"]; 
			}

			if (obj.ContainsKey("headChange"))
			{
				int r = -1;
				if (int.TryParse(obj["headChange"], out r))
				{
					HeadChange = r;
				}
			}

			if (obj.ContainsKey("headRev"))
			{
				int r = -1;
				if (int.TryParse(obj["headRev"], out r))
				{
					HeadRev = r;
				}
			}

			if (obj.ContainsKey("headType"))
			{ 
				HeadType = new FileType(obj["headType"]); 
			}

			if (obj.ContainsKey("headTime"))
			{
				HeadTime = FormBase.ConvertUnixTime(obj["headTime"]);
			}

			if (obj.ContainsKey("headModTime"))
			{
				HeadModTime = FormBase.ConvertUnixTime(obj["headModTime"]);
			}

			if (obj.ContainsKey("haveRev"))
			{
				int r = -1;
				if ((int.TryParse(obj["haveRev"], out r)) && (r > 0))
				{
					HaveRev = r;
				}
			}

			if (obj.ContainsKey("desc"))
			{ Desc = obj["desc"]; }

			if (obj.ContainsKey("digest"))
			{ Digest = obj["digest"]; }

			if (obj.ContainsKey("fileSize"))
			{
				long s = -1;
				if (long.TryParse(obj["fileSize"], out s))
				{
					FileSize = s;
				}
			}

			if (obj.ContainsKey("action"))
			{ 
				_action = obj["action"]; 
			}

			if (obj.ContainsKey("type"))
			{
				Type = new FileType(obj["type"]);
			}
			else if (obj.ContainsKey("headType"))
			{
				// If not on mapped in current client, will not have
				//the Type filed so User the HeadType
				Type = new FileType(obj["headType"]);
			}
			else
			{
				Type = new FileType(BaseFileType.Text, FileTypeModifier.None);
			}

			if (obj.ContainsKey("actionOwner"))
			{ 
				ActionOwner = obj["actionOwner"]; 
			}

			if (obj.ContainsKey("change"))
			{
				int c = -1;
				if (int.TryParse(obj["change"], out c))
				{
					Change = c;
				}
				else
				{
					Change = 0;
				}
			}

			if (obj.ContainsKey("resolved"))
			{ Resolved = true; }

			if (obj.ContainsKey("unresolved"))
			{ Unresolved = true; }

			if (obj.ContainsKey("reresolvable"))
			{ Reresolvable = true; }

			if (obj.ContainsKey("otherOpen"))
			{
				int o = 0;
				if (int.TryParse(obj["otherOpen"], out o))
				{
					OtherOpen = o;
				}
				int idx = 0;

				OtherUsers = new OtherUsers();

				OtherOpenUserClients = new List<String>();
				OtherActions = new List<FileAction>();
				OtherChanges = new List<int>();

				while (true)
				{
					string key = String.Format("otherOpen{0}", idx);
					string otherClientName = null;
					OtherFileUser ofi = null;

					if (obj.ContainsKey(key))
					{
						otherClientName = obj[key];
						OtherOpenUserClients.Add(otherClientName);
					}
					else
						break;
					
					ofi = OtherUsers[otherClientName];
					ofi.Client = otherClientName;

					key = String.Format("otherAction{0}", idx);

					if (obj.ContainsKey(key))
					{
						StringEnum<FileAction> otheraction = obj[key];
						OtherActions.Add(otheraction);
						ofi.Action = otheraction;
					}
					else
						break;

					key = String.Format("otherChange{0}", idx);

					if (obj.ContainsKey(key))
					{
						int otherchange;
						if (!int.TryParse(obj[key], out otherchange))
						{
							otherchange = 0;
						}
						OtherChanges.Add(otherchange);

						ofi.ChangelistId = otherchange;
					}

					idx++;
				}
			}

			if (obj.ContainsKey("otherLock"))
			{
				OtherLock = true;

				int idx = 0;
				OtherLockUserClients = new List<string>();

				while (true)
				{
					string key = String.Format("otherLock{0}", idx);

					if (obj.ContainsKey(key))		
					{
						string s = obj[key];
						OtherLockUserClients.Add(s);

						OtherUsers[s].hasLock = true;
					}
					else
						break;

					idx++;
				}
			}

			if (obj.ContainsKey("ourLock"))
			{ 
				OurLock = true; 
			}

			if (obj.ContainsKey("resolved")	||	obj.ContainsKey("unresolved"))
			{
				int idx = 0;
				StringEnum<ResolveAction> resolveaction = ResolveAction.Unresolved;
				FileSpec resolvebasefile = null;
				FileSpec resolvefromfile = null;
				int resolvestartfromrev = -1;
				int resolveendfromrev = -1;
				FileResolveAction resolverecord = null;

				ResolveRecords = new List<FileResolveAction>();

				while (true)
				{
					string key = String.Format("resolveAction{0}", idx);

					if (obj.ContainsKey(key))
					{ resolveaction = obj[key]; }
					else break;

					key = String.Format("resolveBaseFile{0}", idx);

					if (obj.ContainsKey(key))
					{
						string basefile = obj[key];
						int resolvebaserev = -1;
						int.TryParse(obj["resolveBaseRev{0}"], out resolvebaserev);
						resolvebasefile = new FileSpec(new DepotPath(basefile), new Revision(resolvebaserev));
					}
					else break;

					key = String.Format("resolveFromFile{0}", idx);

					if (obj.ContainsKey(key))
					{
						string fromfile = obj[key];
						int startfromrev, endfromrev = -1;
						int.TryParse(obj["resolveStartFromRev{0}"], out startfromrev);
						int.TryParse(obj["resolveEndFromRev{0}"], out endfromrev);
						resolvefromfile = new FileSpec(new DepotPath(fromfile),
							new VersionRange(new Revision(startfromrev), new Revision(endfromrev)));
					}
					else break;

					resolverecord = new FileResolveAction
						(resolveaction, resolvebasefile, resolvefromfile, resolvestartfromrev, resolveendfromrev);
					ResolveRecords.Add(resolverecord);

					idx++;
				}
			}

			Attributes = new Dictionary<string, object>();

			foreach (string key in obj.Keys)
			{
				if (key.StartsWith("attr-"))
				{
					object val = obj[key];
					string atrib = key.Replace("attr-", "");
					Attributes.Add(atrib, val);
				}
			}

            if (obj.ContainsKey("dir"))
            {
                Directory= PathSpec.UnescapePath(obj["dir"]);
            }
		}
		public string GetFileName()
		{
			if ((DepotPath != null) && (string.IsNullOrEmpty(DepotPath.Path) == false))
			{
				return DepotPath.GetFileName();
			}
			else if ((ClientPath != null) && (string.IsNullOrEmpty(ClientPath.Path) == false))
			{
				return ClientPath.GetFileName();
			}
			else if ((LocalPath != null) && (string.IsNullOrEmpty(LocalPath.Path) == false))
			{
				return LocalPath.GetFileName();
			}
			return null;
		}

		public string GetDirectoryName()
		{
			if ((DepotPath != null) && (string.IsNullOrEmpty(DepotPath.Path) == false))
			{
				return DepotPath.GetDirectoryName();
			}
			else if ((ClientPath != null) && (string.IsNullOrEmpty(ClientPath.Path) == false))
			{
				return ClientPath.GetDirectoryName();
			}
			else if ((LocalPath != null) && (string.IsNullOrEmpty(LocalPath.Path) == false))
			{
				return LocalPath.GetDirectoryName();
			}
			return null;
		}

		public static implicit operator FileSpec(FileMetaData s)
		{
			Revision r = null;
			if (s.HaveRev > 0)
			{
				r = new Revision(s.HaveRev);
			}
			if ((s.DepotPath != null) && (string.IsNullOrEmpty(s.DepotPath.Path) == false))
			{
				return new FileSpec(s.DepotPath, r);
			}
			else if ((s.ClientPath != null) && (string.IsNullOrEmpty(s.ClientPath.Path) == false))
			{
				return new FileSpec(s.ClientPath, r);
			}
			else if ((s.LocalPath != null) && (string.IsNullOrEmpty(s.LocalPath.Path) == false))
			{
				return new FileSpec(s.LocalPath, r);
			}
			return null;
		}

		/// <summary>
		/// Cast a FileSpec to FileMetatData
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static implicit operator FileMetaData(FileSpec f)
		{
			FileMetaData s = new FileMetaData();
			if (f.Version != null && f.Version is Revision)
			{
				s.HaveRev = ((Revision)f.Version).Rev;
			}
			s.DepotPath = f.DepotPath;
			s.ClientPath = f.ClientPath;
			s.LocalPath = f.LocalPath;
			
			return s;
		}

		/// <summary>
		/// Cast a FileSpec to FileMetatData
		/// </summary>
		/// <param name="f"></param>
		/// <returns></returns>
		public static implicit operator FileMetaData(File f)
		{
			FileMetaData s = new FileMetaData();
			if (f.Version != null && f.Version is Revision)
			{
				s.HeadRev = ((Revision)f.Version).Rev;
			}
			s.DepotPath = f.DepotPath;
			s.ClientPath = f.ClientPath;
			s.LocalPath = f.LocalPath;
			s.HaveRev = f.HaveRev.Rev;

			s.Change = f.ChangeId;
			s.Action = f.Action;
			s.Type = f.Type;
			
			//DateTime submittime,
			//string user,
			//string client

			return s;
		}


		public bool IsStale
		{
			get { return ((HaveRev > 0) && (HaveRev < HeadRev)); }
		}
	}
	/// <summary>
	/// Describes how, or if a file has been resolved.
	/// </summary>
	public class FileResolveAction
	{
		public FileResolveAction()
		{
		}
		public FileResolveAction
			(ResolveAction resolveaction, FileSpec resolvebasefile, FileSpec resolvefromfile,
			int resolvestartfromrev, int resolveendfromrev)
		{
			ResolveAction = resolveaction;
			ResolveBaseFile = resolvebasefile;
			ResolveFromFile = resolvefromfile;
			ResolveStartFromRev = resolvestartfromrev;
			ResolveEndFromRev = resolveendfromrev;
		}
		public ResolveAction ResolveAction { get; set; }
		public FileSpec ResolveBaseFile { get; set; }
		public FileSpec ResolveFromFile { get; set; }
		public int ResolveStartFromRev { get; set; }
		public int ResolveEndFromRev { get; set; }
	}
	/// <summary>
	/// The action used in resolving the file.
	/// </summary>
	[Flags]
		public enum ResolveAction
		{
			None = 0x0000,

			Unresolved = 0x001,

			CopyFrom = 0x002,

			MergeFrom = 0x004,

			EditFrom = 0x008,

			Ignored = 0x010
		}
	
}
