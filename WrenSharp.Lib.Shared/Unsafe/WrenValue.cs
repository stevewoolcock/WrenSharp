#if WRENSHARP_EXT
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace WrenSharp.Unsafe
{
    /// <summary>
    /// Represents an internal Wren value type. This is only available if both the Wren native library and
    /// the WrenSharp libraries are compiled with WrenSharp extensions enabled.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public unsafe struct WrenValue : IEquatable<WrenValue>
    {
        // A mask that selects the sign bit.
        private const ulong SIGN_BIT = (ulong)1 << 63;

        // The bits that must be set to indicate a quiet NaN.
        private const ulong QNAN = 0x7ffc000000000000;

        // Masks out the tag bits used to identify the singleton value.
        private const byte MASK_TAG = 7;

        // Tag values for the different singleton values.
        private const byte TAG_NAN = 0;
        private const byte TAG_NULL = 1;
        private const byte TAG_FALSE = 2;
        private const byte TAG_TRUE = 3;
        private const byte TAG_UNDEFINED = 4;
        private const byte TAG_UNUSED2 = 5;
        private const byte TAG_UNUSED3 = 6;
        private const byte TAG_UNUSED4 = 7;

        // Singleton values
        private const ulong NULL_VAL = QNAN | TAG_NULL;
        private const ulong FALSE_VAL = QNAN | TAG_FALSE;
        private const ulong TRUE_VAL = QNAN | TAG_TRUE;
        private const ulong UNDEFINED_VAL = QNAN | TAG_UNDEFINED;

        #region Static

        public static WrenValue Null => new WrenValue(NULL_VAL);

        public static WrenValue Undefined => new WrenValue(UNDEFINED_VAL);

        #endregion

        [FieldOffset(0)]
        private ulong m_Value;

        [FieldOffset(0)]
        private double m_Number;

        #region Properties

        public bool AsBool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => IsBool && m_Value == TRUE_VAL;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(value);
        }

        public double AsNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => IsNumber ? m_Number : default;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(value);
        }

        public WrenObject AsObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            readonly get => IsObject ? new WrenObject(ObjectPtr) : default;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => Set(value);
        }

        public readonly WrenString AsString
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObjectType(WrenObjectType.String) ? new WrenString(CastObj<WrenInternalObjString>()) : default;
        }

        public readonly WrenClass AsClass
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObjectType(WrenObjectType.Class) ? new WrenClass(CastObj<WrenInternalObjClass>()) : default;
        }

        public readonly WrenInstance AsInstance
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObjectType(WrenObjectType.Instance) ? new WrenInstance(CastObj<WrenInternalObjInstance>()) : default;
        }

        public readonly WrenForeign AsForeign
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObjectType(WrenObjectType.Foreign) ? new WrenForeign(CastObj<WrenInternalObjForeign>()) : default;
        }

        public readonly WrenNativeList AsList
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObjectType(WrenObjectType.List) ? new WrenNativeList(CastObj<WrenInternalObjList>()) : default;
        }

        public readonly WrenNativeMap AsMap
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObjectType(WrenObjectType.Map) ? new WrenNativeMap(CastObj<WrenInternalObjMap>()) : default;
        }

        public readonly string StringValue
        {
            get
            {
                if (IsObjectType(WrenObjectType.String))
                {
                    WrenInternalObjString* str = (WrenInternalObjString*)ObjectPtr;
                    return System.Text.Encoding.UTF8.GetString(&str->ValueArray, str->Length);
                }
                else
                {
                    return default;
                }
            }
        }

        public readonly WrenObjectType ObjectType
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => IsObject ? ObjectPtr->Type : WrenObjectType.Unknown;
        }

        public readonly WrenType Type
        {
            get
            {
                if (IsNull) return WrenType.Null;
                if (IsBool) return WrenType.Bool;
                if (IsNumber) return WrenType.Number;
                if (IsObject)
                {
                    switch (ObjectPtr->Type)
                    {
                        case WrenObjectType.Foreign: return WrenType.Foreign;
                        case WrenObjectType.String: return WrenType.String;
                        case WrenObjectType.List: return WrenType.List;
                        case WrenObjectType.Map: return WrenType.Map;
                    }
                }
                return WrenType.Unknown;
            }
        }


        public readonly bool IsBool
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Value == TRUE_VAL || m_Value == FALSE_VAL;
        }

        public readonly bool IsNull
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Value == NULL_VAL;
        }

        public readonly bool IsNumber
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((m_Value) & QNAN) != QNAN;
        }

        public readonly bool IsObject
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => ((m_Value) & (QNAN | SIGN_BIT)) == (QNAN | SIGN_BIT);
        }

        public readonly bool IsUndefined
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => m_Value == UNDEFINED_VAL;
        }

        internal readonly int Tag
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (int)(m_Value & MASK_TAG);
        }

        internal readonly WrenInternalObj* ObjectPtr
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (WrenInternalObj*)(m_Value & ~(SIGN_BIT | QNAN));
        }

        #endregion

        public WrenValue(double number)
        {
            m_Value = default;
            m_Number = number;
        }

        public WrenValue(bool boolean)
        {
            m_Number = default;
            m_Value = boolean ? TRUE_VAL : FALSE_VAL;
        }

        public WrenValue(in WrenObject wrenObject) : this(wrenObject.m_Ptr)
        {
        }


        internal WrenValue(ulong value)
        {
            m_Number = default;
            m_Value = value;
        }

        internal WrenValue(WrenInternalObj* wrenObject)
        {
            m_Number = default;
            m_Value = SIGN_BIT | QNAN | (ulong)wrenObject;
        }

        #region Object

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool Equals(WrenValue other) => other.m_Value == m_Value;

        /// <inheritdoc/>
        public readonly override bool Equals(object obj) => obj is WrenValue value && Equals(value);

        /// <inheritdoc/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly override int GetHashCode() => m_Value.GetHashCode();

        /// <inheritdoc/>
        public readonly override string ToString()
        {
            if (IsNull) return "null";
            if (IsBool) return AsBool.ToString();
            if (IsNumber) return AsNumber.ToString();
            if (IsObject)
            {
                switch (ObjectPtr->Type)
                {
                    case WrenObjectType.String: return WrenString.ToManagedString(CastObj<WrenInternalObjString>());
                    case WrenObjectType.Class: return WrenString.ToManagedString(CastObj<WrenInternalObjClass>()->Name);
                    case WrenObjectType.Range:
                        {
                            WrenInternalObjRange* range = (WrenInternalObjRange*)ObjectPtr;
                            return range->IsInclusive ? $"{range->From}..{range->To}" : $"{range->From}...{range->To}";
                        }
                    default: return $"instance of {WrenString.ToManagedString(ObjectPtr->ClassObj->Name)}";
                }
            }

            return "undefined";
        }

        #endregion

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T* AsForeignPtr<T>() where T : unmanaged => AsForeign.AsPtr<T>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly T AsForeignType<T>() where T : unmanaged => AsForeign.As<T>();

        /// <summary>
        /// Returns a boolean indicating if the value represents a Wren object of <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The object type.</param>
        /// <returns>True if the value is an object of <paramref name="type"/>, otherwise false.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly bool IsObjectType(WrenObjectType type) => IsObject && ObjectPtr->Type == type;

        /// <summary>
        /// Sets the value to boolean.
        /// </summary>
        /// <param name="value">The boolean value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(bool value) => this = new WrenValue(value);

        /// <summary>
        /// Sets the value to a number.
        /// </summary>
        /// <param name="value">The number value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(double value) => this = new WrenValue(value);

        /// <summary>
        /// Sets the value to a wren object.
        /// </summary>
        /// <param name="value">The object value to set.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(WrenObject value) => this = new WrenValue(value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly T* CastObj<T>() where T : unmanaged => (T*)ObjectPtr;

        #region Operators

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(WrenValue left, WrenValue right) => left.Equals(right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(WrenValue left, WrenValue right) => !(left == right);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator bool(WrenValue wrenValue) => wrenValue.AsBool;
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator double(WrenValue wrenValue) => wrenValue.AsNumber;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator WrenObject(WrenValue wrenValue) => wrenValue.AsObject;

        #endregion
    }
}
#endif