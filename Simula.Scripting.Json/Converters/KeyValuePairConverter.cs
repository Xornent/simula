
using System;
using System.Collections.Generic;
using Simula.Scripting.Json.Serialization;
using Simula.Scripting.Json.Utilities;
using System.Reflection;

namespace Simula.Scripting.Json.Converters
{
    public class KeyValuePairConverter : JsonConverter
    {
        private const string KeyName = "Key";
        private const string ValueName = "Value";

        private static readonly ThreadSafeStore<Type, ReflectionObject> ReflectionObjectPerType = new ThreadSafeStore<Type, ReflectionObject>(InitializeReflectionObject);

        private static ReflectionObject InitializeReflectionObject(Type t)
        {
            IList<Type> genericArguments = t.GetGenericArguments();
            Type keyType = genericArguments[0];
            Type valueType = genericArguments[1];

            return ReflectionObject.Create(t, t.GetConstructor(new[] { keyType, valueType }), KeyName, ValueName);
        }
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null)
            {
                writer.WriteNull();
                return;
            }

            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(value.GetType());

            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(KeyName) : KeyName);
            serializer.Serialize(writer, reflectionObject.GetValue(value, KeyName), reflectionObject.GetType(KeyName));
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(ValueName) : ValueName);
            serializer.Serialize(writer, reflectionObject.GetValue(value, ValueName), reflectionObject.GetType(ValueName));
            writer.WriteEndObject();
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.Null)
            {
                if (!ReflectionUtils.IsNullableType(objectType))
                {
                    throw JsonSerializationException.Create(reader, "Cannot convert null value to KeyValuePair.");
                }

                return null;
            }

            object? key = null;
            object? value = null;

            reader.ReadAndAssert();

            Type t = ReflectionUtils.IsNullableType(objectType)
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            ReflectionObject reflectionObject = ReflectionObjectPerType.Get(t);
            JsonContract keyContract = serializer.ContractResolver.ResolveContract(reflectionObject.GetType(KeyName));
            JsonContract valueContract = serializer.ContractResolver.ResolveContract(reflectionObject.GetType(ValueName));

            while (reader.TokenType == JsonToken.PropertyName)
            {
                string propertyName = reader.Value!.ToString();
                if (string.Equals(propertyName, KeyName, StringComparison.OrdinalIgnoreCase))
                {
                    reader.ReadForTypeAndAssert(keyContract, false);

                    key = serializer.Deserialize(reader, keyContract.UnderlyingType);
                }
                else if (string.Equals(propertyName, ValueName, StringComparison.OrdinalIgnoreCase))
                {
                    reader.ReadForTypeAndAssert(valueContract, false);

                    value = serializer.Deserialize(reader, valueContract.UnderlyingType);
                }
                else
                {
                    reader.Skip();
                }

                reader.ReadAndAssert();
            }

            return reflectionObject.Creator!(key, value);
        }
        public override bool CanConvert(Type objectType)
        {
            Type t = (ReflectionUtils.IsNullableType(objectType))
                ? Nullable.GetUnderlyingType(objectType)
                : objectType;

            if (t.IsValueType() && t.IsGenericType())
            {
                return (t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>));
            }

            return false;
        }
    }
}