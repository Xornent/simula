
#if HAVE_BINARY_SERIALIZATION
using System;
using System.Runtime.Serialization;

namespace Simula.Scripting.Json.Serialization
{
    public class JsonISerializableContract : JsonContainerContract
    {
        public ObjectConstructor<object>? ISerializableCreator { get; set; }
        public JsonISerializableContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Serializable;
        }
    }
}

#endif