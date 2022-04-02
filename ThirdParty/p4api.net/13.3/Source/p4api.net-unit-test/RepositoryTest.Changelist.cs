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
		///A test for CreateChangelist
		///</summary>
		[TestMethod()]
		public void CreateChangelistTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 7, unicode);
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

						Changelist c = new Changelist();
						c.Description = "New changelist for unit test";

						Options uFlags = new Options(ChangeCmdFlags.IncludeJobs);
						Changelist newGuy = rep.CreateChangelist(c, null);

						Assert.IsNotNull(newGuy);
						Assert.AreNotEqual(-1, newGuy.Id);

//						newGuy = rep.GetChangelist()
						Assert.AreEqual("admin", newGuy.OwnerName);
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
		///A test for UpdateChangelist
		///</summary>
		[TestMethod()]
		public void UpdateChangelistTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 7, unicode);
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

						Changelist c = rep.GetChangelist(5);
						c.Description = "new desc";
						rep.UpdateChangelist(c);

						Changelist d = rep.GetChangelist(5);
						Assert.AreEqual(d.Description, "new desc");
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
        ///A test for UpdateSubmittedChangelist
        ///</summary>
        [TestMethod()]
        public void UpdateSubmittedChangelistTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, 7, unicode);
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

                        Changelist c = rep.GetChangelist(12);
                        c.Description += "\n\tModified!";
                        rep.UpdateSubmittedChangelist(c,null);

                        Changelist d = rep.GetChangelist(12);
                        Assert.IsTrue(d.Description.Contains("Modified!"));

                        // on the non-unicode server edit the description
                        // of Alex's changelist 8 as an admin
                        if (!unicode)
                        {
                            c = rep.GetChangelist(8);
                            c.Description += "\n\tModified!";
                            Options opts = new Options();
                            opts["-f"] = null;
                            rep.UpdateSubmittedChangelist(c, opts);

                            d = rep.GetChangelist(8);
                            Assert.IsTrue(d.Description.Contains("Modified!"));
                        }

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
		///A test for DeleteChangelist
		///</summary>
		[TestMethod()]
		public void DeleteChangelistTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			//string targetChangelist = "admin_space2";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 7, unicode);
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

						Changelist c = new Changelist();
						c.Description = "New changelist for unit test";

						Changelist newGuy = rep.CreateChangelist(c, null);

						//ChangeOptions uFlags = new ChangeOptions(ChangelistFlags.Force);
						rep.DeleteChangelist(newGuy, null); //uFlags);


						Changelist deadGuy = null;
						try
						{
							deadGuy = rep.GetChangelist(newGuy.Id);
						}
						catch { }

						Assert.IsNull(deadGuy);
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
		///A test for GetChangelist
		///</summary>
		[TestMethod()]
		public void GetChangelistTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
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

						Changelist c = rep.GetChangelist(5, null);

						Assert.IsNotNull(c);
						Assert.AreEqual("admin", c.OwnerName);
						Assert.AreEqual(c.Files.Count, 1);
						Assert.AreEqual(c.Jobs.Count, 1);
						Assert.IsTrue(c.Files[0].DepotPath.Path.Contains("//depot/MyCode/NewFile.txt"));

						if (unicode == false)
						{
							c = rep.GetChangelist(4, null);
							Assert.AreEqual("admin", c.OwnerName);
							Assert.AreEqual(c.Files.Count, 2);
							Assert.AreEqual(c.Jobs.Count, 1);
							Assert.IsTrue(c.Files[0].DepotPath.Path.Contains("//depot/TheirCode/ReadMe.txt"));
							Assert.AreEqual(c.Files[0].Digest, "C7DECE3DB80A73F3F53AF4BCF6AC0576");
							Assert.AreEqual(c.Files[0].FileSize, 30);
						}

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
        ///A test for GetChangelistWithUTCConversion
        ///</summary>
        [TestMethod()]
        public void GetChangelistWithUTCConversionTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
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

                        P4Command cmd = new P4Command(rep, "change", true, "5");
                        Options opts = new Options();
                        opts["-o"] = null;
                        Changelist fromChangeCommand = new Changelist();

                        P4CommandResult results = cmd.Run(opts);
                        if (results.Success)
                        {
                            fromChangeCommand.initialize(rep.Connection);
                            fromChangeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                server.Metadata.DateTimeOffset);
                        }

                        SubmitResults sr = fromChangeCommand.Submit(null);
                        
                        int submitted = 17;
                        if (unicode)
                        {
                            submitted = 13;
                        }

                        cmd = new P4Command(rep, "change", true, submitted.ToString());
                        opts = new Options();
                        opts["-o"] = null;
                        fromChangeCommand = new Changelist();

                        results = cmd.Run(opts);
                        if (results.Success)
                        {
                            fromChangeCommand.initialize(rep.Connection);
                            fromChangeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                server.Metadata.DateTimeOffset);
                        }
                        
                        Changelist fromDescribeCommand = rep.GetChangelist(submitted, null);

                        Assert.AreEqual(fromDescribeCommand.ModifiedDate, fromChangeCommand.ModifiedDate);
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
        ///A test for GetChangelistWithUTCConversionNoTZDetails
        ///</summary>
        [TestMethod()]
        public void GetChangelistWithUTCConversionNoTZDetailsTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";


            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
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

                        P4Command cmd = new P4Command(rep, "describe", true, "5");
                        Changelist fromDescribeCommand = new Changelist();

                        P4CommandResult results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                string.Empty);
                        }

                        DateTime unconverted = fromDescribeCommand.ModifiedDate;

                        fromDescribeCommand = new Changelist();
                        results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                "-200");
                        }

                        Assert.AreEqual(unconverted.AddHours(-2), fromDescribeCommand.ModifiedDate);

                        fromDescribeCommand = new Changelist();
                        results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                "+200");
                        }

                        Assert.AreEqual(unconverted.AddHours(2), fromDescribeCommand.ModifiedDate);

                        fromDescribeCommand = new Changelist();
                        results = cmd.Run(null);
                        if (results.Success)
                        {
                            fromDescribeCommand.FromChangeCmdTaggedOutput((results.TaggedOutput[0]),
                                "200");
                        }

                        Assert.AreEqual(unconverted.AddHours(2), fromDescribeCommand.ModifiedDate);

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
		///A test for GetChangelists
		///</summary>
		[TestMethod()]
		public void GetChangelistsTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 6, unicode);
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

						IList<Changelist> u = rep.GetChangelists(
							new Options(ChangesCmdFlags.LongDescription, "admin_space", 10, ChangeListStatus.Pending, null));

						
						Assert.IsNotNull(u);
						Assert.AreEqual(3, u.Count);

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
