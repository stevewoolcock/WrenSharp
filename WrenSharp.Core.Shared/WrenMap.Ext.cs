#if WRENSHARP_EXT
using WrenSharp.Native;
using WrenSharp.Unsafe;

namespace WrenSharp
{
    public readonly partial struct WrenMap
    {
        #region Properties

        public unsafe WrenNativeMap AsNativeMap => Wren.GetSlotPtr(m_Vm.m_Ptr, m_MapSlot)->AsMap;

        #endregion

        public unsafe ref Unsafe.WrenValue GetValueUnsafe(int keySlot, int? valueSlot = default) => ref *GetValueUnsafePtr(keySlot, valueSlot);

        public unsafe Unsafe.WrenValue* GetValueUnsafePtr(int keySlot, int? valueSlot = default)
        {
            int slot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, keySlot, slot);
            return Wren.GetSlotPtr(m_Vm.m_Ptr, slot);
        }
    }
}
#endif