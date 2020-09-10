
using System;
using System.Collections.Generic;
using Simula.Scripting.Json.Linq.JsonPath;
#if HAVE_DYNAMIC
using System.Dynamic;
using System.Linq.Expressions;
#endif
using System.IO;
#if HAVE_BIG_INTEGER
using System.Numerics;
#endif
using Simula.Scripting.Json.Utilities;
using System.Diagnostics;
using System.Globalization;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#else
using System.Linq;
#endif

namespace Simula.Scripting.Json.Linq
{
    public abstract partial class JToken : IJEnumerable<JToken>, IJsonLineInfo
#if HAVE_ICLONEABLE
        , ICloneable
#endif
#if HAVE_DYNAMIC
        , IDynamicMetaObjectProvider
#endif
    {
        private static JTokenEqualityComparer? _equalityComparer;

        private JContainer? _parent;
        private JToken? _previous;
        private JToken? _next;
        private object? _annotations;

        private static readonly JTokenType[] BooleanTypes = new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Boolean };
        private static readonly JTokenType[] NumberTypes = new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Boolean };
#if HAVE_BIG_INTEGER
        private static readonly JTokenType[] BigIntegerTypes = new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Boolean, JTokenType.Bytes };
#endif
        private static readonly JTokenType[] StringTypes = new[] { JTokenType.Date, JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Boolean, JTokenType.Bytes, JTokenType.Guid, JTokenType.TimeSpan, JTokenType.Uri };
        private static readonly JTokenType[] GuidTypes = new[] { JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Guid, JTokenType.Bytes };
        private static readonly JTokenType[] TimeSpanTypes = new[] { JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.TimeSpan };
        private static readonly JTokenType[] UriTypes = new[] { JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Uri };
        private static readonly JTokenType[] CharTypes = new[] { JTokenType.Integer, JTokenType.Float, JTokenType.String, JTokenType.Comment, JTokenType.Raw };
        private static readonly JTokenType[] DateTimeTypes = new[] { JTokenType.Date, JTokenType.String, JTokenType.Comment, JTokenType.Raw };
        private static readonly JTokenType[] BytesTypes = new[] { JTokenType.Bytes, JTokenType.String, JTokenType.Comment, JTokenType.Raw, JTokenType.Integer };
        public static JTokenEqualityComparer EqualityComparer
        {
            get
            {
                if (_equalityComparer == null)
                {
                    _equalityComparer = new JTokenEqualityComparer();
                }

                return _equalityComparer;
            }
        }
        public JContainer? Parent
        {
            [DebuggerStepThrough]
            get { return _parent; }
            internal set { _parent = value; }
        }
        public JToken Root
        {
            get
            {
                JContainer? parent = Parent;
                if (parent == null)
                {
                    return this;
                }

                while (parent.Parent != null)
                {
                    parent = parent.Parent;
                }

                return parent;
            }
        }

        internal abstract JToken CloneToken();
        internal abstract bool DeepEquals(JToken node);
        public abstract JTokenType Type { get; }
        public abstract bool HasValues { get; }
        public static bool DeepEquals(JToken? t1, JToken? t2)
        {
            return (t1 == t2 || (t1 != null && t2 != null && t1.DeepEquals(t2)));
        }
        public JToken? Next
        {
            get => _next;
            internal set => _next = value;
        }
        public JToken? Previous
        {
            get => _previous;
            internal set => _previous = value;
        }
        public string Path
        {
            get
            {
                if (Parent == null)
                {
                    return string.Empty;
                }

                List<JsonPosition> positions = new List<JsonPosition>();
                JToken? previous = null;
                for (JToken? current = this; current != null; current = current.Parent)
                {
                    switch (current.Type)
                    {
                        case JTokenType.Property:
                            JProperty property = (JProperty)current;
                            positions.Add(new JsonPosition(JsonContainerType.Object) { PropertyName = property.Name });
                            break;
                        case JTokenType.Array:
                        case JTokenType.Constructor:
                            if (previous != null)
                            {
                                int index = ((IList<JToken>)current).IndexOf(previous);

                                positions.Add(new JsonPosition(JsonContainerType.Array) { Position = index });
                            }
                            break;
                    }

                    previous = current;
                }

#if HAVE_FAST_REVERSE
                positions.FastReverse();
#else
                positions.Reverse();
#endif

                return JsonPosition.BuildPath(positions, null);
            }
        }

        internal JToken()
        {
        }
        public void AddAfterSelf(object? content)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            int index = _parent.IndexOfItem(this);
            _parent.AddInternal(index + 1, content, false);
        }
        public void AddBeforeSelf(object? content)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            int index = _parent.IndexOfItem(this);
            _parent.AddInternal(index, content, false);
        }
        public IEnumerable<JToken> Ancestors()
        {
            return GetAncestors(false);
        }
        public IEnumerable<JToken> AncestorsAndSelf()
        {
            return GetAncestors(true);
        }

        internal IEnumerable<JToken> GetAncestors(bool self)
        {
            for (JToken? current = self ? this : Parent; current != null; current = current.Parent)
            {
                yield return current;
            }
        }
        public IEnumerable<JToken> AfterSelf()
        {
            if (Parent == null)
            {
                yield break;
            }

            for (JToken? o = Next; o != null; o = o.Next)
            {
                yield return o;
            }
        }
        public IEnumerable<JToken> BeforeSelf()
        {
            if (Parent == null)
            {
                yield break;
            }

            for (JToken? o = Parent.First; o != this && o != null; o = o.Next)
            {
                yield return o;
            }
        }
        public virtual JToken? this[object key]
        {
            get => throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
            set => throw new InvalidOperationException("Cannot set child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
        }
        public virtual T Value<T>(object key)
        {
            JToken? token = this[key];
            return token == null ? default : Extensions.Convert<JToken, T>(token);
        }
        public virtual JToken? First => throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
        public virtual JToken? Last => throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
        public virtual JEnumerable<JToken> Children()
        {
            return JEnumerable<JToken>.Empty;
        }
        public JEnumerable<T> Children<T>() where T : JToken
        {
            return new JEnumerable<T>(Children().OfType<T>());
        }
        public virtual IEnumerable<T> Values<T>()
        {
            throw new InvalidOperationException("Cannot access child value on {0}.".FormatWith(CultureInfo.InvariantCulture, GetType()));
        }
        public void Remove()
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            _parent.RemoveItem(this);
        }
        public void Replace(JToken value)
        {
            if (_parent == null)
            {
                throw new InvalidOperationException("The parent is missing.");
            }

            _parent.ReplaceItem(this, value);
        }
        public abstract void WriteTo(JsonWriter writer, params JsonConverter[] converters);
        public override string ToString()
        {
            return ToString(Formatting.Indented);
        }
        public string ToString(Formatting formatting, params JsonConverter[] converters)
        {
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            {
                JsonTextWriter jw = new JsonTextWriter(sw);
                jw.Formatting = formatting;

                WriteTo(jw, converters);

                return sw.ToString();
            }
        }

        private static JValue? EnsureValue(JToken value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (value is JProperty property)
            {
                value = property.Value;
            }

            JValue? v = value as JValue;

            return v;
        }

        private static string GetType(JToken token)
        {
            ValidationUtils.ArgumentNotNull(token, nameof(token));

            if (token is JProperty p)
            {
                token = p.Value;
            }

            return token.Type.ToString();
        }

        private static bool ValidateToken(JToken o, JTokenType[] validTypes, bool nullable)
        {
            return (Array.IndexOf(validTypes, o.Type) != -1) || (nullable && (o.Type == JTokenType.Null || o.Type == JTokenType.Undefined));
        }

