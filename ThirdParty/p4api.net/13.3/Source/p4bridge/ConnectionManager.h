#pragma once

class P4Connection;
class StrPtr;
class P4BridgeServer;

class ConnectionManager : public DoublyLinkedList
{
private:
	char* client;
	char* user;
	char* port;
	char* password;
	char* programName;
	char* programVer;
	char* cwd;

	CharSetApi::CharSet charset;
	CharSetApi::CharSet file_charset;

	P4BridgeServer *pServer;

	ConnectionManager(void);

public:
	int Initialized;

	ConnectionManager(P4BridgeServer *pserver);

	virtual ~ConnectionManager(void);

	P4Connection* GetConnection(int cmdId);
	void ReleaseConnection(int cmdId);
	P4Connection* NewConnection(int cmdId);
	P4Connection* DefaultConnection();

	// Set the connection data used
	void SetClient( const char* newVal );
	void SetUser( const char* newVal );
	void SetPort( const char* newVal );
	void SetPassword( const char* newVal );
	void SetProgramName( const char* newVal );
	void SetProgramVer( const char* newVal );
	void SetCwd( const char* newCwd );
	void SetCharset( CharSetApi::CharSet c, CharSetApi::CharSet filec );

	void SetConnection(const char* newPort, const char* newUser, const char* newPassword, const char* newClient)
		{SetClient(newClient); SetUser(newUser);SetPort(newPort);SetPassword(newPassword);}

	const StrPtr* GetCharset( );

	int Disconnect( void );
};

