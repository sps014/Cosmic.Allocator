using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Allocator;
using Cosmic.Element;

namespace Cosmic;

public unsafe ref struct Cosmic
{
    public static Arena* Arena { get;private set; }

    private static long ElementIdCounter = 0;

    public static void Initialize(Arena* arena)
    {
        Arena = arena;
    }

    public static void BeginLayout()
    {
        if (Arena == null)
           throw new NullReferenceException("Arena is not initialized, use Initialize method");

    }

    public static void EndLayout()
    {
        if (Arena == null)
            throw new NullReferenceException("Arena is not initialized, use Initialize method");
    }

    public static Rectangle* Rectangle()
    {
        var rect= (Rectangle*)Arena->Alloc((nuint)sizeof(Rectangle));
        rect->IntenalId = CreateId();
        return rect;
    }
    public static Stack* Stack()
    {
        var stack = (Stack*)Arena->Alloc((nuint)sizeof(Stack));
        stack->IntenalId = CreateId();
        return stack;
    }

    public static long CreateId()
    {
        return ElementIdCounter++;
    }

    public static void Dispose()
    {
        ElementIdCounter = 0;

        if (Arena == null)
            return;

       Arena->Free();
    }




}
