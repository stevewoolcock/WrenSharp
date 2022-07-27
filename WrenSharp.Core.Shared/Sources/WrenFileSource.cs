using System;
using System.IO;
using WrenSharp.Memory;

namespace WrenSharp
{
    /// <summary>
    /// A Wren script source that reads the contents of file from the filesystem.
    /// </summary>
    public unsafe class WrenFileSource : IWrenSource
    {
        private readonly string m_Path;
        private readonly IAllocator m_Allocator;

        private byte* m_Buffer;
        private int m_BufferSize;
        private bool m_Disposed;

        /// <summary>
        /// Creates a new <see cref="WrenFileSource"/> instance.
        /// </summary>
        /// <param name="path">The path to the source script to load.</param>
        /// <param name="allocator"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public WrenFileSource(string path, IAllocator allocator = default)
        {
            if (string.IsNullOrEmpty(path))
                throw new ArgumentNullException(nameof(path), "Path must be non-empty string.");

            m_Path = path;
            m_Allocator = allocator ?? HGlobalAllocator.Default;
        }

        /// <summary>
        /// Returns a pointer to an unmanaged buffer containing the contents of the source file.
        /// </summary>
        /// <param name="byteCount">The number of bytes in the source.</param>
        /// <returns>A pointer to the source bytes.</returns>
        public IntPtr GetSourceBytes(out int byteCount)
        {
            if (m_Buffer == null)
            {
                using var fileStream = File.OpenRead(m_Path);

                if (fileStream.Length > int.MaxValue)
                    throw new InvalidOperationException($"File {m_Path} is too large ({fileStream.Length} bytes)");

                m_BufferSize = (int)fileStream.Length;
                m_Buffer = (byte*)m_Allocator.Allocate(m_BufferSize);

                using var memStream = new UnmanagedMemoryStream(m_Buffer, m_BufferSize, m_BufferSize, FileAccess.Write);
                fileStream.CopyTo(memStream);
            }

            byteCount = m_BufferSize;
            return (IntPtr)m_Buffer;
        }

        /// <summary>
        /// Releases the in-memory buffer containing the contents of the file, if the file has previously
        /// been loaded. Otherwise, calling this method does nothing.
        /// </summary>
        public void ReleaseBuffer()
        {
            if (m_Buffer != null)
            {
                m_Allocator.Free((IntPtr)m_Buffer);
                m_Buffer = null;
                m_BufferSize = 0;
            }
        }

        #region IDisposable

        ~WrenFileSource()
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
                ReleaseBuffer();

                m_Disposed = true;
            }
        }

        #endregion
    }
}
