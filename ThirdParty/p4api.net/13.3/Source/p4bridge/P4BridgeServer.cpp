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
 * Name		: P4BridgeServer.cpp
 *
 * Author	: dbb
 *
 * Description	:  P4BridgeServer
 *
 ******************************************************************************/
#include "StdAfx.h"
#include "P4BridgeServer.h"
#include "ConnectionManager.h"
#include "P4Connection.h"

//#include <strtable.h>
//#include <strarray.h>
#include <spec.h>
#include <debug.h>
#include <enviro.h>
//#include <ignore.h>

bool CheckErrorId(const ErrorId &eid, const ErrorId &tgt)
{
    return eid.Subsystem() == tgt.Subsystem() && eid.SubCode() == tgt.SubCode();
}

bool CheckErrorId(Error  &e, const ErrorId &tgt)
{
	if (e.Test())
	{
		// iterate through the ErrorIds in this Error
		for (int i = 0; ; ++i) 
		{
			ErrorId    *eid = e.GetId(i);
			if (eid == NULL)
				break;
			if (CheckErrorId(*eid, tgt) )
			{
				return true;
			}
		}
	} 
	return false;
}

// This is were the pointer to the log callback is stored if set by the user.
LogCallbackFn * P4BridgeServer::pLogFn = NULL;

int HandleException_Static(unsigned int c, struct _EXCEPTION_POINTERS *e)
{
	return EXCEPTION_EXECUTE_HANDLER;
}

/******************************************************************************
// LogMessage: Use the client logging callback function (if set) to log a 
//   message in the callers log.
******************************************************************************/
int P4BridgeServer::LogMessage(int log_level, char * file, int line, char * message, ...)
{
	if (pLogFn)
	{
		va_list args;
		va_start(args, message);

		int buffSize = 1024;
		char* buff1;

 		int len = -1;
		while (len < 0)
		{
			buff1 = new char[buffSize];
			len = vsnprintf( buff1, buffSize, message, args);
			buffSize *= 2;
		}

		int ret = 0;

		__try
		{
			ret = (*pLogFn)(log_level, file, line, buff1);
		} 
		__except (HandleException_Static(GetExceptionCode(), GetExceptionInformation()))
		{
			// bad ptr?
			pLogFn = NULL; 
		}
		delete[] buff1;
		buff1 = NULL;
	
		return ret;
	}
	return 0;
}

/*******************************************************************************
 *
 *  Default Constructer
 *
 *  Protected, should not be used by a client to create a P4BridgeServer.
 *
 ******************************************************************************/

P4BridgeServer::P4BridgeServer(void)
	 : p4base(Type()), isUnicode(-1), useLogin(0), supportsExtSubmit(0)
{
}

P4BridgeClient* P4BridgeServer::get_ui( int cmdId ) 
{ 
	P4ClientError *err = NULL;
	//if( !connected( &err ) )
	//{
	//	return 0;
	//}
	if (!ConMgr)
	{
		return NULL;
	}
	P4Connection* con = ConMgr->GetConnection(cmdId);
	if (!con)
	{
		return NULL;
	}

	return con->ui; 
}

P4BridgeClient* P4BridgeServer::find_ui( int cmdId ) 
{ 
	P4ClientError *err = NULL;
	//if( !connected( &err ) )
	//{
	//	return 0;
	//}
	if (!ConMgr)
	{
		return NULL;
	}
	P4Connection* con = (P4Connection*) ConMgr->Find(cmdId);
	if (!con)
	{
		return NULL;
	}

	return con->ui; 
}

/*******************************************************************************
 *
 *  Constructer
 *
 *  Create a P4BridgeServer and connect to the specified P4 Server.
 *
 ******************************************************************************/

P4BridgeServer::P4BridgeServer( const char *server, 
								const char *user, 
								const char *pass,
								const char *ws_client)
	: p4base(Type())
{
	Locker.InitCritSection();

	LOCK(&Locker);

	disposed = 0;

	isUnicode = -1;
	useLogin = 0;
	supportsExtSubmit = 0;
	connecting = 0;

	// Clear the the callbacks 
	pTaggedOutputCallbackFn = NULL;
	pErrorCallbackFn = NULL;
	pInfoResultsCallbackFn = NULL;
	pTextResultsCallbackFn = NULL;
	pBinaryResultsCallbackFn = NULL;
	pPromptCallbackFn = NULL;
	pResolveCallbackFn = NULL;
	pResolveACallbackFn = NULL;

	ExceptionError = NULL;

	pProgramName = NULL;
	pProgramVer = NULL;

	// connect to the server using a tagged protocol
	//client_tagged = new ClientApi;
	//if( server )
	//	client_tagged->SetPort( server );
	//if( user )
	//	client_tagged->SetUser( user );
	//if( pass )
	//	client_tagged->SetPassword( pass );
	//if( ws_client )
	//	client_tagged->SetClient( ws_client );
	//client_tagged->SetProtocol( "tag", "" );
	//// return spec string when getting spec based objects
	//client_tagged->SetProtocol( "specstring", "" ); 
	//client_tagged->SetProtocol( "api", "70" ); // 2006.1 api
	//client_tagged->SetProtocol( "enableStreams", "" );

	//
	// Load the current P4CHARSET if set.
	//
	//if( client_tagged->GetCharset().Length() )
	//	char * cs = client_tagged->GetCharset().Text();

	// connect to the server using a untagged protocol
	ConMgr = new ConnectionManager(this);
	if( server )
		ConMgr->SetPort( server );
	if( user )
		ConMgr->SetUser( user );
	if( pass )
		ConMgr->SetPassword( pass );
	if( ws_client )
		ConMgr->SetClient( ws_client );
	//client->SetProtocol( "specstring", "" ); 
	//client->SetProtocol( "api", "72" ); // 2006.1 api
	//client->SetProtocol( "enableStreams", "" );
	//if( server )
	//{
	//	P4ClientError *err = NULL;
	//	if (!connected(&err))
	//	{
	//		return;
	//	}
	//	p4debug.SetLevel("-vnet.maxwait=5");
	//}

	//if( client->GetCharset().Length() )
	//	char * cs = client->GetCharset().Text();
}

