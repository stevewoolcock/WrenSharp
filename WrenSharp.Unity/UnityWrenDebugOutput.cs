using System;
using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void Write(LogType type, string message) => Debug.unityLogger.Log(type, message);

        #region WrenSharp Interfaces

        void IWrenWriteOutput.OutputWrite(WrenVM vm, string text)
        {
            static ReadOnlySpan<char> Parse(string text, out LogType logType)
            {
                if (text.StartsWith(ErrorPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    logType = LogType.Error;
                    return text.AsSpan(ErrorPrefix.Length);
                }

                if (text.StartsWith(WarnPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    logType = LogType.Warning;
                    return text.AsSpan(WarnPrefix.Length);
                }

                logType = LogType.Log;
                return text;
            }

            var message = Parse(text, out LogType logType);
            Write(logType, message.ToString());
        }

        void IWrenErrorOutput.OutputError(WrenVM vm, WrenErrorType errorType, string moduleName, int lineNumber, string message)
        {
            switch (errorType)
            {
                case WrenErrorType.Compile:
                    Write(LogType.Error, $"Wren compile error in {moduleName}:{lineNumber} : {message}");
                    break;

                case WrenErrorType.StackTrace:
                    Write(LogType.Error, $"at {message} in {moduleName}:{lineNumber}");
                    break;

                case WrenErrorType.Runtime:
                    Write(LogType.Error, string.IsNullOrEmpty(moduleName)
                        ? $"{ErrorPrefix}Wren error: {message}"
                        : $"{ErrorPrefix}Wren error in {moduleName}: {message}");
                    break;
            }
        }

        #endregion
    }
}
