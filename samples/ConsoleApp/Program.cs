using System.Collections;

{

    using NativeList<Point> pointArray = new NativeList<Point>();

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

