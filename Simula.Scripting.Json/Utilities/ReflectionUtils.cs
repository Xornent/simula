
using System;
using System.Collections.Generic;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Diagnostics.CodeAnalysis;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json.Utilities
{
#if (DOTNET || PORTABLE || PORTABLE40) && !NETSTANDARD2_0
    [Flags]
    internal enum MemberTypes
    {
        Event = 2,
        Field = 4,
        Method = 8,
        Property = 16
    }
#endif

#if PORTABLE && !NETSTANDARD2_0
    [Flags]
    internal enum BindingFlags
    {
        Default = 0,
        IgnoreCase = 1,
        DeclaredOnly = 2,
        Instance = 4,
        Static = 8,
        Public = 16,
        NonPublic = 32,
        FlattenHierarchy = 64,
        InvokeMethod = 256,
        CreateInstance = 512,
        GetField = 1024,
        SetField = 2048,
        GetProperty = 4096,
        SetProperty = 8192,
        PutDispProperty = 16384,
        ExactBinding = 65536,
        PutRefDispProperty = 32768,
        SuppressChangeType = 131072,
        OptionalParamBinding = 262144,
        IgnoreReturn = 16777216
    }
#endif

    internal static class ReflectionUtils
    {
        public static readonly Type[] EmptyTypes;

        static ReflectionUtils()
        {
#if HAVE_EMPTY_TYPES
            EmptyTypes = Type.EmptyTypes;
#else
            EmptyTypes = CollectionUtils.ArrayEmpty<Type>();
#endif
        }

        public static bool IsVirtual(this PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            MethodInfo? m = propertyInfo.GetGetMethod(true);
            if (m != null && m.IsVirtual) {
                return true;
            }

            m = propertyInfo.GetSetMethod(true);
            if (m != null && m.IsVirtual) {
                return true;
            }

            return false;
        }

        public static MethodInfo? GetBaseDefinition(this PropertyInfo propertyInfo)
        {
            ValidationUtils.ArgumentNotNull(propertyInfo, nameof(propertyInfo));

            MethodInfo? m = propertyInfo.GetGetMethod(true);
            if (m != null) {
                return m.GetBaseDefinition();
            }

            return propertyInfo.GetSetMethod(true)?.GetBaseDefinition();
        }

        public static bool IsPublic(PropertyInfo property)
        {
            var getMethod = property.GetGetMethod();
            if (getMethod != null && getMethod.IsPublic) {
                return true;
            }
            var setMethod = property.GetSetMethod();
            if (setMethod != null && setMethod.IsPublic) {
                return true;
            }

            return false;
        }

        public static Type? GetObjectType(object? v)
        {
            return v?.GetType();
        }

        public static string GetTypeName(Type t, TypeNameAssemblyFormatHandling assemblyFormat, ISerializationBinder? binder)
        {
            string fullyQualifiedTypeName = GetFullyQualifiedTypeName(t, binder);

            switch (assemblyFormat) {
                case TypeNameAssemblyFormatHandling.Simple:
                    return RemoveAssemblyDetails(fullyQualifiedTypeName);
                case TypeNameAssemblyFormatHandling.Full:
                    return fullyQualifiedTypeName;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static string GetFullyQualifiedTypeName(Type t, ISerializationBinder? binder)
        {
            if (binder != null) {
                binder.BindToName(t, out string? assemblyName, out string? typeName);
#if (NET20 || NET35)
                if (assemblyName == null & typeName == null)
                {
                    return t.AssemblyQualifiedName;
                }
#endif
                return typeName + (assemblyName == null ? "" : ", " + assemblyName);
            }

            return t.AssemblyQualifiedName;
        }

        private static string RemoveAssemblyDetails(string fullyQualifiedTypeName)
        {
            StringBuilder builder = new StringBuilder();
            bool writingAssemblyName = false;
            bool skippingAssemblyDetails = false;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++) {
                char current = fullyQualifiedTypeName[i];
                switch (current) {
                    case '[':
                    case ']':
                        writingAssemblyName = false;
                        skippingAssemblyDetails = false;
                        builder.Append(current);
                        break;
                    case ',':
                        if (!writingAssemblyName) {
                            writingAssemblyName = true;
                            builder.Append(current);
                        } else {
                            skippingAssemblyDetails = true;
                        }
                        break;
                    default:
                        if (!skippingAssemblyDetails) {
                            builder.Append(current);
                        }
                        break;
                }
            }

            return builder.ToString();
        }

        public static bool HasDefaultConstructor(Type t, bool nonPublic)
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            if (t.IsValueType()) {
                return true;
            }

            return (GetDefaultConstructor(t, nonPublic) != null);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t)
        {
            return GetDefaultConstructor(t, false);
        }

        public static ConstructorInfo GetDefaultConstructor(Type t, bool nonPublic)
        {
            BindingFlags bindingFlags = BindingFlags.Instance | BindingFlags.Public;
            if (nonPublic) {
                bindingFlags = bindingFlags | BindingFlags.NonPublic;
            }

            return t.GetConstructors(bindingFlags).SingleOrDefault(c => !c.GetParameters().Any());
        }

        public static bool IsNullable(Type t)
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            if (t.IsValueType()) {
                return IsNullableType(t);
            }

            return true;
        }

        public static bool IsNullableType(Type t)
        {
            ValidationUtils.ArgumentNotNull(t, nameof(t));

            return (t.IsGenericType() && t.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static Type EnsureNotNullableType(Type t)
        {
            return (IsNullableType(t))
                ? Nullable.GetUnderlyingType(t)
                : t;
        }

        public static Type EnsureNotByRefType(Type t)
        {
            return (t.IsByRef && t.HasElementType)
                ? t.GetElementType()
                : t;
        }

        public static bool IsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            if (!type.IsGenericType()) {
                return false;
            }

            Type t = type.GetGenericTypeDefinition();
            return (t == genericInterfaceDefinition);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition)
        {
            return ImplementsGenericDefinition(type, genericInterfaceDefinition, out _);
        }

        public static bool ImplementsGenericDefinition(Type type, Type genericInterfaceDefinition, [NotNullWhen(true)] out Type? implementingType)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));
            ValidationUtils.ArgumentNotNull(genericInterfaceDefinition, nameof(genericInterfaceDefinition));

            if (!genericInterfaceDefinition.IsInterface() || !genericInterfaceDefinition.IsGenericTypeDefinition()) {
                throw new ArgumentNullException("'{0}' is not a generic interface definition.".FormatWith(CultureInfo.InvariantCulture, genericInterfaceDefinition));
            }

            if (type.IsInterface()) {
                if (type.IsGenericType()) {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition) {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces()) {
                if (i.IsGenericType()) {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition) {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition)
        {
            return InheritsGenericDefinition(type, genericClassDefinition, out _);
        }

        public static bool InheritsGenericDefinition(Type type, Type genericClassDefinition, out Type? implementingType)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));
            ValidationUtils.ArgumentNotNull(genericClassDefinition, nameof(genericClassDefinition));

            if (!genericClassDefinition.IsClass() || !genericClassDefinition.IsGenericTypeDefinition()) {
                throw new ArgumentNullException("'{0}' is not a generic class definition.".FormatWith(CultureInfo.InvariantCulture, genericClassDefinition));
            }

            return InheritsGenericDefinitionInternal(type, genericClassDefinition, out implementingType);
        }

        private static bool InheritsGenericDefinitionInternal(Type currentType, Type genericClassDefinition, out Type? implementingType)
        {
            do {
                if (currentType.IsGenericType() && genericClassDefinition == currentType.GetGenericTypeDefinition()) {
                    implementingType = currentType;
                    return true;
                }

                currentType = currentType.BaseType();
            }
            while (currentType != null);

            implementingType = null;
            return false;
        }
        public static Type? GetCollectionItemType(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));

            if (type.IsArray) {
                return type.GetElementType();
            }
            if (ImplementsGenericDefinition(type, typeof(IEnumerable<>), out Type? genericListType)) {
                if (genericListType!.IsGenericTypeDefinition()) {
                    throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
                }

                return genericListType!.GetGenericArguments()[0];
            }
            if (typeof(IEnumerable).IsAssignableFrom(type)) {
                return null;
            }

            throw new Exception("Type {0} is not a collection.".FormatWith(CultureInfo.InvariantCulture, type));
        }

        public static void GetDictionaryKeyValueTypes(Type dictionaryType, out Type? keyType, out Type? valueType)
        {
            ValidationUtils.ArgumentNotNull(dictionaryType, nameof(dictionaryType));

            if (ImplementsGenericDefinition(dictionaryType, typeof(IDictionary<,>), out Type? genericDictionaryType)) {
                if (genericDictionaryType!.IsGenericTypeDefinition()) {
                    throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
                }

                Type[] dictionaryGenericArguments = genericDictionaryType!.GetGenericArguments();

                keyType = dictionaryGenericArguments[0];
                valueType = dictionaryGenericArguments[1];
                return;
            }
            if (typeof(IDictionary).IsAssignableFrom(dictionaryType)) {
                keyType = null;
                valueType = null;
                return;
            }

            throw new Exception("Type {0} is not a dictionary.".FormatWith(CultureInfo.InvariantCulture, dictionaryType));
        }
        public static Type GetMemberUnderlyingType(MemberInfo member)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));

            switch (member.MemberType()) {
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                default:
                    throw new ArgumentException("MemberInfo must be of type FieldInfo, PropertyInfo, EventInfo or MethodInfo", nameof(member));
            }
        }

        public static bool IsByRefLikeType(Type type)
        {
            if (!type.IsValueType()) {
                return false;
            }
            Attribute[] attributes = GetAttributes(type, null, false);
            for (int i = 0; i < attributes.Length; i++) {
                if (string.Equals(attributes[i].GetType().FullName, "System.Runtime.CompilerServices.IsByRefLikeAttribute", StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }
        public static bool IsIndexedProperty(PropertyInfo property)
        {
            ValidationUtils.ArgumentNotNull(property, nameof(property));

            return (property.GetIndexParameters().Length > 0);
        }
        public static object GetMemberValue(MemberInfo member, object target)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            switch (member.MemberType()) {
                case MemberTypes.Field:
                    return ((FieldInfo)member).GetValue(target);
                case MemberTypes.Property:
                    try {
                        return ((PropertyInfo)member).GetValue(target, null);
                    } catch (TargetParameterCountException e) {
                        throw new ArgumentException("MemberInfo '{0}' has index parameters".FormatWith(CultureInfo.InvariantCulture, member.Name), e);
                    }
                default:
                    throw new ArgumentException("MemberInfo '{0}' is not of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), nameof(member));
            }
        }
        public static void SetMemberValue(MemberInfo member, object target, object? value)
        {
            ValidationUtils.ArgumentNotNull(member, nameof(member));
            ValidationUtils.ArgumentNotNull(target, nameof(target));

            switch (member.MemberType()) {
                case MemberTypes.Field:
                    ((FieldInfo)member).SetValue(target, value);
                    break;
                case MemberTypes.Property:
                    ((PropertyInfo)member).SetValue(target, value, null);
                    break;
                default:
                    throw new ArgumentException("MemberInfo '{0}' must be of type FieldInfo or PropertyInfo".FormatWith(CultureInfo.InvariantCulture, member.Name), nameof(member));
            }
        }
        public static bool CanReadMemberValue(MemberInfo member, bool nonPublic)
        {
            switch (member.MemberType()) {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (nonPublic) {
                        return true;
                    } else if (fieldInfo.IsPublic) {
                        return true;
                    }
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanRead) {
                        return false;
                    }
                    if (nonPublic) {
                        return true;
                    }
                    return (propertyInfo.GetGetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }
        public static bool CanSetMemberValue(MemberInfo member, bool nonPublic, bool canSetReadOnly)
        {
            switch (member.MemberType()) {
                case MemberTypes.Field:
                    FieldInfo fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsLiteral) {
                        return false;
                    }
                    if (fieldInfo.IsInitOnly && !canSetReadOnly) {
                        return false;
                    }
                    if (nonPublic) {
                        return true;
                    }
                    if (fieldInfo.IsPublic) {
                        return true;
                    }
                    return false;
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)member;

                    if (!propertyInfo.CanWrite) {
                        return false;
                    }
                    if (nonPublic) {
                        return true;
                    }
                    return (propertyInfo.GetSetMethod(nonPublic) != null);
                default:
                    return false;
            }
        }

        public static List<MemberInfo> GetFieldsAndProperties(Type type, BindingFlags bindingAttr)
        {
            List<MemberInfo> targetMembers = new List<MemberInfo>();

            targetMembers.AddRange(GetFields(type, bindingAttr));
            targetMembers.AddRange(GetProperties(type, bindingAttr));
            List<MemberInfo> distinctMembers = new List<MemberInfo>(targetMembers.Count);

            foreach (IGrouping<string, MemberInfo> groupedMember in targetMembers.GroupBy(m => m.Name)) {
                int count = groupedMember.Count();

                if (count == 1) {
                    distinctMembers.Add(groupedMember.First());
                } else {
                    List<MemberInfo> resolvedMembers = new List<MemberInfo>();
                    foreach (MemberInfo memberInfo in groupedMember) {
                        if (resolvedMembers.Count == 0) {
                            resolvedMembers.Add(memberInfo);
                        } else if (!IsOverridenGenericMember(memberInfo, bindingAttr) || memberInfo.Name == "Item") {
                            if (resolvedMembers.Any(m => m.DeclaringType == memberInfo.DeclaringType)) {
                                continue;
                            }

                            resolvedMembers.Add(memberInfo);
                        }
                    }

                    distinctMembers.AddRange(resolvedMembers);
                }
            }

            return distinctMembers;
        }

        private static bool IsOverridenGenericMember(MemberInfo memberInfo, BindingFlags bindingAttr)
        {
            if (memberInfo.MemberType() != MemberTypes.Property) {
                return false;
            }

            PropertyInfo propertyInfo = (PropertyInfo)memberInfo;
            if (!IsVirtual(propertyInfo)) {
                return false;
            }

            Type declaringType = propertyInfo.DeclaringType;
            if (!declaringType.IsGenericType()) {
                return false;
            }
            Type genericTypeDefinition = declaringType.GetGenericTypeDefinition();
            if (genericTypeDefinition == null) {
                return false;
            }
            MemberInfo[] members = genericTypeDefinition.GetMember(propertyInfo.Name, bindingAttr);
            if (members.Length == 0) {
                return false;
            }
            Type memberUnderlyingType = GetMemberUnderlyingType(members[0]);
            if (!memberUnderlyingType.IsGenericParameter) {
                return false;
            }

            return true;
        }

        public static T? GetAttribute<T>(object attributeProvider) where T : Attribute
        {
            return GetAttribute<T>(attributeProvider, true);
        }

        public static T? GetAttribute<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            T[] attributes = GetAttributes<T>(attributeProvider, inherit);

            return attributes?.FirstOrDefault();
        }

#if !(DOTNET || PORTABLE) || NETSTANDARD2_0
        public static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            Attribute[] a = GetAttributes(attributeProvider, typeof(T), inherit);

            if (a is T[] attributes) {
                return attributes;
            }

            return a.Cast<T>().ToArray();
        }

        public static Attribute[] GetAttributes(object attributeProvider, Type? attributeType, bool inherit)
        {
            ValidationUtils.ArgumentNotNull(attributeProvider, nameof(attributeProvider));

            object provider = attributeProvider;

            switch (provider) {
                case Type t:
                    object[] array = attributeType != null ? t.GetCustomAttributes(attributeType, inherit) : t.GetCustomAttributes(inherit);
                    Attribute[] attributes = array.Cast<Attribute>().ToArray();

#if (NET20 || NET35)
                    if (inherit && t.BaseType != null)
                    {
                        attributes = attributes.Union(GetAttributes(t.BaseType, attributeType, inherit)).ToArray();
                    }
#endif

                    return attributes;
                case Assembly a:
                    return (attributeType != null) ? Attribute.GetCustomAttributes(a, attributeType) : Attribute.GetCustomAttributes(a);
                case MemberInfo mi:
                    return (attributeType != null) ? Attribute.GetCustomAttributes(mi, attributeType, inherit) : Attribute.GetCustomAttributes(mi, inherit);
#if !PORTABLE40
                case Module m:
                    return (attributeType != null) ? Attribute.GetCustomAttributes(m, attributeType, inherit) : Attribute.GetCustomAttributes(m, inherit);
#endif
                case ParameterInfo p:
                    return (attributeType != null) ? Attribute.GetCustomAttributes(p, attributeType, inherit) : Attribute.GetCustomAttributes(p, inherit);
                default:
#if !PORTABLE40
                    ICustomAttributeProvider customAttributeProvider = (ICustomAttributeProvider)attributeProvider;
                    object[] result = (attributeType != null) ? customAttributeProvider.GetCustomAttributes(attributeType, inherit) : customAttributeProvider.GetCustomAttributes(inherit);

                    return (Attribute[])result;
#else
                    throw new Exception("Cannot get attributes from '{0}'.".FormatWith(CultureInfo.InvariantCulture, provider));
#endif
            }
        }
