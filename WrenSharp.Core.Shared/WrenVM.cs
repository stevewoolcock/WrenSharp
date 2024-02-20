using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using WrenSharp.Native;
using WrenSharp.Memory;
using WrenSharp.Internal;

namespace WrenSharp
{
    /// <summary>
    /// A delegate used to initialize a new Wren VM. This can be used to override the default
    /// behaviour when creating a new native Wren VM. Implemetnations should return  <see cref="IntPtr.Zero"/>
    /// if the  VM fails to initialize.
    /// </summary>
    /// <param name="configuration">The <see cref="WrenConfiguration"/> to pass to the native Wren VM initializer.</param>
    /// <returns>A pointer to the native Wren VM.</returns>
    public delegate IntPtr WrenVMInitializer(ref WrenConfiguration configuration);

    /// <summary>
    /// A delegate executed for a Wren foreign method call.
    /// </summary>
    /// <param name="call">The method call API.</param>
    public delegate void WrenForeignMethod(WrenCallContext call);

    /// <summary>
    /// Describes a Wren compatible memory reallocation method for custom memory management.
    /// This method should free memory when the new size is zero, or reallocate the existing memory in the
    /// provided pointer to a newly allocated block of at least the requested new size.
    /// </summary>
    /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
    /// <param name="memory">The current memory block. This value can be null (<see cref="IntPtr.Zero"/>).</param>
    /// <param name="newSize">The minimum size of the new memory block to reallocate.</param>
    /// <returns>A pointer to the newly allocated memory block.</returns>
    public delegate IntPtr WrenReallocate(WrenVM vm, IntPtr memory, ulong newSize);


    /// <summary>
    /// A managed instance of a Wren virtual machine. This class wraps the native VM and provides a clean, convenient API
    /// for working with the VM in a managed environment.
    /// </summary>
    public unsafe partial class WrenVM : IDisposable
    {
        #region Static

        private static readonly Dictionary<IntPtr, WrenVM> _vmsByPtr = new Dictionary<IntPtr, WrenVM>();

        /// <summary>
        /// Returns the version number of the Wren library.
        /// </summary>
        public static int VersionNumber => Wren.GetVersionNumber();

        /// <summary>
        /// The maximum number of parameters a method call can accept.
        /// </summary>
        public const int MaxCallParameters = 16;

        /// <summary>
        /// An enumeration of all <see cref="WrenVM"/> instances.
        /// </summary>
        public IEnumerable<WrenVM> VMs => _vmsByPtr.Values;

        /// <summary>
        /// Gets the managed <see cref="WrenVM"/> corresponding to a native Wren VM.
        /// </summary>
        /// <param name="ptr">The native Wren VM pointer.</param>
        /// <returns>A <see cref="WrenVM"/> instance or null if not found.</returns>
        public static WrenVM GetVM(IntPtr ptr)
        {
            _vmsByPtr.TryGetValue(ptr, out WrenVM vm);
            return vm;
        }

        /// <summary>
        /// Gets the managed <see cref="WrenVM"/> wrapping the native VM at <paramref name="ptr"/>, cast as <typeparamref name="T"/>.
        /// </summary>
        /// <param name="ptr">The native Wren VM pointer.</param>
        /// <returns>A <typeparamref name="T"/> instance.</returns>
        public static T GetVM<T>(IntPtr ptr) where T : WrenVM
        {
            _vmsByPtr.TryGetValue(ptr, out WrenVM vm);
            return (T)vm;
        }

        /// <summary>
        /// Attempts to get the managed <see cref="WrenVM"/> wrapping the native VM at <paramref name="ptr"/>.
        /// </summary>
        /// <param name="ptr">The pointer to the native <see cref="WrenVM"/>.</param>
        /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
        /// <returns>True if the VM exists, otherwise false.</returns>
        public static bool TryGetVM(IntPtr ptr, out WrenVM vm)
        {
            if (_vmsByPtr.TryGetValue(ptr, out vm) && vm != null)
                return true;

            vm = default;
            return false;
        }

        /// <summary>
        /// Attempts to get the managed <see cref="WrenVM"/> wrapping the native VM at <paramref name="ptr"/>, cast as <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">The expected type of the VM.</typeparam>
        /// <param name="ptr">The pointer to the native <see cref="WrenVM"/>.</param>
        /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
        /// <returns>True if the VM exists and is of type <typeparamref name="T"/>, otherwise false.</returns>
        public static bool TryGetVM<T>(IntPtr ptr, out T vm) where T : WrenVM
        {
            if (_vmsByPtr.TryGetValue(ptr, out WrenVM untypedVm) && untypedVm is T typedVm)
            {
                vm = typedVm;
                return true;
            }

            vm = default;
            return false;
        }

