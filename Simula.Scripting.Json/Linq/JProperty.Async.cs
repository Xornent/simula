
#if HAVE_ASYNC

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public partial class JProperty
    {
        public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            Task task = writer.WritePropertyNameAsync(_name, cancellationToken);
            if (task.IsCompletedSucessfully())
            {
                return WriteValueAsync(writer, cancellationToken, converters);
            }

            return WriteToAsync(task, writer, cancellationToken, converters);
        }

        private async Task WriteToAsync(Task task, JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            await task.ConfigureAwait(false);

            await WriteValueAsync(writer, cancellationToken, converters).ConfigureAwait(false);
        }

        private Task WriteValueAsync(JsonWriter writer, CancellationToken cancellationToken, JsonConverter[] converters)
        {
            JToken value = Value;
            return value != null
                ? value.WriteToAsync(writer, cancellationToken, converters)
                : writer.WriteNullAsync(cancellationToken);
        }
        public new static Task<JProperty> LoadAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            return LoadAsync(reader, null, cancellationToken);
        }
        public new static async Task<JProperty> LoadAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            if (reader.TokenType == JsonToken.None)
            {
                if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader.");
                }
            }

            await reader.MoveToContentAsync(cancellationToken).ConfigureAwait(false);

            if (reader.TokenType != JsonToken.PropertyName)
            {
                throw JsonReaderException.Create(reader, "Error reading JProperty from JsonReader. Current JsonReader item is not a property: {0}".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
            }

            JProperty p = new JProperty((string)reader.Value!);
            p.SetLineInfo(reader as IJsonLineInfo, settings);

            await p.ReadTokenFromAsync(reader, settings, cancellationToken).ConfigureAwait(false);

            return p;
        }
    }
}

#endif
