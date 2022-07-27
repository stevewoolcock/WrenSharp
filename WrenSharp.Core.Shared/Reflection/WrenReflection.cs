using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace WrenSharp.Reflection
{
    /// <summary>
    /// The WrenSharp reflection API. Provides an interface automating class builders for Wren foreign types,
    /// build Wren classes that mirror managed types, automatic allocation of Wren handles, execute ordered
    /// initializer methods and more.
    /// </summary>
    public static class WrenReflection
    {
        [ThreadStatic]
        private static StringBuilder _cachedStringBuilder;

        /// <summary>
        /// Invokes any static initializers, type builders and handle allocators for all types in <paramref name="types"/>.<para/>
        /// The order of attributes that derive from <see cref="WrenOrderedAttribute"/> is respected for all types.
        /// </summary>
        /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
        /// <param name="types">The collection of types to operate on.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="vm"/> is null<para/>- or -<para/>
        /// If <paramref name="types"/> is null.
        /// </exception>
        public static void Invoke(WrenVM vm, IEnumerable<Type> types)
        {
            var initMethods = new List<(MethodInfo methodInfo, WrenInitializeMethodAttribute initAttribute)>();
            foreach (Type type in types)
            {
                if (type.IsEnum)
                {
                    // Enums have special handling
                    // If the WrenEnum attribute is defined, then the enum is interpreted to a static Wren class
                    var attEnum = type.GetCustomAttribute<WrenEnumAttribute>();
                    if (attEnum != null)
                    {
                        InterpretEnum(vm, type, attEnum.Module, attEnum.ClassName);
                    }

                    continue;
                }

                MethodInfo[] methodInfos = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                for (int i = 0; i < methodInfos.Length; i++)
                {
                    var methodInfo = methodInfos[i];
                    var initAttribute = methodInfo.GetCustomAttribute<WrenInitializeMethodAttribute>();
                    if (initAttribute != null)
                    {
                        initMethods.Add((methodInfo, initAttribute));
                    }
                }
            }

            if (initMethods.Count == 0)
                return;

            // Sort the methods by the order property on the initialization attributes
            initMethods.Sort((x, y) => x.initAttribute.Order.CompareTo(y.initAttribute.Order));

            var args = new object[] { vm };
            for (int i = 0; i < initMethods.Count; i++)
            {
                (MethodInfo methodInfo, WrenInitializeMethodAttribute initAttribute) = initMethods[i];
                methodInfo.Invoke(null, args);
            }
        }

        /// <summary>
        /// Invokes all methods marked with the <see cref="WrenInitializeMethodAttribute"/> on <paramref name="target"/>.
        /// If <paramref name="target"/> is a <see cref="Type"/>, static methods are invoked. If <paramref name="target"/> is
        /// and instance, instance methods are invoked.<para/>
        /// Methods marked with <see cref="WrenInitializeMethodAttribute"/> must have the signature:
        /// <code>
        /// [static] void MethodName(WrenVm vm)
        /// </code>
        /// </summary>
        /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
        /// <param name="target">The type or instance to invoke methods on.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="vm"/> is null<para/>- or -<para/>
        /// If <paramref name="target"/> is null.
        /// </exception>
        public static void InvokeInitializeMethods(WrenVM vm, object target)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var initMethods = new List<(MethodInfo methodInfo, WrenInitializeMethodAttribute initAttribute)>();
            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
            if (target is Type type)
            {
                bindingFlags |= BindingFlags.Static;
            }
            else
            {
                type = target.GetType();
                bindingFlags |= BindingFlags.Instance;
            }

            MethodInfo[] methodInfos = type.GetMethods(bindingFlags);
            for (int i = 0; i < methodInfos.Length; i++)
            {
                var methodInfo = methodInfos[i];
                var initAttribute = methodInfo.GetCustomAttribute<WrenInitializeMethodAttribute>();
                if (initAttribute != null)
                {
                    initMethods.Add((methodInfo, initAttribute));
                }
            }

            // Sort the methods by the order property on the initialization attributes
            initMethods.Sort((x, y) => x.initAttribute.Order.CompareTo(y.initAttribute.Order));

            var args = new object[] { vm };
            for (int i = 0; i < initMethods.Count; i++)
            {
                (MethodInfo methodInfo, WrenInitializeMethodAttribute initAttribute) = initMethods[i];
                methodInfo.Invoke(null, args);
            }
        }

        /// <summary>
        /// Allocates a Wren handle for fields in <paramref name="target"/> that are annotated with a
        /// <see cref="WrenCallHandleAttribute"/> or <see cref="WrenVariableHandleAttribute"/> attribute.<para/>
        /// If <paramref name="target"/> is a <see cref="Type"/>, static fields are set. If <paramref name="target"/> is an instance, instance fields are set.<para/>
        /// Fields with the <see cref="WrenCallHandleAttribute"/> attribute must be of the type <see cref="WrenCallHandle"/>.<br/>
        /// Fields with the <see cref="WrenVariableHandleAttribute"/> attribute must be of the type <see cref="WrenHandle"/>.
        /// </summary>
        /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
        /// <param name="target">The target object or type.</param>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="vm"/> is null<para/>- or -<para/>
        /// If <paramref name="target"/> is null.
        /// </exception>
        public static void AllocateHandleFields(WrenVM vm, object target)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (target == null) throw new ArgumentNullException(nameof(target));

            var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic;
            if (target is Type type)
            {
                bindingFlags |= BindingFlags.Static;
            }
            else
            {
                type = target.GetType();
                bindingFlags |= BindingFlags.Instance;
            }

            FieldInfo[] fields = type.GetFields(bindingFlags);
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];

                var attCallHandle = field.GetCustomAttribute<WrenCallHandleAttribute>();
                if (attCallHandle != null)
                {
                    WrenCallHandle callHandle = vm.CreateCallHandle(attCallHandle.Signature);
                    field.SetValue(target, callHandle);
                    continue;
                }

                var attVarHandle = field.GetCustomAttribute<WrenVariableHandleAttribute>();
                if (attVarHandle != null)
                {
                    vm.LoadVariable(attVarHandle.Module, attVarHandle.Variable, 0);
                    WrenHandle handle = vm.CreateHandle(0);
                    field.SetValue(target, handle);
                    continue;
                }
            }
        }

        /// <summary>
        /// Creates a static Wren class from a managed enum type.
        /// </summary>
        /// <param name="vm">The <see cref="WrenVM"/> instance.</param>
        /// <param name="enumType">The enum type to build the base class from.</param>
        /// <param name="module">The module to create the class in.</param>
        /// <param name="className">The name of the Wren class. If null, the name of <paramref name="enumType"/> is used.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">
        /// If <paramref name="vm"/> is null<para/>- or -<para/>
        /// If <paramref name="enumType"/> is null<para/>- or -<para/>
        /// If <paramref name="module"/> is null.
        /// </exception>
        /// <exception cref="ArgumentException">Thrown if <paramref name="enumType"/> is not an enum type.</exception>
        public static WrenInterpretResult InterpretEnum(WrenVM vm, Type enumType, string module, string className = null)
        {
            if (vm == null) throw new ArgumentNullException(nameof(vm));
            if (enumType == null) throw new ArgumentNullException(nameof(enumType));
            if (module == null) throw new ArgumentNullException(nameof(module));
            if (!enumType.IsEnum) throw new ArgumentException("Type must be an Enum", nameof(enumType));

            if (string.IsNullOrEmpty(className))
            {
                // TODO: Sanitize this to adhere to Wren naming rules 
                className = enumType.Name;
            }

            //
            // Build a Wren class with static getters for each value in the enum type.
            //
            // class className {
            //     static valueName { value }
            //     ...
            // }
            //

            StringBuilder sb = (_cachedStringBuilder ??= new StringBuilder(1024));
            sb.Clear();

            // class className {
            sb.Append("class ").Append(className).Append(" {").AppendLine();

            var enumFields = enumType.GetFields(BindingFlags.Public | BindingFlags.Static);
            for (int i = 0; i < enumFields.Length; i++)
            {
                FieldInfo field = enumFields[i];

                // Field ignored?
                if (field.IsDefined(typeof(WrenIgnoreAttribute)))
                    continue;

                // Name can be overidden by [WrenProperty]
                var propAttribute = field.GetCustomAttribute<WrenPropertyAttribute>();

                // TODO: Sanitize this to adhere to Wren naming rules 
                var name = string.IsNullOrEmpty(propAttribute?.Name) ? field.Name : propAttribute.Name;
                var value = field.GetRawConstantValue();

                // static name { value }
                sb.Append("static ").Append(name)
                  .Append(" { ").Append(value).Append(" }")
                  .AppendLine();
            }
            sb.Append('}');

            return vm.Interpret(module, sb);
        }
    }
}
