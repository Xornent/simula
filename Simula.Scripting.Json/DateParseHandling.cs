
namespace Simula.Scripting.Json
{
    public enum DateParseHandling
    {
        None = 0,
        DateTime = 1,
#if HAVE_DATE_TIME_OFFSET
        DateTimeOffset = 2
#endif
    }
}