        #endregion

        /// <summary>
        /// The native VM pointer.
        /// </summary>
        protected internal IntPtr m_Ptr;

        private readonly object m_HandleLocker = new object();
        private readonly Queue<WrenHandleInternal> m_PooledHandles = new Queue<WrenHandleInternal>();
        private readonly HashSet<WrenHandleInternal> m_ActiveHandles = new HashSet<WrenHandleInternal>();
        private readonly WrenSharedDataTable m_SharedData = new WrenSharedDataTable();
        private readonly List<WrenError> m_Errors = new List<WrenError>(0);
        private IAllocator m_Allocator;

        // The native config struct needs to be kept on the heap as it keeps function pointers to delegates
        // that Wren calls, so they must remain alive for as long as the VM is active.
        private WrenConfiguration m_Config;

        // Cached call handles to Fn.call(...) methods, one for each arity
        // The handles are created on demand and cached in this array for faster access
        private readonly WrenCallHandle[] m_FnCallHandles = new WrenCallHandle[MaxCallParameters + 1];

        // Call handle: clear()
        // Used by List and Map helper methods
        private WrenCallHandle m_CallHandle_Clear;

        // Unmanaged buffer used for encoding strings from StringBuilder
        private byte* m_StringBuffer = (byte*)IntPtr.Zero;
        private int m_StringBufferSize = 0;

        // Unmanaged buffer for storing VM user data
        // This isn't particularly useful in a managed environment, but available to offer parity with the full Wren API.
        private IntPtr m_UserDataBuffer = IntPtr.Zero;
        private int m_UserDataBufferSize = 0;

        private bool m_Initialized;
        private bool m_Disposed;

        #region Properties

        /// <summary>
        /// Gets the Wren API slot interface for interfacing with API slots on the current Wren fiber.
        /// </summary>
        public readonly WrenSlots Slots;

        /// <summary>
        /// Indicates if the VM has been disposed, either by an explicit call to <see cref="IDisposable.Dispose"/>
        /// or by the garbage collector when the instance is finalized.
        /// </summary>
        public bool IsDisposed => m_Disposed;

        /// <summary>
        /// Indicates if the VM has been initialized.
        /// </summary>
        public bool IsInitialized => m_Initialized;

        /// <summary>
        /// Indicates if the VM is initialized and in valid state. Returns true if 
        /// <see cref="IsInitialized"/> is true and <see cref="IsDisposed"/> is false.
        /// </summary>
        public bool IsValid => m_Initialized && !m_Disposed;

        /// <summary>
        /// A list containing the morst recent errors generated by Wren. This list is only filled
        /// if <see cref="WrenVMConfiguration.LogErrors"/> is true. The error log is cleared before
        /// any Wren source is interpreted or method called.
        /// </summary>
        public IReadOnlyList<WrenError> Errors => m_Errors;

        /// <summary>
        /// Determines if errors are logged and stored in the <see cref="Errors"/> list when Wren
        /// generates an error message during an interpret or call.
        /// </summary>
        public bool LogErrors { get; set; } = true;

        /// <summary>
        /// The pointer to the native WrenVM instance. This can be used to pass directly to the native interop
        /// methods in <see cref="Wren"/>.
        /// </summary>
        public IntPtr NativePointer => m_Ptr;

        /// <summary>
        /// The shared data table that can be used to store strong references to managed objects that can 
        /// be referenced by Wren foreign objects.
        /// </summary>
        public WrenSharedDataTable SharedData => m_SharedData;

        /// <summary>
        /// Custom user data that you can attach to this VM instance.
        /// </summary>
        public object HostData { get; set; }

        #endregion

        /// <summary>
        /// Base constructor for derived classes to perform their own initialization.
        /// </summary>
        protected WrenVM()
        {
            m_Allocator = null!;
            Slots = new WrenSlots(this);
        }

        /// <summary>
        /// Creates a new Wren VM to run Wren scripts. This constructor provides the ability to explicitly
        /// set the native configuration passed into the Wren instance, allowing for full control over the VM.
        /// </summary>
        /// <param name="config">The <see cref="WrenConfiguration"/>.</param>
        /// <param name="allocator">The <see cref="IAllocator"/> to use when allocated unmanaged memory from C#. This is not used by the native Wren VM instance.</param>
        public WrenVM(ref WrenConfiguration config, IAllocator allocator = default) : this()
        {
            Initialize(null, ref config, allocator);
        }

