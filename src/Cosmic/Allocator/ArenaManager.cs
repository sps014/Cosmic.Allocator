using System.Runtime.InteropServices;
namespace Cosmic.Allocator;

public unsafe struct ArenaManager
{
    public static Arena* Create(nuint capacity)
    {
        Arena* arena = (Arena*)NativeMemory.Alloc((nuint)sizeof(Arena));
        arena->Capacity = capacity;
        arena->Next = null;
        arena->Size = 0;
        arena->Data = NativeMemory.Alloc(capacity);
        return arena;
    }
    public static void Free(Arena* arena)
    {
        arena->Free();
        NativeMemory.Free(arena);
    }

}