/*******************************************************************************
 *
 *  Destructor
 *
 *  Close the connection and free up resources.
 *
 ******************************************************************************/

P4BridgeServer::~P4BridgeServer(void)
{
	if (disposed != 0)
	{
		return;
	}
	else
	{
		LOCK(&Locker); 
	
		disposed = 1;

		// Clear the the callbacks 
		pTaggedOutputCallbackFn = NULL;
		pErrorCallbackFn = NULL;
		pInfoResultsCallbackFn = NULL;
		pTextResultsCallbackFn = NULL;
		pBinaryResultsCallbackFn = NULL;
		pPromptCallbackFn = NULL;
		pResolveCallbackFn = NULL;
		pResolveACallbackFn = NULL;

		close_connection();
	
		if (ConMgr)
		{
			ConnectionManager* pOldConMgr = ConMgr;
			ConMgr = NULL;
			delete pOldConMgr;
		}
		//delete client_tagged;
		//client_tagged = NULL;

		if (pProgramName)
			delete[] pProgramName;
		pProgramName = NULL;

		if (pProgramVer)
			delete pProgramName;
		pProgramVer = NULL;
	}

	Locker.FreeCriticalSection();
}

/*******************************************************************************
 *
 *  connected
 *
 *  Connect to the specified P4 Server, create a UI.
 *
 ******************************************************************************/

int P4BridgeServer::connected( P4ClientError **err )
{
	__try
	{
		return connected_int( err );
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
	}
	connecting = 0;

	return 0;
}

int P4BridgeServer::connected_int( P4ClientError **err )
{
	LOCK(&Locker); 

	if((connecting) || ( ConMgr && ConMgr->Initialized))
	{
		return 1; // already connected
	}
	connecting = 1;
	if (!ConMgr)
	{
		// Create the p4client instance
		ConMgr = new ConnectionManager(this);
	}
	//char* arg = "-y";
	//char** args = &arg;

	//run_command( "trust", 1, args, 1 );

	//if (!run_command( "trust", 1, args, 1 ))
	//{
	//	P4ClientError *e = ui->GetErrorResults();
	//	if (e!= NULL)
	//	{
	//		*err = CopyStr(e->Message);
	//	}
	//	return 0;
	//}

	// Set the Unicode flag to unknown, to force a retest
	isUnicode = -1;

	apiLevel = -1;
	useLogin = -1;
	supportsExtSubmit = -1;

	if (GetServerProtocols())
	{
		p4debug.SetLevel("-vnet.maxwait=5");

		ConMgr->Initialized = 1;

		connecting = 0;

		return 1;
	}

	P4ClientError *e = ConMgr->DefaultConnection()->ui->GetErrorResults();
	if (e != NULL)
	{
		*err = new P4ClientError(e);
	}
	close_connection();

	ConMgr->Initialized = 0;
	connecting = 0;
	return 0;
}

/*******************************************************************************
 *
 *  connect_and_trust
 *
 *  Connect to the specified P4 Server, create a UI, and establish a trust 
 *	 relationship.
 *
 ******************************************************************************/

int P4BridgeServer::connect_and_trust( P4ClientError **err, char* trust_flag, char* fingerprint )
{
	__try
	{
		return connect_and_trust_int( err, trust_flag, fingerprint );
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
	}
	connecting = 0;

	return 0;
}

int P4BridgeServer::connect_and_trust_int( P4ClientError **err, char* trust_flag, char* fingerprint )
{
	LOCK(&Locker); 

	if((connecting) || ( ConMgr && ConMgr->Initialized))
	{
		return 1; // already connected
	}
	connecting = 1;
	if (!ConMgr)
	{
		// Create the p4client instance
		ConMgr = new ConnectionManager(this);
	}

	char** args = new char*[2];
	args[0] = "-d";

	run_command( "trust", 0, 1, args, 1 );

	args[0] = trust_flag;
	args[1] = fingerprint;

	if (!run_command( "trust", 0, 1, args, (fingerprint != NULL)?2:1 ))
	{
		P4ClientError *e = ConMgr->DefaultConnection()->ui->GetErrorResults();
		if ((e!= NULL) && (e->Severity >= E_FAILED))
		{
			*err = e;
		}

		connecting = 0;
		ConMgr->Initialized = 0;

		return 0;
	}

	// Set the Unicode flag to unknown, to force a retest
	isUnicode = -1;

	apiLevel = -1;
	useLogin = -1;
	supportsExtSubmit = -1;

	if (GetServerProtocols())
	{
		p4debug.SetLevel("-vnet.maxwait=5");

		ConMgr->Initialized = 1;

		connecting = 0;

		return 1;
	}

	P4ClientError *e = ConMgr->DefaultConnection()->ui->GetErrorResults();
	if (e!= NULL)
	{
		*err = e;
	}

	close_connection();

	ConMgr->Initialized = 0;
	connecting = 0;

	return 0;
}

