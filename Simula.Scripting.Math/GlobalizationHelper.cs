using System;
using System.Collections.Generic;
using System.Globalization;

namespace Simula.Maths
{
    /// <summary>
    /// Globalized String Handling Helpers
    /// </summary>
    internal static class GlobalizationHelper
    {
        /// <summary>
        /// Tries to get a <see cref="CultureInfo"/> from the format provider,
        /// returning the current culture if it fails.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>A <see cref="CultureInfo"/> instance.</returns>
        internal static CultureInfo GetCultureInfo(this IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                return CultureInfo.CurrentCulture;
            }

            return (formatProvider as CultureInfo)
                ?? (formatProvider.GetFormat(typeof (CultureInfo)) as CultureInfo)
                    ?? CultureInfo.CurrentCulture;
        }

        /// <summary>
        /// Tries to get a <see cref="NumberFormatInfo"/> from the format
        /// provider, returning the current culture if it fails.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>A <see cref="NumberFormatInfo"/> instance.</returns>
        internal static NumberFormatInfo GetNumberFormatInfo(this IFormatProvider formatProvider)
        {
            return NumberFormatInfo.GetInstance(formatProvider);
        }

        /// <summary>
        /// Tries to get a <see cref="TextInfo"/> from the format provider, returning the current culture if it fails.
        /// </summary>
        /// <param name="formatProvider">
        /// An <see cref="IFormatProvider"/> that supplies culture-specific
        /// formatting information.
        /// </param>
        /// <returns>A <see cref="TextInfo"/> instance.</returns>
        internal static TextInfo GetTextInfo(this IFormatProvider formatProvider)
        {
            if (formatProvider == null)
            {
                return CultureInfo.CurrentCulture.TextInfo;
            }

            return (formatProvider.GetFormat(typeof (TextInfo)) as TextInfo)
                ?? GetCultureInfo(formatProvider).TextInfo;
        }

        /// <summary>
        /// Globalized Parsing: Tokenize a node by splitting it into several nodes.
        /// </summary>
        /// <param name="node">Node that contains the trimmed string to be tokenized.</param>
        /// <param name="keywords">List of keywords to tokenize by.</param>
        /// <param name="skip">keywords to skip looking for (because they've already been handled).</param>
        internal static void Tokenize(LinkedListNode<string> node, string[] keywords, int skip)
        {
            for (int i = skip; i < keywords.Length; i++)
            {
                var keyword = keywords[i];
                int indexOfKeyword;
                while ((indexOfKeyword = node.Value.IndexOf(keyword, StringComparison.Ordinal)) >= 0)
                {
                    if (indexOfKeyword != 0)
                    {
                        // separate part before the token, process recursively
                        string partBeforeKeyword = node.Value.Substring(0, indexOfKeyword).Trim();
                        Tokenize(node.List.AddBefore(node, partBeforeKeyword), keywords, i + 1);

                        // continue processing the rest iteratively
                        node.Value = node.Value.Substring(indexOfKeyword);
                    }

                    if (keyword.Length == node.Value.Length)
                    {
                        return;
                    }

                    // separate the token, done
                    string partAfterKeyword = node.Value.Substring(keyword.Length).Trim();
                    node.List.AddBefore(node, keyword);

                    // continue processing the rest on the right iteratively
                    node.Value = partAfterKeyword;
                }
            }
        }

#if NETSTANDARD1_3
        /// <summary>
        /// Globalized Parsing: Parse a double number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <returns>The parsed double number using the current culture information.</returns>
        /// <exception cref="FormatException" />
        internal static double ParseDouble(ref LinkedListNode<string> token)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            double value;
            if (!double.TryParse(token.Value, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }

        /// <summary>
        /// Globalized Parsing: Parse a float number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <returns>The parsed float number using the current culture information.</returns>
        /// <exception cref="FormatException" />
        internal static float ParseSingle(ref LinkedListNode<string> token)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", StringComparison.CurrentCultureIgnoreCase))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            float value;
            if (!Single.TryParse(token.Value, NumberStyles.Any, CultureInfo.CurrentCulture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }

#else
        /// <summary>
        /// Globalized Parsing: Parse a double number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>The parsed double number using the given culture information.</returns>
        /// <exception cref="FormatException" />
        internal static double ParseDouble(ref LinkedListNode<string> token, CultureInfo culture)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", true, culture))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            double value;
            if (!double.TryParse(token.Value, NumberStyles.Any, culture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }

        /// <summary>
        /// Globalized Parsing: Parse a float number
        /// </summary>
        /// <param name="token">First token of the number.</param>
        /// <param name="culture">Culture Info.</param>
        /// <returns>The parsed float number using the given culture information.</returns>
        /// <exception cref="FormatException" />
        internal static float ParseSingle(ref LinkedListNode<string> token, CultureInfo culture)
        {
            // in case the + and - in scientific notation are separated, join them back together.
            if (token.Value.EndsWith("e", true, culture))
            {
                if (token.Next == null || token.Next.Next == null)
                {
                    throw new FormatException();
                }

                token.Value = token.Value + token.Next.Value + token.Next.Next.Value;

                var list = token.List;
                list.Remove(token.Next.Next);
                list.Remove(token.Next);
            }

            float value;
            if (!Single.TryParse(token.Value, NumberStyles.Any, culture, out value))
            {
                throw new FormatException();
            }

            token = token.Next;
            return value;
        }
#endif
    }
}
