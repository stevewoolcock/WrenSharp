using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    public partial struct WrenMap
    {
        private readonly WrenVM m_Vm;
        private readonly int m_MapSlot;
        private readonly int m_DefaultKeySlot;
        private readonly int m_DefaultValueSlot;

        #region Properties

        /// <summary>
        /// The slot the map resides in.
        /// </summary>
        public int MapSlot => m_MapSlot;

        /// <summary>
        /// The default slot the Wren API will read keys from if an override is not supplied.
        /// </summary>
        public int DefaultKeySlot => m_DefaultKeySlot;

        /// <summary>
        /// The default slot the Wren API will read or write values to if an override is not supplied.
        /// </summary>
        public int DefaultValueSlot => m_DefaultValueSlot;

        /// <summary>
        /// The count of entries in the map.
        /// </summary>
        public int Count => Wren.GetMapCount(m_Vm.m_Ptr, m_MapSlot);

        /// <summary>
        /// The <see cref="WrenVM"/> the map resides within.
        /// </summary>
        public WrenVM VM => m_Vm;

        #endregion

        /// <summary>
        /// Initializes a <see cref="WrenMap"/> and ensures there are enough Wren API slots allocated to read and write to the map.
        /// </summary>
        /// <param name="vm">The <see cref="WrenVM"/> the map resides in.</param>
        /// <param name="mapSlot">The API slot the map resides in.</param>
        /// <param name="keySlot">The API slot to store keys in. If null, uses <paramref name="mapSlot"/><c> + 1</c>.</param>
        /// <param name="valueSlot">The API slot to read and write from. If null, uses <paramref name="keySlot"/><c> + 1</c>.</param>
        /// <exception cref="WrenTypeException">Thrown if the value in <paramref name="mapSlot"/> is not a <see cref="WrenType.Map"/>.</exception>
        public WrenMap(WrenVM vm, int mapSlot, int? keySlot = default, int? valueSlot = default)
        {
            var slotType = Wren.GetSlotType(vm.m_Ptr, mapSlot);
            if (slotType != WrenType.Map)
                throw new WrenTypeException(vm, $"Value is not a Map ({slotType})");

            m_Vm = vm;
            m_MapSlot = mapSlot;
            m_DefaultKeySlot = keySlot.GetValueOrDefault(m_MapSlot + 1);
            m_DefaultValueSlot = valueSlot.GetValueOrDefault(m_DefaultKeySlot + 1);

            int requiredSlotCount = Math.Max(m_MapSlot, Math.Max(m_DefaultKeySlot, m_DefaultValueSlot)) + 1;
            vm.EnsureSlotCount(requiredSlotCount);
        }

        /// <summary>
        /// Clears all values in the map by invoking its clear() method.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear() => m_Vm.MapClear(m_MapSlot);

        /// <summary>
        /// Indicates if the map contains a "null" key.
        /// </summary>
        /// <returns>True if the map contains a "null" key, otherwise false.</returns>
        public bool ContainsKeyNull()
		{
            Wren.SetSlotNull(m_Vm.m_Ptr, m_DefaultKeySlot);
            return Wren.GetMapContainsKey(m_Vm.m_Ptr, m_MapSlot, m_DefaultKeySlot) != 0;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ContainsKey(int keySlot) => Wren.GetMapContainsKey(m_Vm.m_Ptr, m_MapSlot, keySlot) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetValue(int keySlot, int valueSlot) => Wren.SetMapValue(m_Vm.m_Ptr, m_MapSlot, keySlot, valueSlot);

        public WrenSlot GetValue(int keySlot, int? valueSlot = default)
        {
            int slot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, keySlot, slot);
            return new WrenSlot(m_Vm, slot);
        }

        public WrenSlot Remove(int keySlot, int? removedValueSlot = default)
        {
            int valueSlot = removedValueSlot.GetValueOrDefault(m_DefaultValueSlot);
            Wren.RemoveMapValue(m_Vm.m_Ptr, m_MapSlot, keySlot, valueSlot);
            return new WrenSlot(m_Vm, valueSlot);
        }

#if WRENSHARP_EXT
        public unsafe Unsafe.WrenValue GetValueUnsafe(int keySlot, int? valueSlot = default)
        {
            int slot = valueSlot.GetValueOrDefault(m_DefaultValueSlot);
            Wren.GetMapValue(m_Vm.m_Ptr, m_MapSlot, keySlot, slot);
            return *Wren.GetSlotPtr(m_Vm.m_Ptr, slot);
        }
#endif
    }
}