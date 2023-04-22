using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    public unsafe partial class WrenVM
    {
        /// <summary>
        /// Ensures that the foreign method stack has at least <paramref name="slotCount"/> available for use, growing the stack if needed.<para />
        /// Does not shrink the stack if it has more than enough slots.<para />
        /// It is an error to call this from a Wren foriegn finalizer.
        /// </summary>
        /// <param name="slotCount">The number of slots to ensure are allocated.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM EnsureSlotCount(int slotCount)
        {
            Wren.EnsureSlots(m_Ptr, slotCount);
            return this;
        }

        /// <summary>
        /// Returns the number of slots available to the current foreign method.
        /// </summary>
        /// <returns>The number of slots available to the current foreign method.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSlotCount() => Wren.GetSlotCount(m_Ptr);

        #region Value Getters

        /// <summary>
        /// Reads a boolean value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a boolean value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The boolean value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetSlotBool(int slot) => Wren.GetSlotBool(m_Ptr, slot) != 0;

        /// <summary>
        /// Reads a byte value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The byte value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetSlotByte(int slot) => (byte)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a sbyte value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The sbyte value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte GetSlotSByte(int slot) => (sbyte)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a signed 16bit integer value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The signed 16bit integer value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetSlotInt16(int slot) => (short)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads an unsigned 16bit integer value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The unsigned 16bit integer value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetSlotUInt16(int slot) => (ushort)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a signed 32bit integer value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The signed 32bit integer value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetSlotInt32(int slot) => (int)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads an unsigned 32bit integer value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The unsigned 32bit integer value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetSlotUInt32(int slot) => (uint)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a signed 64bit integer value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The signed 64bit integer value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetSlotInt64(int slot) => (long)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a unsigned 64bit integer value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The unsigned 64bit integer value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetSlotUInt64(int slot) => (ulong)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a 32bit float value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The 32bit float value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetSlotFloat(int slot) => (float)Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a 64bit float value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a number value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The 64bit float value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetSlotDouble(int slot) => Wren.GetSlotDouble(m_Ptr, slot);

        /// <summary>
        /// Reads a string value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a string value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The string value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetSlotString(int slot) => WrenInternal.GetSlotString(m_Ptr, slot);

        /// <summary>
        /// Reads the bytes representing a string value from <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a string value.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>The bytes representing the string value in the slot.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetSlotStringBytes(int slot) => WrenInternal.GetSlotStringBytes(m_Ptr, slot);

        /// <summary>
        /// Gets the <see cref="WrenType"/> of the value in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index.</param>
        /// <returns>The <see cref="WrenType"/> of the value.</returns>
        public WrenType GetSlotType(int slot) => Wren.GetSlotType(m_Ptr, slot);

        // 

        /// <summary>
        /// Attempts to read a boolean from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Bool"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Bool"/>.</returns>
        public bool TryGetSlot(int slot, out bool value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Bool)
            {
                value = Wren.GetSlotBool(m_Ptr, slot) != 0;
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an unsigned byte from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out byte value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (byte)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read a signed byte from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out sbyte value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (sbyte)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read a signed 16bit integer from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out short value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (short)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an unsigned 16bit integer from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out ushort value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (ushort)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an signed 32bit integer from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out int value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (int)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an unsigned 32bit integer from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out uint value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (uint)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an signed 64bit integer from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out long value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (long)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an unsigned 64bit integer from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out ulong value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (ulong)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an 32bit float from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out float value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = (float)Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read an 64bit float from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.Number"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out double value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Number)
            {
                value = Wren.GetSlotDouble(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read a string from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.String"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out string value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.String)
            {
                value = WrenInternal.GetSlotString(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to read the bytes representing a string from <paramref name="slot"/>.
        /// Returns true if the <paramref name="slot"/> is less than the number of slots and stores a <see cref="WrenType.String"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value in <paramref name="slot"/>.</param>
        /// <returns>True if <paramref name="slot"/> exists holds a value of type <see cref="WrenType.Number"/>.</returns>
        public bool TryGetSlot(int slot, out ReadOnlySpan<byte> value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.String)
            {
                value = WrenInternal.GetSlotStringBytes(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        #endregion

        #region Setters

        /// <summary>
        /// Stores a boolean value in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlot(int slot, bool value)
        {
            Wren.SetSlotBool(m_Ptr, slot, value);
            return this;
        }

        /// <summary>
        /// Stores a number value in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlot(int slot, double value)
        {
            Wren.SetSlotDouble(m_Ptr, slot, value);
            return this;
        }

        /// <summary>
        /// Stores a string value in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlot(int slot, string value)
        {
            Wren.SetSlotString(m_Ptr, slot, value);
            return this;
        }

        /// <summary>
        /// Stores a string value in <paramref name="slot"/>. This method accepts a span of raw bytes
        /// representing the string.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <param name="stringBytes">The value to write.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlot(int slot, ReadOnlySpan<byte> stringBytes)
        {
            WrenInternal.SetSlotString(m_Ptr, slot, stringBytes);
            return this;
        }

        /// <summary>
        /// Stores the value of a <see cref="WrenHandle"/> in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        /// <exception cref="WrenInvalidHandleException">Thrown if <paramref name="value"/> is not valid.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlot(int slot, WrenHandle value)
        {
            EnsureValidHandle(in value);
            Wren.SetSlotHandle(m_Ptr, slot, value.m_Ptr);
            return this;
        }

        /// <summary>
        /// Stores the value of a <see cref="WrenHandle"/> in <paramref name="slot"/>. This method does not check
        /// the validity of <paramref name="value"/>. If <paramref name="value"/> is an invalid <see cref="WrenHandle"/>,
        /// the results of this operation are undefined.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <param name="value">The value to write.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotUnsafe(int slot, WrenHandle value)
        {
            Wren.SetSlotHandle(m_Ptr, slot, value.m_Ptr);
            return this;
        }

        /// <summary>
        /// Stores a null value in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotNull(int slot)
        {
            Wren.SetSlotNull(m_Ptr, slot);
            return this;
        }

        #endregion

        #region Foreign

        /// <summary>
        /// Gets a pointer to the foreign value stored in <paramref name="slot"/>.<para/>
        /// It is an error to call this if the slot does not contain a foreign class type or instance.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>A pointer to a foreign value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetSlotForeign(int slot) => Wren.GetSlotForeign(m_Ptr, slot);

        /// <summary>
        /// Gets a ref to to the foreign value stored in <paramref name="slot"/>, cast to <typeparamref name="T"/>.<para/>
        /// It is an error to call this if the slot does not contain a foreign class type or instance.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>A ref to the foreign value of type <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetSlotForeign<T>(int slot) where T : unmanaged => ref *(T*)Wren.GetSlotForeign(m_Ptr, slot);

        /// <summary>
        /// Gets a pointer to to the foreign value stored in <paramref name="slot"/>, cast to <typeparamref name="T"/>.<para/>
        /// It is an error to call this if the slot does not contain a foreign class type or instance.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <returns>A pointer to the foreign value of type <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetSlotForeignPtr<T>(int slot) where T : unmanaged => (T*)Wren.GetSlotForeign(m_Ptr, slot);

        /// <summary>
        /// Creates a new instance of the foreign class stored in <paramref name="classSlot"/> with <paramref name="size"/>
        /// bytes of raw storage and places the resulting object in <paramref name="slot"/>.<para />
        /// 
        /// This does not invoke the foreign class's constructor on the new instance. If you need that to happen, call the
        /// constructor from Wren, which will then call the allocator foreign method. In there, call this to create the object
        /// and then the constructor will be invoked when the allocator returns.
        /// </summary>
        /// <param name="slot">The slot to place thew new instance in.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="size">The number of bytes required for the instance.</param>
        /// <returns>A pointer to the new instance's data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr SetSlotNewForeign(int slot, int classSlot, ulong size) => Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, size);

        /// <summary>
        /// Creates a new instance of the foreign class stored in <paramref name="classSlot"/> with enough size to store a value
        /// of type <typeparamref name="T"/> and places the resulting object in <paramref name="slot"/>.<para />
        /// 
        /// This does not invoke the foreign class's constructor on the new instance. If you need that to happen, call the
        /// constructor from Wren, which will then call the allocator foreign method. In there, call this to create the object
        /// and then the constructor will be invoked when the allocator returns.
        /// </summary>
        /// <typeparam name="T">The value type of the data to allocate for the instance.</typeparam>
        /// <param name="slot">The slot to place thew new instance in.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="data">The data to initialize the new instance with.</param>
        /// <returns>A ref to the new instance's data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T SetSlotNewForeign<T>(int slot, int classSlot, in T data = default) where T : unmanaged
        {
            T* ptr = (T*)Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, (ulong)sizeof(T));
            *ptr = data;
            return ref *ptr;
        }

        /// <summary>
        /// Creates a new instance of the foreign class stored in <paramref name="classSlot"/> with enough size to store a value
        /// of type <typeparamref name="T"/> and places the resulting object in <paramref name="slot"/>.<para />
        /// 
        /// This does not invoke the foreign class's constructor on the new instance. If you need that to happen, call the
        /// constructor from Wren, which will then call the allocator foreign method. In there, call this to create the object
        /// and then the constructor will be invoked when the allocator returns.
        /// </summary>
        /// <typeparam name="T">The value type of the data to allocate for the instance.</typeparam>
        /// <param name="slot">The slot to place thew new instance in.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="data">The data to initialize the new instance with.</param>
        /// <returns>The pointer to the new instance's data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* SetSlotNewForeignPtr<T>(int slot, int classSlot, in T data = default) where T : unmanaged
        {
            T* ptr = (T*)Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, (ulong)sizeof(T));
            *ptr = data;
            return ptr;
        }

        /// <summary>
        /// Creates a new instance of the foreign class stored in <paramref name="classSlot"/> with enough size to store <paramref name="span"/>.
        /// Copies the memory within <paramref name="span"/> to the resulting object and places it in <paramref name="slot"/>.<para />
        /// 
        /// This does not invoke the foreign class's constructor on the new instance. If you need that to happen, call the
        /// constructor from Wren, which will then call the allocator foreign method. In there, call this to create the object
        /// and then the constructor will be invoked when the allocator returns.
        /// </summary>
        /// <typeparam name="T">The value type of the data to allocate for the instance.</typeparam>
        /// <param name="slot">The slot to place thew new instance in.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="span">The span to copy into the new instance.</param>
        /// <returns>The pointer to the new instance's data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* SetSlotNewForeignPtr<T>(int slot, int classSlot, in ReadOnlySpan<T> span) where T : unmanaged
        {
            int len = span.Length * sizeof(T);
            T* ptr = (T*)Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, (ulong)len);
            fixed (T* spanPtr = span)
            {
                Buffer.MemoryCopy(spanPtr, ptr, len, len);
            }
            return ptr;
        }


        /// <summary>
        /// Attempts to retrieve a pointer to the foreign class instance data in <paramref name="slot"/>.
        /// Returns true if <paramref name="slot"/> is less than the number of slots and the value it stores is of type <see cref="WrenType.Foreign"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="dataPtr">The location to store the pointer.</param>
        /// <returns>True if the slot exists and the value stored is of type <see cref="WrenType.Foreign"/>, otherwise false.</returns>
        public bool TryGetSlotForeign(int slot, out IntPtr dataPtr)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Foreign)
            {
                dataPtr = Wren.GetSlotForeign(m_Ptr, slot);
                return true;
            }

            dataPtr = default;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the value of the foreign class instance data in <paramref name="slot"/> of type <typeparamref name="T"/>.
        /// Returns true if <paramref name="slot"/> is less than the number of slots and the value it stores is of type <see cref="WrenType.Foreign"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">The location to store the value.</param>
        /// <returns>True if the slot exists and the value stored is of type <see cref="WrenType.Foreign"/>, otherwise false.</returns>
        public bool TryGetSlotForeign<T>(int slot, out T value) where T : unmanaged
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Foreign)
            {
                value = *(T*)Wren.GetSlotForeign(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve a pointer to the of the foreign class instance data in <paramref name="slot"/> of type <typeparamref name="T"/>.
        /// Returns true if <paramref name="slot"/> is less than the number of slots and the value it stores is of type <see cref="WrenType.Foreign"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">The location to store the value.</param>
        /// <returns>True if the slot exists and the value stored is of type <see cref="WrenType.Foreign"/>, otherwise false.</returns>
        public bool TryGetSlotForeignPtr<T>(int slot, out T* value) where T : unmanaged
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Foreign)
            {
                value = (T*)Wren.GetSlotForeign(m_Ptr, slot);
                return true;
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attempts to retrieve the value of the foreign class instance data in <paramref name="slot"/> of type <see cref="Span{T}"/>.
        /// Returns true if <paramref name="slot"/> is less than the number of slots and the value it stores is of type <see cref="WrenType.Foreign"/>.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="elementCount">The number of elements expected in type <typeparamref name="T"/>.</param>
        /// <param name="span">The location to store the value.</param>
        /// <returns>True if the slot exists and the value stored is of type <see cref="WrenType.Foreign"/>, otherwise false.</returns>
        public bool TryGetSlotForeignSpan<T>(int slot, int elementCount, out Span<T> span) where T : unmanaged
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Foreign)
            {
                span = new Span<T>((void*)Wren.GetSlotForeign(m_Ptr, slot), elementCount * sizeof(T));
                return true;
            }

            span = default;
            return false;
        }

        #endregion

        #region SharedData

        /// <summary>
        /// Gets the foreign object in <paramref name="slot"/>, interprets it as a <see cref="WrenSharedDataHandle"/>
        /// and returns the value the handle points to in the VM's shared data table.
        /// </summary>
        /// <param name="slot">The slot containing the shared data handle.</param>
        /// <returns>The value the shared data handle points to.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetSlotSharedData(int slot) => GetSlotSharedData<object>(slot);

        /// <summary>
        /// Gets the foreign object in <paramref name="slot"/>, interprets it as a <see cref="WrenSharedDataHandle"/>
        /// and returns the value the handle points to in the VM's shared data table.
        /// </summary>
        /// <typeparam name="T">The shared data value type.</typeparam>
        /// <param name="slot">The slot containing the shared data handle.</param>
        /// <returns>The value the shared data handle points to.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetSlotSharedData<T>(int slot)
        {
            var ptr = (WrenSharedDataHandle*)Wren.GetSlotForeign(m_Ptr, slot);
            return m_SharedData.Get<T>(*ptr);
        }

        /// <summary>
        /// Gets the foreign object in <paramref name="slot"/>, interprets it as a <see cref="WrenSharedDataHandle"/>
        /// and returns the handle.
        /// </summary>
        /// <param name="slot">The slot containing the shared data handle.</param>
        /// <returns>The shared data handle stored in <paramref name="slot"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenSharedDataHandle GetSlotSharedDataHandle(int slot)
        {
            return *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Ptr, slot);
        }

        /// <summary>
        /// Gets the <see cref="WrenSharedDataHandle"/> in <paramref name="slot"/>, and sets the value it points to
        /// in the shared data table to <paramref name="value"/>.
        /// </summary>
        /// <param name="slot">The slot containing the handle.</param>
        /// <param name="value">The value to set the shared data entry to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotSharedData(int slot, object value)
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Ptr, slot);
            m_SharedData.Set(handle, value);
            return this;
        }

        /// <summary>
        /// Gets the <see cref="WrenSharedDataHandle"/> in <paramref name="slot"/>, and sets the value it points to
        /// in the shared data table to <paramref name="value"/>.
        /// </summary>
        /// <param name="slot">The slot containing the handle.</param>
        /// <param name="value">The value to set the shared data entry to.</param>
        /// <param name="handle">The handle for <paramref name="value"/>.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotSharedData(int slot, object value, out WrenSharedDataHandle handle)
        {
            handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Ptr, slot);
            m_SharedData.Set(handle, value);
            return this;
        }

        /// <summary>
        /// Creates a new instance of the foreign class stored in <paramref name="classSlot"/>, places <paramref name="handle"/>
        /// in the instance's storage and places the instance in <paramref name="slot"/>.<para />
        /// </summary>
        /// <param name="slot">The slot containing the handle.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="handle">The <see cref="WrenSharedDataHandle"/> to place in the instance's storage.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotNewSharedData(int slot, int classSlot, WrenSharedDataHandle handle)
        {
            var ptr = (WrenSharedDataHandle*)Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, (ulong)sizeof(WrenSharedDataHandle));
            *ptr = handle;
            return this;
        }

        /// <summary>
        /// Adds <paramref name="value"/> to the VM's <see cref="SharedData"/> table, creates a new instance of the foreign
        /// class stored in <paramref name="classSlot"/>, assigns the value's <see cref="WrenSharedDataHandle"/>
        /// in the instance's storage and places the instance in <paramref name="slot"/>.<para />
        /// </summary>
        /// <param name="slot">The slot containing the handle.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="value">The value to place in The VM's <see cref="SharedData"/> table.</param>
        /// <returns>The <see cref="WrenSharedDataHandle"/> that was created for <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenSharedDataHandle SetSlotNewSharedData(int slot, int classSlot, object value)
        {
            var handle = m_SharedData.Add(value);
            var ptr = (WrenSharedDataHandle*)Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, (ulong)sizeof(WrenSharedDataHandle));
            *ptr = handle;
            return handle;
        }

        /// <summary>
        /// Adds <paramref name="value"/> to the VM's <see cref="SharedData"/> table, creates a new instance of the foreign
        /// class stored in <paramref name="classSlot"/>, assigns the value's <see cref="WrenSharedDataHandle"/>
        /// in the instance's storage and places the instance in <paramref name="slot"/>.<para />
        /// </summary>
        /// <param name="slot">The slot containing the handle.</param>
        /// <param name="classSlot">The slot containing the class to instance.</param>
        /// <param name="value">The value to place in The VM's <see cref="SharedData"/> table.</param>
        /// <param name="handle">The <see cref="WrenSharedDataHandle"/> that was created for <paramref name="value"/>.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotNewSharedData(int slot, int classSlot, object value, out WrenSharedDataHandle handle)
        {
            handle = m_SharedData.Add(value);
            var ptr = (WrenSharedDataHandle*)Wren.SetSlotNewForeign(m_Ptr, slot, classSlot, (ulong)sizeof(WrenSharedDataHandle));
            *ptr = handle;
            return this;
        }

        /// <summary>
        /// Attemps to retrieve the shared data value the foreign instance stored in <paramref name="slot"/> points to.
        /// Returns true if <paramref name="slot"/> is less than the number of slots, the value it stores is of type <see cref="WrenType.Foreign"/>
        /// and the <see cref="WrenSharedDataHandle"/> stored within points to a valid entry in the shared data table.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value from the shared data table.</param>
        /// <returns>True if the slot exists, the value stored is of type <see cref="WrenType.Foreign"/> and its <see cref="WrenSharedDataHandle"/>
        /// pints to a valid entry, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryGetSlotSharedData(int slot, out object value) => TryGetSlotSharedData<object>(slot, out value);

        /// <summary>
        /// Attemps to retrieve the shared data value the foreign instance stored in <paramref name="slot"/> points to, as type <typeparamref name="T"/>.
        /// Returns true if <paramref name="slot"/> is less than the number of slots, the value it stores is of type <see cref="WrenType.Foreign"/>
        /// and the <see cref="WrenSharedDataHandle"/> stored within points to a valid entry in the shared data table.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="value">Stores the value from the shared data table.</param>
        /// <returns>True if the slot exists, the value stored is of type <see cref="WrenType.Foreign"/> and its <see cref="WrenSharedDataHandle"/>
        /// pints to a valid entry, otherwise false.</returns>
        public bool TryGetSlotSharedData<T>(int slot, out T value)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Foreign)
            {
                var ptr = (WrenSharedDataHandle*)Wren.GetSlotForeign(m_Ptr, slot);
                return m_SharedData.TryGet(*ptr, out value);
            }

            value = default;
            return false;
        }

        /// <summary>
        /// Attemps to retrieve the <see cref="WrenSharedDataHandle"/> stored by the foreign instance in <paramref name="slot"/>.
        /// Returns true if <paramref name="slot"/> is less than the number of slots, the value it stores is of type <see cref="WrenType.Foreign"/>
        /// and the <see cref="WrenSharedDataHandle"/> stored within points to a valid entry in the shared data table.
        /// </summary>
        /// <param name="slot">The slot index to read from.</param>
        /// <param name="handle">Stores the <see cref="WrenSharedDataHandle"/> held by the foreign instance.</param>
        /// <returns>True if the slot exists, the value stored is of type <see cref="WrenType.Foreign"/> and its <see cref="WrenSharedDataHandle"/>
        /// pints to a valid entry, otherwise false.</returns>
        public bool TryGetSlotSharedDataHandle(int slot, out WrenSharedDataHandle handle)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == WrenType.Foreign)
            {
                handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Ptr, slot);
                return m_SharedData.Contains(handle);
            }

            handle = default;
            return false;
        }

        #endregion

        #region Lists

        /// <summary>
        /// If the value in <paramref name="listSlot"/> is of type <see cref="WrenType.List"/>, calls the clear() method
        /// to remove all values in the list. If the value is not a list, this method does nothing.<para/>
        /// 
        /// Note that this method invokes a Wren call which will clear the API stack and the value in slot
        /// 0 will become null. See <see cref="ListLoadAndClear(WrenHandle, int)"/> for an alternative implementation that
        /// avoids this problem.
        /// </summary>
        /// <param name="listSlot">The slot the list has been placed in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        public WrenVM ListClear(int listSlot = 0)
        {
            EnsureType(Wren.GetSlotType(m_Ptr, listSlot), WrenType.List);
            CallClearMethod();
            return this;
        }

        /// <summary>
        /// Loads <paramref name="listHandle"/> into <paramref name="listSlot"/>, calls the clear() method to remove all values
        /// in the list, and places <paramref name="listHandle"/> back into <paramref name="listSlot"/>. If the handle is invalid,
        /// this method does nothing.
        /// </summary>
        /// <param name="listHandle">The handle wrapping the list.</param>
        /// <param name="listSlot">The slot to load the list into.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        public WrenVM ListLoadAndClear(WrenHandle listHandle, int listSlot = 0)
        {
            EnsureValidHandle(in listHandle);

            Wren.SetSlotHandle(m_Ptr, listSlot, listHandle.m_Ptr);
            CallClearMethod();

            // As a Wren call was invoked, the API stack is reset afterwards. Place
            // the list handle back into the original slot to make the API friendlier.
            // This wayoperations on the list can continue without having to remember
            // to restore the list handle into the slot.
            Wren.SetSlotHandle(m_Ptr, listSlot, listHandle.m_Ptr);

            return this;
        }

        /// <summary>
        /// Gets the number of elements in the list stored in <paramref name="listSlot"/>.
        /// </summary>
        /// <param name="listSlot">The slot index the list resides in.</param>
        /// <returns>The number of elements in the list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ListGetCount(int listSlot) => Wren.GetListCount(m_Ptr, listSlot);

        /// <summary>
        /// Places the value of element <paramref name="elementIndex"/> in the list stored in <paramref name="listSlot"/>
        /// into <paramref name="elementSlot"/>.<para/>
        /// Use the <c>GetSlot()</c> family of methods to retrieve the value.
        /// </summary>
        /// <param name="listSlot">The slot index the list resides in.</param>
        /// <param name="elementIndex">The index of the element.</param>
        /// <param name="elementSlot">The slot index to store the value in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM ListGetElement(int listSlot, int elementIndex, int elementSlot)
        {
            Wren.GetListElement(m_Ptr, listSlot, elementIndex, elementSlot);
            return this;
        }

        /// <summary>
        /// Adds the value stored in <paramref name="elementSlot"/> to the list stored in <paramref name="listSlot"/>.
        /// </summary>
        /// <param name="listSlot">The slot index the list resides in.</param>
        /// <param name="elementSlot">The slot index to store the value in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        public WrenVM ListAddElement(int listSlot, int elementSlot)
        {
            Wren.InsertInList(m_Ptr, listSlot, -1, elementSlot);
            return this;
        }

        /// <summary>
        /// Inserts the value stored in <paramref name="elementSlot"/> into the list stored in <paramref name="listSlot"/>
        /// at index <paramref name="elementIndex"/>.<para/>
        /// As in Wren, negative indexes can be used to insert from the end. For example, to append an element, use
        /// <c>-1</c> for <paramref name="elementIndex"/>.
        /// </summary>
        /// <param name="listSlot">The slot index the list resides in.</param>
        /// <param name="elementIndex">The index of the element. Negative values can be used insert from the end of the list.</param>
        /// <param name="elementSlot">The slot index to store the value in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM ListInsertElement(int listSlot, int elementIndex, int elementSlot)
        {
            Wren.InsertInList(m_Ptr, listSlot, elementIndex, elementSlot);
            return this;
        }

        /// <summary>
        /// Sets the value at <paramref name="elementIndex"/> in the list stored in <paramref name="listSlot"/> to the value
        /// stored in <paramref name="elementSlot"/>.
        /// </summary>
        /// <param name="listSlot">The slot index the list resides in.</param>
        /// <param name="elementIndex">The index of the element. Negative values can be used insert from the end of the list.</param>
        /// <param name="elementSlot">The slot index to store the value in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM ListSetElement(int listSlot, int elementIndex, int elementSlot)
        {
            Wren.SetListElement(m_Ptr, listSlot, elementIndex, elementSlot);
            return this;
        }

        /// <summary>
        /// Stores a new empty list in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotNewList(int slot)
        {
            Wren.SetSlotNewList(m_Ptr, slot);
            return this;
        }

        #endregion

        #region Maps

        /// <summary>
        /// If the value in <paramref name="mapSlot"/> is of type <see cref="WrenType.Map"/>, calls the clear() method
        /// to remove all values in the map. If the value is not a map, this method does nothing.<para/>
        /// 
        /// Note that this method invokes a Wren call which will clear the API stack and the value in <paramref name="mapSlot"/>
        /// will become null. See <see cref="MapLoadAndClear(WrenHandle, int)"/> for an alternative implementation that
        /// avoids this problem.
        /// </summary>
        /// <param name="mapSlot">The slot the map has been placed in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        public WrenVM MapClear(int mapSlot = 0)
        {
            EnsureType(Wren.GetSlotType(m_Ptr, mapSlot), WrenType.Map);
            CallClearMethod();
            return this;
        }

        /// <summary>
        /// Loads <paramref name="mapHandle"/> into slot 0, calls the clear() method to remove all values
        /// in the map, and places <paramref name="mapHandle"/> back into slot 0. If the handle is invalid,
        /// this method does nothing.
        /// </summary>
        /// <param name="mapHandle">The handle wrapping the map.</param>
        /// <param name="mapSlot">The slot to load the map into.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        public WrenVM MapLoadAndClear(WrenHandle mapHandle, int mapSlot = 0)
        {
            EnsureValidHandle(in mapHandle);

            Wren.SetSlotHandle(m_Ptr, mapSlot, mapHandle.m_Ptr);
            CallClearMethod();

            // As a Wren call was invoked, the API stack is reset afterwards. Place
            // the handle back into the original slot to make the API friendlier. This way
            // operations on the map can continue without having to remember to restore the handle.
            Wren.SetSlotHandle(m_Ptr, mapSlot, mapHandle.m_Ptr);

            return this;
        }

        /// <summary>
        /// Returns true if the map stored in <paramref name="mapSlot"/> contains the key in <paramref name="keySlot"/>.
        /// </summary>
        /// <param name="mapSlot">The slot index the map resides in.</param>
        /// <param name="keySlot">The slot index the key resides in.</param>
        /// <returns>True if the key is found in the map.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool MapContainsKey(int mapSlot, int keySlot) => Wren.GetMapContainsKey(m_Ptr, mapSlot, keySlot) != 0;

        /// <summary>
        /// Returns the number of entries in the map stored in <paramref name="mapSlot"/>.
        /// </summary>
        /// <param name="mapSlot">The slot index the map resides in.</param>
        /// <returns>The number of entries in the map.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int MapGetCount(int mapSlot) => Wren.GetMapCount(m_Ptr, mapSlot);

        /// <summary>
        /// Places the value with the key in <paramref name="keySlot"/> within the map stored in <paramref name="mapSlot"/>
        /// and places it into <paramref name="valueSlot"/>.
        /// </summary>
        /// <param name="mapSlot">The slot index the map resides in.</param>
        /// <param name="keySlot">The slot index the key resides in.</param>
        /// <param name="valueSlot">The slot index to place the value in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM MapGetValue(int mapSlot, int keySlot, int valueSlot)
        {
            Wren.GetMapValue(m_Ptr, mapSlot, keySlot, valueSlot);
            return this;
        }

        /// <summary>
        /// Removes the value of the key in <paramref name="keySlot"/> from the map stored in <paramref name="mapSlot"/>,
        /// and places the value in <paramref name="removedValueSlot"/>.<para/>
        /// If the key is not found in the map, <paramref name="removedValueSlot"/> is set to null.
        /// </summary>
        /// <param name="mapSlot">The slot index the map resides in.</param>
        /// <param name="keySlot">The slot index the key resides in.</param>
        /// <param name="removedValueSlot">The slot index to write the value that was removed.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM MapRemoveValue(int mapSlot, int keySlot, int removedValueSlot)
        {
            Wren.RemoveMapValue(m_Ptr, mapSlot, keySlot, removedValueSlot);
            return this;
        }

        /// <summary>
        /// Stores the value in <paramref name="valueSlot"/> in the map stored in <paramref name="mapSlot"/>
        /// with the key in <paramref name="keySlot"/>.
        /// </summary>
        /// <param name="mapSlot">The slot index the map resides in.</param>
        /// <param name="keySlot">The slot index the key resides in.</param>
        /// <param name="valueSlot">The slot index to value resides in.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM MapSetValue(int mapSlot, int keySlot, int valueSlot)
        {
            Wren.SetMapValue(m_Ptr, mapSlot, keySlot, valueSlot);
            return this;
        }

        /// <summary>
        /// Stores a new empty map in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot index to write to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenVM SetSlotNewMap(int slot)
        {
            Wren.SetSlotNewMap(m_Ptr, slot);
            return this;
        }

        #endregion
    }
}
