// BZip2InputStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2011 Dino Chiesa.
// All rights reserved.
//
// This code module is part of DotNetZip, a zipfile class library.
//
// ------------------------------------------------------------------
//
// This code is licensed under the Microsoft Public License.
// See the file License.txt for the license details.
// More info on: http://dotnetzip.codeplex.com
//
// ------------------------------------------------------------------
//
// Last Saved: <2011-July-31 11:57:32>
//
// ------------------------------------------------------------------
//
// This module defines the BZip2InputStream class, which is a decompressing
// stream that handles BZIP2. This code is derived from Apache commons source code.
// The license below applies to the original Apache code.
//
// ------------------------------------------------------------------

/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

/*
 * This package is based on the work done by Keiron Liddle, Aftex Software
 * <keiron@aftexsw.com> to whom the Ant project is very grateful for his
 * great code.
 */

// compile: msbuild
// not: csc.exe /t:library /debug+ /out:Ionic.BZip2.dll BZip2InputStream.cs BCRC32.cs Rand.cs

using System;
using System.IO;
using Ionic.Crc;

namespace Ionic.BZip2
{
	/// <summary>
	///     A read-only decorator stream that performs BZip2 decompression on Read.
	/// </summary>
	public class BZip2InputStream : Stream
	{
		private readonly bool _leaveOpen;
		private readonly CRC32 crc = new CRC32( true );
		private bool _disposed;
		private bool blockRandomised;
		private int blockSize100k;
		private int bsBuff;
		private int bsLive;
		private uint computedBlockCRC, computedCombinedCRC;
		private int currentChar = -1;

		private CState currentState = CState.START_BLOCK;
		private DecompressionState data;
		private Stream InputStream;
		private int last;
		private int nInUse;
		private int origPtr;

		private uint storedBlockCRC, storedCombinedCRC;
		private int su_ch2;
		private int su_chPrev;
		private int su_count;
		private int su_i2;
		private int su_j2;
		private int su_rNToGo;
		private int su_rTPos;
		private int su_tPos;
		private char su_z;
		private Int64 totalBytesRead;

		/// <summary>
		///     Create a BZip2InputStream, wrapping it around the given input Stream.
		/// </summary>
		/// <remarks>
		///     <para>
		///         The input stream will be closed when the BZip2InputStream is closed.
		///     </para>
		/// </remarks>
		/// <param name='input'>The stream from which to read compressed data</param>
		public BZip2InputStream( Stream input )
			: this( input, false )
		{
		}

		/// <summary>
		///     Create a BZip2InputStream with the given stream, and
		///     specifying whether to leave the wrapped stream open when
		///     the BZip2InputStream is closed.
		/// </summary>
		/// <param name='input'>The stream from which to read compressed data</param>
		/// <param name='leaveOpen'>
		///     Whether to leave the input stream open, when the BZip2InputStream closes.
		/// </param>
		/// <example>
		///     This example reads a bzip2-compressed file, decompresses it,
		///     and writes the decompressed data into a newly created file.
		///     <code>
		///    var fname = "logfile.log.bz2";
		///    using (var fs = File.OpenRead(fname))
		///    {
		///        using (var decompressor = new Ionic.BZip2.BZip2InputStream(fs))
		///        {
		///            var outFname = fname + ".decompressed";
		///            using (var output = File.Create(outFname))
		///            {
		///                byte[] buffer = new byte[2048];
		///                int n;
		///                while ((n = decompressor.Read(buffer, 0, buffer.Length)) > 0)
		///                {
		///                    output.Write(buffer, 0, n);
		///                }
		///            }
		///        }
		///    }
		///    </code>
		/// </example>
		public BZip2InputStream( Stream input, bool leaveOpen )
		{
			InputStream = input;
			_leaveOpen = leaveOpen;
			init();
		}

		/// <summary>
		///     Indicates whether the stream can be read.
		/// </summary>
		/// <remarks>
		///     The return value depends on whether the captive stream supports reading.
		/// </remarks>
		public override bool CanRead
		{
			get
			{
				if( _disposed )
				{
					throw new ObjectDisposedException( "BZip2Stream" );
				}
				return InputStream.CanRead;
			}
		}

		/// <summary>
		///     Indicates whether the stream supports Seek operations.
		/// </summary>
		/// <remarks>
		///     Always returns false.
		/// </remarks>
		public override bool CanSeek
		{
			get
			{
				return false;
			}
		}

		/// <summary>
		///     Indicates whether the stream can be written.
		/// </summary>
		/// <remarks>
		///     The return value depends on whether the captive stream supports writing.
		/// </remarks>
		public override bool CanWrite
		{
			get
			{
				if( _disposed )
				{
					throw new ObjectDisposedException( "BZip2Stream" );
				}
				return InputStream.CanWrite;
			}
		}

