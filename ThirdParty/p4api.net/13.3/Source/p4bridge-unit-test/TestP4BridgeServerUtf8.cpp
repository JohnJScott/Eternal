#include "StdAfx.h"਍⌀椀渀挀氀甀搀攀 ∀唀渀椀琀吀攀猀琀䘀爀愀洀攀圀漀爀欀⸀栀∀ഀഀ
#include "TestP4BridgeServerUtf8.h"਍ഀഀ
#include "..\p4bridge\P4BridgeClient.h"਍⌀椀渀挀氀甀搀攀 ∀⸀⸀尀瀀㐀戀爀椀搀最攀尀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀⸀栀∀ഀഀ
਍⌀椀渀挀氀甀搀攀 㰀挀漀渀椀漀⸀栀㸀ഀഀ
਍䌀刀䔀䄀吀䔀开吀䔀匀吀开匀唀䤀吀䔀⠀吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀⤀ഀഀ
਍吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀㨀㨀吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀⠀瘀漀椀搀⤀ഀഀ
{਍    唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀攀最椀猀琀攀爀吀攀猀琀⠀匀攀爀瘀攀爀䌀漀渀渀攀挀琀椀漀渀吀攀猀琀Ⰰ ∀匀攀爀瘀攀爀䌀漀渀渀攀挀琀椀漀渀吀攀猀琀∀⤀㬀ഀഀ
    UnitTestSuite::RegisterTest(TestNonUnicodeClientToUnicodeServer, "TestNonUnicodeClientToUnicodeServer");਍    唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀攀最椀猀琀攀爀吀攀猀琀⠀吀攀猀琀唀渀琀愀最最攀搀䌀漀洀洀愀渀搀Ⰰ ∀吀攀猀琀唀渀琀愀最最攀搀䌀漀洀洀愀渀搀∀⤀㬀ഀഀ
    UnitTestSuite::RegisterTest(TestUnicodeUserName, "TestUnicodeUserName");਍    唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀攀最椀猀琀攀爀吀攀猀琀⠀吀攀猀琀吀愀最最攀搀䌀漀洀洀愀渀搀Ⰰ ∀吀攀猀琀吀愀最最攀搀䌀漀洀洀愀渀搀∀⤀㬀ഀഀ
    UnitTestSuite::RegisterTest(TestTextOutCommand, "TestTextOutCommand");਍    唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀攀最椀猀琀攀爀吀攀猀琀⠀吀攀猀琀䈀椀渀愀爀礀伀甀琀䌀漀洀洀愀渀搀Ⰰ ∀吀攀猀琀䈀椀渀愀爀礀伀甀琀䌀漀洀洀愀渀搀∀⤀㬀ഀഀ
    UnitTestSuite::RegisterTest(TestErrorOutCommand, "TestErrorOutCommand");਍紀ഀഀ
਍ഀഀ
TestP4BridgeServerUtf8::~TestP4BridgeServerUtf8(void)਍笀ഀഀ
}਍ഀഀ
char unitTestDir8[MAX_PATH];਍挀栀愀爀 甀渀椀琀吀攀猀琀娀椀瀀㠀嬀䴀䄀堀开倀䄀吀䠀崀㬀ഀഀ
char * TestDir8 = "c:\\MyTestDir";਍挀栀愀爀 ⨀ 吀攀猀琀娀椀瀀㠀 㴀 ∀挀㨀尀尀䴀礀吀攀猀琀䐀椀爀尀尀甀⸀攀砀攀∀㬀ഀഀ
char * rcp_cmd8 = "p4d -r C:/MyTestDir -jr checkpoint.1";਍挀栀愀爀 ⨀ 甀搀戀开挀洀搀㠀 㴀 ∀瀀㐀搀 ⴀ爀 䌀㨀⼀䴀礀吀攀猀琀䐀椀爀 ⴀ砀甀∀㬀ഀഀ
char * p4d_cmd8 = "p4d -p6666 -IdUnitTestServer -rC:/MyTestDir";਍⼀⼀挀栀愀爀 ⨀ 瀀㐀搀开砀椀开挀洀搀㠀 㴀 ∀瀀㐀搀 ⴀ砀椀∀㬀ഀഀ
਍瘀漀椀搀 ⨀ 瀀椀㠀 㴀 一唀䰀䰀㬀ഀഀ
਍戀漀漀氀 吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀㨀㨀匀攀琀甀瀀⠀⤀ഀഀ
{਍    ⼀⼀ 爀攀洀漀瘀攀 琀栀攀 琀攀猀琀 搀椀爀攀挀琀漀爀礀 椀昀 椀琀 攀砀椀猀琀猀ഀഀ
    UnitTestSuite::rmDir( TestDir8 ) ;਍ഀഀ
    GetCurrentDirectory(sizeof(unitTestDir8), unitTestDir8);਍ഀഀ
    strcpy( unitTestZip8, unitTestDir8);਍    猀琀爀挀愀琀⠀ 甀渀椀琀吀攀猀琀娀椀瀀㠀Ⰰ ∀尀尀甀⸀攀砀攀∀⤀㬀ഀഀ
਍    椀昀 ⠀℀䌀爀攀愀琀攀䐀椀爀攀挀琀漀爀礀⠀ 吀攀猀琀䐀椀爀㠀Ⰰ 一唀䰀䰀⤀⤀ 爀攀琀甀爀渀 昀愀氀猀攀㬀ഀഀ
਍    椀昀 ⠀℀䌀漀瀀礀䘀椀氀攀⠀甀渀椀琀吀攀猀琀娀椀瀀㠀Ⰰ 吀攀猀琀娀椀瀀㠀Ⰰ 昀愀氀猀攀⤀⤀ 爀攀琀甀爀渀 昀愀氀猀攀㬀ഀഀ
਍    椀昀 ⠀℀匀攀琀䌀甀爀爀攀渀琀䐀椀爀攀挀琀漀爀礀⠀吀攀猀琀䐀椀爀㠀⤀⤀ 爀攀琀甀爀渀 昀愀氀猀攀㬀ഀഀ
਍    瀀椀㠀㴀 唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀甀渀倀爀漀最爀愀洀⠀∀甀∀Ⰰ 吀攀猀琀䐀椀爀㠀Ⰰ 琀爀甀攀Ⰰ 琀爀甀攀⤀㬀ഀഀ
    if (!pi8) ਍ऀ笀ഀഀ
		SetCurrentDirectory(unitTestDir8);਍ऀऀ爀攀琀甀爀渀 昀愀氀猀攀㬀ഀഀ
	}਍ഀഀ
    delete pi8;਍ഀഀ
    pi8 = UnitTestSuite::RunProgram(rcp_cmd8, TestDir8, true, true);਍    椀昀 ⠀℀瀀椀㠀⤀ ഀഀ
	{਍ऀऀ匀攀琀䌀甀爀爀攀渀琀䐀椀爀攀挀琀漀爀礀⠀甀渀椀琀吀攀猀琀䐀椀爀㠀⤀㬀ഀഀ
		return false;਍ऀ紀ഀഀ
਍    搀攀氀攀琀攀 瀀椀㠀㬀ഀഀ
਍    瀀椀㠀 㴀 唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀甀渀倀爀漀最爀愀洀⠀甀搀戀开挀洀搀㠀Ⰰ 吀攀猀琀䐀椀爀㠀Ⰰ 琀爀甀攀Ⰰ 琀爀甀攀⤀㬀ഀഀ
    if (!pi8) ਍ऀ笀ഀഀ
		SetCurrentDirectory(unitTestDir8);਍ऀऀ爀攀琀甀爀渀 昀愀氀猀攀㬀ഀഀ
	}਍ഀഀ
    delete pi8;਍ഀഀ
    //server deployed by u.ex is already in Unicode mode਍    ⼀⼀瀀椀㠀 㴀 唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀刀甀渀倀爀漀最爀愀洀⠀瀀㐀搀开砀椀开挀洀搀㠀Ⰰ 吀攀猀琀䐀椀爀㠀Ⰰ 昀愀氀猀攀Ⰰ 琀爀甀攀⤀㬀ഀഀ
    //if (!pi8) return false;਍ഀഀ
    //delete pi8;਍ഀഀ
    pi8 = UnitTestSuite::RunProgram(p4d_cmd8, TestDir8, false, false);਍    椀昀 ⠀℀瀀椀㠀⤀ ഀഀ
	{਍ऀऀ匀攀琀䌀甀爀爀攀渀琀䐀椀爀攀挀琀漀爀礀⠀甀渀椀琀吀攀猀琀䐀椀爀㠀⤀㬀ഀഀ
		return false;਍ऀ紀ഀഀ
਍⼀⼀    开最攀琀挀栀⠀⤀㬀ഀഀ
਍    爀攀琀甀爀渀 琀爀甀攀㬀ഀഀ
}਍ഀഀ
bool TestP4BridgeServerUtf8::TearDown()਍笀ഀഀ
    if (pi8)਍        唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀䔀渀搀倀爀漀挀攀猀猀⠀ ⠀䰀倀倀刀伀䌀䔀匀匀开䤀一䘀伀刀䴀䄀吀䤀伀一⤀ 瀀椀㠀 ⤀㬀ഀഀ
