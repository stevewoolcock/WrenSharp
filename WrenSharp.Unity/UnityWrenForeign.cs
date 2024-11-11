using System;
using System.Collections.Generic;
using UnityEngine;
using WrenSharp.Native;

namespace WrenSharp.Unity
{
    /// <summary>
    /// A builder for binding Wren foreign classes and methods.
    /// </summary>
    public sealed class UnityWrenForeign : IWrenForeign
    {
        //
        // Implementation notes:
        //
        // When using IL2CPP, marshalling p/invoke delegates through instances is not possible as there's no JIT
        // available at runtime to compile and perform the correct marshalling.
        //
        // Unfortunately, this means that all p/invoke function pointers into managed code must point to static
        // methods (in Unity, they must also be annotated with the AOT.MonoPInvokeCallback attribute).
        //
        // This solution for binding managed delegates to Wren foreign classes and methods uses a small modification
        // to the Wren source that attaches a uint16 symbol to foreign methods. The symbol is assigned on the managed
        // side (in this class, WrenForeign) and passed through as an argument when Wren calls the function pointer.
        //
        // This means that we can place every foreign method delegate into a static array and have Wren foreign
        // methods call to a static managed method which can use the symbol to lookup the managed delegate.
        //
        // This class provides the same public API as the standard WrenForeign.
        //

        // Internal class used to manage a collection of delegates for foreign method binding
        // Each delegate is given a symbol corresponding to an index within an array
        // Freed delegates return their symbol to the free stack for reuse
        private class DelegateTable<T> where T : Delegate
        {
            struct Entry
            {
                public ushort NextFree;
                public T Delegate;
            }

            private Entry[] m_Entries = Array.Empty<Entry>();
            private ushort m_FreeSymbol = 0;
            private ushort m_TailSymbol = 1;

            public ushort Add(T del)
            {
                ushort symbol;
                if (m_FreeSymbol > 0)
                {
                    symbol = m_FreeSymbol;
                    m_FreeSymbol = m_Entries[symbol - 1].NextFree;
                }
                else
                {
                    symbol = m_TailSymbol++;
                }

                if (m_Entries.Length < symbol)
                {
                    int newCapacity = m_Entries.Length == 0 ? 4 : m_Entries.Length * 2;
                    int minCapacity = symbol + 1;
                    if (newCapacity < minCapacity)
                    {
                        newCapacity = minCapacity;
                    }

                    Array.Resize(ref m_Entries, newCapacity);
                }

                m_Entries[symbol - 1] = new Entry()
                {
                    NextFree = 0,
                    Delegate = del,
                };

                return symbol;
            }

            public T Get(ushort symbol)
            {
                return m_Entries[symbol - 1].Delegate;
            }

            public void Remove(ushort symbol)
            {
                m_Entries[symbol - 1] = new Entry()
                {
                    NextFree = m_FreeSymbol,
                    Delegate = default,
                };
                m_FreeSymbol = symbol;
            }

            public void Clear()
            {
                Array.Clear(m_Entries, 0, m_TailSymbol - 1);
                m_TailSymbol = 1;
                m_FreeSymbol = 0;
            }
        }

        #region Static

        private static readonly WrenNativeFn.ForeignMethod _wrenForeignMethodFn = WrenForeignMethodFn;
        private static readonly WrenNativeFn.Finalizer _wrenFinalizerMethodFn = WrenFinalizerMethodFn;

