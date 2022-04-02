using Perforce.P4;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace p4api.net.unit.test
{


	/// <summary>
	///This is a test class for RepositoryTest and is intended
	///to contain RepositoryTest Unit Tests
	///</summary>
	public partial class RepositoryTest
	{
		/// <summary>
		///A test for CreateDepot
		///</summary>
		[TestMethod()]
		public void CreateDepotTest()
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

						Depot d = new Depot();
						d.Id = "NewDepot";
						d.Description = "created by perforce";
						d.Owner = "admin";
						d.Type = DepotType.Stream;//.Local;
						d.Map = "NewDepot/...";

						Depot newDepot = rep.CreateDepot(d, null);

						Assert.IsNotNull(newDepot);

					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for DeleteDepot
		///</summary>
		[TestMethod()]
		public void DeleteDepotTest()
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

						Depot d = new Depot();
						d.Id = "NewDepot";
						d.Description = "created by perforce";
						d.Owner = "admin";
						d.Type = DepotType.Local;
						d.Map = "NewDepot/...";

						Depot newDepot = rep.CreateDepot(d, null);

						Assert.IsNotNull(newDepot);

						rep.DeleteDepot(newDepot, null);

						IList<Depot> dlist = rep.GetDepots();

						Assert.IsFalse(dlist.Contains(newDepot));

					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for GetDepot
		///</summary>
		[TestMethod()]
		public void GetDepotTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetDepot = "flow";

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

						Depot d = rep.GetDepot(targetDepot, null);

						Assert.IsNotNull(d);
						Assert.AreEqual(targetDepot, d.Id);
					}
				}
				finally
				{
					Utilities.RemoveTestServer(p4d, TestDir);
				}
				unicode = !unicode;
			}
		}

		/// <summary>
		///A test for GetDepots
		///</summary>
		[TestMethod()]
		public void GetDepotsTest()
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

						IList<Depot> dlist = rep.GetDepots();

						Assert.IsTrue(dlist[0].Id.Equals("depot"));
						Assert.IsTrue(dlist[1].Map.Equals("flow/..."));
						Assert.IsTrue(dlist[2].Type.Equals(DepotType.Stream));
						Assert.IsTrue(dlist[3].Description.Equals("Depot For 'Rocket' project\n\nEVENTS/new_stream_events/events0100_create_depots.pl-Event_001-perforce-CREATE_DEPOTS-Creating depots...\n"));

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
