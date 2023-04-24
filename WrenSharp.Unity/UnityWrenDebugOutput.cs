using System.Text;
using UnityEngine;

namespace WrenSharp.Unity
{
    /// <summary>
    /// Forwards Wren write and error operations to the Unity <see cref="UnityEngine.Debug"/> logging methods.
    /// </summary>
    public class UnityWrenDebugOutput : IWrenWriteOutput, IWrenErrorOutput
    {
        /// <summary>
        /// Prefix used to print messages from Wren as warnings to the Unity log and console.
        /// </summary>
        public const string WarnPrefix = "!warn:";

        /// <summary>
        /// Prefix used to print messages from Wren as errors to the Unity log and console.
        /// </summary>
        public const string ErrorPrefix = "!error:";

        private readonly StringBuilder m_WriteBuffer = new StringBuilder(256);
        private LogType m_CurrentType = LogType.Log;

        /// <summary>
        /// Clear the write buffer.
        /// </summary>
        public void Clear()
        {
            m_CurrentType = LogType.Log;
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
            // Buffer could be empty after that, so check and bail out if so
            if (m_WriteBuffer[m_WriteBuffer.Length - 1] == '\n')
            {
                if (--m_WriteBuffer.Length <= 0)
                    return;
            }

            Debug.unityLogger.Log(m_CurrentType, m_WriteBuffer.ToString());
            m_WriteBuffer.Clear();
        }

        private void Write(string text)
        {
            if (m_CurrentType != LogType.Error && text.StartsWith(ErrorPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                m_CurrentType = LogType.Error;
                m_WriteBuffer.Append(text, ErrorPrefix.Length, text.Length - ErrorPrefix.Length);
            }
            else if (m_CurrentType != LogType.Warning && text.StartsWith(WarnPrefix, System.StringComparison.OrdinalIgnoreCase))
            {
                Flush();
                m_CurrentType = LogType.Warning;
                m_WriteBuffer.Append(text, WarnPrefix.Length, text.Length - WarnPrefix.Length);
            }
            else
            {
                if (m_CurrentType != LogType.Log)
                {
                    Flush();
                }

                m_CurrentType = LogType.Log;
                m_WriteBuffer.Append(text);
            }
        }

        #region WrenSharp Interfaces

        void IWrenWriteOutput.OutputWrite(WrenVM vm, string text)
        {
            Write(text);
        }

        void IWrenErrorOutput.OutputError(WrenVM vm, WrenErrorType errorType, string moduleName, int lineNumber, string message)
        {
            switch (errorType)
            {
                case WrenErrorType.Compile:
                    Write($"{ErrorPrefix}Wren compile error in {moduleName}:{lineNumber} : {message}");
                    break;

                case WrenErrorType.StackTrace:
                    Write($"{ErrorPrefix}at {message} in {moduleName}:{lineNumber}");
                    break;

                case WrenErrorType.Runtime:
                    Write(string.IsNullOrEmpty(moduleName)
                        ? $"{ErrorPrefix}Wren error: {message}"
                        : $"{ErrorPrefix}Wren error in {moduleName}: {message}");
                    break;
            }
        }

        #endregion
    }
}
