using System;

namespace Simula.Scripting.Git.Core
{
    internal static class StringExtensions
    {
        public static int OctalToInt32(this string octal)
        {
            return Convert.ToInt32(octal, 8);
        }
    }
}
