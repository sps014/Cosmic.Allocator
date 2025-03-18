using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Allocator;

public unsafe ref struct ArenaAllocator : IDisposable
{
    internal Arena* LastArena { get; private set; }

    public nuint Capacity { get; private set; }

    public int Count { get; private set; }

    public uint TotalArenaCount { get; private set; }

    internal Arena* ArenaMap { get; private set; }

    internal Span<IntPtr> ArenaPtrMapSpan { get; private set; }
  
    
    public ArenaAllocator()
    {
        LastArena = null;
        Capacity = 0;
        Count = 0;
        TotalArenaCount = 0;
        ArenaMap = null;
        ArenaPtrMapSpan = Span<IntPtr>.Empty;
    }
    public ArenaAllocator(nuint capacity,uint maxLinkedArenaCount)
    {
        var mainArena = ArenaManager.CreatePointer(capacity);
        LastArena = mainArena;

        Capacity = capacity;
        Count = 0;
        TotalArenaCount = Math.Max(maxLinkedArenaCount, 1);

        //create arena ptr map so we dont need to build next and previous links 
        uint arenaPtrMapSizeInByte = TotalArenaCount * (uint)Unsafe.SizeOf<IntPtr>();
        ArenaMap = ArenaManager.CreatePointer(arenaPtrMapSizeInByte);
        ArenaMap->Size = arenaPtrMapSizeInByte; 

        ArenaPtrMapSpan = ArenaMap->AsSpan<IntPtr>();

        StoreArenaAddressInMap(mainArena);
    }

    private void StoreArenaAddressInMap(Arena* arena)
    {
        if (Count >= TotalArenaCount)
            throw new Exception("Total Number of linked arena capacity is reached.");

        ArenaPtrMapSpan[Count] = (IntPtr)arena;
        LastArena = arena;
        Count++;
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
        return new Span<T>(ptr, (int)count);
    }

    /// <summary>
    /// Allocates a memory block of the specified size.
    /// </summary>
    /// <param name="size">The size of the memory block to allocate.</param>
    /// <returns>A pointer to the allocated memory block.</returns>
    /// <exception cref="OutOfMemoryException">Thrown when the allocation size is greater than the capacity.</exception>
    public void* Alloc(nuint size)
    {
        if (Count == 0)
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
        
        // If there is no next arena, create a new one and allocate from it
        var newArena = ArenaManager.CreatePointer(Capacity);
        newArena->StartingOffset = LastArena->StartingOffset + Capacity;
        
        StoreArenaAddressInMap(newArena);
        
        return AllocImpl(size);
    }

    public void Dispose()
    {
        if (Count == 0)
            return;

        for (int i = 0; i < Count; i++)
        {
            var arena = (Arena*)ArenaPtrMapSpan[i].ToPointer();
            arena->Free();
        }

        if(ArenaMap != null)
            ArenaMap->Free();
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

        if (nestedArena == null)
            throw new Exception("Invalid Index");

        return nestedArena->DataRegion.GetItem<T>(byteOffset / sizeof(T));

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

        if (nestedArena == null)
            throw new Exception("Invalid Index");

        nestedArena->DataRegion.SetItem(byteOffset / sizeof(T), item);
    }

    public unsafe Arena* GetArenaByItemIndex(int index, int itemSize, out int byteOffset)
    {
        byteOffset = -1;

        if (Count == 0)
            return null;

        var countOfItemsPerArena = (int)Capacity / itemSize;

        int indexInArenaMap = index / countOfItemsPerArena;

        if (indexInArenaMap >= Count)
            throw new Exception("index doesn't is not present in current allocated arenas.");

        var arena = (Arena*)ArenaPtrMapSpan[indexInArenaMap].ToPointer();

        byteOffset = (index % countOfItemsPerArena)*itemSize;

        return arena;
    }
}
