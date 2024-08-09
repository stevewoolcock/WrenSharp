#if WRENSHARP_EXT
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp.Unsafe
{
    /// <summary>
    /// Unsafe extension methods.
    /// </summary>
    public unsafe static class WrenUnsafeExtensions
    {
        #region WrenCall

        /// <summary>
        /// Sets the value of argument <paramref name="arg"/> to a <see cref="WrenValue"/>.
        /// </summary>
        /// <param name="call">The <see cref="WrenCall"/>.</param>
        /// <param name="arg">The argument index.</param>
        /// <param name="value">The value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetArg(this ref WrenCall call, int arg, in WrenValue value)
        {
            Wren.SetSlot(call.m_Vm.m_Ptr, WrenCall.ArgSlot(arg), value);
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value type to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="call">The <see cref="WrenCall"/>.</param>
        /// <param name="returnValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WrenInterpretResult Call(this ref WrenCall call, out WrenValue returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = call.Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? *Wren.GetSlotPtr(call.m_Vm.m_Ptr, 0) : default;
            return result;
        }

        /// <summary>
        /// Calls the Wren method and assigns the return value type to <paramref name="returnValue"/>.
        /// </summary>
        /// <param name="call">The <see cref="WrenCall"/>.</param>
        /// <param name="returnValue">The return value of the call.</param>
        /// <param name="throwOnFailure">Determines if a <see cref="WrenInterpretException"/> is thrown if the call fails.</param>
        /// <returns>A <see cref="WrenInterpretResult"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WrenInterpretResult Call(this ref WrenCall call, out WrenValue* returnValue, bool throwOnFailure = false)
        {
            WrenInterpretResult result = call.Call(throwOnFailure);
            returnValue = result == WrenInterpretResult.Success ? Wren.GetSlotPtr(call.m_Vm.m_Ptr, 0) : default;
            return result;
        }

        #endregion

        #region WrenCallContext

        /// <summary>
        /// Gets the <see cref="WrenValue"/> at argument slot <paramref name="arg"/>.
        /// </summary>
        /// <param name="callContext">The <see cref="WrenCallContext"/>.</param>
        /// <param name="arg">The argument slot.</param>
        /// <returns>A <see cref="WrenValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WrenValue GetArg(this ref WrenCallContext callContext, int arg)
        {
            return *Wren.GetSlotPtr(callContext.m_Vm.m_Ptr, callContext.ArgSlot(arg));
        }

        /// <summary>
        /// Gets a pointer to the <see cref="WrenValue"/> at argument slot <paramref name="arg"/>.
        /// </summary>
        /// <param name="callContext">The <see cref="WrenCallContext"/>.</param>
        /// <param name="arg">The argument slot.</param>
        /// <returns>A pointer to a <see cref="WrenValue"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static WrenValue* GetArgPtr(this ref WrenCallContext callContext, int arg)
        {
            return Wren.GetSlotPtr(callContext.m_Vm.m_Ptr, callContext.ArgSlot(arg));
        }

        /// <summary>
        /// Sets <paramref name="value"/> as the return value for the method call.
        /// </summary>
        /// <param name="callContext">The <see cref="WrenCallContext"/>.</param>
        /// <param name="value">The value to return.</param>
        public static void Return(this ref WrenCallContext callContext, in WrenValue value)
        {
            Wren.SetSlot(callContext.m_Vm.m_Ptr, 0, value);
        }

        #endregion
    }
}
#endif