
using System;
using System.Collections.Generic;
using Simula.Scripting.Json.Utilities;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;

#endif

namespace Simula.Scripting.Json.Linq
{
    public static class Extensions
    {
        public static IJEnumerable<JToken> Ancestors<T>(this IEnumerable<T> source) where T : JToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.Ancestors()).AsJEnumerable();
        }
        public static IJEnumerable<JToken> AncestorsAndSelf<T>(this IEnumerable<T> source) where T : JToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.AncestorsAndSelf()).AsJEnumerable();
        }
        public static IJEnumerable<JToken> Descendants<T>(this IEnumerable<T> source) where T : JContainer
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.Descendants()).AsJEnumerable();
        }
        public static IJEnumerable<JToken> DescendantsAndSelf<T>(this IEnumerable<T> source) where T : JContainer
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(j => j.DescendantsAndSelf()).AsJEnumerable();
        }
        public static IJEnumerable<JProperty> Properties(this IEnumerable<JObject> source)
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(d => d.Properties()).AsJEnumerable();
        }
        public static IJEnumerable<JToken> Values(this IEnumerable<JToken> source, object? key)
        {
            return Values<JToken, JToken>(source, key).AsJEnumerable();
        }
        public static IJEnumerable<JToken> Values(this IEnumerable<JToken> source)
        {
            return source.Values(null);
        }
        public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source, object key)
        {
            return Values<JToken, U>(source, key);
        }
        public static IEnumerable<U> Values<U>(this IEnumerable<JToken> source)
        {
            return Values<JToken, U>(source, null);
        }
        public static U Value<U>(this IEnumerable<JToken> value)
        {
            return value.Value<JToken, U>();
        }
        public static U Value<T, U>(this IEnumerable<T> value) where T : JToken
        {
            ValidationUtils.ArgumentNotNull(value, nameof(value));

            if (!(value is JToken token))
            {
                throw new ArgumentException("Source value must be a JToken.");
            }

            return token.Convert<JToken, U>();
        }

        internal static IEnumerable<U> Values<T, U>(this IEnumerable<T> source, object? key) where T : JToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            if (key == null)
            {
                foreach (T token in source)
                {
                    if (token is JValue value)
                    {
                        yield return Convert<JValue, U>(value);
                    }
                    else
                    {
                        foreach (JToken t in token.Children())
                        {
                            yield return t.Convert<JToken, U>();
                        }
                    }
                }
            }
            else
            {
                foreach (T token in source)
                {
                    JToken? value = token[key];
                    if (value != null)
                    {
                        yield return value.Convert<JToken, U>();
                    }
                }
            }
        }
        public static IJEnumerable<JToken> Children<T>(this IEnumerable<T> source) where T : JToken
        {
            return Children<T, JToken>(source).AsJEnumerable();
        }
        public static IEnumerable<U> Children<T, U>(this IEnumerable<T> source) where T : JToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            return source.SelectMany(c => c.Children()).Convert<JToken, U>();
        }

        internal static IEnumerable<U> Convert<T, U>(this IEnumerable<T> source) where T : JToken
        {
            ValidationUtils.ArgumentNotNull(source, nameof(source));

            foreach (T token in source)
            {
                yield return Convert<JToken, U>(token);
            }
        }

        [return: MaybeNull]
        internal static U Convert<T, U>(this T token) where T : JToken?
        {
            if (token == null)
            {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                return default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
            }

            if (token is U castValue
                && typeof(U) != typeof(IComparable) && typeof(U) != typeof(IFormattable))
            {
                return castValue;
            }
            else
            {
                if (!(token is JValue value))
                {
                    throw new InvalidCastException("Cannot cast {0} to {1}.".FormatWith(CultureInfo.InvariantCulture, token.GetType(), typeof(T)));
                }

                if (value.Value is U u)
                {
                    return u;
                }

                Type targetType = typeof(U);

                if (ReflectionUtils.IsNullableType(targetType))
                {
                    if (value.Value == null)
                    {
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
                        return default;
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
                    }

                    targetType = Nullable.GetUnderlyingType(targetType);
                }

                return (U)System.Convert.ChangeType(value.Value, targetType, CultureInfo.InvariantCulture);
            }
        }
        public static IJEnumerable<JToken> AsJEnumerable(this IEnumerable<JToken> source)
        {
            return source.AsJEnumerable<JToken>();
        }
        public static IJEnumerable<T> AsJEnumerable<T>(this IEnumerable<T> source) where T : JToken
        {
            if (source == null)
            {
                return null!;
            }
            else if (source is IJEnumerable<T> customEnumerable)
            {
                return customEnumerable;
            }
            else
            {
                return new JEnumerable<T>(source);
            }
        }
    }
}