namespace WrenSharp
{
    /// <summary>
    /// Return values from Wren VM interpret or method calls.
    /// </summary>
    public enum WrenInterpretResult
    {
        /// <summary>
        /// A successful result.<para/>
        /// <b>C value:</b> <c>WREN_RESULT_SUCCESS</c>
        /// </summary>
        Success,

        /// <summary>
        /// A compile error result.<para/>
        /// <b>C value:</b> <c>WREN_RESULT_COMPILE_ERROR</c>
        /// </summary>
        CompileError,

        /// <summary>
        /// A runtime error result.<para/>
        /// <b>C value:</b> <c>WREN_RESULT_RUNTIME_ERROR</c>
        /// </summary>
        RuntimeError,
    }

    public static class WrenInterpretResultExtensions
    {
        /// <summary>
        /// Indicates if <paramref name="result"/> is equal to <see cref="WrenInterpretResult.Success"/>.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns>True if <paramref name="result"/> is a success, otherwise false.</returns>
        public static bool IsSuccess(this WrenInterpretResult result) => result == WrenInterpretResult.Success;

        /// <summary>
        /// Indicates if <paramref name="result"/> is not equal to <see cref="WrenInterpretResult.Success"/>.
        /// </summary>
        /// <param name="result">The result to check.</param>
        /// <returns>True if <paramref name="result"/> is not a success, otherwise false.</returns>
        public static bool IsFailure(this WrenInterpretResult result) => result != WrenInterpretResult.Success;
    }
}
