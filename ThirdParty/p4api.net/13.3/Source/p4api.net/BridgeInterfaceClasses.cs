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
 * Name		: BridgeInterfaceClasses.cs
 *
 * Author	: dbb
 *
 * Description	: Utility classes used to interface with the P4Bridged DLL.
 *
 ******************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;

namespace Perforce.P4
{
	/// <summary>
	/// The StrDictListIterator represents the iterator functions exposed by 
	/// the P4 bridge dll to allow the client to read structured (tagged) data.
	/// It is not exposed outside the p4.net assembly as the P4Server class
	/// translates the raw data into a .NET array list of hash tables.
	/// </summary>
	internal class StrDictListIterator : IDisposable
	{
		IntPtr pIterator = IntPtr.Zero;
		P4Server p4Server;

		public StrDictListIterator( P4Server p4server, IntPtr pObj )
		{
			pIterator = pObj;
			p4Server = p4server;
		}

		public bool NextItem()
		{
			IntPtr p = P4Bridge.GetNextItem( pIterator );
			if( p == IntPtr.Zero )
			{
				return false;
			}
			return true;
		}

		public KeyValuePair NextEntry()
		{
			IntPtr p = P4Bridge.GetNextEntry( pIterator );
			if( p == IntPtr.Zero )
			{
				return null;
			}
			return new KeyValuePair( p4Server, p );
		}

		public void Dispose()
		{
			P4Bridge.Release( pIterator );
		}
	}

	/// <summary>
	/// Simple calls used to read key:value pairs from a StrDict object 
	/// referenced by a pointer returned from the bridge
	/// </summary>
	internal class KeyValuePair
	{
		public string Key;
		public string Value;

		public KeyValuePair( P4Server p4Server, IntPtr pObj )
		{
            Key = p4Server.MarshalPtrToString(P4Bridge.GetKey(pObj));
			Value = p4Server.MarshalPtrToString( P4Bridge.GetValue( pObj ) );
		}
	}

	/// <summary>
	/// Error severity levels.
	/// </summary>
	public enum ErrorSeverity : int
	{
		/// <summary>
		/// Unknown
		/// </summary>
		E_UNKNOWN = -1, 
		/// <summary>
		/// nothing yet 
		/// </summary>
		E_EMPTY = 0,	
		/// <summary>
		/// something good happened
		/// </summary>
		E_INFO = 1,	    
		/// <summary>
		/// something not good happened 
		/// </summary>
		E_WARN = 2,	    
		/// <summary>
		/// user did something wrong
		/// </summary>
		E_FAILED = 3,	
		/// <summary>
		/// system broken -- nothing can continue
		/// </summary>
		E_FATAL = 4,    
		/// <summary>
		/// Used to turnoff exceptions
		/// </summary>
		E_NOEXC = 9999  
	};

	public enum ErrorGeneric : int
	{

		EV_NONE = 0,	// misc

		// The fault of the user

		EV_USAGE = 0x01,	// request not consistent with dox
		EV_UNKNOWN = 0x02,	// using unknown entity
		EV_CONTEXT = 0x03,	// using entity in wrong context
		EV_ILLEGAL = 0x04,	// trying to do something you can't
		EV_NOTYET = 0x05,	// something must be corrected first
		EV_PROTECT = 0x06,	// protections prevented operation

		// No fault at all

		EV_EMPTY = 0x11,	// action returned empty results

		// not the fault of the user

		EV_FAULT = 0x21,	// inexplicable program fault
		EV_CLIENT = 0x22,	// client side program errors
		EV_ADMIN = 0x23,	// server administrative action required
		EV_CONFIG = 0x24,	// client configuration inadequate
		EV_UPGRADE = 0x25,	// client or server too old to interact
		EV_COMM = 0x26,	// communications error
		EV_TOOBIG = 0x27	// not ever Perforce can handle this much

	} ;
	public enum ErrorSubsystem : int
	{

		ES_OS = 0,	// OS error
		ES_SUPP = 1,	// Misc support
		ES_LBR = 2,	// librarian
		ES_RPC = 3,	// messaging
		ES_DB = 4,	// database
		ES_DBSUPP = 5,	// database support
		ES_DM = 6,	// data manager
		ES_SERVER = 7,	// top level of server
		ES_CLIENT = 8,	// top level of client
		ES_INFO = 9,	// pseudo subsystem for information messages
		ES_HELP = 10,	// pseudo subsystem for help messages
		ES_SPEC = 11,	// pseudo subsystem for spec/comment messages
		ES_FTPD = 12,	// P4FTP server
		ES_BROKER = 13,	// Perforce Broker
		ES_P4QT = 14	// P4V and other Qt based clients
	} ;

