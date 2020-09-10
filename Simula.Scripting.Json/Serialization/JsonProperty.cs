
using System;
using System.Diagnostics;
using System.Reflection;
using Simula.Scripting.Json.Utilities;

#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#endif

namespace Simula.Scripting.Json.Serialization
{
    public class JsonProperty
    {
        internal Required? _required;
        internal bool _hasExplicitDefaultValue;

        private object? _defaultValue;
        private bool _hasGeneratedDefaultValue;
        private string? _propertyName;
        internal bool _skipPropertyNameEscape;
        private Type? _propertyType;
        internal JsonContract? PropertyContract { get; set; }
        public string? PropertyName
        {
            get => _propertyName;
            set
            {
                _propertyName = value;
                _skipPropertyNameEscape = !JavaScriptUtils.ShouldEscapeJavaScriptString(_propertyName, JavaScriptUtils.HtmlCharEscapeFlags);
            }
        }
        public Type? DeclaringType { get; set; }
        public int? Order { get; set; }
        public string? UnderlyingName { get; set; }
        public IValueProvider? ValueProvider { get; set; }
        public IAttributeProvider? AttributeProvider { get; set; }
        public Type? PropertyType
        {
            get => _propertyType;
            set
            {
                if (_propertyType != value)
                {
                    _propertyType = value;
                    _hasGeneratedDefaultValue = false;
                }
            }
        }
        public JsonConverter? Converter { get; set; }
        [Obsolete("MemberConverter is obsolete. Use Converter instead.")]
        public JsonConverter? MemberConverter
        {
            get => Converter;
            set => Converter = value;
        }
        public bool Ignored { get; set; }
        public bool Readable { get; set; }
        public bool Writable { get; set; }
        public bool HasMemberAttribute { get; set; }
        public object? DefaultValue
        {
            get
            {
                if (!_hasExplicitDefaultValue)
                {
                    return null;
                }

                return _defaultValue;
            }
            set
            {
                _hasExplicitDefaultValue = true;
                _defaultValue = value;
            }
        }

        internal object? GetResolvedDefaultValue()
        {
            if (_propertyType == null)
            {
                return null;
            }

            if (!_hasExplicitDefaultValue && !_hasGeneratedDefaultValue)
            {
                _defaultValue = ReflectionUtils.GetDefaultValue(_propertyType);
                _hasGeneratedDefaultValue = true;
            }

            return _defaultValue;
        }
        public Required Required
        {
            get => _required ?? Required.Default;
            set => _required = value;
        }
        public bool IsRequiredSpecified => _required != null;
        public bool? IsReference { get; set; }
        public NullValueHandling? NullValueHandling { get; set; }
        public DefaultValueHandling? DefaultValueHandling { get; set; }
        public ReferenceLoopHandling? ReferenceLoopHandling { get; set; }
        public ObjectCreationHandling? ObjectCreationHandling { get; set; }
        public TypeNameHandling? TypeNameHandling { get; set; }
        public Predicate<object>? ShouldSerialize { get; set; }
        public Predicate<object>? ShouldDeserialize { get; set; }
        public Predicate<object>? GetIsSpecified { get; set; }
        public Action<object, object?>? SetIsSpecified { get; set; }
        public override string ToString()
        {
            return PropertyName ?? string.Empty;
        }
        public JsonConverter? ItemConverter { get; set; }
        public bool? ItemIsReference { get; set; }
        public TypeNameHandling? ItemTypeNameHandling { get; set; }
        public ReferenceLoopHandling? ItemReferenceLoopHandling { get; set; }

        internal void WritePropertyName(JsonWriter writer)
        {
            string? propertyName = PropertyName;
            MiscellaneousUtils.Assert(propertyName != null);

            if (_skipPropertyNameEscape)
            {
                writer.WritePropertyName(propertyName, false);
            }
            else
            {
                writer.WritePropertyName(propertyName);
            }
        }
    }
}