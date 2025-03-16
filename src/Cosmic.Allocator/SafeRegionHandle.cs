namespace Cosmic.Allocator;

public readonly struct SafeRegionHandle 
{
    public readonly IntPtr Address { get; }
    public readonly int Size { get; }

    public static SafeRegionHandle Zero = new SafeRegionHandle();

    public SafeRegionHandle()
    {
        Address = IntPtr.Zero;
        Size = 0;
    }
    public SafeRegionHandle(IntPtr address, int size)
    {
        if(size<0)
            throw new ArgumentOutOfRangeException(nameof(size));

        Address = address;
        Size = (int)size;
    }
    public unsafe SafeRegionHandle(void* address, int size)
    {
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size));

        Address = (IntPtr)address;
        Size = size;
    }


    /// <summary>
    /// Returns item of current arena as span
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public unsafe Span<T> AsSpan<T>() where T : unmanaged
    {
        return new Span<T>((void*)Address, Size/sizeof(T));
    }

    /// <summary>
    /// Set item at local index <b>in current Arena only</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="index">local index specific to this arena</param>
    /// <param name="item"></param>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public void SetItem<T>(int index,T item) where T : unmanaged
    {
        if(index < 0 || index>=Size)
            throw new ArgumentOutOfRangeException("index");

        var span = AsSpan<T>();
        span[index] = item;
    }

    /// <summary>
    /// Get item at local index <b>in current Arena only</b>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="index">local index specific to this arena</param>
    /// <returns>Item</returns>
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public T GetItem<T>(int index) where T : unmanaged
    {
        if (index < 0 || index >= Size)
            throw new ArgumentOutOfRangeException("index");

        var span = AsSpan<T>();
        return span[index];
    }
}
