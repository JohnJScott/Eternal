// Copyright Eternal Developments, LLC. All Rights Reserved.

using System.Diagnostics;

namespace Eternal.LZMA2SimpleCS.CS
{
	using CExtra = UInt16;
	using CProbability = UInt16;
	using CProbPrice = UInt32;
	using CState = UInt16;

	using int16 = Int16;
	using int32 = Int32;
	using int64 = Int64;
	using int8 = SByte;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	public class LzmaEncoder
	{
		LzmaEncoder()
		{
		}

		public static readonly int8 NumMoveReducingBits = 4;
		public static readonly int8 NumBitPriceShiftBits = 4;

		public static readonly uint8 EncodeStateStart = 0;
		public static readonly uint8 EncodeStateLiteralAfterMatch = 4;
		public static readonly uint8 EncodeStateLiteralAfterRepeat = 5;
		public static readonly uint8 EncodeStateMatchAfterLiteral = 7;
		public static readonly uint8 EncodeStateRepeatAfterLiteral = 8;

		public static readonly uint32 InfinityPrice = 1u << 30;
		public static readonly int64 RangeEncoderBufferSize = 1 << 16;

		public static readonly uint32 NumOptimals = 1u << 11;
		public static readonly uint32 PackReserveSize = NumOptimals << 3;
		public static readonly int32 RepeatLengthCount = 64;
		public static readonly uint32 MarkLiteral = UInt32.MaxValue;

		public static readonly uint32 MaxDictionarySizeBits = 32;
		public static readonly uint32 DistanceTableSize = MaxDictionarySizeBits << 1;
	}

	public partial class CLzmaEncoderProperties
	{
		public uint32 GetDictionarySize()
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

			if( dictionary_size > EstimatedSourceDataSize )
			{
				dictionary_size = ( uint32 )( Math.Clamp( dictionary_size, Lzma.MinDictionarySize, EstimatedSourceDataSize ) );
			}

			dictionary_size = Math.Clamp( dictionary_size, Lzma.MinDictionarySize, Lzma.MaxDictionarySize );
			return dictionary_size;
		}

