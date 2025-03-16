using Cosmic.Allocator;
using System.Collections;

{
    using Arena arena = ArenaManager.Create(10240);
    var data = arena.Alloc<Point>();
    data[0].Y = 10;
    data[0].X = 5;
    Console.WriteLine(data[0].ToString());


    using NativeArray<Point> point = new NativeArray<Point>();

    for(int i=0;i<1000;i++)
    {
        point.Add(new Point()
        {
            X = 10+i,
            Y = 10+i
        });
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
    public NativeArray()
    {
        ItemSize = sizeof(T);
        arena = ArenaManager.Create((nuint)(100 * ItemSize));
    }

    public void Add(T data)
    {
        var t = arena.Alloc<T>();
        t.Fill(data);
        Length++;
    }
    public void Dispose()
    {
        arena.Dispose();
    }

    public T this[Index index]
    {
        get => arena.DataRegion.GetItem<T>(index.GetOffset(Length));
        set => arena.DataRegion.SetItem(index.GetOffset(Length),value);
    }
}

struct Point
{
    public int X { get; set; }
    public int Y { get; set; }

    public override string ToString()
    {
        return $"{X} {Y}";
    }
}

