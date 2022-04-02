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
		///A test for CreateUser
		///</summary>
		[TestMethod()]
		public void CreateUserTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetUser = "thenewguy";

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

						User u = new User();
						u.Id = targetUser;
						u.FullName = "The New Guy";
						u.Password = "ChangeMe!";
						u.EmailAddress = "newguy@p4test.com";

						con.UserName = targetUser;
						connected = con.Connect(null);
						Assert.IsTrue(connected);

						User newGuy = rep.CreateUser(u);

						Assert.IsNotNull(newGuy);
						Assert.AreEqual(targetUser, newGuy.Id);
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
		///A test for DeleteUser
		///</summary>
		[TestMethod()]
		public void DeleteUserTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetUser = "deleteme";

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

						User u = new User();
						u.Id = targetUser;

						Options uFlags = new Options(UserCmdFlags.Force);
						rep.DeleteUser(u, uFlags);

						IList<User> u2 = rep.GetUsers(new Options(UsersCmdFlags.None, -1), targetUser);

						Assert.IsNull(u2);
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
		///A test for GetUser
		///</summary>
		[TestMethod()]
		public void GetUserTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetUser = "Alex";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				if (unicode)
					targetUser = "Алексей";
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

						User u = rep.GetUser(targetUser, null);

						Assert.IsNotNull(u);
						Assert.AreEqual(targetUser, u.Id);
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
		///A test for GetUsers
		///</summary>
		[TestMethod()]
		public void GetUsersTest()
		{
			bool unicode = false;

			string uri = "localhost:6666";
			string user = "admin";
			string pass = string.Empty;
			string ws_client = "admin_space";

			string targetUser = "Alex";

			for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
			{
				if (unicode)
					targetUser = "Алексей";
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

						IList<User> u = rep.GetUsers(new Options(UsersCmdFlags.IncludeAll, 2));

						Assert.IsNotNull(u);
						Assert.AreEqual(2, u.Count);

						u = rep.GetUsers(new Options(UsersCmdFlags.IncludeAll, -1), "admin", "Alice");

						Assert.IsNotNull(u);
						Assert.AreEqual(2, u.Count);

						u = rep.GetUsers(new Options(UsersCmdFlags.IncludeAll, 3), "A*");

						Assert.IsNotNull(u);
						if (unicode)
							Assert.AreEqual(2, u.Count); // no user 'Alex' on unicode server
						else
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