        /// <summary>
        /// Creates a new Wren VM to run Wren scripts. This constructor provides the ability to explicitly
        /// set the delete used to initialize the native Wren VM, as well as the native configuration passed into the Wren instance,
        /// allowing for full control over the VM initializer.
        /// </summary>
        /// <param name="vmInitializer">The <see cref="WrenVMInitializer"/> delegate to initialize the native Wren VM. Leave null to use the default initializer (<see cref="Wren.NewVM(ref WrenConfiguration)"/>).</param>
        /// <param name="config">The <see cref="WrenConfiguration"/>.</param>
        /// <param name="allocator">The <see cref="IAllocator"/> to use when allocated unmanaged memory from C#. This is not used by the native Wren VM instance.</param>
        public WrenVM(WrenVMInitializer vmInitializer, ref WrenConfiguration config, IAllocator allocator = default) : this()
        {
            Initialize(vmInitializer, ref config, allocator);
        }

        /// <summary>
        /// Initializes the VM using the given configuration.
        /// </summary>
        /// <param name="vmInitializer">The initializer to use to create the native Wren VM. Leave null to use the defaultinitializer(<see cref="Wren.NewVM(ref WrenConfiguration)"/>).</param>
        /// <param name="config">The <see cref="WrenConfiguration"/> that is passed to native Wren.</param>
        /// <param name="allocator">An optional memory allocator to use for unamanged buffers. If null, the
        /// default allocator (<see cref="HGlobalAllocator"/>) is used.</param>
        /// <exception cref="InvalidOperationException">Thrown if the VM has already been initialized.</exception>
        protected void Initialize(WrenVMInitializer vmInitializer, ref WrenConfiguration config, IAllocator allocator)
        {
            if (m_Initialized)
                throw new InvalidOperationException("A WrenVM instance can only be initialized once.");

            m_Config = config;
            m_Allocator = allocator ?? HGlobalAllocator.Default;
            m_Ptr = vmInitializer == null ? Wren.NewVM(ref m_Config) : vmInitializer(ref m_Config);
            if (m_Ptr == IntPtr.Zero)
            {
                throw new WrenInitializationException(this, "Failed to intiailize WrenVM");
            }

            lock (_vmsByPtr)
            {
                _vmsByPtr[m_Ptr] = this;
            }

            m_Initialized = true;
        }

        #region Interpret

        /// <summary>
        /// Runs Wren source <paramref name="source"/> in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The source to interpret.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public WrenInterpretResult Interpret(string module, IWrenSource source, bool throwOnFailure = false)
        {
            return Interpret(module, source.GetSourceBytes(out _), throwOnFailure);
        }

        /// <summary>
        /// Runs Wren source <paramref name="source"/> in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The source to interpret.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public WrenInterpretResult Interpret(string module, string source, bool throwOnFailure = false)
        {
            InterpretBegin();

            // NOTE: Allowing p/invoke with ANSI charset to marshal the string is faster than the custom
            // encoding method that is used in the explicit encoding/StringBuilder overloads, so we
            // use that here.
            WrenInterpretResult result = Wren.Interpret(m_Ptr, module, source);

            InterpretEnd(result, throwOnFailure);
            return result;
        }

        /// <summary>
        /// Runs Wren source <paramref name="source"/> in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The source to interpret.</param>
        /// <param name="encoding">The encoding to use when converting the string to a native buffer.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public unsafe WrenInterpretResult Interpret(string module, ReadOnlySpan<char> source, Encoding encoding = null, bool throwOnFailure = false)
        {
            encoding ??= Encoding.UTF8;
            WrenInternal.EncodeTextBufferFromString
            (
                source,
                encoding,
                m_Allocator,
                ref m_StringBuffer,
                ref m_StringBufferSize,
                out _, out _
            );

            InterpretBegin();

            WrenInterpretResult result = Wren.Interpret(m_Ptr, module, (IntPtr)m_StringBuffer);

            InterpretEnd(result, throwOnFailure);
            return result;
        }

        /// <summary>
        /// Runs Wren source (the contents of <paramref name="source"/>) in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The source to interpret.</param>
        /// <param name="encoding">The encoding to use when converting the string to a native buffer. Defaults to UTF8 encoding if not specified.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public unsafe WrenInterpretResult Interpret(string module, StringBuilder source, Encoding encoding = null, bool throwOnFailure = false)
        {
            encoding ??= Encoding.UTF8;
            WrenInternal.EncodeTextBufferFromStringBuilder
            (
                source,
                encoding,
                m_Allocator,
                ref m_StringBuffer,
                ref m_StringBufferSize,
                out _, out _
            );

            InterpretBegin();

            WrenInterpretResult result = Wren.Interpret(m_Ptr, module, (IntPtr)m_StringBuffer);

            InterpretEnd(result, throwOnFailure);
            return result;
        }

