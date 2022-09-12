// Copyright 2022 Eternal Developments LLC. All Rights Reserved.

global using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;
using Eternal.SourceServerIndexer;

namespace Eternal.SourceServerIndexer.Test
{
    [TestClass]
    public class SourceServerIndexerTests
    {
        [TestMethod("Validates the Debugging Tools are installed")]
        public void ValidateEnvironmentTest()
        {
	        Assert.IsTrue( SourceServerIndexer.ValidateEnvironment(), "Failed to find Debugging Tools installed." );
        }
    }
}