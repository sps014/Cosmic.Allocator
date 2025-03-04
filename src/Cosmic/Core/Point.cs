using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Core;

public readonly struct Point(in int x, in int y) : IEquatable<Point>
{
    public readonly int X { get; } = x;
    public readonly int Y { get; } = y;

    public bool Equals(Point other)
    {
        return X == other.X && Y == other.Y;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }

    public override string ToString()
    {
        return $"Point({X},{Y})";
    }
}
