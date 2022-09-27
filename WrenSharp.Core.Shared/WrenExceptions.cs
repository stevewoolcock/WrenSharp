using System;
using System.Linq;

namespace WrenSharp
{
    /// <summary>
    /// Represents errors that occur during WrenSharp execution.
    /// </summary>
    public class WrenException : Exception
    {
        /// <summary>
        /// The VM the exeception occured in.
        /// </summary>
        public WrenVM VM { get; }

        /// <summary>
        /// The errors in the VM's error log when the exception was thrown.
        /// </summary>
        public WrenError[] Errors { get; }

        public WrenException(WrenVM vm) : this(vm, null, null) { }
        public WrenException(WrenVM vm, string message) : this(vm, message, null) { }
        public WrenException(WrenVM vm, string message, Exception innerException) : base(GetMessageOrDefault(vm, message), innerException)
        {
            VM = vm;
            Errors = (VM.Errors.Count > 0) ? VM.Errors.ToArray() : null;
        }

        private static string GetMessageOrDefault(WrenVM vm, string message)
        {
            if (message == null && vm.Errors.Count > 0)
            {
                WrenError error = vm.Errors[0];
                return $"{error.ErrorType}: {error.Message}";
            }

            return message;
        }
    }

    /// <summary>
    /// Represents errors that occur during intialization of a Wren VM.
    /// </summary>
    public class WrenInitializationException : WrenException
    {
        public WrenInitializationException(WrenVM vm, string message) : base(vm, message) { }
        public WrenInitializationException(WrenVM vm, string message, Exception innerException) : base(vm, message, innerException) { }
    }

    /// <summary>
    /// Represents errors that occur during a WrenSharp call.
    /// </summary>
    public class WrenInterpretException : WrenException
    {
        /// <summary>
        /// The result of the call or interpret run.
        /// </summary>
        public WrenInterpretResult Result { get; }

        public WrenInterpretException(WrenVM vm, WrenInterpretResult result) : this(vm, result, null) { }
        public WrenInterpretException(WrenVM vm, WrenInterpretResult result, string message) : base(vm, message)
        {
            Result = result;
        }
    }

    /// <summary>
    /// Represents errors that occur relating to invalid Wren handles.
    /// </summary>
    public class WrenInvalidHandleException : WrenException
    {
        public WrenInvalidHandleException(WrenVM vm) : base(vm, null, null) { }
        public WrenInvalidHandleException(WrenVM vm, string message) : base(vm, message, null) { }
    }
}
