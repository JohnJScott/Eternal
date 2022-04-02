/* compress.c -- compress a memory buffer
 * Copyright (C) 1995-2005 Jean-loup Gailly.
 * For conditions of distribution and use, see copyright notice in zlib.h
 */

#include "zlib.h"

/**
     Compresses the source buffer into the destination buffer. The level parameter has the same meaning as in deflateInit.  sourceLen is the byte
   length of the source buffer. Upon entry, destLen is the total size of the destination buffer, which must be at least 0.1% larger than sourceLen plus
   12 bytes. Upon exit, destLen is the actual size of the compressed buffer.

     compress returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_BUF_ERROR if there was not enough room in the output buffer,
   Z_STREAM_ERROR if the level parameter is invalid.
*/
int32 ZEXPORT compress( uint8* dest, uint32* destLen, const uint8* source, uint32 sourceLen, int32 level )
{
    z_stream stream;
    int32 err;

    if( level < 0 )
    {
        level = Z_DEFAULT_COMPRESSION;
    }

    stream.next_in = ( const uint8* )source;
    stream.avail_in = ( uint32 )sourceLen;
    stream.next_out = dest;
    stream.avail_out = ( uint32 )*destLen;
    if( ( uint32 )stream.avail_out != *destLen )
    {
        return Z_BUF_ERROR;
    }

    stream.zalloc = ( alloc_func )NULL;
    stream.zfree = ( free_func )NULL;

    err = deflateInit( &stream, level );
    if( err != Z_OK )
    {
        return err;
    }

    err = deflate( &stream, Z_FINISH );
    if( err != Z_STREAM_END )
    {
        deflateEnd( &stream );
        return err == Z_OK ? Z_BUF_ERROR : err;
    }

    *destLen = stream.total_out;

    err = deflateEnd( &stream );
    return err;
}

/**
	  compressBound() returns an upper bound on the compressed size after
	compress() or compress2() on sourceLen bytes.  It would be used before a
	compress() or compress2() call to allocate the destination buffer.
*/
uint32 ZEXPORT compressBound( uint32 sourceLen )
{
    return sourceLen + ( sourceLen >> 12 ) + ( sourceLen >> 14 ) + ( sourceLen >> 25 ) + 13;
}
