#include "StdAfx.h"

#include "P4Connection.h"
#include "P4BridgeClient.h"
#include "P4BridgeServer.h"
#include "ConnectionManager.h"

#define DELETE_OBJECT(obj) if( obj != NULL ) { delete obj; obj = NULL; }

ConnectionManager::ConnectionManager(P4BridgeServer* pserver)
	: DoublyLinkedList(&(pserver->Locker))
{
	Initialized = 0;

	pServer = pserver;

	client = NULL;
	user = NULL;
	port = NULL;
	password = NULL;
	programName = NULL;
	programVer = NULL;
	cwd = NULL;

	charset = (CharSetApi::CharSet) 0;
	file_charset = (CharSetApi::CharSet) 0;
}

ConnectionManager::~ConnectionManager(void)
{
	if (disposed != 0)
	{
		return;
	}
	LOCK(&pServer->Locker); 

	disposed = 1;

	// The P4Connection list will be deleted by the destructor for DoublyLinkedList
	//P4Connection* connection = (P4Connection*)First();

	//while (connection)
	//{
	//	P4Connection* nextConnection = (P4Connection*)connection->Next();
	//	delete connection;
	//	connection = nextConnection;
	//}

	Initialized = 0;

	DELETE_OBJECT(client);
	DELETE_OBJECT(user);
	DELETE_OBJECT(port);
	DELETE_OBJECT(password);
	DELETE_OBJECT(programName);
	DELETE_OBJECT(programVer);
	DELETE_OBJECT(cwd);
}
	
const StrPtr* ConnectionManager::GetCharset( )
{
	return &DefaultConnection()->GetCharset( );
}

int ConnectionManager::Disconnect( void )
{
	LOCK(&pServer->Locker); 

	Initialized = 0;

	P4Connection* connection = (P4Connection*)First();

	while (connection)
	{
		if (connection->Id >=1)
		{
			// this connection is running a command and so it cannot
			// be closed, set initialized to false, so it will reinitialize 
			// for the next command

			//connection->clientNeedsInit = 1;
			//Initialized = 0;

			return 0;
		}
		connection = (P4Connection*)connection->Next();;
	}
	// not running any commands, so discard all connections

	connection = (P4Connection*)First();

	while (connection)
	{
		P4Connection* next = (P4Connection*)connection->Next();
		connection->Disconnect();
		Remove(connection);
		connection = next;
	}
	return 1;
}

P4Connection* ConnectionManager::GetConnection(int cmdId)
{
	LOCK(&pServer->Locker); 

	P4Connection* con = (P4Connection*)Find(cmdId);

	if (con) 
	{
		return con;
	}
	con = (P4Connection*)First();
	while (con)
	{
		if (con->Id < 0)
		{
			//LOG_DEBUG1(4 ,"Using existing Connection for command: %X", cmdId);
			//LOG_DEBUG1(4 ,"Now have %d connections in ConnectionManager", itemCount);

			con->Id = cmdId;
			return con;
		}
		con = (P4Connection*)con->Next();
	}
	return NewConnection(cmdId);
}

void ConnectionManager::ReleaseConnection(int cmdId)
{
	LOCK(&pServer->Locker); 

	P4Connection* con = (P4Connection*)Find(cmdId);

	if (con)
	{
		con->Id = -1;
		if (con != pFirstItem)
		{
			con->Disconnect();
			//Remove(con);
		}
	}
}

P4Connection* ConnectionManager::NewConnection(int cmdId)
{
	//LOG_DEBUG1(4 ,"Creating a new Connection for command: %X", cmdId);
	//LOG_DEBUG1(4 ,"Now have %d connections in ConnectionManager", itemCount);

	LOCK(&pServer->Locker); 

	P4Connection* con = new P4Connection(this, pServer, cmdId);

	con->ui = new P4BridgeClient(pServer, con);

	if (client) con->SetClient(	client);
	if (user) con->SetUser(user);
	if (port) con->SetPort(port);
	if (password) con->SetPassword(password);
	if (programName) con->SetProg(programName);
	if (programVer) con->SetVersion(programVer);
	if (cwd) con->SetCwd(cwd);

	con->SetProtocol( "specstring", "" ); 
	con->SetProtocol( "api", "72" ); // 2006.1 api
	con->SetProtocol( "enableStreams", "" );
	//con->SetProtocol( "wingui", "999" );
	
	// Set the character set for the untagged client
	con->SetCharset(charset, file_charset);

	return con;
}

P4Connection* ConnectionManager::DefaultConnection() 
{ 
	P4Connection* value = (P4Connection*) Find(0);
	if (value) 
	{
		return value;
	}
	return NewConnection(0);
}

void ConnectionManager::SetCharset( CharSetApi::CharSet c, CharSetApi::CharSet filec )
{
	charset = c; 
	file_charset = filec;	
	P4Connection* curCon = (P4Connection*)pFirstItem;
	while (curCon)
	{
		curCon->SetCharset(c, filec);
		curCon = (P4Connection*) curCon->Next();
	}
}

void ConnectionManager::SetCwd( const char* newCwd ) 
{
	DELETE_OBJECT(cwd);

	cwd = CopyStr(newCwd);
	if (!pFirstItem)
	{
		DefaultConnection();
		return;
	}
	P4Connection* curCon = (P4Connection*)pFirstItem;
	while (curCon)
	{
		curCon->SetCwd(newCwd);
		curCon = (P4Connection*) curCon->Next();
	}
}

	void ConnectionManager::SetClient( const char* newVal ) 
	{
		DELETE_OBJECT(client);
		if (newVal) client = CopyStr(newVal);
	}
	void ConnectionManager::SetUser( const char* newVal ) 
	{
		DELETE_OBJECT(user);
		if (newVal) user = CopyStr(newVal);
	}
	void ConnectionManager::SetPort( const char* newVal ) 
	{
		DELETE_OBJECT(port);
		if (newVal) port = CopyStr(newVal);
	}
	void ConnectionManager::SetPassword( const char* newVal ) 
	{
		DELETE_OBJECT(password);
		if (newVal)	password = CopyStr(newVal);
	}
	void ConnectionManager::SetProgramName( const char* newVal ) 
	{
		DELETE_OBJECT(programName);
		if (newVal) programName = CopyStr(newVal);
	}
	void ConnectionManager::SetProgramVer( const char* newVal ) 
	{
		DELETE_OBJECT(programVer);
		if (newVal) programVer = CopyStr(newVal);
	}
