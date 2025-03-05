using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe interface IUIElement<T> where T: struct, IUIElement<T>
{
    long IntenalId { get; set; }
    Point Position { get; set; }
    Size Size { get; set; }
    ElementKind Kind { get; }
    ChildInfo* ChildNode { get; set; }
    void* Address {get;internal set;}
    public T Add<G>(ref G child) where G : struct, IUIElement<G>;
}


public enum ElementKind
{
    Stack,
    Rectangle,
    Text,
    Image
}