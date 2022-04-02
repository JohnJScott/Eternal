/* inflate.c -- zlib decompression
 * Copyright (C) 1995-2012 Mark Adler
 * For conditions of distribution and use, see copyright notice in zlib.h
 */

#include "zutil.h"
#include "inftrees.h"
#include "inflate.h"
#include "inffast.h"

int ZEXPORT inflateResetKeep( z_stream* strm )
{
    inflate_state* state;

    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    strm->total_in = 0;
    strm->total_out = 0;
    state->total = 0;
    strm->msg = NULL;

    /* to support ill-conceived Java test suite */
    if( state->wrap )
    {
        strm->adler = state->wrap & 1;
    }

    state->mode = HEAD;
    state->last = 0;
    state->havedict = 0;
    state->dmax = 32768;
    state->hold = 0;
    state->bits = 0;
    state->lencode = state->codes;
    state->distcode = state->codes;
    state->next = state->codes;
    state->sane = 1;
    state->back = -1;
    Tracev( ( stderr, "inflate: reset\n" ) );
    return Z_OK;
}

int ZEXPORT inflateReset( z_stream* strm )
{
    inflate_state* state;

    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    state->wsize = 0;
    state->whave = 0;
    state->wnext = 0;
    return inflateResetKeep( strm );
}

int ZEXPORT inflateReset2( z_stream* strm, int windowBits )
{
    int wrap;
    inflate_state* state;

    /* get the state */
    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;

    /* extract wrap request from windowBits parameter */
    if( windowBits < 0 )
    {
        wrap = 0;
        windowBits = -windowBits;
    }
    else
    {
        wrap = ( windowBits >> 4 ) + 1;
    }

    /* set number of window bits, free window if different */
    if( windowBits && ( windowBits < 8 || windowBits > 15 ) )
    {
        return Z_STREAM_ERROR;
    }

    if( state->window != NULL && state->wbits != ( uint32 )windowBits )
    {
        strm->zfree( state->window );
        state->window = NULL;
    }

    /* update state and reset the rest of it */
    state->wrap = wrap;
    state->wbits = ( uint32 )windowBits;
    return inflateReset( strm );
}

int32 ZEXPORT inflateInit( z_stream* strm )
{
    int ret;
    inflate_state*  state;

    if( strm == NULL )
    {
        return Z_STREAM_ERROR;
    }

    if( strm->version == NULL || strm->version[0] != ZLIB_VERSION[0] || strm->stream_size != sizeof( z_stream ) )
    {
        return Z_VERSION_ERROR;
    }

    /* in case we return an error */
    strm->msg = NULL;
    if( strm->zalloc == ( alloc_func )NULL )
    {
        strm->zalloc = malloc;
    }

    if( strm->zfree == ( free_func )NULL )
    {
        strm->zfree = free;
    }

    if( strm->windowBits == 0 )
    {
        strm->windowBits = MAX_WBITS;
    }

    if( strm->memLevel == 0 )
    {
        strm->memLevel = DEF_MEM_LEVEL;
    }

    if( strm->strategy == 0 )
    {
        strm->strategy = Z_DEFAULT_STRATEGY;
    }

    state = ( inflate_state* )strm->zalloc( sizeof( inflate_state ) );
    if( state == NULL )
    {
        return Z_MEM_ERROR;
    }

    Tracev( ( stderr, "inflate: allocated\n" ) );
    strm->state = ( struct internal_state* )state;
    state->window = NULL;
    ret = inflateReset2( strm, strm->windowBits );
    if( ret != Z_OK )
    {
        strm->zfree( state );
        strm->state = NULL;
    }

    return ret;
}

int ZEXPORT inflatePrime( z_stream* strm, int bits, int value )
{
    inflate_state*  state;

    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    if( bits < 0 )
    {
        state->hold = 0;
        state->bits = 0;
        return Z_OK;
    }

    if( bits > 16 || state->bits + bits > 32 )
    {
        return Z_STREAM_ERROR;
    }

    value &= ( 1 << bits ) - 1;
    state->hold += value << state->bits;
    state->bits += bits;
    return Z_OK;
}

