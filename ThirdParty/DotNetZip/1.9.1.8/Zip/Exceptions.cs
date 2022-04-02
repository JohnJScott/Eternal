// Exceptions.cs
// ------------------------------------------------------------------
//
// Copyright (c) 2008, 2009 Dino Chiesa and Microsoft Corporation.
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
// Time-stamp: <2011-July-12 12:19:10>
//
// ------------------------------------------------------------------
//
// This module defines exceptions used in the class library.
//

using System;
using System.Runtime.Serialization;

namespace Ionic.Zip
{
	///// <summary>
	///// Base exception type for all custom exceptions in the Zip library. It acts as a marker class.
	///// </summary>
	//[AttributeUsage(AttributeTargets.Class)]
	//public class ZipExceptionAttribute : Attribute { }

	/// <summary>
	///     Issued when an <c>ZipEntry.ExtractWithPassword()</c> method is invoked
	///     with an incorrect password.
	/// </summary>
	[Serializable]
	public class BadPasswordException : ZipException
	{
		/// <summary>
		///     Default ctor.
		/// </summary>
		public BadPasswordException()
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		public BadPasswordException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		/// <param name="innerException">The innerException for this exception.</param>
		public BadPasswordException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="info">The serialization info for the exception.</param>
		/// <param name="context">The streaming context from which to deserialize.</param>
		protected BadPasswordException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}

	/// <summary>
	///     Indicates that a read was attempted on a stream, and bad or incomplete data was
	///     received.
	/// </summary>
	[Serializable]
	public class BadReadException : ZipException
	{
		/// <summary>
		///     Default ctor.
		/// </summary>
		public BadReadException()
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		public BadReadException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		/// <param name="innerException">The innerException for this exception.</param>
		public BadReadException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="info">The serialization info for the exception.</param>
		/// <param name="context">The streaming context from which to deserialize.</param>
		protected BadReadException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}

	/// <summary>
	///     Issued when an CRC check fails upon extracting an entry from a zip archive.
	/// </summary>
	[Serializable]
	public class BadCrcException : ZipException
	{
		/// <summary>
		///     Default ctor.
		/// </summary>
		public BadCrcException()
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		public BadCrcException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="info">The serialization info for the exception.</param>
		/// <param name="context">The streaming context from which to deserialize.</param>
		protected BadCrcException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}

	/// <summary>
	///     Issued when errors occur saving a self-extracting archive.
	/// </summary>
	[Serializable]
	public class SfxGenerationException : ZipException
	{
		/// <summary>
		///     Default ctor.
		/// </summary>
		public SfxGenerationException()
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		public SfxGenerationException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="info">The serialization info for the exception.</param>
		/// <param name="context">The streaming context from which to deserialize.</param>
		protected SfxGenerationException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}

	/// <summary>
	///     Indicates that an operation was attempted on a ZipFile which was not possible
	///     given the state of the instance. For example, if you call <c>Save()</c> on a ZipFile
	///     which has no filename set, you can get this exception.
	/// </summary>
	[Serializable]
	public class BadStateException : ZipException
	{
		/// <summary>
		///     Default ctor.
		/// </summary>
		public BadStateException()
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		public BadStateException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		/// <param name="innerException">The innerException for this exception.</param>
		public BadStateException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="info">The serialization info for the exception.</param>
		/// <param name="context">The streaming context from which to deserialize.</param>
		protected BadStateException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}

	/// <summary>
	///     Base class for all exceptions defined by and throw by the Zip library.
	/// </summary>
	[Serializable]
	public class ZipException : Exception
	{
		/// <summary>
		///     Default ctor.
		/// </summary>
		public ZipException()
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		public ZipException( string message )
			: base( message )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="message">The message in the exception.</param>
		/// <param name="innerException">The innerException for this exception.</param>
		public ZipException( string message, Exception innerException )
			: base( message, innerException )
		{
		}

		/// <summary>
		///     Come on, you know how exceptions work. Why are you looking at this documentation?
		/// </summary>
		/// <param name="info">The serialization info for the exception.</param>
		/// <param name="context">The streaming context from which to deserialize.</param>
		protected ZipException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
		}
	}
}
