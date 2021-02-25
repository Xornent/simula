using System;
using Simula.Scripting.Json.Utilities;
using System.Globalization;

namespace Simula.Scripting.Json.Serialization
{
    internal class DefaultReferenceResolver : IReferenceResolver
    {
        private int _referenceCount;

        private BidirectionalDictionary<string, object> GetMappings(object context)
        {
            JsonSerializerInternalBase internalSerializer = context as JsonSerializerInternalBase;
            if (internalSerializer == null)
            {
                JsonSerializerProxy proxy = context as JsonSerializerProxy;
                if (proxy != null)
                {
                    internalSerializer = proxy.GetInternalSerializer();
                }
                else
                {
                    throw new JsonException("The DefaultReferenceResolver can only be used internally.");
                }
            }

            return internalSerializer.DefaultReferenceMappings;
        }

        public object ResolveReference(object context, string reference)
        {
            object value;
            GetMappings(context).TryGetByFirst(reference, out value);
            return value;
        }

        public string GetReference(object context, object value)
        {
            BidirectionalDictionary<string, object> mappings = GetMappings(context);

            string reference;
            if (!mappings.TryGetBySecond(value, out reference))
            {
                _referenceCount++;
                reference = _referenceCount.ToString(CultureInfo.InvariantCulture);
                mappings.Set(reference, value);
            }

            return reference;
        }

        public void AddReference(object context, string reference, object value)
        {
            GetMappings(context).Set(reference, value);
        }

        public bool IsReferenced(object context, object value)
        {
            string reference;
            return GetMappings(context).TryGetBySecond(value, out reference);
        }
    }
}