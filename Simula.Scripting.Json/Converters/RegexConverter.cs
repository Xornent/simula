
using Simula.Scripting.Json.Bson;
using Simula.Scripting.Json.Serialization;
using Simula.Scripting.Json.Utilities;
using System;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Simula.Scripting.Json.Converters
{
    public class RegexConverter : JsonConverter
    {
        private const string PatternName = "Pattern";
        private const string OptionsName = "Options";
        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            if (value == null) {
                writer.WriteNull();
                return;
            }

            Regex regex = (Regex)value;

#pragma warning disable 618
            if (writer is BsonWriter bsonWriter) {
                WriteBson(bsonWriter, regex);
            }
#pragma warning restore 618
            else {
                WriteJson(writer, regex, serializer);
            }
        }

        private bool HasFlag(RegexOptions options, RegexOptions flag)
        {
            return ((options & flag) == flag);
        }

#pragma warning disable 618
        private void WriteBson(BsonWriter writer, Regex regex)
        {

            string? options = null;

            if (HasFlag(regex.Options, RegexOptions.IgnoreCase)) {
                options += "i";
            }

            if (HasFlag(regex.Options, RegexOptions.Multiline)) {
                options += "m";
            }

            if (HasFlag(regex.Options, RegexOptions.Singleline)) {
                options += "s";
            }

            options += "u";

            if (HasFlag(regex.Options, RegexOptions.ExplicitCapture)) {
                options += "x";
            }

            writer.WriteRegex(regex.ToString(), options);
        }
#pragma warning restore 618

        private void WriteJson(JsonWriter writer, Regex regex, JsonSerializer serializer)
        {
            DefaultContractResolver? resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(PatternName) : PatternName);
            writer.WriteValue(regex.ToString());
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(OptionsName) : OptionsName);
            serializer.Serialize(writer, regex.Options);
            writer.WriteEndObject();
        }
        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType) {
                case JsonToken.StartObject:
                    return ReadRegexObject(reader, serializer);
                case JsonToken.String:
                    return ReadRegexString(reader);
                case JsonToken.Null:
                    return null;
            }

            throw JsonSerializationException.Create(reader, "Unexpected token when reading Regex.");
        }

        private object ReadRegexString(JsonReader reader)
        {
            string regexText = (string)reader.Value!;

            if (regexText.Length > 0 && regexText[0] == '/') {
                int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

                if (patternOptionDelimiterIndex > 0) {
                    string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
                    string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

                    RegexOptions options = MiscellaneousUtils.GetRegexOptions(optionsText);

                    return new Regex(patternText, options);
                }
            }

            throw JsonSerializationException.Create(reader, "Regex pattern must be enclosed by slashes.");
        }

        private Regex ReadRegexObject(JsonReader reader, JsonSerializer serializer)
        {
            string? pattern = null;
            RegexOptions? options = null;

            while (reader.Read()) {
                switch (reader.TokenType) {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value!.ToString();

                        if (!reader.Read()) {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
                        }

                        if (string.Equals(propertyName, PatternName, StringComparison.OrdinalIgnoreCase)) {
                            pattern = (string?)reader.Value;
                        } else if (string.Equals(propertyName, OptionsName, StringComparison.OrdinalIgnoreCase)) {
                            options = serializer.Deserialize<RegexOptions>(reader);
                        } else {
                            reader.Skip();
                        }
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        if (pattern == null) {
                            throw JsonSerializationException.Create(reader, "Error deserializing Regex. No pattern found.");
                        }

                        return new Regex(pattern, options ?? RegexOptions.None);
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
        }
        public override bool CanConvert(Type objectType)
        {
            return objectType.Name == nameof(Regex) && IsRegex(objectType);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private bool IsRegex(Type objectType)
        {
            return (objectType == typeof(Regex));
        }
    }
}