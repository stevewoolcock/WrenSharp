using System;
using System.Runtime.CompilerServices;
using WrenSharp.Native;

namespace WrenSharp
{
    public unsafe partial class WrenVM
    {
        /// <summary>
        /// Returns a pointer to the user data associated with the VM.
        /// </summary>
        /// <returns>A pointer to the VM user data.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public IntPtr GetUserData() => Wren.GetUserData(m_Ptr);

        /// <summary>
        /// Returns a ref to the user data associated with the VM, as <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A ref to the VM user data as <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ref T GetUserData<T>() where T : unmanaged => ref *(T*)Wren.GetUserData(m_Ptr);

        /// <summary>
        /// Returns a pointer to the user data associated with the VM, as <typeparamref name="T"/>.
        /// </summary>
        /// <returns>A pointer to the VM user data as <typeparamref name="T"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* GetUserDataPtr<T>() where T : unmanaged => (T*)Wren.GetUserData(m_Ptr);

        /// <summary>
        /// Returns the shared data value assigned as the VM user data, as <typeparamref name="T"/>.
        /// <see cref="SetUserSharedData(object)"/>.
        /// </summary>
        /// <returns>The value from the shared data table, as <typeparamref name="T"/>.</returns>
        /// <seealso cref="SetUserSharedData(object)"/>
        /// <seealso cref="SetUserSharedData(WrenSharedDataHandle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T GetUserSharedData<T>()
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetUserData(m_Ptr);
            return m_SharedData.Get<T>(handle);
        }

        /// <summary>
        /// Returns the shared data value assigned as the VM user data.
        /// <see cref="SetUserSharedData(object)"/>.
        /// </summary>
        /// <returns>A ref to the VM user data.</returns>
        /// <seealso cref="SetUserSharedData(object)"/>
        /// <seealso cref="SetUserSharedData(WrenSharedDataHandle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public object GetUserSharedData()
        {
            var handle = *(WrenSharedDataHandle*)Wren.GetUserData(m_Ptr);
            return m_SharedData.Get(handle);
        }

        /// <summary>
        /// Gets the <see cref="WrenSharedDataHandle"/> assigned as the VM's user data.
        /// </summary>
        /// <returns>A <see cref="WrenSharedDataHandle"/>.</returns>
        /// <seealso cref="SetUserSharedData(object)"/>
        /// <seealso cref="SetUserSharedData(WrenSharedDataHandle)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public WrenSharedDataHandle GetUserSharedDataHandle() => *(WrenSharedDataHandle*)Wren.GetUserData(m_Ptr);

        /// <summary>
        /// Sets the user data associated with the VM to a pointer. It is the user's responsibility to ensure
        /// the pointer is valid. Any previous userData assigned via methods on <see cref="WrenVM"/> will have their underlying memory freed.
        /// </summary>
        /// <param name="userData">A pointer to the user data value.</param>
        /// <seealso cref="GetUserData()"/>
        /// <seealso cref="GetUserData{T}()"/>
        /// <seealso cref="GetUserDataPtr{T}"/>
        public WrenVM SetUserData(IntPtr userData)
        {
            // Free the existing buffer
            if (m_UserDataBuffer != IntPtr.Zero)
            {
                m_Allocator.Free(m_UserDataBuffer);
                m_UserDataBuffer = IntPtr.Zero;
            }

            Wren.SetUserData(m_Ptr, userData);
            return this;
        }

        /// <summary>
        /// Sets the VM's userData pointer to a a copy of <paramref name="userData"/>. Allocates unmanaged memory to
        /// hold the copy. Any previous userData assigned via methods on <see cref="WrenVM"/> will have their underlying memory freed.
        /// </summary>
        /// <param name="userData">The value to set the VM's user data to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/> instance.</returns>
        /// <seealso cref="GetUserData()"/>
        /// <seealso cref="GetUserData{T}()"/>
        /// <seealso cref="GetUserDataPtr{T}"/>
        public WrenVM SetUserData<T>(in T userData) where T : unmanaged
        {
            // The userData argument lives on the stack or the managed heap.
            // The pointer Wren expects for the user data field must remain valid for as long as it
            // is set, so the value must be copied into an unmanaged buffer to escape the GC, and
            // the Wren user data will point that unamanged location.

            // Free the existing buffer
            if (m_UserDataBuffer != IntPtr.Zero)
            {
                m_Allocator.Free(m_UserDataBuffer);
                m_UserDataBuffer = IntPtr.Zero;
            }

            // Allocate a buffer large enough to hold a value of T
            m_UserDataBufferSize = sizeof(T);
            m_UserDataBuffer = m_Allocator.Allocate(m_UserDataBufferSize);

            // Assign userData
            *(T*)m_UserDataBuffer = userData;
            Wren.SetUserData(m_Ptr, m_UserDataBuffer);
            return this;
        }

        /// <summary>
        /// Sets the VM's userData pointer to an empty value of <typeparamref name="T"/>. Allocates unmanaged memory to
        /// hold the value. Any previous userData assigned via methods on <see cref="WrenVM"/> will have their underlying memory freed.
        /// </summary>
        /// <param name="userData">The value to set the VM's user data to.</param>
        /// <returns>A reference to this empty <typeparamref name="T"/> that was allocated.</returns>
        /// <seealso cref="GetUserData()"/>
        /// <seealso cref="GetUserData{T}()"/>
        /// <seealso cref="GetUserDataPtr{T}"/>
        public ref T SetUserData<T>() where T : unmanaged
        {
            SetUserData<T>(default);
            return ref GetUserData<T>();
        }

        /// <summary>
        /// Adds <paramref name="userData"/> to the <see cref="SharedData"/> table, and sets the VM's user data
        /// to a <see cref="WrenSharedDataHandle"/>, pointing to <paramref name="userData"/>.
        /// </summary>
        /// <param name="userData">The value to set the VM's user data to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/> instance.</returns>
        /// <seealso cref="GetUserSharedData"/>
        /// <seealso cref="GetUserSharedData{T}"/>
        /// <seealso cref="GetUserSharedDataHandle"/>
        public WrenVM SetUserSharedData(object userData)
        {
            var handle = m_SharedData.Add(userData);
            return SetUserData(in handle);
        }

        /// <summary>
        /// Sets the VM's user data to <paramref name="handle"/>.
        /// </summary>
        /// <param name="handle">The handle to set the VM's user data to.</param>
        /// <returns>A reference to this <see cref="WrenVM"/> instance.</returns>
        /// <seealso cref="GetUserSharedData"/>
        /// <seealso cref="GetUserSharedData{T}"/>
        /// <seealso cref="GetUserSharedDataHandle"/>
        public WrenVM SetUserSharedData(in WrenSharedDataHandle handle) => SetUserData(in handle);
    }
}
