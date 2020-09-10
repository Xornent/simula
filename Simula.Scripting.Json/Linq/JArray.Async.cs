
#if HAVE_ASYNC

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public partial class JArray
    {
        public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            await writer.WriteStartArrayAsync(cancellationToken).ConfigureAwait(false);

            for (int i = 0; i < _values.Count; i++)
            {
                await _values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(false);
            }

            await writer.WriteEndArrayAsync(cancellationToken).ConfigureAwait(false);
        }
        public new static Task<JArray> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return LoadAsync(reader, null, cancellationToken);
        }
        public new static async Task<JArray> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader.");
                }
            }

            await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false);

            if (reader.TokenType != JsonToken.StartArray)
            {
                throw JsonReaderException.Create(reader, "Error reading JArray from JsonReader. Current JsonReader item is not an array: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JArray a = new JArray();
            a.SetLineInfo(reader as IJsonLineInfo, settings);

            await a.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(false);

            return a;
        }
    }
}

#endif