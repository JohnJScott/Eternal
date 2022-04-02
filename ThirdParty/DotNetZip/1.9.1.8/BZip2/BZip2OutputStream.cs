//#define Trace

// BZip2OutputStream.cs
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
// Last Saved: <2011-August-02 16:44:11>
//
// ------------------------------------------------------------------
//
// This module defines the BZip2OutputStream class, which is a
// compressing stream that handles BZIP2. This code may have been
// derived in part from Apache commons source code. The license below
// applies to the original Apache code.
//
// ------------------------------------------------------------------
// flymake: csc.exe /t:module BZip2InputStream.cs BZip2Compressor.cs Rand.cs BCRC32.cs @@FILE@@

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

// Design Notes:
//
// This class follows the classic Decorator pattern: it is a Stream that
// wraps itself around a Stream, and in doing so provides bzip2
// compression as callers Write into it.
//
// BZip2 is a straightforward data format: there are 4 magic bytes at
// the top of the file, followed by 1 or more compressed blocks. There
// is a small "magic byte" trailer after all compressed blocks. This
// class emits the magic bytes for the header and trailer, and relies on
// a BZip2Compressor to generate each of the compressed data blocks.
//
// BZip2 does byte-shredding - it uses partial fractions of bytes to
// represent independent pieces of information. This class relies on the
// BitWriter to adapt the bit-oriented BZip2 output to the byte-oriented
// model of the .NET Stream class.
//
// ----
//
// Regarding the Apache code base: Most of the code in this particular
// class is related to stream operations, and is my own code. It largely
// does not rely on any code obtained from Apache commons. If you
// compare this code with the Apache commons BZip2OutputStream, you will
// see very little code that is common, except for the
// nearly-boilerplate structure that is common to all subtypes of
// System.IO.Stream. There may be some small remnants of code in this
// module derived from the Apache stuff, which is why I left the license
// in here. Most of the Apache commons compressor magic has been ported
// into the BZip2Compressor class.
//

