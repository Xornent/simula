
#if HAVE_ASYNC

using Simula.Scripting.Json.Utilities;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Simula.Scripting.Json.Linq
{
    public partial class JConstructor
    {
        public override async Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            await writer.WriteStartConstructorAsync(_name ?? string.Empty, cancellationToken).ConfigureAwait(false);

            for (int i = 0; i < _values.Count; i++) {
                await _values[i].WriteToAsync(writer, cancellationToken, converters).ConfigureAwait(false);
            }

            await writer.WriteEndConstructorAsync(cancellationToken).ConfigureAwait(false);
        }
        public new static Task<JConstructor> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return LoadAsync(reader, null, cancellationToken);
        }
        public new static async Task<JConstructor> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            if (reader.TokenType == JsonToken.None) {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                    throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader.");
                }
            }

            await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false);

            if (reader.TokenType != JsonToken.StartConstructor) {
                throw JsonReaderException.Create(reader, "Error reading JConstructor from JsonReader. Current JsonReader item is not a constructor: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JConstructor c = new JConstructor((string)reader.Value!);
            c.SetLineInfo(reader as IJsonLineInfo, settings);

            await c.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(false);

            return c;
        }
    }
}

#endif