#region Cast from operators
        public static explicit operator bool(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BooleanTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return Convert.ToBoolean((int)integer);
            }
#endif

            return Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture);
        }

#if HAVE_DATE_TIME_OFFSET
        public static explicit operator DateTimeOffset(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value is DateTimeOffset offset)
            {
                return offset;
            }

            if (v.Value is string s)
            {
                return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
            }

            return new DateTimeOffset(Convert.ToDateTime(v.Value, CultureInfo.InvariantCulture));
        }
#endif
        public static explicit operator bool?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BooleanTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Boolean.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return Convert.ToBoolean((int)integer);
            }
#endif

            return (v.Value != null) ? (bool?)Convert.ToBoolean(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator long(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (long)integer;
            }
#endif

            return Convert.ToInt64(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator DateTime?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_DATE_TIME_OFFSET
            if (v.Value is DateTimeOffset offset)
            {
                return offset.DateTime;
            }
#endif

            return (v.Value != null) ? (DateTime?)Convert.ToDateTime(v.Value, CultureInfo.InvariantCulture) : null;
        }

#if HAVE_DATE_TIME_OFFSET
        public static explicit operator DateTimeOffset?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to DateTimeOffset.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value == null)
            {
                return null;
            }
            if (v.Value is DateTimeOffset offset)
            {
                return offset;
            }

            if (v.Value is string s)
            {
                return DateTimeOffset.Parse(s, CultureInfo.InvariantCulture);
            }

            return new DateTimeOffset(Convert.ToDateTime(v.Value, CultureInfo.InvariantCulture));
        }
