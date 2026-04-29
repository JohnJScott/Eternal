/* LzFind.h -- Match finder for LZ algorithms
2021-07-13 : Igor Pavlov : Public domain */

#pragma once

typedef uint32 CLzRef;

class CMatchFinder
{
public:
	CMatchFinder( MemoryInterface* alloc )
		: Alloc( alloc )
	{
	}

	virtual ~CMatchFinder()
	{
		Free();
	}

	void Free();
	bool Create( const uint32 inHistorySize, const uint32 keepAddBufferBefore, const uint32 inMatchMaxLen, uint32 keepAddBufferAfter );
	void Init();

protected:
	void CheckLimits();
	void MovePos();
	bool FindDistances( uint32* d2, const uint32 d3, const uint32 maxDistance, uint32* distancesContainer, uint32& matchCount ) const;

	uint32 UpdateMaxLen( const uint32 d2, const uint32 maxLength ) const;
	uint32 CalcHash( uint32* d2, uint32* d3 ) const;
	uint32 CalcHashSkip() const;

private:
	void FreeBuffer();
	bool CreateBuffer( const uint32 newBlockSize );

	void FreeHashes();
	bool AllocHashes( const int64 num );

	void ReadBlock();
	void MoveBlock();
	bool NeedMove() const;
	uint32 GetBlockSize( const uint32 historySize ) const;
	void SetLimits();

	uint32 HashCalcInternal( uint32* h2, uint32* h3, uint32* hv ) const;
	// Keep hash update/skip as protected helpers
	void HashUpdate( const uint32 h2, const uint32 h3, const uint32 hv, uint32* d2, uint32* d3 ) const;
	void HashSkip( const uint32 h2, const uint32 h3, const uint32 hv ) const;

public:
	virtual bool IsBinaryTreeMode() const = 0;
	virtual void GetMatches( uint32* distances, uint32& pairCount ) = 0;
	virtual void Skip( uint32 length ) = 0;

	InStreamInterface* InStream = nullptr;
	uint8* BufferBase = nullptr;

	int64 BufferOffset = 0;
	int64 ExpectedDataSize = INT64_MAX;
	int64 DirectInputRemaining = 0;

	uint32 Position = 0;
	uint32 StreamPosition = 0;
	uint32 CutValue = 32u;
	SevenZipResult Result = SevenZipResult::SevenZipOK;

	bool DirectInput = false;

protected:

	MemoryInterface* Alloc = nullptr;
	CLzRef* Hash = nullptr;
	uint32 PositionLimit = 0;

	/* wrap over Zero is allowed (StreamPosition < Position). Use ( uint32 )( StreamPosition - Position ) */
	uint32 LengthLimit = 0;
	uint32 CyclicBufferPosition = 0;

	/* it must be = (HistorySize + 1) */
	uint32 CyclicBufferSize = 0;
	uint32 SonOffset = 0;

private:
	uint32 MatchMaxLength = 0;
	int64 NumRefs = 0;
	uint32 HashMask = 0;
	uint32 BlockSize = 0;
	uint32 KeepSizeBefore = 0;
	uint32 KeepSizeAfter = 0;
	uint32 HistorySize = 0;
	uint32 FixedHashSize = 0;
	uint32 HashSizeSum = 0;

	bool StreamEndWasReached = false;

	static uint32 CrcLookupTable[256];
};

/* Conditions:
	 HistorySize <= 3 GB
	 keepAddBufferBefore + MatchMaxLength + keepAddBufferAfter < 511MB
*/

CMatchFinder* CreateMatchFinder( const bool useBinaryTree, MemoryInterface* alloc );
