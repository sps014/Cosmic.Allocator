using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe struct Rectangle : IUIElement<Rectangle>
{
    public readonly ElementKind Kind => ElementKind.Rectangle;
    public Point Position { get; set; }
    public Size Size { get; set; }
    public long IntenalId { get; set; }
    public unsafe ChildInfo* ChildNode { get;set;}
    public void* Address { get; set;}
    public LayoutDirection Direction { get; set; }

    public Rectangle Add<G>(G child) where G : struct, IUIElement<G>
    {
        return SharedUILogic.Add(ref this, child);
    }

    public Rectangle Orientation(LayoutDirection layoutDirection)
    {
        Direction = layoutDirection;
        return this;
    }
}
