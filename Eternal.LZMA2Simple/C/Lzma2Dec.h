/* Lzma2Dec.h -- LZMA2 Decoder
2018-02-19 : Igor Pavlov : Public domain */

#pragma once

/*
finishMode:
  It has meaning only if the decoding reaches output limit (*destLen).
  LzmaFinishModeAny - use smallest number of input bytes
  LzmaFinishModeEnd - read EndOfStream marker after decoding

Returns:
  SevenZipOK
	status:
	  LzmaStatusFinishedWithMark
	  LzmaStatusNotFinished
  SevenZipErrorData         - Data error
  SevenZipErrorMem          - Memory allocation error
  SevenZipErrorUnsupported  - Unsupported properties
  SevenZipErrorInputEof     - It needs more bytes in input buffer (src).
*/

SevenZipResult Lzma2Decode( uint8* decompressed, int64& decompressedLength, const uint8* compressed, int64& compressedLength, const uint8 prop, LzmaFinishMode finishMode, LzmaStatus& status, MemoryInterface* alloc );
