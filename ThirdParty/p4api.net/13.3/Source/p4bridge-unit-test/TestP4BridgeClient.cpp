#include "StdAfx.h"
#include "UnitTestFrameWork.h"
#include "TestP4BridgeClient.h"
#include "..\p4bridge\P4BridgeServer.h"
#include "..\p4bridge\P4BridgeClient.h"
#include "..\p4bridge\ClientManager.h"
#include "..\p4bridge\P4Connection.h"

#include <strtable.h>
#include <strarray.h>

CREATE_TEST_SUITE(TestP4BridgeClient)

TestP4BridgeClient::TestP4BridgeClient(void)
{
    UnitTestSuite::RegisterTest(HandleErrorTest, "HandleErrorTest");
    UnitTestSuite::RegisterTest(OutputInfoTest, "OutputInfoTest");
    UnitTestSuite::RegisterTest(OutputTextTest, "OutputTextTest");
    UnitTestSuite::RegisterTest(OutputBinaryTest, "OutputBinaryTest");
    UnitTestSuite::RegisterTest(OutputStatTest, "OutputStatTest");

    UnitTestSuite::RegisterTest(HandleErrorCallbackTest, "HandleErrorCallbackTest");
    UnitTestSuite::RegisterTest(OutputInfoCallbackTest, "OutputInfoCallbackTest");
    UnitTestSuite::RegisterTest(OutputTextCallbackTest, "OutputTextCallbackTest");
    UnitTestSuite::RegisterTest(OutputBinaryCallbackTest, "OutputBinaryCallbackTest");
    UnitTestSuite::RegisterTest(OutputStatCallbackTest, "OutputStatCallbackTest");

    UnitTestSuite::RegisterTest(PromptCallbackTest, "PromptCallbackTest");
}

TestP4BridgeClient::~TestP4BridgeClient(void)
{
}

bool TestP4BridgeClient::Setup()
{
    return true;
}

bool TestP4BridgeClient::TearDown()
{
    return true;
}

bool TestP4BridgeClient::HandleErrorTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);

	P4Connection* pCon = new P4Connection(NULL, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
    // 
    ui->OutputError( "Ouch" ); // For broken servers

    ui->HandleError( 4, 0, "Failed" );

    P4ClientError * pFirstErr = ui->GetErrorResults();

    ASSERT_NOT_NULL(pFirstErr)

    ASSERT_EQUAL(pFirstErr->Severity, -1)

    ASSERT_STRING_EQUAL(pFirstErr->Message, "Ouch")

    P4ClientError * pNextErr = pFirstErr->Next;

    ASSERT_NOT_NULL(pNextErr)

    ASSERT_EQUAL(pNextErr->Severity, 4)

    ASSERT_STRING_EQUAL(pNextErr->Message, "Failed")

    delete ui;

    return true;
}

bool TestP4BridgeClient::OutputInfoTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);

	P4Connection* pCon = new P4Connection(NULL, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
    // 
    ui->OutputInfo( '0', "Zero" ); 

    ui->OutputInfo( '1', "One" );

    ui->OutputInfo( '2', "Two" );

    StrBuf * pInfo = ui->GetInfoResults();

    ASSERT_NOT_NULL(pInfo)

    char * pText = CopyStr(pInfo->Text());
    // split the string into lines:
    char seps[]   = "\r\n";

    int idx = 0;
    char* lines[4];

    lines[0] = NULL;
    char*token = NULL;
     
    token = strtok( pText, seps ); // C4996
    while( token != NULL )
    {
        // While there are tokens in "string"
        lines[idx++] = token;

        // Get next token: 
        token = strtok( NULL, seps ); // C4996
    }
    // should have gotten three lines
    ASSERT_EQUAL(idx, 3)

    ASSERT_STRING_EQUAL(lines[0], "0:Zero")
    ASSERT_STRING_EQUAL(lines[1], "1:One")
    ASSERT_STRING_EQUAL(lines[2], "2:Two")

    delete[] pText;

	delete ui;

    return true;
}

bool TestP4BridgeClient::OutputTextTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);

	P4Connection* pCon = new P4Connection(NULL, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
    // 
    ui->OutputText( "Zero\n", 5 );

    ui->OutputText( "One\n", 4 );

    ui->OutputText( "Two\n", 4 );

    StrBuf * pStrBuf = ui->GetTextResults();

    ASSERT_NOT_NULL(pStrBuf)

    char * pText = CopyStr(pStrBuf->Text());

    // split the string into lines:
    char seps[]   = "\r\n";

    int idx = 0;
    char* lines[4];

    lines[0] = NULL;
    char*token = NULL;
     
    token = strtok( pText, seps ); // C4996
    while( token != NULL )
    {
        // While there are tokens in "string"
        lines[idx++] = token;

        // Get next token: 
        token = strtok( NULL, seps ); // C4996
    }
    // should have gotten three lines
    ASSERT_EQUAL(idx, 3)

    ASSERT_STRING_EQUAL(lines[0], "Zero")
    ASSERT_STRING_EQUAL(lines[1], "One")
    ASSERT_STRING_EQUAL(lines[2], "Two")

    delete ui;
	delete[] pText;

    return true;
}

