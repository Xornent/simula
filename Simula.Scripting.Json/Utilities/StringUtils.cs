
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Globalization;
using System.Diagnostics.CodeAnalysis;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Simula.Scripting.Json.Utilities
{
    internal static class StringUtils
    {
        public const string CarriageReturnLineFeed = "\r\n";
        public const string Empty = "";
        public const char CarriageReturn = '\r';
        public const char LineFeed = '\n';
        public const char Tab = '\t';

        public static bool IsNullOrEmpty([NotNullWhen(false)] string? value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static string FormatWith(this string format, IFormatProvider provider, object? arg0)
        {
            return format.FormatWith(provider, new object?[] { arg0 });
        }

        public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1)
        {
            return format.FormatWith(provider, new object?[] { arg0, arg1 });
        }

        public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1, object? arg2)
        {
            return format.FormatWith(provider, new object?[] { arg0, arg1, arg2 });
        }

        public static string FormatWith(this string format, IFormatProvider provider, object? arg0, object? arg1, object? arg2, object? arg3)
        {
            return format.FormatWith(provider, new object?[] { arg0, arg1, arg2, arg3 });
        }

        private static string FormatWith(this string format, IFormatProvider provider, params object?[] args)
        {
            ValidationUtils.ArgumentNotNull(format, nameof(format));

            return string.Format(provider, format, args);
        }
        public static bool IsWhiteSpace(string s)
        {
            if (s == null) {
                throw new ArgumentNullException(nameof(s));
            }

            if (s.Length == 0) {
                return false;
            }

            for (int i = 0; i < s.Length; i++) {
                if (!char.IsWhiteSpace(s[i])) {
                    return false;
                }
            }

            return true;
        }

        public static StringWriter CreateStringWriter(int capacity)
        {
            StringBuilder sb = new StringBuilder(capacity);
            StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);

            return sw;
        }

        public static void ToCharAsUnicode(char c, char[] buffer)
        {
            buffer[0] = '\\';
            buffer[1] = 'u';
            buffer[2] = MathUtils.IntToHex((c >> 12) & '\x000f');
            buffer[3] = MathUtils.IntToHex((c >> 8) & '\x000f');
            buffer[4] = MathUtils.IntToHex((c >> 4) & '\x000f');
            buffer[5] = MathUtils.IntToHex(c & '\x000f');
        }

        public static TSource ForgivingCaseSensitiveFind<TSource>(this IEnumerable<TSource> source, Func<TSource, string> valueSelector, string testValue)
        {
            if (source == null) {
                throw new ArgumentNullException(nameof(source));
            }
            if (valueSelector == null) {
                throw new ArgumentNullException(nameof(valueSelector));
            }

            IEnumerable<TSource> caseInsensitiveResults = source.Where(s => string.Equals(valueSelector(s), testValue, StringComparison.OrdinalIgnoreCase));
            if (caseInsensitiveResults.Count() <= 1) {
                return caseInsensitiveResults.SingleOrDefault();
            } else {
                IEnumerable<TSource> caseSensitiveResults = source.Where(s => string.Equals(valueSelector(s), testValue, StringComparison.Ordinal));
                return caseSensitiveResults.SingleOrDefault();
            }
        }

        public static string ToCamelCase(string s)
        {
            if (StringUtils.IsNullOrEmpty(s) || !char.IsUpper(s[0])) {
                return s;
            }

            char[] chars = s.ToCharArray();

            for (int i = 0; i < chars.Length; i++) {
                if (i == 1 && !char.IsUpper(chars[i])) {
                    break;
                }

                bool hasNext = (i + 1 < chars.Length);
                if (i > 0 && hasNext && !char.IsUpper(chars[i + 1])) {
                    if (char.IsSeparator(chars[i + 1])) {
                        chars[i] = ToLower(chars[i]);
                    }

                    break;
                }

                chars[i] = ToLower(chars[i]);
            }

            return new string(chars);
        }

        private static char ToLower(char c)
        {
#if HAVE_CHAR_TO_LOWER_WITH_CULTURE
            c = char.ToLower(c, CultureInfo.InvariantCulture);
#else
            c = char.ToLowerInvariant(c);
#endif
            return c;
        }

        public static string ToSnakeCase(string s) => ToSeparatedCase(s, '_');

        public static string ToKebabCase(string s) => ToSeparatedCase(s, '-');

        private enum SeparatedCaseState
        {
            Start,
            Lower,
            Upper,
            NewWord
        }

        private static string ToSeparatedCase(string s, char separator)
        {
            if (StringUtils.IsNullOrEmpty(s)) {
                return s;
            }

            StringBuilder sb = new StringBuilder();
            SeparatedCaseState state = SeparatedCaseState.Start;

            for (int i = 0; i < s.Length; i++) {
                if (s[i] == ' ') {
                    if (state != SeparatedCaseState.Start) {
                        state = SeparatedCaseState.NewWord;
                    }
                } else if (char.IsUpper(s[i])) {
                    switch (state) {
                        case SeparatedCaseState.Upper:
                            bool hasNext = (i + 1 < s.Length);
                            if (i > 0 && hasNext) {
                                char nextChar = s[i + 1];
                                if (!char.IsUpper(nextChar) && nextChar != separator) {
                                    sb.Append(separator);
                                }
                            }
                            break;
                        case SeparatedCaseState.Lower:
                        case SeparatedCaseState.NewWord:
                            sb.Append(separator);
                            break;
                    }

                    char c;
#if HAVE_CHAR_TO_LOWER_WITH_CULTURE
                    c = char.ToLower(s[i], CultureInfo.InvariantCulture);
#else
                    c = char.ToLowerInvariant(s[i]);
#endif
                    sb.Append(c);

                    state = SeparatedCaseState.Upper;
                } else if (s[i] == separator) {
                    sb.Append(separator);
                    state = SeparatedCaseState.Start;
                } else {
                    if (state == SeparatedCaseState.NewWord) {
                        sb.Append(separator);
                    }

                    sb.Append(s[i]);
                    state = SeparatedCaseState.Lower;
                }
            }

            return sb.ToString();
        }

        public static bool IsHighSurrogate(char c)
        {
#if HAVE_UNICODE_SURROGATE_DETECTION
            return char.IsHighSurrogate(c);
#else
            return (c >= 55296 && c <= 56319);
#endif
        }

        public static bool IsLowSurrogate(char c)
        {
#if HAVE_UNICODE_SURROGATE_DETECTION
            return char.IsLowSurrogate(c);
#else
            return (c >= 56320 && c <= 57343);
#endif
        }

        public static bool StartsWith(this string source, char value)
        {
            return (source.Length > 0 && source[0] == value);
        }

        public static bool EndsWith(this string source, char value)
        {
            return (source.Length > 0 && source[source.Length - 1] == value);
        }

        public static string Trim(this string s, int start, int length)
        {
            if (s == null) {
                throw new ArgumentNullException();
            }
            if (start < 0) {
                throw new ArgumentOutOfRangeException(nameof(start));
            }
            if (length < 0) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            int end = start + length - 1;
            if (end >= s.Length) {
                throw new ArgumentOutOfRangeException(nameof(length));
            }
            for (; start < end; start++) {
                if (!char.IsWhiteSpace(s[start])) {
                    break;
                }
            }
            for (; end >= start; end--) {
                if (!char.IsWhiteSpace(s[end])) {
                    break;
                }
            }
            return s.Substring(start, end - start + 1);
        }
    }
}