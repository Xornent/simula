
#if HAVE_ASYNC

using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public partial class JObject
    {
        public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            Task t = writer.WriteStartObjectAsync(cancellationToken);
            if (!t.IsCompletedSucessfully())
            {
                return AwaitProperties(t, 0, writer, cancellationToken, converters);
            }

            for (int i = 0; i < _properties.Count; i++)
            {
                t = _properties[i].WriteToAsync(writer, cancellationToken, converters);
                if (!t.IsCompletedSucessfully())
                {
                    return AwaitProperties(t, i + 1, writer, cancellationToken, converters);
                }
            }

            return writer.WriteEndObjectAsync(cancellationToken);
            async Task AwaitProperties(Task task, int i, JsonWriter Writer, CancellationToken CancellationToken, JsonConverter[] Converters)
            {
                await task.ConfigureAwait(false);
                for (; i < _properties.Count; i++)
                {
                    await _properties[i].WriteToAsync(Writer, CancellationToken, Converters).ConfigureAwait(false);
                }

                await Writer.WriteEndObjectAsync(CancellationToken).ConfigureAwait(false);
            }
        }
        public new static Task<JObject> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return LoadAsync(reader, null, cancellationToken);
        }
        public new static async Task<JObject> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            if (reader.TokenType == JsonToken.None)
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader.");
                }
            }

            await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false);

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw JsonReaderException.Create(reader, "Error reading JObject from JsonReader. Current JsonReader item is not an object: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JObject o = new JObject();
            o.SetLineInfo(reader as IJsonLineInfo, settings);

            await o.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(false);

            return o;
        }
    }
}

#endif
