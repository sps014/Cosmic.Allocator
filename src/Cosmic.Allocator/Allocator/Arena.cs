using System.Runtime.InteropServices;

namespace Cosmic.Allocator
{
    /// <summary>
    /// Represents a memory arena for efficient allocation and deallocation of memory blocks.
    /// </summary>
    public unsafe struct Arena : IDisposable
    {
        /// <summary>
        /// Pointer to the next arena in the chain.
        /// </summary>
        public Arena* Next { get; internal set; }

        /// <summary>
        /// Pointer to the next arena in the chain.
        /// </summary>
        public Arena* Previous { get; internal set; }

        /// <summary>
        /// Pointer to the allocated memory block.
        /// </summary>
        public void* Data { get; internal set; }

        /// <summary>
        /// Total capacity of the memory block.
        /// </summary>
        public nuint Capacity { get; internal set; }

        /// <summary>
        /// Current size of the allocated memory.
        /// </summary>
        public nuint Size { get; internal set; }

        /// <summary>
        /// Return Safe Arena Handle
        /// </summary>
        public SafeHandle<Arena> CurrentHandle => AsSafeHandle();

        /// <summary>
        /// Returns Next Arena's Safe Handle, Returns SafeHandle.Zero if no Next Arena linked
        /// </summary>
        public SafeHandle<Arena> NextArenaHandle=> NextArenaAsSafeHandle();

        /// <summary>
        /// Return Current Allocated Data in the Arena (Continuous Region)
        /// </summary>
        public SafeRegionHandle DataRegion => new SafeRegionHandle(Data, (int)Size);

        /// <summary>
        /// Initializes a new instance of the <see cref="Arena"/> struct with no capacity.
        /// <br></br><b><i>Note: A new arena should be created using the ArenaManager instead of direct constructor call.</i></b>
        /// </summary>
        public Arena()
        {
            Capacity = 0;
            Size = 0;
            Data = null;
            Next = null;
            Previous = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Arena"/> struct with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the memory block.</param>
        internal Arena(nuint capacity)
        {
            Capacity = capacity;
            Size = 0;
            Data = NativeMemory.Alloc(capacity);
            Next = null;
            Previous = null;
        }

        /// <summary>
        /// Allocates memory for the specified number of items of type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The type of items to allocate memory for.</typeparam>
        /// <param name="count">The number of items to allocate memory for. Default is 1.</param>
        /// <returns>A span of the allocated memory.</returns>
        /// <exception cref="Exception">Thrown when the item count is zero.</exception>
        public Span<T> Alloc<T>(uint count = 1) where T : unmanaged
        {
            if (count == 0)
                throw new Exception("Item count can't be equal to 0");

            nuint size = (nuint)sizeof(T) * count;
            void* ptr = Alloc(size);
            return new Span<T>(ptr, (int)size);
        }

        /// <summary>
        /// Allocates a memory block of the specified size.
        /// </summary>
        /// <param name="size">The size of the memory block to allocate.</param>
        /// <returns>A pointer to the allocated memory block.</returns>
        /// <exception cref="OutOfMemoryException">Thrown when the allocation size is greater than the capacity.</exception>
        public void* Alloc(nuint size)
        {
            if (size > Capacity)
            {
                throw new OutOfMemoryException("Allocation size is greater than capacity");
            }

            // Check if there is enough capacity in the current arena
            if (Size + size <= Capacity)
            {
                void* result = (byte*)Data + Size;
                Size += size;
                return result;
            }

            // If there is not enough capacity, try the next arena
            if (Next != null)
            {
                return Next->Alloc(size);
            }

            // If there is no next arena, create a new one and allocate from it
            Next = ArenaManager.CreatePointer(Capacity);
            Next->Previous = AsPointer();
            return Next->Alloc(size);
        }


        /// <summary>
        /// Get a Span over current arena allocated memory
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Span<T> AsSpan<T>() where T : unmanaged
        {
            if(Data==null)
                return Span<T>.Empty;

            return new Span<T>(Data, (int)Size/sizeof(T));
        }

        /// <summary>
        /// Get total Arenas used
        /// </summary>
        /// <returns></returns>
        public int TotalArenaCount()
        {
            int count = 0;

            fixed(Arena* arena = &this)
            {
                var ptr = arena;

                while(ptr!=null)
                {
                    count++;
                    ptr = ptr->Next;
                }
            }

            return count;
        }

        //Return the Current Arena as Pointer
        public Arena* AsPointer()
        {
            fixed (Arena* arena = &this)
            {
                return arena;
            }
        }

        private SafeHandle<Arena> AsSafeHandle()
        {
            return new SafeHandle<Arena>((IntPtr)AsPointer());
        }

        private SafeHandle<Arena> NextArenaAsSafeHandle()
        {
            if (Next == null)
                return SafeHandle<Arena>.Zero;

            return Next->AsSafeHandle();
        }


        /// <summary>
        /// Get the Item by global index (can be in any nested arenas), <b> should be called on first node of Arena</b>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">global index of item</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public T GetItemInAll<T>(int index) where T : unmanaged
        {
            var nestedArena = AsPointer()->GetArenaByItemIndex(index, sizeof(T), out int byteOffset);

            if (nestedArena == SafeHandle<Arena>.Zero)
                throw new Exception("Invalid Index");

            return nestedArena.AsPointer()->DataRegion.GetItem<T>(byteOffset / sizeof(T));

        }


        /// <summary>
        /// Set the Item by global index (can be in any nested arenas) <b> should be called on first node of Arena</b>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="index">global index of item, can be in any nested arena</param>
        /// <param name="item"></param>
        /// <exception cref="Exception"></exception>
        public void SetItemInAll<T>(int index, T item) where T : unmanaged
        {
            var nestedArena = AsPointer()->GetArenaByItemIndex(index, sizeof(T), out int byteOffset);

            if (nestedArena == SafeHandle<Arena>.Zero)
                throw new Exception("Invalid Index");

            nestedArena.AsPointer()->DataRegion.SetItem(byteOffset / sizeof(T), item);
        }


        /// <summary>
        /// Resets the arena, freeing all allocated memory and setting the size to zero.
        /// </summary>
        public void Reset()
        {
            Size = 0;

            if (Data != null)
            {
                NativeMemory.Free(Data); // Free the memory before setting Data to null
                Data = null;
            }

            var next = Next;
            Next = null;

            while (next != null)
            {
                var temp = next->Next;
                next->Reset();
                next = temp;
            }
        }

        /// <summary>
        /// Frees all allocated memory in the arena and resets it.
        /// </summary>
        public void Free()
        {
            NativeMemory.Free(Data);
            Data = null;

            var next = Next;
            while (next != null)
            {
                next->Free();
                next = next->Next;
            }

            Reset();
        }

        public SafeHandle<Arena> GetArenaByItemIndex(int index,int itemSize,out int indexInArena)
        {
            return ArenaManager.GetArenaByItemIndex(CurrentHandle, index, itemSize, out indexInArena);
        }

        /// <summary>
        /// Disposes the arena, freeing all allocated memory.
        /// </summary>
        public void Dispose()
        {
            Free();
        }
    }
}