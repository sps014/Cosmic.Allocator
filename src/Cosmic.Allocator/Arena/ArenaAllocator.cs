using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cosmic.Allocator;

public unsafe struct ArenaAllocator : IDisposable
{
    public Arena* MainArena { get; private set; }
    internal Arena* LastArena { get; private set; }
    public nuint Capacity { get; private set; }

    public int Count { get; private set; }

    public ArenaAllocator()
    {
        MainArena = null;
        LastArena = null;
        Capacity = 0;
        Count = 0;
    }
    public ArenaAllocator(nuint capacity)
    {
        MainArena = ArenaManager.CreatePointer(capacity);
        LastArena = MainArena;
        Capacity = capacity;
        Count = 1;
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
        if (MainArena == null)
            return null;

        return AllocImpl(size);
    }
    private void* AllocImpl(nuint size)
    {
        if (size > Capacity)
        {
            throw new OutOfMemoryException("Allocation size is greater than capacity");
        }

        // Check if there is enough capacity in the current arena
        if (LastArena->Size + size <= Capacity)
        {
            void* result = (byte*)LastArena->Data + LastArena->Size;
            LastArena->Size += size;
            return result;
        }

        var lastArena = LastArena;
        // If there is no next arena, create a new one and allocate from it
        var newArena = ArenaManager.CreatePointer(Capacity);
        newArena->StartingOffset = lastArena->StartingOffset + Capacity;
        
        LastArena->Next = newArena;
        newArena->Previous = lastArena;
        LastArena = newArena;
        Count++;
        
        return AllocImpl(size);
    }

    public void Dispose()
    {
        if(MainArena != null)
            MainArena->Free();
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
        var nestedArena =GetArenaByItemIndex(index, sizeof(T), out int byteOffset);

        if (nestedArena == ArenaSafeHandle.Zero)
            throw new Exception("Invalid Index");

        return nestedArena.AsPointer()->DataRegion.GetItem<T>(byteOffset / sizeof(T));

    }

    public ArenaSafeHandle GetArenaByItemIndex(int index, int itemSize, out int byteOffset)
    {
        return ArenaManager.GetArenaByItemIndex(MainArena->CurrentHandle, index, itemSize, out byteOffset);
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
        var nestedArena = GetArenaByItemIndex(index, sizeof(T), out int byteOffset);

        if (nestedArena == ArenaSafeHandle.Zero)
            throw new Exception("Invalid Index");

        nestedArena.AsPointer()->DataRegion.SetItem(byteOffset / sizeof(T), item);
    }
}
