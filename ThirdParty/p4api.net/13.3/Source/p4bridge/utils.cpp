/*******************************************************************************

Copyright (c) 2010, Perforce Software, Inc.  All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1.  Redistributions of source code must retain the above copyright
    notice, this list of conditions and the following disclaimer.

2.  Redistributions in binary form must reproduce the above copyright
    notice, this list of conditions and the following disclaimer in the
    documentation and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL PERFORCE SOFTWARE, INC. BE LIABLE FOR ANY
DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

*******************************************************************************/

/*******************************************************************************
 * Name		: utils.cpp
 *
 * Author	: dbb
 *
 * Description	: Utilities to copy and duplicate strings to be returned by
 *  the P4 Bridge API.
 *
 ******************************************************************************/
#include "stdafx.h"

/**************************************************************************
*
*  CopyStr: Allocate memory and copy a null terminated char string.
*
*   Returns: Pointer to the new string.
*
**************************************************************************/
char * CopyStr(const char *s)
{
	if (s == NULL)
	{
		return NULL;
	}
    int cnt = strlen(s);
    char * newStr = new char[cnt+1];
    for (int idx = 0; idx < cnt; idx++)
        newStr[idx] = s[idx];
    newStr[cnt] = '\0';
    return newStr;
}

/**************************************************************************
*
*  CopyWStr: Allocate memory and copy a null terminated WHAR string.
*
*   Returns: Pointer to the new string.
*
**************************************************************************/
wchar_t * CopyWStr(const wchar_t *s)
{
	if (s == NULL)
	{
		return NULL;
	}
    int cnt = wcslen(s);
    wchar_t * newStr = new wchar_t[cnt+1];
    for (int idx = 0; idx < cnt; idx++)
        newStr[idx] = s[idx];
    newStr[cnt] = '\0';
    return newStr;
}

/**************************************************************************
*
*  AddStr: Allocate memory and create a null terminated char string that is 
*   the union of to strings. "Abcd" + "Efg" = AbcdEfg"
*
*   Returns: Pointer to the new string.
*
**************************************************************************/
char * AddStr(const char *s1, const char *s2)
{
    int cnt1 = strlen(s1);
    int cnt2 = strlen(s2);
    char * newStr = new char[cnt1 + cnt2 + 1];
    for (int idx = 0; idx < cnt1; idx++)
        newStr[idx] = s1[idx];

    for (int idx = 0; idx < cnt2; idx++)
        newStr[idx + cnt1] = s2[idx];

    newStr[cnt1 + cnt2] = '\0';
    return newStr;
}

/**************************************************************************
*
*  CpyStrBuff: Allocate memory and copy a byte array stored in a StrPtr. Copy
*   is based on byte count, so the buffer can contain null characters and
*   does NOT terminate on null.
*
*   Returns: Pointer to the new char[].
*
**************************************************************************/
char * CpyStrBuff(const char *s, int cnt)
{
    char * newbuff = new char[cnt];
    for (int idx = 0; idx < cnt; idx++)
        newbuff[idx] = s[idx];

    return newbuff;
}

/**************************************************************************
*
*  AddStrBuff: Allocate memory and create a byte array stored in a char[] 
*   that is the union of two btye[]s. Copy is based on byte count, so the 
*   buffer can contain null characters and does NOT terminate on null.
*
*   Returns: Pointer to the new char[].
*
**************************************************************************/
char * AddStrBuff(const char *s1, int cnt1, const char *s2, int cnt2)
{
    char * newbuff = new char[cnt1 + cnt2];
    for (int idx = 0; idx < cnt1; idx++)
        newbuff[idx] = s1[idx];

    for (int idx = 0; idx < cnt2; idx++)
        newbuff[idx + cnt1] = s2[idx];

    return newbuff;
}

//char * CopyStrPtr(const StrPtr *s)
//{
//	char * newStr = new char[s->Length()+1];
//    char * oldStr = s->Text();
//
//	for (int idx = 0; idx < s->Length(); idx++)
//		newStr[idx] = oldStr[idx];
//	newStr[s->Length()] = '\0';
//	return newStr;
//}

//void CopyStrN(char *dst, const char *src, int max)
//{
//	int idx = 0;
//	for (idx = 0; idx < max && src[idx] != '\0' ; idx++)
//		dst[idx] = src[idx];
//	dst[idx] = '\0';
//}

//char * CopyStrPtr(const StrPtr *s)
//{
//	char * newStr = new char[s->Length()+1];
//    char * oldStr = s->Text();
//
//	for (int idx = 0; idx < s->Length(); idx++)
//		newStr[idx] = oldStr[idx];
//	newStr[s->Length()] = '\0';
//	return newStr;
//}

