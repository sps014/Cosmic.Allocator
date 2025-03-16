using System.Runtime.CompilerServices;

namespace Cosmic.Allocator;

public readonly struct SafeHandle<T>  where T : unmanaged
{
    public static SafeHandle<T> Zero = new SafeHandle<T>();
    public readonly IntPtr Address { get; }
    public SafeHandle()
    {
        Address = IntPtr.Zero;
    }

    public SafeHandle(IntPtr address)
    {
        Address = address;
    }

    public unsafe SafeHandle(ref T value)
    {
        Address = (IntPtr)Unsafe.AsPointer(ref value);
    }

    public unsafe T* AsPointer()
    {
        return (T*)Address.ToPointer();
    }

    public bool Equals(SafeHandle<T> other)
    {
        return Address == other.Address;
    }

    public bool Equals(SafeHandle<T> x, SafeHandle<T> y)
    {
        return x.Address == y.Address;
    }

    // Override ==
    public static bool operator==(SafeHandle<T> left, SafeHandle<T> right)
    {
        return left.Address == right.Address;
    }

    // Override !=
    public static bool operator !=(SafeHandle<T> left, SafeHandle<T> right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is SafeHandle<T> other)
        {
            return this == other;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return Address.GetHashCode();
    }
}
