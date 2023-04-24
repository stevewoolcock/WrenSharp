using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A utility value used to construct a call into the Wren VM. This struct implements <see cref="IDisposable"/>,
    /// so should be dispsosed of when finished with. This ensures that if a new Wren Fiber is created for the call
    /// it is cleaned up and the previous Fiber is correctly resumed when the call completes.
    /// </summary>
    public readonly struct WrenCall : IDisposable
    {
        private readonly WrenVM m_Vm;
        private readonly WrenCallHandle m_CallHandle;
        private readonly WrenFiberResume m_FiberResume;

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

        internal WrenCall(WrenVM vm, WrenHandle receiver, WrenCallHandle callHandle, bool newFiber)
        {
            m_Vm = vm;
            m_CallHandle = callHandle;
            m_FiberResume = newFiber ? Wren.CreateFiber(m_Vm.m_Ptr) : default;

            Wren.EnsureSlots(m_Vm.m_Ptr, callHandle.m_ParamCount + 1);
            Wren.SetSlotHandle(m_Vm.m_Ptr, 0, receiver.m_Ptr);
        }

        internal WrenCall(WrenVM vm, string module, string className, WrenCallHandle callHandle, bool newFiber)
        {
            m_Vm = vm;
            m_CallHandle = callHandle;
            m_FiberResume = newFiber ? Wren.CreateFiber(m_Vm.m_Ptr) : default;

            Wren.EnsureSlots(m_Vm.m_Ptr, callHandle.m_ParamCount + 1);
            Wren.GetVariable(m_Vm.m_Ptr, module, className, 0);
        }


        /// <summary>
        /// Disposes of the call and restores the Wren API stack pointer if a new Fiber was created for this call.
        /// </summary>
        /// <seealso cref="WrenVM.CreateCall(WrenHandle, WrenCallHandle, bool)"/>
        public void Dispose()
        {
            if (m_FiberResume.IsValid)
            {
                Wren.ResumeFiber(m_Vm.m_Ptr, m_FiberResume);
            }
        }

        #region Arguments

        /// <summary>
        /// Gets the slot for the specified argument index.
        /// </summary>
        /// <param name="arg">The argument index.</param>
        /// <returns>The slot for <paramref name="arg"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ArgSlot(int arg) => arg + 1;

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

        /// <summary>
        /// Calls the Wren method.
        /// </summary>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(bool throwOnFailure = false) => m_Vm.Call(m_CallHandle, throwOnFailure);

        /// <summary>
        /// Calls the Wren method and assigns the return value to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="returnValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out bool returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? GetReturnBool() : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="returnValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out double returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? GetReturnDouble() : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="returnValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out string returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? GetReturnString() : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value to <paramref name="returnStringBytesValue"/>.
        /// </summary>
        /// <param name="returnStringBytesValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out ReadOnlySpan<byte> returnStringBytesValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnStringBytesValue = result == WrenInterpretResult.Success ? GetReturnStringBytes() : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method, creates a <see cref="WrenHandle"/> to wrap the return value and assigns the handle to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="returnValue">A <see cref="WrenHandle"/> wrapping the return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out WrenHandle returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? CreateReturnHandle() : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="returnValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out WrenSharedDataHandle returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? GetReturnSharedDataHandle() : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value type to <paramref name="returnValueType"/>.
        /// </summary>
        /// <param name="returnValueType">The type of the return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenInterpretResult Call(out WrenType returnValueType, bool throwOnFailure = false)
        {
            WrenInterpretResult result = Call(throwOnFailure);
            returnValueType = result == WrenInterpretResult.Success ? GetReturnType() : WrenType.Null;
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

        /// <summary>
        /// Returns the type of the call's return value. This is only valid after calling the method
        /// </summary>
        /// <returns>The <see cref="WrenType"/> of the return value.</returns>
        /// <seealso cref="Call(bool)"/>.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenType GetReturnType() => Wren.GetSlotType(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a <see cref="bool"/> value from the call's return slot.
        /// </summary>
        /// <returns>A <see cref="bool"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool GetReturnBool() => Wren.GetSlotBool(m_Vm.m_Ptr, 0) != 0;

        /// <summary>
        /// Returns a <see cref="double"/> value from the call's return slot.
        /// </summary>
        /// <returns>A <see cref="double"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public double GetReturnDouble() => Wren.GetSlotDouble(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a <see cref="string"/> value from the call's return slot.
        /// </summary>
        /// <returns>A <see cref="string"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string GetReturnString() => WrenInternal.GetSlotString(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a <see cref="ReadOnlySpan{T}"/> (with a generic type of <see cref="byte"/>) value from the call's return slot.
        /// </summary>
        /// <returns>A <see cref="ReadOnlySpan{T}"/> (with a generic type of <see cref="byte"/>).</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan<byte> GetReturnStringBytes() => WrenInternal.GetSlotStringBytes(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a foreign type <see cref="IntPtr"/> from the call's return slot.
        /// </summary>
        /// <returns>A <see cref="IntPtr"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetReturnForeign() => Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a foreign <typeparamref name="T"/> reference from the call's return slot.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> reference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ref T GetReturnForeign<T>() where T : unmanaged => ref *(T*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a foreign <typeparamref name="T"/> pointer from the call's return slot.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T* GetReturnForeignPtr<T>() where T : unmanaged => (T*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        /// <summary>
        /// Returns a shared data reference of <typeparamref name="T"/> from the call's return slot.
        /// </summary>
        /// <returns>A <typeparamref name="T"/> reference.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe T GetReturnSharedData<T>()
        {
            ref var handle = ref *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);
            return m_Vm.SharedData.Get<T>(handle);
        }

        /// <summary>
        /// Returns a <see cref="WrenSharedDataHandle"/> from the call's return slot.
        /// </summary>
        /// <returns>A <see cref="WrenSharedDataHandle"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe WrenSharedDataHandle GetReturnSharedDataHandle() => *(WrenSharedDataHandle*)Wren.GetSlotForeign(m_Vm.m_Ptr, 0);

        #endregion
    }
}
