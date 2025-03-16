using Cosmic.Allocator;
using System.Collections;

{


    using NativeArray<Point> pointArray = new NativeArray<Point>();

    for(int i=0;i<4009;i++)
    {
        pointArray.Add(new Point(i,i));
    }

    pointArray.Set(4008, new Point(1, 1));


    for (int i = 0; i < pointArray.Length; i++)
    {
        var pt = pointArray[i];
        Console.WriteLine(pt.ToString());
    }
}

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
        var nestedArena = arena.AsPointer()->GetArenaByItemIndex(index, ItemSize, out int byteOffset);

        if (nestedArena == SafeHandle<Arena>.Zero)
            throw new Exception("Invalid Index");

        return nestedArena.AsPointer()->DataRegion.GetItem<T>(byteOffset / ItemSize);

    }

    public void Set(int index,T item)
    {
        var nestedArena = arena.AsPointer()->GetArenaByItemIndex(index, ItemSize, out int byteOffset);

        if (nestedArena == SafeHandle<Arena>.Zero)
            throw new Exception("Invalid Index");

        nestedArena.AsPointer()->DataRegion.SetItem<T>(byteOffset / ItemSize,item);
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