	/*******************************************************************************
	 *  P4ClientError
	 ******************************************************************************/

	/// <summary>
	/// Class used to return a single error or warning from the bridge dll.
	/// </summary>
	public class P4ClientError
	{
		/// <summary>
		/// How severe is the error
		/// </summary>
		public ErrorSeverity SeverityLevel;
		/// <summary>
		/// Generic code for the error
		/// </summary>
		public int ErrorCode;
		/// <summary>
		/// Descriptive error message
		/// </summary>
		public string ErrorMessage;

		/// <summary>
		/// Create a new ClientError
		/// </summary>
		/// <param name="severityLevel"></param>
		/// <param name="errorMessage"></param>
		public P4ClientError(ErrorSeverity severityLevel, string errorMessage)
		{
			SeverityLevel = severityLevel;
			ErrorMessage = errorMessage;
		}

		/// <summary>
		/// Create a new ClientError
		/// </summary>
		/// <param name="severityLevel"></param>
		/// <param name="errorMessage"></param>
		public P4ClientError(IntPtr pErr)
		{
			SeverityLevel = (ErrorSeverity)P4Bridge.Severity(pErr);
			ErrorCode = P4Bridge.ErrorCode(pErr);
			// message is always in ASCII
			ErrorMessage = Marshal.PtrToStringAnsi(P4Bridge.Message(pErr));
		}

		int ErrorSubcode 
		{
			get 
			{
				return unchecked(ErrorCode & 0x3ff);
			}
		}

		ErrorSubsystem ErrorSubsystem 
		{
			get 
			{
				return (ErrorSubsystem) unchecked((ErrorCode >> 10) & 0x3f);
			}
		}

		ErrorGeneric ErrorGeneric 
		{
			get 
			{
				return (ErrorGeneric) unchecked((ErrorCode >> 16) & 0xff);
			}
		}

		int ArgCount 
		{
			get 
			{
				return ((ErrorCode >> 24) & 0x0f);
			}
		}

		ErrorSeverity ErrorSeverity 
		{
			get 
			{
				return (ErrorSeverity) unchecked((ErrorCode >> 28) & 0x0f);
			}
		}

		int UniqueCode 
		{
			get 
			{
				return (ErrorCode & 0xffff);
			}
		}

		static int eErrorSubcode(int ErrorCode)
		{
			return unchecked(ErrorCode & 0x3ff);
		}

		static ErrorSubsystem eErrorSubsystem(int ErrorCode)
		{
			return (ErrorSubsystem)unchecked((ErrorCode >> 10) & 0x3f);
		}

		static ErrorGeneric eErrorGeneric(int ErrorCode)
		{
			return (ErrorGeneric)unchecked((ErrorCode >> 16) & 0xff);
		}

		static int eArgCount(int ErrorCode)
		{
			return ((ErrorCode >> 24) & 0x0f);
		}

		static ErrorSeverity eErrorSeverity(int ErrorCode)
		{
			return (ErrorSeverity)unchecked((ErrorCode >> 28) & 0x0f);
		}

		static int eUniqueCode(int ErrorCode)
		{
			return (ErrorCode & 0xffff);
		}

		/// <summary>
		/// Format the error in the form [ErrorLevel] Message
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return String.Format( "[{0}] {1}", Enum.GetName( typeof( ErrorSeverity ), SeverityLevel ), ErrorMessage );
		}

		public static int ErrorOf(ErrorSubsystem sub, int cod, ErrorSeverity sev, ErrorGeneric gen, int arg)
		{
			return ((int)sev << 28) | (arg << 24) | ((int)gen << 16) | ((int)sub << 10) | (cod);
		}

