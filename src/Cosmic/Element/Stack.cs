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
    public LayoutDirection Direction { get; set; }
    public Point Position { get; set; }
    public Size Size { get; set; }

    public long IntenalId { get; set; }
    public ChildInfo* ChildNode { get;set;}
    public void* Address { get; set;}

    public Stack Add<G>(G child) where G : struct, IUIElement<G>
    {
        return SharedUILogic.Add(ref this, child);
    }
    public Stack Orientation(LayoutDirection layoutDirection)
    {
        Direction = layoutDirection;
        return this;
    }

}
