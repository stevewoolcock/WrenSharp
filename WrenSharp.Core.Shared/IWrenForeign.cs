using System;

namespace WrenSharp
{
    /// <summary>
    /// An interface describing the contract that must be implemented by the Wren foreign class builders.
    /// </summary>
    public interface IWrenForeign
    {
        public delegate void Allocator(WrenVM vm);
        public delegate void Allocator<T>(WrenVM vm, ref T data) where T : unmanaged;
        public delegate void AllocatorCall(WrenCallContext call);
        public delegate void AllocatorCall<T>(WrenCallContext call, ref T data) where T : unmanaged;
        public delegate void Finalizer<T>(ref T data) where T : unmanaged;
        public delegate void Finalizer(IntPtr data);

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This is a bare allocator, and does not actually create any foreign class data. To do so, you must call one of the foreign instance constructor API methods:<para/>
        /// <see cref="WrenVM.SetSlotNewForeign(int, int, ulong)"/><br/>
        /// <see cref="WrenVM.SetSlotNewForeign{T}(int, int, in T)"/>
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this create the foriegn data and to set the initial state of the memory.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Allocate(Allocator allocator);

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This is a bare allocator, and does not actually create any foreign class data. To do so, you must call one of the foreign instance constructor API methods:<para/>
        /// <see cref="WrenVM.SetSlotNewForeign(int, int, ulong)"/><br/>
        /// <see cref="WrenVM.SetSlotNewForeign{T}(int, int, in T)"/>
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this create the foriegn data and to set the initial state of the memory.</param>
        /// <param name="paramCount">The number of parameters expected in the constructor. Note that it is not possible to know which constructor was invoked on the Wren
        /// side from within the allocator. This parameter defaults to <see cref="WrenVM.MaxCallParameters"/>.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Allocate(AllocatorCall allocator, byte paramCount = WrenVM.MaxCallParameters);

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Allocate<T>() where T : unmanaged;

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this to set the initial state of the memory.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Allocate<T>(Allocator<T> allocator) where T : unmanaged;

        /// <summary>
        /// Sets the allocator function called when an instance of this foreign class is created from a Wren program.<para />
        /// This allocator creates allocates the memory to hold a value of <typeparamref name="T"/> and places an initialized value
        /// of <typeparamref name="T"/> at the address of the newly allocated memory, ready to be used.
        /// </summary>
        /// <param name="allocator">The allocator delegate to call when an instance is created. Use this to set the initial state of the memory.</param>
        /// <param name="paramCount">The number of parameters expected in the constructor. Note that it is not possible to know which constructor was invoked on the Wren
        /// side from within the allocator. This parameter defaults to <see cref="WrenVM.MaxCallParameters"/>.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Allocate<T>(AllocatorCall<T> allocator, byte paramCount = WrenVM.MaxCallParameters) where T : unmanaged;

        /// <summary>
        /// Sets the finalizer function called when an instance of this foreign class is cleaned up by the Wren garbage collector.<para />
        /// Use this method to free unmanaged memory that may have been allocated within the class's allocator or in any of its methods.
        /// </summary>
        /// <param name="finalizer">The finalizer delegate to call when the instance is garbage collected.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Finalize(Finalizer finalizer);

        /// <summary>
        /// Sets the finalizer function called when an instance of this foreign class is cleaned up by the Wren garbage collector.<para />
        /// Use this method to free unmanaged memory that may have been allocated within the class's allocator or in any of its methods.
        /// </summary>
        /// <param name="finalizer">The finalizer delegate to call when the instance is garbage collected.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Finalize<T>(Finalizer<T> finalizer) where T : unmanaged;

        /// <summary>
        /// Sets the delegate to be invoked when the foreign instance method <paramref name="signature"/> is called for this class.
        /// </summary>
        /// <param name="signature">The signature of the method.</param>
        /// <param name="method">The method delegate to invoke when called.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Instance(string signature, WrenForeignMethod method);

        /// <summary>
        /// Sets the delegate to be invoked when the foreign static method <paramref name="signature"/> is called for this class.
        /// </summary>
        /// <param name="signature">The signature of the method.</param>
        /// <param name="method">The method delegate to invoke when called.</param>
        /// <returns>A reference to this <see cref="WrenSharp.IWrenForeign"/> instance.</returns>
        IWrenForeign Static(string signature, WrenForeignMethod method);
    }
}
