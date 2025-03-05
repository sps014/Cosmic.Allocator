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
    internal static Arena* StackArena { get;private set; }
    internal static Arena* RectArena { get;private set; }
    internal static Arena* ImageArena { get;private set; }
    internal static Arena* TextArena { get;private set; }
    internal static Arena* ChildInfoArena { get;private set; }

    private static long ElementIdCounter = 0;

    public static void Initialize(int maxCapacity)
    {
        nuint length = (nuint)Enum.GetValues<ElementKind>().Length+1;
        nuint perArena = (nuint)maxCapacity/length;

        StackArena = ArenaManager.Create(perArena);
        RectArena = ArenaManager.Create(perArena);
        ImageArena = ArenaManager.Create(perArena);
        TextArena = ArenaManager.Create(perArena);
        ChildInfoArena = ArenaManager.Create(perArena);

    }
    public static void Initialize(MemoryUsage usage = MemoryUsage.Medium)
    {
        Initialize((int)usage*1024*1024);
    }

    private static void CheckArenasInitialization()
    {
        if (StackArena == null || RectArena == null || ImageArena == null || TextArena == null || ChildInfoArena == null)
            throw new ArgumentException("All Arenas are not initialized, Call Initialize()");
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
        var rectPtr=RectArena->Alloc((nuint)sizeof(Rectangle));
        var rect = CreateElement<Rectangle>(rectPtr);
        return ref *rect;
    }
    public static ref Stack Stack()
    {
        var stackPtr = StackArena->Alloc((nuint)sizeof(Stack));
        var stack = CreateElement<Stack>(stackPtr);
        return ref *stack;
    }


    private static T* CreateElement<T>(void* allocAddress) where T : unmanaged, IUIElement<T>
    {
        var type = (T*)allocAddress;
        type->Address = allocAddress;
        type->IntenalId = CreateId();
        type->ChildNode = null;
        type->Size = new Size();
        type->Position = new Point();
        return type;
    }

    internal static ChildInfo* ChildNode(ElementKind kind,void* addressOfChildElement)
    {
        var childInfo = (ChildInfo*)ChildInfoArena->Alloc((nuint)sizeof(ChildInfo));
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

        StackArena->Free();
        RectArena->Free();
        ImageArena->Free();
        TextArena->Free();
        ChildInfoArena->Free();
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
