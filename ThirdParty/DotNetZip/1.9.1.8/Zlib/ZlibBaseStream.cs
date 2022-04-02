// ZlibBaseStream.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2009 Dino Chiesa and Microsoft Corporation.
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
// last saved (in emacs):
// Time-stamp: <2011-August-06 21:22:38>
//
// ------------------------------------------------------------------
//
// This module defines the ZlibBaseStream class, which is an intnernal
// base class for DeflateStream, ZlibStream and GZipStream.
//
// ------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Ionic.Crc;

namespace Ionic.Zlib
{
	internal enum ZlibStreamFlavor
	{
		ZLIB = 1950,
		DEFLATE = 1951,
		GZIP = 1952
	}

	internal class ZlibBaseStream : Stream
	{
		protected internal ZlibCodec Codec = null;

		protected internal StreamMode _streamMode = StreamMode.Undefined;
		protected internal FlushType _flushMode;
		protected internal ZlibStreamFlavor _flavor;
		protected internal CompressionMode _compressionMode;
		protected internal CompressionLevel _level;
		protected internal bool _leaveOpen;
		protected internal byte[] _workingBuffer;
		protected internal int _bufferSize = ZlibConstants.WorkingBufferSizeDefault;
		protected internal byte[] _buf1 = new byte[1];

		protected internal Stream _stream;
		protected internal CompressionStrategy Strategy = CompressionStrategy.Default;

		private readonly CRC32 crc;
		protected internal string _GzipFileName;
		protected internal string _GzipComment;
		protected internal DateTime _GzipMtime;
		protected internal int _gzipHeaderByteCount;

		internal int Crc32
		{
			get
			{
				if( crc == null )
				{
					return 0;
				}
				return crc.Crc32Result;
			}
		}

		public ZlibBaseStream( Stream stream, CompressionMode compressionMode, CompressionLevel level, ZlibStreamFlavor flavor, bool leaveOpen )
		{
			_flushMode = FlushType.None;
			//this._workingBuffer = new byte[WORKING_BUFFER_SIZE_DEFAULT];
			_stream = stream;
			_leaveOpen = leaveOpen;
			_compressionMode = compressionMode;
			_flavor = flavor;
			_level = level;
			if( flavor == ZlibStreamFlavor.GZIP )
			{
				crc = new CRC32();
			}
		}

		protected internal bool _wantCompress
		{
			get
			{
				return ( _compressionMode == CompressionMode.Compress );
			}
		}

		private ZlibCodec z
		{
			get
			{
				if( Codec == null )
				{
					bool wantRfc1950Header = ( _flavor == ZlibStreamFlavor.ZLIB );
					Codec = new ZlibCodec();
					if( _compressionMode == CompressionMode.Decompress )
					{
						Codec.InitializeInflate( wantRfc1950Header );
					}
					else
					{
						Codec.Strategy = Strategy;
						Codec.InitializeDeflate( _level, wantRfc1950Header );
					}
				}
				return Codec;
			}
		}

		private byte[] workingBuffer
		{
			get
			{
				if( _workingBuffer == null )
				{
					_workingBuffer = new byte[_bufferSize];
				}
				return _workingBuffer;
			}
		}

		public override void Write( Byte[] buffer, int offset, int count )
		{
			// calculate the CRC on the unccompressed data  (before writing)
			if( crc != null )
			{
				crc.ChecksumBlock( buffer, offset, count );
			}

			if( _streamMode == StreamMode.Undefined )
			{
				_streamMode = StreamMode.Writer;
			}
			else if( _streamMode != StreamMode.Writer )
			{
				throw new ZlibException( "Cannot Write after Reading." );
			}

			if( count == 0 )
			{
				return;
			}

			// first reference of z property will initialize the private var _z
			z.InputBuffer = buffer;
			Codec.NextIn = offset;
			Codec.AvailableBytesIn = count;
			bool done = false;
			do
			{
				Codec.OutputBuffer = workingBuffer;
				Codec.NextOut = 0;
				Codec.AvailableBytesOut = _workingBuffer.Length;
				int rc = ( _wantCompress ) ? Codec.Deflate( _flushMode ) : Codec.Inflate( _flushMode );
				if( rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END )
				{
					throw new ZlibException( ( _wantCompress ? "de" : "in" ) + "flating: " + Codec.Message );
				}

				//if (_workingBuffer.Length - _z.AvailableBytesOut > 0)
				_stream.Write( _workingBuffer, 0, _workingBuffer.Length - Codec.AvailableBytesOut );

				done = Codec.AvailableBytesIn == 0 && Codec.AvailableBytesOut != 0;

				// If GZIP and de-compress, we're done when 8 bytes remain.
				if( _flavor == ZlibStreamFlavor.GZIP && !_wantCompress )
				{
					done = ( Codec.AvailableBytesIn == 8 && Codec.AvailableBytesOut != 0 );
				}
			}
			while( !done );
		}

