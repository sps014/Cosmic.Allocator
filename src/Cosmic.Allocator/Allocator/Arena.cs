using System.Runtime.InteropServices;

namespace Cosmic.Allocator
{
    /// <summary>
    /// Represents a memory arena for efficient allocation and deallocation of memory blocks.
    /// </summary>
    public unsafe ref struct Arena : IDisposable
    {
        /// <summary>
        /// Pointer to the next arena in the chain.
        /// </summary>
        public Arena* Next { get; internal set; }

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
        /// Initializes a new instance of the <see cref="Arena"/> struct with no capacity.
        /// <br></br><b><i>Note: A new arena should be created using the ArenaManager instead of direct constructor call.</i></b>
        /// </summary>
        public Arena()
        {
            Capacity = 0;
            Size = 0;
            Data = null;
            Next = null;
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
            return Next->Alloc(size);
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