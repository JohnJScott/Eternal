/* LzmaDec.c -- LZMA Decoder
2021-04-01 : Igor Pavlov : Public domain */

#include "7zTypes.h"

#include "Lzma1Lib.h"
#include "Lzma1Dec.h"

namespace LzmaDecoder
{
	static constexpr uint32 LzmaRequiredInput = 20;

	static constexpr int8 MaxNumPositionBits = 4;
	static constexpr uint32 MaxNumPositionStates = 1u << MaxNumPositionBits;
	static constexpr uint32 NumLengthProbabilities = ( MaxNumPositionStates << ( Lzma::LengthEncoderNumLowBits + 1 ) ) + Lzma::LengthEncoderNumHighSymbols;

	static constexpr uint32 NumExtendedStates = 16u;
	static constexpr uint32 NumStatePositionProbabilities = NumExtendedStates << MaxNumPositionBits;
	static constexpr uint32 NumLiteralStates = 7u;
	static constexpr uint32 MinMatchLength = 2u;

	static constexpr uint32 MaxNormalMatchLength = MinMatchLength + ( Lzma::LengthEncoderNumLowSymbols << 1 ) + Lzma::LengthEncoderNumHighSymbols;
	static constexpr uint32 NormalMatchLengthErrorData = 1u << 9;
	static constexpr uint32 NormalMatchLengthErrorFail = NormalMatchLengthErrorData - 1u;

	static constexpr uint32 IsMatchBase = Lzma::NumFullDistancesSize + NumStatePositionProbabilities + ( NumLengthProbabilities << 1 );
	static constexpr uint32 IsRepeat = IsMatchBase + NumStatePositionProbabilities + Lzma::AlignmentTableSize;

	static_assert( ( ( IsRepeat + (Lzma::NumStates * 4u ) + ( Lzma::NumLengthToPositionStates << Lzma::NumPositionSlotBits ) ) == 1984u ), "Stop_Compiling_Bad_LZMA_PROBS" );

	static constexpr uint32 MaxBound = ( ( UINT32_MAX >> Lzma::NumBitModelTotalBits ) << ( Lzma::NumBitModelTotalBits - 1 ) );
	static constexpr uint32 BadRepeatCode = ( MaxBound + ( ( ( UINT32_MAX - MaxBound ) >> Lzma::NumBitModelTotalBits ) << ( Lzma::NumBitModelTotalBits - 1 ) ) );
	static_assert( BadRepeatCode == 0xC0000000 - 0x400, "Stop_Compiling_Bad_LZMA_Check" );
}

/*
p->RemainingLength : shows status of LZMA decoder:
	< MaxNormalMatchLength  : the number of bytes to be copied with (p->rep0) Offset
	= MaxNormalMatchLength  : the LZMA stream was finished with end mark
	= MaxNormalMatchLength + 1  : need init Range coder
	= MaxNormalMatchLength + 2  : need init Range coder and State
	= NormalMatchLengthErrorFail                : Internal Code Failure
	= NormalMatchLengthErrorData + [0 ... 273]  : LZMA Data Error
*/

/* ---------- LZMA_DECODE_REAL ---------- */
/*
LzmaDec_DecodeRealInternal() can be implemented in external ASM file.
3 - is the Code compatibility version of that function for check at link time.
*/

/*
LzmaDec_DecodeRealInternal()
In:
  RangeCoder is normalized
  if (p->DictionaryPosition == limit)
  {
	LzmaDec_TryDummy() was called before to exclude LITERAL and MATCH-REP cases.
	So first Symbol can be only MATCH-NON-REP. And if that MATCH-NON-REP Symbol
	is not END_OF_PAYALOAD_MARKER, then the function doesn't write any byte to dictionary,
	the function returns SZ_OK, and the caller can use (p->RemainingLength) and (p->RepeatDistances[0]) later.
  }

Processing:
  The first LZMA Symbol will be decoded in any case.
  All main checks for limits are at the end of main loop,
  It decodes additional LZMA-symbols while (p->DataBuffer < bufLimit && DictionaryPosition < limit),
  RangeCoder is still without last normalization when (p->DataBuffer < bufLimit) is being checked.
  But if (p->DataBuffer < bufLimit), the caller provided at least (LzmaRequiredInput + 1) bytes for
  next iteration  before limit (bufLimit + LzmaRequiredInput),
  that is enough for worst case LZMA Symbol with one additional RangeCoder normalization for one Bit.
  So that function never reads bufLimit [LzmaRequiredInput] byte.

Out:
  RangeCoder is normalized
  Result:
	SZ_OK - OK
	  p->RemainingLength:
		< MaxNormalMatchLength : the number of bytes to be copied with (p->RepeatDistances[0]) Offset
		= MaxNormalMatchLength : the LZMA stream was finished with end mark

	SZ_ERROR_DATA - error, when the MATCH-Symbol refers out of dictionary
	  p->RemainingLength : undefined
	  p->RepeatDistances[*]    : undefined
*/

void Lzma1Dec::UpdateWithDecompressed( const uint8* src, const int64 offset, const int64 size )
{
	memcpy( Dictionary + DictionaryPosition, src + offset, static_cast<uint64>( size ) );
	DictionaryPosition += size;
	if( ( CheckDictionarySize == 0u ) && ( DecoderProperties.DictionarySize - ProcessedPosition <= size ) )
	{
		CheckDictionarySize = DecoderProperties.DictionarySize;
	}

	ProcessedPosition += static_cast< uint32 >( size & UINT32_MAX );
}

// Static function
LzmaDummy Lzma1Dec::UpdateRangeDummy( CParameters& parameters, const int64 bufLimitOffset )
{
	if( parameters.Range < Lzma::MaxRangeValue )
	{
		if( parameters.DataBufferOffset >= bufLimitOffset )
		{
			return LzmaDummy::DummyInputEof;
		}

		parameters.Range <<= 8;
		parameters.Code = ( parameters.Code << 8 ) | parameters.DataBufferBase[parameters.DataBufferOffset++];
	}

	return LzmaDummy::DummyOk;
}


// Static function
void Lzma1Dec::UpdateRange( CParameters& parameters )
{
	if( parameters.Range < Lzma::MaxRangeValue )
	{
		parameters.Range <<= 8;
		parameters.Code = ( parameters.Code << 8 ) | parameters.DataBufferBase[parameters.DataBufferOffset++];
	}
}

