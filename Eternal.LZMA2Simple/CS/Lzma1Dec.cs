// Copyright Eternal Developments LLC. All Rights Reserved.

using Math = System.Math;

namespace Eternal.LZMA2SimpleCS.CS
{
	using CProbability = UInt16;
	using int32 = Int32;
	using int64 = Int64;
	using int8 = SByte;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	public enum ELzmaFinishMode
		: int8
	{
		// Finish at any point
		LzmaFinishModeAny,
		// Block must be finished at the end
		LzmaFinishModeEnd
	};

	/**
	 * ELzmaStatus is used only as output value for function call
	 */
	public enum ELzmaStatus
		: int8
	{
		// Use main error Code instead
		LzmaStatusNotSpecified = 0,
		// Stream was finished with end mark.
		LzmaStatusFinishedWithMark,
		// Stream was not finished
		LzmaStatusNotFinished,
		// You must provide more input bytes
		LzmaStatusNeedsMoreInput,
		// There is probability that stream was finished without end mark
		LzmaStatusMaybeFinishedWithoutMark
	};
	
	public enum ELzmaDummy
		: int8
	{
		// Need more input data
		DummyInputEof,
		DummyLiteral,
		DummyMatch,
		DummyRepeat,
		DummyRepeatShort,
		DummyOk
	};

	public class LzmaDecoder
	{
		LzmaDecoder()
		{
		}

		public static readonly uint32 LzmaRequiredInput = 20;

		public static readonly int8 MaxNumPositionBits = 4;
		public static readonly uint32 MaxNumPositionStates = 1u << MaxNumPositionBits;
		public static readonly uint32 NumLengthProbabilities = ( MaxNumPositionStates << ( Lzma.LengthEncoderNumLowBits + 1 ) ) + Lzma.LengthEncoderNumHighSymbols;

		public static readonly uint32 NumExtendedStates = 16u;
		public static readonly uint32 NumStatePositionProbabilities = NumExtendedStates << MaxNumPositionBits;
		public static readonly uint32 NumLiteralStates = 7u;
		public static readonly uint32 MinMatchLength = 2u;

		public static readonly uint32 MaxNormalMatchLength = MinMatchLength + ( Lzma.LengthEncoderNumLowSymbols << 1 ) + Lzma.LengthEncoderNumHighSymbols;
		public static readonly uint32 NormalMatchLengthErrorData = 1u << 9;
		public static readonly uint32 NormalMatchLengthErrorFail = NormalMatchLengthErrorData - 1u;

		public static readonly uint32 IsMatchBase = Lzma.NumFullDistancesSize + NumStatePositionProbabilities + (NumLengthProbabilities << 1 );
		public static readonly uint32 IsRepeat = IsMatchBase + NumStatePositionProbabilities + Lzma.AlignmentTableSize;

		public static readonly uint32 MaxBound = ( ( UInt32.MaxValue >> Lzma.NumBitModelTotalBits ) << ( Lzma.NumBitModelTotalBits - 1 ) );
		public static readonly uint32 BadRepeatCode = ( MaxBound + ( ( ( UInt32.MaxValue - MaxBound ) >> Lzma.NumBitModelTotalBits ) << ( Lzma.NumBitModelTotalBits - 1 ) ) );
	}

	/* ---------- LZMA Parameters ---------- */

	internal struct CParameters
	{
		public uint8[] DataBufferBase;
		public int64 DataBufferOffset;
		public uint32 Range;
		public uint32 Code;
		public uint32 State;
		public uint32 Symbol;
		public uint32 Offset;
		public uint32 Bit;
	}

	internal class Lzma1Dec
	{
		internal struct CLzmaDecoderProperties
		{
			public uint8 LiteralContextBits;
			public uint8 LiteralPositionBits;
			public uint8 PositionBits;
			public uint32 DictionarySize;
		}

		public CLzmaDecoderProperties DecoderProperties;
		public CProbability[] Probabilities = [];
		public int64 DictionaryBufferSize;
		public int64 DictionaryPosition;

		public uint32 PositionMask;
		public uint32 LiteralMask;

		private readonly uint8[] Dictionary;
		private readonly uint32[] RepeatDistances = new uint32[Lzma.NumRepeats];

		private CParameters Parameters = new CParameters();

		private uint32 ProcessedPosition;
		private uint32 CheckDictionarySize;
		private uint32 RemainingLength;

		private uint32 NumProbabilities;
		private uint32 TempBufferSize;

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

		public Lzma1Dec( uint8[] decompressed )
		{
			Dictionary = decompressed;
		}

		public void UpdateWithDecompressed( uint8[] src, int64 offset, int64 size )
		{
			Array.Copy( src, offset, Dictionary, DictionaryPosition, size );
			DictionaryPosition += size;
			if( ( CheckDictionarySize == 0u ) && ( DecoderProperties.DictionarySize - ProcessedPosition <= size ) )
			{
				CheckDictionarySize = DecoderProperties.DictionarySize;
			}

			ProcessedPosition += ( uint32 )( size & UInt32.MaxValue );
		}

		private ELzmaDummy UpdateRangeDummy( ref CParameters parameters, int64 bufLimitOffset )
		{
			if( parameters.Range < Lzma.MaxRangeValue )
			{
				if( parameters.DataBufferOffset >= bufLimitOffset )
				{
					return ELzmaDummy.DummyInputEof;
				}

				parameters.Range <<= 8;
				parameters.Code = ( parameters.Code << 8 ) | parameters.DataBufferBase[parameters.DataBufferOffset++];
			}

			return ELzmaDummy.DummyOk;
		}

		private void UpdateRange( ref CParameters parameters )
		{
			if( parameters.Range < Lzma.MaxRangeValue )
			{
				parameters.Range <<= 8;
				parameters.Code = ( parameters.Code << 8 ) | parameters.DataBufferBase[parameters.DataBufferOffset++];
			}
		}

