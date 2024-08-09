#if WRENSHARP_EXT
using System;
using System.Runtime.CompilerServices;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenNativeHandle : IEquatable<WrenNativeHandle>
    {
        public readonly WrenValue Value;
        public readonly WrenNativeHandle* Prev;
        public readonly WrenNativeHandle* Next;

        #region Object

        public override bool Equals(object obj) => obj is WrenNativeHandle handle && Equals(handle);

        public bool Equals(WrenNativeHandle other) => other.Value == Value && other.Prev == Prev && other.Next == Next;

        public override int GetHashCode() => HashCode.Combine(Value, (IntPtr)Prev, (IntPtr)Next);

        public override string ToString() => $"{nameof(WrenNativeHandle)}({Value})";

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WrenNativeHandle left, WrenNativeHandle right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WrenNativeHandle left, WrenNativeHandle right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WrenNativeHandle left, WrenValue right) => left.Value == right;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WrenNativeHandle left, WrenValue right) => left.Value != right;
    }
}
#endif