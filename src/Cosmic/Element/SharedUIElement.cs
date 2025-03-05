using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmic.Element;

internal ref struct SharedUIElement
{
    internal unsafe static T Add<T,G>(ref T current,ref G child) where G : struct, IUIElement<G> where T : struct,IUIElement<T>
    {
        if (current.ChildNode == null)
            current.ChildNode = Cosmic.ChildNode(child.Kind, child.Address);
        else
            current.ChildNode->AddNextChild(child.Kind, child.Address);
        return current;
    }
}
