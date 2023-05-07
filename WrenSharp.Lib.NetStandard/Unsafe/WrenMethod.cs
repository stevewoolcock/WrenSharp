#if WRENSHARP_EXT
using System;
using System.Runtime.InteropServices;

namespace WrenSharp.Unsafe
{
    internal unsafe readonly partial struct WrenInternalMethod
    {
        [FieldOffset(8)]
        private readonly IntPtr m_Foreign;

        [FieldOffset(16)]
        private readonly IntPtr m_Closure;
    }
}
#endif