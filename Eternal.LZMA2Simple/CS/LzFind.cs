// Copyright Eternal Developments LLC. All Rights Reserved.

namespace Eternal.LZMA2SimpleCS.CS
{
	using CLzRef = UInt32;
	using int32 = Int32;
	using int64 = Int64;
	using int8 = SByte;
	using uint32 = UInt32;
	using uint64 = UInt64;
	using uint8 = Byte;

	public class MatchFinder
	{
		public static readonly uint32 BigHashSizeLimit = 1u << 24;

		public static readonly uint32 MatchFinderHash2Size = 1u << 10;
		public static readonly uint32 MatchFinderHash2Mask = MatchFinderHash2Size - 1u;
		public static readonly uint32 MatchFinderHash3Size = 1u << 16;
		public static readonly uint32 MatchFinderHash3Mask = MatchFinderHash3Size - 1u;

		/*
		  We use up to 3 crc values for hash:
			crc0
			crc1 << Shift_1
			crc2 << Shift_2
		  (Shift_1 = 5) and (Shift_2 = 10) is good tradeoff.
		  Small values for Shift are not good for collision rate.
		  Big value for Shift_2 increases the minimum size
		  of hash table, that will be slow for small files.
		*/

		public static readonly int8 HashCrcShift1 = 5;

		private static readonly int8 NumRefAlignmentBits = 4;
		private static readonly int64 NumRefAlignmentTableSize = 1u << NumRefAlignmentBits;
		public static readonly int64 NumRefAlignmentTableMask = NumRefAlignmentTableSize - 1;
		public static readonly int64 NumRefAlignmentTableTruncate = ~NumRefAlignmentTableMask;

		// alignment for memmove()
		public static readonly uint32 BlockMoveAlignSize = 1u << 7;
		public static readonly uint32 BlockMoveAlignMask = BlockMoveAlignSize - 1u;
		public static readonly uint32 BlockMoveAlignTruncate = ~BlockMoveAlignMask;

		// alignment for block allocation
		public static readonly uint32 BlockSizeAlignSize = 1 << 16;
		public static readonly uint32 BlockSizeAlignMask = BlockSizeAlignSize - 1u;
		public static readonly uint32 BlockSizeAlignTruncate = ~BlockSizeAlignMask;

		// it's 1/256 from 4 GB dictinary
		public static readonly uint32 MinBlockSizeReserve = ( 1u << 24 );
	}

	public abstract class CMatchFinder
	{
		// Public fields
		public InStreamInterface? InStream;
		public uint8[] BufferBase = [];

		public int64 BufferOffset = 0;
		public int64 ExpectedDataSize = int64.MaxValue;
		public int64 DirectInputRemaining = 0;

		public uint32 Position = 0;
		public uint32 StreamPosition = 0;
		public uint32 CutValue = 32u;
		public SevenZipResult Result = SevenZipResult.SevenZipOK;

		public bool DirectInput = false;

		// Protected fields
		protected CLzRef[] Hash = [];
		protected uint32 PositionLimit = 0;

		/* wrap over Zero is allowed (StreamPosition < Position). Use ( uint32 )( StreamPosition - Position ) */
		protected uint32 LengthLimit = 0;
		protected uint32 CyclicBufferPosition = 0;

		/* it must be = (HistorySize + 1) */
		protected uint32 CyclicBufferSize = 0;
		protected uint32 SonOffset = 0;

		private uint32 HashMask = 0;
		private uint32 HashSizeSum = 0;

		private uint32 MatchMaxLength = 0; 
		private uint32 FixedHashSize = 0;
		private uint32 KeepSizeBefore = 0;
		private uint32 KeepSizeAfter = 0;
		private uint32 BlockSize = 0;

		private int64 NumRefs = 0;
		private bool StreamEndWasReached = false;

		private static readonly uint32[] CrcLookupTable = 
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

		public CMatchFinder()
		{
		}

		~CMatchFinder()
		{
		}

		/// <summary>
		/// Returns true if this match finder uses a binary tree structure; false for hash chain.
		/// </summary>
		/// <returns>true for binary tree mode, false for hash chain mode.</returns>
		public abstract bool IsBinaryTreeMode();