/*
 Return state with length and distance decoding tables and index sizes set to fixed code decoding.  Normally this returns fixed tables from inffixed.h.
If BUILDFIXED is defined, then instead this routine builds the tables the first time it's called, and returns those tables the first time and
thereafter.  This reduces the size of the code by about 2K bytes, in exchange for a little execution time.  However, BUILDFIXED should not be
used for threaded applications, since the rewriting of the tables and virgin may not be thread-safe.
*/
static void fixedtables( inflate_state* state )
{
#   include "inffixed.h"

    state->lencode = lenfix;
    state->lenbits = 9;
    state->distcode = distfix;
    state->distbits = 5;
}

/*
Update the window with the last wsize (normally 32K) bytes written before returning.  If window does not exist yet, create it.  This is only called
when a window is already in use, or when output has been written during this inflate call, but the end of the deflate stream has not been reached yet.
It is also called to create a window for dictionary data when a dictionary is loaded.

Providing output buffers larger than 32K to inflate() should provide a speed advantage, since only the last 32K of output is copied to the sliding window
upon return from inflate(), and since all distances after the first 32K of output will fall in the output data, making match copies simpler and faster.
The advantage may be dependent on the size of the processor's data caches.
*/
static int32 updatewindow( z_stream* strm, const uint8* end, uint32 copy )
{
    inflate_state*  state;
    uint32 dist;

    state = ( inflate_state* )strm->state;

    /* if it hasn't been done already, allocate space for the window */
    if( state->window == NULL )
    {
        state->window = ( uint8* )strm->zalloc( ( size_t )( 1 << state->wbits ) );
        if( state->window == NULL )
        {
            return 1;
        }
    }

    /* if window not in use yet, initialize */
    if( state->wsize == 0 )
    {
        state->wsize = 1 << state->wbits;
        state->wnext = 0;
        state->whave = 0;
    }

    /* copy state->wsize or less output bytes into the circular window */
    if( copy >= state->wsize )
    {
        memcpy( state->window, end - state->wsize, state->wsize );
        state->wnext = 0;
        state->whave = state->wsize;
    }
    else
    {
        dist = state->wsize - state->wnext;
        if( dist > copy ) dist = copy;
        memcpy( state->window + state->wnext, end - copy, dist );
        copy -= dist;
        if( copy )
        {
            memcpy( state->window, end - copy, copy );
            state->wnext = copy;
            state->whave = state->wsize;
        }
        else
        {
            state->wnext += dist;
            if( state->wnext == state->wsize )
            {
                state->wnext = 0;
            }

            if( state->whave < state->wsize )
            {
                state->whave += dist;
            }
        }
    }
    return 0;
}

/* Macros for inflate(): */

/* check function to use adler32() for zlib or crc32() for gzip */
#  define UPDATE(check, buf, len) adler32( check, buf, len )

/* Load registers with state in inflate() for speed */
#define LOAD() \
    do { \
        put = strm->next_out; \
        left = strm->avail_out; \
        next = strm->next_in; \
        have = strm->avail_in; \
        hold = state->hold; \
        bits = state->bits; \
    } while (0)

/* Restore state from registers in inflate() */
#define RESTORE() \
    do { \
        strm->next_out = put; \
        strm->avail_out = left; \
        strm->next_in = next; \
        strm->avail_in = have; \
        state->hold = hold; \
        state->bits = bits; \
    } while (0)

/* Clear the input bit accumulator */
#define INITBITS() \
    do { \
        hold = 0; \
        bits = 0; \
    } while (0)

/* Get a byte of input into the bit accumulator, or return from inflate()
if there is no input available. */
#define PULLBYTE() \
    do { \
        if (have == 0) goto inf_leave; \
        have--; \
        hold += (uint32)(*next++) << bits; \
        bits += 8; \
    } while (0)

/* Assure that there are at least n bits in the bit accumulator.  If there is
not enough available input to do that, then return from inflate(). */
#define NEEDBITS(n) \
    do { \
        while (bits < (uint32)(n)) \
            PULLBYTE(); \
    } while (0)

/* Return the low n bits of the bit accumulator (n < 16) */
#define BITS(n) \
    ((uint32)hold & ((1U << (n)) - 1))

/* Remove n bits from the bit accumulator */
#define DROPBITS(n) \
    do { \
        hold >>= (n); \
        bits -= (uint32)(n); \
    } while (0)

/* Remove zero to seven bits as needed to go to a byte boundary */
#define BYTEBITS() \
    do { \
        hold >>= bits & 7; \
        bits -= bits & 7; \
    } while (0)

