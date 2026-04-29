/*  LzmaEnc.h -- LZMA Encoder
2019-10-30 : Igor Pavlov : Public domain */

#pragma once

#include "7zTypes.h"
#include "Lzma1Lib.h"

/* ---------- Lzma1Enc Interface ---------- */

/* LzmaEnc* functions can return the following exit codes:
  SZ_OK           - OK
  SZ_ERROR_MEM    - Memory allocation error
  SZ_ERROR_PARAM  - Incorrect parameter in EncoderProperties
  SZ_ERROR_WRITE  - ISeqOutStream write callback error
  SZ_ERROR_OUTPUT_EOF - output buffer overflow - version with (uint8*) output
  SZ_ERROR_PROGRESS - some break from progress callback
  SZ_ERROR_THREAD - error in multithreading functions (only for Mt version)
*/

typedef uint16 CState;
typedef uint16 CExtra;
typedef uint32 CProbPrice;

namespace LzmaEncoder
{
	static constexpr int8 NumMoveReducingBits = 4;
	static constexpr int8 NumBitPriceShiftBits = 4;

	static constexpr uint8 EncodeStateStart = 0;
	static constexpr uint8 EncodeStateLiteralAfterMatch = 4;
	static constexpr uint8 EncodeStateLiteralAfterRepeat = 5;
	static constexpr uint8 EncodeStateMatchAfterLiteral = 7;
	static constexpr uint8 EncodeStateRepeatAfterLiteral = 8;

	static constexpr uint32 InfinityPrice = 1u << 30;
	static constexpr int64 RangeEncoderBufferSize = 1 << 16;

	static constexpr uint32 NumOptimals = 1u << 11;
	static constexpr uint32 PackReserveSize = NumOptimals << 3;
	static constexpr uint32 RepeatLengthCount = 64;
	static constexpr uint32 MarkLiteral = UINT32_MAX;

	static constexpr uint32 MaxDictionarySizeBits = 32;
	static constexpr uint32 DistanceTableSize = MaxDictionarySizeBits << 1;
}

typedef struct
{
	uint32 Price;
	CState State;
	CExtra Extra;
	// 0   : normal
	// 1   : LIT : MATCH
	// > 1 : MATCH (extra-1) : LIT : REP0 (len)
	uint32 Length;
	uint32 Distance;
	uint32 Repeats[Lzma::NumRepeats];
} COptimal;

class CLengthEncoder
{
public:
	void Init();

	CProbability Low[Lzma::MaxPositionBitsStates << ( Lzma::LengthEncoderNumLowBits + 1 )];
	CProbability High[Lzma::LengthEncoderNumHighSymbols];
};

typedef struct
{
	uint32 TableSize;
	uint32 Prices[Lzma::MaxPositionBitsStates][(Lzma::LengthEncoderNumLowSymbols << 1 ) + Lzma::LengthEncoderNumHighSymbols];
} CLengthPriceEncoder;

class CRangeEncoder
{
public:
	CRangeEncoder( MemoryInterface* alloc )
		: Alloc( alloc )
	{
	}

	~CRangeEncoder()
	{
		FreeMemory();
	}

	void FreeMemory();
	uint8* AllocateMemory();
	void Init();
	void FlushStream();
	void CheckFlush();
	void ShiftLow();
	void FlushData();
	void UpdateRange();
	void UpdateNewBoundUpdateRange( const uint32 bound );
	void SetNewBoundUpdateRange( const uint32 bound );
	uint32 CalcNewBound( const uint32 probability ) const;
	void IterateEndMark( CProbability& probability );
	void EncodeTreeAllOnes( CProbability* probabilities, int8 numBits );
	void EncodeBit0( CProbability& probability );
	void Iterate( CProbability* probabilityLocation, uint32 offset, const uint32 bit );
	void LengthEncode( CLengthEncoder& le, uint32 symbol, const uint32 posState );
	void Encode( CProbability* probabilities, uint32 arrayOffset, uint32 symbol );
	void EncodeMatched( CProbability* probabilities, uint32 arrayOffset, uint32 symbol, uint32 matchByte );
	void ReverseEncode( CProbability* probabilities, uint32 arrayOffset, int8 numBits, uint32 symbol );

	OutStreamInterface* outStream = nullptr;

	int64 Low = 0;
	int64 Processed = 0;
	int64 BufferOffset = 0;
	int64 CacheSize = 0;

	uint32 Range = 0;

	SevenZipResult Result = SevenZipResult::SevenZipOK;

private:
	MemoryInterface* Alloc = nullptr;
	uint8* RangeEncoderBuffer = nullptr;
	uint32 Cache = 0;
};

