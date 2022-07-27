using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Enum types annotated with this attribute are converted to static Wren classes when invoked
    /// via the reflection API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Enum)]
    public class WrenEnumAttribute : WrenAttribute
    {
        /// <summary>
        /// The name of the module to declare the class in.
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// The name of the Wren class. If null, the name of the enum type is used.
        /// </summary>
        public string ClassName { get; }

        public WrenEnumAttribute(string module) : this(module, null)
        {

        }

        public WrenEnumAttribute(string module, string className)
        {
            Module = module;
            ClassName = className;
        }
    }
}
