/* zlib.h -- interface of the 'zlib' general purpose compression library
  version 1.2.8, April 28th, 2013

  Copyright (C) 1995-2013 Jean-loup Gailly and Mark Adler

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jean-loup Gailly        Mark Adler
  jloup@gzip.org          madler@alumni.caltech.edu


  The data format used by the zlib library is described by RFCs (Request for
  Comments) 1950 to 1952 in the files http://tools.ietf.org/html/rfc1950
  (zlib format), rfc1951 (deflate format) and rfc1952 (gzip format).
*/

#ifndef ZLIB_H
#define ZLIB_H

#include "zconf.h"

#ifdef __cplusplus
extern "C" {
#endif

#define ZLIB_VERSION "1.2.8"
#define ZLIB_VERNUM 0x1280
#define ZLIB_VER_MAJOR 1
#define ZLIB_VER_MINOR 2
#define ZLIB_VER_REVISION 8
#define ZLIB_VER_SUBREVISION 0

/*
    The 'zlib' compression library provides in-memory compression and decompression functions, including integrity checks of the uncompressed data.
  This version of the library supports only one compression method (deflation) but other algorithms will be added later and will have the same stream
  interface.

    Compression can be done in a single step if the buffers are large enough, or can be done by repeated calls of the compression function.  In the latter
  case, the application must provide more input and/or consume the output (providing more output space) before each call.

    The compressed data format used by default by the in-memory functions is the zlib format, which is a zlib wrapper documented in RFC 1950, wrapped
  around a deflate stream, which is itself documented in RFC 1951.

    The zlib format was designed to be compact and fast for use in memory and on communications channels.

    The library does not install any signal handler.  The decoder checks the consistency of the compressed data, so the library should never crash
  even in case of corrupted input.
*/

typedef void* ( *alloc_func )( size_t size );
typedef void ( *free_func )( void* address );

typedef struct z_stream_s
{
    uint32 stream_size;				/* sizeof this structure */
    const char* version;			/* version the caller is expecting. Only major version is checked */
    int32 windowBits;				/* the size of the window - default MAX_WBITS */
    int32 memLevel;					/* the amount of memory to use during deflation - default MAX_MEM_LEVEL */
    int32 strategy;					/* the deflation strategy to use - default Z_DEFAULT_STRATEGY */
    const char* msg;				/* last error message, NULL if no error */

    const uint8* next_in;			/* next input byte */
    uint32 avail_in;				/* number of bytes available at next_in */
    uint32 total_in;				/* total number of input bytes read so far */

    uint8* next_out;				/* next output byte should be put there */
    uint32 avail_out;				/* remaining free space at next_out */
    uint32 total_out;				/* total number of bytes output so far */

    struct internal_state* state;	/* not visible by applications */

    alloc_func zalloc;				/* used to allocate the internal state */
    free_func zfree;				/* used to free the internal state */

    uint32 adler;					/* adler32 value of the uncompressed data */
} z_stream;

/*
     The application must update next_in and avail_in when avail_in has dropped to zero.  It must update next_out and avail_out when avail_out has dropped
   to zero.  The application must initialize zalloc, zfree and opaque before calling the init function.  All other fields are set by the compression
   library and must not be updated by the application.

     zalloc must return NULL if there is not enough memory for the object.  If zlib is used in a multi-threaded application, zalloc and zfree must be
   thread safe.

     The fields total_in and total_out can be used for statistics or progress reports.  After compression, total_in holds the total size of the
   uncompressed data and may be saved for use in the decompressor (particularly if the decompressor wants to decompress everything in a single step).
*/

/* constants */

/* Allowed flush values; see deflate() and inflate() below for details */
#define Z_NO_FLUSH				0
#define Z_PARTIAL_FLUSH			1
#define Z_SYNC_FLUSH			2
#define Z_FULL_FLUSH			3
#define Z_FINISH				4
#define Z_BLOCK					5
#define Z_TREES					6

/* Return codes for the compression/decompression functions. Negative values are errors, positive values are used for special but normal events. */
#define Z_OK					0
#define Z_STREAM_END			1
#define Z_NEED_DICT				2
#define Z_ERRNO					-1
#define Z_STREAM_ERROR			-2
#define Z_DATA_ERROR			-3
#define Z_MEM_ERROR				-4
#define Z_BUF_ERROR				-5
#define Z_VERSION_ERROR			-6

/* compression levels */
#define Z_NO_COMPRESSION		0
#define Z_BEST_SPEED			1
#define Z_BEST_COMPRESSION		9
#define Z_DEFAULT_COMPRESSION	-1

/* compression strategy; see deflateInit() below for details */
#define Z_FILTERED				1
#define Z_HUFFMAN_ONLY			2
#define Z_RLE					3
#define Z_FIXED					4
#define Z_DEFAULT_STRATEGY		0

/* The deflate compression method (the only one supported in this version) */
#define Z_DEFLATED				8

/* basic functions */

/* The application can compare zlibVersion and ZLIB_VERSION for consistency. If the first character differs, the library code actually used is not
   compatible with the zlib.h header file used by the application.  This check is automatically made by deflateInit and inflateInit.
 */
ZEXTERN const char* ZEXPORT zlibVersion();

/*
     Initializes the internal stream state for compression.  The fields zalloc and zfree must be initialized before by the caller.  If
   zalloc and zfree are set to NULL, deflateInit updates them to use default allocation functions.

     The compression level must be Z_DEFAULT_COMPRESSION, or between 0 and 9: 1 gives best speed, 9 gives best compression, 0 gives no compression at all
   (the input data is simply copied a block at a time).  Z_DEFAULT_COMPRESSION requests a default compromise between speed and compression (currently
   equivalent to level 6).

     deflateInit returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_STREAM_ERROR if level is not a valid compression level, or
   Z_VERSION_ERROR if the zlib library version (zlib_version) is incompatible with the version assumed by the caller (ZLIB_VERSION).  msg is set to null
   if there is no error message.  deflateInit does not perform any compression: this will be done by deflate().
*/
ZEXTERN int32 ZEXPORT deflateInit( z_stream* strm, int32 level );

/*
    deflate compresses as much data as possible, and stops when the input buffer becomes empty or the output buffer becomes full.  It may introduce
  some output latency (reading input without producing any output) except when forced to flush.

    The detailed semantics are as follows.  deflate performs one or both of the following actions:

  - Compress more input starting at next_in and update next_in and avail_in accordingly.  If not all input can be processed (because there is not
    enough room in the output buffer), next_in and avail_in are updated and processing will resume at this point for the next call of deflate().

  - Provide more output starting at next_out and update next_out and avail_out accordingly.  This action is forced if the parameter flush is non zero.
    Forcing flush frequently degrades the compression ratio, so this parameter should be set only when necessary (in interactive applications).  Some
    output may be provided even if flush is not set.

    Before the call of deflate(), the application should ensure that at least one of the actions is possible, by providing more input and/or consuming more
  output, and updating avail_in or avail_out accordingly; avail_out should never be zero before the call.  The application can consume the compressed
  output when it wants, for example when the output buffer is full (avail_out == 0), or after each call of deflate().  If deflate returns Z_OK and with
  zero avail_out, it must be called again after making room in the output buffer because there might be more output pending.

    Normally the parameter flush is set to Z_NO_FLUSH, which allows deflate to decide how much data to accumulate before producing output, in order to
  maximize compression.

    If the parameter flush is set to Z_SYNC_FLUSH, all pending output is flushed to the output buffer and the output is aligned on a byte boundary, so
  that the decompressor can get all input data available so far.  (In particular avail_in is zero after the call if enough output space has been
  provided before the call.) Flushing may degrade compression for some compression algorithms and so it should be used only when necessary.  This
  completes the current deflate block and follows it with an empty stored block that is three bits plus filler bits to the next byte, followed by four bytes
  (00 00 ff ff).

    If flush is set to Z_PARTIAL_FLUSH, all pending output is flushed to the output buffer, but the output is not aligned to a byte boundary.  All of the
  input data so far will be available to the decompressor, as for Z_SYNC_FLUSH.  This completes the current deflate block and follows it with an empty fixed
  codes block that is 10 bits long.  This assures that enough bytes are output in order for the decompressor to finish the block before the empty fixed code
  block.

    If flush is set to Z_BLOCK, a deflate block is completed and emitted, as for Z_SYNC_FLUSH, but the output is not aligned on a byte boundary, and up to
  seven bits of the current block are held to be written as the next byte after the next deflate block is completed.  In this case, the decompressor may not
  be provided enough bits at this point in order to complete decompression of the data provided so far to the compressor.  It may need to wait for the next
  block to be emitted.  This is for advanced applications that need to control the emission of deflate blocks.

    If flush is set to Z_FULL_FLUSH, all output is flushed as with Z_SYNC_FLUSH, and the compression state is reset so that decompression can
  restart from this point if previous compressed data has been damaged or if random access is desired.  Using Z_FULL_FLUSH too often can seriously degrade
  compression.

    If deflate returns with avail_out == 0, this function must be called again with the same value of the flush parameter and more output space (updated
  avail_out), until the flush is complete (deflate returns with non-zero avail_out).  In the case of a Z_FULL_FLUSH or Z_SYNC_FLUSH, make sure that
  avail_out is greater than six to avoid repeated flush markers due to avail_out == 0 on return.

    If the parameter flush is set to Z_FINISH, pending input is processed, pending output is flushed and deflate returns with Z_STREAM_END if there was
  enough output space; if deflate returns with Z_OK, this function must be called again with Z_FINISH and more output space (updated avail_out) but no
  more input data, until it returns with Z_STREAM_END or an error.  After deflate has returned Z_STREAM_END, the only possible operations on the stream
  are deflateReset or deflateEnd.

    Z_FINISH can be used immediately after deflateInit if all the compression is to be done in a single step.  In this case, avail_out must be at least the
  value returned by deflateBound (see below).  Then deflate is guaranteed to return Z_STREAM_END.  If not enough output space is provided, deflate will
  not return Z_STREAM_END, and it must be called again as described above.

    deflate() sets strm->adler to the adler32 checksum of all input read so far (that is, total_in bytes).

    deflate() may update strm->data_type if it can make a good guess about the input data type (Z_BINARY or Z_TEXT).  In doubt, the data is considered
  binary.  This field is only for information purposes and does not affect the compression algorithm in any manner.

    deflate() returns Z_OK if some progress has been made (more input processed or more output produced), Z_STREAM_END if all input has been
  consumed and all output has been produced (only when flush is set to Z_FINISH), Z_STREAM_ERROR if the stream state was inconsistent (for example
  if next_in or next_out was NULL), Z_BUF_ERROR if no progress is possible (for example avail_in or avail_out was zero).  Note that Z_BUF_ERROR is not
  fatal, and deflate() can be called again with more input and more output space to continue compressing.
*/
ZEXTERN int32 ZEXPORT deflate( z_stream* strm, int32 flush );

/*
     All dynamically allocated data structures for this stream are freed. This function discards any unprocessed input and does not flush any pending
   output.

     deflateEnd returns Z_OK if success, Z_STREAM_ERROR if the stream state was inconsistent, Z_DATA_ERROR if the stream was freed
   prematurely (some input or output was discarded).  In the error case, msg may be set but then points to a static string (which must not be
   deallocated).
*/
ZEXTERN int32 ZEXPORT deflateEnd( z_stream* strm );

/*
     Initializes the internal stream state for decompression.  The fields next_in, avail_in, zalloc and zfree must be initialized before by
   the caller.  If next_in is not NULL and avail_in is large enough (the exact value depends on the compression method), inflateInit determines the
   compression method from the zlib header and allocates all data structures accordingly; otherwise the allocation will be deferred to the first call of
   inflate.  If zalloc and zfree are set to NULL, inflateInit updates them to use default allocation functions.

     inflateInit returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_VERSION_ERROR if the zlib library version is incompatible with the
   version assumed by the caller, or Z_STREAM_ERROR if the parameters are invalid, such as a null pointer to the structure.  msg is set to null if
   there is no error message.  inflateInit does not perform any decompression apart from possibly reading the zlib header if present: actual decompression
   will be done by inflate().  (So next_in and avail_in may be modified, but next_out and avail_out are unused and unchanged.) The current implementation
   of inflateInit() does not process any header information -- that is deferred until inflate() is called.
*/
ZEXTERN int32 ZEXPORT inflateInit( z_stream* strm );


/*
    inflate decompresses as much data as possible, and stops when the input buffer becomes empty or the output buffer becomes full.  It may introduce
  some output latency (reading input without producing any output) except when forced to flush.

  The detailed semantics are as follows.  inflate performs one or both of the following actions:

  - Decompress more input starting at next_in and update next_in and avail_in accordingly.  If not all input can be processed (because there is not
    enough room in the output buffer), next_in is updated and processing will resume at this point for the next call of inflate().

  - Provide more output starting at next_out and update next_out and avail_out accordingly.  inflate() provides as much output as possible, until there is
    no more input data or no more space in the output buffer (see below about the flush parameter).

    Before the call of inflate(), the application should ensure that at least one of the actions is possible, by providing more input and/or consuming more
  output, and updating the next_* and avail_* values accordingly.  The application can consume the uncompressed output when it wants, for example
  when the output buffer is full (avail_out == 0), or after each call of inflate().  If inflate returns Z_OK and with zero avail_out, it must be
  called again after making room in the output buffer because there might be more output pending.

    The flush parameter of inflate() can be Z_NO_FLUSH, Z_SYNC_FLUSH, Z_FINISH, Z_BLOCK, or Z_TREES.  Z_SYNC_FLUSH requests that inflate() flush as much
  output as possible to the output buffer.  Z_BLOCK requests that inflate() stop if and when it gets to the next deflate block boundary.  When decoding
  the zlib or gzip format, this will cause inflate() to return immediately after the header and before the first block.  When doing a raw inflate,
  inflate() will go ahead and process the first block, and will return when it gets to the end of that block, or when it runs out of data.

    The Z_BLOCK option assists in appending to or combining deflate streams. Also to assist in this, on return inflate() will set strm->data_type to the
  number of unused bits in the last byte taken from strm->next_in, plus 64 if inflate() is currently decoding the last block in the deflate stream, plus
  128 if inflate() returned immediately after decoding an end-of-block code or decoding the complete header up to just before the first byte of the deflate
  stream.  The end-of-block will not be indicated until all of the uncompressed data from that block has been written to strm->next_out.  The number of
  unused bits may in general be greater than seven, except when bit 7 of data_type is set, in which case the number of unused bits will be less than
  eight.  data_type is set as noted here every time inflate() returns for all flush options, and so can be used to determine the amount of currently
  consumed input in bits.

    The Z_TREES option behaves as Z_BLOCK does, but it also returns when the end of each deflate block header is reached, before any actual data in that
  block is decoded.  This allows the caller to determine the length of the deflate block header for later use in random access within a deflate block.
  256 is added to the value of strm->data_type when inflate() returns immediately after reaching the end of the deflate block header.

    inflate() should normally be called until it returns Z_STREAM_END or an error.  However if all decompression is to be performed in a single step (a
  single call of inflate), the parameter flush should be set to Z_FINISH.  In this case all pending input is processed and all pending output is flushed;
  avail_out must be large enough to hold all of the uncompressed data for the operation to complete.  (The size of the uncompressed data may have been
  saved by the compressor for this purpose.) The use of Z_FINISH is not required to perform an inflation in one step.  However it may be used to
  inform inflate that a faster approach can be used for the single inflate() call.  Z_FINISH also informs inflate to not maintain a sliding window if the
  stream completes, which reduces inflate's memory footprint.  If the stream does not complete, either because not all of the stream is provided or not
  enough output space is provided, then a sliding window will be allocated and inflate() can be called again to continue the operation as if Z_NO_FLUSH had
  been used.

     In this implementation, inflate() always flushes as much output as possible to the output buffer, and always uses the faster approach on the
  first call.  So the effects of the flush parameter in this implementation are on the return value of inflate() as noted below, when inflate() returns early
  when Z_BLOCK or Z_TREES is used, and when inflate() avoids the allocation of memory for a sliding window when Z_FINISH is used.

     If a preset dictionary is needed after this call (see inflateSetDictionary below), inflate sets strm->adler to the Adler-32 checksum of the dictionary
  chosen by the compressor and returns Z_NEED_DICT; otherwise it sets strm->adler to the Adler-32 checksum of all output produced so far (that is,
  total_out bytes) and returns Z_OK, Z_STREAM_END or an error code as described below.  At the end of the stream, inflate() checks that its computed adler32
  checksum is equal to that saved by the compressor and returns Z_STREAM_END only if the checksum is correct.

    inflate() can decompress and check either zlib-wrapped or gzip-wrapped deflate data.  The header type is detected automatically, if requested when
  initializing with inflateInit().  Any information contained in the gzip header is not retained, so applications that need that information should
  instead use raw inflate, see inflateInit2() below, or inflateBack() and perform their own processing of the gzip header and trailer.  When processing
  gzip-wrapped deflate data, strm->adler32 is set to the CRC-32 of the output producted so far.  The CRC-32 is checked against the gzip trailer.

    inflate() returns Z_OK if some progress has been made (more input processed or more output produced), Z_STREAM_END if the end of the compressed data has
  been reached and all uncompressed output has been produced, Z_NEED_DICT if a preset dictionary is needed at this point, Z_DATA_ERROR if the input data was
  corrupted (input stream not conforming to the zlib format or incorrect check value), Z_STREAM_ERROR if the stream structure was inconsistent (for example
  next_in or next_out was NULL), Z_MEM_ERROR if there was not enough memory, Z_BUF_ERROR if no progress is possible or if there was not enough room in the
  output buffer when Z_FINISH is used.  Note that Z_BUF_ERROR is not fatal, and inflate() can be called again with more input and more output space to
  continue decompressing.  If Z_DATA_ERROR is returned, the application may then call inflateSync() to look for a good compression block if a partial
  recovery of the data is desired.
*/
ZEXTERN int32 ZEXPORT inflate( z_stream* strm, int32 flush );

/*
     All dynamically allocated data structures for this stream are freed. This function discards any unprocessed input and does not flush any pending
   output.

     inflateEnd returns Z_OK if success, Z_STREAM_ERROR if the stream state was inconsistent.  In the error case, msg may be set but then points to a
   static string (which must not be deallocated).
*/
ZEXTERN int32 ZEXPORT inflateEnd( z_stream* strm );


/* Advanced functions */

/*
    The following functions are needed only in some special applications.
*/

/*
     Initializes the compression dictionary from the given byte sequence without producing any compressed output.  When using the zlib format, this
   function must be called immediately after deflateInit or deflateReset, and before any call of deflate.  When doing raw deflate, this
   function must be called either before any call of deflate, or immediately after the completion of a deflate block, i.e. after all input has been
   consumed and all output has been delivered when using any of the flush options Z_BLOCK, Z_PARTIAL_FLUSH, Z_SYNC_FLUSH, or Z_FULL_FLUSH.  The
   compressor and decompressor must use exactly the same dictionary (see inflateSetDictionary).

     The dictionary should consist of strings (byte sequences) that are likely to be encountered later in the data to be compressed, with the most commonly
   used strings preferably put towards the end of the dictionary.  Using a dictionary is most useful when the data to be compressed is short and can be
   predicted with good accuracy; the data can then be compressed better than with the default empty dictionary.

     Depending on the size of the compression data structures selected by deflateInit, a part of the dictionary may in effect be
   discarded, for example if the dictionary is larger than the window size provided in deflateInit.  Thus the strings most likely to be
   useful should be put at the end of the dictionary, not at the front.  In addition, the current implementation of deflate will use at most the window
   size minus 262 bytes of the provided dictionary.

     Upon return of this function, strm->adler is set to the adler32 value of the dictionary; the decompressor may later use this value to determine
   which dictionary has been used by the compressor.  (The adler32 value applies to the whole dictionary even if only a subset of the dictionary is
   actually used by the compressor.) If a raw deflate was requested, then the adler32 value is not computed and strm->adler is not set.

     deflateSetDictionary returns Z_OK if success, or Z_STREAM_ERROR if a parameter is invalid (e.g.  dictionary being NULL) or the stream state is
   inconsistent (for example if deflate has already been called for this stream or if not at a block boundary for raw deflate).  deflateSetDictionary does
   not perform any compression: this will be done by deflate().
*/
ZEXTERN int32 ZEXPORT deflateSetDictionary( z_stream* strm, const uint8* dictionary, uint32 dictLength );

/*
     Sets the destination stream as a complete copy of the source stream.

     This function can be useful when several compression strategies will be tried, for example when there are several ways of pre-processing the input
   data with a filter.  The streams that will be discarded should then be freed by calling deflateEnd.  Note that deflateCopy duplicates the internal
   compression state which can be quite large, so this strategy is slow and can consume lots of memory.

     deflateCopy returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_STREAM_ERROR if the source stream state was inconsistent
   (such as zalloc being NULL).  msg is left unchanged in both source and destination.
*/
ZEXTERN int32 ZEXPORT deflateCopy( z_stream* dest, z_stream* source );

/*
     This function is equivalent to deflateEnd followed by deflateInit, but does not free and reallocate all the internal compression state.  The
   stream will keep the same compression level and any other attributes that may have been set by deflateInit2.

     deflateReset returns Z_OK if success, or Z_STREAM_ERROR if the source stream state was inconsistent (such as zalloc or state being NULL).
*/
ZEXTERN int32 ZEXPORT deflateReset( z_stream* strm );

/*
     Dynamically update the compression level and compression strategy.  The interpretation of level and strategy is as in deflateInit2.  This can be
   used to switch between compression and straight copy of the input data, or to switch to a different kind of input data requiring a different strategy.
   If the compression level is changed, the input available so far is compressed with the old level (and may be flushed); the new level will take
   effect only at the next call of deflate().

     Before the call of deflateParams, the stream state must be set as for a call of deflate(), since the currently available input may have to be
   compressed and flushed.  In particular, strm->avail_out must be non-zero.

     deflateParams returns Z_OK if success, Z_STREAM_ERROR if the source stream state was inconsistent or if a parameter was invalid, Z_BUF_ERROR if
   strm->avail_out was zero.
*/
ZEXTERN int32 ZEXPORT deflateParams( z_stream* strm, int32 level, int32 strategy );

/*
     Fine tune deflate's internal compression parameters.  This should only be used by someone who understands the algorithm used by zlib's deflate for
   searching for the best matching string, and even then only by the most fanatic optimizer trying to squeeze out the last compressed bit for their
   specific input data.  Read the deflate.c source code for the meaning of the max_lazy, good_length, nice_length, and max_chain parameters.

     deflateTune() can be called after deflateInit(), and returns Z_OK on success, or Z_STREAM_ERROR for an invalid deflate stream.
 */
ZEXTERN int32 ZEXPORT deflateTune( z_stream* strm, int32 good_length, int32 max_lazy, int32 nice_length, int32 max_chain );

/*
     deflateBound() returns an upper bound on the compressed size after deflation of sourceLen bytes.  It must be called after deflateInit(),
   and after deflateSetHeader(), if used.  This would be used to allocate an output buffer for deflation in a single pass, and so would be
   called before deflate().  If that first deflate() call is provided the sourceLen input bytes, an output buffer allocated to the size returned by
   deflateBound(), and the flush value Z_FINISH, then deflate() is guaranteed to return Z_STREAM_END.  Note that it is possible for the compressed size to
   be larger than the value returned by deflateBound() if flush options other than Z_FINISH or Z_NO_FLUSH are used.
*/
ZEXTERN uint32 ZEXPORT deflateBound( z_stream* strm, uint32 sourceLen );

/*
     deflatePending() returns the number of bytes and bits of output that have been generated, but not yet provided in the available output.  The bytes not
   provided would be due to the available output space having being consumed. The number of bits of output not provided are between 0 and 7, where they
   await more bits to join them in order to fill out a full byte.  If pending or bits are NULL, then those values are not set.

     deflatePending returns Z_OK if success, or Z_STREAM_ERROR if the source stream state was inconsistent.
 */
ZEXTERN int32 ZEXPORT deflatePending( z_stream* strm, uint32* pending, int32* bits );

/*
     deflatePrime() inserts bits in the deflate output stream.  The intent is that this function is used to start off the deflate output with the bits
   leftover from a previous deflate stream when appending to it.  As such, this function can only be used for raw deflate, and must be used before the first
   deflate() call after a deflateReset().  bits must be less than or equal to 16, and that many of the least significant bits of value
   will be inserted in the output.

     deflatePrime returns Z_OK if success, Z_BUF_ERROR if there was not enough room in the internal buffer to insert the bits, or Z_STREAM_ERROR if the
   source stream state was inconsistent.
*/
ZEXTERN int32 ZEXPORT deflatePrime( z_stream* strm, int32 bits, int32 value );

/*
     Initializes the decompression dictionary from the given uncompressed byte sequence.  This function must be called immediately after a call of inflate,
   if that call returned Z_NEED_DICT.  The dictionary chosen by the compressor can be determined from the adler32 value returned by that call of inflate.
   The compressor and decompressor must use exactly the same dictionary (see deflateSetDictionary).  For raw inflate, this function can be called at any
   time to set the dictionary.  If the provided dictionary is smaller than the window and there is already data in the window, then the provided dictionary
   will amend what's there.  The application must insure that the dictionary that was used for compression is provided.

     inflateSetDictionary returns Z_OK if success, Z_STREAM_ERROR if a parameter is invalid (e.g.  dictionary being NULL) or the stream state is
   inconsistent, Z_DATA_ERROR if the given dictionary doesn't match the expected one (incorrect adler32 value).  inflateSetDictionary does not
   perform any decompression: this will be done by subsequent calls of
   inflate().
*/
ZEXTERN int32 ZEXPORT inflateSetDictionary( z_stream* strm, const uint8* dictionary, uint32 dictLength );

/*
     Returns the sliding dictionary being maintained by inflate.  dictLength is set to the number of bytes in the dictionary, and that many bytes are copied
   to dictionary.  dictionary must have enough space, where 32768 bytes is always enough.  If inflateGetDictionary() is called with dictionary equal to
   NULL, then only the dictionary length is returned, and nothing is copied. Similary, if dictLength is NULL, then it is not set.

     inflateGetDictionary returns Z_OK on success, or Z_STREAM_ERROR if the stream state is inconsistent.
*/
ZEXTERN int32 ZEXPORT inflateGetDictionary( z_stream* strm, uint8* dictionary, uint32* dictLength );

/*
     Skips invalid compressed data until a possible full flush point (see above for the description of deflate with Z_FULL_FLUSH) can be found, or until all
   available input is skipped.  No output is provided.

     inflateSync searches for a 00 00 FF FF pattern in the compressed data. All full flush points have this pattern, but not all occurrences of this
   pattern are full flush points.

     inflateSync returns Z_OK if a possible full flush point has been found, Z_BUF_ERROR if no more input was provided, Z_DATA_ERROR if no flush point
   has been found, or Z_STREAM_ERROR if the stream structure was inconsistent. In the success case, the application may save the current current value of
   total_in which indicates where valid compressed data was found.  In the error case, the application may repeatedly call inflateSync, providing more
   input each time, until success or end of the input data.
*/
ZEXTERN int32 ZEXPORT inflateSync( z_stream* strm );

/*
     Sets the destination stream as a complete copy of the source stream.

     This function can be useful when randomly accessing a large stream.  The first pass through the stream can periodically record the inflate state,
   allowing restarting inflate at those points when randomly accessing the stream.

     inflateCopy returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_STREAM_ERROR if the source stream state was inconsistent
   (such as zalloc being NULL).  msg is left unchanged in both source and destination.
*/
ZEXTERN int32 ZEXPORT inflateCopy( z_stream* dest, z_stream* source );

/*
     This function is equivalent to inflateEnd followed by inflateInit, but does not free and reallocate all the internal decompression state.  The
   stream will keep attributes that may have been set by inflateInit.

     inflateReset returns Z_OK if success, or Z_STREAM_ERROR if the source stream state was inconsistent (such as zalloc or state being NULL).
*/
ZEXTERN int32 ZEXPORT inflateReset( z_stream* strm );

/*
     This function is the same as inflateReset, but it also permits changing the wrap and window size requests.  The windowBits parameter is interpreted
   the same as it is for inflateInit.

     inflateReset2 returns Z_OK if success, or Z_STREAM_ERROR if the source stream state was inconsistent (such as zalloc or state being NULL), or if
   the windowBits parameter is invalid.
*/
ZEXTERN int32 ZEXPORT inflateReset2( z_stream* strm, int32 windowBits );

/*
     This function inserts bits in the inflate input stream.  The intent is that this function is used to start inflating at a bit position in the
   middle of a byte.  The provided bits will be used before any bytes are used from next_in.  This function should only be used with raw inflate, and
   should be used before the first inflate() call after inflateInit() or inflateReset().  bits must be less than or equal to 16, and that many of the
   least significant bits of value will be inserted in the input.

     If bits is negative, then the input stream bit buffer is emptied.  Then inflatePrime() can be called again to put bits in the buffer.  This is used
   to clear out bits leftover after feeding inflate a block description prior to feeding inflate codes.

     inflatePrime returns Z_OK if success, or Z_STREAM_ERROR if the source stream state was inconsistent.
*/
ZEXTERN int32 ZEXPORT inflatePrime( z_stream* strm, int32 bits, int32 value );

/*
     This function returns two values, one in the lower 16 bits of the return value, and the other in the remaining upper bits, obtained by shifting the
   return value down 16 bits.  If the upper value is -1 and the lower value is zero, then inflate() is currently decoding information outside of a block.
   If the upper value is -1 and the lower value is non-zero, then inflate is in the middle of a stored block, with the lower value equaling the number of
   bytes from the input remaining to copy.  If the upper value is not -1, then it is the number of bits back from the current bit position in the input of
   the code (literal or length/distance pair) currently being processed.  In that case the lower value is the number of bytes already emitted for that
   code.

     A code is being processed if inflate is waiting for more input to complete decoding of the code, or if it has completed decoding but is waiting for
   more output space to write the literal or match data.

     inflateMark() is used to mark locations in the input data for random access, which may be at bit positions, and to note those cases where the
   output of a code may span boundaries of random access blocks.  The current location in the input stream can be determined from avail_in and data_type
   as noted in the description for the Z_BLOCK flush parameter for inflate.

     inflateMark returns the value noted above or -1 << 16 if the provided source stream state was inconsistent.
*/
ZEXTERN int32 ZEXPORT inflateMark( z_stream* strm );

/* utility functions */

/*
     The following utility functions are implemented on top of the basic stream-oriented functions.  To simplify the interface, some default options
   are assumed (compression level and memory usage, standard memory allocation functions).  The source code of these utility functions can be modified if
   you need special options.
*/

/*
     Compresses the source buffer into the destination buffer.  sourceLen is the byte length of the source buffer.  Upon entry, destLen is the total size
   of the destination buffer, which must be at least the value returned by compressBound(sourceLen).  Upon exit, destLen is the actual size of the
   compressed buffer.

     compress returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_BUF_ERROR if there was not enough room in the output buffer.
*/
ZEXTERN int32 ZEXPORT compress( uint8* dest, uint32* destLen, const uint8* source, uint32 sourceLen, int32 level );

/**
     compressBound() returns an upper bound on the compressed size after compress() or compress2() on sourceLen bytes.  It would be used before a
   compress() or compress2() call to allocate the destination buffer.
*/
ZEXTERN uint32 ZEXPORT compressBound( uint32 sourceLen );

/*
     Decompresses the source buffer into the destination buffer.  sourceLen is the byte length of the source buffer.  Upon entry, destLen is the total size
   of the destination buffer, which must be large enough to hold the entire uncompressed data.  (The size of the uncompressed data must have been saved
   previously by the compressor and transmitted to the decompressor by some mechanism outside the scope of this compression library.) Upon exit, destLen
   is the actual size of the uncompressed buffer.

     uncompress returns Z_OK if success, Z_MEM_ERROR if there was not enough memory, Z_BUF_ERROR if there was not enough room in the output
   buffer, or Z_DATA_ERROR if the input data was corrupted or incomplete.  In the case where there is not enough room, uncompress() will fill the output
   buffer with the uncompressed data up to that point.
*/
ZEXTERN int32 ZEXPORT uncompress( uint8* dest, uint32* destLen, const uint8* source, uint32 sourceLen );


/* checksum functions */

/*
     These functions are not related to compression but are exported anyway because they might be useful in applications using the compression library.
*/

/*
     Update a running Adler-32 checksum with the bytes buf[0..len-1] and return the updated checksum.  If buf is NULL, this function returns the
   required initial value for the checksum.

     An Adler-32 checksum is almost as reliable as a CRC32 but can be computed much faster.

   Usage example:

     uint32 adler = adler32(0L, NULL, 0);

     while (read_buffer(buffer, length) != EOF) {
       adler = adler32(adler, buffer, length);
     }
     if (adler != original_adler) error();
*/
ZEXTERN uint32 ZEXPORT adler32( uint32 adler, const uint8* buf, uint64 len );

/*
     Update a running CRC-32 with the bytes buf[0..len-1] and return the updated CRC-32.  If buf is NULL, this function returns the required
   initial value for the crc.  Pre- and post-conditioning (one's complement) is performed within this function so it shouldn't be done by the application.

   Usage example:

     uint32 crc = crc32(0L, NULL, 0);

     while (read_buffer(buffer, length) != EOF) {
       crc = crc32(crc, buffer, length);
     }
     if (crc != original_crc) error();
*/
ZEXTERN uint32 ZEXPORT crc32( uint32 crc, const uint8* buf, uint64 len );

/*
	Get the precalculated crc32 table
*/
ZEXTERN const uint32* ZEXPORT get_crc_table();

/* undocumented functions */
ZEXTERN const char* ZEXPORT zError( int32 );
ZEXTERN int32 ZEXPORT inflateSyncPoint( z_stream* );
ZEXTERN int32 ZEXPORT inflateUndermine( z_stream*, int32 );
ZEXTERN int32 ZEXPORT inflateResetKeep( z_stream* );
ZEXTERN int32 ZEXPORT deflateResetKeep( z_stream* );

#ifdef __cplusplus
}
#endif

#endif /* ZLIB_H */
