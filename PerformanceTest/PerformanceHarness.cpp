// Copyright Eternal Developments, LLC. All Rights Reserved.

#include <string>
#include <filesystem>

#include "../Eternal.LZMA2Simple/C/7zTypes.h"
#include "../Eternal.LZMA2Simple/C/Lzma2Lib.h"

#include "../Eternal.LZMA2Utilities/Utilities.h"

#pragma comment( lib, "Eternal.LZMA2Utilities.lib" )

static void TestCompression( const std::string& fileName )
{
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

						std::string input_name = fileName + ".l" + std::to_string( level ) + ".lc" + std::to_string( lc ) + ".lp" + std::to_string( lp ) + ".pb" + std::to_string( pb );
						CLzmaData compress = LoadFile( "Intermediate\\TestData\\refactored\\" + input_name + ".compressed" );

						// Decompress with the refactored version
						CLzmaData decompress = AllocateDecompressionBuffers( compress, compress.SourceLength );
						CLzma2Result decompress_result;
						decompress_result.PropertySummary = LoadProperties( "Intermediate\\TestData\\refactored\\" + input_name + ".meta" );
						Lzma2Decompress( &decompress, &decompress_result, nullptr );	

						delete decompress.DestinationData;
					}
				}
			}
		}
	}
}

int32 main( int32 , char* )
{
	SetWorkingDirectory();
	TestCompression( "SampleBC3" );
}