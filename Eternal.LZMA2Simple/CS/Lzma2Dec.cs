// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int32 = Int32;
	using int8 = SByte;
	using int64 = Int64;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	internal class Lzma2Dec
	{
		/*
		00000000  -  End of data
		00000001 U U  -  Decompressed, reset Dictionary, need reset State and set new DecoderProperties
		00000010 U U  -  Decompressed, no reset
		100uuuuu U U P P  -  LZMA, no reset
		101uuuuu U U P P  -  LZMA, reset State
		110uuuuu U U P P S  -  LZMA, reset State + set new DecoderProperties
		111uuuuu U U P P S  -  LZMA, reset State + set new DecoderProperties, reset Dictionary

		  u, U - Unpack Size
		  P - Pack Size
		  S - Props
		*/

		enum ELzma2State
			: int8
		{
			Lzma2StateControl = 0,
			Lzma2StateUnpack0,
			Lzma2StateUnpack1,
			Lzma2StatePack0,
			Lzma2StatePack1,
			Lzma2StateProperties,
			Lzma2StateData,
			Lzma2StateDataContinued,
			Lzma2StateFinished,
			Lzma2StateError
		};

		Lzma2Dec( uint8[] decompressed )
		{
			Decoder = new Lzma1Dec( decompressed );
		}

		private Lzma1Dec Decoder;

		private ELzma2State StateControl = ELzma2State.Lzma2StateControl;
		private uint8 Control = 0;
		private uint8 NeedInitLevel = 224;
		private uint32 PackSize = 0;
		private uint32 UnpackSize = 0u;

		/** Decode Lzma2 byte summary to 5 byte array of Lzma1 properties */
		public SevenZipResult DecodeLegacyProperties( uint8 prop, uint8[] decoderProperties )
		{
			if( prop > 40 )
			{
				return SevenZipResult.SevenZipErrorUnsupported;
			}

			uint32 dict_size = ( prop == 40u ) ? UInt32.MaxValue : ( ( 2u | ( prop & 1u ) ) << ( prop / 2 + 11 ) );
			decoderProperties[0] = ( uint8 )( Lzma.MaxCombinedLiteralBits );
			decoderProperties[1] = ( uint8 )( ( dict_size >> 0 ) & 0xff );
			decoderProperties[2] = ( uint8 )( ( dict_size >> 8 ) & 0xff );
			decoderProperties[3] = ( uint8 )( ( dict_size >> 16 ) & 0xff );
			decoderProperties[4] = ( uint8 )( ( dict_size >> 24 ) & 0xff );

			return SevenZipResult.SevenZipOK;
		}

		private ELzma2State DecodeProperties( uint8 stateByte )
		{
			if( stateByte >= 9u * 5u * 5u )
			{
				return ELzma2State.Lzma2StateError;
			}

			int32 literal_context_bits = stateByte % 9;
			stateByte /= 9;
			int32 position_bits = stateByte / 5;
			int32 literal_position_bits = stateByte % 5;
			if( literal_context_bits + literal_position_bits > Lzma.MaxCombinedLiteralBits )
			{
				return ELzma2State.Lzma2StateError;
			}

			Decoder.DecoderProperties.PositionBits = ( uint8 )( position_bits & 0xff );
			Decoder.DecoderProperties.LiteralContextBits = ( uint8 )( literal_context_bits & 0xff );
			Decoder.DecoderProperties.LiteralPositionBits = ( uint8 )( literal_position_bits & 0xff );

			Decoder.PositionMask = ( 1u << position_bits ) - 1u;
			Decoder.LiteralMask = ( 256u << literal_position_bits ) - ( 256u >> literal_context_bits );
			return ELzma2State.Lzma2StateData;
		}

		// ELzma2State
		private ELzma2State UpdateState( uint8 stateByte )
		{
			switch( StateControl )
			{
				case ELzma2State.Lzma2StateControl:
					Control = stateByte;
					if( stateByte == 0 )
					{
						return ELzma2State.Lzma2StateFinished;
					}

					if( ( Control & ( 1u << 7 ) ) == 0 )
					{
						if( stateByte == Lzma.Lzma2ControlCopyResetDict )
						{
							NeedInitLevel = 192;
						}
						else if( stateByte > 2 || NeedInitLevel == 224 )
						{
							return ELzma2State.Lzma2StateError;
						}
					}
					else
					{
						if( stateByte < NeedInitLevel )
						{
							return ELzma2State.Lzma2StateError;
						}

						NeedInitLevel = 0;
						UnpackSize = ( stateByte & 31u ) << 16;
					}

					return ELzma2State.Lzma2StateUnpack0;

				case ELzma2State.Lzma2StateUnpack0:
					UnpackSize |= ( uint32 )( stateByte ) << 8;
					return ELzma2State.Lzma2StateUnpack1;

				case ELzma2State.Lzma2StateUnpack1:
					UnpackSize |= stateByte;
					UnpackSize++;
					return ( ( Control & ( 1u << 7 ) ) == 0u ) ? ELzma2State.Lzma2StateData : ELzma2State.Lzma2StatePack0;

				case ELzma2State.Lzma2StatePack0:
					PackSize = ( uint32 )( stateByte ) << 8;
					return ELzma2State.Lzma2StatePack1;

				case ELzma2State.Lzma2StatePack1:
					PackSize |= stateByte;
					PackSize++;
					return ( ( Control & 64 ) != 0 ) ? ELzma2State.Lzma2StateProperties : ELzma2State.Lzma2StateData;

				case ELzma2State.Lzma2StateProperties:
					return DecodeProperties( stateByte );

				case ELzma2State.Lzma2StateData:
				case ELzma2State.Lzma2StateDataContinued:
				case ELzma2State.Lzma2StateError:
				case ELzma2State.Lzma2StateFinished:
					break;
			}

			return ELzma2State.Lzma2StateError;
		}

		public SevenZipResult DecodeToDictionary(  int64 dictLimit, uint8[] compressed, ref int64 compressedLength, ELzmaFinishMode finishMode, ref ELzmaStatus status )
		{
			int64 in_size = compressedLength;
			compressedLength = 0;
			status = ELzmaStatus.LzmaStatusNotSpecified;

			while( StateControl != ELzma2State.Lzma2StateError )
			{
				if( StateControl == ELzma2State.Lzma2StateFinished )
				{
					status = ELzmaStatus.LzmaStatusFinishedWithMark;
					return SevenZipResult.SevenZipOK;
				}

				if( Decoder.DictionaryPosition == dictLimit && finishMode == ELzmaFinishMode.LzmaFinishModeAny )
				{
					status = ELzmaStatus.LzmaStatusNotFinished;
					return SevenZipResult.SevenZipOK;
				}

				if( StateControl != ELzma2State.Lzma2StateData && StateControl != ELzma2State.Lzma2StateDataContinued )
				{
					if( compressedLength == in_size )
					{
						status = ELzmaStatus.LzmaStatusNeedsMoreInput;
						return SevenZipResult.SevenZipOK;
					}

					StateControl = UpdateState( compressed[compressedLength++] );
					if( Decoder.DictionaryPosition == dictLimit && StateControl != ELzma2State.Lzma2StateFinished )
					{
						break;
					}

					continue;
				}

				int64 in_current = in_size - compressedLength;
				int64 initial_dictionary_position = Decoder.DictionaryPosition;
				int64 out_current = dictLimit - initial_dictionary_position;
				ELzmaFinishMode current_finish_mode = ELzmaFinishMode.LzmaFinishModeAny;

				if( out_current >= UnpackSize )
				{
					out_current = UnpackSize;
					current_finish_mode = ELzmaFinishMode.LzmaFinishModeEnd;
				}

				if( ( Control & ( 1u << 7 ) ) == 0u )
				{
					if( in_current == 0u )
					{
						status = ELzmaStatus.LzmaStatusNeedsMoreInput;
						return SevenZipResult.SevenZipOK;
					}

					if( StateControl == ELzma2State.Lzma2StateData )
					{
						bool initDic = ( Control == Lzma.Lzma2ControlCopyResetDict );
						Decoder.InitDictAndState( initDic, false );
					}

					in_current = Math.Min( in_current, out_current );

					if( in_current == 0u )
					{
						break;
					}

					Decoder.UpdateWithDecompressed( compressed, compressedLength, in_current );

					compressedLength += in_current;
					UnpackSize -= ( uint32 )( in_current );
					StateControl = ( UnpackSize == 0u ) ? ELzma2State.Lzma2StateControl : ELzma2State.Lzma2StateDataContinued;
				}
				else
				{
					if( StateControl == ELzma2State.Lzma2StateData )
					{
						bool init_dict = ( Control >= 224u );
						bool init_state = ( Control >= 160u );
						Decoder.InitDictAndState( init_dict, init_state );
						StateControl = ELzma2State.Lzma2StateDataContinued;
					}

					if( in_current > PackSize )
					{
						in_current = PackSize;
					}

					SevenZipResult result = Decoder.DecodeToDict( initial_dictionary_position + out_current, compressed, compressedLength, ref in_current, current_finish_mode, ref status );

					compressedLength += in_current;
					PackSize -= ( uint32 )( in_current );
					out_current = Decoder.DictionaryPosition - initial_dictionary_position;
					UnpackSize -= ( uint32 )( out_current );

					if( result != SevenZipResult.SevenZipOK )
					{
						break;
					}

					if( status == ELzmaStatus.LzmaStatusNeedsMoreInput )
					{
						if( PackSize == 0u )
						{
							break;
						}

						return SevenZipResult.SevenZipOK;
					}

					if( in_current == 0u && out_current == 0u )
					{
						if( status != ELzmaStatus.LzmaStatusMaybeFinishedWithoutMark
							|| UnpackSize != 0u
							|| PackSize != 0u )
						{
							break;
						}

						StateControl = ELzma2State.Lzma2StateControl;
					}

					status = ELzmaStatus.LzmaStatusNotSpecified;
				}
			}

			status = ELzmaStatus.LzmaStatusNotSpecified;
			StateControl = ELzma2State.Lzma2StateError;
			return SevenZipResult.SevenZipErrorData;
		}

		public static SevenZipResult Lzma2Decode( uint8[] decompressed, ref int64 decompressedLength, uint8[] compressed, ref int64 compressedLength, uint8 prop, ELzmaFinishMode finishMode, ref ELzmaStatus status )
		{
			Lzma2Dec dec2 = new Lzma2Dec( decompressed );
			uint8[] decoder_properties = new uint8[Lzma.LzmaPropertiesSize];

			// Decode Lzma2 byte summary to 5 byte array of Lzma1 properties
			SevenZipResult result = dec2.DecodeLegacyProperties( prop, decoder_properties );
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			int64 out_size = decompressedLength;
			int64 in_size = compressedLength;

			decompressedLength = 0u;
			compressedLength = 0u;
			status = ELzmaStatus.LzmaStatusNotSpecified;

			// Decode the Lzma 5 byte array to dictionary size and decompression parameters.
			result = dec2.Decoder.DecodeProperties( decoder_properties, Lzma.LzmaPropertiesSize );
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			// Allocate the probabitilies based on the decompression parameters.
			result = dec2.Decoder.AllocateProbabilities();
			if( result != SevenZipResult.SevenZipOK )
			{
				return result;
			}

			dec2.Decoder.DictionaryBufferSize = out_size;
			dec2.Decoder.DictionaryPosition = 0u;
			dec2.Decoder.InitDictAndState( true, true );

			compressedLength = in_size;
			result = dec2.DecodeToDictionary( out_size, compressed, ref compressedLength, finishMode, ref status );
			decompressedLength = dec2.Decoder.DictionaryPosition;
			if( result == SevenZipResult.SevenZipOK && status == ELzmaStatus.LzmaStatusNeedsMoreInput )
			{
				result = SevenZipResult.SevenZipErrorInputEof;
			}

			dec2.Decoder.FreeProbabilities();
			return result;
		}
	}
}
