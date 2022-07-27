namespace WrenSharp
{
    /// <summary>
    /// Represents error types that can be returned from Wren. These map directly to the
    /// values that Wren can return.
    /// </summary>
    public enum WrenErrorType
    {
        /// <summary>
        /// Represents a compile error while interpreting Wren source.<para/>
        /// <b>C value:</b> <c>WREN_ERROR_COMPILE</c>
        /// </summary>
        Compile,

        /// <summary>
        /// Represents a runtime error while executing Wren bytecode.<para/>
        /// <b>C value:</b> <c>WREN_ERROR_RUNTIME</c>
        /// </summary>
        Runtime,

        /// <summary>
        /// Represents a stack trace line from an error.<para/>
        /// <b>C value:</b> <c>WREN_ERROR_STACK_TRACE</c>
        /// </summary>
        StackTrace,
    }
}