		/// <summary>
		/// Finds all matches at the current stream position and advances one position.
		/// </summary>
		/// <param name="distances">Output array to receive alternating (length, distance-1) match pairs.</param>
		/// <param name="pairCount">On exit: number of values written to distances (two per match).</param>
		public abstract void GetMatches( uint32[] distances, ref uint32 pairCount );

		/// <summary>
		/// Advances the match finder by the given number of positions without recording matches.
		/// </summary>
		/// <param name="length">Number of positions to skip.</param>
		public abstract void Skip( uint32 length );

		/// <summary>
		/// Releases all hash and buffer memory allocated by this match finder.
		/// </summary>
		public void Free()
		{
			FreeHashes();
			FreeBuffer();
		}

		/// <summary>
		/// Creates and returns either a binary-tree or hash-chain match finder.
		/// </summary>
		/// <param name="useBinaryTree">true to create a CMatchFinderBinaryTree; false to create a CMatchFinderHashChain.</param>
		/// <returns>A new CMatchFinder instance of the requested type.</returns>
		public static CMatchFinder CreateMatchFinder( bool useBinaryTree )
		{
			if( useBinaryTree )
			{
				return new CMatchFinderBinaryTree();
			}
			else
			{
				return new CMatchFinderHashChain();
			}
		}

