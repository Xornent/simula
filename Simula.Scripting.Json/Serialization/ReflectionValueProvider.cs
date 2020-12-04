
using Simula.Scripting.Json.Utilities;
using System;
using System.Globalization;
using System.Reflection;

namespace Simula.Scripting.Json.Serialization
{
    public class ReflectionValueProvider : IValueProvider
    {
        private readonly MemberInfo _memberInfo;
        public ReflectionValueProvider(MemberInfo memberInfo)
        {
            ValidationUtils.ArgumentNotNull(memberInfo, nameof(memberInfo));
            _memberInfo = memberInfo;
        }
        public void SetValue(object target, object? value)
        {
            try {
                ReflectionUtils.SetMemberValue(_memberInfo, target, value);
            } catch (Exception ex) {
                throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
            }
        }
        public object? GetValue(object target)
        {
            try {
                if (_memberInfo is PropertyInfo propertyInfo && propertyInfo.PropertyType.IsByRef) {
                    throw new InvalidOperationException("Could not create getter for {0}. ByRef return values are not supported.".FormatWith(CultureInfo.InvariantCulture, propertyInfo));
                }

                return ReflectionUtils.GetMemberValue(_memberInfo, target);
            } catch (Exception ex) {
                throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
            }
        }
    }
}