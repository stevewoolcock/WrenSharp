#if WRENSHARP_EXT
using System;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenForeign : IEquatable<WrenForeign>
    {
        private readonly WrenInternalObjForeign* m_Ptr;

        #region Properties

        /// <summary>
        /// Indicates if the value has a valid underlying pointer.
        /// </summary>
        public bool IsValid => m_Ptr != null;

        /// <summary>
        /// The <see cref="WrenObject"/> header for this object.
        /// </summary>
        public WrenObject Object => new WrenObject(&m_Ptr->Object);

        /// <summary>
        /// The size of the <see cref="Data"/> memory block that was allocated for the instance.
        /// </summary>
        public int Size => (int)m_Ptr->Size;

        /// <summary>
        /// Gets the values of the instance's fields. It is recommended to fetch the span once and cache
        /// it in a local variable before iterating.
        /// </summary>
        public byte* Data => &m_Ptr->Data;

        /// <summary>
        /// Returns a span wrapping the foreign instance's data.
        /// </summary>
        public Span<byte> Span => new Span<byte>(&m_Ptr->Data, (int)m_Ptr->Size);

        #endregion

        internal WrenForeign(WrenInternalObjForeign* instance)
        {
            m_Ptr = instance;
        }

        public Span<T> AsSpan<T>() where T : unmanaged => new Span<T>(&m_Ptr->Data, Size / sizeof(T));

        #region Object

        public bool Equals(WrenForeign other) => other.m_Ptr == m_Ptr;

        public override bool Equals(object obj) => obj is WrenForeign instance && Equals(instance);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => $"instance of {Object.Class.Name}";

        #endregion

        public static bool operator ==(WrenForeign left, WrenForeign right) => left.Equals(right);
        public static bool operator !=(WrenForeign left, WrenForeign right) => !(left == right);
    }
}
#endif