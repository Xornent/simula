﻿
using System;

namespace Simula.Scripting.Json
{
    [Flags]
    public enum TypeNameHandling
    {
        None = 0,
        Objects = 1,
        Arrays = 2,
        All = Objects | Arrays,
        Auto = 4
    }
}