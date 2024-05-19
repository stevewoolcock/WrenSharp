using WrenSharp.Native;
using System;

namespace WrenSharp
{
	public readonly partial struct WrenMap
	{
        #region ContainsKey

		public bool ContainsKey(string key)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            return Wren.GetMapContainsKey(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot) != 0;
		}

		public bool ContainsKey(double key)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            return Wren.GetMapContainsKey(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot) != 0;
		}

		public bool ContainsKey(bool key)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            return Wren.GetMapContainsKey(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot) != 0;
		}

		public bool ContainsKey(WrenHandle key)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            return Wren.GetMapContainsKey(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot) != 0;
		}

        #endregion

        #region GetValue

		public WrenSlot GetValue(string key, int? keySlot = default, int? valueSlot = default)
		{
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return new WrenSlot(m_Vm, vslot);
		}
        
#if WRENSHARP_EXT
        public unsafe Unsafe.WrenValue GetValueUnsafe(string key, int? keySlot = default, int? valueSlot = default)
        {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            
            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return *Wren.GetSlotPtr(m_Vm.m_Ptr, vslot);
        }
#endif
		public WrenSlot GetValue(double key, int? keySlot = default, int? valueSlot = default)
		{
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return new WrenSlot(m_Vm, vslot);
		}
        
#if WRENSHARP_EXT
        public unsafe Unsafe.WrenValue GetValueUnsafe(double key, int? keySlot = default, int? valueSlot = default)
        {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            
            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return *Wren.GetSlotPtr(m_Vm.m_Ptr, vslot);
        }
#endif
		public WrenSlot GetValue(bool key, int? keySlot = default, int? valueSlot = default)
		{
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return new WrenSlot(m_Vm, vslot);
		}
        
#if WRENSHARP_EXT
        public unsafe Unsafe.WrenValue GetValueUnsafe(bool key, int? keySlot = default, int? valueSlot = default)
        {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            
            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return *Wren.GetSlotPtr(m_Vm.m_Ptr, vslot);
        }
#endif
		public WrenSlot GetValue(WrenHandle key, int? keySlot = default, int? valueSlot = default)
		{
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return new WrenSlot(m_Vm, vslot);
		}
        
#if WRENSHARP_EXT
        public unsafe Unsafe.WrenValue GetValueUnsafe(WrenHandle key, int? keySlot = default, int? valueSlot = default)
        {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            
            m_Vm.SetSlot(kslot, key);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, kslot, vslot);
            return *Wren.GetSlotPtr(m_Vm.m_Ptr, vslot);
        }
