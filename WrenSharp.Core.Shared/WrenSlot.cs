using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A wrapper for a Wren API slot.
    /// </summary>
    public readonly partial struct WrenSlot : IEquatable<WrenSlot>
    {
        private readonly WrenVM m_Vm;
        private readonly int m_Index;

        #region Properties

        /// <summary>
        /// Gets the slot index.
        /// </summary>
        public int Index => m_Index;

        /// <summary>
        /// Gets the type of value currently stored in the slot.
        /// </summary>
        public WrenType Type
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Wren.GetSlotType(m_Vm.m_Ptr, m_Index);
        }
        
        /// <summary>
        /// Gets or sets a boolean value in the slot.
        /// </summary>
        public bool Bool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Wren.GetSlotBool(m_Vm.m_Ptr, m_Index) != 0;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotBool(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a byte value in the slot.
        /// </summary>
        public byte Byte
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (byte)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Int16"/> value in the slot.
        /// </summary>
        public short Int16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (short)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Int32"/> value in the slot.
        /// </summary>
        public int Int32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="System.Int64"/> value in the slot.
        /// </summary>
        public long Int64
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (long)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="System.UInt16"/> value in the slot.
        /// </summary>
        public ushort UInt16
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ushort)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="System.UInt32"/> value in the slot.
        /// </summary>
        public uint UInt32
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (uint)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a <see cref="System.UInt64"/> value in the slot.
        /// </summary>
        public ulong UInt64
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (ulong)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a float value in the slot.
        /// </summary>
        public float Float
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (float)Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a double value in the slot.
        /// </summary>
        public double Double
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Wren.GetSlotDouble(m_Vm.m_Ptr, m_Index);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Gets or sets a string value in the slot.
        /// </summary>
        public string String
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type == WrenType.String ? WrenInternal.GetSlotString(m_Vm.m_Ptr, m_Index) : default;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetSlotString(m_Vm.m_Ptr, m_Index, value);
        }

        /// <summary>
        /// Indicates if the slot contains a null value.
        /// </summary>
        public bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type == WrenType.Null;
        }

        /// <summary>
        /// Indicates if the value in the slot is a valid integer value.
        /// </summary>
        /// <seealso cref="WrenUtils.IsInteger(double)"/>
        public bool IsInteger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Type == WrenType.Number && WrenUtils.IsInteger(Double);
        }

        /// <summary>
        /// Indicates if this value represents a valid Wren API slot. Returns true if the VM is initialized
        /// and the slot index is less than the total number of currently allocated slots.
        /// </summary>
        public bool IsValid => m_Vm.IsInitialized && !m_Vm.IsDisposed && m_Index < Wren.GetSlotCount(m_Vm.m_Ptr);

        #endregion

        internal WrenSlot(WrenVM vm, int slot)
        {
            m_Vm = vm;
            m_Index = slot;
        }

        #region Object

        /// <inheritdoc/>
        public bool Equals(WrenSlot other) => other.Index == m_Index && other.m_Vm == m_Vm;

        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is WrenSlot slot && Equals(slot);

        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(m_Index, m_Vm);

        #endregion

        /// <summary>
        /// Copies the value in this slot to <paramref name="slot"/>. If <paramref name="slot"/> is equal to <see cref="Index"/>,
        /// this method does nothing.
        /// </summary>
        /// <param name="slot">The slot to copy the value into.</param>
        public void CopyTo(int slot) => m_Vm.Slots.Copy(m_Index, slot);

        /// <summary>
        /// Creates a <see cref="WrenHandle"/> wrapping the value in the slot.
        /// </summary>
        /// <returns>A <see cref="WrenHandle"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle CreateHandle() => m_Vm.CreateHandle(m_Index);

        /// <summary>
        /// Returns true if <paramref name="type"/> is equal to <see cref="Type"/>.
        /// </summary>
        /// <param name="type">The type to compare the value in the slot against.</param>
        /// <returns>True if <paramref name="type"/> is equal to <see cref="Type"/>, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsType(WrenType type) => Type == type;

        /// <summary>
        /// Ensures the <see cref="WrenType"/> of the value in the slot is equal to <paramref name="type"/> and throws
        /// a <see cref="WrenTypeException"/> if they are not equal.
        /// </summary>
        /// <param name="type">The type compare the value in the slot against.</param>
        /// <exception cref="WrenTypeException">Thrown if <paramref name="type"/> is not equal to <see cref="Type"/>.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void EnsureTypeOrThrow(WrenType type)
        {
            if (Type != type)
                throw new WrenTypeException(m_Vm, $"Expected {type} in slot[{m_Index}], but found {Type}");
        }

        /// <summary>
        /// Ensures the <see cref="WrenType"/> of the value in the slot is equal to <paramref name="type"/> and aborts
        /// the fiber if they are not equal. The resulting error message is placed in slot <paramref name="errorSlot"/>.
        /// </summary>
        /// <param name="type">The type compare the value in the slot against.</param>
        /// <param name="errorSlot">The slot to place the error message in.</param>
        /// <returns>True if <paramref name="type"/> is equal to <see cref="Type"/>, otherwise false.</returns>
        public bool EnsureTypeOrAbort(WrenType type, int errorSlot = 0)
        {
            if (Type != type)
            {
                m_Vm.AbortFiber(errorSlot, $"Expected {type} in slot[{m_Index}], but found {Type}");
                return false;
            }

            return true;
        }

        #region Getters

        /// <summary>
        /// Gets a new <see cref="WrenList"/> wrapper for the Wren list stored in the slot.
        /// </summary>
        /// <param name="defaultElementSlot">The default element slot for the returned <see cref="WrenList"/>.</param>
        /// <returns>A <see cref="WrenList"/>.</returns>
        public WrenList GetList(int? defaultElementSlot = default) => new WrenList(m_Vm, m_Index, defaultElementSlot);

        /// <summary>
        /// Gets a new <see cref="WrenMap"/> wrapper for the Wren map stored in the slot.
        /// </summary>
        /// <param name="defaultKeySlot">The default key slot for the returned <see cref="WrenMap"/>.</param>
        /// <param name="defaultValueSlot">The default value slot for the returned <see cref="WrenMap"/>.</param>
        /// <returns>A <see cref="WrenMap"/>.</returns>
        public WrenMap GetMap(int? defaultKeySlot = default, int? defaultValueSlot = default) => new WrenMap(m_Vm, m_Index, defaultKeySlot, defaultValueSlot);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetForeign() => Wren.GetSlotForeign(m_Vm.m_Ptr, m_Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetForeign<T>() where T : unmanaged => ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T* GetForeignPtr<T>() where T : unmanaged => (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, m_Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetSharedData() => m_Vm.GetSlotSharedData(m_Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetSharedData<T>() => m_Vm.GetSlotSharedData<T>(m_Index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenSharedDataHandle GetSharedDataHandle() => m_Vm.GetSlotSharedDataHandle(m_Index);

        #endregion

        #region Setters

        /// <summary>
        /// Sets the value in this slot to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(bool value) => Wren.SetSlotBool(m_Vm.m_Ptr, m_Index, value);

        /// <summary>
        /// Sets the value in this slot to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double value) => Wren.SetSlotDouble(m_Vm.m_Ptr, m_Index, value);

        /// <summary>
        /// Sets the value in this slot to <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(string value) => Wren.SetSlotString(m_Vm.m_Ptr, m_Index, value);

        /// <summary>
        /// Sets the value in this slot to null.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNull() => Wren.SetSlotNull(m_Vm.m_Ptr, m_Index);

        /// <summary>
        /// Creates a new WrenList in this slot. Returns a <see cref="WrenList"/> wrapper with its default element slot
        /// set to <paramref name="defaultElementSlot"/>. If <paramref name="defaultElementSlot"/> is null, the default <see cref="WrenList"/>
        /// behaviour is used to select a default element slot. (list slot + 1).
        /// </summary>
        /// <param name="defaultElementSlot">The default element slot of the returned <see cref="WrenList"/>.</param>
        /// <returns>A <see cref="WrenList"/> wrapper for the newly created list.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenList SetNewList(int? defaultElementSlot = default)
        {
            Wren.SetSlotNewList(m_Vm.m_Ptr, m_Index);
            return new WrenList(m_Vm, m_Index, defaultElementSlot);
        }

        /// <summary>
        /// Creates a new WrenMap in this slot. Returns a <see cref="WrenMap"/> wrapper with its default key and value slots
        /// set to <paramref name="defaultKeySlot"/> and <paramref name="defaultValueSlot"/>, respectively. If a slot parameter is null,
        /// the default <see cref="WrenMap"/> behaviour is used to select a value for the the slot (mapSlot + 1 for key slot, and mapSlot + 2 for value slot).
        /// </summary>
        /// <param name="defaultKeySlot">The default key slot of the returned <see cref="WrenMap"/>.</param>
        /// <param name="defaultValueSlot">The default value slot of the returned <see cref="WrenMap"/>.</param>
        /// <returns>A <see cref="WrenMap"/> wrapper for the newly created map.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenMap SetNewMap(int? defaultKeySlot = default, int? defaultValueSlot = default)
        {
            Wren.SetSlotNewMap(m_Vm.m_Ptr, m_Index);
            return new WrenMap(m_Vm, m_Index, defaultKeySlot, defaultValueSlot);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr SetNewForeign(int classSlot, ulong size) => Wren.SetSlotNewForeign(m_Vm.m_Ptr, m_Index, classSlot, size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T SetNewForeign<T>(int classSlot, in T value = default) where T : unmanaged => ref m_Vm.SetSlotNewForeign(m_Index, classSlot, in value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T* SetNewForeignPtr<T>(int classSlot, in T value = default) where T : unmanaged => m_Vm.SetSlotNewForeignPtr(m_Index, classSlot, in value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte* SetNewForeignPtr<T>(int classSlot, in ReadOnlySpan<byte> value) where T : unmanaged => m_Vm.SetSlotNewForeignPtr(m_Index, classSlot, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenSharedDataHandle SetNewSharedData<T>(int classSlot, object value) => m_Vm.SetSlotNewSharedData(m_Index, classSlot, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNewSharedData<T>(int classSlot, WrenSharedDataHandle handle) => m_Vm.SetSlotNewSharedData(m_Index, classSlot, handle);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetNewSharedData<T>(int classSlot, object value, out WrenSharedDataHandle handle) => m_Vm.SetSlotNewSharedData(m_Index, classSlot, value, out handle);

        #endregion

        #region Operators

        public static bool operator ==(WrenSlot left, WrenSlot right) => left.Equals(right);
        public static bool operator !=(WrenSlot left, WrenSlot right) => !(left == right);

        public static implicit operator int(WrenSlot slot) => slot.Index;

        #endregion
    }
}
