// Copyright Eternal Developments, LLC. All rights reserved.

#include "Common.h"

#include "../../ThirdParty/7-Zip/lzma2501/C/Lzma2Enc.h"
#include "../../ThirdParty/7-Zip/lzma2501/C/Lzma2Dec.h"

#pragma comment( lib, "OriginalSevenZip.lib" )

namespace EternalLZMA2SimpleTest
{
#define TEST_METHOD_CATEGORY( test_name, category_name ) \
	BEGIN_TEST_METHOD_ATTRIBUTE( test_name ) \
		TEST_METHOD_ATTRIBUTE( L"Category", L#category_name ) \
		TEST_PRIORITY( 50 ) \
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

	struct FMemoryWriter
		: ISeqOutStream
	{
		uint8* DestinationBytes{ nullptr };
		size_t Size{ 0 };
		size_t Offset{ 0 };

		bool Write( const void* buf, size_t size )
		{
			if( Offset + static_cast< int64 >( size ) < Size )
			{
				memcpy( DestinationBytes + Offset, buf, size );
				Offset += size;
				return true;
			}

			return false;
		}
	};

	/** Returns: result - the number of actually written bytes. (result < size) means error */
	static uint64 LZMA2Writer( const ISeqOutStream* p, const void* buf, size_t size )
	{
		FMemoryWriter* writer = static_cast< FMemoryWriter* >( const_cast< ISeqOutStream* >( p ) );
		if( writer->Write( buf, size ) )
		{
			return size;
		}

		return 0;
	}

	struct FMemoryReader
		: ISeqInStream
	{
		const uint8* SourceBytes{ nullptr };
		size_t Size{ 0 };
		size_t Offset{ 0 };

		bool Read( void* buf, size_t* size )
		{
			if( Offset == Size )
			{
				*size = 0;
				return false;
			}

			if( Offset + static_cast< int64 >( *size ) > Size )
			{
				*size = Size - Offset;
			}

			memcpy( buf, SourceBytes + Offset, *size );
			Offset += *size;
			return true;
		}
	};

	/** if (input(*size) != 0 && output(*size) == 0) means end_of_stream. (output(*size) < input(*size)) is allowed */
	static SRes LZMA2Reader( const ISeqInStream* p, void* buf, size_t* size )
	{
		FMemoryReader* reader = static_cast< FMemoryReader* >( const_cast< ISeqInStream* >( p ) );
		reader->Read( buf, size );
		return SZ_OK;
	}

	struct SevenZipAllocator
		: ISzAlloc
	{
		MemoryInterface& Allocator;
	};

	static void* SevenZipSmallMalloc( ISzAllocPtr handle, size_t size )
	{
		const SevenZipAllocator* allocator = static_cast< const SevenZipAllocator* >( handle );
		return allocator->Allocator.Alloc( size, "" );
	}

	static void* SevenZipLargeMalloc( ISzAllocPtr handle, size_t size )
	{
		const SevenZipAllocator* allocator = static_cast< const SevenZipAllocator* >( handle );
		return allocator->Allocator.Alloc( size, "" );
	}

	static void SevenZipSmallFree( ISzAllocPtr handle, void* allocated )
	{
		const SevenZipAllocator* allocator = static_cast< const SevenZipAllocator* >( handle );
		return allocator->Allocator.Free( allocated, 0, "" );
	}

	static void SevenZipLargeFree( ISzAllocPtr handle, void* allocated )
	{
		const SevenZipAllocator* allocator = static_cast< const SevenZipAllocator* >( handle );
		return allocator->Allocator.Free( allocated, 0, "" );
	}

	CLzma2EncProps CreateLzma2Props( CLzmaEncoderProperties* encoderProperties )
	{
		CLzma2EncProps result;

		result.lzmaProps.level = encoderProperties->CompressionLevel;
		result.lzmaProps.dictSize = encoderProperties->DictionarySize;
		result.lzmaProps.reduceSize = encoderProperties->EstimatedSourceDataSize;
		result.lzmaProps.lc = encoderProperties->LiteralContextBits;
		result.lzmaProps.lp = encoderProperties->LiteralPositionBits;
		result.lzmaProps.pb = encoderProperties->PositionBits;
		result.lzmaProps.algo = -1;
		result.lzmaProps.fb = encoderProperties->FastBytes;
		result.lzmaProps.btMode = -1;
		result.lzmaProps.numHashBytes = 4;
		result.lzmaProps.numHashOutBits = 0;
		result.lzmaProps.mc = encoderProperties->MatchCycles;
		result.lzmaProps.writeEndMark = encoderProperties->WriteEndMark;
		result.lzmaProps.numThreads = 1;

		result.blockSize = UINT64_MAX;
		result.numBlockThreads_Reduced = 1;
		result.numBlockThreads_Max = 1;
		result.numTotalThreads = 1;

		return result;
	}

	static MemoryInterface TestAllocator;

	static SevenZipResult Lzma2OriginalCompress( const CLzmaData* data, CLzmaEncoderProperties* encoderProperties, CLzma2Result* result, int32 numHashBytes, MemoryInterface* alloc, ProgressInterface* progress )
	{
		( void )alloc;
		( void )progress;

		result->Result = static_cast<SevenZipResult>( SZ_ERROR_DATA );
		CLzma2EncProps props = CreateLzma2Props( encoderProperties );
		props.lzmaProps.numHashBytes = numHashBytes;

		SevenZipAllocator small_alloc = { { SevenZipSmallMalloc, SevenZipSmallFree }, TestAllocator };
		SevenZipAllocator large_alloc = { { SevenZipLargeMalloc, SevenZipLargeFree }, TestAllocator };

		CLzma2EncHandle handle = Lzma2Enc_Create( &small_alloc, &large_alloc );
		Lzma2Enc_SetProps( handle, &props );

		/** Dict size - this needs passing to Lzma2Decode() */
		result->PropertySummary = Lzma2Enc_WriteProperties( handle );

		FMemoryReader in_stream = { { LZMA2Reader }, data->SourceData, static_cast<size_t>( data->SourceLength ), 0 };
		FMemoryWriter out_stream = { { LZMA2Writer }, data->DestinationData, static_cast<size_t>( data->DestinationLength ), 0 };

		result->Result = static_cast< SevenZipResult >( Lzma2Enc_Encode2( handle,
			&out_stream, nullptr, nullptr,
			&in_stream, nullptr, 0, nullptr ) );

		Lzma2Enc_Destroy( handle );

		result->OutputLength = static_cast<int64>( out_stream.Offset );

		return result->Result;
	}

	static SevenZipResult Lzma2OriginalDecompress( CLzmaData* data, CLzma2Result* result, MemoryInterface* alloc )
	{
		( void )alloc;

		result->Result = static_cast< SevenZipResult >( SZ_ERROR_DATA );

		SevenZipAllocator small_alloc = { { SevenZipSmallMalloc, SevenZipSmallFree }, TestAllocator };

		ELzmaStatus status = LZMA_STATUS_NOT_SPECIFIED;

		result->Result = static_cast< SevenZipResult >( Lzma2Decode( data->DestinationData, reinterpret_cast< SizeT* >( &data->DestinationLength ), data->SourceData, reinterpret_cast< SizeT* >( &data->SourceLength ), result->PropertySummary, LZMA_FINISH_END, &status, &small_alloc ) );

		result->OutputLength = data->DestinationLength;

		return result->Result;
	}

	TEST_CLASS( OriginalComparisonTest )
	{
		static void TestCompression( CLzmaData& compress, CLzmaData& compressOriginal, CLzma2EncoderProperties* encoderProperties, const std::string outputName )
		{
			Allocator compress_allocator;
			Allocator decompress_allocator;
			CLzma2Result compress_result;
			CLzma2Result compress_original_result;
			CLzma2Result decompress_result;
			CLzma2Result decompress_original_result;

			// Compress with the refactored version
			std::chrono::steady_clock::time_point start_compress = std::chrono::steady_clock::now();
			Assert::IsTrue( Lzma2Compress( &compress, encoderProperties, &compress_result, nullptr, nullptr ) == SevenZipResult::SevenZipOK, L"Compression should have succeeded" );
			const std::chrono::duration<double> compress_s = std::chrono::steady_clock::now() - start_compress;

			// Compress with the original version for comparison
			std::chrono::steady_clock::time_point start_original_compress = std::chrono::steady_clock::now();
			Assert::IsTrue( Lzma2OriginalCompress( &compressOriginal, encoderProperties, &compress_original_result, 4, nullptr, nullptr ) == SevenZipResult::SevenZipOK, L"Original compression should have succeeded" );
			const std::chrono::duration<double> compress_original_s = std::chrono::steady_clock::now() - start_original_compress;

			WriteBinaryFile( "Intermediate\\TestData\\refactored\\" + outputName + ".compressed", compress.DestinationData, compress_result.OutputLength );
			WriteBinaryFile( "Intermediate\\TestData\\original\\" + outputName + ".compressed", compressOriginal.DestinationData, compress_original_result.OutputLength );

			Assert::AreEqual( compress_result.OutputLength, compress_original_result.OutputLength, L"Compression should be identical" );
			Assert::IsTrue( memcmp( compress.DestinationData, compressOriginal.DestinationData, compress_result.OutputLength ) == 0, L"Compressed data must match originally compressed data" );

			// Decompress with the refactored version
			CLzmaData decompress = AllocateDecompressionBuffers( compress, compress_result.OutputLength );
			decompress_result.PropertySummary = compress_result.PropertySummary;
			std::chrono::steady_clock::time_point start_decompress = std::chrono::steady_clock::now();
			Assert::IsTrue( Lzma2Decompress( &decompress, &decompress_result, nullptr ) == SevenZipResult::SevenZipOK, L"Decompression should have succeeded" );
			const std::chrono::duration<double> decompress_s = std::chrono::steady_clock::now() - start_decompress;

			Assert::IsTrue( compress.SourceLength == decompress_result.OutputLength, L"Decompressed file should be the same length as the source file" );
			Assert::IsTrue( memcmp( decompress.DestinationData, compress.SourceData, decompress_result.OutputLength ) == 0, L"Decompressed data must match source decompressed data" );

			// Decompress with the original version for comparison
			CLzmaData decompress_original = AllocateDecompressionBuffers( compressOriginal, compress_original_result.OutputLength );
			decompress_original_result.PropertySummary = compress_original_result.PropertySummary;
			std::chrono::steady_clock::time_point start_original_decompress = std::chrono::steady_clock::now();
			Assert::IsTrue( Lzma2OriginalDecompress( &decompress_original, &decompress_original_result, nullptr ) == SevenZipResult::SevenZipOK, L"Decompression should have succeeded" );
			const std::chrono::duration<double> decompress_original_s = std::chrono::steady_clock::now() - start_original_decompress;

			Assert::IsTrue( compressOriginal.SourceLength == decompress_original_result.OutputLength, L"Decompressed file should be the same length as the source file" );
			Assert::IsTrue( memcmp( decompress_original.DestinationData, compressOriginal.SourceData, decompress_original_result.OutputLength ) == 0, L"Original decompressed data must match source decompressed data" );

			delete decompress.DestinationData;
			delete decompress_original.DestinationData;

			Log( "%u, %u, %f, %f, %f, %f",
				encoderProperties->CompressionLevel, encoderProperties->DictionarySize, compress_s.count(), compress_original_s.count(), decompress_s.count(), decompress_original_s.count() );
		}

		static void TestCompressionHashBytes( CLzmaData& compressOriginal, CLzma2EncoderProperties* encoderProperties )
		{
			Allocator compress_allocator;
			CLzma2Result compress2_result;
			CLzma2Result compress3_result;
			CLzma2Result compress4_result;
			CLzma2Result compress5_result;

			// Compress with the original version for comparison
			Assert::IsTrue( Lzma2OriginalCompress( &compressOriginal, encoderProperties, &compress2_result, 2, nullptr, nullptr ) == SevenZipResult::SevenZipOK, L"Original compression should have succeeded" );
			Assert::IsTrue( Lzma2OriginalCompress( &compressOriginal, encoderProperties, &compress3_result, 3, nullptr, nullptr ) == SevenZipResult::SevenZipOK, L"Original compression should have succeeded" );
			Assert::IsTrue( Lzma2OriginalCompress( &compressOriginal, encoderProperties, &compress4_result, 4, nullptr, nullptr ) == SevenZipResult::SevenZipOK, L"Original compression should have succeeded" );
			Assert::IsTrue( Lzma2OriginalCompress( &compressOriginal, encoderProperties, &compress5_result, 5, nullptr, nullptr ) == SevenZipResult::SevenZipOK, L"Original compression should have succeeded" );

			Log( "%u, %u, %d, %d, %d, %d",
				encoderProperties->CompressionLevel, encoderProperties->DictionarySize, compress2_result.OutputLength, compress3_result.OutputLength, compress4_result.OutputLength, compress5_result.OutputLength );
		}

		static void ExhaustiveTest( const std::string& fileName )
		{
			SetWorkingDirectory();

			CLzmaData compress = LoadFile( "Eternal.LZMA2SimpleTest/TestData/" + fileName + ".bin" );
			CLzmaData compress_original = LoadFile( "Eternal.LZMA2SimpleTest/TestData/" + fileName + ".bin" );

			Log( "Testing: %s", fileName.c_str() );
			Log( "Level, DictionarySize, compress time, original compress time, decompress time, original decompress time" );

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

								std::string output_name = fileName + ".l" + std::to_string( level ) + ".lc" + std::to_string( lc ) + ".lp" + std::to_string( lp ) + ".pb" + std::to_string( pb );

								TestCompression( compress, compress_original, &encoder_properties, output_name );
							}
						}
					}
				}
			}
		}

		static void ExhaustiveTestHashBytes( const std::string& fileName )
		{
			SetWorkingDirectory();

			CLzmaData compress = LoadFile( "Eternal.LZMA2SimpleTest/TestData/" + fileName + ".bin" );
			CLzmaData compress_original = LoadFile( "Eternal.LZMA2SimpleTest/TestData/" + fileName + ".bin" );

			Log( "Testing: %s", fileName.c_str() );
			Log( "Level, DictionarySize, 2 hash bytes size, 3 hash bytes size, 4 hash bytes size, 5 hash bytes size" );

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

								TestCompressionHashBytes( compress_original, &encoder_properties );
							}
						}
					}
				}
			}
		}

		TEST_METHOD_CATEGORY( TestCompareExhaustiveSample01, "Original" )
		{
			ExhaustiveTest( "Sample01" );
		}

		TEST_METHOD_CATEGORY( TestCompareExhaustiveSample02, "Original" )
		{
			ExhaustiveTest( "Sample02" );
		}

		TEST_METHOD_CATEGORY( TestCompareExhaustiveBC1, "Original" )
		{
			ExhaustiveTest( "SampleBC1" );
		}

		TEST_METHOD_CATEGORY( TestCompareExhaustiveBC3, "Original" )
		{
			ExhaustiveTest( "SampleBC3" );
		}

		TEST_METHOD_CATEGORY( TestCompareExhaustiveBC1HashBytes, "Original" )
		{
			ExhaustiveTestHashBytes( "SampleBC1" );
		}

		TEST_METHOD_CATEGORY( TestCompareExhaustiveBC3HashBytes, "Original" )
		{
			ExhaustiveTestHashBytes( "SampleBC3" );
		}
	};
}
