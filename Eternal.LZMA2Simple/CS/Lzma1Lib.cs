// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int16 = Int16;
	using int32 = Int32;
	using int64 = Int64;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	/// <summary>A container for the input and output buffers.</summary>
	public class CLzmaData( uint8[] sourceData, int64 sourceLength, uint8[] destinationData, int64 destinationLength )
	{
		public readonly uint8[] SourceData = sourceData;
		public int64 SourceLength = sourceLength;
			   
		public readonly uint8[] DestinationData = destinationData;
		public readonly int64 DestinationLength = destinationLength;
	}

	/// <summary>The struct containing the results from a compress/decompress operation.</summary>
	public class CLzma1Result
	{
		/// <summary>The encoded size of the dictionary - 40 means to find it.</summary>
		public uint8[] Properties = [0, 0, 0, 0, 0];

		/// <summary>The overall result; 0 for OK, positive for error.</summary>
		public SevenZipResult Result;

		/// <summary>The number of bytes output from the compress/decompress operation.</summary>
		public int64 OutputLength;
	}

	public partial class CLzmaEncoderProperties
	{
		/// <summary>0 &lt;= Level &lt;= 9.</summary>
		public uint8 CompressionLevel = 5;

		/// <summary>0 &lt;= LiteralContextBits &lt;= 8, default = 3. LZMA2 has an additional limitation that LiteralContextBits + LiteralPositionBits &lt;= 4.</summary>
		public uint8 LiteralContextBits = 3;

		/// <summary>0 &lt;= LiteralPositionBits &lt;= 4, default = 0.</summary>
		public uint8 LiteralPositionBits = 0;

		/// <summary>0 &lt;= PositionBits &lt;= 4, default = 2.</summary>
		public uint8 PositionBits = 2;

		/// <summary>false - do not write EOPM, true - write EOPM, default = false.</summary>
		public bool WriteEndMark = false;

		/// <summary>LZMA1: 2 &lt;= FastBytes &lt;= 273; LZMA2: 5 &lt;= FastBytes &lt;= 273. Default = 32 if CompressionLevel &lt; 7, or 64.</summary>
		public int16 FastBytes = -1;

		/// <summary>Dictionary size is determined by the compression level, or set explicitly here. Must be at least 4096 ( 1u &lt;&lt; 12 ).</summary>
		public uint32 DictionarySize = 0u;

		/// <summary>1 &lt;= MatchCycles &lt;= (1 &lt;&lt; 30), default = 32. This is one of the primary speed vs. compression trade-offs.</summary>
		public uint32 MatchCycles = 32;

		/// <summary>Estimated size of data that will be compressed, default = UINT64_MAX (no estimate). The encoder will clamp the dictionary size to be no larger than this value.</summary>
		public int64 EstimatedSourceDataSize = int64.MaxValue;
	}

	public static class Lzma1Lib
	{
		/// <summary>
		/// Returns the maximum compressed size for the given uncompressed input size.
		/// </summary>
		/// <param name="size">Uncompressed input size in bytes.</param>
		/// <returns>Upper bound on compressed output size in bytes.</returns>
		public static int64 LzmaWorstCompression( int64 size )
		{
			// LZMA documentations states worst case compression is ( size * 0.001 ) + 32
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
