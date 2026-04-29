// Copyright Eternal Developments LLC. All Rights Reserved.

global using Microsoft.VisualStudio.TestTools.UnitTesting;
using Eternal.ConsoleUtilities;
using Eternal.LZMA2SimpleCS.CS;
using System;
using System.Linq;

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

	[TestClass]
	public sealed class EternalLZMA2ConsistencyTest
	{
		private static void TestCompression( CLzmaData compress, CLzma2EncoderProperties encoderProperties, string outputName )
		{
			Assert.AreEqual( SevenZipResult.SevenZipOK, Lzma2Lib.Lzma2Compress( compress, encoderProperties, out CLzma2Result compress_result, null ), "Compression should have succeeded" );

			uint8[] compressed_data = new uint8[( int32 )compress_result.OutputLength];
			Array.Copy( compress.DestinationData, compressed_data, ( int32 )compress_result.OutputLength );
			Common.SaveFile( $"Intermediate\\TestData\\refactored-cs\\{outputName}.compressed", compressed_data );

			CLzmaData reference = Common.LoadFile( $"Intermediate\\TestData\\refactored\\{outputName}.compressed" );
			Assert.IsTrue( reference.SourceData.SequenceEqual( compressed_data ), "C# Compressed data must match C++ compressed data" );
		}

		private static void ConsistencyTest( string fileName )
		{
			Common.SetWorkingDirectory();

			CLzmaData compress = Common.LoadFile( "Eternal.LZMA2SimpleTest/TestData/" + fileName + ".bin" );

			Common.Log( $"Testing: {fileName}" );

			/* 0 <= Level <= 9 */
			for( uint8 level = 3; level <= 9; level++ )
			{
				/* 0 <= LiteralContextBits <= 4, default = 3 */
				for( uint8 lc = 0; lc <= 4; lc++ )
				{
					/* 0 <= LiteralPositionBits <= 4, default = 0 */
					for( uint8 lp = 0; lp <= 4; lp++ )
					{
						/* 0 <= PositionBits <= 4, default = 2 */
						for( uint8 pb = 0; pb <= 4; pb += 2 )
						{
							if( lc + lp <= Lzma.MaxCombinedLiteralBits )
							{
								CLzma2EncoderProperties encoder_properties = new CLzma2EncoderProperties();

								encoder_properties.CompressionLevel = level;
								encoder_properties.LiteralContextBits = lc;
								encoder_properties.LiteralPositionBits = lp;
								encoder_properties.PositionBits = pb;

								string output_name = fileName + $".l{level}.lc{lc}.lp{lp}.pb{pb}";

								TestCompression( compress, encoder_properties, output_name );
							}
						}
					}
				}
			}
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 60 )]
		public void TestLZMA2ConsistencyBC1()
		{
			ConsistencyTest( "SampleBC1" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 60 )]
		public void TestLZMA2ConsistencyBC3()
		{
			ConsistencyTest( "SampleBC3" );
		}
	}
}
