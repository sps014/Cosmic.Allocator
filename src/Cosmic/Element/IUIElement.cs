﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe interface IUIElement<T> where T : struct, IUIElement<T>
{
    ElementKind Kind { get; }
    LayoutDirection Direction { get; set; }
    long IntenalId { get; set; }
    Point Position { get; set; }
    Size Size { get; set; }
    ChildInfo* ChildNode { get; set; }
    void* Address { get; internal set; }
    public T Add<G>(G child) where G : struct, IUIElement<G>;
    T Orientation(LayoutDirection layoutDirection);
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