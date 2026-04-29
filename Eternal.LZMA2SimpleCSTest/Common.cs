// Copyright Eternal Developments, LLC. All rights reserved.

using Eternal.LZMA2SimpleCS.CS;
using Microsoft.VisualStudio.TestTools.UnitTesting.Logging;

namespace Eternal.LZMA2SimpleCSTest
{
	using int16 = Int16;
	using int32 = Int32;
	using int64 = Int64;
	using int8 = SByte;
	using uint16 = UInt16;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	internal class Common
	{
		public static CLzmaData LoadFile( string filename )
		{
			FileInfo file_info = new FileInfo( filename );
			Assert.IsTrue( file_info.Exists );

			uint8[] source_data = new uint8[file_info.Length];
			using ( FileStream instream = new FileStream( filename, FileMode.Open, FileAccess.Read ) )
			{
				instream.ReadExactly( source_data, 0, ( int32 )file_info.Length );
			}

			uint8[] destination_data = new uint8[Lzma1Lib.LzmaWorstCompression( source_data.Length )];

			return new CLzmaData( source_data, source_data.LongLength, destination_data, destination_data.LongLength );
		}

		public static void SaveFile( string filename, uint8[] data )
		{
			using( FileStream outstream = new FileStream( filename, FileMode.Create, FileAccess.Write ) )
			{
				outstream.Write( data, 0, data.Length );
			}
		}

		public static CLzmaData AllocateDecompressionBuffers( CLzmaData compressed, int64 compressedSize )
		{
			uint8[] destination_data = new uint8[compressed.SourceLength];
			CLzmaData decompress = new CLzmaData( compressed.DestinationData, compressedSize, destination_data, compressed.SourceLength );

			return decompress;
		}

		public static void SetWorkingDirectory()
		{
			// Get the location of the current test assembly
			string assembly_location = System.Reflection.Assembly.GetExecutingAssembly().Location;
			string assembly_directory = Path.GetDirectoryName( assembly_location )!;

			// Navigate up from bin\Debug\net10.0 to project root (3 levels)
			// Then up one more level to solution root
			string solution_root = Path.GetFullPath( Path.Combine( assembly_directory, "..", "..", ".." ) );

			Directory.SetCurrentDirectory( solution_root );

			Directory.CreateDirectory( "Intermediate\\TestData\\refactored-cs" );
		}

		public static void Log( string log )
		{
			Logger.LogMessage( log );
		}
	}
}