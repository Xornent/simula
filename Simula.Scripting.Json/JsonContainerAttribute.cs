
using System;
using Simula.Scripting.Json.Serialization;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false)]
    public abstract class JsonContainerAttribute : Attribute
    {
        public string? Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public Type? ItemConverterType { get; set; }
        public object[]? ItemConverterParameters { get; set; }
        public Type? NamingStrategyType
        {
            get => _namingStrategyType;
            set
            {
                _namingStrategyType = value;
                NamingStrategyInstance = null;
            }
        }
        public object[]? NamingStrategyParameters
        {
            get => _namingStrategyParameters;
            set
            {
                _namingStrategyParameters = value;
                NamingStrategyInstance = null;
            }
        }

        internal NamingStrategy? NamingStrategyInstance { get; set; }
        internal bool? _isReference;
        internal bool? _itemIsReference;
        internal ReferenceLoopHandling? _itemReferenceLoopHandling;
        internal TypeNameHandling? _itemTypeNameHandling;
        private Type? _namingStrategyType;
        private object[]? _namingStrategyParameters;
        public bool IsReference
        {
            get => _isReference ?? default;
            set => _isReference = value;
        }
        public bool ItemIsReference
        {
            get => _itemIsReference ?? default;
            set => _itemIsReference = value;
        }
        public ReferenceLoopHandling ItemReferenceLoopHandling
        {
            get => _itemReferenceLoopHandling ?? default;
            set => _itemReferenceLoopHandling = value;
        }
        public TypeNameHandling ItemTypeNameHandling
        {
            get => _itemTypeNameHandling ?? default;
            set => _itemTypeNameHandling = value;
        }
        protected JsonContainerAttribute()
        {
        }
        protected JsonContainerAttribute(string id)
        {
            Id = id;
        }
    }
}