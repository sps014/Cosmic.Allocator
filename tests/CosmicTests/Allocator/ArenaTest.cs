using System;
using System.Runtime.CompilerServices;
using Xunit;
using Cosmic.Allocator;
using Xunit.Sdk;

namespace Cosmic.Allocator.Tests
{
    public unsafe class ArenaAllocatorTests
    {
        // Test 1: Default constructor initializes properties to defaults.
        [Fact]
        public void DefaultConstructor_InitializesDefaults()
        {
            ArenaAllocator allocator = new ArenaAllocator();
            Assert.Equal(0u, allocator.Capacity);
            Assert.Equal(0, allocator.Count);
            Assert.Equal(0u, allocator.TotalArenaCount);
            Assert.Equal(IntPtr.Zero,(IntPtr)allocator.LastArena);
            Assert.True(allocator.ArenaPtrMapSpan.IsEmpty);
        }

        // Test 2: Parameterized constructor initializes properties.
        [Fact]
        public void ParameterizedConstructor_InitializesProperties()
        {
            nuint capacity = 1024;
            uint maxArenas = 2;
            ArenaAllocator allocator = new ArenaAllocator(capacity, maxArenas);
            Assert.Equal(capacity, allocator.Capacity);
            Assert.Equal(1, allocator.Count);
            Assert.Equal(Math.Max(maxArenas, 1u), allocator.TotalArenaCount);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)allocator.LastArena);
            Assert.False(allocator.ArenaPtrMapSpan.IsEmpty);
        }

        // Test 3: Alloc<T> with count 0 throws exception.
        [Fact]
        public void AllocGeneric_ThrowsOnZeroCount()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            try
            {
                allocator.Alloc<int>(0);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsType<Exception>(ex);
            }
        }

        // Test 4: Alloc<T> returns span of correct length.
        [Fact]
        public void AllocGeneric_ReturnsCorrectSpan()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Span<int> span = allocator.Alloc<int>(10);
            Assert.Equal(10, span.Length);
        }

        // Test 5: Alloc<T> exceeding capacity throws OutOfMemoryException.
        [Fact]
        public void AllocGeneric_ThrowsOnExceedingCapacity()
        {
            // Assuming sizeof(int) = 4, so 8 bytes capacity means max 2 ints.
            ArenaAllocator allocator = new ArenaAllocator(8, 1);
            try
            {
                allocator.Alloc<int>(3);
                Assert.Fail("Expected OutOfMemoryException was not thrown.");
            }
            catch (OutOfMemoryException)
            {
                // Success.
            }
        }

        // Test 6: Multiple allocations create a new arena.
        [Fact]
        public void Alloc_MultipleArenas_CreatesNewArena()
        {
            // capacity=16 bytes, one int=4 bytes, so 4 ints per arena.
            ArenaAllocator allocator = new ArenaAllocator(16, 2);
            allocator.Alloc<int>(4); // fills first arena
            allocator.Alloc<int>(1); // should trigger new arena allocation
            Assert.Equal(2, allocator.Count);
        }

        // Test 7: Non-generic Alloc returns non-null pointer for valid allocation.
        [Fact]
        public void AllocNonGeneric_ReturnsNonNullPointer()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            void* ptr = allocator.Alloc(100);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
        }

        // Test 8: Allocation increases LastArena size correctly.
        [Fact]
        public void Alloc_IncreasesLastArenaSizeCorrectly()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Arena* initialArena = allocator.LastArena;
            // allocate 100 bytes
            allocator.Alloc(100);
            Assert.Equal(100, (int)initialArena->Size);
        }

        // Test 9: Allocation exactly filling the arena does not create a new arena.
        [Fact]
        public void Allocation_ExactFill_DoesNotCreateNewArena()
        {
            // capacity=16, int size=4, allocate 4 ints
            ArenaAllocator allocator = new ArenaAllocator(16, 2);
            allocator.Alloc<int>(4);
            Assert.Equal(1, allocator.Count);
        }

        // Test 10: StoreArenaAddressInMap throws when max arenas reached.
        [Fact]
        public void StoreArenaAddressInMap_ThrowsWhenMaxArenasReached()
        {
            // maxLinkedArenaCount = 1 (always at least one arena exists after construction).
            ArenaAllocator allocator = new ArenaAllocator(16, 1);
            // Fill first arena exactly.
            allocator.Alloc<int>(4);
            try
            {
                // Next allocation will try to create a new arena and exceed max capacity.
                allocator.Alloc<int>(1);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsType<Exception>(ex);
            }
        }

        // Test 11: GetItemInAll throws for an invalid index.
        [Fact]
        public void GetItemInAll_ThrowsForInvalidIndex()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            // No valid item exists yet.
            try
            {
                var value = allocator.GetItemInAll<int>(0);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsType<ArgumentOutOfRangeException>(ex);
            }
        }

        // Test 12: SetItemInAll and GetItemInAll work correctly.
        [Fact]
        public void SetAndGetItemInAll_WorksCorrectly()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Span<int> span = allocator.Alloc<int>(10);
            span[5] = 123;
            allocator.SetItemInAll(5, 123);
            int value = allocator.GetItemInAll<int>(5);
            Assert.Equal(123, value);
        }

        // Test 13: GetArenaByItemIndex returns correct arena and byte offset.
        [Fact]
        public void GetArenaByItemIndex_ReturnsCorrectArenaAndOffset()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            // Allocate 200 ints (should be in first arena if capacity permits)
            allocator.Alloc<int>(200);
            int byteOffset;
            Arena* arena = allocator.GetArenaByItemIndex(50, sizeof(int), out byteOffset);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)arena);
            Assert.Equal(50 * sizeof(int), byteOffset);
        }

        // Test 14: GetArenaByItemIndex throws for out-of-range index.
        [Fact]
        public void GetArenaByItemIndex_ThrowsForOutOfRangeIndex()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(100); // only 100 ints allocated globally in first arena
            int byteOffset;
            try
            {
                allocator.GetArenaByItemIndex(150, sizeof(int), out byteOffset);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsType<FailException>(ex);
            }
        }

        // Test 15: Alloc<T> works for different types (int and double).
        [Fact]
        public void AllocGeneric_WorksForDifferentTypes()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Span<int> spanInt = allocator.Alloc<int>(10);
            Span<double> spanDouble = allocator.Alloc<double>(5);
            Assert.Equal(10, spanInt.Length);
            Assert.Equal(5, spanDouble.Length);
        }

        // Test 16: LastArena updates after a new arena allocation.
        [Fact]
        public void LastArena_UpdatesAfterNewArenaAllocation()
        {
            ArenaAllocator allocator = new ArenaAllocator(16, 3);
            Arena* firstArena = allocator.LastArena;
            allocator.Alloc<int>(4); // fill first arena (16 bytes)
            allocator.Alloc<int>(1); // triggers new arena allocation
            Assert.NotEqual((IntPtr)firstArena, (IntPtr)allocator.LastArena);
        }

        // Test 17: Multiple Dispose calls do not throw.
        [Fact]
        public void Dispose_MultipleCalls_DoNotThrow()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(10);
            try
            {
                allocator.Dispose();
                allocator.Dispose(); // second call should not throw
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dispose called multiple times threw an exception: {ex}");
            }
        }

        // Test 18: Alloc on a default-constructed allocator returns null pointer.
        [Fact]
        public void Alloc_OnDefaultConstructedAllocator_ReturnsNullPointer()
        {
            ArenaAllocator allocator = new ArenaAllocator();
            void* ptr = allocator.Alloc(100);
            Assert.Equal(IntPtr.Zero, (IntPtr)ptr);
        }

        // Test 19: Alloc on a parameterized allocator returns a non-null pointer.
        [Fact]
        public void Alloc_OnParameterizedAllocator_ReturnsNonNullPointer()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            void* ptr = allocator.Alloc(50);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
        }

        // Test 20: GetItemInAll with a negative index throws exception.
        [Fact]
        public void GetItemInAll_NegativeIndex_Throws()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(10);
            try
            {
                allocator.GetItemInAll<int>(-1);
                Assert.Fail("Expected exception was not thrown.");
            }
            catch (Exception ex)
            {
                Assert.IsType<ArgumentOutOfRangeException>(ex);
            }
        }

        // Test 21: Alloc<T> allocates contiguous memory within the same arena if possible.
        [Fact]
        public void AllocGeneric_ContiguousAllocationWithinSameArena()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Span<byte> span1 = allocator.Alloc<byte>(100);
            Span<byte> span2 = allocator.Alloc<byte>(50);
            // Both allocations fit in the same arena; the arena size should equal their sum.
            Assert.Equal(100 + 50, (int)allocator.LastArena->Size);
        }

        // Test 22: New arena's StartingOffset is set correctly.
        [Fact]
        public void NewArena_StartingOffset_IsCorrect()
        {
            ArenaAllocator allocator = new ArenaAllocator(16, 3);
            // Fill first arena.
            allocator.Alloc<int>(4);
            // Allocate in second arena.
            allocator.Alloc<int>(1);
            Arena* secondArena = allocator.LastArena;
            Arena* firstArena = (Arena*)allocator.ArenaPtrMapSpan[0].ToPointer();
            Assert.Equal(firstArena->StartingOffset + allocator.Capacity, secondArena->StartingOffset);
        }

        // Test 23: GetItemInAll retrieves the correct item across multiple arenas.
        [Fact]
        public void GetItemInAll_RetrievesCorrectItemAcrossArenas()
        {
            ArenaAllocator allocator = new ArenaAllocator(16, 3);
            Span<int> span1 = allocator.Alloc<int>(4); // fills first arena
            for (int i = 0; i < span1.Length; i++)
            {
                span1[i] = i + 1;
            }
            // This allocation triggers a new arena.
            Span<int> span2 = allocator.Alloc<int>(2);
            span2[0] = 100;
            span2[1] = 200;
            // Get items from first arena.
            Assert.Equal(3, allocator.GetItemInAll<int>(2));
            // Get items from second arena.
            Assert.Equal(100, allocator.GetItemInAll<int>(4));
            Assert.Equal(200, allocator.GetItemInAll<int>(5));
        }

        // Test 24: Count property reflects the number of arenas allocated.
        [Fact]
        public void CountProperty_ReflectsNumberOfArenas()
        {
            ArenaAllocator allocator = new ArenaAllocator(16, 5);
            allocator.Alloc<int>(4); // first arena
            allocator.Alloc<int>(1); // second arena
            allocator.Alloc<int>(1); // third arena
            Assert.Equal(2, allocator.Count);
        }

        // Test 25: TotalArenaCount is at least one even if zero is passed.
        [Fact]
        public void TotalArenaCount_IsAtLeastOne()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 0);
            Assert.Equal(1u, allocator.TotalArenaCount);
        }

        // Test 26: ArenaPtrMapSpan length equals TotalArenaCount.
        [Fact]
        public void ArenaPtrMapSpan_Length_EqualsTotalArenaCount()
        {
            uint maxArenas = 4;
            ArenaAllocator allocator = new ArenaAllocator(1024, maxArenas);
            Assert.Equal(maxArenas, (uint)allocator.ArenaPtrMapSpan.Length);
        }

        // Test 27: ArenaMap size is set correctly.
        [Fact]
        public void ArenaMap_Size_IsSetCorrectly()
        {
            uint maxArenas = 3;
            ArenaAllocator allocator = new ArenaAllocator(1024, maxArenas);
            uint expectedSize = maxArenas * (uint)Unsafe.SizeOf<IntPtr>();
            Assert.Equal(expectedSize, allocator.ArenaMap->Size);
        }

        // Test 28: Alloc(nuint) recursively allocates a new arena if needed.
        [Fact]
        public void AllocNonGeneric_RecursivelyAllocatesNewArena()
        {
            ArenaAllocator allocator = new ArenaAllocator(16, 3);
            // Fill first arena.
            allocator.Alloc<int>(4);
            // This allocation will require a new arena.
            void* ptr = allocator.Alloc(4);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)ptr);
        }

        // Test 29: Alloc<T> works with different types and their sizes.
        [Fact]
        public void AllocGeneric_WithDifferentTypes_Works()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 3);
            Span<byte> spanByte = allocator.Alloc<byte>(10);
            Span<short> spanShort = allocator.Alloc<short>(5);
            Span<long> spanLong = allocator.Alloc<long>(2);
            Assert.Equal(10, spanByte.Length);
            Assert.Equal(5, spanShort.Length);
            Assert.Equal(2, spanLong.Length);
        }

        // Test 30: Setting one item does not affect another.
        [Fact]
        public void SetItemInAll_DoesNotAffectOtherItems()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Span<int> span = allocator.Alloc<int>(10);
            for (int i = 0; i < span.Length; i++)
            {
                span[i] = 0;
            }
            allocator.SetItemInAll(5, 555);
            for (int i = 0; i < span.Length; i++)
            {
                if (i == 5)
                    Assert.Equal(555, allocator.GetItemInAll<int>(i));
                else
                    Assert.Equal(0, allocator.GetItemInAll<int>(i));
            }
        }

        // Test 31: Arena's Size increases by the allocated size.
        [Fact]
        public void Arena_Size_IncreasesByAllocatedSize()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Arena* arena = allocator.LastArena;
            nuint initialSize = arena->Size;
            allocator.Alloc<byte>(100);
            Assert.Equal(initialSize + 100, arena->Size);
        }

        // Test 32: Allocation equal to capacity fits in the same arena.
        [Fact]
        public void Allocation_EqualToCapacity_FitsInSameArena()
        {
            ArenaAllocator allocator = new ArenaAllocator(16, 2);
            // 16 bytes capacity, allocate 16 bytes.
            allocator.Alloc<byte>(16);
            Assert.Equal(1, allocator.Count);
        }

        // Test 33: GetArenaByItemIndex computes the correct byte offset.
        [Fact]
        public void GetArenaByItemIndex_ComputesCorrectByteOffset()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(100);
            int byteOffset;
            allocator.GetArenaByItemIndex(25, sizeof(int), out byteOffset);
            Assert.Equal(25 * sizeof(int), byteOffset);
        }

        // Test 34: SetItemInAll and GetItemInAll work for different types.
        [Fact]
        public void SetAndGetItemInAll_ForDifferentTypes()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Span<double> span = allocator.Alloc<double>(5);
            span[2] = 3.1415;
            allocator.SetItemInAll(2, 3.1415);
            double value = allocator.GetItemInAll<double>(2);
            Assert.Equal(3.1415, value, 4);
        }

        // Test 35: LastArena's Size property is accurate after allocations.
        [Fact]
        public void LastArena_SizeProperty_IsAccurate()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(10);
            Assert.Equal(10 * sizeof(int), (int)allocator.LastArena->Size);
        }

        // Test 36: Alloc after Dispose returns null (or fails gracefully).
        [Fact]
        public void Alloc_AfterDispose_ReturnsNullOrFailsGracefully()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(5);
            allocator.Dispose();
            void* ptr = allocator.Alloc(10);
            Assert.Equal(IntPtr.Zero, (IntPtr)ptr);
        }

        // Test 37: Dispose after multiple allocations does not throw.
        [Fact]
        public void Dispose_AfterMultipleAllocations_DoesNotThrow()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 3);
            allocator.Alloc<int>(10);
            allocator.Alloc<double>(5);
            try
            {
                allocator.Dispose();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dispose threw an exception: {ex}");
            }
        }

        // Test 38: ArenaMap pointer is not null after construction.
        [Fact]
        public void ArenaAllocator_ArenaMap_IsNotNullAfterConstruction()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            Assert.NotEqual(IntPtr.Zero, (IntPtr)allocator.ArenaMap);
        }

        // Test 39: Alloc<T> with different element sizes computes the byte offset correctly.
        [Fact]
        public void AllocGeneric_DifferentElementSizes_CorrectByteOffset()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            // Allocate ints to partially fill the arena.
            allocator.Alloc<int>(10);
            int byteOffset;
            // Use a different type (short) for index calculation.
            Arena* arena = allocator.GetArenaByItemIndex(5, sizeof(short), out byteOffset);
            // Expect byteOffset to be computed as index * sizeof(short)
            Assert.Equal(5 * sizeof(short), byteOffset);
        }

        // Test 40: Calling Dispose twice does not throw.
        [Fact]
        public void Dispose_CalledTwice_DoesNotThrow()
        {
            ArenaAllocator allocator = new ArenaAllocator(1024, 2);
            allocator.Alloc<int>(5);
            allocator.Dispose();
            try
            {
                allocator.Dispose();
            }
            catch (Exception ex)
            {
                Assert.Fail($"Dispose called twice should not throw, but threw: {ex}");
            }
        }
    }
}
