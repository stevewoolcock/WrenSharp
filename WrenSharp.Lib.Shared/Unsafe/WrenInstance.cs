#if WRENSHARP_EXT
using System;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenInstance : IEquatable<WrenInstance>
    {
        private readonly WrenInternalObjInstance* m_Ptr;

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
        /// Gets the <see cref="WrenClass"/> of the instance.
        /// </summary>
        public WrenClass Class => new WrenClass(m_Ptr->Object.ClassObj);

        /// <summary>
        /// The number of fields.
        /// </summary>
        public int FieldCount => m_Ptr->Object.ClassObj->NumFields;

        /// <summary>
        /// Gets the values of the instance's fields. It is recommended to fetch the span once and cache
        /// it in a local variable before iterating.
        /// </summary>
        public Span<WrenValue> Fields => new Span<WrenValue>(&m_Ptr->Fields, m_Ptr->Object.ClassObj->NumFields);

        #endregion

        internal WrenInstance(WrenInternalObjInstance* instance)
        {
            m_Ptr = instance;
        }

        #region Object

        public bool Equals(WrenInstance other) => other.m_Ptr == m_Ptr;

        public override bool Equals(object obj) => obj is WrenInstance instance && Equals(instance);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => $"instance of {Object.Class.Name}";

        #endregion

        public static bool operator ==(WrenInstance left, WrenInstance right) => left.Equals(right);
        public static bool operator !=(WrenInstance left, WrenInstance right) => !(left == right);
    }
}
#endif