        /// <summary>
        /// Runs Wren source (the contents of <paramref name="source"/>) in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.<para />
        /// <paramref name="source"/> is expected to contain a null-terminated C-style string that is either ANSI or UTF8 encoded.<para />
        /// For UTF8 encoding, is recommended that the string contain the UTF8 BOM (Byte Order Mark) at the beginning of the file, or Wren may fail
        /// to parse the source string correctly.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">The raw bytes of Wren source to interpret.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public WrenInterpretResult Interpret(string module, byte[] source, bool throwOnFailure = false)
        {
            InterpretBegin();

            WrenInterpretResult result = Wren.Interpret(m_Ptr, module, source);

            InterpretEnd(result, throwOnFailure);
            return result;
        }

        /// <summary>
        /// Runs Wren source (the contents of <paramref name="source"/>) in a new fiber, in the context of module <paramref name="module"/>.
        /// If the resolved module is not found, a new module will be created.<para />
        /// <paramref name="source"/> should point to a null-terminated C-style string that is either ANSI or UTF8 encoded.<para />
        /// For UTF8 encoding, is recommended that the string contain the UTF8 BOM (Byte Order Mark) at the beginning of the file, or Wren may fail
        /// to parse the source string correctly.
        /// </summary>
        /// <param name="module">The name of the resolved module to run the source within.</param>
        /// <param name="source">A pointer to the Wren source to interpret.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>The result of the interpret operation.</returns>
        public WrenInterpretResult Interpret(string module, IntPtr source, bool throwOnFailure = false)
        {
            InterpretBegin();

            WrenInterpretResult result = Wren.Interpret(m_Ptr, module, source);

            InterpretEnd(result, throwOnFailure);
            return result;
        }

        #endregion

        #region Public API

        /// <summary>
        /// Sets the current fiber to be aborted, and uses the value in <paramref name="errorSlot"/> as the runtime error. Usually
        /// this would be a string that can be read in Wren via the <c>Fibre.error</c> instance field.
        /// </summary>
        /// <param name="errorSlot">The slot containing the error value.</param>
        /// <seealso cref="AbortFiber(int, string)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AbortFiber(int errorSlot) => Wren.AbortFiber(m_Ptr, errorSlot);

        /// <summary>
        /// Places the string <paramref name="errorMessage"/> in <paramref name="errorSlot"/>, then aborts the current fibre with
        /// <paramref name="errorSlot"/>.
        /// </summary>
        /// <param name="errorSlot">The slot to contain <paramref name="errorMessage"/>.</param>
        /// <param name="errorMessage">The error message to set for the abort.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AbortFiber(int errorSlot, string errorMessage)
        {
            Wren.SetSlotString(m_Ptr, errorSlot, errorMessage);
            Wren.AbortFiber(m_Ptr, errorSlot);
        }

        /// <summary>
        /// Clears all stored <see cref="WrenError"/> instances in <see cref="Errors"/>.
        /// </summary>
        /// <seealso cref="Errors"/>
        /// <seealso cref="WrenError"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClearErrorLog() => m_Errors.Clear();

        /// <summary>
        /// Runs the VM's garbage collector immediately, freeing any unused memory.
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void CollectGarbage() => Wren.CollectGarbage(m_Ptr);

        /// <summary>
        /// Indicates if the module <paramref name="module"/> has been imported and resolved.
        /// </summary>
        /// <param name="module">The name of the module.</param>
        /// <returns>True if <paramref name="module"/> has been imported and resolved, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool HasModule(string module) => Wren.HasModule(m_Ptr, module) != 0;

        /// <summary>
        /// Writes <paramref name="text"/>, followed by a newline, using the Wren writer function.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public WrenVM Print(string text) => Write(text + "\n");

        /// <summary>
        /// Writes <paramref name="text"/> using the Wren writer function.
        /// </summary>
        /// <param name="text">The text to write.</param>
        public WrenVM Write(string text)
        {
            m_Config.Write?.Invoke(m_Ptr, text);
            return this;
        }

        #region Calls

        /// <summary>
        /// Calls a Wren value via a previously allocated handle.
        /// </summary>
        /// <param name="handle">The handle to call.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if the call is unsuccessful.</param>
        /// <returns>The result of the call operation.</returns>
        /// <exception cref="WrenInterpretException">Thrown if the call is unsuccessful and <paramref name="throwOnFailure"/> is true.</exception>
        /// <seealso cref="CreateHandle(int)"/>
        /// <seealso cref="CreateHandle(string, string, int)"/>
        public WrenInterpretResult Call(WrenHandle handle, bool throwOnFailure = true)
        {
            EnsureValidHandle(handle);
            InterpretBegin();
            WrenInterpretResult result = Wren.Call(m_Ptr, handle.m_Ptr);
            InterpretEnd(result, throwOnFailure);
            return result;
        }

