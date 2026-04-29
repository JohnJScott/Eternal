/* Lzma2Lib.h -- LZMA2 library interface
2021-04-03 : Igor Pavlov : Public domain */

#pragma once

#include "Lzma1Lib.h"

class CLzma2Result
{
public:
	/** The encoded size of the dictionary - 40 means to find it. */
	uint8 PropertySummary = 0;

	/** The requested finish mode. */
	LzmaFinishMode FinishMode = LzmaFinishMode::LzmaFinishModeEnd;

	/** The status of the decompression */
	LzmaStatus Status = LzmaStatus::LzmaStatusNotSpecified;

	/** The overall result. */
	SevenZipResult Result = SevenZipResult::SevenZipOK;

	/** The number of bytes output from the compress/decompress operation */
	int64 OutputLength = 0;
};

class CLzma2EncoderProperties
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

/**
LzmaCompress - compress a block of memory
------------
Returns:
  SZ_OK               - OK
  SZ_ERROR_MEM        - Memory allocation error
  SZ_ERROR_PARAM      - Incorrect parameter
  SZ_ERROR_OUTPUT_EOF - output buffer overflow
  SZ_ERROR_THREAD     - errors in multithreading functions (only for Mt version)
*/

SevenZipResult Lzma2Compress( const CLzmaData* data, CLzma2EncoderProperties* encoderProperties, CLzma2Result* result, MemoryInterface* alloc, ProgressInterface* progress );

/**
 * Lzma2Decompress - decompress a block of memory
 * Returns:
 * SZ_OK                - OK
 * SZ_ERROR_DATA        - Data error
 * SZ_ERROR_MEM         - Memory allocation arror
 * SZ_ERROR_UNSUPPORTED - Unsupported properties
 * SZ_ERROR_INPUT_EOF   - it needs more bytes in input buffer (src)
 */
SevenZipResult Lzma2Decompress( CLzmaData* data, CLzma2Result* result, MemoryInterface* alloc );