bool TestP4BridgeClient::OutputBinaryTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);

	P4Connection* pCon = new P4Connection(NULL, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
  
    char * StringData = "Zer\0\nOne\nTw\0\n";

    ui->OutputBinary( StringData, 5 );

    ui->OutputBinary( (StringData + 5), 4 );

    ui->OutputBinary( (StringData + 9), 4 );

    void * pBinaryData = ui->GetBinaryResults();

    ASSERT_NOT_NULL(pBinaryData)

    for (int idx = 0; idx < 13; idx++)
    {
        ASSERT_EQUAL((((char*)pBinaryData)[idx]), (StringData[idx]))
    }
    delete ui;

    return true;
}

StrBufDict* Objects[2];

bool TestP4BridgeClient::OutputStatTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);

	P4Connection* pCon = new P4Connection(NULL, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);

    StrBufDict * pObj1 = new StrBufDict();

    pObj1->SetVar("one", "1");
    pObj1->SetVar("two", "2");
    pObj1->SetVar("three", "3");
    pObj1->SetVar("many", "???");

    Objects[0] = pObj1;
    ui->OutputStat( pObj1 );
  
    StrBufDict * pObj2 = new StrBufDict();

    pObj2->SetVar("A", "Ey?");
    pObj2->SetVar("B", "Bee");
    pObj2->SetVar("C", "See");

    Objects[1] = pObj2;
    ui->OutputStat( pObj2 );

    StrDictListIterator * pTaggedData = ui->GetTaggedOutput();

    ASSERT_NOT_NULL(pTaggedData)

    StrDictList * curItem = pTaggedData->GetNextItem();

    int objIdx = 0;
    while (curItem != NULL)
    {
        KeyValuePair * curEntry = pTaggedData->GetNextEntry();

        while (curEntry != NULL)
        {

            ASSERT_STRING_EQUAL( curEntry->value, Objects[objIdx]->GetVar(curEntry->key)->Text() ) 

            curEntry = pTaggedData->GetNextEntry();
        }

        objIdx++;
        curItem = pTaggedData->GetNextItem();
    }

    delete pObj1;
    delete pObj2;
	delete pTaggedData;
    delete ui;

    return true;
}

bool bPassedCallbacksTests = true;

void __stdcall ErrorCallbackFn(int cmdId, int severity, int errorId, const char* msg)
{
	if (cmdId != 7)
	{
		bPassedCallbacksTests = false;
		return;
	}

    if ((severity == -1) && (strcmp(msg, "Ouch") == 0 ))
        return; // correct, so no change
    if ((severity == 4) && (strcmp(msg, "Failed") == 0 ))
        return; // correct, so no change

    // not valid

    bPassedCallbacksTests = false;
}

bool TestP4BridgeClient::HandleErrorCallbackTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);
	ConnectionManager* conMgr = new ConnectionManager(pServer);
	pServer->ConMgr = conMgr;
	P4Connection* pCon = new P4Connection(conMgr, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
	pCon->ui = ui;

    // Set the call back function to receive the error output
    pServer->SetErrorCallbackFn(ErrorCallbackFn);
    
    ui->OutputError( "Ouch" ); // For broken servers

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->HandleError( 4, 0, "Failed" );

    ASSERT_TRUE(bPassedCallbacksTests);

    // pass some bad pointer for callback function. should not crash
    // if we do crash, will be picked up in the unit test framework.
    
    pServer->SetErrorCallbackFn((IntIntIntTextCallbackFn *) 0x00);
    
    ui->OutputError( "Ouch" ); // For broken servers

    //still alive
    
    pServer->SetErrorCallbackFn((IntIntIntTextCallbackFn *) 0xFFFFFFFF);
    
    ui->OutputError( "Ouch" ); // For broken servers

    //still alive

    delete ui;

    return true;
}

void __stdcall InfoOutputCallbackFn(int cmdId, int level, const char* msg)
{
	if (cmdId != 7)
	{
		bPassedCallbacksTests = false;
		return;
	}

    if ((level == 0) && (strcmp(msg, "Zero") == 0 ))
        return; // correct, so no change
    if ((level == 1) && (strcmp(msg, "One") == 0 ))
        return; // correct, so no change
    if ((level == 2) && (strcmp(msg, "Two") == 0 ))
        return; // correct, so no change

    // not valid

    bPassedCallbacksTests = false;
}

