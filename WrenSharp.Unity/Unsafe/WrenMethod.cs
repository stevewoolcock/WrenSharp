#if WRENSHARP_EXT
using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Unsafe
{
    internal unsafe readonly partial struct WrenInternalMethod
    {
        [FieldOffset(8)]
        private readonly Native.WrenForeignMethodData m_Foreign;

        [FieldOffset(24)]
        private readonly IntPtr m_Closure;
    }
}
#endif