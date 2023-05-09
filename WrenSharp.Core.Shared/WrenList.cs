using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;
using WrenSharp.Unsafe;

namespace WrenSharp
{
    public partial struct WrenList
    {
        private readonly WrenVM m_Vm;
        private readonly int m_ListSlot;
        private readonly int m_DefaultElementSlot;

        #region Properties

        /// <summary>
        /// Gets the count of elements in the list.
        /// </summary>
        public int Count => Wren.GetListCount(m_Vm.m_Ptr, m_ListSlot);

        /// <summary>
        /// The slot the list resides in.
        /// </summary>
        public int ListSlot => m_ListSlot;

        /// <summary>
        /// The default slot to use for storing element values if an override is not supplied.
        /// </summary>
        public int DefaultElementSlot => m_DefaultElementSlot;

        /// <summary>
        /// The <see cref="WrenVM"/> that the list belongs to.
        /// </summary>
        public WrenVM VM => m_Vm;

        /// <summary>
        /// Gets or sets the value at <paramref name="index"/>. The value is placed in <see cref="DefaultElementSlot"/>.
        /// </summary>
        /// <param name="index">The index</param>
        /// <returns>A <see cref="WrenSlot"/> pointing to <see cref="DefaultElementSlot"/>.</returns>
        public WrenSlot this[int index]
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Get(index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(index, value);
        }

        #endregion

        public WrenList(WrenVM vm, int listSlot, int? defaultElementSlot = default)
        {
            var slotType = Wren.GetSlotType(vm.m_Ptr, listSlot);
            if (slotType != WrenType.List)
                throw new WrenTypeException(vm, $"Slot does not contain a List ({slotType})");

            m_Vm = vm;
            m_ListSlot = listSlot;
            m_DefaultElementSlot = defaultElementSlot.GetValueOrDefault(m_ListSlot + 1);
        }

        /// <summary>
        /// Clears all values in the list
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => m_Vm.ListClear(m_ListSlot);

        /// <summary>
        /// Creates a <see cref="WrenHandle"/> wrapping the value in <paramref name="index"/>.
        /// </summary>
        /// <param name="index">The index of the value.</param>
        /// <returns>A <see cref="WrenHandle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle CreateHandle(int index) => Get(index).CreateHandle();

        #region Get

        public WrenSlot Get(int index, int? elementSlot = default)
        {
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, elementSlot.GetValueOrDefault(m_DefaultElementSlot));
            return new WrenSlot(m_Vm, elementSlot.GetValueOrDefault(m_DefaultElementSlot));
        }

        public ReadOnlySpan<byte> GetStringBytes(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotStringBytes(slot);
        }

        public unsafe IntPtr GetForeign(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return Wren.GetSlotForeign(m_Vm.m_Ptr, slot);
        }