		// P4 API errors are defined in various files in the API including msgrpc.cc, msgclient.cc, msgdm.cc,
		// msghelp.cc, msglbr.cc, msgrpc.cc, msgserver.cc, and msgsupp.cc, in the p4\msgs directory
		public static int HostKeyMismatch		= ErrorOf( ErrorSubsystem.ES_RPC, 49, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 3 );//, "******* WARNING P4PORT IDENTIFICATION HAS CHANGED! *******\n"
		public static int HostKeyUnknown		= ErrorOf( ErrorSubsystem.ES_RPC, 48, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 3 ); //"The authenticity of '%host%' can't be established,\n"
		public static int MustSetPassword		= ErrorOf( ErrorSubsystem.ES_SERVER, 307, ErrorSeverity.E_FAILED, ErrorGeneric.EV_USAGE, 0); // "Password must be set before access can be granted." 
		public static int TicketOnly			= ErrorOf( ErrorSubsystem.ES_SERVER, 322,  ErrorSeverity.E_FAILED, ErrorGeneric.EV_USAGE, 0); // "Password not allowed at this server security level, use 'p4 login'."
		public static int LoggedOut				= ErrorOf( ErrorSubsystem.ES_SERVER, 318, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 0); // "Your session was logged out, please login again."
		public static int LoginExpired			= ErrorOf( ErrorSubsystem.ES_SERVER, 312, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 0 ); // "Your session has expired, please login again."
		public static int NoSuchDomain2			= ErrorOf( ErrorSubsystem.ES_DM, 35, ErrorSeverity.E_FAILED, ErrorGeneric.EV_UNKNOWN, 3);//"%type% '%name%' unknown - use '%command%' command to create it."
		public static int NotUnderRoot			= ErrorOf( ErrorSubsystem.ES_DB, 39, ErrorSeverity.E_FAILED, ErrorGeneric.EV_CONTEXT, 2 );// "Path '%path%' is not under client's root '%root%'."
		public static int NotUnderClient		= ErrorOf( ErrorSubsystem.ES_DB, 40, ErrorSeverity.E_FAILED, ErrorGeneric.EV_CONTEXT, 2 );// "Path '%path%' is not under client '%client%'."
		public static int IntegMovedUnmapped	= ErrorOf( ErrorSubsystem.ES_DM, 551, ErrorSeverity.E_INFO, 0, 2 );// "%depotFile% - not in client view (remapped from %movedFrom%)" };
		public static int ExVIEW                = ErrorOf( ErrorSubsystem.ES_DM, 367, ErrorSeverity.E_WARN, ErrorGeneric.EV_EMPTY, 1 );// "[%argc% - file(s)|File(s)] not in client view." } ;
		public static int ExVIEW2				= ErrorOf( ErrorSubsystem.ES_DM, 477, ErrorSeverity.E_WARN, ErrorGeneric.EV_EMPTY, 2);// "%!%[%argc% - file(s)|File(s)] not in client view." };
		public static int ActionResolve111		= ErrorOf( ErrorSubsystem.ES_SERVER, 498, ErrorSeverity.E_WARN,  ErrorGeneric.EV_UPGRADE, 2);//"%localFile% - upgrade to a 2011.1 or later client to perform an interactive %resolveType% resolve, or use resolve -a."

		public static int WeakPassword			= ErrorOf( ErrorSubsystem.ES_SERVER, 321,  ErrorSeverity.E_FAILED, ErrorGeneric.EV_USAGE, 0 ); // "The security level of this server requires the password to be reset."
		public static int BadPassword			= ErrorOf( ErrorSubsystem.ES_SERVER, 21, ErrorSeverity.E_FAILED, ErrorGeneric.EV_CONFIG, 0);// "Perforce password (P4PASSWD) invalid or unset." 
		public static int BadPassword0			= ErrorOf( ErrorSubsystem.ES_SERVER, 38, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 0 ); // "Password invalid."
		public static int BadPassword1			= ErrorOf( ErrorSubsystem.ES_SERVER, 39, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 0 ); // "Passwords don't match."
		public static int PasswordTooShort		= ErrorOf( ErrorSubsystem.ES_SERVER, 308, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 0 ); // "Password should be at least 8 characters in length."
		public static int PasswordTooLong		= ErrorOf( ErrorSubsystem.ES_SERVER, 527, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 1 ); // "Password should be no longer than %maxLength% bytes in length."
		public static int PasswordTooSimple		= ErrorOf( ErrorSubsystem.ES_SERVER, 309, ErrorSeverity.E_FAILED, ErrorGeneric.EV_ILLEGAL, 0); // "Password should be mixed case or contain non alphabetic characters."

		public static int TcpAccept             = ErrorOf( ErrorSubsystem.ES_RPC, 11, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "TCP connection accept failed." } ;
		public static int TcpConnect            = ErrorOf( ErrorSubsystem.ES_RPC, 12, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "TCP connect to %host% failed." } ;
		public static int TcpHost               = ErrorOf( ErrorSubsystem.ES_RPC, 13, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "%host%: host unknown." } ;
		public static int TcpListen             = ErrorOf( ErrorSubsystem.ES_RPC, 14, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "TCP listen on %service% failed." } ;
		public static int TcpPortInvalid        = ErrorOf( ErrorSubsystem.ES_RPC, 22, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "TCP port number %service% is out of range." } ;
		public static int TcpRecv               = ErrorOf( ErrorSubsystem.ES_RPC, 15, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "TCP receive failed." } ;
		public static int TcpSend               = ErrorOf( ErrorSubsystem.ES_RPC, 16, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "TCP send failed." } ;
		public static int TcpService            = ErrorOf( ErrorSubsystem.ES_RPC, 17, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "%service%: service unknown." } ;
		public static int TcpPeerSsl            = ErrorOf( ErrorSubsystem.ES_RPC, 31, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "Failed client SSL connection setup, server not using SSL." };

