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
        public static int GetParameterCount(string signature) => GetParameterCount(signature.AsSpan());

        /// <summary>
        /// Scans a Wren call signature and returns the number of parameters it contains. This method does
        /// not ensure the validity of the signature, only the number of argument tokens it contains.
        /// </summary>
        /// <param name="signature">The call signature to scan.</param>
        /// <returns>The number of arugment tokens in <paramref name="signature"/>.</returns>
        public static int GetParameterCount(ReadOnlySpan<char> signature)
        {
            if (signature.Length == 0)
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


        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as a signed 8 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static byte AsInt8(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (byte)trunc;
            }

            throw new ArgumentException($"Value is not a valid 8 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as an unsigned 8 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static sbyte AsUInt8(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (sbyte)trunc;
            }

            throw new ArgumentException($"Value is not a valid unsigned 8 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as a signed 16 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static short AsInt16(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (short)trunc;
            }

            throw new ArgumentException($"Value is not a valid signed 16 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as an usigned 16 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static ushort AsUInt16(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (ushort)trunc;
            }

            throw new ArgumentException($"Value is not a valid unsigned 16 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as a signed 32 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static int AsInt32(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (int)trunc;
            }

            throw new ArgumentException($"Value is not a valid signed 32 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as an unsigned 32 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static uint AsUInt32(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (uint)trunc;
            }

            throw new ArgumentException($"Value is not a valid unsigned 32 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as a signed 64 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static long AsInt64(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (long)trunc;
            }

            throw new ArgumentException($"Value is not a valid signed 64 bit integer: {value}");
        }

        /// <summary>
        /// If <paramref name="value"/> is an integer, returns the integral value as an unsigned 64 bit integer. Otherwise, a <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="value">The value to conver to an integer.</param>
        /// <returns>The integral value.</returns>
        /// <exception cref="ArgumentException">Thrown is <paramref name="value"/> does not represent an integral value.</exception>
        public static ulong AsUInt64(double value)
        {
            double trunc = Math.Truncate(value);
            if (trunc == value)
            {
                return (ulong)trunc;
            }

            throw new ArgumentException($"Value is not a valid unsigned 64 bit integer: {value}");
        }


        /// <summary>
        /// Indicates if <paramref name="value"/> represents an integral value with no fractional components.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if <paramref name="value"/> has no fractional component.</returns>
        public static bool IsInteger(double value) => Math.Truncate(value) == value;

        /// <summary>
        /// Indicates if <paramref name="value"/> represents a signed 16 bit integral value with no fractional components.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if <paramref name="value"/> has no fractional component and fits with the range of a <see cref="byte"/>.</returns>
        public static bool IsInt8(double value)
        {
            double trunc = Math.Truncate(value);
            return trunc >= byte.MinValue && trunc <= byte.MaxValue;
        }

        /// <summary>
        /// Indicates if <paramref name="value"/> represents a signed 16 bit integral value with no fractional components.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if <paramref name="value"/> has no fractional component and fits with the range of a <see cref="short"/>.</returns>
        public static bool IsInt16(double value)
        {
            double trunc = Math.Truncate(value);
            return trunc >= short.MinValue && trunc <= short.MaxValue;
        }

        /// <summary>
        /// Indicates if <paramref name="value"/> represents a signed 32 bit integral value with no fractional components.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if <paramref name="value"/> has no fractional component and fits with the range of a <see cref="int"/>.</returns>
        public static bool IsInt32(double value)
        {
            double trunc = Math.Truncate(value);
            return trunc >= int.MinValue && trunc <= int.MaxValue;
        }

        /// <summary>
        /// Indicates if <paramref name="value"/> represents a signed 16 bit integral value with no fractional components.
        /// </summary>
        /// <param name="value">The value to test.</param>
        /// <returns>True if <paramref name="value"/> has no fractional component and fits with the range of a <see cref="long"/>.</returns>
        public static bool IsInt64(double value)
        {
            double trunc = Math.Truncate(value);
            return trunc>= long.MinValue && trunc <= long.MaxValue;
        }
    }
}
