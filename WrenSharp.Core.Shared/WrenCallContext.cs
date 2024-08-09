using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// Specifies the type of a Wren foriegn method binding.
    /// </summary>
    public enum WrenMethodType : byte
    {
        /// <summary>
        /// A foreign instance method.
        /// </summary>
        Instance,

        /// <summary>
        /// A foreign static method.
        /// </summary>
        Static,

        /// <summary>
        /// A foreign class allocator method.
        /// </summary>
        Allocator,
    }

    /// <summary>
    /// A helper that is passed into foreign method calls in managed code. When a foreign method is called from within Wren,
    /// the delegate assigned to that method on the managed side will be invoked and passed a <see cref="WrenCallContext"/>.
    /// This can be used to intuitively retrieve the value of arguments, set return values and access information about the receiver.
    /// </summary>
    public readonly struct WrenCallContext
    {
        internal readonly WrenVM m_Vm;
        internal readonly WrenMethodType m_Type;
        internal readonly byte m_ArgCount;

        #region Properties

        /// <summary>
        /// Gets the <see cref="WrenVM"/> the call was made from.
        /// </summary>
        public WrenVM VM
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Vm;
        }

        /// <summary>
        /// The number of arugments supplied to the call.<para/>
        /// Note that if the method is an allocator (<see cref="IsAllocator"/> and <see cref="MethodType"/>),
        /// this value will be <see cref="byte.MaxValue"/>.
        /// </summary>
        public int ArgCount => m_ArgCount;

        /// <summary>
        /// Gets the method type of this call.
        /// </summary>
        public WrenMethodType MethodType => m_Type;

        /// <summary>
        /// Indicates if this call is a foreign class allocator.
        /// </summary>
        public bool IsAllocator => m_Type == WrenMethodType.Allocator;

        /// <summary>
        /// Indicates if this call is a foreign static method.
        /// </summary>
        public bool IsStatic => m_Type == WrenMethodType.Static;

        /// <summary>
        /// Gets the <see cref="WrenType"/> of the receiver object of the method call.
        /// </summary>
        /// <returns>The type of the receiver object.</returns>
        public WrenType ReceiverType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Wren.GetSlotType(m_Vm.m_Ptr, 0);
        }

        #endregion

        internal WrenCallContext(WrenVM vm, WrenMethodType type, byte argCount)
        {
            m_Vm = vm;
            m_Type = type;
            m_ArgCount = argCount;
        }

        #region Receiver

        /// <summary>
        /// Gets a pointer to the data for the foreign receiver of the method call. This only applies to calls where the receiver is a
        /// foreign class or and instance of a foreign class.
        /// </summary>
        /// <returns>The data allocated for receiver (a foreign class instance).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetReceiverForeign() => Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Gets the data for the foreign receiver of the method call. This only applies to calls where the receiver is a
        /// foreign class or and instance of a foreign class.
        /// </summary>
        /// <typeparam name="T">The data type allocated for the foreign class.</typeparam>
        /// <returns>The data allocated for receiver (a foreign class instance).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetReceiverForeign<T>() where T : unmanaged => ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Gets the <see cref="WrenSharedDataHandle"/> foreign receiver of the method call.
        /// </summary>
        /// <returns>The <see cref="WrenSharedDataHandle"> for receiver.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle GetReceiverSharedDataHandle() => *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Gets the shared data value for the foreign receiver of the method call.
        /// </summary>
        /// <typeparam name="T">The data type allocated for the foreign class.</typeparam>
        /// <returns>The data allocated for receiver (a foreign class instance).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetReceiverSharedData<T>()
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
            return m_Vm.SharedData.Get<T>(handle);
        }

        public unsafe bool TryGetReceiverSharedData<T>(out T value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, 0) == WrenType.Foreign)
            {
                var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
                return m_Vm.SharedData.TryGet(handle, out value);
            }

            value = default!;
            return false;
        }

        public unsafe bool TryGetReceiverSharedData(out WrenSharedDataHandle handle)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, 0) == WrenType.Foreign)
            {
                handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
                return handle.IsValid;
            }

            handle = default!;
            return false;
        }

        #endregion

        #region Arguments

        /// <summary>
        /// Returns the slot index for an argument index.
        /// </summary>
        /// <param name="arg">The argument index</param>
        /// <returns>The slot index for <paramref name="arg"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgSlot(int arg) => arg + 1;

        /// <summary>
        /// Compares the <see cref="WrenType"/> of the argument at index <paramref name="arg"/> to <paramref name="type"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="type">The type to check the value at <paramref name="arg"/> against.</param>
        /// <returns>True if the argument is of the specified type.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool ArgIsType(int arg, WrenType type) => ArgIndexInRange(arg) && Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == type;

        /// <summary>
        /// Creates a handle wrapping the value of the argument at index <paramref name="arg"/>.<para />
        /// The handle should be released when it is no longer needed.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle CreateArgHandle(int arg) => ArgIndexInRange(arg) ? m_Vm.CreateHandle(ArgSlot(arg)) : default;

        /// <summary>
        /// Return true if argument at index <paramref name="arg"/> is a null value.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>True if the argument is a null value, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetArgNull(int arg) => ArgIsType(arg, WrenType.Null);

        /// <summary>
        /// Gets the value of a <see cref="bool"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetArgBool(int arg) => ArgIndexInRange(arg) ? Wren.GetSlotBool(m_Vm.m_Ptr, ArgSlot(arg)) != 0 : default;


        /// <summary>
        /// Gets the value of a <see cref="double"/> argument at index <paramref name="arg"/> and casts it to a <see cref="float"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float GetArgFloat(int arg) => ArgIndexInRange(arg) ? (float)Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg)) : default;

        /// <summary>
        /// Gets the value of a <see cref="double"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetArgDouble(int arg) => ArgIndexInRange(arg) ? Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg)) : default;

        /// <summary>
        /// Gets the value of a <see cref="byte"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte GetArgInt8(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsInt8(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="sbyte"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public sbyte GetArgUInt8(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsUInt8(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="short"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short GetArgInt16(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsInt16(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="ushort"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort GetArgUInt16(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsUInt16(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="int"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int GetArgInt32(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsInt32(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="uint"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint GetArgUInt32(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsUInt32(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="long"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public long GetArgInt64(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsInt64(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;

        /// <summary>
        /// Gets the value of a <see cref="ulong"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        /// <exception cref="ArgumentException">Thrown if the value is not an a valid intergral.</exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ulong GetArgUInt64(int arg) => ArgIndexInRange(arg) ? WrenUtils.AsUInt64(Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg))) : default;


        /// <summary>
        /// Gets a pointer to the data of a foreign object argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetArgForeign(int arg) => ArgIndexInRange(arg) ? Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg)) : default;

        /// <summary>
        /// Gets the data of a foreign object argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetArgForeign<T>(int arg) where T : unmanaged
        {
            EnsureArgIndexInRange(arg);
            return ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetArgSharedData<T>(int arg)
        {
            if (!ArgIndexInRange(arg))
                return default!;

            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
            return m_Vm.SharedData.Get<T>(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle GetArgSharedDataHandle(int arg) => ArgIndexInRange(arg) ? *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg)) : default;

        /// <summary>
        /// Gets the value of a <see cref="string"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetArgString(int arg) => ArgIndexInRange(arg) ? WrenInternal.GetSlotString(m_Vm.m_Ptr, ArgSlot(arg)) : default!;

        /// <summary>
        /// Gets the raw bytes value of a <see cref="string"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetArgStringBytes(int arg) => ArgIndexInRange(arg) ? WrenInternal.GetSlotStringBytes(m_Vm.m_Ptr, ArgSlot(arg)) : default;

        /// <summary>
        /// Gets the <see cref="WrenType"/> of the argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The type of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenType GetArgType(int arg) => ArgIndexInRange(arg) ? Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) : WrenType.Null;

        public bool TryGetArg(int arg, out bool value)
        {
            if (ArgIsType(arg, WrenType.Bool))
            {
                value = Wren.GetSlotBool(m_Vm.m_Ptr, ArgSlot(arg)) != 0;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetArg(int arg, out double value)
        {
            if (ArgIsType(arg, WrenType.Number))
            {
                value = Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetArg(int arg, out string value)
        {
            if (ArgIsType(arg, WrenType.String))
            {
                value = WrenInternal.GetSlotString(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default!;
            return false;
        }

        public bool TryGetArgStringBytes(int arg, out ReadOnlySpan<byte> value)
        {
            if (ArgIsType(arg, WrenType.String))
            {
                value = WrenInternal.GetSlotStringBytes(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default!;
            return false;
        }

        public bool TryGetArgForeign(int arg, out IntPtr value)
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                value = Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public unsafe bool TryGetArgForeign(int arg, int length, out ReadOnlySpan<byte> value)
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                value = new ReadOnlySpan<byte>((void*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg)), length);
                return true;
            }

            value = default;
            return false;
        }

        public unsafe bool TryGetArgForeign<T>(int arg, out T value) where T : unmanaged
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                value = *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public unsafe bool TryGetArgForeign<T>(int arg, out T* value) where T : unmanaged
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                value = (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public unsafe bool TryGetArgSharedData<T>(int arg, out T value)
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return m_Vm.SharedData.TryGet(handle, out value);
            }

            value = default!;
            return false;
        }

        public unsafe bool TryGetArgSharedData(int arg, out object value)
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return m_Vm.SharedData.TryGet(handle, out value);
            }

            value = default!;
            return false;
        }

        public unsafe bool TryGetArgSharedData(int arg, out WrenSharedDataHandle handle)
        {
            if (ArgIsType(arg, WrenType.Foreign))
            {
                handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return handle.IsValid;
            }

            handle = default!;
            return false;
        }

        #endregion

        #region Return Values

        /// <summary>
        /// Sets a <see cref="bool"/> value as the return value for the method call.
        /// </summary>
        /// <param name="value">The value to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(bool value) => Wren.SetSlotBool(m_Vm.m_Ptr, 0, value);

        /// <summary>
        /// Sets a <see cref="double"/> value as the return value for the method call.
        /// </summary>
        /// <param name="value">The value to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(double value) => Wren.SetSlotDouble(m_Vm.m_Ptr, 0, value);

        /// <summary>
        /// Sets a <see cref="string"/> value as the return value for the method call.
        /// </summary>
        /// <param name="value">The value to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(string value) => Wren.SetSlotString(m_Vm.m_Ptr, 0, value);

        /// <summary>
        /// Sets a series of raw bytes representing a <see cref="string"/> as the return value for the method call.
        /// </summary>
        /// <param name="value">The value to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(ReadOnlySpan<byte> value) => WrenInternal.SetSlotString(m_Vm.m_Ptr, 0, value);

        /// <summary>
        /// Sets a <see cref="WrenHandle"/> value as the return value for the method call.
        /// </summary>
        /// <param name="value">The value to return.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(in WrenHandle value)
        {
            m_Vm.EnsureValidHandle(in value);
            Wren.SetSlotHandle(m_Vm.m_Ptr, 0, value.m_Ptr);
        }

        /// <summary>
        /// Sets a null value as the return value for the method call.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReturnNull() => Wren.SetSlotNull(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Sets a new instance of a foreign class as the return value for the method call.
        /// </summary>
        /// <param name="classHandle">A handle to the class being instantiated.</param>
        /// <param name="size">The number of bytes to allocate for the instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ReturnForeign(in WrenHandle classHandle, ulong size)
        {
            m_Vm.EnsureValidHandle(in classHandle);
            m_Vm.SetSlot(0, in classHandle);
            return Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, size);
        }

        /// <summary>
        /// Sets a new instance of a foreign class as the return value for the method call.
        /// </summary>
        /// <param name="module">The name of the module the class resides in.</param>
        /// <param name="className">The name of the class to instantiate.</param>
        /// <param name="size">The number of bytes to allocate for the instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr ReturnForeign(string module, string className, ulong size)
        {
            m_Vm.GetVariable(module, className, 0);
            return Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, size);
        }

        /// <summary>
        /// Sets a new instance of a foreign class as the return value for the method call.
        /// </summary>
        /// <typeparam name="T">The foreign class' data type.</typeparam>
        /// <param name="classHandle">A handle to the class being instantiated.</param>
        /// <param name="value">The value of the new instance.</param>
        public unsafe ref T ReturnForeign<T>(in WrenHandle classHandle, in T value = default) where T : unmanaged
        {
            m_Vm.EnsureValidHandle(in classHandle);
            m_Vm.SetSlot(0, in classHandle);
            var ptr = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
            *ptr = value;
            return ref *ptr;
        }

        /// <summary>
        /// Sets a new instance of a foreign class as the return value for the method call.
        /// </summary>
        /// <typeparam name="T">The foreign class' data type.</typeparam>
        /// <param name="module">The name of the module the class resides in.</param>
        /// <param name="className">The name of the class to instantiate.</param>
        /// <param name="value">The value of the new instance.</param>
        public unsafe ref T ReturnForeign<T>(string module, string className, in T value = default) where T : unmanaged
        {
            m_Vm.GetVariable(module, className, 0);
            var ptr = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
            *ptr = value;
            return ref *ptr;
        }

        /// <summary>
        /// Sets a new shared data reference as the return value for the method call. <paramref name="value"/> will be added
        /// to the VM's shared data table and the handle to it is returned.
        /// </summary>
        /// <param name="value">The value to add to the shared data table and return the handle of.</param>
        /// <returns>The new <see cref="WrenSharedDataHandle"/> for <paramref name="value"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle ReturnSharedData(object value)
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
            m_Vm.SharedData.Set(handle, value);
            return handle;
        }

        /// <summary>
        /// Sets a shared data reference as the return value for the method call.
        /// </summary>
        /// <param name="handle">A handle to the shared data value.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe void ReturnSharedData(in WrenSharedDataHandle handle)
        {
            m_Vm.EnsureValidHandle(in handle);
            var ptr = (WrenSharedDataHandle*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(WrenSharedDataHandle));
            *ptr = handle;
        }

        #endregion

        #region Abort

        /// <summary>
        /// Abort the fibre with an error.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort()
        {
            Wren.SetSlotNull(m_Vm.m_Ptr, 0);
            Wren.AbortFiber(m_Vm.m_Ptr, 0);
        }

        /// <summary>
        /// Abort the fibre with an error.
        /// </summary>
        /// <param name="error">The error code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort(int error)
        {
            Wren.SetSlotDouble(m_Vm.m_Ptr, 0, error);
            Wren.AbortFiber(m_Vm.m_Ptr, 0);
        }

        /// <summary>
        /// Abort the fibre with an error.
        /// </summary>
        /// <param name="error">The error code.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort(double error)
        {
            Wren.SetSlotDouble(m_Vm.m_Ptr, 0, error);
            Wren.AbortFiber(m_Vm.m_Ptr, 0);
        }

        /// <summary>
        /// Abort the fibre with an error.
        /// </summary>
        /// <param name="error">The error message.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort(string error)
        {
            Wren.SetSlotString(m_Vm.m_Ptr, 0, error);
            Wren.AbortFiber(m_Vm.m_Ptr, 0);
        }

        /// <summary>
        /// Abort the fibre with an error.
        /// </summary>
        /// <param name="error">The error object handle.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Abort(WrenHandle error)
        {
            Wren.SetSlotHandle(m_Vm.m_Ptr, 0, error.m_Ptr);
            Wren.AbortFiber(m_Vm.m_Ptr, 0);
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool ArgIndexInRange(int arg) => arg >= 0 && arg < m_ArgCount;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void EnsureArgIndexInRange(int arg)
        {
            if (arg < 0 || arg >= m_ArgCount)
                throw new ArgumentOutOfRangeException(nameof(arg));
        }
    }
}
