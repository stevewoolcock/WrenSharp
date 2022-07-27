namespace WrenSharp
{
    /// <summary>
    /// An interface for implementing Wren error callbacks.
    /// </summary>
    public interface IWrenErrorOutput
    {
        void OutputError(WrenVM vm, WrenErrorType errorType, string module, int lineNumber, string message);
    }

    /// <summary>
    /// A wrapper class for delegates that match the <see cref="IWrenErrorOutput.OutputError(WrenVM, WrenErrorType, string, int, string)"/>
    /// signature.
    /// </summary>
    public class WrenDelegateErrorOutput : IWrenErrorOutput
    {
        public delegate void Callback(WrenVM vm, WrenErrorType errorType, string module, int lineNumber, string message);

        private Callback m_Callback;

        public WrenDelegateErrorOutput(Callback callback)
        {
            m_Callback = callback;
        }

        void IWrenErrorOutput.OutputError(WrenVM vm, WrenErrorType errorType, string module, int lineNumber, string message)
        {
            m_Callback(vm, errorType, module, lineNumber, message);
        }

        public static explicit operator WrenDelegateErrorOutput(Callback callback) => new WrenDelegateErrorOutput(callback);
    }
}
