using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Core;

namespace Cosmic.Element;

public ref struct Rectangle : IUIElement
{
    public ElementKind Kind => ElementKind.Rectangle;
    public Point Position { get; set; }
    public FixedSize Size { get; set; }
    public long IntenalId { get; set; }
}
