using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Core;

public struct Size : IEquatable<Size>
{
    public SizeMode SizeMode;
    public double Width { get; set; }
    public double Height { get; set; }
    public double MaximumWidth { get; set; }
    public double MaximumHeight { get; set; }
    public double MinimumWidth { get; set; }
    public double MinimumHeight { get; set; }

    public Size()
    {
        SizeMode=SizeMode.Fit;
        Width = -1;
        Height = -1;
        MaximumWidth = -1;
        MaximumHeight = -1;
        MinimumWidth = -1;
        MinimumHeight = -1;

    }

    public Size(double width, double height)
    {
        SizeMode = SizeMode.Fit;
        Width = width;
        Height = height;
    }
    public Size(double width, double height, double minWidth, double minHeight)
    {
        SizeMode = SizeMode.Fit;
        Width = width;
        Height = height;
        MinimumWidth = minWidth;
        MinimumHeight = minHeight;
    }
    public Size(double width, double height, double minWidth, double minHeight, double maxWidth, double maxHeight)
    {
        SizeMode = SizeMode.Fit;
        Width = width;
        Height = height;
        MinimumWidth = minWidth;
        MinimumHeight = minHeight;
        MaximumHeight = maxHeight;
        MaximumWidth = maxWidth;
    }

    public readonly bool Equals(Size other)
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

public enum SizeMode
{
    Fit,
    Grow,
    Fixed
}