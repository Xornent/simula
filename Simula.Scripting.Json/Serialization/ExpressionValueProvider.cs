#if !(NET20 || NET35)

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
    /// <summary>
    /// Get and set values for a <see cref="MemberInfo"/> using dynamic methods.
    /// </summary>
    public class ExpressionValueProvider : IValueProvider
    {
        private readonly MemberInfo _memberInfo;
        private Func<object, object> _getter;
        private Action<object, object> _setter;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionValueProvider"/> class.
        /// </summary>
        /// <param name="memberInfo">The member info.</param>
        public ExpressionValueProvider(MemberInfo memberInfo)
        {
            ValidationUtils.ArgumentNotNull(memberInfo, nameof(memberInfo));
            _memberInfo = memberInfo;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="target">The target to set the value on.</param>
        /// <param name="value">The value to set on the target.</param>
        public void SetValue(object target, object value)
        {
            try
            {
                if (_setter == null)
                {
                    _setter = ExpressionReflectionDelegateFactory.Instance.CreateSet<object>(_memberInfo);
                }

#if DEBUG
                // dynamic method doesn't check whether the type is 'legal' to set
                // add this check for unit tests
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

        /// <summary>
        /// Gets the value.
        /// </summary>
        /// <param name="target">The target to get the value from.</param>
        /// <returns>The value.</returns>
        public object GetValue(object target)
        {
            try
            {
                if (_getter == null)
                {
                    _getter = ExpressionReflectionDelegateFactory.Instance.CreateGet<object>(_memberInfo);
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