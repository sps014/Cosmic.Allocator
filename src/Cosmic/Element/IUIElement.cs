using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe interface IUIElement<T> where T : struct, IUIElement<T>
{
    ElementKind Kind { get; }
    LayoutDirection direction { get; set; }
    long IntenalId { get; set; }
    Point position { get; set; }
    Size size { get; set; }
    ChildInfo* ChildNode { get; set; }
    void* Address { get; internal set; }
    public T Add<G>(G child) where G : struct, IUIElement<G>;
    ref T Orientation(LayoutDirection layoutDirection);
}

public enum ElementKind
{
    Stack,
    Rectangle,
    Text,
    Image
}

public enum LayoutDirection
{
    LeftToRight,
    TopToBottom,
}