#if WRENSHARP_EXT
using System;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenClass : IEquatable<WrenClass>
    {
        private readonly WrenInternalObjClass* m_Ptr;

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
        /// The number of fields.
        /// </summary>
        public int FieldCount => m_Ptr->NumFields;

        /// <summary>
        /// The superclass of this class.
        /// </summary>
        public WrenClass Superclass => new WrenClass(m_Ptr->Superclass);

        /// <summary>
        /// The name of the class.
        /// </summary>
        public WrenString Name => new WrenString(m_Ptr->Name);

        #endregion

        internal WrenClass(WrenInternalObjClass* cls)
        {
            m_Ptr = cls;
        }

        #region Object

        public bool Equals(WrenClass other) => other.m_Ptr == m_Ptr;

        public override bool Equals(object obj) => obj is WrenClass cls && Equals(cls);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => Name.ToString();

        #endregion

        #region Operators

        public static bool operator ==(WrenClass left, WrenClass right) => left.Equals(right);
        public static bool operator !=(WrenClass left, WrenClass right) => !(left == right);

        #endregion
    }
}
#endif