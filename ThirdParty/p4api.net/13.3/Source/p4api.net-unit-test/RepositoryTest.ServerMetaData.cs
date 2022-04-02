using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.IO;

namespace p4api.net.unit.test
{


	/// <summary>
	///This is a test class for RepositoryTest and is intended
	///to contain RepositoryTest Unit Tests
	///</summary>
	public partial class RepositoryTest
	{

		/// <summary>
		///A test for GetServerMetaData
		///</summary>
		[TestMethod()]
		public void GetServerMetaDataTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
				Server server = new Server(new ServerAddress(uri));
				try
				{
					Repository rep = new Repository(server);

					using (Connection con = rep.Connection)
					{
						con.UserName = user;
						con.Client = new Client();
						con.Client.Name = ws_client;

						bool connected = con.Connect(null);
						Assert.IsTrue(connected);

						Assert.AreEqual(con.Status, ConnectionStatus.Connected);

						ServerMetaData s = rep.GetServerMetaData();

						Assert.IsNotNull(s);

						if (unicode == true)
						{
							Assert.AreEqual(s.UnicodeEnabled, true);
						}

						Assert.AreEqual(s.Root, "c:\\MyTestDir");

						Assert.AreEqual(s.CaseSensitive, false);

						Assert.AreEqual(s.MoveEnabled, true);

						
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

	}
}