		private void finish()
		{
			if( Codec == null )
			{
				return;
			}

			if( _streamMode == StreamMode.Writer )
			{
				bool done = false;
				do
				{
					Codec.OutputBuffer = workingBuffer;
					Codec.NextOut = 0;
					Codec.AvailableBytesOut = _workingBuffer.Length;
					int rc = ( _wantCompress ) ? Codec.Deflate( FlushType.Finish ) : Codec.Inflate( FlushType.Finish );

					if( rc != ZlibConstants.Z_STREAM_END && rc != ZlibConstants.Z_OK )
					{
						string verb = ( _wantCompress ? "de" : "in" ) + "flating";
						if( Codec.Message == null )
						{
							throw new ZlibException( String.Format( "{0}: (rc = {1})", verb, rc ) );
						}
						throw new ZlibException( verb + ": " + Codec.Message );
					}

					if( _workingBuffer.Length - Codec.AvailableBytesOut > 0 )
					{
						_stream.Write( _workingBuffer, 0, _workingBuffer.Length - Codec.AvailableBytesOut );
					}

					done = Codec.AvailableBytesIn == 0 && Codec.AvailableBytesOut != 0;
					// If GZIP and de-compress, we're done when 8 bytes remain.
					if( _flavor == ZlibStreamFlavor.GZIP && !_wantCompress )
					{
						done = ( Codec.AvailableBytesIn == 8 && Codec.AvailableBytesOut != 0 );
					}
				}
				while( !done );

				Flush();

				if( _flavor == ZlibStreamFlavor.GZIP )
				{
					if( _wantCompress )
					{
						// Emit the GZIP trailer: CRC32 and  size mod 2^32
						int c1 = crc.Crc32Result;
						_stream.Write( BitConverter.GetBytes( c1 ), 0, 4 );
						int c2 = ( Int32 )( crc.TotalBytesRead & 0x00000000FFFFFFFF );
						_stream.Write( BitConverter.GetBytes( c2 ), 0, 4 );
					}
					else
					{
						throw new ZlibException( "Writing with decompression is not supported." );
					}
				}
			}
			else if( _streamMode == StreamMode.Reader )
			{
				if( _flavor == ZlibStreamFlavor.GZIP )
				{
					if( !_wantCompress )
					{
						// workitem 8501: handle edge case (decompress empty stream)
						if( Codec.TotalBytesOut == 0L )
						{
							return;
						}

						// Read and potentially verify the GZIP trailer:
						// CRC32 and size mod 2^32
						byte[] trailer = new byte[8];

						// workitems 8679 & 12554
						if( Codec.AvailableBytesIn < 8 )
						{
							// Make sure we have read to the end of the stream
							Array.Copy( Codec.InputBuffer, Codec.NextIn, trailer, 0, Codec.AvailableBytesIn );
							int bytesNeeded = 8 - Codec.AvailableBytesIn;
							int bytesRead = _stream.Read( trailer, Codec.AvailableBytesIn, bytesNeeded );
							if( bytesNeeded != bytesRead )
							{
								throw new ZlibException( String.Format( "Missing or incomplete GZIP trailer. Expected 8 bytes, got {0}.", Codec.AvailableBytesIn + bytesRead ) );
							}
						}
						else
						{
							Array.Copy( Codec.InputBuffer, Codec.NextIn, trailer, 0, trailer.Length );
						}

						Int32 crc32_expected = BitConverter.ToInt32( trailer, 0 );
						Int32 crc32_actual = crc.Crc32Result;
						Int32 isize_expected = BitConverter.ToInt32( trailer, 4 );
						Int32 isize_actual = ( Int32 )( Codec.TotalBytesOut & 0x00000000FFFFFFFF );

						if( crc32_actual != crc32_expected )
						{
							throw new ZlibException( String.Format( "Bad CRC32 in GZIP trailer. (actual({0:X8})!=expected({1:X8}))", crc32_actual, crc32_expected ) );
						}

						if( isize_actual != isize_expected )
						{
							throw new ZlibException( String.Format( "Bad size in GZIP trailer. (actual({0})!=expected({1}))", isize_actual, isize_expected ) );
						}
					}
					else
					{
						throw new ZlibException( "Reading with compression is not supported." );
					}
				}
			}
		}

		private void end()
		{
			if( z == null )
			{
				return;
			}
			if( _wantCompress )
			{
				Codec.EndDeflate();
			}
			else
			{
				Codec.EndInflate();
			}
			Codec = null;
		}

