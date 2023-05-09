using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Profiling;
using UnityEngine;
using WrenSharp.Native;

namespace WrenSharp.Unity
{
    /// <summary>
    /// A WrenVM configured for use with the Unity game engine.
    /// </summary>
    public sealed class UnityWrenVM : WrenVM
    {
        #region Static

        private static readonly ProfilerMarker _profilerMarkerInterpet = new ProfilerMarker(ProfilerCategory.Scripts, "Wren Interpret");
        private static readonly UnityWrenDebugOutput _defaultUnityOutput = new UnityWrenDebugOutput();

        // Support disabled domain reloads in the Unity editor
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            //
        }

        #endregion

        private readonly ForeignLookup<WrenForeign> m_ForeignLookup = new ForeignLookup<WrenForeign>();

        private readonly IWrenWriteOutput m_WriteOutput;
        private readonly IWrenErrorOutput m_ErrorOutput;
        private readonly IWrenModuleProvider m_ModuleProvider;
        private readonly IWrenModuleResolver m_ModuleResolver;
        private readonly UnityWrenDebugOutput m_UnityDebugOutput;

        // Module load state
        private WrenNativeFn.LoadModuleComplete m_LoadModuleCallback;
        private IWrenSource m_LoadModuleSource;

        /// <summary>
        /// Creates a new Wren VM to run Wren scripts using the default configuration.
        /// </summary>
        public UnityWrenVM() : this(config: null) { }

        /// <summary>
        /// Creates a new Wren VM to run Wren scripts using the supplied configuration.
        /// </summary>
        /// <param name="config">The configuration to use.</param>
        public UnityWrenVM(WrenVMConfiguration config)
        {
            if (config == null)
            {
                LogErrors = true;
                m_WriteOutput = _defaultUnityOutput;
                m_ErrorOutput = _defaultUnityOutput;
            }
            else
            {
                LogErrors = config.LogErrors;
                m_WriteOutput = config.WriteOutput != null ? config.WriteOutput : _defaultUnityOutput;
                m_ErrorOutput = config.ErrorOutput != null ? config.ErrorOutput : _defaultUnityOutput;
                m_ModuleProvider = config.ModuleProvider;
                m_ModuleResolver = config.ModuleResolver;
            }

            // This is used to avoid casting when it is needed
            m_UnityDebugOutput = m_WriteOutput as UnityWrenDebugOutput;

            var nativeConfig = WrenConfiguration.InitializeNew();
            CopyManagedConfigToNativeConfig(config, ref nativeConfig);

            // Initialize the VM with the configuration
            Initialize(config.Initializer, ref nativeConfig, config.Allocator);
        }

        private void CopyManagedConfigToNativeConfig(WrenVMConfiguration config, ref WrenConfiguration nativeConfig)
        {
            nativeConfig.InitialHeapSize = config.InitialHeapSize.GetValueOrDefault(nativeConfig.InitialHeapSize);
            nativeConfig.MinHeapSize = config.MinHeapSize.GetValueOrDefault(nativeConfig.MinHeapSize);
            nativeConfig.HeapGrowthPercent = config.HeapGrowthPercent.HasValue ? (int)(config.HeapGrowthPercent * 100) : nativeConfig.HeapGrowthPercent;

            if (config.Reallocate != null)
            {
                nativeConfig.Reallocate = config.Reallocate;
            }

            nativeConfig.LoadModule = _LoadModule;
            nativeConfig.ResolveModule = _ResolveModule;
            nativeConfig.Write = _WriteFn;
            nativeConfig.Error = _ErrorFn;
            nativeConfig.BindForeignMethod = _BindForeignMethodFn;
            nativeConfig.BindForeignClass = _BindForeignClassFn;
            m_LoadModuleCallback = _OnModuleLoadComplete;
        }

        protected override void DisposeManagedState()
        {
            base.DisposeManagedState();

            // Ensure all foreign methods free up their symbols so holes in the static delegate tables
            // can be reused and don't leak memory
            m_ForeignLookup.ForEachClass(x => x.Dispose());
        }

        #region Public API

        /// <summary>
        /// Gets the <see cref="WrenForeign"/> object for building foreign classes and methods.
        /// </summary>
        /// <param name="module">The Wren module name.</param>
        /// <param name="cls">The Wren class name.</param>
        /// <returns>The <see cref="WrenForeign"/> instance for the supplied class.</returns>
        public WrenForeign Foreign(string module, string cls)
        {
            WrenForeign foreign = m_ForeignLookup.GetClass(module, cls);
            if (foreign == null)
            {
                foreign = new WrenForeign(this);
                m_ForeignLookup.AddClass(module, cls, foreign);
            }

            return foreign;
        }