/*******************************************************************************
 *
 * close_connection
 *
 *  Final disconnect from the P4 Server.
 *
 ******************************************************************************/

int P4BridgeServer::close_connection()
{
	LOCK(&Locker); 

	// Close connections
	if (ConMgr)
	{
		if (!ConMgr->Disconnect())
		{
			return 0;
		}

		//delete ConMgr;
		//ConMgr = NULL;
	}
	//ConMgr= new ConnectionManager(this);

	// Set the Unicode flag to unknown, to force a retest
	isUnicode = -1;

	apiLevel = -1;
	useLogin = -1;
	supportsExtSubmit = -1;

	return 1;
}

/*******************************************************************************
 *
 * disconnect
 *
 *  Disconnect from the P4 Server after a command, but save protocols and other
 *	 settings.
 *
 ******************************************************************************/

int P4BridgeServer::disconnect( void )
{
	LOCK(&Locker); 

	if (ConMgr)
	{
		return ConMgr->Disconnect();
	}
	return 1;
}

/*******************************************************************************
 *
 *  get_charset
 *
 * Get the character set from the environment.
 *
 ******************************************************************************/

const StrPtr* P4BridgeServer::get_charset( )
{
	return ConMgr->GetCharset();
}

CharSetApi::CharSet GetDefaultCharSet()
{
    switch (GetACP())
    {
        case 437:   return CharSetApi::WIN_US_OEM;
        case 932:   return CharSetApi::SHIFTJIS;
        case 936:   return CharSetApi::CP936;
        case 949:   return CharSetApi::CP949;
        case 950:   return CharSetApi::CP950;
        case 1200:  return CharSetApi::UTF_16_LE_BOM;
        case 1201:  return CharSetApi::UTF_16_BE_BOM;
        case 1251:  return CharSetApi::WIN_CP_1251;
        case 10000: return CharSetApi::MACOS_ROMAN;
        case 12000: return CharSetApi::UTF_32_LE_BOM;
        case 12001: return CharSetApi::UTF_32_BE_BOM;
        case 20866: return CharSetApi::KOI8_R;
        case 20932: return CharSetApi::EUCJP;
        case 28591: return CharSetApi::ISO8859_1;
        case 28595: return CharSetApi::ISO8859_5;
        case 28605: return CharSetApi::ISO8859_15;
        case 65001: return CharSetApi::UTF_8;
        default:
        case 1252:  return CharSetApi::WIN_US_ANSI;
   }
} 

/*******************************************************************************
 *
 *  set_charset
 *
 * Set the character set for encoding Unicode strings for command parameters 
 *  and output. Optionally, a separate encoding can be specified for the 
 *  contents of files that are directly saved in the client's file system.
 *
 ******************************************************************************/

char * P4BridgeServer::set_charset( const char* c, const char * filec )
{
	CharSetApi::CharSet cs;
	if (c)
	{
		// Lookup the correct enum for the specified character set for the API
		cs = CharSetApi::Lookup( c );
		if( cs < 0 )
		{
			StrBuf	m;
			m = "Unknown or unsupported charset: ";
			m.Append( c );

			LOG_ERROR( m.Text() );
			return m.Text();
		}
	}
	else
	{
		cs = CharSetApi::UTF_8;
		c = CharSetApi::Name(cs);
	}

	CharSetApi::CharSet filecs;

	// Lookup the correct enum for the specified character set for file 
	//  contents
	if (filec)
	{
		filecs = CharSetApi::Lookup( filec );
		if( filecs < 0 )
		{
			StrBuf	m;
			m = "Unknown or unsupported charset: ";
			m.Append( filec );

			LOG_ERROR( m.Text() );
			return m.Text();
		}
	}
	else 
	{
		// default value
		filecs = CharSetApi::WIN_US_ANSI;

		const StrPtr* filec = ConMgr->GetCharset();

		if (filec)
		{
			filecs = CharSetApi::Lookup(filec->Text());

			//if ((int)filecs <= 0)
			//{
			//	// see if there is a p4 config file, and it has a setting for p4cheaset

			//	const StrPtr *cfgFile = get_config();

			//	if (cfgFile != NULL)
			//	{
			//		char* cfgFileName = cfgFile->Text();
			//		if ((cfgFileName != NULL) && (strncmp(cfgFileName,"noconfig", 8)!= 0))
			//		{
			//		}
			//	}
			//}

			if ((int)filecs <= 0)
			{
				// see if there is a p4 config file, and it has a setting for p4cheaset

				//see if there is  asetting for p4 charset

				// not set, get a value based on the system code page
				filecs = GetDefaultCharSet();
			}
		}
	}
	LOG_INFO1( "[P4] Setting charset: %s\n", CharSetApi::Name(filecs) );

	// Set the character set in the untagged client manager

	ConMgr->SetCharset( cs, filecs );

	// Tell the UI to use Unicode
	//UseUnicode(1);

	return NULL;
}

