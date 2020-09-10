
using System;
using System.Collections.Generic;
using System.Reflection;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Serialization
{
    public class ReflectionAttributeProvider : IAttributeProvider
    {
        private readonly object _attributeProvider;
        public ReflectionAttributeProvider(object attributeProvider)
        {
            ValidationUtils.ArgumentNotNull(attributeProvider, nameof(attributeProvider));
            _attributeProvider = attributeProvider;
        }
        public IList<Attribute> GetAttributes(bool inherit)
        {
            return ReflectionUtils.GetAttributes(_attributeProvider, null, inherit);
        }
        public IList<Attribute> GetAttributes(Type attributeType, bool inherit)
        {
            return ReflectionUtils.GetAttributes(_attributeProvider, attributeType, inherit);
        }
    }
}