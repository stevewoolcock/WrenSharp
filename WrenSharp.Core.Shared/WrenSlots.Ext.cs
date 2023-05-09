#if WRENSHARP_EXT
using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;
using WrenSharp.Unsafe;

namespace WrenSharp
{
    public partial class WrenSlots
    {
        /// <summary>
        /// Returns a direct view to the native Wren API slot memory for fast access by avoiding subsequent
        /// calls into the Wren API to retrieve value information. This method calls the Wren API to retrieve
        /// the current total number of slots and a pointer to the first slot, therefore the returned <see cref="Span{T}"/>
        /// should be cached in a local variable, or any performance gains are lost.
        /// <para/>
        /// The returned span beings at slot[0] and ends at slot[Count]. Call <see cref="EnsureCapacity(int)"/>
        /// first to ensure the span contains at least the required number of slots.
        /// </summary>
        public unsafe Span<WrenValue> Span
        {
            get
            {
                int slotCount = Wren.GetSlotCount(m_Vm.m_Ptr);
                WrenValue* slotPtr = Wren.GetSlotPtr(m_Vm.m_Ptr, 0);
                return new Span<WrenValue>(slotPtr, slotCount);
            }
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

            *Wren.GetSlotPtr(m_Vm.m_Ptr, slotTo) = *Wren.GetSlotPtr(m_Vm.m_Ptr, slotFrom);
            return this;
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
        /// <returns>A span around the native slot values.</returns>
        public unsafe Span<WrenValue> AsSpan(int count)
        {
            Wren.EnsureSlots(m_Vm.m_Ptr, count);
            WrenValue* slotPtr = Wren.GetSlotPtr(m_Vm.m_Ptr, 0);
            return new Span<WrenValue>(slotPtr, count);
        }

        /// <summary>
        /// Returns a direct view to the native Wren API slot memory for fast access by avoiding subsequent
        /// calls into the Wren API to retrieve value information. This method ensures that required number of
        /// slots will exist, there is no need to call <see cref="EnsureCapacity(int)"/> before invoking this method.
        /// The returned <see cref="Span{T}"/> should be cached in a local variable, or any performance gains are lost.
        /// <para/>
        /// The returned span beings at slot[<paramref name="startSlot"/>] and ends at slot[<paramref name="startSlot"/> + <paramref name="count"/>].
        /// </summary>
        /// <param name="startSlot">The slot to being the span at.</param>
        /// <param name="count">The number of slots to span.</param>
        /// <returns>A span around the native slot values.</returns>
        public unsafe Span<WrenValue> AsSpan(int startSlot, int count)
        {
            Wren.EnsureSlots(m_Vm.m_Ptr, startSlot + count);
            WrenValue* slotPtr = Wren.GetSlotPtr(m_Vm.m_Ptr, 0);
            return new Span<WrenValue>(slotPtr + startSlot, count);
        }

        /// <summary>
        /// Indicates if the values in slot[<paramref name="slot1"/>] and slot[<paramref name="slot2"/>] hold the same values.
        /// </summary>
        /// <param name="slot1">The first slot to compare.</param>
        /// <param name="slot2">The second slot to compare.</param>
        /// <returns>True if the values are the same, otherwise false.</returns>
        public unsafe bool ValuesEqual(int slot1, int slot2) => *Wren.GetSlotPtr(m_Vm.m_Ptr, slot1) == *Wren.GetSlotPtr(m_Vm.m_Ptr, slot2);
    }
}
#endif