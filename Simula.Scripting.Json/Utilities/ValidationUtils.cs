
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Simula.Scripting.Json.Utilities
{
    internal static class ValidationUtils
    {
        public static void ArgumentNotNull([NotNull]object? value, string parameterName)
        {
            if (value == null)
            {
                throw new ArgumentNullException(parameterName);
            }
        }
    }
}