		private bool SimpleIterate( ref CParameters parameters, ref CProbability probability )
		{
			uint32 bound = ( parameters.Range >> Lzma.NumBitModelTotalBits ) * probability;
			if( parameters.Code < bound )
			{
				/* Bit = 0 path */
				parameters.Range = bound;
				probability = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );
				return true;
			}
			else
			{
				/* Bit = 1 path */
				parameters.Range -= bound;
				parameters.Code -= bound;
				probability = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );
				return false;
			}
		}

		private bool DummySimpleIterate( ref CParameters parameters, ref CProbability probability )
		{
			uint32 bound = ( parameters.Range >> Lzma.NumBitModelTotalBits ) * probability;
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
		private ELzmaDummy DummyDecodeTreeBit( ref CParameters parameters, ref CProbability probability, int64 bufLimitOffset )
		{
			if( UpdateRangeDummy( ref parameters, bufLimitOffset ) == ELzmaDummy.DummyInputEof )
			{
				return ELzmaDummy.DummyInputEof;
			}

			if( DummySimpleIterate( ref parameters, ref probability ) )
			{
				parameters.Symbol <<= 1;
			}
			else
			{
				parameters.Symbol <<= 1;
				parameters.Symbol++;
			}

			return ELzmaDummy.DummyOk;
		}

		/// <summary>
		/// Identical to LzmaDec_Iterate, but does not write any data.
		/// </summary>
		/// <returns></returns>
		private ELzmaDummy DummyDecodeMatchedLiteralBit( ref CParameters parameters, ref CProbability probability, int64 bufLimitOffset )
		{
			if( UpdateRangeDummy( ref parameters, bufLimitOffset ) == ELzmaDummy.DummyInputEof )
			{
				return ELzmaDummy.DummyInputEof;
			}

			if( DummySimpleIterate( ref parameters, ref probability ) )
			{
				parameters.Symbol <<= 1;
				parameters.Offset ^= parameters.Bit;
			}
			else
			{
				parameters.Symbol <<= 1;
				parameters.Symbol++;
			}

			return ELzmaDummy.DummyOk;
		}

		/// <summary>
		/// Identical to LzmaDec_Iterate, but does not write any data.
		/// </summary>
		/// <returns></returns>
		private ELzmaDummy DummyDecodePositionModelBit( ref CParameters parameters, ref CProbability probability, int64 bufLimitOffset )
		{
			if( UpdateRangeDummy( ref parameters, bufLimitOffset ) == ELzmaDummy.DummyInputEof )
			{
				return ELzmaDummy.DummyInputEof;
			}

			if( DummySimpleIterate( ref parameters, ref probability ) )
			{
				parameters.Symbol += parameters.Offset;
				parameters.Offset <<= 1;
			}
			else
			{
				parameters.Offset <<= 1;
				parameters.Symbol += parameters.Offset;
			}

			return ELzmaDummy.DummyOk;
		}

		/// <summary>
		/// Identical to LzmaDec_Iterate, but does not write any data.
		/// </summary>
		/// <returns></returns>
		private ELzmaDummy DummyConsumeBit( ref CParameters parameters, ref CProbability probability, int64 bufLimitOffset )
		{
			if( UpdateRangeDummy( ref parameters, bufLimitOffset ) == ELzmaDummy.DummyInputEof )
			{
				return ELzmaDummy.DummyInputEof;
			}

			DummySimpleIterate( ref parameters, ref probability );

			return ELzmaDummy.DummyOk;
		}

		private void DecodeTreeBit( ref CParameters parameters, ref CProbability prob )
		{
			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref prob ) )
			{
				parameters.Symbol <<= 1;
			}
			else
			{
				parameters.Symbol <<= 1;
				parameters.Symbol++;
			}
		}

		private void DecodeMatchedLiteralBit( ref CParameters parameters, ref CProbability prob )
		{
			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref prob ) )
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

		private void DecodeReverseBit( ref CParameters parameters, ref CProbability prob )
		{
			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref prob ) )
			{
				parameters.Symbol += parameters.Offset;
			}
			else
			{
				parameters.Symbol += ( parameters.Offset << 1 );
			}
		}

		private void DecodeReverseBitLast( ref CParameters parameters, ref CProbability prob )
		{
			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref prob ) )
			{
				parameters.Symbol -= 8;
			}
		}

		private void DecodePositionModelBit( ref CParameters parameters, ref CProbability prob )
		{
			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref prob ) )
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

		private void UpdateDistance( ref CParameters parameters )
		{
			uint32 distance;

			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + ( Lzma.NumStates * 2u ) + parameters.State] ) )
			{
				distance = RepeatDistances[1];
			}
			else
			{
				UpdateRange( ref parameters );

				if( SimpleIterate( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + ( Lzma.NumStates * 3u ) + parameters.State] ) )
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

		private void UpdateSymbol( ref CParameters parameters )
		{
			uint32 probability_offset = LzmaDecoder.IsRepeat + ( Lzma.NumStates * 4u ) + ( Lzma.NumLengthToPositionStates << Lzma.NumPositionSlotBits );
			if( ProcessedPosition != 0u || CheckDictionarySize != 0u )
			{
				uint32 add = ( ( ProcessedPosition << 8 ) + Dictionary[( DictionaryPosition == 0u ? DictionaryBufferSize : DictionaryPosition ) - 1u] ) & LiteralMask;
				probability_offset += 3u * ( add << DecoderProperties.LiteralContextBits );
			}

			ProcessedPosition++;
			parameters.Symbol = 1u;

			uint32 old_state = parameters.State;
			parameters.State = Lzma.LiteralNextStateLut[old_state];

			if( old_state < LzmaDecoder.NumLiteralStates )
			{
				for( uint32 count = 0; count < 8; count++ )
				{
					DecodeTreeBit( ref parameters, ref Probabilities[probability_offset + parameters.Symbol] );
				}
			}
			else
			{
				uint32 match_byte = Dictionary[DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u )];
				parameters.Offset = 256u;

				for( uint32 count = 0; count < 8; count++ )
				{
					match_byte <<= 1;
					parameters.Bit = parameters.Offset;
					parameters.Offset &= match_byte;

					DecodeMatchedLiteralBit( ref parameters, ref Probabilities[probability_offset + parameters.Symbol + parameters.Offset + parameters.Bit] );
				}
			}

			Dictionary[DictionaryPosition++] = ( uint8 )( parameters.Symbol & 0xff );
		}

		private uint32 UpdateLength( ref CParameters parameters, uint32 probabilityOffset, uint32 positionState )
		{
			uint32 length;
			if( SimpleIterate( ref parameters, ref Probabilities[probabilityOffset + Lzma.LengthEncoderNumLowSymbols] ) )
			{
				uint32 length_offset = probabilityOffset + positionState + ( 1u << Lzma.LengthEncoderNumLowBits );
				parameters.Symbol = 1u;

				DecodeTreeBit( ref parameters, ref Probabilities[length_offset + parameters.Symbol] );
				DecodeTreeBit( ref parameters, ref Probabilities[length_offset + parameters.Symbol] );
				DecodeTreeBit( ref parameters, ref Probabilities[length_offset + parameters.Symbol] );

				length = parameters.Symbol;
			}
			else
			{
				uint32 length_offset = probabilityOffset + ( LzmaDecoder.MaxNumPositionStates << ( Lzma.LengthEncoderNumLowBits + 1 ) );
				parameters.Symbol = 1u;

				do
				{
					DecodeTreeBit( ref parameters, ref Probabilities[length_offset + parameters.Symbol] );
				} while( parameters.Symbol < Lzma.LengthEncoderNumHighSymbols );

				length = parameters.Symbol - Lzma.LengthEncoderNumHighSymbols;
				length += Lzma.LengthEncoderNumLowSymbols << 1;
			}

			return length;
		}

		private bool UpdateRepeatLength( ref CParameters parameters, ref uint32 length )
		{
			// Decode 6-Bit position slot
			uint32 len_to_pos_state = ( length < Lzma.NumLengthToPositionStates ) ? length : Lzma.NumLengthToPositionStates - 1u;
			uint32 prob_offset = LzmaDecoder.IsRepeat + ( Lzma.NumStates * 4u ) + ( len_to_pos_state << Lzma.NumPositionSlotBits );

			parameters.Symbol = 1u;

			for( uint32 count = 0; count < 6; count++ )
			{
				DecodeTreeBit( ref parameters, ref Probabilities[prob_offset + parameters.Symbol] );
			}

			uint32 distance = parameters.Symbol - 64u;

			// Fast path: small distances (0-3) need no footer
			if( distance < Lzma.StartPositionModelIndex )
			{
				distance++;
			}
			else
			{
				// Decode distance footer
				uint32 pos_slot = distance;
				int8 num_direct_bits = ( int8 )( ( distance >> 1 ) - 1 );
				distance = 2u | ( distance & 1u );

				if( pos_slot < Lzma.EndPositionModelIndex )
				{
					// Position model (4-13)
					distance <<= num_direct_bits;

					parameters.Symbol = distance + 1u;
					parameters.Offset = 1u;

					do
					{
						DecodePositionModelBit( ref parameters, ref Probabilities[parameters.Symbol] );
					} while( --num_direct_bits != 0 );

					distance = parameters.Symbol - parameters.Offset;
				}
				else
				{
					// Large distances (14+): direct bits + alignment
					num_direct_bits -= Lzma.NumAlignmentBits;

					// Decode direct bits without probability model
					do
					{
						UpdateRange( ref parameters );

						parameters.Range >>= 1;
						uint32 t = ( parameters.Code - parameters.Range ) >> 31;
						parameters.Code -= parameters.Range & ( t - 1u );
						distance = ( distance << 1 ) + ( 1u - t );
					} while( --num_direct_bits != 0 );

					// Decode 4-Bit alignment
					uint32 prob_align_offset = Lzma.NumFullDistancesSize + ( LzmaDecoder.NumStatePositionProbabilities << 1 ) + ( LzmaDecoder.NumLengthProbabilities << 1 );
					distance <<= Lzma.NumAlignmentBits;

					parameters.Symbol = 1u;

					// Unrolled alignment decoding
					parameters.Offset = 1u;
					DecodeReverseBit( ref parameters, ref Probabilities[prob_align_offset + parameters.Symbol] );

					parameters.Offset = 2u;
					DecodeReverseBit( ref parameters, ref Probabilities[prob_align_offset + parameters.Symbol] );

					parameters.Offset = 4u;
					DecodeReverseBit( ref parameters, ref Probabilities[prob_align_offset + parameters.Symbol] );

					DecodeReverseBitLast( ref parameters, ref Probabilities[prob_align_offset + parameters.Symbol] );

					distance |= parameters.Symbol;

					// Check for end marker
					if( distance == UInt32.MaxValue )
					{
						length = LzmaDecoder.MaxNormalMatchLength;
						parameters.State -= Lzma.NumStates;
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
			parameters.State = Lzma.MatchNextStateLut[parameters.State - Lzma.NumStates];

			// Validate distance
			uint32 max_distance = ( CheckDictionarySize == 0u ) ? ProcessedPosition : CheckDictionarySize;
			if( distance > max_distance )
			{
				length += LzmaDecoder.NormalMatchLengthErrorData + LzmaDecoder.MinMatchLength;
				return true;
			}

			return false;
		}

		private uint32 FinishBlock( uint32 length, int64 remaining )
		{
			// Clamp length to available space
			uint32 copy_length = ( remaining > UInt32.MaxValue ) ? length : Math.Min( ( uint32 )( remaining ), length );

			// Calculate source position with wraparound
			int64 src_pos = DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u );

			// Update processed position
			ProcessedPosition += copy_length;

			// Check if we can use fast path (no wraparound)
			if( copy_length <= DictionaryBufferSize - src_pos )
			{
				int64 dest_start = DictionaryPosition;
				DictionaryPosition += copy_length;

				if( RepeatDistances[0] >= copy_length )
				{
					// No overlap — bulk copy
					Array.Copy(  Dictionary, src_pos, Dictionary, dest_start, copy_length );
				}
				else if( RepeatDistances[0] == 1u )
				{
					// Run of one byte
					Array.Fill( Dictionary, Dictionary[src_pos], ( int32 )dest_start, ( int32 )copy_length  );
				}
				else
				{
					// Short-period pattern: seed first period, then double-copy
					uint32 written = RepeatDistances[0];
					Array.Copy( Dictionary, src_pos, Dictionary, dest_start, written );
					while( written < copy_length )
					{
						uint32 chunk = Math.Min( written, copy_length - written );
						Array.Copy( Dictionary, dest_start, Dictionary, dest_start + written, chunk );
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
		private bool DecodeShortRepeat( ref CParameters parameters, uint32 positionState )
		{
			uint32 probability_offset = Lzma.NumFullDistancesSize + positionState + parameters.State;
			uint32 probability = Probabilities[probability_offset];
			uint32 bound = ( parameters.Range >> Lzma.NumBitModelTotalBits ) * probability;

			if( parameters.Code < bound )
			{
				// Short repeat: copy one byte from REP0 distance
				parameters.Range = bound;
				Probabilities[probability_offset] = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );

				int64 src_pos = DictionaryPosition - RepeatDistances[0] + ( DictionaryPosition < RepeatDistances[0] ? DictionaryBufferSize : 0u );
				Dictionary[DictionaryPosition] = Dictionary[src_pos];
				DictionaryPosition++;
				ProcessedPosition++;
				parameters.State = Lzma.ShortRepNextStateLut[parameters.State];
				return true;
			}

			parameters.Range -= bound;
			parameters.Code -= bound;
			Probabilities[probability_offset] = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );
			return false;
		}

		// Decode match/repeat type and set up for length decoding
		private uint32 DecodeMatchType( ref CParameters parameters, uint32 positionState )
		{
			if( SimpleIterate( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + parameters.State] ) )
			{
				// New match
				parameters.State += Lzma.NumStates;
				return Lzma.NumFullDistancesSize + LzmaDecoder.NumStatePositionProbabilities + LzmaDecoder.NumLengthProbabilities;
			}

			// Repeat match
			UpdateRange( ref parameters );

			if( SimpleIterate( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + Lzma.NumStates + parameters.State] ) )
			{
				// Try short repeat (REP0 with length 1)
				UpdateRange( ref parameters );

				if( DecodeShortRepeat( ref parameters, positionState ) )
				{
					// Signal: short repeat handled, continue main loop
					return 0u; 
				}
			}
			else
			{
				// REP1/REP2/REP3
				UpdateDistance( ref parameters );
			}

			parameters.State = Lzma.RepNextStateLut[parameters.State];
			return Lzma.NumFullDistancesSize + LzmaDecoder.NumStatePositionProbabilities;
		}

		// Decode low-Range length (3 bits, values 0-7)
		private uint32 DecodeLowLength( ref CParameters parameters, uint32 positionState )
		{
			parameters.Symbol = 1u;

			DecodeTreeBit( ref parameters, ref Probabilities[positionState + parameters.Symbol] );
			DecodeTreeBit( ref parameters, ref Probabilities[positionState + parameters.Symbol] );
			DecodeTreeBit( ref parameters, ref Probabilities[positionState + parameters.Symbol] );

			return parameters.Symbol - 8u;
		}

		// Simplified main decode function
		SevenZipResult DecodeRealInternal( int64 limit, int64 bufLimitOffset )
		{
			uint32 length = 0u;

			do
			{
				UpdateRange( ref Parameters );

				// Calculate position State
				uint32 position_state = ( ProcessedPosition & PositionMask ) << 4;

				// Decode isMatch Bit
				uint32 probability_offset = LzmaDecoder.IsMatchBase + position_state + Parameters.State;
				uint32 probability = Probabilities[probability_offset];
				uint32 bound = ( Parameters.Range >> Lzma.NumBitModelTotalBits ) * probability;

				if( Parameters.Code < bound )
				{
					// LITERAL path
					Parameters.Range = bound;
					Probabilities[probability_offset] = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );

					// Decode literal
					UpdateSymbol( ref Parameters );
					continue;
				}

				// MATCH or REPEAT path
				Parameters.Range -= bound;
				Parameters.Code -= bound;
				UpdateRange( ref Parameters );
				Probabilities[probability_offset] = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );

				// Decode match/repeat type and get length probability table
				uint32 prob_offset = DecodeMatchType( ref Parameters, position_state );

				// Check if short repeat was handled
				if( prob_offset == 0u )
				{
					continue;
				}

				// Decode match length
				UpdateRange( ref Parameters );

				if( SimpleIterate( ref Parameters, ref Probabilities[prob_offset] ) )
				{
					// Low length (0-7)
					length = DecodeLowLength( ref Parameters, prob_offset + position_state );
				}
				else
				{
					// Mid or high length
					UpdateRange( ref Parameters );
					length = UpdateLength( ref Parameters, prob_offset, position_state );
				}

				// Decode distance for new matches
				if( Parameters.State >= Lzma.NumStates )
				{
					if( UpdateRepeatLength( ref Parameters, ref length ) )
					{
						// End marker or error
						break;
					}
				}

				// Add minimum match length
				length += LzmaDecoder.MinMatchLength;

				// Check if we have space to copy
				int64 remaining = limit - DictionaryPosition;
				if( remaining == 0u )
				{
					break;
				}

				// Copy match data
				length = FinishBlock( length, remaining );

			} while( DictionaryPosition < limit && Parameters.DataBufferOffset < bufLimitOffset );

			// Final Range normalization
			UpdateRange( ref Parameters );

			// Store remaining length
			RemainingLength = length;

			// Check for error
			if( length >= LzmaDecoder.NormalMatchLengthErrorData )
			{
				return SevenZipResult.SevenZipErrorData;
			}

			return SevenZipResult.SevenZipOK;
		}

		private void WriteRemaining( int64 limit )
		{
			uint32 length = RemainingLength;
			if( length == 0u )
			{
				return;
			}

			int64 dic_pos = DictionaryPosition;
			int64 remaining = limit - dic_pos;
			if( remaining < length )
			{
				length = ( uint32 )( remaining );
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
			int64 rep0 = RepeatDistances[0];
			int64 dic_buf_size = DictionaryBufferSize;
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

		private SevenZipResult DecodeReal( int64 limit, int64 bufLimitOffset )
		{
			if( CheckDictionarySize == 0u )
			{
				uint32 remaining = DecoderProperties.DictionarySize - ProcessedPosition;
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

		private ELzmaDummy TryDummyLit( ref CParameters parameters, int64 bufOutOffset )
		{
			int64 probability_offset = LzmaDecoder.IsRepeat + ( Lzma.NumStates * 4u ) + ( Lzma.NumLengthToPositionStates << Lzma.NumPositionSlotBits );
			if( ( CheckDictionarySize != 0u ) || ( ProcessedPosition != 0u ) )
			{
				int64 a = ( ProcessedPosition & ( ( 1u << DecoderProperties.LiteralPositionBits ) - 1u ) ) << DecoderProperties.LiteralContextBits;
				int64 b = ( Dictionary[( DictionaryPosition == 0u ? DictionaryBufferSize : DictionaryPosition ) - 1u] >> ( 8 - DecoderProperties.LiteralContextBits ) );
				probability_offset += Lzma.LiteralSize * ( a + b );
			}

			parameters.Symbol = 1u;
			if( parameters.State < LzmaDecoder.NumLiteralStates )
			{
				do
				{
					if( DummyDecodeTreeBit( ref parameters, ref Probabilities[probability_offset + parameters.Symbol], bufOutOffset ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
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

					if( DummyDecodeMatchedLiteralBit( ref parameters, ref Probabilities[probability_offset + parameters.Symbol + parameters.Offset + parameters.Bit], bufOutOffset ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}
				} while( parameters.Symbol < 0x100u );
			}

			return ELzmaDummy.DummyLiteral;
		}

		private ELzmaDummy TryDummyRep( ref CParameters parameters, ref int64 probOffset, int64 bufOutOffset, uint32 posState )
		{
			if( UpdateRangeDummy( ref parameters, bufOutOffset ) == ELzmaDummy.DummyInputEof )
			{
				return ELzmaDummy.DummyInputEof;
			}

			if( DummySimpleIterate( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + Lzma.NumStates + parameters.State] ) )
			{
				if( UpdateRangeDummy( ref parameters, bufOutOffset ) == ELzmaDummy.DummyInputEof )
				{
					return ELzmaDummy.DummyInputEof;
				}

				if( DummySimpleIterate( ref parameters, ref Probabilities[Lzma.NumFullDistancesSize + posState + parameters.State] ) )
				{
					return ELzmaDummy.DummyRepeatShort;
				}
			}
			else
			{
				if( UpdateRangeDummy( ref parameters, bufOutOffset ) == ELzmaDummy.DummyInputEof )
				{
					return ELzmaDummy.DummyInputEof;
				}

				if( !DummySimpleIterate( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + ( Lzma.NumStates * 2u ) + parameters.State] ) )
				{
					if( DummyConsumeBit( ref parameters, ref Probabilities[LzmaDecoder.IsRepeat + ( Lzma.NumStates * 3u ) + parameters.State], bufOutOffset ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}
				}
			}

			parameters.State = Lzma.NumStates;
			probOffset = Lzma.NumFullDistancesSize + LzmaDecoder.NumStatePositionProbabilities;

			return ELzmaDummy.DummyRepeat;
		}

		private ELzmaDummy TryDummyDistance( ref CParameters parameters, int64 bufOutOffset, uint32 length )
		{
			if( parameters.State < 4u )
			{
				uint32 probability_offset = LzmaDecoder.IsRepeat + ( Lzma.NumStates * 4u ) + ( ( length < Lzma.NumLengthToPositionStates - 1u ? length : Lzma.NumLengthToPositionStates - 1u ) << Lzma.NumPositionSlotBits );
				parameters.Symbol = 1u;
				do
				{
					if( DummyDecodeTreeBit( ref parameters, ref Probabilities[probability_offset + parameters.Symbol], bufOutOffset ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}
				} while( parameters.Symbol < ( 1u << Lzma.NumPositionSlotBits ) );

				uint32 pos_slot = parameters.Symbol - ( 1u << Lzma.NumPositionSlotBits );
				if( pos_slot >= Lzma.StartPositionModelIndex )
				{
					int8 num_direct_bits = ( int8 )( ( pos_slot >> 1 ) - 1 );

					if( pos_slot < Lzma.EndPositionModelIndex )
					{
						probability_offset = ( 2u | ( pos_slot & 1u ) ) << num_direct_bits;
					}
					else
					{
						num_direct_bits -= Lzma.NumAlignmentBits;
						do
						{
							if( UpdateRangeDummy( ref parameters, bufOutOffset ) == ELzmaDummy.DummyInputEof )
							{
								return ELzmaDummy.DummyInputEof;
							}

							parameters.Range >>= 1;
							parameters.Code -= parameters.Range & ( ( ( parameters.Code - parameters.Range ) >> 31 ) - 1u );
						} while( --num_direct_bits != 0 );

						probability_offset = LzmaDecoder.IsMatchBase + LzmaDecoder.NumStatePositionProbabilities;
						num_direct_bits = Lzma.NumAlignmentBits;
					}

					parameters.Symbol = 1u;
					parameters.Offset = 1u;
					do
					{
						if( DummyDecodePositionModelBit( ref parameters, ref Probabilities[probability_offset + parameters.Symbol], bufOutOffset ) == ELzmaDummy.DummyInputEof )
						{
							return ELzmaDummy.DummyInputEof;
						}
					} while( --num_direct_bits != 0 );
				}
			}

			return ELzmaDummy.DummyOk;
		}

		private ELzmaDummy TryDummy( uint8[] buffer, int64 bufferOffset, ref int64 bufOutOffset )
		{
			ELzmaDummy result;

			CParameters dummy = Parameters;
			dummy.DataBufferBase = buffer;
			dummy.DataBufferOffset = bufferOffset;

			// Convert bufOutOffset from size to absolute position
			bufOutOffset = bufferOffset + bufOutOffset;

			while( true )
			{
				uint32 pos_state = ( ProcessedPosition & PositionMask ) << 4;

				if( UpdateRangeDummy( ref dummy, bufOutOffset ) == ELzmaDummy.DummyInputEof )
				{
					return ELzmaDummy.DummyInputEof;
				}

				if( DummySimpleIterate( ref dummy, ref Probabilities[LzmaDecoder.IsMatchBase + pos_state + dummy.State] ) )
				{
					result = TryDummyLit( ref dummy, bufOutOffset );
					if( result == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}
				}
				else
				{
					int64 prob_offset = 0;
					
					if( UpdateRangeDummy( ref dummy, bufOutOffset ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}

					if( DummySimpleIterate( ref dummy, ref Probabilities[LzmaDecoder.IsRepeat + dummy.State] ) )
					{
						dummy.State = 0u;
						prob_offset = Lzma.NumFullDistancesSize + LzmaDecoder.NumStatePositionProbabilities + LzmaDecoder.NumLengthProbabilities;
						result = ELzmaDummy.DummyMatch;
					}
					else
					{
						result = TryDummyRep( ref dummy, ref prob_offset, bufOutOffset, pos_state );
						if( result == ELzmaDummy.DummyInputEof )
						{
							return ELzmaDummy.DummyInputEof;
						}

						if( result == ELzmaDummy.DummyRepeatShort )
						{
							break;
						}
					}

					if( UpdateRangeDummy( ref dummy, bufOutOffset ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}

					uint32 limit;
					uint32 offset2;
					int64 prob_len_offset;

					if( DummySimpleIterate( ref dummy, ref Probabilities[prob_offset] ) )
					{
						prob_len_offset = prob_offset + pos_state;
						offset2 = 0u;
						limit = Lzma.LengthEncoderNumLowSymbols;
					}
					else
					{
						if( UpdateRangeDummy( ref dummy, bufOutOffset ) == ELzmaDummy.DummyInputEof )
						{
							return ELzmaDummy.DummyInputEof;
						}

						if( DummySimpleIterate( ref dummy, ref Probabilities[prob_offset + Lzma.LengthEncoderNumLowSymbols] ) )
						{
							prob_len_offset = prob_offset + pos_state + Lzma.LengthEncoderNumLowSymbols;
							offset2 = Lzma.LengthEncoderNumLowSymbols;
							limit = Lzma.LengthEncoderNumLowSymbols;
						}
						else
						{
							prob_len_offset = prob_offset + ( LzmaDecoder.MaxNumPositionStates << ( Lzma.LengthEncoderNumLowBits + 1 ) );
							offset2 = Lzma.LengthEncoderNumLowSymbols << 1;
							limit = ( uint32 )Lzma.LengthEncoderNumHighSymbols;
						}
					}

					dummy.Symbol = 1u;
					do
					{
						if( DummyDecodeTreeBit( ref dummy, ref Probabilities[prob_len_offset + dummy.Symbol], bufOutOffset ) == ELzmaDummy.DummyInputEof )
						{
							return ELzmaDummy.DummyInputEof;
						}
					} while( dummy.Symbol < limit );

					uint32 length = dummy.Symbol;
					length -= limit;
					length += offset2;

					if( TryDummyDistance( ref dummy, bufOutOffset, length ) == ELzmaDummy.DummyInputEof )
					{
						return ELzmaDummy.DummyInputEof;
					}
				}
				break;
			}

			if( UpdateRangeDummy( ref dummy, bufOutOffset ) == ELzmaDummy.DummyInputEof )
			{
				return ELzmaDummy.DummyInputEof;
			}

			bufOutOffset = dummy.DataBufferOffset - bufferOffset;
			return result;
		}

		public void InitDictAndState( bool initDict, bool initState )
		{
			RemainingLength = LzmaDecoder.MaxNormalMatchLength + 1u;
			TempBufferSize = 0u;

			if( initDict )
			{
				ProcessedPosition = 0u;
				CheckDictionarySize = 0u;
				RemainingLength = LzmaDecoder.MaxNormalMatchLength + 2u;
			}

			if( initState )
			{
				RemainingLength = LzmaDecoder.MaxNormalMatchLength + 2u;
			}
		}

		/*
		LZMA supports optional end_marker.
		So the decoder can lookahead for one additional LZMA-Symbol to check end_marker.
		That additional LZMA-Symbol can require up to LzmaRequiredInput bytes in input stream.
		When the decoder reaches dicLimit, it looks (finishMode) parameter:
		  if (finishMode == LZMA_FINISH_ANY), the decoder doesn't lookahead
		  if (finishMode != LZMA_FINISH_ANY), the decoder lookahead, if end_marker is possible for current position

		When the decoder lookahead, and the lookahead Symbol is not end_marker, we have two ways:
		  1) Strict mode (default) : the decoder returns SZ_ERROR_DATA.
		  2) The relaxed mode (alternative mode) : we could return SZ_OK, and the caller
			 must check (status) value. The caller can show the error,
			 if the end of stream is expected, and the (status) is noit
			 LzmaStatusFinishedWithMark or LzmaStatusMaybeFinishedWithoutMark.
		*/

		public SevenZipResult DecodeToDict( int64 dicLimit, uint8[] compressed, int64 compressedOffset, ref int64 compressedLength, ELzmaFinishMode finishMode, ref ELzmaStatus status )
		{
			SevenZipResult result;
			uint8[] temporary_buffer = new uint8[LzmaDecoder.LzmaRequiredInput];

			int64 in_size = compressedLength;
			compressedLength = 0;
			status = ELzmaStatus.LzmaStatusNotSpecified;

			if( RemainingLength > LzmaDecoder.MaxNormalMatchLength )
			{
				if( RemainingLength > LzmaDecoder.MaxNormalMatchLength + 2u )
				{
					return RemainingLength == LzmaDecoder.NormalMatchLengthErrorFail ? SevenZipResult.SevenZipErrorFail : SevenZipResult.SevenZipErrorData;
				}

				for( ; in_size > 0 && TempBufferSize < Lzma.LzmaPropertiesSize; compressedLength++, in_size-- )
				{
					temporary_buffer[TempBufferSize++] = compressed[compressedOffset++];
				}

				if( TempBufferSize != 0u && temporary_buffer[0] != 0u )
				{
					return SevenZipResult.SevenZipErrorData;
				}

				if( TempBufferSize < Lzma.LzmaPropertiesSize )
				{
					status = ELzmaStatus.LzmaStatusNeedsMoreInput;
					return SevenZipResult.SevenZipOK;
				}

				Parameters.Code =
					( ( uint32 )( temporary_buffer[1] ) << 24 )
					| ( ( uint32 )( temporary_buffer[2] ) << 16 )
					| ( ( uint32 )( temporary_buffer[3] ) << 8 )
					| ( ( uint32 )( temporary_buffer[4] ) << 0 );

				if( ( CheckDictionarySize == 0u ) && ( ProcessedPosition == 0u ) && ( Parameters.Code >= LzmaDecoder.BadRepeatCode ) )
				{
					return SevenZipResult.SevenZipErrorData;
				}

				Parameters.Range = UInt32.MaxValue;
				TempBufferSize = 0u;

				if( RemainingLength > LzmaDecoder.MaxNormalMatchLength + 1u )
				{
					uint32 num_probs = LzmaDecoder.IsRepeat + ( Lzma.NumStates * 4u ) + ( Lzma.NumLengthToPositionStates << Lzma.NumPositionSlotBits ) 
					                   + ( Lzma.LiteralSize << ( DecoderProperties.LiteralContextBits + DecoderProperties.LiteralPositionBits ) );
					Array.Fill( Probabilities, ( CProbability )( Lzma.BitModelTableSize >> 1 ), 0, ( int32 )num_probs );

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
			
				if( RemainingLength == LzmaDecoder.MaxNormalMatchLength )
				{
					if( Parameters.Code != 0u )
					{
						return SevenZipResult.SevenZipErrorData;
					}

					status = ELzmaStatus.LzmaStatusFinishedWithMark;
					return SevenZipResult.SevenZipOK;
				}

				WriteRemaining( dicLimit );

				bool check_end_mark = false;

				if( DictionaryPosition >= dicLimit )
				{
					if( RemainingLength == 0u && Parameters.Code == 0u )
					{
						status = ELzmaStatus.LzmaStatusMaybeFinishedWithoutMark;
						return SevenZipResult.SevenZipOK;
					}

					if( finishMode == ELzmaFinishMode.LzmaFinishModeAny )
					{
						status = ELzmaStatus.LzmaStatusNotFinished;
						return SevenZipResult.SevenZipOK;
					}

					if( RemainingLength != 0u )
					{
						status = ELzmaStatus.LzmaStatusNotFinished;
						// for strict mode
						return SevenZipResult.SevenZipErrorData;
					}

					check_end_mark = true;
				}

				int32 dummy_processed = -1;
				if( TempBufferSize == 0 )
				{
					int64 buf_limit_offset;

					// In the first branch (TempBufferSize == 0):
					if( in_size < LzmaDecoder.LzmaRequiredInput || check_end_mark )
					{
						int64 buf_out_offset = in_size;

						ELzmaDummy dummy_result = TryDummy( compressed, compressedOffset, ref buf_out_offset );

						if( dummy_result == ELzmaDummy.DummyInputEof )
						{
							if( in_size >= LzmaDecoder.LzmaRequiredInput )
							{
								break;
							}

							compressedLength += in_size;
							TempBufferSize = ( uint32 )( in_size );

							Array.Copy( compressed, compressedOffset, temporary_buffer, 0, in_size );

							status = ELzmaStatus.LzmaStatusNeedsMoreInput;
							return SevenZipResult.SevenZipOK;
						}

						dummy_processed = ( int32 )( buf_out_offset );
						if( dummy_processed > LzmaDecoder.LzmaRequiredInput )
						{
							break;
						}

						if( check_end_mark && dummy_result != ELzmaDummy.DummyMatch )
						{
							uint32 unsigned_dummy_processed = ( uint32 )dummy_processed;
							compressedLength += unsigned_dummy_processed;
							TempBufferSize = unsigned_dummy_processed;
							Array.Copy( compressed, compressedOffset, temporary_buffer, 0, unsigned_dummy_processed );

							status = ELzmaStatus.LzmaStatusNotFinished;
							return SevenZipResult.SevenZipErrorData; // for strict mode
						}

						buf_limit_offset = 0;
						// we will decode only one iteration
					}
					else
					{
						buf_limit_offset = in_size - LzmaDecoder.LzmaRequiredInput;
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
					else if( ( uint32 )( dummy_processed ) != processed )
					{
						break;
					}

					compressedOffset += processed;
					in_size -= processed;
					compressedLength += processed;

					if( result != SevenZipResult.SevenZipOK )
					{
						RemainingLength = LzmaDecoder.NormalMatchLengthErrorData;
						return SevenZipResult.SevenZipErrorData;
					}

					continue;
				}

				// we have some data in (dec1->tempBuf)
				// in strict mode: TempBufferSize is not enough for one Symbol decoding.
				// in relaxed mode: TempBufferSize not larger than required for one Symbol decoding.

				uint32 remaining = TempBufferSize;
				uint32 ahead = 0u;
				dummy_processed = -1;

				while( ( remaining < LzmaDecoder.LzmaRequiredInput ) && ( ahead < in_size ) )
				{
					temporary_buffer[remaining++] = compressed[compressedOffset + ahead++];
				}

				// ahead - the size of new data copied from (compressed) to (dec1->tempBuf)
				// rem   - the size of temp buffer including new data from (compressed)

				if( ( remaining < LzmaDecoder.LzmaRequiredInput ) || check_end_mark )
				{
					int64 buf_out_offset = remaining;

					ELzmaDummy dummy_result = TryDummy( temporary_buffer, 0, ref buf_out_offset );

					if( dummy_result == ELzmaDummy.DummyInputEof )
					{
						if( remaining >= LzmaDecoder.LzmaRequiredInput )
						{
							break;
						}

						TempBufferSize = remaining;
						compressedLength += ( int64 )ahead;
						status = ELzmaStatus.LzmaStatusNeedsMoreInput;
						return SevenZipResult.SevenZipOK;
					}

					dummy_processed = ( int32 )( buf_out_offset );

					if( ( uint32 )( dummy_processed ) < TempBufferSize )
					{
						break;
					}

					if( check_end_mark && dummy_result != ELzmaDummy.DummyMatch )
					{
						compressedLength += ( uint32 )( dummy_processed ) - TempBufferSize;
						TempBufferSize = ( uint32 )( dummy_processed );
						status = ELzmaStatus.LzmaStatusNotFinished;
						// for strict mode
						return SevenZipResult.SevenZipErrorData;
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
					if( processed > LzmaDecoder.LzmaRequiredInput )
					{
						break;
					}

					if( processed < remaining )
					{
						break;
					}
				}
				else if( ( uint32 )( dummy_processed ) != processed )
				{
					break;
				}

				processed -= remaining;

				compressedOffset += processed;
				in_size -= processed;
				compressedLength += processed;
				TempBufferSize = 0u;

				if( result != SevenZipResult.SevenZipOK )
				{
					RemainingLength = LzmaDecoder.NormalMatchLengthErrorData;
					return SevenZipResult.SevenZipErrorData;
				}
			}

			/* Some unexpected error: internal error of Code, memory corruption or hardware failure */
			RemainingLength = LzmaDecoder.NormalMatchLengthErrorFail;
			return SevenZipResult.SevenZipErrorFail;
		}

		/** Decode the Lzma 5 byte array to dictionary size and decompression parameters. */
		public SevenZipResult DecodeProperties( uint8[] decoderProperties, uint32 propsSize )
		{
			if( propsSize < Lzma.LzmaPropertiesSize )
			{
				return SevenZipResult.SevenZipErrorUnsupported;
			}

			uint32 dict_size = ( ( uint32 )( decoderProperties[1] ) << 0 )
			   | ( ( uint32 )( decoderProperties[2] ) << 8 )
			   | ( ( uint32 )( decoderProperties[3] ) << 16 )
			   | ( ( uint32 )( decoderProperties[4] ) << 24 );

			DecoderProperties.DictionarySize = Math.Clamp( dict_size, Lzma.MinDictionarySize, Lzma.MaxDictionarySize );

			uint8 encoded_parameters = decoderProperties[0];
			if( encoded_parameters >= ( 9u * 5u * 5u ) )
			{
				return SevenZipResult.SevenZipErrorUnsupported;
			}

			DecoderProperties.LiteralContextBits = ( uint8 )( encoded_parameters % 9u );
			encoded_parameters /= 9;
			DecoderProperties.PositionBits = ( uint8 )( encoded_parameters / 5u );
			DecoderProperties.LiteralPositionBits = ( uint8 )( encoded_parameters % 5u );

			PositionMask = ( 1u << DecoderProperties.PositionBits ) - 1u;
			LiteralMask = ( 256u << DecoderProperties.LiteralPositionBits ) - ( 256u >> DecoderProperties.LiteralContextBits );

			return SevenZipResult.SevenZipOK;
		}

		public void FreeProbabilities()
		{
			Probabilities = [];
		}

		public SevenZipResult AllocateProbabilities()
		{
			uint32 num_probabilities = LzmaDecoder.IsRepeat + ( Lzma.NumStates * 4u ) + ( Lzma.NumLengthToPositionStates << Lzma.NumPositionSlotBits )
			                           + ( Lzma.LiteralSize << ( DecoderProperties.LiteralContextBits + DecoderProperties.LiteralPositionBits ) );
			if( ( Probabilities == null ) || ( num_probabilities != NumProbabilities ) )
			{
				FreeProbabilities();

				Probabilities = new CProbability[num_probabilities];
				NumProbabilities = num_probabilities;
			}

			return SevenZipResult.SevenZipOK;
		}

		public static SevenZipResult Lzma1Decode( uint8[] decompressed, ref int64 decompressedLength, uint8[] compressed, ref int64 compressedLength, uint8[] propData, uint32 propSize, ELzmaFinishMode finishMode, out ELzmaStatus status )
		{
			Lzma1Dec dec1 = new Lzma1Dec( decompressed );
			int64 out_size = decompressedLength;
			int64 in_size = compressedLength;

			decompressedLength = 0;
			compressedLength = 0;
			status = ELzmaStatus.LzmaStatusNotSpecified;

			SevenZipResult result = dec1.DecodeProperties( propData, propSize );
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			result = dec1.AllocateProbabilities();
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			dec1.DictionaryBufferSize = out_size;
			dec1.InitDictAndState( true, true );

			compressedLength = in_size;
			result = dec1.DecodeToDict( out_size, compressed, 0, ref compressedLength, finishMode, ref status );
			decompressedLength = dec1.DictionaryPosition;
			if( result == SevenZipResult.SevenZipOK && status == ELzmaStatus.LzmaStatusNeedsMoreInput )
			{
				result = SevenZipResult.SevenZipErrorInputEof;
			}

			dec1.FreeProbabilities();
			return result;
		}
	}
}