/*******************************************************************************
 *
 * set_cwd
 *
 *  Set the working directory.
 *
 ******************************************************************************/

void P4BridgeServer::set_cwd( const char* newCwd )
{
	if (ConMgr)
	{
		ConMgr->SetCwd( newCwd );
	}
}

/*******************************************************************************
 *
 * get_cwd
 *
 *  Get the working directory.
 *
 ******************************************************************************/

static StrBuf EmptStr("");

const StrPtr&  P4BridgeServer::get_cwd( void )
{
	if (ConMgr)
	{
		P4Connection *pCon = ConMgr->DefaultConnection();
		if (pCon)
		{
			return pCon->GetCwd();
		}
	}
	return EmptStr;
}

void P4BridgeServer::Run_int(P4Connection* client, const char *cmd, P4BridgeClient* ui)
{
	__try
	{
		client->Run(cmd, ui );
	} 
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		if (ui)
		{
			ui->HandleError( E_FATAL, 0, ExceptionError->Text());
		}
	}
}

/*******************************************************************************
 *
 * run_command
 *
 * Run a command using the supplied parameters. The command can either be run 
 *  in tagged or untagged protocol. If the target server supports Unicode, the 
 *  strings in the parameter list need to be encoded in the character set 
 *  specified by a previous call to set_charset().
 *
 ******************************************************************************/

int P4BridgeServer::run_command( const char *cmd, int cmdId, int tagged, char **args, int argc )
{
	//if (cmdId == 2)
	//{
	//	_asm int 3;
	//}
	P4ClientError *err = NULL;
	if( connected( &err ) )
	{
		err = NULL;
	}
	Error e;

	StrBuf msg;

	P4Connection* client = ConMgr->GetConnection(cmdId);
	if(!client)
	{
		LOG_ERROR1( "Error getting connection for command: %d", cmdId );
		return 0;
	}

	P4BridgeClient* ui = client->ui;
	if (ui)
	{
		if (err != NULL)
		{
			// couldn't connect
			ui->HandleError( err );
			return 0;
		}
		ui->clear_results();
	}
	else
	{
		ui = new P4BridgeClient(this, client);
		client->ui = ui;
	}
	client->IsAlive(1);

	// Connect to server
	//client->Init( &e );
	if(client->Dropped())
	{
		client->Final(&e);
		if( e.Test() )
		{
			ui->HandleError(&e);
			return 0;
		}
		client->clientNeedsInit = 1;
	}
	if (client->clientNeedsInit)
	{
		client->Init( &e );
		if( e.Test() )
		{
			ui->HandleError(&e);
			return 0;
		}
		client->clientNeedsInit = 0;
	}

	// Label Connections for p4 monitor
	if (pProgramName)
		client->SetProg( pProgramName );
	else
		client->SetProg( "dot-net-api-p4" );

	if (pProgramVer)
		client->SetVersion( pProgramVer );
	else
		client->SetVersion( "1.0" );

	client->SetVar(P4Tag::v_tag, tagged ? "yes" : 0);

	client->SetArgv( argc, args );
	client->SetBreak(client);

	Run_int(client, cmd, ui);

	//ConMgr->ReleaseConnection(cmdId);

	P4ClientError* errors = ui->GetErrorResults();

	// close the server connection
	//client->Final( &e );

	//if( e.Test() )
	//{
		//e.Fmt( &msg, EF_NEWLINE );
		//err = CopyStr( msg.Text() );
		//LOG_ERROR1( "Error connecting to server: %s", err );
		//ui->HandleError(&e);
	//}

	if (errors != NULL)
	{
		int maxSeverity = errors->MaxSeverity();
		if ( maxSeverity >= 3 )
		{
			return 0;
		}
	}
	if(client->Dropped())
	{
		client->Final(&e);
		if( e.Test() )
		{
			ui->HandleError(&e);
		}
		client->clientNeedsInit = 1;
	}

	return 1;
}

void P4BridgeServer::cancel_command(int cmdId) 
{
	if (ConMgr)
	{
		P4Connection* con = ConMgr->GetConnection(cmdId);
		if (con)
		{
			con->cancel_command();
		}
	}
}
	
void P4BridgeServer::ReleaseConnection(int cmdId)
{
	if (ConMgr)
	{
		ConMgr->ReleaseConnection(cmdId);
	}
}

