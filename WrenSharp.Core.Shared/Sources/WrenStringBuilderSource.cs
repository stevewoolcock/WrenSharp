using System;
using System.Text;
using WrenSharp.Native;
using WrenSharp.Memory;

namespace WrenSharp
{
    /// <summary>
    /// A Wren script source that wraps a <see cref="System.Text.StringBuilder"/>. The contents of the source is copied
    /// to an unmanaged buffer, which is passed directly to the native Wren VM when the source is interpreted.
    /// <para/>
    /// This method is signficantly faster than passing a <see cref="System.Text.StringBuilder"/> directly to the native
    /// WrenVM via P/Invoke.
    /// </summary>
    public unsafe class WrenStringBuilderSource : IWrenSource
    {
        private readonly StringBuilder m_StringBuilder;
        private readonly Encoding m_Encoding;
        private readonly IAllocator m_Allocator;

        private byte* m_BufferPtr;
        private int m_BufferSize;
        private int m_ByteCount;
        private int m_CharCount;
        private bool m_SourceDirty;
        private bool m_Disposed;

        #region Properties

        /// <summary>
        /// The <see cref="System.Text.StringBuilder"/> containing the Wren script.
        /// </summary>
        public StringBuilder StringBuilder => m_StringBuilder;

        #endregion

        /// <summary>
        /// Creates a new <see cref="WrenStringSource"/>, wrapping a <see cref="System.Text.StringBuilder"/>.
        /// </summary>
        /// <param name="stringBuilder">The <see cref="System.Text.StringBuilder"/> containing the Wren source.</param>
        /// <param name="encoding">The Encoding of the string. If this parameter is null, UTF8 encoding is used.</param>
        /// <param name="allocator">The memory allocator to use when an unmanaged buffer is required.
        /// If null, the default allocator (<see cref="HGlobalAllocator.Default"/>) is used.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="stringBuilder"/> is null.</exception>
        public WrenStringBuilderSource(StringBuilder stringBuilder, Encoding encoding = null, IAllocator allocator = null)
        {
            m_StringBuilder = stringBuilder ?? throw new ArgumentNullException(nameof(stringBuilder));
            m_Encoding = encoding ?? Encoding.UTF8;
            m_Allocator = allocator ?? HGlobalAllocator.Default;
        }

        /// <summary>
        /// Sets an internal flag to indicate the contents of the source <see cref="StringBuilder"/> has changed.
        /// The next time <see cref="GetSourceBytes(out int)"/> is called, the contents will be re-written to
        /// the internal buffer before it is returned.
        /// </summary>
        public void MarkSourceChanged()
        {
            m_SourceDirty = true;
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
            if (m_SourceDirty || m_StringBuilder.Length != m_CharCount)
            {
                WrenInternal.EncodeTextBufferFromStringBuilder(
                    m_StringBuilder, m_Encoding, m_Allocator,
                    ref m_BufferPtr, ref m_BufferSize,
                    out m_CharCount, out m_ByteCount);

                m_SourceDirty = false;
            }

            byteCount = m_ByteCount;
            return (IntPtr)m_BufferPtr;
        }

        #endregion

        #region IDisposable

        ~WrenStringBuilderSource()
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
                    // Dispose managed state
                }

                // Dispose unmanaged state
                if (m_BufferPtr != null)
                {
                    m_Allocator.Free((IntPtr)m_BufferPtr);
                    m_BufferPtr = default;
                }

                m_Disposed = true;
            }
        }

        #endregion
    }
}
