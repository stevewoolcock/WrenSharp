﻿using System;
using System.Runtime.CompilerServices;
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
    public readonly struct WrenHandle : IDisposable, IEquatable<WrenHandle>
    {
        internal readonly WrenHandleInternal m_Handle;
        internal readonly IntPtr m_Ptr;
        internal readonly int m_Version;

        #region Properties

        /// <summary>
        /// Indicates if the handle is valid. A handle is valid if it has been created and not released.
        /// Once a handle is released, all <see cref="WrenHandle"/> values pointing to it will become invalid.
        /// </summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Handle?.IsValid() == true && m_Version == m_Handle.Version && m_Ptr == m_Handle.Ptr;
        }

        /// <summary>
        /// The handle's native pointer.
        /// </summary>
        public IntPtr NativePointer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsValid ? m_Ptr : IntPtr.Zero;
        }

        #endregion

        internal WrenHandle(WrenHandleInternal handle)
        {
            m_Handle = handle;
            m_Ptr = m_Handle.Ptr;
            m_Version = m_Handle.Version;
        }

        /// <summary>
        /// Releases the handle. Once released, the handle can no longer be used.
        /// </summary>
        public void Dispose() => m_Handle?.VM.ReleaseHandle(in this);

        #region Object

        /// <inheritdoc/>
        public bool Equals(WrenHandle other) => other.m_Ptr == m_Ptr && other.m_Version == m_Version && other.m_Handle == m_Handle;

        /// <inheritdoc/>
        public override bool Equals(object other) => other is WrenHandle && Equals(other);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(m_Ptr, m_Version, m_Handle);

        /// <inheritdoc/>
        public override string ToString()
        {
            if (!IsValid)
                return string.Empty;

            return IntPtr.Size == 4 ? $"0x{m_Ptr:X8}" : $"0x{m_Ptr:X16}";
        }

        #endregion

        #region Operator Overloads

        /// <inheritdoc/>
        public static bool operator ==(WrenHandle left, WrenHandle right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(WrenHandle left, WrenHandle right) => !(left == right);

        #endregion
    }
}
