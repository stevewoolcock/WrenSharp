namespace WrenSharp
{
    /// <summary>
    /// An interface for implementing module importing functionality for a <see cref="WrenVM"/>.
    /// </summary>
    public interface IWrenModuleProvider
    {
        /// <summary>
        /// Provides an <see cref="IWrenSource"/> instance that contains the Wren source for the given module.
        /// </summary>
        /// <param name="vm">The VM context of the request.</param>
        /// <param name="module">The name of the module to retrieve.</param>
        /// <returns>An <see cref="IWrenSource"/> instance.</returns>
        IWrenSource GetModuleSource(WrenVM vm, string module);

        /// <summary>
        /// The callback to execute when a module has been loaded by a VM. The <paramref name="source"/> of the module
        /// is provided so that any unmanaged resources that it may have acquired to load the module can freed.
        /// </summary>
        /// <param name="vm">The VM the module was loaded into.</param>
        /// <param name="module">The name of the module that was loaded.</param>
        /// <param name="source">The <see cref="IWrenSource"/> instance that was loaded.</param>
        void OnModuleLoadComplete(WrenVM vm, string module, IWrenSource source);
    }
}
