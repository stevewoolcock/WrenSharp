using System;
using System.Runtime.CompilerServices;

namespace WrenSharp
{
    /// <summary>
    /// Represents a dynamic Wren function created via <see cref="WrenVM.CreateFunctionCall(WrenHandle, int)"/>.
    /// </summary>
    public readonly struct WrenFn : IDisposable, IEquatable<WrenFn>
    {
        #region Properties

        /// <summary>
        /// The <see cref="WrenVM"/> the function was created with.
        /// </summary>
        public WrenVM VM { get; }

        /// <summary>
        /// The name of the module the function was created within.
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// The <see cref="WrenHandle"/> for the Wren Fn instance.
        /// </summary>
        public WrenHandle Handle { get; }

        /// <summary>
        /// The parameter count of the function.
        /// </summary>
        public byte ParamCount { get; }

        /// <summary>
        /// Returns true if the function handle is valid.
        /// </summary>
        public bool IsValid
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Handle.IsValid;
        }

        #endregion

        internal WrenFn(WrenVM vm, string module, WrenHandle handle, byte paramCount)
        {
            VM = vm;
            Module = module;
            Handle = handle;
            ParamCount = paramCount;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Handle.Dispose();
        }

        /// <summary>
        /// Iniitlaizes a <see cref="WrenCall"/> for the function.
        /// </summary>
        /// <returns>A <see cref="WrenCall"/> value for the function.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenCall CreateCall() => VM?.CreateFunctionCall(Handle, ParamCount) ?? default;

#if WRENSHARP_EXT
        /// <summary>
        /// Iniitlaizes a <see cref="WrenCall"/> for the function.
        /// </summary>
        /// <param name="createNewFibre">If true, a new Wren Fiber is created to execute the call.</param>
        /// <returns>A <see cref="WrenCall"/> value for the function.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenCall CreateCall(bool createNewFibre) => VM?.CreateFunctionCall(Handle, ParamCount, createNewFibre) ?? default;
#endif

        #region Object

        /// <inheritdoc/>
        public bool Equals(WrenFn other)
        {
            return other.ParamCount == ParamCount && other.Handle == Handle && other.VM == VM && other.Module == Module;
        }
        
        /// <inheritdoc/>
        public override bool Equals(object obj) => obj is WrenFn fn && Equals(fn);
        
        /// <inheritdoc/>
        public override int GetHashCode() => HashCode.Combine(ParamCount, Handle, VM, Module);

        /// <inheritdoc/>
        public override string ToString() => Handle.ToString();

        #endregion

        #region Operators

        /// <inheritdoc/>
        public static bool operator ==(WrenFn left, WrenFn right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(WrenFn left, WrenFn right) => !(left == right);

        #endregion
    }
}
