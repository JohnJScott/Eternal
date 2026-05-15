// Copyright Eternal Developments, LLC. All rights reserved.

#pragma once

CLzmaData LoadFile( const std::string& filename );
uint8 LoadProperties( const std::string& filename );
CLzmaData AllocateDecompressionBuffers( const CLzmaData& compressed, int64 compressedSize );
void SetWorkingDirectory();
void WriteBinaryFile( const std::string& filename, const uint8* data, int64 size );
void WriteProperties( const std::string& filename, const uint8 data );
