using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe interface IUIElement
{
    long IntenalId { get; set; }
    Point Position { get; set; }
    Size Size { get; set; }
    ElementKind Kind { get; }
    ChildInfo* ChildNode { get; set; }
    void* Address {get;internal set;}
}


public enum ElementKind
{
    Stack,
    Rectangle,
    Text,
    Image
}