
using System;

namespace Simula.Scripting.Json.Serialization
{
    public interface IContractResolver
    {
        JsonContract ResolveContract(Type type);
    }
}