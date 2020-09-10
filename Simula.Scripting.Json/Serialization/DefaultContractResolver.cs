
using System;
using System.Collections;
#if HAVE_CONCURRENT_DICTIONARY
using System.Collections.Concurrent;
#endif
using Simula.Scripting.Json.Schema;
using System.Collections.Generic;
using System.ComponentModel;
#if HAVE_DYNAMIC
using System.Dynamic;
#endif
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
#if HAVE_CAS
using System.Security.Permissions;
#endif
using Simula.Scripting.Json.Converters;
using Simula.Scripting.Json.Utilities;
using Simula.Scripting.Json.Linq;
using System.Runtime.CompilerServices;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json.Serialization
{
    public class DefaultContractResolver : IContractResolver
    {
        private static readonly IContractResolver _instance = new DefaultContractResolver();
        internal static IContractResolver Instance => _instance;

        private static readonly string[] BlacklistedTypeNames =
        {
            "System.IO.DriveInfo",
            "System.IO.FileInfo",
            "System.IO.DirectoryInfo"
        };

        private static readonly JsonConverter[] BuiltInConverters =
        {
#if HAVE_ENTITY_FRAMEWORK
            new EntityKeyMemberConverter(),
#endif
#if HAVE_DYNAMIC
            new ExpandoObjectConverter(),
#endif
#if (HAVE_XML_DOCUMENT || HAVE_XLINQ)
            new XmlNodeConverter(),
#endif
#if HAVE_ADO_NET
            new BinaryConverter(),
            new DataSetConverter(),
            new DataTableConverter(),
#endif
#if HAVE_FSHARP_TYPES
            new DiscriminatedUnionConverter(),
#endif
            new KeyValuePairConverter(),
#pragma warning disable 618
            new BsonObjectIdConverter(),
#pragma warning restore 618
            new RegexConverter()
        };

        private readonly DefaultJsonNameTable _nameTable = new DefaultJsonNameTable();

        private readonly ThreadSafeStore<Type, JsonContract> _contractCache;
        public bool DynamicCodeGeneration => JsonTypeReflector.DynamicCodeGeneration;

#if !PORTABLE
        [Obsolete("DefaultMembersSearchFlags is obsolete. To modify the members serialized inherit from DefaultContractResolver and override the GetSerializableMembers method instead.")]
        public BindingFlags DefaultMembersSearchFlags { get; set; }
#else
        private readonly BindingFlags DefaultMembersSearchFlags;
#endif
        public bool SerializeCompilerGeneratedMembers { get; set; }

#if HAVE_BINARY_SERIALIZATION
        public bool IgnoreSerializableInterface { get; set; }
        public bool IgnoreSerializableAttribute { get; set; }
#endif
        public bool IgnoreIsSpecifiedMembers { get; set; }
        public bool IgnoreShouldSerializeMembers { get; set; }
        public NamingStrategy? NamingStrategy { get; set; }
        public DefaultContractResolver()
        {
#if HAVE_BINARY_SERIALIZATION
            IgnoreSerializableAttribute = true;
#endif

#pragma warning disable 618
            DefaultMembersSearchFlags = BindingFlags.Instance | BindingFlags.Public;
#pragma warning restore 618

            _contractCache = new ThreadSafeStore<Type, JsonContract>(CreateContract);
        }
        public virtual JsonContract ResolveContract(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));

            return _contractCache.Get(type);
        }

        private static bool FilterMembers(MemberInfo member)
        {
            if (member is PropertyInfo property)
            {
                if (ReflectionUtils.IsIndexedProperty(property))
                {
                    return false;
                }

                return !ReflectionUtils.IsByRefLikeType(property.PropertyType);
            }
            else if (member is FieldInfo field)
            {
                return !ReflectionUtils.IsByRefLikeType(field.FieldType);
            }

            return true;
        }
        protected virtual List<MemberInfo> GetSerializableMembers(Type objectType)
        {
            bool ignoreSerializableAttribute;
#if HAVE_BINARY_SERIALIZATION
            ignoreSerializableAttribute = IgnoreSerializableAttribute;
#else
            ignoreSerializableAttribute = true;
#endif

            MemberSerialization memberSerialization = JsonTypeReflector.GetObjectMemberSerialization(objectType, ignoreSerializableAttribute);
            IEnumerable<MemberInfo> allMembers = ReflectionUtils.GetFieldsAndProperties(objectType, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m is PropertyInfo p ? !ReflectionUtils.IsIndexedProperty(p) : true);

            List<MemberInfo> serializableMembers = new List<MemberInfo>();

            if (memberSerialization != MemberSerialization.Fields)
            {
#if HAVE_DATA_CONTRACTS
                DataContractAttribute? dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(objectType);
#endif

#pragma warning disable 618
                List<MemberInfo> defaultMembers = ReflectionUtils.GetFieldsAndProperties(objectType, DefaultMembersSearchFlags)
                    .Where(FilterMembers).ToList();
#pragma warning restore 618

                foreach (MemberInfo member in allMembers)
                {
                    if (SerializeCompilerGeneratedMembers || !member.IsDefined(typeof(CompilerGeneratedAttribute), true))
                    {
                        if (defaultMembers.Contains(member))
                        {
                            serializableMembers.Add(member);
                        }
                        else
                        {
                            if (JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(member) != null)
                            {
                                serializableMembers.Add(member);
                            }
                            else if (JsonTypeReflector.GetAttribute<JsonRequiredAttribute>(member) != null)
                            {
                                serializableMembers.Add(member);
                            }
#if HAVE_DATA_CONTRACTS
                            else if (dataContractAttribute != null && JsonTypeReflector.GetAttribute<DataMemberAttribute>(member) != null)
                            {
                                serializableMembers.Add(member);
                            }
#endif
                            else if (memberSerialization == MemberSerialization.Fields && member.MemberType() == MemberTypes.Field)
                            {
                                serializableMembers.Add(member);
                            }
                        }
                    }
                }

#if HAVE_DATA_CONTRACTS
                if (objectType.AssignableToTypeName("System.Data.Objects.DataClasses.EntityObject", false, out _))
                {
                    serializableMembers = serializableMembers.Where(ShouldSerializeEntityMember).ToList();
                }
#endif
                if (typeof(Exception).IsAssignableFrom(objectType))
                {
                    serializableMembers = serializableMembers.Where(m => !string.Equals(m.Name, "TargetSite", StringComparison.Ordinal)).ToList();
                }
            }
            else
            {
                foreach (MemberInfo member in allMembers)
                {
                    if (member is FieldInfo field && !field.IsStatic)
                    {
                        serializableMembers.Add(member);
                    }
                }
            }

            return serializableMembers;
        }

