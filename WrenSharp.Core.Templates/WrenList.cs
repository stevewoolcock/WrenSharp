using WrenSharp.Native;
using System;

namespace WrenSharp
{
	public readonly partial struct WrenList
	{
		public string GetString(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotString(slot);
		}
        

		public bool Contains(string value, int? elementSlot = default)
		{
            return IndexOf(value, elementSlot) >= 0;
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

#if WRENSHARP_EXT
		public int IndexOf(string value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, slot);
		}
#else
        public int IndexOf(string value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(m_DefaultElementSlot, value);

            int count = Wren.GetListCount(m_Vm.m_Ptr, m_ListSlot);
            for (int i = 0; i < count; i++)
            {
                Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, i, slot);
                if (m_Vm.GetSlotString(slot) == value)
                {
                    return i;
                }

            }

            return -1;
        }
#endif

		public double GetDouble(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotDouble(slot);
		}
        

		public bool Contains(double value, int? elementSlot = default)
		{
            return IndexOf(value, elementSlot) >= 0;
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

#if WRENSHARP_EXT
		public int IndexOf(double value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, slot);
		}
#else
        public int IndexOf(double value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(m_DefaultElementSlot, value);

            int count = Wren.GetListCount(m_Vm.m_Ptr, m_ListSlot);
            for (int i = 0; i < count; i++)
            {
                Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, i, slot);
                if (m_Vm.GetSlotDouble(slot) == value)
                {
                    return i;
                }

            }

            return -1;
        }
#endif

		public bool GetBool(int index, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, index, slot);
            return m_Vm.GetSlotBool(slot);
		}
        

		public bool Contains(bool value, int? elementSlot = default)
		{
            return IndexOf(value, elementSlot) >= 0;
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

#if WRENSHARP_EXT
		public int IndexOf(bool value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, slot);
		}
#else
        public int IndexOf(bool value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(m_DefaultElementSlot, value);

            int count = Wren.GetListCount(m_Vm.m_Ptr, m_ListSlot);
            for (int i = 0; i < count; i++)
            {
                Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, i, slot);
                if (m_Vm.GetSlotBool(slot) == value)
                {
                    return i;
                }

            }

            return -1;
        }
#endif


		public bool Contains(WrenHandle value, int? elementSlot = default)
		{
            return IndexOf(value, elementSlot) >= 0;
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

#if WRENSHARP_EXT
		public int IndexOf(WrenHandle value, int? elementSlot = default)
		{
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(slot, value);
            return Wren.GetListIndexOf(m_Vm.m_Ptr, m_ListSlot, slot);
		}
#else
        public int IndexOf(WrenHandle value, int? elementSlot = default)
        {
            int slot = elementSlot.GetValueOrDefault(m_DefaultElementSlot);
            m_Vm.SetSlot(m_DefaultElementSlot, value);

            int count = Wren.GetListCount(m_Vm.m_Ptr, m_ListSlot);
            for (int i = 0; i < count; i++)
            {
                Wren.GetListElement(m_Vm.m_Ptr, m_ListSlot, i, slot);
                if (m_Vm.GetSlotHandle(slot) == value.m_Ptr)
                {
                    return i;
                }

            }

            return -1;
        }
#endif

	}
}