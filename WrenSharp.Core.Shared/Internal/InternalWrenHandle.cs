using System;

namespace WrenSharp.Native
{
    internal class WrenHandleInternal
    {
        public readonly WrenVM VM;
        public IntPtr Ptr = IntPtr.Zero;

        public WrenHandleInternal(WrenVM vm)
        {
            VM = vm;
        }
    }
}
