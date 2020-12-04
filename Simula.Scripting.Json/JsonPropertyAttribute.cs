
using System;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class JsonPropertyAttribute : Attribute
    {
        internal NullValueHandling? _nullValueHandling;
        internal DefaultValueHandling? _defaultValueHandling;
        internal ReferenceLoopHandling? _referenceLoopHandling;
        internal ObjectCreationHandling? _objectCreationHandling;
        internal TypeNameHandling? _typeNameHandling;
        internal bool? _isReference;
        internal int? _order;
        internal Required? _required;
        internal bool? _itemIsReference;
        internal ReferenceLoopHandling? _itemReferenceLoopHandling;
        internal TypeNameHandling? _itemTypeNameHandling;
        public Type? ItemConverterType { get; set; }
        public object[]? ItemConverterParameters { get; set; }
        public Type? NamingStrategyType { get; set; }
        public object[]? NamingStrategyParameters { get; set; }
        public NullValueHandling NullValueHandling {
            get => _nullValueHandling ?? default;
            set => _nullValueHandling = value;
        }
        public DefaultValueHandling DefaultValueHandling {
            get => _defaultValueHandling ?? default;
            set => _defaultValueHandling = value;
        }
        public ReferenceLoopHandling ReferenceLoopHandling {
            get => _referenceLoopHandling ?? default;
            set => _referenceLoopHandling = value;
        }
        public ObjectCreationHandling ObjectCreationHandling {
            get => _objectCreationHandling ?? default;
            set => _objectCreationHandling = value;
        }
        public TypeNameHandling TypeNameHandling {
            get => _typeNameHandling ?? default;
            set => _typeNameHandling = value;
        }
        public bool IsReference {
            get => _isReference ?? default;
            set => _isReference = value;
        }
        public int Order {
            get => _order ?? default;
            set => _order = value;
        }
        public Required Required {
            get => _required ?? Required.Default;
            set => _required = value;
        }
        public string? PropertyName { get; set; }
        public ReferenceLoopHandling ItemReferenceLoopHandling {
            get => _itemReferenceLoopHandling ?? default;
            set => _itemReferenceLoopHandling = value;
        }
        public TypeNameHandling ItemTypeNameHandling {
            get => _itemTypeNameHandling ?? default;
            set => _itemTypeNameHandling = value;
        }
        public bool ItemIsReference {
            get => _itemIsReference ?? default;
            set => _itemIsReference = value;
        }
        public JsonPropertyAttribute()
        {
        }
        public JsonPropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}