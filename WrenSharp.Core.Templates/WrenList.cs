using WrenSharp.Native;
using System;

namespace WrenSharp
{
	public partial struct WrenList
	{
        #region Contains

		public bool Contains(string value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
		}

		public bool Contains(double value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
		}

		public bool Contains(bool value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
		}

		public bool Contains(WrenHandle value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
		}

        #endregion

        #region Get

		public string GetString(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotString(slot);
		}

		public double GetDouble(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotDouble(slot);
		}

		public bool GetBool(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotBool(slot);
		}

        #endregion
        
        #region Add / Insert

		public void Add(string value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
		}
        
		public void Insert(int index, string value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

		public void Add(double value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
		}
        
		public void Insert(int index, double value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

		public void Add(bool value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
		}
        
		public void Insert(int index, bool value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

		public void Add(WrenHandle value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, -1, slot);
		}
        
		public void Insert(int index, WrenHandle value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.InsertInList(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

        #endregion
	}
}