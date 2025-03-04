using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Core;

public interface ISize
{
    public SizeMode SizeMode { get; }

    public double MaximumWidth { get; set; }
    public double MaximumHeight { get; set; }

    public double MinimumWidth { get; set; }
    public double MinimumHeight { get; set; }
}

public enum SizeMode
{
    Fixed,
    Grow
}