typedef struct
{
	CProbability* LiteralProbabilities;

	uint32 State;
	uint32 Repeats[Lzma::NumRepeats];

	CProbability PositionAlignEncoder[1 << Lzma::NumAlignmentBits];
	CProbability IsRep[Lzma::NumStates];
	CProbability IsRepG0[Lzma::NumStates];
	CProbability IsRepG1[Lzma::NumStates];
	CProbability IsRepG2[Lzma::NumStates];
	CProbability IsMatch[Lzma::NumStates][Lzma::MaxPositionBitsStates];
	CProbability IsRep0Long[Lzma::NumStates][Lzma::MaxPositionBitsStates];

	CProbability PositionSlotEncoder[Lzma::NumLengthToPositionStates][1 << Lzma::NumPositionSlotBits];
	CProbability PositionEncoders[Lzma::NumFullDistancesSize];

	CLengthEncoder LengthProbabilities;
	CLengthEncoder RepeatLengthProbabilities;
} CSaveState;

class Lzma1Enc
{
public:
	Lzma1Enc( const CLzmaEncoderProperties* encoderProperties, MemoryInterface* alloc, ProgressInterface* progress );
	~Lzma1Enc();

	const uint8* GetBufferBase() const;
	int64 GetCurrentOffset() const;
	SevenZipResult CodeOneMemBlock( bool reInit, uint8* baseDest, int64 offset, int64& destLen, uint32 desiredPackSize, uint32& unpackSize );
	void SaveState();
	void RestoreState();
	SevenZipResult GetCodedProperties( uint8* properties, uint64& size ) const;
	void SetDataSize( int64 expectedDataSize ) const;
	SevenZipResult Prepare( InStreamInterface* inStream, uint32 keepWindowSize );
	SevenZipResult MemEncode( uint8* compressed, int64& compressedLength, const uint8* decompressed, int64 decompressedLength );

private:
	SevenZipResult CheckErrors();
	uint32 GetPrice( const uint32 literalContext, uint32 symbol ) const;
	uint32 MatchedGetPrice( const uint32 literalContext, uint32 symbol, uint32 matchByte ) const;
	uint32 CalcTreeAlignPrice( uint32 symbol, uint32& m ) const;
	uint32 CalcTreeDistancePrice( uint32 lps, uint32 symbol ) const;
	static void SetPrices( const CProbability* baseArray, const uint32 baseOffset, const uint32 startPrice, uint32* prices, uint32 priceOffset );
	static void UpdateTables( CLengthPriceEncoder* lpe, const uint32 numPosStates, const CLengthEncoder* le );

	void FreeLits();
	void Lit( const uint32 nowPos32 );
	bool GetBestPrice( uint32& curRef, const uint32 last );
	uint32 GetPricePureRep( const uint32 repIndex, const uint64 state, const uint64 posState ) const;
	uint32 GetLength( uint32 nowPos32 );
	uint32 Backward( uint32 cur );
	uint32 GetOptimumFast();
	uint32 GetOptimum( uint32 position );
	void InitFirstOptimal( const uint32 position, const int64 dataOffset, const uint32 curByte, const uint32 matchByte, const uint32 positionState ) const;
	void GetOptimalRepeats( uint32* repeats, const uint32 previous, const uint32 distance ) const;
	void InitRepeatLengths( uint32* reps, uint32* repLens, const int64 dataOffset, const uint32 numAvail, uint32& repeatMaxIndex ) const;
	uint32 ReadMatchDistances( uint32& numPairs );
	uint32 LiteralBit( CRangeEncoder& re );
	void FillAlignPrices();
	uint32 GetPositionModelPrice( const uint32 positionIndex, const uint32 positionOffset, int8 footerBits, uint32& m ) const;
	void FillDistancesPrices();
	void ProcessMainMatchesInLoop( const uint32 cur, uint32& last, const uint32 newLength, const uint32 pairCount, const uint32 startLength, const uint32 numAvailFull ) const;
	void ProcessMainMatches( const uint32 mainLength, const uint32 pairCount, const uint32* repLens, const uint32 matchPrice, const uint32 positionState ) const;
	void TryRepLitRep0( const uint32 cur, uint32& last, const uint32 repIndex, const uint32 length, uint32 price, const int64 dataOffset, const uint32 numAvailFull ) const;
	void ProcessRepeatsInLoop( const uint32 cur, uint32& last, uint32& startLength, const uint32 numAvailFull ) const;
	void ProcessRepeatMatches( const uint32* repLens, const uint32 repeatMatchPrice, const uint32 positionState ) const;
	void TryShortRepeat( const uint32 curByte, const uint32 matchByte, const uint32* repLens, const uint32 repeatMatchPrice, const uint32 positionState, uint32& last );
	void TryLiteralRep0( const uint32 cur, uint32& last, const bool nextIsLiteral, const bool bytesMatch, const uint32 literalPrice, const uint32 numAvailFull ) const;
	void TryMatchLitRep0( const uint32 cur, uint32& last, const uint32 matchLength, const uint32 matchDistance, uint32 price, const uint32 numAvailFull ) const;
	uint32 CheckFastMatches( const uint32 mainLength, const uint32 pairCount, const uint32* repeatLengths, const uint32 repeatMaxIndex );
	SevenZipResult CodeOneBlock( uint32 maxPackSize, const uint32 maxUnpackSize );
	SevenZipResult AllocateMemory( uint32 keepWindowSize );
	void InitPrices();
	SevenZipResult AllocAndInit( uint32 keepWindowSize );
	SevenZipResult MemPrepare( const uint8* src, int64 srcLen, uint32 keepWindowSize );
	void Init();
	SevenZipResult ReportProgress() const;
	void WriteEndMarker( uint32 posState );
	void Flush( uint32 nowPos );

