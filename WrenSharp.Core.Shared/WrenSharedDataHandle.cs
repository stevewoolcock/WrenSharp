using System;
using System.Runtime.InteropServices;

namespace WrenSharp
{
    /// <summary>
    /// A handle that points to an entry in a <see cref="WrenSharedDataTable"/>. This is a blittable value that is compatible
    /// with Wren foreign objects. It is useful for linking a Wren foreign instance to a managed object instance for cross
    /// comunication between native Wren and C#.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct WrenSharedDataHandle : IEquatable<WrenSharedDataHandle>
    {
        #region Static

        /// <summary>
        /// An <see cref="WrenSharedDataHandle"/> representing an invalid address.
        /// </summary>
        public static WrenSharedDataHandle Invalid => new WrenSharedDataHandle(0);

        #endregion

        private readonly int m_Value;

        #region Properties

        /// <summary>
        /// Indicates if the handle represents a valid address.
        /// </summary>
        public bool IsValid => m_Value > 0;

        #endregion

        internal WrenSharedDataHandle(int value)
        {
            m_Value = value;
        }

        #region Object

        public bool Equals(WrenSharedDataHandle other) => other.m_Value == m_Value;

        public override bool Equals(object obj) => (obj is WrenSharedDataHandle handle) && Equals(handle);

        public override int GetHashCode() => m_Value;

        public override string ToString() => $"0x{m_Value:X8}";

        #endregion

        public static bool operator ==(WrenSharedDataHandle left, WrenSharedDataHandle right) => left.Equals(right);
        public static bool operator !=(WrenSharedDataHandle left, WrenSharedDataHandle right) => !(left == right);

        public static implicit operator int(WrenSharedDataHandle handle) => handle.m_Value;
        public static implicit operator WrenSharedDataHandle(int handle) => new WrenSharedDataHandle(handle);
    }
}