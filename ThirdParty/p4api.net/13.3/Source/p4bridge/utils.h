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
 * Name		: utils.h
 *
 * Author	: dbb
 *
 * Description	: Utilities to copy and duplicate strings to be returned by
 *  the P4 Bridge API.
 *
 ******************************************************************************/

/**************************************************************************
*
*  CopyStr: Allocate memory and copy a null terminated char string.
*
*   Returns: Pointer to the new string.
*
**************************************************************************/
char * CopyStr(const char *s);

/**************************************************************************
*
*  CopyWStr: Allocate memory and copy a null terminated WHAR string.
*
*   Returns: Pointer to the new string.
*
**************************************************************************/
wchar_t * CopyWStr(const wchar_t *s);

/**************************************************************************
*
*  AddStr: Allocate memory and create a null terminated char string that is 
*   the union of to strings. "Abcd" + "Efg" = AbcdEfg"
*
*   Returns: Pointer to the new string.
*
**************************************************************************/
char * AddStr(const char *s1, const char *s2);

/**************************************************************************
*
*  CpyStrBuff: Allocate memory and copy a byte array stored in a char[]. 
*   Copy is based on byte count, so the buffer can contain null characters 
*   and does NOT terminate on null.
*
*   Returns: Pointer to the new char[].
*
**************************************************************************/
char * CpyStrBuff(const char *s, int cnt);

/**************************************************************************
*
*  AddStrBuff: Allocate memory and create a byte array stored in a char[] 
*   that is the union of two btye[]s. Copy is based on byte count, so the 
*   buffer can contain null characters and does NOT terminate on null.
*
*   Returns: Pointer to the new char[].
*
**************************************************************************/
char * AddStrBuff(const char *s1, int cnt1, const char *s2, int cnt2);

//char * CopyStrPtr(const StrPtr *s);

//void CopyStrN(char *dst, const char *src, int max);

