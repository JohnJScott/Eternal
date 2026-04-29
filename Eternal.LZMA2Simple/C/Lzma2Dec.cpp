/* Lzma2Dec.c -- LZMA2 Decoder
2021-02-09 : Igor Pavlov : Public domain */

#include "7zTypes.h"

#include "Lzma1Lib.h"
#include "Lzma1Dec.h"
#include "Lzma2Dec.h"

#include <algorithm>

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

enum class Lzma2State
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

class Lzma2Dec
{
public:
	Lzma2Dec( uint8* decompressed, MemoryInterface* alloc )
		: Decoder( decompressed, alloc )
	{
	}

	/** Decode Lzma2 byte summary to 5 byte array of Lzma1 properties */
	static SevenZipResult DecodeLegacyProperties( uint8 prop, uint8* decoderProperties );
	SevenZipResult DecodeToDictionary( const int64 dictLimit, const uint8* compressed, int64& compressedLength, const LzmaFinishMode finishMode, LzmaStatus& status );

	Lzma1Dec Decoder;

private:
	Lzma2State DecodeProperties( uint8 stateByte );
	Lzma2State UpdateState( uint8 stateByte );

	Lzma2State StateControl = Lzma2State::Lzma2StateControl;
	uint8 Control = 0;
	uint8 NeedInitLevel = 224u;
	uint32 PackSize = 0;
	uint32 UnpackSize = 0u;
};

/** Decode Lzma2 byte summary to 5 byte array of Lzma1 properties */
SevenZipResult Lzma2Dec::DecodeLegacyProperties( uint8 prop, uint8* decoderProperties )
{
	if( prop > 40 )
	{
		return SevenZipResult::SevenZipErrorUnsupported;
	}

	uint32 dict_size = ( prop == 40u ) ? UINT32_MAX : ( ( 2u | ( prop & 1u ) ) << ( prop / 2 + 11 ) );
	decoderProperties[0] = static_cast< uint8 >(Lzma::MaxCombinedLiteralBits );
	decoderProperties[1] = static_cast< uint8 >( ( dict_size >> 0 ) & 0xff );
	decoderProperties[2] = static_cast< uint8 >( ( dict_size >> 8 ) & 0xff );
	decoderProperties[3] = static_cast< uint8 >( ( dict_size >> 16 ) & 0xff );
	decoderProperties[4] = static_cast< uint8 >( ( dict_size >> 24 ) & 0xff );

	return SevenZipResult::SevenZipOK;
}

Lzma2State Lzma2Dec::DecodeProperties( uint8 stateByte )
{
	if( stateByte >= 9u * 5u * 5u )
	{
		return Lzma2State::Lzma2StateError;
	}

	const int32 literal_context_bits = stateByte % 9;
	stateByte /= 9u;
	const int32 position_bits = stateByte / 5;
	const int32 literal_position_bits = stateByte % 5;
	if( literal_context_bits + literal_position_bits > Lzma::MaxCombinedLiteralBits )
	{
		return Lzma2State::Lzma2StateError;
	}

	Decoder.DecoderProperties.PositionBits = static_cast< uint8 >( position_bits & 0xff );
	Decoder.DecoderProperties.LiteralContextBits = static_cast< uint8 >( literal_context_bits & 0xff );
	Decoder.DecoderProperties.LiteralPositionBits = static_cast< uint8 >( literal_position_bits & 0xff );

	Decoder.PositionMask = ( 1u << position_bits ) - 1u;
	Decoder.LiteralMask = ( 256u << literal_position_bits ) - ( 256u >> literal_context_bits );
	return Lzma2State::Lzma2StateData;
}

