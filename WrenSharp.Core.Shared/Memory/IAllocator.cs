using System;

namespace WrenSharp.Memory
{
    /// <summary>
    /// An interface for memory allocators used by some WrenSharp wrappers for working with unmanaged memory.
    /// </summary>
    public interface IAllocator
    {
        /// <summary>
        /// Allocates a contiguous block of memory that is guaranted to be at least <paramref name="size"/> bytes wide.
        /// </summary>
        /// <param name="size">The minimum number of bytes to allocate.</param>
        /// <returns>An <see cref="IntPtr"/> to the allocated block of memory.</returns>
        IntPtr Allocate(int size);

        /// <summary>
        /// Frees a previously allocated block of memory.
        /// </summary>
        /// <param name="ptr">A pointer to the block of memory to free.</param>
        void Free(IntPtr ptr);
    }
}
