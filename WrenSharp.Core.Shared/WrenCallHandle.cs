using System;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A handle representing a callable Wren method. Use <see cref="WrenCallHandle"/> to call
    /// Wren functions from managed code.
    /// <para/>
    /// Handles must be released to free the memory allocated for them. <see cref="WrenCallHandle"/>
    /// implements <see cref="IDisposable"/>, and can be disposed by calling <see cref="Dispose"/>
    /// or <see cref="WrenVM.ReleaseHandle(in WrenCallHandle)"/>. <see cref="WrenVM"/> instances
    /// will automatically release all handles allocated when disposed.
    /// </summary>
    /// <seealso cref="WrenVM.Call(WrenCallHandle, bool)"/>
    /// <seealso cref="WrenVM.CreateCall(WrenHandle, WrenCallHandle, bool)"/>
    public readonly struct WrenCallHandle : IDisposable, IEquatable<WrenCallHandle>
    {
        internal readonly WrenHandleInternal m_Handle;
        internal readonly IntPtr m_Ptr;
        internal readonly int m_Version;
        internal readonly byte m_ParamCount;

        #region Properties

        /// <summary>
        /// Indicates if the handle is valid. A handle is valid if it has been created and not released.
        /// Once a handle is released, all <see cref="WrenCallHandle"/> values pointing to it will become invalid.
        /// </summary>
        public bool IsValid => m_Handle != null && m_Handle.IsValid() && m_Version == m_Handle.Version && m_Ptr == m_Handle.Ptr;

        /// <summary>
        /// The number of parameters the method call requires.
        /// </summary>
        public int ParamCount => m_ParamCount;

        #endregion

        internal WrenCallHandle(WrenHandleInternal handle, byte paramCount)
        {
            m_Handle = handle;
            m_Ptr = handle.Ptr;
            m_Version = handle.Version;
            m_ParamCount = paramCount;
        }

        /// <summary>
        /// Releases the handle. Once released, the handle can no longer be used.
        /// </summary>
        public void Dispose() => m_Handle.VM?.ReleaseHandle(in this);

        #region Object

        /// <inheritdoc/>
        public bool Equals(WrenCallHandle other)
        {
            return
                other.m_Ptr == m_Ptr &&
                other.m_Version == m_Version &&
                other.m_ParamCount == m_ParamCount &&
                other.m_Handle == m_Handle;
        }

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is WrenCallHandle callHandle && Equals(callHandle);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(m_Ptr, m_Version, m_ParamCount, m_Handle);

        /// <inheritdoc/>
        public override string ToString() => IsValid ? m_Ptr.ToString() : string.Empty;

        #endregion

        #region Operator Overloads

        /// <inheritdoc/>
        public static bool operator ==(WrenCallHandle left, WrenCallHandle right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(WrenCallHandle left, WrenCallHandle right) => !(left == right);

        #endregion
    }
}
