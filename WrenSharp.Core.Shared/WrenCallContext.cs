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
        private readonly WrenVM m_Vm;
        private readonly WrenMethodType m_Type;
        private readonly byte m_ArgCount;

        #region Properties

        /// <summary>
        /// Gets the <see cref="WrenVM"/> the call was made from.
        /// </summary>
        public WrenVM VM => m_Vm;

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
        public WrenType ReceiverType => Wren.GetSlotType(m_Vm.m_Ptr, 0);

        #endregion

        internal WrenCallContext(WrenVM vm, WrenMethodType type, byte argCount)
        {
            m_Vm = vm;
            m_Type = type;
            m_ArgCount = argCount;
        }

        /// <summary>
        /// Returns the slot index for an argument index.
        /// </summary>
        /// <param name="arg">The argument index</param>
        /// <returns>The slot index for <paramref name="arg"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgSlot(int arg) => arg + 1;

        /// <summary>
        /// Gets a pointer to the data for the foreign receiver of the method call. This only applies to calls where the receiver is a
        /// foreign class or and instance of a foreign class.
        /// </summary>
        /// <typeparam name="T">The data type allocated for the foreign class.</typeparam>
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle GetReceiverSharedDataHandle() => *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetReceiverSharedData<T>()
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
            return m_Vm.SharedData.Get<T>(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe bool TryGetReceiverSharedData<T>(out T value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, 0) == WrenType.Foreign)
            {
                var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
                return m_Vm.SharedData.TryGet(handle, out value);
            }

            value = default;
            return false;
        }

        #region Arguments

        /// <summary>
        /// Gets the value of a <see cref="bool"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetArgBool(int arg) => Wren.GetSlotBool(m_Vm.m_Ptr, ArgSlot(arg)) != 0;

        /// <summary>
        /// Gets the value of a <see cref="double"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetArgDouble(int arg) => Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Gets a pointer to the data of a foreign object argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetArgForeign(int arg) => Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Gets the data of a foreign object argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetArgForeign<T>(int arg) where T : unmanaged => ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Creates a handle wrapping the value of the argument at index <paramref name="arg"/>.<para />
        /// The handle should be released when it is no longer needed.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle GetArgHandle(int arg) => m_Vm.CreateHandle(ArgSlot(arg));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetArgSharedData<T>(int arg)
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
            return m_Vm.SharedData.Get<T>(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle GetArgSharedDataHandle(int arg) => *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Gets the value of a <see cref="string"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetArgString(int arg) => WrenInternal.GetSlotString(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Gets the raw bytes value of a <see cref="string"/> argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetArgStringBytes(int arg) => WrenInternal.GetSlotStringBytes(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Gets the <see cref="WrenType"/> of the argument at index <paramref name="arg"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The value of the argument.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenType GetArgType(int arg) => Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg));

        public bool TryGetArg(int arg, out bool value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.Bool)
            {
                value = Wren.GetSlotBool(m_Vm.m_Ptr, ArgSlot(arg)) != 0;
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetArg(int arg, out double value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.Number)
            {
                value = Wren.GetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetArg(int arg, out string value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.String)
            {
                value = WrenInternal.GetSlotString(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetArg(int arg, out ReadOnlySpan<byte> value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.String)
            {
                value = WrenInternal.GetSlotStringBytes(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public bool TryGetArgForeign(int arg, out IntPtr value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.Foreign)
            {
                value = Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return true;
            }

            value = default;
            return false;
        }

        public unsafe bool TryGetArgSharedData<T>(int arg, out T value)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.Foreign)
            {
                var handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return m_Vm.SharedData.TryGet(handle, out value);
            }

            value = default;
            return false;
        }

        public unsafe bool TryGetArgSharedDataHandle(int arg, out WrenSharedDataHandle handle)
        {
            if (Wren.GetSlotType(m_Vm.m_Ptr, ArgSlot(arg)) == WrenType.Foreign)
            {
                handle = *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, ArgSlot(arg));
                return handle.IsValid;
            }

            handle = default;
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
        public void Return(WrenHandle value)
        {
            m_Vm.EnsureValidHandle(in value);
            Wren.SetSlotHandle(m_Vm.m_Ptr, 0, value.m_Ptr);
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
    }
}