਍    匀攀琀䌀甀爀爀攀渀琀䐀椀爀攀挀琀漀爀礀⠀甀渀椀琀吀攀猀琀䐀椀爀㠀⤀㬀ഀഀ
਍    唀渀椀琀吀攀猀琀匀甀椀琀攀㨀㨀爀洀䐀椀爀⠀ 吀攀猀琀䐀椀爀㠀 ⤀ 㬀ഀഀ
਍    爀攀琀甀爀渀 琀爀甀攀㬀ഀഀ
}਍ഀഀ
bool TestP4BridgeServerUtf8::ServerConnectionTest()਍笀ഀഀ
    P4ClientError** connectionError = NULL;਍    ⼀⼀ 挀爀攀愀琀攀 愀 渀攀眀 猀攀爀瘀攀爀ഀഀ
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "");਍    䄀匀匀䔀刀吀开一伀吀开一唀䰀䰀⠀瀀猀⤀㬀ഀഀ
਍    ⼀⼀ 挀漀渀渀攀挀琀 愀渀搀 猀攀攀 椀昀 琀栀攀 愀瀀椀 爀攀琀甀爀渀攀搀 愀渀 攀爀爀漀爀⸀ ഀഀ
    if( !ps->connected( connectionError ) )਍    笀ഀഀ
        char buff[256];਍        猀瀀爀椀渀琀昀⠀戀甀昀昀Ⰰ ∀䌀漀渀渀攀挀琀椀漀渀 攀爀爀漀爀㨀 ─猀∀Ⰰ ⨀挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀⤀㬀ഀഀ
        // Abort if the connect did not succeed਍        䄀匀匀䔀刀吀开䘀䄀䤀䰀⠀戀甀昀昀⤀㬀ഀഀ
    }਍ഀഀ
    ASSERT_TRUE(ps->unicodeServer());਍    瀀猀ⴀ㸀猀攀琀开挀栀愀爀猀攀琀⠀∀甀琀昀㠀∀Ⰰ ∀甀琀昀㄀㘀氀攀∀⤀㬀ഀഀ
