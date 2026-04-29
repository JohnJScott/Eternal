// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using int16 = Int16;
	using int32 = Int32;
	using int8 = SByte;
	using int64 = Int64;
	using uint16 = UInt16;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	using CProbability = UInt16;
	using CProbPrice = UInt32;

	public enum SevenZipResult
		: int8
	{
		SevenZipOK = 0,
		SevenZipErrorData = 1,
		SevenZipErrorMemory = 2,
		SevenZipErrorCrc = 3,
		SevenZipErrorUnsupported = 4,
		SevenZipErrorParam = 5,
		SevenZipErrorInputEof = 6,
		SevenZipErrorOutputEof = 7,
		SevenZipErrorRead = 8,
		SevenZipErrorWrite = 9,
		SevenZipErrorProgress = 10,
		SevenZipErrorFail = 11,
		SevenZipErrorThread = 12,
		SevenZipErrorArchive = 16,
		SevenZipErrorNoArchive = 17
	};

	/**
	 * Interface for sequential input stream.
	 */
	public abstract class InStreamInterface
	{
		public InStreamInterface()
		{
		}

		~InStreamInterface()
		{
		}

		/**
		 * if ( input( size ) != 0 && output( size ) == 0 ) means end_of_stream.
		 * ( output( size ) < input( size ) ) is allowed
		 */
		public abstract SevenZipResult Read( uint8[] bufferBase, int64 offset, ref int64 size );
	};

	/**
	 * Interface for sequential output stream.
	 */
	abstract class OutStreamInterface
	{
		public OutStreamInterface()
		{
		}

		~OutStreamInterface()
		{
		}

		/**
		 * Returns the number of actually written bytes. ( result < size ) means error.
		 */
		public abstract int64 Write( uint8[] bufferBase, int64 offset, int64 blockSize );
	};

	/**
	 * Interface for progress reporting during compression.
	 */
	public abstract class ProgressInterface
	{
		public ProgressInterface()
		{
		}

		~ProgressInterface()
		{
		}

		/**
		 * Returns: SZ_RESULT. (result != SZ_OK) means break.
		 * Value UINT64_MAX for size means unknown value.
		 */
		public abstract SevenZipResult Progress( int64 inSize, int64 outSize );
	};

	public partial class Lzma
	{
		// Static constructor to initialize static readonly fields
		static Lzma()
		{
		}

		// Common constants
		public static readonly int8 NumAlignmentBits = 4;
		public static readonly uint32 AlignmentTableSize = 1u << NumAlignmentBits;
		public static readonly uint32 AlignmentTableMask = AlignmentTableSize - 1u;

		public static readonly int8 LengthEncoderNumLowBits = 3;
		public static readonly uint32 LengthEncoderNumLowSymbols = 1u << LengthEncoderNumLowBits;
		public static readonly int8 LengthEncoderNumHighBits = 8;
		public static readonly uint32 LengthEncoderNumHighSymbols = 1u << LengthEncoderNumHighBits;

		public static readonly uint32 LzmaPropertiesSize = 5u;
		public static readonly uint8 MaxPositionBits = 4;
		public static readonly uint32 MaxPositionBitsStates = 1u << MaxPositionBits;
		public static readonly uint8 MaxLiteralContextBits = 8;
		public static readonly uint8 MaxLiteralPositionBits = 4;
		public static readonly uint32 MinDictionarySize = 1u << 12;
		public static readonly uint32 MaxDictionarySize = 15u << 28;
		public static readonly int16 Lzma1MinMatchLength = 2;
		public static readonly int16 Lzma2MinMatchLength = 5;
		public static readonly int16 MaxMatchLength = ( int16 )( Lzma1MinMatchLength + ( ( LengthEncoderNumLowSymbols << 1 ) + LengthEncoderNumHighSymbols ) - 1 );
		
		public static readonly int8 NumMoveBits = 5;
		public static readonly int8 NumPositionSlotBits = 6;

		public static readonly int8 NumBitModelTotalBits = 11;
		public static readonly uint32 BitModelTableSize = 1u << NumBitModelTotalBits;
		public static readonly CProbability InitProbabilityValue = ( CProbability )( BitModelTableSize >> 1 );
		public static readonly uint32 BitModelTableMask = BitModelTableSize - 1u;

		public static readonly uint32 StartPositionModelIndex = 4u;
		public static readonly int8 EndPositionModelIndex = 14;
		public static readonly uint32 NumFullDistancesSize = 1u << ( EndPositionModelIndex >> 1 );
		public static readonly uint32 NumFullDistancesMask = NumFullDistancesSize - 1u;

		public static readonly uint32 LiteralSize = 768u;
		public static readonly uint32 NumStates = 12u;
		public static readonly uint32 NumLengthToPositionStates = 4u;
		public static readonly int8 NumLogBits = 11 + ( 3 * ( sizeof( uint64 ) / 8 ) );
		public static readonly uint32 NumLogTableSize = 1u << NumLogBits;
		public static readonly uint32 MaxRangeValue = 1u << 24;
		public static readonly uint32 NumRepeats = 4u;

		// Lzma2 specific constants
		public static readonly uint8 MaxCombinedLiteralBits = 4;

		public static readonly uint8 Lzma2ControlEof = 0;
		public static readonly uint8 Lzma2ControlCopyResetDict = 1;
		public static readonly uint8 Lzma2ControlCopy = 2;
		public static readonly uint8 Lzma2ControlLzma = 1 << 7;

		// Lzma2 encoder constants

		public static readonly uint32 Lzma2MaxPackSize = 1u << 16;
		public static readonly uint32 Lzma2CopyChunkSize = Lzma2MaxPackSize;
		public static readonly uint32 Lzma2MaxUnpackSize = 1u << 21;
		public static readonly uint32 Lzma2KeepWindowSize = Lzma2MaxUnpackSize;

		public static readonly uint32 Lzma2MaxCompressedChunkSize = ( 1u << 16 ) + 16u;

		public static readonly uint8[] LiteralNextStateLut = [0, 0, 0, 0, 1, 2, 3, 4, 5, 6, 4, 5];
		public static readonly uint8[] MatchNextStateLut = [7, 7, 7, 7, 7, 7, 7, 10, 10, 10, 10, 10];
		public static readonly uint8[] RepNextStateLut = [8, 8, 8, 8, 8, 8, 8, 11, 11, 11, 11, 11];
		public static readonly uint8[] ShortRepNextStateLut = [9, 9, 9, 9, 9, 9, 9, 11, 11, 11, 11, 11];

		public static readonly CProbPrice[] ProbabilityPrices =
		[
			0x80, 0x67, 0x5B, 0x54, 0x4E, 0x49, 0x45, 0x42, 0x3F, 0x3D, 0x3A, 0x38, 0x36, 0x34, 0x33, 0x31, 0x30, 0x2E, 0x2D, 0x2C, 0x2B, 0x2A, 0x29, 0x28, 0x27, 0x26, 0x25, 0x24, 0x23, 0x22, 0x22, 0x21,
			0x20, 0x1F, 0x1F, 0x1E, 0x1D, 0x1D, 0x1C, 0x1C, 0x1B, 0x1A, 0x1A, 0x19, 0x19, 0x18, 0x18, 0x17, 0x17, 0x16, 0x16, 0x16, 0x15, 0x15, 0x14, 0x14, 0x13, 0x13, 0x13, 0x12, 0x12, 0x11, 0x11, 0x11,
			0x10, 0x10, 0x10, 0x0F, 0x0F, 0x0F, 0x0E, 0x0E, 0x0E, 0x0D, 0x0D, 0x0D, 0x0C, 0x0C, 0x0C, 0x0B, 0x0B, 0x0B, 0x0B, 0x0A, 0x0A, 0x0A, 0x0A, 0x09, 0x09, 0x09, 0x09, 0x08, 0x08, 0x08, 0x08, 0x07,
			0x07, 0x07, 0x07, 0x06, 0x06, 0x06, 0x06, 0x05, 0x05, 0x05, 0x05, 0x05, 0x04, 0x04, 0x04, 0x04, 0x03, 0x03, 0x03, 0x03, 0x03, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x01, 0x01, 0x01, 0x01, 0x01
		];

		public static readonly uint32[] CrcLookupTable =
		[
			0x00000000u, 0x77073096u, 0xEE0E612Cu, 0x990951BAu, 0x076DC419u, 0x706AF48Fu, 0xE963A535u, 0x9E6495A3u, 0x0EDB8832u, 0x79DCB8A4u, 0xE0D5E91Eu, 0x97D2D988u, 0x09B64C2Bu, 0x7EB17CBDu, 0xE7B82D07u, 0x90BF1D91u,
			0x1DB71064u, 0x6AB020F2u, 0xF3B97148u, 0x84BE41DEu, 0x1ADAD47Du, 0x6DDDE4EBu, 0xF4D4B551u, 0x83D385C7u, 0x136C9856u, 0x646BA8C0u, 0xFD62F97Au, 0x8A65C9ECu, 0x14015C4Fu, 0x63066CD9u, 0xFA0F3D63u, 0x8D080DF5u,
			0x3B6E20C8u, 0x4C69105Eu, 0xD56041E4u, 0xA2677172u, 0x3C03E4D1u, 0x4B04D447u, 0xD20D85FDu, 0xA50AB56Bu, 0x35B5A8FAu, 0x42B2986Cu, 0xDBBBC9D6u, 0xACBCF940u, 0x32D86CE3u, 0x45DF5C75u, 0xDCD60DCFu, 0xABD13D59u,
			0x26D930ACu, 0x51DE003Au, 0xC8D75180u, 0xBFD06116u, 0x21B4F4B5u, 0x56B3C423u, 0xCFBA9599u, 0xB8BDA50Fu, 0x2802B89Eu, 0x5F058808u, 0xC60CD9B2u, 0xB10BE924u, 0x2F6F7C87u, 0x58684C11u, 0xC1611DABu, 0xB6662D3Du,
			0x76DC4190u, 0x01DB7106u, 0x98D220BCu, 0xEFD5102Au, 0x71B18589u, 0x06B6B51Fu, 0x9FBFE4A5u, 0xE8B8D433u, 0x7807C9A2u, 0x0F00F934u, 0x9609A88Eu, 0xE10E9818u, 0x7F6A0DBBu, 0x086D3D2Du, 0x91646C97u, 0xE6635C01u,
			0x6B6B51F4u, 0x1C6C6162u, 0x856530D8u, 0xF262004Eu, 0x6C0695EDu, 0x1B01A57Bu, 0x8208F4C1u, 0xF50FC457u, 0x65B0D9C6u, 0x12B7E950u, 0x8BBEB8EAu, 0xFCB9887Cu, 0x62DD1DDFu, 0x15DA2D49u, 0x8CD37CF3u, 0xFBD44C65u,
			0x4DB26158u, 0x3AB551CEu, 0xA3BC0074u, 0xD4BB30E2u, 0x4ADFA541u, 0x3DD895D7u, 0xA4D1C46Du, 0xD3D6F4FBu, 0x4369E96Au, 0x346ED9FCu, 0xAD678846u, 0xDA60B8D0u, 0x44042D73u, 0x33031DE5u, 0xAA0A4C5Fu, 0xDD0D7CC9u,
			0x5005713Cu, 0x270241AAu, 0xBE0B1010u, 0xC90C2086u, 0x5768B525u, 0x206F85B3u, 0xB966D409u, 0xCE61E49Fu, 0x5EDEF90Eu, 0x29D9C998u, 0xB0D09822u, 0xC7D7A8B4u, 0x59B33D17u, 0x2EB40D81u, 0xB7BD5C3Bu, 0xC0BA6CADu,
			0xEDB88320u, 0x9ABFB3B6u, 0x03B6E20Cu, 0x74B1D29Au, 0xEAD54739u, 0x9DD277AFu, 0x04DB2615u, 0x73DC1683u, 0xE3630B12u, 0x94643B84u, 0x0D6D6A3Eu, 0x7A6A5AA8u, 0xE40ECF0Bu, 0x9309FF9Du, 0x0A00AE27u, 0x7D079EB1u,
			0xF00F9344u, 0x8708A3D2u, 0x1E01F268u, 0x6906C2FEu, 0xF762575Du, 0x806567CBu, 0x196C3671u, 0x6E6B06E7u, 0xFED41B76u, 0x89D32BE0u, 0x10DA7A5Au, 0x67DD4ACCu, 0xF9B9DF6Fu, 0x8EBEEFF9u, 0x17B7BE43u, 0x60B08ED5u,
			0xD6D6A3E8u, 0xA1D1937Eu, 0x38D8C2C4u, 0x4FDFF252u, 0xD1BB67F1u, 0xA6BC5767u, 0x3FB506DDu, 0x48B2364Bu, 0xD80D2BDAu, 0xAF0A1B4Cu, 0x36034AF6u, 0x41047A60u, 0xDF60EFC3u, 0xA867DF55u, 0x316E8EEFu, 0x4669BE79u,
			0xCB61B38Cu, 0xBC66831Au, 0x256FD2A0u, 0x5268E236u, 0xCC0C7795u, 0xBB0B4703u, 0x220216B9u, 0x5505262Fu, 0xC5BA3BBEu, 0xB2BD0B28u, 0x2BB45A92u, 0x5CB36A04u, 0xC2D7FFA7u, 0xB5D0CF31u, 0x2CD99E8Bu, 0x5BDEAE1Du,
			0x9B64C2B0u, 0xEC63F226u, 0x756AA39Cu, 0x026D930Au, 0x9C0906A9u, 0xEB0E363Fu, 0x72076785u, 0x05005713u, 0x95BF4A82u, 0xE2B87A14u, 0x7BB12BAEu, 0x0CB61B38u, 0x92D28E9Bu, 0xE5D5BE0Du, 0x7CDCEFB7u, 0x0BDBDF21u,
			0x86D3D2D4u, 0xF1D4E242u, 0x68DDB3F8u, 0x1FDA836Eu, 0x81BE16CDu, 0xF6B9265Bu, 0x6FB077E1u, 0x18B74777u, 0x88085AE6u, 0xFF0F6A70u, 0x66063BCAu, 0x11010B5Cu, 0x8F659EFFu, 0xF862AE69u, 0x616BFFD3u, 0x166CCF45u,
			0xA00AE278u, 0xD70DD2EEu, 0x4E048354u, 0x3903B3C2u, 0xA7672661u, 0xD06016F7u, 0x4969474Du, 0x3E6E77DBu, 0xAED16A4Au, 0xD9D65ADCu, 0x40DF0B66u, 0x37D83BF0u, 0xA9BCAE53u, 0xDEBB9EC5u, 0x47B2CF7Fu, 0x30B5FFE9u,
			0xBDBDF21Cu, 0xCABAC28Au, 0x53B39330u, 0x24B4A3A6u, 0xBAD03605u, 0xCDD70693u, 0x54DE5729u, 0x23D967BFu, 0xB3667A2Eu, 0xC4614AB8u, 0x5D681B02u, 0x2A6F2B94u, 0xB40BBE37u, 0xC30C8EA1u, 0x5A05DF1Bu, 0x2D02EF8Du
		];
	}
}
