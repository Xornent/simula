
#if HAVE_ASYNC

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Simula.Scripting.Json.Utilities;

namespace Simula.Scripting.Json
{
    public abstract partial class JsonReader
    {
        public virtual Task<bool> ReadAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<bool>() ?? Read().ToAsync();
        }
        public async Task SkipAsync(CancellationToken cancellationToken = default)
        {
            if (TokenType == JsonToken.PropertyName)
            {
                await ReadAsync(cancellationToken).ConfigureAwait(false);
            }

            if (JsonTokenUtils.IsStartToken(TokenType))
            {
                int depth = Depth;

                while (await ReadAsync(cancellationToken).ConfigureAwait(false) && depth < Depth)
                {
                }
            }
        }

        internal async Task ReaderReadAndAssertAsync(CancellationToken cancellationToken)
        {
            if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
            {
                throw CreateUnexpectedEndException();
            }
        }
        public virtual Task<bool?> ReadAsBooleanAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<bool?>() ?? Task.FromResult(ReadAsBoolean());
        }
        public virtual Task<byte[]?> ReadAsBytesAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<byte[]?>() ?? Task.FromResult(ReadAsBytes());
        }

        internal async Task<byte[]?> ReadArrayIntoByteArrayAsync(CancellationToken cancellationToken)
        {
            List<byte> buffer = new List<byte>();

            while (true)
            {
                if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    SetToken(JsonToken.None);
                }

                if (ReadArrayElementIntoByteArrayReportDone(buffer))
                {
                    byte[] d = buffer.ToArray();
                    SetToken(JsonToken.Bytes, d, false);
                    return d;
                }
            }
        }
        public virtual Task<DateTime?> ReadAsDateTimeAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<DateTime?>() ?? Task.FromResult(ReadAsDateTime());
        }
        public virtual Task<DateTimeOffset?> ReadAsDateTimeOffsetAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<DateTimeOffset?>() ?? Task.FromResult(ReadAsDateTimeOffset());
        }
        public virtual Task<decimal?> ReadAsDecimalAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<decimal?>() ?? Task.FromResult(ReadAsDecimal());
        }
        public virtual Task<double?> ReadAsDoubleAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(ReadAsDouble());
        }
        public virtual Task<int?> ReadAsInt32Async(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<int?>() ?? Task.FromResult(ReadAsInt32());
        }
        public virtual Task<string?> ReadAsStringAsync(CancellationToken cancellationToken = default)
        {
            return cancellationToken.CancelIfRequestedAsync<string?>() ?? Task.FromResult(ReadAsString());
        }

        internal async Task<bool> ReadAndMoveToContentAsync(CancellationToken cancellationToken)
        {
            return await ReadAsync(cancellationToken).ConfigureAwait(false) && await MoveToContentAsync(cancellationToken).ConfigureAwait(false);
        }

        internal Task<bool> MoveToContentAsync(CancellationToken cancellationToken)
        {
            switch (TokenType)
            {
                case JsonToken.None:
                case JsonToken.Comment:
                    return MoveToContentFromNonContentAsync(cancellationToken);
                default:
                    return AsyncUtils.True;
            }
        }

        private async Task<bool> MoveToContentFromNonContentAsync(CancellationToken cancellationToken)
        {
            while (true)
            {
                if (!await ReadAsync(cancellationToken).ConfigureAwait(false))
                {
                    return false;
                }

                switch (TokenType)
                {
                    case JsonToken.None:
                    case JsonToken.Comment:
                        break;
                    default:
                        return true;
                }
            }
        }
    }
}

#endif
