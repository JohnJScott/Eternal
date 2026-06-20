/* LzmaEnc.c -- LZMA Encoder
2021-11-18: Igor Pavlov : Public domain */

#pragma once
#include "Lzma1Enc.h"

enum class LzmaDummy
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

/* ---------- LZMA Properties ---------- */

typedef struct
{
	uint8 LiteralContextBits;
	uint8 LiteralPositionBits;
	uint8 PositionBits;
	uint32 DictionarySize;
} CLzmaDecoderProperties;

/* ---------- LZMA Parameters ---------- */

typedef struct
{
	const uint8* DataBufferBase;
	int64 DataBufferOffset;
	uint32 Range;
	uint32 Code;
	uint32 State;
	uint32 Symbol;
	uint32 Offset;
	uint32 Bit;
} CParameters;

/* ---------- LZMA Decoder State ---------- */

/* LZMA_REQUIRED_INPUT_MAX = number of required input bytes for worst case.
   Num bits = log2((2^11 / 31) ^ 22) + 26 < 134 + 26 = 160; */

class Lzma1Dec
{
public:
	Lzma1Dec() = default;

	Lzma1Dec( uint8* decompressed, MemoryInterface* alloc )
		: Alloc( alloc )
		, Dictionary( decompressed )
	{
	}

	/** Decode the Lzma 5 byte array to dictionary size and decompression parameters. */
	SevenZipResult DecodeProperties( const uint8* decoderProperties, const uint32 propsSize );
	void InitDictAndState( bool initDict, bool initState );
	void UpdateWithDecompressed( const uint8* src, const int64 offset, const int64 size );
	SevenZipResult AllocateProbabilities();
	void FreeProbabilities();

	/**
	 * LzmaDec_DecodeToDict
	 * The decoding to internal dictionary buffer (Lzma1Dec::Dictionary).
	 *  You must manually update Lzma1Dec::DictionaryPosition, if it reaches Lzma1Dec::DictionaryBufferSize !!!
	 *
	 * finishMode:
	 *  It has meaning only if the decoding reaches output limit (dicLimit).
	 *  LzmaFinishModeAny - Decode just dicLimit bytes.
	 *  LzmaFinishModeEnd - Stream must be finished after dicLimit.
	 *
	 * Returns:
	 * SevenZipOK
	 *	status:
	 *	  LzmaStatusFinishedWithMark
	 *	  LzmaStatusNotFinished
	 *	  LzmaStatusNeedsMoreInput
	 *	  LzmaStatusMaybeFinishedWithoutMark
	 * SZ_ERROR_DATA - Data error
	 * SZ_ERROR_FAIL - Some unexpected error: internal error of Code, memory corruption or hardware failure
	 */
	SevenZipResult DecodeToDict( const int64 dicLimit, const uint8* compressed, int64 compressedOffset, int64& compressedLength, LzmaFinishMode finishMode, LzmaStatus& status );

	CLzmaDecoderProperties DecoderProperties{};
	CProbability* Probabilities = nullptr;
	int64 DictionaryBufferSize = 0;
	int64 DictionaryPosition = 0;

	uint32 PositionMask = 0u;
	uint32 LiteralMask = 0u;

private:
	static LzmaDummy UpdateRangeDummy( CParameters& parameters, const int64 bufLimitOffset );
	static void UpdateRange( CParameters& parameters );
	static bool SimpleIterate( CParameters& parameters, CProbability& probability );
	static bool DummySimpleIterate( CParameters& parameters, const CProbability probability );

