using Cosmic.Allocator;

{
    using Arena arena = ArenaManager.Create(10240);
    var data = arena.Alloc<Point>();
    data[0].Y = 10;
    data[0].X = 5;
    Console.WriteLine(data[0].ToString());
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

