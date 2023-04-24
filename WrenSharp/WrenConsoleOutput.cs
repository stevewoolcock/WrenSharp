namespace WrenSharp
{
    /// <summary>
    /// A Wren writer and error receiver that forwards output to <see cref="System.Console"/>.
    /// </summary>
    public class WrenConsoleOutput : IWrenWriteOutput, IWrenErrorOutput
    {
        public void OutputWrite(WrenVM vm, string text)
        {
            Console.Write(text);
        }

        public void OutputError(WrenVM vm, WrenErrorType errorType, string moduleName, int lineNumber, string message)
        {
            switch (errorType)
            {
                case WrenErrorType.Compile:
                    Console.WriteLine($"Wren compile error in {moduleName}:{lineNumber} : {message}");
                    break;

                case WrenErrorType.StackTrace:
                    Console.WriteLine($"at {message} in {moduleName}:{lineNumber}");
                    break;

                case WrenErrorType.Runtime:
                    Console.WriteLine(string.IsNullOrEmpty(moduleName)
                        ? $"Wren error: {message}"
                        : $"Wren error in {moduleName}: {message}");
                    break;
            }
        }
    }
}
