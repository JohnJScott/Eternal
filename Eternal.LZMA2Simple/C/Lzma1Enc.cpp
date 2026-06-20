/**
 * @file Lzma1Enc.cpp
 * @brief LZMA1 Encoder.
 *
 * 2021-11-18 : Igor Pavlov : Public domain
 */

#include "7zTypes.h"
#include "Lzma1Enc.h"
#include "LzFind.h"

#ifdef _DEBUG
#ifdef _WINDOWS
#define NOMINMAX
#include <Windows.h>
static int32 EventNumber = 0;
static void DebugPrint( const char* fmt, ... )
{
	char buffer[256];

	EventNumber++;

	sprintf_s( buffer, sizeof( buffer ), "Event %06d: ", EventNumber );
	OutputDebugStringA( buffer );

	va_list args;
	va_start( args, fmt );
	vsnprintf( buffer, sizeof( buffer ), fmt, args );
	va_end( args );
	OutputDebugStringA( buffer );
	OutputDebugStringA( "\n" );
}

#else
static void DebugPrint( const char*, ... ) {}
#endif
#else
static void DebugPrint( const char*, ... ) {}
#endif

CProbPrice Lzma1Enc::ProbabilityPrices[Lzma::BitModelTableSize >> LzmaEncoder::NumMoveReducingBits] =
{
	0x80, 0x67, 0x5B, 0x54, 0x4E, 0x49, 0x45, 0x42, 0x3F, 0x3D, 0x3A, 0x38, 0x36, 0x34, 0x33, 0x31, 0x30, 0x2E, 0x2D, 0x2C, 0x2B, 0x2A, 0x29, 0x28, 0x27, 0x26, 0x25, 0x24, 0x23, 0x22, 0x22, 0x21,
	0x20, 0x1F, 0x1F, 0x1E, 0x1D, 0x1D, 0x1C, 0x1C, 0x1B, 0x1A, 0x1A, 0x19, 0x19, 0x18, 0x18, 0x17, 0x17, 0x16, 0x16, 0x16, 0x15, 0x15, 0x14, 0x14, 0x13, 0x13, 0x13, 0x12, 0x12, 0x11, 0x11, 0x11,
	0x10, 0x10, 0x10, 0x0F, 0x0F, 0x0F, 0x0E, 0x0E, 0x0E, 0x0D, 0x0D, 0x0D, 0x0C, 0x0C, 0x0C, 0x0B, 0x0B, 0x0B, 0x0B, 0x0A, 0x0A, 0x0A, 0x0A, 0x09, 0x09, 0x09, 0x09, 0x08, 0x08, 0x08, 0x08, 0x07,
	0x07, 0x07, 0x07, 0x06, 0x06, 0x06, 0x06, 0x05, 0x05, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x01, 0x01
};

/**
 * @brief Returns the position-slot index for a distance value (equivalent to floor(log2(position))).
 *
 * The lookup-table variant is ~5% faster than computing the value each time but requires a 16 KB table.
 * Defined at file scope so the compiler can inline it at every call site.
 *
 * @param position Distance value to classify.
 * @return Position-slot index.
 */
static uint8 GetBlockSizeFromPosition( const uint32 position )
{
#if USE_BLOCK_SIZE_LOOKUP
	return BlockSizeLookup[position];
#else
	if( position < 5u )
	{
		return ( uint8 )( position & 0xff );
	}

	int32 msb = 1u;
	uint32 lookup = position >> 2;
	while( lookup != 0u )
	{
		lookup >>= 1;
		msb++;
	}

	const uint8 slot = ( uint8 )( ( ( msb << 1 ) + ( position >= ( 3u << ( msb - 1 ) ) ? 1u : 0u ) ) & 0xff );
	return slot;
#endif
}

class CheckedSeqOutStream
	: public OutStreamInterface
{
public:
	CheckedSeqOutStream( uint8* dest, const int64 destOffset, const int64 destSize )
	{
		WorkBuffer = dest;
		WorkBufferOffset = destOffset;
		WorkBufferSize = destOffset + destSize;
		Overflow = false;
	}

	/**
	 * @brief Copies bytes into the output buffer, capping the write if the buffer would overflow.
	 *
	 * @param data   Source data array.
	 * @param offset Byte offset within @p data to begin copying from.
	 * @param size   Number of bytes requested; may be reduced if the buffer is nearly full.
	 * @return Number of bytes actually written.
	 */
	virtual int64 Write( const uint8* data, const int64 offset, int64 size ) override
	{
		const int64 remaining = WorkBufferSize - WorkBufferOffset;
		if( remaining < size )
		{
			size = remaining;
			Overflow = true;
		}

		if( size != 0 )
		{
			memcpy( &WorkBuffer[WorkBufferOffset], &data[offset], static_cast<uint64>( size ) );
			WorkBufferOffset += size;
		}

		return size;
	}

	bool Overflow = false;
	int64 WorkBufferOffset = 0;

private:
	uint8* WorkBuffer = nullptr;
	int64 WorkBufferSize = 0;
};

/**
 * @brief Returns the effective dictionary size, selecting a default based on compression level if none was set.
 *
 * Clamps the result to the valid encoder range and optionally limits it
 * to the estimated source data size.
 *
 * @return Effective dictionary size in bytes.
 */
uint32 CLzmaEncoderProperties::GetDictionarySize() const
{
	uint32 dictionary_size = DictionarySize;
	if( DictionarySize == 0u )
	{
		switch( CompressionLevel )
		{
		case 0:
		case 1:
		case 2:
		case 3:
		case 4:
			dictionary_size = 1u << ( ( CompressionLevel * 2 ) + 16 );
			break;

		case 5:
		case 6:
			dictionary_size = 1u << ( CompressionLevel + 19 );
			break;

		case 7:
			dictionary_size = 1u << 25;
			break;

		case 8:
		case 9:
		default:
			dictionary_size = 1u << 26;
			break;
		}
	}

	if(std::cmp_greater( dictionary_size, EstimatedSourceDataSize ) )
	{
		dictionary_size = static_cast< uint32 >( std::clamp<int64>( dictionary_size, Lzma::MinDictionarySize, EstimatedSourceDataSize ) );
	}

	dictionary_size = std::clamp( dictionary_size, Lzma::MinDictionarySize, Lzma::MaxDictionarySize );
	return dictionary_size;
}

/**
 * @brief Enforces the limits common to both LZMA1 and LZMA2, filling in any unset fields with defaults.
 *
 * @return SevenZipOK (always succeeds for the base class; derived classes may return an error).
 */