#else
        public static T[] GetAttributes<T>(object attributeProvider, bool inherit) where T : Attribute
        {
            return GetAttributes(attributeProvider, typeof(T), inherit).Cast<T>().ToArray();
        }

        public static Attribute[] GetAttributes(object provider, Type? attributeType, bool inherit)
        {
            switch (provider)
            {
                case Type t:
                    return (Attribute[])((attributeType != null)
                        ? (t.GetTypeInfo().GetCustomAttributes(attributeType, inherit)).ToArray()
                        : t.GetTypeInfo().GetCustomAttributes(inherit).ToArray());
                case Assembly a:
                    return (attributeType != null) ? a.GetCustomAttributes(attributeType).ToArray() : a.GetCustomAttributes().ToArray();
                case MemberInfo memberInfo:
                    return (Attribute[])((attributeType != null) ? memberInfo.GetCustomAttributes(attributeType, inherit).ToArray() : memberInfo.GetCustomAttributes(inherit).ToArray());
                case Module module:
                    return (attributeType != null) ? module.GetCustomAttributes(attributeType).ToArray() : module.GetCustomAttributes().ToArray();
                case ParameterInfo parameterInfo:
                    return (Attribute[])((attributeType != null) ? parameterInfo.GetCustomAttributes(attributeType, inherit).ToArray() : parameterInfo.GetCustomAttributes(inherit).ToArray());
            }

            throw new Exception("Cannot get attributes from '{0}'.".FormatWith(CultureInfo.InvariantCulture, provider));
        }
