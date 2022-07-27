namespace WrenSharp
{
    /// <summary>
    /// An interface for implementing Wren write callbacks.
    /// </summary>
    public interface IWrenWriteOutput
    {
        void OutputWrite(WrenVM vm, string text);
    }

    public class WrenDelegateWriteOutput : IWrenWriteOutput
    {
        public delegate void Callback(WrenVM vm, string text);

        private Callback m_Callback;

        public WrenDelegateWriteOutput(Callback callback)
        {
            m_Callback = callback;
        }

        void IWrenWriteOutput.OutputWrite(WrenVM vm, string text)
        {
            m_Callback(vm, text);
        }

        public static explicit operator WrenDelegateWriteOutput(Callback callback) => new WrenDelegateWriteOutput(callback);
    }
}
