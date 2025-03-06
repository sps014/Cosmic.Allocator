using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe struct Stack : IUIElement<Stack>
{
    public readonly ElementKind Kind => ElementKind.Stack;
    public LayoutDirection direction { get; set; }
    public Point position { get; set; }
    public Size size { get; set; }

    public long IntenalId { get; set; }
    public ChildInfo* ChildNode { get;set;}
    public void* Address { get; set;}

    public Stack Add<G>(G child) where G : struct, IUIElement<G>
    {
        return SharedUILogic.Add(ref this, child);
    }
    public ref Stack Orientation(LayoutDirection layoutDirection)
    {
        direction = layoutDirection;
        return ref this;
    }

}