        /// <summary>
        /// Runs Wren source (the contents of <paramref name="source"/>) in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.
        /// <para/>
        /// This method passes the <see cref="TextAsset"/> native data to the Wren VM which avoids allocating extra memory to access the asset's
        /// text content.
        /// <para/>
        /// Wren scripts as <see cref="TextAsset"/>s, you should ensure they are encoded as UTF8 <b>with</b> BOM.
        /// Failing to include the BOM in the encoding may result in Wren failing to interpret the source.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The source to interpret.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public WrenInterpretResult Interpret(string module, TextAsset source, bool throwOnFailure = false)
        {
            // Get the raw bytes from the asset and get its pointer
            // This is the fastest way to access the text data and send it to Wren without any memory allocations,
            // since the raw script data can be passed directly into the Wren interpret fuction.
            return Interpret(module, source.GetData<byte>(), throwOnFailure);
        }

        /// <summary>
        /// Runs Wren source (the contents of <paramref name="source"/>) in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The source to interpret.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public unsafe WrenInterpretResult Interpret(string module, NativeArray<byte> source, bool throwOnFailure = false)
        {
            var ptr = (byte*)NativeArrayUnsafeUtility.GetUnsafeBufferPointerWithoutChecks(source);
            return Interpret(module, (IntPtr)ptr, throwOnFailure);
        }

        #endregion

        #region Protected API

        /// <summary>
        /// Called when before Wren interpets any source or a direct call is made.
        /// </summary>
        protected override void OnInterpretBegin()
        {
            _profilerMarkerInterpet.Begin();
        }

        /// <summary>
        /// Called after Wren interprets source or a direct call is made.
        /// </summary>
        /// <param name="result">The result of the interpretation or call.</param>
        protected override void OnInterpretEnd(WrenInterpretResult result)
        {
            _profilerMarkerInterpet.End();
        }

        #endregion

        private string ResolveModule(string importer, string module)
        {
            IWrenModuleResolver resolver = m_ModuleResolver;
            if (resolver == null)
                return module;

            return resolver.ResolveModule(this, importer, module);
        }

        private WrenLoadModuleResult LoadModule(string module)
        {
            IWrenModuleProvider provider = m_ModuleProvider;
            if (provider == null)
                return WrenLoadModuleResult.Failed;

            IWrenSource source = provider.GetModuleSource(this, module);
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

                // IL2CPP fails to marshal the WrenNativeFn.LoadModuleComplete via p/invoke automatically (a bug, most likely).
                // It likely has something to do with the signature having a struct argument. Using an IntPtr and marshalling it
                // manually as the correct type seems to work around this problem.
                OnCompleteCallback = Marshal.GetFunctionPointerForDelegate(m_LoadModuleCallback),
            };
        }

        private void OnLoadModuleComplete(string module)
        {
            IWrenSource source = m_LoadModuleSource;
            m_LoadModuleSource = default;
            m_ModuleProvider.OnModuleLoadComplete(this, module, source);
        }

        private void OnError(WrenErrorType errorType, string module, int lineNumber, string message)
        {
            LogError(errorType, module, lineNumber, message);
            m_ErrorOutput?.OutputError(this, errorType, module, lineNumber, message);
        }

        #region IL2CPP Compatible P/Invoke Functions

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.Write))]
        private static void _WriteFn(IntPtr vmPtr, string text)
        {
            var vm = GetVM<UnityWrenVM>(vmPtr);
            vm.m_WriteOutput?.OutputWrite(vm, text);
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.Error))]
        private static void _ErrorFn(IntPtr vmPtr, WrenErrorType errorType, string module, int line, string message)
        {
            GetVM<UnityWrenVM>(vmPtr).OnError(errorType, module, line, message);
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.BindForeignMethod))]
        private static WrenForeignMethodData _BindForeignMethodFn(IntPtr vmPtr, string module, string cls, byte isStatic, string signature)
        {
            WrenForeign foreign = GetVM<UnityWrenVM>(vmPtr).m_ForeignLookup.GetClass(module, cls);
            if (foreign == null)
                return WrenForeignMethodData.NotFound;

            return foreign.FindMethod(isStatic != 0, signature);
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.BindForeignClass))]
        private static WrenForeignClassMethods _BindForeignClassFn(IntPtr vmPtr, string module, string cls)
        {
            WrenForeign foreign = GetVM<UnityWrenVM>(vmPtr).m_ForeignLookup.GetClass(module, cls);
            return foreign?.GetClassMethods() ?? default;
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.ResolveModule))]
        private static string _ResolveModule(IntPtr vmPtr, string importer, string module)
        {
            return GetVM<UnityWrenVM>(vmPtr).ResolveModule(importer, module);
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.LoadModule))]
        private static WrenLoadModuleResult _LoadModule(IntPtr vmPtr, string module)
        {
            return GetVM<UnityWrenVM>(vmPtr).LoadModule(module);
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.LoadModuleComplete))]
        private static void _OnModuleLoadComplete(IntPtr vmPtr, string name, WrenLoadModuleResult result)
        {
            GetVM<UnityWrenVM>(vmPtr).OnLoadModuleComplete(name);
        }

        #endregion
    }
}
