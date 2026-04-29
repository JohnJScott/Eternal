# Lzma1Dec.cpp — Performance Review

This is a C++ refactor of Igor Pavlov's `LzmaDec.c` (LZMA SDK). The reference is heavily tuned via macros (`NORMALIZE`, `GET_BIT`, `GET_BIT2`, `TREE_DECODE`, …); several of the abstractions introduced in the C++ port reintroduce overhead the macros eliminate. Findings are grouped by expected impact and ordered roughly by ROI.

---

## High-impact

### 1. Hoist the range-coder state into locals across the inner loop

`DecodeRealInternal` reads and writes `Range`, `Code`, `DataBufferOffset`, `DataBufferBase` through `this` on every bit decode. Every `SimpleIterate(CProbability& prob)` call takes a reference into `Probabilities[]`; the compiler cannot prove `&Probabilities[i]` doesn't alias `this->Range`/`this->Code`, so it must reload them after each probability store. The C reference avoids this by hoisting `range`, `code`, `buf` into locals for the duration of the loop.

Sketch:

```cpp
SevenZipResult Lzma1Dec::DecodeRealInternal(int64 limit, int64 bufLimitOffset)
{
    uint32 range = Range;
    uint32 code  = Code;
    const uint8* buf  = DataBufferBase;
    int64        bofs = DataBufferOffset;
    const uint32 posMask = PositionMask;
    const uint32 litMask = LiteralMask;
    uint8* const dict     = Dictionary;
    const int64  dictSize = DictionaryBufferSize;
    CProbability* const probs = Probabilities;

    // ... entire main loop on these locals ...

    Range = range;
    Code  = code;
    DataBufferOffset = bofs;
}
```

For the hoist to actually pay off, the bit-decode helpers (`UpdateRange`, `SimpleIterate`, `Iterate`) must accept these locals as parameters rather than touching `*this` — otherwise the optimizer will rematerialize them every call.

This is the single biggest opportunity in the file.

### 2. Templatize `Low(mode)` / `High(mode)` instead of runtime-switching on `mode`

Every call site passes a literal `0`–`5`. The runtime `switch` is at best folded by the optimizer after inlining and at worst kept as a jump table. Replace with templates:

```cpp
template <uint8 Mode> __forceinline void Low();
template <uint8 Mode> __forceinline void High();
```

and template `Iterate`/`DummyIterate` on `Mode`. This mirrors the original `GET_BIT_*` macro family and lets the compiler emit only the relevant body at each call site.

### 3. Replace byte-by-byte match copy with a two-tier strategy

`FinishBlock` (lines 524–527) and `WriteRemaining` (lines 748–752) copy a byte at a time. For non-overlapping copies (distance ≥ length) this should be `memcpy`; for self-extending copies with distance ≥ 16, you can do overlap-safe SIMD chunks; only the tiny-distance RLE case actually requires a byte loop. zstd/lz4 do this aggressively. Even just the simple split (`memcpy` when distance ≥ length, byte loop otherwise) recovers most of the gain.

The wrap-around copy in both functions also re-evaluates the wrap conditional per byte. Split into pre-wrap segment + post-wrap segment, each a single `memmove`.

---

## Medium-impact

### 4. Cache the previous dictionary byte

`UpdateSymbol` (line 326) and `TryDummyLit` (line 807) compute
`Dictionary[(DictionaryPosition == 0 ? DictionaryBufferSize : DictionaryPosition) - 1u]`
on every literal. Maintain a `uint8 PrevByte` member, write to it whenever the dictionary advances, and read it directly. Removes a conditional plus a modular subtract from the literal hot path.

### 5. Replace State-transition ternaries with small lookup tables

Per-symbol/per-match transitions:

```cpp
State -= (State < 4u)  ? State : 3u;     // line 334
State -= (State < 10u) ? 3u : 6u;        // line 344
State  = (State < N)   ? A : B;          // 492, 566, 606
```

A 16-entry `uint8` LUT per transition kind is one load and zero branches.

### 6. Force inlining on hot helpers

Mark `UpdateRange`, `SimpleIterate`, `Iterate`, `Low`, `High`, `DummySimpleIterate`, `UpdateRangeDummy`, `DecodeShortRepeat`, `DecodeMatchType`, `DecodeLowLength` with `[[gnu::always_inline]] inline` (or `__forceinline` on MSVC). Without this, MSVC in particular sometimes refuses to inline methods through `this`, and you pay call/return overhead on the busiest functions in the program.

### 7. Vectorized probability initialization

Lines 1148–1151 store `BitModelTableSize >> 1` into ~28 KB of `uint16` slots in a scalar loop. Replace with `std::fill_n(Probabilities, num_probs, …)` so the compiler emits `vpbroadcastw` + a vectorized store loop. Run once per stream init, but eliminating it as a source of cold-path latency is cheap.

### 8. Reduce aliasing in iterate/SimpleIterate signatures

`CProbability&` arguments alias anything reachable from `Probabilities`. Switching to `CProbability* __restrict` (with the index passed separately) lets the optimizer assume no aliasing with range-coder state and unlocks register allocation alongside #1.

---

## Low-impact / cleanup

### 9. Dead store in `UpdateSymbol` matched path
Line 349 (`Bit = Offset;`) is overwritten on the first iteration at line 354. Drop it.

### 10. Short-circuit matched-literal when `Offset` becomes zero
Once `Offset == 0` mid-loop, remaining iterations degenerate into the unmatched literal path. The SDK detects this; the port doesn't.

### 11. Redundant range check in `FinishBlock`
Line 508 `(remaining > UINT32_MAX) ? length : std::min(...)` — `length` is `uint32`, so `static_cast<uint32>(std::min<int64>(remaining, length))` works without the branch.

### 12. Three-call sequence in `UpdateLength` mid-length branch
Lines 372–374 issue three serial `Iterate(Probabilities[length_offset + Symbol], 0)` calls. After #2 they fold into inlined bodies sharing `range`/`code`; without #2 the compiler can't see across them.

### 13. Direct-bit decode in `UpdateRepeatLength` (lines 443–451) processes one bit per iteration
Multi-bit batched decoding is feasible when `range` is large; the upstream SDK does this for long-distance matches. Modest because these are infrequent.

### 14. Avoid the full `Lzma1Dec` copy in `TryDummy`
The copy ctor copies 20+ scalars; only ~10 are actually read on the dummy path. Either make `TryDummy` borrow a `const Lzma1Dec&` and operate on locals (no copy at all — dummy decode never writes `Probabilities`), or split the dummy state into a smaller struct.

---

## Verify (not bugs, but fragile)

### 15. `Iterate(Probabilities[Symbol], 4)` at line 432
Addresses into the probability table at index `Symbol` with no offset. Relies on the position-slot tree being placed at probability-array offset 0 (SDK layout). Worth a comment.

### 16. Matched-literal masking math
Lines 354–357 + the corresponding `Low(1)`/`High(1)` bodies must replicate `GET_BIT2_MATCHED(symbol, offs, bit, prob)` semantics from the SDK. Fuzz against upstream `LzmaDec.c` on a corpus before and after any structural changes.

---

## Suggested order of attack

1. #1 (range-coder hoist) and #6 (always-inline) together — measure on a representative corpus.
2. #2 (template `Low`/`High`).
3. #3 (split match-copy fast/slow paths).
4. #5 (state LUT) and #4 (prev-byte cache) — clean incremental wins.
5. The rest as opportunistic cleanup.

Items 1–4 should account for the bulk of the achievable speedup; everything else is incremental.
