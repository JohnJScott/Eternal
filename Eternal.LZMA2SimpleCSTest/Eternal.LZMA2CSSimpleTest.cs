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

	public class ProgressInterfaceReporter
		: ProgressInterface
	{
		public ProgressInterfaceReporter()
		{
		}

		public override SevenZipResult Progress( int64 inSize, int64 outSize )
		{
			return SevenZipResult.SevenZipOK;
		}
	}

	[TestClass]
	public sealed class EternalLZMA2CSSimpleTest
	{
		private static CLzma2Result Compress2Data( CLzmaData compress )
		{
			ProgressInterfaceReporter progress_interface_reporter = new ProgressInterfaceReporter();
			int64 destination_length = compress.DestinationLength;

			CLzma2EncoderProperties encoder_properties = new CLzma2EncoderProperties();

			Assert.AreEqual( SevenZipResult.SevenZipOK, Lzma2Lib.Lzma2Compress( compress, encoder_properties, out CLzma2Result result, progress_interface_reporter ), "Compression should have succeeded" );

			ConsoleLogger.Log( $"LZMA2: Compressed {compress.SourceLength} to {result.OutputLength}" );

			Assert.AreEqual( destination_length, compress.DestinationLength, "Destination length should not have changed" );
			return result;
		}

		private static CLzma2Result Decompress2Data( CLzmaData decompress, uint8 property )
		{
			int64 destination_length = decompress.DestinationLength;

			CLzma2Result result = new CLzma2Result();

			result.PropertySummary = property;
			Assert.AreEqual( SevenZipResult.SevenZipOK, Lzma2Lib.Lzma2Decompress( decompress, ref result ), "Decompression should have succeeded" );

			ConsoleLogger.Log( $"LZMA2: Decompressed {decompress.SourceLength} to {result.OutputLength}" );

			Assert.AreEqual( destination_length, decompress.DestinationLength, "Destination length should not have changed" );
			return result;
		}

		private static CLzmaData Create2Source( int64 size )
		{
			uint8[] source_data = new uint8[size];
			uint8[] destination_data = new uint8[size];

			for( int64 index = 0; index < size; index++ )
			{
				source_data[index] = ( uint8 )( ( index + 31 ) ^ 85 );
				destination_data[index] = 0;
			}

			CLzmaData compress = new CLzmaData( source_data, source_data.LongLength, destination_data, destination_data.LongLength );
			return compress;
		}

		private static CLzmaData Create2Destination( CLzmaData source, CLzma2Result result )
		{
			uint8[] source_data = new uint8[result.OutputLength];
			Array.Copy( source.DestinationData, source_data, result.OutputLength );
			uint8[] destination_data = new uint8[source.SourceLength];
			Array.Fill<uint8>( destination_data, 85 );

			CLzmaData decompress = new CLzmaData( source_data, source_data.LongLength, destination_data, destination_data.LongLength );
			return decompress;
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2Compress()
		{
			CLzma2Result result = new CLzma2Result();

			int64 size = 256u;

			CLzmaData compress = Create2Source( size );
			result = Compress2Data( compress );

			Assert.AreEqual( 243, result.OutputLength, "Compressed size incorrect" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );
		
			Assert.AreEqual( size, result.OutputLength, "Decompressed size incorrect" );
			Assert.IsTrue( compress.SourceData.SequenceEqual( decompress.DestinationData ), "Decompressed data must match source decompressed data" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2CompressLarge()
		{
			int64 size = 1024u * 1024u;

			CLzmaData compress = Create2Source( size );
			CLzma2Result result = Compress2Data( compress );

			Assert.AreEqual( 456, result.OutputLength, "Compressed size incorrect" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );

			Assert.AreEqual( size, result.OutputLength, "Decompressed size incorrect" );
			Assert.IsTrue( compress.SourceData.SequenceEqual( decompress.DestinationData ), "Decompressed data must match source decompressed data" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2CompressVeryLarge()
		{
			int64 size = 1024 * 1024 * 1024;

			CLzmaData compress = Create2Source( size );
			CLzma2Result result = Compress2Data( compress );

			Assert.AreEqual( 156625, result.OutputLength, "Compressed size should be 156625" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );

			Assert.AreEqual( size, result.OutputLength, "Decompressed size should be 1GB" );
			Assert.IsTrue( decompress.DestinationData.SequenceEqual( compress.SourceData ), "Decompressed data must match source decompressed data" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2CompressExtraLarge()
		{
			// This file is too big for C#
			
			// int64 gigabyte = 1024u * 1024u * 1024u;
			// int64 size = 6u * gigabyte;
			//
			// CLzmaData compress = Create2Source( size );
			// CLzma2Result result = Compress2Data( compress );
			//
			// Assert.AreEqual( 909083, result.OutputLength, "Compressed size should be 909083" );
			//
			// CLzmaData decompress = Create2Destination( compress, result );
			// result = Decompress2Data( decompress, result.PropertySummary );
			//
			// Assert.AreEqual( size, result.OutputLength, "Decompressed size should be 6GB" );
			// Assert.IsTrue( decompress.DestinationData.SequenceEqual( compress.SourceData ), "Decompressed data must match source decompressed data" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2FileCompress()
		{
			Common.SetWorkingDirectory();

			CLzmaData compress = Common.LoadFile( "Eternal.LZMA2SimpleTest/TestData/Sample01.bin" );
			CLzma2Result compress_result = Compress2Data( compress );

			CLzmaData decompress = Common.AllocateDecompressionBuffers( compress, compress_result.OutputLength );
			CLzma2Result decompress_result = Decompress2Data( decompress, compress_result.PropertySummary );

			Assert.AreEqual( compress.SourceLength, decompress.DestinationLength, "Decompressed file should be the same length as the source file" );
			Assert.AreEqual( compress.SourceLength, decompress_result.OutputLength, "Decompressed file should be the same length as the source file" );
			Assert.IsTrue( decompress.DestinationData.SequenceEqual( compress.SourceData ), "Decompressed data must match source decompressed data" );
		}

		private static void TestCompression( CLzmaData compress, CLzma2EncoderProperties encoderProperties )
		{
			CLzma2Result decompress_result = new CLzma2Result();

			DateTime start_compress = DateTime.UtcNow;
			Assert.AreEqual( SevenZipResult.SevenZipOK, Lzma2Lib.Lzma2Compress( compress, encoderProperties, out CLzma2Result compress_result, null ), "Compression should have succeeded" );
			TimeSpan compress_s = DateTime.UtcNow - start_compress;

			CLzmaData decompress = Common.AllocateDecompressionBuffers( compress, compress_result.OutputLength );
			decompress_result.PropertySummary = compress_result.PropertySummary;
			DateTime start_decompress = DateTime.UtcNow;
			Assert.AreEqual( SevenZipResult.SevenZipOK, Lzma2Lib.Lzma2Decompress( decompress, ref decompress_result ), "Decompression should have succeeded" );
			TimeSpan decompress_s = DateTime.UtcNow - start_decompress;

			Assert.AreEqual( compress.SourceLength, decompress_result.OutputLength, "Decompressed file should be the same length as the source file" );
			Assert.IsTrue( compress.SourceData.SequenceEqual( decompress.DestinationData ), "Decompressed data must match source decompressed data" );

			Common.Log( $"{encoderProperties.CompressionLevel}, {encoderProperties.LiteralContextBits}, {encoderProperties.LiteralPositionBits}, {encoderProperties.PositionBits}, {encoderProperties.FastBytes}, {encoderProperties.MatchCycles},{encoderProperties.DictionarySize}," +
			            $" {compress.SourceLength}, {compress_result.OutputLength}, {compress_s.TotalSeconds}, {decompress_s.TotalSeconds}" );
		}

		private static void ExhaustiveTest( string fileName )
		{
			Common.SetWorkingDirectory();

			CLzmaData compress = Common.LoadFile( fileName );

			Common.Log( $"Testing: {fileName}" );
			Common.Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			/* 0 <= Level <= 9 */
			for(uint8 level = 3; level <= 9; level++ )
			{
				/* 0 <= LiteralContextBits <= 4, default = 3 */
				for(uint8 lc = 0; lc <= 4; lc++ )
				{
					/* 0 <= LiteralPositionBits <= 4, default = 0 */
					for(uint8 lp = 0; lp <= 4; lp++ )
					{
						/* 0 <= PositionBits <= 4, default = 2 */
						for(uint8 pb = 0; pb <= 4; pb += 2 )
						{
							if(lc + lp <= Lzma.MaxCombinedLiteralBits )
							{
								CLzma2EncoderProperties encoder_properties = new CLzma2EncoderProperties();

								encoder_properties.CompressionLevel = level;
								encoder_properties.LiteralContextBits = lc;
								encoder_properties.LiteralPositionBits = lp;
								encoder_properties.PositionBits = pb;

								TestCompression( compress, encoder_properties );
							}
						}
					}
				}
			}
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2Exhaustive()
		{
			ExhaustiveTest( "Eternal.LZMA2SimpleTest/TestData/Sample01.bin" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2ExhaustiveBC1()
		{
			ExhaustiveTest( "Eternal.LZMA2SimpleTest/TestData/SampleBC1.bin" );
		}

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2ExhaustiveBC3()
		{
			ExhaustiveTest( "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin" );
		}

		private static uint32[] dictionary_sizes =
		[
			0,
			1u << 11,
			1u << 12,
			1u << 13,
			65537,
			65536 + 32768,
			( 1u << 25 ) + ( 1u << 12 ),
			( 15u << 24 ),
			
			// Hitting the 2GB C# array limit with these
			// 1u << 30,
			// ( 1u << 30 ) + ( 1u << 14 ),
			// ( 1u << 31 ) + ( 1u << 30 ),
			// 1u << 31,
			// uint32.MaxValue
		];

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2DictionarySize()
		{
			Common.SetWorkingDirectory();

			string file_name = "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin";
			CLzmaData compress = Common.LoadFile( file_name );

			Common.Log( $"Testing: {file_name}" );
			Common.Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			/* 0 <= Level <= 9 */
			for( uint8 level = 0; level <= 9; level++ )
			{
				foreach( uint32 dictionary_size in dictionary_sizes )
				{
					CLzma2EncoderProperties encoder_properties = new CLzma2EncoderProperties();

					encoder_properties.CompressionLevel = level;
					encoder_properties.DictionarySize = dictionary_size;
					TestCompression( compress, encoder_properties );
				}
			}
		}

		private static int16[] fast_bytes =
		[
			-1,
			0,
			2,
			3,
			4,
			5,
			31,
			64,
			128,
			256,
			275
		];

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2FastBytes()
		{
			Common.SetWorkingDirectory();

			string file_name = "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin";
			CLzmaData compress = Common.LoadFile( file_name );

			Common.Log( $"Testing: {file_name}" );
			Common.Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			/* 0 <= Level <= 9 */
			for( uint8 level = 0; level <= 9; level++ )
			{
				foreach( int16 fast_byte in fast_bytes )
				{
					CLzma2EncoderProperties encoder_properties = new CLzma2EncoderProperties();

					encoder_properties.CompressionLevel = level;
					encoder_properties.FastBytes = fast_byte;
					TestCompression( compress, encoder_properties );
				}
			}
		}

		private static uint32[] match_cycles =
		[
			0,
			1,
			2,
			6,
			8,
			16,
			1u << 8,
			1u << 10,
			1u << 12,
			1u << 14
		];

		[TestMethod]
		[TestCategory( "LZMA2-CS" )]
		[Priority( 22 )]
		public void TestLZMA2MatchCycles()
		{
			Common.SetWorkingDirectory();

			string file_name = "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin";
			CLzmaData compress = Common.LoadFile( file_name );

			Common.Log( "Testing: {file_name}" );
			Common.Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			/* 0 <= Level <= 9 */
			for( uint8 level = 0; level <= 9; level++ )
			{
				foreach( uint32 match_cycle in match_cycles )
				{
					CLzma2EncoderProperties encoder_properties = new CLzma2EncoderProperties();

					encoder_properties.CompressionLevel = level;
					encoder_properties.MatchCycles = match_cycle;
					TestCompression( compress, encoder_properties );
				}
			}
		}
	}
}