#endif
	    public unsafe IntPtr GetValueForeign(string key, int? keySlot = default, int? valueSlot = default)
	    {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            m_Vm.MapGetValue(m_MapSlot, kslot, vslot);
            return Wren.GetSlotForeign(m_Vm.m_Ptr, vslot);
	    }
        
	    public unsafe ref T GetValueForeign<T>(string key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }
        
	    public unsafe T* GetValueForeignPtr<T>(string key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe T GetValueSharedData<T>(string key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
            return m_Vm.SharedData.Get<T>(handle);
	    }
        
	    public unsafe WrenSharedDataHandle GetValueSharedDataHandle(string key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe IntPtr GetValueForeign(double key, int? keySlot = default, int? valueSlot = default)
	    {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            m_Vm.MapGetValue(m_MapSlot, kslot, vslot);
            return Wren.GetSlotForeign(m_Vm.m_Ptr, vslot);
	    }
        
	    public unsafe ref T GetValueForeign<T>(double key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }
        
	    public unsafe T* GetValueForeignPtr<T>(double key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe T GetValueSharedData<T>(double key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
            return m_Vm.SharedData.Get<T>(handle);
	    }
        
	    public unsafe WrenSharedDataHandle GetValueSharedDataHandle(double key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe IntPtr GetValueForeign(bool key, int? keySlot = default, int? valueSlot = default)
	    {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            m_Vm.MapGetValue(m_MapSlot, kslot, vslot);
            return Wren.GetSlotForeign(m_Vm.m_Ptr, vslot);
	    }
        
	    public unsafe ref T GetValueForeign<T>(bool key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }
        
	    public unsafe T* GetValueForeignPtr<T>(bool key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe T GetValueSharedData<T>(bool key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
            return m_Vm.SharedData.Get<T>(handle);
	    }
        
	    public unsafe WrenSharedDataHandle GetValueSharedDataHandle(bool key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe IntPtr GetValueForeign(WrenHandle key, int? keySlot = default, int? valueSlot = default)
	    {
            int kslot = keySlot.GetValueOrDefault(m_DefaultKeySlot);
            int vslot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);

            m_Vm.SetSlot(kslot, key);
            m_Vm.MapGetValue(m_MapSlot, kslot, vslot);
            return Wren.GetSlotForeign(m_Vm.m_Ptr, vslot);
	    }
        
	    public unsafe ref T GetValueForeign<T>(WrenHandle key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }
        
	    public unsafe T* GetValueForeignPtr<T>(WrenHandle key) where T : unmanaged
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

	    public unsafe T GetValueSharedData<T>(WrenHandle key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
            return m_Vm.SharedData.Get<T>(handle);
	    }
        
	    public unsafe WrenSharedDataHandle GetValueSharedDataHandle(WrenHandle key)
	    {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.MapGetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_DefaultValueSlot);
	    }

        #endregion
        
        #region SetValue

		public void SetValue(string key, string value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(string key, double value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(string key, bool value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(string key, WrenHandle value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(double key, string value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(double key, double value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(double key, bool value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(double key, WrenHandle value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(bool key, string value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(bool key, double value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(bool key, bool value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(bool key, WrenHandle value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(WrenHandle key, string value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(WrenHandle key, double value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(WrenHandle key, bool value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValue(WrenHandle key, WrenHandle value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlot(m_DefaultValueSlot, value);
            m_Vm.MapSetValue(m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}

		public void SetValueSharedData(string key, object value, out WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotSharedData(m_DefaultValueSlot, value, out handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(string key, int classSlot, WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(string key, int classSlot, object value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewForeign(string key, int classSlot, ulong size)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, m_DefaultValueSlot, classSlot, size);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public ref T SetValueNewForeign<T>(string key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref returnVal;
		}
        
		public unsafe T* SetValueNewForeignPtr<T>(string key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
		}

        public unsafe T* SetSlotNewForeignPtr<T>(string key, int classSlot, in ReadOnlySpan<T> span) where T : unmanaged
        {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, in span);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
        }

		public void SetValueSharedData(double key, object value, out WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotSharedData(m_DefaultValueSlot, value, out handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(double key, int classSlot, WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(double key, int classSlot, object value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewForeign(double key, int classSlot, ulong size)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, m_DefaultValueSlot, classSlot, size);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public ref T SetValueNewForeign<T>(double key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref returnVal;
		}
        
		public unsafe T* SetValueNewForeignPtr<T>(double key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
		}

        public unsafe T* SetSlotNewForeignPtr<T>(double key, int classSlot, in ReadOnlySpan<T> span) where T : unmanaged
        {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, in span);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
        }

		public void SetValueSharedData(bool key, object value, out WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotSharedData(m_DefaultValueSlot, value, out handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(bool key, int classSlot, WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(bool key, int classSlot, object value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewForeign(bool key, int classSlot, ulong size)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, m_DefaultValueSlot, classSlot, size);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public ref T SetValueNewForeign<T>(bool key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref returnVal;
		}
        
		public unsafe T* SetValueNewForeignPtr<T>(bool key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
		}

        public unsafe T* SetSlotNewForeignPtr<T>(bool key, int classSlot, in ReadOnlySpan<T> span) where T : unmanaged
        {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, in span);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
        }

		public void SetValueSharedData(WrenHandle key, object value, out WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotSharedData(m_DefaultValueSlot, value, out handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(WrenHandle key, int classSlot, WrenSharedDataHandle handle)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, handle);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewSharedData(WrenHandle key, int classSlot, object value)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            m_Vm.SetSlotNewSharedData(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public void SetValueNewForeign(WrenHandle key, int classSlot, ulong size)
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.SetSlotNewForeign(m_Vm.m_Ptr, m_DefaultValueSlot, classSlot, size);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
		}
        
		public ref T SetValueNewForeign<T>(WrenHandle key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            ref T returnVal = ref m_Vm.SetSlotNewForeign(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return ref returnVal;
		}
        
		public unsafe T* SetValueNewForeignPtr<T>(WrenHandle key, int classSlot, in T value = default) where T : unmanaged
		{
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, value);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
		}

        public unsafe T* SetSlotNewForeignPtr<T>(WrenHandle key, int classSlot, in ReadOnlySpan<T> span) where T : unmanaged
        {
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            T* returnVal = m_Vm.SetSlotNewForeignPtr(m_DefaultValueSlot, classSlot, in span);
            Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, m_DefaultValueSlot);
            return returnVal;
        }

        #endregion
        
        #region Remove

		public WrenSlot Remove(string key, int? removedValueSlot = default)
		{
            int valueSlot = removedValueSlot.GetValueOrDefault(m_DefaultValueSlot);
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.RemoveMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, valueSlot);
            return new WrenSlot(m_Vm, valueSlot);
		}

		public WrenSlot Remove(double key, int? removedValueSlot = default)
		{
            int valueSlot = removedValueSlot.GetValueOrDefault(m_DefaultValueSlot);
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.RemoveMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, valueSlot);
            return new WrenSlot(m_Vm, valueSlot);
		}

		public WrenSlot Remove(bool key, int? removedValueSlot = default)
		{
            int valueSlot = removedValueSlot.GetValueOrDefault(m_DefaultValueSlot);
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.RemoveMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, valueSlot);
            return new WrenSlot(m_Vm, valueSlot);
		}

		public WrenSlot Remove(WrenHandle key, int? removedValueSlot = default)
		{
            int valueSlot = removedValueSlot.GetValueOrDefault(m_DefaultValueSlot);
            m_Vm.SetSlot(m_DefaultKeySlot, key);
            Wren.RemoveMapValue(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot, valueSlot);
            return new WrenSlot(m_Vm, valueSlot);
		}

        #endregion
	}
}