		public static int SslAccept             = ErrorOf( ErrorSubsystem.ES_RPC, 38, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "SSL connection accept failed %error%.\n\tClient must add SSL protocol prefix to P4PORT." };
		public static int SslConnect			= ErrorOf( ErrorSubsystem.ES_RPC, 23, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 2 ); // "SSL connect to %host% failed %error%.\n\tRemove SSL protocol prefix from P4PORT." };
		public static int SslListen				= ErrorOf( ErrorSubsystem.ES_RPC, 24, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "SSL listen on %service% failed." };
		public static int SslRecv				= ErrorOf( ErrorSubsystem.ES_RPC, 25, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "SSL receive failed." };
		public static int SslSend				= ErrorOf( ErrorSubsystem.ES_RPC, 26, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "SSL send failed." };
		public static int SslClose				= ErrorOf( ErrorSubsystem.ES_RPC, 27, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "SSL close failed." };
		public static int SslInvalid			= ErrorOf( ErrorSubsystem.ES_RPC, 28, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "Invalid operation for SSL on %service%." };
		public static int SslCtx				= ErrorOf( ErrorSubsystem.ES_RPC, 29, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 1 ); // "Fail create ctx on %service%." };
		public static int SslShutdown			= ErrorOf( ErrorSubsystem.ES_RPC, 30, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "SSL read/write failed since in Shutdown." };
		public static int SslInit				= ErrorOf( ErrorSubsystem.ES_RPC, 32, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "Failed to initialize SSL library." };
		public static int SslCleartext			= ErrorOf( ErrorSubsystem.ES_RPC, 33, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "Failed client connect, server using SSL.\nClient must add SSL protocol prefix to P4PORT." };
		public static int SslCertGen			= ErrorOf( ErrorSubsystem.ES_RPC, 34, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "Unable to generate certificate or private key for server." };
		public static int SslNoSsl				= ErrorOf( ErrorSubsystem.ES_RPC, 35, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "Trying to use SSL when SSL library has not been compiled into program." };
		public static int SslBadKeyFile			= ErrorOf( ErrorSubsystem.ES_RPC, 36, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "Either privatekey.txt or certificate.txt files do not exist." };
		public static int SslGetPubKey          = ErrorOf( ErrorSubsystem.ES_RPC, 40, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "Unable to get public key for token generation." } ;
		public static int SslBadDir             = ErrorOf( ErrorSubsystem.ES_RPC, 41, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "P4SSLDIR not defined or does not reference a valid directory." } ;
		public static int SslBadFsSecurity      = ErrorOf( ErrorSubsystem.ES_RPC, 42, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "P4SSLDIR directory or key and certificate files not secure." } ;
		public static int SslDirHasCreds        = ErrorOf( ErrorSubsystem.ES_RPC, 43, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "P4SSLDIR contains credentials, please remove key and certificate files." } ;
		public static int SslCredsBadOwner      = ErrorOf( ErrorSubsystem.ES_RPC, 44, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "P4SSLDIR or credentials files not owned by Perforce process effective user." } ;
		public static int SslCertBadDates       = ErrorOf( ErrorSubsystem.ES_RPC, 45, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "Certificate date range invalid." } ;
		public static int SslNoCredentials      = ErrorOf( ErrorSubsystem.ES_RPC, 46, ErrorSeverity.E_FATAL, ErrorGeneric.EV_COMM, 0 ); // "SSL credentials do not exist." } ;
		public static int SslFailGetExpire      = ErrorOf( ErrorSubsystem.ES_RPC, 47, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "Failed to get certificate's expiration date." } ;

		public static int Connect				= ErrorOf( ErrorSubsystem.ES_CLIENT, 1, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "Connect to server failed; check $P4PORT." } ;
		public static int ZCResolve				= ErrorOf( ErrorSubsystem.ES_CLIENT, 33, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 0 ); // "Zeroconf resolved '%name%' to '%port%'." } ;
		public static int Fatal					= ErrorOf( ErrorSubsystem.ES_CLIENT, 2, ErrorSeverity.E_FATAL, ErrorGeneric.EV_CLIENT, 0); // "Fatal client error; disconnecting!" };

