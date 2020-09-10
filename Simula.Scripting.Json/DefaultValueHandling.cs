
using System;
using System.ComponentModel;

namespace Simula.Scripting.Json
{
    [Flags]
    public enum DefaultValueHandling
    {
        Include = 0,
        Ignore = 1,
        Populate = 2,
        IgnoreAndPopulate = Ignore | Populate
    }
}