਍    搀攀氀攀琀攀 瀀猀㬀ഀഀ
਍    爀攀琀甀爀渀 琀爀甀攀㬀ഀഀ
}਍ഀഀ
bool TestP4BridgeServerUtf8::TestNonUnicodeClientToUnicodeServer()਍笀ഀഀ
    P4ClientError** connectionError = NULL;਍    ⼀⼀ 挀爀攀愀琀攀 愀 渀攀眀 猀攀爀瘀攀爀ഀഀ
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");਍    䄀匀匀䔀刀吀开一伀吀开一唀䰀䰀⠀瀀猀⤀㬀ഀഀ
਍    ⼀⼀ 挀漀渀渀攀挀琀 愀渀搀 猀攀攀 椀昀 琀栀攀 愀瀀椀 爀攀琀甀爀渀攀搀 愀渀 攀爀爀漀爀⸀ ഀഀ
    if( !ps->connected( connectionError ) )਍    笀ഀഀ
        char buff[256];਍        猀瀀爀椀渀琀昀⠀戀甀昀昀Ⰰ ∀䌀漀渀渀攀挀琀椀漀渀 攀爀爀漀爀㨀 ─猀∀Ⰰ ⨀挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀⤀㬀ഀഀ
        // Abort if the connect did not succeed਍        䄀匀匀䔀刀吀开䘀䄀䤀䰀⠀戀甀昀昀⤀㬀ഀഀ
    }਍ഀഀ
    ਍ऀ䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀甀渀椀挀漀搀攀匀攀爀瘀攀爀⠀⤀⤀㬀ഀഀ
਍    挀栀愀爀⨀ 瀀愀爀愀洀猀嬀㄀崀㬀ഀഀ
    params[0] = "//depot/mycode/*";਍ഀഀ
    ASSERT_FALSE(ps->run_command("files", 5, 0, params, 1))਍ഀഀ
    P4ClientError * out = ps->get_ui(5)->GetErrorResults();਍ഀഀ
    ASSERT_STRING_STARTS_WITH(out->Message, "Unicode server permits only unicode enabled clients.")਍   ഀഀ
    delete ps;਍ഀഀ
    return true;਍紀ഀഀ