        /// <summary>
        /// Calls a Wren method via a previously allocated handle.
        /// </summary>
        /// <param name="callHandle">The handle to call.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if the call is unsuccessful.</param>
        /// <returns>The result of the call operation.</returns>
        /// <exception cref="WrenInterpretException">Thrown if the call is unsuccessful and <paramref name="throwOnFailure"/> is true.</exception>
        /// <seealso cref="CreateCall(WrenHandle, WrenCallHandle, bool)"/>
        /// <seealso cref="CreateCallHandle(string)"/>
        public WrenInterpretResult Call(WrenCallHandle callHandle, bool throwOnFailure = true)
        {
            EnsureValidHandle(callHandle);
            InterpretBegin();
            WrenInterpretResult result = Wren.Call(m_Ptr, callHandle.m_Ptr);
            InterpretEnd(result, throwOnFailure);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="WrenCall"/> value that can be used to prepare and execute a call.
        /// </summary>
        /// <param name="receiverHandle">A handle to the receiver object.</param>
        /// <param name="callHandle">A handle for the method to call.</param>
        /// <param name="createNewFiber">If true, a new Wren Fiber is created to execute the call. This allows for foreign methods called from within Wren
        /// to call back into Wren from the managed side without clobbering the Wren API stack.</param>
        /// <returns>A <see cref="WrenCall"/> value.</returns>
        public WrenCall CreateCall(WrenHandle receiverHandle, WrenCallHandle callHandle, bool createNewFiber = false)
        {
            EnsureValidHandle(receiverHandle);
            EnsureValidHandle(callHandle);
            return new WrenCall(this, receiverHandle, callHandle, createNewFiber);
        }

        /// <summary>
        /// Creates a <see cref="WrenCall"/> value that can be used to prepare and execute a call to a static class method.
        /// </summary>
        /// <param name="module">The name of the module the class resides in.</param>
        /// <param name="className">The name of the class to execute the call on.</param>
        /// <param name="callHandle">A handle for the method to call.</param>
        /// <param name="createNewFiber">If true, a new Wren Fiber is created to execute the call. This allows for foreign methods called from within Wren
        /// to call back into Wren from the managed side without clobbering the Wren API stack.</param>
        /// <returns>A <see cref="WrenCall"/> value.</returns>
        public WrenCall CreateCall(string module, string className, WrenCallHandle callHandle, bool createNewFiber = false)
        {
            EnsureValidHandle(callHandle);
            return new WrenCall(this, module, className, callHandle, createNewFiber);
        }

        /// <summary>
        /// Creates a <see cref="WrenCall"/> for a Wren Fn object, that can be used to prepare and execute the call.
        /// </summary>
        /// <param name="functionHandle">A handle wrapping a Wren Fn object.</param>
        /// <param name="argCount">The number of arguments the function expects.</param>
        /// <param name="createNewFiber">If true, a new Wren Fiber is created to execute the call. This allows for foreign methods called from within Wren
        /// to call back into Wren from the managed side without clobbering the Wren API stack.</param>
        /// <returns>A <see cref="WrenCall"/> value.</returns>
        public WrenCall CreateFunctionCall(WrenHandle functionHandle, int argCount, bool createNewFiber = false)
        {
            WrenCallHandle callHandle = GetFunctionCallHandle(argCount);
            return new WrenCall(this, functionHandle, callHandle, createNewFiber);
        }

        #endregion

        #region Handles

        /// <summary>
        /// Creates a <see cref="WrenCallHandle"/> that can be used with the <see cref="Call(WrenCallHandle, bool)"/> and <see cref="CreateCall(WrenHandle, WrenCallHandle, bool)"/> methods
        /// to call a Wren method from C#.<para />
        /// Call signatures must conform to standard Wren call signatures, for example:
        /// <code>
        /// methodName()      // Method, no arguments
        /// methodName(_,_)   // Method, two arguments
        /// propertyName      // Property getter (no arguments)
        /// propertyName=(_)  // Property setter (one argument)
        /// [_]               // Subscript getter, one argument
        /// [_,_]=(_)         // Subscript setter, two arguments
        /// -                 // Unary negation (no arguments)
        /// +(_)              // Addition operator (one argument)
        /// </code>
        /// See <see href="https://wren.io/method-calls.html"/>  for more information on Wren call signatures.
        /// </summary>
        /// <param name="signature">The call signature.</param>
        /// <returns></returns>
        public WrenCallHandle CreateCallHandle(string signature)
        {
            var paramCount = WrenUtils.GetParameterCount(signature);
            if (paramCount == MaxCallParameters)
                throw new ArgumentException("Signature exceeds maximum parameter count.");

            var ptr = Wren.MakeCallHandle(m_Ptr, signature);
            return new WrenCallHandle(AcquireHandle(ptr), paramCount);
        }

        /// <summary>
        /// Interprets a Wren function by wrapping <paramref name="functionBody"/> in the Wren function syntax (<c>Fn.new {...}</c>)
        /// and returns a handle to the newly created function object. This is useful for creating functions dynamically from managed code
        /// that can then be called at a later time.<para/>
        /// Use <see cref="CreateFunctionCall(WrenHandle, int, bool)"/> to call the function after it has been created.<para/>
        /// See <see href="https://wren.io/functions.html"/> for more information on functions in Wren.
        /// </summary>
        /// <example>
        /// <code>
        /// var dynamicFn = vm.CreateFunction(
        ///     module: "main",
        ///     paramsSignature: "active, num",
        ///     functionBody: @"
        ///         System.print(""arguments: active=%(active), num=%(num)"")
        ///         return num * 2"
        ///     );
        /// 
        /// // If the function did not compile, the handle will be invalid
        /// if (dynamicFn.IsValid)
        /// {
        ///     var call = vm.CreateFunctionCall(dynamicFn, 2);
        ///     call.SetArg(0, true); // arg: active
        ///     call.SetArg(1, 1234); // arg: num
        ///     call.Call(throwOnFailure: true, out double returnValue);
        ///     
        ///     Console.WriteLine(returnValue); // Output: 2468
        /// }
        /// </code>
        /// </example>
        /// <param name="module">The module to create the function in.</param>
        /// <param name="paramsSignature">The function's parameter signature.</param>
        /// <param name="functionBody">The function's body.</param>
        /// <param name="slot">The slot index to load the function into and create the handle from.</param>
        /// <param name="throwOnFailure">If true, a <see cref="WrenInterpretException"/> will be thrown if an unsuccessful result is returned.</param>
        /// <returns>A handle to the newly created function.</returns>
        public WrenHandle CreateFunction(string module, ReadOnlySpan<char> paramsSignature, ReadOnlySpan<char> functionBody, int slot = 0, bool throwOnFailure = false)
        {
            // This method works by interpreting dynamically created Wren source that assigns
            // a new Wren function to a variable, then creating a handle to that variable. This
            // is done in the internal WrenSharp module to hide this functionality.
            const string varName = "ws__dynFn__";

            // Ensure the target variable exists so we can assign it
            // Unfortunately we need to look this up by name every time, as Wren does not provide
            // a way to get a handle to a *variable*, only values/objects.
            if (!HasModule(module) || !HasVariable(module, varName))
            {
                Interpret(module, $"var {varName}");
            }

            int len = 16 + paramsSignature.Length + functionBody.Length + varName.Length;
            var sb = StringBuilderCache.Acquire(len);
            sb.Append(varName)
              .Append(" = Fn.new {");

            if (paramsSignature.Length > 0)
            {
                sb
                  .Append('|')
                  .Append(paramsSignature)
                  .Append('|');
            }

            sb
              .Append('\n')
              .Append(functionBody)
              .Append("\n}");

            var script = StringBuilderCache.GetStringAndRelease(sb);
            var result = Interpret(module, script, throwOnFailure);
            if (result != WrenInterpretResult.Success)
                return default;

            LoadVariable(module, varName, slot);
            return CreateHandle(slot);
        }

        /// <summary>
        /// Creates a <see cref="WrenHandle"/> that wraps the value in <paramref name="slot"/>.
        /// </summary>
        /// <param name="slot">The slot containing the value to wrap.</param>
        /// <returns>A <see cref="WrenHandle"/> value.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle CreateHandle(int slot)
        {
            var ptr = Wren.GetSlotHandle(m_Ptr, slot);
            return new WrenHandle(AcquireHandle(ptr));
        }

        /// <summary>
        /// Creates a handle to <paramref name="variableName"/> in resolved module <paramref name="module"/>.
        /// The value of the variable is loaded into slot <paramref name="slot"/>, and the handle is created from the value in that slot.
        /// </summary>
        /// <param name="module">The module the variable resides in.</param>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="slot">The slot to place the value in and create the handle from.</param>
        /// <returns>A <see cref="WrenHandle"/> wrapping the value stored in <paramref name="variableName"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenHandle CreateHandle(string module, string variableName, int slot = 0)
        {
            Wren.GetVariable(m_Ptr, module, variableName, slot);
            return CreateHandle(slot);
        }

        /// <summary>
        /// Returns a cached <see cref="WrenCallHandle"/> for the Wren <c>Fn.call()</c> method signature with <paramref name="paramCount"/> parameters.
        /// The maximum number of parameters is <see cref="MaxCallParameters"/> (16 at the time of writing).
        /// </summary>
        /// <param name="paramCount">The number of parameters in the method signature.</param>
        /// <returns>A <see cref="WrenCallHandle"/>.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="paramCount"/> is greater than <see cref="MaxCallParameters"/>.</exception>
        public WrenCallHandle GetFunctionCallHandle(int paramCount)
        {
            if (paramCount > MaxCallParameters)
                throw new ArgumentOutOfRangeException(nameof(paramCount), "Max call parameter count is " + MaxCallParameters);

            WrenCallHandle callHandle = m_FnCallHandles[paramCount];
            if (!callHandle.IsValid)
            {
                string sig = WrenUtils.MethodSignature("call", paramCount);
                callHandle = CreateCallHandle(sig);
                m_FnCallHandles[paramCount] = callHandle;
            }

            return callHandle;
        }


        /// <summary>
        /// Attemps to create a <see cref="WrenHandle"/> from the value in <paramref name="slot"/>.
        /// Returns true if <paramref name="slot"/> is less than the nubmer of slots and holds a value of <paramref name="expectedType"/>.
        /// </summary>
        /// <param name="slot">The slot containing the value to wrap.</param>
        /// <param name="expectedType">The expected type of the value in <paramref name="slot"/>.</param>
        /// <param name="handle">Stores the created handle.</param>
        /// <returns>True if the handle was created, otherwise false.</returns>
        public bool TryCreateHandle(int slot, WrenType expectedType, out WrenHandle handle)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.GetSlotType(m_Ptr, slot) == expectedType)
            {
                handle = CreateHandle(slot);
                return true;
            }

