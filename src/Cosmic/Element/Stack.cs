using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe ref struct Stack : IUIElement
{
    public Point Position { get; set; }
    public Size Size { get; set; }

    public ElementKind Kind => ElementKind.Stack;

    public long IntenalId { get; set; }
    public ChildInfo* ChildNode { get;set;}
    public void* Address { get; set;}

    public Stack Add(ref Rectangle rectangle)
    {
        if(ChildNode==null)
        {
            ChildNode = Cosmic.ChildNode(ElementKind.Rectangle,rectangle.Address);
            return this;
        }

        ChildNode->AddNextChild(ElementKind.Rectangle,rectangle.Address);

        return this;
    }
    public Stack Add(ref Stack stack)
    {
        if(ChildNode==null)
        {
            ChildNode = Cosmic.ChildNode(ElementKind.Stack,stack.Address);
            return this;
        }

        ChildNode->AddNextChild(ElementKind.Stack,stack.Address);

        return this;
    }
}
