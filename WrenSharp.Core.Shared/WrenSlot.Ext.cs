#if WRENSHARP_EXT
using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    public readonly partial struct WrenSlot
    {
        #region Properties

        /// <summary>
        /// A pointer to the <see cref="Unsafe.WrenValue"/> in the API slot.
        /// </summary>
        public unsafe Unsafe.WrenValue* ValuePtr => Wren.GetSlotPtr(m_Vm.m_Ptr, m_Index);

        /// <summary>
        /// The <see cref="Unsafe.WrenValue"/> in the API slot.
        /// </summary>
        public unsafe ref Unsafe.WrenValue Value => ref *Wren.GetSlotPtr(m_Vm.m_Ptr, m_Index);

        #endregion

        /// <summary>
        /// Indicates if the values in this slot is equal to the value in slot[<paramref name="otherSlot"/>].
        /// </summary>
        /// <param name="otherSlot">The slot to compare to.</param>
        /// <returns>True if the values are the same, otherwise false.</returns>
        public unsafe bool ValueEquals(int otherSlot)
        {
            return *Wren.GetSlotPtr(m_Vm.m_Ptr, m_Index) == *Wren.GetSlotPtr(m_Vm.m_Ptr, otherSlot);
        }
    }
}
#endif
