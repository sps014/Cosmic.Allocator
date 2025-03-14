using Cosmic.Allocator;
using System.Collections;

{
    using Arena arena = ArenaManager.Create(10240);
    var data = arena.Alloc<Point>();
    data[0].Y = 10;
    data[0].X = 5;
    Console.WriteLine(data[0].ToString());


    using List<Point> point = new List<Point>();

    point.Add(new Point()
    {
        X = 10,
        Y = 10
    });
    point.Add(new Point()
    {
        X = 5,
        Y = 5
    });
    point.Add(new Point()
    {
        X = 3,
        Y = 2
    });

    foreach(var item in point.AsSpan())
    {
        Console.WriteLine(item.ToString());
    }
}

unsafe ref struct List<T>: IDisposable where T : unmanaged
{
    Arena arena;
    public int Length { get; private set; }
    private int ItemSize { get; }
    public List()
    {
        ItemSize = sizeof(T);
        arena = ArenaManager.Create((nuint)(1000 * ItemSize));
    }

    public void Add(T data)
    {
        var t = arena.Alloc<T>();
        t.Fill(data);
        Length++;
    }

    public Span<T> AsSpan()
    {
        return arena.AsSpan<T>();
    }

    public void Dispose()
    {
        arena.Dispose();
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

