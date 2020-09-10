
using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonIgnoreAttribute : Attribute
    {
    }
}