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
			/** Returns: result - the number of actually written bytes. (result < size) means error */
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

			public int64 GetOffset()
			{
				return Offset;
			}

			private int64 Offset = 0;
		};

		private class FMemoryReader( uint8[] sourceData, int64 size ) 
			: InStreamInterface
		{
			/** if (input(*size) != 0 && output(*size) == 0) means end_of_stream. (output(*size) < input(*size)) is allowed */
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

		/**
		 * The main LZMA2 compress function.
		 */
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

		/**
		 * The main LZMA2 decompress function.
		 */
		public static SevenZipResult Lzma2Decompress( CLzmaData data, ref CLzma2Result result )
		{
			ELzmaStatus status = ELzmaStatus.LzmaStatusNotSpecified;
			result.OutputLength = data.DestinationLength;
			result.Result = Lzma2Dec.Lzma2Decode( data.DestinationData, ref result.OutputLength, data.SourceData, ref data.SourceLength, result.PropertySummary, ELzmaFinishMode.LzmaFinishModeEnd, ref status );

			return result.Result;
		}
	}
}
