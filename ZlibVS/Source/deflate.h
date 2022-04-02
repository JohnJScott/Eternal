/* deflate.h -- internal compression state
 * Copyright (C) 1995-2012 Jean-loup Gailly
 * For conditions of distribution and use, see copyright notice in zlib.h
 */

/* WARNING: this file should *not* be used by applications. It is
   part of the implementation of the compression library and is
   subject to change. Applications should only use zlib.h.
 */

#ifndef DEFLATE_H
#define DEFLATE_H

#include "zutil.h"

/*
 * Internal compression state.
 */

/* number of length codes, not counting the special END_BLOCK code */
#define LENGTH_CODES 29

/* number of literal bytes 0..255 */
#define LITERALS  256

/* number of Literal or Length codes, including the END_BLOCK code */
#define L_CODES ( LITERALS + 1 + LENGTH_CODES )

/* number of distance codes */
#define D_CODES   30

/* number of codes used to transfer the bit lengths */
#define BL_CODES  19

/* maximum heap size */
#define HEAP_SIZE ( 2 * L_CODES + 1 )

/* All codes must not exceed MAX_BITS bits */
#define MAX_BITS 15

/* Stream status */
#define INIT_STATE    42
#define EXTRA_STATE   69
#define NAME_STATE    73
#define COMMENT_STATE 91
#define HCRC_STATE   103
#define BUSY_STATE   113
#define FINISH_STATE 666


/* Data structure describing a single value and its code string. */
typedef struct ct_data_s
{
    union
    {
        /* frequency count */
        uint16  freq;
        /* bit string */
        uint16  code;
    } fc;
    union
    {
        /* father node in Huffman tree */
        uint16  dad;
        /* length of bit string */
        uint16  len;
    } dl;
}  ct_data;

typedef struct static_tree_desc_s  static_tree_desc;

typedef struct tree_desc_s
{
    /* the dynamic tree */
    ct_data* dyn_tree;
    /* largest code with non zero frequency */
    int32     max_code;
    /* the corresponding static tree */
    static_tree_desc* stat_desc;
}  tree_desc;