#endif
        public static explicit operator decimal?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (decimal?)integer;
            }
#endif

            return (v.Value != null) ? (decimal?)Convert.ToDecimal(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator double?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (double?)integer;
            }
#endif

            return (v.Value != null) ? (double?)Convert.ToDouble(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator char?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, CharTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Char.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (char?)integer;
            }
#endif

            return (v.Value != null) ? (char?)Convert.ToChar(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator int(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (int)integer;
            }
#endif

            return Convert.ToInt32(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator short(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (short)integer;
            }
#endif

            return Convert.ToInt16(v.Value, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static explicit operator ushort(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (ushort)integer;
            }
#endif

            return Convert.ToUInt16(v.Value, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static explicit operator char(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, CharTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Char.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (char)integer;
            }
#endif

            return Convert.ToChar(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator byte(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Byte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (byte)integer;
            }
#endif

            return Convert.ToByte(v.Value, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static explicit operator sbyte(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to SByte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (sbyte)integer;
            }
#endif

            return Convert.ToSByte(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator int?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Int32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (int?)integer;
            }
#endif

            return (v.Value != null) ? (int?)Convert.ToInt32(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator short?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Int16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (short?)integer;
            }
#endif

            return (v.Value != null) ? (short?)Convert.ToInt16(v.Value, CultureInfo.InvariantCulture) : null;
        }
        [CLSCompliant(false)]
        public static explicit operator ushort?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to UInt16.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (ushort?)integer;
            }
#endif

            return (v.Value != null) ? (ushort?)Convert.ToUInt16(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator byte?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Byte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (byte?)integer;
            }
#endif

            return (v.Value != null) ? (byte?)Convert.ToByte(v.Value, CultureInfo.InvariantCulture) : null;
        }
        [CLSCompliant(false)]
        public static explicit operator sbyte?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to SByte.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (sbyte?)integer;
            }
#endif

            return (v.Value != null) ? (sbyte?)Convert.ToSByte(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator DateTime(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, DateTimeTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to DateTime.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_DATE_TIME_OFFSET
            if (v.Value is DateTimeOffset offset)
            {
                return offset.DateTime;
            }
#endif

            return Convert.ToDateTime(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator long?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Int64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (long?)integer;
            }
#endif

            return (v.Value != null) ? (long?)Convert.ToInt64(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator float?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (float?)integer;
            }
#endif

            return (v.Value != null) ? (float?)Convert.ToSingle(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator decimal(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Decimal.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (decimal)integer;
            }
#endif

            return Convert.ToDecimal(v.Value, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static explicit operator uint?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (uint?)integer;
            }
#endif

            return (v.Value != null) ? (uint?)Convert.ToUInt32(v.Value, CultureInfo.InvariantCulture) : null;
        }
        [CLSCompliant(false)]
        public static explicit operator ulong?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (ulong?)integer;
            }
#endif

            return (v.Value != null) ? (ulong?)Convert.ToUInt64(v.Value, CultureInfo.InvariantCulture) : null;
        }
        public static explicit operator double(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Double.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (double)integer;
            }
#endif

            return Convert.ToDouble(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator float(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Single.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (float)integer;
            }
#endif

            return Convert.ToSingle(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator string?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, StringTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to String.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value == null)
            {
                return null;
            }

            if (v.Value is byte[] bytes)
            {
                return Convert.ToBase64String(bytes);
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return integer.ToString(CultureInfo.InvariantCulture);
            }
#endif

            return Convert.ToString(v.Value, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static explicit operator uint(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to UInt32.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (uint)integer;
            }
#endif

            return Convert.ToUInt32(v.Value, CultureInfo.InvariantCulture);
        }
        [CLSCompliant(false)]
        public static explicit operator ulong(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, NumberTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to UInt64.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return (ulong)integer;
            }
#endif

            return Convert.ToUInt64(v.Value, CultureInfo.InvariantCulture);
        }
        public static explicit operator byte[]?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BytesTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value is string)
            {
                return Convert.FromBase64String(Convert.ToString(v.Value, CultureInfo.InvariantCulture));
            }
#if HAVE_BIG_INTEGER
            if (v.Value is BigInteger integer)
            {
                return integer.ToByteArray();
            }
#endif

            if (v.Value is byte[] bytes)
            {
                return bytes;
            }

            throw new ArgumentException("Can not convert {0} to byte array.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
        }
        public static explicit operator Guid(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, GuidTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to Guid.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value is byte[] bytes)
            {
                return new Guid(bytes);
            }

            return (v.Value is Guid guid) ? guid : new Guid(Convert.ToString(v.Value, CultureInfo.InvariantCulture));
        }
        public static explicit operator Guid?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, GuidTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Guid.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value == null)
            {
                return null;
            }

            if (v.Value is byte[] bytes)
            {
                return new Guid(bytes);
            }

            return (v.Value is Guid guid) ? guid : new Guid(Convert.ToString(v.Value, CultureInfo.InvariantCulture));
        }
        public static explicit operator TimeSpan(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, TimeSpanTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to TimeSpan.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            return (v.Value is TimeSpan span) ? span : ConvertUtils.ParseTimeSpan(Convert.ToString(v.Value, CultureInfo.InvariantCulture));
        }
        public static explicit operator TimeSpan?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, TimeSpanTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to TimeSpan.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value == null)
            {
                return null;
            }

            return (v.Value is TimeSpan span) ? span : ConvertUtils.ParseTimeSpan(Convert.ToString(v.Value, CultureInfo.InvariantCulture));
        }
        public static explicit operator Uri?(JToken? value)
        {
            if (value == null)
            {
                return null;
            }

            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, UriTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to Uri.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value == null)
            {
                return null;
            }

            return (v.Value is Uri uri) ? uri : new Uri(Convert.ToString(v.Value, CultureInfo.InvariantCulture));
        }

