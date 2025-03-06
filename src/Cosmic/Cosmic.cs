using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Cosmic.Allocator;
using Cosmic.Core;
using Cosmic.Element;

namespace Cosmic;

public unsafe ref struct Cosmic
{
    internal static Arena* Arena { get;private set; }

    private static long ElementIdCounter = 0;
    private const nuint MinArenaSize =  1024 * 1024;

    public static void Initialize(nuint maxCapacity)
    {
        if (maxCapacity < MinArenaSize)
            throw new Exception("Arena size too small");
        Arena = ArenaManager.Create(maxCapacity);

    }
    public static void Initialize(MemoryUsage usage = MemoryUsage.Medium)
    {
        Initialize((nuint)usage*1024*1024);
    }

    private static void CheckArenasInitialization()
    {
        if (Arena == null)
            throw new ArgumentException("Arena is not initialized, Call Initialize()");
    }

    public static void BeginLayout()
    {
        CheckArenasInitialization();
    }

    public static void EndLayout()
    {
        CheckArenasInitialization();
    }

    public static ref Rectangle Rectangle()
    {
        var rectPtr=Arena->Alloc((nuint)sizeof(Rectangle));
        var rect = CreateElement<Rectangle>(rectPtr);
        return ref *rect;
    }
    public static ref Stack Stack(LayoutDirection layoutDirection=LayoutDirection.LeftToRight)
    {
        var stackPtr = Arena->Alloc((nuint)sizeof(Stack));
        var stack = CreateElement<Stack>(stackPtr);
        stack->direction = layoutDirection;
        return ref *stack;
    }


    private static T* CreateElement<T>(void* allocAddress) where T : unmanaged, IUIElement<T>
    {
        var type = (T*)allocAddress;
        type->Address = allocAddress;
        type->IntenalId = CreateId();
        type->ChildNode = null;
        type->size = new Size();
        type->position = new Point();
        type->direction = LayoutDirection.LeftToRight;
        return type;
    }

    internal static ChildInfo* ChildNode(ElementKind kind,void* addressOfChildElement)
    {
        var childInfo = (ChildInfo*)Arena->Alloc((nuint)sizeof(ChildInfo));
        childInfo->Next=null;
        childInfo->Address = addressOfChildElement;
        childInfo->Kind= kind;
        return childInfo;
    }

    public static long CreateId()
    {
        return ElementIdCounter++;
    }

    public static void Dispose()
    {
        CheckArenasInitialization();
        ElementIdCounter = 0;
        Arena->Free();
    }
}

/// <summary>
/// Memory usage in MB
/// </summary>
public enum MemoryUsage
{
    /// <summary>
    /// Allocate low memory
    /// </summary>
    Low = 5,

    /// <summary>
    /// Allocate medium memory
    /// </summary>
    Medium = 20,

    /// <summary>
    /// Allocate high memory
    /// </summary>
    High = 40
}