bool TestP4BridgeClient::OutputInfoCallbackTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);
	ConnectionManager* conMgr = new ConnectionManager(pServer);
	pServer->ConMgr = conMgr;
	P4Connection* pCon = new P4Connection(conMgr, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
	pCon->ui = ui;

    // Set the call back function to receive the error output
    pServer->SetInfoResultsCallbackFn(InfoOutputCallbackFn);

    ui->OutputInfo( '0', "Zero" ); 

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->OutputInfo( '1', "One" );

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->OutputInfo( '2', "Two" );

    ASSERT_TRUE(bPassedCallbacksTests);

    // pass some bad pointer for callback function. should not crash
    // if we do crash, will be picked up in the unit test framework.
    
    pServer->SetInfoResultsCallbackFn((IntIntTextCallbackFn *) 0x00);
    
    ui->OutputInfo( '1', "One" );

    //still alive
    
    pServer->SetInfoResultsCallbackFn((IntIntTextCallbackFn *) 0xFFFFFFFF);
    
    ui->OutputInfo( '1', "One" );

    //still alive

    delete ui;

    return true;
}

void __stdcall TextOutputCallbackFn(int cmdId, const char* msg)
{
	if (cmdId != 7)
	{
		bPassedCallbacksTests = false;
		return;
	}

    if (strcmp(msg, "Zero") == 0 )
        return; // correct, so no change
    if (strcmp(msg, "One") == 0 )
        return; // correct, so no change
    if (strcmp(msg, "Two") == 0 )
        return; // correct, so no change

    // not valid

    bPassedCallbacksTests = false;
}

bool TestP4BridgeClient::OutputTextCallbackTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);
	ConnectionManager* conMgr = new ConnectionManager(pServer);
	pServer->ConMgr = conMgr;
	P4Connection* pCon = new P4Connection(conMgr, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
	pCon->ui = ui;

    // Set the call back function to receive the error output
    pServer->SetTextResultsCallbackFn(TextOutputCallbackFn);

    ui->OutputText( "Zero" , 4 ); 

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->OutputText( "One", 3 );

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->OutputText( "Two", 3 );

    ASSERT_TRUE(bPassedCallbacksTests);

    // pass some bad pointer for callback function. should not crash
    // if we do crash, will be picked up in the unit test framework.
    
    pServer->SetTextResultsCallbackFn((TextCallbackFn *) 0x00);
    
    ui->OutputText( "One", 3 );

    //still alive
    
    pServer->SetTextResultsCallbackFn((TextCallbackFn *) 0xFFFFFFFF);
    
    ui->OutputText( "One", 3 );

    //still alive

    delete ui;

    return true;
}

void __stdcall BinaryResultsCallbackFn( int cmdId, void* msg, int cnt)
{
	if (cmdId != 7)
    {
		bPassedCallbacksTests = false;
		return;
	}
    if ((cnt == 5) && (strncmp((char*) msg, "Zero ", cnt) == 0 ))
        return; // correct, so no change
    if ((cnt == 4) && (strncmp((char*) msg, "One ", cnt) == 0 ))
        return; // correct, so no change
    if ((cnt == 3) && (strncmp((char*) msg, "Two", cnt) == 0 ))
        return; // correct, so no change

    // not valid

    bPassedCallbacksTests = false;
}

bool TestP4BridgeClient::OutputBinaryCallbackTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);
	ConnectionManager* conMgr = new ConnectionManager(pServer);
	pServer->ConMgr = conMgr;
	P4Connection* pCon = new P4Connection(conMgr, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
	pCon->ui = ui;

    pServer->SetBinaryResultsCallbackFn(BinaryResultsCallbackFn);

    ui->OutputBinary( "Zero ", 5 );

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->OutputBinary( "One ", 4 );

    ASSERT_TRUE(bPassedCallbacksTests);

    ui->OutputBinary( "Two", 3 );

    ASSERT_TRUE(bPassedCallbacksTests);

    // pass some bad pointer for callback function. should not crash
    // if we do crash, will be picked up in the unit test framework.
    
    pServer->SetBinaryResultsCallbackFn((BinaryCallbackFn *) 0x00);
    
    ui->OutputBinary( "One ", 4 );

    //still alive
    
    pServer->SetBinaryResultsCallbackFn((BinaryCallbackFn *) 0xFFFFFFFF);
    
    ui->OutputBinary( "One ", 4 );

    //still alive

    delete ui;

    return true;
}

void __stdcall TaggedOutputCallbackFn(int cmdId, int objId, const char* key, const char* val)
{
	if (cmdId != 7)
	{
		bPassedCallbacksTests = false;
		return;
	}

    if (strcmp(val, Objects[objId]->GetVar(key)->Text() ) == 0)
        return; // correct, so no change

    // not valid

    bPassedCallbacksTests = false;
}

