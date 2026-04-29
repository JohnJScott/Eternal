/* 7zTypes.h -- Basic types
2021-12-25 : Igor Pavlov : Public domain */

#pragma once

#include <stddef.h>
#include <stdlib.h>
#include <stdint.h>
#include <string.h>

#ifdef __cplusplus
#include <algorithm>
#endif

// Reduced set of types
typedef int16_t int16;
typedef int32_t int32;
typedef int64_t int64;
typedef int8_t int8;
typedef uint16_t uint16;
typedef uint32_t uint32;
typedef uint64_t uint64;
typedef uint8_t uint8;

// Int32 -> int32
// UInt32 -> uint32
// unsigned -> uint32

// Int64 -> int64
// UInt64 -> uint64
// size_t -> uint64
// SizeT -> uint64
// ptrdiff_t -> int64

typedef uint16 CProbability;

enum class SevenZipResult
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

#define USE_BLOCK_SIZE_LOOKUP			1

namespace Lzma
{
	// Common constants
	static constexpr int8 NumAlignmentBits = 4;
	static constexpr uint32 AlignmentTableSize = 1u << NumAlignmentBits;
	static constexpr uint32 AlignmentTableMask = AlignmentTableSize - 1u;

	static constexpr int8 LengthEncoderNumLowBits = 3;
	static constexpr uint32 LengthEncoderNumLowSymbols = 1u << LengthEncoderNumLowBits;
	static constexpr int8 LengthEncoderNumHighBits = 8;
	static constexpr uint32 LengthEncoderNumHighSymbols = 1u << LengthEncoderNumHighBits;

	static constexpr uint32 LzmaPropertiesSize = 5u;
	static constexpr uint8 MaxPositionBits = 4;
	static constexpr uint32 MaxPositionBitsStates = 1u << MaxPositionBits;
	static constexpr uint8 MaxLiteralContextBits = 8;
	static constexpr uint8 MaxLiteralPositionBits = 4;
	static constexpr uint32 MinDictionarySize = 1u << 12;
	static constexpr uint32 MaxDictionarySize = 15u << 28;
	static constexpr int16 Lzma1MinMatchLength = 2;
	static constexpr int16 Lzma2MinMatchLength = 5;
	static constexpr int16 MaxMatchLength = Lzma1MinMatchLength + ( ( LengthEncoderNumLowSymbols << 1 ) + LengthEncoderNumHighSymbols ) - 1;

	static constexpr int8 NumMoveBits = 5;
	static constexpr int8 NumPositionSlotBits = 6;

	static constexpr int8 NumBitModelTotalBits = 11;
	static constexpr uint32 BitModelTableSize = 1u << NumBitModelTotalBits;
	static constexpr CProbability InitProbabilityValue = BitModelTableSize >> 1;
	static constexpr uint32 BitModelTableMask = BitModelTableSize - 1u;

	static constexpr uint32 StartPositionModelIndex = 4u;
	static constexpr int8 EndPositionModelIndex = 14;
	static constexpr uint32 NumFullDistancesSize = 1u << ( EndPositionModelIndex >> 1 );
	static constexpr uint32 NumFullDistancesMask = NumFullDistancesSize - 1u;

	static constexpr uint32 LiteralSize = 768u;
	static constexpr uint32 NumStates = 12u;
	static constexpr uint32 NumLengthToPositionStates = 4u;
	static constexpr int8 NumLogBits = 11 + ( 3 * ( sizeof( uint64 ) / 8 ) );
	static constexpr uint32 NumLogTableSize = 1u << NumLogBits;
	static constexpr uint32 MaxRangeValue = 1u << 24;
	static constexpr uint32 NumRepeats = 4u;

	// Lzma2 specific constants
	static constexpr uint8 MaxCombinedLiteralBits = 4;

	static constexpr uint8 Lzma2ControlEof = 0u;
	static constexpr uint8 Lzma2ControlCopyResetDict = 1u;
	static constexpr uint8 Lzma2ControlCopy = 2u;
	static constexpr uint8 Lzma2ControlLzma = 1u << 7;

	static constexpr uint32 Lzma2MaxPackSize = 1u << 16;
	static constexpr uint32 Lzma2CopyChunkSize = Lzma2MaxPackSize;
	static constexpr uint32 Lzma2MaxUnpackSize = 1u << 21;
	static constexpr uint32 Lzma2KeepWindowSize = Lzma2MaxUnpackSize;

	static constexpr uint32 Lzma2MaxCompressedChunkSize = ( 1u << 16 ) + 16u;

	static constexpr uint8 LiteralNextStateLut[NumStates] = { 0, 0, 0, 0, 1, 2, 3, 4,  5,  6, 4, 5 };
	static constexpr uint8 MatchNextStateLut[NumStates] = { 7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10 };
	static constexpr uint8 RepNextStateLut[NumStates] = { 8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11 };
	static constexpr uint8 ShortRepNextStateLut[NumStates] = { 9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11 };
}

/**
 * Interface for sequential input stream.
 */
class InStreamInterface
{
public:
	InStreamInterface() = default;
	virtual ~InStreamInterface() = default;

	/** 
	 * if ( input( *size ) != 0 && output( *size ) == 0 ) means end_of_stream.  
	 * ( output( *size ) < input( *size ) ) is allowed 
	 */
	virtual SevenZipResult Read( uint8* bufferBase, const int64 offset, int64* size ) = 0;
};

/** 
 * Interface for sequential output stream.
 */
class OutStreamInterface
{
public:
	OutStreamInterface() = default;
	virtual ~OutStreamInterface() = default;

	/** 
	 * Returns the number of actually written bytes. ( result < size ) means error.
	 */
	virtual int64 Write( const uint8* bufferBase, const int64 offset, int64 size ) = 0;
};

/**
 * Interface for progress reporting during compression.
 */
class ProgressInterface
{
public:
	ProgressInterface() = default;
	virtual ~ProgressInterface() = default;

	/** 
	 * Returns: SZ_RESULT. (result != SZ_OK) means break.
	 * Value UINT64_MAX for size means unknown value. 
	 */
	virtual SevenZipResult Progress( int64 inSize, int64 outSize ) = 0;
};

/**
 * Interface for memory allocation which includes a default implementation.
 */
class MemoryInterface
{
public:
	MemoryInterface() = default;
	virtual ~MemoryInterface() = default;

	/** 
	 * Allocates size bytes of memory if size > 0. Returns nullptr if size == 0. 
	 */
	virtual void* Alloc( const int64 size, const char* tag )
	{
		( void )tag;

		if( size != 0 )
		{
			void* address = malloc( static_cast<uint64>( size ) );
			memset( address, 0, static_cast< uint64 >( size ) );
			return address;
		}

		return nullptr;
	}

	/** 
	 * Frees up the memory allocated by Alloc above. 
	 * The size is passed in to this function so as to allow validation that the same amount of memory was freed as was allocated. 
	 */
	virtual void Free( void* address, const int64 size, const char* tag )
	{
		( void )size;
		( void )tag;

		if( address != nullptr )
		{
			free( address );
		}
	}
};

/**
 * Lookup table for block sizes which is used if USE_BLOCK_SIZE_LOOKUP is defined to 1.
 * There is a Code version of this which saves about 16KB of memory, but is about 5% slower.
 */
extern const uint8 BlockSizeLookup[1u << Lzma::NumLogBits];

