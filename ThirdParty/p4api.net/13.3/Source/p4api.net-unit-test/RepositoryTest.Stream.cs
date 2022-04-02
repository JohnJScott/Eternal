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
        ///A test for CreateStream
        ///</summary>
        [TestMethod()]
        public void CreateStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = new Process();

                p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);
                
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

                            Stream s = new Stream();
                            string targetId = "//Rocket/rel1";
                            s.Id = targetId;
                            s.Type = StreamType.Release;
                            s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                            s.Parent = new DepotPath("//Rocket/main");
                            s.Name = "Release1";
                            s.Paths = new ViewMap();
							MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                            s.Paths.Add(p1);
							MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                            s.Paths.Add(p2);
                            s.OwnerName = "admin";
                            s.Description = "release stream for first release";
                            s.Ignored = new ViewMap();
							MapEntry ig1 = new MapEntry(MapType.Include, new DepotPath(".tmp"), null);
                            s.Ignored.Add(ig1);
							MapEntry ig2 = new MapEntry(MapType.Include, new DepotPath("/bmps/..."), null);
                            s.Ignored.Add(ig2);
							MapEntry ig3 = new MapEntry(MapType.Include, new DepotPath("/test"), null);
                            s.Ignored.Add(ig3);
							MapEntry ig4 = new MapEntry(MapType.Include, new DepotPath(".jpg"), null);
                            s.Ignored.Add(ig4);
							s.Remapped = new ViewMap();
							MapEntry re1 = new MapEntry(MapType.Include, new DepotPath("..."), new DepotPath("x/..."));
                            s.Remapped.Add(re1);
							MapEntry re2 = new MapEntry(MapType.Include, new DepotPath("y/*"), new DepotPath("y/z/*"));
                            s.Remapped.Add(re2);
							MapEntry re3 = new MapEntry(MapType.Include, new DepotPath("ab/..."), new DepotPath("a/..."));
                            s.Remapped.Add(re3);

                            Stream newStream = rep.CreateStream(s, null);


                            Assert.IsNotNull(newStream);
                            Assert.AreEqual(targetId, newStream.Id);

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
        ///A test for CreateTaskStream
        ///</summary>
        [TestMethod()]
        public void CreateTaskStreamTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = new Process();

                p4d = Utilities.DeployP4TestServer(TestDir, 8, unicode);

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

                        Stream s = new Stream();
                        string targetId = "//Rocket/rel1";
                        s.Id = targetId;
                        s.Type = StreamType.Task;
                        s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                        s.Parent = new DepotPath("//Rocket/main");
                        s.Name = "Release1";
                        s.Paths = new ViewMap();
                        MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
                        MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "admin";
                        s.Description = "release stream for first release";
                        s.Ignored = new ViewMap();
                        MapEntry ig1 = new MapEntry(MapType.Include, new DepotPath(".tmp"), null);
                        s.Ignored.Add(ig1);
                        MapEntry ig2 = new MapEntry(MapType.Include, new DepotPath("/bmps/..."), null);
                        s.Ignored.Add(ig2);
                        MapEntry ig3 = new MapEntry(MapType.Include, new DepotPath("/test"), null);
                        s.Ignored.Add(ig3);
                        MapEntry ig4 = new MapEntry(MapType.Include, new DepotPath(".jpg"), null);
                        s.Ignored.Add(ig4);
                        s.Remapped = new ViewMap();
                        MapEntry re1 = new MapEntry(MapType.Include, new DepotPath("..."), new DepotPath("x/..."));
                        s.Remapped.Add(re1);
                        MapEntry re2 = new MapEntry(MapType.Include, new DepotPath("y/*"), new DepotPath("y/z/*"));
                        s.Remapped.Add(re2);
                        MapEntry re3 = new MapEntry(MapType.Include, new DepotPath("ab/..."), new DepotPath("a/..."));
                        s.Remapped.Add(re3);

                        Stream newStream = rep.CreateStream(s, null);


                        Assert.IsNotNull(newStream);
                        Assert.AreEqual(targetId, newStream.Id);
                        Assert.AreEqual(newStream.Type,StreamType.Task);

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
        ///A test for DeleteStream
        ///</summary>
        [TestMethod()]
        public void DeleteStreamTest()
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

                        Stream s = new Stream();
                        string targetId = "//Rocket/rel1";
                        s.Id = targetId;
                        s.Type = StreamType.Release;
                        s.Options = new StreamOptionEnum(StreamOption.Locked | StreamOption.NoToParent);
                        s.Parent = new DepotPath("//Rocket/main");
                        s.Name = "Release1";
						s.Paths = new ViewMap();
						MapEntry p1 = new MapEntry(MapType.Import, new DepotPath("..."), null);
                        s.Paths.Add(p1);
						MapEntry p2 = new MapEntry(MapType.Share, new DepotPath("core/gui/..."), null);
                        s.Paths.Add(p2);
                        s.OwnerName = "admin";
                        s.Description = "release stream for first release";
						s.Ignored = new ViewMap();
						MapEntry ig1 = new MapEntry(MapType.Include, new DepotPath(".tmp"), null);
                        s.Ignored.Add(ig1);
						MapEntry ig2 = new MapEntry(MapType.Include, new DepotPath("/bmps/..."), null);
                        s.Ignored.Add(ig2);
						MapEntry ig3 = new MapEntry(MapType.Include, new DepotPath("/test"), null);
                        s.Ignored.Add(ig3);
						MapEntry ig4 = new MapEntry(MapType.Include, new DepotPath(".jpg"), null);
                        s.Ignored.Add(ig4);
                        s.Remapped = new ViewMap();
						MapEntry re1 = new MapEntry(MapType.Include, new DepotPath("..."), new DepotPath("x/..."));
                        s.Remapped.Add(re1);
						MapEntry re2 = new MapEntry(MapType.Include, new DepotPath("y/*"), new DepotPath("y/z/*"));
                        s.Remapped.Add(re2);
						MapEntry re3 = new MapEntry(MapType.Include, new DepotPath("ab/..."), new DepotPath("a/..."));
                        s.Remapped.Add(re3);

                        Stream newStream = rep.CreateStream(s, null);

                        Assert.IsNotNull(newStream);

						IList<Stream> slist = rep.GetStreams(new Options(StreamsCmdFlags.None, null, null, "//Rocket/rel1", -1));

						Assert.AreEqual(slist.Count, 35);

                        rep.DeleteStream(newStream, null);

						slist = rep.GetStreams(new Options(StreamsCmdFlags.None, null, null, "//Rocket/rel1", -1));

						Assert.AreEqual(slist.Count, 34);

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
        ///A test for GetStream
        ///</summary>
        [TestMethod()]
        public void GetStreamTest()
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

                        string targetStream = "//Rocket/GUI";

                        Stream s = rep.GetStream(targetStream);

                        Assert.IsNotNull(s);

                        Assert.AreEqual(targetStream, s.Id);
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                //unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for GetStreamWithSpacesViewMap
        ///</summary>
        [TestMethod()]
        public void GetStreamWithSpacesViewMapTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
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

                        Stream s = new Stream();
                        s.Id = "//Rocket/SPACE";
                        s.Name = "SPACE";
                        s.OwnerName = "admin";
                        s.Description = "stream with spaces in viewmap";
                        s.Type = StreamType.Development;
                        PathSpec thisParent = new DepotPath("//Rocket/MAIN");
                        s.Parent = thisParent;
                        s.Options = StreamOption.None;
                        List<string> map = new List<string>();
                        map.Add("import \"core/gui/...\" \"//Rocket/there are spaces/core/gui/...\"");
                        ViewMap spacesMap = new ViewMap(map);
                        s.Paths = spacesMap;

                        s = rep.CreateStream(s);


                        string targetStream = "//Rocket/SPACE";

                        s = rep.GetStream(targetStream);

                        Assert.IsNotNull(s);

                        Assert.AreEqual(targetStream, s.Id);
                        Assert.AreEqual(s.Type, StreamType.Development);
                        Assert.AreEqual(s.Paths[0].Type, MapType.Import);
                        Assert.IsTrue(s.Paths[0].Right.Path.Contains("there are spaces"));
                    }
                }
                finally
                {
                    Utilities.RemoveTestServer(p4d, TestDir);
                }
                //unicode = !unicode;
            }
        }

        /// <summary>
        ///A test for GetStreams
        ///</summary>
        [TestMethod()]
        public void GetStreamsTest()
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

                        IList<Stream> s = rep.GetStreams(new Options(StreamsCmdFlags.None,
                            "Parent=//flow/mainline & Type=development", null, "//...", 3));


                        Assert.IsNotNull(s);
                        Assert.AreEqual(3, s.Count);
                        Assert.AreEqual("D2", s[1].Name);

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
        ///A test for GetStreamsCheckDescription
        ///</summary>
        [TestMethod()]
        public void GetStreamsCheckDescriptionTest()
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

                        IList<Stream> s = rep.GetStreams(new Options(StreamsCmdFlags.None,
                            null, null, "//flow/D1", 3));


                        Assert.IsNotNull(s);
                        Assert.AreEqual(3, s.Count);
                        Assert.AreEqual("D2", s[1].Name);
                        Assert.IsTrue(s[0].Description.Contains("Introduces two new paths not in the parent, but shared to children"));

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
        ///A test for GetStreamMetaData
        ///</summary>
        [TestMethod()]
        public void GetStreamMetaDataTest()
        {
            bool unicode = false;

            string uri = "localhost:6666";
            string user = "admin";
            string pass = string.Empty;
            string ws_client = "admin_space";

            for (int i = 0; i < 2; i++) // run once for ascii, once for unicode
            {
                Process p4d = Utilities.DeployP4TestServer(TestDir, 13, unicode);
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

                        Stream s = rep.GetStream("//Rocket/GUI",null,null);
                        StreamMetaData smd = rep.GetStreamMetaData(s, null);
                        Assert.IsNotNull(smd);
                        Assert.AreEqual(smd.Stream, new DepotPath("//Rocket/GUI"));
                        Assert.AreEqual(smd.Parent, new DepotPath("//Rocket/MAIN"));
                        Assert.IsTrue(smd.ChangeFlowsToParent);
                        Assert.IsTrue(smd.ChangeFlowsFromParent);
                        Assert.IsFalse(smd.FirmerThanParent);
                        Assert.AreEqual(smd.Type, StreamType.Development);
                        Assert.AreEqual(smd.ParentType, StreamType.Mainline);
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
