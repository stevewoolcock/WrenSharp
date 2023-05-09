#if WRENSHARP_EXT
using System;
using System.Runtime.CompilerServices;

namespace WrenSharp.Unsafe
{
    public readonly unsafe struct WrenObject : IEquatable<WrenObject>
    {
        internal readonly WrenInternalObj* m_Ptr;

        #region Properties

        /// <summary>
        /// Indicates if the value has a valid underlying pointer.
        /// </summary>
        public bool IsValid => m_Ptr != null;

        /// <summary>
        /// The class of the object.
        /// </summary>
        public WrenClass Class => new WrenClass(m_Ptr->ClassObj);

        /// <summary>
        /// The type of the object
        /// </summary>
        public WrenObjectType Type => m_Ptr->Type;

        #endregion

        internal WrenObject(WrenInternalObj* cls)
        {
            m_Ptr = cls;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsType(WrenObjectType type) => m_Ptr->Type == type;

        public WrenClass AsClass() => IsType(WrenObjectType.Class) ? new WrenClass((WrenInternalObjClass*)m_Ptr) : default;

        public WrenInstance AsInstance() => IsType(WrenObjectType.Instance) ? new WrenInstance((WrenInternalObjInstance*)m_Ptr) : default;

        public WrenForeign AsForeign() => IsType(WrenObjectType.Foreign) ? new WrenForeign((WrenInternalObjForeign*)m_Ptr) : default;

        public T AsForeign<T>() where T : unmanaged => IsType(WrenObjectType.Foreign) ? *(T*)((WrenInternalObjForeign*)m_Ptr)->Data : default;

        public T* AsForeignPtr<T>() where T : unmanaged => IsType(WrenObjectType.Foreign) ? (T*)((WrenInternalObjForeign*)m_Ptr)->Data : default;

        public WrenString AsString() => IsType(WrenObjectType.String) ? new WrenString((WrenInternalObjString*)m_Ptr) : default;

        public WrenNativeList AsList() => IsType(WrenObjectType.List) ? new WrenNativeList((WrenInternalObjList*)m_Ptr) : default;

        public WrenNativeMap AsMap() => IsType(WrenObjectType.Map) ? new WrenNativeMap((WrenInternalObjMap*)m_Ptr) : default;

        #region Object

        public bool Equals(WrenObject other) => other.m_Ptr == m_Ptr;

        public override bool Equals(object obj) => obj is WrenObject cls && Equals(cls);

        public override int GetHashCode() => ((IntPtr)m_Ptr).GetHashCode();

        public override string ToString() => WrenString.ToManagedString(m_Ptr->ClassObj->Name);

        #endregion

        #region Operators

        public static bool operator ==(WrenObject left, WrenObject right) => left.Equals(right);
        public static bool operator !=(WrenObject left, WrenObject right) => !(left == right);

        public static explicit operator WrenObjectType(WrenObject obj) => obj.Type;
        public static explicit operator WrenClass(WrenObject obj) => obj.AsClass();
        public static explicit operator WrenInstance(WrenObject obj) => obj.AsInstance();
        public static explicit operator WrenString(WrenObject obj) => obj.AsString();
        public static explicit operator WrenForeign(WrenObject obj) => obj.AsForeign();
        public static explicit operator WrenNativeList(WrenObject obj) => obj.AsList();
        public static explicit operator WrenNativeMap(WrenObject obj) => obj.AsMap();

        #endregion
    }
}
#endif