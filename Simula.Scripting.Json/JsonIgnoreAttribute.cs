using System;
using System.Collections.Generic;
using System.Text;

namespace Simula.Scripting.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> not to serialize the public field or public read/write property value.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public sealed class JsonIgnoreAttribute : Attribute
    {
    }
}