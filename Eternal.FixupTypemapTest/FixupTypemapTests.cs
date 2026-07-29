// Copyright Eternal Developments LLC. All Rights Reserved.

using Perforce.P4;
using Eternal.FixupTypemap;

[assembly: Parallelize( Scope = ExecutionScope.MethodLevel )]

namespace Eternal.FixupTypemapTest
{
	[TestClass]
	public sealed class FixupTypemapTests
	{
		[TestMethod( DisplayName="Check the validity of the type change" )]
		public void CheckChangeTest()
		{
			foreach( BaseFileType source_type in Enum.GetValues<BaseFileType>() )
			{
				foreach( BaseFileType dest_type in Enum.GetValues<BaseFileType>() )
				{
					if( source_type != BaseFileType.Unspecified && dest_type != BaseFileType.Unspecified )
					{
						FileType source = new FileType( source_type, FileTypeModifier.None );
						FileType dest = new FileType( dest_type, FileTypeModifier.None );
						Assert.AreNotEqual( FixupTypemap.FixupTypemap.FileTypeChangeSafety.Unknown, FixupTypemap.FixupTypemap.CheckChange( source, dest ), $"Changing type {source_type} to {dest_type} is unhandled." );
					}
				}
			}
		}
	}
}
