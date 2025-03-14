using System;
using System.Runtime.InteropServices;
using Xunit;

namespace Cosmic.Allocator.Tests
{
    public unsafe class ArenaManagerTests
    {
        [Fact]
        public void Test_CreateArena()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // Compare capacity and size using ulong casts.
            Assert.Equal((ulong)capacity, (ulong)arena.Capacity);
            Assert.Equal(0UL, (ulong)arena.Size);
            // Check that data pointer is allocated.
            Assert.NotEqual(IntPtr.Zero, (IntPtr)arena.Data);
        }

        [Fact]
        public void Test_AllocWithinCapacity()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            nuint allocSize = 50;
            void* ptr = arena.Alloc(allocSize);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
            Assert.Equal((ulong)allocSize, (ulong)arena.Size);
        }

        [Fact]
        public void Test_AllocExactCapacity()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            void* ptr = arena.Alloc(capacity);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
            Assert.Equal((ulong)capacity, (ulong)arena.Size);
        }

        [Fact]
        public void Test_AllocExceedingCapacity_ThrowsOutOfMemory()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            try
            {
                // Requesting allocation larger than the capacity should throw.
                arena.Alloc(capacity + 1);
                Assert.Fail("Expected OutOfMemoryException was not thrown.");
            }
            catch (OutOfMemoryException)
            {
                Assert.True(true);
            }
        }

        [Fact]
        public void Test_ChainAllocation()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // First allocation uses part of the arena.
            void* ptr1 = arena.Alloc(80);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr1);
            Assert.Equal((ulong)80, (ulong)arena.Size);

            // Second allocation (30 bytes) exceeds remaining capacity in the first arena,
            // so it allocates in a new arena.
            void* ptr2 = arena.Alloc(30);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr2);
            Assert.NotEqual((IntPtr)arena.Next,IntPtr.Zero);
            Assert.Equal((ulong)30, arena.Next->Size);
        }

        [Fact]
        public void Test_ResetArena()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // Allocate some memory
            arena.Alloc(50);
            Assert.Equal((ulong)50, (ulong)arena.Size);

            // Reset the arena
            arena.Reset();
            Assert.Equal(0UL, (ulong)arena.Size);
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Data);
        }

        [Fact]
        public void Test_FreeArena()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // Allocate some memory
            arena.Alloc(50);
            Assert.Equal((ulong)50, (ulong)arena.Size);

            // Free the arena
            arena.Dispose();
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Data);
        }

        [Fact]
        public void Test_ChainAllocationAndFree()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // First allocation uses part of the arena.
            void* ptr1 = arena.Alloc(80);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr1);
            Assert.Equal((ulong)80, (ulong)arena.Size);

            // Second allocation (30 bytes) exceeds remaining capacity in the first arena,
            // so it allocates in a new arena.
            void* ptr2 = arena.Alloc(30);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr2);
            Assert.NotEqual((IntPtr)arena.Next, IntPtr.Zero);
            Assert.Equal((ulong)30, (ulong)arena.Next->Size);

            // Free the entire chain of arenas
            arena.Dispose();
            Assert.Equal(IntPtr.Zero,(IntPtr)arena.Data);
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Next);
        }

        [Fact]
        public void Test_MultipleChainAllocationsAndFree()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // First allocation uses part of the arena.
            void* ptr1 = arena.Alloc(80);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr1);
            Assert.Equal((ulong)80, (ulong)arena.Size);

            // Second allocation (30 bytes) exceeds remaining capacity in the first arena,
            // so it allocates in a new arena.
            void* ptr2 = arena.Alloc(30);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr2);
            Assert.NotEqual(IntPtr.Zero,(IntPtr)arena.Next);
            Assert.Equal((ulong)30, (ulong)arena.Next->Size);

            // Third allocation (50 bytes) in the second arena.
            void* ptr3 = arena.Next->Alloc(50);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr3);
            Assert.Equal((ulong)80, (ulong)arena.Next->Size);

            // Free the entire chain of arenas
            arena.Dispose();
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Data);
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Next);
        }

        [Fact]
        public void Test_ChainAllocationWithReset()
        {
            nuint capacity = 100;
            using Arena arena = ArenaManager.Create(capacity);
            // First allocation uses part of the arena.
            void* ptr1 = arena.Alloc(80);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr1);
            Assert.Equal((ulong)80, (ulong)arena.Size);

            // Second allocation (30 bytes) exceeds remaining capacity in the first arena,
            // so it allocates in a new arena.
            void* ptr2 = arena.Alloc(30);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr2);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)arena.Next);
            Assert.Equal((ulong)30, (ulong)arena.Next->Size);

            // Reset the arena
            arena.Reset();
            Assert.Equal(0UL, (ulong)arena.Size);
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Next);
            Assert.Equal(IntPtr.Zero, (IntPtr)arena.Data);
        }

        [Fact]
        public void Test_ChainAllocationAndFreeMultipleTimes()
        {
            nuint capacity = 100;
            for (int i = 0; i < 5; i++)
            {
                using Arena arena = ArenaManager.Create(capacity);
                // First allocation uses part of the arena.
                void* ptr1 = arena.Alloc(80);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr1);
                Assert.Equal((ulong)80, (ulong)arena.Size);

                // Second allocation (30 bytes) exceeds remaining capacity in the first arena,
                // so it allocates in a new arena.
                void* ptr2 = arena.Alloc(30);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr2);
                Assert.NotEqual(IntPtr.Zero, (IntPtr)arena.Next);
                Assert.Equal((ulong)30, (ulong)arena.Next->Size);

                // Free the entire chain of arenas
                arena.Dispose();
                Assert.Equal(IntPtr.Zero, (IntPtr)arena.Next);
                Assert.Equal(IntPtr.Zero, (IntPtr)arena.Data);
            }

        }
    }
}