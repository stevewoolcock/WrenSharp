using WrenSharp.Native;
using System;

namespace WrenSharp
{
	public partial struct WrenList
	{
		public string GetString(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotString(slot);
		}
        
		public bool Contains(string value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
		}

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
        
		public void Set(int index, string value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

		public double GetDouble(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotDouble(slot);
		}
        
		public bool Contains(double value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
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
        
		public void Set(int index, double value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

		public bool GetBool(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotBool(slot);
		}
        
		public bool Contains(bool value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
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
        
		public void Set(int index, bool value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

		public bool Contains(WrenHandle value)
		{
            m_Vm.SetSlot(m_DefaultElementSlot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, m_DefaultElementSlot) >= 0;
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
        
		public void Set(int index, WrenHandle value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            Wren.SetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
		}

	}
}