		/// <summary>
		/// Allocates and configures the match finder for the given stream parameters.
		/// </summary>
		/// <param name="inHistorySize">Size of the history (dictionary) in bytes.</param>
		/// <param name="keepAddBufferBefore">Extra bytes to keep before the current position.</param>
		/// <param name="inMatchMaxLen">Maximum match length to search for.</param>
		/// <param name="keepAddBufferAfter">Extra bytes to keep after the current position.</param>
		/// <returns>true on success, false if allocation failed.</returns>
		public bool Create( uint32 inHistorySize, uint32 keepAddBufferBefore, uint32 inMatchMaxLen, uint32 keepAddBufferAfter )
		{
			/* we need one additional byte in (mf->KeepSizeBefore),
			   since we use MoveBlock() after (mf->Position++) and before dictionary using */
			KeepSizeBefore = inHistorySize + keepAddBufferBefore + 1u;

			keepAddBufferAfter += inMatchMaxLen;
			/* we need (mf->KeepSizeAfter >= mf->NumHashBytes) */
			keepAddBufferAfter = Math.Max( keepAddBufferAfter, 4u );

			bool buffer_created = false;

			KeepSizeAfter = keepAddBufferAfter;
			if( DirectInput )
			{
				BlockSize = 0u;
			}
			else
			{
				buffer_created = CreateBuffer( GetBlockSize( inHistorySize ) );
			}

			if( DirectInput || buffer_created )
			{
				// do not change it
				uint32 new_cyclic_buffer_size = inHistorySize + 1u;
				MatchMaxLength = inMatchMaxLen;

				// uint32 hs4;
				FixedHashSize = 0u;
				uint32 history_size = inHistorySize;
				if( history_size > ExpectedDataSize )
				{
					history_size = ( uint32 )ExpectedDataSize;
				}

				if( history_size != 0u )
				{
					history_size--;
				}

				history_size |= ( history_size >> 1 );
				history_size |= ( history_size >> 2 );
				history_size |= ( history_size >> 4 );
				history_size |= ( history_size >> 8 );

				// we propagated 16 bits in (hs). Low 16 bits must be set later
				history_size >>= 1;
				if( history_size >= MatchFinder.BigHashSizeLimit )
				{
					history_size >>= 1;
				}

				// (hash_size >= (1 << 16)) : Required for (NumHashBytes > 2)
				history_size |= MatchFinder.BlockSizeAlignMask; /* don't change it! */

				HashMask = history_size;
				history_size++;

				FixedHashSize += MatchFinder.MatchFinderHash2Size + MatchFinder.MatchFinderHash3Size;

				history_size += FixedHashSize;

				HashSizeSum = history_size;
				// it must be = (HistorySize + 1)
				CyclicBufferSize = new_cyclic_buffer_size;

				int64 son_count = new_cyclic_buffer_size;
				if( IsBinaryTreeMode() )
				{
					son_count <<= 1;
				}

				int64 new_size = history_size + son_count;

				// Aligned size is not required here, but it can be better for some loops
				new_size = ( new_size + MatchFinder.NumRefAlignmentTableMask ) & MatchFinder.NumRefAlignmentTableTruncate;

				if( AllocHashes( new_size ) )
				{
					SonOffset = HashSizeSum;
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Resets all internal state and reads the first block of input data, preparing the match finder for use.
		/// </summary>
		public void Init()
		{
			// MatchFinder_Init_LowHash( mf );
			Array.Fill( Hash, 0u, 0, ( int32 )FixedHashSize );

			// MatchFinder_Init_HighHash( mf );
			Array.Fill( Hash, 0u, ( int32 )FixedHashSize, ( int32 )( HashMask + 1 ) );

			BufferOffset = 0;

			/* EMPTY_HASH_VALUE = 0 (Zero) is used in hash tables as NO-VALUE marker.
			   the code in CMatchFinderMt expects (Position = 1) */
			Position = 1u;
			// it's smallest optimal value. do not change it
			StreamPosition = 1u;

			Result = SevenZipResult.SevenZipOK;
			StreamEndWasReached = false;

			ReadBlock();

			/* if we init (CyclicBufferPosition = Position), then we can use one variable
			   instead of both (CyclicBufferPosition) and (Position) : only before (CyclicBufferPosition) wrapping */
			CyclicBufferPosition = Position; // init with relation to (Position)
			SetLimits();
		}


		// Call MatchFinder.CheckLimits() only after (p->Position++) update

		protected void CheckLimits()
		{
			if( KeepSizeAfter == StreamPosition - Position )
			{
				// we try to read only in exact state (mf->KeepSizeAfter == GetNumAvailableBytes( mf ))
				if( NeedMove() )
				{
					MoveBlock();
				}

				ReadBlock();
			}

			if( CyclicBufferPosition == CyclicBufferSize )
			{
				CyclicBufferPosition = 0u;
			}

			SetLimits();
		}

		protected void MovePos()
		{
			/* we go here at the end of stream data, when (avail < num_hash_bytes)
			   We don't update sons[CyclicBufferPosition << BinaryTreeMode].
			   So (sons) record will contain junk. And we cannot resume match searching
			   to normal operation, even if we will provide more input data in buffer.
			   p->sons[p->CyclicBufferPosition << p->BinaryTreeMode] = 0;  // EMPTY_HASH_VALUE
			   if (p->BinaryTreeMode)
				  p->sons[(p->CyclicBufferPosition << p->BinaryTreeMode) + 1] = 0;  // EMPTY_HASH_VALUE
			*/
			++CyclicBufferPosition;
			BufferOffset++;
			Position++;

			if( Position == PositionLimit )
			{
				CheckLimits();
			}
		}

		protected bool FindDistances( ref uint32 d2, uint32 d3, uint32 maxDistance, uint32[] distances, ref uint32 matchCount )
		{
			// Fast path: Check d2 match first (most common case)
			if( d2 < maxDistance && BufferBase[BufferOffset - d2] == BufferBase[BufferOffset] )
			{
				// Write 2-byte match
				distances[matchCount++] = 2u;
				distances[matchCount++] = d2 - 1u;

				// Check if 3rd byte matches
				if( BufferBase[BufferOffset - d2 + 2] != BufferBase[BufferOffset + 2] )
				{
					// Try d3 as fallback
					if( d3 < maxDistance && BufferBase[BufferOffset - d3] == BufferBase[BufferOffset] )
					{
						matchCount++;
						distances[matchCount++] = d3 - 1u;
						d2 = d3;
					}
					else
					{
						return true;
					}
				}

				return false;
			}

			// Secondary path: Check d3 match
			if( d3 < maxDistance && BufferBase[BufferOffset - d3] == BufferBase[BufferOffset] )
			{
				matchCount++;
				distances[matchCount++] = d3 - 1u;
				d2 = d3;
				return false;
			}

			// No matches found
			return true;
		}

		protected uint32 UpdateMaxLen( uint32 d2, uint32 maxLength )
		{
			int64 index = BufferOffset + maxLength;
			int64 limit = BufferOffset + LengthLimit;

			while( index < limit )
			{
				if( BufferBase[index - d2] != BufferBase[index] )
				{
					break;
				}

				index++;
			}

			return ( uint32 )( index - BufferOffset );
		}


		protected uint32 CalcHash( out uint32 d2, out uint32 d3 )
		{
			uint32 h2, h3, hv;

			uint32 current_match = HashCalcInternal( out h2, out h3, out hv );
			HashUpdate( h2, h3, hv, out d2, out d3 );
			return current_match;
		}

		protected uint32 CalcHashSkip()
		{
			uint32 h2, h3, hv;

			uint32 current_match = HashCalcInternal( out h2, out h3, out hv );
			HashSkip( h2, h3, hv );
			return current_match;
		}

		private void FreeBuffer()
		{
			if( !DirectInput )
			{
				BufferBase = [];
			}
		}

		private bool CreateBuffer( uint32 newBlockSize )
		{
			if( newBlockSize == 0u )
			{
				return false;
			}

			if( BlockSize != newBlockSize )
			{
				BlockSize = newBlockSize;
				BufferBase = new uint8[BlockSize];
			}

			return true;
		}

		private void FreeHashes()
		{
			Hash = [];
		}

		private bool AllocHashes( int64 num )
		{
			if( Hash.LongLength == 0 || NumRefs != num )
			{
				NumRefs = num;
				Hash = new CLzRef[NumRefs];
			}

			return Hash.LongLength != 0;
		}


		private void ReadBlock()
		{
			if( StreamEndWasReached || Result != SevenZipResult.SevenZipOK )
			{
				return;
			}

			// Handle direct input mode
			if( DirectInput )
			{
				uint32 max_size = UInt32.MaxValue - StreamPosition - Position;
				uint32 current_size = ( max_size < DirectInputRemaining ) ? max_size : ( uint32 )DirectInputRemaining;

				DirectInputRemaining -= current_size;
				StreamPosition += current_size;

				if( DirectInputRemaining == 0u )
				{
					StreamEndWasReached = true;
				}

				return;
			}

			// Handle stream input mode
			uint32 available_bytes = StreamPosition - Position;
			int64 offset = BufferOffset + available_bytes;
			int64 size = BlockSize - BufferOffset - available_bytes;

			// Check if buffer has space (should always be true in normal operation)
			if( size == 0u )
			{
				// This shouldn't happen if NeedMove() and MoveBlock() were called properly
				return;
			}

			// Read from stream
			Result = InStream!.Read( BufferBase, offset, ref size );
			if( Result != SevenZipResult.SevenZipOK )
			{
				return;
			}

			// Check for end of stream
			if( size == 0u )
			{
				StreamEndWasReached = true;
				return;
			}

			StreamPosition += ( uint32 )size;
		}

		private void MoveBlock()
		{
			uint32 offset = ( uint32 )BufferOffset - KeepSizeBefore;
			uint32 keep_before = ( offset & MatchFinder.BlockMoveAlignMask ) + KeepSizeBefore;
			uint32 move_size = keep_before + StreamPosition - Position;

			BufferOffset = keep_before;

			Array.Copy( BufferBase, offset & MatchFinder.BlockMoveAlignTruncate, BufferBase, 0, move_size );
		}

		/* We call MoveBlock() before ReadBlock().
		   So MoveBlock() can be wasteful operation, if the whole input data
		   can fit in current block even without calling MoveBlock().
		   in important case where (dataSize <= HistorySize)
			 condition (p->BlockSize > dataSize + p->KeepSizeAfter) is met
			 So there is no MoveBlock() in that case case.
		*/
		private bool NeedMove()
		{
			if( DirectInput )
			{
				return false;
			}

			if( StreamEndWasReached || Result != SevenZipResult.SevenZipOK )
			{
				return false;
			}

			return ( BlockSize - BufferOffset ) <= KeepSizeAfter;
		}

		private uint32 GetBlockSize( uint32 inHistorySize )
		{
			uint32 block_size = KeepSizeBefore + KeepSizeAfter;

			// if 32-bit overflow
			if( KeepSizeBefore < inHistorySize || block_size < KeepSizeBefore )
			{
				return 0u;
			}

			uint32 max_block_size = MatchFinder.BlockSizeAlignTruncate;
			uint32 remaining = max_block_size - block_size;
			// do not overflow 32-bit here
			uint32 reserve = ( block_size >> ( block_size < ( 1u << 30 ) ? 1 : 2 ) ) + ( 1u << 12 ) + MatchFinder.BlockMoveAlignSize + MatchFinder.BlockSizeAlignSize;
			// we reject settings that will be slow
			if( block_size >= max_block_size || remaining < MatchFinder.MinBlockSizeReserve )
			{
				return 0u;
			}

			if( reserve >= remaining )
			{
				block_size = max_block_size;
			}
			else
			{
				block_size += reserve;
				block_size &= MatchFinder.BlockSizeAlignTruncate;
			}

			return block_size;
		}

		private void SetLimits()
		{
			uint32 length = 0u - Position;
			if( length == 0u )
			{
				length = UInt32.MaxValue;
			}

			length = Math.Min( length, CyclicBufferSize - CyclicBufferPosition );
			uint32 num_available_bytes = StreamPosition - Position;

			uint32 keep_size_after = KeepSizeAfter;
			uint32 max_length = MatchMaxLength;
			if( num_available_bytes > keep_size_after )
			{
				// we must limit exactly to KeepSizeAfter for ReadBlock
				num_available_bytes -= keep_size_after;
			}
			else if( num_available_bytes >= max_length )
			{
				// the limitation for (mf->LengthLimit) update
				// optimization : to reduce the number of checks
				num_available_bytes -= max_length;
				num_available_bytes++;
				// k = 1; // non-optimized version : for debug
			}
			else
			{
				max_length = num_available_bytes;
				if( num_available_bytes != 0u )
				{
					num_available_bytes = 1u;
				}
			}

			LengthLimit = max_length;

			length = Math.Min( num_available_bytes, length );

			PositionLimit = Position + length;
		}

		private uint32 HashCalcInternal( out uint32 h2, out uint32 h3, out uint32 hv )
		{
			// Load all bytes once
			uint32 b0 = BufferBase[BufferOffset + 0];
			uint32 b1 = BufferBase[BufferOffset + 1];
			uint32 b2 = BufferBase[BufferOffset + 2];
			uint32 b3 = BufferBase[BufferOffset + 3];

			// Compute intermediate hash values
			uint32 temp1 = CrcLookupTable[b0] ^ b1;
			h2 = temp1 & MatchFinder.MatchFinderHash2Mask;

			uint32 temp2 = temp1 ^ ( b2 << 8 );
			h3 = temp2 & MatchFinder.MatchFinderHash3Mask;

			hv = ( temp2 ^ ( CrcLookupTable[b3] << MatchFinder.HashCrcShift1 ) ) & HashMask;

			return Hash[hv + MatchFinder.MatchFinderHash2Size + MatchFinder.MatchFinderHash3Size];
		}

		private void HashUpdate( uint32 h2, uint32 h3, uint32 hv, out uint32 d2, out uint32 d3 )
		{
			d2 = Position - Hash[h2];
			d3 = Position - Hash[h3 + MatchFinder.MatchFinderHash2Size];

			Hash[h2] = Position;
			Hash[h3 + MatchFinder.MatchFinderHash2Size] = Position;
			Hash[hv + MatchFinder.MatchFinderHash2Size + MatchFinder.MatchFinderHash3Size] = Position;
		}

		private void HashSkip( uint32 h2, uint32 h3, uint32 hv )
		{
			Hash[h2] = Hash[h3 + MatchFinder.MatchFinderHash2Size]
				= Hash[hv + MatchFinder.MatchFinderHash2Size + MatchFinder.MatchFinderHash3Size]
					= Position;
		}
	}

	public class CMatchFinderHashChain
		: CMatchFinder
	{
		public CMatchFinderHashChain()
		{
		}

		/// <inheritdoc/>
		public override bool IsBinaryTreeMode()
		{
			return false;
		}

		/// <inheritdoc/>
		public override void GetMatches( uint32[] distances, ref uint32 pairCount )
		{
			if( LengthLimit < 4u )
			{
				MovePos();
				return;
			}

			uint32 current_match = CalcHash( out uint32 d2, out uint32 d3 );
			uint32 max_distance = Math.Min( CyclicBufferSize, Position );
			uint32 max_length = 3u;

			// Try to find short distance matches
			if( !FindDistances( ref d2, d3, max_distance, distances, ref pairCount ) )
			{
				// HC: just update max_length
				max_length = UpdateMaxLen( d2, max_length );
				distances[pairCount - 2] = max_length;

				if( max_length == LengthLimit )
				{
					Hash[SonOffset + CyclicBufferPosition] = current_match;
					MovePos();
					return;
				}
			}

			// Search for longer matches using hash chain
			pairCount += GetMatchesSpec( current_match, distances, pairCount, max_length );

			MovePos();
		}

		/// <inheritdoc/>
		public override void Skip( uint32 length )
		{
			while( length > 0u )
			{
				if( LengthLimit < 4u )
				{
					MovePos();
					length--;
				}
				else
				{
					// Skip multiple positions at once up to position limit
					uint32 skip_count = Math.Min( length, PositionLimit - Position );
					length -= skip_count;

					// Update hash chain for skipped positions
					uint32 son_base = SonOffset;
					uint32 son_idx = son_base + CyclicBufferPosition;
					CyclicBufferPosition += skip_count;

					uint32 remaining = skip_count;
					do
					{
						Hash[son_idx++] = CalcHashSkip();
						BufferOffset++;
						Position++;
					} while( --remaining > 0u );

					if( Position == PositionLimit )
					{
						CheckLimits();
					}
				}
			}
		}

		/*
		  (LengthLimit > maxLength)
		*/
		private uint32 GetMatchesSpec( uint32 currentMatch, uint32[] distances, uint32 pairCount, uint32 maxLength )
		{
			uint32 cut_value = CutValue;
			uint32 match_count = 0u;

			Hash[SonOffset + CyclicBufferPosition] = currentMatch;

			do
			{
				if( currentMatch == 0u )
				{
					break;
				}

				uint32 delta = Position - currentMatch;
				if( delta >= CyclicBufferSize )
				{
					break;
				}

				int64 cyclic_idx = CyclicBufferPosition - delta + ( ( delta > CyclicBufferPosition ) ? CyclicBufferSize : 0u );
				currentMatch = Hash[SonOffset + cyclic_idx];

				int64 current_offset = BufferOffset;
				int64 history_offset = current_offset - delta;

				if( BufferBase[current_offset + maxLength] == BufferBase[history_offset + maxLength] )
				{
					uint32 length = 0u;
					while( length < LengthLimit && BufferBase[current_offset + length] == BufferBase[history_offset + length] )
					{
						length++;
					}

					if( length == LengthLimit )
					{
						distances[pairCount + match_count++] = LengthLimit;
						distances[pairCount + match_count++] = delta - 1u;
						return match_count;
					}

					if( maxLength < length )
					{
						maxLength = length;
						distances[pairCount + match_count++] = length;
						distances[pairCount + match_count++] = delta - 1u;
					}
				}
			} while( --cut_value != 0 );

			return match_count;
		}
	}

	public class CMatchFinderBinaryTree
		: CMatchFinder
	{
		public CMatchFinderBinaryTree()
		{
		}

		/// <inheritdoc/>
		public override bool IsBinaryTreeMode()
		{
			return true;
		}

		/// <inheritdoc/>
		public override void GetMatches( uint32[] distances, ref uint32 pairCount )
		{
			if( LengthLimit < 4u )
			{
				MovePos();
				return;
			}

			uint32 current_match = CalcHash( out uint32 d2, out uint32 d3 );
			uint32 max_distance = Math.Min( CyclicBufferSize, Position );
			uint32 max_length = 3u;

			// Try to find short distance matches
			if( !FindDistances( ref d2, d3, max_distance, distances, ref pairCount ) )
			{
				// BT4: just update max_length
				max_length = UpdateMaxLen( d2, max_length );
				distances[pairCount - 2] = max_length;

				if( max_length == LengthLimit )
				{
					SkipMatchesSpec( current_match );
					MovePos();
					return;
				}
			}

			// Search for longer matches using binary tree
			pairCount += BinaryTreeTraverse( current_match, distances, pairCount, max_length, true );

			MovePos();
		}

		/// <inheritdoc/>
		public override void Skip( uint32 length )
		{
			do
			{
				if( LengthLimit < 4u )
				{
					MovePos();
				}
				else
				{
					uint32 current_match = CalcHashSkip();
					SkipMatchesSpec( current_match );

					MovePos();
				}

			} while( --length != 0u );
		}

		/*
		  (LengthLimit > maxLength)
		*/
		private uint32 BinaryTreeTraverse( uint32 currentMatch, uint32[] distances, uint32 pairCount, uint32 maxLength, bool recordMatches )
		{
			uint32 cut_value = CutValue;
			uint32 match_count = 0u;

			uint32 son_base = SonOffset;
			uint32 son_index0 = ( CyclicBufferPosition << 1 ) + 1u;
			uint32 son_index1 = ( CyclicBufferPosition << 1 );

			uint32 cyclic_check = ( Position < CyclicBufferSize ) ? 0u : Position - CyclicBufferSize;

			if( cyclic_check < currentMatch )
			{
				int64 current_offset = BufferOffset;
				uint32 length0 = 0u;
				uint32 length1 = 0u;

				do
				{
					uint32 delta = Position - currentMatch;
					uint32 pair_idx =  ( ( CyclicBufferPosition - delta + ( ( delta > CyclicBufferPosition ) ? CyclicBufferSize : 0u ) ) << 1 );
					int64 history_offset = current_offset - delta;
					uint32 length = ( length0 < length1 ) ? length0 : length1;

					uint32 pair0 = Hash[son_base + pair_idx];
					uint32 pair1 = Hash[son_base + pair_idx + 1];

					// Find the full match length
					if( BufferBase[history_offset + length] == BufferBase[current_offset + length] )
					{
						while( length != LengthLimit && BufferBase[history_offset + length] == BufferBase[current_offset + length] )
						{
							length++;
						}

						// Only difference: record matches or just check for LengthLimit
						if( recordMatches )
						{
							if( maxLength < length )
							{
								maxLength = length;
								distances[pairCount + match_count++] = length;
								distances[pairCount + match_count++] = delta - 1u;

								if( length == LengthLimit )
								{
									Hash[son_base + son_index1] = pair0;
									Hash[son_base + son_index0] = pair1;
									return match_count;
								}
							}
						}
						else
						{
							// Skip mode
							if( length == LengthLimit )
							{
								Hash[son_base + son_index1] = pair0;
								Hash[son_base + son_index0] = pair1;
								return match_count;
							}
						}
					}

					// Update binary tree pointers (same for both)
					if( BufferBase[history_offset + length] < BufferBase[current_offset + length] )
					{
						Hash[son_base + son_index1] = currentMatch;
						currentMatch = pair1;
						son_index1 = pair_idx + 1;
						length1 = length;
					}
					else
					{
						Hash[son_base + son_index0] = currentMatch;
						currentMatch = pair0;
						son_index0 = pair_idx;
						length0 = length;
					}
				} while( --cut_value != 0 && cyclic_check < currentMatch );
			}

			Hash[son_base + son_index0] = 0u;
			Hash[son_base + son_index1] = 0u;
			return match_count;
		}

		private void SkipMatchesSpec( uint32 currentMatch )
		{
			BinaryTreeTraverse( currentMatch, [], 0u, 0u, false );
		}
	}
}

