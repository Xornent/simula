using System;
using System.Text.RegularExpressions;
using Simula.Scripting.Json.Bson;
using System.Globalization;
using System.Runtime.CompilerServices;
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json.Converters
{
    /// <summary>
    /// Converts a <see cref="Regex"/> to and from JSON and BSON.
    /// </summary>
    public class RegexConverter : JsonConverter
    {
        private const string PatternName = "Pattern";
        private const string OptionsName = "Options";

        /// <summary>
        /// Writes the JSON representation of the object.
        /// </summary>
        /// <param name="writer">The <see cref="JsonWriter"/> to write to.</param>
        /// <param name="value">The value.</param>
        /// <param name="serializer">The calling serializer.</param>
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            Regex regex = (Regex)value;

#pragma warning disable 618
            BsonWriter bsonWriter = writer as BsonWriter;
            if (bsonWriter != null)
            {
                WriteBson(bsonWriter, regex);
            }
#pragma warning restore 618
            else
            {
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
            // Regular expression - The first cstring is the regex pattern, the second
            // is the regex options string. Options are identified by characters, which 
            // must be stored in alphabetical order. Valid options are 'i' for case 
            // insensitive matching, 'm' for multiline matching, 'x' for verbose mode, 
            // 'l' to make \w, \W, etc. locale dependent, 's' for dotall mode 
            // ('.' matches everything), and 'u' to make \w, \W, etc. match unicode.

            string options = null;

            if (HasFlag(regex.Options, RegexOptions.IgnoreCase))
            {
                options += "i";
            }

            if (HasFlag(regex.Options, RegexOptions.Multiline))
            {
                options += "m";
            }

            if (HasFlag(regex.Options, RegexOptions.Singleline))
            {
                options += "s";
            }

            options += "u";

            if (HasFlag(regex.Options, RegexOptions.ExplicitCapture))
            {
                options += "x";
            }

            writer.WriteRegex(regex.ToString(), options);
        }
#pragma warning restore 618

        private void WriteJson(JsonWriter writer, Regex regex, JsonSerializer serializer)
        {
            DefaultContractResolver resolver = serializer.ContractResolver as DefaultContractResolver;

            writer.WriteStartObject();
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(PatternName) : PatternName);
            writer.WriteValue(regex.ToString());
            writer.WritePropertyName((resolver != null) ? resolver.GetResolvedPropertyName(OptionsName) : OptionsName);
            serializer.Serialize(writer, regex.Options);
            writer.WriteEndObject();
        }

        /// <summary>
        /// Reads the JSON representation of the object.
        /// </summary>
        /// <param name="reader">The <see cref="JsonReader"/> to read from.</param>
        /// <param name="objectType">Type of the object.</param>
        /// <param name="existingValue">The existing value of object being read.</param>
        /// <param name="serializer">The calling serializer.</param>
        /// <returns>The object value.</returns>
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
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
            string regexText = (string)reader.Value;
            int patternOptionDelimiterIndex = regexText.LastIndexOf('/');

            string patternText = regexText.Substring(1, patternOptionDelimiterIndex - 1);
            string optionsText = regexText.Substring(patternOptionDelimiterIndex + 1);

            RegexOptions options = RegexOptions.None;
            foreach (char c in optionsText)
            {
                switch (c)
                {
                    case 'i':
                        options |= RegexOptions.IgnoreCase;
                        break;
                    case 'm':
                        options |= RegexOptions.Multiline;
                        break;
                    case 's':
                        options |= RegexOptions.Singleline;
                        break;
                    case 'x':
                        options |= RegexOptions.ExplicitCapture;
                        break;
                }
            }

            return new Regex(patternText, options);
        }

        private Regex ReadRegexObject(JsonReader reader, JsonSerializer serializer)
        {
            string pattern = null;
            RegexOptions? options = null;

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        string propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
                        }

                        if (string.Equals(propertyName, PatternName, StringComparison.OrdinalIgnoreCase))
                        {
                            pattern = (string)reader.Value;
                        }
                        else if (string.Equals(propertyName, OptionsName, StringComparison.OrdinalIgnoreCase))
                        {
                            options = serializer.Deserialize<RegexOptions>(reader);
                        }
                        else
                        {
                            reader.Skip();
                        }
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        if (pattern == null)
                        {
                            throw JsonSerializationException.Create(reader, "Error deserializing Regex. No pattern found.");
                        }

                        return new Regex(pattern, options ?? RegexOptions.None);
                }
            }

            throw JsonSerializationException.Create(reader, "Unexpected end when reading Regex.");
        }

        /// <summary>
        /// Determines whether this instance can convert the specified object type.
        /// </summary>
        /// <param name="objectType">Type of the object.</param>
        /// <returns>
        /// 	<c>true</c> if this instance can convert the specified object type; otherwise, <c>false</c>.
        /// </returns>
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