#endif

        public static StructMultiKey<string?, string> SplitFullyQualifiedTypeName(string fullyQualifiedTypeName)
        {
            int? assemblyDelimiterIndex = GetAssemblyDelimiterIndex(fullyQualifiedTypeName);

            string typeName;
            string? assemblyName;

            if (assemblyDelimiterIndex != null) {
                typeName = fullyQualifiedTypeName.Trim(0, assemblyDelimiterIndex.GetValueOrDefault());
                assemblyName = fullyQualifiedTypeName.Trim(assemblyDelimiterIndex.GetValueOrDefault() + 1, fullyQualifiedTypeName.Length - assemblyDelimiterIndex.GetValueOrDefault() - 1);
            } else {
                typeName = fullyQualifiedTypeName;
                assemblyName = null;
            }

            return new StructMultiKey<string?, string>(assemblyName, typeName);
        }

        private static int? GetAssemblyDelimiterIndex(string fullyQualifiedTypeName)
        {
            int scope = 0;
            for (int i = 0; i < fullyQualifiedTypeName.Length; i++) {
                char current = fullyQualifiedTypeName[i];
                switch (current) {
                    case '[':
                        scope++;
                        break;
                    case ']':
                        scope--;
                        break;
                    case ',':
                        if (scope == 0) {
                            return i;
                        }
                        break;
                }
            }

            return null;
        }

        public static MemberInfo GetMemberInfoFromType(Type targetType, MemberInfo memberInfo)
        {
            const BindingFlags bindingAttr = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

            switch (memberInfo.MemberType()) {
                case MemberTypes.Property:
                    PropertyInfo propertyInfo = (PropertyInfo)memberInfo;

                    Type[] types = propertyInfo.GetIndexParameters().Select(p => p.ParameterType).ToArray();

                    return targetType.GetProperty(propertyInfo.Name, bindingAttr, null, propertyInfo.PropertyType, types, null);
                default:
                    return targetType.GetMember(memberInfo.Name, memberInfo.MemberType(), bindingAttr).SingleOrDefault();
            }
        }

        public static IEnumerable<FieldInfo> GetFields(Type targetType, BindingFlags bindingAttr)
        {
            ValidationUtils.ArgumentNotNull(targetType, nameof(targetType));

            List<MemberInfo> fieldInfos = new List<MemberInfo>(targetType.GetFields(bindingAttr));
#if !PORTABLE
            GetChildPrivateFields(fieldInfos, targetType, bindingAttr);
#endif

            return fieldInfos.Cast<FieldInfo>();
        }

