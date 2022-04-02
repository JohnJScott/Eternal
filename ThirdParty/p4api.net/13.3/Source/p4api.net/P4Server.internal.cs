/*******************************************************************************

Copyright (c) 2010, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
	notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
	notice, this list of conditions and the following disclaimer in the
	documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: P4Server.internal.cs
 *
 * Author	: dbb
 *
 * Description	: Classes used to wrap calls in the P4Bridge DLL in C#.
 *
 ******************************************************************************/
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.IO;
using System.Threading;

namespace Perforce.P4
{
	/// <summary>
	/// P4Server
	/// 
	/// Represents the connection to a Perforce Server using the the P4 Bridge 
	/// DLL. It wraps the calls exported by the DLL and transforms the data
	/// types exported by the DLL as native C#.NET data types.
	/// </summary>
	/// <remarks>
	/// This file contains the internal data a methods that are not part of the
	/// public interface
	/// </remarks>
	partial class P4Server
	{
		/// <summary>
		/// P4Encoding
		/// 
		/// How do we encode Unicode UTF-16 strings, the .NET internal format
		/// before sending them to the bridge dll?
		/// </summary>
		internal enum P4Encoding : int
		{
			ASCII = 0, // required for non Unicode 
			//     enabled servers
			utf8 = 1, // P4's 'native' encoding

			utf16 = 2, // Windows preferred encoding 
			//     for Unicode files
			utf16bom = 3
		};

		String[] Charset = { String.Empty, "utf8", "utf16le", "utf16le-bom" };

		// default encoding to ASCII
		private P4Encoding CurrentEncodeing = P4Encoding.ASCII;

		/// <summary>
		/// Is the server Unicode enabled
		/// </summary>
		private bool isUnicode;

		/// <summary>
		/// What API level does the server support
		/// </summary>
		private int apiLevel;

		/// <summary>
		/// Does the server require the login command be used
		/// </summary>
		private bool requiresLogin;

		/// <summary>
		/// Marshall a returned pointer as a UTF-16 encoded string
		/// </summary>
		/// <param name="pStr"> Native pointer to the string</param>
		/// <returns>UTF-16 String</returns>
		internal String MarshalPtrToStringUtf16(IntPtr pStr)
		{
			if (pStr == IntPtr.Zero)
				return null;

			if (UseUnicode)
				return Marshal.PtrToStringUni(pStr);
			else
				return Marshal.PtrToStringAnsi(pStr);
		}

		/// <summary>
		/// Marshall a returned pointer as a UTF-8 encoded string
		/// </summary>
		/// <param name="pStr"> Native pointer to the string</param>
		/// <returns>UTF-16 String</returns>
		internal String MarshalPtrToStringUtf8(IntPtr pStr)
		{
			if (pStr == IntPtr.Zero)
				return null;

			if (UseUnicode)
			{
				// there is no Marshall utility to directly translate
				// the strings encoding, so we need to copy it into a
				// byte[] and use the Encoding.UTF8 class to translate.
				Encoding utf8 = Encoding.UTF8;
				int cnt = 0;
				// count the characters till a null
				while (Marshal.ReadByte(pStr, cnt) != 0)
					cnt++;
				byte[] StrBytes = new byte[cnt]; //+ 1 ];
				for (int idx = 0; idx < cnt; idx++)
					StrBytes[idx] = Marshal.ReadByte(pStr, idx);
				//StrBytes[ cnt ] = 0; // null terminate
				return utf8.GetString(StrBytes);
			}
			else
				return Marshal.PtrToStringAnsi(pStr);
		}

		/// <summary>
		/// Translate a returned string based on the current encoding
		/// </summary>
		/// <param name="pStr"> Native pointer to the string</param>
		/// <returns>UTF-16 String</returns>
		internal String MarshalPtrToString(IntPtr pStr)
		{
			switch (CurrentEncodeing)
			{
				case P4Encoding.utf8:
					return MarshalPtrToStringUtf8(pStr);
				case P4Encoding.utf16:
					return MarshalPtrToStringUtf16(pStr);
				default:
				case P4Encoding.ASCII:
					return Marshal.PtrToStringAnsi(pStr);
			}
		}