// Static function
bool Lzma1Dec::SimpleIterate( CParameters& parameters, CProbability& probability )
{
	const uint32 bound = ( parameters.Range >> Lzma::NumBitModelTotalBits ) * probability;
	if( parameters.Code < bound )
	{
		/* Bit = 0 path */
		parameters.Range = bound;
		probability = static_cast<CProbability>( probability + ( ( Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );
		return true;
	}
	else
	{
		/* Bit = 1 path */
		parameters.Range -= bound;
		parameters.Code -= bound;
		probability = static_cast<CProbability>( probability - ( probability >> Lzma::NumMoveBits ) );
		return false;
	}
}

// Static function
bool Lzma1Dec::DummySimpleIterate( CParameters& parameters, const CProbability& probability )
{
	const uint32 bound = ( parameters.Range >> Lzma::NumBitModelTotalBits ) * probability;
	if( parameters.Code < bound )
	{
		/* Bit = 0 path */
		parameters.Range = bound;
		//SetProbabilityPositive();
		return true;
	}
	else
	{
		/* Bit = 1 path */
		parameters.Range -= bound;
		parameters.Code -= bound;
		//SetProbabilityNegative();
		return false;
	}
}

	/// <summary>
	/// Identical to LzmaDec_Iterate, but does not write any data.
	/// </summary>
	/// <returns></returns>
	LzmaDummy Lzma1Dec::DummyDecodeTreeBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset )
	{
		if( UpdateRangeDummy( parameters, bufLimitOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		if( DummySimpleIterate( parameters, probability ) )
		{
			parameters.Symbol <<= 1;
		}
		else
		{
			parameters.Symbol <<= 1;
			parameters.Symbol++;
		}

		return LzmaDummy::DummyOk;
	}

	/// <summary>
	/// Identical to LzmaDec_Iterate, but does not write any data.
	/// </summary>
	/// <returns></returns>
	LzmaDummy Lzma1Dec::DummyDecodeMatchedLiteralBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset )
	{
		if( UpdateRangeDummy( parameters, bufLimitOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		if( DummySimpleIterate( parameters, probability ) )
		{
			parameters.Symbol <<= 1;
			parameters.Offset ^= parameters.Bit;
		}
		else
		{
			parameters.Symbol <<= 1;
			parameters.Symbol++;
		}

		return LzmaDummy::DummyOk;
	}

	/// <summary>
	/// Identical to LzmaDec_Iterate, but does not write any data.
	/// </summary>
	/// <returns></returns>
	LzmaDummy Lzma1Dec::DummyDecodePositionModelBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset )
	{
		if( UpdateRangeDummy( parameters, bufLimitOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		if( DummySimpleIterate( parameters, probability ) )
		{
			parameters.Symbol += parameters.Offset;
			parameters.Offset <<= 1;
		}
		else
		{
			parameters.Offset <<= 1;
			parameters.Symbol += parameters.Offset;
		}

		return LzmaDummy::DummyOk;
	}

	/// <summary>
	/// Identical to LzmaDec_Iterate, but does not write any data.
	/// </summary>
	/// <returns></returns>
	LzmaDummy Lzma1Dec::DummyConsumeBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset )
	{
		if( UpdateRangeDummy( parameters, bufLimitOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		DummySimpleIterate( parameters, probability );

		return LzmaDummy::DummyOk;
	}

	void Lzma1Dec::DecodeTreeBit( CParameters& parameters, CProbability& prob )
	{
		UpdateRange( parameters );

		if( SimpleIterate( parameters, prob ) )
		{
			parameters.Symbol <<= 1;
		}
		else
		{
			parameters.Symbol <<= 1;
			parameters.Symbol++;
		}
	}

	void Lzma1Dec::DecodeMatchedLiteralBit( CParameters& parameters, CProbability& prob )
	{
		UpdateRange( parameters );

		if( SimpleIterate( parameters, prob ) )
		{
			parameters.Symbol <<= 1;
			parameters.Offset ^= parameters.Bit;
		}
		else
		{
			parameters.Symbol <<= 1;
			parameters.Symbol++;
		}
	}

	void Lzma1Dec::DecodeReverseBit( CParameters& parameters, CProbability& prob )
	{
		UpdateRange( parameters );

		if( SimpleIterate( parameters, prob ) )
		{
			parameters.Symbol += parameters.Offset;
		}
		else
		{
			parameters.Symbol += ( parameters.Offset << 1 );
		}
	}

	void Lzma1Dec::DecodeReverseBitLast( CParameters& parameters, CProbability& prob )
	{
		UpdateRange( parameters );

		if( SimpleIterate( parameters, prob ) )
		{
			parameters.Symbol -= 8;
		}
	}

	void Lzma1Dec::DecodePositionModelBit( CParameters& parameters, CProbability& prob )
	{
		UpdateRange( parameters );

		if( SimpleIterate( parameters, prob ) )
		{
			parameters.Symbol += parameters.Offset;
			parameters.Offset <<= 1;
		}
		else
		{
			parameters.Offset <<= 1;
			parameters.Symbol += parameters.Offset;
		}
	}

void Lzma1Dec::UpdateDistance( CParameters& parameters )
{
	uint32 distance;

	UpdateRange( parameters );

	if( SimpleIterate( parameters, Probabilities[LzmaDecoder::IsRepeat + ( Lzma::NumStates * 2u ) + parameters.State] ) )
	{
		distance = RepeatDistances[1];
	}
	else
	{
		UpdateRange( parameters );

		if( SimpleIterate( parameters, Probabilities[LzmaDecoder::IsRepeat + ( Lzma::NumStates * 3u ) + parameters.State] ) )
		{
			distance = RepeatDistances[2];
		}
		else
		{
			distance = RepeatDistances[3];
			RepeatDistances[3] = RepeatDistances[2];
		}

		RepeatDistances[2] = RepeatDistances[1];
	}

	RepeatDistances[1] = RepeatDistances[0];
	RepeatDistances[0] = distance;
}

void Lzma1Dec::UpdateSymbol( CParameters& parameters )
{
	uint32 probability_offset = LzmaDecoder::IsRepeat + ( Lzma::NumStates * 4u ) + ( Lzma::NumLengthToPositionStates << Lzma::NumPositionSlotBits );
	if( ProcessedPosition != 0u || CheckDictionarySize != 0u )
	{
		const uint32 add = ( ( ProcessedPosition << 8 ) + Dictionary[( DictionaryPosition == 0 ? DictionaryBufferSize : DictionaryPosition ) - 1u] ) & LiteralMask;
		probability_offset += 3u * ( add << DecoderProperties.LiteralContextBits );
	}

	ProcessedPosition++;
	parameters.Symbol = 1u;

	const uint32 old_state = parameters.State;
	parameters.State = Lzma::LiteralNextStateLut[old_state];

	if( old_state < LzmaDecoder::NumLiteralStates )
	{
		for( uint32 count = 0; count < 8; count++ )
		{
			DecodeTreeBit( parameters, Probabilities[probability_offset + parameters.Symbol] );
		}
	}
	else
	{
		uint32 match_byte = Dictionary[DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u )];
		parameters.Offset = 0x100u;

		for( uint32 count = 0; count < 8; count++ )
		{
			match_byte <<= 1;
			parameters.Bit = parameters.Offset;
			parameters.Offset &= match_byte;

			DecodeMatchedLiteralBit( parameters, Probabilities[probability_offset + parameters.Symbol + parameters.Offset + parameters.Bit] );
		}
	}

	Dictionary[DictionaryPosition++] = static_cast<uint8>( parameters.Symbol & 0xff );
}

uint32 Lzma1Dec::UpdateLength( CParameters& parameters, uint32 probabilityOffset, const uint32 positionState ) const
{
	uint32 length;
	if( SimpleIterate( parameters, Probabilities[probabilityOffset + Lzma::LengthEncoderNumLowSymbols] ) )
	{
		uint32 length_offset = probabilityOffset + positionState + ( 1u << Lzma::LengthEncoderNumLowBits );
		parameters.Symbol = 1u;

		DecodeTreeBit( parameters, Probabilities[length_offset + parameters.Symbol] );
		DecodeTreeBit( parameters, Probabilities[length_offset + parameters.Symbol] );
		DecodeTreeBit( parameters, Probabilities[length_offset + parameters.Symbol] );

		length = parameters.Symbol;
	}
	else
	{
		uint32 length_offset = probabilityOffset + ( LzmaDecoder::MaxNumPositionStates << ( Lzma::LengthEncoderNumLowBits + 1 ) );
		parameters.Symbol = 1u;

		do
		{
			DecodeTreeBit( parameters, Probabilities[length_offset + parameters.Symbol] );
		} while( parameters.Symbol < Lzma::LengthEncoderNumHighSymbols );

		length = parameters.Symbol - Lzma::LengthEncoderNumHighSymbols;
		length += Lzma::LengthEncoderNumLowSymbols << 1;
	}

	return length;
}

bool Lzma1Dec::UpdateRepeatLength( CParameters& parameters, uint32& length )
{
	// Decode 6-Bit position slot
	const uint32 len_to_pos_state = ( length < Lzma::NumLengthToPositionStates ) ? length : Lzma::NumLengthToPositionStates - 1u;
	uint32 prob_offset = LzmaDecoder::IsRepeat + ( Lzma::NumStates * 4u ) + ( len_to_pos_state << Lzma::NumPositionSlotBits );

	parameters.Symbol = 1u;

	for( uint32 count = 0; count < 6; count++ )
	{
		DecodeTreeBit( parameters, Probabilities[prob_offset + parameters.Symbol] );
	}

	uint32 distance = parameters.Symbol - 64u;

	// Fast path: small distances (0-3) need no footer
	if( distance < Lzma::StartPositionModelIndex )
	{
		distance++;
	}
	else
	{
		// Decode distance footer
		const uint32 pos_slot = distance;
		int8 num_direct_bits = static_cast<int8>( ( distance >> 1 ) - 1 );
		distance = 2u | ( distance & 1u );

		if( pos_slot < Lzma::EndPositionModelIndex )
		{
			// Position model (4-13)
			distance <<= num_direct_bits;

			parameters.Symbol = distance + 1u;
			parameters.Offset = 1u;

			do
			{
				DecodePositionModelBit( parameters, Probabilities[parameters.Symbol] );
			} while( --num_direct_bits != 0 );

			distance = parameters.Symbol - parameters.Offset;
		}
		else
		{
			// Large distances (14+): direct bits + alignment
			num_direct_bits -= Lzma::NumAlignmentBits;

			// Decode direct bits without probability model
			do
			{
				UpdateRange( parameters );

				parameters.Range >>= 1;
				const uint32 t = ( parameters.Code - parameters.Range ) >> 31;
				parameters.Code -= parameters.Range & ( t - 1u );
				distance = ( distance << 1 ) + ( 1u - t );
			} while( --num_direct_bits != 0 );

			// Decode 4-Bit alignment
			uint32 prob_align_offset = Lzma::NumFullDistancesSize + ( LzmaDecoder::NumStatePositionProbabilities << 1 ) + ( LzmaDecoder::NumLengthProbabilities << 1 );
			distance <<= Lzma::NumAlignmentBits;

			parameters.Symbol = 1u;

			// Unrolled alignment decoding
			parameters.Offset = 1u;
			DecodeReverseBit( parameters, Probabilities[prob_align_offset + parameters.Symbol] );

			parameters.Offset = 2u;
			DecodeReverseBit( parameters, Probabilities[prob_align_offset + parameters.Symbol] );

			parameters.Offset = 4u;
			DecodeReverseBit( parameters, Probabilities[prob_align_offset + parameters.Symbol] );

			DecodeReverseBitLast( parameters, Probabilities[prob_align_offset + parameters.Symbol] );

			distance |= parameters.Symbol;

			// Check for end marker
			if( distance == UINT32_MAX )
			{
				length = LzmaDecoder::MaxNormalMatchLength;
				parameters.State -= Lzma::NumStates;
				return true;
			}
		}

		distance++;
	}

	// Update repeat distance history
	RepeatDistances[3] = RepeatDistances[2];
	RepeatDistances[2] = RepeatDistances[1];
	RepeatDistances[1] = RepeatDistances[0];
	RepeatDistances[0] = distance;

	// Update State
	parameters.State = Lzma::MatchNextStateLut[parameters.State - Lzma::NumStates];

	// Validate distance
	const uint32 max_distance = ( CheckDictionarySize == 0u ) ? ProcessedPosition : CheckDictionarySize;
	if( distance > max_distance )
	{
		length += LzmaDecoder::NormalMatchLengthErrorData + LzmaDecoder::MinMatchLength;
		return true;
	}

	return false;
}

uint32 Lzma1Dec::FinishBlock( const uint32 length, const int64 remaining )
{
	// Clamp length to available space
	const uint32 copy_length = ( remaining > UINT32_MAX ) ? length : std::min( static_cast<uint32>( remaining ), length );

	// Calculate source position with wraparound
	const int64 src_pos = DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u );

	// Update processed position
	ProcessedPosition += copy_length;

	// Check if we can use fast path (no wraparound)
	if( copy_length <= DictionaryBufferSize - src_pos )
	{
		// Fast path: single contiguous copy
		const int64 dest_start = DictionaryPosition;
		DictionaryPosition += copy_length;

		if( RepeatDistances[0] >= copy_length )
		{
			// No overlap — bulk copy
			memcpy( Dictionary + dest_start, Dictionary + src_pos, copy_length );
		}
		else if( RepeatDistances[0] == 1u )
		{
			// Run of one byte
			memset( Dictionary + dest_start, Dictionary[src_pos], copy_length );
		}
		else
		{
			// Short-period pattern: seed first period, then double-copy
			uint32 written = RepeatDistances[0];
			memcpy( Dictionary + dest_start, Dictionary + src_pos, written );
			while( written < copy_length )
			{
				uint32 chunk = std::min( written, copy_length - written );
				memcpy( Dictionary + dest_start + written, Dictionary + dest_start, chunk );
				written += chunk;
			}
		}
	}
	else
	{
		// Slow path: wraparound copy
		int64 pos = src_pos;
		uint32 remaining_copy = copy_length;

		do
		{
			Dictionary[DictionaryPosition++] = Dictionary[pos];

			if( ++pos == DictionaryBufferSize )
			{
				pos = 0u;
			}
		} while( --remaining_copy != 0u );
	}

	return length - copy_length;
}

// Decode short repeat (REP0 with length 1)
bool Lzma1Dec::DecodeShortRepeat( CParameters& parameters, const uint32 positionState )
{
	const uint32 probability_offset = Lzma::NumFullDistancesSize + positionState + parameters.State;
	const uint32 probability = Probabilities[probability_offset];
	const uint32 bound = ( parameters.Range >> Lzma::NumBitModelTotalBits ) * probability;

	if( parameters.Code < bound )
	{
		// Short repeat: copy one byte from REP0 distance
		parameters.Range = bound;
		Probabilities[probability_offset] = static_cast< CProbability >( probability + ( ( Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );

		const int64 src_pos = DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u );
		Dictionary[DictionaryPosition] = Dictionary[src_pos];
		DictionaryPosition++;
		ProcessedPosition++;
		parameters.State = Lzma::ShortRepNextStateLut[parameters.State];
		return true;
	}

	parameters.Range -= bound;
	parameters.Code -= bound;
	Probabilities[probability_offset] = static_cast<CProbability>( probability - ( probability >> Lzma::NumMoveBits ) );
	return false;
}

// Decode match/repeat type and set up for length decoding
uint32 Lzma1Dec::DecodeMatchType( CParameters& parameters, const uint32 positionState )
{
	if( SimpleIterate( parameters, Probabilities[LzmaDecoder::IsRepeat + parameters.State] ) )
	{
		// New match
		parameters.State += Lzma::NumStates;
		return Lzma::NumFullDistancesSize + LzmaDecoder::NumStatePositionProbabilities + LzmaDecoder::NumLengthProbabilities;
	}

	// Repeat match
	UpdateRange( parameters );

	if( SimpleIterate( parameters, Probabilities[LzmaDecoder::IsRepeat + Lzma::NumStates + parameters.State] ) )
	{
		// Try short repeat (REP0 with length 1)
		UpdateRange( parameters );

		if( DecodeShortRepeat( parameters, positionState ) )
		{
			// Signal: short repeat handled, continue main loop
			return 0u; 
		}
	}
	else
	{
		// REP1/REP2/REP3
		UpdateDistance( parameters );
	}

	parameters.State = Lzma::RepNextStateLut[parameters.State];
	return Lzma::NumFullDistancesSize + LzmaDecoder::NumStatePositionProbabilities;
}

// Decode low-Range length (3 bits, values 0-7)
uint32 Lzma1Dec::DecodeLowLength( CParameters& parameters, const uint32 positionState ) const
{
	parameters.Symbol = 1u;

	DecodeTreeBit( parameters, Probabilities[positionState + parameters.Symbol] );
	DecodeTreeBit( parameters, Probabilities[positionState + parameters.Symbol] );
	DecodeTreeBit( parameters, Probabilities[positionState + parameters.Symbol] );

	return parameters.Symbol - 8u;
}

// Simplified main decode function
SevenZipResult Lzma1Dec::DecodeRealInternal( int64 limit, const int64 bufLimitOffset )
{
	uint32 length = 0u;

	do
	{
		UpdateRange( Parameters );

		// Calculate position State
		const uint32 position_state = ( ProcessedPosition & PositionMask ) << 4u;

		// Decode isMatch Bit
		const uint32 probability_offset = LzmaDecoder::IsMatchBase + position_state + Parameters.State;
		const uint32 probability = Probabilities[probability_offset];
		const uint32 bound = ( Parameters.Range >> Lzma::NumBitModelTotalBits ) * probability;

		if( Parameters.Code < bound )
		{
			// LITERAL path
			Parameters.Range = bound;
			Probabilities[probability_offset] = static_cast< CProbability >( probability + ( (Lzma::BitModelTableSize - probability ) >> Lzma::NumMoveBits ) );

			// Decode literal
			UpdateSymbol( Parameters );
			continue;
		}

		// MATCH or REPEAT path
		Parameters.Range -= bound;
		Parameters.Code -= bound;
		UpdateRange( Parameters );
		Probabilities[probability_offset] = static_cast< CProbability >( probability - ( probability >> Lzma::NumMoveBits ) );

		// Decode match/repeat type and get length probability table
		uint32 prob_offset = DecodeMatchType( Parameters, position_state );

		// Check if short repeat was handled
		if( prob_offset == 0u )
		{
			continue;
		}

		// Decode match length
		UpdateRange( Parameters );

		if( SimpleIterate( Parameters, Probabilities[prob_offset] ) )
		{
			// Low length (0-7)
			length = DecodeLowLength( Parameters, prob_offset + position_state );
		}
		else
		{
			// Mid or high length
			UpdateRange( Parameters );
			length = UpdateLength( Parameters, prob_offset, position_state );
		}

		// Decode distance for new matches
		if( Parameters.State >= Lzma::NumStates )
		{
			if( UpdateRepeatLength( Parameters, length ) )
			{
				// End marker or error
				break;
			}
		}

		// Add minimum match length
		length += LzmaDecoder::MinMatchLength;

		// Check if we have space to copy
		const int64 remaining = limit - DictionaryPosition;
		if( remaining == 0 )
		{
			break;
		}

		// Copy match data
		length = FinishBlock( length, remaining );

	} while( DictionaryPosition < limit && Parameters.DataBufferOffset < bufLimitOffset );

	// Final Range normalization
	UpdateRange( Parameters );

	// Store remaining length
	RemainingLength = length;

	// Check for error
	if( length >= LzmaDecoder::NormalMatchLengthErrorData )
	{
		return SevenZipResult::SevenZipErrorData;
	}

	return SevenZipResult::SevenZipOK;
}

void Lzma1Dec::WriteRemaining( const int64 limit )
{
	uint32 length = RemainingLength;
	if( length == 0u )
	{
		return;
	}

	int64 dic_pos = DictionaryPosition;
	const int64 remaining = limit - dic_pos;
	if( remaining < length )
	{
		length = static_cast<uint32>( remaining );
		if( length == 0u )
		{
			return;
		}
	}

	if( ( CheckDictionarySize == 0u ) && ( DecoderProperties.DictionarySize - ProcessedPosition <= length ) )
	{
		CheckDictionarySize = DecoderProperties.DictionarySize;
	}

	ProcessedPosition += length;
	RemainingLength -= length;
	const int64 rep0 = RepeatDistances[0];
	const int64 dic_buf_size = DictionaryBufferSize;
	do
	{
		Dictionary[dic_pos] = Dictionary[dic_pos - rep0 + ( dic_pos < rep0 ? dic_buf_size : 0u )];
		dic_pos++;
	} while( --length != 0 );

	DictionaryPosition = dic_pos;
}

/*
At staring of new stream we have one of the following symbols:
  - Literal        - is allowed
  - Non-Rep-Match  - is allowed only if it's end marker Symbol
  - Rep-Match      - is not allowed
We use early check of (RangeCoder:Code) over BadRepeatCode to simplify main decoding Code
*/

/*
LzmaDec_DecodeReal():
  It calls LzmaDec_DecodeRealInternal() and it adjusts limit according (p->CheckDictionarySize).

We correct (p->CheckDictionarySize) after LzmaDec_DecodeRealInternal() and in LzmaDec_WriteRemaining(),
and we support the following State of (p->CheckDictionarySize):
  if (total_processed < p->DecoderProperties.DictionarySize) then
  {
	(total_processed == p->ProcessedPosition)
	(p->CheckDictionarySize == 0)
  }
  else
	(p->CheckDictionarySize == p->DecoderProperties.DictionarySize)
*/

SevenZipResult Lzma1Dec::DecodeReal( int64 limit, const int64 bufLimitOffset )
{
	if( CheckDictionarySize == 0u )
	{
		const uint32 remaining = DecoderProperties.DictionarySize - ProcessedPosition;
		if( limit - DictionaryPosition > remaining )
		{
			limit = DictionaryPosition + remaining;
		}
	}

	SevenZipResult result = DecodeRealInternal( limit, bufLimitOffset );

	if( ( CheckDictionarySize == 0u ) && ( ProcessedPosition >= DecoderProperties.DictionarySize ) )
	{
		CheckDictionarySize = DecoderProperties.DictionarySize;
	}

	return result;
}

LzmaDummy Lzma1Dec::TryDummyLit( CParameters& parameters, const int64 bufOutOffset ) const
{
	int64 probability_offset = LzmaDecoder::IsRepeat + ( Lzma::NumStates * 4u ) + ( Lzma::NumLengthToPositionStates << Lzma::NumPositionSlotBits );
	if( ( CheckDictionarySize != 0u ) || ( ProcessedPosition != 0u ) )
	{
		const int64 a = ( ProcessedPosition & ( ( 1u << DecoderProperties.LiteralPositionBits ) - 1u ) ) << DecoderProperties.LiteralContextBits;
		const int64 b = Dictionary[( DictionaryPosition == 0u ? DictionaryBufferSize : DictionaryPosition ) - 1u] >> ( 8 - DecoderProperties.LiteralContextBits );
		probability_offset += Lzma::LiteralSize * ( a + b );
	}

	parameters.Symbol = 1u;
	if( parameters.State < LzmaDecoder::NumLiteralStates )
	{
		do
		{
			if( DummyDecodeTreeBit( parameters, Probabilities[probability_offset + parameters.Symbol], bufOutOffset ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}
		} while( parameters.Symbol < 0x100u );
	}
	else
	{
		uint32 match_byte = Dictionary[DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u )];
		parameters.Offset = 0x100u;
		do
		{
			match_byte += match_byte;
			parameters.Bit = parameters.Offset;
			parameters.Offset &= match_byte;

			if( DummyDecodeMatchedLiteralBit( parameters, Probabilities[probability_offset + parameters.Symbol + parameters.Offset + parameters.Bit], bufOutOffset ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}
		} while( parameters.Symbol < 0x100u );
	}

	return LzmaDummy::DummyLiteral;
}

LzmaDummy Lzma1Dec::TryDummyRep( CParameters& parameters, int64& probOffset, const int64 bufOutOffset, const uint32 posState ) const
{
	if( UpdateRangeDummy( parameters, bufOutOffset ) == LzmaDummy::DummyInputEof )
	{
		return LzmaDummy::DummyInputEof;
	}

	if( DummySimpleIterate( parameters, Probabilities[LzmaDecoder::IsRepeat + Lzma::NumStates + parameters.State] ) )
	{
		if( UpdateRangeDummy( parameters, bufOutOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		if( DummySimpleIterate( parameters, Probabilities[Lzma::NumFullDistancesSize + posState + parameters.State] ) )
		{
			return LzmaDummy::DummyRepeatShort;
		}
	}
	else
	{
		if( UpdateRangeDummy( parameters, bufOutOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		if( !DummySimpleIterate( parameters, Probabilities[LzmaDecoder::IsRepeat + ( Lzma::NumStates * 2u ) + parameters.State] ) )
		{
			if( DummyConsumeBit( parameters, Probabilities[LzmaDecoder::IsRepeat + ( Lzma::NumStates * 3u ) + parameters.State], bufOutOffset ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}
		}
	}

	parameters.State = Lzma::NumStates;
	probOffset = Lzma::NumFullDistancesSize + LzmaDecoder::NumStatePositionProbabilities;

	return LzmaDummy::DummyRepeat;
}

LzmaDummy Lzma1Dec::TryDummyDistance( CParameters& parameters, const int64 bufOutOffset, const uint32 length ) const
{
	if( parameters.State < 4u )
	{
		uint32 probability_offset = LzmaDecoder::IsRepeat + ( Lzma::NumStates * 4u ) + ( ( length < Lzma::NumLengthToPositionStates - 1u ? length : Lzma::NumLengthToPositionStates - 1u ) << Lzma::NumPositionSlotBits );
		parameters.Symbol = 1u;
		do
		{
			if( DummyDecodeTreeBit( parameters, Probabilities[probability_offset + parameters.Symbol], bufOutOffset ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}
		} while( parameters.Symbol < ( 1u << Lzma::NumPositionSlotBits ) );

		const uint32 pos_slot = parameters.Symbol - ( 1u << Lzma::NumPositionSlotBits );
		if( pos_slot >= Lzma::StartPositionModelIndex )
		{
			int8 num_direct_bits = static_cast<int8>( ( pos_slot >> 1 ) - 1 );

			if( pos_slot < Lzma::EndPositionModelIndex )
			{
				probability_offset = ( 2u | ( pos_slot & 1u ) ) << num_direct_bits;
			}
			else
			{
				num_direct_bits -= Lzma::NumAlignmentBits;
				do
				{
					if( UpdateRangeDummy( parameters, bufOutOffset ) == LzmaDummy::DummyInputEof )
					{
						return LzmaDummy::DummyInputEof;
					}

					parameters.Range >>= 1;
					parameters.Code -= parameters.Range & ( ( ( parameters.Code - parameters.Range ) >> 31 ) - 1u );
				} while( --num_direct_bits != 0 );

				probability_offset = LzmaDecoder::IsMatchBase + LzmaDecoder::NumStatePositionProbabilities;
				num_direct_bits = Lzma::NumAlignmentBits;
			}

			parameters.Symbol = 1u;
			parameters.Offset = 1u;
			do
			{
				if( DummyDecodePositionModelBit( parameters, Probabilities[probability_offset + parameters.Symbol], bufOutOffset ) == LzmaDummy::DummyInputEof )
				{
					return LzmaDummy::DummyInputEof;
				}
			} while( --num_direct_bits != 0 );
		}
	}

	return LzmaDummy::DummyOk;
}

LzmaDummy Lzma1Dec::TryDummy( const uint8* buffer, int64 bufferOffset, int64& bufOutOffset ) const
{
	LzmaDummy result;

	CParameters dummy = Parameters;
	dummy.DataBufferBase = buffer;
	dummy.DataBufferOffset = bufferOffset;

	// Convert bufOutOffset from size to absolute position
	bufOutOffset = bufferOffset + bufOutOffset;

	while( true )
	{
		uint32 pos_state = ( ProcessedPosition & PositionMask ) << 4;

		if( UpdateRangeDummy( dummy, bufOutOffset ) == LzmaDummy::DummyInputEof )
		{
			return LzmaDummy::DummyInputEof;
		}

		if( DummySimpleIterate( dummy, Probabilities[LzmaDecoder::IsMatchBase + pos_state + dummy.State] ) )
		{
			result = TryDummyLit( dummy, bufOutOffset );
			if( result == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}
		}
		else
		{
			int64 prob_offset = 0;

			if( UpdateRangeDummy( dummy, bufOutOffset ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}

			if( DummySimpleIterate( dummy, Probabilities[LzmaDecoder::IsRepeat + dummy.State] ) )
			{
				dummy.State = 0u;
				prob_offset = Lzma::NumFullDistancesSize + LzmaDecoder::NumStatePositionProbabilities + LzmaDecoder::NumLengthProbabilities;
				result = LzmaDummy::DummyMatch;
			}
			else
			{
				result = TryDummyRep( dummy, prob_offset, bufOutOffset, pos_state );
				if( result == LzmaDummy::DummyInputEof )
				{
					return LzmaDummy::DummyInputEof;
				}

				if( result == LzmaDummy::DummyRepeatShort )
				{
					break;
				}
			}

			if( UpdateRangeDummy( dummy, bufOutOffset ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}

			uint32 limit;
			uint32 offset2;
			int64 prob_len_offset;

			if( DummySimpleIterate( dummy, Probabilities[prob_offset] ) )
			{
				prob_len_offset = prob_offset + pos_state;
				offset2 = 0u;
				limit = Lzma::LengthEncoderNumLowSymbols;
			}
			else
			{
				if( UpdateRangeDummy( dummy, bufOutOffset ) == LzmaDummy::DummyInputEof )
				{
					return LzmaDummy::DummyInputEof;
				}

				if( DummySimpleIterate( dummy, Probabilities[prob_offset + Lzma::LengthEncoderNumLowSymbols] ) )
				{
					prob_len_offset = prob_offset + pos_state + Lzma::LengthEncoderNumLowSymbols;
					offset2 = Lzma::LengthEncoderNumLowSymbols;
					limit = Lzma::LengthEncoderNumLowSymbols;
				}
				else
				{
					prob_len_offset = prob_offset + ( LzmaDecoder::MaxNumPositionStates << ( Lzma::LengthEncoderNumLowBits + 1 ) );
					offset2 = Lzma::LengthEncoderNumLowSymbols << 1;
					limit = Lzma::LengthEncoderNumHighSymbols;
				}
			}

			dummy.Symbol = 1u;
			do
			{
				if( DummyDecodeTreeBit( dummy, Probabilities[prob_len_offset + dummy.Symbol], bufOutOffset ) == LzmaDummy::DummyInputEof )
				{
					return LzmaDummy::DummyInputEof;
				}
			} while( dummy.Symbol < limit );

			uint32 length = dummy.Symbol;
			length -= limit;
			length += offset2;

			if( TryDummyDistance( dummy, bufOutOffset, length ) == LzmaDummy::DummyInputEof )
			{
				return LzmaDummy::DummyInputEof;
			}
		}
		break;
	}

	if( UpdateRangeDummy( dummy, bufOutOffset ) == LzmaDummy::DummyInputEof )
	{
		return LzmaDummy::DummyInputEof;
	}

	// Convert back from absolute position to bytes consumed
	bufOutOffset = dummy.DataBufferOffset - bufferOffset;
	return result;
}

void Lzma1Dec::InitDictAndState( const bool initDict, const bool initState )
{
	RemainingLength = LzmaDecoder::MaxNormalMatchLength + 1u;
	TempBufferSize = 0u;

	if( initDict )
	{
		ProcessedPosition = 0u;
		CheckDictionarySize = 0u;
		RemainingLength = LzmaDecoder::MaxNormalMatchLength + 2u;
	}

	if( initState )
	{
		RemainingLength = LzmaDecoder::MaxNormalMatchLength + 2u;
	}
}

/*
LZMA supports optional end_marker.
So the decoder can lookahead for one additional LZMA-Symbol to check end_marker.
That additional LZMA-Symbol can require up to LzmaRequiredInput bytes in input stream.
When the decoder reaches dicLimit, it looks (finishMode) parameter:
  if (finishMode == LzmaFinishModeAny), the decoder doesn't lookahead
  if (finishMode != LzmaFinishModeAny), the decoder lookahead, if end_marker is possible for current position

When the decoder lookahead, and the lookahead Symbol is not end_marker, we have two ways:
  1) Strict mode (default) : the decoder returns SZ_ERROR_DATA.
  2) The relaxed mode (alternative mode) : we could return SZ_OK, and the caller
	 must check (status) value. The caller can show the error,
	 if the end of stream is expected, and the (status) is noit
	 LzmaStatusFinishedWithMark or LzmaStatusMaybeFinishedWithoutMark.
*/

SevenZipResult Lzma1Dec::DecodeToDict( int64 dicLimit, const uint8* compressed, int64 compressedOffset, int64& compressedLength, const LzmaFinishMode finishMode, LzmaStatus& status )
{
	SevenZipResult result;
	uint8 temporary_buffer[LzmaDecoder::LzmaRequiredInput];

	int64 in_size = compressedLength;
	compressedLength = 0;
	status = LzmaStatus::LzmaStatusNotSpecified;

	if( RemainingLength > LzmaDecoder::MaxNormalMatchLength )
	{
		if( RemainingLength > LzmaDecoder::MaxNormalMatchLength + 2u )
		{
			return RemainingLength == LzmaDecoder::NormalMatchLengthErrorFail ? SevenZipResult::SevenZipErrorFail : SevenZipResult::SevenZipErrorData;
		}

		for( ; in_size > 0 && TempBufferSize < Lzma::LzmaPropertiesSize; compressedLength++, in_size-- )
		{
			temporary_buffer[TempBufferSize++] = compressed[compressedOffset++];
		}

		if( TempBufferSize != 0u && temporary_buffer[0] != 0u )
		{
			return SevenZipResult::SevenZipErrorData;
		}

		if( TempBufferSize < Lzma::LzmaPropertiesSize )
		{
			status = LzmaStatus::LzmaStatusNeedsMoreInput;
			return SevenZipResult::SevenZipOK;
		}

		Parameters.Code =
			( static_cast< uint32 >( temporary_buffer[1] ) << 24 )
			| ( static_cast< uint32 >( temporary_buffer[2] ) << 16 )
			| ( static_cast< uint32 >( temporary_buffer[3] ) << 8 )
			| ( static_cast< uint32 >( temporary_buffer[4] ) << 0 );

		if( ( CheckDictionarySize == 0u ) && ( ProcessedPosition == 0u ) && ( Parameters.Code >= LzmaDecoder::BadRepeatCode ) )
		{
			return SevenZipResult::SevenZipErrorData;
		}

		Parameters.Range = UINT32_MAX;
		TempBufferSize = 0u;

		if( RemainingLength > LzmaDecoder::MaxNormalMatchLength + 1u )
		{
			const uint32 num_probs = LzmaDecoder::IsRepeat + ( Lzma::NumStates * 4u ) + ( Lzma::NumLengthToPositionStates << Lzma::NumPositionSlotBits )
									 + ( Lzma::LiteralSize << ( DecoderProperties.LiteralContextBits + DecoderProperties.LiteralPositionBits ) );
			std::fill_n( Probabilities, num_probs, static_cast< CProbability >( Lzma::BitModelTableSize >> 1 ) );

			RepeatDistances[0] = 1u;
			RepeatDistances[1] = 1u;
			RepeatDistances[2] = 1u;
			RepeatDistances[3] = 1u;
			Parameters.State = 0u;
		}

		RemainingLength = 0u;
	}

	while( true )
	{
		int64 processed;
			
		if( RemainingLength == LzmaDecoder::MaxNormalMatchLength )
		{
			if( Parameters.Code != 0u )
			{
				return SevenZipResult::SevenZipErrorData;
			}

			status = LzmaStatus::LzmaStatusFinishedWithMark;
			return SevenZipResult::SevenZipOK;
		}

		WriteRemaining( dicLimit );

		bool check_end_mark = false;

		if( DictionaryPosition >= dicLimit )
		{
			if( RemainingLength == 0u && Parameters.Code == 0u )
			{
				status = LzmaStatus::LzmaStatusMaybeFinishedWithoutMark;
				return SevenZipResult::SevenZipOK;
			}

			if( finishMode == LzmaFinishMode::LzmaFinishModeAny )
			{
				status = LzmaStatus::LzmaStatusNotFinished;
				return SevenZipResult::SevenZipOK;
			}

			if( RemainingLength != 0u )
			{
				status = LzmaStatus::LzmaStatusNotFinished;
				// for strict mode
				return SevenZipResult::SevenZipErrorData;
			}

			check_end_mark = true;
		}

		int32 dummy_processed = -1;
		if( TempBufferSize == 0 )
		{
			int64 buf_limit_offset;

			// In the first branch (TempBufferSize == 0):
			if( in_size < LzmaDecoder::LzmaRequiredInput || check_end_mark )
			{
				int64 buf_out_offset = in_size;

				LzmaDummy dummy_result = TryDummy( compressed, compressedOffset, buf_out_offset );

				if( dummy_result == LzmaDummy::DummyInputEof )
				{
					if( in_size >= LzmaDecoder::LzmaRequiredInput )
					{
						break;
					}

					compressedLength += in_size;
					TempBufferSize = static_cast< uint32 >( in_size );

					memcpy( temporary_buffer, compressed + compressedOffset, static_cast<uint64>( in_size ) );

					status = LzmaStatus::LzmaStatusNeedsMoreInput;
					return SevenZipResult::SevenZipOK;
				}

				dummy_processed = static_cast< int32 >( buf_out_offset );
				if( dummy_processed > LzmaDecoder::LzmaRequiredInput )
				{
					break;
				}

				if( check_end_mark && dummy_result != LzmaDummy::DummyMatch )
				{
					const uint32 unsigned_dummy_processed = static_cast< uint32 >( dummy_processed );
					compressedLength += unsigned_dummy_processed;
					TempBufferSize = unsigned_dummy_processed;
					memcpy( temporary_buffer, compressed + compressedOffset, unsigned_dummy_processed );

					status = LzmaStatus::LzmaStatusNotFinished;
					return SevenZipResult::SevenZipErrorData;
				}

				buf_limit_offset = 0;
				// we will decode only one iteration
			}
			else
			{
				buf_limit_offset = in_size - LzmaDecoder::LzmaRequiredInput;
			}

			Parameters.DataBufferBase = compressed;
			Parameters.DataBufferOffset = compressedOffset;

			result = DecodeReal( dicLimit, buf_limit_offset );

			processed = Parameters.DataBufferOffset - compressedOffset;
			if( dummy_processed < 0 )
			{
				if( processed > in_size )
				{
					break;
				}
			}
			else if( static_cast< uint32 >( dummy_processed ) != processed )
			{
				break;
			}

			compressedOffset += processed;
			in_size -= processed;
			compressedLength += processed;

			if( result != SevenZipResult::SevenZipOK )
			{
				RemainingLength = LzmaDecoder::NormalMatchLengthErrorData;
				return SevenZipResult::SevenZipErrorData;
			}

			continue;
		}

		// we have some data in (dec1->tempBuf)
		// in strict mode: TempBufferSize is not enough for one Symbol decoding.
		// in relaxed mode: TempBufferSize not larger than required for one Symbol decoding.

		uint32 remaining = TempBufferSize;
		uint32 ahead = 0u;
		dummy_processed = -1;

		while( ( remaining < LzmaDecoder::LzmaRequiredInput ) && ( ahead < in_size ) )
		{
			temporary_buffer[remaining++] = compressed[compressedOffset + ahead++];
		}

		// In the second branch (temp buffer path):
		if( ( remaining < LzmaDecoder::LzmaRequiredInput ) || check_end_mark )
		{
			int64 buf_out_offset = remaining;

			LzmaDummy dummy_result = TryDummy( temporary_buffer, 0, buf_out_offset );

			if( dummy_result == LzmaDummy::DummyInputEof )
			{
				if( remaining >= LzmaDecoder::LzmaRequiredInput )
				{
					break;
				}

				TempBufferSize = remaining;
				compressedLength += static_cast< int64 >( ahead );
				status = LzmaStatus::LzmaStatusNeedsMoreInput;
				return SevenZipResult::SevenZipOK;
			}

			dummy_processed = static_cast< int32 >( buf_out_offset );

			if( static_cast< uint32 >( dummy_processed ) < TempBufferSize )
			{
				break;
			}

			if( check_end_mark && dummy_result != LzmaDummy::DummyMatch )
			{
				compressedLength += static_cast< uint32 >( dummy_processed ) - TempBufferSize;
				TempBufferSize = static_cast< uint32 >( dummy_processed );
				status = LzmaStatus::LzmaStatusNotFinished;
				// for strict mode
				return SevenZipResult::SevenZipErrorData;
			}
		}

		Parameters.DataBufferBase = temporary_buffer;
		Parameters.DataBufferOffset = 0u;

		// we decode one Symbol from (dec1->tempBuf) here, so the (bufLimit) is equal to (dec1->DataBuffer)
		result = DecodeReal( dicLimit, remaining );

		processed = Parameters.DataBufferOffset;
		remaining = TempBufferSize;

		if( dummy_processed < 0 )
		{
			if( processed > LzmaDecoder::LzmaRequiredInput )
			{
				break;
			}

			if( processed < remaining )
			{
				break;
			}
		}
		else if( static_cast< uint32 >( dummy_processed ) != processed )
		{
			break;
		}

		processed -= remaining;

		compressedOffset += processed;
		in_size -= processed;
		compressedLength += processed;
		TempBufferSize = 0u;

		if( result != SevenZipResult::SevenZipOK )
		{
			RemainingLength = LzmaDecoder::NormalMatchLengthErrorData;
			return SevenZipResult::SevenZipErrorData;
		}
	}

	RemainingLength = LzmaDecoder::NormalMatchLengthErrorFail;
	return SevenZipResult::SevenZipErrorFail;
}

/** Decode the Lzma 5 byte array to dictionary size and decompression parameters. */
SevenZipResult Lzma1Dec::DecodeProperties( const uint8* decoderProperties, const uint32 propsSize )
{
	if( propsSize < Lzma::LzmaPropertiesSize )
	{
		return SevenZipResult::SevenZipErrorUnsupported;
	}

	const uint32 dict_size = ( static_cast< uint32 >( decoderProperties[1] ) << 0 )
		| ( static_cast< uint32 >( decoderProperties[2] ) << 8 )
		| ( static_cast< uint32 >( decoderProperties[3] ) << 16 )
		| ( static_cast< uint32 >( decoderProperties[4] ) << 24 );

	DecoderProperties.DictionarySize = std::clamp( dict_size, Lzma::MinDictionarySize, Lzma::MaxDictionarySize );

	uint8 encoded_parameters = decoderProperties[0];
	if( encoded_parameters >= ( 9u * 5u * 5u ) )
	{
		return SevenZipResult::SevenZipErrorUnsupported;
	}

	DecoderProperties.LiteralContextBits = static_cast< uint8 >( encoded_parameters % 9u );
	encoded_parameters /= 9;
	DecoderProperties.PositionBits = static_cast< uint8 >( encoded_parameters / 5u );
	DecoderProperties.LiteralPositionBits = static_cast< uint8 >( encoded_parameters % 5u );

	PositionMask = ( 1u << DecoderProperties.PositionBits ) - 1u;
	LiteralMask = ( 256u << DecoderProperties.LiteralPositionBits ) - ( 256u >> DecoderProperties.LiteralContextBits );

	return SevenZipResult::SevenZipOK;
}

void Lzma1Dec::FreeProbabilities()
{
	if( Probabilities != nullptr )
	{
		Alloc->Free( Probabilities, NumProbabilities * sizeof( CProbability ), "Lzma1Dec::Probabilities" );
		Probabilities = nullptr;
	}
}

SevenZipResult Lzma1Dec::AllocateProbabilities()
{
	const uint32 num_probabilities = LzmaDecoder::IsRepeat + ( Lzma::NumStates * 4u ) + ( Lzma::NumLengthToPositionStates << Lzma::NumPositionSlotBits )
									 + ( Lzma::LiteralSize << ( DecoderProperties.LiteralContextBits + DecoderProperties.LiteralPositionBits ) );
	if( ( Probabilities == nullptr ) || ( num_probabilities != NumProbabilities ) )
	{
		FreeProbabilities();

		Probabilities = static_cast<CProbability*>( Alloc->Alloc( num_probabilities * sizeof( CProbability ), "Lzma1Dec::Probabilities" ) );
		if( Probabilities == nullptr )
		{
			return SevenZipResult::SevenZipErrorMemory;
		}

		NumProbabilities = num_probabilities;
	}

	return SevenZipResult::SevenZipOK;
}

SevenZipResult Lzma1Decode( uint8* decompressed, int64& decompressedLength, const uint8* compressed, int64& compressedLength, const uint8* propData, const uint32 propSize, const LzmaFinishMode finishMode, LzmaStatus& status, MemoryInterface* alloc )
{
	Lzma1Dec dec1( decompressed, alloc );
	int64 out_size = decompressedLength;
	int64 in_size = compressedLength;

	decompressedLength = 0;
	compressedLength = 0;
	status = LzmaStatus::LzmaStatusNotSpecified;

	SevenZipResult result = dec1.DecodeProperties( propData, propSize );
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	result = dec1.AllocateProbabilities();
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	dec1.DictionaryBufferSize = out_size;
	dec1.InitDictAndState( true, true );

	compressedLength = in_size;
	result = dec1.DecodeToDict( out_size, compressed, 0, compressedLength, finishMode, status );
	decompressedLength = dec1.DictionaryPosition;
	if( result == SevenZipResult::SevenZipOK && status == LzmaStatus::LzmaStatusNeedsMoreInput )
	{
		result = SevenZipResult::SevenZipErrorInputEof;
	}

	dec1.FreeProbabilities();
	return result;
}
