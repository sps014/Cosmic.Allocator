using System.Runtime.CompilerServices;
using Cosmic.Allocator;

ref struct NativeList<T> : IDisposable where T : unmanaged
{
    Arena arena;
    public int Count { get; private set; }
    private int ItemSize { get; }

    public NativeList()
    {
        ItemSize = Unsafe.SizeOf<T>();
        arena = ArenaManager.Create((nuint)(ItemSize)*4);
    }

    public void Add(T data)
    {
        Span<T> t = arena.Alloc<T>(1);
        t[0] = data;
        Count++;
    }

    T GetItem(int index)
    {
        return arena.GetItemInAll<T>(index);
    }

    public unsafe void RemoveAt(Index index)
    {
        ValidateIndex(index);

        int id = index.GetOffset(Count);

        //shift all items in arena after index
        for(int i = id; i <Count-1; i++)
        {
            var nextVal = this[i+1];
            Set(i, nextVal);
        }

        var cur = ValidateIndex(Count);

        cur.AsPointer()->Reduce((nuint)ItemSize);
        Count--;
    }

    public void InsertAt(Index index, T data)
    {
        ValidateIndex(index);

        //add some space in end for shifting values one place
        Add(data);

        int id = index.GetOffset(Count);

        // shift all items in arena after index
        for (int i = Count-1; i > id; i--)
        {
            var prevVal = this[i - 1];
            Set(i, prevVal);
        }

        Set(id,data);
    }

    public void Set(Index index,T item)
    {
        arena.SetItemInAll(index.GetOffset(Count),item);
    }

    public void Dispose()
    {
        arena.Dispose();
    }

    public T this[Index index]
    {
        get => GetItem(index.GetOffset(Count));
        set=> arena.SetItemInAll<T>(index.GetOffset(Count), value);
    }

    ArenaSafeHandle ValidateIndex(Index index)
    {
        var cur = arena.GetArenaByItemIndex(index.GetOffset(Count), ItemSize, out int indexInArena);

        if (indexInArena == -1 || cur == ArenaSafeHandle.Zero)
            throw new IndexOutOfRangeException();

        return cur;
    }

    public void IterateAll()
    {
        ArenaSafeHandle currentArenaHandle = arena.CurrentHandle;

        do
        {
            var intSpan = currentArenaHandle.DataRegion.AsSpan<T>(); // read all memory block as Span
                                                                      //do anything with span data of a given Arena

            foreach (T i in intSpan)
            {
                Console.WriteLine(i);
            }


            //Proceed to next Arena node to read it also
            currentArenaHandle = currentArenaHandle.NextHandle;
        }
        while (currentArenaHandle != ArenaSafeHandle.Zero);
    }
}

