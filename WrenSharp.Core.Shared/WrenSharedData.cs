using System;
using System.Runtime.CompilerServices;

namespace WrenSharp
{
    /// <summary>
    /// A table that maps managed objects with addresses (<see cref="WrenSharedDataHandle"/>) that can be used to facilitate
    /// communication between Wren foregin objects/methods and managed C# data.
    /// </summary>
    public class WrenSharedDataTable
    {
        private struct Entry
        {
            public object Value;
            public WrenSharedDataHandle NextFree;
        }

        private Entry[] m_Entries = Array.Empty<Entry>();
        private WrenSharedDataHandle m_Free = (WrenSharedDataHandle)1;
        private WrenSharedDataHandle m_Tail = WrenSharedDataHandle.Invalid;
        private int m_Count;

        #region Properties

        /// <summary>
        /// The number of entries in the table.
        /// </summary>
        public int Count => m_Count;

        #endregion

        internal WrenSharedDataTable() { }

        #region Public API

        /// <summary>
        /// Adds a value to the table and returns a handle pointing to the value's address in the table.
        /// </summary>
        /// <param name="value">The value to add to the table.</param>
        /// <returns>A <see cref="WrenSharedDataHandle"/> pointing to <paramref name="value"/>.</returns>
        public WrenSharedDataHandle Add(object value)
        {
            WrenSharedDataHandle handle = m_Free;

            // Grow the storage table if at capacity
            // Each grow doubles the size, reducing the number of times the table
            // needs to be resized the more elements are added.
            if (handle >= m_Entries.Length)
            {
                int newCapacity = m_Entries.Length * 2;
                if (newCapacity < 8) newCapacity = 8;
                else if (newCapacity > int.MaxValue) newCapacity = int.MaxValue;

                Array.Resize(ref m_Entries, newCapacity);
            }

            // Keep track of the highest address handle assigned
            // This acts as a sentinel - no valid handle can exist past the this address
            if (handle > m_Tail)
            {
                m_Tail = handle;
            }

            int entryIndex = HandleToIndex(handle);

            // Update the free handle.
            // If reusing a previously freed handle, the entry contains the location of the next
            // free handle. If that is an invalid handle, the next free handle is at the tail.
            WrenSharedDataHandle nextFree = m_Entries[entryIndex].NextFree;
            m_Free = nextFree.IsValid ? nextFree : (WrenSharedDataHandle)(m_Tail + 1);

            // Now we have a handle, add the entry and return the handle
            m_Entries[entryIndex] = new Entry()
            {
                Value = value,
                NextFree = WrenSharedDataHandle.Invalid,
            };

            m_Count++;
            return handle;
        }

        /// <summary>
        /// Removes an entry from the table by its handle.
        /// </summary>
        /// <param name="handle">The handle of the object to remove.</param>
        /// <returns>True if the object was removed, otherwise false.</returns>
        public bool Remove(in WrenSharedDataHandle handle)
        {
            if (!Contains(handle))
                return false;

            RemoveAt(HandleToIndex(handle));
            return true;
        }

        /// <summary>
        /// Removes the first entry containing <paramref name="value"/> from the table.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>True if the value was found, otherwise false.</returns>
        public bool Remove(object value)
        {
            int indexToRemove = -1;
            for (int i = 0, len = HandleToIndex(m_Tail + 1); i < len; i++)
            {
                if (m_Entries[i].Value == value)
                {
                    indexToRemove = i;
                    break;
                }
            }

            if (indexToRemove <= -1)
                return false;

            RemoveAt(indexToRemove);
            return true;
        }

        /// <summary>
        /// Removes all entries from the table and resets its state.
        /// </summary>
        public void Clear()
        {
            // Clear the array so reference can be GC'd
            Array.Clear(m_Entries, 0, m_Tail);

            // Reset state
            m_Free = (WrenSharedDataHandle)1;
            m_Tail = WrenSharedDataHandle.Invalid;
            m_Count = 0;
        }