/*
inflate() uses a state machine to process as much input data and generate as
much output data as possible before returning.  The state machine is
structured roughly as follows:

for (;;) switch (state) {
...
case STATEn:
if (not enough input data or output space to make progress)
return;
... make progress ...
state = STATEm;
break;
...
}

so when inflate() is called again, the same case is attempted again, and
if the appropriate resources are provided, the machine proceeds to the
next state.  The NEEDBITS() macro is usually the way the state evaluates
whether it can proceed or should return.  NEEDBITS() does the return if
the requested bits are not available.  The typical use of the BITS macros
is:

NEEDBITS(n);
... do something with BITS(n) ...
DROPBITS(n);

where NEEDBITS(n) either returns from inflate() if there isn't enough
input left to load n bits into the accumulator, or it continues.  BITS(n)
gives the low n bits in the accumulator.  When done, DROPBITS(n) drops
the low n bits off the accumulator.  INITBITS() clears the accumulator
and sets the number of available bits to zero.  BYTEBITS() discards just
enough bits to put the accumulator on a byte boundary.  After BYTEBITS()
and a NEEDBITS(8), then BITS(8) would return the next byte in the stream.

NEEDBITS(n) uses PULLBYTE() to get an available byte of input, or to return
if there is no input available.  The decoding of variable length codes uses
PULLBYTE() directly in order to pull just enough bytes to decode the next
code, and no more.

Some states loop until they get enough input, making sure that enough
state information is maintained to continue the loop where it left off
if NEEDBITS() returns in the loop.  For example, want, need, and keep
would all have to actually be part of the saved state in case NEEDBITS()
returns:

case STATEw:
while (want < need) {
NEEDBITS(n);
keep[want++] = BITS(n);
DROPBITS(n);
}
state = STATEx;
case STATEx:

As shown above, if the next state is also the next case, then the break
is omitted.

A state may also return if there is not enough output space available to
complete that state.  Those states are copying stored data, writing a
literal byte, and copying a matching string.

When returning, a "goto inf_leave" is used to update the total counters,
update the check value, and determine whether any progress has been made
during that inflate() call in order to return the proper return code.
Progress is defined as a change in either strm->avail_in or strm->avail_out.
When there is a window, goto inf_leave will update the window with the last
output written.  If a goto inf_leave occurs in the middle of decompression
and there is no window currently, goto inf_leave will create one and copy
output to the window for the next call of inflate().

In this implementation, the flush parameter of inflate() only affects the
return code (per zlib.h).  inflate() always writes as much as possible to
strm->next_out, given the space available and the provided input--the effect
documented in zlib.h of Z_SYNC_FLUSH.  Furthermore, inflate() always defers
the allocation of and copying into a sliding window until necessary, which
provides the effect documented in zlib.h for Z_FINISH when the entire input
stream available.  So the only thing the flush parameter actually does is:
when flush is set to Z_FINISH, inflate() cannot return Z_OK.  Instead it
will return Z_BUF_ERROR if it has not reached the end of the stream.
*/