		/// <summary>
		/// Convert a returned c++ byte pointer (void *) to a byte[]
		/// </summary>
		/// <param name="pData">byte pointer</param>
		/// <param name="byteCnt">byte count</param>
		/// <returns>Converted byte[]</returns>
		internal byte[] MarshalPtrToByteArrary(IntPtr pData, int byteCnt)
		{
			// allocate a new byte[] to hold the data
			byte[] data = new byte[byteCnt];

			// copy each byte
			for (int idx = 0; idx < byteCnt; idx++)
				data[idx] = Marshal.ReadByte(pData, idx);

			return data;
		}

		/// <summary>
		/// Marshal a String[] into an array of native pointers to strings
		/// in the correct encoding for Unicode enabled servers
		/// </summary>
		/// <param name="args">The args to encode</param>
		/// <param name="argc">Count of args to encode</param>
		/// <returns>Array of IntPtrs to encoded strings</returns>
		internal PinnedByteArrays
			MarshalStringArrayToIntPtrArray(String[] args,
											  int argc)
		{
			Encoding encode = Encoding.Unicode;
			switch (CurrentEncodeing)
			{
				case P4Encoding.utf8:
					encode = Encoding.UTF8;
					break;
				case P4Encoding.utf16:
					encode = Encoding.Unicode;
					break;
			}

			PinnedByteArray[] args_b = new PinnedByteArray[argc];

			for (int i = 0; i < argc; i++)
			{
				//null terminate the strings
				byte[] arg_b = encode.GetBytes(args[i] + '\0');
				args_b[i] = new PinnedByteArray(arg_b);
			}
			return new PinnedByteArrays(args_b);
		}

		//        static GCHandle nullGCHandle = new GCHandle();

		/// <summary>
		/// Marshal a String into the correct encoding to pass to a Unicode
		/// enabled server.
		/// </summary>
		/// <param name="arg">String to encode</param>
		/// <returns>IntPtr of the encoded string</returns>
		internal PinnedByteArray MarshalStringToIntPtr(String arg)
		{
			if (arg == null)
			{
				return null;
			}
			Encoding encode = Encoding.UTF8;
			switch (CurrentEncodeing)
			{
				case P4Encoding.utf8:
					encode = Encoding.UTF8;
					break;
				case P4Encoding.utf16:
					encode = Encoding.Unicode;
					break;
			}
			//null terminate the string
			byte[] bytes = encode.GetBytes(arg + '\0');

			return new PinnedByteArray(bytes);
		}

		internal void  CopyStringToIntPtr(String src, IntPtr dst, int maxBytes )
		{
			Encoding encode = Encoding.UTF8;
			switch (CurrentEncodeing)
			{
				case P4Encoding.utf8:
					encode = Encoding.UTF8;
					break;
				case P4Encoding.utf16:
					encode = Encoding.Unicode;
					break;
			}
			//null terminate the string
			byte[] bytes = encode.GetBytes(src + '\0');

			int max = bytes.Length;
			if (max > maxBytes)
			{
				max = maxBytes;
				// make sure the string ends with a null
				bytes[maxBytes - 1] = 0; 
			}
			Marshal.Copy(bytes, 0, dst, max);
		}

		/// <summary>
		/// Handle (pointer) to the P4BridgeServer wrapped by this P4Server
		/// </summary>
		internal IntPtr pServer = IntPtr.Zero;

		/// <summary>
		/// Internal creator for unit testing other classes
		/// </summary>
		/// <param name="unicode"></param>
		/// 
		internal P4Server(bool unicode)
		{
			isUnicode = unicode;
			if (unicode)
			{
				CurrentEncodeing = P4Encoding.utf8;
			}
		}

