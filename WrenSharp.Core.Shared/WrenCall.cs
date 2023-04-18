using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A utility value used to construct a call into the Wren VM.
    /// </summary>
    public readonly struct WrenCall
    {
        private readonly WrenVM m_Vm;
        private readonly WrenCallHandle m_CallHandle;

        #region Properties

        /// <summary>
        /// The VM the call was created with.
        /// </summary>
        public WrenVM VM => m_Vm;
        
        /// <summary>
        /// The number of parameters required by the call.
        /// </summary>
        public int ParamCount => m_CallHandle.ParamCount;

        #endregion

        internal WrenCall(WrenVM vm, WrenHandle receiver, WrenCallHandle callHandle)
        {
            m_Vm = vm;
            m_CallHandle = callHandle;
            Wren.EnsureSlots(m_Vm.m_Ptr, callHandle.m_ParamCount + 1);
            Wren.SetSlotHandle(m_Vm.m_Ptr, 0, receiver.m_Ptr);
        }

        /// <summary>
        /// Gets the slot for the specified argument index.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The slot for <paramref name="arg"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgSlot(int arg) => arg + 1;

        #region Arguments

        /// <summary>
        /// Sets the value of argument <paramref name="arg"/> to a boolean.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArg(int arg, bool value) => Wren.SetSlotBool(m_Vm.m_Ptr, ArgSlot(arg), value);

        /// <summary>
        /// Sets the value of argument <paramref name="arg"/> to a double.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArg(int arg, double value) => Wren.SetSlotDouble(m_Vm.m_Ptr, ArgSlot(arg), value);

        /// <summary>
        /// Sets the value of argument <paramref name="arg"/> to a string.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArg(int arg, string value) => Wren.SetSlotString(m_Vm.m_Ptr, ArgSlot(arg), value);

        /// <summary>
        /// Sets the value of argument <paramref name="arg"/> to a string, represented by <paramref name="stringBytes"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="stringBytes">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArg(int arg, ReadOnlySpan<byte> stringBytes) => WrenInternal.SetSlotString(m_Vm.m_Ptr, ArgSlot(arg), stringBytes);

        /// <summary>
        /// Sets the value in argument slot <paramref name="arg"/> to <paramref name="handle"/>.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="handle">The handle to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArg(int arg, WrenHandle handle)
        {
            m_Vm.EnsureValidHandle(in handle);
            Wren.SetSlotHandle(m_Vm.m_Ptr, ArgSlot(arg), handle.m_Ptr);
        }

        /// <summary>
        /// Sets the value in argument slot <paramref name="arg"/> to null.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetArgNull(int arg) => Wren.SetSlotNull(m_Vm.m_Ptr, ArgSlot(arg));

        /// <summary>
        /// Sets the value in argument slot <paramref name="arg"/> to a new foreign class instance.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="classSlot">The slot index containing the class to instantiate.</param>
        /// <param name="size">The number of bytes to allocate for the instance.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr SetArgNewForeign(int arg, int classSlot, ulong size) => Wren.SetSlotNewForeign(m_Vm.m_Ptr, ArgSlot(arg), classSlot, size);

        /// <summary>
        /// Sets the value in argument slot <paramref name="arg"/> to a new foreign class instance.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="classSlot">The slot index containing the class to instantiate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T SetArgNewForeign<T>(int arg, int classSlot) where T : unmanaged => ref *(T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, ArgSlot(arg), classSlot, (ulong)sizeof(T));

        /// <summary>
        /// Sets the value in argument slot <paramref name="arg"/> to a new foreign class instance.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="classSlot">The slot index containing the class to instantiate.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T* SetArgNewForeignPtr<T>(int arg, int classSlot) where T : unmanaged => (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, ArgSlot(arg), classSlot, (ulong)sizeof(T));

        /// <summary>
        /// Sets the value in argument slot <paramref name="arg"/> to a new foreign class with a shared data reference.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <param name="classSlot">The slot index containing the class to instantiate.</param>
        /// <param name="data">The shared data to reference.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle SetArgNewSharedData(int arg, int classSlot, object data) => m_Vm.SetSlotNewSharedData(ArgSlot(arg), classSlot, data);

        #endregion

        #region Call

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(bool throwOnFailure = false)
        {
            return m_Vm.Call(m_CallHandle, throwOnFailure);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out bool returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnValue = GetReturnBool();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out double returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnValue = GetReturnDouble();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out string returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnValue = GetReturnString();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out ReadOnlySpan<byte> returnStringBytesValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnStringBytesValue = GetReturnStringBytes();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out WrenHandle returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnValue = CreateReturnHandle();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out WrenSharedDataHandle returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnValue = GetReturnSharedDataHandle();
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out WrenType returnValueType, bool throwOnFailure = false)
        {
            WrenInterpretResult result = m_Vm.Call(m_CallHandle, throwOnFailure);
            returnValueType = GetReturnType();
            return result;
        }

        #endregion

        #region Return Values

        /// <summary>
        /// Creates a new <see cref="WrenHandle"/> wrapping the call's return value. The handle should be released
        /// when it is no longer required.
        /// </summary>
        /// <returns>A <see cref="WrenHandle"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle CreateReturnHandle() => m_Vm.CreateHandle(0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenType GetReturnType() => Wren.GetSlotType(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetReturnBool() => Wren.GetSlotBool(m_Vm.m_Ptr, 0) != 0;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetReturnDouble() => Wren.GetSlotDouble(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetReturnString() => WrenInternal.GetSlotString(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetReturnStringBytes() => WrenInternal.GetSlotStringBytes(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetReturnForeign() => Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetReturnForeign<T>() where T : unmanaged => ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T* GetReturnForeignPtr<T>() where T : unmanaged => (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetReturnSharedData<T>()
        {
            ref var handle = ref *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
            return m_Vm.SharedData.Get<T>(handle);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle GetReturnSharedDataHandle() => *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        #endregion
    }
}
