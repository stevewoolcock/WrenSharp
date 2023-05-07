#if WRENSHARP_EXT
using System;
using System.Collections.Generic;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenNativeList : IEquatable<WrenNativeList>, IReadOnlyList<WrenValue>
    {
        private readonly WrenInternalObjList* m_Ptr;

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
        /// Gets the count of elements in the list
        /// </summary>
        public int Count => m_Ptr->Elements.Count;

        /// <summary>
        /// Gets the capacity of the list.
        /// </summary>
        public int Capacity => m_Ptr->Elements.Capacity;

        /// <summary>
        /// Gets a span of the elements in the list.
        /// </summary>
        public Span<WrenValue> Elements => m_Ptr->Elements.AsSpan();

        /// <summary>
        /// Gets or sets the <see cref="WrenValue"/> at <paramref name="index"/>.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public WrenValue this[int index]
        {
            get
            {
                if (index < 0 || index >= m_Ptr->Elements.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return m_Ptr->Elements.Data[index];
            }

            set
            {
                if (index < 0 || index >= m_Ptr->Elements.Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                m_Ptr->Elements.Data[index] = value;
            }
        }

        #endregion

        internal WrenNativeList(WrenInternalObjList* instance)
        {
            m_Ptr = instance;
        }

        public bool Contains(WrenValue value) => IndexOf(value) >= 0;

        public int IndexOf(WrenValue value)
        {
            for (int i = 0, len = m_Ptr->Elements.Count; i < len; i++) 
            {
                if (m_Ptr->Elements.Data[i] == value)
                    return i;
            }

            return -1;
        }

        #region Object

        public bool Equals(WrenNativeList other) => other.m_Ptr == m_Ptr;

        public override bool Equals(object obj) => obj is WrenNativeList instance && Equals(instance);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => $"instance of {Object.Class.Name}";

        public Enumerator GetEnumerator() => new Enumerator(m_Ptr);

        #endregion

        #region IEnumerable

        IEnumerator<WrenValue> IEnumerable<WrenValue>.GetEnumerator() => new Enumerator(m_Ptr);

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => new Enumerator(m_Ptr);

        #endregion

        public static bool operator ==(WrenNativeList left, WrenNativeList right) => left.Equals(right);
        public static bool operator !=(WrenNativeList left, WrenNativeList right) => !(left == right);

        [Serializable]
        public struct Enumerator : IEnumerator<WrenValue>, System.Collections.IEnumerator
        {
            private readonly WrenInternalObjList* m_List;
            private WrenValue* m_Current;
            private int m_Index;

            internal Enumerator(WrenInternalObjList* buffer)
            {
                m_List = buffer;
                m_Index = 0;
                m_Current = default;
            }

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                if ((uint)m_Index < (uint)m_List->Elements.Count)
                {
                    m_Current = &m_List->Elements.Data[m_Index];
                    m_Index++;
                    return true;
                }

                return MoveNextRare();
            }

            private bool MoveNextRare()
            {
                m_Index = m_List->Elements.Count + 1;
                m_Current = default;
                return false;
            }

            public WrenValue Current => *m_Current;

            object System.Collections.IEnumerator.Current => Current;

            void System.Collections.IEnumerator.Reset()
            {
                m_Index = 0;
                m_Current = default;
            }
        }
    }
}
#endif