	static LzmaDummy DummyDecodeTreeBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset );
	static LzmaDummy DummyDecodeMatchedLiteralBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset );
	static LzmaDummy DummyDecodePositionModelBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset );
	static LzmaDummy DummyConsumeBit( CParameters& parameters, const CProbability& probability, const int64 bufLimitOffset );

	static void DecodeTreeBit( CParameters& parameters, CProbability& probability ) ;
	static void DecodeMatchedLiteralBit( CParameters& parameters, CProbability& probability );
	static void DecodeReverseBit( CParameters& parameters, CProbability& probability );
	static void DecodeReverseBitLast( CParameters& parameters, CProbability& probability );
	static void DecodePositionModelBit( CParameters& parameters, CProbability& probability );

	void UpdateDistance( CParameters& parameters );
	void UpdateSymbol( CParameters& parameters );
	uint32 UpdateLength( CParameters& parameters, uint32 probabilityOffset, const uint32 positionState ) const;
	bool UpdateRepeatLength( CParameters& parameters, uint32& length );
	uint32 FinishBlock( const uint32 length, const int64 remaining );
	bool DecodeShortRepeat( CParameters& parameters, const uint32 positionState );
	uint32 DecodeMatchType( CParameters& parameters, const uint32 positionState );
	uint32 DecodeLowLength( CParameters& parameters, const uint32 positionState ) const;
	SevenZipResult DecodeRealInternal( int64 limit, const int64 bufLimitOffset );
	void WriteRemaining( const int64 limit );
	SevenZipResult DecodeReal( int64 limit, const int64 bufLimitOffset );
	LzmaDummy TryDummyLit( CParameters& parameters, const int64 bufOutOffset ) const;
	LzmaDummy TryDummyRep( CParameters& parameters, int64& probOffset, const int64 bufOutOffset, const uint32 posState ) const;
	LzmaDummy TryDummyDistance( CParameters& parameters, const int64 bufOutOffset, const uint32 length ) const;
	LzmaDummy TryDummy( const uint8* buffer, int64 bufferOffset, int64& bufOutOffset ) const;

	MemoryInterface* Alloc = nullptr;
	
	uint8* Dictionary = nullptr;
	uint32 RepeatDistances[Lzma::NumRepeats] = { 0 };

	CParameters Parameters{};

	uint32 ProcessedPosition = 0u;
	uint32 CheckDictionarySize = 0u;
	uint32 RemainingLength = 0u;

	uint32 NumProbabilities = 0u;
	uint32 TempBufferSize = 0u;
};

/* There are two types of LZMA streams:
	 - Stream with end mark. That end mark adds about 6 bytes to compressed size.
	 - Stream without end mark. You must know the exact decompressed size to decompress such stream. */

/* ---------- Dictionary Interface ---------- */

/* You can use it, if you want to eliminate the overhead for data copying from
   dictionary to some other external buffer.
   You must work with Lzma1Dec variables directly in this interface.

   STEPS:
	 LzmaDec_Allocate()
	 for (each new stream)
	 {
	   LzmaDec_Init()
	   while (it needs more decompression)
	   {
		 LzmaDec_DecodeToDict()
		 use data from Lzma1Dec::Dictionary and update Lzma1Dec::DictionaryPosition
	   }
	 }
	 LzmaDec_Free()
*/

/** Lzma1Decode

finishMode:
  It has meaning only if the decoding reaches output limit (*destLen).
  LzmaFinishModeAny - Decode just destLen bytes.
  LzmaFinishModeEnd - Stream must be finished after (*destLen).

Returns:
  SevenZipOK
	status:
	  LzmaStatusFinishedWithMark
	  LzmaStatusNotFinished
	  LzmaStatusMaybeFinishedWithoutMark
  SevenZipErrorData        - Data error
  SevenZipErrorMem         - Memory allocation error
  SevenZipErrorUnsupported - Unsupported properties
  SevenZipErrorInputEof    - It needs more bytes in input buffer (src).
  SevenZipErrorFail        - Some unexpected error: internal error of Code, memory corruption or hardware failure
*/

SevenZipResult Lzma1Decode( uint8* decompressed, int64& decompressedLength, const uint8* compressed, int64& compressedLength, const uint8* propData, uint32 propSize, const LzmaFinishMode finishMode, LzmaStatus& status, MemoryInterface* alloc );
