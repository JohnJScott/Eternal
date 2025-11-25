// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

global using Microsoft.VisualStudio.TestTools.UnitTesting;

[assembly: Parallelize( Scope = ExecutionScope.MethodLevel )]

namespace Eternal.SourceServerIndexerTest
{
    [TestClass]
    public class SourceServerIndexerTests
    {
        [TestMethod( DisplayName = "Validates the Debugging Tools are installed" )]
        public void ValidateEnvironmentTest()
        {
	        Assert.IsTrue( SourceServerIndexer.SourceServerIndexer.ValidateEnvironment(), "Failed to find Debugging Tools installed." );
        }
    }
}
