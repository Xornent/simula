
#if HAVE_ASYNC

using System;
using System.Globalization;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using System.Threading;
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public partial class JValue
    {
        public override Task WriteToAsync(JsonWriter writer, CancellationToken cancellationToken, params JsonConverter[] converters)
        {
            if (converters != null && converters.Length > 0 && _value != null) {
                JsonConverter? matchingConverter = JsonSerializer.GetMatchingConverter(converters, _value.GetType());
                if (matchingConverter != null && matchingConverter.CanWrite) {
                    matchingConverter.WriteJson(writer, _value, JsonSerializer.CreateDefault());
                    return AsyncUtils.CompletedTask;
                }
            }

            switch (_valueType) {
                case JTokenType.Comment:
                    return writer.WriteCommentAsync(_value?.ToString(), cancellationToken);
                case JTokenType.Raw:
                    return writer.WriteRawValueAsync(_value?.ToString(), cancellationToken);
                case JTokenType.Null:
                    return writer.WriteNullAsync(cancellationToken);
                case JTokenType.Undefined:
                    return writer.WriteUndefinedAsync(cancellationToken);
                case JTokenType.Integer:
                    if (_value is int i) {
                        return writer.WriteValueAsync(i, cancellationToken);
                    }

                    if (_value is long l) {
                        return writer.WriteValueAsync(l, cancellationToken);
                    }

                    if (_value is ulong ul) {
                        return writer.WriteValueAsync(ul, cancellationToken);
                    }

#if HAVE_BIG_INTEGER
                    if (_value is BigInteger integer)
                    {
                        return writer.WriteValueAsync(integer, cancellationToken);
                    }
#endif

                    return writer.WriteValueAsync(Convert.ToInt64(_value, CultureInfo.InvariantCulture), cancellationToken);
                case JTokenType.Float:
                    if (_value is decimal dec) {
                        return writer.WriteValueAsync(dec, cancellationToken);
                    }

                    if (_value is double d) {
                        return writer.WriteValueAsync(d, cancellationToken);
                    }

                    if (_value is float f) {
                        return writer.WriteValueAsync(f, cancellationToken);
                    }

                    return writer.WriteValueAsync(Convert.ToDouble(_value, CultureInfo.InvariantCulture), cancellationToken);
                case JTokenType.String:
                    return writer.WriteValueAsync(_value?.ToString(), cancellationToken);
                case JTokenType.Boolean:
                    return writer.WriteValueAsync(Convert.ToBoolean(_value, CultureInfo.InvariantCulture), cancellationToken);
                case JTokenType.Date:
                    if (_value is DateTimeOffset offset) {
                        return writer.WriteValueAsync(offset, cancellationToken);
                    }

                    return writer.WriteValueAsync(Convert.ToDateTime(_value, CultureInfo.InvariantCulture), cancellationToken);
                case JTokenType.Bytes:
                    return writer.WriteValueAsync((byte[]?)_value, cancellationToken);
                case JTokenType.Guid:
                    return writer.WriteValueAsync(_value != null ? (Guid?)_value : null, cancellationToken);
                case JTokenType.TimeSpan:
                    return writer.WriteValueAsync(_value != null ? (TimeSpan?)_value : null, cancellationToken);
                case JTokenType.Uri:
                    return writer.WriteValueAsync((Uri?)_value, cancellationToken);
            }

            throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(Type), _valueType, "Unexpected token type.");
        }
    }
}

#endif