int ZEXPORT inflate( z_stream* strm, int flush )
{
    inflate_state* state;
    const uint8* next;			/* next input */
    uint8* put;					/* next output */
    uint32 have;
    uint32 left;				/* available input and output */
    uint32 hold;				/* bit buffer */
    uint32 bits;              /* bits in bit buffer */
    uint32 in, out;           /* save starting available input and output */
    uint32 copy;              /* number of stored or match bytes to copy */
    uint8* from;				/* where to copy match bytes from */
    code here;                  /* current decoding table entry */
    code last;                  /* parent table entry */
    uint32 len;               /* length to copy for repeats, bits to drop */
    int ret;                    /* return code */

    /* permutation of code lengths */
    static const uint16 order[19] = { 16, 17, 18, 0, 8, 7, 9, 6, 10, 5, 11, 4, 12, 3, 13, 2, 14, 1, 15 };

    if( strm == NULL || strm->state == NULL || strm->next_out == NULL || ( strm->next_in == NULL && strm->avail_in != 0 ) )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    if( state->mode == TYPE )
    {
        /* skip check */
        state->mode = TYPEDO;
    }

    LOAD();
    in = have;
    out = left;
    ret = Z_OK;
    for( ;; )
        switch( state->mode )
        {
        case HEAD:
            if( state->wrap == 0 )
            {
                state->mode = TYPEDO;
                break;
            }
            NEEDBITS( 16 );
            if( ( ( BITS( 8 ) << 8 ) + ( hold >> 8 ) ) % 31 )
            {
                strm->msg = "incorrect header check";
                state->mode = BAD;
                break;
            }
            if( BITS( 4 ) != Z_DEFLATED )
            {
                strm->msg = "unknown compression method";
                state->mode = BAD;
                break;
            }
            DROPBITS( 4 );
            len = BITS( 4 ) + 8;
            if( state->wbits == 0 )
            {
                state->wbits = len;
            }
            else if( len > state->wbits )
            {
                strm->msg = "invalid window size";
                state->mode = BAD;
                break;
            }
            state->dmax = 1U << len;
            Tracev( ( stderr, "inflate:   zlib header ok\n" ) );
            strm->adler = state->check = adler32( 0, NULL, 0 );
            state->mode = hold & 0x200 ? DICTID : TYPE;
            INITBITS();
            break;
        case DICTID:
            NEEDBITS( 32 );
            strm->adler = state->check = ZSWAP32( hold );
            INITBITS();
            state->mode = DICT;
        case DICT:
            if( state->havedict == 0 )
            {
                RESTORE();
                return Z_NEED_DICT;
            }
            strm->adler = state->check = adler32( 0, NULL, 0 );
            state->mode = TYPE;
        case TYPE:
            if( flush == Z_BLOCK || flush == Z_TREES )
            {
                goto inf_leave;
            }
        case TYPEDO:
            if( state->last )
            {
                BYTEBITS();
                state->mode = CHECK;
                break;
            }
            NEEDBITS( 3 );
            state->last = BITS( 1 );
            DROPBITS( 1 );
            switch( BITS( 2 ) )
            {
            case 0:                             /* stored block */
                Tracev( ( stderr, "inflate:     stored block%s\n", state->last ? " (last)" : "" ) );
                state->mode = STORED;
                break;
            case 1:                             /* fixed block */
                fixedtables( state );
                Tracev( ( stderr, "inflate:     fixed codes block%s\n", state->last ? " (last)" : "" ) );
                state->mode = LEN_;             /* decode codes */
                if( flush == Z_TREES )
                {
                    DROPBITS( 2 );
                    goto inf_leave;
                }
                break;
            case 2:                             /* dynamic block */
                Tracev( ( stderr, "inflate:     dynamic codes block%s\n", state->last ? " (last)" : "" ) );
                state->mode = TABLE;
                break;
            case 3:
                strm->msg = "invalid block type";
                state->mode = BAD;
            }
            DROPBITS( 2 );
            break;
        case STORED:
            BYTEBITS();                         /* go to byte boundary */
            NEEDBITS( 32 );
            if( ( hold & 0xffff ) != ( ( hold >> 16 ) ^ 0xffff ) )
            {
                strm->msg = "invalid stored block lengths";
                state->mode = BAD;
                break;
            }
            state->length = ( uint32 )hold & 0xffff;
            Tracev( ( stderr, "inflate:       stored length %u\n",
                      state->length ) );
            INITBITS();
            state->mode = COPY_;
            if( flush == Z_TREES )
            {
                goto inf_leave;
            }
        case COPY_:
            state->mode = COPY;
        case COPY:
            copy = state->length;
            if( copy )
            {
                if( copy > have )
                {
                    copy = have;
                }
                if( copy > left )
                {
                    copy = left;
                }
                if( copy == 0 )
                {
                    goto inf_leave;
                }
                memcpy( put, next, copy );
                have -= copy;
                next += copy;
                left -= copy;
                put += copy;
                state->length -= copy;
                break;
            }
            Tracev( ( stderr, "inflate:       stored end\n" ) );
            state->mode = TYPE;
            break;
        case TABLE:
            NEEDBITS( 14 );
            state->nlen = BITS( 5 ) + 257;
            DROPBITS( 5 );
            state->ndist = BITS( 5 ) + 1;
            DROPBITS( 5 );
            state->ncode = BITS( 4 ) + 4;
            DROPBITS( 4 );

            Tracev( ( stderr, "inflate:       table sizes ok\n" ) );
            state->have = 0;
            state->mode = LENLENS;
        case LENLENS:
            while( state->have < state->ncode )
            {
                NEEDBITS( 3 );
                state->lens[order[state->have++]] = ( uint16 )BITS( 3 );
                DROPBITS( 3 );
            }

            while( state->have < 19 )
            {
                state->lens[order[state->have++]] = 0;
            }

            state->next = state->codes;
            state->lencode = ( const code* )( state->next );
            state->lenbits = 7;
            ret = inflate_table( CODES, state->lens, 19, &( state->next ), &( state->lenbits ), state->work );
            if( ret )
            {
                strm->msg = "invalid code lengths set";
                state->mode = BAD;
                break;
            }
            Tracev( ( stderr, "inflate:       code lengths ok\n" ) );
            state->have = 0;
            state->mode = CODELENS;
        case CODELENS:
            while( state->have < state->nlen + state->ndist )
            {
                for( ;; )
                {
                    here = state->lencode[BITS( state->lenbits )];
                    if( ( uint32 )( here.bits ) <= bits )
                    {
                        break;
                    }
                    PULLBYTE();
                }

                if( here.val < 16 )
                {
                    DROPBITS( here.bits );
                    state->lens[state->have++] = here.val;
                }
                else
                {
                    if( here.val == 16 )
                    {
                        NEEDBITS( here.bits + 2 );
                        DROPBITS( here.bits );
                        if( state->have == 0 )
                        {
                            strm->msg = "invalid bit length repeat";
                            state->mode = BAD;
                            break;
                        }

                        len = state->lens[state->have - 1];
                        copy = 3 + BITS( 2 );
                        DROPBITS( 2 );
                    }
                    else if( here.val == 17 )
                    {
                        NEEDBITS( here.bits + 3 );
                        DROPBITS( here.bits );
                        len = 0;
                        copy = 3 + BITS( 3 );
                        DROPBITS( 3 );
                    }
                    else
                    {
                        NEEDBITS( here.bits + 7 );
                        DROPBITS( here.bits );
                        len = 0;
                        copy = 11 + BITS( 7 );
                        DROPBITS( 7 );
                    }

                    if( state->have + copy > state->nlen + state->ndist )
                    {
                        strm->msg = "invalid bit length repeat";
                        state->mode = BAD;
                        break;
                    }

                    while( copy-- )
                    {
                        state->lens[state->have++] = ( uint16 )len;
                    }
                }
            }

            /* handle error breaks in while */
            if( state->mode == BAD )
            {
                break;
            }

            /* check for end-of-block code (better have one) */
            if( state->lens[256] == 0 )
            {
                strm->msg = "invalid code -- missing end-of-block";
                state->mode = BAD;
                break;
            }

            /* build code tables -- note: do not change the lenbits or distbits values here (9 and 6) without reading the comments in inftrees.h
            concerning the ENOUGH constants, which depend on those values */
            state->next = state->codes;
            state->lencode = ( const code* )state->next;
            state->lenbits = 9;
            ret = inflate_table( LENS, state->lens, state->nlen, &state->next, &state->lenbits, state->work );
            if( ret )
            {
                strm->msg = "invalid literal/lengths set";
                state->mode = BAD;
                break;
            }

            state->distcode = ( const code* )state->next;
            state->distbits = 6;
            ret = inflate_table( DISTS, state->lens + state->nlen, state->ndist, &state->next, &state->distbits, state->work );
            if( ret )
            {
                strm->msg = "invalid distances set";
                state->mode = BAD;
                break;
            }

            Tracev( ( stderr, "inflate:       codes ok\n" ) );
            state->mode = LEN_;
            if( flush == Z_TREES )
            {
                goto inf_leave;
            }
        case LEN_:
            state->mode = LEN;
        case LEN:
            if( have >= 6 && left >= 258 )
            {
                RESTORE();
                inflate_fast( strm, out );
                LOAD();
                if( state->mode == TYPE )
                {
                    state->back = -1;
                }

                break;
            }

            state->back = 0;
            for( ;;)
            {
                here = state->lencode[BITS( state->lenbits )];
                if( ( uint32 )( here.bits ) <= bits )
                {
                    break;
                }

                PULLBYTE();
            }
            if( here.op && ( here.op & 0xf0 ) == 0 )
            {
                last = here;
                for( ;;)
                {
                    here = state->lencode[last.val + ( BITS( last.bits + last.op ) >> last.bits )];
                    if( ( uint32 )( last.bits + here.bits ) <= bits )
                    {
                        break;
                    }

                    PULLBYTE();
                }

                DROPBITS( last.bits );
                state->back += last.bits;
            }
            DROPBITS( here.bits );
            state->back += here.bits;
            state->length = ( uint32 )here.val;
            if( ( int )( here.op ) == 0 )
            {
                Tracevv( ( stderr, here.val >= 0x20 && here.val < 0x7f ?
                           "inflate:         literal '%c'\n" :
                           "inflate:         literal 0x%02x\n", here.val ) );
                state->mode = LIT;
                break;
            }
            if( here.op & 32 )
            {
                Tracevv( ( stderr, "inflate:         end of block\n" ) );
                state->back = -1;
                state->mode = TYPE;
                break;
            }
            if( here.op & 64 )
            {
                strm->msg = "invalid literal/length code";
                state->mode = BAD;
                break;
            }
            state->extra = ( uint32 )( here.op ) & 15;
            state->mode = LENEXT;
        case LENEXT:
            if( state->extra )
            {
                NEEDBITS( state->extra );
                state->length += BITS( state->extra );
                DROPBITS( state->extra );
                state->back += state->extra;
            }
            Tracevv( ( stderr, "inflate:         length %u\n", state->length ) );
            state->was = state->length;
            state->mode = DIST;
        case DIST:
            for( ;;)
            {
                here = state->distcode[BITS( state->distbits )];
                if( ( uint32 )( here.bits ) <= bits )
                {
                    break;
                }
                PULLBYTE();
            }
            if( ( here.op & 0xf0 ) == 0 )
            {
                last = here;
                for( ;;)
                {
                    here = state->distcode[last.val + ( BITS( last.bits + last.op ) >> last.bits )];
                    if( ( uint32 )( last.bits + here.bits ) <= bits )
                    {
                        break;
                    }
                    PULLBYTE();
                }
                DROPBITS( last.bits );
                state->back += last.bits;
            }
            DROPBITS( here.bits );
            state->back += here.bits;
            if( here.op & 64 )
            {
                strm->msg = "invalid distance code";
                state->mode = BAD;
                break;
            }
            state->offset = ( uint32 )here.val;
            state->extra = ( uint32 )( here.op ) & 15;
            state->mode = DISTEXT;
        case DISTEXT:
            if( state->extra )
            {
                NEEDBITS( state->extra );
                state->offset += BITS( state->extra );
                DROPBITS( state->extra );
                state->back += state->extra;
            }
#ifdef INFLATE_STRICT
            if( state->offset > state->dmax )
            {
                strm->msg = "invalid distance too far back";
                state->mode = BAD;
                break;
            }
#endif
            Tracevv( ( stderr, "inflate:         distance %u\n", state->offset ) );
            state->mode = MATCH;
        case MATCH:
            if( left == 0 )
            {
                goto inf_leave;
            }
            copy = out - left;
            if( state->offset > copy )
            {
                /* copy from window */
                copy = state->offset - copy;
                if( copy > state->whave )
                {
                    if( state->sane )
                    {
                        strm->msg = "invalid distance too far back";
                        state->mode = BAD;
                        break;
                    }

                }

                if( copy > state->wnext )
                {
                    copy -= state->wnext;
                    from = state->window + ( state->wsize - copy );
                }
                else
                {
                    from = state->window + ( state->wnext - copy );
                }

                if( copy > state->length )
                {
                    copy = state->length;
                }
            }
            else
            {
                /* copy from output */
                from = put - state->offset;
                copy = state->length;
            }

            if( copy > left )
            {
                copy = left;
            }

            left -= copy;
            state->length -= copy;
            do
            {
                *put++ = *from++;
            }
            while( --copy );

            if( state->length == 0 )
            {
                state->mode = LEN;
            }
            break;
        case LIT:
            if( left == 0 )
            {
                goto inf_leave;
            }
            *put++ = ( uint8 )( state->length );
            left--;
            state->mode = LEN;
            break;
        case CHECK:
            if( state->wrap )
            {
                NEEDBITS( 32 );
                out -= left;
                strm->total_out += out;
                state->total += out;
                if( out )
                {
                    strm->adler = state->check = UPDATE( state->check, put - out, out );
                }

                out = left;
                if( ( ZSWAP32( hold ) ) != state->check )
                {
                    strm->msg = "incorrect data check";
                    state->mode = BAD;
                    break;
                }
                INITBITS();
                Tracev( ( stderr, "inflate:   check matches trailer\n" ) );
            }
            state->mode = DONE;
        case DONE:
            ret = Z_STREAM_END;
            goto inf_leave;
        case BAD:
            ret = Z_DATA_ERROR;
            goto inf_leave;
        case MEM:
            return Z_MEM_ERROR;
        case SYNC:
        default:
            return Z_STREAM_ERROR;
        }

    /*
    	  Return from inflate(), updating the total counts and the check value. If there was no progress during the inflate() call, return a buffer
    	error.  Call updatewindow() to create and/or update the window state. Note: a memory error from inflate() is non-recoverable.
    */
inf_leave:
    RESTORE();
    if( state->wsize || ( out != strm->avail_out && state->mode < BAD && ( state->mode < CHECK || flush != Z_FINISH ) ) )
    {
        if( updatewindow( strm, strm->next_out, out - strm->avail_out ) )
        {
            state->mode = MEM;
            return Z_MEM_ERROR;
        }
    }

    in -= strm->avail_in;
    out -= strm->avail_out;
    strm->total_in += in;
    strm->total_out += out;
    state->total += out;
    if( state->wrap && out )
    {
        strm->adler = state->check = UPDATE( state->check, strm->next_out - out, out );
    }

    if( ( ( in == 0 && out == 0 ) || flush == Z_FINISH ) && ret == Z_OK )
    {
        ret = Z_BUF_ERROR;
    }
    return ret;
}

