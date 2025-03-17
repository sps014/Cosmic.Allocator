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
        /// What is the starting position of arena if all arenas are considered to be continously allocated 
        /// </summary>
        public nuint StartingOffset { get; internal set; }

        /// <summary>
        /// Return Safe Arena Handle
        /// </summary>
        public ArenaSafeHandle CurrentHandle => AsSafeHandle();

        /// <summary>
        /// Returns Next Arena's Safe Handle, Returns SafeHandle.Zero if no Next Arena linked
        /// </summary>
        public ArenaSafeHandle NextArenaHandle=> NextArenaAsSafeHandle();

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
            StartingOffset = 0;
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
            StartingOffset = 0;
        }


        /// <summary>
        /// Reduce the Size pointer  (it doesn't dealloc memory) , <i>should not be used unless you know what you are doing.</i>
        /// </summary>
        /// <param name="size">amount to decrement</param>
        public void Reduce(nuint size)
        {
        
            if(size > Capacity)
                throw new OutOfMemoryException("Can't reduce more than the size or capacity");

            if (size > Size)
            {
                Size = 0;
                return;
            }

            Size = Size - size;
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


        //Return the Current Arena as Pointer
        public Arena* AsPointer()
        {
            fixed (Arena* arena = &this)
            {
                return arena;
            }
        }

        private ArenaSafeHandle AsSafeHandle()
        {
            return new ArenaSafeHandle((IntPtr)AsPointer());
        }

        private ArenaSafeHandle NextArenaAsSafeHandle()
        {
            if (Next == null)
                return ArenaSafeHandle.Zero;

            return Next->AsSafeHandle();
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


        /// <summary>
        /// Disposes the arena, freeing all allocated memory.
        /// </summary>
        public void Dispose()
        {
            Free();
        }
    }
}