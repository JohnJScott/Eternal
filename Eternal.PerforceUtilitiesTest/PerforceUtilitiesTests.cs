// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Eternal.PerforceUtilities;

[assembly: Parallelize( Scope = ExecutionScope.MethodLevel )]

namespace Eternal.PerforceUtilitiesTest
{
	/// <summary>
	/// Basic tests for the PerforceUtilities class.
	/// </summary>
    [TestClass]
    public class PerforceUtilitiesTests
    {
		/// <summary>
		/// Finds the local Perforce connection based on the current directory.
		/// </summary>
		[TestMethod( DisplayName = "Get the local Perforce connection based on the current directory." )]
        public void GetConnectionInfo()
        {
	        string current_directory = Directory.GetCurrentDirectory();
	        PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( current_directory );
			Assert.IsGreaterThan( 0, connection_info.Workspace.Length, "Failed to get default connection" );
        }

		/// <summary>
		/// Syncs the default local connection to head.
		/// </summary>
        [TestMethod( DisplayName = "Get latest revision for all files in the workspace" )]
        public void SyncWorkspace()
        {
	        string current_directory = Directory.GetCurrentDirectory();
	        PerforceConnectionInfo connection_info = PerforceUtilities.PerforceUtilities.GetConnectionInfo( current_directory );

			Assert.IsTrue( PerforceUtilities.PerforceUtilities.Connect( connection_info ), "Failed to connect" );

			PerforceUtilities.PerforceUtilities.SyncWorkspace( connection_info );

	        Assert.IsTrue( PerforceUtilities.PerforceUtilities.Disconnect( connection_info ), "Failed to disconnect" );
        }
	}
}