int ZEXPORT inflateEnd( z_stream* strm )
{
    inflate_state* state;
    if( strm == NULL || strm->state == NULL || strm->zfree == ( free_func )0 )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    if( state->window != NULL )
    {
        strm->zfree( state->window );
    }
    strm->zfree( strm->state );
    strm->state = NULL;
    Tracev( ( stderr, "inflate: end\n" ) );
    return Z_OK;
}

int ZEXPORT inflateGetDictionary( z_stream* strm, uint8* dictionary, uint32* dictLength )
{
    inflate_state* state;

    /* check state */
    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;

    /* copy dictionary */
    if( state->whave && dictionary != NULL )
    {
        memcpy( dictionary, state->window + state->wnext, state->whave - state->wnext );
        memcpy( dictionary + state->whave - state->wnext, state->window, state->wnext );
    }

    if( dictLength != NULL )
    {
        *dictLength = state->whave;
    }

    return Z_OK;
}

int ZEXPORT inflateSetDictionary( z_stream* strm, const uint8* dictionary, uint32 dictLength )
{
    inflate_state*  state;
    uint32 dictid;
    int ret;

    /* check state */
    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    if( state->wrap != 0 && state->mode != DICT )
    {
        return Z_STREAM_ERROR;
    }

    /* check for correct dictionary identifier */
    if( state->mode == DICT )
    {
        dictid = adler32( 0L, NULL, 0 );
        dictid = adler32( dictid, dictionary, dictLength );
        if( dictid != state->check )
        {
            return Z_DATA_ERROR;
        }
    }

    /* copy dictionary to window using updatewindow(), which will amend the existing dictionary if appropriate */
    ret = updatewindow( strm, dictionary + dictLength, dictLength );
    if( ret )
    {
        state->mode = MEM;
        return Z_MEM_ERROR;
    }

    state->havedict = 1;
    Tracev( ( stderr, "inflate:   dictionary set\n" ) );
    return Z_OK;
}