#if HAVE_DATA_CONTRACTS
        private bool ShouldSerializeEntityMember(MemberInfo memberInfo)
        {
            if (memberInfo is PropertyInfo propertyInfo)
            {
                if (propertyInfo.PropertyType.IsGenericType() && propertyInfo.PropertyType.GetGenericTypeDefinition().FullName == "System.Data.Objects.DataClasses.EntityReference`1")
                {
                    return false;
                }
            }

            return true;
        }
#endif
        protected virtual JsonObjectContract CreateObjectContract(Type objectType)
        {
            JsonObjectContract contract = new JsonObjectContract(objectType);
            InitializeContract(contract);

            bool ignoreSerializableAttribute;
#if HAVE_BINARY_SERIALIZATION
            ignoreSerializableAttribute = IgnoreSerializableAttribute;
#else
            ignoreSerializableAttribute = true;
#endif

            contract.MemberSerialization = JsonTypeReflector.GetObjectMemberSerialization(contract.NonNullableUnderlyingType, ignoreSerializableAttribute);
            contract.Properties.AddRange(CreateProperties(contract.NonNullableUnderlyingType, contract.MemberSerialization));

            Func<string, string>? extensionDataNameResolver = null;

            JsonObjectAttribute? attribute = JsonTypeReflector.GetCachedAttribute<JsonObjectAttribute>(contract.NonNullableUnderlyingType);
            if (attribute != null)
            {
                contract.ItemRequired = attribute._itemRequired;
                contract.ItemNullValueHandling = attribute._itemNullValueHandling;
                contract.MissingMemberHandling = attribute._missingMemberHandling;

                if (attribute.NamingStrategyType != null)
                {
                    NamingStrategy namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(attribute)!;
                    extensionDataNameResolver = s => namingStrategy.GetDictionaryKey(s);
                }
            }

            if (extensionDataNameResolver == null)
            {
                extensionDataNameResolver = ResolveExtensionDataName;
            }

            contract.ExtensionDataNameResolver = extensionDataNameResolver;

            if (contract.IsInstantiable)
            {
                ConstructorInfo? overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);
                if (overrideConstructor != null)
                {
                    contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(overrideConstructor);
                    contract.CreatorParameters.AddRange(CreateConstructorParameters(overrideConstructor, contract.Properties));
                }
                else if (contract.MemberSerialization == MemberSerialization.Fields)
                {
#if HAVE_BINARY_FORMATTER
                    if (JsonTypeReflector.FullyTrusted)
                    {
                        contract.DefaultCreator = contract.GetUninitializedObject;
                    }
#endif
                }
                else if (contract.DefaultCreator == null || contract.DefaultCreatorNonPublic)
                {
                    ConstructorInfo? constructor = GetParameterizedConstructor(contract.NonNullableUnderlyingType);
                    if (constructor != null)
                    {
                        contract.ParameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
                        contract.CreatorParameters.AddRange(CreateConstructorParameters(constructor, contract.Properties));
                    }
                }
                else if (contract.NonNullableUnderlyingType.IsValueType())
                {
                    ConstructorInfo? constructor = GetImmutableConstructor(contract.NonNullableUnderlyingType, contract.Properties);
                    if (constructor != null)
                    {
                        contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructor);
                        contract.CreatorParameters.AddRange(CreateConstructorParameters(constructor, contract.Properties));
                    }
                }
            }

            MemberInfo extensionDataMember = GetExtensionDataMemberForType(contract.NonNullableUnderlyingType);
            if (extensionDataMember != null)
            {
                SetExtensionDataDelegates(contract, extensionDataMember);
            }
            if (Array.IndexOf(BlacklistedTypeNames, objectType.FullName) != -1)
            {
                contract.OnSerializingCallbacks.Add(ThrowUnableToSerializeError);
            }

            return contract;
        }

        private static void ThrowUnableToSerializeError(object o, StreamingContext context)
        {
            throw new JsonSerializationException("Unable to serialize instance of '{0}'.".FormatWith(CultureInfo.InvariantCulture, o.GetType()));
        }

        private MemberInfo GetExtensionDataMemberForType(Type type)
        {
            IEnumerable<MemberInfo> members = GetClassHierarchyForType(type).SelectMany(baseType =>
            {
                IList<MemberInfo> m = new List<MemberInfo>();
                m.AddRange(baseType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
                m.AddRange(baseType.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));

                return m;
            });

            MemberInfo extensionDataMember = members.LastOrDefault(m =>
            {
                MemberTypes memberType = m.MemberType();
                if (memberType != MemberTypes.Property && memberType != MemberTypes.Field)
                {
                    return false;
                }
                if (!m.IsDefined(typeof(JsonExtensionDataAttribute), false))
                {
                    return false;
                }

                if (!ReflectionUtils.CanReadMemberValue(m, true))
                {
                    throw new JsonException("Invalid extension data attribute on '{0}'. Member '{1}' must have a getter.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(m.DeclaringType), m.Name));
                }

                Type t = ReflectionUtils.GetMemberUnderlyingType(m);

                if (ReflectionUtils.ImplementsGenericDefinition(t, typeof(IDictionary<,>), out Type? dictionaryType))
                {
                    Type keyType = dictionaryType.GetGenericArguments()[0];
                    Type valueType = dictionaryType.GetGenericArguments()[1];

                    if (keyType.IsAssignableFrom(typeof(string)) && valueType.IsAssignableFrom(typeof(JToken)))
                    {
                        return true;
                    }
                }

                throw new JsonException("Invalid extension data attribute on '{0}'. Member '{1}' type must implement IDictionary<string, JToken>.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(m.DeclaringType), m.Name));
            });

            return extensionDataMember;
        }

        private static void SetExtensionDataDelegates(JsonObjectContract contract, MemberInfo member)
        {
            JsonExtensionDataAttribute? extensionDataAttribute = ReflectionUtils.GetAttribute<JsonExtensionDataAttribute>(member);
            if (extensionDataAttribute == null)
            {
                return;
            }

            Type t = ReflectionUtils.GetMemberUnderlyingType(member);

            ReflectionUtils.ImplementsGenericDefinition(t, typeof(IDictionary<,>), out Type? dictionaryType);

            Type keyType = dictionaryType!.GetGenericArguments()[0];
            Type valueType = dictionaryType!.GetGenericArguments()[1];

            Type createdType;
            if (ReflectionUtils.IsGenericDefinition(t, typeof(IDictionary<,>)))
            {
                createdType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
            }
            else
            {
                createdType = t;
            }

            Func<object, object?> getExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(member);

            if (extensionDataAttribute.ReadData)
            {
                Action<object, object?>? setExtensionDataDictionary = (ReflectionUtils.CanSetMemberValue(member, true, false))
                 ? JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(member)
                 : null;
                Func<object> createExtensionDataDictionary = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
                MethodInfo? setMethod = t.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, valueType, new[] { keyType }, null)?.GetSetMethod();
                if (setMethod == null)
                {
                    setMethod = dictionaryType!.GetProperty("Item", BindingFlags.Public | BindingFlags.Instance, null, valueType, new[] { keyType }, null)?.GetSetMethod();
                }

                MethodCall<object, object?> setExtensionDataDictionaryValue = JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(setMethod!);

                ExtensionDataSetter extensionDataSetter = (o, key, value) =>
                {
                    object? dictionary = getExtensionDataDictionary(o);
                    if (dictionary == null)
                    {
                        if (setExtensionDataDictionary == null)
                        {
                            throw new JsonSerializationException("Cannot set value onto extension data member '{0}'. The extension data collection is null and it cannot be set.".FormatWith(CultureInfo.InvariantCulture, member.Name));
                        }

                        dictionary = createExtensionDataDictionary();
                        setExtensionDataDictionary(o, dictionary);
                    }

                    setExtensionDataDictionaryValue(dictionary, key, value);
                };

                contract.ExtensionDataSetter = extensionDataSetter;
            }

            if (extensionDataAttribute.WriteData)
            {
                Type enumerableWrapper = typeof(EnumerableDictionaryWrapper<,>).MakeGenericType(keyType, valueType);
                ConstructorInfo constructors = enumerableWrapper.GetConstructors().First();
                ObjectConstructor<object> createEnumerableWrapper = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructors);

                ExtensionDataGetter extensionDataGetter = o =>
                {
                    object? dictionary = getExtensionDataDictionary(o);
                    if (dictionary == null)
                    {
                        return null;
                    }

                    return (IEnumerable<KeyValuePair<object, object>>)createEnumerableWrapper(dictionary);
                };

                contract.ExtensionDataGetter = extensionDataGetter;
            }

            contract.ExtensionDataValueType = valueType;
        }
        internal class EnumerableDictionaryWrapper<TEnumeratorKey, TEnumeratorValue> : IEnumerable<KeyValuePair<object, object>>
        {
            private readonly IEnumerable<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> _e;

            public EnumerableDictionaryWrapper(IEnumerable<KeyValuePair<TEnumeratorKey, TEnumeratorValue>> e)
            {
                ValidationUtils.ArgumentNotNull(e, nameof(e));
                _e = e;
            }

            public IEnumerator<KeyValuePair<object, object>> GetEnumerator()
            {
                foreach (KeyValuePair<TEnumeratorKey, TEnumeratorValue> item in _e)
                {
                    yield return new KeyValuePair<object, object>(item.Key!, item.Value!);
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        private ConstructorInfo? GetAttributeConstructor(Type objectType)
        {
            IEnumerator<ConstructorInfo> en = objectType.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).Where(c => c.IsDefined(typeof(JsonConstructorAttribute), true)).GetEnumerator();

            if (en.MoveNext())
            {
                ConstructorInfo conInfo = en.Current;
                if (en.MoveNext())
                {
                    throw new JsonException("Multiple constructors with the JsonConstructorAttribute.");
                }

                return conInfo;
            }
            if (objectType == typeof(Version))
            {
                return objectType.GetConstructor(new[] { typeof(int), typeof(int), typeof(int), typeof(int) });
            }

            return null;
        }

        private ConstructorInfo? GetImmutableConstructor(Type objectType, JsonPropertyCollection memberProperties)
        {
            IEnumerable<ConstructorInfo> constructors = objectType.GetConstructors();
            IEnumerator<ConstructorInfo> en = constructors.GetEnumerator();
            if (en.MoveNext())
            {
                ConstructorInfo constructor = en.Current;
                if (!en.MoveNext())
                {
                    ParameterInfo[] parameters = constructor.GetParameters();
                    if (parameters.Length > 0)
                    {
                        foreach (ParameterInfo parameterInfo in parameters)
                        {
                            JsonProperty? memberProperty = MatchProperty(memberProperties, parameterInfo.Name, parameterInfo.ParameterType);
                            if (memberProperty == null || memberProperty.Writable)
                            {
                                return null;
                            }
                        }

                        return constructor;
                    }
                }
            }

            return null;
        }

        private ConstructorInfo? GetParameterizedConstructor(Type objectType)
        {
#if PORTABLE
            IEnumerable<ConstructorInfo> constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            IEnumerator<ConstructorInfo> en = constructors.GetEnumerator();
            if (en.MoveNext())
            {
                ConstructorInfo conInfo = en.Current;
                if (!en.MoveNext())
                {
                    return conInfo;
                }
            }
#else
            ConstructorInfo[] constructors = objectType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (constructors.Length == 1)
            {
                return constructors[0];
            }
#endif
            return null;
        }
        protected virtual IList<JsonProperty> CreateConstructorParameters(ConstructorInfo constructor, JsonPropertyCollection memberProperties)
        {
            ParameterInfo[] constructorParameters = constructor.GetParameters();

            JsonPropertyCollection parameterCollection = new JsonPropertyCollection(constructor.DeclaringType);

            foreach (ParameterInfo parameterInfo in constructorParameters)
            {
                if (parameterInfo.Name == null)
                {
                    continue;
                }

                JsonProperty? matchingMemberProperty = MatchProperty(memberProperties, parameterInfo.Name, parameterInfo.ParameterType);
                if (matchingMemberProperty != null || parameterInfo.Name != null)
                {
                    JsonProperty property = CreatePropertyFromConstructorParameter(matchingMemberProperty, parameterInfo);

                    if (property != null)
                    {
                        parameterCollection.AddProperty(property);
                    }
                }
            }

            return parameterCollection;
        }

        private JsonProperty? MatchProperty(JsonPropertyCollection properties, string name, Type type)
        {
            if (name == null)
            {
                return null;
            }

            JsonProperty? property = properties.GetClosestMatchProperty(name);
            if (property == null || property.PropertyType != type)
            {
                return null;
            }

            return property;
        }
        protected virtual JsonProperty CreatePropertyFromConstructorParameter(JsonProperty? matchingMemberProperty, ParameterInfo parameterInfo)
        {
            JsonProperty property = new JsonProperty();
            property.PropertyType = parameterInfo.ParameterType;
            property.AttributeProvider = new ReflectionAttributeProvider(parameterInfo);

            SetPropertySettingsFromAttributes(property, parameterInfo, parameterInfo.Name, parameterInfo.Member.DeclaringType, MemberSerialization.OptOut, out _);

            property.Readable = false;
            property.Writable = true;
            if (matchingMemberProperty != null)
            {
                property.PropertyName = (property.PropertyName != parameterInfo.Name) ? property.PropertyName : matchingMemberProperty.PropertyName;
                property.Converter = property.Converter ?? matchingMemberProperty.Converter;

                if (!property._hasExplicitDefaultValue && matchingMemberProperty._hasExplicitDefaultValue)
                {
                    property.DefaultValue = matchingMemberProperty.DefaultValue;
                }

                property._required = property._required ?? matchingMemberProperty._required;
                property.IsReference = property.IsReference ?? matchingMemberProperty.IsReference;
                property.NullValueHandling = property.NullValueHandling ?? matchingMemberProperty.NullValueHandling;
                property.DefaultValueHandling = property.DefaultValueHandling ?? matchingMemberProperty.DefaultValueHandling;
                property.ReferenceLoopHandling = property.ReferenceLoopHandling ?? matchingMemberProperty.ReferenceLoopHandling;
                property.ObjectCreationHandling = property.ObjectCreationHandling ?? matchingMemberProperty.ObjectCreationHandling;
                property.TypeNameHandling = property.TypeNameHandling ?? matchingMemberProperty.TypeNameHandling;
            }

            return property;
        }
        protected virtual JsonConverter? ResolveContractConverter(Type objectType)
        {
            return JsonTypeReflector.GetJsonConverter(objectType);
        }

        private Func<object> GetDefaultCreator(Type createdType)
        {
            return JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(createdType);
        }

#if NET35
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Portability", "CA1903:UseOnlyApiFromTargetedFramework", MessageId = "System.Runtime.Serialization.DataContractAttribute.#get_IsReference()")]
#endif
        private void InitializeContract(JsonContract contract)
        {
            JsonContainerAttribute? containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(contract.NonNullableUnderlyingType);
            if (containerAttribute != null)
            {
                contract.IsReference = containerAttribute._isReference;
            }
#if HAVE_DATA_CONTRACTS
            else
            {
                DataContractAttribute? dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(contract.NonNullableUnderlyingType);
                if (dataContractAttribute != null && dataContractAttribute.IsReference)
                {
                    contract.IsReference = true;
                }
            }
#endif

            contract.Converter = ResolveContractConverter(contract.NonNullableUnderlyingType);
            contract.InternalConverter = JsonSerializer.GetMatchingConverter(BuiltInConverters, contract.NonNullableUnderlyingType);

            if (contract.IsInstantiable
                && (ReflectionUtils.HasDefaultConstructor(contract.CreatedType, true) || contract.CreatedType.IsValueType()))
            {
                contract.DefaultCreator = GetDefaultCreator(contract.CreatedType);

                contract.DefaultCreatorNonPublic = (!contract.CreatedType.IsValueType() &&
                                                    ReflectionUtils.GetDefaultConstructor(contract.CreatedType) == null);
            }

            ResolveCallbackMethods(contract, contract.NonNullableUnderlyingType);
        }

        private void ResolveCallbackMethods(JsonContract contract, Type t)
        {
            GetCallbackMethodsForType(
                t,
                out List<SerializationCallback>? onSerializing,
                out List<SerializationCallback>? onSerialized,
                out List<SerializationCallback>? onDeserializing,
                out List<SerializationCallback>? onDeserialized,
                out List<SerializationErrorCallback>? onError);

            if (onSerializing != null)
            {
                contract.OnSerializingCallbacks.AddRange(onSerializing);
            }

            if (onSerialized != null)
            {
                contract.OnSerializedCallbacks.AddRange(onSerialized);
            }

            if (onDeserializing != null)
            {
                contract.OnDeserializingCallbacks.AddRange(onDeserializing);
            }

            if (onDeserialized != null)
            {
                contract.OnDeserializedCallbacks.AddRange(onDeserialized);
            }

            if (onError != null)
            {
                contract.OnErrorCallbacks.AddRange(onError);
            }
        }

        private void GetCallbackMethodsForType(Type type, out List<SerializationCallback>? onSerializing, out List<SerializationCallback>? onSerialized, out List<SerializationCallback>? onDeserializing, out List<SerializationCallback>? onDeserialized, out List<SerializationErrorCallback>? onError)
        {
            onSerializing = null;
            onSerialized = null;
            onDeserializing = null;
            onDeserialized = null;
            onError = null;

            foreach (Type baseType in GetClassHierarchyForType(type))
            {
                MethodInfo? currentOnSerializing = null;
                MethodInfo? currentOnSerialized = null;
                MethodInfo? currentOnDeserializing = null;
                MethodInfo? currentOnDeserialized = null;
                MethodInfo? currentOnError = null;

                bool skipSerializing = ShouldSkipSerializing(baseType);
                bool skipDeserialized = ShouldSkipDeserialized(baseType);

                foreach (MethodInfo method in baseType.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly))
                {
                    if (method.ContainsGenericParameters)
                    {
                        continue;
                    }

                    Type? prevAttributeType = null;
                    ParameterInfo[] parameters = method.GetParameters();

                    if (!skipSerializing && IsValidCallback(method, parameters, typeof(OnSerializingAttribute), currentOnSerializing, ref prevAttributeType))
                    {
                        onSerializing = onSerializing ?? new List<SerializationCallback>();
                        onSerializing.Add(JsonContract.CreateSerializationCallback(method));
                        currentOnSerializing = method;
                    }
                    if (IsValidCallback(method, parameters, typeof(OnSerializedAttribute), currentOnSerialized, ref prevAttributeType))
                    {
                        onSerialized = onSerialized ?? new List<SerializationCallback>();
                        onSerialized.Add(JsonContract.CreateSerializationCallback(method));
                        currentOnSerialized = method;
                    }
                    if (IsValidCallback(method, parameters, typeof(OnDeserializingAttribute), currentOnDeserializing, ref prevAttributeType))
                    {
                        onDeserializing = onDeserializing ?? new List<SerializationCallback>();
                        onDeserializing.Add(JsonContract.CreateSerializationCallback(method));
                        currentOnDeserializing = method;
                    }
                    if (!skipDeserialized && IsValidCallback(method, parameters, typeof(OnDeserializedAttribute), currentOnDeserialized, ref prevAttributeType))
                    {
                        onDeserialized = onDeserialized ?? new List<SerializationCallback>();
                        onDeserialized.Add(JsonContract.CreateSerializationCallback(method));
                        currentOnDeserialized = method;
                    }
                    if (IsValidCallback(method, parameters, typeof(OnErrorAttribute), currentOnError, ref prevAttributeType))
                    {
                        onError = onError ?? new List<SerializationErrorCallback>();
                        onError.Add(JsonContract.CreateSerializationErrorCallback(method));
                        currentOnError = method;
                    }
                }
            }
        }

        private static bool IsConcurrentOrObservableCollection(Type t)
        {
            if (t.IsGenericType())
            {
                Type definition = t.GetGenericTypeDefinition();

                switch (definition.FullName)
                {
                    case "System.Collections.Concurrent.ConcurrentQueue`1":
                    case "System.Collections.Concurrent.ConcurrentStack`1":
                    case "System.Collections.Concurrent.ConcurrentBag`1":
                    case JsonTypeReflector.ConcurrentDictionaryTypeName:
                    case "System.Collections.ObjectModel.ObservableCollection`1":
                        return true;
                }
            }

            return false;
        }

        private static bool ShouldSkipDeserialized(Type t)
        {
            if (IsConcurrentOrObservableCollection(t))
            {
                return true;
            }

#if HAVE_FSHARP_TYPES
            if (t.Name == FSharpUtils.FSharpSetTypeName || t.Name == FSharpUtils.FSharpMapTypeName)
            {
                return true;
            }
#endif

            return false;
        }

        private static bool ShouldSkipSerializing(Type t)
        {
            if (IsConcurrentOrObservableCollection(t))
            {
                return true;
            }

#if HAVE_FSHARP_TYPES
            if (t.Name == FSharpUtils.FSharpSetTypeName || t.Name == FSharpUtils.FSharpMapTypeName)
            {
                return true;
            }
#endif

            return false;
        }

        private List<Type> GetClassHierarchyForType(Type type)
        {
            List<Type> ret = new List<Type>();

            Type current = type;
            while (current != null && current != typeof(object))
            {
                ret.Add(current);
                current = current.BaseType();
            }
            ret.Reverse();
            return ret;
        }
        protected virtual JsonDictionaryContract CreateDictionaryContract(Type objectType)
        {
            JsonDictionaryContract contract = new JsonDictionaryContract(objectType);
            InitializeContract(contract);

            JsonContainerAttribute? containerAttribute = JsonTypeReflector.GetAttribute<JsonContainerAttribute>(objectType);
            if (containerAttribute?.NamingStrategyType != null)
            {
                NamingStrategy namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(containerAttribute)!;
                contract.DictionaryKeyResolver = s => namingStrategy.GetDictionaryKey(s);
            }
            else
            {
                contract.DictionaryKeyResolver = ResolveDictionaryKey;
            }

            ConstructorInfo? overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

            if (overrideConstructor != null)
            {
                ParameterInfo[] parameters = overrideConstructor.GetParameters();
                Type expectedParameterType = (contract.DictionaryKeyType != null && contract.DictionaryValueType != null)
                    ? typeof(IEnumerable<>).MakeGenericType(typeof(KeyValuePair<,>).MakeGenericType(contract.DictionaryKeyType, contract.DictionaryValueType))
                    : typeof(IDictionary);

                if (parameters.Length == 0)
                {
                    contract.HasParameterizedCreator = false;
                }
                else if (parameters.Length == 1 && expectedParameterType.IsAssignableFrom(parameters[0].ParameterType))
                {
                    contract.HasParameterizedCreator = true;
                }
                else
                {
                    throw new JsonException("Constructor for '{0}' must have no parameters or a single parameter that implements '{1}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, expectedParameterType));
                }

                contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(overrideConstructor);
            }

            return contract;
        }
        protected virtual JsonArrayContract CreateArrayContract(Type objectType)
        {
            JsonArrayContract contract = new JsonArrayContract(objectType);
            InitializeContract(contract);

            ConstructorInfo? overrideConstructor = GetAttributeConstructor(contract.NonNullableUnderlyingType);

            if (overrideConstructor != null)
            {
                ParameterInfo[] parameters = overrideConstructor.GetParameters();
                Type expectedParameterType = (contract.CollectionItemType != null)
                    ? typeof(IEnumerable<>).MakeGenericType(contract.CollectionItemType)
                    : typeof(IEnumerable);

                if (parameters.Length == 0)
                {
                    contract.HasParameterizedCreator = false;
                }
                else if (parameters.Length == 1 && expectedParameterType.IsAssignableFrom(parameters[0].ParameterType))
                {
                    contract.HasParameterizedCreator = true;
                }
                else
                {
                    throw new JsonException("Constructor for '{0}' must have no parameters or a single parameter that implements '{1}'.".FormatWith(CultureInfo.InvariantCulture, contract.UnderlyingType, expectedParameterType));
                }

                contract.OverrideCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(overrideConstructor);
            }

            return contract;
        }
        protected virtual JsonPrimitiveContract CreatePrimitiveContract(Type objectType)
        {
            JsonPrimitiveContract contract = new JsonPrimitiveContract(objectType);
            InitializeContract(contract);

            return contract;
        }
        protected virtual JsonLinqContract CreateLinqContract(Type objectType)
        {
            JsonLinqContract contract = new JsonLinqContract(objectType);
            InitializeContract(contract);

            return contract;
        }

#if HAVE_BINARY_SERIALIZATION
        protected virtual JsonISerializableContract CreateISerializableContract(Type objectType)
        {
            JsonISerializableContract contract = new JsonISerializableContract(objectType);
            InitializeContract(contract);

            if (contract.IsInstantiable)
            {
                ConstructorInfo constructorInfo = contract.NonNullableUnderlyingType.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, new[] {typeof(SerializationInfo), typeof(StreamingContext)}, null);
                if (constructorInfo != null)
                {
                    ObjectConstructor<object> creator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(constructorInfo);

                    contract.ISerializableCreator = creator;
                }
            }

            return contract;
        }
#endif

#if HAVE_DYNAMIC
        protected virtual JsonDynamicContract CreateDynamicContract(Type objectType)
        {
            JsonDynamicContract contract = new JsonDynamicContract(objectType);
            InitializeContract(contract);

            JsonContainerAttribute? containerAttribute = JsonTypeReflector.GetAttribute<JsonContainerAttribute>(objectType);
            if (containerAttribute?.NamingStrategyType != null)
            {
                NamingStrategy namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(containerAttribute)!;
                contract.PropertyNameResolver = s => namingStrategy.GetDictionaryKey(s);
            }
            else
            {
                contract.PropertyNameResolver = ResolveDictionaryKey;
            }

            contract.Properties.AddRange(CreateProperties(objectType, MemberSerialization.OptOut));

            return contract;
        }
#endif
        protected virtual JsonStringContract CreateStringContract(Type objectType)
        {
            JsonStringContract contract = new JsonStringContract(objectType);
            InitializeContract(contract);

            return contract;
        }
        protected virtual JsonContract CreateContract(Type objectType)
        {
            Type t = ReflectionUtils.EnsureNotByRefType(objectType);

            if (IsJsonPrimitiveType(t))
            {
                return CreatePrimitiveContract(objectType);
            }

            t = ReflectionUtils.EnsureNotNullableType(t);
            JsonContainerAttribute? containerAttribute = JsonTypeReflector.GetCachedAttribute<JsonContainerAttribute>(t);

            if (containerAttribute is JsonObjectAttribute)
            {
                return CreateObjectContract(objectType);
            }

            if (containerAttribute is JsonArrayAttribute)
            {
                return CreateArrayContract(objectType);
            }

            if (containerAttribute is JsonDictionaryAttribute)
            {
                return CreateDictionaryContract(objectType);
            }

            if (t == typeof(JToken) || t.IsSubclassOf(typeof(JToken)))
            {
                return CreateLinqContract(objectType);
            }

            if (CollectionUtils.IsDictionaryType(t))
            {
                return CreateDictionaryContract(objectType);
            }

            if (typeof(IEnumerable).IsAssignableFrom(t))
            {
                return CreateArrayContract(objectType);
            }

            if (CanConvertToString(t))
            {
                return CreateStringContract(objectType);
            }

#if HAVE_BINARY_SERIALIZATION
            if (!IgnoreSerializableInterface && typeof(ISerializable).IsAssignableFrom(t) && JsonTypeReflector.IsSerializable(t))
            {
                return CreateISerializableContract(objectType);
            }
#endif

#if HAVE_DYNAMIC
            if (typeof(IDynamicMetaObjectProvider).IsAssignableFrom(t))
            {
                return CreateDynamicContract(objectType);
            }
#endif

#if HAVE_ICONVERTIBLE
            if (IsIConvertible(t))
            {
                return CreatePrimitiveContract(t);
            }
#endif

            return CreateObjectContract(objectType);
        }

        internal static bool IsJsonPrimitiveType(Type t)
        {
            PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(t);

            return (typeCode != PrimitiveTypeCode.Empty && typeCode != PrimitiveTypeCode.Object);
        }

#if HAVE_ICONVERTIBLE
        internal static bool IsIConvertible(Type t)
        {
            if (typeof(IConvertible).IsAssignableFrom(t)
                || (ReflectionUtils.IsNullableType(t) && typeof(IConvertible).IsAssignableFrom(Nullable.GetUnderlyingType(t))))
            {
                return !typeof(JToken).IsAssignableFrom(t);
            }

            return false;
        }
#endif

        internal static bool CanConvertToString(Type type)
        {
#if HAVE_TYPE_DESCRIPTOR
            if (JsonTypeReflector.CanTypeDescriptorConvertString(type, out _))
            {
                return true;
            }
#endif

            if (type == typeof(Type) || type.IsSubclassOf(typeof(Type)))
            {
                return true;
            }

            return false;
        }

        private static bool IsValidCallback(MethodInfo method, ParameterInfo[] parameters, Type attributeType, MethodInfo? currentCallback, ref Type? prevAttributeType)
        {
            if (!method.IsDefined(attributeType, false))
            {
                return false;
            }

            if (currentCallback != null)
            {
                throw new JsonException("Invalid attribute. Both '{0}' and '{1}' in type '{2}' have '{3}'.".FormatWith(CultureInfo.InvariantCulture, method, currentCallback, GetClrTypeFullName(method.DeclaringType), attributeType));
            }

            if (prevAttributeType != null)
            {
                throw new JsonException("Invalid Callback. Method '{3}' in type '{2}' has both '{0}' and '{1}'.".FormatWith(CultureInfo.InvariantCulture, prevAttributeType, attributeType, GetClrTypeFullName(method.DeclaringType), method));
            }

            if (method.IsVirtual)
            {
                throw new JsonException("Virtual Method '{0}' of type '{1}' cannot be marked with '{2}' attribute.".FormatWith(CultureInfo.InvariantCulture, method, GetClrTypeFullName(method.DeclaringType), attributeType));
            }

            if (method.ReturnType != typeof(void))
            {
                throw new JsonException("Serialization Callback '{1}' in type '{0}' must return void.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method));
            }

            if (attributeType == typeof(OnErrorAttribute))
            {
                if (parameters == null || parameters.Length != 2 || parameters[0].ParameterType != typeof(StreamingContext) || parameters[1].ParameterType != typeof(ErrorContext))
                {
                    throw new JsonException("Serialization Error Callback '{1}' in type '{0}' must have two parameters of type '{2}' and '{3}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext), typeof(ErrorContext)));
                }
            }
            else
            {
                if (parameters == null || parameters.Length != 1 || parameters[0].ParameterType != typeof(StreamingContext))
                {
                    throw new JsonException("Serialization Callback '{1}' in type '{0}' must have a single parameter of type '{2}'.".FormatWith(CultureInfo.InvariantCulture, GetClrTypeFullName(method.DeclaringType), method, typeof(StreamingContext)));
                }
            }

            prevAttributeType = attributeType;

            return true;
        }

        internal static string GetClrTypeFullName(Type type)
        {
            if (type.IsGenericTypeDefinition() || !type.ContainsGenericParameters())
            {
                return type.FullName;
            }

            return "{0}.{1}".FormatWith(CultureInfo.InvariantCulture, type.Namespace, type.Name);
        }
        protected virtual IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            List<MemberInfo> members = GetSerializableMembers(type);
            if (members == null)
            {
                throw new JsonSerializationException("Null collection of serializable members returned.");
            }

            DefaultJsonNameTable nameTable = GetNameTable();

            JsonPropertyCollection properties = new JsonPropertyCollection(type);

            foreach (MemberInfo member in members)
            {
                JsonProperty property = CreateProperty(member, memberSerialization);

                if (property != null)
                {
                    lock (nameTable)
                    {
                        property.PropertyName = nameTable.Add(property.PropertyName!);
                    }

                    properties.AddProperty(property);
                }
            }

            IList<JsonProperty> orderedProperties = properties.OrderBy(p => p.Order ?? -1).ToList();
            return orderedProperties;
        }

        internal virtual DefaultJsonNameTable GetNameTable()
        {
            return _nameTable;
        }
        protected virtual IValueProvider CreateMemberValueProvider(MemberInfo member)
        {
            IValueProvider valueProvider;

#if !(PORTABLE40 || PORTABLE || DOTNET || NETSTANDARD2_0)
            if (DynamicCodeGeneration)
            {
                valueProvider = new DynamicValueProvider(member);
            }
            else
            {
                valueProvider = new ReflectionValueProvider(member);
            }
#elif !(PORTABLE40)
            valueProvider = new ExpressionValueProvider(member);
#else
            valueProvider = new ReflectionValueProvider(member);
#endif

            return valueProvider;
        }
        protected virtual JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            JsonProperty property = new JsonProperty();
            property.PropertyType = ReflectionUtils.GetMemberUnderlyingType(member);
            property.DeclaringType = member.DeclaringType;
            property.ValueProvider = CreateMemberValueProvider(member);
            property.AttributeProvider = new ReflectionAttributeProvider(member);

            SetPropertySettingsFromAttributes(property, member, member.Name, member.DeclaringType, memberSerialization, out bool allowNonPublicAccess);

            if (memberSerialization != MemberSerialization.Fields)
            {
                property.Readable = ReflectionUtils.CanReadMemberValue(member, allowNonPublicAccess);
                property.Writable = ReflectionUtils.CanSetMemberValue(member, allowNonPublicAccess, property.HasMemberAttribute);
            }
            else
            {
                property.Readable = true;
                property.Writable = true;
            }

            if (!IgnoreShouldSerializeMembers)
            {
                property.ShouldSerialize = CreateShouldSerializeTest(member);
            }

            if (!IgnoreIsSpecifiedMembers)
            {
                SetIsSpecifiedActions(property, member, allowNonPublicAccess);
            }

            return property;
        }

        private void SetPropertySettingsFromAttributes(JsonProperty property, object attributeProvider, string name, Type declaringType, MemberSerialization memberSerialization, out bool allowNonPublicAccess)
        {
#if HAVE_DATA_CONTRACTS
            DataContractAttribute? dataContractAttribute = JsonTypeReflector.GetDataContractAttribute(declaringType);

            MemberInfo? memberInfo = attributeProvider as MemberInfo;

            DataMemberAttribute? dataMemberAttribute;
            if (dataContractAttribute != null && memberInfo != null)
            {
                dataMemberAttribute = JsonTypeReflector.GetDataMemberAttribute((MemberInfo)memberInfo);
            }
            else
            {
                dataMemberAttribute = null;
            }
#endif

            JsonPropertyAttribute? propertyAttribute = JsonTypeReflector.GetAttribute<JsonPropertyAttribute>(attributeProvider);
            JsonRequiredAttribute? requiredAttribute = JsonTypeReflector.GetAttribute<JsonRequiredAttribute>(attributeProvider);

            string mappedName;
            bool hasSpecifiedName;
            if (propertyAttribute?.PropertyName != null)
            {
                mappedName = propertyAttribute.PropertyName;
                hasSpecifiedName = true;
            }
#if HAVE_DATA_CONTRACTS
            else if (dataMemberAttribute?.Name != null)
            {
                mappedName = dataMemberAttribute.Name;
                hasSpecifiedName = true;
            }
#endif
            else
            {
                mappedName = name;
                hasSpecifiedName = false;
            }

            JsonContainerAttribute? containerAttribute = JsonTypeReflector.GetAttribute<JsonContainerAttribute>(declaringType);

            NamingStrategy? namingStrategy;
            if (propertyAttribute?.NamingStrategyType != null)
            {
                namingStrategy = JsonTypeReflector.CreateNamingStrategyInstance(propertyAttribute.NamingStrategyType, propertyAttribute.NamingStrategyParameters);
            }
            else if (containerAttribute?.NamingStrategyType != null)
            {
                namingStrategy = JsonTypeReflector.GetContainerNamingStrategy(containerAttribute);
            }
            else
            {
                namingStrategy = NamingStrategy;
            }

            if (namingStrategy != null)
            {
                property.PropertyName = namingStrategy.GetPropertyName(mappedName, hasSpecifiedName);
            }
            else
            {
                property.PropertyName = ResolvePropertyName(mappedName);
            }
            
            property.UnderlyingName = name;

            bool hasMemberAttribute = false;
            if (propertyAttribute != null)
            {
                property._required = propertyAttribute._required;
                property.Order = propertyAttribute._order;
                property.DefaultValueHandling = propertyAttribute._defaultValueHandling;
                hasMemberAttribute = true;
                property.NullValueHandling = propertyAttribute._nullValueHandling;
                property.ReferenceLoopHandling = propertyAttribute._referenceLoopHandling;
                property.ObjectCreationHandling = propertyAttribute._objectCreationHandling;
                property.TypeNameHandling = propertyAttribute._typeNameHandling;
                property.IsReference = propertyAttribute._isReference;

                property.ItemIsReference = propertyAttribute._itemIsReference;
                property.ItemConverter = propertyAttribute.ItemConverterType != null ? JsonTypeReflector.CreateJsonConverterInstance(propertyAttribute.ItemConverterType, propertyAttribute.ItemConverterParameters) : null;
                property.ItemReferenceLoopHandling = propertyAttribute._itemReferenceLoopHandling;
                property.ItemTypeNameHandling = propertyAttribute._itemTypeNameHandling;
            }
            else
            {
                property.NullValueHandling = null;
                property.ReferenceLoopHandling = null;
                property.ObjectCreationHandling = null;
                property.TypeNameHandling = null;
                property.IsReference = null;
                property.ItemIsReference = null;
                property.ItemConverter = null;
                property.ItemReferenceLoopHandling = null;
                property.ItemTypeNameHandling = null;
#if HAVE_DATA_CONTRACTS
                if (dataMemberAttribute != null)
                {
                    property._required = (dataMemberAttribute.IsRequired) ? Required.AllowNull : Required.Default;
                    property.Order = (dataMemberAttribute.Order != -1) ? (int?)dataMemberAttribute.Order : null;
                    property.DefaultValueHandling = (!dataMemberAttribute.EmitDefaultValue) ? (DefaultValueHandling?)DefaultValueHandling.Ignore : null;
                    hasMemberAttribute = true;
                }
#endif
            }

            if (requiredAttribute != null)
            {
                property._required = Required.Always;
                hasMemberAttribute = true;
            }

            property.HasMemberAttribute = hasMemberAttribute;

            bool hasJsonIgnoreAttribute =
                JsonTypeReflector.GetAttribute<JsonIgnoreAttribute>(attributeProvider) != null
                || JsonTypeReflector.GetAttribute<JsonExtensionDataAttribute>(attributeProvider) != null
#if HAVE_NON_SERIALIZED_ATTRIBUTE
                || JsonTypeReflector.IsNonSerializable(attributeProvider)
#endif
                ;

            if (memberSerialization != MemberSerialization.OptIn)
            {
                bool hasIgnoreDataMemberAttribute = false;

#if HAVE_IGNORE_DATA_MEMBER_ATTRIBUTE
                hasIgnoreDataMemberAttribute = (JsonTypeReflector.GetAttribute<IgnoreDataMemberAttribute>(attributeProvider) != null);
#endif
                property.Ignored = (hasJsonIgnoreAttribute || hasIgnoreDataMemberAttribute);
            }
            else
            {
                property.Ignored = (hasJsonIgnoreAttribute || !hasMemberAttribute);
            }
            property.Converter = JsonTypeReflector.GetJsonConverter(attributeProvider);

            DefaultValueAttribute? defaultValueAttribute = JsonTypeReflector.GetAttribute<DefaultValueAttribute>(attributeProvider);
            if (defaultValueAttribute != null)
            {
                property.DefaultValue = defaultValueAttribute.Value;
            }

            allowNonPublicAccess = false;
#pragma warning disable 618
            if ((DefaultMembersSearchFlags & BindingFlags.NonPublic) == BindingFlags.NonPublic)
            {
                allowNonPublicAccess = true;
            }
#pragma warning restore 618
            if (hasMemberAttribute)
            {
                allowNonPublicAccess = true;
            }
            if (memberSerialization == MemberSerialization.Fields)
            {
                allowNonPublicAccess = true;
            }
        }

        private Predicate<object>? CreateShouldSerializeTest(MemberInfo member)
        {
            MethodInfo shouldSerializeMethod = member.DeclaringType.GetMethod(JsonTypeReflector.ShouldSerializePrefix + member.Name, ReflectionUtils.EmptyTypes);

            if (shouldSerializeMethod == null || shouldSerializeMethod.ReturnType != typeof(bool))
            {
                return null;
            }

            MethodCall<object, object?> shouldSerializeCall =
                JsonTypeReflector.ReflectionDelegateFactory.CreateMethodCall<object>(shouldSerializeMethod);

            return o => (bool)shouldSerializeCall(o)!;
        }

        private void SetIsSpecifiedActions(JsonProperty property, MemberInfo member, bool allowNonPublicAccess)
        {
            MemberInfo? specifiedMember = member.DeclaringType.GetProperty(member.Name + JsonTypeReflector.SpecifiedPostfix, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (specifiedMember == null)
            {
                specifiedMember = member.DeclaringType.GetField(member.Name + JsonTypeReflector.SpecifiedPostfix, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (specifiedMember == null || ReflectionUtils.GetMemberUnderlyingType(specifiedMember) != typeof(bool))
            {
                return;
            }

            Func<object, object> specifiedPropertyGet = JsonTypeReflector.ReflectionDelegateFactory.CreateGet<object>(specifiedMember)!;

            property.GetIsSpecified = o => (bool)specifiedPropertyGet(o);

            if (ReflectionUtils.CanSetMemberValue(specifiedMember, allowNonPublicAccess, false))
            {
                property.SetIsSpecified = JsonTypeReflector.ReflectionDelegateFactory.CreateSet<object>(specifiedMember);
            }
        }
        protected virtual string ResolvePropertyName(string propertyName)
        {
            if (NamingStrategy != null)
            {
                return NamingStrategy.GetPropertyName(propertyName, false);
            }

            return propertyName;
        }
        protected virtual string ResolveExtensionDataName(string extensionDataName)
        {
            if (NamingStrategy != null)
            {
                return NamingStrategy.GetExtensionDataName(extensionDataName);
            }

            return extensionDataName;
        }
        protected virtual string ResolveDictionaryKey(string dictionaryKey)
        {
            if (NamingStrategy != null)
            {
                return NamingStrategy.GetDictionaryKey(dictionaryKey);
            }

            return ResolvePropertyName(dictionaryKey);
        }
        public string GetResolvedPropertyName(string propertyName)
        {
            return ResolvePropertyName(propertyName);
        }
    }
}