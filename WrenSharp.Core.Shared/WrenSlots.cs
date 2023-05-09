using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// Exposes an API for manipulating Wren API slots.
    /// </summary>
    public partial class WrenSlots
    {
        private readonly WrenVM m_Vm;

        #region Properties

        /// <summary>
        /// Returns the number of allocated API slots for the current Wren fiber.
        /// </summary>
        public int Count
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Wren.GetSlotCount(m_Vm.m_Ptr);
        }

        /// <summary>
        /// Gets a <see cref="WrenSlot"/> value for slot[<paramref name="index"/>].
        /// </summary>
        /// <param name="index">The index of the slot.</param>
        /// <returns>A <see cref="WrenSlot"/> value.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="index"/>is less than zero or greater or equal to <see cref="Count"/>.</exception>
        public WrenSlot this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                    throw new ArgumentOutOfRangeException(nameof(index));

                return new WrenSlot(m_Vm, index);
            }
        }

        #endregion

        internal WrenSlots(WrenVM vm)
        {
            m_Vm = vm;
        }

        /// <summary>
        /// Ensures the current Wren fiber has at least <paramref name="capacity"/> API slots available.
        /// </summary>
        /// <param name="capacity">The capacity of API slots to ensure.</param>
        /// <returns>A reference to this <see cref="WrenSlots"/>.</returns>
        public WrenSlots EnsureCapacity(int capacity)
        {
            Wren.EnsureSlots(m_Vm.m_Ptr, capacity);
            return this;
        }

#if !WRENSHARP_EXT
        /// <summary>
        /// Copies the value in slot[<paramref name="slotFrom"/>] to slot[<paramref name="slotTo"/>].
        /// If <paramref name="slotFrom"/> is equal to <paramref name="slotTo"/>, this method does nothing.
        /// This method does not ensure that <paramref name="slotFrom"/> or <paramref name="slotTo"/> are valid
        /// before attempting the copy.
        /// </summary>
        /// <param name="slotFrom">The slot containing the value to copy.</param>
        /// <param name="slotTo">The slot to copy the value into.</param>
        /// <returns>A reference to this <see cref="WrenSlots"/>.</returns>
        public unsafe WrenSlots Copy(int slotFrom, int slotTo)
        {
            if (slotFrom == slotTo)
                return this;

            IntPtr handle = Wren.GetSlotHandle(m_Vm.m_Ptr, slotFrom);
            Wren.SetSlotHandle(m_Vm.m_Ptr, slotTo, handle);
            Wren.ReleaseHandle(m_Vm.m_Ptr, handle);
            return this;
        }
#endif
    }
}
