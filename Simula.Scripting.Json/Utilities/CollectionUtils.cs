
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Collections;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif
#if HAVE_METHOD_IMPL_ATTRIBUTE
#endif

namespace Simula.Scripting.Json.Utilities
{
    internal static class CollectionUtils
    {
        public static bool IsNullOrEmpty<T>(ICollection<T> collection)
        {
            if (collection != null) {
                return (collection.Count == 0);
            }
            return true;
        }
        public static void AddRange<T>(this IList<T> initial, IEnumerable<T> collection)
        {
            if (initial == null) {
                throw new ArgumentNullException(nameof(initial));
            }

            if (collection == null) {
                return;
            }

            foreach (T value in collection) {
                initial.Add(value);
            }
        }

#if !HAVE_COVARIANT_GENERICS
        public static void AddRange<T>(this IList<T> initial, IEnumerable collection)
        {
            ValidationUtils.ArgumentNotNull(initial, nameof(initial));
            initial.AddRange(collection.Cast<T>());
        }
#endif

        public static bool IsDictionaryType(Type type)
        {
            ValidationUtils.ArgumentNotNull(type, nameof(type));

            if (typeof(IDictionary).IsAssignableFrom(type)) {
                return true;
            }
            if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(IDictionary<,>))) {
                return true;
            }
#if HAVE_READ_ONLY_COLLECTIONS
            if (ReflectionUtils.ImplementsGenericDefinition(type, typeof(IReadOnlyDictionary<,>))) {
                return true;
            }
#endif

            return false;
        }

        public static ConstructorInfo? ResolveEnumerableCollectionConstructor(Type collectionType, Type collectionItemType)
        {
            Type genericConstructorArgument = typeof(IList<>).MakeGenericType(collectionItemType);

            return ResolveEnumerableCollectionConstructor(collectionType, collectionItemType, genericConstructorArgument);
        }

        public static ConstructorInfo? ResolveEnumerableCollectionConstructor(Type collectionType, Type collectionItemType, Type constructorArgumentType)
        {
            Type genericEnumerable = typeof(IEnumerable<>).MakeGenericType(collectionItemType);
            ConstructorInfo? match = null;

            foreach (ConstructorInfo constructor in collectionType.GetConstructors(BindingFlags.Public | BindingFlags.Instance)) {
                IList<ParameterInfo> parameters = constructor.GetParameters();

                if (parameters.Count == 1) {
                    Type parameterType = parameters[0].ParameterType;

                    if (genericEnumerable == parameterType) {
                        match = constructor;
                        break;
                    }
                    if (match == null) {
                        if (parameterType.IsAssignableFrom(constructorArgumentType)) {
                            match = constructor;
                        }
                    }
                }
            }

            return match;
        }

        public static bool AddDistinct<T>(this IList<T> list, T value)
        {
            return list.AddDistinct(value, EqualityComparer<T>.Default);
        }

        public static bool AddDistinct<T>(this IList<T> list, T value, IEqualityComparer<T> comparer)
        {
            if (list.ContainsValue(value, comparer)) {
                return false;
            }

            list.Add(value);
            return true;
        }
        public static bool ContainsValue<TSource>(this IEnumerable<TSource> source, TSource value, IEqualityComparer<TSource> comparer)
        {
            if (comparer == null) {
                comparer = EqualityComparer<TSource>.Default;
            }

            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }

            foreach (TSource local in source) {
                if (comparer.Equals(local, value)) {
                    return true;
                }
            }

            return false;
        }

        public static bool AddRangeDistinct<T>(this IList<T> list, IEnumerable<T> values, IEqualityComparer<T> comparer)
        {
            bool allAdded = true;
            foreach (T value in values) {
                if (!list.AddDistinct(value, comparer)) {
                    allAdded = false;
                }
            }

            return allAdded;
        }

        public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> predicate)
        {
            int index = 0;
            foreach (T value in collection) {
                if (predicate(value)) {
                    return index;
                }

                index++;
            }

            return -1;
        }

        public static bool Contains<T>(this List<T> list, T value, IEqualityComparer comparer)
        {
            for (int i = 0; i < list.Count; i++) {
                if (comparer.Equals(value, list[i])) {
                    return true;
                }
            }
            return false;
        }

        public static int IndexOfReference<T>(this List<T> list, T item)
        {
            for (int i = 0; i < list.Count; i++) {
                if (ReferenceEquals(item, list[i])) {
                    return i;
                }
            }

            return -1;
        }

#if HAVE_FAST_REVERSE
        public static void FastReverse<T>(this List<T> list)
        {
            int i = 0;
            int j = list.Count - 1;
            while (i < j)
            {
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
                i++;
                j--;
            }
        }
#endif

        private static IList<int> GetDimensions(IList values, int dimensionsCount)
        {
            IList<int> dimensions = new List<int>();

            IList currentArray = values;
            while (true) {
                dimensions.Add(currentArray.Count);
                if (dimensions.Count == dimensionsCount) {
                    break;
                }

                if (currentArray.Count == 0) {
                    break;
                }

                object v = currentArray[0];
                if (v is IList list) {
                    currentArray = list;
                } else {
                    break;
                }
            }

            return dimensions;
        }

        private static void CopyFromJaggedToMultidimensionalArray(IList values, Array multidimensionalArray, int[] indices)
        {
            int dimension = indices.Length;
            if (dimension == multidimensionalArray.Rank) {
                multidimensionalArray.SetValue(JaggedArrayGetValue(values, indices), indices);
                return;
            }

            int dimensionLength = multidimensionalArray.GetLength(dimension);
            IList list = (IList)JaggedArrayGetValue(values, indices);
            int currentValuesLength = list.Count;
            if (currentValuesLength != dimensionLength) {
                throw new Exception("Cannot deserialize non-cubical array as multidimensional array.");
            }

            int[] newIndices = new int[dimension + 1];
            for (int i = 0; i < dimension; i++) {
                newIndices[i] = indices[i];
            }

            for (int i = 0; i < multidimensionalArray.GetLength(dimension); i++) {
                newIndices[dimension] = i;
                CopyFromJaggedToMultidimensionalArray(values, multidimensionalArray, newIndices);
            }
        }

        private static object JaggedArrayGetValue(IList values, int[] indices)
        {
            IList currentList = values;
            for (int i = 0; i < indices.Length; i++) {
                int index = indices[i];
                if (i == indices.Length - 1) {
                    return currentList[index];
                } else {
                    currentList = (IList)currentList[index];
                }
            }
            return currentList;
        }

        public static Array ToMultidimensionalArray(IList values, Type type, int rank)
        {
            IList<int> dimensions = GetDimensions(values, rank);

            while (dimensions.Count < rank) {
                dimensions.Add(0);
            }

            Array multidimensionalArray = Array.CreateInstance(type, dimensions.ToArray());
            CopyFromJaggedToMultidimensionalArray(values, multidimensionalArray, ArrayEmpty<int>());

            return multidimensionalArray;
        }

        public static T[] ArrayEmpty<T>()
        {
            return EmptyArrayContainer<T>.Empty;
        }

        private static class EmptyArrayContainer<T>
        {
#pragma warning disable CA1825 // Avoid zero-length array allocations.
            public static readonly T[] Empty = new T[0];
#pragma warning restore CA1825 // Avoid zero-length array allocations.
        }
    }
}