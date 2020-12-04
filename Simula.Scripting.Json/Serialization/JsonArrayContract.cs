
using Simula.Scripting.Json.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else

#endif

namespace Simula.Scripting.Json.Serialization
{
    public class JsonArrayContract : JsonContainerContract
    {
        public Type? CollectionItemType { get; }
        public bool IsMultidimensionalArray { get; }

        private readonly Type? _genericCollectionDefinitionType;

        private Type? _genericWrapperType;
        private ObjectConstructor<object>? _genericWrapperCreator;
        private Func<object>? _genericTemporaryCollectionCreator;

        internal bool IsArray { get; }
        internal bool ShouldCreateWrapper { get; }
        internal bool CanDeserialize { get; private set; }

        private readonly ConstructorInfo? _parameterizedConstructor;

        private ObjectConstructor<object>? _parameterizedCreator;
        private ObjectConstructor<object>? _overrideCreator;

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
            set {
                _overrideCreator = value;
                CanDeserialize = true;
            }
        }
        public bool HasParameterizedCreator { get; set; }

        internal bool HasParameterizedCreatorInternal => (HasParameterizedCreator || _parameterizedCreator != null || _parameterizedConstructor != null);
        public JsonArrayContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Array;
            IsArray = CreatedType.IsArray ||
                (NonNullableUnderlyingType.IsGenericType() && NonNullableUnderlyingType.GetGenericTypeDefinition().FullName == "System.Linq.EmptyPartition`1");

            bool canDeserialize;

            Type? tempCollectionType;
            if (IsArray) {
                CollectionItemType = ReflectionUtils.GetCollectionItemType(UnderlyingType);
                IsReadOnlyOrFixedSize = true;
                _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);

                canDeserialize = true;
                IsMultidimensionalArray = (CreatedType.IsArray && UnderlyingType.GetArrayRank() > 1);
            } else if (typeof(IList).IsAssignableFrom(NonNullableUnderlyingType)) {
                if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType)) {
                    CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];
                } else {
                    CollectionItemType = ReflectionUtils.GetCollectionItemType(NonNullableUnderlyingType);
                }

                if (NonNullableUnderlyingType == typeof(IList)) {
                    CreatedType = typeof(List<object>);
                }

                if (CollectionItemType != null) {
                    _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
                }

                IsReadOnlyOrFixedSize = ReflectionUtils.InheritsGenericDefinition(NonNullableUnderlyingType, typeof(ReadOnlyCollection<>));
                canDeserialize = true;
            } else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>), out _genericCollectionDefinitionType)) {
                CollectionItemType = _genericCollectionDefinitionType.GetGenericArguments()[0];

                if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(ICollection<>))
                    || ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IList<>))) {
                    CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
                }

#if HAVE_ISET
                if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(ISet<>))) {
                    CreatedType = typeof(HashSet<>).MakeGenericType(CollectionItemType);
                }
#endif

                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);
                canDeserialize = true;
                ShouldCreateWrapper = true;
            }
#if HAVE_READ_ONLY_COLLECTIONS
            else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyCollection<>), out tempCollectionType)) {
                CollectionItemType = tempCollectionType.GetGenericArguments()[0];

                if (ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyCollection<>))
                    || ReflectionUtils.IsGenericDefinition(NonNullableUnderlyingType, typeof(IReadOnlyList<>))) {
                    CreatedType = typeof(ReadOnlyCollection<>).MakeGenericType(CollectionItemType);
                }

                _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);
                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(CreatedType, CollectionItemType);

#if HAVE_FSHARP_TYPES
                StoreFSharpListCreatorIfNecessary(NonNullableUnderlyingType);
#endif

                IsReadOnlyOrFixedSize = true;
                canDeserialize = HasParameterizedCreatorInternal;
            }
