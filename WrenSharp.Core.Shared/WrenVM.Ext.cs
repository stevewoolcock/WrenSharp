#if WRENSHARP_EXT
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    public unsafe partial class WrenVM
    {
        #region Properties
        
        /// <summary>
        /// Gets the number of bytes known to be allocated for alive objects in the VM.
        /// </summary>
        public ulong BytesAllocated => Wren.BytesAllocated(m_Ptr);

        /// <summary>
        /// Gets or sets the enabled state of the Wren garbage collected. Use <see cref="CollectGarbage"/>
        /// to trigger manual garbage collections.
        /// </summary>
        public bool GCEnabled
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Wren.GetGCEnabled(m_Ptr);

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Wren.SetGCEnabled(m_Ptr, value);
        }

        #endregion

        /// <summary>
        /// Sets the enabled state of the Wren garbage collector.
        /// </summary>
        /// <param name="gcEnabled"></param>
        public void SetGCEnabled(bool gcEnabled) => Wren.SetGCEnabled(m_Ptr, gcEnabled);

        public bool GetGCEnabled() => Wren.GetGCEnabled(m_Ptr);
    }
}
#endif
