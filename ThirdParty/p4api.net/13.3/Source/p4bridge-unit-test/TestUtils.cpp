#include "StdAfx.h"
#include "UnitTestFrameWork.h"
#include <string.h>

#include "TestUtils.h"

#include "..\p4bridge\utils.h"

CREATE_TEST_SUITE(TestUtils)

TestUtils::TestUtils(void)
{
    UnitTestSuite::RegisterTest(&TestCopyStr, "TestCopyStr");
    UnitTestSuite::RegisterTest(&TestCopyWStr, "TestCopyWStr");
    UnitTestSuite::RegisterTest(&TestCopyWStr, "TestAddStr");
    UnitTestSuite::RegisterTest(&TestCopyWStr, "TestCpyStrBuff");
    UnitTestSuite::RegisterTest(&TestCopyWStr, "TestAddStrBuff");
}

TestUtils::~TestUtils(void)
{
}

bool TestUtils::Setup()
{
    return true;
}

bool TestUtils::TearDown()
{
    return true;
}

bool TestUtils::TestCopyStr(void)
{
    char * pCopy = CopyStr("12345");

    ASSERT_EQUAL(5,strlen(pCopy));

    ASSERT_EQUAL(0,strcmp(pCopy, "12345"));

	delete[] pCopy;

    return true;
}

bool TestUtils::TestCopyWStr(void)
{
    wchar_t * pCopy = CopyWStr(L"12345");

    ASSERT_EQUAL(5,wcslen(pCopy));

    ASSERT_EQUAL(0,wcscmp(pCopy, L"12345"));

	delete[] pCopy;

	return true;
}

bool TestUtils::TestAddStr(void)
{
    char * pCopy = AddStr("12345", "6789");

    ASSERT_EQUAL(5,strlen(pCopy));

    ASSERT_EQUAL(0,strcmp(pCopy, "123456789"));

	delete[] pCopy;

	return true;
}

bool TestUtils::TestCpyStrBuff(void)
{
    char * pStr = "123\'0'45";
    char * pCopy = CpyStrBuff(pStr, 6);

    for (int i = 0; i < 6; i++)
        ASSERT_EQUAL(pCopy[i], pStr[i]);

	delete[] pCopy;

	return true;
}

bool TestUtils::TestAddStrBuff(void)
{
    char * pStr1 = "123\'0'5";
    char * pStr2 = "678\'0'";
    char * pCopy = AddStrBuff(pStr1, 5, pStr2, 4);
    
    int i = 0;
    for (i = 0; i < 5; i++)
        ASSERT_EQUAL(pCopy[i], pStr1[i]);

    for (i = 0; i < 4; i++)
        ASSERT_EQUAL(pCopy[i+5], pStr2[i]);

	delete[] pCopy;

	return true;
}
