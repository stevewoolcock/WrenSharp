using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Fields or properties marked with this attribute are ignored by the reflection API.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class WrenIgnoreAttribute : WrenAttribute
    {

    }
}
