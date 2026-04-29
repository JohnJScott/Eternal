// Copyright Eternal Developments, LLC. All rights reserved.

#include "Common.h"

namespace EternalLZMA2SimpleTest
{
#define TEST_METHOD_CATEGORY( test_name, category_name ) \
	BEGIN_TEST_METHOD_ATTRIBUTE( test_name ) \
		TEST_METHOD_ATTRIBUTE( L"Category", L#category_name ) \
		TEST_PRIORITY( 12 ) \
	END_TEST_METHOD_ATTRIBUTE() \
	TEST_METHOD( test_name )

	class Allocator
		: public MemoryInterface
	{
	public:
		Allocator()
		{
			TotalAllocated = 0;
		}

		virtual ~Allocator() override = default;

		virtual void* Alloc( const int64 size, const char* tag ) override
		{
			if( size != 0 )
			{
#ifdef _DEBUG
				Log( "Allocate, %lld, %s", size, tag );
#else
				( void )tag;
#endif
				TotalAllocated += size;
				return malloc( size );
			}

			return nullptr;
		}

		virtual void Free( void* address, const int64 size, const char* tag ) override
		{
			if( address != nullptr )
			{
#ifdef _DEBUG
				Log( "Free, %lld, %s", size, tag );
#else
				( void )tag;
#endif
				free( address );
				TotalAllocated -= size;
			}
		}

		int64 TotalAllocated;
	};

	class ProgressReporter
		: public ProgressInterface
	{
		public:
		ProgressReporter() = default;
		virtual ~ProgressReporter() override = default;

		virtual SevenZipResult Progress( int64 inSize, int64 outSize ) override
		{
			( void )inSize;
			( void )outSize;
			return SevenZipResult::SevenZipOK;
		}
	};

	TEST_CLASS( EternalLZMA2SimpleTest )
	{
	public:
		static CLzma2Result Compress2Data( CLzmaData& compress )
		{
			Allocator overridden_allocator;
			ProgressReporter progress_reporter;
			int64 destination_length = compress.DestinationLength;

			CLzma2Result result;
			CLzma2EncoderProperties encoder_properties;

			Assert::IsTrue( Lzma2Compress( &compress, &encoder_properties, &result, &overridden_allocator, &progress_reporter ) == SevenZipResult::SevenZipOK, L"Compression should have succeeded" );

			Log( "LZMA2: Compressed %lld to %lld", compress.SourceLength, result.OutputLength );

			Assert::IsTrue( destination_length == compress.DestinationLength, L"Destination length should not have changed" );
			Assert::AreEqual( 0ll, overridden_allocator.TotalAllocated, L"Mismatch in malloc/free in compression" );
			return result;
		}

		static CLzma2Result Decompress2Data( CLzmaData& decompress, uint8 property )
		{
			Allocator overridden_allocator;
			int64 destination_length = decompress.DestinationLength;

			CLzma2Result result;

			result.PropertySummary = property;
			Assert::IsTrue( Lzma2Decompress( &decompress, &result, &overridden_allocator ) == SevenZipResult::SevenZipOK, L"Decompression should have succeeded" );

			Log( "LZMA2: Decompressed %lld to %lld", decompress.SourceLength, decompress.DestinationLength );

			Assert::IsTrue( destination_length == decompress.DestinationLength, L"Destination length should not have changed" );
			Assert::AreEqual( 0ll, overridden_allocator.TotalAllocated,  L"Mismatch in malloc/free in decompression" );
			return result;
		}

		static CLzmaData Create2Source( int64 size )
		{
			CLzmaData compress;

			compress.SourceLength = size;
			compress.SourceData = new uint8[compress.SourceLength];
			compress.DestinationLength = size;
			compress.DestinationData = new uint8[compress.DestinationLength];

			for( int64 index = 0; index < size; index++ )
			{
				compress.SourceData[index] = static_cast< unsigned char >( ( index + 31 ) ^ 85 );
				compress.DestinationData[index] = 0;
			}

			return compress;
		}

		static CLzmaData Create2Destination( CLzmaData source, CLzma2Result result )
		{
			CLzmaData decompress;

			decompress.SourceLength = result.OutputLength;
			decompress.SourceData = new uint8[decompress.SourceLength];
			memcpy_s( decompress.SourceData, result.OutputLength, source.DestinationData, result.OutputLength );

			decompress.DestinationLength = source.SourceLength;
			decompress.DestinationData = new uint8[decompress.DestinationLength];
			memset( decompress.DestinationData, 85, decompress.DestinationLength );

			return decompress;
		}

		TEST_METHOD_CATEGORY( TestLZMA2Compress, "LZMA2" )
		{
			constexpr int64 size = 256;

			CLzmaData compress = Create2Source( size );
			CLzma2Result result = Compress2Data( compress );

			Assert::AreEqual( 243ll, result.OutputLength , L"Compressed size incorrect" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );

			Assert::AreEqual( size, result.OutputLength, L"Decompressed size incorrect" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2CompressLarge, "LZMA2" )
		{
			constexpr int64 size = 1024 * 1024;

			CLzmaData compress = Create2Source( size );
			CLzma2Result result = Compress2Data( compress );

			Assert::AreEqual( 456ll, result.OutputLength, L"Compressed size incorrect" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );

			Assert::AreEqual( size, result.OutputLength, L"Decompressed size incorrect" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2CompressVeryLarge, "LZMA2" )
		{
			const int64 size = 1024 * 1024 * 1024;

			CLzmaData compress = Create2Source( size );
			CLzma2Result result = Compress2Data( compress );

			Assert::AreEqual( 156625ll, result.OutputLength, L"Compressed size should be 156625" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );

			Assert::AreEqual( size, result.OutputLength, L"Decompressed size should be 1GB" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2CompressExtraLarge, "LZMA2" )
		{
			const int64 gigabyte = 1024 * 1024 * 1024;
			const int64 size = 6 * gigabyte;

			CLzmaData compress = Create2Source( size );
			CLzma2Result result = Compress2Data( compress );

			Assert::IsTrue( result.OutputLength == 938230, L"Compressed size should be 938230" );

			CLzmaData decompress = Create2Destination( compress, result );
			result = Decompress2Data( decompress, result.PropertySummary );

			Assert::IsTrue( result.OutputLength == size, L"Decompressed size should be 6GB" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2FileCompress, "LZMA2" )
		{
			SetWorkingDirectory();

			CLzmaData compress = LoadFile( "Eternal.LZMA2SimpleTest/TestData/Sample01.bin" );
			CLzma2Result compress_result = Compress2Data( compress );

			CLzmaData decompress = AllocateDecompressionBuffers( compress, compress_result.OutputLength );
			CLzma2Result decompress_result = Decompress2Data( decompress, compress_result.PropertySummary );

			Assert::IsTrue( compress.SourceLength == decompress.DestinationLength, L"Decompressed file should be the same length as the source file" );
			Assert::IsTrue( compress.SourceLength == decompress_result.OutputLength, L"Decompressed file should be the same length as the source file" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, decompress_result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );

			delete compress.SourceData;
			delete decompress.SourceData;
			delete decompress.DestinationData;
		}

		static void TestCompression( CLzmaData& compress, CLzma2EncoderProperties* encoderProperties )
		{
			Allocator compress_allocator;
			Allocator decompress_allocator;
			CLzma2Result compress_result;
			CLzma2Result decompress_result;

			std::chrono::steady_clock::time_point start_compress = std::chrono::steady_clock::now();
			Assert::IsTrue( Lzma2Compress( &compress, encoderProperties, &compress_result, &compress_allocator, nullptr ) == SevenZipResult::SevenZipOK, L"Compression should have succeeded" );
			const std::chrono::duration<double> compress_s = std::chrono::steady_clock::now() - start_compress;
			Assert::AreEqual( 0ll, compress_allocator.TotalAllocated, L"Mismatch in malloc/free in compression" );

			CLzmaData decompress = AllocateDecompressionBuffers( compress, compress_result.OutputLength );
			decompress_result.PropertySummary = compress_result.PropertySummary;
			std::chrono::steady_clock::time_point start_decompress = std::chrono::steady_clock::now();
			Assert::IsTrue( Lzma2Decompress( &decompress, &decompress_result, &decompress_allocator ) == SevenZipResult::SevenZipOK, L"Decompression should have succeeded" );
			const std::chrono::duration<double> decompress_s = std::chrono::steady_clock::now() - start_decompress;
			Assert::AreEqual( 0ll, decompress_allocator.TotalAllocated, L"Mismatch in malloc/free in decompression" );

			Assert::IsTrue( compress.SourceLength == decompress_result.OutputLength, L"Decompressed file should be the same length as the source file" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, decompress_result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );

			delete decompress.DestinationData;

			Log( "%u, %u, %u, %u, %u, %u, %u, %u, %u, %f, %f", 
				encoderProperties->CompressionLevel, encoderProperties->LiteralContextBits, encoderProperties->LiteralPositionBits, encoderProperties->PositionBits, encoderProperties->FastBytes, encoderProperties->MatchCycles,
				encoderProperties->DictionarySize, compress.SourceLength, compress_result.OutputLength, compress_s.count(), decompress_s.count() );
		}

		static void ExhaustiveTest( const std::string& fileName )
		{
			SetWorkingDirectory();

			CLzmaData compress = LoadFile( fileName );

			Log( "Testing: %s", fileName.c_str() );
			Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

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
							if( lc + lp <= Lzma::MaxCombinedLiteralBits )
							{
								CLzma2EncoderProperties encoder_properties;
								
								encoder_properties.CompressionLevel = level;
								encoder_properties.LiteralContextBits = lc;
								encoder_properties.LiteralPositionBits = lp;
								encoder_properties.PositionBits = pb;

								TestCompression( compress, &encoder_properties );
							}
						}
					}
				}
			}
		}

		TEST_METHOD_CATEGORY( TestLZMA2Exhaustive, "LZMA2" )
		{
			ExhaustiveTest( "Eternal.LZMA2SimpleTest/TestData/Sample01.bin" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2ExhaustiveBC1, "LZMA2" )
		{
			ExhaustiveTest( "Eternal.LZMA2SimpleTest/TestData/SampleBC1.bin" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2ExhaustiveBC3, "LZMA2" )
		{
			ExhaustiveTest( "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin" );
		}

		TEST_METHOD_CATEGORY( TestLZMA2DictionarySize, "LZMA2" )
		{
			SetWorkingDirectory();

			const std::string file_name = "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin";
			CLzmaData compress = LoadFile( file_name );

			Log( "Testing: %s", file_name.c_str() );
			Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			static const uint32 dictionary_sizes[12] =
			{
				0,
				1u << 11,
				1u << 12,
				1u << 13,
				65537,
				65536 + 32768,
				( 1u << 25 ) + ( 1u << 12 ),
				1u << 30,
				( 1u << 30 ) + ( 1u << 14 ),
				( 1u << 31 ) + ( 1u << 30 ),
				1u << 31,
				UINT32_MAX
			};

			/* 0 <= Level <= 9 */
			for( uint8 level = 0; level <= 9; level++ )
			{
				for( uint32 dictionary_size : dictionary_sizes )
				{
					CLzma2EncoderProperties encoder_properties;

					encoder_properties.CompressionLevel = level;
					encoder_properties.DictionarySize = dictionary_size;
					TestCompression( compress, &encoder_properties );
				}
			}
		}

		TEST_METHOD_CATEGORY( TestLZMA2FastBytes, "LZMA2" )
		{
			SetWorkingDirectory();

			const std::string file_name = "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin";
			CLzmaData compress = LoadFile( file_name );

			Log( "Testing: %s", file_name.c_str() );
			Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			static const int16 fast_bytes[12] =
			{
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
			};

			/* 0 <= Level <= 9 */
			for( uint8 level = 0; level <= 9; level++ )
			{
				for( int16 fast_byte : fast_bytes )
				{
					CLzma2EncoderProperties encoder_properties;

					encoder_properties.CompressionLevel = level;
					encoder_properties.FastBytes = fast_byte;
					TestCompression( compress, &encoder_properties );
				}
			}
		}


		TEST_METHOD_CATEGORY( TestLZMA2MatchCycles, "LZMA2" )
		{
			SetWorkingDirectory();

			const std::string file_name = "Eternal.LZMA2SimpleTest/TestData/SampleBC3.bin";
			CLzmaData compress = LoadFile( file_name );

			Log( "Testing: %s", file_name.c_str() );
			Log( "Level, LiteralContextBits, LiteralPositionBits, PositionBits, FastBytes, MatchCycles, DictionarySize, decompressed, compressed, compress time, decompress time" );

			static const uint32 match_cycles[12] =
			{
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
			};

			/* 0 <= Level <= 9 */
			for( uint8 level = 0; level <= 9; level++ )
			{
				for( uint32 match_cycle : match_cycles )
				{
					CLzma2EncoderProperties encoder_properties;

					encoder_properties.CompressionLevel = level;
					encoder_properties.MatchCycles = match_cycle;
					TestCompression( compress, &encoder_properties );
				}
			}
		}
	};
}