/**
  Search buf[0..len-1] for the pattern: 0, 0, 0xff, 0xff.  Return when found or when out of input.  When called, *have is the number of pattern bytes
 found in order so far, in 0..3.  On return *have is updated to the new state.  If on return *have equals four, then the pattern was found and the
 return value is how many bytes were read including the last byte of the pattern.  If *have is less than four, then the pattern has not been found
 yet and the return value is len.  In the latter case, syncsearch() can be called again with more data and the *have state.  *have is initialized to
 zero for the first call.
*/
static uint32 syncsearch( uint32* have, const uint8* buf, uint32 len )
{
    uint8 got;
    uint32 next;

    got = ( uint8 )*have;
    next = 0;
    while( next < len && got < 4 )
    {
        if( buf[next] == ( got < 2 ? 0 : 0xff ) )
        {
            got++;
        }
        else if( buf[next] != 0 )
        {
            got = 0;
        }
        else
        {
            got = 4 - got;
        }

        next++;
    }

    *have = got;
    return next;
}

int ZEXPORT inflateSync( z_stream* strm )
{
    uint32 len;               /* number of bytes to look at or looked at */
    uint32 in, out;      /* temporary to save total_in and total_out */
    uint8 buf[4];       /* to restore bit buffer to byte string */
    inflate_state* state;

    /* check parameters */
    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    if( strm->avail_in == 0 && state->bits < 8 )
    {
        return Z_BUF_ERROR;
    }

    /* if first time, start search in bit buffer */
    if( state->mode != SYNC )
    {
        state->mode = SYNC;
        state->hold <<= state->bits & 7;
        state->bits -= state->bits & 7;
        len = 0;
        while( state->bits >= 8 )
        {
            buf[len++] = ( uint8 )state->hold;
            state->hold >>= 8;
            state->bits -= 8;
        }

        state->have = 0;
        syncsearch( &state->have, buf, len );
    }

    /* search available input */
    len = syncsearch( &state->have, strm->next_in, strm->avail_in );
    strm->avail_in -= len;
    strm->next_in += len;
    strm->total_in += len;

    /* return no joy or set up to restart inflate() on a new block */
    if( state->have != 4 )
    {
        return Z_DATA_ERROR;
    }

    in = strm->total_in;
    out = strm->total_out;
    inflateReset( strm );
    strm->total_in = in;
    strm->total_out = out;
    state->mode = TYPE;
    return Z_OK;
}

