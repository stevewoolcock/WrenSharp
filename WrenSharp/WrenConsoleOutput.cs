using System;

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
                    Console.WriteLine($"[{moduleName}: ln {lineNumber}] [Error] {message}");
                    break;

                case WrenErrorType.StackTrace:
                    Console.WriteLine($"[{moduleName}: ln {lineNumber}] in {message}");
                    break;

                case WrenErrorType.Runtime:
                    Console.WriteLine($"[Error] {message}");
                    break;
            }
        }
    }
}
