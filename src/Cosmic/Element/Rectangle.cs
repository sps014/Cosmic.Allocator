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
    public Point position { get; set; }
    public Size size { get; set; }
    public long IntenalId { get; set; }
    public unsafe ChildInfo* ChildNode { get;set;}
    public void* Address { get; set;}
    public LayoutDirection direction { get; set; }

    public Rectangle Add<G>(G child) where G : struct, IUIElement<G>
    {
        return SharedUILogic.Add(ref this, child);
    }

    public ref Rectangle Orientation(LayoutDirection layoutDirection)
    {
        direction = layoutDirection;
        return ref this;
    }
}
