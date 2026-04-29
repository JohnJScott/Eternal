# Eternal Lzma2Simple

## Eternal.Lzma2SimpleCS NuGet library package
Copyright 2026 Eternal Developments, LLC. All Rights Reserved.

## License

MIT

# Functionality
## Lzma2Simple

A simplified library to compress and decompress using the 7-Zip libary. It has targets for Windows x64, Linux x64, and C# and is based on the 25.01 version of 7-Zip.

I am working on another project that uses 7-Zip extensively; basically, I compress many smaller blocks in C++ and then visualize in PaintDotNET. This meant I needed a solid C# implementation of the decompressor. As the reference
implementation uses macros extensively, a direct port of the C code to C# was not practical. What I did was convert the C code to C++, remove a lot of the dead code, and removed several of the unused options.

The biggest limitation is that C# does not support arrays greater than 2GB. This means files greater than 2GB can't be compressed or decompressed with the C# library. The Array helpers (such as Fill and Copy) all
take 64 bit indices, so maybe future versions will.

Usage:
	// A source buffer with the uncompressed data. A destination buffer with enough storage to store the worst possible compression.
	CLzmaData data; 
	// Encoder properties - defaults are set in the constructor.
	CLzma1EncoderProperties encoder1_properties;
	// The result of the compression
	CLzma1Result compress_result;
	// The memory interface can be used to override all memory operation, and the progress interface can be used to report progress
	Lzma1Compress( &data, encoder_properties, &compress_result, &memory_interface, &progress_interface );
	
Note: the compress_result returns an array of 5 bytes that need passing into the decompressor

	// A source buffer with the compressed data. A destionation buffer set to the size of the decompressed data.
	CLzmaData data; 
	CLzma1Result decompess_result;
	memcpy_s( decompress_result.Properties, 5, compress_result.properties, 5 );
	Lzma1Decompress( &decompress, &result, &memory_interface );
	
A very similar process is used for Lzma2. The differences being:
CLzma2EncoderProperties has the same data members, but there are different limitations.
CLzma2Result has only a single byte for the properties, not 5.

Look at the function CreateLzma2Props() in https://github.com/JohnJScott/Eternal/blob/master/Eternal.LZMA2SimpleTest/OriginalComparisonTests.cpp to see the removed encoder settings.

# Unit test priorities:
0 - Generate various lookup tables
11 - Various C++ Lzma1 compress/decompress validation
12 - Various C++ Lzma2 compress/decompress validation
21 - Various C# Lzma1 compress/decompress validation
22 - Various C# Lzma2 compress/decompress validation
50 - Ensures the refactored C++ compressed data matches the compressed data from the vanilla 7-Zip SDK
60 - Ensures the C# compressed data matches the C++ compressed data.

The OriginalSevenZip project references the original SDK; it's just a compile wrapper.
Given this readme resides at ~/Eternal/Eternal.LZMA2Simple/
Put the 7-Zip SDK at ~/ThirdParty/7-Zip/lzma2501/ to make everything work seamlessly.

Performance: The refactored C++ decompress seems to be about 50% slower than the reference version. This is unacceptable and I'm working on it. Run the test TestCompareExhaustiveBC3 to get test output in a CSV format.

# Changes 21st April 2026

Initial release

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)

