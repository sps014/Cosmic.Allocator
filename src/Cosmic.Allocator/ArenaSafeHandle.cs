using System.Drawing;
using System.Runtime.CompilerServices;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Cosmic.Allocator;

public readonly struct ArenaSafeHandle
{
    public static ArenaSafeHandle Zero = new ArenaSafeHandle();
    public readonly IntPtr Address { get; }
    public unsafe SafeRegionHandle DataRegion => AsPointer()->DataRegion;

    public ArenaSafeHandle()
    {
        Address = IntPtr.Zero;
    }

    public ArenaSafeHandle(IntPtr address)
    {
        Address = address;
    }

    public unsafe ArenaSafeHandle(ref Arena value)
    {
        Address = (IntPtr)Unsafe.AsPointer(ref value);
    }

    public unsafe Arena* AsPointer()
    {
        return (Arena*)Address.ToPointer();
    }

    public bool Equals(ArenaSafeHandle other)
    {
        return Address == other.Address;
    }

    public bool Equals(ArenaSafeHandle x, ArenaSafeHandle y)
    {
        return x.Address == y.Address;
    }

    // Override ==
    public static bool operator==(ArenaSafeHandle left, ArenaSafeHandle right)
    {
        return left.Address == right.Address;
    }

    // Override !=
    public static bool operator !=(ArenaSafeHandle left, ArenaSafeHandle right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj is ArenaSafeHandle other)
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