਍戀漀漀氀 吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀㨀㨀吀攀猀琀唀渀琀愀最最攀搀䌀漀洀洀愀渀搀⠀⤀ഀഀ
{਍    倀㐀䌀氀椀攀渀琀䔀爀爀漀爀⨀⨀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 㴀 一唀䰀䰀㬀ഀഀ
    // create a new server਍    倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀 ⨀ 瀀猀 㴀 渀攀眀 倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀⠀∀氀漀挀愀氀栀漀猀琀㨀㘀㘀㘀㘀∀Ⰰ ∀愀搀洀椀渀∀Ⰰ ∀∀Ⰰ ∀愀搀洀椀渀开猀瀀愀挀攀∀⤀㬀ഀഀ
    ASSERT_NOT_NULL(ps);਍ഀഀ
    // connect and see if the api returned an error. ਍    椀昀⠀ ℀瀀猀ⴀ㸀挀漀渀渀攀挀琀攀搀⠀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 ⤀ ⤀ഀഀ
    {਍        挀栀愀爀 戀甀昀昀嬀㈀㔀㘀崀㬀ഀഀ
        sprintf(buff, "Connection error: %s", *connectionError);਍        ⼀⼀ 䄀戀漀爀琀 椀昀 琀栀攀 挀漀渀渀攀挀琀 搀椀搀 渀漀琀 猀甀挀挀攀攀搀ഀഀ
        ASSERT_FAIL(buff);਍    紀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀甀渀椀挀漀搀攀匀攀爀瘀攀爀⠀⤀⤀㬀ഀഀ
    ps->set_charset("utf8", "utf16le");਍ഀഀ
    char* params[1];਍    瀀愀爀愀洀猀嬀　崀 㴀 ∀⼀⼀搀攀瀀漀琀⼀洀礀挀漀搀攀⼀⨀∀㬀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀爀甀渀开挀漀洀洀愀渀搀⠀∀昀椀氀攀猀∀Ⰰ 㜀Ⰰ 　Ⰰ 瀀愀爀愀洀猀Ⰰ ㄀⤀⤀ഀഀ
਍    匀琀爀䈀甀昀 ⨀ 漀甀琀 㴀 瀀猀ⴀ㸀最攀琀开甀椀⠀㜀⤀ⴀ㸀䜀攀琀䤀渀昀漀刀攀猀甀氀琀猀⠀⤀㬀ഀഀ
਍    䄀匀匀䔀刀吀开匀吀刀䤀一䜀开䔀儀唀䄀䰀⠀漀甀琀ⴀ㸀吀攀砀琀⠀⤀Ⰰ ∀　㨀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀刀攀愀搀䴀攀⸀琀砀琀⌀㄀ ⴀ 愀搀搀 挀栀愀渀最攀 ㄀ ⠀琀攀砀琀⤀尀爀尀渀　㨀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀匀椀氀氀礀⸀戀洀瀀⌀㄀ ⴀ 愀搀搀 挀栀愀渀最攀 ㄀ ⠀戀椀渀愀爀礀⤀尀爀尀渀　㨀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀尀砀䐀　尀砀㤀䘀尀砀䐀㄀尀砀㠀䔀尀砀䐀　尀砀䈀䘀⸀琀砀琀⌀㄀ ⴀ 愀搀搀 挀栀愀渀最攀 ㌀ ⠀甀琀昀㄀㘀⤀尀爀尀渀∀⤀ഀഀ
਍    搀攀氀攀琀攀 瀀猀㬀ഀഀ
਍    爀攀琀甀爀渀 琀爀甀攀㬀ഀഀ
}਍ഀഀ
bool TestP4BridgeServerUtf8::TestUnicodeUserName()਍笀ഀഀ
    P4ClientError** connectionError = NULL;਍    ⼀⼀ 挀爀攀愀琀攀 愀 渀攀眀 猀攀爀瘀攀爀ഀഀ
    //Алексей = "\xD0\x90\xD0\xbb\xD0\xB5\xD0\xBA\xD1\x81\xD0\xB5\xD0\xB9\0" IN utf-8਍    倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀 ⨀ 瀀猀 㴀 渀攀眀 倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀⠀∀氀漀挀愀氀栀漀猀琀㨀㘀㘀㘀㘀∀Ⰰ ∀尀砀䐀　尀砀㤀　尀砀䐀　尀砀䈀䈀尀砀䐀　尀砀䈀㔀尀砀䐀　尀砀䈀䄀尀砀䐀㄀尀砀㠀㄀尀砀䐀　尀砀䈀㔀尀砀䐀　尀砀䈀㤀尀　∀Ⰰ ∀瀀愀猀猀∀Ⰰ ∀尀砀䐀　尀砀㤀　尀砀䐀　尀砀戀戀尀砀䐀　尀砀䈀㔀尀砀䐀　尀砀䈀䄀尀砀䐀㄀尀砀㠀㄀尀砀䐀　尀砀䈀㔀尀砀䐀　尀砀䈀㤀尀　∀⤀㬀ഀഀ
    ASSERT_NOT_NULL(ps);਍ഀഀ
    // connect and see if the api returned an error. ਍    椀昀⠀ ℀瀀猀ⴀ㸀挀漀渀渀攀挀琀攀搀⠀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 ⤀ ⤀ഀഀ
    {਍        挀栀愀爀 戀甀昀昀嬀㈀㔀㘀崀㬀ഀഀ
        sprintf(buff, "Connection error: %s", *connectionError);਍        ⼀⼀ 䄀戀漀爀琀 椀昀 琀栀攀 挀漀渀渀攀挀琀 搀椀搀 渀漀琀 猀甀挀挀攀攀搀ഀഀ
        ASSERT_FAIL(buff);਍    紀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀甀渀椀挀漀搀攀匀攀爀瘀攀爀⠀⤀⤀㬀ഀഀ
    ps->set_charset("utf8", "utf16le");਍ഀഀ
    char* params[1];਍    瀀愀爀愀洀猀嬀　崀 㴀 ∀⼀⼀搀攀瀀漀琀⼀洀礀挀漀搀攀⼀⨀∀㬀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀爀甀渀开挀漀洀洀愀渀搀⠀∀昀椀氀攀猀∀Ⰰ 㜀Ⰰ 　Ⰰ 瀀愀爀愀洀猀Ⰰ ㄀⤀⤀ഀഀ
਍    匀琀爀䈀甀昀 ⨀ 漀甀琀 㴀 瀀猀ⴀ㸀最攀琀开甀椀⠀㜀⤀ⴀ㸀䜀攀琀䤀渀昀漀刀攀猀甀氀琀猀⠀⤀㬀ഀഀ
਍    䄀匀匀䔀刀吀开匀吀刀䤀一䜀开䔀儀唀䄀䰀⠀漀甀琀ⴀ㸀吀攀砀琀⠀⤀Ⰰ ∀　㨀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀刀攀愀搀䴀攀⸀琀砀琀⌀㄀ ⴀ 愀搀搀 挀栀愀渀最攀 ㄀ ⠀琀攀砀琀⤀尀爀尀渀　㨀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀匀椀氀氀礀⸀戀洀瀀⌀㄀ ⴀ 愀搀搀 挀栀愀渀最攀 ㄀ ⠀戀椀渀愀爀礀⤀尀爀尀渀　㨀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀尀砀䐀　尀砀㤀䘀尀砀䐀㄀尀砀㠀䔀尀砀䐀　尀砀䈀䘀⸀琀砀琀⌀㄀ ⴀ 愀搀搀 挀栀愀渀最攀 ㌀ ⠀甀琀昀㄀㘀⤀尀爀尀渀∀⤀ഀഀ
਍    搀攀氀攀琀攀 瀀猀㬀ഀഀ
਍    爀攀琀甀爀渀 琀爀甀攀㬀ഀഀ
}਍ഀഀ
bool TestP4BridgeServerUtf8::TestTaggedCommand()਍笀ഀഀ
    P4ClientError** connectionError = NULL;਍    ⼀⼀ 挀爀攀愀琀攀 愀 渀攀眀 猀攀爀瘀攀爀ഀഀ
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");਍    䄀匀匀䔀刀吀开一伀吀开一唀䰀䰀⠀瀀猀⤀㬀ഀഀ
਍    ⼀⼀ 挀漀渀渀攀挀琀 愀渀搀 猀攀攀 椀昀 琀栀攀 愀瀀椀 爀攀琀甀爀渀攀搀 愀渀 攀爀爀漀爀⸀ ഀഀ
    if( !ps->connected( connectionError ) )਍    笀ഀഀ
        char buff[256];਍        猀瀀爀椀渀琀昀⠀戀甀昀昀Ⰰ ∀䌀漀渀渀攀挀琀椀漀渀 攀爀爀漀爀㨀 ─猀∀Ⰰ ⨀挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀⤀㬀ഀഀ
        // Abort if the connect did not succeed਍        䄀匀匀䔀刀吀开䘀䄀䤀䰀⠀戀甀昀昀⤀㬀ഀഀ
    }਍ഀഀ
    ASSERT_TRUE(ps->unicodeServer());਍    瀀猀ⴀ㸀猀攀琀开挀栀愀爀猀攀琀⠀∀甀琀昀㠀∀Ⰰ ∀甀琀昀㄀㘀氀攀∀⤀㬀ഀഀ
