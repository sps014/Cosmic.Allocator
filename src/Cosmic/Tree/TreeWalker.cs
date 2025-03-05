using System.Runtime.CompilerServices;
using Cosmic.Element;

namespace Cosmic.Tree;

public unsafe ref struct TreeWalker
{
    public static Stack* Root { get; private set; }
    public static void Init(ref Stack root)
    {
        Root = (Stack*)Unsafe.AsPointer(ref root);
    }

    public static void Dfs()
    {
        if (Root == null)
            return;

        DfsInternal(Root);
    }
    public static void DfsInternal<T>(T* current,int sp =0) where T : unmanaged,IUIElement<T> 
    {
        Console.WriteLine("|".PadRight(sp,'_')+" "+typeof(T).Name);

        var child = current->ChildNode;

        while (child != null)
        {
           switch(child->Kind)
            {
                case ElementKind.Stack:
                    DfsInternal(child->GetStack(),sp+4);
                    break;
                case ElementKind.Rectangle:
                    DfsInternal(child->GetRect(),sp+4);
                    break;
                default:
                    throw new NotImplementedException();
            }
            child = child->Next;
        }
    }
}