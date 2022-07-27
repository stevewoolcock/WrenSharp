namespace WrenSharp
{
    /// <summary>
    /// Represents Wren data types, used for indicating the type of a value in a variable or slot.
    /// </summary>
    public enum WrenType
    {
        /// <summary>
        /// A boolean value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_BOOL</c>
        /// </summary>
        Bool,

        /// <summary>
        /// A number value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_NUM</c>
        /// </summary>
        Number,

        /// <summary>
        /// A foreign value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_FOREIGN</c>
        /// </summary>
        Foreign,

        /// <summary>
        /// A list value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_LIST</c>
        /// </summary>
        List,

        /// <summary>
        /// A map value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_MAP</c>
        /// </summary>
        Map,

        /// <summary>
        /// A null value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_NULL</c>
        /// </summary>
        Null,

        /// <summary>
        /// A string value.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_STRING</c>
        /// </summary>
        String,

        /// <summary>
        /// A unknown value. Receiving this type indicates an error.<para/>
        /// <b>C value:</b> <c>WREN_TYPE_UNKNOWN</c>
        /// </summary>
        Unknown
    }
}
