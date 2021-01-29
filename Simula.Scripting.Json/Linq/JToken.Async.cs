
#if HAVE_ASYNC

using Simula.Scripting.Json.Utilities;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Simula.Scripting.Json.Linq
{
    public abstract partial class JToken
    {
        public virtual Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            throw new NotImplementedException();
        }
        public Task WriteToAsync(JsonWriter writer, params JsonConverter[] converters)
        {
            return WriteToAsync(writer, default, converters);
        }
        public static Task<JToken> ReadFromAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return ReadFromAsync(reader, null, cancellationToken);
        }
        public static async Task<JToken> ReadFromAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (reader.TokenType == JsonToken.None) {
                if (!await (settings != null && settings.CommentHandling == CommentHandling.Ignore ? reader.ReadAndMoveToContentAsync(cancellationToken) : reader.ReadAsync(cancellationToken)).ConfigureAwait(false)) {
                    throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader.");
                }
            }

            IJsonLineInfo? lineInfo = reader as IJsonLineInfo;

            switch (reader.TokenType) {
                case JsonToken.StartObject:
                    return await JObject.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(false);
                case JsonToken.StartArray:
                    return await JArray.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(false);
                case JsonToken.StartConstructor:
                    return await JConstructor.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(false);
                case JsonToken.PropertyName:
                    return await JProperty.LoadAsync(reader, settings, cancellationToken).ConfigureAwait(false);
                case JsonToken.String:
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.Date:
                case JsonToken.Boolean:
                case JsonToken.Bytes:
                    JValue v = new JValue(reader.Value);
                    v.SetLineInfo(lineInfo, settings);
                    return v;
                case JsonToken.Comment:
                    v = JValue.CreateComment(reader.Value?.ToString());
                    v.SetLineInfo(lineInfo, settings);
                    return v;
                case JsonToken.Null:
                    v = JValue.CreateNull();
                    v.SetLineInfo(lineInfo, settings);
                    return v;
                case JsonToken.Undefined:
                    v = JValue.CreateUndefined();
                    v.SetLineInfo(lineInfo, settings);
                    return v;
                default:
                    throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader. Unexpected token: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }
        }
        public static Task<JToken> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return LoadAsync(reader, null, cancellationToken);
        }
        public static Task<JToken> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            return ReadFromAsync(reader, settings, cancellationToken);
        }
    }
}

#endif