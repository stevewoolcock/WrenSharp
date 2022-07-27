namespace WrenSharp
{
    public partial class WrenVMConfiguration
    {
        /// <summary>
        /// <para>
        /// A custom memory allocation delegate. Leave null to use Wren's default native memory reallocation function.
        /// </para>
        /// <para>
        /// This delegate should free memory when the <b>newSize</b> parameter is zero, and reallocate the
        /// pointer <b>memory</b> to a new block of memory of at least <b>newSize</b> bytes.
        /// </para>
        /// <para>
        /// Failing to free allocated memory when <b>newSize</b> is zero will result in memory leaks.
        /// </para>
        /// </summary>
        public WrenReallocate Reallocator { get; set; }
    }
}