        /// <summary>
        /// Returns true if <paramref name="handle"/> points to an entry in the table.
        /// </summary>
        /// <param name="handle">The handle to lookup.</param>
        /// <returns>True if <paramref name="handle"/> points to an entry in the table, otherwise false.</returns>
        public bool Contains(in WrenSharedDataHandle handle)
        {
            if (!IsValidHandle(handle))
                return false;

            // Handle points to a valid location
            // The entry is valid if it's not a tombstone
            return !IsTombstone(in m_Entries[HandleToIndex(handle)]);
        }

        /// <summary>
        /// Returns true if <paramref name="value"/> exists in the table.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <returns>True if <paramref name="value"/> exists in the table, otherwise false.</returns>
        public bool Contains(object value)
        {
            for (int i = 0, len = HandleToIndex(m_Tail + 1); i < len; i++)
            {
                var entry = m_Entries[i];
                if (entry.Value == value)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the value <paramref name="handle"/> points to.
        /// </summary>
        /// <param name="handle"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="handle"/> is invalid or does not point to an existing table entry.</exception>
        public object Get(in WrenSharedDataHandle handle) => Get<object>(handle);

        /// <summary>
        /// Gets the value <paramref name="handle"/> points to, cast as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="handle"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="handle"/> is invalid or does not point to an existing table entry.</exception>
        /// <exception cref="InvalidCastException">Thrown if the value in the table is not assignable from <typeparamref name="T"/>.</exception>
        public T Get<T>(in WrenSharedDataHandle handle)
        {
            EnsureValidHandle(handle);

            ref var entry = ref m_Entries[HandleToIndex(handle)];
            var value = IsTombstone(in entry)
                ? throw new InvalidOperationException("Invalid handle.")
                : entry.Value;

            if (value == null)
                return default;

            return (T)value;
        }

        /// <summary>
        /// Finds <paramref name="value"/> in the table and returns a <see cref="WrenSharedDataHandle"/> pointing to the entry.
        /// If the value is not found in the table, <see cref="WrenSharedDataHandle.Invalid"/> is returned. Note that is generally
        /// more efficient to cache the handles returned via <see cref="Add(object)"/> than to fetch handles on demand.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <returns>A <see cref="WrenSharedDataHandle"/> pointing to <paramref name="value"/>, or
        /// <see cref="WrenSharedDataHandle.Invalid"/> if it does not exist in the table.</returns>
        public WrenSharedDataHandle GetHandle(object value)
        {
            for (int i = 0, len = HandleToIndex(m_Tail + 1); i < len; i++)
            {
                var entry = m_Entries[i];
                if (entry.Value == value)
                {
                    return IndexToHandle(i);
                }
            }

            return WrenSharedDataHandle.Invalid;
        }

        /// <summary>
        /// Gets the type of the value <paramref name="handle"/> points to. If <paramref name="handle"/> is invalid
        /// or does not point to a value in the table, this method returns <c>null</c>.
        /// </summary>
        /// <param name="handle">The handle of the value.</param>
        /// <returns>The <see cref="Type"/> of the value <paramref name="handle"/> points to, or null if <paramref name="handle"/>
        /// is not valid or does not point to an existing table entry.</returns>
        public Type GetValueType(in WrenSharedDataHandle handle)
        {
            return TryGet(handle, out object value) ? value.GetType() : null;
        }

        /// <summary>
        /// Sets the value <paramref name="handle"/> points to to <paramref name="value"/>. If <paramref name="handle"/> is invalid
        /// or does not point to an existing value in the table, <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        /// <param name="handle">The entry handle.</param>
        /// <param name="value">The value to set.</param>
        /// <exception cref="InvalidOperationException">Thrown if <paramref name="handle"/> is invalid or does not point to an existing table entry.</exception>
        public void Set(in WrenSharedDataHandle handle, object value)
        {
            EnsureValidHandle(handle);

            ref var entry = ref m_Entries[HandleToIndex(handle)];
            if (IsTombstone(in entry))
                throw new InvalidOperationException("Invalid handle.");

            entry.Value = value;
            entry.NextFree = WrenSharedDataHandle.Invalid;
        }

        /// <summary>
        /// Attempts to get the value <paramref name="handle"/> points to. Returns false if <paramref name="handle"/>
        /// is invalid or does point to existing table entry.
        /// </summary>
        /// <param name="handle">The entry handle.</param>
        /// <param name="value">The value <paramref name="handle"/> points to.</param>
        /// <returns>True if <paramref name="handle"/> points to a valid entry, otherwise false.</returns>
        public bool TryGet(in WrenSharedDataHandle handle, out object value) => TryGet<object>(handle, out value);

        /// <summary>
        /// Attempts to get the value <paramref name="handle"/> points to, cast as <typeparamref name="T"/>. Returns false if <paramref name="handle"/>
        /// is invalid, does point to existing table entry or the value is not assignable from <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of <paramref name="value"/>.</typeparam>
        /// <param name="handle">The entry handle.</param>
        /// <param name="value">The value <paramref name="handle"/> points to.</param>
        /// <returns>True if <paramref name="handle"/> points to a valid entry of type <typeparamref name="T"/>, otherwise false.</returns>
        public bool TryGet<T>(in WrenSharedDataHandle handle, out T value)
        {
            if (!handle.IsValid || handle <= 0 || handle >= m_Tail)
            {
                value = default;
                return false;
            }

            Entry entry = m_Entries[HandleToIndex(handle)];
            if (IsTombstone(in entry))
            {
                value = default;
                return false;
            }

            if (entry.Value is T typedValue)
            {
                value = typedValue;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Finds <paramref name="value"/> in the table and sets <paramref name="handle"/> to a <see cref="WrenSharedDataHandle"/>
        /// pointing to the entry. If the value is not found in the table, <paramref name="handle"/> is set to <see cref="WrenSharedDataHandle.Invalid"/>.
        /// Note that is generally more efficient to cache the handles returned via <see cref="Add(object)"/> than to fetch handles on demand.
        /// </summary>
        /// <param name="value">The value to find.</param>
        /// <param name="handle">A <see cref="WrenSharedDataHandle"/> pointing to the entry.</param>
        /// <returns>True if <paramref name="value"/> was found in the table, otherwise false.</returns>
        public bool TryGetHandle(object value, out WrenSharedDataHandle handle)
        {
            handle = GetHandle(value);
            return handle != WrenSharedDataHandle.Invalid;
        }

        /// <summary>
        /// Attempts to set the value <paramref name="handle"/> points to. <paramref name="handle"/> must be valid and point to an existing entry in the table.
        /// </summary>
        /// <param name="handle">The entry handle.</param>
        /// <param name="value">The value to set.</param>
        /// <returns>True if <paramref name="handle"/> points to a valid entry, otherwise false.</returns>
        public bool TrySet(in WrenSharedDataHandle handle, object value)
        {
            if (!handle.IsValid || handle <= 0 || handle >= m_Tail)
                return false;

            ref var entry = ref m_Entries[HandleToIndex(handle)];
            if (IsTombstone(in entry))
                return false;

            entry.Value = value;
            entry.NextFree = WrenSharedDataHandle.Invalid;
            return true;
        }

        #endregion

        private void RemoveAt(int index)
        {
            // Entry is tombstoned and points to the current free handle
            m_Entries[index] = new Entry()
            {
                Value = null!,
                NextFree = m_Free
            };

            // Free handle is now this handle
            var handle = IndexToHandle(index);
            m_Free = handle;
            m_Count--;

            if (m_Tail == handle)
            {
                m_Tail--;
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static int HandleToIndex(WrenSharedDataHandle handle)
        {
            return handle - 1;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static WrenSharedDataHandle IndexToHandle(int index)
        {
            return (WrenSharedDataHandle)(index + 1);
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsTombstone(in Entry entry)
        {
            return entry.NextFree != WrenSharedDataHandle.Invalid;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsValidHandle(WrenSharedDataHandle handle)
        {
            return handle.IsValid && handle > 0 && handle <= m_Tail;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureValidHandle(WrenSharedDataHandle handle)
        {
            if (!IsValidHandle(handle))
                throw new InvalidOperationException("Invalid handle.");
        }
    }
}
