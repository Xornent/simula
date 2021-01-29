

#if HAVE_ASYNC

using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simula.Scripting.Json.Linq
{
    public partial class JRaw
    {
        public static async Task<JRaw> CreateAsync(JsonReader reader, CancellationToken cancellationToken = default)
        {
            using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
            using (JsonTextWriter jsonWriter = new JsonTextWriter(sw)) {
                await jsonWriter.WriteTokenSyncReadingAsync(reader, cancellationToken).ConfigureAwait(false);

                return new JRaw(sw.ToString());
            }
        }
    }
}

#endif
