// Copyright Eternal Developments, LLC. All rights reserved.

#include "Common.h"

namespace EternalLZMA2SimpleTest
{
#define TEST_METHOD_CATEGORY( test_name, category_name ) \
	BEGIN_TEST_METHOD_ATTRIBUTE( test_name ) \
		TEST_METHOD_ATTRIBUTE( L"Category", L#category_name ) \
		TEST_PRIORITY( 0 ) \
	END_TEST_METHOD_ATTRIBUTE() \
	TEST_METHOD( test_name )
	
// (crc[0 ... 255] & 0xFF) provides one-to-one correspondence to [0 ... 255]
#define CRC_POLYNOMIAL				0xEDB88320

	TEST_CLASS( EternalUtility )
	{
		TEST_METHOD_CATEGORY( GenerateCRCTable, "Utility" )
		{
			// Generate CRC table
			uint32 crc[256];

			for( uint32 i = 0; i < 256; i++ )
			{
				uint32 r = i;
				for( uint32 j = 0; j < 8; j++ )
				{
					r = ( r >> 1 ) ^ ( CRC_POLYNOMIAL & ( 0u - ( r & 1u ) ) );
				}

				crc[i] = r;
			}

			// Log it as C code
			Log( "static const uint32 kCrcTable[256] =\n{" );
			for( uint32 i = 0; i < 256; i++ )
			{
				if( ( i % 16 ) == 0 )
				{
					Log( "\n" );
				}

				Log( "0x%08X, ", crc[i] );
			}

			Log( "\n};\n" );
		}

		// highBit is the msb of the integer
		// const uint32 slot = ( highBit << 1 ) + ( lookup >= ( 3u << ( highBit - 1 ) ) );

		static uint8 GetBlockSize( const uint32 pos )
		{
			if( pos < 5 )
			{
				return static_cast< uint8 >( pos & 0xff );
			}

			uint32 msb = 1;
			uint32 lookup = pos >> 2;
			while( lookup != 0 )
			{
				lookup >>= 1;
				msb++;
			}

			const uint8 slot = static_cast< uint8 >( ( ( msb << 1 ) + ( pos >= ( 3u << ( msb - 1 ) ) ? 1u : 0u ) ) & 0xff );
			return slot;
		}

		TEST_METHOD_CATEGORY( CountFastPosTable, "Utility" )
		{
			uint8 BlockSizeLookupTest[1u << Lzma::NumLogBits] = { 0 };
			uint8* table = BlockSizeLookupTest;
			table[0] = 0u;
			table[1] = 1u;
			table += 2u;

			for( uint32 slot = 2u; slot < Lzma::NumLogBits * 2u; slot++ )
			{
				uint32 k = 1u << ( ( slot >> 1u ) - 1u );
				for( uint32 j = 0; j < k; j++ )
				{
					table[j] = static_cast<uint8>(slot);
				}

				table += k;
			}

			// Find indices where slot changes
			// uint8 current = 255;
			// for ( uint32 count = 0; count < 1u << NUM_LOG_BITS; count++ )
			// {
			// 	if( BlockSizeLookup[count] != current )
			// 	{
			// 		Log( "Change from %d to %d at 0x%x", current, BlockSizeLookup[count], count );
			// 		current = BlockSizeLookup[count];
			// 	}
			// }

			Log( "Final count: %d", 1u << Lzma::NumLogBits );

			// Verify against GetBlockSize
			for( uint16 lookup = 0; lookup < ( 1u << Lzma::NumLogBits ); lookup++ )
			{
				Log( "Lookup: %d", lookup );
				Assert::AreEqual( BlockSizeLookupTest[lookup], GetBlockSize( lookup ), L"BlockSizeLookup mismatch" );
			}
		}

		TEST_METHOD_CATEGORY( GenerateFastPosTable, "Utility" )
		{
			Log( "static const uint8 BlockSizeLookup[1u << NUM_LOG_BITS] =\n{" );

			Log( "\n\t// Slot 0" );
			Log( "\n\t0x00, " );
			Log( "\n\n\t// Slot 1" );
			Log( "\n\n\t0x01, \n" );

			for( uint32 slot = 2u; slot < Lzma::NumLogBits * 2u; slot++ )
			{
				Log( "\n\t// Slot %u\n\t", slot );
				bool newline = false;
				uint32 k = 1u << ( ( slot >> 1u ) - 1u );
				for( uint32 j = 0; j < k; j++ )
				{
					Log( "0x%02X, ", slot );
					if( ( j % 32 ) == 31 )
					{
						Log( "\n" );
						newline = true;
					}
				}

				if( !newline )
				{
					Log( "\n" );
				}
			}

			Log( "\n};\n" );
		}

		TEST_METHOD_CATEGORY( ProbabilityPricesTable, "Utility" )
		{
			Log( "const CProbPrice CLzma1Enc::ProbabilityPrices[Lzma::NumBitModelTotalBits >> NUM_MOVE_REDUCING_BITS] =\n{\n\t" );

			for( uint32 i = 0u; i < ( Lzma::BitModelTableSize >> LzmaEncoder::NumMoveReducingBits ); i++ )
			{
				uint32 w = ( i << LzmaEncoder::NumMoveReducingBits ) + ( 1u << ( LzmaEncoder::NumMoveReducingBits - 1u ) );
				uint32 bit_count = 0u;
				for( uint32 j = 0u; j < LzmaEncoder::NumBitPriceShiftBits; j++ )
				{
					w = w * w;
					bit_count <<= 1;
					while( w >= LzmaEncoder::RangeEncoderBufferSize )
					{
						w >>= 1;
						bit_count++;
					}
				}

				uint32 prob = ( Lzma::NumBitModelTotalBits << LzmaEncoder::NumBitPriceShiftBits ) - 15u - bit_count;
				Log( "0x%02X, ", prob );
				if ( ( i % 32 ) == 31 )
				{
					if( i != ( Lzma::BitModelTableSize >> LzmaEncoder::NumMoveReducingBits ) - 1 )
					{
						Log( "\n\t" );
					}
				}
			}
			
			Log( "\n};\n" );
		}
	};
}
