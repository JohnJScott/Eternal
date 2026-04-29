/* Lzma2Enc.c -- LZMA2 Encoder
2021-02-09 : Igor Pavlov : Public domain */

#include "7zTypes.h"

#include "Lzma1Lib.h"
#include "Lzma1Enc.h"
#include "Lzma2Enc.h"

/* ---------- CheckedSeqInStream ---------- */

class CheckedSeqInStream
	: public InStreamInterface
{
public:
	CheckedSeqInStream( InStreamInterface& inRealStream )
		: RealStream( inRealStream )
	{
		Processed = 0u;
		Finished = false;
	}

	/**
	 * @brief Resets the processed-byte counter and the end-of-stream flag.
	 */
	void Reset()
	{
		Processed = 0u;
		Finished = false;
	}

	/**
	 * @brief Reads bytes from the underlying stream and accumulates the total bytes processed.
	 *
	 * @param bufferBase Destination buffer.
	 * @param offset     Byte offset into bufferBase to write to.
	 * @param size       On entry: maximum bytes to read.
	 *                   On exit:  bytes actually read (0 signals end of stream).
	 * @return SevenZipOK, or an error code from the underlying stream.
	 */
	virtual SevenZipResult Read( uint8* bufferBase, const int64 offset, int64* size ) override
	{
		SevenZipResult result = SevenZipResult::SevenZipOK;

		if( *size != 0u )
		{
			result = RealStream.Read( bufferBase, offset, size );
			Finished = ( *size == 0 );
			Processed += *size;
		}

		return result;
	}

	int64 Processed;
	bool Finished;

private:
	InStreamInterface& RealStream;
};

class Lzma2Enc
{
public:
	Lzma2Enc( const CLzma2EncoderProperties* encoderProperties, MemoryInterface* alloc, ProgressInterface* progress )
		: EncoderProperties( encoderProperties )
		, Alloc( alloc )
		, Progress( progress )
		, Encoder( encoderProperties, alloc, progress )
	{
		WorkBuffer = static_cast< uint8* >( Alloc->Alloc(Lzma::Lzma2MaxCompressedChunkSize, "Lzma2Enc::WorkBuffer" ) );
	}

	~Lzma2Enc()
	{
		if( WorkBuffer != nullptr )
		{
			Alloc->Free( WorkBuffer, Lzma::Lzma2MaxCompressedChunkSize, "Lzma2Enc::WorkBuffer" );
			WorkBuffer = nullptr;
		}
	}

	uint8 GetCodedDictionary() const;
	SevenZipResult InitStream();
	void InitBlock();
	SevenZipResult EncodeSubblock( int64& packSizeRes, OutStreamInterface& outStream );
	SevenZipResult EncodeStream( OutStreamInterface& outStream, InStreamInterface& inStream, bool finished );

	bool PropertiesAreSet = false;

private:
	const CLzmaEncoderProperties* EncoderProperties = nullptr;
	MemoryInterface* Alloc = nullptr;
	ProgressInterface* Progress = nullptr;
	uint8* WorkBuffer = nullptr;

	Lzma1Enc Encoder;
	int64 ExpectedDataSize = INT64_MAX;
	int64 SourcePosition = 0;

	uint8 PropertiesByte = 0;
	bool NeedInitState = false;
	bool NeedInitProp = false;
};

/* ---------- CLzma2EncInt ---------- */

/**
 * @brief Reads and caches the encoder property byte if it has not already been retrieved.
 *
 * @return SevenZipOK on success, or an error code from GetCodedProperties().
 */
