using System.Collections;

{

    using NativeList<Point> pointList = new NativeList<Point>();

    for(int i=0;i<4009;i++)
    {
        pointList.Add(new Point(i,i));
    }

    pointList.Set(4008, new Point(1, 1));

    pointList.RemoveAt(0);
    pointList.InsertAt(^1, new Point(-2, -2));
    //pointList.RemoveAt(^1);

    pointList.IterateAll();



    //for (int i = 0; i < pointList.Count; i++)
    //{
    //    var pt = pointList[i];
    //    Console.WriteLine(pt.ToString());
    //}
}

readonly struct Point
{
    public int X { get; init; }
    public int Y { get; init; }

    public Point(int x, int y)
    {
        X = x; Y = y;
    }

    public override string ToString()
    {
        return $"Point ->  ({X},{Y})";
    }
}