int P4BridgeServer::GetServerProtocols()
{
	if (isUnicode >= 0)
	{
		// already read the protocols
		return 1;
	}

	// set to 0 for now so we don't call this again when running the help 
	//   command to get the protocols
	isUnicode = 0;

	// running the 'help' command on the server is the only command that 
	//   does not lock any tables on the server, so it has the least impact.

	if (!run_command( "help", 0, 1, NULL, 0 ))
	{
		bool ok = false;
		P4ClientError* error = ConMgr->DefaultConnection()->ui->GetErrorResults();
		while (error)
		{
			//int tc = ErrorOf(13, 334, 0, 0, 0);

			if (error->ErrorCode == ErrorOf(13, 334, 0, 0, 0))
			{
				ok = true;
				break;
			}
			error= error->Next;
		}
		if (!ok)
		{
			isUnicode = -1;
			return 0;
		}
	}

	StrPtr *st = 0;

	if ( st = ConMgr->DefaultConnection()->GetProtocol( P4Tag::v_unicode ) )
	{
		if( st->Length() && st->Atoi() )
		{
			isUnicode = 1;
		}
		else
		{
			isUnicode = 0;
		}
	}

	// Check server level
	st = ConMgr->DefaultConnection()->GetProtocol( "server2" );
	if ( st != NULL ) {
		apiLevel = st->Atoi();

		if (apiLevel == 0)
		{
			// this failed
			isUnicode = -1;
			useLogin = -1;
			supportsExtSubmit = -1;

			return 0;
		}
		// Login/logout capable [2004.2 higher]
		if ( apiLevel >= SERVER_SECURITY_PROTOCOL ) {
			useLogin = 1;
		}
		else
		{
			useLogin = 0;
		}

		// Supports new submit options [2006.2 higher]
		if ( apiLevel >= SERVER_EXTENDED_SUBMIT ) {
			supportsExtSubmit = 1;
		}
		else
		{
			supportsExtSubmit = 0;
		}
	}

	return 1;
}

/*******************************************************************************
 *
 * unicodeServer
 *
 * Does the connected server support unicode? If already determined, return the
 *  cached results, otherwise issue a help command and query the server to see
 *  if Unicode support is enabled.
 *
 ******************************************************************************/

int P4BridgeServer::unicodeServer(  )
{
	GetServerProtocols();
	
	return isUnicode;
}

/*******************************************************************************
 *
 * APILevel
 *
 * The API level the connected server supports If already determined, return the
 *  cached results, otherwise issue a help command and query the server to see 
 *  what protocols the server supports.
 *
 ******************************************************************************/

int P4BridgeServer::APILevel(  )
{
	GetServerProtocols();
	
	return apiLevel;
}

/*******************************************************************************
 *
 * UseLogin
 *
 * Does the connected server require the login command be used? If already 
 *  determined, return the cached results, otherwise issue a help command and 
 *  query the server to see if Unicode support is enabled.
 *
 ******************************************************************************/

int P4BridgeServer::UseLogin()
{
	GetServerProtocols();
	
	return useLogin;
}

//Does the connected sever support extended submit options (2006.2 higher)?
/*******************************************************************************
 *
 * SupportsExtSubmit
 *
 * Does the connected server support extended submit options (2006.2 higher)? 
 *  If already determined, return the cached results, otherwise issue a help 
 *  command and query the server to see if Unicode support is enabled.
 *
 ******************************************************************************/
int P4BridgeServer::SupportsExtSubmit()
{
	GetServerProtocols();
	
	return supportsExtSubmit;
}

/*******************************************************************************
 *
 * SetConnection
 *
 * Set some or all of the parameters used for the connection.
 *
 ******************************************************************************/

void  P4BridgeServer::set_connection(const char* newPort, 
									const char* newUser, 
									const char* newPassword, 
									const char* newClient)
{
	// close the connection to force reconnection with new value(s)
	close_connection();

	if (newPort)
	{
		ConMgr->SetPort( newPort );
	}

	if (newUser)
	{
		ConMgr->SetUser( newUser );
	}

	if (newUser)
	{
		ConMgr->SetPassword( newUser );
	}

	if (newClient)
	{
		ConMgr->SetClient( newClient );
	}
}

/*******************************************************************************
 *
 * set_client
 *
 * Set the workspace used for the connection.
 *
 ******************************************************************************/

void P4BridgeServer::set_client( const char* newVal )
{
	if (ConMgr)
	{
		// close the connection to force reconnection with new value(s)
		close_connection();

		ConMgr->SetClient( newVal );
		//client_tagged->SetClient( newVal );
	}
}

/*******************************************************************************
 *
 * set_user
 *
 * Set the user name used for the connection.
 *
 ******************************************************************************/

void P4BridgeServer::set_user( const char* newVal )
{
	if (ConMgr)
	{
		// close the connection to force reconnection with new value(s)
		close_connection();

		ConMgr->SetUser( newVal );
		//client_tagged->SetUser( newVal );
	}
}

/*******************************************************************************
 *
 * set_port
 *
 * Set the port (hostname:portnumber) used for the connection.
 *
 ******************************************************************************/

void P4BridgeServer::set_port( const char* newVal )
{
	if (ConMgr)
	{
		// close the connection to force reconnection with new value(s)
		close_connection();

		ConMgr->SetPort( newVal);
		//client_tagged->SetPort( newVal );
	}
}

/*******************************************************************************
 *
 * set_password
 *
 * Set the password used for the connection.
 *
 ******************************************************************************/

void P4BridgeServer::set_password( const char* newVal )
{
	if (ConMgr)
	{
		// close the connection to force reconnection with new value(s)
		close_connection();

		ConMgr->SetPassword( newVal );
		//client_tagged->SetPassword( newVal );
	}
}

/*******************************************************************************
 *
 * set_programName
 *
 * Set the program name used for the connection.
 *
 ******************************************************************************/

void P4BridgeServer::set_programName( const char* newVal )
{
	if (pProgramName)
		delete[] pProgramName;

	pProgramName = CopyStr(newVal);
}

/*******************************************************************************
 *
 * set_programVer
 *
 * Set the program version used for the connection.
 *
 ******************************************************************************/