// ELzma2State
Lzma2State Lzma2Dec::UpdateState( uint8 stateByte )
{
	switch( StateControl )
	{
	case Lzma2State::Lzma2StateControl:
		Control = stateByte;
		if( stateByte == 0 )
		{
			return Lzma2State::Lzma2StateFinished;
		}

		if( ( Control & ( 1u << 7 ) ) == 0 )
		{
			if( stateByte == Lzma::Lzma2ControlCopyResetDict )
			{
				NeedInitLevel = 192;
			}
			else if( stateByte > 2 || NeedInitLevel == 224 )
			{
				return Lzma2State::Lzma2StateError;
			}
		}
		else
		{
			if( stateByte < NeedInitLevel )
			{
				return Lzma2State::Lzma2StateError;
			}

			NeedInitLevel = 0;
			UnpackSize = ( stateByte & 31u ) << 16;
		}

		return Lzma2State::Lzma2StateUnpack0;

	case Lzma2State::Lzma2StateUnpack0:
		UnpackSize |= static_cast<uint32>( stateByte ) << 8;
		return Lzma2State::Lzma2StateUnpack1;

	case Lzma2State::Lzma2StateUnpack1:
		UnpackSize |= stateByte;
		UnpackSize++;
		return ( ( Control & ( 1u << 7 ) ) == 0u ) ? Lzma2State::Lzma2StateData : Lzma2State::Lzma2StatePack0;

	case Lzma2State::Lzma2StatePack0:
		PackSize = static_cast<uint32>( stateByte ) << 8;
		return Lzma2State::Lzma2StatePack1;

	case Lzma2State::Lzma2StatePack1:
		PackSize |= stateByte;
		PackSize++;
		return ( ( Control & 64 ) != 0 ) ? Lzma2State::Lzma2StateProperties : Lzma2State::Lzma2StateData;

	case Lzma2State::Lzma2StateProperties:
		return DecodeProperties( stateByte );

	case Lzma2State::Lzma2StateData:
	case Lzma2State::Lzma2StateDataContinued:
	case Lzma2State::Lzma2StateError:
	case Lzma2State::Lzma2StateFinished:
		break;
	}

	return Lzma2State::Lzma2StateError;
}

SevenZipResult Lzma2Dec::DecodeToDictionary( const int64 dictLimit, const uint8* compressed, int64& compressedLength, const LzmaFinishMode finishMode, LzmaStatus& status )
{
	int64 in_size = compressedLength;
	compressedLength = 0u;
	status = LzmaStatus::LzmaStatusNotSpecified;

	while( StateControl != Lzma2State::Lzma2StateError )
	{
		if( StateControl == Lzma2State::Lzma2StateFinished )
		{
			status = LzmaStatus::LzmaStatusFinishedWithMark;
			return SevenZipResult::SevenZipOK;
		}

		if( Decoder.DictionaryPosition == dictLimit && finishMode == LzmaFinishMode::LzmaFinishModeAny )
		{
			status = LzmaStatus::LzmaStatusNotFinished;
			return SevenZipResult::SevenZipOK;
		}

		if( StateControl != Lzma2State::Lzma2StateData && StateControl != Lzma2State::Lzma2StateDataContinued )
		{
			if( compressedLength == in_size )
			{
				status = LzmaStatus::LzmaStatusNeedsMoreInput;
				return SevenZipResult::SevenZipOK;
			}

			StateControl = UpdateState( compressed[compressedLength++] );
			if( Decoder.DictionaryPosition == dictLimit && StateControl != Lzma2State::Lzma2StateFinished )
			{
				break;
			}

			continue;
		}

		int64 in_current = in_size - compressedLength;
		const int64 initial_dictionary_position = Decoder.DictionaryPosition;
		int64 out_current = dictLimit - initial_dictionary_position;
		LzmaFinishMode current_finish_mode = LzmaFinishMode::LzmaFinishModeAny;

		if( out_current >= UnpackSize )
		{
			out_current = static_cast< int64 >( UnpackSize );
			current_finish_mode = LzmaFinishMode::LzmaFinishModeEnd;
		}

		if( ( Control & ( 1u << 7 ) ) == 0u )
		{
			if( in_current == 0u )
			{
				status = LzmaStatus::LzmaStatusNeedsMoreInput;
				return SevenZipResult::SevenZipOK;
			}

			if( StateControl == Lzma2State::Lzma2StateData )
			{
				const bool initDic = ( Control == Lzma::Lzma2ControlCopyResetDict );
				Decoder.InitDictAndState( initDic, false );
			}

			in_current = std::min(in_current, out_current);

			if( in_current == 0u )
			{
				break;
			}

			Decoder.UpdateWithDecompressed( compressed, compressedLength, in_current );

			compressedLength += in_current;
			UnpackSize -= static_cast< uint32 >( in_current );
			StateControl = ( UnpackSize == 0u ) ? Lzma2State::Lzma2StateControl : Lzma2State::Lzma2StateDataContinued;
		}
		else
		{
			if( StateControl == Lzma2State::Lzma2StateData )
			{
				bool init_dict = ( Control >= 224u );
				bool init_state = ( Control >= 160u );
				Decoder.InitDictAndState( init_dict, init_state );
				StateControl = Lzma2State::Lzma2StateDataContinued;
			}

			if( in_current > PackSize )
			{
				in_current = static_cast< int64 >( PackSize );
			}

			SevenZipResult result = Decoder.DecodeToDict( initial_dictionary_position + out_current, compressed, compressedLength, in_current, current_finish_mode, status );

			compressedLength += in_current;
			PackSize -= static_cast< uint32 >( in_current );
			out_current = Decoder.DictionaryPosition - initial_dictionary_position;
			UnpackSize -= static_cast< uint32 >( out_current );

			if( result != SevenZipResult::SevenZipOK )
			{
				break;
			}

			if( status == LzmaStatus::LzmaStatusNeedsMoreInput )
			{
				if( PackSize == 0u )
				{
					break;
				}

				return SevenZipResult::SevenZipOK;
			}

			if( in_current == 0u && out_current == 0u )
			{
				if( status != LzmaStatus::LzmaStatusMaybeFinishedWithoutMark
					|| UnpackSize != 0u
					|| PackSize != 0u )
				{
					break;
				}

				StateControl = Lzma2State::Lzma2StateControl;
			}

			status = LzmaStatus::LzmaStatusNotSpecified;
		}
	}

	status = LzmaStatus::LzmaStatusNotSpecified;
	StateControl = Lzma2State::Lzma2StateError;
	return SevenZipResult::SevenZipErrorData;
}

