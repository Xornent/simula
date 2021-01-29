
#if HAVE_ASYNC

using Simula.Scripting.Json.Utilities;
using System;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace Simula.Scripting.Json.Linq
{
    public abstract partial class JContainer
    {
        internal async Task ReadTokenFromAsync(JsonReader reader, JsonLoadSettings? options, CancellationToken cancellationToken = default)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));
            int startDepth = reader.Depth;

            if (!await reader.ReadAsync(cancellationToken).ConfigureAwait(false)) {
                throw JsonReaderException.Create(reader, "Error reading {0} from JsonReader.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }

            await ReadContentFromAsync(reader, options, cancellationToken).ConfigureAwait(false);

            if (reader.Depth > startDepth) {
                throw JsonReaderException.Create(reader, "Unexpected end of content while loading {0}.".FormatWith(CultureInfo.InvariantCulture, GetType().Name));
            }
        }

        private async Task ReadContentFromAsync(JsonReader reader, JsonLoadSettings? settings, CancellationToken cancellationToken = default)
        {
            IJsonLineInfo? lineInfo = reader as IJsonLineInfo;

            JContainer? parent = this;

            do {
                if (parent is JProperty p && p.Value != null) {
                    if (parent == this) {
                        return;
                    }

                    parent = parent.Parent;
                }

                MiscellaneousUtils.Assert(parent != null);

                switch (reader.TokenType) {
                    case JsonToken.None:
                        break;
                    case JsonToken.StartArray:
                        JArray a = new JArray();
                        a.SetLineInfo(lineInfo, settings);
                        parent.Add(a);
                        parent = a;
                        break;

                    case JsonToken.EndArray:
                        if (parent == this) {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartObject:
                        JObject o = new JObject();
                        o.SetLineInfo(lineInfo, settings);
                        parent.Add(o);
                        parent = o;
                        break;
                    case JsonToken.EndObject:
                        if (parent == this) {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.StartConstructor:
                        JConstructor constructor = new JConstructor(reader.Value!.ToString());
                        constructor.SetLineInfo(lineInfo, settings);
                        parent.Add(constructor);
                        parent = constructor;
                        break;
                    case JsonToken.EndConstructor:
                        if (parent == this) {
                            return;
                        }

                        parent = parent.Parent;
                        break;
                    case JsonToken.String:
                    case JsonToken.Integer:
                    case JsonToken.Float:
                    case JsonToken.Date:
                    case JsonToken.Boolean:
                    case JsonToken.Bytes:
                        JValue v = new JValue(reader.Value);
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Comment:
                        if (settings != null && settings.CommentHandling == CommentHandling.Load) {
                            v = JValue.CreateComment(reader.Value!.ToString());
                            v.SetLineInfo(lineInfo, settings);
                            parent.Add(v);
                        }
                        break;
                    case JsonToken.Null:
                        v = JValue.CreateNull();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.Undefined:
                        v = JValue.CreateUndefined();
                        v.SetLineInfo(lineInfo, settings);
                        parent.Add(v);
                        break;
                    case JsonToken.PropertyName:
                        JProperty? property = ReadProperty(reader, settings, lineInfo, parent);
                        if (property != null) {
                            parent = property;
                        } else {
                            await reader.SkipAsync().ConfigureAwait(false);
                        }
                        break;
                    default:
                        throw new InvalidOperationException("The JsonReader should not be on a token of type {0}.".FormatWith(CultureInfo.InvariantCulture, reader.TokenType));
                }
            } while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false));
        }
    }
}

#endif