		public static int MsgOs_Net				= ErrorOf( ErrorSubsystem.ES_OS, 3, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 3 );// "%operation%: %arg%: %errmsg%" } ;
		public static int MsgOs_NetUn			= ErrorOf( ErrorSubsystem.ES_OS, 4, ErrorSeverity.E_FAILED, ErrorGeneric.EV_COMM, 3 );// "%operation%: %arg%: unknown network error %errno%" } ;

		public static bool IsLoginError(int errorNum)
		{
			return (errorNum == BadPassword) || (errorNum == MustSetPassword) || (errorNum == LoggedOut);
		}

		public static bool IsBadPasswdError(int errorNum)
		{
			return (errorNum == WeakPassword) || (errorNum == BadPassword0) || (errorNum == BadPassword1) ||
				(errorNum == PasswordTooShort) || (errorNum == PasswordTooLong) || (errorNum == PasswordTooSimple);
		}

		public static bool IsTCPError(int errorNum)
		{
//#if DEBUG
//            int eSubCode = eErrorSubcode(errorNum);
//            ErrorSubsystem eSubSys = eErrorSubsystem(errorNum);
//            ErrorGeneric eGeneric = eErrorGeneric(errorNum);
//            int eArgC = eArgCount(errorNum);
//            ErrorSeverity eSeverity = eErrorSeverity(errorNum);
//            int eUnique = eUniqueCode(errorNum);

//            int ConnectSubCode = eErrorSubcode(Connect);
//            ErrorSubsystem ConnectSubSys = eErrorSubsystem(Connect);
//            ErrorGeneric ConnectGeneric = eErrorGeneric(Connect);
//            int ConnectArgC = eArgCount(Connect);
//            ErrorSeverity ConnectSeverity = eErrorSeverity(Connect);
//            int ConnectUnique = eUniqueCode(Connect);

//            int TcpConnectCode = eErrorSubcode(TcpConnect);
//            ErrorSubsystem TcpConnectSubSys = eErrorSubsystem(TcpConnect);
//            ErrorGeneric TcpConnectGeneric = eErrorGeneric(TcpConnect);
//            int TcpConnectArgC = eArgCount(TcpConnect);
//            ErrorSeverity TcpConnectSeverity = eErrorSeverity(TcpConnect);
//            int TcpConnectUnique = eUniqueCode(TcpConnect);

//#endif
			return (errorNum == TcpAccept) || (errorNum == TcpConnect) || (errorNum == TcpHost) ||
				(errorNum == TcpListen) || (errorNum == TcpPortInvalid) || (errorNum == TcpRecv) ||
				(errorNum == TcpSend) || (errorNum == TcpService) || (errorNum == TcpPeerSsl) || 
				(errorNum == Connect) ||  (errorNum == ZCResolve) ||  (errorNum == Fatal) ||
				(errorNum == MsgOs_Net) || (errorNum == MsgOs_NetUn);
		}

