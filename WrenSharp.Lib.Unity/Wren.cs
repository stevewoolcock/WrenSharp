using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Native
{
    public partial class Wren
    {
#if WRENSHARP_UNITY_INTERNAL
        public const string NativeLibrary = "__Internal";
#else
        public const string NativeLibrary = "wren_unity";
#endif
    }

    public partial class WrenNativeFn
    {
        // Foriegn
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WrenForeignMethodData BindForeignMethod(IntPtr vm, string module, string className, byte isStatic, string signature);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ForeignMethod(IntPtr vm, ushort symbol);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Finalizer(IntPtr data, ushort symbol);
    }

    /// <summary>
    /// Represents the native <c>WrenForeignClassMethods</c> struct that provides <c>allocate</c> and <c>finalize</c> 
    /// function pointers for a Wren foreign class instantiation.<para/>
    /// <b>C type:</b> <c>struct WrenForeignClassMethods</c>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = WrenForeignClassMethods.Size)]
    public struct WrenForeignClassMethods
    {
        /// <summary>
        /// The size of the struct type, in bytes.
        /// </summary>
        public const int Size = 32;

        // This struct is passed directly into Wren native functions,
        // so the layout must match the native C WrenConfiguration type.

        public WrenNativeFn.ForeignMethod Allocate;
        public ushort AllocateSymbol;

        public WrenNativeFn.Finalizer Finalize;
        public ushort FinalizeSymbol;
    }

    /// <summary>
    /// Represents the native <c>WrenFindForeignMethodResult</c> struct that provides a function pointer for a foreign method
    /// in a Wren class.<para/>
    /// <b>C type:</b> <c>struct WrenFindForeignMethodResult</c>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = WrenForeignMethodData.Size)]
    public unsafe struct WrenForeignMethodData
    {
        /// <summary>
        /// The size of the struct type, in bytes.
        /// </summary>
        public const int Size = 16;

        /// <summary>
        /// A <see cref="WrenForeignMethodData"/> representing a method that could not be found.
        /// </summary>
        public static WrenForeignMethodData NotFound => new WrenForeignMethodData() { Function = null, Symbol = 0 };

        // This struct is passed directly into Wren native functions,
        // so the layout must match the native C WrenConfiguration type.

        public WrenNativeFn.ForeignMethod Function;
        public ushort Symbol;
    }
}
