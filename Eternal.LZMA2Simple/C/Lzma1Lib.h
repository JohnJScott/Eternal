/* Lzma1Lib.h -- LZMA library interface
2021-04-03 : Igor Pavlov : Public domain */

#pragma once

/**
 * ELzmaFinishMode has meaning only if the decoding reaches output limit !!!
 *
 * You must use LzmaFinishModeEnd, when you know that current output buffer
 *  covers last bytes of block. In other cases you must use LzmaFinishModeAny.
 *
 *  If LZMA Decoder sees end marker before reaching output limit, it returns SZ_OK,
 *  and output value of destLen will be less than output buffer size limit.
 *  You can check status result also.
 *
 *  You can use multiple checks to test data integrity after full decompression:
 *	 1) Check Result and "status" variable.
 *	 2) Check that output(destLen) = decompressedSize, if you know real decompressedSize.
 *	 3) Check that output(srcLen) = compressedSize, if you know real compressedSize.
 *		You must use correct finish mode in that case.
 */
enum class LzmaFinishMode
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
enum class LzmaStatus
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

inline int64 LzmaWorstCompression( int64 size )
{
	/** LZMA documentations states worst case compression is ( size * 0.001 ) + 32 */
	return ( size + ( ( size + 511 ) >> 9 ) ) + 32;
}

/** A container for the input and output buffers */
class CLzmaData
{
public:
	uint8* SourceData = nullptr;
	int64 SourceLength = 0;

	uint8* DestinationData = nullptr;
	int64 DestinationLength = 0;
};

class CLzma1Result
{
public:
	/** The encoded size of the dictionary - 40 means to find it. */
	uint8 Properties[5];

	/** The requested finish mode. */
	LzmaFinishMode FinishMode = LzmaFinishMode::LzmaFinishModeAny;

	/** The status of the decompression */
	LzmaStatus Status = LzmaStatus::LzmaStatusNotSpecified;

	/** The overall result; 0 for OK, positive for error */
	SevenZipResult Result = SevenZipResult::SevenZipOK;

	/** The number of bytes output from the compress/decompress operation */
	int64 OutputLength = 0;
};

class CLzmaEncoderProperties
{
public:
	/** 0 <= Level <= 9 */
	uint8 CompressionLevel = 5;

	/** LZMA2 has an additional limitation that LiteralContextBits + LiteralPositionBits <= 4 */
	/** 0 <= LiteralContextBits <= 8, default = 3 */
	uint8 LiteralContextBits = 3;

	/** 0 <= LiteralPositionBits <= 4, default = 0 */
	uint8 LiteralPositionBits = 0;

	/** 0 <= PositionBits <= 4, default = 2 */
	uint8 PositionBits = 2;

	/** false - do not write EOPM, true - write EOPM, default = false */
	bool WriteEndMark = false;

	/** LZMA1: 2 <= FastBytes <= 273, default = 32 if CompressionLevel < 7, or 64 */
	/** LZMA2: 5 <= FastBytes <= 273, default = 32 if CompressionLevel < 7, or 64 */
	int16 FastBytes = -1;

	/**
	 * Dictionary size is determined by the compression level, or set explicitly here.
	 * Must be at least 4096 ( 1u << 12 ).
	 */
	uint32 DictionarySize = 0u;

	/** 
	 * 1 <= MatchCycles <= (1 << 30), default = 32 
	 * This is one of the primary speed vs. compression trade-offs.
	 */
	uint32 MatchCycles = 32;

	/**
	 * Estimated size of data that will be compressed. default = UINT64_MAX (no estimate).
	 * The encoder will clamp the dictionary size to be no larger than this value.
	 */
	int64 EstimatedSourceDataSize = INT64_MAX;

	virtual SevenZipResult Normalize();

	uint32 GetDictionarySize() const;

	virtual ~CLzmaEncoderProperties() = default;
};

/**
 * The class containing the parameters used for compression.
 */
class CLzma1EncoderProperties
	: public CLzmaEncoderProperties
{
public:
	virtual SevenZipResult Normalize() override;
};

/*
RAM requirements for LZMA:
  for compression:   (DictionarySize * 11.5 + 6 MB) + state_size
  for decompression: DictionarySize + state_size
	state_size = (4 + (1.5 << (LiteralContextBits + LiteralPositionBits))) KB
	by default (LiteralContextBits=3, LiteralPositionBits=0), state_size = 16 KB.
*/

/*
LzmaCompress
------------
Returns:
  SZ_OK               - OK
  SZ_ERROR_MEM        - Memory allocation error
  SZ_ERROR_PARAM      - Incorrect parameter
  SZ_ERROR_OUTPUT_EOF - output buffer overflow
  SZ_ERROR_THREAD     - errors in multithreading functions (only for Mt version)
*/

SevenZipResult Lzma1Compress( const CLzmaData* data, CLzmaEncoderProperties* encoderProperties, CLzma1Result* result, MemoryInterface* alloc, ProgressInterface* progress );

/*
LzmaDecompress
--------------
Returns:
  SZ_OK                - OK
  SZ_ERROR_DATA        - Data error
  SZ_ERROR_MEM         - Memory allocation error
  SZ_ERROR_UNSUPPORTED - Unsupported properties
  SZ_ERROR_INPUT_EOF   - it needs more bytes in input buffer (src)
*/

SevenZipResult Lzma1Decompress( CLzmaData* data, CLzma1Result* result, MemoryInterface* alloc );
