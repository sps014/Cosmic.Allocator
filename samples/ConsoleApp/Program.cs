using Cosmic.Allocator;
using System.Collections;

{


    using NativeArray<Point> point = new NativeArray<Point>();

    for(int i=0;i<4009;i++)
    {
        point.Add(new Point(i,i));
    }


    for (int i = 0; i < point.Length; i++)
    {
        var pt = point[i];
        Console.WriteLine(pt.ToString());
    }
}

unsafe ref struct NativeArray<T> : IDisposable where T : unmanaged
{
    Arena arena;
    public int Length { get; private set; }
    private int ItemSize { get; }

    SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1,1);
    public NativeArray()
    {
        ItemSize = sizeof(T);
        arena = ArenaManager.Create((nuint)(ItemSize)*4);
    }

    public unsafe void Add(T data)
    {
        semaphoreSlim.Wait();
        unsafe
        {
            T* t = (T*)arena.Alloc((nuint)sizeof(T));
            *t = data;
            Length++;
        }
        semaphoreSlim.Release();
    }

    unsafe T GetItem(int index)
    {
        var nestedArena = ArenaManager.GetArenaByIndex(arena.AsPointer()->CurrentHandle,index, ItemSize, out int offset);

        if (nestedArena == SafeHandle<Arena>.Zero)
            throw new Exception("Invalid Index");

        return nestedArena.AsPointer()->DataRegion.GetItem<T>(offset/ItemSize);

    }
    public void Dispose()
    {
        arena.Dispose();
    }

    public T this[Index index]
    {
        get => GetItem(index.GetOffset(Length));
        set => arena.DataRegion.SetItem(index.GetOffset(Length),value);
    }
}

struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public Point(int x, int y)
    {
        X = x; Y = y;
    }

    public override string ToString()
    {
        return $"{X} {Y}";
    }
}

