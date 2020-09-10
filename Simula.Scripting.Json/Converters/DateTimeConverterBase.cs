
using System;

namespace Simula.Scripting.Json.Converters
{
    public abstract class DateTimeConverterBase : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            if (objectType == typeof(DateTime) || objectType == typeof(DateTime?))
            {
                return true;
            }
#if HAVE_DATE_TIME_OFFSET
            if (objectType == typeof(DateTimeOffset) || objectType == typeof(DateTimeOffset?))
            {
                return true;
            }
#endif

            return false;
        }
    }
}