        private static readonly DelegateTable<Action> _methodTable = new DelegateTable<Action>();
        private static readonly DelegateTable<Action<IntPtr>> _finalizerTable = new DelegateTable<Action<IntPtr>>();

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.ForeignMethod))]
        private static void WrenForeignMethodFn(IntPtr vm, ushort symbol)
        {
            if (symbol > 0)
            {
                _methodTable.Get(symbol)();
            }
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.Finalizer))]
        private static void WrenFinalizerMethodFn(IntPtr data, ushort symbol)
        {
            if (symbol > 0)
            {
                _finalizerTable.Get(symbol)(data);
            }
        }

        // Support disabled domain reloads in the Unity editor
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration()
        {
            _methodTable.Clear();
            _finalizerTable.Clear();
        }

        #endregion

        private readonly UnityWrenVM m_Vm;
        private ushort m_Allocator;
        private ushort m_Finalizer;
        private Dictionary<string, ushort> m_StaticMethods;
        private Dictionary<string, ushort> m_InstanceMethods;

        internal UnityWrenForeign(UnityWrenVM vm)
        {
            m_Vm = vm;
        }

        internal void Dispose()
        {
            if (m_Allocator > 0) _methodTable.Remove(m_Allocator);
            if (m_Finalizer > 0) _finalizerTable.Remove(m_Finalizer);

            if (m_StaticMethods != null)
            {
                foreach (var symbol in m_StaticMethods.Values)
                {
                    _methodTable.Remove(symbol);
                }
            }

            if (m_InstanceMethods != null)
            {
                foreach (var symbol in m_InstanceMethods.Values)
                {
                    _methodTable.Remove(symbol);
                }
            }

            m_Allocator = 0;
            m_Finalizer = 0;
            m_StaticMethods = null;
            m_InstanceMethods = null;
        }

        #region Public API

        /// <inheritdoc/>
        public IWrenForeign Allocate(IWrenForeign.Allocator allocator)
        {
            m_Allocator = _methodTable.Add(() => allocator(m_Vm));
            return this;
        }

        /// <inheritdoc/>
        public IWrenForeign Allocate(IWrenForeign.AllocatorCall allocator, byte paramCount = WrenVM.MaxCallParameters)
        {
            m_Allocator = _methodTable.Add(() => allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, paramCount)));
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Allocate<T>() where T : unmanaged
        {
            m_Allocator = _methodTable.Add(() =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
            });
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Allocate<T>(IWrenForeign.Allocator<T> allocator) where T : unmanaged
        {
            m_Allocator = _methodTable.Add(() =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(m_Vm, ref *data);
            });
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Allocate<T>(IWrenForeign.AllocatorCall<T> allocator, byte paramCount = WrenVM.MaxCallParameters) where T : unmanaged
        {
            m_Allocator = _methodTable.Add(() =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, paramCount), ref *data);
            });
            return this;
        }

        /// <inheritdoc/>
        public IWrenForeign Finalize(IWrenForeign.Finalizer finalizer)
        {
            m_Finalizer = _finalizerTable.Add((data) => finalizer(data));
            return this;
        }

        /// <inheritdoc/>
        public unsafe IWrenForeign Finalize<T>(IWrenForeign.Finalizer<T> finalizer) where T : unmanaged
        {
            m_Finalizer = _finalizerTable.Add((data) => finalizer(ref *(T*)data));
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

            var symbol = _methodTable.Add(() => method(new WrenCallContext(m_Vm, methodType, (byte)paramCount)));

            if (methodType == WrenMethodType.Static)
            {
                m_StaticMethods ??= new Dictionary<string, ushort>();
                m_StaticMethods[signature] = symbol;
            }
            else
            {
                m_InstanceMethods ??= new Dictionary<string, ushort>();
                m_InstanceMethods[signature] = symbol;
            }
        }

        internal WrenForeignMethodData FindMethod(bool isStatic, string signature)
        {
            var methods = isStatic ? m_StaticMethods : m_InstanceMethods;
            if (methods == null)
                return WrenForeignMethodData.NotFound;

            if (!methods.TryGetValue(signature, out var symbol))
                return WrenForeignMethodData.NotFound;

            return new WrenForeignMethodData()
            {
                Symbol = symbol,
                Function = _wrenForeignMethodFn,
            };
        }

        internal WrenForeignClassMethods GetClassMethods() => new WrenForeignClassMethods()
        {
            Allocate = _wrenForeignMethodFn,
            AllocateSymbol = m_Allocator,

            Finalize = _wrenFinalizerMethodFn,
            FinalizeSymbol = m_Finalizer,
        };
    }
}
