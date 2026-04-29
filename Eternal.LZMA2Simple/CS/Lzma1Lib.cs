// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int16 = Int16;
	using int32 = Int32;
	using int64 = Int64;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	/**
	 * A container for the input and output buffers
	 */
	public class CLzmaData( uint8[] sourceData, int64 sourceLength, uint8[] destinationData, int64 destinationLength )
	{
		public uint8[] SourceData = sourceData;
		public int64 SourceLength = sourceLength;
			   
		public uint8[] DestinationData = destinationData;
		public int64 DestinationLength = destinationLength;
	}

	/**
	 * The struct containing the results from a compress/decompress operation.
	 */
	public class CLzma1Result
	{
		/** The encoded size of the dictionary - 40 means to find it. */
		public uint8[] Properties = [0, 0, 0, 0, 0];

		/** The overall result; 0 for OK, positive for error */
		public SevenZipResult Result;

		/** The number of bytes output from the compress/decompress operation */
		public int64 OutputLength;
	}

	public partial class CLzmaEncoderProperties
	{
		/** 0 <= Level <= 9 */
		public uint8 CompressionLevel = 5;

		/** LZMA2 has an additional limitation that LiteralContextBits + LiteralPositionBits <= 4 */
		/** 0 <= LiteralContextBits <= 8, default = 3 */
		public uint8 LiteralContextBits = 3;

		/** 0 <= LiteralPositionBits <= 4, default = 0 */
		public uint8 LiteralPositionBits = 0;

		/** 0 <= PositionBits <= 4, default = 2 */
		public uint8 PositionBits = 2;

		/** false - do not write EOPM, true - write EOPM, default = false */
		public bool WriteEndMark = false;

		/** LZMA1: 2 <= FastBytes <= 273, default = 32 if CompressionLevel < 7, or 64 */
		/** LZMA2: 5 <= FastBytes <= 273, default = 32 if CompressionLevel < 7, or 64 */
		public int16 FastBytes = -1;

		/**
		 * Dictionary size is determined by the compression level, or set explicitly here.
		 * Must be at least 4096 ( 1u << 12 ).
		 */
		public uint32 DictionarySize = 0u;

		/**
		 * 1 <= MatchCycles <= (1 << 30), default = 32
		 * This is one of the primary speed vs. compression trade-offs.
		 */
		public uint32 MatchCycles = 32;

		/**
		 * Estimated size of data that will be compressed. default = UINT64_MAX (no estimate).
		 * The encoder will clamp the dictionary size to be no larger than this value.
		 */
		public int64 EstimatedSourceDataSize = Int64.MaxValue;
	}

	public class Lzma1Lib
	{
		/// <summary>
		/// Returns the maximum compressed size for the given uncompressed input size.
		/// </summary>
		/// <param name="size">Uncompressed input size in bytes.</param>
		/// <returns>Upper bound on compressed output size in bytes.</returns>
		public static int64 LzmaWorstCompression( int64 size )
		{
			/** LZMA documentations states worst case compression is ( size * 0.001 ) + 32 */
			return ( size + ( ( size + 511 ) >> 9 ) ) + 32;
		}

		/// <summary>
		/// Compresses a block of memory using LZMA1.
		/// </summary>
		/// <param name="data">Source and destination buffers with their sizes.</param>
		/// <param name="encoderProperties">Encoder configuration parameters.</param>
		/// <param name="result">Receives the compression result, encoded properties, and output length.</param>
		/// <param name="progress">Optional progress callback; pass null to disable.</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public static SevenZipResult Lzma1Compress( CLzmaData data, CLzmaEncoderProperties encoderProperties, out CLzma1Result result, ProgressInterface? progress )
		{
			result = new CLzma1Result();
			int64 out_prop_size = 5;

			result.Result = encoderProperties.Normalize();
			if( result.Result != SevenZipResult.SevenZipOK )
			{
				return result.Result;
			}

			result.OutputLength = data.DestinationLength;
			result.Result = Lzma1Enc.Lzma1Encode( data.DestinationData, ref result.OutputLength, data.SourceData, data.SourceLength, encoderProperties, ref result.Properties, ref out_prop_size, progress );

			return result.Result;
		}

		/// <summary>
		/// Decompresses a block of LZMA1-compressed memory.
		/// </summary>
		/// <param name="data">Source and destination buffers with their sizes.</param>
		/// <param name="result">On entry: must contain the Properties array from compression. On exit: receives the decompressed length and result code.</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public static SevenZipResult Lzma1Decompress( CLzmaData data, ref CLzma1Result result )
		{
			result.OutputLength = data.DestinationLength;
			result.Result = Lzma1Dec.Lzma1Decode( data.DestinationData, ref result.OutputLength, data.SourceData, ref data.SourceLength, result.Properties, 5, ELzmaFinishMode.LzmaFinishModeAny, out ELzmaStatus status );
			return result.Result;
		}
	}
}
