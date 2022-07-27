using System;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// Fields marked with this attribute are automatically allocated a <see cref="WrenCallHandle"/>
    /// matching the supplied signature when invoked via the reflection API.<para/>
    /// See <see href="https://wren.io/method-calls.html"/>  for more information on Wren call signatures.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field)]
    public class WrenCallHandleAttribute : WrenAttribute
    {
        /// <summary>
        /// The signature of the call handle.
        /// </summary>
        public string Signature { get; }

        public WrenCallHandleAttribute(string signature)
        {
            Signature = signature;
        }
    }
}
