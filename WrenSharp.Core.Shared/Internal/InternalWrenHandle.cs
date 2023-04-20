using System;
using System.Runtime.CompilerServices;

namespace WrenSharp.Native
{
    internal sealed class WrenHandleInternal
    {
        public readonly WrenVM VM;
        public IntPtr Ptr = IntPtr.Zero;

        public WrenHandleInternal(WrenVM vm)
        {
            VM = vm;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsValid() => Ptr != IntPtr.Zero;
    }
}