		/// <summary>
		/// Set the character set encoding used to pass parameters to a Unicode
		/// enabled server.
		/// </summary>
		/// <remarks>
		/// This is handled automatically after connecting with a P4 server.
		/// </remarks>
		/// <param name="charSet"></param>
		/// <param name="FileCharSet"></param>
		internal void SetCharacterSet(String charSet, String FileCharSet)
		{
			IntPtr pErrorStr = P4Bridge.SetCharacterSet(pServer, charSet, FileCharSet);

			if (pErrorStr == IntPtr.Zero)  // no error
				return;

			String ErrorMsg = MarshalPtrToString(pErrorStr);
			throw new P4Exception(ErrorSeverity.E_FAILED, ErrorMsg);
		}

		/// <summary>
		/// Holds the call back passed to the bridge used to receive the 
		/// individual key:value pairs for an object 
		/// </summary>
		internal P4CallBacks.TaggedOutputDelegate
			TaggedOutputCallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr pTaggedOutputCallbackFn = IntPtr.Zero;

		/// <summary>
		/// Used to build up an object as its key:value pairs are received
		/// </summary>
		internal TaggedObject CurrentObject = null;

		//We'll receive multiple calls for each StrDict object, one for each 
		//  key:value pair that will get combined into a single TaggedObject.

		/// <summary>
		/// Delegate used to send real time tagged results generated by a 
		/// command
		/// </summary>
		/// <remarks>
		/// We receive multiple calls for each StrDict object, one for each 
		/// key:value pair that will get combined into a single TaggedObject used
		/// to represent an 'object'. This client delegate receives a single
		/// TaggedObject representing the complete object.
		/// </remarks>
		internal void SetTaggedOutputCallback()
		{
			if (TaggedOutputCallbackFn_Int == null)
			{
				// initialize on first setting
				TaggedOutputCallbackFn_Int =
					new P4CallBacks.TaggedOutputDelegate(TaggedOutputCallback_Int);
				pTaggedOutputCallbackFn = Marshal.GetFunctionPointerForDelegate(TaggedOutputCallbackFn_Int);
				P4Bridge.SetTaggedOutputCallbackFn(pServer, pTaggedOutputCallbackFn);
			}
		}

		object TaggedOutputCallback_Int_Sync = new object();

