using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// Specifies user credentials for a specific connection. 
	/// </summary>
	public class Credential
	{
		public String Ticket { get; private set; }
		internal String UserName { get; private set; }
		public DateTime Expires { get; private set; }

		internal Credential(string user, string password)
		{
			UserName = user;
			Ticket = password;
			Expires = DateTime.MaxValue;
		}

		internal Credential(string user, string password, DateTime expires)
		{
			UserName = user;
			Ticket = password;
			Expires = expires;
		}

		public override string ToString()
		{
			return string.Format("User: {0}, Expires: {1} {2}", UserName, Expires.ToShortDateString(), Expires.ToShortTimeString());
		}
	}
}
