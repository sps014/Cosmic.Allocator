using System;
using Xunit;
using Cosmic.Allocator;
using System.Runtime.InteropServices;

namespace CosmicTests.Allocator.Tests
{
    public unsafe class ArenaTests
    {
        [Fact]
        public void Test_CreateArena()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                // Check that arena pointer is not null
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena);
                // Compare capacity and size using ulong casts.
                Assert.Equal((ulong)capacity, (ulong)arena->Capacity);
                Assert.Equal(0UL, (ulong)arena->Size);
                // Check that data pointer is allocated.
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena->Data);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_AllocWithinCapacity()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                nuint allocSize = 50;
                void* ptr = arena->Alloc(allocSize);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
                Assert.Equal((ulong)allocSize, (ulong)arena->Size);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_AllocExactCapacity()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                void* ptr = arena->Alloc(capacity);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
                Assert.Equal((ulong)capacity, (ulong)arena->Size);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_AllocExceedingCapacity_ThrowsOutOfMemory()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                // Requesting allocation larger than the capacity should throw.
                Assert.Throws<OutOfMemoryException>(() => arena->Alloc(capacity + 1));
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_ChainAllocation()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                // First allocation uses part of the arena.
                void* ptr1 = arena->Alloc(80);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr1);
                Assert.Equal((ulong)80, (ulong)arena->Size);

                // Second allocation (30 bytes) exceeds remaining capacity in the first arena,
                // so it allocates in a new arena.
                void* ptr2 = arena->Alloc(30);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr2);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena->Next);
                Assert.Equal((ulong)30, (ulong)arena->Next->Size);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_Reset()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                arena->Alloc(50);
                arena->Alloc(60);  // This allocation creates a second arena.
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena->Next);

                arena->Reset();
                Assert.Equal(0UL, (ulong)arena->Size);
                Assert.Equal(IntPtr.Zero, (IntPtr)arena->Next);
                Assert.Equal(IntPtr.Zero, (IntPtr)arena->Data);
            }
            finally
            {
                // Even if Data is null after reset, Free should handle it gracefully.
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_Free_DoesNotThrow()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            arena->Alloc(50);
            arena->Alloc(60);
            var exception = Record.Exception(() => ArenaManager.Free(arena));
            Assert.Null(exception);
        }

        [Fact]
        public void Test_MultipleAllocationsWithinCapacity()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                nuint allocationCount = 10;
                nuint allocSize = 10; // 10 allocations of 10 bytes equals 100 bytes.
                for (int i = 0; i < (int)allocationCount; i++)
                {
                    void* ptr = arena->Alloc(allocSize);
                    Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
                }
                Assert.Equal((ulong)(allocationCount * allocSize), (ulong)arena->Size);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_AllocationChainMultipleNodes()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                // Fill first arena partially.
                arena->Alloc(90);
                // This allocation exceeds remaining space and goes to a second arena.
                arena->Alloc(20);
                // Fill the second arena.
                arena->Alloc(80);
                // Force creation of a third arena.
                void* ptr3 = arena->Alloc(50);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr3);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena->Next);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena->Next->Next);
                Assert.Equal((ulong)50, (ulong)arena->Next->Next->Size);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_AllocDoesNotModifyOriginalArenaSizeIfNotEnough()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                arena->Alloc(70);
                void* ptr = arena->Alloc(40);
                // The first arena's size remains unchanged.
                Assert.Equal((ulong)70, (ulong)arena->Size);
                // And the second arena has the 40-byte allocation.
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena->Next);
                Assert.Equal((ulong)40, (ulong)arena->Next->Size);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }

        [Fact]
        public void Test_PointerOffsetCorrectness()
        {
            nuint capacity = 100;
            Arena* arena = ArenaManager.Create(capacity);
            try
            {
                byte* basePtr = (byte*)arena->Data;
                // First allocation of 10 bytes.
                void* ptr1 = arena->Alloc(10);
                // Second allocation of 20 bytes.
                void* ptr2 = arena->Alloc(20);
                // Since the first allocation was 10 bytes, the second allocation should be offset by 10 bytes.
                Assert.Equal((long)basePtr + 10, (long)ptr2);
            }
            finally
            {
                ArenaManager.Free(arena);
            }
        }


        // ------------------------------------------------------------------
        // New Test: Create a chain of arenas, free the entire chain using the
        // provided API, then create a new arena and perform allocations.
        // This verifies that after freeing the old chain, new allocations work.
        // ------------------------------------------------------------------
        [Fact]
        public void Test_FreeChainAndReallocateNewChain()
        {
            nuint capacity = 100;
            // Create a chain of arenas.
            Arena* arena1 = ArenaManager.Create(capacity);
            try
            {
                arena1->Alloc(90);
                arena1->Alloc(20); // This allocation creates a second node.
            }
            finally
            {
                // Free the entire chain using the provided API.
                ArenaManager.Free(arena1);
            }

            // Now create a new arena chain and perform allocations.
            Arena* arena2 = ArenaManager.Create(capacity);
            try
            {
                void* ptr = arena2->Alloc(50);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
                Assert.Equal((ulong)50, (ulong)arena2->Size);
            }
            finally
            {
                ArenaManager.Free(arena2);
            }
        }
    }
}
