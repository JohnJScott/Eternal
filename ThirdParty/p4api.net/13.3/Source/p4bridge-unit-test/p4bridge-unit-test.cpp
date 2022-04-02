// p4bridge-unit-test.cpp : Defines the entry point for the console application.
//

#include "StdAfx.h"
#include "UnitTestFrameWork.h"

#include <conio.h>
#include <string.h>

int main(int argc, char* argv[])
{
    if (argc > 0)
        for (int idx = 1; idx < argc; idx++)
        {
            if (strcmp(argv[idx], "-b") == 0) // break on fail
                UnitTestSuite::BreakOnFailure(true); 
            if (strcmp(argv[idx], "-e") == 0) // end on fail
                UnitTestSuite::EndOnFailure(true); 
        }
    UnitTestFrameWork::RunTests();

    printf("Hit 'x' to exit");
    _getch();

    return 0;
}

