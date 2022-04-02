#pragma once

#include "unittestframework.h"

class TestP4BridgeServer :
    public UnitTestSuite
{
public:
    TestP4BridgeServer(void);
    ~TestP4BridgeServer(void);

    DECLARE_TEST_SUITE(TestP4BridgeServer)

    bool Setup();

    bool TearDown();

    static bool ServerConnectionTest();
    static bool TestUnicodeClientToNonUnicodeServer();
    static bool TestUnicodeUserName();
    static bool TestUntaggedCommand();
    static bool TestTaggedCommand();
    static bool TestTextOutCommand();
    static bool TestBinaryOutCommand();
    static bool TestErrorOutCommand();
};