using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace Ionic.BZip2
{
	/// <summary>
	///     A write-only decorator stream that compresses data as it is
	///     written using the BZip2 algorithm.
	/// </summary>
	public class BZip2OutputStream : Stream
	{
		private readonly int blockSize100k; // 0...9
		private readonly bool leaveOpen;
		private BitWriter bw;
		private uint combinedCRC;
		private BZip2Compressor compressor;

		private Stream output;
		private int totalBytesWrittenIn;

		/// <summary>
		///     Constructs a new <c>BZip2OutputStream</c>, that sends its
		///     compressed output to the given output stream.
		/// </summary>
		/// <param name='output'>
		///     The destination stream, to which compressed output will be sent.
		/// </param>
		/// <example>
		///     This example reads a file, then compresses it with bzip2 file,
		///     and writes the compressed data into a newly created file.
		///     <code>
		///    var fname = "logfile.log";
		///    using (var fs = File.OpenRead(fname))
		///    {
		///        var outFname = fname + ".bz2";
		///        using (var output = File.Create(outFname))
		///        {
		///            using (var compressor = new Ionic.BZip2.BZip2OutputStream(output))
		///            {
		///                byte[] buffer = new byte[2048];
		///                int n;
		///                while ((n = fs.Read(buffer, 0, buffer.Length)) > 0)
		///                {
		///                    compressor.Write(buffer, 0, n);
		///                }
		///            }
		///        }
		///    }
		///    </code>
		/// </example>
		public BZip2OutputStream( Stream output )
			: this( output, BZip2.MaxBlockSize, false )
		{
		}

		/// <summary>
		///     Constructs a new <c>BZip2OutputStream</c> with specified blocksize.
		/// </summary>
		/// <param name="output">the destination stream.</param>
		/// <param name="blockSize">
		///     The blockSize in units of 100000 bytes.
		///     The valid range is 1..9.
		/// </param>
		public BZip2OutputStream( Stream output, int blockSize )
			: this( output, blockSize, false )
		{
		}

		/// <summary>
		///     Constructs a new <c>BZip2OutputStream</c>.
		/// </summary>
		/// <param name="output">the destination stream.</param>
		/// <param name="leaveOpen">
		///     whether to leave the captive stream open upon closing this stream.
		/// </param>
		public BZip2OutputStream( Stream output, bool leaveOpen )
			: this( output, BZip2.MaxBlockSize, leaveOpen )
		{
		}

		/// <summary>
		///     Constructs a new <c>BZip2OutputStream</c> with specified blocksize,
		///     and explicitly specifies whether to leave the wrapped stream open.
		/// </summary>
		/// <param name="output">the destination stream.</param>
		/// <param name="blockSize">
		///     The blockSize in units of 100000 bytes.
		///     The valid range is 1..9.
		/// </param>
		/// <param name="leaveOpen">
		///     whether to leave the captive stream open upon closing this stream.
		/// </param>
		public BZip2OutputStream( Stream output, int blockSize, bool leaveOpen )
		{
			if( blockSize < BZip2.MinBlockSize || blockSize > BZip2.MaxBlockSize )
			{
				string ExceptionMessage = "blockSize=" + blockSize + " is out of range; must be between " + BZip2.MinBlockSize + " and " + BZip2.MaxBlockSize;
				throw new ArgumentException( ExceptionMessage, "blockSize" );
			}

			this.output = output;
			if( !this.output.CanWrite )
			{
				throw new ArgumentException( "The stream is not writable.", "output" );
			}

			bw = new BitWriter( this.output );
			blockSize100k = blockSize;
			compressor = new BZip2Compressor( bw, blockSize );
			this.leaveOpen = leaveOpen;
			combinedCRC = 0;
			EmitHeader();
		}

		/// <summary>
		///     The blocksize parameter specified at construction time.
		/// </summary>
		public int BlockSize
		{
			get
			{
				return blockSize100k;
			}
		}

		/// <summary>
		///     Indicates whether the stream can be read.
		/// </summary>
		/// <remarks>
		///     The return value is always false.
		/// </remarks>
		public override bool CanRead
		{
			get
			{
				return false;
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
		///     The return value should always be true, unless and until the
		///     object is disposed and closed.
		/// </remarks>
		public override bool CanWrite
		{
			get
			{
				if( output == null )
				{
					throw new ObjectDisposedException( "BZip2Stream" );
				}
				return output.CanWrite;
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
		///     total number of uncompressed bytes written through.
		/// </remarks>
		public override long Position
		{
			get
			{
				return totalBytesWrittenIn;
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		///     Close the stream.
		/// </summary>
		/// <remarks>
		///     <para>
		///         This may or may not close the underlying stream.  Check the constructors that accept a bool value.
		///     </para>
		/// </remarks>
		public override void Close()
		{
			if( output != null )
			{
				Stream o = output;
				Finish();
				if( !leaveOpen )
				{
					o.Close();
				}
			}
		}

		/// <summary>
		///     Flush the stream.
		/// </summary>
		public override void Flush()
		{
			if( output != null )
			{
				bw.Flush();
				output.Flush();
			}
		}

		private void EmitHeader()
		{
			byte[] magic =
			{
				( byte )'B',
				( byte )'Z',
				( byte )'h',
				( byte )( '0' + blockSize100k )
			};

			// not necessary to shred the initial magic bytes
			output.Write( magic, 0, magic.Length );
		}

		private void EmitTrailer()
		{
			// A magic 48-bit number, 0x177245385090, to indicate the end
			// of the last block. (sqrt(pi), if you want to know)

			// must shred
			bw.WriteByte( 0x17 );
			bw.WriteByte( 0x72 );
			bw.WriteByte( 0x45 );
			bw.WriteByte( 0x38 );
			bw.WriteByte( 0x50 );
			bw.WriteByte( 0x90 );

			bw.WriteInt( combinedCRC );

			bw.FinishAndPad();
		}

		private void Finish()
		{
			// Console.WriteLine("BZip2:Finish");

			try
			{
				int totalBefore = bw.TotalBytesWrittenOut;
				compressor.CompressAndWrite();

				combinedCRC = ( combinedCRC << 1 ) | ( combinedCRC >> 31 );
				combinedCRC ^= compressor.Crc32;

				EmitTrailer();
			}
			finally
			{
				output = null;
				compressor = null;
				bw = null;
			}
		}

		/// <summary>
		///     Write data to the stream.
		/// </summary>
		/// <remarks>
		///     <para>
		///         Use the <c>BZip2OutputStream</c> to compress data while writing:
		///         create a <c>BZip2OutputStream</c> with a writable output stream.
		///         Then call <c>Write()</c> on that <c>BZip2OutputStream</c>, providing
		///         uncompressed data as input.  The data sent to the output stream will
		///         be the compressed form of the input data.
		///     </para>
		///     <para>
		///         A <c>BZip2OutputStream</c> can be used only for <c>Write()</c> not for <c>Read()</c>.
		///     </para>
		/// </remarks>
		/// <param name="buffer">The buffer holding data to write to the stream.</param>
		/// <param name="offset">the offset within that data array to find the first byte to write.</param>
		/// <param name="count">the number of bytes to write.</param>
		public override void Write( byte[] buffer, int offset, int count )
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

			if( output == null )
			{
				throw new IOException( "the stream is not open" );
			}

			if( count == 0 )
			{
				// nothing to do
				return; 
			}

			int bytesWritten = 0;
			int bytesRemaining = count;

			do
			{
				int n = compressor.Fill( buffer, offset, bytesRemaining );
				if( n != bytesRemaining )
				{
					// The compressor data block is full.  Compress and
					// write out the compressed data, then reset the
					// compressor and continue.

					int totalBefore = bw.TotalBytesWrittenOut;
					compressor.CompressAndWrite();

					// and now any remaining bits
					combinedCRC = ( combinedCRC << 1 ) | ( combinedCRC >> 31 );
					combinedCRC ^= compressor.Crc32;
					offset += n;
				}

				bytesRemaining -= n;
				bytesWritten += n;
			}
			while( bytesRemaining > 0 );

			totalBytesWrittenIn += bytesWritten;
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
		/// <returns>never returns anything; always throws</returns>
		public override int Read( byte[] buffer, int offset, int count )
		{
			throw new NotImplementedException();
		}
	}
}
