using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// A versioned object that describes an individual file in a Perforce repository. 
	/// </summary>
	public class File : FileSpec
	{
		public int ChangeId;
		public FileAction Action;
		public FileType Type;
		public DateTime SubmitTime;
		public Revision HaveRev;
		public string User;
		public string Client;

		public File() { }

		public File(
			DepotPath depotPath,
			ClientPath clientPath,
			Revision rev,
			Revision haveRev,
			int change,
			FileAction action,
			FileType type,
			DateTime submittime,
			string user,
			string client)
			: base(depotPath, clientPath, null, rev) 
		{
			ChangeId = change;
			Action = action;
			Type = type;
			SubmitTime = submittime;
			HaveRev = haveRev;
			User = user;
			Client = client;
		}
		public void ParseFilesCmdTaggedData(TaggedObject obj)
		{
			if (obj.ContainsKey("depotFile"))
			{
				base.DepotPath = new DepotPath(obj["depotFile"]);
			}

			if (obj.ContainsKey("clientFile"))
			{
				base.ClientPath = new ClientPath(obj["clientFile"]);
			}

			if (obj.ContainsKey("rev"))
			{
				int rev = -1;
				int.TryParse(obj["rev"], out rev);
				base.Version = new Revision(rev);
			}

			if (obj.ContainsKey("haveRev"))
			{
				int rev = -1;
				int.TryParse(obj["haveRev"], out rev);
				HaveRev = new Revision(rev);
			}

			if (obj.ContainsKey("change"))
			{
				int change = -1;
				int.TryParse(obj["change"], out change);
				ChangeId = change;
			}

			if (obj.ContainsKey("action"))
			{
				StringEnum<FileAction> Action = obj["action"];
			}

			if (obj.ContainsKey("type"))
			{
				Type = new FileType(obj["type"]);
			}

			if (obj.ContainsKey("time"))
			{
				SubmitTime = FormBase.ConvertUnixTime(obj["time"]);
			}

			if (obj.ContainsKey("user"))
			{
				User = obj["user"];
			}

			if (obj.ContainsKey("client"))
			{
				Client = obj["client"];
			}
		}

		public static File FromFilesCmdTaggedData(TaggedObject obj)
		{
			File val = new File();
			val.ParseFilesCmdTaggedData(obj);
			return val;
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
