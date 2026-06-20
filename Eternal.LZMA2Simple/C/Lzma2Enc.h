/* Lzma2Enc.h -- LZMA2 Encoder
2017-07-27 : Igor Pavlov : Public domain */

#pragma once

#include "Lzma2Lib.h"
#include "Lzma1Enc.h"

/** Lzma2Enc_* functions can return the following exit codes:
SevenZipResult:
  SevenZipOK             - OK
  SevenZipErrorMem       - Memory allocation error
  SevenZipErrorParam     - Incorrect parameter in EncoderProperties
  SevenZipErrorWrite     - ISeqOutStream write callback error
  SevenZipErrorOutputEof - output buffer overflow - version with (uint8*) output
  SevenZipErrorProgress  - some break from progress callback
  SevenZipErrorThread    - error in multithreading functions (only for Mt version)
*/

SevenZipResult Lzma2Encode( OutStreamInterface& outStream, InStreamInterface& inStream, const CLzma2EncoderProperties* encoderProperties, uint8* propertySummary, MemoryInterface* alloc, ProgressInterface* progress );
