using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Marks a field or property for inclusion by the reflection API, where inclusion is opt-in.
    /// Also allows for setting an explicit name for the member when interpreted to a Wren type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class WrenPropertyAttribute : WrenAttribute
    {
        /// <summary>
        /// The name of the property. If null, the name of the member the attribute is annotating is used.
        /// </summary>
        public string Name { get; }

        public WrenPropertyAttribute() { }

        public WrenPropertyAttribute(string name)
        {
            Name = name;
        }
    }
}
