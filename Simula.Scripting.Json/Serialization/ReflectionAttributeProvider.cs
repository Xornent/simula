using System;
using System.Collections.Generic;
using System.Reflection;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Serialization
{
    /// <summary>
    /// Provides methods to get attributes from a <see cref="System.Type"/>, <see cref="MemberInfo"/>, <see cref="ParameterInfo"/> or <see cref="Assembly"/>.
    /// </summary>
    public class ReflectionAttributeProvider : IAttributeProvider
    {
        private readonly object _attributeProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReflectionAttributeProvider"/> class.
        /// </summary>
        /// <param name="attributeProvider">The instance to get attributes for. This parameter should be a <see cref="System.Type"/>, <see cref="MemberInfo"/>, <see cref="ParameterInfo"/> or <see cref="Assembly"/>.</param>
        public ReflectionAttributeProvider(object attributeProvider)
        {
            ValidationUtils.ArgumentNotNull(attributeProvider, nameof(attributeProvider));
            _attributeProvider = attributeProvider;
        }

        /// <summary>
        /// Returns a collection of all of the attributes, or an empty collection if there are no attributes.
        /// </summary>
        /// <param name="inherit">When <c>true</c>, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>A collection of <see cref="Attribute"/>s, or an empty collection.</returns>
        public IList<Attribute> GetAttributes(bool inherit)
        {
            return ReflectionUtils.GetAttributes(_attributeProvider, null, inherit);
        }

        /// <summary>
        /// Returns a collection of attributes, identified by type, or an empty collection if there are no attributes.
        /// </summary>
        /// <param name="attributeType">The type of the attributes.</param>
        /// <param name="inherit">When <c>true</c>, look up the hierarchy chain for the inherited custom attribute.</param>
        /// <returns>A collection of <see cref="Attribute"/>s, or an empty collection.</returns>
        public IList<Attribute> GetAttributes(Type attributeType, bool inherit)
        {
            return ReflectionUtils.GetAttributes(_attributeProvider, attributeType, inherit);
        }
    }
}