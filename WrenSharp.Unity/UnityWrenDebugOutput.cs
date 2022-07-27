using System.Text;
using UnityEngine;

namespace WrenSharp.Unity
{
    /// <summary>
    /// Forwards Wren write and error operations to the Unity <see cref="UnityEngine.Debug"/> logging methods.
    /// </summary>
    public class UnityWrenDebugOutput : IWrenWriteOutput, IWrenErrorOutput
    {
        private readonly StringBuilder m_WriteBuffer = new StringBuilder(256);

        /// <summary>
        /// Clear the write buffer.
        /// </summary>
        public void Clear()
        {
            m_WriteBuffer.Clear();
        }

        /// <summary>
        /// Flush the write buffer. Sends the contents to <see cref="UnityEngine.Debug.Log(object)"/>, then clears
        /// the contents of the buffer.
        /// </summary>
        public void Flush()
        {
            if (m_WriteBuffer.Length <= 0)
                return;
            
            // Trim final newline, as Debug.Log will add one
            if (m_WriteBuffer[m_WriteBuffer.Length - 1] == '\n')
            {
                m_WriteBuffer.Length--;
            }

            Debug.Log(m_WriteBuffer.ToString());
            m_WriteBuffer.Clear();
        }

        #region WrenSharp Interfaces

        void IWrenWriteOutput.OutputWrite(WrenVM vm, string text)
        {
            m_WriteBuffer.Append(text);
        }

        void IWrenErrorOutput.OutputError(WrenVM vm, WrenErrorType errorType, string module, int lineNumber, string message)
        {
            switch (errorType)
            {
                case WrenErrorType.Compile:
                    Debug.LogError($"[{module}: ln {lineNumber}] [Error] {message}");
                    break;

                case WrenErrorType.StackTrace:
                    Debug.LogError($"[{module}: ln {lineNumber}] in {message}");
                    break;

                case WrenErrorType.Runtime:
                    Debug.LogError($"[Error] {message}");
                    break;
            }
        }

        #endregion
    }
}
