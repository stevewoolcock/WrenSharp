using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;
#if WRENSHARP_EXT
using WrenSharp.Unsafe;
#endif

namespace WrenSharp
{
    /// <summary>
    /// Exposes an API for manipulating Wren API slots.
    /// </summary>
    public class WrenSlots
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

#if WRENSHARP_EXT
            *Wren.GetSlotPtr(m_Vm.m_Ptr, slotTo) = *Wren.GetSlotPtr(m_Vm.m_Ptr, slotFrom);
#else
            IntPtr handle = Wren.GetSlotHandle(m_Vm.m_Ptr, slotFrom);
            Wren.SetSlotHandle(m_Vm.m_Ptr, slotTo, handle);
            Wren.ReleaseHandle(m_Vm.m_Ptr, handle);
#endif
            return this;
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

#if WRENSHARP_EXT
        /// <summary>
        /// Returns a direct view to the native Wren API slot memory for fast access by avoiding subsequent
        /// calls into the Wren API to retrieve value information. This method retrieves the current total number
        /// of slots and a pointer to the first slot, therefore the returned <see cref="Span{T}"/> should be cached
        /// in a local variable, or any performance gains are lost.
        /// <para/>
        /// The returned span beings at slot[0] and ends at slot[Count]. Call <see cref="EnsureCapacity(int)"/>
        /// first to ensure the span contains at least the required number of slots.
        /// </summary>
        public unsafe Span<WrenValue> Values
        {
            get
            {
                int slotCount = Wren.GetSlotCount(m_Vm.m_Ptr);
                var slotPtr = Wren.GetSlotPtr(m_Vm.m_Ptr, 0);
                return new Span<WrenValue>(slotPtr, slotCount);
            }
        }

        /// <summary>
        /// Returns a direct view to the native Wren API slot memory for fast access by avoiding subsequent
        /// calls into the Wren API to retrieve value information. This method ensures that <paramref name="count"/>
        /// slots will exist, there is no need to call <see cref="EnsureCapacity(int)"/> before invoking this method.
        /// The returned <see cref="Span{T}"/> should be cached in a local variable, or any performance gains are lost.
        /// <para/>
        /// The returned span beings at slot[0] and ends at slot[<paramref name="count"/>].
        /// </summary>
        /// <param name="count">The number of slots to span.</param>
        public unsafe Span<WrenValue> GetValues(int count)
        {
            Wren.EnsureSlots(m_Vm.m_Ptr, count);
            var slotPtr = Wren.GetSlotPtr(m_Vm.m_Ptr, 0);
            return new Span<WrenValue>(slotPtr, count);
        }

        /// <summary>
        /// Indicates if the values in slot[<paramref name="slot1"/>] and slot[<paramref name="slot2"/>] hold the same values.
        /// </summary>
        /// <param name="slot1">The first slot to compare.</param>
        /// <param name="slot2">The second slot to compare.</param>
        /// <returns>True if the values are the same, otherwise false.</returns>
        public unsafe bool ValuesEqual(int slot1, int slot2) => *Wren.GetSlotPtr(m_Vm.m_Ptr, slot1) == *Wren.GetSlotPtr(m_Vm.m_Ptr, slot2);
#endif
    }
}