਍    挀栀愀爀⨀ 瀀愀爀愀洀猀嬀㄀崀㬀ഀഀ
    params[0] = "//depot/mycode/*";਍ഀഀ
    ASSERT_TRUE(ps->run_command("files", 7, 1, params, 1))਍ഀഀ
    StrDictListIterator * out = ps->get_ui(7)->GetTaggedOutput();਍ഀഀ
    ASSERT_NOT_NULL(out);਍ഀഀ
    int itemCnt = 0;਍    眀栀椀氀攀 ⠀匀琀爀䐀椀挀琀䰀椀猀琀 ⨀ 瀀䤀琀攀洀 㴀 漀甀琀ⴀ㸀䜀攀琀一攀砀琀䤀琀攀洀⠀⤀⤀ഀഀ
    {਍        椀渀琀 攀渀琀爀礀䌀渀琀 㴀 　㬀ഀഀ
਍        眀栀椀氀攀 ⠀䬀攀礀嘀愀氀甀攀倀愀椀爀 ⨀ 瀀䔀渀琀爀礀 㴀 漀甀琀ⴀ㸀䜀攀琀一攀砀琀䔀渀琀爀礀⠀⤀⤀ഀഀ
        {਍            椀昀 ⠀⠀椀琀攀洀䌀渀琀 㴀㴀 　⤀ ☀☀ ⠀猀琀爀挀洀瀀⠀瀀䔀渀琀爀礀ⴀ㸀欀攀礀Ⰰ ∀搀攀瀀漀琀䘀椀氀攀∀⤀ 㴀㴀 　⤀⤀ഀഀ
                ASSERT_STRING_EQUAL(pEntry->value, "//depot/MyCode/ReadMe.txt")਍            椀昀 ⠀⠀椀琀攀洀䌀渀琀 㴀㴀 ㄀⤀ ☀☀ ⠀猀琀爀挀洀瀀⠀瀀䔀渀琀爀礀ⴀ㸀欀攀礀Ⰰ ∀搀攀瀀漀琀䘀椀氀攀∀⤀ 㴀㴀 　⤀⤀ഀഀ
                ASSERT_STRING_EQUAL(pEntry->value, "//depot/MyCode/Silly.bmp")਍            椀昀 ⠀⠀椀琀攀洀䌀渀琀 㴀㴀 ㈀⤀ ☀☀ ⠀猀琀爀挀洀瀀⠀瀀䔀渀琀爀礀ⴀ㸀欀攀礀Ⰰ ∀搀攀瀀漀琀䘀椀氀攀∀⤀ 㴀㴀 　⤀⤀ഀഀ
                ASSERT_STRING_EQUAL(pEntry->value, "//depot/MyCode/\xD0\x9F\xD1\x8E\xD0\xBF.txt")਍            攀渀琀爀礀䌀渀琀⬀⬀㬀ഀഀ
        }਍        䄀匀匀䔀刀吀开一伀吀开䔀儀唀䄀䰀⠀攀渀琀爀礀䌀渀琀Ⰰ 　⤀㬀ഀഀ
        itemCnt++;਍    紀ഀഀ
    ASSERT_EQUAL(itemCnt, 3);਍ഀഀ
    delete out;਍ഀഀ
	delete ps;਍ഀഀ
਍    爀攀琀甀爀渀 琀爀甀攀㬀ഀഀ
}਍ഀഀ
bool TestP4BridgeServerUtf8::TestTextOutCommand()਍笀ഀഀ
    P4ClientError** connectionError = NULL;਍    ⼀⼀ 挀爀攀愀琀攀 愀 渀攀眀 猀攀爀瘀攀爀ഀഀ
    P4BridgeServer * ps = new P4BridgeServer("localhost:6666", "admin", "", "admin_space");਍    䄀匀匀䔀刀吀开一伀吀开一唀䰀䰀⠀瀀猀⤀㬀ഀഀ
