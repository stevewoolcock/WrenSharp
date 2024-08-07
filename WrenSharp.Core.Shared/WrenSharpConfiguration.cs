using WrenSharp.Memory;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// Configuration data for a <see cref="WrenVM"/> managed instance.
    /// </summary>
    public struct WrenSharpConfiguration
    {
        /// <summary>
        /// The <see cref="WrenVMInitializer"/> delegate to use to initialize the native Wren VM. Leave this value null
        /// to use the default initializer (<see cref="Wren.NewVM(ref WrenConfiguration)"/>. Supplying a custom
        /// initializer allows for greater control over the routine used to create the VM (for example, using a native plugin
        /// to initialize the VM with a custom native reallocator function).
        /// </summary>
        public WrenVMInitializer Initializer;

        /// <summary>
        /// The <see cref="WrenVMDestructor"/> delegate to execute when the VM is disposed.
        /// </summary>
        public WrenVMDestructor Destructor;

        /// <summary>
        /// The allocator to use for unmanaged memory. Some WrenSharp features use unmanaged memory for performance reasons.
        /// The native Wren VM does not use this. If null, the default allocator (<see cref="HGlobalAllocator.Default"/>) is used.
        /// </summary>
        public IAllocator Allocator;

        /// <summary>
        /// The initial capacity of the internal <see cref="WrenHandle"/> pool for the VM.
        /// The specified number of handles will be pre-allocated during intialization.<para/>
        /// If zero, no handles will be allocated during initialization.
        /// </summary>
        public int HandlePoolCapacityInitial;

        /// <summary>
        /// The maximum capacity of the internal <see cref="WrenHandle"/> pool for the VM.
        /// The VM will never store more than this number of handles in the pool. If the number of handles
        /// allocated exceeds this value, the excess managed handle wrappers will not be added to the pool
        /// and left for the garbage collector to dispose.<para/>
        /// If zero, no maximum limit is enforced.
        /// </summary>
        public int HandlePoolCapacityMax;
    }
}
