// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Eternal.PerforceUtilities;

namespace Eternal.PerforceUtilities.Test
{
    [TestClass]
    public class PerforceUtilitiesTest
    {
        [TestMethod("Get the local Perforce connection based on the current directory.")]
        public void GetConnectionInfo()
        {
	        string current_directory = Directory.GetCurrentDirectory();
	        PerforceConnectionInfo connection_info = PerforceUtilities.GetConnectionInfo( current_directory );
			Assert.IsTrue( connection_info.Workspace.Length > 0, "Failed to get default connection" );
        }

        [TestMethod("Get latest revision for all files in the workspace")]
        public void SyncWorkspace()
        {
	        string current_directory = Directory.GetCurrentDirectory();
	        PerforceConnectionInfo connection_info = PerforceUtilities.GetConnectionInfo( current_directory );

			Assert.IsTrue( PerforceUtilities.Connect( connection_info ), "Failed to connect" );

	        PerforceUtilities.SyncWorkspace( connection_info );

	        Assert.IsTrue( PerforceUtilities.Disconnect( connection_info ), "Failed to disconnect" );
        }

	}
}