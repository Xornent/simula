
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Json
{
    public enum ReferenceLoopHandling
    {
        Error = 0,
        Ignore = 1,
        Serialize = 2
    }
}