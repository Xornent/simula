
#if (DOTNET || PORTABLE40 || PORTABLE)
using System;

namespace Simula.Scripting.Json
{
    [Obsolete("SerializationBinder is obsolete. Use ISerializationBinder instead.")]
    public abstract class SerializationBinder
    {
        public abstract Type BindToType(string? assemblyName, string typeName);
        public virtual void BindToName(Type serializedType, out string? assemblyName, out string? typeName)
        {
            assemblyName = null;
            typeName = null;
        }
    }
}

#endif