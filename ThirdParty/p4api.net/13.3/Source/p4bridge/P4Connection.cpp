#include "stdafx.h"
#include "P4BridgeClient.h"
#include "P4Connection.h"

P4Connection::P4Connection(ConnectionManager* conMgr, P4BridgeServer* pServer, int cmdId)
	: DoublyLinkedListItem((DoublyLinkedList *)conMgr, cmdId)
{
		clientNeedsInit = 1;

		ui = NULL;
		isAlive = 1;
}

P4Connection::~P4Connection(void)
{
	if (clientNeedsInit == 0)
	{
		Error e;
		this->Final( &e );
		clientNeedsInit = 1;
	}
	if (ui)
	{
		delete ui;
	}
}

void P4Connection::cancel_command() 
{
	isAlive = 0;
}

// KeepAlive functionality
int	P4Connection::IsAlive()
{
	return isAlive;
}

void P4Connection::Disconnect( void )
{
	if (clientNeedsInit == 0)
	{
		Error e;
		this->Final( &e );

		clientNeedsInit = 1;
	}
}

void P4Connection::SetCharset( CharSetApi::CharSet c, CharSetApi::CharSet filec )
{	
	ClientApi::SetCharset(CharSetApi::Name(filec));
	SetTrans( CharSetApi::NOCONV, filec, c, CharSetApi::NOCONV );
}
