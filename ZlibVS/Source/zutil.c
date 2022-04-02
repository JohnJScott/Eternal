/* zutil.c -- target dependent utility functions for the compression library
 * Copyright (C) 1995-2005, 2010, 2011, 2012 Jean-loup Gailly.
 * For conditions of distribution and use, see copyright notice in zlib.h
 */

#include "zutil.h"

const char* const z_errmsg[10] =
{
    "need dictionary",     /* Z_NEED_DICT       2  */
    "stream end",          /* Z_STREAM_END      1  */
    "",                    /* Z_OK              0  */
    "file error",          /* Z_ERRNO         (-1) */
    "stream error",        /* Z_STREAM_ERROR  (-2) */
    "data error",          /* Z_DATA_ERROR    (-3) */
    "insufficient memory", /* Z_MEM_ERROR     (-4) */
    "buffer error",        /* Z_BUF_ERROR     (-5) */
    "incompatible version",/* Z_VERSION_ERROR (-6) */
    ""
};


/* exported to allow conversion of error code to string for compress() and uncompress() */
const char* ZEXPORT zError( int32 err )
{
    return z_errmsg[Z_NEED_DICT - err];
}

const char* ZEXPORT zlibVersion()
{
    return ZLIB_VERSION;
}

#ifdef _DEBUG

#  ifndef verbose
#    define verbose 0
#  endif
int32 z_verbose = verbose;

void z_error (char* m)
{
    fprintf(stderr, "%s\n", m);
    exit(1);
}
#endif

