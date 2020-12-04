
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Json.Utilities
{
    internal delegate T Creator<T>();

    internal static class MiscellaneousUtils
    {
        [Conditional("DEBUG")]
        public static void Assert([DoesNotReturnIf(false)] bool condition, string? message = null)
        {
            Debug.Assert(condition, message);
        }

        public static bool ValueEquals(object? objA, object? objB)
        {
            if (objA == objB) {
                return true;
            }
            if (objA == null || objB == null) {
                return false;
            }
            if (objA.GetType() != objB.GetType()) {
                if (ConvertUtils.IsInteger(objA) && ConvertUtils.IsInteger(objB)) {
                    return Convert.ToDecimal(objA, CultureInfo.CurrentCulture).Equals(Convert.ToDecimal(objB, CultureInfo.CurrentCulture));
                } else if ((objA is double || objA is float || objA is decimal) && (objB is double || objB is float || objB is decimal)) {
                    return MathUtils.ApproxEquals(Convert.ToDouble(objA, CultureInfo.CurrentCulture), Convert.ToDouble(objB, CultureInfo.CurrentCulture));
                } else {
                    return false;
                }
            }

            return objA.Equals(objB);
        }

        public static ArgumentOutOfRangeException CreateArgumentOutOfRangeException(string paramName, object actualValue, string message)
        {
            string newMessage = message + Environment.NewLine + @"Actual value was {0}.".FormatWith(CultureInfo.InvariantCulture, actualValue);

            return new ArgumentOutOfRangeException(paramName, newMessage);
        }

        public static string ToString(object? value)
        {
            if (value == null) {
                return "{null}";
            }

            return (value is string s) ? @"""" + s + @"""" : value!.ToString();
        }

        public static int ByteArrayCompare(byte[] a1, byte[] a2)
        {
            int lengthCompare = a1.Length.CompareTo(a2.Length);
            if (lengthCompare != 0) {
                return lengthCompare;
            }

            for (int i = 0; i < a1.Length; i++) {
                int valueCompare = a1[i].CompareTo(a2[i]);
                if (valueCompare != 0) {
                    return valueCompare;
                }
            }

            return 0;
        }

        public static string? GetPrefix(string qualifiedName)
        {
            GetQualifiedNameParts(qualifiedName, out string? prefix, out _);

            return prefix;
        }

        public static string GetLocalName(string qualifiedName)
        {
            GetQualifiedNameParts(qualifiedName, out _, out string localName);

            return localName;
        }

        public static void GetQualifiedNameParts(string qualifiedName, out string? prefix, out string localName)
        {
            int colonPosition = qualifiedName.IndexOf(':');

            if ((colonPosition == -1 || colonPosition == 0) || (qualifiedName.Length - 1) == colonPosition) {
                prefix = null;
                localName = qualifiedName;
            } else {
                prefix = qualifiedName.Substring(0, colonPosition);
                localName = qualifiedName.Substring(colonPosition + 1);
            }
        }

        internal static RegexOptions GetRegexOptions(string optionsText)
        {
            RegexOptions options = RegexOptions.None;

            for (int i = 0; i < optionsText.Length; i++) {
                switch (optionsText[i]) {
                    case 'i':
                        options |= RegexOptions.IgnoreCase;
                        break;
                    case 'm':
                        options |= RegexOptions.Multiline;
                        break;
                    case 's':
                        options |= RegexOptions.Singleline;
                        break;
                    case 'x':
                        options |= RegexOptions.ExplicitCapture;
                        break;
                }
            }

            return options;
        }
    }
}