		/// <summary>
		/// Internal callback used to receive the individual Key:Value pair 
		/// data for an object
		/// </summary>
		/// We receive multiple calls for each StrDict object, one for each 
		/// key:value pair that will get combined into a single TaggedObject used
		/// to represent an 'object'. Object IDs are unique for the objects
		/// returned by a single command, but are not unique across commands.
		/// <remarks>
		/// </remarks>
		/// <param name="objID">Object ID assigned by the bridge</param>
		/// <param name="Key">Key for this entry</param>
		/// <param name="pValue">Value for this entry</param>
		internal void TaggedOutputCallback_Int( uint cmdId,
												int objID,
												String Key,
												IntPtr pValue)
		{
			lock (TaggedOutputCallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					// no callback set, so ignore
					if (TaggedOutputReceived == null)
						return;

					if ((!String.IsNullOrEmpty(Key)) && (pValue != IntPtr.Zero))
					{
						if (CurrentObject == null)
							CurrentObject = new TaggedObject();

						CurrentObject[Key] = MarshalPtrToString(pValue);
					}
					else
					{
						Delegate[] targetList = TaggedOutputReceived.GetInvocationList();
						foreach (TaggedOutputDelegate d in targetList)
						{
							try
							{
								d(cmdId, objID, CurrentObject);
							}
							catch
							{
								// problem with delegate, so remove from the list
								TaggedOutputReceived -= d;
							}
						}
						//get ready for the next object
						CurrentObject = null;
					}
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}
		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr pErrorCallbackFn = IntPtr.Zero;

		object ErrorCallback_Int_Sync = new object();

		/// <summary>
		/// Internal callback used to receive the raw data. 
		/// </summary>
		/// The text data pointed to by a char* is marshaled into a String
		/// <remarks>
		/// </remarks>
		/// <param name="severity">Severity level</param>
		/// <param name="pData">char* pointer for error message</param>
		internal void ErrorCallback_Int(uint cmdId, int severity, int errorNumber, IntPtr pData)
		{
			lock (ErrorCallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					// no callback set, so ignore
					if (ErrorReceived == null)
						return;

					String data = null;
					if (pData != IntPtr.Zero)
					{
						data = MarshalPtrToString(pData);
					}

					Delegate[] targetList = ErrorReceived.GetInvocationList();
					foreach (ErrorDelegate d in targetList)
					{
						try
						{
							d(cmdId, severity, errorNumber, data);
						}
						catch
						{
							// problem with delegate, so remove from the list
							ErrorReceived -= d;
						}
					}
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		/// <summary>
		/// Set the callback used to return real time Errors.
		/// </summary>
		internal void SetErrorCallback()
		{
			if (ErrorCallbackFn_Int == null)
			{
				// initialize on first setting
				ErrorCallbackFn_Int =
					new P4CallBacks.ErrorDelegate(ErrorCallback_Int);
				pErrorCallbackFn = Marshal.GetFunctionPointerForDelegate(ErrorCallbackFn_Int);
				P4Bridge.SetErrorCallbackFn(pServer, pErrorCallbackFn);
			}
		}

		/// <summary>
		/// Holds the call back passed to the bridge used to receive the 
		/// raw  data 
		/// </summary>
		P4CallBacks.InfoResultsDelegate
			InfoResultsCallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr pInfoResultsCallbackFn = IntPtr.Zero;

		object InfoResultsCallback_Sync = new object();

		/// <summary>
		/// Internal callback used to receive the raw data. 
		/// </summary>
		/// The text data pointed to by a char* is marshaled into a String
		/// <remarks>
		/// </remarks>
		/// <param name="level">message level</param>
		/// <param name="pData">char* pointer to message</param>
		internal void InfoResultsCallback_Int(uint cmdId, int level, IntPtr pData)
		{
			lock (InfoResultsCallback_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					// no callback set, so ignore
					if (InfoResultsReceived == null)
						return;

					String data = null;
					if (pData != IntPtr.Zero)
					{
						data = MarshalPtrToString(pData);
					}

					Delegate[] targetList = InfoResultsReceived.GetInvocationList();
					foreach (InfoResultsDelegate d in targetList)
					{
						try
						{
							d(cmdId, level, data);
						}
						catch
						{
							// problem with delegate, so remove from the list
							InfoResultsReceived -= d;
						}
					}
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		/// <summary>
		/// Set the callback used to return real time info output.
		/// </summary>
		internal void SetInfoResultsCallback() //InfoResultsDelegate cb )
		{
			if (InfoResultsCallbackFn_Int == null)
			{
				// initialize on first setting
				InfoResultsCallbackFn_Int =
					new P4CallBacks.InfoResultsDelegate(InfoResultsCallback_Int);
				pInfoResultsCallbackFn = Marshal.GetFunctionPointerForDelegate(InfoResultsCallbackFn_Int);
				P4Bridge.SetInfoResultsCallbackFn(pServer, pInfoResultsCallbackFn);
			}
		}

		/// <summary>
		/// Holds the call back passed to the bridge used to receive the 
		/// raw binary data 
		/// </summary>
		P4CallBacks.TextResultsDelegate
			TextResultsCallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr pTextResultsCallbackFn = IntPtr.Zero;

		object TextResultsCallback_Int_Sync = new object();

		/// <summary>
		/// Internal callback used to receive the raw text data. 
		/// </summary>
		/// The text data pointed to by a char* is marshaled into a String
		/// <remarks>
		/// </remarks>
		/// <param name="pData">char* pointer</param>
		internal void TextResultsCallback_Int(uint cmdId, IntPtr pData)
		{
			lock (TextResultsCallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					// no callback set, so ignore
					if (TextResultsReceived == null)
						return;

					String data = null;
					if (pData != IntPtr.Zero)
					{
						data = MarshalPtrToString(pData);
					}

					Delegate[] targetList = TextResultsReceived.GetInvocationList();
					foreach (TextResultsDelegate d in targetList)
					{
						try
						{
							d(cmdId, data);
						}
						catch
						{
							// problem with delegate, so remove from the list
							TextResultsReceived -= d;
						}
					}
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		/// <summary>
		/// Set the callback used to return real time text output.
		/// </summary>
		/// <remarks>
		/// Far large output, the client may receive multiple callbacks.
		/// Simply concatenate the data to get the complete data.
		/// </remarks>
		internal void SetTextResultsCallback()
		{
			if (TextResultsCallbackFn_Int == null)
			{
				// initialize on first setting
				TextResultsCallbackFn_Int =
					new P4CallBacks.TextResultsDelegate(TextResultsCallback_Int);
				pTextResultsCallbackFn = Marshal.GetFunctionPointerForDelegate(TextResultsCallbackFn_Int);
				P4Bridge.SetTextResultsCallbackFn(pServer, pTextResultsCallbackFn);
			}
		}

		/// <summary>
		/// Holds the call back passed to the bridge used to receive the 
		/// raw binary data 
		/// </summary>
		P4CallBacks.BinaryResultsDelegate
			BinaryResultsCallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr pBinaryResultsCallbackFn = IntPtr.Zero;

		object BinaryResultsCallback_Int_Sync = new object();

		/// <summary>
		/// Internal callback used to receive the raw binary data. 
		/// </summary>
		/// The binary data pointed to by a void* is marshaled into a byte[]
		/// <remarks>
		/// </remarks>
		/// <param name="pData">void* pointer</param>
		/// <param name="cnt">Byte count</param>
		internal void BinaryResultsCallback_Int(uint cmdId, IntPtr pData, int cnt)
		{
			lock (BinaryResultsCallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					// no callback set, so ignore
					if (BinaryResultsReceived == null)
						return;

					byte[] data = null;
					if (pData != IntPtr.Zero)
					{
						data = MarshalPtrToByteArrary(pData, cnt);
					}

					Delegate[] targetList = BinaryResultsReceived.GetInvocationList();
					foreach (BinaryResultsDelegate d in targetList)
					{
						try
						{
							d(cmdId, data);
						}
						catch
						{
							// problem with delegate, so remove from the list
							BinaryResultsReceived -= d;
						}
					}
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		/// <summary>
		/// Set the callback used to return real time binary output.
		/// </summary>
		/// <remarks>
		/// Far large output, the client may receive multiple callbacks.
		/// Simply concatenate the data to get the complete data.
		/// </remarks>
		internal void SetBinaryResultsCallback()
		{
			if (BinaryResultsCallbackFn_Int == null)
			{
				// initialize on first setting
				BinaryResultsCallbackFn_Int =
					new P4CallBacks.BinaryResultsDelegate(BinaryResultsCallback_Int);
				pBinaryResultsCallbackFn = Marshal.GetFunctionPointerForDelegate(BinaryResultsCallbackFn_Int);
				P4Bridge.SetBinaryResultsCallbackFn(pServer, pBinaryResultsCallbackFn);
			}
		}

		/// <summary>
		/// Holds the call back passed to the bridge used to receive input 
		/// prompts from the server
		/// </summary>
		P4CallBacks.PromptDelegate
		   PromptCallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr pPromptCallbackFn = IntPtr.Zero;

		object PromptCallback_Int_Sync = new object();

		internal void PromptCallback_Int(uint cmdId, IntPtr pMsg, IntPtr pRspBuf, int buffSize, bool display)
		{
			lock (PromptCallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					// no callback set, so ignore
					if (PromptHandler == null)
						return;

					String msg = null;
					if (pMsg != IntPtr.Zero)
					{
						msg = MarshalPtrToString(pMsg);
					}

					String response = null;
					try
					{
						response = PromptHandler(cmdId, msg, display);
					}
					catch
					{
						// problem with delegate, so clear it
						PromptHandler = null;
					}
					CopyStringToIntPtr(response, pRspBuf, buffSize);
				}
				finally 
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		internal void SetPromptCallback()
		{
			if (PromptCallbackFn_Int == null)
			{
				// initialize on first setting
				PromptCallbackFn_Int =
					new P4CallBacks.PromptDelegate(PromptCallback_Int);
				pPromptCallbackFn = Marshal.GetFunctionPointerForDelegate(PromptCallbackFn_Int);
				P4Bridge.SetPromptCallbackFn(pServer, pPromptCallbackFn);
			}
		}

		/// <summary>
		/// Holds the call back passed to the bridge used to receive resolve callbacks 
		/// with a ClientMerge object from the server
		/// </summary>
		P4CallBacks.ResolveDelegate
		   ResolveCallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr ResolveCallbackFn = IntPtr.Zero;

		object ResolveCallback_Int_Sync = new object();

		internal int ResolveCallback_Int(uint cmdId, IntPtr pMerger)
		{
			lock (ResolveCallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					P4ClientMerge.MergeStatus result = P4ClientMerge.MergeStatus.CMS_NONE;

					// no callback set, so ignore
					if (ResolveHandler == null)
						return -1;

					P4ClientMerge Merger = null;
					if (pMerger != IntPtr.Zero)
					{
						Merger = new P4ClientMerge(this, pMerger);
					}

					try
					{
						result = ResolveHandler(cmdId, Merger);
					}
					catch
					{
						// problem with delegate, so clear it
						PromptHandler = null;
						result = P4ClientMerge.MergeStatus.CMS_QUIT;
					}
					return (int)result;
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		internal void SetResolveCallback()
		{
			if (ResolveCallbackFn_Int == null)
			{
				// initialize on first setting
				ResolveCallbackFn_Int =
					new P4CallBacks.ResolveDelegate(ResolveCallback_Int);
				ResolveCallbackFn = Marshal.GetFunctionPointerForDelegate(ResolveCallbackFn_Int);
				P4ClientMergeBridge.SetResolveCallbackFn(pServer, ResolveCallbackFn);
			}
		}
		/// <summary>
		/// Holds the call back passed to the bridge used to receive resolve callbacks 
		/// with a ClientResolve object from the server
		/// </summary>
		P4CallBacks.ResolveADelegate
			ResolveACallbackFn_Int = null;

		/// <summary>
		/// Pinned function pointer for the delegate passed to the dll
		/// </summary>
		IntPtr ResolveACallbackFn = IntPtr.Zero;

		object ResolveACallback_Int_Sync = new object();

		internal int ResolveACallback_Int(uint cmdId, IntPtr pResolver, bool Preview)
		{
			lock (ResolveACallback_Int_Sync)
			{
				PauseRunCmdTimer(cmdId);
				try
				{
					P4ClientMerge.MergeStatus result = P4ClientMerge.MergeStatus.CMS_NONE;

					// no callback set, so ignore
					if (ResolveAHandler == null)
						return -1;

					P4ClientResolve Resolver = null;
					if (pResolver != IntPtr.Zero)
					{
						Resolver = new P4ClientResolve(this, pResolver);
					}

					try
					{
						result = ResolveAHandler(cmdId, Resolver);
					}
					catch
					{
						// problem with delegate, so clear it
						PromptHandler = null;
						result = P4ClientMerge.MergeStatus.CMS_QUIT;
					}
					return (int)result;
				}
				finally
				{
					ContinueRunCmdTimer(cmdId);
				}
			}
		}

		internal void SetResolveACallback()
		{
			if (ResolveACallbackFn_Int == null)
			{
				// initialize on first setting
				ResolveACallbackFn_Int =
					new P4CallBacks.ResolveADelegate(ResolveACallback_Int);
				ResolveACallbackFn = Marshal.GetFunctionPointerForDelegate(ResolveACallbackFn_Int);
				P4ClientResolveBridge.SetResolveACallbackFn(pServer, ResolveACallbackFn);
			}
		}
	}
}
