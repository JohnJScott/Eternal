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



# Changes 21st April 2026

Initial release

# Notes

Full Doxygen documentation at https://eternaldevelopments.com/docs

This utility appeals to the most niche aspects of development, but if you feel like making a donation, please send DOGE to DFbEt36Qg2s2CVAdk5hZgRJfH8p1g6tW9i or buy a [#programmerlife t-shirt](https://www.bonfire.com/store/programmer-life/)