void P4BridgeServer::set_programVer( const char* newVal )
{
	if (pProgramVer)
		delete pProgramVer;

	pProgramVer = CopyStr(newVal);
}

/*******************************************************************************
 *
 * get_client
 *
 *  Get the workspace used for the connection.
 *
 ******************************************************************************/

const StrPtr* P4BridgeServer::get_client()
{
	if (ConMgr)
	{
		return &(ConMgr->DefaultConnection())->GetClient();
	}
	return NULL;
}

/*******************************************************************************
 *
 * get_user
 *
 *  Get the user name used for the connection.
 *
 ******************************************************************************/

const StrPtr* P4BridgeServer::get_user()
{
	if (ConMgr)
	{
		 return &ConMgr->DefaultConnection()->GetUser();
	}
	return NULL;
}

/*******************************************************************************
 *
 * get_port
 *
 *  Get the user port used for the connection.
 *
 ******************************************************************************/

const StrPtr* P4BridgeServer::get_port()
{
	if (ConMgr)
	{
		return &ConMgr->DefaultConnection()->GetPort();
	}
	return NULL;
}

/*******************************************************************************
 *
 * get_password
 *
 *  Get the password used for the connection.
 *
 ******************************************************************************/

const StrPtr* P4BridgeServer::get_password()
{
	if (ConMgr)
	{
		return &ConMgr->DefaultConnection()->GetPassword();
	}
	return NULL;
}

/*******************************************************************************
 *
 * get_programName
 *
 *  Get the program name used for the connection.
 *
 ******************************************************************************/

const char* P4BridgeServer::get_programName()
{
	return pProgramName;
}

/*******************************************************************************
 *
 * get_programVer
 *
 *  Get the program version used for the connection.
 *
 ******************************************************************************/

const char* P4BridgeServer::get_programVer()
{
	return pProgramVer;
}

/*******************************************************************************
 *
 * get_config
 *
 *  Get the config file used for the connection, if any.
 *
 ******************************************************************************/

const StrPtr* P4BridgeServer::get_config()
{
	P4ClientError *err = NULL;
	if( !connected( &err ) )
	{
		return 0;
	}

	if (ConMgr)
	{
		return &ConMgr->DefaultConnection()->GetConfig();
	}

	return NULL;
}

/*******************************************************************************
 *
 *  HandleException
 *
 *  Handle any platform exceptions. The Microsoft Structured Exception Handler
 *      allows software to catch platform exceptions such as array overrun. The
 *      exception is logged, but the application will continue to run.
 *
 ******************************************************************************/

int P4BridgeServer::HandleException(unsigned int c, struct _EXCEPTION_POINTERS *e)
{
	unsigned int code = c;
	struct _EXCEPTION_POINTERS *ep = e;

	// Log the exception
	char * exType = "Unknown";

	switch (code)
	{
	case EXCEPTION_ACCESS_VIOLATION:
		exType = "EXCEPTION_ACCESS_VIOLATION\r\n";
		break;
	case EXCEPTION_DATATYPE_MISALIGNMENT:
		exType = "EXCEPTION_DATATYPE_MISALIGNMENT\r\n";
		break;
	case EXCEPTION_BREAKPOINT:
		exType = "EXCEPTION_BREAKPOINT\r\n";
		break;
	case EXCEPTION_SINGLE_STEP:
		exType = "EXCEPTION_SINGLE_STEP\r\n";
		break;
	case EXCEPTION_ARRAY_BOUNDS_EXCEEDED:
		exType = "EXCEPTION_ARRAY_BOUNDS_EXCEEDED\r\n";
		break;
	case EXCEPTION_FLT_DENORMAL_OPERAND:
		exType = "EXCEPTION_FLT_DENORMAL_OPERAND\r\n";
		break;
	case EXCEPTION_FLT_DIVIDE_BY_ZERO:
		exType = "EXCEPTION_FLT_DIVIDE_BY_ZERO\r\n";
		break;
	case EXCEPTION_FLT_INEXACT_RESULT:
		exType = "EXCEPTION_FLT_INEXACT_RESULT\r\n";
		break;
	case EXCEPTION_FLT_INVALID_OPERATION:
		exType = "EXCEPTION_FLT_INVALID_OPERATION\r\n";
		break;
	case EXCEPTION_FLT_OVERFLOW:
		exType = "EXCEPTION_FLT_OVERFLOW\r\n";
		break;
	case EXCEPTION_FLT_STACK_CHECK:
		exType = "EXCEPTION_FLT_STACK_CHECK\r\n";
		break;
	case EXCEPTION_FLT_UNDERFLOW:
		exType = "EXCEPTION_FLT_UNDERFLOW\r\n";
		break;
	case EXCEPTION_INT_DIVIDE_BY_ZERO:
		exType = "EXCEPTION_INT_DIVIDE_BY_ZERO\r\n";
		break;
	case EXCEPTION_INT_OVERFLOW:
		exType = "EXCEPTION_INT_OVERFLOW\r\n";
		break;
	case EXCEPTION_PRIV_INSTRUCTION:
		exType = "EXCEPTION_PRIV_INSTRUCTION\r\n";
		break;
	case EXCEPTION_IN_PAGE_ERROR:
		exType = "EXCEPTION_IN_PAGE_ERROR\r\n";
		break;
	case EXCEPTION_ILLEGAL_INSTRUCTION:
		exType = "EXCEPTION_ILLEGAL_INSTRUCTION\r\n";
		break;
	case EXCEPTION_NONCONTINUABLE_EXCEPTION:
		exType = "EXCEPTION_NONCONTINUABLE_EXCEPTION\r\n";
		break;
	case EXCEPTION_STACK_OVERFLOW:
		exType = "EXCEPTION_STACK_OVERFLOW\r\n";
		break;
	case EXCEPTION_INVALID_DISPOSITION:
		exType = "EXCEPTION_INVALID_DISPOSITION\r\n";
		break;
	case EXCEPTION_GUARD_PAGE:
		exType = "EXCEPTION_GUARD_PAGE\r\n";
		break;
	case EXCEPTION_INVALID_HANDLE:
		exType = "EXCEPTION_INVALID_HANDLE\r\n";
		break;
	//case EXCEPTION_POSSIBLE_DEADLOCK:
	//    exType = "EXCEPTION_POSSIBLE_DEADLOCK\r\n");
	//    break;
	default:
		printf("UNKOWN EXCEPTION\r\n");
		break;
	}
	if (ExceptionError != NULL)
		delete ExceptionError;
		
	ExceptionError = new StrBuf();

	ExceptionError->Append("Exception Detected in callback function: ");
	ExceptionError->Append(exType);
	ExceptionError->Terminate();

	LOG_ERROR(ExceptionError->Text());
	return EXCEPTION_EXECUTE_HANDLER;
}