bool TestP4BridgeClient::OutputStatCallbackTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);
	ConnectionManager* conMgr = new ConnectionManager(pServer);
	pServer->ConMgr = conMgr;
	P4Connection* pCon = new P4Connection(conMgr, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
	pCon->ui = ui;

    pServer->SetTaggedOutputCallbackFn(TaggedOutputCallbackFn);

    StrBufDict * pObj1 = new StrBufDict();

    pObj1->SetVar("one", "1");
    pObj1->SetVar("two", "2");
    pObj1->SetVar("three", "3");
    pObj1->SetVar("many", "???");

    Objects[0] = pObj1;
    ui->OutputStat( pObj1 );
  
    ASSERT_TRUE(bPassedCallbacksTests);

    StrBufDict * pObj2 = new StrBufDict();

    pObj2->SetVar("A", "Ey?");
    pObj2->SetVar("B", "Bee");
    pObj2->SetVar("C", "See");

    Objects[1] = pObj2;

    ui->OutputStat( pObj2 );
        
    ASSERT_TRUE(bPassedCallbacksTests);

    // pass some bad pointer for callback function. should not crash
    // if we do crash, will be picked up in the unit test framework.
    
    pServer->SetTaggedOutputCallbackFn((IntTextTextCallbackFn *) 0x00);
    
    ui->OutputStat( pObj1 );

    //still alive
    
    pServer->SetTaggedOutputCallbackFn((IntTextTextCallbackFn *) 0xFFFFFFFF);
    
    ui->OutputStat( pObj1 );

    //still alive
    // if the callbacks use __stdcall, this will cause the app to have an 
    //  unhandled exception, as it corrupts the stack and the SEH in Windows 
    //  does not catch it. It will be caught if the callbacks use __cdecl,
    //  unfortunately, we cannot specify the calling convention for a C#
    //  delegate, so the bridge's caller will have to make sure to pass a
    //  function correct stack image to the library
    
    // pass a function with the wrong parameter list
    //ui->SetTaggedOutputCallbackFn((IntTextTextCallbackFn *) BinaryResultsCallbackFn);
    
    //ui->OutputStat( pObj1 );

    //still alive

    delete pObj1;
    delete pObj2;
    delete ui;

    return true;
}

void __stdcall MyPromptCallbackFn(int cmdId, const char * msg, char * rspBuf, 
				int bufsz, int noEcho)
{
	if (cmdId != 7)
	{
		bPassedCallbacksTests = false;
		return;
	}

    if (strcmp(msg, "What's Up?" ) == 0)
    {
		strcpy(rspBuf, "The Sky");
		return; // correct, so no change
	}
    // not valid

    bPassedCallbacksTests = false;
}


bool TestP4BridgeClient::PromptCallbackTest()
{
	P4BridgeServer* pServer = new P4BridgeServer(NULL, NULL, NULL, NULL);
	ConnectionManager* conMgr = new ConnectionManager(pServer);
	pServer->ConMgr = conMgr;
	P4Connection* pCon = new P4Connection(conMgr, pServer, 7);
    P4BridgeClient * ui = new P4BridgeClient(pServer, pCon);
	pCon->ui = ui;

	pServer->SetPromptCallbackFn(MyPromptCallbackFn);

	StrBuf msg("What's Up?");
	StrBuf rsp; 
	int noEcho = 0;
	Error *e = new Error();

    ui->Prompt( msg, rsp, noEcho, e );
  
    ASSERT_TRUE(bPassedCallbacksTests);
	ASSERT_STRING_EQUAL(rsp.Text(), "The Sky");
    ASSERT_TRUE(bPassedCallbacksTests);

    // pass some bad pointer for callback function. should not crash
    // if we do crash, will be picked up in the unit test framework.
    
    pServer->SetPromptCallbackFn((PromptCallbackFn *) 0x00);
    
    ui->Prompt( msg, rsp, noEcho, e );

    //still alive
    
    pServer->SetPromptCallbackFn((PromptCallbackFn *) 0xFFFFFFFF);
    
    ui->Prompt( msg, rsp, noEcho, e );

    //still alive
    // if the callbacks use __stdcall, this will cause the app to have an 
    //  unhandled exception, as it corrupts the stack and the SEH in Windows 
    //  does not catch it. It will be caught if the callbacks use __cdecl,
    //  unfortunately, we cannot specify the calling convention for a C#
    //  delegate, so the bridge's caller will have to make sure to pass a
    //  function correct stack image to the library
    
    // pass a function with the wrong parameter list
    //ui->SetPromptCallbackFn((IntTextTextCallbackFn *) BinaryResultsCallbackFn);
    
    //ui->Prompt( msg, rsp, noEcho, e );


    //still alive

	delete e;

    delete ui;

    return true;
}