		public virtual SevenZipResult Normalize()
		{
			CompressionLevel = Math.Clamp( CompressionLevel, ( uint8 )0, ( uint8 )9 );
			DictionarySize = GetDictionarySize();

			LiteralContextBits = Math.Clamp( LiteralContextBits, ( uint8 )0, Lzma.MaxLiteralContextBits );
			LiteralPositionBits = Math.Clamp( LiteralPositionBits, ( uint8 )0, Lzma.MaxLiteralPositionBits );
			PositionBits = Math.Clamp( PositionBits, ( uint8 )0, Lzma.MaxPositionBits );

			if( FastBytes < 0 )
			{
				FastBytes = ( int16 )( ( CompressionLevel < 7 ) ? 32 : 64 );
			}

			return SevenZipResult.SevenZipOK;
		}
	}

	/**
	 * The class containing the parameters used for compression.
	 */
	public class CLzma1EncoderProperties
		: CLzmaEncoderProperties
	{
		public override SevenZipResult Normalize()
		{
			base.Normalize();

			FastBytes = Math.Clamp( FastBytes, Lzma.Lzma1MinMatchLength, Lzma.MaxMatchLength );
			if( MatchCycles == 0u )
			{
				MatchCycles = 16u + ( ( ( uint32 )FastBytes ) >> 1 );
				if( CompressionLevel < 5 )
				{
					MatchCycles >>= 1;
				}
			}

			return SevenZipResult.SevenZipOK;
		}
	}

	internal class Lzma1Enc
	{
		private static readonly CProbPrice[] ProbabilityPrices =
		[
			0x80, 0x67, 0x5B, 0x54, 0x4E, 0x49, 0x45, 0x42, 0x3F, 0x3D, 0x3A, 0x38, 0x36, 0x34, 0x33, 0x31, 0x30, 0x2E, 0x2D, 0x2C, 0x2B, 0x2A, 0x29, 0x28, 0x27, 0x26, 0x25, 0x24, 0x23, 0x22, 0x22, 0x21,
			0x20, 0x1F, 0x1F, 0x1E, 0x1D, 0x1D, 0x1C, 0x1C, 0x1B, 0x1A, 0x1A, 0x19, 0x19, 0x18, 0x18, 0x17, 0x17, 0x16, 0x16, 0x16, 0x15, 0x15, 0x14, 0x14, 0x13, 0x13, 0x13, 0x12, 0x12, 0x11, 0x11, 0x11,
			0x10, 0x10, 0x10, 0x0F, 0x0F, 0x0F, 0x0E, 0x0E, 0x0E, 0x0D, 0x0D, 0x0D, 0x0C, 0x0C, 0x0C, 0x0B, 0x0B, 0x0B, 0x0B, 0x0A, 0x0A, 0x0A, 0x0A, 0x09, 0x09, 0x09, 0x09, 0x08, 0x08, 0x08, 0x08, 0x07,
			0x07, 0x07, 0x07, 0x06, 0x06, 0x06, 0x06, 0x05, 0x05, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x01, 0x01
		];

		private readonly ProgressInterface? Progress = null;
		private readonly CMatchFinder MatchFinder;
		private readonly CRangeEncoder RangeCoder = new CRangeEncoder();

		private readonly int32 LiteralContextBits;
		private readonly int32 LiteralPositionBits;
		private readonly int32 PositionBits;
		private readonly uint32 FastBytes;
		private readonly uint32 DictionarySize;
		private readonly uint32[] Repeats = new uint32[Lzma.NumRepeats];

		private readonly bool FastMode;

		private CProbability[] LiteralProbabilities = [];
		private uint32[] NewRepeats = [];
		private COptimal[] Optimals = [];

		private int64 NowPos64;

		private uint32 OptimalCurrent;
		private uint32 OptimalEnd;
		private uint32 LongestMatchLength;
		private uint32 NumPairs;
		private uint32 NumAvail;
		private uint32 State;
		private uint32 AdditionalOffset;
		private uint32 LiteralMask;
		private uint32 PositionMask;
		private uint32 BackRes;
		private int32 TotalLiteralBits;

		private uint32 MatchPriceCount;
		private int32 RepeatLenEncCounter;
		private uint32 DistanceTableSize;

		private uint32 Position;
		private uint32 PositionState;
		private uint32 NewState;
		private uint32 NumAvailOther;
		private uint32 MatchPrice;
		private uint32 RepeatMatchPrice;

		// we want {len, dist} pairs to be 8-bytes aligned in matches array
		private readonly uint32[] Matches = new uint32[( Lzma.MaxMatchLength << 1 ) + 2];

		// we want 8-bytes alignment here
		private readonly uint32[] AlignPrices = new uint32[Lzma.AlignmentTableSize];

		// Array of NumLengthToPositionStates arrays of NumFullDistancesSize prices
		private readonly uint32[][] DistancesPrices =
		[
			new uint32[Lzma.NumFullDistancesSize],
			new uint32[Lzma.NumFullDistancesSize],
			new uint32[Lzma.NumFullDistancesSize],
			new uint32[Lzma.NumFullDistancesSize]
		];

		// Array of NumLengthToPositionStates arrays of DIST_TABLE_SIZE_MAX prices
		private readonly uint32[][] PositionSlotPrices =
		[
			new uint32[LzmaEncoder.DistanceTableSize],
			new uint32[LzmaEncoder.DistanceTableSize],
			new uint32[LzmaEncoder.DistanceTableSize],
			new uint32[LzmaEncoder.DistanceTableSize]
		];

		// Array of NumLengthToPositionStates arrays of 2^NumPositionSlotBits probabilities
		private readonly CProbability[][] PositionSlotEncoder =
		[
			new CProbability[1 << Lzma.NumPositionSlotBits],
			new CProbability[1 << Lzma.NumPositionSlotBits],
			new CProbability[1 << Lzma.NumPositionSlotBits],
			new CProbability[1 << Lzma.NumPositionSlotBits]
		];

		// Array of NUM_STATES arrays of MaxPositionBitsStates probabilities
		private readonly CProbability[][] IsMatch =
		[
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates]
		];

		// Array of NUM_STATES arrays of MaxPositionBitsStates probabilities
		private readonly CProbability[][] IsRep0Long =
		[
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates],
			new CProbability[Lzma.MaxPositionBitsStates]
		];

		private readonly CProbability[] PositionAlignEncoder = new CProbability[1 << Lzma.NumAlignmentBits];
		private readonly CProbability[] IsRep = new CProbability[Lzma.NumStates];
		private readonly CProbability[] IsRepG0 = new CProbability[Lzma.NumStates];
		private readonly CProbability[] IsRepG1 = new CProbability[Lzma.NumStates];
		private readonly CProbability[] IsRepG2 = new CProbability[Lzma.NumStates];
		private readonly CProbability[] PositionEncoders = new CProbability[Lzma.NumFullDistancesSize];

		private readonly CLengthPriceEncoder LenEnc = new CLengthPriceEncoder();
		private readonly CLengthPriceEncoder RepeatLenEnc = new CLengthPriceEncoder();

		private CLengthEncoder LengthProbabilities = new CLengthEncoder();
		private CLengthEncoder RepeatLengthProbabilities = new CLengthEncoder();

		public readonly CSaveState SavedState = new CSaveState();

		private bool Finished;
		private bool NeedInit;
		private bool WriteEndMark;
		private SevenZipResult Result;

		// The lookup table is 5% faster than calculating the value each time, but requires the 16KB table.
		// Function defined here so the compiler can inline it.
		static uint8 GetBlockSizeFromPosition( uint32 position )
		{
#if USE_BLOCK_SIZE_LOOKUP
			return BlockSizeLookup[position];
#else
			if( position < 5u )
			{
				return ( uint8 )( position & 0xff );
			}

			int32 msb = 1;
			uint32 lookup = position >> 2;
			while( lookup != 0u )
			{
				lookup >>= 1;
				msb++;
			}

			uint8 slot = ( uint8 )( ( ( msb << 1 ) + ( position >= ( 3u << ( msb - 1 ) ) ? 1u : 0u ) ) & 0xff );
			return slot;
#endif
		}

		public class CSaveState
		{
			public CProbability[] LiteralProbabilities = [];

			private uint32 State;
			private uint32[] Repeats = new uint32[Lzma.NumRepeats];

			private CProbability[] PositionAlignEncoder = new CProbability[1 << Lzma.NumAlignmentBits];
			private CProbability[] IsRep = new CProbability[Lzma.NumStates];
			private CProbability[] IsRepG0 = new CProbability[Lzma.NumStates];
			private CProbability[] IsRepG1 = new CProbability[Lzma.NumStates];
			private CProbability[] IsRepG2 = new CProbability[Lzma.NumStates];

			// Array of NUM_STATES arrays of MaxPositionBitsStates probabilities
			private CProbability[][] IsMatch =
			[
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates]
			];

			// Array of NUM_STATES arrays of MaxPositionBitsStates probabilities
			private CProbability[][] IsRep0Long =
			[
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates],
				new CProbability[Lzma.MaxPositionBitsStates]
			];

			private CProbability[][] PositionSlotEncoder =
			[
				new CProbability[1 << Lzma.NumPositionSlotBits],
				new CProbability[1 << Lzma.NumPositionSlotBits],
				new CProbability[1 << Lzma.NumPositionSlotBits],
				new CProbability[1 << Lzma.NumPositionSlotBits]
			];

			private CProbability[] PositionEncoders = new CProbability[Lzma.NumFullDistancesSize];

			private CLengthEncoder LengthProbabilities = new CLengthEncoder();
			private CLengthEncoder RepeatLengthProbabilities = new CLengthEncoder();


			public void SaveState( Lzma1Enc enc1 )
			{
				State = enc1.State;
				LengthProbabilities = enc1.LengthProbabilities;
				RepeatLengthProbabilities = enc1.RepeatLengthProbabilities;

				Array.Copy( enc1.Repeats, Repeats, Lzma.NumRepeats );
				Array.Copy( enc1.PositionAlignEncoder, PositionAlignEncoder, 1 << Lzma.NumAlignmentBits );
				Array.Copy( enc1.IsRep, IsRep, Lzma.NumStates );
				Array.Copy( enc1.IsRepG0, IsRepG0, Lzma.NumStates );
				Array.Copy( enc1.IsRepG1, IsRepG1, Lzma.NumStates );
				Array.Copy( enc1.IsRepG2, IsRepG2, Lzma.NumStates );

				for( int32 array_index = 0; array_index < Lzma.NumStates; array_index++ )
				{
					Array.Copy( enc1.IsMatch[array_index], IsMatch[array_index], Lzma.MaxPositionBitsStates );
					Array.Copy( enc1.IsRep0Long[array_index], IsRep0Long[array_index], Lzma.MaxPositionBitsStates );
				}

				for( int32 array_index = 0; array_index < Lzma.NumLengthToPositionStates; array_index++ )
				{
					Array.Copy( enc1.PositionSlotEncoder[array_index], PositionSlotEncoder[array_index], 1 << Lzma.NumPositionSlotBits );
				}

				Array.Copy( enc1.PositionEncoders, PositionEncoders, Lzma.NumLengthToPositionStates );

				uint32 prob_size = 0x300u << enc1.TotalLiteralBits;
				LiteralProbabilities = new CProbability[prob_size];
				Array.Copy( enc1.LiteralProbabilities, LiteralProbabilities, prob_size );
			}

			public void RestoreState( Lzma1Enc enc1 )
			{
				enc1.State = State;
				enc1.LengthProbabilities = LengthProbabilities;
				enc1.RepeatLengthProbabilities = RepeatLengthProbabilities;

				Array.Copy( Repeats, enc1.Repeats, Lzma.NumRepeats );
				Array.Copy( PositionAlignEncoder, enc1.PositionAlignEncoder, 1 << Lzma.NumAlignmentBits );
				Array.Copy( IsRep, enc1.IsRep, Lzma.NumStates );
				Array.Copy( IsRepG0, enc1.IsRepG0, Lzma.NumStates );
				Array.Copy( IsRepG1, enc1.IsRepG1, Lzma.NumStates );
				Array.Copy( IsRepG2, enc1.IsRepG2, Lzma.NumStates );

				for( int32 array_index = 0; array_index < Lzma.NumStates; array_index++ )
				{
					Array.Copy( IsMatch[array_index], enc1.IsMatch[array_index], Lzma.MaxPositionBitsStates );
					Array.Copy( IsRep0Long[array_index], enc1.IsRep0Long[array_index], Lzma.MaxPositionBitsStates );
				}

				for( int32 array_index = 0; array_index < Lzma.NumLengthToPositionStates; array_index++ )
				{
					Array.Copy( PositionSlotEncoder[array_index], enc1.PositionSlotEncoder[array_index], 1 << Lzma.NumPositionSlotBits );
				}

				Array.Copy( PositionEncoders, enc1.PositionEncoders, Lzma.NumLengthToPositionStates );

				uint32 prob_size = 0x300u << enc1.TotalLiteralBits;
				Array.Copy( LiteralProbabilities, enc1.LiteralProbabilities, prob_size );
			}
		};

		private class COptimal
		{
			public uint32 Price;
			public CState State;

			public CExtra Extra;

			// 0   : normal
			// 1   : LIT : MATCH
			// > 1 : MATCH (extra-1) : LIT : REP0 (len)
			public uint32 Length;
			public uint32 Distance;
			public uint32[] Repeats = new uint32[Lzma.NumRepeats];
		}

		private class CheckedOutStreamInterface
			: OutStreamInterface
		{
			public CheckedOutStreamInterface( uint8[] dest, int64 destOffset, int64 destSize )
			{
				WorkBuffer = dest;
				WorkBufferOffset = destOffset;
				WorkBufferSize = destOffset + destSize;
				Overflow = false;
			}

			public override int64 Write( uint8[] data, int64 offset, int64 blockSize )
			{
				int64 remaining = WorkBufferSize - WorkBufferOffset;
				if( remaining < blockSize )
				{
					blockSize = remaining;
					Overflow = true;
				}

				if( blockSize != 0 )
				{
					Array.Copy( data, offset, WorkBuffer, WorkBufferOffset, blockSize );
					WorkBufferOffset += blockSize;
				}

				return blockSize;
			}

			public bool Overflow = false;
			public int64 WorkBufferOffset = 0;

			private readonly uint8[] WorkBuffer;
			private readonly int64 WorkBufferSize = 0;
		};

		private class CLengthEncoder
		{
			public CProbability[] Low = new CProbability[Lzma.MaxPositionBitsStates << ( Lzma.LengthEncoderNumLowBits + 1 )];
			public CProbability[] High = new CProbability[Lzma.LengthEncoderNumHighSymbols];

			public void Init()
			{
				Array.Fill<CProbability>( Low, Lzma.InitProbabilityValue );
				Array.Fill<CProbability>( High, Lzma.InitProbabilityValue );
			}
		}

		private class CLengthPriceEncoder
		{
			public uint32 TableSize;
			public uint32[,] Prices = new uint32[Lzma.MaxPositionBitsStates, ( Lzma.LengthEncoderNumLowSymbols << 1 ) + Lzma.LengthEncoderNumHighSymbols];
		};

		private class CRangeEncoder
		{
			public OutStreamInterface? outStream = null;

			public int64 Low = 0;
			public int64 Processed = 0;
			public int64 BufferOffset = 0;
			public int64 CacheSize = 0;

			public uint32 Range = 0;

			public SevenZipResult Result = SevenZipResult.SevenZipOK;

			private uint8[] RangeEncoderBuffer = new uint8[LzmaEncoder.RangeEncoderBufferSize];
			private uint32 Cache = 0;

			public void Init()
			{
				Range = UInt32.MaxValue;
				Cache = 0u;
				Low = 0u;
				CacheSize = 0u;
				BufferOffset = 0u;
				Processed = 0u;
				Result = SevenZipResult.SevenZipOK;
			}

			public void FlushStream()
			{
				if( Result == SevenZipResult.SevenZipOK )
				{
					if( BufferOffset != outStream!.Write( RangeEncoderBuffer, 0, BufferOffset ) )
					{
						Result = SevenZipResult.SevenZipErrorWrite;
					}
				}

				Processed += BufferOffset;
				BufferOffset = 0u;
			}


			public void CheckFlush()
			{
				if( BufferOffset >= LzmaEncoder.RangeEncoderBufferSize )
				{
					FlushStream();
				}
			}

			public void ShiftLow()
			{
				uint32 low32 = ( uint32 )( Low & uint32.MaxValue );
				uint32 high32 = ( uint32 )( ( Low >> 32 ) & UInt32.MaxValue );
				Low = low32 << 8;
				if( low32 < 0xFF000000u || high32 != 0u )
				{
					RangeEncoderBuffer[BufferOffset++] = ( uint8 )( ( Cache + high32 ) & 0xff );
					Cache = low32 >> 24;

					CheckFlush();

					if( CacheSize != 0u )
					{
						high32 += 255u;

						do
						{
							RangeEncoderBuffer[BufferOffset++] = ( uint8 )( high32 & 0xff );
							CheckFlush();

						} while( --CacheSize != 0u );
					}
				}
				else
				{
					CacheSize++;
				}
			}

			public void FlushData()
			{
				for( int32 i = 0; i < 5; i++ )
				{
					ShiftLow();
				}
			}

			public void UpdateRange()
			{
				if( Range < Lzma.MaxRangeValue )
				{
					Range <<= 8;
					ShiftLow();
				}
			}

			public void UpdateNewBoundUpdateRange( uint32 bound )
			{
				Range -= bound;
				Low += bound;

				UpdateRange();
			}

			public void SetNewBoundUpdateRange( uint32 bound )
			{
				Range = bound;
				UpdateRange();
			}

			public uint32 CalcNewBound( uint32 probability )
			{
				return ( Range >> Lzma.NumBitModelTotalBits ) * probability;
			}

			public void EncodeBit0( ref CProbability probability )
			{
				Range = CalcNewBound( probability );
				probability = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );

				UpdateRange();
			}

			public void Iterate( CProbability[] probabilities, uint32 offset, uint32 bit )
			{
				CProbability probability = probabilities[offset];
				uint32 new_bound = CalcNewBound( probability );

				if( bit == 0u )
				{
					/* bit == 0: encode lower half of range */
					Range = new_bound;
					probabilities[offset] = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );
				}
				else
				{
					/* bit == 1: encode upper half of range */
					Range -= new_bound;
					Low += new_bound;
					probabilities[offset] = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );
				}

				UpdateRange();
			}

			public void Encode( CProbability[] probabilities, uint32 arrayOffset, uint32 symbol )
			{
				symbol |= 0x100u;
				do
				{
					uint32 le_offset = symbol >> 8;
					uint32 bit = ( symbol >> 7 ) & 1u;
					symbol <<= 1;

					Iterate( probabilities, arrayOffset + le_offset, bit );

				} while( symbol < 0x10000u );
			}

			public void EncodeMatched( CProbability[] probabilities, uint32 arrayOffset, uint32 symbol, uint32 matchByte )
			{
				uint32 offset = 0x100u;
				symbol |= 0x100u;
				do
				{
					matchByte <<= 1;
					uint32 le_offset = offset + ( matchByte & offset ) + ( symbol >> 8 );
					uint32 bit = ( symbol >> 7 ) & 1u;
					symbol <<= 1;
					offset &= ~( matchByte ^ symbol );

					Iterate( probabilities, arrayOffset + le_offset, bit );

				} while( symbol < 0x10000u );
			}

			public void ReverseEncode( CProbability[] probabilities, uint32 arrayOffset, int8 numBits, uint32 symbol )
			{
				uint32 offset = 1u;
				do
				{
					uint32 bit = symbol & 1u;
					symbol >>= 1;

					Iterate( probabilities, arrayOffset + offset, bit );

					offset <<= 1;
					offset |= bit;
				} while( --numBits != 0u );
			}

			public void LengthEncode( CLengthEncoder le, uint32 symbol, uint32 posState )
			{
				uint32 le_offset = 0u;
				CProbability probability = le.Low[le_offset];
				uint32 new_bound = CalcNewBound( probability );

				// Check if symbol is in low range (0-7)
				if( symbol >= Lzma.LengthEncoderNumLowSymbols )
				{
					// Update probability and move to mid range
					le.Low[le_offset] = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );
					UpdateNewBoundUpdateRange( new_bound );

					le_offset += Lzma.LengthEncoderNumLowSymbols;

					probability = le.Low[le_offset];
					new_bound = CalcNewBound( probability );

					// Check if symbol is in high range (16+)
					if( symbol >= ( Lzma.LengthEncoderNumLowSymbols << 1 ) )
					{
						le.Low[le_offset] = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );
						UpdateNewBoundUpdateRange( new_bound );

						Encode( le.High, 0, symbol - ( Lzma.LengthEncoderNumLowSymbols << 1 ) );
						return;
					}

					// Symbol is in mid range (8-15)
					symbol -= Lzma.LengthEncoderNumLowSymbols;
				}

				// Encode symbol in low/mid range
				SetNewBoundUpdateRange( new_bound );
				le.Low[le_offset] = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );

				// Encode 3 bits using binary tree
				le_offset += posState << ( 1 + Lzma.LengthEncoderNumLowBits );
				uint32 offset = 1u;

				for( int32 i = 0; i < 3; i++ )
				{
					uint32 bit = ( symbol >> ( 2 - i ) ) & 1u;
					Iterate( le.Low, le_offset + offset, bit );
					offset = ( offset << 1 ) + bit;
				}
			}

			public void IterateEndMark( ref CProbability probability )
			{
				uint32 new_bound = CalcNewBound( probability );

				Range -= new_bound;
				Low += new_bound;

				UpdateRange();
				probability = ( CProbability )( probability - ( probability >> Lzma.NumMoveBits ) );
			}

			// Helper function to encode a tree symbol with all bits set to 1
			public void EncodeTreeAllOnes( CProbability[] probabilities, int8 numBits )
			{
				uint32 offset = 1u;
				for( int8 i = 0; i < numBits; i++ )
				{
					IterateEndMark( ref probabilities[offset] );
					offset = ( offset << 1 ) + 1u;
				}
			}
		}

		public Lzma1Enc( CLzmaEncoderProperties encoderProperties, ProgressInterface? progress )
		{
			Progress = progress;
			DictionarySize = encoderProperties.DictionarySize;
			FastBytes = ( uint32 )encoderProperties.FastBytes;

			LiteralContextBits = encoderProperties.LiteralContextBits;
			LiteralPositionBits = encoderProperties.LiteralPositionBits;
			PositionBits = encoderProperties.PositionBits;
			FastMode = ( encoderProperties.CompressionLevel < 5 );

			bool use_binary_tree = ( encoderProperties.CompressionLevel >= 5 );
			MatchFinder = CMatchFinder.CreateMatchFinder( use_binary_tree );

			MatchFinder.CutValue = encoderProperties.MatchCycles;
			WriteEndMark = encoderProperties.WriteEndMark;
		}

		~Lzma1Enc()
		{
			FreeLits();
		}

		public void SetDataSize( int64 expectedDataSize )
		{
			MatchFinder.ExpectedDataSize = expectedDataSize;
		}

		private uint32 GetPrice( uint32 literalContext, uint32 symbol )
		{
			uint32 base_offset = 3u * ( literalContext << LiteralContextBits );

			uint32 price = 0u;
			symbol |= 0x100u;
			do
			{
				uint32 bit = symbol & 1u;
				symbol >>= 1;
				uint32 xor_value = ( 0u - bit ) & Lzma.BitModelTableMask;
				price += ProbabilityPrices[( LiteralProbabilities[base_offset + symbol] ^ xor_value ) >> LzmaEncoder.NumMoveReducingBits];
			} while( symbol >= 2u );

			return price;
		}

		private uint32 MatchedGetPrice( uint32 literalContext, uint32 symbol, uint32 matchByte )
		{
			uint32 base_offset = 3u * ( literalContext << LiteralContextBits );

			uint32 price = 0u;
			uint32 offset = 0x100u;
			symbol |= 0x100u;
			do
			{
				matchByte <<= 1;
				uint32 xor_value = ( 0u - ( ( symbol >> 7 ) & 1u ) ) & Lzma.BitModelTableMask;
				price += ProbabilityPrices[( LiteralProbabilities[base_offset + offset + ( matchByte & offset ) + ( symbol >> 8 )] ^ xor_value ) >> LzmaEncoder.NumMoveReducingBits];
				symbol <<= 1;
				offset &= ~( matchByte ^ symbol );
			} while( symbol < 0x10000u );

			return price;
		}

		private void SetPrices( CProbability[] baseArray, uint32 baseOffset, uint32 startPrice, ref uint32[,] prices, uint32 posState, uint32 priceOffset )
		{
			for( uint32 i = 0u; i < 8u; i += 2u )
			{
				uint32 price = startPrice;
				uint32 xor1 = ( 0u - ( i >> 2 ) ) & Lzma.BitModelTableMask;
				price += ProbabilityPrices[( baseArray[baseOffset + 1] ^ xor1 ) >> LzmaEncoder.NumMoveReducingBits];
				uint32 xor2 = ( 0u - ( ( i >> 1 ) & 1u ) ) & Lzma.BitModelTableMask;
				price += ProbabilityPrices[( ( baseArray[baseOffset + 2 + ( i >> 2 )] ) ^ xor2 ) >> LzmaEncoder.NumMoveReducingBits];
				uint32 probability = baseArray[baseOffset + 4 + ( i >> 1 )];
				prices[posState, priceOffset + i] = price + ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
				prices[posState, priceOffset + i + 1] = price + ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
			}
		}

		private void UpdateTables( CLengthPriceEncoder lpe, uint32 numPosStates, CLengthEncoder le )
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---UpdateTables" );
			uint32 probability = le.Low[0];
			uint32 b = ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
			uint32 a = ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
			uint32 c = b + ProbabilityPrices[le.Low[Lzma.LengthEncoderNumLowSymbols] >> LzmaEncoder.NumMoveReducingBits];
			for( uint32 pos_state = 0u; pos_state < numPosStates; pos_state++ )
			{
				uint32 base_offset = ( pos_state << ( 1 + Lzma.LengthEncoderNumLowBits ) );
				SetPrices( le.Low, base_offset, a, ref lpe.Prices, pos_state, 0 );
				SetPrices( le.Low, base_offset + Lzma.LengthEncoderNumLowSymbols, c, ref lpe.Prices, pos_state, Lzma.LengthEncoderNumLowSymbols );
			}

			uint32 table_size = lpe.TableSize;

			if( table_size > Lzma.LengthEncoderNumLowSymbols * 2u )
			{
				uint32 price_offset = Lzma.LengthEncoderNumLowSymbols << 1;
				table_size -= ( Lzma.LengthEncoderNumLowSymbols << 1 ) - 1u;
				table_size >>= 1;
				b += ProbabilityPrices[( le.Low[Lzma.LengthEncoderNumLowSymbols] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
				do
				{
					uint32 symbol = --table_size + ( 1u << ( Lzma.LengthEncoderNumHighBits - 1 ) );
					uint32 price = b;
					do
					{
						uint32 bit = symbol & 1u;
						symbol >>= 1;
						uint32 xor_value = ( 0u - bit ) & Lzma.BitModelTableMask;
						uint32 price_index = le.High[symbol] ^ xor_value;
						price += ProbabilityPrices[price_index >> LzmaEncoder.NumMoveReducingBits];
					} while( symbol >= 2 );

					probability = le.High[table_size + ( 1u << ( Lzma.LengthEncoderNumHighBits - 1 ) )];
					lpe.Prices[0, price_offset + ( table_size << 1 )] = price + ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
					lpe.Prices[0, price_offset + ( table_size << 1 ) + 1u] = price + ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
				} while( table_size != 0 );

				//Debug.Print( $"Event {++EventNumber:D6}: Update Tables: {lpe.Prices[0, 0]}, {lpe.Prices[0, 1]}, {lpe.Prices[0, 2]}" );

				uint32 start_offset = Lzma.LengthEncoderNumLowSymbols << 1;
				uint32 num_elements = lpe.TableSize - start_offset;
				for( uint32 pos_state = 1u; pos_state < numPosStates; pos_state++ )
				{
					for( uint32 i = 0u; i < num_elements; i++ )
					{
						lpe.Prices[pos_state, start_offset + i] = lpe.Prices[0, start_offset + i];
					}
				}
			}
		}

		private uint32 ReadMatchDistances( ref uint32 numPairs )
		{
			numPairs = 0u;
			AdditionalOffset++;
			NumAvail = MatchFinder.StreamPosition - MatchFinder.Position;
			MatchFinder.GetMatches( Matches, ref numPairs );

			if( numPairs == 0u )
			{
				return 0u;
			}

			//Debug.Print( $"Event {++EventNumber:D6}: ReadMatchDistances matches: ({numPairs}) {Matches[0]}, {Matches[1]}, {Matches[2]}, {Matches[3]}, {Matches[4]}, {Matches[5]}, {Matches[6]}, {Matches[7]}" );

			uint32 length = Matches[numPairs - 2u];
			if( length != FastBytes )
			{
				return length;
			}

			uint32 num_avail = Math.Min( NumAvail, ( uint32 )Lzma.MaxMatchLength );

			int64 base_offset = MatchFinder.BufferOffset - 1u;
			int64 distance_offset = -1 - ( int64 )( Matches[numPairs - 1u] );
			int64 current_index = base_offset + length;
			int64 limit_index = base_offset + num_avail;
			uint8[] buffer_base = MatchFinder.BufferBase;

			for( ; current_index != limit_index && buffer_base[current_index] == buffer_base[current_index + distance_offset]; current_index++ )
			{
			}

			return ( uint32 )( current_index - base_offset );
		}


		private uint32 GetPricePureRep( uint32 repIndex, uint64 state, uint64 posState )
		{
			uint32 price;
			uint32 probability = IsRepG0[state];
			if( repIndex == 0u )
			{
				price = ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
				price += ProbabilityPrices[( IsRep0Long[state][posState] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
			}
			else
			{
				price = ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
				probability = IsRepG1[state];
				if( repIndex == 1u )
				{
					price += ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
				}
				else
				{
					price += ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
					uint32 xor_value = ( 0u - ( repIndex - 2 ) ) & Lzma.BitModelTableMask;
					price += ProbabilityPrices[( IsRepG2[state] ^ xor_value ) >> LzmaEncoder.NumMoveReducingBits];
				}
			}

			return price;
		}

		private uint32 Backward( uint32 cur )
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---Backward" );
			
			uint32 wr = cur + 1u;
			uint32 distance = 0u;
			uint32 length = 0u;
			OptimalEnd = wr;

			while( cur != 0u )
			{
				distance = Optimals[cur].Distance;
				length = Optimals[cur].Length;

				uint32 extra = Optimals[cur].Extra;
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
						distance = LzmaEncoder.MarkLiteral;
					}
					else
					{
						Optimals[wr].Distance = 0u;
						length--;
						wr--;
						Optimals[wr].Distance = LzmaEncoder.MarkLiteral;
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

		private bool GetBestPrice( ref uint32 curRef, uint32 last )
		{
			// 18.06
			uint32 cur = curRef;
			if( cur >= LzmaEncoder.NumOptimals - 64 )
			{
				//Debug.Print( $"Event {++EventNumber:D6}: ---GetBestPrice" );
				uint32 price = Optimals[cur].Price;
				uint32 best = cur;
				for( uint32 price_index = cur + 1; price_index <= last; price_index++ )
				{
					uint32 price2 = Optimals[price_index].Price;
					if( price >= price2 )
					{
						price = price2;
						best = price_index;
					}
				}

				uint32 delta = best - cur;
				if( delta != 0 )
				{
					// MOVE_POS
					AdditionalOffset += delta;
					MatchFinder.Skip( delta );
				}

				curRef = best;
				return true;
			}

			return false;
		}

		private void GetOptimalRepeats( uint32[] repeats, uint32 previous, uint32 distance )
		{
			if( distance < Lzma.NumRepeats )
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
				repeats[0] = distance - Lzma.NumRepeats + 1;
				repeats[1] = Optimals[previous].Repeats[0];
				repeats[2] = Optimals[previous].Repeats[1];
				repeats[3] = Optimals[previous].Repeats[2];
			}
		}

		// Initialize repeat distance lengths
		private void InitRepeatLengths( uint32[] reps, uint32[] repLens, int64 dataOffset, uint32 numAvail, ref uint32 repeatMaxIndex )
		{
			repeatMaxIndex = 0u;
			uint8[] buffer_base = MatchFinder.BufferBase;

			for( uint32 i = 0u; i < Lzma.NumRepeats; i++ )
			{
				reps[i] = Repeats[i];
				int64 compare_offset = dataOffset - reps[i];

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
				if( length == Lzma.MaxMatchLength )
				{
					break;
				}
			}
		}

		// Check for immediate fast matches
		private uint32 CheckFastMatches( uint32 mainLength, uint32 pairCount, uint32[] repeatLengths, uint32 repeatMaxIndex )
		{
			uint32 best_rep_len = repeatLengths[repeatMaxIndex];

			// Check if best repeat match is good enough
			if( best_rep_len >= FastBytes )
			{
				BackRes = repeatMaxIndex;
				AdditionalOffset += best_rep_len - 1u;
				MatchFinder.Skip( best_rep_len - 1u );
				return best_rep_len;
			}

			// Check if main match is good enough
			if( mainLength >= FastBytes )
			{
				BackRes = Matches[pairCount - 1u] + Lzma.NumRepeats;
				AdditionalOffset += mainLength - 1u;
				MatchFinder.Skip( mainLength - 1u );
				return mainLength;
			}

			// Continue with optimal parsing
			return 0u;
		}

		// Initialize first optimal entry (literal)
		private void InitFirstOptimal( uint32 position, int64 dataOffset, uint32 curByte, uint32 matchByte, uint32 positionState )
		{
			// Set initial state
			Optimals[0].State = ( CState )State;

			// Calculate literal context and get probability array
			uint32 literal_context = ( ( position << 8 ) + MatchFinder.BufferBase[dataOffset - 1] ) & LiteralMask;

			// Calculate literal price based on encoder state
			uint32 literal_price = ( State >= 7u ) ? MatchedGetPrice( literal_context, curByte, matchByte ) : GetPrice( literal_context, curByte );

			// Initialize first optimal with literal cost
			Optimals[1].Price = ProbabilityPrices[IsMatch[State][positionState] >> LzmaEncoder.NumMoveReducingBits] + literal_price;
			Optimals[1].Distance = LzmaEncoder.MarkLiteral;
			Optimals[1].Extra = 0;
			Optimals[1].Length = 1u;

			//Debug.Print( $"Event {++EventNumber:D6}: InitFirstOptimal: {Optimals[1].Price}" );
		}

		// Try short repeat (REP0 with length 1)
		private void TryShortRepeat( uint32 curByte, uint32 matchByte, uint32[] repLens, uint32 repeatMatchPrice, uint32 positionState, ref uint32 last )
		{
			// Only process if bytes match and REP0 distance is valid
			if( matchByte != curByte || repLens[0] != 0u )
			{
				return;
			}

			//Debug.Print( $"Event {++EventNumber:D6}: ---TryShortRepeat" );

			// Calculate short repeat price (REP0 with length 1)
			uint32 short_repeat_price = repeatMatchPrice +
			                            ProbabilityPrices[IsRepG0[State] >> LzmaEncoder.NumMoveReducingBits] +
			                            ProbabilityPrices[IsRep0Long[State][positionState] >> LzmaEncoder.NumMoveReducingBits];

			// Update optimal if this is better
			if( short_repeat_price < Optimals[1].Price )
			{
				//Debug.Print( $"Event {++EventNumber:D6}: ShortRepeatPrice: {short_repeat_price}" );
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

		// Process repeat matches
		private void ProcessRepeatMatches( uint32[] repLens, uint32 repeatMatchPrice, uint32 positionState )
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---ProcessRepeatMatches" );

			for( uint32 distance = 0u; distance < Lzma.NumRepeats; distance++ )
			{
				uint32 repeat_length = repLens[distance];
				if( repeat_length < 2u )
				{
					continue;
				}

				// Calculate base price for this repeat distance
				uint32 base_price = repeatMatchPrice + GetPricePureRep( distance, State, positionState );

				// Update prices for all possible lengths (from repLen down to 2)
				for( uint32 length = repeat_length; length >= 2u; length-- )
				{
					uint32 price = base_price + RepeatLenEnc.Prices[positionState, length - Lzma.Lzma1MinMatchLength];
					if( price < Optimals[length].Price )
					{
						//Debug.Print( $"Event {++EventNumber:D6}: ProcessRepeatMatchesPrice: {price}" );

						Optimals[length].Price = price;
						Optimals[length].Length = length;
						Optimals[length].Distance = distance;
						Optimals[length].Extra = 0;
					}
				}
			}
		}

		// Process main matches
		private void ProcessMainMatches( uint32 mainLength, uint32 pairCount, uint32[] repLens, uint32 matchPrice, uint32 positionState )
		{
			// Early exit if no main matches to process
			uint32 length = repLens[0] + 1u;
			if( length > mainLength )
			{
				return;
			}

			//Debug.Print( $"Event {++EventNumber:D6}: ---ProcessMainMatches" );

			// Calculate base price for normal (non-repeat) matches
			uint32 normal_match_price = matchPrice + ProbabilityPrices[IsRep[State] >> LzmaEncoder.NumMoveReducingBits];

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
				uint32 distance = Matches[match_offset + 1u];

				// Calculate price for this length
				uint32 price = normal_match_price + LenEnc.Prices[positionState, length - Lzma.Lzma1MinMatchLength];

				// Calculate length-to-position state for distance pricing
				uint32 length_to_position_state = ( length < Lzma.NumLengthToPositionStates + 1u ) ? length - 2u : Lzma.NumLengthToPositionStates - 1u;

				// Add distance-specific price
				if( distance < Lzma.NumFullDistancesSize )
				{
					price += DistancesPrices[length_to_position_state][distance & Lzma.NumFullDistancesMask];
				}
				else
				{
					uint32 distance_limit = ( 1u << ( Lzma.NumLogBits + 6 ) );
					int8 shift = ( distance < distance_limit ) ? ( int8 )6 : ( int8 )( 5 + Lzma.NumLogBits );
					uint32 slot = GetBlockSizeFromPosition( distance >> shift ) + ( uint32 )( shift << 1 );
					price += PositionSlotPrices[length_to_position_state][slot] + AlignPrices[distance & Lzma.AlignmentTableMask];
				}

				// Update optimal if this is better
				if( price < Optimals[length].Price )
				{
					//Debug.Print( $"Event {++EventNumber:D6}: ProcessMainMatches: {price}" );
					Optimals[length].Price = price;
					Optimals[length].Length = length;
					Optimals[length].Distance = distance + Lzma.NumRepeats;
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

		// Try literal followed by REP0
		private void TryLiteralRep0( uint32 cur, ref uint32 last, bool nextIsLiteral, bool bytesMatch, uint32 literalPrice, uint32 numAvailFull )
		{
			// Early exit checks - skip if any condition fails
			if( nextIsLiteral || bytesMatch || literalPrice == 0u || numAvailFull <= 2u )
			{
				return;
			}

			//Debug.Print( $"Event {++EventNumber:D6}: ---TryLiteralRep0" );

			// Check if current byte matches at REP0 distance (would make this redundant)
			uint32 new_repeat = NewRepeats[0];
			if( MatchFinder.BufferBase[MatchFinder.BufferOffset - 1u] == MatchFinder.BufferBase[MatchFinder.BufferOffset - new_repeat - 1u]
			    || MatchFinder.BufferBase[MatchFinder.BufferOffset] != MatchFinder.BufferBase[MatchFinder.BufferOffset - new_repeat]
			    || MatchFinder.BufferBase[MatchFinder.BufferOffset + 1u] != MatchFinder.BufferBase[MatchFinder.BufferOffset - new_repeat + 1u] )
			{
				return;
			}

			// Find match length starting from position 3
			uint32 limit = Math.Min( numAvailFull, FastBytes + 1u );
			uint32 length = 3u;
			while( length < limit
			       && MatchFinder.BufferBase[length + MatchFinder.BufferOffset - 1u] == MatchFinder.BufferBase[length + MatchFinder.BufferOffset - new_repeat - 1u] )
			{
				length++;
			}

			// Calculate the price for LIT : REP0 sequence
			uint32 state2 = Lzma.LiteralNextStateLut[NewState];
			uint32 position_state2 = ( Position + 1u ) & PositionMask;

			uint32 price = literalPrice +
			               ProbabilityPrices[( IsMatch[state2][position_state2] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			               ProbabilityPrices[( IsRep0Long[state2][position_state2] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			               ProbabilityPrices[( IsRep[state2] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			               ProbabilityPrices[IsRepG0[state2] >> LzmaEncoder.NumMoveReducingBits];

			// Update last position if this extends it
			uint32 offset = cur + length;
			last = Math.Max( last, offset );

			// Calculate final price with length encoding (length-1 since we already counted the literal)
			uint32 final_price = price + RepeatLenEnc.Prices[position_state2, length - 1u - Lzma.Lzma1MinMatchLength];

			// Update optimal if this is better
			if( final_price < Optimals[offset].Price )
			{
				//Debug.Print( $"Event {++EventNumber:D6}: TryLiteralRep: {final_price}" );

				Optimals[offset].Price = final_price;
				Optimals[offset].Length = length - 1u;
				Optimals[offset].Distance = 0u;
				Optimals[offset].Extra = 1;
			}
		}

		// Try REP : LIT : REP0 sequence
		private void TryRepLitRep0( uint32 cur, ref uint32 last, uint32 repIndex, uint32 length, uint32 price, int64 dataOffset, uint32 numAvailFull )
		{
			// Calculate initial limit and check viability
			uint32 start_position = length + 1u;
			uint32 limit = start_position + FastBytes;
			limit = Math.Min( limit, numAvailFull );

			// Check if the two bytes after the rep match (needed for LIT : REP0)
			start_position += 2;
			int64 buffer_offset = start_position + MatchFinder.BufferOffset - 1;
			if( start_position > limit
			    || MatchFinder.BufferBase[buffer_offset - 2u] != MatchFinder.BufferBase[buffer_offset - dataOffset - 2u]
			    || MatchFinder.BufferBase[buffer_offset - 1u] != MatchFinder.BufferBase[buffer_offset - dataOffset - 1u] )
			{
				return;
			}

			// Calculate base price for REP : LIT sequence
			uint32 literal_position = length;
			uint32 state2 = Lzma.RepNextStateLut[NewState];
			uint32 position_state2 = ( Position + length ) & PositionMask;
			uint32 literal_context = ( ( ( Position + length ) << 8 ) + MatchFinder.BufferBase[literal_position + MatchFinder.BufferOffset - 2u] ) & LiteralMask;

			int64 literal_buffer_offset = MatchFinder.BufferOffset + literal_position - 1u;
			price += RepeatLenEnc.Prices[PositionState, length - Lzma.Lzma1MinMatchLength] +
	         ProbabilityPrices[IsMatch[state2][position_state2] >> LzmaEncoder.NumMoveReducingBits] +
	         MatchedGetPrice( literal_context, MatchFinder.BufferBase[literal_buffer_offset], MatchFinder.BufferBase[literal_buffer_offset - dataOffset] );

			// Add price for the final REP0
			uint32 final_state = LzmaEncoder.EncodeStateLiteralAfterRepeat;
			uint32 final_position_state = ( position_state2 + 1u ) & PositionMask;

			price += ProbabilityPrices[( IsMatch[final_state][final_position_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			         ProbabilityPrices[( IsRep0Long[final_state][final_position_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			         ProbabilityPrices[( IsRep[final_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			         ProbabilityPrices[IsRepG0[final_state] >> LzmaEncoder.NumMoveReducingBits];

			// Find full REP0 match length
			uint32 repeat0_length = start_position;
			int64 rep0_buffer_offset = MatchFinder.BufferOffset - 1u;
			while( repeat0_length < limit && MatchFinder.BufferBase[rep0_buffer_offset + repeat0_length] == MatchFinder.BufferBase[rep0_buffer_offset + repeat0_length - dataOffset] )
			{
				repeat0_length++;
			}

			// Calculate final position and update last if extended
			repeat0_length -= length;
			uint32 offset = cur + length + repeat0_length;
			if( last < offset )
			{
				last = offset;
			}

			// Calculate final price with length encoding
			uint32 final_price = price + RepeatLenEnc.Prices[final_position_state, repeat0_length - 1u - Lzma.Lzma1MinMatchLength];

			// Update optimal if this is better
			if( final_price < Optimals[offset].Price )
			{
				//Debug.Print( $"Event {++EventNumber:D6}: TryRepLitRep: {final_price}" );

				Optimals[offset].Price = final_price;
				Optimals[offset].Length = repeat0_length - 1u;
				Optimals[offset].Extra = ( CExtra )( length + 1u );
				Optimals[offset].Distance = repIndex;
			}
		}

		// Process repeat distances in optimal parsing loop
		private void ProcessRepeatsInLoop( uint32 cur, ref uint32 last, ref uint32 startLength, uint32 numAvailFull )
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---ProcessRepeatsInLoop" );
			for( uint32 repeat_index = 0u; repeat_index < Lzma.NumRepeats; repeat_index++ )
			{
				// Check if first two bytes match
				int64 base_offset = MatchFinder.BufferOffset - 1u;
				int64 dest_offset = base_offset - NewRepeats[repeat_index];
				if( MatchFinder.BufferBase[base_offset] != MatchFinder.BufferBase[dest_offset]
				    || MatchFinder.BufferBase[base_offset + 1u] != MatchFinder.BufferBase[dest_offset + 1u] )
				{
					continue;
				}

				// Find full match length
				uint32 match_length = 2u;
				while( match_length < NumAvailOther
				       && MatchFinder.BufferBase[match_length + base_offset] == MatchFinder.BufferBase[match_length + base_offset - NewRepeats[repeat_index]] )
				{
					match_length++;
				}

				// Update last position if extended
				uint32 end_position = cur + match_length;
				last = Math.Max( last, end_position );

				// Calculate base price for this repeat distance
				uint32 base_price = RepeatMatchPrice + GetPricePureRep( repeat_index, NewState, PositionState );

				// Update optimal prices for all lengths (from match_length down to 2)
				for( uint32 length = match_length; length >= 2u; length-- )
				{
					uint32 price = base_price + RepeatLenEnc.Prices[PositionState, length - Lzma.Lzma1MinMatchLength];
					if( price < Optimals[cur + length].Price )
					{
						//Debug.Print( $"Event {++EventNumber:D6}: ProcessRepeatsInLoop: {price}" );
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
				TryRepLitRep0( cur, ref last, repeat_index, match_length, base_price, NewRepeats[repeat_index], numAvailFull );
			}
		}

		// Try MATCH : LIT : REP0 sequence
		private void TryMatchLitRep0( uint32 cur, ref uint32 last, uint32 matchLength, uint32 matchDistance, uint32 price, uint32 numAvailFull )
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---TryMatchLitRep0" );
			
			// Calculate initial limit and check viability
			uint32 start_position = matchLength + 1u;
			uint32 limit = start_position + FastBytes;
			if( limit > numAvailFull )
			{
				return;
			}

			// Check if the two bytes after the match are needed for LIT : REP0
			if( start_position + 2u > limit
			    || MatchFinder.BufferBase[start_position + MatchFinder.BufferOffset - 1u] != MatchFinder.BufferBase[MatchFinder.BufferOffset - matchDistance + start_position - 2u]
			    || MatchFinder.BufferBase[start_position + MatchFinder.BufferOffset] != MatchFinder.BufferBase[MatchFinder.BufferOffset - matchDistance + start_position - 1u] )
			{
				return;
			}

			// Find full REP0 match length after the literal
			uint32 rep0_length = start_position + 2u;
			while( rep0_length < limit
			       && MatchFinder.BufferBase[rep0_length + MatchFinder.BufferOffset - 1u] == MatchFinder.BufferBase[rep0_length + MatchFinder.BufferOffset - matchDistance - 2u] )
			{
				rep0_length++;
			}

			// Calculate total length consumed by REP0 portion
			rep0_length -= matchLength;

			// Calculate price for MATCH : LIT sequence
			uint32 literal_position = matchLength;
			uint32 state2 = Lzma.MatchNextStateLut[NewState];
			uint32 position_state2 = ( Position + matchLength ) & PositionMask;
			uint32 literal_context = ( ( ( Position + matchLength ) << 8 ) + MatchFinder.BufferBase[literal_position + MatchFinder.BufferOffset - 2u] ) & LiteralMask;

			price += ProbabilityPrices[IsMatch[state2][position_state2] >> LzmaEncoder.NumMoveReducingBits] +
			         MatchedGetPrice( literal_context, MatchFinder.BufferBase[literal_position + MatchFinder.BufferOffset - 1u], MatchFinder.BufferBase[literal_position + MatchFinder.BufferOffset - matchDistance - 2u] );

			// Add price for the final REP0
			uint32 final_state = LzmaEncoder.EncodeStateLiteralAfterMatch;
			uint32 final_position_state = ( position_state2 + 1u ) & PositionMask;

			price += ProbabilityPrices[( IsMatch[final_state][final_position_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			         ProbabilityPrices[( IsRep0Long[final_state][final_position_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			         ProbabilityPrices[( IsRep[final_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits] +
			         ProbabilityPrices[IsRepG0[final_state] >> LzmaEncoder.NumMoveReducingBits];

			// Update last position if this extends it
			uint32 end_position = cur + matchLength + rep0_length;
			if( last < end_position )
			{
				last = end_position;
			}

			// Calculate final price with REP0 length encoding
			uint32 final_price = price + RepeatLenEnc.Prices[final_position_state, rep0_length - 1u - Lzma.Lzma1MinMatchLength];

			// Update optimal if this is better
			if( final_price < Optimals[end_position].Price )
			{
				//Debug.Print( $"Event {++EventNumber:D6}: TryMatchLitRep: {final_price}" );
				Optimals[end_position].Price = final_price;
				Optimals[end_position].Length = rep0_length - 1u;
				Optimals[end_position].Extra = ( CExtra )( matchLength + 1u );
				Optimals[end_position].Distance = matchDistance + Lzma.NumRepeats;
			}
		}

		// Process main matches in optimal parsing loop
		private void ProcessMainMatchesInLoop( uint32 cur, ref uint32 last, uint32 newLength, uint32 pairCount, uint32 startLength, uint32 numAvailFull )
		{
			// Early exit if new length doesn't extend past start length
			if( newLength < startLength )
			{
				return;
			}

			//Debug.Print( $"Event {++EventNumber:D6}: ---ProcessMainMatchesInLoop" );

			// Calculate base price for normal (non-repeat) matches
			uint32 normal_match_price = MatchPrice + ProbabilityPrices[IsRep[NewState] >> LzmaEncoder.NumMoveReducingBits];

			// Update last position if new length extends it
			last = Math.Max( last, cur + newLength );

			// Find starting position in matches array
			uint32 match_offset = 0u;
			while( startLength > Matches[match_offset] )
			{
				match_offset += 2u;
			}

			// Get first match distance and calculate position slot
			uint32 match_distance = Matches[match_offset + 1u];
			uint32 match_limit = ( 1u << ( Lzma.NumLogBits + 6 ) );
			int8 distance_shift = ( match_distance < match_limit ) ? ( int8 )6 : ( int8 )( 5 + Lzma.NumLogBits );
			uint32 position_slot = GetBlockSizeFromPosition( match_distance >> distance_shift ) + ( uint32 )( distance_shift << 1 );

			// Process all match lengths from start_len to new_len
			for( uint32 match_length = startLength;; match_length++ )
			{
				// Calculate base price for this length
				uint32 price = normal_match_price + LenEnc.Prices[PositionState, match_length - Lzma.Lzma1MinMatchLength];

				// Calculate length-to-position state for distance pricing
				uint32 length_to_position_state = ( match_length < Lzma.NumLengthToPositionStates + 1u ) ? match_length - 2u : Lzma.NumLengthToPositionStates - 1u;

				// Add distance-specific price
				if( match_distance < Lzma.NumFullDistancesSize )
				{
					price += DistancesPrices[length_to_position_state][match_distance & Lzma.NumFullDistancesMask];
				}
				else
				{
					price += PositionSlotPrices[length_to_position_state][position_slot] + AlignPrices[match_distance & Lzma.AlignmentTableMask];
				}

				// Update optimal if this is better
				if( price < Optimals[cur + match_length].Price )
				{
					//Debug.Print( $"Event {++EventNumber:D6}: ProcessMainMatchesInLoop: {price}" );
					Optimals[cur + match_length].Price = price;
					Optimals[cur + match_length].Length = match_length;
					Optimals[cur + match_length].Distance = match_distance + Lzma.NumRepeats;
					Optimals[cur + match_length].Extra = 0;
				}

				// Check if we've reached a match boundary
				if( match_length == Matches[match_offset] )
				{
					// Try MATCH : LIT : REP0 sequence
					TryMatchLitRep0( cur, ref last, match_length, match_distance, price, numAvailFull );

					// Move to next match
					match_offset += 2u;
					if( match_offset == pairCount )
					{
						break;
					}

					// Get next match distance and recalculate position slot
					match_distance = Matches[match_offset + 1u];
					match_limit = ( 1u << ( Lzma.NumLogBits + 6 ) );
					distance_shift = ( match_distance < match_limit ) ? ( int8 )6 : ( int8 )( 5 + Lzma.NumLogBits );
					position_slot = GetBlockSizeFromPosition( match_distance >> distance_shift ) + ( uint32 )( distance_shift << 1 );
				}
			}
		}

		// Main optimal parsing function (now much shorter!)
		private uint32 GetOptimum( uint32 position )
		{
			uint32[] reps = new uint32[Lzma.NumRepeats];
			uint32[] repeat_lengths = new uint32[Lzma.NumRepeats];
			uint32 pair_count = 0u;
			uint32 main_length;

			OptimalCurrent = 0u;
			OptimalEnd = 0u;

			// Get match distances
			if( AdditionalOffset == 0u )
			{
				main_length = ReadMatchDistances( ref pair_count );
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
				BackRes = LzmaEncoder.MarkLiteral;
				return 1u;
			}

			num_avail = Math.Min( num_avail, ( CProbPrice )Lzma.MaxMatchLength );

			int64 data_offset = MatchFinder.BufferOffset - 1u;
			uint32 rep_max_index = 0u;

			// Initialize repeat lengths
			InitRepeatLengths( reps, repeat_lengths, data_offset, num_avail, ref rep_max_index );

			// Check for immediate fast matches
			uint32 fast_result = CheckFastMatches( main_length, pair_count, repeat_lengths, rep_max_index );
			if( fast_result != 0u )
			{
				return fast_result;
			}

			// Get current and match bytes
			uint8 current_byte = MatchFinder.BufferBase[data_offset];
			uint8 match_byte = MatchFinder.BufferBase[data_offset - reps[0]];

			// Determine last position to check
			uint32 last = Math.Max( repeat_lengths[rep_max_index], main_length );
			if( last < 2u && current_byte != match_byte )
			{
				BackRes = LzmaEncoder.MarkLiteral;
				return 1u;
			}

			// Initialize first optimal entry
			uint32 position_state = position & PositionMask;
			InitFirstOptimal( position, data_offset, current_byte, match_byte, position_state );

			// Calculate base prices
			uint32 match_price = ProbabilityPrices[( IsMatch[State][position_state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
			uint32 repeat_match_price = match_price + ProbabilityPrices[( IsRep[State] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];

			// Try short repeat
			TryShortRepeat( current_byte, match_byte, repeat_lengths, repeat_match_price, position_state, ref last );
			if( last == 1u )
			{
				return 1u;
			}

			// Copy initial repeat distances
			Optimals[0].Repeats[0] = reps[0];
			Optimals[0].Repeats[1] = reps[1];
			Optimals[0].Repeats[2] = reps[2];
			Optimals[0].Repeats[3] = reps[3];

			// Process initial repeat matches
			ProcessRepeatMatches( repeat_lengths, repeat_match_price, position_state );

			// Process initial main matches
			ProcessMainMatches( main_length, pair_count, repeat_lengths, match_price, position_state );

			// Main optimal parsing loop
			uint32 cur = 0u;
			while( ++cur != last )
			{
				//Debug.Print( $"Event {++EventNumber:D6}: cur, last: {cur}, {last}" );

				if( GetBestPrice( ref cur, last ) )
				{
					break;
				}

				uint32 new_length = ReadMatchDistances( ref pair_count );

				//Debug.Print( $"Event {++EventNumber:D6}: matches: {Matches[0]}, {Matches[1]}, {Matches[2]}, {Matches[3]}" );

				if( new_length >= FastBytes )
				{
					NumPairs = pair_count;
					LongestMatchLength = new_length;
					break;
				}

				// Process current position
				position++;

				uint32 prev = cur - Optimals[cur].Length;
				uint32 state;

				// Determine state based on previous optimal
				if( Optimals[cur].Length == 1u )
				{
					state = Optimals[prev].State;
					state = ( Optimals[cur].Distance == 0u ) ? Lzma.ShortRepNextStateLut[state] : Lzma.LiteralNextStateLut[state];
				}
				else
				{
					uint32 dist = Optimals[cur].Distance;
					uint32 adjusted_prev = prev;

					if( Optimals[cur].Extra != 0 )
					{
						adjusted_prev -= Optimals[cur].Extra;
						state = LzmaEncoder.EncodeStateRepeatAfterLiteral;
						if( Optimals[cur].Extra == 1u )
						{
							state = ( dist < Lzma.NumRepeats ) ? LzmaEncoder.EncodeStateRepeatAfterLiteral : LzmaEncoder.EncodeStateMatchAfterLiteral;
						}
					}
					else
					{
						state = Optimals[adjusted_prev].State;
						state = ( dist < Lzma.NumRepeats ) ? Lzma.RepNextStateLut[state] : Lzma.MatchNextStateLut[state];
					}

					GetOptimalRepeats( reps, adjusted_prev, dist );
				}

				// Update current optimal's state and reps
				Optimals[cur].State = ( CState )state;
				Optimals[cur].Repeats[0] = reps[0];
				Optimals[cur].Repeats[1] = reps[1];
				Optimals[cur].Repeats[2] = reps[2];
				Optimals[cur].Repeats[3] = reps[3];

				// Update context
				Position = position;
				PositionState = position & PositionMask;
				NewRepeats = reps;
				NewState = state;

				uint8 current_byte2 = MatchFinder.BufferBase[MatchFinder.BufferOffset - 1u];
				uint8 match_byte2 = MatchFinder.BufferBase[MatchFinder.BufferOffset - reps[0] - 1u];

				// Calculate prices for current position
				uint32 current_price = Optimals[cur].Price;
				uint32 prob = IsMatch[state][PositionState];
				MatchPrice = current_price + ProbabilityPrices[( prob ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
				uint32 literal_price = current_price + ProbabilityPrices[prob >> LzmaEncoder.NumMoveReducingBits];

				bool next_is_literal = false;

				// Try literal
				if( ( Optimals[cur + 1u].Price < LzmaEncoder.InfinityPrice && match_byte2 == current_byte2 ) || literal_price > Optimals[cur + 1u].Price )
				{
					literal_price = 0u;
				}
				else
				{
					uint32 literal_context = ( ( position << 8 ) + MatchFinder.BufferBase[MatchFinder.BufferOffset - 2u] ) & LiteralMask;
					literal_price += ( state >= 7u ) ? 
						MatchedGetPrice( literal_context, current_byte2, match_byte2 ) :
						GetPrice( literal_context, current_byte2 );

					if( literal_price < Optimals[cur + 1u].Price )
					{
						Optimals[cur + 1u].Price = literal_price;
						Optimals[cur + 1u].Length = 1u;
						Optimals[cur + 1u].Distance = LzmaEncoder.MarkLiteral;
						Optimals[cur + 1u].Extra = 0;
						next_is_literal = true;
						//Debug.Print( $"Event {++EventNumber:D6}: Write literal optimal: {literal_price}" );
					}
				}

				RepeatMatchPrice = MatchPrice + ProbabilityPrices[( IsRep[state] ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];

				// Calculate available data
				uint32 num_avail_full = Math.Min( NumAvail, LzmaEncoder.NumOptimals - 1u - cur );

				// Try short repeat at current position
				if( state < 7u && match_byte2 == current_byte2 && RepeatMatchPrice < Optimals[cur + 1u].Price )
				{
					if( Optimals[cur + 1u].Length < 2u || Optimals[cur + 1u].Distance != 0u )
					{
						uint32 short_repeat_price = RepeatMatchPrice +
						                            ProbabilityPrices[IsRepG0[state] >> LzmaEncoder.NumMoveReducingBits] +
						                            ProbabilityPrices[IsRep0Long[state][PositionState] >> LzmaEncoder.NumMoveReducingBits];

						if( short_repeat_price < Optimals[cur + 1u].Price )
						{
							Optimals[cur + 1u].Price = short_repeat_price;
							Optimals[cur + 1u].Length = 1u;
							Optimals[cur + 1u].Distance = 0u;
							Optimals[cur + 1u].Extra = 0;
							next_is_literal = false;
							//Debug.Print( $"Event {++EventNumber:D6}: Write non-literal optimal: {short_repeat_price}" );
						}
					}
				}

				if( num_avail_full < 2u )
				{
					continue;
				}

				NumAvailOther = Math.Min( num_avail_full, FastBytes );

				// Try LIT : REP_0
				TryLiteralRep0( cur, ref last, next_is_literal, current_byte2 == match_byte2, literal_price, num_avail_full );

				// Process all repeat distances
				uint32 start_length = 2u;
				ProcessRepeatsInLoop( cur, ref last, ref start_length, num_avail_full );

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

				ProcessMainMatchesInLoop( cur, ref last, new_length, pair_count, start_length, num_avail_full );
			}

			// Reset infinity prices
			do
			{
				Optimals[last].Price = LzmaEncoder.InfinityPrice;
			} while( --last != 0u );

			return Backward( cur );
		}

		private uint32 GetOptimumFast()
		{
			// Get match distances from match finder
			uint32 main_length;
			uint32 pair_count = 0u;
			if( AdditionalOffset == 0u )
			{
				main_length = ReadMatchDistances( ref pair_count );
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
				BackRes = LzmaEncoder.MarkLiteral;
				return 1u;
			}

			num_avail = Math.Min( num_avail, ( uint32 )Lzma.MaxMatchLength );
			int64 data_offset = MatchFinder.BufferOffset - 1u;

			//Debug.Print( $"Event {++EventNumber:D6}: GetOptimumFast: {num_avail}, {pair_count}" );

			// Find the best repeat match
			uint32 best_rep_len = 0u;
			uint32 best_rep_index = 0u;
			for( uint32 i = 0u; i < Lzma.NumRepeats; i++ )
			{
				int64 compare_offset = data_offset - Repeats[i];
				if( MatchFinder.BufferBase[data_offset] != MatchFinder.BufferBase[compare_offset]
				    || MatchFinder.BufferBase[data_offset + 1u] != MatchFinder.BufferBase[compare_offset + 1u] )
				{
					continue;
				}

				// Find match length for this repeat distance
				uint32 length = 2u;
				while( length < num_avail && MatchFinder.BufferBase[data_offset + length] == MatchFinder.BufferBase[compare_offset + length] )
				{
					length++;
				}

				// If match is long enough, use it immediately
				if( length >= FastBytes )
				{
					BackRes = i;
					AdditionalOffset += length - 1u;
					MatchFinder.Skip( length - 1u );
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
				BackRes = Matches[pair_count - 1u] + Lzma.NumRepeats;
				AdditionalOffset += main_length - 1u;
				MatchFinder.Skip( main_length - 1u );
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
					uint32 prev_length = Matches[pair_count - 4u];
					uint32 prev_distance = Matches[pair_count - 3u];

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
				bool use_repeat =
					( best_rep_len + 1u >= main_length ) ||
					( best_rep_len + 2u >= main_length && main_distance >= ( 1u << 9 ) ) ||
					( best_rep_len + 3u >= main_length && main_distance >= ( 1u << 15 ) );

				if( use_repeat )
				{
					BackRes = best_rep_index;
					AdditionalOffset += best_rep_len - 1u;
					MatchFinder.Skip( best_rep_len - 1u );
					return best_rep_len;
				}
			}

			// If no good match found, return literal
			if( main_length < 2u || num_avail <= 2u )
			{
				BackRes = LzmaEncoder.MarkLiteral;
				return 1u;
			}

			// Look ahead one position to see if we can find a better match
			uint32 next_length = ReadMatchDistances( ref NumPairs );
			LongestMatchLength = next_length;

			if( next_length >= 2u )
			{
				uint32 next_distance = Matches[NumPairs - 1u];

				// Use next position if it has a better match
				bool use_next =
					( next_length >= main_length && next_distance < main_distance ) ||
					( next_length == main_length + 1u && ( next_distance >> 7 ) <= main_distance ) ||
					( next_length > main_length + 1u ) ||
					( next_length + 1u >= main_length && main_length >= 3u && ( main_distance >> 7 ) > next_distance );

				if( use_next )
				{
					BackRes = LzmaEncoder.MarkLiteral;
					return 1u;
				}
			}

			// Check if any repeat distances would match at next position
			data_offset = MatchFinder.BufferOffset - 1u;

			foreach( uint32 repeat in Repeats )
			{
				int64 compare_offset = data_offset - repeat;
				if( MatchFinder.BufferBase[data_offset] != MatchFinder.BufferBase[compare_offset]
				    || MatchFinder.BufferBase[data_offset + 1u] != MatchFinder.BufferBase[compare_offset + 1u] )
				{
					continue;
				}

				// If repeat would match almost as well at next position, delay
				uint32 limit = main_length - 1u;
				uint32 length = 2u;
				while( length < limit && MatchFinder.BufferBase[data_offset + length] == MatchFinder.BufferBase[compare_offset + length] )
				{
					length++;
				}

				if( length >= limit )
				{
					BackRes = LzmaEncoder.MarkLiteral;
					return 1u;
				}
			}

			// Use main match
			BackRes = main_distance + Lzma.NumRepeats;
			if( main_length > 2u )
			{
				AdditionalOffset += main_length - 2u;
				MatchFinder.Skip( main_length - 2u );
			}

			return main_length;
		}

		private void WriteEndMarker( uint32 posState )
		{
			// Encode isMatch = 1 (this is a match, not a literal)
			RangeCoder.IterateEndMark( ref IsMatch[State][posState] );

			// Encode isRep = 0 (this is a new match, not a repeat)
			uint32 probability = IsRep[State];
			uint32 bound = RangeCoder.CalcNewBound( probability );
			RangeCoder.SetNewBoundUpdateRange( bound );
			IsRep[State] = ( CProbability )( probability + ( ( Lzma.BitModelTableSize - probability ) >> Lzma.NumMoveBits ) );

			// Update state to match state
			State = Lzma.MatchNextStateLut[State];

			// Encode match length = 0 (minimum length, which signals end marker)
			RangeCoder.LengthEncode( LengthProbabilities, 0u, posState );

			// Encode position slot = maximum (all bits set to 1)
			RangeCoder.EncodeTreeAllOnes( PositionSlotEncoder[0], Lzma.NumPositionSlotBits );

			// Encode direct bits (30 - NumAlignmentBits) with all bits set to 1
			int32 num_direct_bits = 30 - Lzma.NumAlignmentBits;
			for( uint32 i = 0u; i < num_direct_bits; i++ )
			{
				RangeCoder.Range >>= 1;
				RangeCoder.Low += RangeCoder.Range;
				RangeCoder.UpdateRange();
			}

			// Encode alignment bits (NumAlignmentBits) with all bits set to 1
			RangeCoder.EncodeTreeAllOnes( PositionAlignEncoder, Lzma.NumAlignmentBits );
		}

		private SevenZipResult CheckErrors()
		{
			if( Result != SevenZipResult.SevenZipOK )
			{
				return Result;
			}

			if( RangeCoder.Result != SevenZipResult.SevenZipOK )
			{
				Result = SevenZipResult.SevenZipErrorWrite;
			}

			if( MatchFinder.Result != SevenZipResult.SevenZipOK )
			{
				Result = SevenZipResult.SevenZipErrorRead;
			}

			if( Result != SevenZipResult.SevenZipOK )
			{
				Finished = true;
			}

			return Result;
		}

		private void Flush( uint32 nowPos )
		{
			Finished = true;
			if( WriteEndMark )
			{
				WriteEndMarker( nowPos & PositionMask );
			}

			RangeCoder.FlushData();
			RangeCoder.FlushStream();
		}

		// Helper function to calculate price for encoding a symbol through binary tree
		private uint32 CalcTreeAlignPrice( uint32 symbol, ref uint32 m )
		{
			uint32 price = 0u;

			for( uint32 count = 0; count < 3; count++ )
			{
				uint32 bit = symbol & 1u;
				symbol >>= 1;
				uint32 xor_value = ( 0u - bit ) & Lzma.BitModelTableMask;
				uint32 price_index = PositionAlignEncoder[m] ^ xor_value;
				price += ProbabilityPrices[price_index >> LzmaEncoder.NumMoveReducingBits];

				m = ( m << 1 ) + bit;
			}

			return price;
		}

		private uint32 GetPositionModelPrice( uint32 positionIndex, uint32 positionOffset, int8 footerBits, ref uint32 m )
		{
			uint32 price = 0;

			if( footerBits != 0 )
			{
				uint32 symbol = positionIndex;
				do
				{
					uint32 bit = symbol & 1;
					symbol >>= 1;
					uint32 xor_value = ( 0u - bit ) & Lzma.BitModelTableMask;
					uint32 price_index = PositionEncoders[positionOffset + m] ^ xor_value;
					price += ProbabilityPrices[price_index >> LzmaEncoder.NumMoveReducingBits];

					m = ( m << 1 ) + bit;
				} while( --footerBits != 0 );
			}

			return price;
		}

		private uint32 CalcTreeDistancePrice( uint32 lps, uint32 symbol )
		{
			CProbability[] probabilities = PositionSlotEncoder[lps];
			uint32 price = 0u;

			// Start from tree root
			symbol += ( 1u << ( Lzma.NumPositionSlotBits - 1 ) );

			for( uint32 count = 0; count < Lzma.NumPositionSlotBits - 1; count++ )
			{
				uint32 bit = symbol & 1u;
				symbol >>= 1;
				uint32 xor_value = ( 0u - bit ) & Lzma.BitModelTableMask;
				uint32 price_index = probabilities[symbol] ^ xor_value;
				price += ProbabilityPrices[price_index >> LzmaEncoder.NumMoveReducingBits];
			}

			return price;
		}

		private void FillAlignPrices()
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---FillAlignPrices" );

			// Calculate prices for 4-bit alignment values (0-15)
			// Process pairs to calculate both bit=0 and bit=1 prices for final bit
			for( uint32 i = 0u; i < Lzma.AlignmentTableSize / 2u; i++ )
			{
				// Calculate price for first 3 bits using the tree price helper
				uint32 m = 1;
				uint32 base_price = CalcTreeAlignPrice( i, ref m );

				// Calculate final bit prices manually (bit 3)
				uint32 probability = PositionAlignEncoder[m];
				AlignPrices[i] = base_price + ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
				AlignPrices[i + 8u] = base_price + ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
			}
		}

		private void FillDistancesPrices()
		{
			//Debug.Print( $"Event {++EventNumber:D6}: ---FillDistancesPrices" );

			uint32[] temp_prices = new uint32[Lzma.NumFullDistancesSize];

			MatchPriceCount = 0u;

			// Calculate prices for position model distances (4-127)
			for( uint32 position_index = Lzma.StartPositionModelIndex / 2u; position_index < Lzma.NumFullDistancesSize / 2u; position_index++ )
			{ 
				uint8 pos_slot = GetBlockSizeFromPosition( position_index );
				int8 footer_bits = ( int8 )( ( pos_slot >> 1 ) - 1 );
				uint32 base_pos = ( 2u | ( pos_slot & 1u ) ) << footer_bits;
				uint32 position_offset = base_pos << 1;

				uint32 m = 1;
				uint32 price = GetPositionModelPrice( position_index, position_offset, footer_bits, ref m );

				uint32 probability = PositionEncoders[position_offset + m];
				base_pos += position_index;
				temp_prices[base_pos] = price + ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
				temp_prices[base_pos + ( 1u << footer_bits )] = price + ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
			}

			// Calculate prices for each length-to-position state
			for( uint32 lps = 0u; lps < Lzma.NumLengthToPositionStates; lps++ )
			{
				uint32 dist_table_size = ( DistanceTableSize + 1 ) >> 1;
				uint32[] pos_slot_prices = PositionSlotPrices[lps];

				// Calculate position slot prices (6-bit encoding)
				for( uint32 slot = 0u; slot < dist_table_size; slot++ )
				{
					uint32 price = CalcTreeDistancePrice( lps, slot );
					uint32 probability = PositionSlotEncoder[lps][slot + ( 1u << ( Lzma.NumPositionSlotBits - 1 ) )];

					pos_slot_prices[slot << 1] = price + ProbabilityPrices[probability >> LzmaEncoder.NumMoveReducingBits];
					pos_slot_prices[( slot << 1 ) + 1u] = price + ProbabilityPrices[( probability ^ Lzma.BitModelTableMask ) >> LzmaEncoder.NumMoveReducingBits];
				}

				// Add delta for slots beyond position model range (aligned distances)
				uint32 delta = ( uint32 )( ( ( Lzma.EndPositionModelIndex / 2u - 1u ) - Lzma.NumAlignmentBits ) << LzmaEncoder.NumBitPriceShiftBits );
				for( int32 slot = Lzma.EndPositionModelIndex / 2; slot < dist_table_size; slot++ )
				{
					pos_slot_prices[slot << 1] += delta;
					pos_slot_prices[( slot << 1 ) + 1u] += delta;
					delta += ( 1u << LzmaEncoder.NumBitPriceShiftBits );
				}

				// Combine slot prices with position model prices
				uint32[] distance_price = DistancesPrices[lps];

				// Copy direct slot prices for distances 0-3
				Array.Copy( pos_slot_prices, distance_price, Lzma.NumLengthToPositionStates );

				// Combine slot and model prices for distances 4+
				for( uint32 i = 4u; i < Lzma.NumFullDistancesSize; i += 2u )
				{
					uint32 slot_price = pos_slot_prices[GetBlockSizeFromPosition( i )];
					distance_price[i] = slot_price + temp_prices[i];
					distance_price[i + 1u] = slot_price + temp_prices[i + 1u];
				}
			}
		}

		private void FreeLits()
		{
			LiteralProbabilities = [];
			SavedState.LiteralProbabilities = [];
		}

		private void Lit( uint32 nowPos32 )
		{
			int64 data_offset = MatchFinder.BufferOffset - AdditionalOffset;
			uint32 work = ( ( nowPos32 << 8 ) + MatchFinder.BufferBase[data_offset - 1u] ) & LiteralMask;
			uint32 prob_offset = 3u * ( work << LiteralContextBits );
			uint32 state = State;
			State = Lzma.LiteralNextStateLut[State];
			if( state < 7 )
			{
				RangeCoder.Encode( LiteralProbabilities, prob_offset, MatchFinder.BufferBase[data_offset] );
			}
			else
			{
				RangeCoder.EncodeMatched( LiteralProbabilities, prob_offset, MatchFinder.BufferBase[data_offset], MatchFinder.BufferBase[data_offset - Repeats[0]] );
			}
		}

		private uint32 GetLength( uint32 nowPos32 )
		{
			uint32 length;
			if( FastMode )
			{
				length = GetOptimumFast();
			}
			else
			{
				uint32 oci = OptimalCurrent;
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


		private uint32 LiteralBit( CRangeEncoder re )
		{
			if( NowPos64 == 0 )
			{
				if( MatchFinder.StreamPosition - MatchFinder.Position == 0u )
				{
					Flush( 0u );
					return 0u;
				}

				uint32 pair_count = 0u;
				ReadMatchDistances( ref pair_count );
				re.EncodeBit0( ref IsMatch[LzmaEncoder.EncodeStateStart][0] );
				uint8 current_byte = MatchFinder.BufferBase[MatchFinder.BufferOffset - AdditionalOffset];
				re.Encode( LiteralProbabilities, 0, current_byte );
				AdditionalOffset--;
				return 1u;
			}

			return ( uint32 )( NowPos64 & UInt32.MaxValue );
		}

		private SevenZipResult CodeOneBlock( uint32 maxPackSize, uint32 maxUnpackSize )
		{
			if( NeedInit )
			{
				MatchFinder.Init();
				NeedInit = false;
			}

			if( Finished )
			{
				return Result;
			}

			SevenZipResult result = CheckErrors();
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			uint32 start_pos_32 = ( uint32 )( NowPos64 & UInt32.MaxValue );

			uint32 now_pos_32 = LiteralBit( RangeCoder );

			result = CheckErrors();
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			if( MatchFinder.StreamPosition - MatchFinder.Position != 0u )
			{
				while( true )
				{
					uint32 length = GetLength( now_pos_32 );
					uint32 pos_state = ( now_pos_32 & PositionMask );
					uint32 dist = BackRes;

					// Encode isMatch bit
					if( dist == LzmaEncoder.MarkLiteral )
					{
						// Literal
						RangeCoder.Iterate( IsMatch[State], pos_state, 0u );
						Lit( now_pos_32 );
					}
					else
					{
						// Match or Repeat
						RangeCoder.Iterate( IsMatch[State], pos_state, 1u );

						if( dist < Lzma.NumRepeats )
						{
							// Repeat distance encoding
							RangeCoder.Iterate( IsRep, State, 1u );

							if( dist == 0u )
							{
								// REP0
								//Debug.Print( $"Event {++EventNumber:D6}: REP0" );
								RangeCoder.Iterate( IsRepG0, State, 0u );

								// Check if short rep (length == 1)
								if( length == 1u )
								{
									RangeCoder.Iterate( IsRep0Long[State], pos_state, 0u );
									State = Lzma.ShortRepNextStateLut[State];
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
									//Debug.Print( $"Event {++EventNumber:D6}: REP1" );
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
										//Debug.Print( $"Event {++EventNumber:D6}: REP2" );
										RangeCoder.Iterate( IsRepG2, State, 0u );
										dist = Repeats[2];
									}
									else
									{
										// REP3
										//Debug.Print( $"Event {++EventNumber:D6}: REP3" );
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
								RangeCoder.LengthEncode( RepeatLengthProbabilities, ( CProbPrice )( length - Lzma.Lzma1MinMatchLength ), pos_state );
								--RepeatLenEncCounter;
								State = Lzma.RepNextStateLut[State];
							}
						}
						else
						{
							// New match distance
							RangeCoder.Iterate( IsRep, State, 0u );

							State = Lzma.MatchNextStateLut[State];
							RangeCoder.LengthEncode( LengthProbabilities, ( CProbPrice )( length - Lzma.Lzma1MinMatchLength ), pos_state );

							dist -= Lzma.NumRepeats;
							Repeats[3] = Repeats[2];
							Repeats[2] = Repeats[1];
							Repeats[1] = Repeats[0];
							Repeats[0] = dist + 1u;

							MatchPriceCount++;

							// Encode position slot
							uint32 pos_slot;
							if( dist < Lzma.NumFullDistancesSize )
							{
								pos_slot = GetBlockSizeFromPosition( dist & Lzma.NumFullDistancesMask );
							}
							else
							{
								uint32 dist_limit = ( 1u << ( Lzma.NumLogBits + 6 ) );
								int8 shift = ( dist < dist_limit ) ? ( int8 )6 : ( int8 )( 5 + Lzma.NumLogBits );
								pos_slot = GetBlockSizeFromPosition( dist >> shift ) + ( uint32 )( shift << 1 );
							}

							uint32 len_to_pos_state = ( length < Lzma.NumLengthToPositionStates + 1u ) ? length - 2u : Lzma.NumLengthToPositionStates - 1u;
							CProbability[] position_slot_probabilities = PositionSlotEncoder[len_to_pos_state];

							uint32 symbol = pos_slot + ( 1u << Lzma.NumPositionSlotBits );
							do
							{
								uint32 bit = ( symbol >> ( Lzma.NumPositionSlotBits - 1 ) ) & 1u;
								RangeCoder.Iterate( position_slot_probabilities, symbol >> Lzma.NumPositionSlotBits, bit );
								symbol <<= 1;
							} while( symbol < ( 1u << ( Lzma.NumPositionSlotBits << 1 ) ) );

							// Encode distance footer
							if( dist >= Lzma.StartPositionModelIndex )
							{
								int8 footer_bits = ( int8 )( ( pos_slot >> 1 ) - 1u );

								if( dist < Lzma.NumFullDistancesSize )
								{
									uint32 base_dist = ( 2u | ( pos_slot & 1u ) ) << footer_bits;
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
										uint32 bit = dist & 1u;
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
								UpdateTables( LenEnc, 1u << PositionBits, LengthProbabilities );
							}

							if( RepeatLenEncCounter <= 0 )
							{
								RepeatLenEncCounter = LzmaEncoder.RepeatLengthCount;
								UpdateTables( RepeatLenEnc, 1u << PositionBits, RepeatLengthProbabilities );
							}
						}

						if( MatchFinder.StreamPosition - MatchFinder.Position == 0u )
						{
							break;
						}

						uint32 processed = now_pos_32 - start_pos_32;

						if( maxPackSize != 0 )
						{
							if( processed + LzmaEncoder.NumOptimals + 300u >= maxUnpackSize
							    || ( RangeCoder.Processed + RangeCoder.BufferOffset + RangeCoder.CacheSize ) + LzmaEncoder.PackReserveSize >= maxPackSize )
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

		private SevenZipResult AllocateMemory( uint32 keepWindowSize )
		{
			// Allocate or reallocate literal probability tables if needed
			int32 lclp = LiteralContextBits + LiteralPositionBits;
			if( LiteralProbabilities.LongLength == 0 || SavedState.LiteralProbabilities.LongLength == 0 || TotalLiteralBits != lclp )
			{
				FreeLits();

				uint32 prob_array_size = ( 0x300u << lclp );
				LiteralProbabilities = new CProbability[prob_array_size];
				SavedState.LiteralProbabilities = new CProbability[prob_array_size];

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
			uint32 before_size = LzmaEncoder.NumOptimals;
			if( before_size + dict_size < keepWindowSize )
			{
				before_size = keepWindowSize - dict_size;
			}

			// Create match finder with calculated buffer sizes
			if( !MatchFinder.Create( dict_size, before_size, FastBytes, ( CProbPrice )Lzma.MaxMatchLength + 1u ) )
			{
				return SevenZipResult.SevenZipErrorMemory;
			}

			Optimals = new COptimal[LzmaEncoder.NumOptimals];
			for( int32 optimal_index = 0; optimal_index < LzmaEncoder.NumOptimals; optimal_index++ )
			{
				Optimals[optimal_index] = new COptimal();
			}

			return SevenZipResult.SevenZipOK;
		}

		private void Init()
		{
			State = 0u;
			Repeats[0] = 1u;
			Repeats[1] = 1u;
			Repeats[2] = 1u;
			Repeats[3] = 1u;

			RangeCoder.Init();

			Array.Fill( PositionAlignEncoder, Lzma.InitProbabilityValue );

			for( uint32 i = 0u; i < Lzma.NumStates; i++ )
			{
				for( uint32 j = 0u; j < Lzma.MaxPositionBitsStates; j++ )
				{
					IsMatch[i][j] = Lzma.InitProbabilityValue;
					IsRep0Long[i][j] = Lzma.InitProbabilityValue;
				}

				IsRep[i] = Lzma.InitProbabilityValue;
				IsRepG0[i] = Lzma.InitProbabilityValue;
				IsRepG1[i] = Lzma.InitProbabilityValue;
				IsRepG2[i] = Lzma.InitProbabilityValue;
			}

			for( uint32 i = 0u; i < Lzma.NumLengthToPositionStates; i++ )
			{
				for( uint32 j = 0u; j < ( 1 << Lzma.NumPositionSlotBits ); j++ )
				{
					PositionSlotEncoder[i][j] = Lzma.InitProbabilityValue;
				}
			}

			Array.Fill( PositionEncoders, Lzma.InitProbabilityValue );

			uint32 count = Lzma.LiteralSize << ( LiteralPositionBits + LiteralContextBits );
			for( uint32 i = 0; i < count; i++ )
			{
				LiteralProbabilities[i] = Lzma.InitProbabilityValue;
			}

			LengthProbabilities.Init();
			RepeatLengthProbabilities.Init();

			OptimalEnd = 0u;
			OptimalCurrent = 0u;

			for( uint32 optimal_index = 0; optimal_index < LzmaEncoder.NumOptimals; optimal_index++ )
			{
				Optimals[optimal_index].Price = LzmaEncoder.InfinityPrice;
			}

			AdditionalOffset = 0u;

			PositionMask = ( 1u << PositionBits ) - 1u;
			LiteralMask = ( 0x100u << LiteralPositionBits ) - ( 0x100u >> LiteralContextBits );
		}

		private void InitPrices()
		{
			if( !FastMode )
			{
				FillDistancesPrices();
				FillAlignPrices();
			}

			LenEnc.TableSize = ( uint32 )( FastBytes + 1 - Lzma.Lzma1MinMatchLength );
			RepeatLenEnc.TableSize = ( uint32 )( FastBytes + 1 - Lzma.Lzma1MinMatchLength );

			RepeatLenEncCounter = LzmaEncoder.RepeatLengthCount;

			UpdateTables( LenEnc, 1u << PositionBits, LengthProbabilities );
			UpdateTables( RepeatLenEnc, 1u << PositionBits, RepeatLengthProbabilities );
		}

		private SevenZipResult AllocAndInit( uint32 keepWindowSize )
		{
			int32 shift;
			for( shift = Lzma.EndPositionModelIndex / 2; shift < LzmaEncoder.MaxDictionarySizeBits; shift++ )
			{
				if( DictionarySize <= ( 1u << shift ) )
				{
					break;
				}
			}

			DistanceTableSize = ( uint32 )( shift << 1 );

			Finished = false;
			Result = SevenZipResult.SevenZipOK;

			SevenZipResult result = AllocateMemory( keepWindowSize );
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			Init();
			InitPrices();

			NowPos64 = 0u;
			return SevenZipResult.SevenZipOK;
		}

		public SevenZipResult Prepare( InStreamInterface inStreamInterface, uint32 keepWindowSize )
		{
			MatchFinder.InStream = inStreamInterface;
			NeedInit = true;
			return AllocAndInit( keepWindowSize );
		}

		private SevenZipResult MemPrepare( uint8[] src, int64 srcLen, uint32 keepWindowSize )
		{
			MatchFinder.DirectInput = true;
			MatchFinder.BufferBase = src;
			MatchFinder.DirectInputRemaining = srcLen;
			NeedInit = true;

			SetDataSize( srcLen );
			return AllocAndInit( keepWindowSize );
		}

		public uint8[] GetBufferBase()
		{
			return MatchFinder.BufferBase;
		}

		public int64 GetCurrentOffset()
		{
			return MatchFinder.BufferOffset - AdditionalOffset;
		}

		private SevenZipResult ReportProgress()
		{
			if( Progress != null )
			{
				int64 processed = RangeCoder.Processed + RangeCoder.BufferOffset + RangeCoder.CacheSize;
				SevenZipResult result = Progress.Progress( NowPos64, processed );
				return ( result != SevenZipResult.SevenZipOK ) ? SevenZipResult.SevenZipErrorProgress : SevenZipResult.SevenZipOK;
			}

			return SevenZipResult.SevenZipOK;
		}

		public SevenZipResult GetCodedProperties( uint8[] properties, ref int64 size )
		{
			if( size < Lzma.LzmaPropertiesSize )
			{
				return SevenZipResult.SevenZipErrorParam;
			}

			size = Lzma.LzmaPropertiesSize;

			// Encode properties byte (LiteralContextBits, LiteralPositionBits, PositionBits)
			int32 full_properties = ( PositionBits * 5 + LiteralPositionBits ) * 9 + LiteralContextBits;
			properties[0] = ( uint8 )( full_properties & 0xff );

			// Encode aligned dictionary size for decoder
			uint32 encoded_dict_size;
			if( DictionarySize >= ( 1u << 21 ) )
			{
				// For large dictionaries (>= 2MB), align to 1MB boundary
				uint32 alignment_mask = ( 1u << 20 ) - 1u;
				encoded_dict_size = ( DictionarySize + alignment_mask ) & ~alignment_mask;

				// Prevent overflow from alignment
				encoded_dict_size = Math.Max( encoded_dict_size, DictionarySize );
			}
			else
			{
				// For small dictionaries, find next power-of-2-like size
				int8 shift = 11;
				do
				{
					encoded_dict_size = ( uint32 )( 2u + ( shift & 1 ) ) << ( shift >> 1 );
					shift++;
				} while( encoded_dict_size < DictionarySize );
			}

			// Write dictionary size as 4-byte little-endian
			properties[1] = ( uint8 )( ( encoded_dict_size >> 0 ) & 0xff );
			properties[2] = ( uint8 )( ( encoded_dict_size >> 8 ) & 0xff );
			properties[3] = ( uint8 )( ( encoded_dict_size >> 16 ) & 0xff );
			properties[4] = ( uint8 )( ( encoded_dict_size >> 24 ) & 0xff );

			return SevenZipResult.SevenZipOK;
		}

		public SevenZipResult CodeOneMemBlock( bool reInit, uint8[] baseDest, int64 offset, ref int64 destLen, uint32 desiredPackSize, ref uint32 unpackSize )
		{
			CheckedOutStreamInterface out_stream_interface = new CheckedOutStreamInterface( baseDest, offset, destLen );

			WriteEndMark = false;
			Finished = false;
			Result = SevenZipResult.SevenZipOK;

			if( reInit )
			{
				Init();
			}

			InitPrices();

			int64 now_pos_64 = NowPos64;
			RangeCoder.Init();
			RangeCoder.outStream = out_stream_interface;

			if( desiredPackSize == 0 )
			{
				return SevenZipResult.SevenZipErrorOutputEof;
			}

			SevenZipResult result = CodeOneBlock( desiredPackSize, unpackSize );

			unpackSize = ( uint32 )( NowPos64 - now_pos_64 );
			destLen = out_stream_interface.WorkBufferOffset - offset;
			if( out_stream_interface.Overflow )
			{
				return SevenZipResult.SevenZipErrorOutputEof;
			}

			return result;
		}

		private SevenZipResult MemEncode( uint8[] compressed, ref int64 compressedLength, uint8[] decompressed, int64 decompressedLength )
		{
			CheckedOutStreamInterface out_stream_interface = new CheckedOutStreamInterface( compressed, 0, compressedLength );

			RangeCoder.outStream = out_stream_interface;

			SevenZipResult result = MemPrepare( decompressed, decompressedLength, 0 );

			if( result == SevenZipResult.SevenZipOK )
			{
				while( !Finished && result == SevenZipResult.SevenZipOK )
				{
					result = CodeOneBlock( 0u, 0u );
					if( result == SevenZipResult.SevenZipOK )
					{
						result = ReportProgress();
					}
				}

				if( result == SevenZipResult.SevenZipOK && NowPos64 != decompressedLength )
				{
					result = SevenZipResult.SevenZipErrorFail;
				}
			}

			compressedLength = out_stream_interface.WorkBufferOffset;
			if( out_stream_interface.Overflow )
			{
				return SevenZipResult.SevenZipErrorOutputEof;
			}

			return result;
		}

		public static SevenZipResult Lzma1Encode( uint8[] compressed, ref int64 compressedLength, uint8[] decompressed, int64 decompressedLength, CLzmaEncoderProperties encoderProperties, ref uint8[] propsEncoded, ref int64 outPropsSize, ProgressInterface? progress )
		{
			Lzma1Enc enc1 = new Lzma1Enc( encoderProperties, progress );

			SevenZipResult result = enc1.GetCodedProperties( propsEncoded, ref outPropsSize );
			if( result == SevenZipResult.SevenZipOK )
			{
				result = enc1.MemEncode( compressed, ref compressedLength, decompressed, decompressedLength );
			}

			return result;
		}
	}
}
