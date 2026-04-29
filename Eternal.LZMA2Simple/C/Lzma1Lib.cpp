/* Lzma1Lib.c -- LZMA library wrapper
2015-06-13 : Igor Pavlov : Public domain */

#include "7zTypes.h"

#include "Lzma1Lib.h"
#include "Lzma1Dec.h"
#include "Lzma1Enc.h"

static MemoryInterface allocator;

/**
 * The main LZMA1 compress function.
 */
SevenZipResult Lzma1Compress( const CLzmaData* data, CLzmaEncoderProperties* encoderProperties, CLzma1Result* result, MemoryInterface* alloc, ProgressInterface* progress )
{
	if( alloc == nullptr )
	{
		alloc = &allocator;
	}

	result->Result = encoderProperties->Normalize();
	if( result->Result != SevenZipResult::SevenZipOK )
	{
		return result->Result;
	}

	uint64 out_prop_size = 5;
	result->OutputLength = data->DestinationLength;
	result->Result = Lzma1Encode( data->DestinationData, result->OutputLength, data->SourceData, data->SourceLength, encoderProperties, result->Properties, out_prop_size, alloc, progress );

	return result->Result;
}

/**
 * The main LZMA1 decompress function.
 */
SevenZipResult Lzma1Decompress( CLzmaData* data, CLzma1Result* result, MemoryInterface* alloc )
{
	if( alloc == nullptr )
	{
		alloc = &allocator;
	}

	result->OutputLength = data->DestinationLength;
	result->Result = Lzma1Decode( data->DestinationData, result->OutputLength, data->SourceData, data->SourceLength, result->Properties, 5, result->FinishMode, result->Status, alloc );

	return result->Result;
}