#endif
            else if (ReflectionUtils.ImplementsGenericDefinition(NonNullableUnderlyingType, typeof(IEnumerable<>), out tempCollectionType)) {
                CollectionItemType = tempCollectionType.GetGenericArguments()[0];

                if (ReflectionUtils.IsGenericDefinition(UnderlyingType, typeof(IEnumerable<>))) {
                    CreatedType = typeof(List<>).MakeGenericType(CollectionItemType);
                }

                _parameterizedConstructor = CollectionUtils.ResolveEnumerableCollectionConstructor(NonNullableUnderlyingType, CollectionItemType);

#if HAVE_FSHARP_TYPES
                StoreFSharpListCreatorIfNecessary(NonNullableUnderlyingType);
#endif

                if (NonNullableUnderlyingType.IsGenericType() && NonNullableUnderlyingType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    _genericCollectionDefinitionType = tempCollectionType;

                    IsReadOnlyOrFixedSize = false;
                    ShouldCreateWrapper = false;
                    canDeserialize = true;
                } else {
                    _genericCollectionDefinitionType = typeof(List<>).MakeGenericType(CollectionItemType);

                    IsReadOnlyOrFixedSize = true;
                    ShouldCreateWrapper = true;
                    canDeserialize = HasParameterizedCreatorInternal;
                }
            } else {
                canDeserialize = false;
                ShouldCreateWrapper = true;
            }

            CanDeserialize = canDeserialize;

#if (NET20 || NET35)
            if (CollectionItemType != null && ReflectionUtils.IsNullableType(CollectionItemType))
            {
                if (ReflectionUtils.InheritsGenericDefinition(CreatedType, typeof(List<>), out tempCollectionType)
                    || (IsArray && !IsMultidimensionalArray))
                {
                    ShouldCreateWrapper = true;
                }
            }
#endif

            if (CollectionItemType != null &&
                ImmutableCollectionsUtils.TryBuildImmutableForArrayContract(
                NonNullableUnderlyingType,
                CollectionItemType,
                out Type? immutableCreatedType,
                out ObjectConstructor<object>? immutableParameterizedCreator)) {
                CreatedType = immutableCreatedType;
                _parameterizedCreator = immutableParameterizedCreator;
                IsReadOnlyOrFixedSize = true;
                CanDeserialize = true;
            }
        }

        internal IWrappedCollection CreateWrapper(object list)
        {
            if (_genericWrapperCreator == null) {
                MiscellaneousUtils.Assert(_genericCollectionDefinitionType != null);

                _genericWrapperType = typeof(CollectionWrapper<>).MakeGenericType(CollectionItemType);

                Type constructorArgument;

                if (ReflectionUtils.InheritsGenericDefinition(_genericCollectionDefinitionType, typeof(List<>))
                    || _genericCollectionDefinitionType.GetGenericTypeDefinition() == typeof(IEnumerable<>)) {
                    constructorArgument = typeof(ICollection<>).MakeGenericType(CollectionItemType);
                } else {
                    constructorArgument = _genericCollectionDefinitionType;
                }

                ConstructorInfo genericWrapperConstructor = _genericWrapperType.GetConstructor(new[] { constructorArgument });
                _genericWrapperCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateParameterizedConstructor(genericWrapperConstructor);
            }

            return (IWrappedCollection)_genericWrapperCreator(list);
        }

        internal IList CreateTemporaryCollection()
        {
            if (_genericTemporaryCollectionCreator == null) {
                Type collectionItemType = (IsMultidimensionalArray || CollectionItemType == null)
                    ? typeof(object)
                    : CollectionItemType;

                Type temporaryListType = typeof(List<>).MakeGenericType(collectionItemType);
                _genericTemporaryCollectionCreator = JsonTypeReflector.ReflectionDelegateFactory.CreateDefaultConstructor<object>(temporaryListType);
            }

            return (IList)_genericTemporaryCollectionCreator();
        }

#if HAVE_FSHARP_TYPES
        private void StoreFSharpListCreatorIfNecessary(Type underlyingType)
        {
            if (!HasParameterizedCreatorInternal && underlyingType.Name == FSharpUtils.FSharpListTypeName) {
                FSharpUtils.EnsureInitialized(underlyingType.Assembly());
                _parameterizedCreator = FSharpUtils.Instance.CreateSeq(CollectionItemType!);
            }
        }
#endif
    }
}