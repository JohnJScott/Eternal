// OffsetStream.cs
// ------------------------------------------------------------------
//
// Copyright (c)  2009 Dino Chiesa
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
// Time-stamp: <2009-August-27 12:50:35>
//
// ------------------------------------------------------------------
//
// This module defines logic for handling reading of zip archives embedded 
// into larger streams.  The initial position of the stream serves as
// the base offset for all future Seek() operations.
// 
// ------------------------------------------------------------------

using System;
using System.IO;

namespace Ionic.Zip
{
	internal class OffsetStream 
		: Stream
		, IDisposable
	{
		private readonly Stream InnerSubStream;
		private readonly Int64 OriginalPosition;

		public OffsetStream( Stream SubStream )
		{
			OriginalPosition = SubStream.Position;
			InnerSubStream = SubStream;
		}

		public override bool CanRead
		{
			get
			{
				return InnerSubStream.CanRead;
			}
		}

		public override bool CanSeek
		{
			get
			{
				return InnerSubStream.CanSeek;
			}
		}

		public override bool CanWrite
		{
			get
			{
				return false;
			}
		}

		public override long Length
		{
			get
			{
				return InnerSubStream.Length;
			}
		}

		public override long Position
		{
			get
			{
				return InnerSubStream.Position - OriginalPosition;
			}
			set
			{
				InnerSubStream.Position = OriginalPosition + value;
			}
		}

		void IDisposable.Dispose()
		{
			Close();
		}

		public override int Read( byte[] buffer, int offset, int count )
		{
			return InnerSubStream.Read( buffer, offset, count );
		}

		public override void Write( byte[] buffer, int offset, int count )
		{
			throw new NotImplementedException();
		}

		public override void Flush()
		{
			InnerSubStream.Flush();
		}

		public override long Seek( long offset, SeekOrigin origin )
		{
			return InnerSubStream.Seek( OriginalPosition + offset, origin ) - OriginalPosition;
		}

		public override void SetLength( long value )
		{
			throw new NotImplementedException();
		}
	}
}