	MemoryInterface* Alloc = nullptr;
	ProgressInterface* Progress = nullptr;
	class CMatchFinder* MatchFinder = nullptr;
	CRangeEncoder RangeCoder;

	int32 LiteralContextBits = 0;
	int32 LiteralPositionBits = 0;
	int32 PositionBits = 0;
	uint32 FastBytes = 0;
	uint32 DictionarySize = 0;
	uint32 Repeats[Lzma::NumRepeats];

	bool FastMode = false;

	CProbability* LiteralProbabilities = nullptr;
	uint32* NewRepeats = nullptr;
	COptimal* Optimals = nullptr;

	int64 NowPos64 = 0;

	uint32 OptimalCurrent = 0u;
	uint32 OptimalEnd = 0u;
	uint32 LongestMatchLength = 0u;
	uint32 NumPairs = 0u;
	uint32 NumAvail = 0u;
	uint32 State = 0u;
	uint32 AdditionalOffset = 0u;
	uint32 LiteralMask = 0u;
	uint32 PositionMask = 0u;
	uint32 BackRes = 0u;
	int32 TotalLiteralBits = 0;

	uint32 MatchPriceCount = 0u;
	int32 RepeatLenEncCounter = 0;
	uint32 DistanceTableSize = 0u;

	uint32 Position = 0u;
	uint32 PositionState = 0u;
	uint32 NewState = 0u;
	uint32 NumAvailOther = 0u;
	uint32 MatchPrice = 0u;
	uint32 RepeatMatchPrice = 0u;

	// we want {len, dist} pairs to be 8-bytes aligned in matches array
	uint32 Matches[(Lzma::MaxMatchLength << 1 ) + 2];

	// we want 8-bytes alignment here
	uint32 AlignPrices[Lzma::AlignmentTableSize];

	uint32 DistancesPrices[Lzma::NumLengthToPositionStates][Lzma::NumFullDistancesSize];

	uint32 PositionSlotPrices[Lzma::NumLengthToPositionStates][LzmaEncoder::DistanceTableSize];

	CProbability PositionSlotEncoder[Lzma::NumLengthToPositionStates][1 << Lzma::NumPositionSlotBits];
	CProbability IsMatch[Lzma::NumStates][Lzma::MaxPositionBitsStates];
	CProbability IsRep0Long[Lzma::NumStates][Lzma::MaxPositionBitsStates];

	CProbability PositionAlignEncoder[1 << Lzma::NumAlignmentBits];
	CProbability IsRep[Lzma::NumStates];
	CProbability IsRepG0[Lzma::NumStates];
	CProbability IsRepG1[Lzma::NumStates];
	CProbability IsRepG2[Lzma::NumStates];
	CProbability PositionEncoders[Lzma::NumFullDistancesSize];

	CLengthPriceEncoder LenEnc;
	CLengthPriceEncoder RepeatLenEnc;

	CLengthEncoder LengthProbabilities;
	CLengthEncoder RepeatLengthProbabilities;

	CSaveState SavedState;

	static CProbPrice ProbabilityPrices[Lzma::BitModelTableSize >> LzmaEncoder::NumMoveReducingBits];

	bool Finished = false;
	bool NeedInit = false;
	bool WriteEndMark = false;
	SevenZipResult Result = SevenZipResult::SevenZipOK;
};

/* ---------- One Call Interface ---------- */
SevenZipResult Lzma1Encode( uint8* compressed, int64& compressedLength, const uint8* decompressed, int64 decompressedLength, const CLzmaEncoderProperties* encoderProperties, uint8* propsEncoded, uint64& outPropsSize, MemoryInterface* alloc, ProgressInterface* progress );
