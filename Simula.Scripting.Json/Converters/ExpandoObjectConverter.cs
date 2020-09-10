
#if HAVE_DYNAMIC

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Text;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Converters
{
    public class ExpandoObjectConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object? ReadValue(JsonReader reader)
        {
            if (!reader.MoveToContent())
            {
                throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadList(reader);
                default:
                    if (JsonTokenUtils.IsPrimitiveToken(reader.TokenType))
                    {
                        return reader.Value;
                    }

                    throw JsonSerializationException.Create(reader, "Unexpected token when converting ExpandoObject: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }

        private object ReadList(JsonReader reader)
        {
            IList<object?> list = new List<object?>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    default:
                        object? v = ReadValue(reader);

                        list.Add(v);
                        break;
                    case JsonToken.EndArray:
                        return list;
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
        }

        private object ReadObject(JsonReader reader)
        {
            IDictionary<string, object?> expandoObject = new ExpandoObject();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value!.ToString();

                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
                        }

                        object? v = ReadValue(reader);

                        expandoObject[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return expandoObject;
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading ExpandoObject.");
        }
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ExpandoObject));
        }
        public override bool CanWrite => false;
    }
}

#endif