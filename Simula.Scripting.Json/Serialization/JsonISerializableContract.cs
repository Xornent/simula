#if HAVE_BINARY_SERIALIZATION
using System;
using System.Runtime.Serialization;

namespace Simula.Scripting.Json.Serialization
{
    /// <summary>
    /// Contract details for a <see cref="Type"/> used by the <see cref="JsonSerializer"/>.
    /// </summary>
    public class JsonISerializableContract : JsonContainerContract
    {
        /// <summary>
        /// Gets or sets the <see cref="ISerializable"/> object constructor.
        /// </summary>
        /// <value>The <see cref="ISerializable"/> object constructor.</value>
        public ObjectConstructor<object> ISerializableCreator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonISerializableContract"/> class.
        /// </summary>
        /// <param name="underlyingType">The underlying type for the contract.</param>
        public JsonISerializableContract(Type underlyingType)
            : base(underlyingType)
        {
            ContractType = JsonContractType.Serializable;
        }
    }
}

#endif