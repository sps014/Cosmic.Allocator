using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Allocator;

public unsafe ref struct Arena
{
    public Arena* Next { get; internal set; }
    public void* Data { get; internal set; }
    public nuint Capacity { get; internal set; }
    public nuint Size { get; internal set; }

    /// <summary>
    /// Creates an empty arena.
    /// <b><i>new arena should be created with the ArenaManager instead of direct constructor call</i></b>
    /// </summary>
    public Arena()
    {
        Capacity = 0;
        Size = 0;
        Data = null;
        Next = null;
    }

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
        Next = ArenaManager.Create(Capacity);
        return Next->Alloc(size);
    }

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
}
