/**
 * @file Lzma2Lib.cpp
 * @brief LZMA2 library wrapper.
 *
 * 2015-06-13 : Igor Pavlov : Public domain
 */

#include "7zTypes.h"

#include "Lzma2Lib.h"
#include "Lzma2Dec.h"
#include "Lzma2Enc.h"

static MemoryInterface allocator;

class FMemoryWriter
	: public OutStreamInterface
{
public:
	FMemoryWriter( uint8* destinationData, int64 size )
	{
		DestinationData = destinationData;
		Size = size;
		Offset = 0;
	}

	virtual ~FMemoryWriter() override = default;

	/**
	 * @brief Writes bytes from the provided buffer to the output buffer.
	 *
	 * @param bufferBase Source buffer to write from.
	 * @param offset Byte offset within bufferBase to start reading.
	 * @param size Number of bytes to write.
	 * @return Number of bytes actually written; less than size indicates an error.
	 */
	virtual int64 Write( const uint8* bufferBase, const int64 offset, int64 size ) override
	{
		if( Offset + size < Size )
		{
			memcpy( DestinationData + Offset, bufferBase + offset, static_cast<uint64>( size ) );
			Offset += size;
			return size;
		}

		return 0;
	}

	/**
	 * @brief Returns the number of bytes written to the output buffer so far.
	 * @return Current write position as a byte offset from the start of the buffer.
	 */
	int64 GetOffset() const
	{
		return Offset;
	}

private:
	uint8* DestinationData;
	int64 Size;
	int64 Offset;
};

class FMemoryReader
	: public InStreamInterface
{
public:
	FMemoryReader( const uint8* sourceData, int64 size )
	{
		SourceData = sourceData;
		Size = size;
		Offset = 0;
	}

	virtual ~FMemoryReader() override = default;

	/**
	 * @brief Reads bytes from the source memory block into the provided buffer.
	 *
	 * @param bufferBase Destination buffer to read into.
	 * @param offset Byte offset within bufferBase to start writing.
	 * @param size On entry: maximum bytes to read. On exit: bytes actually read; 0 signals end of stream.
	 * @return SevenZipOK on success.
	 */
	virtual SevenZipResult Read( uint8* bufferBase, const int64 offset, int64* size ) override
	{
		if( Offset == Size )
		{
			*size = 0;
			return SevenZipResult::SevenZipOK;
		}

		if( Offset + *size > Size )
		{
			*size = Size - Offset;
		}

		memcpy( bufferBase + offset, SourceData + Offset, static_cast<uint64>( *size ) );
		Offset += *size;
		return SevenZipResult::SevenZipOK;
	}

private:
	const uint8* SourceData;
	int64 Size;
	int64 Offset;
};

/**
 * @brief Compresses a block of memory using LZMA2.
 *
 * @param data Source and destination buffers with their sizes.
 * @param encoderProperties Encoder configuration parameters.
 * @param result Receives the compression result, property summary byte, and output length.
 * @param alloc Memory allocator; pass nullptr to use the default allocator.
 * @param progress Optional progress callback; pass nullptr to disable.
 * @return SevenZipOK on success, or an error code.
 */
SevenZipResult Lzma2Compress( const CLzmaData* data, CLzma2EncoderProperties* encoderProperties, CLzma2Result* result, MemoryInterface* alloc, ProgressInterface* progress )
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

	FMemoryReader in_stream( data->SourceData, data->SourceLength );
	FMemoryWriter out_stream( data->DestinationData, data->DestinationLength );

	result->Result = Lzma2Encode( out_stream, in_stream, encoderProperties, &result->PropertySummary, alloc, progress );

	result->OutputLength = out_stream.GetOffset();
	return result->Result;
}

/**
 * @brief Decompresses a block of LZMA2-compressed memory.
 *
 * @param data Source and destination buffers with their sizes.
 * @param result On entry: must contain the PropertySummary byte from compression. On exit: receives the decompressed length and result code.
 * @param alloc Memory allocator; pass nullptr to use the default allocator.
 * @return SevenZipOK on success, or an error code.
 */
SevenZipResult Lzma2Decompress( CLzmaData* data, CLzma2Result* result, MemoryInterface* alloc )
{
	if( alloc == nullptr )
	{
		alloc = &allocator;
	}

	result->OutputLength = data->DestinationLength;
	result->Result = Lzma2Decode( data->DestinationData, result->OutputLength, data->SourceData, data->SourceLength, result->PropertySummary, result->FinishMode, result->Status, alloc );

	return result->Result;
}
