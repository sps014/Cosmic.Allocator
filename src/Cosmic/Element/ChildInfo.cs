using System.Runtime.InteropServices;

namespace Cosmic.Element;

public unsafe ref struct ChildInfo
{
    public ChildInfo* Next {get; set;}
    public ElementKind Kind  {get; set;}
    public void* Address  {get; set;}

    public void AddNextChild(ElementKind kind, void* addressOfChildElement)
    {
        if(Next == null)
        {
            var node =Cosmic.ChildNode(kind,addressOfChildElement);
            Next= node;
            return;
        }

        Next->AddNextChild(kind, addressOfChildElement);   
    }

    public Rectangle* GetRect()
    {
        if(Address==null)
            return null;
        return (Rectangle*)Address;
    }
     public Stack* GetStack()
    {
        if(Address==null)
            return null;
        return (Stack*)Address;
    }
}