char* P4BridgeServer::Get( const char *var )
{
	P4ClientError *err = NULL;
	if( !connected( &err ) )
	{
		return NULL;
	}

	P4Connection* client = ConMgr->DefaultConnection();
	if(!client)
	{
		return 0;
	}
	// Connect to server
	//client->Init( &e );
	int clientNeedsInit = client->clientNeedsInit;
	if (clientNeedsInit)
	{
		Error e;
		client->Init( &e );
		client->clientNeedsInit = 0;
	}
	P4BridgeClient* ui = client->ui;

	if ((ui) && (ui->enviro))
	{
		//__try
		//{
			return ui->enviro->Get( var );
		//}
		//__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
		//{
		//	// sometime the C++ API is in a bad state and the Get call will crash
		//	return NULL;
		//}
	}
	if (clientNeedsInit)
	{
		ConMgr->ReleaseConnection(0);
	}
	return NULL;
}

void P4BridgeServer::Set( const char *var, const char *value )
{
	P4ClientError *err = NULL;
	if( !connected( &err ) )
	{
		return;
	}

	P4BridgeClient* ui = ConMgr->DefaultConnection()->ui;
	
	if ((ui) && (ui->enviro))
	{
		ui->clear_results();

		Error e;
		ui->enviro->Set( var, value, &e );

		if( e.Test() )
		{
			//e.Fmt( &msg, EF_NEWLINE );
			//err = CopyStr( msg.Text() );
			//LOG_ERROR1( "Error connecting to server: %s", err );
			ui->HandleError(&e);
			return;
		}
		if (!value)
		{
			ui->enviro->Reload();
		}
	}
}

/*******************************************************************************
 *
 *  CallTextResultsCallbackFn
 *
 *  Simple wrapper to call the callback function (if it has been set) within a
 *      SEH __try block to catch any platform exception. SEH __try blocks must
 *      be contained in simple functions or you will get Compiler Error C2712,
 *      "cannot use __try in functions that require object unwinding"
 *
 ******************************************************************************/

void P4BridgeServer::CallTextResultsCallbackFn(int cmdId, const char *data)
{
	__try
	{
		if ((cmdId > 0) && (pTextResultsCallbackFn != NULL))
		{
			(*pTextResultsCallbackFn)( cmdId, data );
		}
	}  __except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
}

/*******************************************************************************
 *
 *  CallInfoResultsCallbackFn
 *
 *  Simple wrapper to call the callback function (if it has been set) within a
 *      SEH __try block to catch any platform exception. SEH __try blocks must
 *      be contained in simple functions or you will get Compiler Error C2712,
 *      "cannot use __try in functions that require object unwinding"
 *
 ******************************************************************************/

