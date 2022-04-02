/* uncompr.c -- decompress a memory buffer
 * Copyright (C) 1995-2003, 2010 Jean-loup Gailly.
 * For conditions of distribution and use, see copyright notice in zlib.h
 */

#include "zlib.h"

/*
     Decompresses the source buffer into the destination buffer.  sourceLen is the byte length of the source buffer. Upon entry, destLen is the total
   size of the destination buffer, which must be large enough to hold the entire uncompressed data. (The size of the uncompressed data must have
   been saved previously by the compressor and transmitted to the decompressor by some mechanism outside the scope of this compression library.)
   Upon exit, destLen is the actual size of the compressed buffer.

     uncompress returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_BUF_ERROR if there was not enough room in the output
   buffer, or Z_DATA_ERROR if the input data was corrupted.
*/
int ZEXPORT uncompress( uint8* dest, uint32* destLen, const uint8* source, uint32 sourceLen )
{
    z_stream stream;
    int err;

    stream.next_in = ( const uint8* )source;
    stream.avail_in = ( uint32 )sourceLen;

    stream.next_out = dest;
    stream.avail_out = ( uint32 )*destLen;

    stream.zalloc = ( alloc_func )NULL;
    stream.zfree = ( free_func )NULL;

    err = inflateInit( &stream );
    if( err != Z_OK )
    {
        return err;
    }

    err = inflate( &stream, Z_FINISH );
    if( err != Z_STREAM_END )
    {
        inflateEnd( &stream );
        if( err == Z_NEED_DICT || ( err == Z_BUF_ERROR && stream.avail_in == 0 ) )
        {
            return Z_DATA_ERROR;
        }

        return err;
    }
    *destLen = stream.total_out;

    err = inflateEnd( &stream );
    return err;
}
