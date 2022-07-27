using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Declares a method that should be invoked via WrenSharp reflection API initialization methods.<para/>
    /// Methods marked with this attribute must have the signature:
    /// <code>
    /// [static] void MethodName(WrenVm vm)
    /// </code>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class WrenInitializeMethodAttribute : WrenOrderedAttribute
    {
        public WrenInitializeMethodAttribute() { }
        public WrenInitializeMethodAttribute(int order) : base(order) { }
    }
}
