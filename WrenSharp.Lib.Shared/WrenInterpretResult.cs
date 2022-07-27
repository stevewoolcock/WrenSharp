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
}
