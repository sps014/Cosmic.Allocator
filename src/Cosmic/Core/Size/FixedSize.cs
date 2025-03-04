using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Core;

public struct FixedSize : IEquatable<FixedSize>, ISize
{
    public double Width { get; set; }
    public double Height { get; set; }

    public readonly SizeMode SizeMode => SizeMode.Fixed;

    public double MaximumWidth { get; set; } = -1;
    public double MaximumHeight { get; set; } = -1;
    public double MinimumWidth { get; set; } = -1;
    public double MinimumHeight { get; set; } = -1;

    public FixedSize(double width, double height)
    {
        Width = width;
        Height = height;
    }
    public FixedSize(double width, double height, double minWidth, double minHeight)
    {
        Width = width;
        Height = height;
        MinimumWidth = minWidth;
        MinimumHeight = minHeight;
    }
    public FixedSize(double width, double height, double minWidth, double minHeight, double maxWidth, double maxHeight)
    {
        Width = width;
        Height = height;
        MinimumWidth = minWidth;
        MinimumHeight = minHeight;
        MaximumHeight = maxHeight;
        MaximumWidth = maxWidth;
    }

    public readonly bool Equals(FixedSize other)
    {
        return Width == other.Height && Height == other.Height;
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(Width, Height);
    }

    public override readonly string ToString()
    {
        return $"Size({Width},{Height})";
    }
}