using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Fields marked with this attribute are automatically allocated a <see cref="WrenHandle"/>
    /// pointing to a variable declared within a resolved module, when invoked via the reflection API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class WrenVariableHandleAttribute : WrenAttribute
    {
        /// <summary>
        /// The name of the module the variable resides in. The module must be loaded and resolved
        /// for the handle to be successfully created.
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// The name of the variable within the module to create a handle to.
        /// </summary>
        public string Variable { get; }

        public WrenVariableHandleAttribute(string module, string variable)
        {
            Module = module;
            Variable = variable;
        }
    }
}
