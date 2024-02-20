using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Native
{
    public partial class WrenNativeFn
    {
        #region Delegate Declarations

        // Memory
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate IntPtr Reallocate(IntPtr memory, ulong newSize, IntPtr userData);

        // Modules
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WrenLoadModuleResult LoadModule(IntPtr vm, string name);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void LoadModuleComplete(IntPtr vm, string name, WrenLoadModuleResult result);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate string ResolveModule(IntPtr vm, string importer, string name);

        // Writers
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Write(IntPtr vm, string text);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void Error(IntPtr vm, WrenErrorType errorType, string module, int line, string message);

        // Foreign
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate WrenForeignClassMethods BindForeignClass(IntPtr vm, string module, string className);


        #endregion
    }

    /// <summary>
    /// Represents the native <c>WrenConfiguration</c> struct that is passed to Wren when creating new VM.<para/>
    /// <b>C type:</b> <c>struct WrenConfiguration</c>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 88)]
    public struct WrenConfiguration
    {
        /// <summary>
        /// Creates a new, initialized <see cref="WrenConfiguration"/>. This method creates a new configuration,
        /// calls <see cref="Wren.InitConfiguration(ref WrenConfiguration)"/> on it and returns the initialized value.
        /// </summary>
        /// <returns>An initialized <see cref="WrenConfiguration"/>.</returns>
        public static WrenConfiguration InitializeNew()
        {
            var config = new WrenConfiguration();
            Wren.InitConfiguration(ref config);
            return config;
        }

        // This struct is passed directly into Wren native functions,
        // so the layout must match the native C WrenConfiguration type.

        public WrenNativeFn.Reallocate Reallocate;
        public WrenNativeFn.ResolveModule ResolveModule;
        public WrenNativeFn.LoadModule LoadModule;
        public WrenNativeFn.BindForeignMethod BindForeignMethod;
        public WrenNativeFn.BindForeignClass BindForeignClass;
        public WrenNativeFn.Write Write;
        public WrenNativeFn.Error Error;
        public ulong InitialHeapSize;
        public ulong MinHeapSize;
        public int HeapGrowthPercent;
        public IntPtr UserData;

        /// <summary>
        /// Indicates if the configuration has been initialized.
        /// </summary>
        public readonly bool IsInitialized => Reallocate != null && InitialHeapSize > 0;
    }

    /// <summary>
    /// Represents the native <c>WrenLoadModuleResult</c> struct that is sent to Wren when a module load request is made.<para/>
    /// <b>C type:</b> <c>struct WrenLoadModuleResult</c>
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 24)]
    public struct WrenLoadModuleResult
    {
        /// <summary>
        /// Gets a <see cref="WrenLoadModuleResult"/> representing a failed load with no callback or user data attached.
        /// </summary>
        public static WrenLoadModuleResult Failed => new WrenLoadModuleResult
        {
            Source = IntPtr.Zero,
            UserData = IntPtr.Zero,
#if WRENSHARP_UNITY
            OnCompleteCallback = IntPtr.Zero,
#else
            OnCompleteCallback = null,
#endif
        };

        // This struct is passed directly into Wren native functions,
        // so the layout must match the native C WrenConfiguration type.

        /// <summary>
        /// Pointer to the string of Wren source that will be interpreted for the module.
        /// A value of <see cref="IntPtr.Zero"/> should be used if the module was not found.
        /// </summary>
        public IntPtr Source;

        /// <summary>
        /// Function pointer to a <see cref="LoadModuleComplete"/> delegate.
        /// </summary>
#if WRENSHARP_UNITY
        // Unity IL2CPP fails to marshal the WrenNativeFn.LoadModuleComplete via p/invoke automatically.
        // Using an IntPtr and marshalling it manually as the correct type seems to work around this problem.
        // Possibly because one of its arguments is a struct of this type (WrenLoadModuleResult)?
        public IntPtr OnCompleteCallback;
#else
        public WrenNativeFn.LoadModuleComplete OnCompleteCallback;
#endif

        /// <summary>
        /// Pointer to user data
        /// </summary>
        public IntPtr UserData;
    }

#if WRENSHARP_EXT
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    public readonly struct WrenFiberResume
    {
        private readonly IntPtr m_Fiber;
        private readonly IntPtr m_ApiStack;

        /// <summary>
        /// Indicates if the value represents a value resume token.
        /// </summary>
        public bool IsValid => m_Fiber != IntPtr.Zero;
    }
#endif
}
