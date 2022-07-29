using System;
using System.Collections.Generic;
using UnityEngine;
using WrenSharp.Native;

namespace WrenSharp.Unity
{
    /// <summary>
    /// A builder for binding Wren foreign classes and methods.
    /// </summary>
    public sealed class WrenForeign
    {
        //
        // Implementation notes:
        //
        // When using IL2CPP, marshalling p/invoke delegates through instances is not possible as there's no JIT
        // available at runtime compile and perform the correct marshalling.
        //
        // Unfortunately, this means that all p/invoke function pointers into managed code must point to static
        // methods (in Unity, they must also be annotated with the AOT.MonoPInvokeCallback attribute).
        //
        // This solution for binding managed delegates to Wren foreign classes and methods uses a small modification
        // to the Wren source that attaches a uint16 symbol to foreign methods. The symbol is assigned on on the managed
        // side (in this class, WrenForeign) and passed through as an argument when Wren calls the function pointer.
        //
        // This means that we can place every foreign method delegate into a static array and have Wren foriegn
        // methods call to a static managed method which can use the symbol to lookup the managed delegate.
        //
        //
        // This class provides the same public API as the standard WrenForeignBuilder.
        //

        public delegate void Allocator(UnityWrenVM vm);
        public delegate void Allocator<T>(UnityWrenVM vm, ref T data) where T : unmanaged;
        public delegate void AllocatorCall(WrenCallContext call);
        public delegate void AllocatorCall<T>(WrenCallContext call, ref T data) where T : unmanaged;
        public delegate void Finalizer<T>(ref T data) where T : unmanaged;
        public delegate void Finalizer(IntPtr data);

        // Internal class used to manage a collection of delegates for foreign method binding
        // Each delegate is given a symbol corresponding to an index within an array
        // Freed delegates return their symbol to the free stack for reuse
        private class DelegateTable<T> where T : Delegate
        {
            private readonly Stack<ushort> m_FreeSymbols = new Stack<ushort>();
            private ushort m_TailSymbol = 1;

            public T[] Delegates = new T[0];

            public ushort Add(T del)
            {
                ushort symbol = m_FreeSymbols.Count > 0 ? m_FreeSymbols.Pop() : m_TailSymbol++;

                if (Delegates.Length < symbol)
                {
                    int newCapacity = Delegates.Length == 0 ? 4 : Delegates.Length * 2;
                    int minCapacity = symbol + 1;
                    if (newCapacity < minCapacity)
                    {
                        newCapacity = minCapacity;
                    }

                    Array.Resize(ref Delegates, newCapacity);
                }

                Delegates[symbol - 1] = del;
                return symbol;
            }

            public void Remove(ushort symbol)
            {
                Delegates[symbol - 1] = null;
                m_FreeSymbols.Push(symbol);
            }

            public void Clear()
            {
                Array.Clear(Delegates, 0, m_TailSymbol - 1);
                m_TailSymbol = 1;
                m_FreeSymbols.Clear();
            }
        }

        #region Static

        private static WrenNativeFn.ForeignMethod _wrenForeignMethodFn = WrenForeignMethodFn;
        private static WrenNativeFn.Finalizer _wrenFinalizerMethodFn = WrenFinalizerMethodFn;

        private static readonly object _delegateTableLocker = new object();

        private static readonly DelegateTable<Action> _methodTable = new DelegateTable<Action>();
        private static readonly DelegateTable<Action<IntPtr>> _finalizerTable = new DelegateTable<Action<IntPtr>>();

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.ForeignMethod))]
        private static void WrenForeignMethodFn(IntPtr vm, ushort symbol)
        {
            if (symbol > 0)
            {
                _methodTable.Delegates[symbol - 1]();
            }
        }

        [AOT.MonoPInvokeCallback(typeof(WrenNativeFn.Finalizer))]
        private static void WrenFinalizerMethodFn(IntPtr data, ushort symbol)
        {
            if (symbol > 0)
            {
                _finalizerTable.Delegates[symbol - 1](data);
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

        internal WrenForeign(UnityWrenVM vm)
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
            m_Allocator = _methodTable.Add(() => allocator(m_Vm));
            return this;
        }

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This is a bare allocator, and does not actually create any foreign class data. To do so, you must call one of the foreign instance constructor API methods:<para/>
        /// <see cref="WrenVM.SetSlotNewForeign(int, int, ulong)"/><br/>
        /// <see cref="WrenVM.SetSlotNewForeign{T}(int, int, in T)"/>
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this create the foriegn data and to set the initial state of the memory.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public WrenForeign Allocate(AllocatorCall allocator)
        {
            m_Allocator = _methodTable.Add(() => allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, byte.MaxValue)));
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
            m_Allocator = _methodTable.Add(() =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(m_Vm, ref *data);
            });
            return this;
        }

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this to set the initial state of the memory.</param>
        /// <returns>A reference to this <see cref="WrenForeign"/> instance.</returns>
        public unsafe WrenForeign Allocate<T>(AllocatorCall<T> allocator) where T : unmanaged
        {
            m_Allocator = _methodTable.Add(() =>
            {
                T* data = (T*)Wren.SetSlotNewForeign(m_Vm.m_Ptr, 0, 0, (ulong)sizeof(T));
                *data = new T();
                allocator(new WrenCallContext(m_Vm, WrenMethodType.Allocator, byte.MaxValue), ref *data);
            });
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
            m_Finalizer = _finalizerTable.Add((data) => finalizer(data));
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
            m_Finalizer = _finalizerTable.Add((data) => finalizer(ref *(T*)data));
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

            var symbol = _methodTable.Add(() => method(new WrenCallContext(m_Vm, methodType, paramCount)));

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
                symbol = symbol,
                fn = _wrenForeignMethodFn,
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
