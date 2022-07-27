using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Abstract base class for attributes that require ordering.
    /// </summary>
    public abstract class WrenOrderedAttribute : WrenAttribute
    {
        /// <summary>
        /// The order value of the attribute.
        /// </summary>
        public int Order { get; set; }

        public WrenOrderedAttribute() : this(0) { }

        public WrenOrderedAttribute(int order)
        {
            Order = order;
        }
    }
}
