
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;

#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#endif

namespace Simula.Scripting.Json.Serialization
{
    public class JsonDictionaryContract : JsonContainerContract
    {
        public Func<string, string>? DictionaryKeyResolver { get; set; }
        public Type? DictionaryKeyType { get; }
        public Type? DictionaryValueType { get; }

        internal JsonContract? KeyContract { get; set; }

        private readonly Type? _genericCollectionDefinitionType;

        private Type? _genericWrapperType;
        private ObjectConstructor<object>? _genericWrapperCreator;

        private Func<object>? _genericTemporaryDictionaryCreator;

        internal bool ShouldCreateWrapper { get; }

        private readonly ConstructorInfo? _parameterizedConstructor;

        private ObjectConstructor<object>? _overrideCreator;
        private ObjectConstructor<object>? _parameterizedCreator;

        internal ObjectConstructor<object>? ParameterizedCreator {
            get {
                if (_parameterizedCreator == null && _parameterizedConstructor != null) {
                    _parameterizedCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(_parameterizedConstructor);
                }

                return _parameterizedCreator;
            }
        }
        public ObjectConstructor<object>? OverrideCreator {
            get => _overrideCreator;
            set => _overrideCreator = value;
        }
        public bool HasParameterizedCreator { get; set; }

        internal bool HasParameterizedCreatorInternal => (HasParameterizedCreator || _parameterizedCreator != null || _parameterizedConstructor != null);
        public JsonDictionaryContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Dictionary;

            Type? keyType;
            Type? valueType;

            if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IDictionary<,>), out _genericCollectionDefinitionType)) {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IDictionary<,>))) {
                    CreatedType = typeof(Dictionary<,>).MakeGenericType(keyType, valueType);
                } else if (underlyingType.IsGenericType()) {
                    Type typeDefinition = underlyingType.GetGenericTypeDefinition();
                    if (typeDefinition.FullName == JsonTypeReflector.ConcurrentDictionaryTypeName) {
                        ShouldCreateWrapper = true;
                    }
                }

#if HAVE_READ_ONLY_COLLECTIONS
                IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(underlyingType, typeof(ReadOnlyDictionary<,>));
#endif

            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (ReflectionUtils.ImplementsGenericDefinition(underlyingType, typeof(IReadOnlyDictionary<,>), out _genericCollectionDefinitionType)) {
                keyType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                valueType = _genericCollectionDefinitionType.GetGenericArguments()[1];

                if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IReadOnlyDictionary<,>))) {
                    CreatedType = typeof(ReadOnlyDictionary<,>).MakeGenericType(keyType, valueType);
                }

                IsReadOnlyOrFixedSize = true;
            }
#endif
            else {
                ReflectionUtils.GetDictionaryKeyValueTypes(UnderlyingType, out keyType, out valueType);

                if (UnderlyingType == typeof(IDictionary)) {
                    CreatedType = typeof(Dictionary<object, object>);
                }
            }

            if (keyType != null && valueType != null) {
                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(
                    CreatedType,
                    typeof(KeyValuePair<,>).MakeGenericType(keyType, valueType),
                    typeof(IDictionary<,>).MakeGenericType(keyType, valueType));

#if HAVE_FSHARP_TYPES
                if (!HasParameterizedCreatorInternal && underlyingType.Name == FSharpUtils.FSharpMapTypeName) {
                    FSharpUtils.EnsureInitialized(underlyingType.Assembly());
                    _parameterizedCreator = FSharpUtils.Instance.CreateMap(keyType, valueType);
                }
#endif
            }

            if (!typeof(IDictionary).IsAssignableFrom(CreatedType)) {
                ShouldCreateWrapper = true;
            }

            DictionaryKeyType = keyType;
            DictionaryValueType = valueType;

#if (NET20 || NET35)
            if (DictionaryValueType != null && ReflectionUtils.IsNullableType(DictionaryValueType))
            {
                if (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(Dictionary<,>), out _))
                {
                    ShouldCreateWrapper = true;
                }
            }
#endif

            if (DictionaryKeyType != null &&
                DictionaryValueType != null &&
                ImmutableCollectionsUtils.TryBuildImmutableForDictionaryContract(
                    underlyingType,
                    DictionaryKeyType,
                    DictionaryValueType,
                    out Type? immutableCreatedType,
                    out ObjectConstructor<object>? immutableParameterizedCreator)) {
                CreatedType = immutableCreatedType;
                _parameterizedCreator = immutableParameterizedCreator;
                IsReadOnlyOrFixedSize = true;
            }
        }

        internal IWrappedDictionary CreateWrapper(object dictionary)
        {
            if (_genericWrapperCreator == null) {
                _genericWrapperType = typeof(DictionaryWrapper<,>).MakeGenericType(DictionaryKeyType, DictionaryValueType);

                ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { _genericCollectionDefinitionType! });
                _genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
            }

            return (IWrappedDictionary)_genericWrapperCreator(dictionary);
        }

        internal IDictionary CreateTemporaryDictionary()
        {
            if (_genericTemporaryDictionaryCreator == null) {
                Type temporaryDictionaryType = typeof(Dictionary<,>).MakeGenericType(DictionaryKeyType ?? typeof(object), DictionaryValueType ?? typeof(object));

                _genericTemporaryDictionaryCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryDictionaryType);
            }

            return (IDictionary)_genericTemporaryDictionaryCreator();
        }
    }
}