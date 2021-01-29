
using System;

namespace Simula.Scripting.Json
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class JsonConverterAttribute : Attribute
    {
        private readonly Type _converterType;
        public Type ConverterType => _converterType;
        public object[]? ConverterParameters { get; }
        public JsonConverterAttribute(Type converterType)
        {
            if (converterType == null) {
                throw new ArgumentNullException(nameof(converterType));
            }

            _converterType = converterType;
        }
        public JsonConverterAttribute(Type converterType, params object[] converterParameters)
            : this(converterType)
        {
            ConverterParameters = converterParameters;
        }
    }
}