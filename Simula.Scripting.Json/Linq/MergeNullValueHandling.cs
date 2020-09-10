using System;

namespace Simula.Scripting.Json.Linq
{
    [Flags]
    public enum MergeNullValueHandling
    {
        Ignore = 0,
        Merge = 1
    }
}