#if HAVE_BIG_INTEGER
        private static BigInteger ToBigInteger(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BigIntegerTypes, false))
            {
                throw new ArgumentException("Can not convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            return ConvertUtils.ToBigInteger(v.Value!);
        }

        private static BigInteger? ToBigIntegerNullable(JToken value)
        {
            JValue? v = EnsureValue(value);
            if (v == null || !ValidateToken(v, BigIntegerTypes, true))
            {
                throw new ArgumentException("Can not convert {0} to BigInteger.".FormatWith(CultureInfo.InvariantCulture, GetType(value)));
            }

            if (v.Value == null)
            {
                return null;
            }

            return ConvertUtils.ToBigInteger(v.Value);
        }
#endif
#endregion

#region Cast to operators
        public static implicit operator JToken(bool value)
        {
            return new JValue(value);
        }

#if HAVE_DATE_TIME_OFFSET
        public static implicit operator JToken(DateTimeOffset value)
        {
            return new JValue(value);
        }
#endif
        public static implicit operator JToken(byte value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(byte? value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(sbyte value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(sbyte? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(bool? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(long value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(DateTime? value)
        {
            return new JValue(value);
        }

#if HAVE_DATE_TIME_OFFSET
        public static implicit operator JToken(DateTimeOffset? value)
        {
            return new JValue(value);
        }
#endif
        public static implicit operator JToken(decimal? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(double? value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(short value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(ushort value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(int value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(int? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(DateTime value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(long? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(float? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(decimal value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(short? value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(ushort? value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(uint? value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(ulong? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(double value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(float value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(string? value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(uint value)
        {
            return new JValue(value);
        }
        [CLSCompliant(false)]
        public static implicit operator JToken(ulong value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(byte[] value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(Uri? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(TimeSpan value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(TimeSpan? value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(Guid value)
        {
            return new JValue(value);
        }
        public static implicit operator JToken(Guid? value)
        {
            return new JValue(value);
        }
#endregion

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable<JToken>)this).GetEnumerator();
        }

        IEnumerator<JToken> IEnumerable<JToken>.GetEnumerator()
        {
            return Children().GetEnumerator();
        }

        internal abstract int GetDeepHashCode();

        IJEnumerable<JToken> IJEnumerable<JToken>.this[object key] => this[key]!;
        public JsonReader CreateReader()
        {
            return new JTokenReader(this);
        }

        internal static JToken FromObjectInternal(object o, JsonSerializer jsonSerializer)
        {
            ValidationUtils.ArgumentNotNull(o, nameof(o));
            ValidationUtils.ArgumentNotNull(jsonSerializer, nameof(jsonSerializer));

            JToken token;
            using (JTokenWriter jsonWriter = new JTokenWriter())
            {
                jsonSerializer.Serialize(jsonWriter, o);
                token = jsonWriter.Token!;
            }

            return token;
        }
        public static JToken FromObject(object o)
        {
            return FromObjectInternal(o, JsonSerializer.CreateDefault());
        }
        public static JToken FromObject(object o, JsonSerializer jsonSerializer)
        {
            return FromObjectInternal(o, jsonSerializer);
        }
        [return: MaybeNull]
        public T ToObject<T>()
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return (T)ToObject(typeof(T));
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        public object? ToObject(Type objectType)
        {
            if (JsonConvert.DefaultSettings == null)
            {
                PrimitiveTypeCode typeCode = ConvertUtils.GetTypeCode(objectType, out bool isEnum);

                if (isEnum)
                {
                    if (Type == JTokenType.String)
                    {
                        try
                        {
                            return ToObject(objectType, JsonSerializer.CreateDefault());
                        }
                        catch (Exception ex)
                        {
                            Type enumType = objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType);
                            throw new ArgumentException("Could not convert '{0}' to {1}.".FormatWith(CultureInfo.InvariantCulture, (string?)this, enumType.Name), ex);
                        }
                    }

                    if (Type == JTokenType.Integer)
                    {
                        Type enumType = objectType.IsEnum() ? objectType : Nullable.GetUnderlyingType(objectType);
                        return Enum.ToObject(enumType, ((JValue)this).Value);
                    }
                }

                switch (typeCode)
                {
                    case PrimitiveTypeCode.BooleanNullable:
                        return (bool?)this;
                    case PrimitiveTypeCode.Boolean:
                        return (bool)this;
                    case PrimitiveTypeCode.CharNullable:
                        return (char?)this;
                    case PrimitiveTypeCode.Char:
                        return (char)this;
                    case PrimitiveTypeCode.SByte:
                        return (sbyte)this;
                    case PrimitiveTypeCode.SByteNullable:
                        return (sbyte?)this;
                    case PrimitiveTypeCode.ByteNullable:
                        return (byte?)this;
                    case PrimitiveTypeCode.Byte:
                        return (byte)this;
                    case PrimitiveTypeCode.Int16Nullable:
                        return (short?)this;
                    case PrimitiveTypeCode.Int16:
                        return (short)this;
                    case PrimitiveTypeCode.UInt16Nullable:
                        return (ushort?)this;
                    case PrimitiveTypeCode.UInt16:
                        return (ushort)this;
                    case PrimitiveTypeCode.Int32Nullable:
                        return (int?)this;
                    case PrimitiveTypeCode.Int32:
                        return (int)this;
                    case PrimitiveTypeCode.UInt32Nullable:
                        return (uint?)this;
                    case PrimitiveTypeCode.UInt32:
                        return (uint)this;
                    case PrimitiveTypeCode.Int64Nullable:
                        return (long?)this;
                    case PrimitiveTypeCode.Int64:
                        return (long)this;
                    case PrimitiveTypeCode.UInt64Nullable:
                        return (ulong?)this;
                    case PrimitiveTypeCode.UInt64:
                        return (ulong)this;
                    case PrimitiveTypeCode.SingleNullable:
                        return (float?)this;
                    case PrimitiveTypeCode.Single:
                        return (float)this;
                    case PrimitiveTypeCode.DoubleNullable:
                        return (double?)this;
                    case PrimitiveTypeCode.Double:
                        return (double)this;
                    case PrimitiveTypeCode.DecimalNullable:
                        return (decimal?)this;
                    case PrimitiveTypeCode.Decimal:
                        return (decimal)this;
                    case PrimitiveTypeCode.DateTimeNullable:
                        return (DateTime?)this;
                    case PrimitiveTypeCode.DateTime:
                        return (DateTime)this;
#if HAVE_DATE_TIME_OFFSET
                    case PrimitiveTypeCode.DateTimeOffsetNullable:
                        return (DateTimeOffset?)this;
                    case PrimitiveTypeCode.DateTimeOffset:
                        return (DateTimeOffset)this;
#endif
                    case PrimitiveTypeCode.String:
                        return (string?)this;
                    case PrimitiveTypeCode.GuidNullable:
                        return (Guid?)this;
                    case PrimitiveTypeCode.Guid:
                        return (Guid)this;
                    case PrimitiveTypeCode.Uri:
                        return (Uri?)this;
                    case PrimitiveTypeCode.TimeSpanNullable:
                        return (TimeSpan?)this;
                    case PrimitiveTypeCode.TimeSpan:
                        return (TimeSpan)this;
#if HAVE_BIG_INTEGER
                    case PrimitiveTypeCode.BigIntegerNullable:
                        return ToBigIntegerNullable(this);
                    case PrimitiveTypeCode.BigInteger:
                        return ToBigInteger(this);
#endif
                }
            }

            return ToObject(objectType, JsonSerializer.CreateDefault());
        }
        [return: MaybeNull]
        public T ToObject<T>(JsonSerializer jsonSerializer)
        {
#pragma warning disable CS8601 // Possible null reference assignment.
            return (T)ToObject(typeof(T), jsonSerializer);
#pragma warning restore CS8601 // Possible null reference assignment.
        }
        public object? ToObject(Type objectType, JsonSerializer jsonSerializer)
        {
            ValidationUtils.ArgumentNotNull(jsonSerializer, nameof(jsonSerializer));

            using (JTokenReader jsonReader = new JTokenReader(this))
            {
                return jsonSerializer.Deserialize(jsonReader, objectType);
            }
        }
        public static JToken ReadFrom(JsonReader reader)
        {
            return ReadFrom(reader, null);
        }
        public static JToken ReadFrom(JsonReader reader, JsonLoadSettings? settings)
        {
            ValidationUtils.ArgumentNotNull(reader, nameof(reader));

            bool hasContent;
            if (reader.TokenType == JsonToken.None)
            {
                hasContent = (settings != null && settings.CommentHandling == CommentHandling.Ignore)
                    ? reader.ReadAndMoveToContent()
                    : reader.Read();
            }
            else if (reader.TokenType == JsonToken.Comment && settings?.CommentHandling == CommentHandling.Ignore)
            {
                hasContent = reader.ReadAndMoveToContent();
            }
            else
            {
                hasContent = true;
            }

            if (!hasContent)
            {
                throw JsonReaderException.Create(reader, "Error reading JToken from JsonReader.");
            }

            IJsonLineInfo? lineInfo = reader as IJsonLineInfo;

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return JObject.Load(reader, settings);
                case JsonToken.StartArray:
                    return JArray.Load(reader, settings);
                case JsonToken.StartConstructor:
                    return JConstructor.Load(reader, settings);
                case JsonToken.PropertyName:
                    return JProperty.Load(reader, settings);
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
                    v = JValue.CreateComment(reader.Value!.ToString());
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
        public static JToken Parse(string json)
        {
            return Parse(json, null);
        }
        public static JToken Parse(string json, JsonLoadSettings? settings)
        {
            using (JsonReader reader = new JsonTextReader(new StringReader(json)))
            {
                JToken t = Load(reader, settings);

                while (reader.Read())
                {
                }

                return t;
            }
        }
        public static JToken Load(JsonReader reader, JsonLoadSettings? settings)
        {
            return ReadFrom(reader, settings);
        }
        public static JToken Load(JsonReader reader)
        {
            return Load(reader, null);
        }

        internal void SetLineInfo(IJsonLineInfo? lineInfo, JsonLoadSettings? settings)
        {
            if (settings != null && settings.LineInfoHandling != LineInfoHandling.Load)
            {
                return;
            }

            if (lineInfo == null || !lineInfo.HasLineInfo())
            {
                return;
            }

            SetLineInfo(lineInfo.LineNumber, lineInfo.LinePosition);
        }

        private class LineInfoAnnotation
        {
            internal readonly int LineNumber;
            internal readonly int LinePosition;

            public LineInfoAnnotation(int lineNumber, int linePosition)
            {
                LineNumber = lineNumber;
                LinePosition = linePosition;
            }
        }

        internal void SetLineInfo(int lineNumber, int linePosition)
        {
            AddAnnotation(new LineInfoAnnotation(lineNumber, linePosition));
        }

        bool IJsonLineInfo.HasLineInfo()
        {
            return (Annotation<LineInfoAnnotation>() != null);
        }

        int IJsonLineInfo.LineNumber
        {
            get
            {
                LineInfoAnnotation? annotation = Annotation<LineInfoAnnotation>();
                if (annotation != null)
                {
                    return annotation.LineNumber;
                }

                return 0;
            }
        }

        int IJsonLineInfo.LinePosition
        {
            get
            {
                LineInfoAnnotation? annotation = Annotation<LineInfoAnnotation>();
                if (annotation != null)
                {
                    return annotation.LinePosition;
                }

                return 0;
            }
        }
        public JToken? SelectToken(string path)
        {
            return SelectToken(path, false);
        }
        public JToken? SelectToken(string path, bool errorWhenNoMatch)
        {
            JPath p = new JPath(path);

            JToken? token = null;
            foreach (JToken t in p.Evaluate(this, this, errorWhenNoMatch))
            {
                if (token != null)
                {
                    throw new JsonException("Path returned multiple tokens.");
                }

                token = t;
            }

            return token;
        }
        public IEnumerable<JToken> SelectTokens(string path)
        {
            return SelectTokens(path, false);
        }
        public IEnumerable<JToken> SelectTokens(string path, bool errorWhenNoMatch)
        {
            JPath p = new JPath(path);
            return p.Evaluate(this, this, errorWhenNoMatch);
        }

#if HAVE_DYNAMIC
        protected virtual DynamicMetaObject GetMetaObject(Expression parameter)
        {
            return new DynamicProxyMetaObject<JToken>(parameter, this, new DynamicProxy<JToken>());
        }
        DynamicMetaObject IDynamicMetaObjectProvider.GetMetaObject(Expression parameter)
        {
            return GetMetaObject(parameter);
        }
#endif

#if HAVE_ICLONEABLE
        object ICloneable.Clone()
        {
            return DeepClone();
        }
#endif
        public JToken DeepClone()
        {
            return CloneToken();
        }
        public void AddAnnotation(object annotation)
        {
            if (annotation == null)
            {
                throw new ArgumentNullException(nameof(annotation));
            }

            if (_annotations == null)
            {
                _annotations = (annotation is object[]) ? new[] { annotation } : annotation;
            }
            else
            {
                if (!(_annotations is object[] annotations))
                {
                    _annotations = new[] { _annotations, annotation };
                }
                else
                {
                    int index = 0;
                    while (index < annotations.Length && annotations[index] != null)
                    {
                        index++;
                    }
                    if (index == annotations.Length)
                    {
                        Array.Resize(ref annotations, index * 2);
                        _annotations = annotations;
                    }
                    annotations[index] = annotation;
                }
            }
        }
        public T? Annotation<T>() where T : class
        {
            if (_annotations != null)
            {
                if (!(_annotations is object[] annotations))
                {
                    return (_annotations as T);
                }
                for (int i = 0; i < annotations.Length; i++)
                {
                    object annotation = annotations[i];
                    if (annotation == null)
                    {
                        break;
                    }

                    if (annotation is T local)
                    {
                        return local;
                    }
                }
            }

            return default;
        }
        public object? Annotation(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_annotations != null)
            {
                if (!(_annotations is object[] annotations))
                {
                    if (type.IsInstanceOfType(_annotations))
                    {
                        return _annotations;
                    }
                }
                else
                {
                    for (int i = 0; i < annotations.Length; i++)
                    {
                        object o = annotations[i];
                        if (o == null)
                        {
                            break;
                        }

                        if (type.IsInstanceOfType(o))
                        {
                            return o;
                        }
                    }
                }
            }

            return null;
        }
        public IEnumerable<T> Annotations<T>() where T : class
        {
            if (_annotations == null)
            {
                yield break;
            }

            if (_annotations is object[] annotations)
            {
                for (int i = 0; i < annotations.Length; i++)
                {
                    object o = annotations[i];
                    if (o == null)
                    {
                        break;
                    }

                    if (o is T casted)
                    {
                        yield return casted;
                    }
                }
                yield break;
            }

            if (!(_annotations is T annotation))
            {
                yield break;
            }

            yield return annotation;
        }
        public IEnumerable<object> Annotations(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_annotations == null)
            {
                yield break;
            }

            if (_annotations is object[] annotations)
            {
                for (int i = 0; i < annotations.Length; i++)
                {
                    object o = annotations[i];
                    if (o == null)
                    {
                        break;
                    }

                    if (type.IsInstanceOfType(o))
                    {
                        yield return o;
                    }
                }
                yield break;
            }

            if (!type.IsInstanceOfType(_annotations))
            {
                yield break;
            }

            yield return _annotations;
        }
        public void RemoveAnnotations<T>() where T : class
        {
            if (_annotations != null)
            {
                if (!(_annotations is object?[] annotations))
                {
                    if (_annotations is T)
                    {
                        _annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int keepCount = 0;
                    while (index < annotations.Length)
                    {
                        object? obj2 = annotations[index];
                        if (obj2 == null)
                        {
                            break;
                        }

                        if (!(obj2 is T))
                        {
                            annotations[keepCount++] = obj2;
                        }

                        index++;
                    }

                    if (keepCount != 0)
                    {
                        while (keepCount < index)
                        {
                            annotations[keepCount++] = null;
                        }
                    }
                    else
                    {
                        _annotations = null;
                    }
                }
            }
        }
        public void RemoveAnnotations(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (_annotations != null)
            {
                if (!(_annotations is object?[] annotations))
                {
                    if (type.IsInstanceOfType(_annotations))
                    {
                        _annotations = null;
                    }
                }
                else
                {
                    int index = 0;
                    int keepCount = 0;
                    while (index < annotations.Length)
                    {
                        object? o = annotations[index];
                        if (o == null)
                        {
                            break;
                        }

                        if (!type.IsInstanceOfType(o))
                        {
                            annotations[keepCount++] = o;
                        }

                        index++;
                    }

                    if (keepCount != 0)
                    {
                        while (keepCount < index)
                        {
                            annotations[keepCount++] = null;
                        }
                    }
                    else
                    {
                        _annotations = null;
                    }
                }
            }
        }
    }
}