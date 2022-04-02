#pragma once

class ConnectionManager;
class P4BridgeClient;
class P4BridgeServer;

class P4Connection : public ClientApi, public KeepAlive, public DoublyLinkedListItem
{
private:
	P4Connection(void);

	// KeepAlive status
	int	isAlive;
	
public:
	P4Connection(ConnectionManager* conMgr, P4BridgeServer* pServer, int cmdId);
	virtual ~P4Connection(void);
	
	// has the client been initialized
	int clientNeedsInit;

	void Disconnect( void );

	// KeepAlive functionality
	virtual int	IsAlive();
	void IsAlive(int val) {isAlive = val;}

	void cancel_command();

	P4BridgeClient* ui;

	void SetCharset( CharSetApi::CharSet c, CharSetApi::CharSet filec );
};

