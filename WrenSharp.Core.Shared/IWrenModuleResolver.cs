namespace WrenSharp
{
    /// <summary>
    /// An interface for implementing module name resolution for a <see cref="WrenVM"/>.
    /// This allows for mapping of one module name to another for the purposes of path resolution, aliasing, etc.
    /// </summary>
    public interface IWrenModuleResolver
    {
        /// <summary>
        /// Returns the resolved name of the module <paramref name="name"/>, which is being imported
        /// within by the module <paramref name="importer"/>.
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="importer"></param>
        /// <param name="name"></param>
        /// <returns>The resolved name of the module.</returns>
        string ResolveModule(WrenVM vm, string importer, string name);
    }
}
