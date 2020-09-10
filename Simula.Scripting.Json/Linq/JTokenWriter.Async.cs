
#if HAVE_ASYNC

using System.Threading;
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json.Linq
{
    public partial class JTokenWriter
    {
        internal override Task WriteTokenAsync(JsonReader reader, bool writeChildren, bool writeDateConstructorAsDate, bool writeComments, CancellationToken cancellationToken)
        {
            if (reader is JTokenReader)
            {
                WriteToken(reader, writeChildren, writeDateConstructorAsDate, writeComments);
                return AsyncUtils.CompletedTask;
            }

            return WriteTokenSyncReadingAsync(reader, cancellationToken);
        }
    }
}

#endif