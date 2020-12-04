
using System;
using System.Diagnostics.CodeAnalysis;

namespace Simula.Scripting.Json.Utilities
{
    internal static class ValidationUtils
    {
        public static void ArgumentNotNull([NotNull] object? value, string parameterName)
        {
            if (value == null) {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}