/*
  Returns true if inflate is currently at the end of a block generated by Z_SYNC_FLUSH or Z_FULL_FLUSH. This function is used by one PPP
 implementation to provide an additional safety check. PPP uses Z_SYNC_FLUSH but removes the length bytes of the resulting empty stored
 block. When decompressing, PPP checks that at the end of input packet, inflate is waiting for these length bytes.
*/
int ZEXPORT inflateSyncPoint( z_stream* strm )
{
    inflate_state*  state;

    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    return state->mode == STORED && state->bits == 0;
}

int ZEXPORT inflateCopy( z_stream* dest, z_stream* source )
{
    inflate_state* state;
    inflate_state* copy;
    uint8* window;
    uint32 wsize;

    /* check input */
    if( dest == NULL || source == NULL || source->state == NULL || source->zalloc == ( alloc_func )0 || source->zfree == ( free_func )0 )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )source->state;

    /* allocate space */
    copy = ( inflate_state* )sizeof( inflate_state );
    if( copy == NULL )
    {
        return Z_MEM_ERROR;
    }

    window = NULL;
    if( state->window != NULL )
    {
        window = ( uint8* )source->zalloc( ( size_t )( 1 << state->wbits ) );
        if( window == NULL )
        {
            source->zfree( copy );
            return Z_MEM_ERROR;
        }
    }

    /* copy state */
    memcpy( ( void* )dest, ( void* )source, sizeof( z_stream ) );
    memcpy( ( void* )copy, ( void* )state, sizeof( inflate_state ) );
    if( state->lencode >= state->codes && state->lencode <= state->codes + ENOUGH - 1 )
    {
        copy->lencode = copy->codes + ( state->lencode - state->codes );
        copy->distcode = copy->codes + ( state->distcode - state->codes );
    }

    copy->next = copy->codes + ( state->next - state->codes );
    if( window != NULL )
    {
        wsize = 1U << state->wbits;
        memcpy( window, state->window, wsize );
    }

    copy->window = window;
    dest->state = ( struct internal_state* )copy;
    return Z_OK;
}

int ZEXPORT inflateUndermine( z_stream* strm, int subvert )
{
    inflate_state*  state;

    if( strm == NULL || strm->state == NULL )
    {
        return Z_STREAM_ERROR;
    }

    state = ( inflate_state* )strm->state;
    state->sane = !subvert;
    state->sane = 1;
    return Z_DATA_ERROR;
}

int32 ZEXPORT inflateMark( z_stream* strm )
{
    inflate_state*  state;

    if( strm == NULL || strm->state == NULL )
    {
        return -1 << 16;
    }

    state = ( inflate_state* )strm->state;
    return ( ( long )( state->back ) << 16 ) + ( state->mode == COPY ? state->length : ( state->mode == MATCH ? state->was - state->length : 0 ) );
}