SevenZipResult Lzma2Decode( uint8* decompressed, int64& decompressedLength, const uint8* compressed, int64& compressedLength, const uint8 prop, LzmaFinishMode finishMode, LzmaStatus& status, MemoryInterface* alloc )
{
	Lzma2Dec dec2( decompressed, alloc );
	uint8 decoder_properties[Lzma::LzmaPropertiesSize];

	// Decode Lzma2 byte summary to 5 byte array of Lzma1 properties
	SevenZipResult result = dec2.DecodeLegacyProperties( prop, decoder_properties );
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	int64 out_size = decompressedLength;
	int64 in_size = compressedLength;

	decompressedLength = 0u;
	compressedLength = 0u;
	status = LzmaStatus::LzmaStatusNotSpecified;

	// Decode the Lzma 5 byte array to dictionary size and decompression parameters.
	result = dec2.Decoder.DecodeProperties( decoder_properties, Lzma::LzmaPropertiesSize );
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	// Allocate the probabitilies based on the decompression parameters.
	result = dec2.Decoder.AllocateProbabilities();
	if( result != SevenZipResult::SevenZipOK )
	{
		return result;
	}

	dec2.Decoder.DictionaryBufferSize = out_size;
	dec2.Decoder.DictionaryPosition = 0u;
	dec2.Decoder.InitDictAndState( true, true );

	compressedLength = in_size;
	result = dec2.DecodeToDictionary( out_size, compressed, compressedLength, finishMode, status );
	decompressedLength = dec2.Decoder.DictionaryPosition;
	if( result == SevenZipResult::SevenZipOK && status == LzmaStatus::LzmaStatusNeedsMoreInput )
	{
		result = SevenZipResult::SevenZipErrorInputEof;
	}

	dec2.Decoder.FreeProbabilities();
	return result;
}
