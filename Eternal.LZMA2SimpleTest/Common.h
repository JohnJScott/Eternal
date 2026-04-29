// Copyright Eternal Developments, LLC. All rights reserved.

#pragma once

#include "CppUnitTest.h"
#include <chrono>
#include <stdint.h>
#include "../Eternal.LZMA2Simple/C/7zTypes.h"
#include "../Eternal.LZMA2Simple/C/Lzma1Enc.h"
#include "../Eternal.LZMA2Simple/C/Lzma1Lib.h"
#include "../Eternal.LZMA2Simple/C/Lzma2Lib.h"

namespace EternalLZMA2SimpleTest
{
	CLzmaData LoadFile( const std::string& filename );
	CLzmaData AllocateDecompressionBuffers( const CLzmaData& compressed, int64 compressedSize );

	void SetWorkingDirectory();
	void Log( const char* log, ... );
	void WriteBinaryFile( const std::string& filename, const uint8* data, int64 size );
}

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