            handle = default;
            return false;
        }

        /// <summary>
        /// Attemps to create a <see cref="WrenHandle"/> from the value in <paramref name="variableName"/> in resolved module <paramref name="module"/>.
        /// The value of the variable is loaded into slot <paramref name="slot"/>, and the handle is created from the value in that slot.<para/>
        /// Returns true if <paramref name="slot"/> is less than the number of slots and the variable exists.
        /// </summary>
        /// <param name="module">The module the variable resides in.</param>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="slot">The slot to place the value in, from which the handle will be created.</param>
        /// <param name="handle">Stores the created handle.</param>
        /// <returns>True if the handle was created, otherwise false.</returns>
        public bool TryCreateHandle(string module, string variableName, int slot, out WrenHandle handle)
        {
            if (Wren.GetSlotCount(m_Ptr) > slot && Wren.HasVariable(m_Ptr, module, variableName) != 0)
            {
                Wren.GetVariable(m_Ptr, module, variableName, slot);
                handle = CreateHandle(slot);
                return true;
            }

            handle = default;
            return false;
        }

        /// <summary>
        /// Attemps to create a <see cref="WrenHandle"/> from the value in <paramref name="variableName"/> in resolved module <paramref name="module"/>.
        /// The value of the variable is loaded into slot 0, and the handle is created from the value in that slot.<para/>
        /// Returns true if there is at least one slot and the variable exists.
        /// </summary>
        /// <param name="module">The module the variable resides in.</param>
        /// <param name="variableName">The name of the variable.</param>
        /// <param name="handle">Stores the created handle.</param>
        /// <returns>True if the handle was created, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryCreateHandle(string module, string variableName, out WrenHandle handle) => TryCreateHandle(module, variableName, 0, out handle);


