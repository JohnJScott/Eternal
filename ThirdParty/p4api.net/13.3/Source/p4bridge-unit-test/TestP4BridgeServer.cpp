#include "StdAfx.h"
#include "UnitTestFrameWork.h"
#include "TestP4BridgeServer.h"

#include "..\p4bridge\P4BridgeClient.h"
#include "..\p4bridge\P4BridgeServer.h"

#include <strtable.h>
#include <strarray.h>

#include <conio.h>

CREATE_TEST_SUITE(TestP4BridgeServer)

TestP4BridgeServer::TestP4BridgeServer(void)
{
    UnitTestSuite::RegisterTest(ServerConnectionTest, "ServerConnectionTest");
    UnitTestSuite::RegisterTest(TestUnicodeClientToNonUnicodeServer, "TestUnicodeClientToNonUnicodeServer");
    UnitTestSuite::RegisterTest(TestUnicodeUserName, "TestUnicodeUserName");
    UnitTestSuite::RegisterTest(TestUntaggedCommand, "TestUntaggedCommand");
    UnitTestSuite::RegisterTest(TestTaggedCommand, "TestTaggedCommand");
    UnitTestSuite::RegisterTest(TestTextOutCommand, "TestTextOutCommand");
    UnitTestSuite::RegisterTest(TestBinaryOutCommand, "TestBinaryOutCommand");
    UnitTestSuite::RegisterTest(TestErrorOutCommand, "TestErrorOutCommand");
}


TestP4BridgeServer::~TestP4BridgeServer(void)
{
}

char unitTestDir[MAX_PATH];
char unitTestZip[MAX_PATH];
char * TestDir = "c:\\MyTestDir";
char * TestZip = "c:\\MyTestDir\\a.exe";
char * rcp_cmd = "p4d -r C:/MyTestDir -jr checkpoint.1";
char * udb_cmd = "p4d -r C:/MyTestDir -xu";
char * p4d_cmd = "p4d -p6666 -IdUnitTestServer -rC:/MyTestDir";

void * pi = NULL;

bool TestP4BridgeServer::Setup()
{
    // remove the test directory if it exists
    UnitTestSuite::rmDir( TestDir ) ;

    GetCurrentDirectory(sizeof(unitTestDir), unitTestDir);

    strcpy( unitTestZip, unitTestDir);
    strcat( unitTestZip, "\\a.exe");

    if (!CreateDirectory( TestDir, NULL)) return false;

    if (!CopyFile(unitTestZip, TestZip, false)) return false;

    if (!SetCurrentDirectory(TestDir)) return false;

    pi = UnitTestSuite::RunProgram("a", TestDir, true, true);
    if (!pi) 
    {
        SetCurrentDirectory(unitTestDir);
        return false;
    }

    delete pi;

    pi = UnitTestSuite::RunProgram(rcp_cmd, TestDir, true, true);
    if (!pi) 
    {
        SetCurrentDirectory(unitTestDir);
        return false;
    }

    delete pi;

    pi = UnitTestSuite::RunProgram(udb_cmd, TestDir, true, true);
    if (!pi) 
    {
        SetCurrentDirectory(unitTestDir);
        return false;
    }

    delete pi;

    pi = UnitTestSuite::RunProgram(p4d_cmd, TestDir, false, false);
    if (!pi) 
    {
        SetCurrentDirectory(unitTestDir);
        return false;
    }

//    _getch();

    return true;
}

bool TestP4BridgeServer::TearDown()
{
    if (pi)
        UnitTestSuite::EndProcess( (LPPROCESS_INFORMATION) pi );

    SetCurrentDirectory(unitTestDir);

    UnitTestSuite::rmDir( TestDir ) ;

    return true;
}

bool TestP4BridgeServer::ServerConnectionTest()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }

    delete ps;

    return true;
}

bool TestP4BridgeServer::TestUnicodeClientToNonUnicodeServer()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }

    ASSERT_NOT_EQUAL(ps->unicodeServer(), 1);
    ps->set_charset("utf8", "utf16le");

    char* params[1];
    params[0] = "//depot/mycode/*";

    ASSERT_FALSE(ps->run_command("files", 3456, 0, params, 1))

    P4ClientError * out = ps->get_ui(3456)->GetErrorResults();

    ASSERT_STRING_STARTS_WITH(out->Message, "Unicode clients require a unicode enabled server.")
   
    delete ps;

    return true;
}

