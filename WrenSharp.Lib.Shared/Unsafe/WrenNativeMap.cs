#if WRENSHARP_EXT
using System;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenNativeMap : IEquatable<WrenNativeMap>
    {
        private readonly WrenInternalObjMap* m_Ptr;

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
        /// Gets the count of entries in the map.
        /// </summary>
        public int Count => (int)m_Ptr->Count;

        /// <summary>
        /// Gets the capacity of the map.
        /// </summary>
        public int Capacity => (int)m_Ptr->Capacity;

        #endregion

        internal WrenNativeMap(WrenInternalObjMap* instance)
        {
            m_Ptr = instance;
        }

        #region Object

        public bool Equals(WrenNativeMap other) => other.m_Ptr == m_Ptr;

        public override bool Equals(object obj) => obj is WrenNativeMap instance && Equals(instance);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => $"instance of Map";

        #endregion

        public static bool operator ==(WrenNativeMap left, WrenNativeMap right) => left.Equals(right);
        public static bool operator !=(WrenNativeMap left, WrenNativeMap right) => !(left == right);
    }
}
#endif