#if !PORTABLE
        private static void GetChildPrivateFields(IList<MemberInfo> initialFields, Type targetType, BindingFlags bindingAttr)
        {
            if ((bindingAttr & BindingFlags.NonPublic) != 0)
            {
                BindingFlags nonPublicBindingAttr = bindingAttr.RemoveFlag(BindingFlags.Public);

                while ((targetType = targetType.BaseType()) != null)
                {
                    IEnumerable<FieldInfo> childPrivateFields =
                        targetType.GetFields(nonPublicBindingAttr).Where(f => f.IsPrivate);

                    initialFields.AddRange(childPrivateFields);
                }
            }
        }
#endif

        public static IEnumerable<PropertyInfo> GetProperties(Type targetType, BindingFlags bindingAttr)
        {
            ValidationUtils.ArgumentNotNull(targetType, nameof(targetType));

            List<PropertyInfo> propertyInfos = new List<PropertyInfo>(targetType.GetProperties(bindingAttr));
            if (targetType.IsInterface()) {
                foreach (Type i in targetType.GetInterfaces()) {
                    propertyInfos.AddRange(i.GetProperties(bindingAttr));
                }
            }

            GetChildPrivateProperties(propertyInfos, targetType, bindingAttr);
            for (int i = 0; i < propertyInfos.Count; i++) {
                PropertyInfo member = propertyInfos[i];
                if (member.DeclaringType != targetType) {
                    PropertyInfo declaredMember = (PropertyInfo)GetMemberInfoFromType(member.DeclaringType, member);
                    propertyInfos[i] = declaredMember;
                }
            }

            return propertyInfos;
        }

        public static BindingFlags RemoveFlag(this BindingFlags bindingAttr, BindingFlags flag)
        {
            return ((bindingAttr & flag) == flag)
                ? bindingAttr ^ flag
                : bindingAttr;
        }

        private static void GetChildPrivateProperties(IList<PropertyInfo> initialProperties, Type targetType, BindingFlags bindingAttr)
        {

            while ((targetType = targetType.BaseType()) != null) {
                foreach (PropertyInfo propertyInfo in targetType.GetProperties(bindingAttr)) {
                    PropertyInfo subTypeProperty = propertyInfo;

                    if (!subTypeProperty.IsVirtual()) {
                        if (!IsPublic(subTypeProperty)) {
                            int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name);
                            if (index == -1) {
                                initialProperties.Add(subTypeProperty);
                            } else {
                                PropertyInfo childProperty = initialProperties[index];
                                if (!IsPublic(childProperty)) {
                                    initialProperties[index] = subTypeProperty;
                                }
                            }
                        } else {
                            int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                                                       && p.DeclaringType == subTypeProperty.DeclaringType);

                            if (index == -1) {
                                initialProperties.Add(subTypeProperty);
                            }
                        }
                    } else {
                        Type subTypePropertyDeclaringType = subTypeProperty.GetBaseDefinition()?.DeclaringType ?? subTypeProperty.DeclaringType;

                        int index = initialProperties.IndexOf(p => p.Name == subTypeProperty.Name
                                                                   && p.IsVirtual()
                                                                   && (p.GetBaseDefinition()?.DeclaringType ?? p.DeclaringType).IsAssignableFrom(subTypePropertyDeclaringType));
                        if (index == -1) {
                            initialProperties.Add(subTypeProperty);
                        }
                    }
                }
            }
        }

        public static bool IsMethodOverridden(Type currentType, Type methodDeclaringType, string method)
        {
            bool isMethodOverriden = currentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Any(info =>
                    info.Name == method &&
                    info.DeclaringType != methodDeclaringType
                    && info.GetBaseDefinition().DeclaringType == methodDeclaringType
                );

            return isMethodOverriden;
        }

        public static object? GetDefaultValue(Type type)
        {
            if (!type.IsValueType()) {
                return null;
            }

            switch (ConvertUtils.GetTypeCode(type)) {
                case PrimitiveTypeCode.Boolean:
                    return false;
                case PrimitiveTypeCode.Char:
                case PrimitiveTypeCode.SByte:
                case PrimitiveTypeCode.Byte:
                case PrimitiveTypeCode.Int16:
                case PrimitiveTypeCode.UInt16:
                case PrimitiveTypeCode.Int32:
                case PrimitiveTypeCode.UInt32:
                    return 0;
                case PrimitiveTypeCode.Int64:
                case PrimitiveTypeCode.UInt64:
                    return 0L;
                case PrimitiveTypeCode.Single:
                    return 0f;
                case PrimitiveTypeCode.Double:
                    return 0.0;
                case PrimitiveTypeCode.Decimal:
                    return 0m;
                case PrimitiveTypeCode.DateTime:
                    return new DateTime();
#if HAVE_BIG_INTEGER
                case PrimitiveTypeCode.BigInteger:
                    return new BigInteger();
#endif
                case PrimitiveTypeCode.Guid:
                    return new Guid();
#if HAVE_DATE_TIME_OFFSET
                case PrimitiveTypeCode.DateTimeOffset:
                    return new DateTimeOffset();
#endif
            }

            if (IsNullable(type)) {
                return null;
            }
            return Activator.CreateInstance(type);
        }
    }
}