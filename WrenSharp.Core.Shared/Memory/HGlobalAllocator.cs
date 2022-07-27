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

        public IntPtr Allocate(int size)
        {
            return Marshal.AllocHGlobal(size);
        }

        public void Free(IntPtr ptr)
        {
            Marshal.FreeHGlobal(ptr);
        }
    }
}
