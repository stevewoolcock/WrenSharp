namespace WrenSharp
{
    /// <summary>
    /// An interface describing the contact that must be implemented by <see cref="WrenVM"/> types
    /// that provide access to <see cref="IWrenForeign"/> instances.
    /// </summary>
    public interface IWrenVMWithForeign
    {
        /// <summary>
        /// Gets an <see cref="IWrenForeign"/> object for building foreign classes and methods.
        /// </summary>
        /// <param name="moduleName">The Wren module name.</param>
        /// <param name="className">The Wren class name.</param>
        /// <returns>The <see cref="IWrenForeign"/> instance for the supplied class.</returns>
        IWrenForeign Foreign(string moduleName, string className);
    }
}
