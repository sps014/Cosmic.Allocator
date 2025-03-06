using Cosmic.Attributes;
using Cosmic.Core;

namespace Cosmic.Element;

[UIStruct]
public unsafe partial struct Stack : IUIElement<Stack>
{
    public readonly ElementKind Kind => ElementKind.Stack;

    [UIProperty]
    public LayoutDirection direction { get; set; }

    [UIProperty]
    public Point position { get; set; }

    [UIProperty]
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
