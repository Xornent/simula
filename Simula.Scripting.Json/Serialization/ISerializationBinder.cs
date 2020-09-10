
using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Simula.Scripting.Json.Serialization
{
    public interface ISerializationBinder
    {
        Type BindToType(string? assemblyName, string typeName);
        void BindToName(Type serializedType, out string? assemblyName, out string? typeName);
    }
}