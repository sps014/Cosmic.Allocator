using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public unsafe ref struct Rectangle : IUIElement
{
    public ElementKind Kind => ElementKind.Rectangle;
    public Point Position { get; set; }
    public Size Size { get; set; }
    public long IntenalId { get; set; }
    public unsafe ChildInfo* ChildNode { get;set;}
    public void* Address { get; set;}

}