SevenZipResult CLzmaEncoderProperties::Normalize()
{
	CompressionLevel = std::clamp<uint8>( CompressionLevel, 0, 9 );
	DictionarySize = GetDictionarySize();

	LiteralContextBits = std::clamp<uint8>( LiteralContextBits, 0, Lzma::MaxLiteralContextBits );
	LiteralPositionBits = std::clamp<uint8>( LiteralPositionBits, 0, Lzma::MaxLiteralPositionBits );
	PositionBits = std::clamp<uint8>( PositionBits, 0, Lzma::MaxPositionBits );

	if( FastBytes < 0 )
	{
		FastBytes = static_cast<int16>( ( CompressionLevel < 7 ) ? 32 : 64 );
	}

	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Validates and fills in default values for all LZMA1-specific encoder properties.
 *
 * Calls the base-class Normalize() first, then clamps FastBytes and sets MatchCycles
 * when not explicitly provided.
 *
 * @return SevenZipOK on success.
 */
SevenZipResult CLzma1EncoderProperties::Normalize()
{
	CLzmaEncoderProperties::Normalize();

	FastBytes = std::clamp<int16>( FastBytes, Lzma::Lzma1MinMatchLength, Lzma::MaxMatchLength );

	if( MatchCycles == 0u )
	{
		MatchCycles = 16u + ( static_cast<uint32>( FastBytes ) >> 1 );
		if( CompressionLevel < 5 )
		{
			MatchCycles >>= 1;
		}
	}

	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Initialises all probability tables in this length encoder to the default probability value.
 */
void CLengthEncoder::Init()
{
	for( CProbability& probability : Low )
	{
		probability = Lzma::InitProbabilityValue;
	}

	for( CProbability& probability : High )
	{
		probability = Lzma::InitProbabilityValue;
	}
}

/**
 * @brief Frees the output buffer allocated by AllocateMemory().
 */
void CRangeEncoder::FreeMemory()
{
	if( RangeEncoderBuffer != nullptr )
	{
		Alloc->Free( RangeEncoderBuffer, LzmaEncoder::RangeEncoderBufferSize, "CRangeEncoder::RangeEncoderBuffer" );
		RangeEncoderBuffer = nullptr;
	}
}

/**
 * @brief Allocates the internal output buffer for the range coder.
 *
 * @return Pointer to the allocated buffer, or nullptr on failure.
 */
uint8* CRangeEncoder::AllocateMemory()
{
	if( RangeEncoderBuffer == nullptr )
	{
		RangeEncoderBuffer = static_cast< uint8* >( Alloc->Alloc( LzmaEncoder::RangeEncoderBufferSize, "CRangeEncoder::RangeEncoderBuffer" ) );
	}

	return RangeEncoderBuffer;
}

/**
 * @brief Resets the range coder to its initial state ready for a new encoding pass.
 */
void CRangeEncoder::Init()
{
	Range = UINT32_MAX;
	Cache = 0u;
	Low = 0u;
	CacheSize = 0u;
	BufferOffset = 0u;
	Processed = 0u;
	Result = SevenZipResult::SevenZipOK;
}

/**
 * @brief Writes any bytes remaining in the internal buffer to the output stream.
 */
void CRangeEncoder::FlushStream()
{
	if( Result == SevenZipResult::SevenZipOK )
	{
		if( BufferOffset != OutStream->Write( RangeEncoderBuffer, 0, BufferOffset ) )
		{
			Result = SevenZipResult::SevenZipErrorWrite;
		}
	}

	Processed += BufferOffset;
	BufferOffset = 0u;
}


/**
 * @brief Flushes the output buffer to the stream if it is full.
 */
void CRangeEncoder::CheckFlush()
{
	if( BufferOffset >= LzmaEncoder::RangeEncoderBufferSize )
	{
		FlushStream();
	}
}

/**
 * @brief Propagates any carry and outputs a byte when the low register crosses the range threshold.
 */
void CRangeEncoder::ShiftLow()
{
	uint32 low32 = static_cast< uint32 >( Low & UINT32_MAX );
	uint32 high32 = static_cast< uint32 >( ( Low >> 32 ) & UINT32_MAX );
	Low = low32 << 8;
	if( low32 < 0xFF000000u || high32 != 0u )
	{
		RangeEncoderBuffer[BufferOffset++] = static_cast< uint8 >( ( Cache + high32 ) & 0xff );
		Cache = low32 >> 24;

		CheckFlush();

		if( CacheSize != 0u )
		{
			high32 += 255u;

			do
			{
				RangeEncoderBuffer[BufferOffset++] = static_cast< uint8 >( high32 & 0xff );
				CheckFlush();

			} while( --CacheSize != 0u );
		}
	}
	else
	{
		CacheSize++;
	}
}

/**
 * @brief Outputs the final five range-coder state bytes to terminate the stream.
 */
void CRangeEncoder::FlushData()
{
	for( int32 i = 0; i < 5; i++ )
	{
		ShiftLow();
	}
}

/**
 * @brief Normalises the range by left-shifting and flushing a byte when Range falls below the threshold.
 */
void CRangeEncoder::UpdateRange()
{
	if( Range < Lzma::MaxRangeValue )
	{
		Range <<= 8;
		ShiftLow();
	}
}

/**
 * @brief Updates the range after encoding a 1-bit at the given bound.
 *
 * @param bound The partition point returned by CalcNewBound().
 */
void CRangeEncoder::UpdateNewBoundUpdateRange( const uint32 bound )
{
	Range -= bound;
	Low += bound;

	UpdateRange();
}

/**
 * @brief Sets the range to the given bound and normalises.
 *
 * @param bound New range bound (upper half for a 0-bit, lower half for a 1-bit).
 */
void CRangeEncoder::SetNewBoundUpdateRange( const uint32 bound )
{
	Range = bound;
	UpdateRange();
}

/**
 * @brief Computes the partition boundary for the next bit given its probability.
 *
 * @param probability Current probability of a 0-bit (out of BitModelTableSize).
 * @return Range boundary that separates the 0 and 1 coding intervals.
 */
uint32 CRangeEncoder::CalcNewBound( const uint32 probability ) const
{
	return ( Range >> Lzma::NumBitModelTotalBits ) * probability;
}

/**
 * @brief Encodes a 0-bit and updates the probability upward.
 *
 * @param probability Probability model entry, updated in place.
 */
void CRangeEncoder::EncodeBit0( CProbability& probability )
{
	Range = CalcNewBound( probability );
	probability = static_cast< CProbability >( probability + ( (Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );

	UpdateRange();
}

/**
 * @brief Encodes one bit through a probability table entry and updates the probability.
 *
 * @param probabilityLocation Probability array.
 * @param offset              Index of the probability entry to use.
 * @param bit                 Bit value to encode (0 or 1).
 */
void CRangeEncoder::Iterate( CProbability* probabilityLocation, uint32 offset, const uint32 bit )
{
	const CProbability probability = probabilityLocation[offset];
	const uint32 new_bound = CalcNewBound( probability );

	if( bit == 0u )
	{
		// bit == 0: encode lower half of range
		Range = new_bound;
		probabilityLocation[offset] = static_cast< CProbability >( probability + ( (Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );
	}
	else
	{
		// bit == 1: encode upper half of range
		Range -= new_bound;
		Low += new_bound;
		probabilityLocation[offset] = static_cast< CProbability >( probability - ( probability >> Lzma::NumMoveBits ) );
	}

	UpdateRange();
}

/**
 * @brief Encodes a byte through an 8-level binary literal probability tree.
 *
 * @param probabilities Base probability array.
 * @param arrayOffset   Offset into probabilities for this literal context.
 * @param symbol        Byte value to encode.
 */
void CRangeEncoder::Encode( CProbability* probabilities, uint32 arrayOffset, uint32 symbol )
{
	symbol |= 0x100u;
	do
	{
		const uint32 le_offset = symbol >> 8;
		const uint32 bit = ( symbol >> 7 ) & 1u;
		symbol <<= 1;

		Iterate( probabilities, arrayOffset + le_offset, bit );

	} while( symbol < 0x10000u );
}

/**
 * @brief Encodes a literal byte using matched-literal coding, where the match byte guides tree traversal.
 *
 * @param probabilities Base probability array.
 * @param arrayOffset   Offset into probabilities for this literal context.
 * @param symbol        Byte value to encode.
 * @param matchByte     Byte at the same position in the previous match (REP0 distance).
 */
void CRangeEncoder::EncodeMatched( CProbability* probabilities, uint32 arrayOffset, uint32 symbol, uint32 matchByte )
{
	uint32 offset = 0x100u;
	symbol |= 0x100u;
	do
	{
		matchByte <<= 1;
		const uint32 le_offset = offset + ( matchByte & offset ) + ( symbol >> 8 );
		uint32 bit = ( symbol >> 7 ) & 1u;
		symbol <<= 1;
		offset &= ~( matchByte ^ symbol );

		Iterate( probabilities, arrayOffset + le_offset, bit );

	} while( symbol < 0x10000u );
}


/**
 * @brief Encodes footer bits (LSB-first) into a position-encoder binary tree.
 *
 * @param probabilities Position encoder probability array.
 * @param arrayOffset   Starting offset in the array.
 * @param numBits       Number of footer bits to encode.
 * @param symbol        Distance footer value.
 */
void CRangeEncoder::ReverseEncode( CProbability* probabilities, uint32 arrayOffset, int8 numBits, uint32 symbol )
{
	uint32 offset = 1u;
	do
	{
		const uint32 bit = symbol & 1u;
		symbol >>= 1;

		Iterate( probabilities, arrayOffset + offset, bit );

		offset <<= 1;
		offset |= bit;
	} while( --numBits != 0u );
}

/**
 * @brief Encodes a match length using the three-tier LZMA length model.
 *
 * @param le       Length encoder containing the probability tables.
 * @param symbol   Adjusted match length (already offset by Lzma1MinMatchLength).
 * @param posState Position state index.
 */
void CRangeEncoder::LengthEncode( CLengthEncoder& le, uint32 symbol, const uint32 posState )
{
	uint32 le_offset = 0u;
	CProbability probability = le.Low[le_offset];
	uint32 new_bound = CalcNewBound( probability );

	// Check if symbol is in low range (0-7)
	if( symbol >= Lzma::LengthEncoderNumLowSymbols )
	{
		// Update probability and move to mid range
		le.Low[le_offset] = static_cast<CProbability>( probability - ( probability >> Lzma::NumMoveBits ) );
		UpdateNewBoundUpdateRange( new_bound );

		le_offset += Lzma::LengthEncoderNumLowSymbols;

		probability = le.Low[le_offset];
		new_bound = CalcNewBound( probability );

		// Check if symbol is in high range (16+)
		if( symbol >= ( Lzma::LengthEncoderNumLowSymbols << 1 ) )
		{
			le.Low[le_offset] = static_cast<CProbability>( probability - ( probability >> Lzma::NumMoveBits ) );
			UpdateNewBoundUpdateRange( new_bound );

			Encode( le.High, 0, symbol - ( Lzma::LengthEncoderNumLowSymbols << 1 ) );
			return;
		}

		// Symbol is in mid range (8-15)
		symbol -= Lzma::LengthEncoderNumLowSymbols;
	}

	// Encode symbol in low/mid range
	SetNewBoundUpdateRange( new_bound );
	le.Low[le_offset] = static_cast<CProbability>( probability + ( (Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );

	// Encode 3 bits using binary tree
	le_offset += posState << ( 1 + Lzma::LengthEncoderNumLowBits );
	uint32 offset = 1u;

	for( int32 i = 0; i < 3; i++ )
	{
		const uint32 bit = ( symbol >> ( 2 - i ) ) & 1u;
		Iterate( le.Low, le_offset + offset, bit );
		offset = ( offset << 1 ) + bit;
	}
}

/**
 * @brief Encodes the 1-bit that opens the LZMA end-of-stream marker sequence.
 *
 * @param probability IsMatch probability entry, updated in place.
 */
void CRangeEncoder::IterateEndMark( CProbability& probability )
{
	const uint32 new_bound = CalcNewBound( probability );

	Range -= new_bound;
	Low += new_bound;

	UpdateRange();
	probability = static_cast< CProbability >( probability - ( probability >> Lzma::NumMoveBits ) );
}

/**
 * @brief Encodes all-ones through a binary tree model (used for the EOS position slot).
 *
 * @param probabilities Binary tree probability array.
 * @param numBits       Depth of the tree (number of bits to encode).
 */
void CRangeEncoder::EncodeTreeAllOnes( CProbability* probabilities, int8 numBits )
{
	uint32 offset = 1u;
	for( int8 i = 0; i < numBits; i++ )
	{
		IterateEndMark( probabilities[offset] );
		offset = ( offset << 1 ) + 1u;
	}
}

Lzma1Enc::Lzma1Enc( const CLzmaEncoderProperties* encoderProperties, MemoryInterface* alloc, ProgressInterface* progress )
	: Alloc( alloc )
	, Progress( progress )
	, RangeCoder( alloc )
{
	SavedState.LiteralProbabilities = nullptr;

	DictionarySize = encoderProperties->DictionarySize;
	FastBytes = static_cast< uint32 >( encoderProperties->FastBytes );

	LiteralContextBits = encoderProperties->LiteralContextBits;
	LiteralPositionBits = encoderProperties->LiteralPositionBits;
	PositionBits = encoderProperties->PositionBits;
	FastMode = ( encoderProperties->CompressionLevel < 5 );

	const bool use_binary_tree = ( encoderProperties->CompressionLevel >= 5 );
	MatchFinder = CreateMatchFinder( use_binary_tree, Alloc );

	MatchFinder->CutValue = encoderProperties->MatchCycles;
	WriteEndMark = encoderProperties->WriteEndMark;
}

Lzma1Enc::~Lzma1Enc()
{
	FreeLiterals();

	if( Optimals != nullptr )
	{
		Alloc->Free( Optimals, LzmaEncoder::NumOptimals * sizeof( COptimal ), "Lzma1Enc::Optimals" );
		Optimals = nullptr;
	}

	if( MatchFinder != nullptr )
	{
		MatchFinder->Free();
		Alloc->Free( MatchFinder, sizeof( CMatchFinder ), "Lzma1Enc::MatchFinder" );
		MatchFinder = nullptr;
	}
}

/**
 * @brief Snapshots the current encoder probability state into SavedState.
 *
 * Used by the LZMA2 encoder to allow fallback to copy-block mode when
 * LZMA compression does not achieve sufficient size reduction.
 */
void Lzma1Enc::SaveState()
{
	CSaveState* dest = &SavedState;

	dest->State = State;
	dest->LengthProbabilities = LengthProbabilities;
	dest->RepeatLengthProbabilities = RepeatLengthProbabilities;

	memcpy( dest->Repeats, Repeats, sizeof( Repeats ) );
	memcpy( dest->PositionAlignEncoder, PositionAlignEncoder, sizeof( PositionAlignEncoder ) );
	memcpy( dest->IsRep, IsRep, sizeof( IsRep ) );
	memcpy( dest->IsRepG0, IsRepG0, sizeof( IsRepG0 ) );
	memcpy( dest->IsRepG1, IsRepG1, sizeof( IsRepG1 ) );
	memcpy( dest->IsRepG2, IsRepG2, sizeof( IsRepG2 ) );

	memcpy( dest->IsMatch, IsMatch, sizeof( IsMatch ) );
	memcpy( dest->IsRep0Long, IsRep0Long, sizeof( IsRep0Long ) );
	memcpy( dest->PositionSlotEncoder, PositionSlotEncoder, sizeof( PositionSlotEncoder ) );
	memcpy( dest->PositionEncoders, PositionEncoders, sizeof( PositionEncoders ) );

	uint32 prob_size = ( 0x300u << TotalLiteralBits ) * sizeof( CProbability );
	memcpy( dest->LiteralProbabilities, LiteralProbabilities, prob_size );
}

/**
 * @brief Restores the encoder probability state from the previously saved SavedState.
 */
void Lzma1Enc::RestoreState()
{
	const CSaveState* src = &SavedState;

	State = src->State;
	LengthProbabilities = src->LengthProbabilities;
	RepeatLengthProbabilities = src->RepeatLengthProbabilities;

	memcpy( Repeats, src->Repeats, sizeof( src->Repeats ) );
	memcpy( PositionAlignEncoder, src->PositionAlignEncoder, sizeof( src->PositionAlignEncoder ) );
	memcpy( IsRep, src->IsRep, sizeof( src->IsRep ) );
	memcpy( IsRepG0, src->IsRepG0, sizeof( src->IsRepG0 ) );
	memcpy( IsRepG1, src->IsRepG1, sizeof( src->IsRepG1 ) );
	memcpy( IsRepG2, src->IsRepG2, sizeof( src->IsRepG2 ) );

	memcpy( IsMatch, src->IsMatch, sizeof( src->IsMatch ) );
	memcpy( IsRep0Long, src->IsRep0Long, sizeof( src->IsRep0Long ) );
	memcpy( PositionSlotEncoder, src->PositionSlotEncoder, sizeof( src->PositionSlotEncoder ) );
	memcpy( PositionEncoders, src->PositionEncoders, sizeof( src->PositionEncoders ) );

	uint32 prob_size = ( 0x300u << TotalLiteralBits ) * sizeof( CProbability );
	memcpy( LiteralProbabilities, src->LiteralProbabilities, prob_size );
}

/**
 * @brief Informs the match finder of the total expected input size.
 *
 * @param expectedDataSize Total number of bytes to be encoded, or INT64_MAX if unknown.
 */
void Lzma1Enc::SetDataSize( int64 expectedDataSize ) const
{
	MatchFinder->ExpectedDataSize = expectedDataSize;
}

/**
 * @brief Returns the bit-price to encode a literal byte in the given literal context.
 *
 * @param literalContext Combined position/previous-byte context index.
 * @param symbol         Byte value to price.
 * @return Price in probability-price units.
 */
uint32 Lzma1Enc::GetPrice( const uint32 literalContext, uint32 symbol ) const
{
	const uint32 base_offset = 3u * ( literalContext << LiteralContextBits );
	
	uint32 price = 0u;
	symbol |= 0x100u;
	do
	{
		uint32 bit = symbol & 1u;
		symbol >>= 1;
		uint32 xor_value = ( 0u - bit ) & Lzma::BitModelTableMask;
		price += ProbabilityPrices[( LiteralProbabilities[base_offset + symbol] ^ xor_value ) >> LzmaEncoder::NumMoveReducingBits];
	} while( symbol >= 2u );

	return price;
}


/**
 * @brief Returns the bit-price to encode a matched literal byte.
 *
 * @param literalContext Combined position/previous-byte context index.
 * @param symbol         Byte value to price.
 * @param matchByte      Byte at the same position in the previous match.
 * @return Price in probability-price units.
 */
uint32 Lzma1Enc::MatchedGetPrice( const uint32 literalContext, uint32 symbol, uint32 matchByte ) const
{
	const uint32 base_offset = 3u * ( literalContext << LiteralContextBits );
	
	uint32 price = 0u;
	uint32 offset = 0x100u;
	symbol |= 0x100u;
	do
	{
		matchByte <<= 1;
		uint32 xor_value = ( 0u - ( ( symbol >> 7 ) & 1u ) ) & Lzma::BitModelTableMask;
		price += ProbabilityPrices[( LiteralProbabilities[base_offset + offset + ( matchByte & offset ) + ( symbol >> 8 )] ^ xor_value ) >> LzmaEncoder::NumMoveReducingBits];
		symbol <<= 1;
		offset &= ~( matchByte ^ symbol );
	} while( symbol < 0x10000u );

	return price;
}

/**
 * @brief Fills eight consecutive price entries for a single position state from the length probability tree.
 *
 * @param baseArray   Probability array to read from.
 * @param baseOffset  Base offset within baseArray for this position state.
 * @param startPrice  Starting price to which tree-node prices are added.
 * @param prices      Output price array.
 * @param priceOffset Starting index in prices to write to.
 */
void Lzma1Enc::SetPrices( const CProbability* baseArray, const uint32 baseOffset, const uint32 startPrice, uint32* prices, uint32 priceOffset )
{
	for( uint32 i = 0u; i < 8u; i += 2u )
	{
		uint32 price = startPrice;
		const uint32 xor1 = ( 0u - ( i >> 2 ) ) & Lzma::BitModelTableMask;
		price += ProbabilityPrices[( baseArray[baseOffset + 1] ^ xor1 ) >> LzmaEncoder::NumMoveReducingBits];
		const uint32 xor2 = ( 0u - ( ( i >> 1 ) & 1u ) ) & Lzma::BitModelTableMask;
		price += ProbabilityPrices[( ( baseArray[baseOffset + 2 + ( i >> 2 )] ) ^ xor2 ) >> LzmaEncoder::NumMoveReducingBits];
		const uint32 probability = baseArray[baseOffset + 4 + ( i >> 1 )];
		prices[priceOffset + i] = price + ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
		prices[priceOffset + i + 1] = price + ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
	}
}

/**
 * @brief Rebuilds all position-state price tables for a length encoder.
 *
 * @param lpe          Length price encoder whose tables to rebuild.
 * @param numPosStates Number of position states to fill.
 * @param le           Source length encoder probability tables.
 */
void Lzma1Enc::UpdateTables( CLengthPriceEncoder* lpe, const uint32 numPosStates, const CLengthEncoder* le )
{
	DebugPrint( "---UpdateTables" );
	uint32 probability = le->Low[0];
	uint32 b = ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
	uint32 a = ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
	uint32 c = b + ProbabilityPrices[le->Low[Lzma::LengthEncoderNumLowSymbols] >> LzmaEncoder::NumMoveReducingBits];
	for( uint32 pos_state = 0u; pos_state < numPosStates; pos_state++ )
	{
		const uint32 base_offset = ( pos_state << ( 1 + Lzma::LengthEncoderNumLowBits ) );
		SetPrices( le->Low, base_offset, a, lpe->Prices[pos_state], 0 );
		SetPrices( le->Low, base_offset + Lzma::LengthEncoderNumLowSymbols, c, lpe->Prices[pos_state], Lzma::LengthEncoderNumLowSymbols );
	}
	
	uint32 table_size = lpe->TableSize;

	if( table_size > Lzma::LengthEncoderNumLowSymbols * 2u )
	{
		uint32 price_offset = Lzma::LengthEncoderNumLowSymbols << 1;
		table_size -= ( Lzma::LengthEncoderNumLowSymbols << 1 ) - 1u;
		table_size >>= 1;
		b += ProbabilityPrices[( le->Low[Lzma::LengthEncoderNumLowSymbols] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
		do
		{
			uint32 symbol = --table_size + ( 1u << ( Lzma::LengthEncoderNumHighBits - 1 ) );
			uint32 price = b;
			do
			{
				const uint32 bit = symbol & 1u;
				symbol >>= 1;
				const uint32 xor_value = ( 0u - bit ) & Lzma::BitModelTableMask;
				uint32 price_index = le->High[symbol] ^ xor_value;
				price += ProbabilityPrices[price_index >> LzmaEncoder::NumMoveReducingBits];
			} while( symbol >= 2 );

			probability = le->High[table_size + ( 1u << ( Lzma::LengthEncoderNumHighBits - 1 ) )];
			lpe->Prices[0][price_offset + ( table_size << 1 )] = price + ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
			lpe->Prices[0][price_offset + ( table_size << 1 ) + 1u] = price + ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
		} while( table_size != 0 );

		DebugPrint( "Update Tables: %d, %d, %d", lpe->Prices[0][0], lpe->Prices[0][1], lpe->Prices[0][2] );

		const uint32 start_offset = Lzma::LengthEncoderNumLowSymbols << 1;
		const uint32 num_elements = lpe->TableSize - start_offset;
		for( uint32 pos_state = 1u; pos_state < numPosStates; pos_state++ )
		{
			for( uint32 i = 0u; i < num_elements; i++ )
			{
				lpe->Prices[pos_state][start_offset + i] = lpe->Prices[0][start_offset + i];
			}
		}
	}
}

/**
 * @brief Queries the match finder for available matches at the current position.
 *
 * @param numPairs On exit, the number of (length, distance) pairs placed in Matches[].
 * @return The length of the longest match found (extended to NumAvail if the match finder hit MaxMatchLength).
 */
uint32 Lzma1Enc::ReadMatchDistances( uint32& numPairs )
{
	numPairs = 0u;

	AdditionalOffset++;
	NumAvail = MatchFinder->StreamPosition - MatchFinder->Position;
	MatchFinder->GetMatches( Matches, numPairs );

	if( numPairs == 0u )
	{
		return 0u;
	}
	
	DebugPrint( "ReadMatchDistances matches: (%d) %d, %d, %d, %d, %d, %d, %d, %d", numPairs, Matches[0], Matches[1], Matches[2], Matches[3], Matches[4], Matches[5], Matches[6], Matches[7] );

	const uint32 length = Matches[numPairs - 2u];
	if( length != FastBytes )
	{
		return length;
	}

	uint32 num_avail = std::min( NumAvail, static_cast<uint32>( Lzma::MaxMatchLength ) );

	const int64 base_offset = MatchFinder->BufferOffset - 1u;
	const int64 distance_offset = -1 - static_cast<int64>( Matches[numPairs - 1u] );
	int64 current_index = base_offset + length;
	const int64 limit_index = base_offset + num_avail;
	uint8* buffer_base = MatchFinder->BufferBase;

	for( ; current_index != limit_index && buffer_base[current_index] == buffer_base[current_index + static_cast<int64>( distance_offset )]; current_index++ )
	{
	}

	return static_cast< uint32 >( current_index - base_offset );
}


/**
 * @brief Returns the bit-price for encoding a pure repeat match (without the length cost).
 *
 * @param repIndex Which of the four repeat distances to use (0–3).
 * @param state    Current LZMA encoder state.
 * @param posState Current position state.
 * @return Price in probability-price units.
 */
uint32 Lzma1Enc::GetPricePureRep( const uint32 repIndex, const uint64 state, const uint64 posState ) const
{
	uint32 price;
	uint32 probability = IsRepG0[state];
	if( repIndex == 0u )
	{
		price = ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
		price += ProbabilityPrices[( IsRep0Long[state][posState] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
	}
	else
	{
		price = ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
		probability = IsRepG1[state];
		if( repIndex == 1u )
		{
			price += ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
		}
		else
		{
			price += ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
			const uint32 xor_value = ( 0u - ( repIndex - 2 ) ) & Lzma::BitModelTableMask;
			price += ProbabilityPrices[( IsRepG2[state] ^ xor_value ) >> LzmaEncoder::NumMoveReducingBits];
		}
	}

	return price;
}

/**
 * @brief Traces the optimal path from position cur back to 0 and records the sequence in Optimals[].
 *
 * @param cur Final position index in the optimal array.
 * @return Length of the first symbol in the optimal sequence.
 */
uint32 Lzma1Enc::Backward( uint32 cur )
{
	DebugPrint( "---Backward" );
	
	uint32 wr = cur + 1u;
	uint32 distance = 0u;
	uint32 length = 0u;
	OptimalEnd = wr;

	while( cur != 0u )
	{
		distance = Optimals[cur].Distance;
		length = Optimals[cur].Length;

		const uint32 extra = Optimals[cur].Extra;
		cur -= length;

		if( extra != 0u )
		{
			wr--;
			Optimals[wr].Length = length;
			cur -= extra;
			length = extra;
			if( extra == 1u )
			{
				Optimals[wr].Distance = distance;
				distance = LzmaEncoder::MarkLiteral;
			}
			else
			{
				Optimals[wr].Distance = 0u;
				length--;
				wr--;
				Optimals[wr].Distance = LzmaEncoder::MarkLiteral;
				Optimals[wr].Length = 1u;
			}
		}

		if( cur != 0u )
		{
			wr--;
			Optimals[wr].Distance = distance;
			Optimals[wr].Length = length;
		}
	}

	BackRes = distance;
	OptimalCurrent = wr;
	return length;
}

/**
 * @brief Tests whether the current optimal entry is cheap enough to terminate the forward pass.
 *
 * If the parser is near the end of the optimal buffer, finds the cheapest entry and
 * skips the match finder forward to align with it.
 *
 * @param curRef Current index; updated to the cheapest entry if a skip occurs.
 * @param last   Last populated index in the optimal array.
 * @return True if the forward pass should stop at curRef.
 */
bool Lzma1Enc::GetBestPrice( uint32& curRef, const uint32 last )
{
	// 18.06
	const uint32 cur = curRef;
	if( cur >= LzmaEncoder::NumOptimals - 64 )
	{
		DebugPrint( "---GetBestPrice" );
		uint32 price = Optimals[cur].Price;
		uint32 best = cur;
		for( uint32 price_index = cur + 1; price_index <= last; price_index++ )
	{
			const uint32 price2 = Optimals[price_index].Price;
			if( price >= price2 )
			{
				price = price2;
				best = price_index;
			}
		}

		const uint32 delta = best - cur;
		if( delta != 0 )
		{
			// MOVE_POS
			AdditionalOffset += delta;
			MatchFinder->Skip( delta );
		}

		curRef = best;
		return true;
	}

	return false;
}

/**
 * @brief Fills repeats[] with the four repeat distances that will be active after the symbol at Optimals[previous].
 *
 * @param repeats  Output array of four repeat distances.
 * @param previous Index in the optimal array of the preceding symbol.
 * @param distance Distance of the symbol at previous (may be a repeat index or a new-match distance + NumRepeats).
 */
void Lzma1Enc::GetOptimalRepeats( uint32* repeats, const uint32 previous, const uint32 distance ) const
{
	if( distance < Lzma::NumRepeats )
	{
		if( distance == 0 )
		{
			repeats[0] = Optimals[previous].Repeats[0];
			repeats[1] = Optimals[previous].Repeats[1];
			repeats[2] = Optimals[previous].Repeats[2];
			repeats[3] = Optimals[previous].Repeats[3];
		}
		else
		{
			repeats[1] = Optimals[previous].Repeats[0];
			if( distance == 1 )
			{
				repeats[0] = Optimals[previous].Repeats[1];
				repeats[2] = Optimals[previous].Repeats[2];
				repeats[3] = Optimals[previous].Repeats[3];
			}
			else
			{
				repeats[2] = Optimals[previous].Repeats[1];
				repeats[0] = Optimals[previous].Repeats[distance];
				repeats[3] = Optimals[previous].Repeats[distance ^ 1];
			}
		}
	}
	else
	{
		repeats[0] = distance - Lzma::NumRepeats + 1;
		repeats[1] = Optimals[previous].Repeats[0];
		repeats[2] = Optimals[previous].Repeats[1];
		repeats[3] = Optimals[previous].Repeats[2];
	}
}

/**
 * @brief Initialises the repeat-distance lengths array and finds the best repeat index.
 *
 * @param reps           Output: current repeat distances from Repeats[].
 * @param repLens        Output: match length for each repeat distance (0 if no match).
 * @param dataOffset     Buffer offset of the current byte.
 * @param numAvail       Number of bytes available from the current position.
 * @param repeatMaxIndex Output: index of the repeat distance with the longest match.
 */
void Lzma1Enc::InitRepeatLengths( uint32* reps, uint32* repLens, const int64 dataOffset, const uint32 numAvail, uint32& repeatMaxIndex ) const
{
	repeatMaxIndex = 0u;
	uint8* buffer_base = MatchFinder->BufferBase;

	for( uint32 i = 0u; i < Lzma::NumRepeats; i++ )
	{
		reps[i] = Repeats[i];
		const int64 compare_offset = dataOffset - reps[i];

		if( buffer_base[dataOffset] != buffer_base[compare_offset] ||
			buffer_base[dataOffset + 1u] != buffer_base[compare_offset + 1u] )
		{
			repLens[i] = 0u;
			continue;
		}

		// Find match length
		uint32 length;
		for( length = 2u; length < numAvail && buffer_base[dataOffset + length] == buffer_base[compare_offset + length]; length++ )
		{
		}

		repLens[i] = length;
		if( length > repLens[repeatMaxIndex] )
		{
			repeatMaxIndex = i;
		}

		// Optimization: break early if we hit max length
		if( length == Lzma::MaxMatchLength )
		{
			break;
		}
	}
}

/**
 * @brief Checks for very-long matches that allow skipping the full optimal pass.
 *
 * If the best repeat or the main match meets or exceeds FastBytes, skips the match
 * finder and returns its length immediately (bypassing optimal parsing).
 *
 * @param mainLength     Length of the main match from the match finder.
 * @param pairCount      Number of (length, distance) pairs in Matches[].
 * @param repeatLengths  Match lengths for each of the four repeat distances.
 * @param repeatMaxIndex Index of the longest repeat match in repeatLengths.
 * @return Length of the fast match if one was found, 0 to continue with optimal parsing.
 */
uint32 Lzma1Enc::CheckFastMatches( const uint32 mainLength, const uint32 pairCount, const uint32* repeatLengths, const uint32 repeatMaxIndex )
{
	const uint32 best_rep_len = repeatLengths[repeatMaxIndex];

	// Check if best repeat match is good enough
	if( best_rep_len >= FastBytes )
	{
		BackRes = repeatMaxIndex;
		AdditionalOffset += best_rep_len - 1u;
		MatchFinder->Skip(  best_rep_len - 1u );
		return best_rep_len;
	}

	// Check if main match is good enough
	if( mainLength >= FastBytes )
	{
		BackRes = Matches[pairCount - 1u] + Lzma::NumRepeats;
		AdditionalOffset += mainLength - 1u;
		MatchFinder->Skip( mainLength - 1u );
		return mainLength;
	}

	// Continue with optimal parsing
	return 0u;
}

/**
 * @brief Fills the first COptimal entry and seeds the forward pass with a literal price.
 *
 * @param position      Current absolute encoding position.
 * @param dataOffset    Buffer offset of the current byte.
 * @param curByte       Byte at the current position.
 * @param matchByte     Byte at distance Repeats[0] from the current position.
 * @param positionState Position state (position & PositionMask).
 */
void Lzma1Enc::InitFirstOptimal( const uint32 position, const int64 dataOffset, const uint32 curByte, const uint32 matchByte, const uint32 positionState ) const
{
	// Set initial state
	Optimals[0].State = static_cast<CState>( State );

	// Calculate literal context and get probability array
	const uint32 literal_context = ( ( position << 8 ) + MatchFinder->BufferBase[dataOffset - 1] ) & LiteralMask;

	// Calculate literal price based on encoder state
	const uint32 literal_price = ( State >= 7u ) ? MatchedGetPrice( literal_context, curByte, matchByte ) : GetPrice( literal_context, curByte );

	// Initialize first optimal with literal cost
	Optimals[1].Price = ProbabilityPrices[IsMatch[State][positionState] >> LzmaEncoder::NumMoveReducingBits] + literal_price;
	Optimals[1].Distance = LzmaEncoder::MarkLiteral;
	Optimals[1].Extra = 0;
	Optimals[1].Length = 1u;

	DebugPrint( "InitFirstOptimal: %d", Optimals[1].Price );
}

/**
 * @brief Tests a length-1 REP0 (short repeat) at the current position and updates Optimals[1] if it wins.
 *
 * @param curByte          Byte at the current position.
 * @param matchByte        Byte at distance Repeats[0].
 * @param repLens          Match lengths for each of the four repeat distances.
 * @param repeatMatchPrice Base price for entering a repeat-match sequence.
 * @param positionState    Current position state.
 * @param last             Last populated optimal index; updated to 1 if a short repeat terminates the pass.
 */
void Lzma1Enc::TryShortRepeat( const uint32 curByte, const uint32 matchByte, const uint32* repLens, const uint32 repeatMatchPrice, const uint32 positionState, uint32& last )
{
	// Only process if bytes match and REP0 distance is valid
	if( matchByte != curByte || repLens[0] != 0u )
	{
		return;
	}

	DebugPrint( "---TryShortRepeat" );

	// Calculate short repeat price (REP0 with length 1)
	const uint32 short_repeat_price = repeatMatchPrice +
		ProbabilityPrices[IsRepG0[State] >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[IsRep0Long[State][positionState] >> LzmaEncoder::NumMoveReducingBits];

	// Update optimal if this is better
	if( short_repeat_price < Optimals[1].Price )
	{
		DebugPrint( "ShortRepeatPrice: %d", short_repeat_price );

		Optimals[1].Price = short_repeat_price;
		Optimals[1].Distance = 0u;
		Optimals[1].Extra = 0;
	}

	// Early exit if we should use this result immediately
	if( last < 2u )
	{
		BackRes = Optimals[1].Distance;
		last = 1u;
	}
}

/**
 * @brief Evaluates all four repeat-distance candidates at the first optimal position and seeds Optimals[].
 *
 * @param repLens          Match length for each of the four repeat distances.
 * @param repeatMatchPrice Base price for entering a repeat-match sequence.
 * @param positionState    Current position state.
 */
void Lzma1Enc::ProcessRepeatMatches( const uint32* repLens, const uint32 repeatMatchPrice, const uint32 positionState ) const
{
	DebugPrint( "---ProcessRepeatMatches" );

	for( uint32 distance = 0u; distance < Lzma::NumRepeats; distance++ )
	{
		const uint32 repeat_length = repLens[distance];
		if( repeat_length < 2u )
		{
			continue;
		}

		// Calculate base price for this repeat distance
		const uint32 base_price = repeatMatchPrice + GetPricePureRep( distance, State, positionState );

		// Update prices for all possible lengths (from repLen down to 2)
		for( uint32 length = repeat_length; length >= 2u; length-- )
		{
			const uint32 price = base_price + RepeatLenEnc.Prices[positionState][length - Lzma::Lzma1MinMatchLength];
			if( price < Optimals[length].Price )
			{
				DebugPrint( "ProcessRepeatMatchesPrice: %d", price );
				
				Optimals[length].Price = price;
				Optimals[length].Length = length;
				Optimals[length].Distance = distance;
				Optimals[length].Extra = 0;
			}
		}
	}
}

/**
 * @brief Evaluates new-match candidates from the match finder at the first optimal position.
 *
 * @param mainLength    Length of the main match.
 * @param pairCount     Number of (length, distance) pairs in Matches[].
 * @param repLens       Match lengths for the four repeat distances (used to skip dominated candidates).
 * @param matchPrice    Base price for entering a match sequence.
 * @param positionState Current position state.
 */
void Lzma1Enc::ProcessMainMatches( const uint32 mainLength, const uint32 pairCount, const uint32* repLens, const uint32 matchPrice, const uint32 positionState ) const
{
	// Early exit if no main matches to process
	uint32 length = repLens[0] + 1u;
	if( length > mainLength )
	{
		return;
	}

	DebugPrint( "---ProcessMainMatches" );
	// Calculate base price for normal (non-repeat) matches
	const uint32 normal_match_price = matchPrice + ProbabilityPrices[IsRep[State] >> LzmaEncoder::NumMoveReducingBits];

	// Find starting position in matches array
	uint32 match_offset = 0u;
	if( length < 2u )
	{
		length = 2u;
	}
	else
	{
		// Skip matches shorter than best repeat
		while( length > Matches[match_offset] )
		{
			match_offset += 2u;
		}
	}

	// Process all match lengths
	while( true )
	{
		const uint32 distance = Matches[match_offset + 1u];

		// Calculate price for this length
		uint32 price = normal_match_price + LenEnc.Prices[positionState][length - Lzma::Lzma1MinMatchLength];

		// Calculate length-to-position state for distance pricing
		const uint32 length_to_position_state = ( length < Lzma::NumLengthToPositionStates + 1u ) ? length - 2u : Lzma::NumLengthToPositionStates - 1u;

		// Add distance-specific price
		if( distance < Lzma::NumFullDistancesSize )
		{
			price += DistancesPrices[length_to_position_state][distance & Lzma::NumFullDistancesMask];
		}
		else
		{
			constexpr uint32 distance_limit = ( 1u << ( Lzma::NumLogBits + 6 ) );
			const int8 shift = ( distance < distance_limit ) ? 6 : 5 + Lzma::NumLogBits;
			const uint32 slot = GetBlockSizeFromPosition( distance >> shift ) + static_cast<uint32>( shift << 1 );
			price += PositionSlotPrices[length_to_position_state][slot] + AlignPrices[distance & Lzma::AlignmentTableMask];
		}

		// Update optimal if this is better
		if( price < Optimals[length].Price )
		{
			DebugPrint( "ProcessMainMatches: %d", price );
			Optimals[length].Price = price;
			Optimals[length].Length = length;
			Optimals[length].Distance = distance + Lzma::NumRepeats;
			Optimals[length].Extra = 0;
		}

		// Check if we've processed all matches
		if( length == Matches[match_offset] )
		{
			match_offset += 2u;
			if( match_offset == pairCount )
			{
				break;
			}
		}

		length++;
	}
}

/**
 * @brief Tests the literal + REP0 combined sequence as a candidate at position cur+length.
 *
 * @param cur          Current position in the optimal array.
 * @param last         Last populated optimal index; extended if a new entry is written.
 * @param nextIsLiteral True if Optimals[cur+1] already holds a literal candidate.
 * @param bytesMatch   True if the next byte equals the byte at REP0 distance.
 * @param literalPrice Price of the literal at cur+1 (0 if the literal was skipped).
 * @param numAvailFull Number of bytes available at cur.
 */
void Lzma1Enc::TryLiteralRep0( const uint32 cur, uint32& last, const bool nextIsLiteral, const bool bytesMatch, const uint32 literalPrice, const uint32 numAvailFull ) const
{
	// Early exit checks - skip if any condition fails
	if( nextIsLiteral || bytesMatch || literalPrice == 0u || numAvailFull <= 2u )
	{
		return;
	}

	DebugPrint( "---TryLiteralRep0" );

	// Check if current byte matches at REP0 distance (would make this redundant)
	uint32 new_repeat = NewRepeats[0];
	if( MatchFinder->BufferBase[MatchFinder->BufferOffset - 1u] == MatchFinder->BufferBase[MatchFinder->BufferOffset - new_repeat - 1u]
		|| MatchFinder->BufferBase[MatchFinder->BufferOffset] != MatchFinder->BufferBase[MatchFinder->BufferOffset - new_repeat]
		|| MatchFinder->BufferBase[MatchFinder->BufferOffset + 1u] != MatchFinder->BufferBase[MatchFinder->BufferOffset - new_repeat + 1u] )
	{
		return;
	}

	// Find match length starting from position 3
	const uint32 limit = std::min( numAvailFull, FastBytes + 1u );
	uint32 length = 3u;
	while( length < limit 
		&& MatchFinder->BufferBase[length + MatchFinder->BufferOffset - 1u] == MatchFinder->BufferBase[length + MatchFinder->BufferOffset - new_repeat - 1u] )
	{
		length++;
	}

	// Calculate the price for LIT : REP0 sequence
	const uint32 state2 = Lzma::LiteralNextStateLut[NewState];
	const uint32 position_state2 = ( Position + 1u ) & PositionMask;

	const uint32 price = literalPrice +
		ProbabilityPrices[( IsMatch[state2][position_state2] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[( IsRep0Long[state2][position_state2] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[( IsRep[state2] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[IsRepG0[state2] >> LzmaEncoder::NumMoveReducingBits];

	// Update last position if this extends it
	const uint32 offset = cur + length;
	last = std::max( last, offset );

	// Calculate final price with length encoding (length-1 since we already counted the literal)
	const uint32 final_price = price + RepeatLenEnc.Prices[position_state2][length - 1u - Lzma::Lzma1MinMatchLength];

	// Update optimal if this is better
	if( final_price < Optimals[offset].Price )
	{
		DebugPrint( "TryLiteralRep: %d", final_price );

		Optimals[offset].Price = final_price;
		Optimals[offset].Length = length - 1u;
		Optimals[offset].Distance = 0u;
		Optimals[offset].Extra = 1;
	}
}

/**
 * @brief Tests the repeat + literal + REP0 three-symbol sequence as a candidate.
 *
 * @param cur         Current position in the optimal array.
 * @param last        Last populated optimal index; extended if a new entry is written.
 * @param repIndex    Repeat-distance index (0–3) of the leading repeat.
 * @param length      Length of the leading repeat match.
 * @param price       Base price accumulated so far (before the repeat length is added).
 * @param dataOffset  Repeat distance (buffer byte offset delta).
 * @param numAvailFull Number of bytes available at cur.
 */
void Lzma1Enc::TryRepLitRep0( const uint32 cur, uint32& last, const uint32 repIndex, const uint32 length, uint32 price, const int64 dataOffset, const uint32 numAvailFull ) const
{
	// Calculate initial limit and check viability
	uint32 start_position = length + 1u;
	uint32 limit = start_position + FastBytes;
	limit = std::min( limit, numAvailFull );

	// Check if the two bytes after the rep match (needed for LIT : REP0)
	start_position += 2;
	const int64 buffer_offset = start_position + MatchFinder->BufferOffset - 1;
	if( start_position > limit
		|| MatchFinder->BufferBase[buffer_offset - 2u] != MatchFinder->BufferBase[buffer_offset - dataOffset - 2u]
		|| MatchFinder->BufferBase[buffer_offset - 1u] != MatchFinder->BufferBase[buffer_offset - dataOffset - 1u] )
	{
		return;
	}

	// Calculate base price for REP : LIT sequence
	const uint32 literal_position = length;
	const uint32 state2 = Lzma::RepNextStateLut[NewState];
	const uint32 position_state2 = ( Position + length ) & PositionMask;
	const uint32 literal_context = ( ( ( Position + length ) << 8 ) + MatchFinder->BufferBase[literal_position + MatchFinder->BufferOffset - 2u] ) & LiteralMask;

	const int64 data_offset_literal = MatchFinder->BufferOffset + literal_position - 1u;
	price += RepeatLenEnc.Prices[PositionState][length - Lzma::Lzma1MinMatchLength] +
		ProbabilityPrices[IsMatch[state2][position_state2] >> LzmaEncoder::NumMoveReducingBits] +
		MatchedGetPrice( literal_context, MatchFinder->BufferBase[data_offset_literal], MatchFinder->BufferBase[data_offset_literal - dataOffset] );

	// Add price for the final REP0
	const uint32 final_position_state = ( position_state2 + 1u ) & PositionMask;

	price += ProbabilityPrices[( IsMatch[LzmaEncoder::EncodeStateLiteralAfterRepeat][final_position_state] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[( IsRep0Long[LzmaEncoder::EncodeStateLiteralAfterRepeat][final_position_state] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[( IsRep[LzmaEncoder::EncodeStateLiteralAfterRepeat] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[IsRepG0[LzmaEncoder::EncodeStateLiteralAfterRepeat] >> LzmaEncoder::NumMoveReducingBits];

	// Find full REP0 match length
	uint32 repeat0_length = start_position;
	const int64 data_offset_rep0 = MatchFinder->BufferOffset - 1u;
	while( repeat0_length < limit && MatchFinder->BufferBase[data_offset_rep0 + repeat0_length] == MatchFinder->BufferBase[data_offset_rep0 + repeat0_length - dataOffset] )
	{
		repeat0_length++;
	}

	// Calculate final position and update last if extended
	repeat0_length -= length;
	const uint32 offset = cur + length + repeat0_length;
	if( last < offset )
	{
		last = offset;
	}

	// Calculate final price with length encoding
	const uint32 final_price = price + RepeatLenEnc.Prices[final_position_state][repeat0_length - 1u - Lzma::Lzma1MinMatchLength];

	// Update optimal if this is better
	if( final_price < Optimals[offset].Price )
	{
		DebugPrint( "TryRepLitRep: %d", final_price );
		Optimals[offset].Price = final_price;
		Optimals[offset].Length = repeat0_length - 1u;
		Optimals[offset].Extra = static_cast<CExtra>( length + 1u );
		Optimals[offset].Distance = repIndex;
	}
}

/**
 * @brief Iterates over all four repeat distances at position cur during the forward optimal pass.
 *
 * @param cur          Current position in the optimal array.
 * @param last         Last populated optimal index; extended as new entries are written.
 * @param startLength  Updated to one past the longest REP0 match found (guides main-match start length).
 * @param numAvailFull Number of bytes available at cur.
 */
void Lzma1Enc::ProcessRepeatsInLoop( const uint32 cur, uint32& last, uint32& startLength, const uint32 numAvailFull ) const
{
	DebugPrint( "---ProcessRepeatsInLoop" );
	for( uint32 repeat_index = 0u; repeat_index < Lzma::NumRepeats; repeat_index++ )
	{
		// Check if first two bytes match
		int64 base_offset = MatchFinder->BufferOffset - 1u;
		int64 dest_offset = base_offset - NewRepeats[repeat_index];
		if( MatchFinder->BufferBase[base_offset] != MatchFinder->BufferBase[dest_offset] 
			|| MatchFinder->BufferBase[base_offset + 1u] != MatchFinder->BufferBase[dest_offset + 1u] )
		{
			continue;
		}

		// Find full match length
		uint32 match_length = 2u;
		while( match_length < NumAvailOther 
			&& MatchFinder->BufferBase[match_length + base_offset] == MatchFinder->BufferBase[match_length + base_offset - NewRepeats[repeat_index]] )
		{
			match_length++;
		}

		// Update last position if extended
		const uint32 end_position = cur + match_length;
		last = std::max( last, end_position );

		// Calculate base price for this repeat distance
		const uint32 base_price = RepeatMatchPrice + GetPricePureRep( repeat_index, NewState, PositionState );

		// Update optimal prices for all lengths (from match_length down to 2)
		for( uint32 length = match_length; length >= 2u; length-- )
		{
			const uint32 price = base_price + RepeatLenEnc.Prices[PositionState][length - Lzma::Lzma1MinMatchLength];
			if( price < Optimals[cur + length].Price )
			{
				DebugPrint( "ProcessRepeatsInLoop: %d", price );
				Optimals[cur + length].Price = price;
				Optimals[cur + length].Length = length;
				Optimals[cur + length].Distance = repeat_index;
				Optimals[cur + length].Extra = 0;
			}
		}

		// Track start length for first repeat distance (REP0)
		if( repeat_index == 0u )
		{
			startLength = match_length + 1u;
		}

		// Try REP : LIT : REP0 sequence
		TryRepLitRep0( cur, last, repeat_index, match_length, base_price, NewRepeats[repeat_index], numAvailFull );
	}
}

/**
 * @brief Tests the match + literal + REP0 three-symbol sequence as a candidate.
 *
 * @param cur           Current position in the optimal array.
 * @param last          Last populated optimal index; extended if a new entry is written.
 * @param matchLength   Length of the leading new match.
 * @param matchDistance Distance of the leading new match.
 * @param price         Price of the leading match (before length encoding is added).
 * @param numAvailFull  Number of bytes available at cur.
 */
void Lzma1Enc::TryMatchLitRep0( const uint32 cur, uint32& last, const uint32 matchLength, const uint32 matchDistance, uint32 price, const uint32 numAvailFull ) const
{
	DebugPrint( "---TryMatchLitRep0" );

	// Calculate initial limit and check viability
	const uint32 start_position = matchLength + 1u;
	const uint32 limit = start_position + FastBytes;
	if( limit > numAvailFull )
	{
		return;
	}

	// Check if the two bytes after the match are needed for LIT : REP0
	if( start_position + 2u > limit 
		|| MatchFinder->BufferBase[start_position + MatchFinder->BufferOffset - 1u] != MatchFinder->BufferBase[MatchFinder->BufferOffset - matchDistance + start_position - 2u]
		|| MatchFinder->BufferBase[start_position + MatchFinder->BufferOffset] != MatchFinder->BufferBase[MatchFinder->BufferOffset - matchDistance + start_position - 1u] )
	{
		return;
	}

	// Find full REP0 match length after the literal
	uint32 rep0_length = start_position + 2u;
	while( rep0_length < limit 
		&& MatchFinder->BufferBase[rep0_length + MatchFinder->BufferOffset - 1u] == MatchFinder->BufferBase[rep0_length + MatchFinder->BufferOffset - matchDistance - 2u] )
	{
		rep0_length++;
	}

	// Calculate total length consumed by REP0 portion
	rep0_length -= matchLength;

	// Calculate price for MATCH : LIT sequence
	const uint32 literal_position = matchLength;
	const uint32 state2 = Lzma::MatchNextStateLut[NewState];
	const uint32 position_state2 = ( Position + matchLength ) & PositionMask;
	const uint32 literal_context = ( ( ( Position + matchLength ) << 8 ) + MatchFinder->BufferBase[literal_position + MatchFinder->BufferOffset - 2u] ) & LiteralMask;

	price += ProbabilityPrices[IsMatch[state2][position_state2] >> LzmaEncoder::NumMoveReducingBits] +
		MatchedGetPrice( literal_context, MatchFinder->BufferBase[literal_position + MatchFinder->BufferOffset - 1u], MatchFinder->BufferBase[literal_position + MatchFinder->BufferOffset - matchDistance - 2u] );

	// Add price for the final REP0
	const uint32 final_position_state = ( position_state2 + 1u ) & PositionMask;

	price += ProbabilityPrices[( IsMatch[LzmaEncoder::EncodeStateLiteralAfterMatch][final_position_state] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[( IsRep0Long[LzmaEncoder::EncodeStateLiteralAfterMatch][final_position_state] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[( IsRep[LzmaEncoder::EncodeStateLiteralAfterMatch] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits] +
		ProbabilityPrices[IsRepG0[LzmaEncoder::EncodeStateLiteralAfterMatch] >> LzmaEncoder::NumMoveReducingBits];

	// Update last position if this extends it
	const uint32 end_position = cur + matchLength + rep0_length;
	if( last < end_position )
	{
		last = end_position;
	}

	// Calculate final price with REP0 length encoding
	const uint32 final_price = price + RepeatLenEnc.Prices[final_position_state][rep0_length - 1u - Lzma::Lzma1MinMatchLength];

	// Update optimal if this is better
	if( final_price < Optimals[end_position].Price )
	{
		DebugPrint( "TryMatchLitRep: %d", final_price );
		Optimals[end_position].Price = final_price;
		Optimals[end_position].Length = rep0_length - 1u;
		Optimals[end_position].Extra = static_cast<CExtra>( matchLength + 1u );
		Optimals[end_position].Distance = matchDistance + Lzma::NumRepeats;
	}
}

/**
 * @brief Iterates over new-match candidates at position cur during the forward optimal pass.
 *
 * @param cur          Current position in the optimal array.
 * @param last         Last populated optimal index; extended as new entries are written.
 * @param newLength    Longest match length returned by the match finder at cur.
 * @param pairCount    Number of (length, distance) pairs in Matches[].
 * @param startLength  Minimum match length to process (set by ProcessRepeatsInLoop).
 * @param numAvailFull Number of bytes available at cur.
 */
void Lzma1Enc::ProcessMainMatchesInLoop( const uint32 cur, uint32& last, const uint32 newLength, const uint32 pairCount, const uint32 startLength, const uint32 numAvailFull ) const
{
	// Early exit if new length doesn't extend past start length
	if( newLength < startLength )
	{
		return;
	}

	DebugPrint( "---ProcessMainMatchesInLoop" );

	// Calculate base price for normal (non-repeat) matches
	const uint32 normal_match_price = MatchPrice + ProbabilityPrices[IsRep[NewState] >> LzmaEncoder::NumMoveReducingBits];

	// Update last position if new length extends it
	last = std::max( last, cur + newLength );

	// Find starting position in matches array
	uint32 match_offset = 0u;
	while( startLength > Matches[match_offset] )
	{
		match_offset += 2u;
	}

	// Get first match distance and calculate position slot
	uint32 match_distance = Matches[match_offset + 1u];
	uint32 match_limit = ( 1u << ( Lzma::NumLogBits + 6 ) );
	int8 distance_shift = ( match_distance < match_limit ) ? 6 : 5 + Lzma::NumLogBits;
	uint32 position_slot = GetBlockSizeFromPosition( match_distance >> distance_shift ) + static_cast<uint32>( distance_shift << 1 );

	// Process all match lengths from start_len to new_len
	for( uint32 match_length = startLength; ; match_length++ )
	{
		// Calculate base price for this length
		uint32 price = normal_match_price + LenEnc.Prices[PositionState][match_length - Lzma::Lzma1MinMatchLength];

		// Calculate length-to-position state for distance pricing
		const uint32 length_to_position_state = ( match_length < Lzma::NumLengthToPositionStates + 1u ) ? match_length - 2u : Lzma::NumLengthToPositionStates - 1u;

		// Add distance-specific price
		if( match_distance < Lzma::NumFullDistancesSize )
		{
			price += DistancesPrices[length_to_position_state][match_distance & Lzma::NumFullDistancesMask];
		}
		else
		{
			price += PositionSlotPrices[length_to_position_state][position_slot] + AlignPrices[match_distance & Lzma::AlignmentTableMask];
		}

		// Update optimal if this is better
		if( price < Optimals[cur + match_length].Price )
		{
			DebugPrint( "ProcessMainMatchesInLoop: %d", price );
			Optimals[cur + match_length].Price = price;
			Optimals[cur + match_length].Length = match_length;
			Optimals[cur + match_length].Distance = match_distance + Lzma::NumRepeats;
			Optimals[cur + match_length].Extra = 0;
		}

		// Check if we've reached a match boundary
		if( match_length == Matches[match_offset] )
		{
			// Try MATCH : LIT : REP0 sequence
			TryMatchLitRep0( cur, last, match_length, match_distance, price, numAvailFull );

			// Move to next match
			match_offset += 2u;
			if( match_offset == pairCount )
			{
				break;
			}

			// Get next match distance and recalculate position slot
			match_distance = Matches[match_offset + 1u];
			match_limit = ( 1u << ( Lzma::NumLogBits + 6u ) );
			distance_shift = ( match_distance < match_limit ) ? 6u : 5u + Lzma::NumLogBits;
			position_slot = GetBlockSizeFromPosition( match_distance >> distance_shift ) + static_cast<uint32>( distance_shift << 1 );
		}
	}
}

/**
 * @brief Runs the full optimal (dynamic-programming) parser for the current position.
 *
 * @param position Current absolute encoding position.
 * @return Length of the cheapest first-symbol encoding.
 */
uint32 Lzma1Enc::GetOptimum( uint32 position )
{
	uint32 reps[Lzma::NumRepeats];
	uint32 repeat_lengths[Lzma::NumRepeats];
	uint32 pair_count;
	uint32 main_length;

	OptimalCurrent = 0u;
	OptimalEnd = 0u;

	// Get match distances
	if( AdditionalOffset == 0u )
	{
		main_length = ReadMatchDistances( pair_count );
	}
	else
	{
		main_length = LongestMatchLength;
		pair_count = NumPairs;
	}

	// Check minimum data availability
	uint32 num_avail = NumAvail;
	if( num_avail < 2u )
	{
		BackRes = LzmaEncoder::MarkLiteral;
		return 1u;
	}

	num_avail = std::min( num_avail, static_cast<uint32>( Lzma::MaxMatchLength ) );

	const int64 data_offset = MatchFinder->BufferOffset - 1;
	uint32 rep_max_index = 0u;

	// Initialize repeat lengths
	InitRepeatLengths( reps, repeat_lengths, data_offset, num_avail, rep_max_index );

	// Check for immediate fast matches
	const uint32 fast_result = CheckFastMatches( main_length, pair_count, repeat_lengths, rep_max_index );
	if( fast_result != 0u )
	{
		return fast_result;
	}

	// Get current and match bytes
	const uint8 current_byte = MatchFinder->BufferBase[data_offset];
	const uint8 match_byte = MatchFinder->BufferBase[data_offset - reps[0]];

	// Determine last position to check
	uint32 last = std::max( repeat_lengths[rep_max_index], main_length );
	if( last < 2u && current_byte != match_byte )
	{
		BackRes = LzmaEncoder::MarkLiteral;
		return 1u;
	}

	// Initialize first optimal entry
	const uint32 position_state = position & PositionMask;
	InitFirstOptimal( position, data_offset, current_byte, match_byte, position_state );

	// Calculate base prices
	const uint32 match_price = ProbabilityPrices[( IsMatch[State][position_state] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
	const uint32 repeat_match_price = match_price + ProbabilityPrices[( IsRep[State] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];

	// Try short repeat
	TryShortRepeat( current_byte, match_byte, repeat_lengths, repeat_match_price, position_state, last );
	if( last == 1u )
	{
		return 1u;
	}

	// Copy initial repeat distance
	std::ranges::copy( std::as_const(reps), Optimals[0].Repeats );

	// Process initial repeat matches
	ProcessRepeatMatches( repeat_lengths, repeat_match_price, position_state );

	// Process initial main matches
	ProcessMainMatches( main_length, pair_count, repeat_lengths, match_price, position_state );

	// Main optimal parsing loop
	uint32 cur = 0u;
	while( ++cur != last )
	{
		DebugPrint( "cur, last: %d, %d", cur, last );

		if( GetBestPrice( cur, last ) )
		{
			break;
		}

		uint32 new_length = ReadMatchDistances( pair_count );

		DebugPrint( "matches: %d, %d, %d, %d", Matches[0], Matches[1], Matches[2], Matches[3] );

		if( new_length >= FastBytes )
		{
			NumPairs = pair_count;
			LongestMatchLength = new_length;
			break;
		}

		// Process current position
		position++;

		const uint32 prev = cur - Optimals[cur].Length;
		uint32 state;

		// Determine state based on previous optimal
		if( Optimals[cur].Length == 1u )
		{
			state = Optimals[prev].State;
			state = ( Optimals[cur].Distance == 0u ) ? Lzma::ShortRepNextStateLut[state] : Lzma::LiteralNextStateLut[state];
		}
		else
		{
			const uint32 dist = Optimals[cur].Distance;
			uint32 adjusted_prev = prev;

			if( Optimals[cur].Extra != 0 )
			{
				adjusted_prev -= Optimals[cur].Extra;
				state = LzmaEncoder::EncodeStateRepeatAfterLiteral;
				if( Optimals[cur].Extra == 1u )
				{
					state = ( dist < Lzma::NumRepeats ) ? LzmaEncoder::EncodeStateRepeatAfterLiteral : LzmaEncoder::EncodeStateMatchAfterLiteral;
				}
			}
			else
			{
				state = Optimals[adjusted_prev].State;
				state = ( dist < Lzma::NumRepeats ) ? Lzma::RepNextStateLut[state] : Lzma::MatchNextStateLut[state];
			}

			GetOptimalRepeats( reps, adjusted_prev, dist );
		}

		// Update current optimal's state and reps
		Optimals[cur].State = static_cast<CState>( state );
		Optimals[cur].Repeats[0] = reps[0];
		Optimals[cur].Repeats[1] = reps[1];
		Optimals[cur].Repeats[2] = reps[2];
		Optimals[cur].Repeats[3] = reps[3];

		// Update context
		Position = position;
		PositionState = position & PositionMask;
		NewRepeats = reps;
		NewState = state;

		const uint8 current_byte2 = MatchFinder->BufferBase[MatchFinder->BufferOffset - 1u];
		const uint8 match_byte2 = MatchFinder->BufferBase[MatchFinder->BufferOffset - reps[0] - 1u];

		// Calculate prices for current position
		const uint32 current_price = Optimals[cur].Price;
		const uint32 prob = IsMatch[state][PositionState];
		MatchPrice = current_price + ProbabilityPrices[( prob ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
		uint32 literal_price = current_price + ProbabilityPrices[prob >> LzmaEncoder::NumMoveReducingBits];

		bool next_is_literal = false;

		// Try literal
		if( ( Optimals[cur + 1u].Price < LzmaEncoder::InfinityPrice && match_byte2 == current_byte2 ) || literal_price > Optimals[cur + 1u].Price )
		{
			literal_price = 0u;
		}
		else
		{
			const uint32 literal_context = ( ( position << 8 ) + MatchFinder->BufferBase[MatchFinder->BufferOffset - 2u] ) & LiteralMask;
			literal_price += ( state >= 7u ) ?
				MatchedGetPrice( literal_context, current_byte2, match_byte2 ) :
				GetPrice( literal_context, current_byte2 );

			if( literal_price < Optimals[cur + 1u].Price )
			{
				Optimals[cur + 1u].Price = literal_price;
				Optimals[cur + 1u].Length = 1u;
				Optimals[cur + 1u].Distance = LzmaEncoder::MarkLiteral;
				Optimals[cur + 1u].Extra = 0;
				next_is_literal = true;

				DebugPrint( "Write literal optimal: %d", literal_price );
			}
		}

		RepeatMatchPrice = MatchPrice + ProbabilityPrices[( IsRep[state] ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];

		// Calculate available data
		const uint32 num_avail_full = std::min( NumAvail, LzmaEncoder::NumOptimals - 1u - cur );

		// Try short repeat at current position
		if( state < 7u && match_byte2 == current_byte2 && RepeatMatchPrice < Optimals[cur + 1u].Price )
		{
			if( Optimals[cur + 1u].Length < 2u || Optimals[cur + 1u].Distance != 0u )
			{
				const uint32 short_repeat_price = RepeatMatchPrice +
												  ProbabilityPrices[IsRepG0[state] >> LzmaEncoder::NumMoveReducingBits] +
												  ProbabilityPrices[IsRep0Long[state][PositionState] >> LzmaEncoder::NumMoveReducingBits];

				if( short_repeat_price < Optimals[cur + 1u].Price )
				{
					Optimals[cur + 1u].Price = short_repeat_price;
					Optimals[cur + 1u].Length = 1u;
					Optimals[cur + 1u].Distance = 0u;
					Optimals[cur + 1u].Extra = 0;
					next_is_literal = false;

					DebugPrint( "Write non-literal optimal: %d", short_repeat_price );
				}
			}
		}

		if( num_avail_full < 2u )
		{
			continue;
		}

		NumAvailOther = std::min( num_avail_full, FastBytes );

		// Try LIT : REP_0
		TryLiteralRep0( cur, last, next_is_literal, current_byte2 == match_byte2, literal_price, num_avail_full );

		// Process all repeat distances
		uint32 start_length = 2u;
		ProcessRepeatsInLoop( cur, last, start_length, num_avail_full );

		// Process main matches
		if( new_length > NumAvailOther )
		{
			new_length = NumAvailOther;
			for( pair_count = 0u; new_length > Matches[pair_count]; pair_count += 2u )
			{
			}

			Matches[pair_count] = new_length;
			pair_count += 2u;
		}

		ProcessMainMatchesInLoop( cur, last, new_length, pair_count, start_length, num_avail_full );
	}

	// Reset infinity prices
	do
	{
		Optimals[last].Price = LzmaEncoder::InfinityPrice;
	} while( --last != 0u );

	return Backward( cur );
}

/**
 * @brief Runs the fast (greedy, single-pass) match selector.
 *
 * @return Length of the selected match, or 1 for a literal.
 */
uint32 Lzma1Enc::GetOptimumFast()
{
	// Get match distances from match finder
	uint32 main_length;
	uint32 pair_count = 0u;
	if( AdditionalOffset == 0u )
	{
		main_length = ReadMatchDistances( pair_count );
	}
	else
	{
		main_length = LongestMatchLength;
		pair_count = NumPairs;
	}

	// Check if we have enough data to process
	uint32 num_avail = NumAvail;
	if( num_avail < 2u )
	{
		BackRes = LzmaEncoder::MarkLiteral;
		return 1u;
	}

	num_avail = std::min( num_avail, static_cast<uint32>( Lzma::MaxMatchLength ) );
	int64 data_offset = MatchFinder->BufferOffset - 1u;

	DebugPrint( "GetOptimumFast: %d, %d", num_avail, pair_count );

	// Find the best repeat match
	uint32 best_rep_len = 0u;
	uint32 best_rep_index = 0u;
	for( uint32 i = 0u; i < Lzma::NumRepeats; i++ )
	{
		const int64 compare_offset = data_offset - Repeats[i];
		if( MatchFinder->BufferBase[data_offset] != MatchFinder->BufferBase[compare_offset]
			|| MatchFinder->BufferBase[data_offset + 1u] != MatchFinder->BufferBase[compare_offset + 1u] )
		{
			continue;
		}

		// Find match length for this repeat distance
		uint32 length = 2u;
		while( length < num_avail && MatchFinder->BufferBase[data_offset + length] == MatchFinder->BufferBase[compare_offset + length] )
		{
			length++;
		}

		// If match is long enough, use it immediately
		if( length >= FastBytes )
		{
			BackRes = i;
			AdditionalOffset += length - 1u;
			MatchFinder->Skip( length - 1u );
			return length;
		}

		// Track best repeat match
		if( length > best_rep_len )
		{
			best_rep_index = i;
			best_rep_len = length;
		}
	}

	// If main match is long enough, use it immediately
	if( main_length >= FastBytes )
	{
		BackRes = Matches[pair_count - 1u] + Lzma::NumRepeats;
		AdditionalOffset += main_length - 1u;
		MatchFinder->Skip( main_length - 1u );
		return main_length;
	}

	// Optimize main match length by removing worse alternatives
	uint32 main_distance = 0u;
	if( main_length >= 2u )
	{
		main_distance = Matches[pair_count - 1u];

		// Remove shorter matches at similar or longer distances
		while( pair_count > 2u )
		{
			const uint32 prev_length = Matches[pair_count - 4u];
			const uint32 prev_distance = Matches[pair_count - 3u];

			// Keep match if it's not just 1 byte longer or if distance is significantly shorter
			if( main_length != prev_length + 1u || ( main_distance >> 7 ) <= prev_distance )
			{
				break;
			}

			pair_count -= 2u;
			main_length--;
			main_distance = prev_distance;
		}

		// Reject 2-byte matches with large distances
		if( main_length == 2u && main_distance >= 0x80u )
		{
			main_length = 1u;
		}
	}

	// Choose between repeat match and main match
	if( best_rep_len >= 2u )
	{
		// Use repeat if it's almost as good as main match (with distance penalty)
		const bool use_repeat =
			( best_rep_len + 1u >= main_length ) ||
			( best_rep_len + 2u >= main_length && main_distance >= ( 1u << 9 ) ) ||
			( best_rep_len + 3u >= main_length && main_distance >= ( 1u << 15 ) );

		if( use_repeat )
		{
			BackRes = best_rep_index;
			AdditionalOffset += best_rep_len - 1u;
			MatchFinder->Skip( best_rep_len - 1u );
			return best_rep_len;
		}
	}

	// If no good match found, return literal
	if( main_length < 2u || num_avail <= 2u )
	{
		BackRes = LzmaEncoder::MarkLiteral;
		return 1u;
	}

	// Look ahead one position to see if we can find a better match
	const uint32 next_length = ReadMatchDistances( NumPairs );
	LongestMatchLength = next_length;

	if( next_length >= 2u )
	{
		const uint32 next_distance = Matches[NumPairs - 1u];

		// Use next position if it has a better match
		const bool use_next =
			( next_length >= main_length && next_distance < main_distance ) ||
			( next_length == main_length + 1u && ( next_distance >> 7 ) <= main_distance ) ||
			( next_length > main_length + 1u ) ||
			( next_length + 1u >= main_length && main_length >= 3u && ( main_distance >> 7 ) > next_distance );

		if( use_next )
		{
			BackRes = LzmaEncoder::MarkLiteral;
			return 1u;
		}
	}

	// Check if any repeat distances would match at next position
	data_offset = MatchFinder->BufferOffset - 1u;

	for( uint32 repeat : Repeats )
	{
		const int64 compare_offset = data_offset - repeat;
		if( MatchFinder->BufferBase[data_offset] != MatchFinder->BufferBase[compare_offset]
			|| MatchFinder->BufferBase[data_offset + 1u] != MatchFinder->BufferBase[compare_offset + 1u] )
		{
			continue;
		}

		// If repeat would match almost as well at next position, delay
		const uint32 limit = main_length - 1u;
		uint32 length = 2u;
		while( length < limit && MatchFinder->BufferBase[data_offset + length] == MatchFinder->BufferBase[compare_offset + length] )
		{
			length++;
		}

		if( length >= limit )
		{
			BackRes = LzmaEncoder::MarkLiteral;
			return 1u;
		}
	}

	// Use main match
	BackRes = main_distance + Lzma::NumRepeats;
	if( main_length > 2u )
	{
		AdditionalOffset += main_length - 2u;
		MatchFinder->Skip( main_length - 2u );
	}

	return main_length;
}

/**
 * @brief Writes the LZMA end-of-stream (EOS) marker sequence.
 *
 * @param posState Current position state (selects the IsMatch probability context).
 */
void Lzma1Enc::WriteEndMarker( uint32 posState )
{
	// Encode isMatch = 1 (this is a match, not a literal)
	RangeCoder.IterateEndMark( IsMatch[State][posState] );

	// Encode isRep = 0 (this is a new match, not a repeat)
	uint32 probability = IsRep[State];
	const uint32 bound = RangeCoder.CalcNewBound( probability );
	RangeCoder.SetNewBoundUpdateRange( bound );
	IsRep[State] = static_cast<CProbability>( probability + ( ( Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );

	// Update state to match state
	State = Lzma::MatchNextStateLut[State];

	// Encode match length = 0 (minimum length, which signals end marker)
	RangeCoder.LengthEncode( LengthProbabilities, 0u, posState );

	// Encode position slot = maximum (all bits set to 1)
	RangeCoder.EncodeTreeAllOnes( PositionSlotEncoder[0], Lzma::NumPositionSlotBits );

	// Encode direct bits (30 - NUM_ALIGN_BITS) with all bits set to 1
	constexpr int32 num_direct_bits = 30 - Lzma::NumAlignmentBits;
	for( uint32 i = 0u; i < num_direct_bits; i++ )
	{
		RangeCoder.Range >>= 1;
		RangeCoder.Low += RangeCoder.Range;
		RangeCoder.UpdateRange();
	}

	// Encode alignment bits (NUM_ALIGN_BITS) with all bits set to 1
	RangeCoder.EncodeTreeAllOnes( PositionAlignEncoder, Lzma::NumAlignmentBits );
}

/**
 * @brief Checks the range coder and match finder for I/O errors and sets Finished on failure.
 *
 * @return Current result code.
 */
SevenZipResult Lzma1Enc::CheckErrors()
{
	if( Result != SevenZipResult::SevenZipOK )
	{
		return Result;
	}

	if( RangeCoder.Result != SevenZipResult::SevenZipOK )
	{
		Result = SevenZipResult::SevenZipErrorWrite;
	}

	if( MatchFinder->Result != SevenZipResult::SevenZipOK )
	{
		Result = SevenZipResult::SevenZipErrorRead;
	}

	if( Result != SevenZipResult::SevenZipOK )
	{
		Finished = true;
	}

	return Result;
}

/**
 * @brief Finalises the stream: optionally writes EOS, then flushes range-coder and stream buffers.
 *
 * @param nowPos Current encoding position (used to derive the EOS position state).
 */
void Lzma1Enc::Flush( uint32 nowPos )
{
	Finished = true;
	if( WriteEndMark )
	{
		WriteEndMarker( nowPos &PositionMask );
	}

	RangeCoder.FlushData();
	RangeCoder.FlushStream();
}

/**
 * @brief Computes the bit-price for the first three alignment bits of a symbol.
 *
 * @param symbol Alignment symbol (lower 3 bits are used).
 * @param m      Binary-tree node index, updated in place as bits are consumed.
 * @return Price in probability-price units.
 */
uint32 Lzma1Enc::CalcTreeAlignPrice( uint32 symbol, uint32& m ) const
{
	uint32 price = 0u;

	for( uint32 count = 0; count < 3; count++ )
	{
		const uint32 bit = symbol & 1u;
		symbol >>= 1;
		const uint32 xor_value = ( 0u - bit ) & Lzma::BitModelTableMask;
		const uint32 price_index = PositionAlignEncoder[m] ^ xor_value;
		price += ProbabilityPrices[price_index >> LzmaEncoder::NumMoveReducingBits];

		m = ( m << 1 ) + bit;
	}

	return price;
}

/**
 * @brief Computes the bit-price for the footer bits of a position-model distance.
 *
 * @param positionIndex  Distance index within the position-model block.
 * @param positionOffset Offset into PositionEncoders[] for this block.
 * @param footerBits     Number of footer bits to price.
 * @param m              Binary-tree node index, updated in place.
 * @return Price in probability-price units.
 */
uint32 Lzma1Enc::GetPositionModelPrice( const uint32 positionIndex, const uint32 positionOffset, int8 footerBits, uint32& m ) const
{
	uint32 price = 0;

	if( footerBits != 0 )
	{
		uint32 symbol = positionIndex;
		do
		{
			const uint32 bit = symbol & 1;
			symbol >>= 1;
			const uint32 xor_value = ( 0u - bit ) & Lzma::BitModelTableMask;
			const uint32 price_index = PositionEncoders[positionOffset + m] ^ xor_value;
			price += ProbabilityPrices[price_index >> LzmaEncoder::NumMoveReducingBits];

			m = ( m << 1 ) + bit;
		} while( --footerBits != 0 );
	}

	return price;
}

/**
 * @brief Computes the price for a position-slot symbol through the binary tree.
 *
 * @param lps    Length-to-position state index.
 * @param symbol Position slot index to price.
 * @return Price in probability-price units.
 */
uint32 Lzma1Enc::CalcTreeDistancePrice( uint32 lps, uint32 symbol ) const
{
	const CProbability* probabilities = PositionSlotEncoder[lps];
	uint32 price = 0u;
	// Start from tree root
	symbol += ( 1u << ( Lzma::NumPositionSlotBits - 1 ) );

	for ( uint32 count = 0; count < Lzma::NumPositionSlotBits - 1; count++ )
	{
		const uint32 bit = symbol & 1u;
		symbol >>= 1;
		const uint32 xor_value = ( 0u - bit ) & Lzma::BitModelTableMask;
		const uint32 price_index = probabilities[symbol] ^ xor_value;
		price += ProbabilityPrices[price_index >> LzmaEncoder::NumMoveReducingBits];
	}

	return price;
}

/**
 * @brief Precomputes AlignPrices[] for all 16 alignment bit values.
 */
void Lzma1Enc::FillAlignPrices()
{
	DebugPrint( "---FillAlignPrices" );

	// Calculate prices for 4-bit alignment values (0-15)
	// Process pairs to calculate both bit=0 and bit=1 prices for final bit
	for( uint32 i = 0u; i < Lzma::AlignmentTableSize / 2u; i++ )
	{
		// Calculate price for first 3 bits using the tree price helper
		uint32 m = 1;
		const uint32 base_price = CalcTreeAlignPrice( i, m );

		// Calculate final bit prices manually (bit 3)
		const uint32 probability = PositionAlignEncoder[m];
		AlignPrices[i] = base_price + ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
		AlignPrices[i + 8u] = base_price + ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
	}
}

/**
 * @brief Precomputes DistancesPrices[] and PositionSlotPrices[] for all length-to-position states.
 */
void Lzma1Enc::FillDistancesPrices()
{
	DebugPrint( "---FillDistancesPrices" );

	uint32 temp_prices[Lzma::NumFullDistancesSize];

	MatchPriceCount = 0u;

	// Calculate prices for position model distances (4-127)
	for( uint32 position_index = Lzma::StartPositionModelIndex / 2u; position_index < Lzma::NumFullDistancesSize / 2u; position_index++ )
	{
		const uint8 pos_slot = GetBlockSizeFromPosition( position_index );
		int8 footer_bits = static_cast< int8 >( ( pos_slot >> 1 ) - 1 );
		uint32 base_pos = ( 2u | ( pos_slot & 1u ) ) << footer_bits;
		const uint32 position_offset = base_pos << 1;
		
		uint32 m = 1;
		const uint32 price = GetPositionModelPrice( position_index, position_offset, footer_bits, m );

		uint32 probability = PositionEncoders[position_offset + m];
		base_pos += position_index;
		temp_prices[base_pos] = price + ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
		temp_prices[base_pos + ( 1u << footer_bits )] = price + ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
	}

	// Calculate prices for each length-to-position state
	for( uint32 lps = 0u; lps < Lzma::NumLengthToPositionStates; lps++ )
	{
		const uint32 dist_table_size = ( DistanceTableSize + 1 ) >> 1;
		uint32* pos_slot_prices = PositionSlotPrices[lps];

		// Calculate position slot prices (6-bit encoding)
		for( uint32 slot = 0u; slot < dist_table_size; slot++ )
		{
			const uint32 price = CalcTreeDistancePrice( lps, slot );
			const uint32 probability = PositionSlotEncoder[lps][slot + ( 1u << ( Lzma::NumPositionSlotBits - 1 ) )];

			pos_slot_prices[slot << 1] = price + ProbabilityPrices[probability >> LzmaEncoder::NumMoveReducingBits];
			pos_slot_prices[( slot << 1 ) + 1u] = price + ProbabilityPrices[( probability ^ Lzma::BitModelTableMask ) >> LzmaEncoder::NumMoveReducingBits];
		}

		// Add delta for slots beyond position model range (aligned distances)
		uint32 delta = ( ( Lzma::EndPositionModelIndex / 2u - 1u ) - Lzma::NumAlignmentBits ) << LzmaEncoder::NumBitPriceShiftBits;
		for( uint32 slot = Lzma::EndPositionModelIndex / 2; slot < dist_table_size; slot++ )
		{
			pos_slot_prices[slot << 1] += delta;
			pos_slot_prices[( slot << 1 ) + 1u] += delta;
			delta += ( 1u << LzmaEncoder::NumBitPriceShiftBits );
		}

		// Combine slot prices with position model prices
		uint32* distance_price = DistancesPrices[lps];

		// Copy direct slot prices for distances 0-3
		distance_price[0] = pos_slot_prices[0];
		distance_price[1] = pos_slot_prices[1];
		distance_price[2] = pos_slot_prices[2];
		distance_price[3] = pos_slot_prices[3];

		// Combine slot and model prices for distances 4+
		for( uint32 i = 4u; i < Lzma::NumFullDistancesSize; i += 2u )
		{
			const uint32 slot_price = pos_slot_prices[GetBlockSizeFromPosition( i )];
			distance_price[i] = slot_price + temp_prices[i];
			distance_price[i + 1u] = slot_price + temp_prices[i + 1u];
		}
	}
}


/**
 * @brief Frees both the live and saved-state literal probability arrays.
 */
void Lzma1Enc::FreeLiterals()
{
	const uint32 prob_array_size = ( 0x300u << TotalLiteralBits ) * sizeof( CProbability );

	if( LiteralProbabilities != nullptr )
	{
		Alloc->Free( LiteralProbabilities, prob_array_size, "Lzma1Enc::LiteralProbabilities" );
		LiteralProbabilities = nullptr;
	}

	if( SavedState.LiteralProbabilities != nullptr )
	{
		Alloc->Free( SavedState.LiteralProbabilities, prob_array_size, "Lzma1Enc::SavedState.LiteralProbabilities" );
		SavedState.LiteralProbabilities = nullptr;
	}
}

/**
 * @brief Encodes the literal byte at the current match-finder offset.
 *
 * @param nowPos32 Current 32-bit encoding position (selects the literal probability context).
 */
void Lzma1Enc::Lit( const uint32 nowPos32 )
{
	const int64 data_offset = MatchFinder->BufferOffset - AdditionalOffset;
	const uint32 work = ( ( nowPos32 << 8 ) + MatchFinder->BufferBase[data_offset - 1u] ) & LiteralMask;
	const uint32 prob_offset = 3u * ( work << LiteralContextBits );
	uint32 state = State;
	State = Lzma::LiteralNextStateLut[State];
	if( state < 7 )
	{
		RangeCoder.Encode( LiteralProbabilities, prob_offset, MatchFinder->BufferBase[data_offset] );
	}
	else
	{
		RangeCoder.EncodeMatched( LiteralProbabilities, prob_offset, MatchFinder->BufferBase[data_offset], MatchFinder->BufferBase[data_offset - Repeats[0]] );
	}
}

/**
 * @brief Returns the optimal match length for the current encoding position.
 *
 * @param nowPos32 Current 32-bit encoding position.
 * @return Optimal match length selected by the parser.
 */
uint32 Lzma1Enc::GetLength( uint32 nowPos32 )
{
	uint32 length;
	if( FastMode )
	{
		length = GetOptimumFast();
	}
	else
	{
		const uint32 oci = OptimalCurrent;
		if( OptimalEnd == oci )
		{
			length = GetOptimum( nowPos32 );
		}
		else
		{
			length = Optimals[oci].Length;
			BackRes = Optimals[oci].Distance;
			OptimalCurrent = oci + 1;
		}
	}

	return length;
}

/**
 * @brief Handles the first-byte special case and returns the current 32-bit position.
 *
 * At position 0, if no data is available, flushes the stream.  Otherwise, reads the first
 * match distance and encodes the first byte as a literal before returning 1.  For all later
 * positions, simply returns the low 32 bits of NowPos64.
 *
 * @param re Range encoder used to emit the initial literal bits at position 0.
 * @return 1 if the first byte was encoded, or the low 32 bits of the current position otherwise.
 */
uint32 Lzma1Enc::LiteralBit( CRangeEncoder& re )
{
	if( NowPos64 == 0 )
	{
		if( MatchFinder->StreamPosition - MatchFinder->Position == 0u )
		{
			Flush( 0u );
			return 0u;
		}

		uint32 pair_count = 0;
		ReadMatchDistances( pair_count );
		re.EncodeBit0( IsMatch[LzmaEncoder::EncodeStateStart][0] );
		uint8 current_byte = MatchFinder->BufferBase[MatchFinder->BufferOffset - AdditionalOffset];
		re.Encode( LiteralProbabilities, 0, current_byte );
		AdditionalOffset--;
		
		return 1u;
	}

	return static_cast<uint32>( NowPos64 & UINT32_MAX );
}

/**
 * @brief Encodes one block of data, stopping when a pack or unpack size limit is reached.
 *
 * Initialises the match finder on the first call, then loops encoding symbols (literals,
 * repeat matches, and new matches) until the input is exhausted or a size limit fires.
 * Price tables are refreshed periodically during the loop.
 *
 * @param maxPackSize   Maximum compressed bytes to emit before yielding (0 = no limit).
 * @param maxUnpackSize Maximum uncompressed bytes to consume before yielding.
 * @return SevenZipOK, or a read/write error code if an I/O failure was detected.
 */
SevenZipResult Lzma1Enc::CodeOneBlock( const uint32 maxPackSize, const int64 maxUnpackSize )
{
	if( NeedInit )
	{
		MatchFinder->Init();
		NeedInit = false;
	}

	if( Finished )
	{
		return Result;
	}

	SevenZipResult result = CheckErrors();
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	const uint32 start_pos_32 = static_cast<uint32>( NowPos64 & UINT32_MAX );
	uint32 now_pos_32 = LiteralBit( RangeCoder );

	result = CheckErrors();
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	if( MatchFinder->StreamPosition - MatchFinder->Position != 0u )
	{
		while( true )
		{
			const uint32 length = GetLength( now_pos_32 );
			const uint32 pos_state = ( now_pos_32 & PositionMask );
			uint32 dist = BackRes;

			// Encode isMatch bit
			if( dist == LzmaEncoder::MarkLiteral )
			{
				// Literal
				RangeCoder.Iterate( IsMatch[State], pos_state, 0u );
				Lit( now_pos_32 );
			}
			else
			{
				// Match or Repeat
				RangeCoder.Iterate( IsMatch[State], pos_state, 1u );

				if( dist < Lzma::NumRepeats )
				{
					// Repeat distance encoding
					RangeCoder.Iterate( IsRep, State, 1u );

					if( dist == 0u )
					{
						// REP0
						DebugPrint( "REP0" );
						RangeCoder.Iterate( IsRepG0, State, 0u );

						// Check if short rep (length == 1)
						if( length == 1u )
						{
							RangeCoder.Iterate( IsRep0Long[State], pos_state, 0u );
							State = Lzma::ShortRepNextStateLut[State];
						}
						else
						{
							RangeCoder.Iterate( IsRep0Long[State], pos_state, 1u );
						}
					}
					else
					{
						// REP1, REP2, or REP3
						RangeCoder.Iterate( IsRepG0, State, 1u );

						if( dist == 1u )
						{
							// REP1
							DebugPrint( "REP1" );
							RangeCoder.Iterate( IsRepG1, State, 0u );
							dist = Repeats[1];
						}
						else
						{
							// REP2 or REP3
							RangeCoder.Iterate( IsRepG1, State, 1u );

							if( dist == 2u )
							{
								// REP2
								DebugPrint( "REP2" );
								RangeCoder.Iterate( IsRepG2, State, 0u );
								dist = Repeats[2];
							}
							else
							{
								// REP3
								DebugPrint( "REP3" );
								RangeCoder.Iterate( IsRepG2, State, 1u );
								dist = Repeats[3];
								Repeats[3] = Repeats[2];
							}

							Repeats[2] = Repeats[1];
						}

						Repeats[1] = Repeats[0];
						Repeats[0] = dist;
					}

					// Encode repeat length (if not short rep)
					if( length != 1u )
					{
						RangeCoder.LengthEncode( RepeatLengthProbabilities, length - Lzma::Lzma1MinMatchLength, pos_state );
						--RepeatLenEncCounter;
						State = Lzma::RepNextStateLut[State];
					}
				}
				else
				{
					// New match distance
					RangeCoder.Iterate( IsRep, State, 0u );

					State = Lzma::MatchNextStateLut[State];
					RangeCoder.LengthEncode( LengthProbabilities, length - Lzma::Lzma1MinMatchLength, pos_state );

					dist -= Lzma::NumRepeats;
					Repeats[3] = Repeats[2];
					Repeats[2] = Repeats[1];
					Repeats[1] = Repeats[0];
					Repeats[0] = dist + 1u;

					MatchPriceCount++;

					// Encode position slot
					uint32 pos_slot;
					if( dist < Lzma::NumFullDistancesSize )
					{
						pos_slot = GetBlockSizeFromPosition( dist & Lzma::NumFullDistancesMask );
					}
					else
					{
						constexpr uint32 dist_limit = 1u << ( Lzma::NumLogBits + 6 );
						const int8 shift = ( dist < dist_limit ) ? 6 : 5 + Lzma::NumLogBits;
						pos_slot = GetBlockSizeFromPosition( dist >> shift ) + static_cast<uint32>( shift << 1 );
					}

					const uint32 len_to_pos_state = ( length < Lzma::NumLengthToPositionStates + 1u ) ? length - 2u : Lzma::NumLengthToPositionStates - 1u;
					CProbability* position_slot_probabilities = PositionSlotEncoder[len_to_pos_state];

					uint32 symbol = pos_slot + ( 1u << Lzma::NumPositionSlotBits );
					do
					{
						const uint32 bit = ( symbol >> ( Lzma::NumPositionSlotBits - 1 ) ) & 1u;
						RangeCoder.Iterate( position_slot_probabilities, symbol >> Lzma::NumPositionSlotBits, bit );
						symbol <<= 1;
					} while( symbol < ( 1u << ( Lzma::NumPositionSlotBits << 1 ) ) );

					// Encode distance footer
					if( dist >= Lzma::StartPositionModelIndex )
					{
						const int8 footer_bits = static_cast<int8>( ( pos_slot >> 1 ) - 1u );

						if( dist < Lzma::NumFullDistancesSize )
						{
							const uint32 base_dist = ( 2u | ( pos_slot & 1u ) ) << footer_bits;
							RangeCoder.ReverseEncode( PositionEncoders, base_dist, footer_bits, dist );
						}
						else
						{
							// Encode high bits directly
							uint32 pos2 = ( dist | 0xFu ) << ( 32 - footer_bits );
							do
							{
								RangeCoder.Range >>= 1;
								RangeCoder.Low += RangeCoder.Range & ( 0u - ( pos2 >> 31 ) );
								pos2 <<= 1;
								RangeCoder.UpdateRange();
							} while( pos2 != 0xF0000000u );

							// Encode alignment bits using Enc_Iterate
							uint32 offset = 1u;
							for( uint32 i = 0u; i < 4u; i++ )
							{
								const uint32 bit = dist & 1u;
								dist >>= 1;
								RangeCoder.Iterate( PositionAlignEncoder, offset, bit );
								offset = ( offset << 1 ) + bit;
							}
						}
					}
				}
			}

			now_pos_32 += length;

			AdditionalOffset -= length;

			if( AdditionalOffset == 0u )
			{
				if( !FastMode )
				{
					if( MatchPriceCount >= 64u )
					{
						FillAlignPrices();
						FillDistancesPrices();
						UpdateTables( &LenEnc, 1u << PositionBits, &LengthProbabilities );
					}

					if( RepeatLenEncCounter <= 0 )
					{
						RepeatLenEncCounter = LzmaEncoder::RepeatLengthCount;
						UpdateTables( &RepeatLenEnc, 1u << PositionBits, &RepeatLengthProbabilities );
					}
				}

				if( MatchFinder->StreamPosition - MatchFinder->Position == 0u )
				{
					break;
				}

				const uint32 processed = now_pos_32 - start_pos_32;

				if( maxPackSize != 0 )
				{
					if( processed + LzmaEncoder::NumOptimals + 300u >= maxUnpackSize
						|| ( RangeCoder.Processed + RangeCoder.BufferOffset + RangeCoder.CacheSize ) + LzmaEncoder::PackReserveSize >= maxPackSize )
					{
						break;
					}
				}
				else if( processed >= ( 1u << 17 ) )
				{
					NowPos64 += now_pos_32 - start_pos_32;
					return CheckErrors();
				}
			}
		}
	}

	NowPos64 += now_pos_32 - start_pos_32;
	Flush( now_pos_32 );
	
	return CheckErrors();
}

/**
 * @brief Allocates the range-encoder buffer, literal probability tables, match-finder
 *        window, and optimal-parser array.
 *
 * @param keepWindowSize Number of bytes at the beginning of the dictionary window to preserve
 *                       across re-initialisation (passed to the match finder).
 * @return SevenZipOK on success, or SevenZipErrorMemory if any allocation fails.
 */
SevenZipResult Lzma1Enc::AllocateMemory( uint32 keepWindowSize )
{
	// Allocate range encoder buffer
	if( RangeCoder.AllocateMemory() == nullptr )
	{
		return SevenZipResult::SevenZipErrorMemory;
	}

	// Allocate or reallocate literal probability tables if needed
	const int32 lclp = LiteralContextBits + LiteralPositionBits;
	if( LiteralProbabilities == nullptr || SavedState.LiteralProbabilities == nullptr || TotalLiteralBits != lclp )
	{
		FreeLiterals();

		const uint32 prob_array_size = ( 0x300u << lclp ) * sizeof( CProbability );
		LiteralProbabilities = static_cast<CProbability*>( Alloc->Alloc( prob_array_size, "Lzma1Enc::LiteralProbabilities" ) );
		SavedState.LiteralProbabilities = static_cast<CProbability*>( Alloc->Alloc( prob_array_size, "Lzma1Enc::SavedState.LiteralProbabilities" ) );

		if( LiteralProbabilities == nullptr || SavedState.LiteralProbabilities == nullptr )
		{
			FreeLiterals();
			return SevenZipResult::SevenZipErrorMemory;
		}

		TotalLiteralBits = lclp;
	}

	// Adjust dictionary size for large dictionaries to avoid decoder issues
	uint32 dict_size = DictionarySize;
	if( dict_size == ( 2u << 30 ) || dict_size == ( 3u << 30 ) )
	{
		// Reduce by 1 to avoid 32-bit wraparound issues in decoder
		dict_size--;
	}

	// Calculate buffer size before dictionary window
	uint32 before_size = LzmaEncoder::NumOptimals;
	if( before_size + dict_size < keepWindowSize )
	{
		before_size = keepWindowSize - dict_size;
	}

	// Create match finder with calculated buffer sizes
	if( !MatchFinder->Create( dict_size, before_size, FastBytes, Lzma::MaxMatchLength + 1u ) )
	{
		return SevenZipResult::SevenZipErrorMemory;
	}

	if( Optimals == nullptr )
	{
		Optimals = static_cast< COptimal* >( Alloc->Alloc( LzmaEncoder::NumOptimals * sizeof( COptimal ), "Lzma1Enc::Optimals" ) );
		if( Optimals == nullptr )
		{
			return SevenZipResult::SevenZipErrorMemory;
		}
	}

	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Resets all encoder state to its initial values.
 *
 * Zeroes the LZ state, repeat distances, all probability models, position masks,
 * literal mask, and the optimal-parser queue.
 */
void Lzma1Enc::Init()
{
	State = 0u;
	std::ranges::fill(Repeats, 1u );

	RangeCoder.Init();

	std::ranges::fill( PositionAlignEncoder, Lzma::InitProbabilityValue );

	std::ranges::fill( IsRep, Lzma::InitProbabilityValue );
	std::ranges::fill( IsRepG0, Lzma::InitProbabilityValue );
	std::ranges::fill( IsRepG1, Lzma::InitProbabilityValue );
	std::ranges::fill( IsRepG2, Lzma::InitProbabilityValue );

	for( uint32 i = 0u; i < Lzma::NumStates; i++ )
	{
		std::ranges::fill( IsMatch[i], Lzma::InitProbabilityValue );
		std::ranges::fill( IsRep0Long[i], Lzma::InitProbabilityValue );
	}

	for( CProbability( &probability )[64] : PositionSlotEncoder )
	{
		std::ranges::fill( probability, Lzma::InitProbabilityValue );
	}

	std::ranges::fill( PositionEncoders, Lzma::InitProbabilityValue );

	const uint32 count = Lzma::LiteralSize << ( LiteralPositionBits + LiteralContextBits );
	std::ranges::fill_n( LiteralProbabilities, count, Lzma::InitProbabilityValue );

	LengthProbabilities.Init();
	RepeatLengthProbabilities.Init();

	OptimalEnd = 0u;
	OptimalCurrent = 0u;

	for( uint32 optimal_index = 0; optimal_index < LzmaEncoder::NumOptimals; optimal_index++ )
	{
		Optimals[optimal_index].Price = LzmaEncoder::InfinityPrice;
	}

	AdditionalOffset = 0u;

	PositionMask = ( 1u << PositionBits ) - 1u;
	LiteralMask = ( 0x100u << LiteralPositionBits ) - ( 0x100u >> LiteralContextBits );
}

/**
 * @brief Initialises price tables and length-encoder counters after Init().
 *
 * Fills distance and alignment price caches when not in fast mode, sets the
 * length-table sizes, and seeds both length encoders with initial prices.
 */
void Lzma1Enc::InitPrices()
{
	if( !FastMode )
	{
		FillDistancesPrices();
		FillAlignPrices();
	}

	LenEnc.TableSize = FastBytes + 1 - Lzma::Lzma1MinMatchLength;
	RepeatLenEnc.TableSize = FastBytes + 1 - Lzma::Lzma1MinMatchLength;

	RepeatLenEncCounter = LzmaEncoder::RepeatLengthCount;

	UpdateTables( &LenEnc, 1u << PositionBits, &LengthProbabilities );
	UpdateTables( &RepeatLenEnc, 1u << PositionBits, &RepeatLengthProbabilities );
}

/**
 * @brief Computes the distance table size, then allocates and fully initialises the encoder.
 *
 * @param keepWindowSize Number of dictionary bytes to preserve across re-initialisation.
 * @return SevenZipOK on success, or a memory-allocation error code.
 */
SevenZipResult Lzma1Enc::AllocAndInit( uint32 keepWindowSize )
{
	int32 shift;
	for( shift = Lzma::EndPositionModelIndex / 2; shift < LzmaEncoder::MaxDictionarySizeBits; shift++ )
	{
		if( DictionarySize <= ( 1u << shift ) )
		{
			break;
		}
	}

	DistanceTableSize = static_cast<uint32>( shift << 1 );

	Finished = false;
	Result = SevenZipResult::SevenZipOK;

	const SevenZipResult result = AllocateMemory( keepWindowSize );
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	Init();
	InitPrices();

	NowPos64 = 0u;
	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Prepares the encoder to read uncompressed data from the given stream.
 *
 * @param inStream       Input stream to encode from.
 * @param keepWindowSize Number of bytes at the start of the window to preserve across Init calls.
 * @return SevenZipOK on success, or a memory-allocation error code.
 */
SevenZipResult Lzma1Enc::Prepare( InStreamInterface* inStream, uint32 keepWindowSize )
{
	MatchFinder->InStream = inStream;
	NeedInit = true;
	return AllocAndInit( keepWindowSize );
}

/**
 * @brief Prepares the encoder to compress an in-memory buffer.
 *
 * Configures the match finder to read directly from the supplied byte array instead of
 * a stream, then calls AllocAndInit() to reset and allocate all encoder state.
 *
 * @param src            Pointer to the uncompressed input data.
 * @param srcLen         Length of the input data in bytes.
 * @param keepWindowSize Number of dictionary bytes to preserve across re-initialisation.
 * @return SevenZipOK on success, or a memory-allocation error code.
 */
SevenZipResult Lzma1Enc::MemPrepare( const uint8* src, int64 srcLen, uint32 keepWindowSize )
{
	MatchFinder->DirectInput = true;
	MatchFinder->BufferBase = const_cast< uint8* >( src );
	MatchFinder->DirectInputRemaining = srcLen;
	NeedInit = true;

	SetDataSize( srcLen );
	return AllocAndInit( keepWindowSize );
}

/**
 * @brief Returns a pointer to the base of the match-finder input buffer.
 *
 * @return Pointer to the first byte of the internal buffer held by the match finder.
 */
const uint8* Lzma1Enc::GetBufferBase() const
{
	return MatchFinder->BufferBase;
}

/**
 * @brief Returns the current read offset within the match-finder input buffer.
 *
 * @return Number of bytes consumed from the buffer so far.
 */
int64 Lzma1Enc::GetCurrentOffset() const
{
	return MatchFinder->BufferOffset - AdditionalOffset;
}

/**
 * @brief Reports the current compress progress to the optional progress callback.
 *
 * @return SevenZipOK if no callback is registered or the callback succeeded;
 *         SevenZipErrorProgress if the callback signalled cancellation.
 */
SevenZipResult Lzma1Enc::ReportProgress() const
{
	if( Progress != nullptr )
	{
		const int64 processed = RangeCoder.Processed + RangeCoder.BufferOffset + RangeCoder.CacheSize;
		const SevenZipResult result = Progress->Progress( NowPos64, processed );
		return ( result != SevenZipResult::SevenZipOK ) ? SevenZipResult::SevenZipErrorProgress : SevenZipResult::SevenZipOK;
	}

	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Encodes the encoder settings as the standard 5-byte LZMA properties block.
 *
 * @param properties Output buffer to receive the encoded properties.
 * @param size       On entry: capacity of properties (must be >= 5).
 *                   On exit:  number of bytes written (always 5 on success).
 * @return SevenZipOK on success, SevenZipErrorParam if the buffer is too small.
 */
SevenZipResult Lzma1Enc::GetCodedProperties( uint8* properties, uint64& size ) const
{
	if( size < Lzma::LzmaPropertiesSize )
	{
		return SevenZipResult::SevenZipErrorParam;
	}

	size = Lzma::LzmaPropertiesSize;

	// Encode properties byte (LiteralContextBits, LiteralPositionBits, PositionBits)
	const int32 full_properties = ( ( ( PositionBits * 5 ) + LiteralPositionBits ) * 9 ) + LiteralContextBits;
	properties[0] = static_cast<uint8>( full_properties & 0xff );

	// Encode aligned dictionary size for decoder
	uint32 encoded_dict_size;
	if( DictionarySize >= ( 1u << 21 ) )
	{
		// For large dictionaries (>= 2MB), align to 1MB boundary
		constexpr uint32 alignment_mask = ( 1u << 20 ) - 1u;
		encoded_dict_size = ( DictionarySize + alignment_mask ) & ~alignment_mask;

		// Prevent overflow from alignment
		encoded_dict_size = std::max( encoded_dict_size, DictionarySize );
	}
	else
	{
		// For small dictionaries, find next power-of-2-like size
		int8 shift = 11;
		do
		{
			encoded_dict_size = static_cast<uint32>( ( 2 + (shift & 1 ) ) << ( shift >> 1 ) );
			shift++;
		} while( encoded_dict_size < DictionarySize );
	}

	// Write dictionary size as 4-byte little-endian
	properties[1] = static_cast<uint8>( ( encoded_dict_size >> 0 ) & 0xff );
	properties[2] = static_cast<uint8>( ( encoded_dict_size >> 8 ) & 0xff );
	properties[3] = static_cast<uint8>( ( encoded_dict_size >> 16 ) & 0xff );
	properties[4] = static_cast<uint8>( ( encoded_dict_size >> 24 ) & 0xff );

	return SevenZipResult::SevenZipOK;
}

/**
 * @brief Encodes one LZMA block into a caller-supplied memory buffer.
 *
 * @param reInit          If true, re-initialises encoder state before encoding.
 * @param baseDest        Base pointer of the destination buffer.
 * @param offset          Byte offset into baseDest at which to start writing.
 * @param destLen         On entry: maximum bytes to write.
 *                        On exit:  bytes actually written.
 * @param desiredPackSize Maximum number of compressed bytes to produce.
 * @param unpackSize      On exit: number of uncompressed bytes consumed.
 * @return SevenZipOK on success, or SevenZipErrorOutputEof if the output buffer is full.
 */
SevenZipResult Lzma1Enc::CodeOneMemBlock( const bool reInit, uint8* baseDest, const int64 offset, int64& destLen, const uint32 desiredPackSize, int64& unpackSize )
{
	CheckedSeqOutStream out_stream( baseDest, offset, destLen );

	WriteEndMark = false;
	Finished = false;
	Result = SevenZipResult::SevenZipOK;

	if( reInit )
	{
		Init();
	}

	InitPrices();

	const int64 now_pos_64 = NowPos64;
	RangeCoder.Init();
	RangeCoder.OutStream = &out_stream;

	if( desiredPackSize == 0 )
	{
		return SevenZipResult::SevenZipErrorOutputEof;
	}

	const SevenZipResult result = CodeOneBlock( desiredPackSize, unpackSize );

	unpackSize = NowPos64 - now_pos_64;
	destLen = out_stream.WorkBufferOffset - offset;
	if( out_stream.Overflow )
	{
		return SevenZipResult::SevenZipErrorOutputEof;
	}

	return result;
}

/**
 * @brief Compresses an in-memory buffer, writing the output to another in-memory buffer.
 *
 * @param compressed         Output buffer to receive the compressed data.
 * @param compressedLength   On entry: capacity of the output buffer.
 *                           On exit:  number of compressed bytes written.
 * @param decompressed       Pointer to the uncompressed input data.
 * @param decompressedLength Number of uncompressed bytes to encode.
 * @return SevenZipOK on success, or an error code.
 */
SevenZipResult Lzma1Enc::MemEncode( uint8* compressed, int64& compressedLength, const uint8* decompressed, int64 decompressedLength )
{
	CheckedSeqOutStream out_stream( compressed, 0, compressedLength );

	RangeCoder.OutStream = &out_stream;

	SevenZipResult result = MemPrepare( decompressed, decompressedLength, 0 );

	if( result == SevenZipResult::SevenZipOK )
	{
		while( !Finished && result == SevenZipResult::SevenZipOK )
		{
			result = CodeOneBlock( 0u, 0 );
			if( result == SevenZipResult::SevenZipOK )
			{
				result = ReportProgress();
			}
		}

		if( result == SevenZipResult::SevenZipOK && NowPos64 != decompressedLength )
		{
			result = SevenZipResult::SevenZipErrorFail;
		}
	}

	compressedLength = out_stream.WorkBufferOffset;
	if( out_stream.Overflow )
	{
		return SevenZipResult::SevenZipErrorOutputEof;
	}

	return result;
}

/**
 * @brief Compresses a buffer using LZMA1 in a single call.
 *
 * @param compressed         Output buffer to receive the compressed data.
 * @param compressedLength   On entry: capacity of compressed.
 *                           On exit:  number of bytes written.
 * @param decompressed       Pointer to the uncompressed input data.
 * @param decompressedLength Number of uncompressed bytes to encode.
 * @param encoderProperties  Encoder configuration parameters.
 * @param propsEncoded       Output buffer to receive the 5-byte LZMA properties block.
 * @param outPropsSize       On entry: capacity of propsEncoded.
 *                           On exit:  number of bytes written (always 5 on success).
 * @param alloc              Memory allocator; pass nullptr to use the default allocator.
 * @param progress           Optional progress callback; pass nullptr to disable.
 * @return SevenZipOK on success, or an error code.
 */
SevenZipResult Lzma1Encode( uint8* compressed, int64& compressedLength, const uint8* decompressed, int64 decompressedLength, const CLzmaEncoderProperties* encoderProperties, uint8* propsEncoded, uint64& outPropsSize, MemoryInterface* alloc, ProgressInterface* progress )
{
	Lzma1Enc enc1( encoderProperties, alloc, progress );

	SevenZipResult result = enc1.GetCodedProperties( propsEncoded, outPropsSize );
	if( result == SevenZipResult::SevenZipOK )
	{
		result = enc1.MemEncode( compressed, compressedLength, decompressed, decompressedLength );
	}

	return result;
}

