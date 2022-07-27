using System;
using System.Text;
using WrenSharp.Native;
using WrenSharp.Memory;

namespace WrenSharp
{
    /// <summary>
    /// A Wren script source that wraps a string. The source is copied to an unmanaged buffer, which is passed
    /// directly to the native Wren VM when the source is interpreted.
    /// </summary>
    public unsafe class WrenStringSource : IWrenSource
    {
        private readonly string m_Source;
        private readonly Encoding m_Encoding;
        private readonly IAllocator m_Allocator;

        private byte* m_BufferPtr;
        private int m_BufferSize;
        private bool m_Disposed;

        #region Properties

        /// <summary>
        /// The string containing the Wren script.
        /// </summary>
        public string Source => m_Source;

        #endregion

        /// <summary>
        /// Creates a new <see cref="WrenStringSource"/>, wrapping a string.
        /// </summary>
        /// <param name="source">The string of Wren source.</param>
        /// <param name="encoding">The Encoding of the string. If this parameter is null, UTF8 encoding is used.</param>
        /// <param name="allocator">The memory allocator to use when an unmanaged buffer is required.
        /// If null, the default allocator (<see cref="HGlobalAllocator.Default"/>) is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="source"/> is null.</exception>
        public WrenStringSource(string source, Encoding encoding = null, IAllocator allocator = null)
        {
            m_Source = source ?? throw new ArgumentNullException(nameof(source));
            m_Encoding = encoding ?? Encoding.UTF8;
            m_Allocator = allocator ?? HGlobalAllocator.Default;
        }

        #region IModuleSource

        /// <summary>
        /// Get a pointer to the bytes containing the Wren script source that can be passed
        /// to the native Wren VM.
        /// </summary>
        /// <param name="byteCount">The number of bytes in the source.</param>
        /// <returns>A pointer to the source bytes.</returns>
        public IntPtr GetSourceBytes(out int byteCount)
        {
            if (m_BufferPtr == null)
            {
                WrenInternal.EncodeTextBufferFromString(
                    m_Source, m_Encoding, m_Allocator,
                    ref m_BufferPtr, ref m_BufferSize,
                    out _, out byteCount);
            }
            else
            {
                byteCount = m_BufferSize;
            }

            return (IntPtr)m_BufferPtr;
        }

        #endregion

        #region IDisposable

        ~WrenStringSource()
        {
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    // Dispose managed state (managed objects)
                }

                // Dispose unmanaged state
                if (m_BufferPtr == null)
                {
                    m_Allocator.Free((IntPtr)m_BufferPtr);
                    m_BufferPtr = null;
                }

                m_Disposed = true;
            }
        }

        #endregion
    }
}
