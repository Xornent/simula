
#if HAVE_REFLECTION_EMIT
using System;
using System.Collections.Generic;
#if !HAVE_LINQ
using Simula.Scripting.Json.Utilities.LinqBridge;
#endif
using System.Text;
using System.Reflection;
using Simula.Scripting.Json.Utilities;
using System.Globalization;

namespace Simula.Scripting.Json.Serialization
{
    public class DynamicValueProvider : IValueProvider
    {
        private readonly MemberInfo _memberInfo;
        private Func<object, object?>? _getter;
        private Action<object, object?>? _setter;
        public DynamicValueProvider(MemberInfo memberInfo)
        {
            ValidationUtils.ArgumentNotNull(memberInfo, nameof(memberInfo));
            _memberInfo = memberInfo;
        }
        public void SetValue(object target, object? value)
        {
            try
            {
                if (_setter == null)
                {
                    _setter = DynamicReflectionDelegateFactory.Instance.CreateSet<object>(_memberInfo);
                }

#if DEBUG
                if (value == null)
                {
                    if (!ReflectionUtils.IsNullable(ReflectionUtils.GetMemberUnderlyingType(_memberInfo)))
                    {
                        throw new JsonSerializationException("Incompatible value. Cannot set {0} to null.".FormatWith(CultureInfo.InvariantCulture, _memberInfo));
                    }
                }
                else if (!ReflectionUtils.GetMemberUnderlyingType(_memberInfo).IsAssignableFrom(value.GetType()))
                {
                    throw new JsonSerializationException("Incompatible value. Cannot set {0} to type {1}.".FormatWith(CultureInfo.InvariantCulture, _memberInfo, value.GetType()));
                }
#endif

                _setter(target, value);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Error setting value to '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
            }
        }
        public object? GetValue(object target)
        {
            try
            {
                if (_getter == null)
                {
                    _getter = DynamicReflectionDelegateFactory.Instance.CreateGet<object>(_memberInfo);
                }

                return _getter(target);
            }
            catch (Exception ex)
            {
                throw new JsonSerializationException("Error getting value from '{0}' on '{1}'.".FormatWith(CultureInfo.InvariantCulture, _memberInfo.Name, target.GetType()), ex);
            }
        }
    }
}

#endif