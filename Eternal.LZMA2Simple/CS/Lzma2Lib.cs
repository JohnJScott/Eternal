// Copyright Eternal Developments, LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int64 = Int64;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	public class CLzma2Result
	{
		/** The encoded size of the dictionary - 40 means to find it. */
		public uint8 PropertySummary = 0;

		/** The overall result. */
		public SevenZipResult Result = SevenZipResult.SevenZipOK;

		/** The number of bytes output from the compress/decompress operation */
		public int64 OutputLength = 0;
	}

	public class Lzma2Lib
	{
		private class FMemoryWriter( uint8[] destinationData, int64 size ) 
			: OutStreamInterface
		{
			/// <summary>
			/// Writes bytes from the provided buffer into the destination memory block.
			/// </summary>
			/// <param name="bufferBase">Source buffer to write from.</param>
			/// <param name="offset">Byte offset within bufferBase to start reading.</param>
			/// <param name="blockSize">Number of bytes to write.</param>
			/// <returns>Number of bytes actually written; 0 if the destination buffer would be exceeded.</returns>
			public override int64 Write( uint8[] bufferBase, int64 offset, int64 blockSize )
			{
				if( Offset + blockSize < size )
				{
					Array.Copy( bufferBase, offset, destinationData, Offset, blockSize );
					Offset += blockSize;
					return blockSize;
				}

				return 0;
			}

			/// <summary>
			/// Returns the number of bytes written to the output buffer so far.
			/// </summary>
			/// <returns>Current write position as a byte offset from the start of the buffer.</returns>
			public int64 GetOffset()
			{
				return Offset;
			}

			private int64 Offset = 0;
		};

		private class FMemoryReader( uint8[] sourceData, int64 size ) 
			: InStreamInterface
		{
			/// <summary>
			/// Reads bytes from the source memory block into the provided buffer.
			/// </summary>
			/// <param name="bufferBase">Destination buffer to read into.</param>
			/// <param name="offset">Byte offset within bufferBase to start writing.</param>
			/// <param name="size1">On entry: maximum bytes to read. On exit: bytes actually read (0 signals end of stream).</param>
			/// <returns>SevenZipOK on success.</returns>
			public override SevenZipResult Read( uint8[] bufferBase, int64 offset, ref int64 size1 ) 
			{
				if( Offset == size )
				{
					size1 = 0;
					return SevenZipResult.SevenZipOK;
				}

				if( Offset + size1 > size )
				{
					size1 = size - Offset;
				}

				Array.Copy( sourceData, ( int64 )Offset, bufferBase, ( int64 )offset, ( int64 )size1 );
				Offset += size1;
				return SevenZipResult.SevenZipOK;
			}

			private int64 Offset = 0;
		};

		/// <summary>
		/// Compresses a block of memory using LZMA2.
		/// </summary>
		/// <param name="data">Source and destination buffers with their sizes.</param>
		/// <param name="encoderProperties">Encoder configuration parameters.</param>
		/// <param name="result">Receives the compression result, property summary byte, and output length.</param>
		/// <param name="progress">Optional progress callback; pass null to disable.</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public static SevenZipResult Lzma2Compress( CLzmaData data, CLzma2EncoderProperties encoderProperties, out CLzma2Result result, ProgressInterface? progress )
		{
			result = new CLzma2Result();
			result.Result = encoderProperties.Normalize();
			if( result.Result != SevenZipResult.SevenZipOK )
			{
				return result.Result;
			}

			FMemoryReader in_stream = new FMemoryReader( data.SourceData, data.SourceLength );
			FMemoryWriter out_stream = new FMemoryWriter( data.DestinationData, data.DestinationLength );

			result.Result = Lzma2Enc.Lzma2Encode( out_stream, in_stream, encoderProperties, ref result.PropertySummary, progress );

			result.OutputLength = out_stream.GetOffset();
			return result.Result;
		}

		/// <summary>
		/// Decompresses a block of LZMA2-compressed memory.
		/// </summary>
		/// <param name="data">Source and destination buffers with their sizes.</param>
		/// <param name="result">On entry: must contain the PropertySummary byte from compression. On exit: receives the decompressed length and result code.</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public static SevenZipResult Lzma2Decompress( CLzmaData data, ref CLzma2Result result )
		{
			ELzmaStatus status = ELzmaStatus.LzmaStatusNotSpecified;
			result.OutputLength = data.DestinationLength;
			result.Result = Lzma2Dec.Lzma2Decode( data.DestinationData, ref result.OutputLength, data.SourceData, ref data.SourceLength, result.PropertySummary, ELzmaFinishMode.LzmaFinishModeEnd, ref status );

			return result.Result;
		}
	}
}
