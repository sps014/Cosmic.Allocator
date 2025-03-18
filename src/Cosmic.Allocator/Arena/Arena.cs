using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
[assembly: InternalsVisibleTo("CosmicTests")]

namespace Cosmic.Allocator;

/// <summary>
/// Represents a memory arena for efficient allocation and deallocation of memory blocks.
/// </summary>
public unsafe struct Arena : IDisposable
{

    /// <summary>
    /// Pointer to the allocated memory block.
    /// </summary>
    public void* Data { get; internal set; }

    /// <summary>
    /// Total capacity of the memory block.
    /// </summary>
    public nuint Capacity { get; internal set; }

    /// <summary>
    /// Current size of the allocated memory.
    /// </summary>
    public nuint Size { get; internal set; }

    /// <summary>
    /// What is the starting position of arena if all arenas are considered to be continously allocated 
    /// </summary>
    public nuint StartingOffset { get; internal set; }

    /// <summary>
    /// Return Safe Arena Handle
    /// </summary>
    public ArenaSafeHandle CurrentHandle => AsSafeHandle();


    /// <summary>
    /// Return Current Allocated Data in the Arena (Continuous Region)
    /// </summary>
    public SafeRegionHandle DataRegion => new SafeRegionHandle(Data, (int)Size);

    /// <summary>
    /// Initializes a new instance of the <see cref="Arena"/> struct with no capacity.
    /// <br></br><b><i>Note: A new arena should be created using the ArenaManager instead of direct constructor call.</i></b>
    /// </summary>
    public Arena()
    {
        StartingOffset = 0;
        Capacity = 0;
        Size = 0;
        Data = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Arena"/> struct with the specified capacity.
    /// </summary>
    /// <param name="capacity">The capacity of the memory block.</param>
    internal Arena(nuint capacity)
    {
        Capacity = capacity;
        Size = 0;
        Data = NativeMemory.Alloc(capacity);
        StartingOffset = 0;
    }

    /// <summary>
    /// Allocate memory in a given arena
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public void* Alloc(nuint size)
    {
        if (size + Size > Capacity)
            throw new ArgumentException("size can't exceed the arena capacity");

        void* result = (byte*)Data + Size;
        Size += size;
        return result;
    }


    /// <summary>
    /// Reduce the Size pointer  (it doesn't dealloc memory) , <i>should not be used unless you know what you are doing.</i>
    /// </summary>
    /// <param name="size">amount to decrement</param>
    public void Reduce(nuint size)
    {
    
        if(size > Capacity)
            throw new OutOfMemoryException("Can't reduce more than the size or capacity");

        if (size > Size)
        {
            Size = 0;
            return;
        }

        Size = Size - size;
    }

    /// <summary>
    /// Get a Span over current arena allocated memory
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public Span<T> AsSpan<T>() where T : unmanaged
    {
        if(Data==null)
            return Span<T>.Empty;

        return new Span<T>(Data, (int)Size/sizeof(T));
    }


    //Return the Current Arena as Pointer
    public Arena* AsPointer()
    {
        fixed (Arena* arena = &this)
        {
            return arena;
        }
    }

    private ArenaSafeHandle AsSafeHandle()
    {
        return new ArenaSafeHandle((IntPtr)AsPointer());
    }



    /// <summary>
    /// Frees all allocated memory in the arena and resets it.
    /// </summary>
    public void Free()
    {
        if (Data != null)
            NativeMemory.Free(Data);
        
        Data = null;
        Size = 0;

    }


    /// <summary>
    /// Disposes the arena, freeing all allocated memory.
    /// </summary>
    public void Dispose()
    {
        Free();
    }
}