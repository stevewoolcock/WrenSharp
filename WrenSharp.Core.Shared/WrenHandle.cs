using System;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A handle wrapping a Wren value. When a handle is allocated, Wren guarantees the object
    /// will not be removed by the garbage collector. Handles can be passed into various WrenSharp
    /// API methods to make calls, store values, etc.
    /// <para/>
    /// Handles must be released to free the memory allocated for them. <see cref="WrenHandle"/>
    /// implements <see cref="IDisposable"/>, and can be disposed by calling <see cref="Dispose"/>
    /// or <see cref="WrenVM.ReleaseHandle(in WrenHandle)"/>. <see cref="WrenVM"/> instances
    /// will automatically release all handles allocated when disposed.
    /// </summary>
    public readonly struct WrenHandle : IDisposable
    {
        internal readonly WrenHandleInternal m_Handle;
        internal readonly IntPtr m_Ptr;

        #region Properties

        /// <summary>
        /// Indicates if the handle is valid. A handle is valid if it has been created and not released.
        /// Once a handle is released, all <see cref="WrenHandle"/> values pointing to it will become invalid.
        /// </summary>
        public bool IsValid => m_Handle != null && m_Ptr == m_Handle.Ptr;

        #endregion

        internal WrenHandle(WrenHandleInternal handle)
        {
            m_Handle = handle;
            m_Ptr = m_Handle.Ptr;
        }

        /// <summary>
        /// Releases the handle. Once released, the handle can no longer be used.
        /// </summary>
        public void Dispose() => m_Handle.VM.ReleaseHandle(in this);
    }
}