SevenZipResult Lzma2Enc::InitStream()
{
	if( !PropertiesAreSet )
	{
		uint64 props_size = Lzma::LzmaPropertiesSize;
		uint8 props_encoded[Lzma::LzmaPropertiesSize];
		SevenZipResult result = Encoder.GetCodedProperties( props_encoded, props_size );
		if( result != SevenZipResult::SevenZipOK )
		{
			return result;
		}

		PropertiesByte = props_encoded[0];
		PropertiesAreSet = true;
	}

	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Resets per-block state, preparing the encoder to start a new LZMA2 block.
 */
void Lzma2Enc::InitBlock()
{
	SourcePosition = 0u;
	NeedInitState = true;
	NeedInitProp = true;
}

/**
 * @brief Encodes one LZMA2 sub-block, choosing between LZMA and copy mode automatically.
 *
 * @param packSizeRes On entry: maximum allowed compressed size for this sub-block.
 *                   On exit:  number of compressed bytes written to outStream.
 * @param outStream  Destination stream to write the encoded sub-block to.
 * @return SevenZipOK on success, SevenZipErrorOutputEof if the output limit is exceeded.
 */
SevenZipResult Lzma2Enc::EncodeSubblock( int64& packSizeRes, OutStreamInterface& outStream )
{
	const int64 pack_size_limit = packSizeRes;
	int64 pack_size = pack_size_limit;
	uint32 unpack_size = Lzma::Lzma2MaxUnpackSize;
	const uint32 lz_header_size = 5u + ( NeedInitProp ? 1u : 0u );
	bool use_copy_block;

	packSizeRes = 0;
	if( pack_size < static_cast<int64>( lz_header_size ) )
	{
		return SevenZipResult::SevenZipErrorOutputEof;
	}

	pack_size -= lz_header_size;

	Encoder.SaveState();
	SevenZipResult result = Encoder.CodeOneMemBlock( NeedInitState, WorkBuffer, lz_header_size, pack_size, Lzma::Lzma2MaxPackSize, unpack_size );

	if( unpack_size == 0u )
	{
		return result;
	}

	if( result == SevenZipResult::SevenZipOK )
	{
		use_copy_block = ( pack_size + 2u >= unpack_size ) || ( pack_size > static_cast<int64>( Lzma::Lzma2MaxPackSize ) );
	}
	else
	{
		if( result != SevenZipResult::SevenZipErrorOutputEof )
		{
			return result;
		}

		use_copy_block = true;
	}

	uint32 dest_position = 0;
	if( use_copy_block )
	{
		while( unpack_size > 0u )
		{
			const uint32 copy_chunk_size = ( unpack_size < Lzma::Lzma2CopyChunkSize ) ? unpack_size : Lzma::Lzma2CopyChunkSize;
			if( pack_size_limit - dest_position < copy_chunk_size + 3u )
			{
				return SevenZipResult::SevenZipErrorOutputEof;
			}

			WorkBuffer[dest_position++] = static_cast<uint8>( ( SourcePosition == 0 ? Lzma::Lzma2ControlCopyResetDict : Lzma::Lzma2ControlCopy ) & 0xff );
			WorkBuffer[dest_position++] = static_cast<uint8>( ( ( copy_chunk_size - 1u ) >> 8 ) & 0xff );
			WorkBuffer[dest_position++] = static_cast<uint8>( ( copy_chunk_size - 1u ) & 0xff );
			memcpy( WorkBuffer + dest_position, Encoder.GetBufferBase() + Encoder.GetCurrentOffset() - unpack_size, copy_chunk_size );
			unpack_size -= copy_chunk_size;
			dest_position += copy_chunk_size;
			SourcePosition += copy_chunk_size;

			packSizeRes += dest_position;
			if( outStream.Write( WorkBuffer, 0, dest_position ) != dest_position )
			{
				return SevenZipResult::SevenZipErrorWrite;
			}

			dest_position = 0u;
		}

		Encoder.RestoreState();
		return SevenZipResult::SevenZipOK;
	}

	dest_position = 0u;
	uint32 copy_size = unpack_size - 1u;
	const uint32 write_pack_size = static_cast<uint32>( pack_size - 1u );
	const uint32 mode = ( SourcePosition == 0 ) ? 3u : ( NeedInitState ? ( NeedInitProp ? 2u : 1u ) : 0u );

	WorkBuffer[dest_position++] = static_cast<uint8>( (Lzma::Lzma2ControlLzma | ( mode << 5 ) | ( ( copy_size >> 16 ) & 31u ) ) );
	WorkBuffer[dest_position++] = static_cast<uint8>( ( copy_size >> 8 ) & 0xff );
	WorkBuffer[dest_position++] = static_cast<uint8>( copy_size & 0xff );
	WorkBuffer[dest_position++] = static_cast<uint8>( ( write_pack_size >> 8 ) & 0xff );
	WorkBuffer[dest_position++] = static_cast<uint8>( write_pack_size & 0xff );

	if( NeedInitProp )
	{
		WorkBuffer[dest_position++] = PropertiesByte;
	}

	NeedInitProp = false;
	NeedInitState = false;
	dest_position += static_cast<uint32>( pack_size );
	SourcePosition += unpack_size;

	if( outStream.Write( WorkBuffer, 0, dest_position ) != dest_position )
	{
		return SevenZipResult::SevenZipErrorWrite;
	}

	packSizeRes = dest_position;
	return SevenZipResult::SevenZipOK;
}

/* ---------- Lzma2 Props ---------- */

/**
 * @brief Validates and fills in default values for all LZMA2 encoder properties.
 *
 * @return SevenZipOK on success, SevenZipErrorParam if the literal-bit combination is invalid.
 */
SevenZipResult CLzma2EncoderProperties::Normalize()
{
	CLzmaEncoderProperties::Normalize();

	FastBytes = std::clamp<int16>( FastBytes, Lzma::Lzma2MinMatchLength, Lzma::MaxMatchLength );

	if( MatchCycles == 0u )
	{
		MatchCycles = 16u + ( static_cast<uint32>( FastBytes ) >> 1 );
		if( CompressionLevel < 5 )
		{
			MatchCycles >>= 1;
		}
	}

	if( LiteralContextBits + LiteralPositionBits > Lzma::MaxCombinedLiteralBits )
	{
		return SevenZipResult::SevenZipErrorParam;
	}

	return SevenZipResult::SevenZipOK;
}

/* ---------- Lzma2 ---------- */

/**
 * @brief Returns the one-byte LZMA2 dictionary-size index used in the stream header.
 *
 * @return Encoded dictionary index in the range [0, 40].
 */
uint8 Lzma2Enc::GetCodedDictionary() const
{
	uint8 dict_index;
	const uint32 dict_size = EncoderProperties->GetDictionarySize();
	for( dict_index = 0u; dict_index < 40u; dict_index++ )
	{
		if( dict_size <= ( 2u | ( dict_index & 1u ) ) << ( ( dict_index / 2 ) + 11 ) )
		{
			break;
		}
	}

	return dict_index;
}

/**
 * @brief Encodes an entire input stream into an LZMA2 output stream.
 *
 * @param outStream Destination stream to write the compressed LZMA2 data to.
 * @param inStream  Source stream supplying the uncompressed data.
 * @param finished  If true, writes the LZMA2 end-of-stream marker after the last block.
 * @return SevenZipOK on success, or an error code.
 */
SevenZipResult Lzma2Enc::EncodeStream( OutStreamInterface& outStream, InStreamInterface& inStream, bool finished )
{
	int64 unpack_total = 0u;
	int64 pack_total = 0u;
	CheckedSeqInStream limited_in_stream( inStream );

	// Initialize stream encoding
	SevenZipResult result = InitStream();
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	// Main encoding loop - process blocks
	while( true )
	{
		// Initialize block
		InitBlock();

		limited_in_stream.Reset();

		// Prepare Encoder for current block
		int64 expected_size = INT64_MAX;

		// in_stream version works only in one thread. So we use Lzma2Enc::ExpectedDataSize
		if( ExpectedDataSize != INT64_MAX && ExpectedDataSize >= unpack_total )
		{
			expected_size = ExpectedDataSize - unpack_total;
		}

		Encoder.SetDataSize( expected_size );

		result = Encoder.Prepare( &limited_in_stream, Lzma::Lzma2KeepWindowSize );
		if( result != SevenZipResult::SevenZipOK )
		{
			return result;
		}

		// Encode subblocks within current block
		while( true )
		{
			int64 pack_size = Lzma::Lzma2MaxCompressedChunkSize;

			result = EncodeSubblock( pack_size, outStream );

			if( result != SevenZipResult::SevenZipOK )
			{
				break;
			}

			pack_total += pack_size;

			if( Progress != nullptr )
			{
				result = Progress->Progress( unpack_total + SourcePosition, pack_total );
				if( result != SevenZipResult::SevenZipOK )
				{
					break;
				}
			}

			// Check if subblock encoding is complete
			if( pack_size == 0 )
			{
				break;
			}
		}

		unpack_total += SourcePosition;

		if( result != SevenZipResult::SevenZipOK )
		{
			return result;
		}

		// Verify processed data matches expected
		const int64 processed_size = limited_in_stream.Processed;
		if( SourcePosition != processed_size )
		{
			return SevenZipResult::SevenZipErrorFail;
		}

		// Check if all input data has been processed
		if( limited_in_stream.Finished )
		{
			// Write EOF marker if requested
			if( finished )
			{
				constexpr uint8 eof_byte = Lzma::Lzma2ControlEof;
				if( outStream.Write( &eof_byte, 0, 1u ) != 1u )
				{
					return SevenZipResult::SevenZipErrorWrite;
				}
			}

			return SevenZipResult::SevenZipOK;
		}
	}
}

/**
 * @brief Compresses an input stream using LZMA2 and writes the result to an output stream.
 *
 * @param outStream         Destination stream to receive the compressed output.
 * @param inStream          Source stream supplying the uncompressed data.
 * @param encoderProperties Encoder configuration parameters.
 * @param propertySummary   Output byte to receive the one-byte LZMA2 property summary.
 * @param alloc             Memory allocator for internal buffers.
 * @param progress          Optional progress callback; pass nullptr to disable.
 * @return SevenZipOK on success, or an error code.
 */
SevenZipResult Lzma2Encode( OutStreamInterface& outStream, InStreamInterface& inStream, const CLzma2EncoderProperties* encoderProperties, uint8* propertySummary, MemoryInterface* alloc, ProgressInterface* progress )
{
	Lzma2Enc enc2( encoderProperties, alloc, progress );

	/** Dict size - this needs passing to Lzma2Decode() */
	*propertySummary = enc2.GetCodedDictionary();

	enc2.PropertiesAreSet = false;

	SevenZipResult result = enc2.EncodeStream( outStream, inStream, true );
	return result;
}
