/**
 * @file Lzma1Lib.cpp
 * @brief LZMA1 library wrapper.
 *
 * 2015-06-13 : Igor Pavlov : Public domain
 */

#include "7zTypes.h"

#include "Lzma1Lib.h"
#include "Lzma1Dec.h"
#include "Lzma1Enc.h"

static MemoryInterface allocator;

/**
 * @brief Compresses a block of memory using LZMA1.
 *
 * @param data              Source and destination buffers with their sizes.
 * @param encoderProperties Encoder configuration parameters.
 * @param result            Receives the compression result, properties, and output length.
 * @param alloc             Memory allocator; pass nullptr to use the default allocator.
 * @param progress          Optional progress callback; pass nullptr to disable.
 * @return SevenZipOK on success, or an error code.
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
 * @brief Decompresses a block of LZMA1-compressed memory.
 *
 * @param data   Source and destination buffers with their sizes.
 * @param result On entry: must contain the Properties from compression. On exit: receives the decompressed length and result code.
 * @param alloc  Memory allocator; pass nullptr to use the default allocator.
 * @return SevenZipOK on success, or an error code.
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
