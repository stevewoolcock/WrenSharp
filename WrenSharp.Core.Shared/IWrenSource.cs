using System;

namespace WrenSharp
{
    /// <summary>
    /// An interface representing a Wren source provider. <see cref="IWrenSource"/> instances
    /// can be passed into the Wren VM to be interpreted, and are also returned by
    /// <see cref="IWrenModuleProvider"/> instances when module source is requested.
    /// </summary>
    public interface IWrenSource : IDisposable
    {
        /// <summary>
        /// Get a pointer to the bytes containing the Wren script source that can be passed
        /// to the native Wren VM.
        /// </summary>
        /// <param name="byteCount">The number of bytes in the source.</param>
        /// <returns>A pointer to the source bytes.</returns>
        IntPtr GetSourceBytes(out int byteCount);
    }
}
