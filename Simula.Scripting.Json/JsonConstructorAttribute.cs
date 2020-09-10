
using System;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public sealed class JsonConstructorAttribute : Attribute
    {
    }
}