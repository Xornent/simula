
using System;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public class JTokenReader : JsonReader, IJsonLineInfo
    {
        private readonly JToken _root;
        private string? _initialPath;
        private JToken? _parent;
        private JToken? _current;
        public JToken? CurrentToken => _current;
        public JTokenReader(JToken token)
        {
            ValidationUtils.ArgumentNotNull(token, nameof(token));

            _root = token;
        }
        public JTokenReader(JToken token, string initialPath)
            : this(token)
        {
            _initialPath = initialPath;
        }
        public override bool Read()
        {
            if (CurrentState != State.Start)
            {
                if (_current == null)
                {
                    return false;
                }

                if (_current is JContainer container && _parent != container)
                {
                    return ReadInto(container);
                }
                else
                {
                    return ReadOver(_current);
                }
            }
            if (_current == _root)
            {
                return false;
            }

            _current = _root;
            SetToken(_current);
            return true;
        }

        private bool ReadOver(JToken t)
        {
            if (t == _root)
            {
                return ReadToEnd();
            }

            JToken? next = t.Next;
            if ((next == null || next == t) || t == t.Parent!.Last)
            {
                if (t.Parent == null)
                {
                    return ReadToEnd();
                }

                return SetEnd(t.Parent);
            }
            else
            {
                _current = next;
                SetToken(_current);
                return true;
            }
        }

        private bool ReadToEnd()
        {
            _current = null;
            SetToken(JsonToken.None);
            return false;
        }

        private JsonToken? GetEndToken(JContainer c)
        {
            switch (c.Type)
            {
                case JTokenType.Object:
                    return JsonToken.EndObject;
                case JTokenType.Array:
                    return JsonToken.EndArray;
                case JTokenType.Constructor:
                    return JsonToken.EndConstructor;
                case JTokenType.Property:
                    return null;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(c.Type), c.Type, "Unexpected JContainer type.");
            }
        }

        private bool ReadInto(JContainer c)
        {
            JToken? firstChild = c.First;
            if (firstChild == null)
            {
                return SetEnd(c);
            }
            else
            {
                SetToken(firstChild);
                _current = firstChild;
                _parent = c;
                return true;
            }
        }

        private bool SetEnd(JContainer c)
        {
            JsonToken? endToken = GetEndToken(c);
            if (endToken != null)
            {
                SetToken(endToken.GetValueOrDefault());
                _current = c;
                _parent = c;
                return true;
            }
            else
            {
                return ReadOver(c);
            }
        }

        private void SetToken(JToken token)
        {
            switch (token.Type)
            {
                case JTokenType.Object:
                    SetToken(JsonToken.StartObject);
                    break;
                case JTokenType.Array:
                    SetToken(JsonToken.StartArray);
                    break;
                case JTokenType.Constructor:
                    SetToken(JsonToken.StartConstructor, ((JConstructor)token).Name);
                    break;
                case JTokenType.Property:
                    SetToken(JsonToken.PropertyName, ((JProperty)token).Name);
                    break;
                case JTokenType.Comment:
                    SetToken(JsonToken.Comment, ((JValue)token).Value);
                    break;
                case JTokenType.Integer:
                    SetToken(JsonToken.Integer, ((JValue)token).Value);
                    break;
                case JTokenType.Float:
                    SetToken(JsonToken.Float, ((JValue)token).Value);
                    break;
                case JTokenType.String:
                    SetToken(JsonToken.String, ((JValue)token).Value);
                    break;
                case JTokenType.Boolean:
                    SetToken(JsonToken.Boolean, ((JValue)token).Value);
                    break;
                case JTokenType.Null:
                    SetToken(JsonToken.Null, ((JValue)token).Value);
                    break;
                case JTokenType.Undefined:
                    SetToken(JsonToken.Undefined, ((JValue)token).Value);
                    break;
                case JTokenType.Date:
                    {
                        object? v = ((JValue)token).Value;
                        if (v is DateTime dt)
                        {
                            v = DateTimeUtils.EnsureDateTime(dt, DateTimeZoneHandling);
                        }

                        SetToken(JsonToken.Date, v);
                        break;
                    }
                case JTokenType.Raw:
                    SetToken(JsonToken.Raw, ((JValue)token).Value);
                    break;
                case JTokenType.Bytes:
                    SetToken(JsonToken.Bytes, ((JValue)token).Value);
                    break;
                case JTokenType.Guid:
                    SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
                    break;
                case JTokenType.Uri:
                    {
                        object? v = ((JValue)token).Value;
                        SetToken(JsonToken.String, v is Uri uri ? uri.OriginalString : SafeToString(v));
                        break;
                    }
                case JTokenType.TimeSpan:
                    SetToken(JsonToken.String, SafeToString(((JValue)token).Value));
                    break;
                default:
                    throw MiscellaneousUtils.CreateArgumentOutOfRangeException(nameof(token.Type), token.Type, "Unexpected JTokenType.");
            }
        }

        private string? SafeToString(object? value)
        {
            return value?.ToString();
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            if (CurrentState == State.Start)
            {
                return false;
            }

            IJsonLineInfo? info = _current;
            return (info != null && info.HasLineInfo());
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                if (CurrentState == State.Start)
                {
                    return 0;
                }

                IJsonLineInfo? info = _current;
                if (info != null)
                {
                    return info.LineNumber;
                }

                return 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                if (CurrentState == State.Start)
                {
                    return 0;
                }

                IJsonLineInfo? info = _current;
                if (info != null)
                {
                    return info.LinePosition;
                }

                return 0;
            }
        }
        public override string Path
        {
            get
            {
                string path = base.Path;

                if (_initialPath == null)
                {
                    _initialPath = _root.Path;
                }

                if (!StringUtils.IsNullOrEmpty(_initialPath))
                {
                    if (StringUtils.IsNullOrEmpty(path))
                    {
                        return _initialPath;
                    }

                    if (path.StartsWith('['))
                    {
                        path = _initialPath + path;
                    }
                    else
                    {
                        path = _initialPath + "." + path;
                    }
                }

                return path;
            }
        }
    }
}