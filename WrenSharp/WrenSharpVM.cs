using WrenSharp.Native;

namespace WrenSharp
{
    public class WrenSharpVM : WrenVM
    {
        private readonly ForeignLookup<WrenForeign> m_ForeignLookup = new ForeignLookup<WrenForeign>();

        private readonly IWrenWriteOutput m_Writer;
        private readonly IWrenErrorOutput m_ErrorReceiver;
        private readonly IWrenModuleProvider m_ModuleProvider;
        private readonly IWrenModuleResolver m_ModuleResolver;
        private readonly WrenReallocate m_Reallocator;

        private WrenNativeFn.LoadModuleComplete m_LoadModuleCallback;
        private IWrenSource m_LoadModuleSource;

        /// <summary>
        /// Creates a new Wren VM to run Wren scripts. A copy of <paramref name="config"/> is made, so any further
        /// changes to the user provided configuration will not affect this VM instance.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="config"/> is null.</exception>
        public WrenSharpVM(WrenVMConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));

            LogErrors = config.LogErrors;
            m_Writer = config.WriteOutput;
            m_ErrorReceiver = config.ErrorOutput;
            m_ModuleProvider = config.ModuleProvider;
            m_ModuleResolver = config.ModuleResolver;
            m_Reallocator = config.Reallocator;

            // Initialize a WrenConfiguration
            var wrenCfg = WrenConfiguration.InitializeNew();

            // Copy managed configuration to native config
            CopyManagedConfigToNativeConfig(config, ref wrenCfg);

            // Initialize the VM with the 
            Initialize(ref wrenCfg, config.Allocator);
        }

        private void CopyManagedConfigToNativeConfig(WrenVMConfiguration config, ref WrenConfiguration nativeConfig)
        {
            // Memory
            nativeConfig.InitialHeapSize = config.InitialHeapSize.GetValueOrDefault(nativeConfig.InitialHeapSize);
            nativeConfig.MinHeapSize = config.MinHeapSize.GetValueOrDefault(nativeConfig.MinHeapSize);
            nativeConfig.HeapGrowthPercent = config.HeapGrowthPercent.HasValue ? (int)(config.HeapGrowthPercent * 100) : nativeConfig.HeapGrowthPercent;

            // Memory
            if (m_Reallocator != null)
            {
                nativeConfig.Reallocate = (memory, newSize, userData) => m_Reallocator(this, memory, newSize);
            }

            // Modules
            nativeConfig.LoadModule = (_, module) => LoadModule(module);
            nativeConfig.ResolveModule = (_, importer, module) => ResolveModule(importer, module);
            m_LoadModuleCallback = (_, moduleName, result) => OnLoadModuleComplete(moduleName);

            // Writers
            nativeConfig.Write = (_, text) => m_Writer?.OutputWrite(this, text);
            nativeConfig.Error = (_, errorType, module, line, message) => ReceiveError(errorType, module, line, message);

            // Foreign binding
            nativeConfig.BindForeignMethod = (vm, module, className, isStatic, signature) => BindForeignMethod(module, className, isStatic != 0, signature);
            nativeConfig.BindForeignClass = (vm, module, className) => BindForeignClass(module, className);
        }

        #region Public API

        /// <summary>
        /// Gets the <see cref="WrenForeign"/> object for building foreign classes and methods.
        /// </summary>
        /// <param name="moduleName">The Wren module name.</param>
        /// <param name="className">The Wren class name.</param>
        /// <returns>The <see cref="WrenForeign"/> instance for the supplied class.</returns>
        public WrenForeign Foreign(string moduleName, string className)
        {
            WrenForeign foreign = m_ForeignLookup.GetClass(moduleName, className);
            if (foreign == null)
            {
                foreign = new WrenForeign(this);
                m_ForeignLookup.AddClass(moduleName, className, foreign);
            }

            return foreign;
        }

        #endregion

        private WrenLoadModuleResult LoadModule(string moduleName)
        {
            IWrenModuleProvider provider = m_ModuleProvider;
            if (provider == null)
                return WrenLoadModuleResult.Failed;

            IWrenSource source = provider.GetModuleSource(this, moduleName);
            if (source == null)
                return WrenLoadModuleResult.Failed;

            IntPtr buffer = source.GetSourceBytes(out int bufferSize);
            if (buffer == IntPtr.Zero || bufferSize == 0)
                return WrenLoadModuleResult.Failed;

            // Store state source so it can be returned in the callback
            m_LoadModuleSource = source;

            return new WrenLoadModuleResult()
            {
                Source = buffer,
                UserData = IntPtr.Zero,
                OnCompleteCallback = m_LoadModuleCallback,
            };
        }

        private void OnLoadModuleComplete(string moduleName)
        {
            IWrenSource source = m_LoadModuleSource!;

            m_LoadModuleSource = default;
            m_ModuleProvider.OnModuleLoadComplete(this, moduleName, source);
        }

        private string ResolveModule(string importer, string moduleName)
        {
            IWrenModuleResolver resolver = m_ModuleResolver;
            if (resolver == null)
                return moduleName;

            return resolver.ResolveModule(this, importer, moduleName);
        }

        private WrenNativeFn.ForeignMethod BindForeignMethod(string moduleName, string className, bool isStatic, string signature)
        {
            WrenForeign foreign = m_ForeignLookup.GetClass(moduleName, className);
            if (foreign == null)
                return null;

            return foreign.FindMethod(isStatic, signature);
        }

        private WrenForeignClassMethods BindForeignClass(string moduleName, string className)
        {
            WrenForeign foreign = m_ForeignLookup.GetClass(moduleName, className);
            return foreign?.GetClassMethods() ?? default;
        }

        private void ReceiveError(WrenErrorType errorType, string moduleName, int lineNumber, string message)
        {
            LogError(errorType, moduleName, lineNumber, message);
            m_ErrorReceiver?.OutputError(this, errorType, moduleName, lineNumber, message);
        }
    }
}