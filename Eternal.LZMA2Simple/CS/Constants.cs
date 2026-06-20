// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int16 = Int16;
	using int32 = Int32;
	using int8 = SByte;
	using int64 = Int64;
	using uint16 = UInt16;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	using CProbability = UInt16;
	using CProbPrice = UInt32;

	public enum SevenZipResult
		: int8
	{
		SevenZipOK = 0,
		SevenZipErrorData = 1,
		SevenZipErrorMemory = 2,
		SevenZipErrorCrc = 3,
		SevenZipErrorUnsupported = 4,
		SevenZipErrorParam = 5,
		SevenZipErrorInputEof = 6,
		SevenZipErrorOutputEof = 7,
		SevenZipErrorRead = 8,
		SevenZipErrorWrite = 9,
		SevenZipErrorProgress = 10,
		SevenZipErrorFail = 11,
		SevenZipErrorThread = 12,
		SevenZipErrorArchive = 16,
		SevenZipErrorNoArchive = 17
	};

	/// <summary>Interface for sequential input stream.</summary>
	public abstract class InStreamInterface
	{
		/// <summary>
		/// Reads bytes from the stream into the provided buffer.
		/// </summary>
		/// <remarks>If input size is non-zero and output size is zero, the stream has ended. Output size less than input size is allowed.</remarks>
		/// <param name="bufferBase">Destination buffer to read into.</param>
		/// <param name="offset">Byte offset within bufferBase to start writing.</param>
		/// <param name="size">On entry: maximum bytes to read. On exit: bytes actually read (0 signals end of stream).</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public abstract SevenZipResult Read( uint8[] bufferBase, int64 offset, ref int64 size );
	};

	/// <summary>Interface for sequential output stream.</summary>
	abstract class OutStreamInterface
	{
		/// <summary>
		/// Writes bytes from the provided buffer to the stream.
		/// </summary>
		/// <param name="bufferBase">Source buffer to write from.</param>
		/// <param name="offset">Byte offset within bufferBase to start reading.</param>
		/// <param name="blockSize">Number of bytes to write.</param>
		/// <returns>Number of bytes actually written; a value less than blockSize indicates an error.</returns>
		public abstract int64 Write( uint8[] bufferBase, int64 offset, int64 blockSize );
	};

	/// <summary>Interface for progress reporting during compression.</summary>
	public abstract class ProgressInterface
	{
		/// <summary>
		/// Reports compression progress.
		/// </summary>
		/// <param name="inSize">Number of uncompressed bytes processed so far; INT64_MAX means unknown.</param>
		/// <param name="outSize">Number of compressed bytes produced so far; INT64_MAX means unknown.</param>
		/// <returns>SevenZipOK to continue; any other value aborts the operation.</returns>
		public abstract SevenZipResult Progress( int64 inSize, int64 outSize );
	};

	public static partial class Lzma
	{
		// Static constructor to initialize static readonly fields
		static Lzma()
		{
		}

		// Common constants
		public const int8 NumAlignmentBits = 4;
		public const uint32 AlignmentTableSize = 1u << NumAlignmentBits;
		public const uint32 AlignmentTableMask = AlignmentTableSize - 1u;

		public const int8 LengthEncoderNumLowBits = 3;
		public const uint32 LengthEncoderNumLowSymbols = 1u << LengthEncoderNumLowBits;
		public const int8 LengthEncoderNumHighBits = 8;
		public const uint32 LengthEncoderNumHighSymbols = 1u << LengthEncoderNumHighBits;

		public const uint32 LzmaPropertiesSize = 5u;
		public const uint8 MaxPositionBits = 4;
		public const uint32 MaxPositionBitsStates = 1u << MaxPositionBits;
		public const uint8 MaxLiteralContextBits = 8;
		public const uint8 MaxLiteralPositionBits = 4;
		public const uint32 MinDictionarySize = 1u << 12;
		public const uint32 MaxDictionarySize = 15u << 28;
		public const int16 Lzma1MinMatchLength = 2;
		public const int16 Lzma2MinMatchLength = 5;
		public const int16 MaxMatchLength = ( int16 )( Lzma1MinMatchLength + ( ( LengthEncoderNumLowSymbols << 1 ) + LengthEncoderNumHighSymbols ) - 1 );
		
		public const int8 NumMoveBits = 5;
		public const int8 NumPositionSlotBits = 6;

		public const int8 NumBitModelTotalBits = 11;
		public const uint32 BitModelTableSize = 1u << NumBitModelTotalBits;
		public const CProbability InitProbabilityValue = ( CProbability )( BitModelTableSize >> 1 );
		public const uint32 BitModelTableMask = BitModelTableSize - 1u;

		public const uint32 StartPositionModelIndex = 4u;
		public const int8 EndPositionModelIndex = 14;
		public const uint32 NumFullDistancesSize = 1u << ( EndPositionModelIndex >> 1 );
		public const uint32 NumFullDistancesMask = NumFullDistancesSize - 1u;

		public const uint32 LiteralSize = 768u;
		public const uint32 NumStates = 12u;
		public const uint32 NumLengthToPositionStates = 4u;
		public const int8 NumLogBits = 11 + ( 3 * ( sizeof( uint64 ) / 8 ) );
		public const uint32 NumLogTableSize = 1u << NumLogBits;
		public const uint32 MaxRangeValue = 1u << 24;
		public const uint32 NumRepeats = 4u;

		// Lzma2 specific constants
		public const uint8 MaxCombinedLiteralBits = 4;

		public const uint8 Lzma2ControlEof = 0;
		public const uint8 Lzma2ControlCopyResetDict = 1;
		public const uint8 Lzma2ControlCopy = 2;
		public const uint8 Lzma2ControlLzma = 1 << 7;

		// Lzma2 encoder constants
		public const uint32 Lzma2MaxPackSize = 1u << 16;
		public const uint32 Lzma2CopyChunkSize = Lzma2MaxPackSize;
		public const uint32 Lzma2MaxUnpackSize = 1u << 21;
		public const uint32 Lzma2KeepWindowSize = Lzma2MaxUnpackSize;

		public const uint32 Lzma2MaxCompressedChunkSize = ( 1u << 16 ) + 16u;

		public static readonly uint8[] LiteralNextStateLut = [0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 4, 5];
		public static readonly uint8[] MatchNextStateLut = [7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10];
		public static readonly uint8[] RepNextStateLut = [8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11];
		public static readonly uint8[] ShortRepNextStateLut = [9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11];
	}
}