void P4BridgeServer::CallInfoResultsCallbackFn( int cmdId, char level, const char *data )
{
	__try
	{
		if 	((cmdId > 0) && (pInfoResultsCallbackFn != NULL))
		{
			int nlevel = (int)(level - '0');
			(*pInfoResultsCallbackFn)( cmdId, nlevel, data );
		}
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
}

/*******************************************************************************
 *
 *  CallTaggedOutputCallbackFn
 *
 *  Simple wrapper to call the callback function (if it has been set) within a
 *      SEH __try block to catch any platform exception. SEH __try blocks must
 *      be contained in simple functions or you will get Compiler Error C2712,
 *      "cannot use __try in functions that require object unwinding"
 *
 ******************************************************************************/

void P4BridgeServer::CallTaggedOutputCallbackFn( int cmdId, int objId, const char *pKey, const char * pVal )
{
	__try
	{
		if ((cmdId > 0) && (pTaggedOutputCallbackFn != NULL))
		{
			(*pTaggedOutputCallbackFn)( cmdId, objId, pKey, pVal );
		}
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
}

/*******************************************************************************
 *
 *  CallErrorCallbackFn
 *
 *  Simple wrapper to call the callback function (if it has been set) within a
 *      SEH __try block to catch any platform exception. SEH __try blocks must
 *      be contained in simple functions or you will get Compiler Error C2712,
 *      "cannot use __try in functions that require object unwinding"
 *
 ******************************************************************************/

void P4BridgeServer::CallErrorCallbackFn( int cmdId, int severity, int errorId, const char * errMsg )
{
	__try
	{
		if 	((cmdId > 0) && (pErrorCallbackFn != NULL))
		{
			(*pErrorCallbackFn)( cmdId, severity, errorId, errMsg );
		}
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		// could cause infinite recursion if we keep producing errors 
		//  when reporting errors
		pErrorCallbackFn = NULL;
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
}
/*******************************************************************************
 *
 *  CallErrorCallbackFn
 *
 *  Simple wrapper to call the callback function (if it has been set) within a
 *      SEH __try block to catch any platform exception. SEH __try blocks must
 *      be contained in simple functions or you will get Compiler Error C2712,
 *      "cannot use __try in functions that require object unwinding"
 *
 ******************************************************************************/

void P4BridgeServer::CallBinaryResultsCallbackFn( int cmdId, void * data, int length )
{
	__try
	{
		if ((cmdId > 0) && (pBinaryResultsCallbackFn))
			(*pBinaryResultsCallbackFn)( cmdId, (void *) data, length );
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
}
// Set the call back function to receive the tagged output
void P4BridgeServer::SetTaggedOutputCallbackFn(IntTextTextCallbackFn* pNew)
{
	pTaggedOutputCallbackFn = pNew;
}

// Set the call back function to receive the error output
void P4BridgeServer::SetErrorCallbackFn(IntIntIntTextCallbackFn* pNew)
{
	pErrorCallbackFn = pNew;
}

void P4BridgeServer::Prompt( int cmdId, const StrPtr &msg, StrBuf &rsp, 
			int noEcho, Error *e )
{
	__try
	{
		if ((cmdId > 0) && (pPromptCallbackFn))
		{
			char response[1024];

			(*pPromptCallbackFn)( cmdId, msg.Text(), response, sizeof(response), noEcho);

			rsp.Set(response);
		}
	}  __except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
}

void P4BridgeServer::SetPromptCallbackFn( PromptCallbackFn * pNew)
{
	pPromptCallbackFn = pNew;
}

// Set the call back function to receive the information output
void P4BridgeServer::SetInfoResultsCallbackFn(IntIntTextCallbackFn* pNew)
{
	pInfoResultsCallbackFn = pNew;
}

// Set the call back function to receive the text output
void P4BridgeServer::SetTextResultsCallbackFn(TextCallbackFn* pNew)
{
	pTextResultsCallbackFn = pNew;
}

// Set the call back function to receive the binary output
void P4BridgeServer::SetBinaryResultsCallbackFn(BinaryCallbackFn* pNew)
{
	pBinaryResultsCallbackFn = pNew;
}

// Callbacks for handling interactive resolve
int	P4BridgeServer::Resolve( int cmdId, ClientMerge *m, Error *e )
{
	if (pResolveCallbackFn == NULL)
	{
		return CMS_SKIP;
	}
	P4ClientMerge *merger = new P4ClientMerge(m);
	int result = -1;

	result = Resolve_int( cmdId, merger );

	delete merger;

	if (result == -1)
	{
		return ConMgr->GetConnection(cmdId)->ui->ClientUser::Resolve( m, e );
	}
	return result;
}

int	P4BridgeServer::Resolve( int cmdId, ClientResolveA *r, int preview, Error *e )
{
	if (pResolveACallbackFn == NULL)
	{
		return CMS_SKIP;
	}

	P4ClientResolve *resolver = new P4ClientResolve(r, isUnicode);
	int result = -1;

	result = Resolve_int( cmdId, resolver, preview, e);

	delete resolver;

	if (result == -1)
	{
		return CMS_SKIP;
	}
	return result;
}

void P4BridgeServer::SetResolveCallbackFn(ResolveCallbackFn * pNew)
{
	pResolveCallbackFn = pNew;
}

void P4BridgeServer::SetResolveACallbackFn(ResolveACallbackFn * pNew)
{
	pResolveACallbackFn = pNew;
}

int P4BridgeServer::Resolve_int( int cmdId, P4ClientMerge *merger)
{
	int result = -1;
	__try
	{
		if ((cmdId > 0) && (pResolveCallbackFn != NULL))
		{
			 result = (*pResolveCallbackFn)(cmdId, merger);
		}  
	}
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
	return result;
}

int P4BridgeServer::Resolve_int( int cmdId, P4ClientResolve *resolver, int preview, Error *e)
{
	int result = -1;
	__try
	{
		if ((cmdId > 0) && (pResolveACallbackFn != NULL))
		{
			result = (*pResolveACallbackFn)(cmdId, resolver, preview);
		}
	}  
	__except (HandleException(GetExceptionCode(), GetExceptionInformation()))
	{
		ConMgr->GetConnection(cmdId)->ui->HandleError( E_FATAL, 0, ExceptionError->Text());
	}
	return result;
}