		public override void Close()
		{
			if( _stream == null )
			{
				return;
			}
			try
			{
				finish();
			}
			finally
			{
				end();
				if( !_leaveOpen )
				{
					_stream.Close();
				}
				_stream = null;
			}
		}

		public override void Flush()
		{
			_stream.Flush();
		}

		public override Int64 Seek( Int64 offset, SeekOrigin origin )
		{
			throw new NotImplementedException();
			//_outStream.Seek(offset, origin);
		}

		public override void SetLength( Int64 value )
		{
			_stream.SetLength( value );
		}

		private bool nomoreinput;

		private string ReadZeroTerminatedString()
		{
			List<byte> list = new List<byte>();
			bool done = false;
			do
			{
				int n = _stream.Read( _buf1, 0, 1 );
				if( n != 1 )
				{
					throw new ZlibException( "Unexpected EOF reading GZIP header." );
				}
				if( _buf1[0] == 0 )
				{
					done = true;
				}
				else
				{
					list.Add( _buf1[0] );
				}
			}
			while( !done );
			byte[] a = list.ToArray();
			return GZipStream.iso8859dash1.GetString( a, 0, a.Length );
		}

		private int _ReadAndValidateGzipHeader()
		{
			int totalBytesRead = 0;
			// read the header on the first read
			byte[] header = new byte[10];
			int n = _stream.Read( header, 0, header.Length );

			// workitem 8501: handle edge case (decompress empty stream)
			if( n == 0 )
			{
				return 0;
			}

			if( n != 10 )
			{
				throw new ZlibException( "Not a valid GZIP stream." );
			}

			if( header[0] != 0x1F || header[1] != 0x8B || header[2] != 8 )
			{
				throw new ZlibException( "Bad GZIP header." );
			}

			Int32 timet = BitConverter.ToInt32( header, 4 );
			_GzipMtime = GZipStream._unixEpoch.AddSeconds( timet );
			totalBytesRead += n;
			if( ( header[3] & 0x04 ) == 0x04 )
			{
				// read and discard extra field
				n = _stream.Read( header, 0, 2 ); // 2-byte length field
				totalBytesRead += n;

				Int16 extraLength = ( Int16 )( header[0] + header[1] * 256 );
				byte[] extra = new byte[extraLength];
				n = _stream.Read( extra, 0, extra.Length );
				if( n != extraLength )
				{
					throw new ZlibException( "Unexpected end-of-file reading GZIP header." );
				}
				totalBytesRead += n;
			}
			if( ( header[3] & 0x08 ) == 0x08 )
			{
				_GzipFileName = ReadZeroTerminatedString();
			}
			if( ( header[3] & 0x10 ) == 0x010 )
			{
				_GzipComment = ReadZeroTerminatedString();
			}
			if( ( header[3] & 0x02 ) == 0x02 )
			{
				Read( _buf1, 0, 1 ); // CRC16, ignore
			}

			return totalBytesRead;
		}

