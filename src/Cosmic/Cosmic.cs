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

    public static void Initialize(nuint capacity)
    {
        Arena = ArenaManager.Create(capacity);
    }
    public static void Initialize(MemoryUsage usage = MemoryUsage.Medium)
    {
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
        rect->ChildNode = null;
        return rect;
    }
    public static Stack* Stack()
    {
        var stack = (Stack*)Arena->Alloc((nuint)sizeof(Stack));
        stack->IntenalId = CreateId();
        stack->ChildNode = null;
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

/// <summary>
/// Memory usage in MB
/// </summary>
public enum MemoryUsage
{
    Low = 4,
    Medium = 10,
    High = 20
}
