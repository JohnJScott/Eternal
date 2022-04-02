/* crc32.c -- compute the CRC-32 of a data stream
 * Copyright (C) 1995-2006, 2010, 2011, 2012 Mark Adler
 * For conditions of distribution and use, see copyright notice in zlib.h
 *
 * Thanks to Rodney Brown <rbrown64@csc.com.au> for his contribution of faster
 * CRC methods: exclusive-oring 32 bits of data at a time, and pre-computing
 * tables for updating the shift register in one step with three exclusive-ors
 * instead of four steps with four exclusive-ors.  This results in about a
 * factor of two increase in speed on a Power PC G4 (PPC7455) using gcc -O3.
 */

/* for STDC and definitions */
#include "zutil.h"

/** Tables of CRC-32s of all single-byte values. */
#include "crc32.h"

/** This function can be used by asm versions of crc32() */
const uint32* ZEXPORT get_crc_table()
{
    return ( const uint32* )crc_table;
}

/* Calculate the CRC checksum */
uint32 ZEXPORT crc32( uint32 crc, const uint8* buf, uint64 len )
{
    if( buf == NULL )
    {
        return 0;
    }

    crc = crc ^ 0xffffffff;
    while( len >= 8 )
    {
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        len -= 8;
    }

    if( len > 0 )
    {
        do
        {
            crc = crc_table[( ( int32 )crc ^ ( *buf++ ) ) & 0xff] ^ ( crc >> 8 );
        }
        while( --len );
    }

    return crc ^ 0xffffffff;
}
