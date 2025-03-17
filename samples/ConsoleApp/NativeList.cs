using System.Runtime.CompilerServices;
using Cosmic.Allocator;

ref struct NativeList<T> : IDisposable where T : unmanaged
{
    ArenaAllocator arena;
    public int Count { get; private set; }
    private int ItemSize { get; }

    private const int ArenaSizeMultiplier = 1<<8;

    public NativeList()
    {
        ItemSize = Unsafe.SizeOf<T>();
        arena = ArenaManager.Create((nuint)(ItemSize)*ArenaSizeMultiplier);
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

        int id = index.GetOffset(Count);
        ValidateIndex(id);


        //shift all items in arena after index
        for (int i = id; i <Count-1; i++)
        {
            var nextVal = this[i+1];
            Set(i, nextVal);
        }

        var cur = ValidateIndex(Count-1);

        cur->Reduce((nuint)ItemSize);
        Count--;
    }

    public unsafe void InsertAt(Index index, T data)
    {
        ValidateIndex(index.GetOffset(index.GetOffset(Count)));

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

    unsafe Arena* ValidateIndex(int index)
    {
        var cur = arena.GetArenaByItemIndex(index, ItemSize, out int indexInArena);

        if (indexInArena == -1 || cur == null)
            throw new IndexOutOfRangeException();

        return cur;
    }
}

