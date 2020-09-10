
#if HAVE_COMPONENT_MODEL
using System;
using System.ComponentModel;

namespace Simula.Scripting.Json.Linq
{
    public class JPropertyDescriptor : PropertyDescriptor
    {
        public JPropertyDescriptor(string name)
            : base(name, null)
        {
        }

        private static JObject CastInstance(object instance)
        {
            return (JObject)instance;
        }
        public override bool CanResetValue(object component)
        {
            return false;
        }
        public override object? GetValue(object component)
        {
            return (component as JObject)?[Name];
        }
        public override void ResetValue(object component)
        {
        }
        public override void SetValue(object component, object value)
        {
            if (component is JObject o)
            {
                JToken token = value as JToken ?? new JValue(value);

                o[Name] = token;
            }
        }
        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
        public override Type ComponentType => typeof(JObject);
        public override bool IsReadOnly => false;
        public override Type PropertyType => typeof(object);
        protected override int NameHashCode
        {
            get
            {
                int nameHashCode = base.NameHashCode;
                return nameHashCode;
            }
        }
    }
}

#endif