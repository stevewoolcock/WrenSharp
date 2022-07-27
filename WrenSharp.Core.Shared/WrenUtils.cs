using System;

namespace WrenSharp
{
    /// <summary>
    /// A collection of useful utility methods for working with Wren.
    /// </summary>
    public static class WrenUtils
    {
        /// <summary>
        /// This array contains the argument signatures from 0 to <see cref="WrenVM.MaxCallParameters"/>.
        /// This is useful when constructing signatures dynamically, as the number of parameters or arguments
        /// corresponds directly to the array index.
        /// </summary>
        public static readonly string[] CallArgSignatures = new string[WrenVM.MaxCallParameters + 1]
        {
            "",
            "_",
            "_,_",
            "_,_,_",
            "_,_,_,_",
            "_,_,_,_,_",
            "_,_,_,_,_,_",
            "_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_,_,_,_,_,_",
            "_,_,_,_,_,_,_,_,_,_,_,_,_,_,_,_",
        };

        /// <summary>
        /// Scans a Wren call signature and returns the number of parameters it contains. This method does
        /// not ensure the validity of the signature, only the number of argument tokens it contains.
        /// </summary>
        /// <param name="signature">The call signature to scan.</param>
        /// <returns>The number of arugment tokens in <paramref name="signature"/>.</returns>
        public static byte GetParameterCount(string signature)
        {
            if (string.IsNullOrEmpty(signature))
                return 0;

            // Search for the call token: either a '(' (function) or '[' (subscript)
            // If no call token exists, the signature has no parameters
            int index = -1;
            int length = signature.Length;
            for (int i = 0; i < length; i++)
            {
                char c = signature[i];
                if (c == '(' || c == '[')
                {
                    // Found a call token
                    index = i;
                    break;
                }
            }

            if (index == -1)
                return 0;

            // Move to the char after the call token, since we know that the call token
            // cannot be an argument token.
            index++;

            // Each underscore reprents an argument, so scan ahead from the call token 
            // and count the number of underscores encountered. This does not ensure a
            // valid signature.
            byte paramCount = 0;
            while (index < length)
            {
                char c = signature[index++];
                if (c == '_')
                {
                    paramCount++;
                }
            }

            return paramCount;
        }
        
        /// <summary>
        /// Creates a method signature from the input arguments.
        /// </summary>
        /// <param name="methodName">The name of the method.</param>
        /// <param name="paramCount">The number of parameters the call requires.</param>
        /// <returns>A method signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="paramCount"/> is out of the valid parameter count range.</exception>
        public static string MethodSignature(string methodName, int paramCount)
        {
            if (paramCount < 0 || paramCount > WrenVM.MaxCallParameters)
                throw new ArgumentOutOfRangeException(nameof(paramCount), "Parameter count must be between 0 and 16, inclusive.");

            return $"{methodName}({CallArgSignatures[paramCount]})";
        }

        /// <summary>
        /// Creates a subscrupt signature from the input arguments.
        /// </summary>
        /// <param name="paramCount">The number of parameters the call requires.</param>
        /// <returns>A subscript signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="paramCount"/> is out of the valid parameter count range.</exception>
        public static string SubscriptSignature(int paramCount)
        {
            if (paramCount < 0 || paramCount > WrenVM.MaxCallParameters)
                throw new ArgumentOutOfRangeException(nameof(paramCount), "Parameter count must be between 0 and 16, inclusive.");

            return $"[{CallArgSignatures[paramCount]}]";
        }

        /// <summary>
        /// Creates a subscript setter signature from the input arguments.
        /// </summary>
        /// <param name="paramCount">The number of parameters the call requires.</param>
        /// <returns>A subscript setter signature.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="paramCount"/> is out of the valid parameter count range.</exception>
        public static string SubscriptSetterSignature(int paramCount)
        {
            if (paramCount < 0 || paramCount > WrenVM.MaxCallParameters)
                throw new ArgumentOutOfRangeException(nameof(paramCount), "Parameter count must be between 0 and 16, inclusive.");

            return $"[{CallArgSignatures[paramCount]}]=(_)";
        }

        /// <summary>
        /// Creates a property getter signature from the input arguments.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>A property getter signature.</returns>
        public static string GetterSignature(string propertyName)
        {
            return propertyName;
        }

        /// <summary>
        /// Creates a property setter signature from the input arguments.
        /// </summary>
        /// <param name="propertyName">The name of the property.</param>
        /// <returns>A property setter signature.</returns>
        public static string SetterSignature(string propertyName)
        {
            return $"{propertyName}=(_)";
        }
    }
}
