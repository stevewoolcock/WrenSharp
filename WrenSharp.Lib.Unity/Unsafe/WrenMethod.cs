#if WRENSHARP_EXT
using System;
using System.Runtime.InteropServices;
using WrenSharp.Native;

namespace WrenSharp.Unsafe
{
    internal unsafe partial struct WrenInternalMethod
    {
        [FieldOffset(8)]
        private fixed byte methods[WrenForeignMethodData.Size];

        [FieldOffset(8 + WrenForeignMethodData.Size)]
        private readonly IntPtr m_Closure;
    }
}
#endif