		public override Int32 Read( Byte[] buffer, Int32 offset, Int32 count )
		{
			// According to MS documentation, any implementation of the IO.Stream.Read function must:
			// (a) throw an exception if offset & count reference an invalid part of the buffer,
			//     or if count < 0, or if buffer is null
			// (b) return 0 only upon EOF, or if count = 0
			// (c) if not EOF, then return at least 1 byte, up to <count> bytes

			if( _streamMode == StreamMode.Undefined )
			{
				if( !_stream.CanRead )
				{
					throw new ZlibException( "The stream is not readable." );
				}
				// for the first read, set up some controls.
				_streamMode = StreamMode.Reader;
				// (The first reference to _z goes through the private accessor which
				// may initialize it.)
				z.AvailableBytesIn = 0;
				if( _flavor == ZlibStreamFlavor.GZIP )
				{
					_gzipHeaderByteCount = _ReadAndValidateGzipHeader();
					// workitem 8501: handle edge case (decompress empty stream)
					if( _gzipHeaderByteCount == 0 )
					{
						return 0;
					}
				}
			}

			if( _streamMode != StreamMode.Reader )
			{
				throw new ZlibException( "Cannot Read after Writing." );
			}

			if( count == 0 )
			{
				return 0;
			}
			if( nomoreinput && _wantCompress )
			{
				return 0; // workitem 8557
			}
			if( buffer == null )
			{
				throw new ArgumentNullException( "buffer" );
			}
			if( count < 0 )
			{
				throw new ArgumentOutOfRangeException( "count" );
			}
			if( offset < buffer.GetLowerBound( 0 ) )
			{
				throw new ArgumentOutOfRangeException( "offset" );
			}
			if( ( offset + count ) > buffer.GetLength( 0 ) )
			{
				throw new ArgumentOutOfRangeException( "count" );
			}

			int rc = 0;

			// set up the output of the deflate/inflate codec:
			Codec.OutputBuffer = buffer;
			Codec.NextOut = offset;
			Codec.AvailableBytesOut = count;

			// This is necessary in case _workingBuffer has been resized. (new byte[])
			// (The first reference to _workingBuffer goes through the private accessor which
			// may initialize it.)
			Codec.InputBuffer = workingBuffer;

			do
			{
				// need data in _workingBuffer in order to deflate/inflate.  Here, we check if we have any.
				if( ( Codec.AvailableBytesIn == 0 ) && ( !nomoreinput ) )
				{
					// No data available, so try to Read data from the captive stream.
					Codec.NextIn = 0;
					Codec.AvailableBytesIn = _stream.Read( _workingBuffer, 0, _workingBuffer.Length );
					if( Codec.AvailableBytesIn == 0 )
					{
						nomoreinput = true;
					}
				}
				// we have data in InputBuffer; now compress or decompress as appropriate
				rc = ( _wantCompress ) ? Codec.Deflate( _flushMode ) : Codec.Inflate( _flushMode );

				if( nomoreinput && ( rc == ZlibConstants.Z_BUF_ERROR ) )
				{
					return 0;
				}

				if( rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END )
				{
					throw new ZlibException( String.Format( "{0}flating:  rc={1}  msg={2}", ( _wantCompress ? "de" : "in" ), rc, Codec.Message ) );
				}

				if( ( nomoreinput || rc == ZlibConstants.Z_STREAM_END ) && ( Codec.AvailableBytesOut == count ) )
				{
					break; // nothing more to read
				}
			}
				//while (_z.AvailableBytesOut == count && rc == ZlibConstants.Z_OK);
			while( Codec.AvailableBytesOut > 0 && !nomoreinput && rc == ZlibConstants.Z_OK );

			// is there more room in output?
			if( Codec.AvailableBytesOut > 0 )
			{
				if( rc == ZlibConstants.Z_OK && Codec.AvailableBytesIn == 0 )
				{
					// deferred
				}

				// are we completely done reading?
				if( nomoreinput )
				{
					// and in compression?
					if( _wantCompress )
					{
						// no more input data available; therefore we flush to
						// try to complete the read
						rc = Codec.Deflate( FlushType.Finish );

						if( rc != ZlibConstants.Z_OK && rc != ZlibConstants.Z_STREAM_END )
						{
							throw new ZlibException( String.Format( "Deflating:  rc={0}  msg={1}", rc, Codec.Message ) );
						}
					}
				}
			}

			rc = ( count - Codec.AvailableBytesOut );

			// calculate CRC after reading
			if( crc != null )
			{
				crc.ChecksumBlock( buffer, offset, rc );
			}

			return rc;
		}

		public override Boolean CanRead
		{
			get
			{
				return _stream.CanRead;
			}
		}

		public override Boolean CanSeek
		{
			get
			{
				return _stream.CanSeek;
			}
		}

		public override Boolean CanWrite
		{
			get
			{
				return _stream.CanWrite;
			}
		}

		public override Int64 Length
		{
			get
			{
				return _stream.Length;
			}
		}

		public override long Position
		{
			get
			{
				throw new NotImplementedException();
			}
			set
			{
				throw new NotImplementedException();
			}
		}

		internal enum StreamMode
		{
			Writer,
			Reader,
			Undefined,
		}

		public static void CompressString( string s, Stream compressor )
		{
			byte[] uncompressed = Encoding.UTF8.GetBytes( s );
			using( compressor )
			{
				compressor.Write( uncompressed, 0, uncompressed.Length );
			}
		}

		public static void CompressBuffer( byte[] b, Stream compressor )
		{
			using( compressor )
			{
				compressor.Write( b, 0, b.Length );
			}
		}

		public static string UncompressString( byte[] compressed, Stream decompressor )
		{
			byte[] working = new byte[1024];
			Encoding encoding = Encoding.UTF8;
			using( MemoryStream output = new MemoryStream() )
			{
				using( decompressor )
				{
					int n;
					while( ( n = decompressor.Read( working, 0, working.Length ) ) != 0 )
					{
						output.Write( working, 0, n );
					}
				}

				// reset to allow read from start
				output.Seek( 0, SeekOrigin.Begin );
				StreamReader sr = new StreamReader( output, encoding );
				return sr.ReadToEnd();
			}
		}

		public static byte[] UncompressBuffer( byte[] compressed, Stream decompressor )
		{
			byte[] working = new byte[1024];
			using( MemoryStream output = new MemoryStream() )
			{
				using( decompressor )
				{
					int n;
					while( ( n = decompressor.Read( working, 0, working.Length ) ) != 0 )
					{
						output.Write( working, 0, n );
					}
				}
				return output.ToArray();
			}
		}
	}
}