਍    ⼀⼀ 挀漀渀渀攀挀琀 愀渀搀 猀攀攀 椀昀 琀栀攀 愀瀀椀 爀攀琀甀爀渀攀搀 愀渀 攀爀爀漀爀⸀ ഀഀ
    if( !ps->connected( connectionError ) )਍    笀ഀഀ
        char buff[256];਍        猀瀀爀椀渀琀昀⠀戀甀昀昀Ⰰ ∀䌀漀渀渀攀挀琀椀漀渀 攀爀爀漀爀㨀 ─猀∀Ⰰ ⨀挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀⤀㬀ഀഀ
        // Abort if the connect did not succeed਍        䄀匀匀䔀刀吀开䘀䄀䤀䰀⠀戀甀昀昀⤀㬀ഀഀ
    }਍ഀഀ
    ASSERT_TRUE(ps->unicodeServer());਍    瀀猀ⴀ㸀猀攀琀开挀栀愀爀猀攀琀⠀∀甀琀昀㠀∀Ⰰ ∀甀琀昀㄀㘀氀攀∀⤀㬀ഀഀ
਍    挀栀愀爀⨀ 瀀愀爀愀洀猀嬀㄀崀㬀ഀഀ
    params[0] = "//depot/MyCode/ReadMe.txt";਍ഀഀ
    ASSERT_TRUE(ps->run_command("print", 7, 1, params, 1))਍ഀഀ
    StrBuf * out = ps->get_ui(7)->GetTextResults();਍ഀഀ
    ASSERT_NOT_NULL(out);਍ഀഀ
    ASSERT_STRING_EQUAL(out->Text(), "Don't Read This!\n\nIt's Secret!")਍ഀഀ
    delete ps;਍ഀഀ
    return true;਍紀ഀഀ