/* A uint16 is an index in the character window. We use short instead of int to save space in the various tables. uint32 is used only for parameter passing. */
typedef struct internal_state
{
    /* pointer back to this zlib stream */
    z_stream* strm;
    /* as the name implies */
    int32 status;
    /* output still pending */
    uint8* pending_buf;
    /* size of pending_buf */
    uint32 pending_buf_size;
    /* next pending byte to output to the stream */
    uint8* pending_out;
    /* nb of bytes in the pending buffer */
    uint32 pending;
    /* bit 0 true for zlib, bit 1 true for gzip */
    int32 wrap;
    /* value of flush param for previous deflate call */
    int32 last_flush;

    /* used by deflate.c: */

    /* LZ77 window size (32K by default) */
    uint32  w_size;
    /* log2(w_size)  (8..16) */
    uint32  w_bits;
    /* w_size - 1 */
    uint32  w_mask;

    /* Sliding window. Input bytes are read into the second half of the window, and move to the first half later to keep a dictionary of at least wSize
    * bytes. With this organization, matches are limited to a distance of wSize-MAX_MATCH bytes, but this ensures that IO is always
    * performed with a length multiple of the block size. Also, it limits the window size to 64K, which is quite useful on MSDOS.
    * To do: use the user input buffer as sliding window. */
    uint8* window;

    /* Actual size of window: 2*wSize, except when the user input buffer is directly used as sliding window. */
    uint32 window_size;

    /* Link to older string with same hash index. To limit the size of this array to 64K, this link is maintained only for the last 32K strings.
    * An index in this array is thus a window index modulo 32K. */
    uint16* prev;

    /* Heads of the hash chains or 0. */
    uint16* head;

    /* hash index of string to be inserted */
    uint32  ins_h;
    /* number of elements in hash table */
    uint32  hash_size;
    /* log2(hash_size) */
    uint32  hash_bits;
    /* hash_size-1 */
    uint32  hash_mask;

    /* Number of bits by which ins_h must be shifted at each input step. It must be such that after MIN_MATCH steps, the oldest
    * byte no longer takes part in the hash key, that is: hash_shift * MIN_MATCH >= hash_bits */
    uint32  hash_shift;

    /* Window position at the beginning of the current output block. Gets negative when the window is moved backwards. */
    int32 block_start;

    /* length of best match */
    uint32 match_length;
    /* previous match */
    uint32 prev_match;
    /* set if previous match exists */
    int32 match_available;
    /* start of string to insert */
    uint32 strstart;
    /* start of matching string */
    uint32 match_start;
    /* number of valid bytes ahead in window */
    uint32 lookahead;

    /* Length of the best match at previous step. Matches not greater than this are discarded. This is used in the lazy match evaluation. */
    uint32 prev_length;

    /* To speed up deflation, hash chains are never searched beyond this length.  A higher limit improves compression ratio but degrades the speed. */
    uint32 max_chain_length;

    /* Attempt to find a better match only when the current match is strictly smaller than this value. This mechanism is used only for compression
    * levels >= 4. */
    uint32 max_lazy_match;

    /* Insert new strings in the hash table only if the match length is not
    * greater than this length. This saves time but degrades compression.
    * max_insert_length is used only for compression levels <= 3.
    */
#   define max_insert_length  max_lazy_match

    /* compression level (1..9) */
    int32 level;
    /* favor or force Huffman coding*/
    int32 strategy;

    /* Use a faster search when the previous match is longer than this */
    uint32 good_match;

    /* Stop searching when current match exceeds this */
    int32 nice_match;

    /* used by trees.c: */
    /* Didn't use ct_data typedef below to suppress compiler warning */
    struct ct_data_s dyn_ltree[HEAP_SIZE];   /* literal and length tree */
    struct ct_data_s dyn_dtree[2 * D_CODES + 1]; /* distance tree */
    struct ct_data_s bl_tree[2 * BL_CODES + 1];  /* Huffman tree for bit lengths */

    /* desc. for literal tree */
    struct tree_desc_s l_desc;
    /* desc. for distance tree */
    struct tree_desc_s d_desc;
    /* desc. for bit length tree */
    struct tree_desc_s bl_desc;

    /* number of codes at each bit length for an optimal tree */
    uint16 bl_count[MAX_BITS + 1];

    /* heap used to build the Huffman trees */
    int32 heap[2 * L_CODES + 1];
    /* number of elements in the heap */
    int32 heap_len;
    /* element of largest frequency */
    int32 heap_max;
    /* The sons of heap[n] are heap[2*n] and heap[2*n+1]. heap[0] is not used. The same heap array is used to build all trees. */

    /* Depth of each subtree used as tie breaker for trees of equal frequency */
    uint8 depth[2 * L_CODES + 1];

    /* buffer for literals or lengths */
    uint8* l_buf;

    uint32  lit_bufsize;
    /* Size of match buffer for literals/lengths.  There are 4 reasons for
    * limiting lit_bufsize to 64K:
    *   - frequencies can be kept in 16 bit counters
    *   - if compression is not successful for the first block, all input data is still in the window so we can still emit a stored block even
    *     when input comes from standard input.  (This can also be done for all blocks if lit_bufsize is not greater than 32K.)
    *   - if compression is not successful for a file smaller than 64K, we can even emit a stored file instead of a stored block (saving 5 bytes).
    *     This is applicable only for zip (not gzip or zlib).
    *   - creating new Huffman trees less frequently may not provide fast adaptation to changes in the input data statistics. (Take for
    *     example a binary file with poorly compressible code followed by a highly compressible string table.) Smaller buffer sizes give
    *     fast adaptation but have of course the overhead of transmitting trees more frequently.
    */

    /* running index in l_buf */
    uint32 last_lit;

    /* Buffer for distances. To simplify the code, d_buf and l_buf have the same number of elements. To use different lengths, an extra flag
    * array would be necessary. */
    uint16* d_buf;

    /* bit length of current block with optimal trees */
    uint32 opt_len;
    /* bit length of current block with static trees */
    uint32 static_len;
    /* number of string matches in current block */
    uint32 matches;
    /* bytes at end of window left to insert */
    uint32 insert;

#ifdef _DEBUG
    /* total bit length of compressed file mod 2^32 */
    uint32 compressed_len;
    /* bit length of compressed data sent mod 2^32 */
    uint32 bits_sent;
#endif

    /* Output buffer. bits are inserted starting at the bottom (least significant bits). */
    uint16 bi_buf;

    /* Number of valid bits in bi_buf.  All bits above the last valid bit are always zero. */
    int32 bi_valid;

    /* High water mark offset in window for initialized bytes -- bytes above this are set to zero in order to avoid memory check warnings when
    * longest match routines access bytes past the input.  This is then updated to the new high water mark. */
    uint32 high_water;

}  deflate_state;

/* Output a byte on the stream.
 * IN assertion: there is enough room in pending_buf.
 */
#define put_byte(s, c) {s->pending_buf[s->pending++] = (c);}

/* Minimum amount of lookahead, except at the end of the input file. See deflate.c for comments about the MIN_MATCH+1. */
#define MIN_LOOKAHEAD (MAX_MATCH+MIN_MATCH+1)

/* In order to simplify the code, particularly on 16 bit machines, match distances are limited to MAX_DIST instead of WSIZE. */
#define MAX_DIST(s)  ((s)->w_size-MIN_LOOKAHEAD)

/* Number of bytes after end of data in window to initialize in order to avoid memory checker errors from longest match routines */
#define WIN_INIT MAX_MATCH

/* in trees.c */
void _tr_init( deflate_state* s );
int32 _tr_tally( deflate_state* s, uint32 dist, uint32 lc );
void _tr_flush_block( deflate_state* s, int8* buf, uint32 stored_len, int32 last );
void _tr_flush_bits( deflate_state* s );
void _tr_align( deflate_state* s );
void _tr_stored_block( deflate_state* s, int8* buf, uint32 stored_len, int32 last );

/* Mapping from a distance to a distance code. dist is the distance - 1 and must not have side effects. _dist_code[256] and _dist_code[257] are never used. */
#define d_code(dist) ((dist) < 256 ? _dist_code[dist] : _dist_code[256+((dist)>>7)])

# define _tr_tally_lit(s, c, flush) flush = _tr_tally(s, 0, c)
# define _tr_tally_dist(s, distance, length, flush) flush = _tr_tally(s, distance, length)

#endif /* DEFLATE_H */