		public static bool IsSSLError(int errorNum)
		{
			return (errorNum == SslAccept) || (errorNum == SslConnect) || (errorNum == SslListen) ||
				(errorNum == SslRecv) || (errorNum == SslSend) || (errorNum == SslClose) ||
				(errorNum == SslInvalid) || (errorNum == SslCtx) || (errorNum == SslShutdown) ||
				(errorNum == SslInit) || (errorNum == SslCleartext) || (errorNum == SslCertGen) ||
				(errorNum == SslNoSsl) || (errorNum == SslBadKeyFile) || (errorNum == SslGetPubKey) || 
				(errorNum == SslBadDir) || (errorNum == SslBadFsSecurity) || (errorNum == SslDirHasCreds) ||
				(errorNum == SslCredsBadOwner) || (errorNum == SslCertBadDates) || 
				(errorNum == SslNoCredentials) ||  (errorNum == SslFailGetExpire);
		}
	}

	/*******************************************************************************
	 *  P4ClientErrorList
	 ******************************************************************************/

	/// <summary>
	/// Class used to return a list of errors and warnings returned by a command.
	/// </summary>
	public class P4ClientErrorList : List<P4ClientError>
	{
		//public P4ClientError Error;

		//internal P4ClientErrorList NextError = null;

		/// <summary>
		/// Read an error list from the server by traversing the linked list.
		/// </summary>
		/// <param name="p4Server">Perforce server</param>
		/// <param name="pObj">First error in the list</param>
		private void Init( P4Server p4Server, IntPtr pObj )
		{
			IntPtr pNext = pObj;

			while (pNext != IntPtr.Zero)
			{
				IntPtr pCurrent = pNext; 

				pNext = P4Bridge.Next(pNext);
				this.Add(new P4ClientError(pCurrent));
			}
		}

		/// <summary>
		/// Create a list of a single error
		/// </summary>
		/// <param name="Err">Error</param>
		internal P4ClientErrorList(P4ClientError Err)
		{
			this.Add(Err);
		}

		/// <summary>
		/// Create a list of a single error
		/// </summary>
		/// <param name="msg">Error message</param>
		/// <param name="severity">Severity level</param>
		internal P4ClientErrorList(string msg, ErrorSeverity severity)
		{
			this.Add(new P4ClientError(severity, msg));
		}

		/// <summary>
		/// Read the error list from the server
		/// </summary>
		/// <param name="p4Server"></param>
		/// <param name="pObj"></param>
		public P4ClientErrorList( P4Server p4Server, IntPtr pObj )
		{
			Init( p4Server, pObj );
		}

		/// <summary>
		/// Create a list from the current error results on the server
		/// </summary>
		/// <param name="p4Server">Perforce Server</param>
		public P4ClientErrorList( P4Server p4Server, uint cmdId )
		{
			IntPtr pObj = P4Bridge.GetErrorResults( p4Server.pServer, cmdId );

			Init( p4Server, pObj );
		}

		/// <summary>
		/// Cast the errors to a String[]
		/// </summary>
		/// <param name="l">the list to cast </param>
		/// <returns></returns>
		public static implicit operator String[](P4ClientErrorList l)
		{
			String[] r = new String[l.Count];
			int idx = 0;
			foreach (P4ClientError e in l)
				r[idx++] = e.ToString();
			return r;
		}

		/// <summary>
		/// Cast the errors to a String. Each error is separated by \r\n
		/// </summary>
		/// <param name="l">the list to cast </param>
		/// <returns></returns>
		public static implicit operator String(P4ClientErrorList l)
		{
			StringBuilder r = new StringBuilder(l.Count * 80);
			foreach (P4ClientError e in l)
			{
				r.Append(e.ToString());
				r.Append("/r/n");
			}
			return r.ToString();
		}
	}

	/// <summary>
	/// Used to wrap a byte[] that is pinned in memory to pass down to the dll.
	/// In general, since .NET does not natively support marshaling Strings to
	/// UTF-8 for char * parameters, we must do our own encoding of the 
	/// strings into byte[], pin them, pass them to the dll, and then free the
	/// pinned memory on return. This class wraps that process with an 
	/// IDisposable object to ensure the memory is freed after the call.
	/// </summary>
	internal class PinnedByteArray :IDisposable
	{
		private byte[] Value;
		private GCHandle hValue;
		private IntPtr pValue;

		private bool isValid;

		public static PinnedByteArray nullv = new PinnedByteArray();

		private PinnedByteArray()
		{
			isValid = false;
		}

		~PinnedByteArray()
		{
			Free();
		}

		public PinnedByteArray( byte[] value )
		{
			Value = value;
			hValue = GCHandle.Alloc( Value, GCHandleType.Pinned );
			pValue = hValue.AddrOfPinnedObject();

			isValid = true;
		}

		public static implicit operator IntPtr( PinnedByteArray p )
		{
			if (p == null)
			{
				return IntPtr.Zero;
			}
			return p.pValue;
		}

		public static implicit operator GCHandle( PinnedByteArray p )
		{
			if (p == null)
			{
				throw new ArgumentNullException("p");
			}
			return p.hValue;
		}

		public static implicit operator byte[]( PinnedByteArray p )
		{
			if (p == null)
			{
				return null;
			}
			return p.Value;
		}

		public void Free()
		{
			if( !isValid )
				return;

			isValid = false;

			Value = null;

			hValue.Free();
			// hValue = null; value type so can set to null

			pValue = IntPtr.Zero;
		}

		public void Dispose()
		{
			Free();
		}
	}

	/**********************************************************************
	*
	*  P4ClientMerge
	*
	*  This simple class is a ClientMerge object.
	*
	*********************************************************************/
	public class P4ClientMerge
	{
		public enum MergeType
		{
			CMT_BINARY = 0,	// binary merge
			CMT_3WAY, 	// 3-way text 
			CMT_2WAY	// 2-way text
		} ;

		public enum MergeStatus
		{
			CMS_NONE = -1,	// Has not been determined
			CMS_QUIT = 0,	// user wants to quit
			CMS_SKIP,	// skip the integration record
			CMS_MERGED,	// accepted merged theirs and yours
			CMS_EDIT,	// accepted edited merge
			CMS_THEIRS,	// accepted theirs
			CMS_YOURS	// accepted yours,
		} ;

		public enum MergeForce
		{
			CMF_AUTO = 0,	// don't force			// -am
			CMF_SAFE,	// accept only non-conflicts	// -as
			CMF_FORCE	// accept anything		// -af
		} ;

		P4Server p4Server = null;
		IntPtr pObj = IntPtr.Zero;

		public P4ClientMerge(P4Server pserver,IntPtr p)
		{
			p4Server = pserver;
			pObj = p;
		}

		public MergeStatus AutoResolve(MergeForce forceMerge)
		{
			return (MergeStatus)P4ClientMergeBridge.AutoResolve(pObj, (int)forceMerge);
		}

		public MergeStatus Resolve()
		{
			return (MergeStatus)P4ClientMergeBridge.Resolve(pObj);
		}

		public MergeStatus DetectResolve()
		{
			return (MergeStatus)P4ClientMergeBridge.DetectResolve(pObj);
		}

		public bool IsAcceptable()
		{
			return (bool)(P4ClientMergeBridge.IsAcceptable(pObj) != 0);
		}

		public string GetBaseFile()
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetBaseFile(pObj) );
		}

		public string GetYourFile()
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetYourFile(pObj) );
		}

		public string GetTheirFile()
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetTheirFile(pObj) );
		}

		public string GetResultFile()
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetResultFile(pObj) );
		}

		public int GetYourChunks()
		{
			return P4ClientMergeBridge.GetYourChunks(pObj);
		}

		public int GetTheirChunks()
		{
			return P4ClientMergeBridge.GetTheirChunks(pObj);
		}

		public int GetBothChunks()
		{
			return P4ClientMergeBridge.GetBothChunks(pObj);
		}

		public int GetConflictChunks()
		{
			return P4ClientMergeBridge.GetConflictChunks(pObj);
		}

		public string GetMergeDigest(IntPtr pObj)
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetMergeDigest(pObj) );
		}

		public string GetYourDigest()
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetYourDigest(pObj) );
		}

		public string GetTheirDigest()
		{
			return p4Server.MarshalPtrToString( P4ClientMergeBridge.GetTheirDigest(pObj) );
		}

		public P4ClientError GetLastError(IntPtr pObj)
		{
			return new P4ClientError(P4ClientMergeBridge.GetLastError(pObj));
		}
	}
	/**********************************************************************
	*
	*  P4ClientResolve
	*
	*  This simple class is a ClientResolve object.
	*
	*********************************************************************/
	/// <summary>
	/// Class containing the DLL imports for the P4Bridge DLL.
	/// </summary>
	public class P4ClientResolve
	{
		P4Server p4Server = null;
		IntPtr pObj = IntPtr.Zero;

		public P4ClientResolve(P4Server pserver, IntPtr p)
		{
			p4Server = pserver;
			pObj = p;
		}

		public P4ClientMerge.MergeStatus AutoResolve(P4ClientMerge.MergeForce force)
		{
			return (P4ClientMerge.MergeStatus)P4ClientResolveBridge.AutoResolve(pObj, (int)force);
		}

		public P4ClientMerge.MergeStatus Resolve(bool preview)
		{
			return (P4ClientMerge.MergeStatus)P4ClientResolveBridge.Resolve(pObj, preview);
		}

		private string _resolveType = null;
		public string ResolveType
		{
			get
			{
				if (_resolveType == null)
				{
					_resolveType = GetResolveType();
				}
				return _resolveType;
			}
		}

	private string _mergeAction;
	public string MergeAction
	{
		get
		{
			if (_mergeAction == null)
			{
				_mergeAction = GetMergeAction();
			}
			return _mergeAction;
		}
	}
	private string _yoursAction;
	public string YoursAction
	{
		get
		{
			if (_yoursAction == null)
			{
				_yoursAction = GetYoursAction();
			}
			return _yoursAction;
		}
	}
	private string _theirAction;
	public string TheirAction
	{
		get
		{
			if (_theirAction == null)
			{
				_theirAction = GetTheirAction();
			}
			return _theirAction;
		}
	}

	// For the CLI interface, probably not of interest to others

	private string _mergePrompt;
	public string MergePrompt
	{
		get
		{
			if (_mergePrompt == null)
			{
				_mergePrompt = GetMergePrompt();
			}
			return _mergePrompt;
		}
	}
	private string _yoursPrompt;
	public string YoursPrompt
	{
		get
		{
			if (_yoursPrompt == null)
			{
				_yoursPrompt = GetYoursPrompt();
			}
			return _yoursPrompt;
		}
	}
	private string _theirPrompt;
	public string TheirPrompt
	{
		get
		{
			if (_theirPrompt == null)
			{
				_theirPrompt = GetTheirPrompt();
			}
			return _theirPrompt;
		}
	}

	private string _mergeOpt;
	public string MergeOpt
	{
		get
		{
			if (_mergeOpt == null)
			{
				_mergeOpt = GetMergeOpt();
			}
			return _mergeOpt;
		}
	}
	private string _yoursOpt;
	public string YoursOpt
	{
		get
		{
			if (_yoursOpt == null)
			{
				_yoursOpt = GetYoursOpt();
			}
			return _yoursOpt;
		}
	}
	private string _theirOpt;
	public string TheirOpt
	{
		get
		{
			if (_theirOpt == null)
			{
				_theirOpt = GetTheirOpt();
			}
			return _theirOpt;
		}
	}
	private string _skipOpt;
	public string SkipOpt
	{
		get
		{
			if (_skipOpt == null)
			{
				_skipOpt = GetSkipOpt();
			}
			return _skipOpt;
		}
	}
	private string _helpOpt;
	public string HelpOpt
	{
		get
		{
			if (_helpOpt == null)
			{
				_helpOpt = GetHelpOpt();
			}
			return _helpOpt;
		}
	}
	private string _autoOpt;
	public string AutoOpt
	{
		get
		{
			if (_autoOpt == null)
			{
				_autoOpt = GetAutoOpt();
			}
			return _autoOpt;
		}
	}

	private string _prompt;
	public string Prompt
	{
		get
		{
			if (_prompt == null)
			{
				_prompt = GetPrompt();
			}
			return _prompt;
		}
	}
	private string _typePrompt;
	public string TypePrompt
	{
		get
		{
			if (_typePrompt == null)
			{
				_typePrompt = GetTypePrompt();
			}
			return _typePrompt;
		}
	}
	private string _usageError;
	public string UsageError
	{
		get
		{
			if (_usageError == null)
			{
				_usageError = GetUsageError();
			}
			return _usageError;
		}
	}
	private string _help;
	public string Help
	{
		get
		{
			if (_help == null)
			{
				_help = GetHelp();
			}
			return _help;
		}
	}

		private String GetResolveType()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetResolveType(pObj));
		}

		private String GetMergeAction()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetMergeAction(pObj));
		}

		private String GetYoursAction()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetYoursAction(pObj));
		}

		private String GetTheirAction()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetTheirAction(pObj));
		}

		private String GetMergePrompt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetMergePrompt(pObj));
		}

		private String GetYoursPrompt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetYoursPrompt(pObj));
		}

		private String GetTheirPrompt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetTheirPrompt(pObj));
		}

		private String GetMergeOpt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetMergeOpt(pObj));
		}

		private String GetYoursOpt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetYoursOpt(pObj));
		}

		private String GetTheirOpt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetTheirOpt(pObj));
		}

		private String GetSkipOpt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetSkipOpt(pObj));
		}

		private String GetHelpOpt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetHelpOpt(pObj));
		}

		private String GetAutoOpt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetAutoOpt(pObj));
		}

		private String GetPrompt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetPrompt(pObj));
		}

		private String GetTypePrompt()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetTypePrompt(pObj));
		}

		private String GetUsageError()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetUsageError(pObj));
		}

		private String GetHelp()
		{
			return p4Server.MarshalPtrToString(P4ClientResolveBridge.GetHelp(pObj));
		}

		public P4ClientError GetLastError()
		{
			return new P4ClientError(P4ClientResolveBridge.GetLastError(pObj));
		}
	}

	/// <summary>
	/// Wrapper for an array of PinnedByteArrays, used to wrap the arg list
	/// passed to a command to make sure the pinned memory is freed.
	/// </summary>
	internal class PinnedByteArrays : IDisposable
	{
		private PinnedByteArray[] Values;
		private IntPtr[] pValues = null;

		public PinnedByteArrays( PinnedByteArray[] values )
		{
			Values = values;
		}

		~PinnedByteArrays()
		{
			Free();
		}

		public static explicit operator IntPtr[]( PinnedByteArrays p )
		{
			if( p.pValues != null )
				return p.pValues;

			p.pValues = new IntPtr[ p.Values.Length ];
			for( int idx = 0; idx < p.Values.Length; idx++ )
			{
				p.pValues[ idx ] = (IntPtr)p.Values[ idx ];
			}
			return p.pValues;
		}

		public PinnedByteArray this[ int idx ]
		{
			get { return Values[ idx ]; }
		}

		public void Free()
		{
			if( Values == null )
				return;

			for( int idx = 0; idx < Values.Length; idx++ )
			{
				Values[ idx ].Free();
			}
		}

		public void Dispose()
		{
			Free();
		}
	}
}