        /// <summary>
        /// Releases all allocated handles.
        /// </summary>
        public void ReleaseAllHandles()
        {
            if (m_ActiveHandles.Count <= 0)
                return;

            lock (m_HandleLocker)
            {
                foreach (WrenHandleInternal handle in m_ActiveHandles)
                {
                    Wren.ReleaseHandle(m_Ptr, handle.Ptr);
                    handle.Ptr = IntPtr.Zero;

                    m_PooledHandles.Enqueue(handle);
                }

                m_ActiveHandles.Clear();
            }
        }

        /// <summary>
        /// Releases a handle. Does nothing if the handle is not valid, or has already been released.
        /// </summary>
        /// <param name="handle">The handle to release.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="handle"/> is not owned by this <see cref="WrenVM"/> instance.</exception>
        public void ReleaseHandle(in WrenHandle handle)
        {
            if (!handle.IsValid)
                return;

            ReleaseHandle(handle.m_Handle);
        }

        /// <summary>
        /// Releases a call handle. Does nothing if the handle is not valid, or has already been released.
        /// </summary>
        /// <param name="handle">The handle to release.</param>
        /// <exception cref="ArgumentException">Thrown if <paramref name="handle"/> is not owned by this <see cref="WrenVM"/> instance.</exception>
        public void ReleaseHandle(in WrenCallHandle handle)
        {
            if (!handle.IsValid)
                return;

            ReleaseHandle(handle.m_Handle);
        }

        #endregion

        #endregion

        #region Protected API

