using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A builder for binding Wren foreign classes and methods.
    /// </summary>
    public sealed class WrenForeign
    {
        public delegate void Allocator(WrenVM vm);
        public delegate void Allocator<T>(WrenVM vm, ref T data) where T : unmanaged;
        public delegate void AllocatorCall(WrenCallContext call);
        public delegate void AllocatorCall<T>(WrenCallContext call, ref T data) where T : unmanaged;
        public delegate void Finalizer<T>(ref T data) where T : unmanaged;
        public delegate void Finalizer(IntPtr data);

        private readonly WrenVM m_Vm;
        private WrenNativeFn.ForeignMethod m_Allocator;
        private WrenNativeFn.Finalizer m_Finalizer;
        private Dictionary<string, WrenNativeFn.ForeignMethod> m_StaticMethods;
        private Dictionary<string, WrenNativeFn.ForeignMethod> m_InstanceMethods;

        internal WrenForeign(WrenVM vm)
        {
            m_Vm = vm;
        }

        #region Public API

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This is a bare allocator, and does not actually create any foreign class data. To do so, you must call one of the foreign instance constructor API methods:<para/>
        /// <see cref="WrenVM.SetSlotNewForeign(int, int, ulong)"/><br/>
        /// <see cref="WrenVM.SetSlotNewForeign{T}(int, int, in T)"/>
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this create the foriegn data and to set the initial state of the memory.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public WrenForeign Allocate(Allocator allocator)
        {
            m_Allocator = (_) => allocator(m_Vm);
            return this;
        }

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This is a bare allocator, and does not actually create any foreign class data. To do so, you must call one of the foreign instance constructor API methods:<para/>
        /// <see cref="WrenVM.SetSlotNewForeign(int, int, ulong)"/><br/>
        /// <see cref="WrenVM.SetSlotNewForeign{T}(int, int, in T)"/>
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this create the foriegn data and to set the initial state of the memory.</param>
        /// <param name="paramCount">The number of parameters expected in the constructor. Note that it is not possible to know which constructor was invoked on the Wren
        /// side from within the allocator. This parameter defaults to <see cref="WrenVM.MaxCallParameters"/>.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public WrenForeign Allocate(AllocatorCall allocator, byte paramCount = WrenVM.MaxCallParameters)
        {
            m_Allocator = (_) => allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, paramCount));
            return this;
        }

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public unsafe WrenForeign Allocate<T>() where T : unmanaged
        {
            m_Allocator = (vmPtr) =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(vmPtr, 0, 0, (ulong)sizeof(T));
                *data = new T();
            };
            return this;
        }

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this to set the initial state of the memory.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public unsafe WrenForeign Allocate<T>(Allocator<T> allocator) where T : unmanaged
        {
            m_Allocator = (vmPtr) =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(vmPtr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(m_Vm, ref *data);
            };
            return this;
        }

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this to set the initial state of the memory.</param>
        /// <param name="paramCount">The number of parameters expected in the constructor. Note that it is not possible to know which constructor was invoked on the Wren
        /// side from within the allocator. This parameter defaults to <see cref="WrenVM.MaxCallParameters"/>.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public unsafe WrenForeign Allocate<T>(AllocatorCall<T> allocator, byte paramCount = WrenVM.MaxCallParameters) where T : unmanaged
        {
            m_Allocator = (_) =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, paramCount), ref *data);
            };
            return this;
        }

        /// <summary>
        /// Sets the finalizer function called when an instance of this foreign class is cleaned up by the Wren garbage collector.<para />
        /// Use this method to free unmanaged memory that may have been allocated within the class's allocator or in any of its methods.
        /// </summary>
        /// <param name="finalizer">The finalizer delegate to call when the instance is garbage collected.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public WrenForeign Finalize(Finalizer finalizer)
        {
            m_Finalizer = (data) => finalizer(data);
            return this;
        }

        /// <summary>
        /// Sets the finalizer function called when an instance of this foreign class is cleaned up by the Wren garbage collector.<para />
        /// Use this method to free unmanaged memory that may have been allocated within the class's allocator or in any of its methods.
        /// </summary>
        /// <param name="finalizer">The finalizer delegate to call when the instance is garbage collected.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public unsafe WrenForeign Finalize<T>(Finalizer<T> finalizer) where T : unmanaged
        {
            m_Finalizer = (data) => finalizer(ref *(T*)data);
            return this;
        }

        /// <summary>
        /// Sets the delegate to be invoked when the foreign instance method <paramref name="signature"/> is called for this class.
        /// </summary>
        /// <param name="signature">The signature of the method.</param>
        /// <param name="method">The method delegate to invoke when called.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public WrenForeign Instance(string signature, WrenForeignMethod method)
        {
            InternalAddMethod(WrenMethodType.Instance, signature, method);
            return this;
        }

        /// <summary>
        /// Sets the delegate to be invoked when the foreign static method <paramref name="signature"/> is called for this class.
        /// </summary>
        /// <param name="signature">The signature of the method.</param>
        /// <param name="method">The method delegate to invoke when called.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public WrenForeign Static(string signature, WrenForeignMethod method)
        {
            InternalAddMethod(WrenMethodType.Static, signature, method);
            return this;
        }

        #endregion

        private void InternalAddMethod(WrenMethodType methodType, string signature, WrenForeignMethod method)
        {
            var paramCount = WrenUtils.GetParameterCount(signature);
            if (paramCount == WrenVM.MaxCallParameters)
                throw new ArgumentException("Signature exceeds maximum parameter count.");

            void fn(IntPtr _) => method(new WrenCallContext(m_Vm, methodType, paramCount));

            if (methodType == WrenMethodType.Static)
            {
                m_StaticMethods ??= new Dictionary<string, WrenNativeFn.ForeignMethod>();
                m_StaticMethods[signature] = fn;
            }
            else
            {
                m_InstanceMethods ??= new Dictionary<string, WrenNativeFn.ForeignMethod>();
                m_InstanceMethods[signature] = fn;
            }
        }

        #region Internal

        internal WrenNativeFn.ForeignMethod FindMethod(bool isStatic, string signature)
        {
            var methods = isStatic ? m_StaticMethods : m_InstanceMethods;
            if (methods == null)
                return default;

            methods.TryGetValue(signature, out var method);
            return method;
        }

        internal WrenForeignClassMethods GetClassMethods() => new WrenForeignClassMethods()
        {
            Allocate = m_Allocator,
            Finalize = m_Finalizer,
        };

        #endregion
    }
}
