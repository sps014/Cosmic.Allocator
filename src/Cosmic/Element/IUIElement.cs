using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe interface IUIElement
{
    public long IntenalId { get; set; }
    public Point Position { get; set; }
    public Size Size { get; set; }
    public ElementKind Kind { get; }
    public ChildInfo* ChildNode { get; set; }
}


public enum ElementKind
{
    Stack,
    Rectangle,
    Text,
    Image
}