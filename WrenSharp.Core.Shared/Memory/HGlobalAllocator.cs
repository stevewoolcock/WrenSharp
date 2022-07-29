using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Memory
{
    /// <summary>
    /// A simple memory allocator that wraps the <see cref="Marshal.AllocHGlobal(int)"/> and
    /// <see cref="Marshal.FreeHGlobal(IntPtr)"/> methods.
    /// </summary>
    public class HGlobalAllocator : IAllocator
    {
        #region Static

        private static readonly Lazy<HGlobalAllocator> _default = new Lazy<HGlobalAllocator>(isThreadSafe: true);

        /// <summary>
        /// The single default instance of <see cref="HGlobalAllocator"/>.
        /// </summary>
        public static HGlobalAllocator Default => _default.Value;

        #endregion

        /// <summary>
        /// Allocates a contiguous block of memory that is guaranted to be at least <paramref name="size"/> bytes wide.
        /// This method wraps <see cref="Marshal.AllocHGlobal(int)"/>.
        /// </summary>
        /// <param name="size">The minimum number of bytes to allocate.</param>
        /// <returns>An <see cref="IntPtr"/> to the allocated block of memory.</returns>
        public IntPtr Allocate(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        /// <summary>
        /// Frees a previously allocated block of memory.
        /// This method wraps <see cref="Marshal.FreeHGlobal(IntPtr)"/>.
        /// </summary>
        /// <param name="ptr">A pointer to the block of memory to free.</param>
        public void Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
