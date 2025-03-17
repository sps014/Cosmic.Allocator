using System.Collections;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

var summary = BenchmarkRunner.Run<Benchmarks>();


public class Benchmarks
{

    [Benchmark]
    public void NativeViaArena()
    {
        using NativeList<Point> pointList = new NativeList<Point>();

        for (int i = 0; i < 4009; i++)
        {
            pointList.Add(new Point(i, i));
        }

        pointList.Set(4008, new Point(1, 1));

        pointList.RemoveAt(0);
        pointList.InsertAt(^1, new Point(-2, -2));
        //pointList.RemoveAt(^1);
    }

    [Benchmark]
    public void ListViaArena()
    {
        List<Point> pointList = new List<Point>();

        // Add points to the list
        for (int i = 0; i < 4009; i++)
        {
            pointList.Add(new Point(i, i));
        }

        // Set a value at a specific index
        pointList[4008] = new Point(1, 1);

        // Remove the first element
        pointList.RemoveAt(0);

        // Insert a new element at the end
        pointList.Insert(pointList.Count, new Point(-2, -2)); // Equivalent to InsertAt(^1)

    }
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

