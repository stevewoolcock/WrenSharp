using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace WrenSharp
{
    /// <summary>
    /// A Wren script source that reads from a Unity <see cref="UnityEngine.TextAsset"/>.
    /// </summary>
    public unsafe class WrenTextAssetSource : IWrenSource
    {
        private TextAsset m_Asset;

        #region Properties

        /// <summary>
        /// The <see cref="UnityEngine.TextAsset"/> containing the Wren script.
        /// </summary>
        public TextAsset TextAsset
        {
            get => m_Asset;
            set => m_Asset = value;
        }

        #endregion

        /// <summary>
        /// Creates a new <see cref="WrenTextAssetSource"/>.
        /// </summary>
        public WrenTextAssetSource()
        {

        }

        /// <summary>
        /// Creates a new <see cref="WrenTextAssetSource"/>.
        /// </summary>
        /// <param name="textAsset">The <see cref="UnityEngine.TextAsset"/> to read from.</param>
        public WrenTextAssetSource(TextAsset textAsset)
        {
            m_Asset = textAsset;
        }


        #region IModuleSource

        /// <summary>
        /// Get a pointer to the bytes containing the Wren script source that can be passed
        /// to the native Wren VM.
        /// </summary>
        /// <param name="byteCount">The number of bytes in the source.</param>
        /// <returns>A pointer to the source bytes.</returns>
        public IntPtr GetSourceBytes(out int byteCount)
        {
            if (m_Asset == null)
            {
                byteCount = 0;
                return IntPtr.Zero;
            }

            NativeArray<byte> data = m_Asset.GetData<byte>();
            byteCount = data.Length;
            return (IntPtr)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(data);
        }

        #endregion

        #region IDisposable

        void IDisposable.Dispose()
        {

        }

        #endregion
    }
}