		/// <summary>
		///     Reading this property always throws a <see cref="NotImplementedException" />.
		/// </summary>
		public override long Length
		{
			get
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		///     The position of the stream pointer.
		/// </summary>
		/// <remarks>
		///     Setting this property always throws a <see cref="NotImplementedException" />. Reading will return the
		///     total number of uncompressed bytes read in.
		/// </remarks>
		public override long Position
		{
			get
			{
				return totalBytesRead;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		///     Read data from the stream.
		/// </summary>
		/// <remarks>
		///     <para>
		///         To decompress a BZip2 data stream, create a <c>BZip2InputStream</c>,
		///         providing a stream that reads compressed data.  Then call Read() on
		///         that <c>BZip2InputStream</c>, and the data read will be decompressed
		///         as you read.
		///     </para>
		///     <para>
		///         A <c>BZip2InputStream</c> can be used only for <c>Read()</c>, not for <c>Write()</c>.
		///     </para>
		/// </remarks>
		/// <param name="buffer">The buffer into which the read data should be placed.</param>
		/// <param name="offset">the offset within that data array to put the first byte read.</param>
		/// <param name="count">the number of bytes to read.</param>
		/// <returns>the number of bytes actually read</returns>
		public override int Read( byte[] buffer, int offset, int count )
		{
			if( offset < 0 )
			{
				throw new IndexOutOfRangeException( "offset (" + offset + ") must be > 0" );
			}
			
			if( count < 0 )
			{
				throw new IndexOutOfRangeException( "count (" + count + ") must be > 0" );
			}

			if( offset + count > buffer.Length )
			{
				throw new IndexOutOfRangeException( "offset(" + offset + ") count(" + count + ") bLength(" + buffer.Length + ")" );
			}

			if( InputStream == null )
			{
				throw new IOException( "the stream is not open" );
			}

			int MaxOffset = offset + count;
			int DestOffset = offset;
			for( int Datum; ( DestOffset < MaxOffset ) && ( ( Datum = ReadByte() ) >= 0 ); )
			{
				buffer[DestOffset++] = ( byte )Datum;
			}

			return ( DestOffset == offset ) ? -1 : ( DestOffset - offset );
		}

		private void MakeMaps()
		{
			bool[] inUse = data.inUse;
			byte[] seqToUnseq = data.seqToUnseq;

			int n = 0;

			for( int i = 0; i < 256; i++ )
			{
				if( inUse[i] )
				{
					seqToUnseq[n++] = ( byte )i;
				}
			}

			nInUse = n;
		}

		/// <summary>
		///     Read a single byte from the stream.
		/// </summary>
		/// <returns>the byte read from the stream, or -1 if EOF</returns>
		public override int ReadByte()
		{
			int retChar = currentChar;
			totalBytesRead++;
			switch( currentState )
			{
			case CState.EOF:
				return -1;

			case CState.START_BLOCK:
				throw new IOException( "bad state" );

			case CState.RAND_PART_A:
				throw new IOException( "bad state" );

			case CState.RAND_PART_B:
				SetupRandPartB();
				break;

			case CState.RAND_PART_C:
				SetupRandPartC();
				break;

			case CState.NO_RAND_PART_A:
				throw new IOException( "bad state" );

			case CState.NO_RAND_PART_B:
				SetupNoRandPartB();
				break;

			case CState.NO_RAND_PART_C:
				SetupNoRandPartC();
				break;

			default:
				throw new IOException( "bad state" );
			}

			return retChar;
		}

		/// <summary>
		///     Flush the stream.
		/// </summary>
		public override void Flush()
		{
			if( _disposed )
			{
				throw new ObjectDisposedException( "BZip2Stream" );
			}
			InputStream.Flush();
		}

		/// <summary>
		///     Calling this method always throws a <see cref="NotImplementedException" />.
		/// </summary>
		/// <param name="offset">this is irrelevant, since it will always throw!</param>
		/// <param name="origin">this is irrelevant, since it will always throw!</param>
		/// <returns>irrelevant!</returns>
		public override long Seek( long offset, SeekOrigin origin )
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Calling this method always throws a <see cref="NotImplementedException" />.
		/// </summary>
		/// <param name="value">this is irrelevant, since it will always throw!</param>
		public override void SetLength( long value )
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Calling this method always throws a <see cref="NotImplementedException" />.
		/// </summary>
		/// <param name='buffer'>this parameter is never used</param>
		/// <param name='offset'>this parameter is never used</param>
		/// <param name='count'>this parameter is never used</param>
		public override void Write( byte[] buffer, int offset, int count )
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///     Dispose the stream.
		/// </summary>
		/// <param name="disposing">
		///     indicates whether the Dispose method was invoked by user code.
		/// </param>
		protected override void Dispose( bool disposing )
		{
			try
			{
				if( !_disposed )
				{
					if( disposing && ( InputStream != null ) )
					{
						InputStream.Close();
					}
					_disposed = true;
				}
			}
			finally
			{
				base.Dispose( disposing );
			}
		}

		private void init()
		{
			if( null == InputStream )
			{
				throw new IOException( "No input Stream" );
			}

			if( !InputStream.CanRead )
			{
				throw new IOException( "Unreadable input Stream" );
			}

			CheckMagicChar( 'B', 0 );
			CheckMagicChar( 'Z', 1 );
			CheckMagicChar( 'h', 2 );

			int blockSize = InputStream.ReadByte();

			if( ( blockSize < '1' ) || ( blockSize > '9' ) )
			{
				throw new IOException( "Stream is not BZip2 formatted: illegal " + "blocksize " + ( char )blockSize );
			}

			blockSize100k = blockSize - '0';

			InitBlock();
			SetupBlock();
		}

		private void CheckMagicChar( char ExpectedDatum, int ReadOffset )
		{
			int MagicDatum = InputStream.ReadByte();
			if( MagicDatum != ExpectedDatum )
			{
				string ExceptionMessage = "Not a valid BZip2 stream. Byte offset " + ReadOffset + ", expected '" + ExpectedDatum + "', got '" + MagicDatum + "'";
				throw new IOException( ExceptionMessage );
			}
		}

		private void InitBlock()
		{
			char magic0 = bsGetUByte();
			char magic1 = bsGetUByte();
			char magic2 = bsGetUByte();
			char magic3 = bsGetUByte();
			char magic4 = bsGetUByte();
			char magic5 = bsGetUByte();

			if( magic0 == 0x17 && magic1 == 0x72 && magic2 == 0x45 && magic3 == 0x38 && magic4 == 0x50 && magic5 == 0x90 )
			{
				complete(); // end of file
			}
			else if( magic0 != 0x31 || magic1 != 0x41 || magic2 != 0x59 || magic3 != 0x26 || magic4 != 0x53 || magic5 != 0x59 )
			{
				currentState = CState.EOF;
				string ExceptionMessage = "Bad block header at offset 0x" + InputStream.Position.ToString( "x" );
				throw new IOException( ExceptionMessage );
			}
			else
			{
				storedBlockCRC = bsGetInt();

				blockRandomised = ( GetBits( 1 ) == 1 );

				// Lazily allocate data
				if( data == null )
				{
					data = new DecompressionState( blockSize100k );
				}

				// currBlockNo++;
				getAndMoveToFrontDecode();

				crc.Reset();
				currentState = CState.START_BLOCK;
			}
		}

		private void EndBlock()
		{
			computedBlockCRC = ( uint )crc.Crc32Result;

			// A bad CRC is considered a fatal error.
			if( storedBlockCRC != computedBlockCRC )
			{
				// make next blocks readable without error
				// (repair feature, not yet documented, not tested)
				// this.computedCombinedCRC = (this.storedCombinedCRC << 1) | (this.storedCombinedCRC >> 31);
				// this.computedCombinedCRC ^= this.storedBlockCRC;

				string ExceptionMessage = "BZip2 CRC error (expected " + storedBlockCRC.ToString( "x8" ) + ", computed " + computedBlockCRC.ToString( "x8" ) + ")";
				throw new IOException( ExceptionMessage );
			}

			// Console.WriteLine(" combined CRC (before): {0:X8}", this.computedCombinedCRC);
			computedCombinedCRC = ( computedCombinedCRC << 1 ) | ( computedCombinedCRC >> 31 );
			computedCombinedCRC ^= computedBlockCRC;
			// Console.WriteLine(" computed block  CRC  : {0:X8}", this.computedBlockCRC);
			// Console.WriteLine(" combined CRC (after) : {0:X8}", this.computedCombinedCRC);
			// Console.WriteLine();
		}

		private void complete()
		{
			storedCombinedCRC = bsGetInt();
			currentState = CState.EOF;
			data = null;

			if( storedCombinedCRC != computedCombinedCRC )
			{
				string ExceptionMessage = "BZip2 CRC error (expected " + storedCombinedCRC.ToString( "x8" ) + ", computed " + computedCombinedCRC.ToString( "x8" ) + ")";
				throw new IOException( ExceptionMessage );
			}
		}

		/// <summary>
		///     Close the stream.
		/// </summary>
		public override void Close()
		{
			Stream inShadow = InputStream;
			if( inShadow != null )
			{
				try
				{
					if( !_leaveOpen )
					{
						inShadow.Close();
					}
				}
				finally
				{
					data = null;
					InputStream = null;
				}
			}
		}

		/// <summary>
		///     Read n bits from input, right justifying the result.
		/// </summary>
		/// <remarks>
		///     <para>
		///         For example, if you read 1 bit, the result is either 0
		///         or 1.
		///     </para>
		/// </remarks>
		/// <param name="n">
		///     The number of bits to read, always between 1 and 32.
		/// </param>
		private int GetBits( int n )
		{
			int bsLiveShadow = bsLive;
			int bsBuffShadow = bsBuff;

			if( bsLiveShadow < n )
			{
				do
				{
					int thech = InputStream.ReadByte();

					if( thech < 0 )
					{
						throw new IOException( "Unexpected end of stream" );
					}

					// Console.WriteLine("R {0:X2}", thech);

					bsBuffShadow = ( bsBuffShadow << 8 ) | thech;
					bsLiveShadow += 8;
				}
				while( bsLiveShadow < n );

				bsBuff = bsBuffShadow;
			}

			bsLive = bsLiveShadow - n;
			return ( bsBuffShadow >> ( bsLiveShadow - n ) ) & ( ( 1 << n ) - 1 );
		}

		private bool bsGetBit()
		{
			int bit = GetBits( 1 );
			return bit != 0;
		}

		private char bsGetUByte()
		{
			return ( char )GetBits( 8 );
		}

		private uint bsGetInt()
		{
			return ( uint )( ( ( ( ( ( GetBits( 8 ) << 8 ) | GetBits( 8 ) ) << 8 ) | GetBits( 8 ) ) << 8 ) | GetBits( 8 ) );
		}

		/**
		 * Called by createHuffmanDecodingTables() exclusively.
		 */

		private static void hbCreateDecodeTables( int[] limit, int[] bbase, int[] perm, char[] length, int minLen, int maxLen, int alphaSize )
		{
			for( int i = minLen, pp = 0; i <= maxLen; i++ )
			{
				for( int j = 0; j < alphaSize; j++ )
				{
					if( length[j] == i )
					{
						perm[pp++] = j;
					}
				}
			}

			for( int i = BZip2.MaxCodeLength; --i > 0; )
			{
				bbase[i] = 0;
				limit[i] = 0;
			}

			for( int i = 0; i < alphaSize; i++ )
			{
				bbase[length[i] + 1]++;
			}

			for( int i = 1, b = bbase[0]; i < BZip2.MaxCodeLength; i++ )
			{
				b += bbase[i];
				bbase[i] = b;
			}

			for( int i = minLen, vec = 0, b = bbase[i]; i <= maxLen; i++ )
			{
				int nb = bbase[i + 1];
				vec += nb - b;
				b = nb;
				limit[i] = vec - 1;
				vec <<= 1;
			}

			for( int i = minLen + 1; i <= maxLen; i++ )
			{
				bbase[i] = ( ( limit[i - 1] + 1 ) << 1 ) - bbase[i];
			}
		}

		private void recvDecodingTables()
		{
			DecompressionState s = data;
			bool[] inUse = s.inUse;
			byte[] pos = s.recvDecodingTables_pos;
			//byte[] selector = s.selector;

			int inUse16 = 0;

			/* Receive the mapping table */
			for( int i = 0; i < 16; i++ )
			{
				if( bsGetBit() )
				{
					inUse16 |= 1 << i;
				}
			}

			for( int i = 256; --i >= 0; )
			{
				inUse[i] = false;
			}

			for( int i = 0; i < 16; i++ )
			{
				if( ( inUse16 & ( 1 << i ) ) != 0 )
				{
					int i16 = i << 4;
					for( int j = 0; j < 16; j++ )
					{
						if( bsGetBit() )
						{
							inUse[i16 + j] = true;
						}
					}
				}
			}

			MakeMaps();
			int alphaSize = nInUse + 2;

			/* Now the selectors */
			int nGroups = GetBits( 3 );
			int nSelectors = GetBits( 15 );

			for( int i = 0; i < nSelectors; i++ )
			{
				int j = 0;
				while( bsGetBit() )
				{
					j++;
				}
				s.selectorMtf[i] = ( byte )j;
			}

			/* Undo the MTF values for the selectors. */
			for( int v = nGroups; --v >= 0; )
			{
				pos[v] = ( byte )v;
			}

			for( int i = 0; i < nSelectors; i++ )
			{
				int v = s.selectorMtf[i];
				byte tmp = pos[v];
				while( v > 0 )
				{
					// nearly all times v is zero, 4 in most other cases
					pos[v] = pos[v - 1];
					v--;
				}
				pos[0] = tmp;
				s.selector[i] = tmp;
			}

			char[][] len = s.temp_charArray2d;

			/* Now the coding tables */
			for( int t = 0; t < nGroups; t++ )
			{
				int curr = GetBits( 5 );
				char[] len_t = len[t];
				for( int i = 0; i < alphaSize; i++ )
				{
					while( bsGetBit() )
					{
						curr += bsGetBit() ? -1 : 1;
					}
					len_t[i] = ( char )curr;
				}
			}

			// finally create the Huffman tables
			createHuffmanDecodingTables( alphaSize, nGroups );
		}

		/**
		 * Called by recvDecodingTables() exclusively.
		 */
		private void createHuffmanDecodingTables( int alphaSize, int nGroups )
		{
			DecompressionState s = data;
			char[][] len = s.temp_charArray2d;

			for( int t = 0; t < nGroups; t++ )
			{
				int minLen = 32;
				int maxLen = 0;
				char[] len_t = len[t];
				for( int i = alphaSize; --i >= 0; )
				{
					char lent = len_t[i];
					if( lent > maxLen )
					{
						maxLen = lent;
					}

					if( lent < minLen )
					{
						minLen = lent;
					}
				}
				hbCreateDecodeTables( s.gLimit[t], s.gBase[t], s.gPerm[t], len[t], minLen, maxLen, alphaSize );
				s.gMinlen[t] = minLen;
			}
		}

		private void getAndMoveToFrontDecode()
		{
			DecompressionState s = data;
			origPtr = GetBits( 24 );

			if( origPtr < 0 )
			{
				throw new IOException( "BZ_DATA_ERROR" );
			}
			if( origPtr > 10 + BZip2.BlockSizeMultiple * blockSize100k )
			{
				throw new IOException( "BZ_DATA_ERROR" );
			}

			recvDecodingTables();

			byte[] yy = s.getAndMoveToFrontDecode_yy;
			int limitLast = blockSize100k * BZip2.BlockSizeMultiple;

			/*
			 * Setting up the unzftab entries here is not strictly necessary, but it does save having to do it later in a separate pass, and so saves a
			 * block's worth of cache misses.
			 */
			for( int i = 256; --i >= 0; )
			{
				yy[i] = ( byte )i;
				s.unzftab[i] = 0;
			}

			int groupNo = 0;
			int groupPos = BZip2.G_SIZE - 1;
			int eob = nInUse + 1;
			int nextSym = getAndMoveToFrontDecode0( 0 );
			int bsBuffShadow = bsBuff;
			int bsLiveShadow = bsLive;
			int lastShadow = -1;
			int zt = s.selector[groupNo] & 0xff;
			int[] base_zt = s.gBase[zt];
			int[] limit_zt = s.gLimit[zt];
			int[] perm_zt = s.gPerm[zt];
			int minLens_zt = s.gMinlen[zt];

			while( nextSym != eob )
			{
				if( ( nextSym == BZip2.RUNA ) || ( nextSym == BZip2.RUNB ) )
				{
					int es = -1;

					for( int n = 1;; n <<= 1 )
					{
						if( nextSym == BZip2.RUNA )
						{
							es += n;
						}
						else if( nextSym == BZip2.RUNB )
						{
							es += n << 1;
						}
						else
						{
							break;
						}

						if( groupPos == 0 )
						{
							groupPos = BZip2.G_SIZE - 1;
							zt = s.selector[++groupNo] & 0xff;
							base_zt = s.gBase[zt];
							limit_zt = s.gLimit[zt];
							perm_zt = s.gPerm[zt];
							minLens_zt = s.gMinlen[zt];
						}
						else
						{
							groupPos--;
						}

						int zn = minLens_zt;

						// Inlined:
						// int zvec = GetBits(zn);
						while( bsLiveShadow < zn )
						{
							int thech = InputStream.ReadByte();
							if( thech >= 0 )
							{
								bsBuffShadow = ( bsBuffShadow << 8 ) | thech;
								bsLiveShadow += 8;
							}
							throw new IOException( "unexpected end of stream" );
						}
						int zvec = ( bsBuffShadow >> ( bsLiveShadow - zn ) ) & ( ( 1 << zn ) - 1 );
						bsLiveShadow -= zn;

						while( zvec > limit_zt[zn] )
						{
							zn++;
							while( bsLiveShadow < 1 )
							{
								int thech = InputStream.ReadByte();
								if( thech >= 0 )
								{
									bsBuffShadow = ( bsBuffShadow << 8 ) | thech;
									bsLiveShadow += 8;
								}
								throw new IOException( "unexpected end of stream" );
							}
							bsLiveShadow--;
							zvec = ( zvec << 1 ) | ( ( bsBuffShadow >> bsLiveShadow ) & 1 );
						}
						nextSym = perm_zt[zvec - base_zt[zn]];
					}

					byte ch = s.seqToUnseq[yy[0]];
					s.unzftab[ch & 0xff] += es + 1;

					while( es-- >= 0 )
					{
						s.ll8[++lastShadow] = ch;
					}

					if( lastShadow >= limitLast )
					{
						throw new IOException( "block overrun" );
					}
				}
				else
				{
					if( ++lastShadow >= limitLast )
					{
						throw new IOException( "block overrun" );
					}

					byte tmp = yy[nextSym - 1];
					s.unzftab[s.seqToUnseq[tmp] & 0xff]++;
					s.ll8[lastShadow] = s.seqToUnseq[tmp];

					/*
					 * This loop is hammered during decompression, hence avoid
					 * native method call overhead of System.Buffer.BlockCopy for very
					 * small ranges to copy.
					 */
					if( nextSym <= 16 )
					{
						for( int j = nextSym - 1; j > 0; )
						{
							yy[j] = yy[--j];
						}
					}
					else
					{
						Buffer.BlockCopy( yy, 0, yy, 1, nextSym - 1 );
					}

					yy[0] = tmp;

					if( groupPos == 0 )
					{
						groupPos = BZip2.G_SIZE - 1;
						zt = s.selector[++groupNo] & 0xff;
						base_zt = s.gBase[zt];
						limit_zt = s.gLimit[zt];
						perm_zt = s.gPerm[zt];
						minLens_zt = s.gMinlen[zt];
					}
					else
					{
						groupPos--;
					}

					int zn = minLens_zt;

					// Inlined:
					// int zvec = GetBits(zn);
					while( bsLiveShadow < zn )
					{
						int thech = InputStream.ReadByte();
						if( thech >= 0 )
						{
							bsBuffShadow = ( bsBuffShadow << 8 ) | thech;
							bsLiveShadow += 8;
						}
						throw new IOException( "unexpected end of stream" );
					}
					int zvec = ( bsBuffShadow >> ( bsLiveShadow - zn ) ) & ( ( 1 << zn ) - 1 );
					bsLiveShadow -= zn;

					while( zvec > limit_zt[zn] )
					{
						zn++;
						while( bsLiveShadow < 1 )
						{
							int thech = InputStream.ReadByte();
							if( thech >= 0 )
							{
								bsBuffShadow = ( bsBuffShadow << 8 ) | thech;
								bsLiveShadow += 8;
							}
							throw new IOException( "unexpected end of stream" );
						}
						bsLiveShadow--;
						zvec = ( zvec << 1 ) | ( ( bsBuffShadow >> bsLiveShadow ) & 1 );
					}
					nextSym = perm_zt[zvec - base_zt[zn]];
				}
			}

			last = lastShadow;
			bsLive = bsLiveShadow;
			bsBuff = bsBuffShadow;
		}

		private int getAndMoveToFrontDecode0( int groupNo )
		{
			DecompressionState s = data;
			int zt = s.selector[groupNo] & 0xff;
			int[] limit_zt = s.gLimit[zt];
			int zn = s.gMinlen[zt];
			int zvec = GetBits( zn );
			int bsLiveShadow = bsLive;
			int bsBuffShadow = bsBuff;

			while( zvec > limit_zt[zn] )
			{
				zn++;
				while( bsLiveShadow < 1 )
				{
					int thech = InputStream.ReadByte();

					if( thech >= 0 )
					{
						bsBuffShadow = ( bsBuffShadow << 8 ) | thech;
						bsLiveShadow += 8;
					}
					throw new IOException( "unexpected end of stream" );
				}
				bsLiveShadow--;
				zvec = ( zvec << 1 ) | ( ( bsBuffShadow >> bsLiveShadow ) & 1 );
			}

			bsLive = bsLiveShadow;
			bsBuff = bsBuffShadow;

			return s.gPerm[zt][zvec - s.gBase[zt][zn]];
		}

		private void SetupBlock()
		{
			if( data == null )
			{
				return;
			}

			int i;
			DecompressionState s = data;
			int[] tt = s.initTT( last + 1 );

			/* Check: unzftab entries in range. */
			for( i = 0; i <= 255; i++ )
			{
				if( s.unzftab[i] < 0 || s.unzftab[i] > last )
				{
					throw new Exception( "BZ_DATA_ERROR" );
				}
			}

			/* Actually generate cftab. */
			s.cftab[0] = 0;
			for( i = 1; i <= 256; i++ )
			{
				s.cftab[i] = s.unzftab[i - 1];
			}
			for( i = 1; i <= 256; i++ )
			{
				s.cftab[i] += s.cftab[i - 1];
			}
			/* Check: cftab entries in range. */
			for( i = 0; i <= 256; i++ )
			{
				if( s.cftab[i] < 0 || s.cftab[i] > last + 1 )
				{
					string ExceptionMessage = "BZ_DATA_ERROR: cftab[" + i + "]=" + s.cftab[i] + " last=" + last;
					throw new Exception( ExceptionMessage );
				}
			}
			/* Check: cftab entries non-descending. */
			for( i = 1; i <= 256; i++ )
			{
				if( s.cftab[i - 1] > s.cftab[i] )
				{
					throw new Exception( "BZ_DATA_ERROR" );
				}
			}

			int lastShadow;
			for( i = 0, lastShadow = last; i <= lastShadow; i++ )
			{
				tt[s.cftab[s.ll8[i] & 0xff]++] = i;
			}

			if( ( origPtr < 0 ) || ( origPtr >= tt.Length ) )
			{
				throw new IOException( "stream corrupted" );
			}

			su_tPos = tt[origPtr];
			su_count = 0;
			su_i2 = 0;
			su_ch2 = 256; /* not a valid 8-bit byte value?, and not EOF */

			if( blockRandomised )
			{
				su_rNToGo = 0;
				su_rTPos = 0;
				SetupRandPartA();
			}
			else
			{
				SetupNoRandPartA();
			}
		}

		private void SetupRandPartA()
		{
			if( su_i2 <= last )
			{
				su_chPrev = su_ch2;
				int su_ch2Shadow = data.ll8[su_tPos] & 0xff;
				su_tPos = data.tt[su_tPos];
				if( su_rNToGo == 0 )
				{
					su_rNToGo = Rand.Rnums( su_rTPos ) - 1;
					if( ++su_rTPos == 512 )
					{
						su_rTPos = 0;
					}
				}
				else
				{
					su_rNToGo--;
				}
				su_ch2 = su_ch2Shadow ^= ( su_rNToGo == 1 ) ? 1 : 0;
				su_i2++;
				currentChar = su_ch2Shadow;
				currentState = CState.RAND_PART_B;
				crc.UpdateCRC( ( byte )su_ch2Shadow );
			}
			else
			{
				EndBlock();
				InitBlock();
				SetupBlock();
			}
		}

		private void SetupNoRandPartA()
		{
			if( su_i2 <= last )
			{
				su_chPrev = su_ch2;
				int su_ch2Shadow = data.ll8[su_tPos] & 0xff;
				su_ch2 = su_ch2Shadow;
				su_tPos = data.tt[su_tPos];
				su_i2++;
				currentChar = su_ch2Shadow;
				currentState = CState.NO_RAND_PART_B;
				crc.UpdateCRC( ( byte )su_ch2Shadow );
			}
			else
			{
				currentState = CState.NO_RAND_PART_A;
				EndBlock();
				InitBlock();
				SetupBlock();
			}
		}

		private void SetupRandPartB()
		{
			if( su_ch2 != su_chPrev )
			{
				currentState = CState.RAND_PART_A;
				su_count = 1;
				SetupRandPartA();
			}
			else if( ++su_count >= 4 )
			{
				su_z = ( char )( data.ll8[su_tPos] & 0xff );
				su_tPos = data.tt[su_tPos];
				if( su_rNToGo == 0 )
				{
					su_rNToGo = Rand.Rnums( su_rTPos ) - 1;
					if( ++su_rTPos == 512 )
					{
						su_rTPos = 0;
					}
				}
				else
				{
					su_rNToGo--;
				}
				su_j2 = 0;
				currentState = CState.RAND_PART_C;
				if( su_rNToGo == 1 )
				{
					su_z ^= ( char )1;
				}
				SetupRandPartC();
			}
			else
			{
				currentState = CState.RAND_PART_A;
				SetupRandPartA();
			}
		}

		private void SetupRandPartC()
		{
			if( su_j2 < su_z )
			{
				currentChar = su_ch2;
				crc.UpdateCRC( ( byte )su_ch2 );
				su_j2++;
			}
			else
			{
				currentState = CState.RAND_PART_A;
				su_i2++;
				su_count = 0;
				SetupRandPartA();
			}
		}

		private void SetupNoRandPartB()
		{
			if( su_ch2 != su_chPrev )
			{
				su_count = 1;
				SetupNoRandPartA();
			}
			else if( ++su_count >= 4 )
			{
				su_z = ( char )( data.ll8[su_tPos] & 0xff );
				su_tPos = data.tt[su_tPos];
				su_j2 = 0;
				SetupNoRandPartC();
			}
			else
			{
				SetupNoRandPartA();
			}
		}

		private void SetupNoRandPartC()
		{
			if( su_j2 < su_z )
			{
				int su_ch2Shadow = su_ch2;
				currentChar = su_ch2Shadow;
				crc.UpdateCRC( ( byte )su_ch2Shadow );
				su_j2++;
				currentState = CState.NO_RAND_PART_C;
			}
			else
			{
				su_i2++;
				su_count = 0;
				SetupNoRandPartA();
			}
		}

		/// <summary>
		///     Compressor State
		/// </summary>
		private enum CState
		{
			EOF = 0,
			START_BLOCK = 1,
			RAND_PART_A = 2,
			RAND_PART_B = 3,
			RAND_PART_C = 4,
			NO_RAND_PART_A = 5,
			NO_RAND_PART_B = 6,
			NO_RAND_PART_C = 7,
		}

		private sealed class DecompressionState
		{
			// (with blockSize 900k)
			public readonly int[] cftab;
			public readonly int[][] gBase;
			public readonly int[][] gLimit;
			public readonly int[] gMinlen;
			public readonly int[][] gPerm;

			public readonly byte[] getAndMoveToFrontDecode_yy;
			public readonly bool[] inUse = new bool[256];
			public readonly byte[] ll8; // 900000 byte
			public readonly byte[] recvDecodingTables_pos;
			public readonly byte[] selector = new byte[BZip2.MaxSelectors]; // 18002 byte
			public readonly byte[] selectorMtf = new byte[BZip2.MaxSelectors]; // 18002 byte
			public readonly byte[] seqToUnseq = new byte[256]; // 256 byte
			public readonly char[][] temp_charArray2d;
			public readonly int[] unzftab;
			// ---------------
			// 60798 byte

			public int[] tt; // 3600000 byte

			// ---------------
			// 4560782 byte
			// ===============

			public DecompressionState( int blockSize100k )
			{
				unzftab = new int[256]; // 1024 byte

				gLimit = BZip2.InitRectangularArray<int>( BZip2.NGroups, BZip2.MaxAlphaSize );
				gBase = BZip2.InitRectangularArray<int>( BZip2.NGroups, BZip2.MaxAlphaSize );
				gPerm = BZip2.InitRectangularArray<int>( BZip2.NGroups, BZip2.MaxAlphaSize );
				gMinlen = new int[BZip2.NGroups]; // 24 byte

				cftab = new int[257]; // 1028 byte
				getAndMoveToFrontDecode_yy = new byte[256]; // 512 byte
				temp_charArray2d = BZip2.InitRectangularArray<char>( BZip2.NGroups, BZip2.MaxAlphaSize );
				recvDecodingTables_pos = new byte[BZip2.NGroups]; // 6 byte

				ll8 = new byte[blockSize100k * BZip2.BlockSizeMultiple];
			}

			/**
			 * Initializes the tt array.
			 *
			 * This method is called when the required length of the array is known.
			 * I don't initialize it at construction time to avoid unneccessary
			 * memory allocation when compressing small files.
			 */

			public int[] initTT( int length )
			{
				int[] ttShadow = tt;

				// tt.length should always be >= length, but theoretically
				// it can happen, if the compressor mixed small and large
				// blocks. Normally only the last block will be smaller
				// than others.
				if( ( ttShadow == null ) || ( ttShadow.Length < length ) )
				{
					tt = ttShadow = new int[length];
				}

				return ttShadow;
			}
		}
	}

	// /**
	//  * Checks if the signature matches what is expected for a bzip2 file.
	//  *
	//  * @param signature
	//  *            the bytes to check
	//  * @param length
	//  *            the number of bytes to check
	//  * @return true, if this stream is a bzip2 compressed stream, false otherwise
	//  *
	//  * @since Apache Commons Compress 1.1
	//  */
	// public static boolean MatchesSig(byte[] signature)
	// {
	//     if ((signature.Length < 3) ||
	//         (signature[0] != 'B') ||
	//         (signature[1] != 'Z') ||
	//         (signature[2] != 'h'))
	//         return false;
	//
	//     return true;
	// }

	internal static class BZip2
	{
		public const int BlockSizeMultiple = 100000;
		public const int MinBlockSize = 1;
		public const int MaxBlockSize = 9;
		public const int MaxAlphaSize = 258;
		public const int MaxCodeLength = 23;
		public const char RUNA = ( char )0;
		public const char RUNB = ( char )1;
		public const int NGroups = 6;
		public const int G_SIZE = 50;
		public const int N_ITERS = 4;
		public const int MaxSelectors = ( 2 + ( 900000 / G_SIZE ) );
		public const int NUM_OVERSHOOT_BYTES = 20;
		/*
		 * <p> If you are ever unlucky/improbable enough to get a stack overflow whilst sorting, increase the following constant and
		 * try again. In practice I have never seen the stack go above 27 elems, so the following limit seems very generous.  </p>
		 */
		internal const int QSORT_STACK_SIZE = 1000;

		internal static T[][] InitRectangularArray<T>( int d1, int d2 )
		{
			T[][] x = new T[d1][];
			for( int i = 0; i < d1; i++ )
			{
				x[i] = new T[d2];
			}
			return x;
		}
	}
}
