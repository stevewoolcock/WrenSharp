using WrenSharp.Native;

namespace WrenSharp
{
    /// <summary>
    /// A builder for binding Wren foreign classes and methods.
    /// </summary>
    public sealed class WrenForeign : IWrenForeign
    {
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

        /// <inheritdoc/>
        public IWrenForeign Allocate(IWrenForeign.Allocator allocator)
        {
            m_Allocator = (_) => allocator(m_Vm);
            return this;
        }

        /// <inheritdoc/>
        public IWrenForeign Allocate(IWrenForeign.AllocatorCall allocator, byte paramCount = WrenVM.MaxCallParameters)
        {
            m_Allocator = (_) => allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, paramCount));
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Allocate<T>() where T : unmanaged
        {
            m_Allocator = (vmPtr) =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(vmPtr, 0, 0, (ulong)sizeof(T));
                *data = new T();
            };
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Allocate<T>(IWrenForeign.Allocator<T> allocator) where T : unmanaged
        {
            m_Allocator = (vmPtr) =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(vmPtr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(m_Vm, ref *data);
            };
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Allocate<T>(IWrenForeign.AllocatorCall<T> allocator, byte paramCount = WrenVM.MaxCallParameters) where T : unmanaged
        {
            m_Allocator = (_) =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, paramCount), ref *data);
            };
            return this;
        }

        /// <inheritdoc/>
        public IWrenForeign Finalize(IWrenForeign.Finalizer finalizer)
        {
            m_Finalizer = (data) => finalizer(data);
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Finalize<T>(IWrenForeign.Finalizer<T> finalizer) where T : unmanaged
        {
            m_Finalizer = (data) => finalizer(ref *(T*)data);
            return this;
        }

        /// <inheritdoc/>
        public IWrenForeign Instance(string signature, WrenForeignMethod method)
        {
            InternalAddMethod(WrenMethodType.Instance, signature, method);
            return this;
        }

        /// <inheritdoc/>
        public IWrenForeign Static(string signature, WrenForeignMethod method)
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

            void fn(IntPtr _) => method(new WrenCallContext(m_Vm, methodType, (byte)paramCount));

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