਍戀漀漀氀 吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀㨀㨀吀攀猀琀䈀椀渀愀爀礀伀甀琀䌀漀洀洀愀渀搀⠀⤀ഀഀ
{਍    倀㐀䌀氀椀攀渀琀䔀爀爀漀爀⨀⨀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 㴀 一唀䰀䰀㬀ഀഀ
    // create a new server਍    倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀 ⨀ 瀀猀 㴀 渀攀眀 倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀⠀∀氀漀挀愀氀栀漀猀琀㨀㘀㘀㘀㘀∀Ⰰ ∀愀搀洀椀渀∀Ⰰ ∀∀Ⰰ ∀愀搀洀椀渀开猀瀀愀挀攀∀⤀㬀ഀഀ
    ASSERT_NOT_NULL(ps);਍ഀഀ
    // connect and see if the api returned an error. ਍    椀昀⠀ ℀瀀猀ⴀ㸀挀漀渀渀攀挀琀攀搀⠀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 ⤀ ⤀ഀഀ
    {਍        挀栀愀爀 戀甀昀昀嬀㈀㔀㘀崀㬀ഀഀ
        sprintf(buff, "Connection error: %s", *connectionError);਍        ⼀⼀ 䄀戀漀爀琀 椀昀 琀栀攀 挀漀渀渀攀挀琀 搀椀搀 渀漀琀 猀甀挀挀攀攀搀ഀഀ
        ASSERT_FAIL(buff);਍    紀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀甀渀椀挀漀搀攀匀攀爀瘀攀爀⠀⤀⤀㬀ഀഀ
    ps->set_charset("utf8", "utf16le");਍ഀഀ
    char* params[1];਍    瀀愀爀愀洀猀嬀　崀 㴀 ∀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀匀椀氀氀礀⸀戀洀瀀∀㬀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀爀甀渀开挀漀洀洀愀渀搀⠀∀瀀爀椀渀琀∀Ⰰ ㌀Ⰰ ㄀Ⰰ 瀀愀爀愀洀猀Ⰰ ㄀⤀⤀ഀഀ
਍    椀渀琀 挀渀琀 㴀 瀀猀ⴀ㸀最攀琀开甀椀⠀㌀⤀ⴀ㸀䜀攀琀䈀椀渀愀爀礀刀攀猀甀氀琀猀䌀漀甀渀琀⠀⤀㬀ഀഀ
਍    䄀匀匀䔀刀吀开䔀儀唀䄀䰀⠀挀渀琀Ⰰ ㌀㄀㈀㘀⤀ഀഀ
਍    瘀漀椀搀 ⨀ 漀甀琀 㴀 瀀猀ⴀ㸀最攀琀开甀椀⠀㌀⤀ⴀ㸀䜀攀琀䈀椀渀愀爀礀刀攀猀甀氀琀猀⠀⤀㬀ഀഀ
਍    䄀匀匀䔀刀吀开一伀吀开一唀䰀䰀⠀漀甀琀⤀㬀ഀഀ
    ASSERT_EQUAL((*(((unsigned char*)out) + 1)), 0x4d)਍ഀഀ
    delete ps;਍ഀഀ
    return true;਍紀ഀഀ
਍戀漀漀氀 吀攀猀琀倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀唀琀昀㠀㨀㨀吀攀猀琀䔀爀爀漀爀伀甀琀䌀漀洀洀愀渀搀⠀⤀ഀഀ
{਍    倀㐀䌀氀椀攀渀琀䔀爀爀漀爀⨀⨀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 㴀 一唀䰀䰀㬀ഀഀ
    // create a new server਍    倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀 ⨀ 瀀猀 㴀 渀攀眀 倀㐀䈀爀椀搀最攀匀攀爀瘀攀爀⠀∀氀漀挀愀氀栀漀猀琀㨀㘀㘀㘀㘀∀Ⰰ ∀愀搀洀椀渀∀Ⰰ ∀∀Ⰰ ∀愀搀洀椀渀开猀瀀愀挀攀∀⤀㬀ഀഀ
    ASSERT_NOT_NULL(ps);਍ഀഀ
    // connect and see if the api returned an error. ਍    椀昀⠀ ℀瀀猀ⴀ㸀挀漀渀渀攀挀琀攀搀⠀ 挀漀渀渀攀挀琀椀漀渀䔀爀爀漀爀 ⤀ ⤀ഀഀ
    {਍        挀栀愀爀 戀甀昀昀嬀㈀㔀㘀崀㬀ഀഀ
        sprintf(buff, "Connection error: %s", *connectionError);਍        ⼀⼀ 䄀戀漀爀琀 椀昀 琀栀攀 挀漀渀渀攀挀琀 搀椀搀 渀漀琀 猀甀挀挀攀攀搀ഀഀ
        ASSERT_FAIL(buff);਍    紀ഀഀ
਍    䄀匀匀䔀刀吀开吀刀唀䔀⠀瀀猀ⴀ㸀甀渀椀挀漀搀攀匀攀爀瘀攀爀⠀⤀⤀㬀ഀഀ
    ps->set_charset("utf8", "utf16le");਍ഀഀ
    char* params[1];਍    瀀愀爀愀洀猀嬀　崀 㴀 ∀⼀⼀搀攀瀀漀琀⼀䴀礀䌀漀搀攀⼀䈀椀氀氀礀⸀戀洀瀀∀㬀ഀഀ
਍    ⼀⼀ 爀甀渀 愀 挀漀洀洀愀渀搀 愀最愀椀渀猀琀 愀 渀漀渀攀砀椀猀琀攀渀琀 昀椀氀攀ഀഀ
    // Should fail਍    䄀匀匀䔀刀吀开䘀䄀䰀匀䔀⠀瀀猀ⴀ㸀爀甀渀开挀漀洀洀愀渀搀⠀∀爀攀渀琀∀Ⰰ 㠀㠀Ⰰ ㄀Ⰰ 瀀愀爀愀洀猀Ⰰ ㄀⤀⤀ഀഀ
਍    倀㐀䌀氀椀攀渀琀䔀爀爀漀爀 ⨀ 漀甀琀 㴀 瀀猀ⴀ㸀最攀琀开甀椀⠀㠀㠀⤀ⴀ㸀䜀攀琀䔀爀爀漀爀刀攀猀甀氀琀猀⠀⤀㬀ഀഀ
਍    䄀匀匀䔀刀吀开一伀吀开一唀䰀䰀⠀漀甀琀⤀㬀ഀഀ
਍    䄀匀匀䔀刀吀开匀吀刀䤀一䜀开匀吀䄀刀吀匀开圀䤀吀䠀⠀漀甀琀ⴀ㸀䴀攀猀猀愀最攀Ⰰ ∀唀渀欀渀漀眀渀 挀漀洀洀愀渀搀⸀  吀爀礀 ✀瀀㐀 栀攀氀瀀✀ 昀漀爀 椀渀昀漀∀⤀ഀഀ
    ASSERT_NULL(out->Next)਍ഀഀ
    delete ps;਍ഀഀ
    return true;਍紀ഀഀ
