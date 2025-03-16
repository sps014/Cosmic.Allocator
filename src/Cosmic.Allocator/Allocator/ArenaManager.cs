using System.Runtime.InteropServices;

namespace Cosmic.Allocator
{
    /// <summary>
    /// Manages the creation and deallocation of memory arenas.
    /// </summary>
    public unsafe struct ArenaManager
    {
        /// <summary>
        /// Creates a pointer to a new arena with the specified capacity.
        /// </summary>
        /// <param name="capacity">The capacity of the memory block to allocate.</param>
        /// <returns>A pointer to the newly created arena.</returns>
        public static Arena* CreatePointer(nuint capacity)
        {
            if (capacity == 0)
                throw new Exception("Arena size can't be 0");

            Arena* arena = (Arena*)NativeMemory.Alloc((nuint)sizeof(Arena));
            arena->Capacity = capacity;
            arena->Next = null;
            arena->Size = 0;
            arena->Data = NativeMemory.Alloc(capacity);
            return arena;
        }

        /// <summary>
        /// Creates a new arena with the specified capacity.
        /// </summary>
        /// <param name="capacity">The amount of bytes to allocate in the arena. This amount is used if the arena needs to grow.</param>
        /// <returns>A new instance of the <see cref="Arena"/> struct.</returns>
        /// <exception cref="Exception">Thrown when the capacity is zero.</exception>
        public static Arena Create(nuint capacity)
        {
            if (capacity == 0)
                throw new Exception("Arena size can't be 0");

            var arena = new Arena(capacity);
            return arena;
        }

        /// <summary>
        /// Releases the memory of the specified arena and its subsequent arenas.
        /// </summary>
        /// <param name="arena">A pointer to the arena to be freed.</param>
        public static void Free(Arena* arena)
        {
            arena->Free();
            NativeMemory.Free(arena);
        }
    }
}