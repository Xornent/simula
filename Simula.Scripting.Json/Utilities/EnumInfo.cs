
namespace Simula.Scripting.Json.Utilities
{
    internal class EnumInfo
    {
        public EnumInfo(bool isFlags, ulong[] values, string[] names, string[] resolvedNames)
        {
            IsFlags = isFlags;
            Values = values;
            Names = names;
            ResolvedNames = resolvedNames;
        }

        public readonly bool IsFlags;
        public readonly ulong[] Values;
        public readonly string[] Names;
        public readonly string[] ResolvedNames;
    }
}