        /// <summary>
        /// Provides a method for derived classes to hook the beginning of an interpret or method call.
        /// </summary>
        protected virtual void OnInterpretBegin() { }

        /// <summary>
        /// Provides a method for derived classes to hook to the end of an interpret or method call.
        /// </summary>
        protected virtual void OnInterpretEnd(WrenInterpretResult result) { }

        /// <summary>
        /// Logs a Wren error to <see cref="Errors"/>, if <see cref="LogErrors"/> is <c>true</c>.
        /// If <see cref="LogErrors"/> is <c>false</c>, this method does nothing.
        /// </summary>
        /// <param name="errorType">The <see cref="WrenErrorType"/>.</param>
        /// <param name="module">The name of the module the error occured in.</param>
        /// <param name="lineNumber">The line number of the source the error occured in.</param>
        /// <param name="message">The error message.</param>
        protected void LogError(WrenErrorType errorType, string module, int lineNumber, string message)
        {
            if (!LogErrors)
                return;

            m_Errors.Add(new WrenError(errorType, module, lineNumber, message));
        }

        #endregion

        private void CallClearMethod()
        {
            if (!m_CallHandle_Clear.IsValid)
            {
                m_CallHandle_Clear = CreateCallHandle("clear()");
            }
            
            Wren.Call(m_Ptr, m_CallHandle_Clear.m_Ptr);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InterpretBegin()
        {
            m_Errors.Clear();
            OnInterpretBegin();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void InterpretEnd(WrenInterpretResult result, bool throwOnFailure)
        {
            OnInterpretEnd(result);

            if (throwOnFailure && result != WrenInterpretResult.Success)
                throw new WrenInterpretException(this, result);
        }


        private WrenHandleInternal AcquireHandle(IntPtr handlePtr)
        {
            WrenHandleInternal handle;
            lock (m_HandleLocker)
            {
                handle = m_PooledHandles.Count > 0
                    ? m_PooledHandles.Dequeue()
                    : new WrenHandleInternal(this);

                handle.Ptr = handlePtr;
                handle.Version++;
                m_ActiveHandles.Add(handle);
            }
            return handle;
        }

        private void ReleaseHandle(WrenHandleInternal handle)
        {
            if (handle == null || !handle.IsValid())
                return;

            if (handle.VM != this)
                throw new ArgumentException("Handle is not owned by this VM.");

            Wren.ReleaseHandle(m_Ptr, handle.Ptr);
            handle.Ptr = IntPtr.Zero;

            lock (m_HandleLocker)
            {
                m_PooledHandles.Enqueue(handle);
                m_ActiveHandles.Remove(handle);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureValidHandle(in WrenHandle handle)
        {
            if (!handle.IsValid)
            {
                throw new WrenInvalidHandleException(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureValidHandle(in WrenCallHandle handle)
        {
            if (!handle.IsValid)
            {
                throw new WrenInvalidHandleException(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureValidHandle(in WrenSharedDataHandle handle)
        {
            if (!handle.IsValid)
            {
                throw new WrenInvalidHandleException(this);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void EnsureType(WrenType type, WrenType expectedType)
        {
            if (type != expectedType)
                throw new WrenTypeException(this, $"Expected type {expectedType}, but received {type}");
        }

        #region IDisposable

        ~WrenVM()
        {
            Dispose(disposing: false);
        }

        /// <summary>
        /// Disposes of the VM and releases all allocated resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_Disposed)
            {
                if (disposing)
                {
                    // Dispose managed state
                    DisposeManagedState();

                    lock (_vmsByPtr)
                    {
                        _vmsByPtr.Remove(m_Ptr);
                    }

                    m_SharedData.Clear();
                    m_Errors.Clear();
                }

                // Dispose unmanaged resources
                DisposeUnmanagedState();
                ReleaseAllHandles();

                if (m_Ptr != IntPtr.Zero)
                {
                    Wren.FreeVM(m_Ptr);
                }

                if (m_StringBuffer != null)
                {
                    m_Allocator.Free((IntPtr)m_StringBuffer);
                    m_StringBuffer = null;
                    m_StringBufferSize = 0;
                }

                if (m_UserDataBuffer != IntPtr.Zero)
                {
                    m_Allocator.Free(m_UserDataBuffer);
                    m_UserDataBuffer = default;
                    m_UserDataBufferSize = 0;
                }
                
                m_Disposed = true;
            }
        }

        /// <summary>
        /// Invoked when the VM is being disposed. Override this method to dispose of any managed state.
        /// </summary>
        protected virtual void DisposeManagedState() { }

        /// <summary>
        /// Invoked when the VM is being disposed. Override this method to dispose of any unmanaged resources.
        /// </summary>
        protected virtual void DisposeUnmanagedState() { }

        #endregion
    }
}
