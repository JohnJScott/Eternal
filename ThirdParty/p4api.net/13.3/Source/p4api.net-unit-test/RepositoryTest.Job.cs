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
		///A test for CreateJob
		///</summary>
		[TestMethod()]
		public void CreateJobTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetJob = "thenewjob";

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

						Job u = new Job();
						u.Id = targetJob;
						u.Add("Status", "open");
						u.Add("User", "admin");
						u.Add("Description", "this is a test job");

						Job newGuy = rep.CreateJob(u, null);

						Job equalJob = rep.GetJob("thenewjob");

						Assert.IsNotNull(equalJob);
						Assert.AreEqual(equalJob.Id, newGuy.Id);

						u = new Job();
						u.Id = "new";
						u.Add("Status", "open");
						u.Add("User", "admin");
						u.Add("Description", "this is a test job");

						newGuy = rep.CreateJob(u, null);

						equalJob = rep.GetJob(u.Id);

						Assert.IsNotNull(equalJob);
						Assert.AreEqual("job000002", newGuy.Id);

                        // test to confirm creating a new job without
                        // specifying .Id will not fail if there is a
                        // Job object in the spec
                        u = new Job();
                        u.Add("Job", "targetJobWithoutId");
                        u.Add("Status", "open");
                        u.Add("User", "admin");
                        u.Add("Description", "this is a test job");

                        newGuy = rep.CreateJob(u, null);

                        equalJob = rep.GetJob(u.Id);

                        Assert.IsNotNull(equalJob);
                        Assert.AreEqual("targetJobWithoutId", newGuy.Id);
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
        ///A test for UpdateJob
        ///</summary>
        [TestMethod()]
        public void UpdateJobTest()
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

                        Job fetchedJob = rep.GetJob("job000001");

                        fetchedJob["Description"] += "\n\tModified!";

                        rep.UpdateJob(fetchedJob);

                        Job fetchedJob2 = rep.GetJob("job000001");

                        Assert.IsTrue(fetchedJob2["Description"].ToString().Contains("Modified!"));
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
		///A test for DeleteJob
		///</summary>
		[TestMethod()]
		public void DeleteJobTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetJob = "job000002";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				Process p4d = Utilities.DeployP4TestServer(TestDir, 11, unicode);
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

						Job u = new Job();
						u.Id = targetJob;

						IList<Job> u2 = rep.GetJobs(new Options(JobsCmdFlags.LongDescriptions, null, 5));

						Assert.IsNotNull(u2);
						Assert.AreEqual(3, u2.Count);

						rep.DeleteJob(u, null);

						u2 = rep.GetJobs(new Options(JobsCmdFlags.LongDescriptions, null, 5));

						Assert.IsNotNull(u2);
						Assert.AreEqual(2, u2.Count);
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
		///A test for GetJob
		///</summary>
		[TestMethod()]
		public void GetJobTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetJob = "job000001";

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

						Job u = rep.GetJob(targetJob, null);

						Assert.IsNotNull(u);
						Assert.AreEqual(targetJob, u.Id);
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
		///A test for GetJobs
		///</summary>
		[TestMethod()]
		public void GetJobsTest()
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

                        FileSpec path = new FileSpec(new DepotPath(@"//depot/..."), null);
                        
						IList<Job> u = rep.GetJobs((new Options(JobsCmdFlags.LongDescriptions, null, 2)),path);

						Assert.IsNotNull(u);
						Assert.AreEqual(1, u.Count);

                        path = new FileSpec(new DepotPath(@"//depot/..."), null);

                        string jobView = "Test job";
                        u = rep.GetJobs((new Options(JobsCmdFlags.LongDescriptions, jobView, 2)), path);

                        Assert.IsNotNull(u);
                        Assert.AreEqual(1, u.Count);
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
