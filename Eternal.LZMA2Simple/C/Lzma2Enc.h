/* Lzma2Enc.h -- LZMA2 Encoder
2017-07-27 : Igor Pavlov : Public domain */

#pragma once

#include "Lzma2Lib.h"
#include "Lzma1Enc.h"


/* ---------- CLzmaEnc2Handle Interface ---------- */

/* Lzma2Enc_* functions can return the following exit codes:
int32:
  SZ_OK           - OK
  SZ_ERROR_MEM    - Memory allocation error
  SZ_ERROR_PARAM  - Incorrect parameter in EncoderProperties
  SZ_ERROR_WRITE  - ISeqOutStream write callback error
  SZ_ERROR_OUTPUT_EOF - output buffer overflow - version with (uint8*) output
  SZ_ERROR_PROGRESS - some break from progress callback
  SZ_ERROR_THREAD - error in multithreading functions (only for Mt version)
*/

SevenZipResult Lzma2Encode( OutStreamInterface& outStream, InStreamInterface& inStream, const CLzma2EncoderProperties* encoderProperties, uint8* propertySummary, MemoryInterface* alloc, ProgressInterface* progress );
