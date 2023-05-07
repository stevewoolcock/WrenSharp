#if WRENSHARP_EXT
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WrenSharp.Unsafe
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObj
    {
        public WrenObjectType Type;
        private readonly byte IsDark;
        public WrenInternalObjClass* ClassObj;
        private readonly WrenInternalObj* Next;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe readonly struct WrenInternalObjClass
    {
        public readonly WrenInternalObj Object;
        public readonly WrenInternalObjClass* Superclass;
        public readonly int NumFields;
        private readonly WrenInternalBuffer<WrenInternalMethod> m_MethodBuffer;
        public readonly WrenInternalObjString* Name;

        // Flexible array, size is undefined
        public readonly WrenValue AttributesValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObjString
    {
        public WrenInternalObj Object;
        public int Length;
        public int Hash;

        // Flexible array, size is undefined
        public byte ValueArray;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObjInstance
    {
        public WrenInternalObj Object;

        // Flexible array, size is undefined
        public WrenValue Fields;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObjForeign
    {
        public WrenInternalObj Object;

        // WrenSharp extension
        // Vanilla Wren does not store the size of the data block
        public ulong Size;

        // Flexible array, size is undefined
        public byte Data;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObjList
    {
        public WrenInternalObj Object;
        public WrenInternalBuffer<WrenValue> Elements;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObjMap
    {
        [StructLayout(LayoutKind.Sequential)]
        internal unsafe struct Entry
        {
            public WrenValue Key;
            public WrenValue Value;
        }

        public WrenInternalObj Object;
        public uint Capacity;
        public uint Count;
        public Entry* Entries;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalObjRange
    {
        public WrenInternalObj Object;
        public double From;
        public double To;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsInclusive;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenInternalBuffer<T> where T : unmanaged
    {
        public T* Data;
        public int Count;
        public int Capacity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span<T> AsSpan() => new Span<T>(Data, Count);
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe partial struct WrenInternalMethod
    {
        [FieldOffset(0)]
        private readonly IntPtr m_Primitive;

        // The rest of this must be filled out by the main project implementation,
        // as it differs between the standard lib and the Unity lib.
    }
}
#endif