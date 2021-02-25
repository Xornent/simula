using System;
using Simula.Scripting.Json.Utilities;
using System.Globalization;

namespace Simula.Scripting.Json
{
    /// <summary>
    /// Instructs the <see cref="JsonSerializer"/> to use the specified <see cref="JsonConverter"/> when serializing the member or class.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface | AttributeTargets.Enum | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class JsonConverterAttribute : Attribute
    {
        private readonly Type _converterType;

        /// <summary>
        /// Gets the <see cref="Type"/> of the <see cref="JsonConverter"/>.
        /// </summary>
        /// <value>The <see cref="Type"/> of the <see cref="JsonConverter"/>.</value>
        public Type ConverterType
        {
            get { return _converterType; }
        }

        /// <summary>
        /// The parameter list to use when constructing the <see cref="JsonConverter"/> described by <see cref="ConverterType"/>.
        /// If <c>null</c>, the default constructor is used.
        /// </summary>
        public object[] ConverterParameters { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the <see cref="JsonConverter"/>.</param>
        public JsonConverterAttribute(Type converterType)
        {
            if (converterType == null)
            {
                throw new ArgumentNullException(nameof(converterType));
            }

            _converterType = converterType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonConverterAttribute"/> class.
        /// </summary>
        /// <param name="converterType">Type of the <see cref="JsonConverter"/>.</param>
        /// <param name="converterParameters">Parameter list to use when constructing the <see cref="JsonConverter"/>. Can be <c>null</c>.</param>
        public JsonConverterAttribute(Type converterType, params object[] converterParameters)
            : this(converterType)
        {
            ConverterParameters = converterParameters;
        }
    }
}