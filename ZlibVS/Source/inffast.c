/* inffast.c -- fast decoding
 * Copyright (C) 1995-2008, 2010, 2013 Mark Adler
 * For conditions of distribution and use, see copyright notice in zlib.h
 */

#include "zutil.h"
#include "inftrees.h"
#include "inflate.h"
#include "inffast.h"

/*
	Decode literal, length, and distance codes and write out the resulting literal and match bytes until either not enough input or output is
	available, an end-of-block is encountered, or a data error is encountered. When large enough input and output buffers are supplied to inflate(), for
	example, a 16K input buffer and a 64K output buffer, more than 95% of the inflate execution time is spent in this routine.

	Entry assumptions:

		state->mode == LEN
		strm->avail_in >= 6
		strm->avail_out >= 258
		start >= strm->avail_out
		state->bits < 8

	On return, state->mode is one of:

		LEN -- ran out of enough output space or enough available input
		TYPE -- reached end of block code, inflate() to interpret next block
		BAD -- error in block data

	Notes:

	- The maximum input bits used by a length/distance pair is 15 bits for the length code, 5 bits for the length extra, 15 bits for the distance code,
	and 13 bits for the distance extra.  This totals 48 bits, or six bytes. Therefore if strm->avail_in >= 6, then there is enough input to avoid
	checking for available input while decoding.

	- The maximum bytes that a single length/distance pair can output is 258 bytes, which is the maximum length that can be coded.  inflate_fast()
	requires strm->avail_out >= 258 for each loop to avoid checking for output space.
*/
void inflate_fast( z_stream* strm, uint32 start )
{
    inflate_state* state;
    const uint8* in;			/* local strm->next_in */
    const uint8* last;			/* have enough input while in < last */
    uint8* out;					/* local strm->next_out */
    uint8* beg;					/* inflate()'s initial strm->next_out */
    uint8* end;					/* while out < end, enough space available */
#ifdef INFLATE_STRICT
    uint32 dmax;				/* maximum distance from zlib header */
#endif
    uint32 wsize;				/* window size or zero if not using window */
    uint32 whave;				/* valid bytes in the window */
    uint32 wnext;				/* window write index */
    uint8* window;				/* allocated sliding window, if wsize != 0 */
    uint32 hold;				/* local strm->hold */
    uint32 bits;				/* local strm->bits */
    code const* lcode;			/* local strm->lencode */
    code const* dcode;			/* local strm->distcode */
    uint32 lmask;				/* mask for first level of length codes */
    uint32 dmask;				/* mask for first level of distance codes */
    code here;					/* retrieved table entry */
    uint32 op;					/* code bits, operation, extra bits, or */

    /* window position, window bytes to copy */
    uint32 len;					/* match length, unused bytes */
    uint32 dist;				/* match distance */
    uint8* from;				/* where to copy match from */

    /* copy state to local variables */
    state = ( inflate_state* )strm->state;
    in = strm->next_in;
    last = in + ( strm->avail_in - 5 );
    out = strm->next_out;
    beg = out - ( start - strm->avail_out );
    end = out + ( strm->avail_out - 257 );
#ifdef INFLATE_STRICT
    dmax = state->dmax;
#endif
    wsize = state->wsize;
    whave = state->whave;
    wnext = state->wnext;
    window = state->window;
    hold = state->hold;
    bits = state->bits;
    lcode = state->lencode;
    dcode = state->distcode;
    lmask = ( 1 << state->lenbits ) - 1;
    dmask = ( 1 << state->distbits ) - 1;

    /* decode literals and length/distances until end-of-block or not enough input data or output space */
    do
    {
        if( bits < 15 )
        {
            hold += ( uint32 )*in++ << bits;
            bits += 8;
            hold += ( uint32 )*in++ << bits;
            bits += 8;
        }

        here = lcode[hold & lmask];
dolen:
        op = ( uint32 )here.bits;
        hold >>= op;
        bits -= op;
        op = ( uint32 )here.op;
        if( op == 0 )
        {
            /* literal */
            Tracevv( ( stderr, here.val >= 0x20 && here.val < 0x7f ? "inflate:         literal '%c'\n" : "inflate:         literal 0x%02x\n", here.val ) );
            *out++ = ( uint8 )here.val;
        }
        else if( op & 16 )
        {
            /* length base */
            len = ( uint32 )here.val;
            /* number of extra bits */
            op &= 15;
            if( op )
            {
                if( bits < op )
                {
                    hold += ( uint32 )*in++ << bits;
                    bits += 8;
                }

                len += ( uint32 )hold & ( ( 1 << op ) - 1 );
                hold >>= op;
                bits -= op;
            }

            Tracevv( ( stderr, "inflate:         length %u\n", len ) );
            if( bits < 15 )
            {
                hold += ( uint32 )*in++ << bits;
                bits += 8;
                hold += ( uint32 )*in++ << bits;
                bits += 8;
            }

            here = dcode[hold & dmask];
dodist:
            op = ( uint32 )here.bits;
            hold >>= op;
            bits -= op;
            op = ( uint32 )here.op;
            if( op & 16 )
            {
                /* distance base */
                dist = ( uint32 )here.val;
                /* number of extra bits */
                op &= 15;
                if( bits < op )
                {
                    hold += ( uint32 )*in++ << bits;
                    bits += 8;
                    if( bits < op )
                    {
                        hold += ( uint32 )*in++ << bits;
                        bits += 8;
                    }
                }

                dist += ( uint32 )hold & ( ( 1U << op ) - 1 );
#ifdef INFLATE_STRICT
                if( dist > dmax )
                {
                    strm->msg = "invalid distance too far back";
                    state->mode = BAD;
                    break;
                }
#endif
                hold >>= op;
                bits -= op;
                Tracevv( ( stderr, "inflate:         distance %u\n", dist ) );
                op = ( uint32 )( out - beg );     /* max distance in output */
                if( dist > op )
                {   /* see if copy from window */
                    op = dist - op;             /* distance back in window */
                    if( op > whave )
                    {
                        if( state->sane )
                        {
                            strm->msg =
                                ( char* )"invalid distance too far back";
                            state->mode = BAD;
                            break;
                        }
                    }
                    from = window;
                    if( wnext == 0 )
                    {   /* very common case */
                        from += wsize - op;
                        if( op < len )
                        {   /* some from window */
                            len -= op;
                            do
                            {
                                *out++ = *from++;
                            }
                            while( --op );
                            from = out - dist;  /* rest from output */
                        }
                    }
                    else if( wnext < op )
                    {   /* wrap around window */
                        from += wsize + wnext - op;
                        op -= wnext;
                        if( op < len )
                        {   /* some from end of window */
                            len -= op;
                            do
                            {
                                *out++ = *from++;
                            }
                            while( --op );

                            from = window;
                            if( wnext < len )
                            {   /* some from start of window */
                                op = wnext;
                                len -= op;
                                do
                                {
                                    *out++ = *from++;
                                }
                                while( --op );
                                from = out - dist;      /* rest from output */
                            }
                        }
                    }
                    else
                    {   /* contiguous in window */
                        from += wnext - op;
                        if( op < len )
                        {   /* some from window */
                            len -= op;
                            do
                            {
                                *out++ = *from++;
                            }
                            while( --op );
                            from = out - dist;  /* rest from output */
                        }
                    }
                    while( len > 2 )
                    {
                        *out++ = *from++;
                        *out++ = *from++;
                        *out++ = *from++;
                        len -= 3;
                    }
                    if( len )
                    {
                        *out++ = *from++;
                        if( len > 1 )
                        {
                            *out++ = *from++;
                        }
                    }
                }
                else
                {
                    from = out - dist;          /* copy direct from output */
                    do
                    {   /* minimum length is three */
                        *out++ = *from++;
                        *out++ = *from++;
                        *out++ = *from++;
                        len -= 3;
                    }
                    while( len > 2 );
                    if( len )
                    {
                        *out++ = *from++;
                        if( len > 1 )
                        {
                            *out++ = *from++;
                        }
                    }
                }
            }
            else if( ( op & 64 ) == 0 )
            {   /* 2nd level distance code */
                here = dcode[here.val + ( hold & ( ( 1U << op ) - 1 ) )];
                goto dodist;
            }
            else
            {
                strm->msg = "invalid distance code";
                state->mode = BAD;
                break;
            }
        }
        else if( ( op & 64 ) == 0 )
        {   /* 2nd level length code */
            here = lcode[here.val + ( hold & ( ( 1U << op ) - 1 ) )];
            goto dolen;
        }
        else if( op & 32 )
        {   /* end-of-block */
            Tracevv( ( stderr, "inflate:         end of block\n" ) );
            state->mode = TYPE;
            break;
        }
        else
        {
            strm->msg = "invalid literal/length code";
            state->mode = BAD;
            break;
        }
    }
    while( in < last && out < end );

    /* return unused bytes (on entry, bits < 8, so in won't go too far back) */
    len = bits >> 3;
    in -= len;
    bits -= len << 3;
    hold &= ( 1U << bits ) - 1;

    /* update state and return */
    strm->next_in = in;
    strm->next_out = out;
    strm->avail_in = ( uint32 )( in < last ? 5 + ( last - in ) : 5 - ( in - last ) );
    strm->avail_out = ( uint32 )( out < end ?
                                  257 + ( end - out ) : 257 - ( out - end ) );
    state->hold = hold;
    state->bits = bits;
    return;
}

/*
inflate_fast() speedups that turned out slower (on a PowerPC G3 750CXe):
- Using bit fields for code structure
- Different op definition to avoid & for extra bits (do & for table bits)
- Three separate decoding do-loops for direct, window, and wnext == 0
- Special case for distance > 1 copies to do overlapped load and store copy
- Explicit branch predictions (based on measured branch probabilities)
- Deferring match copy and interspersed it with decoding subsequent codes
- Swapping literal/length else
- Swapping window/direct else
- Larger unrolled copy loops (three is about right)
- Moving len -= 3 statement into middle of loop
*/