        public unsafe ref T GetForeign<T>(int index, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, slot);
        }

        public unsafe T* GetForeignPtr<T>(int index, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, slot);
        }

        public unsafe T GetSharedData<T>(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, slot);
            return m_Vm.SharedData.Get<T>(handle);
        }

        public unsafe WrenSharedDataHandle GetSharedDataHandle(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, slot);
        }

        #endregion

        #region Add

        /// <summary>
        /// Appends the value in slot[<paramref name="elementSlot"/>] to the end of the list.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="elementSlot">The slot containing the value to append. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void Add(int? elementSlot = default)
        {
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, elementSlot.GetValueOrDefault(DefaultElementSlot));
        }

        /// <summary>
        /// Creates a new list in <paramref name="elementSlot"/> and appends it to the end of list.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="elementSlot">The slot to create the new list in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void AddNewList(int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewList(m_Vm.m_Ptr, slot);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        /// <summary>
        /// Creates a new map in <paramref name="elementSlot"/> and appends it to the end of list.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="elementSlot">The slot to create the new map in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void AddNewMap(int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewMap(m_Vm.m_Ptr, slot);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        public void AddSharedData(object value, out WrenSharedDataHandle handle, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotSharedData(slot, value, out handle);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        public void AddNewSharedData(int classSlot, WrenSharedDataHandle handle, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotNewSharedData(slot, classSlot, handle);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        public void AddNewSharedData(int classSlot, object value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotNewSharedData(slot, classSlot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        public void AddNewForeign(int classSlot, ulong size, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, slot, classSlot, size);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        public ref T AddNewForeign<T>(int classSlot, in T value = default, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(slot, classSlot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
            return ref returnVal;
        }

        public unsafe T* AddNewForeignPtr<T>(int classSlot, in T value = default, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(slot, classSlot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
            return returnVal;
        }

        public unsafe T* AddNewForeignPtr<T>(int classSlot, in ReadOnlySpan<T> span, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(slot, classSlot, in span);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
            return returnVal;
        }

        #endregion

        #region Insert

        /// <summary>
        /// Inserts the value stored in slot[<paramref name="elementSlot"/>] into the list at <paramref name="index"/>.
        /// If <paramref name="elementSlot"/> is not supplied, the default value, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index to insert the value at.</param>
        /// <param name="elementSlot">The slot containing the value to insert. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void Insert(int index, int? elementSlot = default)
        {
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, elementSlot.GetValueOrDefault(DefaultElementSlot));
        }

        /// <summary>
        /// Creates a new list in <paramref name="elementSlot"/> and inserts it into the list at <paramref name="index"/>.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index to insert the list at.</param>
        /// <param name="elementSlot">The slot to create the new list in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void InsertNewList(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewList(m_Vm.m_Ptr, slot);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        /// <summary>
        /// Creates a new map in <paramref name="elementSlot"/> and inserts it into the list at <paramref name="index"/>.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index to insert the map at.</param>
        /// <param name="elementSlot">The slot to create the new map in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void InsertNewMap(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewMap(m_Vm.m_Ptr, slot);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void InsertSharedData(int index, object value, out WrenSharedDataHandle handle, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotSharedData(slot, value, out handle);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void InsertNewSharedData(int index, int classSlot, WrenSharedDataHandle handle, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotNewSharedData(slot, classSlot, handle);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void InsertNewSharedData(int index, int classSlot, object value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotNewSharedData(slot, classSlot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void InsertNewForeign(int index, int classSlot, ulong size, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, slot, classSlot, size);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public ref T InsertNewForeign<T>(int index, int classSlot, in T value = default, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(slot, classSlot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return ref returnVal;
        }

        public unsafe T* InsertNewForeignPtr<T>(int index, int classSlot, in T value = default, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(slot, classSlot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return returnVal;
        }

        public unsafe T* InsertNewForeignPtr<T>(int index, int classSlot, in ReadOnlySpan<T> span, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(slot, classSlot, in span);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return returnVal;
        }

        #endregion

        #region Set

        /// <summary>
        /// Creates a new list in <paramref name="elementSlot"/> and sets it as the value at <paramref name="index"/>.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index of to set the value at.</param>
        /// <param name="elementSlot">The slot to create the new list in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void Set(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewList(m_Vm.m_Ptr, slot);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        /// <summary>
        /// Creates a new list in <paramref name="elementSlot"/> and sets it as the value at <paramref name="index"/>.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index of to set the value at.</param>
        /// <param name="elementSlot">The slot to create the new list in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void SetNewList(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewList(m_Vm.m_Ptr, slot);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        /// <summary>
        /// Creates a new map in <paramref name="elementSlot"/> and sets it as the value at <paramref name="index"/>.
        /// If <paramref name="elementSlot"/> is not supplied, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index of to set the value at.</param>
        /// <param name="elementSlot">The slot to create the new map in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public void SetNewMap(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(DefaultElementSlot);
            Wren.SetSlotNewMap(m_Vm.m_Ptr, slot);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void SetSharedData(int index, object value, out WrenSharedDataHandle handle, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotSharedData(slot, value, out handle);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void SetNewSharedData(int index, int classSlot, WrenSharedDataHandle handle, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotNewSharedData(slot, classSlot, handle);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void SetNewSharedData(int index, int classSlot, object value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlotNewSharedData(slot, classSlot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public void SetNewForeign(int index, int classSlot, ulong size, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, slot, classSlot, size);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public ref T SetNewForeign<T>(int index, int classSlot, in T value = default, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(slot, classSlot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return ref returnVal;
        }

        public unsafe T* SetNewForeignPtr<T>(int index, int classSlot, in T value = default, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(slot, classSlot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return returnVal;
        }

        public unsafe T* SetNewForeignPtr<T>(int index, int classSlot, in ReadOnlySpan<T> span, int? elementSlot = default) where T : unmanaged
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(slot, classSlot, in span);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return returnVal;
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes the value stored in <paramref name="index"/> from the list and places the removed value in slot[<paramref name="removedValueSlot"/>].
        /// If <paramref name="removedValueSlot"/> is not supplied, the default value, <see cref="DefaultElementSlot"/>, is used.
        /// </summary>
        /// <param name="index">The index to insert the value at.</param>
        /// <param name="removedValueSlot">The slot to place the removed value in. Defaults to <see cref="DefaultElementSlot"/>.</param>
        public WrenSlot Remove(int index, int? removedValueSlot = default)
        {
            int slot = removedValueSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.ListRemove(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return new WrenSlot(m_Vm, slot);
        }

        #endregion
    }
}
