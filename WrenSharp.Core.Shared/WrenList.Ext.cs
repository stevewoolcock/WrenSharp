#if WRENSHARP_EXT
using System;
using WrenSharp.Native;
using WrenSharp.Unsafe;

namespace WrenSharp
{
    public partial struct WrenList
    {
        #region Properties

        public unsafe WrenNativeList AsNativeList => Wren.GetSlotPtr(m_Vm.m_Ptr, m_ListSlot)->AsList;

        #endregion

        public void Add(Unsafe.WrenValue value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.SetSlot(m_Vm.m_Ptr, slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
        }

        public void Insert(Unsafe.WrenValue value, int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.SetSlot(m_Vm.m_Ptr, slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
        }

        public bool Remove(Unsafe.WrenValue value, int? removedValueSlot = default)
        {
            int slot = removedValueSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.SetSlot(m_Vm.m_Ptr, m_DefaultElementSlot, value);
            int index = Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot);
            if (index > -1)
            {
                Wren.ListRemove(m_Vm.m_Ptr, m_ListSlot, index, slot);
                return true;
            }

            return false;
        }

        public bool Remove(Unsafe.WrenValue value, ref WrenSlot removedValueSlot)
        {
            Wren.SetSlot(m_Vm.m_Ptr, m_DefaultElementSlot, value);
            int index = Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot);
            if (index > -1)
            {
                Wren.ListRemove(m_Vm.m_Ptr, m_ListSlot, index, removedValueSlot);
                return true;
            }

            return false;
        }

        public unsafe ref Unsafe.WrenValue GetUnsafe(int index, int? elementSlot = default) => ref *GetUnsafePtr(index, elementSlot);

        public unsafe Unsafe.WrenValue* GetUnsafePtr(int index, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return Wren.GetSlotPtr(m_Vm.m_Ptr, slot);
        }
    }
}
#endif
