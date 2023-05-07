#if WRENSHARP_EXT
using System;

namespace WrenSharp.Unsafe
{
    /// <summary>
    /// A managed wrapper for a Wren string object.
    /// </summary>
    public readonly unsafe struct WrenString : IEquatable<WrenString>
    {
        /// <summary>
        /// Converts a UTF8 Wren string to a managed string.
        /// </summary>
        /// <param name="str">The <see cref="WrenInternalObjString"/> to convert.</param>
        /// <returns>A managed string containing the contents of <paramref name="str"/>.</returns>
        internal static string ToManagedString(WrenInternalObjString* str) => System.Text.Encoding.UTF8.GetString(&str->ValueArray, str->Length);

        private readonly WrenInternalObjString* m_Ptr;

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
        /// The number of code points in the string. Wren strings are UTF8 encoded.
        /// </summary>
        public int Length => m_Ptr->Length;
        
        /// <summary>
        /// The hash of the string.
        /// </summary>
        public int Hash => m_Ptr->Hash;

        /// <summary>
        /// The pointer to the contents of the string.
        /// </summary>
        public byte* ValuePtr => &m_Ptr->ValueArray;

        /// <summary>
        /// A span containing the the code points of the string.
        /// </summary>
        public ReadOnlySpan<byte> ValueSpan => new ReadOnlySpan<byte>(&m_Ptr->ValueArray, m_Ptr->Length);

        #endregion

        internal WrenString(WrenInternalObjString* stringObj)
        {
            m_Ptr = stringObj;
        }

        public bool Equals(WrenString other)
        {
            if (other.m_Ptr == m_Ptr)
                return true;

            return other.m_Ptr->Hash == m_Ptr->Hash &&
                   other.m_Ptr->Length == m_Ptr->Length &&
                   UnsafeUtils.Memeq(&other.m_Ptr->ValueArray, &m_Ptr->ValueArray, m_Ptr->Length);
        }

        public override bool Equals(object obj) => obj is WrenString other && Equals(other);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => ToManagedString(m_Ptr);
    }
}
#endif