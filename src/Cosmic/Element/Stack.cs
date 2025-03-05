using System;
using System.Collections.Generic;
using System.Linq;
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
    public unsafe ChildInfo* ChildNode { get;set;}


    public Stack Add(Rectangle* rectangle)
    {
        return this;
    }
    public Stack Add(Stack* stack)
    {
        return this;
    }
}
