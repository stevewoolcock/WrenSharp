#if WRENSHARP_EXT
using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Unsafe
{
    public enum WrenObjType
    {
        // No Wren equivalent
        Unknown = -1,

        // OBJ_CLASS
        Class,

        // OBJ_CLOSURE
        Closure,

        // OBJ_FIBER
        Fiber,

        // OBJ_FN
        Function,

        // OBJ_FOREIGN
        Foreign,

        // OBJ_INSTANCE
        Instance,

        // OBJ_LIST
        List,

        // OBJ_MAP
        Map,

        // OBJ_MODULE
        Module,

        // OBJ_RANGE
        Range,

        // OBJ_STRING
        String,

        // OBJ_UPVALUE
        Upvalue
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct WrenObj
    {
        public WrenObjType Type;
        private readonly byte IsDark;
        public WrenObjClass* ClassObj;
        private readonly WrenObj* Next;
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe readonly struct WrenObjClass
    {
        public readonly WrenObj Object;
        public readonly WrenObjClass* Superclass;
        public readonly int NumFields;
        private readonly WrenBuffer<WrenMethod> m_MethodBuffer;
        internal readonly WrenObjString* m_Name;

        // Flexible array, size is undefined
        internal readonly WrenValue m_AttributesValue;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenObjString
    {
        /// <summary>
        /// Converts a UTF8 Wren string to a managed string.
        /// </summary>
        /// <param name="str">The <see cref="WrenObjString"/> to convert.</param>
        /// <returns>A managed string containing the contents of <paramref name="str"/>.</returns>
        public static string ToManagedString(WrenObjString* str) => System.Text.Encoding.UTF8.GetString(&str->ValueArray, str->Length);

        public WrenObj Obj;
        public int Length;
        public int Hash;

        // Flexible array, size is undefined
        public byte ValueArray;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenObjInstance
    {
        public WrenObj Obj;

        // Flexible array, size is undefined
        public WrenValue Fields;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenObjRange
    {
        public WrenObj Obj;
        public double From;
        public double To;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsInclusive;
    }

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct WrenBuffer<T> where T : unmanaged
    {
        internal T* m_Data;
        internal int m_Count;
        internal int m_Capacity;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal unsafe readonly partial struct WrenMethod
    {
        [FieldOffset(0)]
        private readonly IntPtr m_Primitive;

        // The rest of this must be filled out by the main project implementation,
        // as it differs between the standard lib and the Unity lib.
    }
}
#endif