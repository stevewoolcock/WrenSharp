using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Native
{
    public partial class Wren
    {
        public const string NativeLibrary = "wren";
    }

    public partial class WrenNativeFn
    {
        // Foriegn
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate ForeignMethod BindForeignMethod(IntPtr vm, string module, string className, byte isStatic, string signature);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void ForeignMethod(IntPtr vm);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Finalizer(IntPtr data);
    }

    /// <summary>
    /// Represents the native <c>WrenForeignClassMethods</c> struct that provides <c>allocate</c> and <c>finalize</c> 
    /// function pointers for a Wren foreign class instantiation.<para/>
    /// <b>C type:</b> <c>struct WrenForeignClassMethods</c>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public struct WrenForeignClassMethods
    {
        // This struct is passed directly into Wren native functions,
        // so the layout must match the native C WrenConfiguration type.

        public WrenNativeFn.ForeignMethod Allocate;
        public WrenNativeFn.Finalizer Finalize;
    }
}
