﻿
using System;
using System.Text;
using System.Text.RegularExpressions;

namespace Simula.Editor.Search
{
    /// <summary>
    /// Provides factory methods for ISearchStrategies.
    /// </summary>
    public static class SearchStrategyFactory
    {
        /// <summary>
        /// Creates a default ISearchStrategy with the given parameters.
        /// </summary>
        public static ISearchStrategy Create(string searchPattern, bool ignoreCase, bool matchWholeWords, SearchMode mode)
        {
            if (searchPattern == null)
                throw new ArgumentNullException("searchPattern");
            RegexOptions options = RegexOptions.Compiled | RegexOptions.Multiline;
            if (ignoreCase)
                options |= RegexOptions.IgnoreCase;

            switch (mode) {
                case SearchMode.Normal:
                    searchPattern = Regex.Escape(searchPattern);
                    break;
                case SearchMode.Wildcard:
                    searchPattern = ConvertWildcardsToRegex(searchPattern);
                    break;
            }
            try {
                Regex pattern = new Regex(searchPattern, options);
                return new RegexSearchStrategy(pattern, matchWholeWords);
            } catch (ArgumentException ex) {
                throw new SearchPatternException(ex.Message, ex);
            }
        }

        private static string ConvertWildcardsToRegex(string searchPattern)
        {
            if (string.IsNullOrEmpty(searchPattern))
                return "";

            StringBuilder builder = new StringBuilder();

            foreach (char ch in searchPattern) {
                switch (ch) {
                    case '?':
                        builder.Append(".");
                        break;
                    case '*':
                        builder.Append(".*");
                        break;
                    default:
                        builder.Append(Regex.Escape(ch.ToString()));
                        break;
                }
            }

            return builder.ToString();
        }
    }
}
