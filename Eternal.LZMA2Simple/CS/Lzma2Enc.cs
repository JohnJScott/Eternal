// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int64 = Int64;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	/* ---------- Lzma2 Props ---------- */
	public class CLzma2EncoderProperties
		: CLzmaEncoderProperties
	{
		/// <summary>
		/// Validates and fills in default values for all LZMA2 encoder properties.
		/// </summary>
		/// <returns>SevenZipOK on success, or SevenZipErrorParam if the literal-bit combination is invalid.</returns>
		public override SevenZipResult Normalize()
		{
			base.Normalize();

			FastBytes = Math.Clamp( FastBytes, Lzma.Lzma2MinMatchLength, Lzma.MaxMatchLength );

			if( MatchCycles == 0u )
			{
				MatchCycles = 16u + ( ( uint32 )( FastBytes ) >> 1 );
				if( CompressionLevel < 5 )
				{
					MatchCycles >>= 1;
				}
			}

			if( LiteralContextBits + LiteralPositionBits > Lzma.MaxCombinedLiteralBits )
			{
				return SevenZipResult.SevenZipErrorParam;
			}

			return SevenZipResult.SevenZipOK;
		}
	}

	internal class Lzma2Enc
	{
		private readonly CLzmaEncoderProperties EncoderProperties;
		private readonly ProgressInterface? Progress;
		private readonly uint8[] WorkBuffer;

		private readonly Lzma1Enc Encoder;
		private readonly int64 ExpectedDataSize = Int64.MaxValue;

		private int64 SourcePosition = 0;

		private bool PropertiesAreSet = false;
		private uint8 PropertiesByte = 0;
		private bool NeedInitState = false;
		private bool NeedInitProp = false;

		/* ---------- CheckedSeqInStream ---------- */

		private class CheckedInStreamInterface
			: InStreamInterface
		{
			public CheckedInStreamInterface( InStreamInterface inRealStreamInterface )
			{
				Processed = 0u;
				Finished = false;
				RealStreamInterface = inRealStreamInterface;
			}

			public void Reset()
			{
				Processed = 0u;
				Finished = false;
			}

			public override SevenZipResult Read( uint8[] bufferBase, int64 offset, ref int64 size ) 
			{
				SevenZipResult result = SevenZipResult.SevenZipOK;

				if( size != 0u )
				{
					result = RealStreamInterface.Read( bufferBase, offset, ref size );
					Finished = ( size == 0 );
					Processed += size;
				}

				return result;
			}

			public int64 Processed = 0;
			public bool Finished = false;

			private InStreamInterface RealStreamInterface;
		};

		public Lzma2Enc( CLzma2EncoderProperties encoderProperties, ProgressInterface? progress )
		{
			EncoderProperties = encoderProperties;
			Progress = progress;
			Encoder = new Lzma1Enc( encoderProperties, progress );
		
			WorkBuffer = new uint8[Lzma.Lzma2MaxCompressedChunkSize];
		}

		~Lzma2Enc()
		{
		}

		/// <summary>
		/// Reads and caches the encoder property byte if it has not already been retrieved.
		/// </summary>
		/// <returns>SevenZipOK on success, or an error code from GetCodedProperties().</returns>
		public SevenZipResult InitStream()
		{
			if( !PropertiesAreSet )
			{
				int64 props_size = Lzma.LzmaPropertiesSize;
				uint8[] props_encoded = new uint8[Lzma.LzmaPropertiesSize];
				SevenZipResult result = Encoder.GetCodedProperties( props_encoded, ref props_size );
				if( result != SevenZipResult.SevenZipOK )
				{
					return result;
				}

				PropertiesByte = props_encoded[0];
				PropertiesAreSet = true;
			}

			return SevenZipResult.SevenZipOK;
		}

		/// <summary>
		/// Resets per-block state, preparing the encoder to start a new LZMA2 block.
		/// </summary>
		public void InitBlock()
		{
			SourcePosition = 0u;
			NeedInitState = true;
			NeedInitProp = true;
		}

		/// <summary>
		/// Encodes one LZMA2 sub-block, choosing between LZMA and copy mode automatically.
		/// </summary>
		/// <param name="packSizeRes">On entry: maximum allowed compressed size for this sub-block. On exit: number of compressed bytes written to outStreamInterface.</param>
		/// <param name="outStreamInterface">Destination stream to write the encoded sub-block to.</param>
		/// <returns>SevenZipOK on success, or SevenZipErrorOutputEof if the output limit is exceeded.</returns>
		public SevenZipResult EncodeSubblock( ref int64 packSizeRes, OutStreamInterface outStreamInterface )
		{
			int64 pack_size_limit = packSizeRes;
			int64 pack_size = pack_size_limit;
			uint32 unpack_size = Lzma.Lzma2MaxUnpackSize;
			uint32 lz_header_size = 5u + ( NeedInitProp ? 1u : 0u );
			bool use_copy_block;

			packSizeRes = 0;
			if( pack_size < lz_header_size )
			{
				return SevenZipResult.SevenZipErrorOutputEof;
			}

			pack_size -= lz_header_size;

			Encoder.SavedState.SaveState( Encoder );
			SevenZipResult result = Encoder.CodeOneMemBlock( NeedInitState, WorkBuffer, lz_header_size, ref pack_size, Lzma.Lzma2MaxPackSize, ref unpack_size );

			if( unpack_size == 0u )
			{
				return result;
			}

			if( result == SevenZipResult.SevenZipOK )
			{
				use_copy_block = ( pack_size + 2u >= unpack_size ) || ( pack_size > Lzma.Lzma2MaxPackSize );
			}
			else
			{
				if( result != SevenZipResult.SevenZipErrorOutputEof )
				{
					return result;
				}

				use_copy_block = true;
			}

			uint32 dest_position = 0u;
			if( use_copy_block )
			{
				while( unpack_size > 0u )
				{
					uint32 copy_chunk_size = ( unpack_size < Lzma.Lzma2CopyChunkSize ) ? unpack_size : Lzma.Lzma2CopyChunkSize;
					if( pack_size_limit - dest_position < copy_chunk_size + 3u )
					{
						return SevenZipResult.SevenZipErrorOutputEof;
					}

					WorkBuffer[dest_position++] = (uint8)( ( SourcePosition == 0 ? Lzma.Lzma2ControlCopyResetDict : Lzma.Lzma2ControlCopy ) & 0xff );
					WorkBuffer[dest_position++] = (uint8)( ( ( copy_chunk_size - 1u ) >> 8 ) & 0xff );
					WorkBuffer[dest_position++] = (uint8)( ( copy_chunk_size - 1u ) & 0xff );
					// MatchFinder->BufferBase + MatchFinder->BufferOffset - AdditionalOffset
					Array.Copy( Encoder.GetBufferBase(), ( int64 )( Encoder.GetCurrentOffset() - unpack_size ), WorkBuffer, ( int64 )dest_position, copy_chunk_size );
					unpack_size -= copy_chunk_size;
					dest_position += copy_chunk_size;
					SourcePosition += copy_chunk_size;

					packSizeRes += dest_position;
					if( outStreamInterface.Write( WorkBuffer, 0, dest_position ) != dest_position )
					{
						return SevenZipResult.SevenZipErrorWrite;
					}

					dest_position = 0u;
				}

				Encoder.SavedState.RestoreState( Encoder );
				return SevenZipResult.SevenZipOK;
			}

			dest_position = 0u;
			uint32 copy_size = unpack_size - 1u;
			uint32 write_pack_size = ( uint32 )( pack_size - 1u );
			uint32 mode = ( SourcePosition == 0 ) ? 3u : ( NeedInitState ? ( NeedInitProp ? 2u : 1u ) : 0u );

			WorkBuffer[dest_position++] = (uint8)( ( Lzma.Lzma2ControlLzma | ( mode << 5 ) | ( ( copy_size >> 16 ) & 31u ) ) );
			WorkBuffer[dest_position++] = (uint8)( ( copy_size >> 8 ) & 0xff );
			WorkBuffer[dest_position++] = (uint8)( copy_size & 0xff );
			WorkBuffer[dest_position++] = (uint8)( ( write_pack_size >> 8 ) & 0xff );
			WorkBuffer[dest_position++] = (uint8)( write_pack_size & 0xff );

			if( NeedInitProp )
			{
				WorkBuffer[dest_position++] = PropertiesByte;
			}

			NeedInitProp = false;
			NeedInitState = false;
			dest_position += ( uint32 )pack_size;
			SourcePosition += unpack_size;

			if( outStreamInterface.Write( WorkBuffer, 0, dest_position ) != dest_position )
			{
				return SevenZipResult.SevenZipErrorWrite;
			}

			packSizeRes = dest_position;
			return SevenZipResult.SevenZipOK;
		}

		/// <summary>
		/// Returns the one-byte LZMA2 dictionary-size index used in the stream header.
		/// </summary>
		/// <returns>Encoded dictionary index in the range [0, 40].</returns>
		public uint8 GetCodedDictionary()
		{
			uint8 dict_index;
			uint32 dict_size = EncoderProperties.GetDictionarySize();
			for( dict_index = 0; dict_index < 40; dict_index++ )
			{
				if( dict_size <= ( 2u | ( dict_index & 1u ) ) << ( ( dict_index / 2 ) + 11 ) )
				{
					break;
				}
			}

			return dict_index;
		}

		/// <summary>
		/// Encodes an entire input stream into an LZMA2 output stream.
		/// </summary>
		/// <param name="outStreamInterface">Destination stream to write the compressed LZMA2 data to.</param>
		/// <param name="inStreamInterface">Source stream supplying the uncompressed data.</param>
		/// <param name="finished">If true, writes the LZMA2 end-of-stream marker after the last block.</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public SevenZipResult EncodeStream( OutStreamInterface outStreamInterface, InStreamInterface inStreamInterface, bool finished )
		{
			int64 unpack_total = 0u;
			int64 pack_total = 0u;
			CheckedInStreamInterface limited_in_stream_interface = new CheckedInStreamInterface( inStreamInterface );

			// Initialize stream encoding
			SevenZipResult result = InitStream();
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			// Main encoding loop - process blocks
			while( true )
			{
				// Initialize block
				InitBlock();

				limited_in_stream_interface.Reset();

				// Prepare Encoder for current block
				int64 expected_size = int64.MaxValue;

				// in_stream version works only in one thread. So we use CLzma2Enc::ExpectedDataSize
				if( ExpectedDataSize != int64.MaxValue && ExpectedDataSize >= unpack_total )
				{
					expected_size = ExpectedDataSize - unpack_total;
				}

				Encoder.SetDataSize( expected_size );

				result = Encoder.Prepare( limited_in_stream_interface, Lzma.Lzma2KeepWindowSize );
				if( result != SevenZipResult.SevenZipOK )
				{
					return result;
				}

				// Encode subblocks within current block
				while( true )
				{
					int64 pack_size = Lzma.Lzma2MaxCompressedChunkSize;

					result = EncodeSubblock( ref pack_size, outStreamInterface );

					if( result != SevenZipResult.SevenZipOK )
					{
						break;
					}

					pack_total += pack_size;

					if( Progress != null )
					{
						result = Progress.Progress( unpack_total + SourcePosition, pack_total );
						if( result != SevenZipResult.SevenZipOK )
						{
							break;
						}
					}

					// Check if subblock encoding is complete
					if( pack_size == 0 )
					{
						break;
					}
				}

				unpack_total += SourcePosition;

				if( result != SevenZipResult.SevenZipOK )
				{
					return result;
				}

				// Verify processed data matches expected
				int64 processed_size = limited_in_stream_interface.Processed;
				if( SourcePosition != processed_size )
				{
					return SevenZipResult.SevenZipErrorFail;
				}

				// Check if all input data has been processed
				if( limited_in_stream_interface.Finished )
				{
					// Write EOF marker if requested
					if( finished )
					{
						uint8[] eof_byte = [Lzma.Lzma2ControlEof];
						if( outStreamInterface.Write( eof_byte, 0, 1u ) != 1u )
						{
							return SevenZipResult.SevenZipErrorWrite;
						}
					}

					return SevenZipResult.SevenZipOK;
				}
			}
		}

		/// <summary>
		/// Compresses an input stream using LZMA2 and writes the result to an output stream.
		/// </summary>
		/// <param name="outStreamInterface">Destination stream to receive the compressed output.</param>
		/// <param name="inStreamInterface">Source stream supplying the uncompressed data.</param>
		/// <param name="encoderProperties">Encoder configuration parameters.</param>
		/// <param name="propertySummary">Receives the one-byte LZMA2 property summary (dictionary size index) needed for decompression.</param>
		/// <param name="progress">Optional progress callback; pass null to disable.</param>
		/// <returns>SevenZipOK on success, or an error code.</returns>
		public static SevenZipResult Lzma2Encode( OutStreamInterface outStreamInterface, InStreamInterface inStreamInterface, CLzma2EncoderProperties encoderProperties, ref uint8 propertySummary, ProgressInterface? progress )
		{
			Lzma2Enc enc2 = new Lzma2Enc( encoderProperties, progress );

			// Dict size - this needs passing to Lzma2Decode()
			propertySummary = enc2.GetCodedDictionary();
			enc2.PropertiesAreSet = false;

			SevenZipResult result = enc2.EncodeStream( outStreamInterface, inStreamInterface, true );
			return result;
		}
	}
}
