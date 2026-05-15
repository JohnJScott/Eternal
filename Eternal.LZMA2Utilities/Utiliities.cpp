// Copyright Eternal Developments, LLC. All rights reserved.

#include <Windows.h>
#include <filesystem>
#include <string>
#include <fstream>

#include "../Eternal.LZMA2Simple/C/7zTypes.h"
#include "../Eternal.LZMA2Simple/C/Lzma1Lib.h"

CLzmaData LoadFile( const std::string& filename )
{
	CLzmaData compress;

	compress.SourceLength = std::filesystem::file_size( filename );
	compress.SourceData = new uint8[compress.SourceLength];

	std::ifstream instream( filename, std::ios::binary );
	instream.read( reinterpret_cast< char* >( compress.SourceData ), compress.SourceLength );
	instream.close();

	compress.DestinationLength = LzmaWorstCompression( compress.SourceLength );
	compress.DestinationData = new uint8[compress.DestinationLength];

	return compress;
}

uint8 LoadProperties( const std::string& filename )
{
	uint8 properties;

	std::ifstream instream( filename, std::ios::binary );
	instream.read( reinterpret_cast< char* >( &properties ), 1 );
	instream.close();

	return properties;
}

CLzmaData AllocateDecompressionBuffers( const CLzmaData& compressed, int64 compressedSize )
{
	CLzmaData decompress;

	decompress.SourceData = compressed.DestinationData;
	decompress.SourceLength = compressedSize;

	decompress.DestinationLength = compressed.SourceLength;
	decompress.DestinationData = new uint8[decompress.DestinationLength];

	return decompress;
}

void SetWorkingDirectory()
{
	// Attempts to set current working directory to base folder of unit test project
	HMODULE current_module = GetModuleHandleA( "Eternal.LZMA2SimpleTest.dll" );
	char current_module_path[MAX_PATH];
	GetModuleFileNameA( current_module, current_module_path, MAX_PATH );

	std::string path( current_module_path );
	path = path.substr( 0, path.find_last_of( '\\' ) ) + "\\..\\..\\..\\";

	SetCurrentDirectoryA( path.c_str() );

	std::filesystem::create_directories( "Intermediate\\TestData\\original" );
	std::filesystem::create_directories( "Intermediate\\TestData\\refactored" );
}

void WriteBinaryFile( const std::string& filename, const uint8* data, int64 size )
{
	std::ofstream outstream( filename, std::ios::binary );
	outstream.write( reinterpret_cast< const char* >( data ), size );
	outstream.close();
}

void WriteProperties( const std::string& filename, const uint8 data )
{
	std::ofstream outstream( filename, std::ios::binary );
	outstream.write( reinterpret_cast< const char* >( &data ), 1 );
	outstream.close();
}

