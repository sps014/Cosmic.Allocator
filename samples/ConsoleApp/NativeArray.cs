using Cosmic.Allocator;

unsafe ref struct NativeArray<T> : IDisposable where T : unmanaged
{
    Arena arena;
    public int Length { get; private set; }
    private int ItemSize { get; }

    public NativeArray()
    {
        ItemSize = sizeof(T);
        arena = ArenaManager.Create((nuint)(ItemSize)*4);
    }

    public void Add(T data)
    {
        Span<T> t = arena.Alloc<T>(1);
        t[0] = data;
        Length++;
    }

    T GetItem(int index)
    {
        return arena.GetItemInAll<T>(index);
    }

    public void Set(int index,T item)
    {
        arena.SetItemInAll(index,item);
    }

    public void Dispose()
    {
        arena.Dispose();
    }

    public T this[Index index]
    {
        get => GetItem(index.GetOffset(Length));
    }
}

