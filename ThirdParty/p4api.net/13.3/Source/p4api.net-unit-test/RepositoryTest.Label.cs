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
		///A test for CreateLabel
		///</summary>
		[TestMethod()]
		public void CreateLabelTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = new Process();

				p4d = Utilities.DeployP4TestServer(TestDir, 9, unicode);

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

						Label l = new Label();
						l.Id = "newLabel";
						l.Owner = "admin";
						l.Description = "created by admin";
						l.Options = "unlocked";
						l.ViewMap = new ViewMap();
						string v0 = "//depot/main/...";
						string v1 = "//depot/rel1/...";
						string v2 = "//depot/rel2/...";
						string v3 = "//depot/dev/...";
						l.ViewMap.Add(v0);
						l.ViewMap.Add(v1);
						l.ViewMap.Add(v2);
						l.ViewMap.Add(v3);

						Label newLabel = rep.CreateLabel(l);

						Assert.IsNotNull(newLabel);
						Assert.AreEqual("newLabel", newLabel.Id);

                        string v4 = "\"//depot/rel2/a file with spaces\"";
                        newLabel.ViewMap.Add(v4);

                        newLabel = rep.UpdateLabel(newLabel);

                        Assert.IsNotNull(newLabel);
                        Assert.AreEqual(newLabel.ViewMap.Count,5);
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
		/// A test for DeleteLabel
		///</summary>
		[TestMethod()]
		public void DeleteLabelTest()
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

						IList<Label> llist = rep.GetLabels(null);

						Assert.IsNotNull(llist);

						Label deleteTarget = new Label();
						deleteTarget.Id = "admin_label";
						rep.DeleteLabel(deleteTarget, null);

						llist = rep.GetLabels(null);

						Assert.IsNull(llist);

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
        /// A test for LockLabel
        ///</summary>
        [TestMethod()]
        public void LockLabelTest()
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

                        Label l = rep.GetLabel("admin_label");
                        l.Locked = true;
                        rep.UpdateLabel(l);
                        Label l2 = rep.GetLabel("admin_label");
                        Assert.IsTrue(l2.Locked);
                        l2.Locked = false;
                        rep.UpdateLabel(l2);
                        Label l3 = rep.GetLabel("admin_label");
                        Assert.IsFalse(l3.Locked);
                        
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
		///A test for GetLabel
		///</summary>
		[TestMethod()]
		public void GetLabelTest()
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

						string targetLabel = "admin_label";

						Label l = rep.GetLabel(targetLabel);

						Assert.IsNotNull(l);

						Assert.AreEqual(targetLabel, l.Id);

						Assert.AreEqual(l.ViewMap.Count, 1);
						Assert.IsTrue(l.ViewMap[0].Type.Equals(MapType.Include));
						Assert.IsTrue(l.ViewMap[0].Left.Path.Equals("//depot/..."));

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
        ///A test for GetLabelWithRevision
        ///</summary>
        [TestMethod()]
        public void GetLabelWithRevisionTest()
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

                        string targetLabel = "newLabel2";

                        Label l = new Label();
                        l.Id = targetLabel;
                        l.Owner = "admin";
                        l.Description = "created by admin";
                        l.Options = "unlocked";
                        l.ViewMap = new ViewMap();
                        string v0 = "//depot/main/...";
                        string v1 = "//depot/rel1/...";
                        string v2 = "//depot/rel2/...";
                        string v3 = "//depot/dev/...";
                        l.ViewMap.Add(v0);
                        l.ViewMap.Add(v1);
                        l.ViewMap.Add(v2);
                        l.ViewMap.Add(v3);
                        l.Revision = "2";

                        rep.CreateLabel(l);

                        

                        l = rep.GetLabel(targetLabel);

                        Assert.IsNotNull(l);

                        Assert.AreEqual(targetLabel, l.Id);

                        Assert.AreEqual(l.Revision, "2");
                        Assert.AreEqual(l.ViewMap.Count, 4);
                        Assert.IsTrue(l.ViewMap[0].Type.Equals(MapType.Include));
                        Assert.IsTrue(l.ViewMap[0].Left.Path.Equals("//depot/main/..."));

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
		///A test for GetLabels
		///</summary>
		[TestMethod()]
		public void GetLabelsTest()
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

						Options ops = new Options();
						ops["-m"] = "1";
						IList<Label> l = rep.GetLabels(ops);


						Assert.IsNotNull(l);
						Assert.AreEqual(1, l.Count);
						Assert.AreEqual("Created by admin.\n", l[0].Description);

                        //now test for options set for a label that does not exist
                        ops = new Options();
                        ops["-u"] = "nonexistantuser";
                        l = rep.GetLabels(ops);
                        Assert.IsNull(l);
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