bool TestP4BridgeServer::TestUnicodeUserName()
{

    P4ClientError** connectionError = NULL;
    // create a new server
    //Aleksey (Alexei) in Cyrillic = "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0" IN utf-8
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "\xD0\x90\xD0\xBB\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0", "pass", "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        printf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }

    ASSERT_FALSE(ps->unicodeServer());
    ps->set_charset("utf8", "utf16le");

    char* params[1];
    params[0] = "//depot/mycode/*";

    ASSERT_FALSE(ps->run_command("files", 7, 0, params, 1))

    P4ClientError * out = ps->get_ui(7)->GetErrorResults();

    ASSERT_STRING_STARTS_WITH(out->Message, "Unicode clients require a unicode enabled server.")

    delete ps;

    return true;
}

bool TestP4BridgeServer::TestUntaggedCommand()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    char* params[1];
    params[0] = "//depot/mycode/*";

    ASSERT_TRUE(ps->run_command("files", 7, 0, params, 1))

    StrBuf * out = ps->get_ui(7)->GetInfoResults();

    ASSERT_STRING_STARTS_WITH(out->Text(), "0://depot/MyCode/")
    delete ps;

    return true;
}

bool TestP4BridgeServer::TestTaggedCommand()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    char* params[1];
    params[0] = "//depot/mycode/*";

    ASSERT_TRUE(ps->run_command("files", 7, 1, params, 1))

    StrDictListIterator * out = ps->get_ui(7)->GetTaggedOutput();

    ASSERT_NOT_NULL(out);

    int itemCnt = 0;
    while (StrDictList * pItem = out->GetNextItem())
    {
        int entryCnt = 0;

        while (KeyValuePair * pEntry = out->GetNextEntry())
        {
            if ((itemCnt == 0) && (strcmp(pEntry->key, "depotFile") == 0))
                ASSERT_STRING_STARTS_WITH(pEntry->value, "//depot/MyCode/")
            if ((itemCnt == 1) && (strcmp(pEntry->key, "depotFile") == 0))
                ASSERT_STRING_STARTS_WITH(pEntry->value, "//depot/MyCode/")
            entryCnt++;
        }
        ASSERT_NOT_EQUAL(entryCnt, 0);
        itemCnt++;
    }
    ASSERT_EQUAL(itemCnt, 3);

	delete out;

    delete ps;

    return true;
}

bool TestP4BridgeServer::TestTextOutCommand()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    char* params[1];
    params[0] = "//depot/MyCode/ReadMe.txt";

    ASSERT_TRUE(ps->run_command("print", 7, 1, params, 1))

    StrBuf * out = ps->get_ui(7)->GetTextResults();

    ASSERT_NOT_NULL(out);

    ASSERT_STRING_EQUAL(out->Text(), "Don't Read This!\n\nIt's Secret!")

	delete ps;

    return true;
}

bool TestP4BridgeServer::TestBinaryOutCommand()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    char* params[1];
    params[0] = "//depot/MyCode/Silly.bmp";

    ASSERT_TRUE(ps->run_command("print", 7, 1, params, 1))

    int cnt = ps->get_ui(7)->GetBinaryResultsCount();

    ASSERT_EQUAL(cnt, 3126)

    void * out = ps->get_ui(7)->GetBinaryResults();

    ASSERT_NOT_NULL(out);
    ASSERT_EQUAL((*(((unsigned char*)out) + 1)), 0x4d)

    delete ps;

    return true;
}

bool TestP4BridgeServer::TestErrorOutCommand()
{
    P4ClientError** connectionError = NULL;
    // create a new server
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");
    ASSERT_NOT_NULL(ps);

    // connect and see if the api returned an error. 
    if( !ps->connected( connectionError ) )
    {
        char buff[256];
        sprintf(buff, "Connection error: %s", *connectionError);
        // Abort if the connect did not succeed
        ASSERT_FAIL(buff);
    }
    char* params[1];
    params[0] = "//depot/MyCode/Billy.bmp";

    // run a command against a nonexistent file
    // Should fail
    ASSERT_FALSE(ps->run_command("rent", 7, 1, params, 1))

    P4ClientError * out = ps->get_ui(7)->GetErrorResults();

    ASSERT_NOT_NULL(out);

    ASSERT_STRING_STARTS_WITH(out->Message, "Unknown command.  Try 'p4 help' for info")
    ASSERT_NULL(out->Next)

    delete ps;

    return true;
}
