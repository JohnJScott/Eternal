#pragma once
#include "unittestframework.h"
class TestUtils :
    public UnitTestSuite
{
public:
    TestUtils(void);
    ~TestUtils(void);

    DECLARE_TEST_SUITE(TestUtils)

    bool Setup();

    bool TearDown();

    static bool TestCopyStr();
    static bool TestCopyWStr();
    static bool TestAddStr();
    static bool TestCpyStrBuff();
    static bool TestAddStrBuff();
};

