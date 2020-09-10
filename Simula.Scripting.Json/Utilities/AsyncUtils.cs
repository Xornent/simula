
#if HAVE_ASYNC

using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Simula.Scripting.Json.Utilities
{
    internal static class AsyncUtils
    {
        public static readonly Task<bool> False = Task.FromResult(false);
        public static readonly Task<bool> True = Task.FromResult(true);

        internal static Task<bool> ToAsync(this bool value) => value ? True : False;

        public static Task? CancelIfRequestedAsync(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : null;
        }

        public static Task<T>? CancelIfRequestedAsync<T>(this CancellationToken cancellationToken)
        {
            return cancellationToken.IsCancellationRequested ? FromCanceled<T>(cancellationToken) : null;
        }
        public static Task FromCanceled(this CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(cancellationToken.IsCancellationRequested);
            return new Task(() => {}, cancellationToken);
        }

        public static Task<T> FromCanceled<T>(this CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(cancellationToken.IsCancellationRequested);
#pragma warning disable CS8653 // A default expression introduces a null value for a type parameter.
            return new Task<T>(() => default, cancellationToken);
#pragma warning restore CS8653 // A default expression introduces a null value for a type parameter.
        }
        internal static readonly Task CompletedTask = Task.Delay(0);

        public static Task WriteAsync(this TextWriter writer, char value, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value);
        }

        public static Task WriteAsync(this TextWriter writer, string? value, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value);
        }

        public static Task WriteAsync(this TextWriter writer, char[] value, int start, int count, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(writer != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled(cancellationToken) : writer.WriteAsync(value, start, count);
        }

        public static Task<int> ReadAsync(this TextReader reader, char[] buffer, int index, int count, CancellationToken cancellationToken)
        {
            MiscellaneousUtils.Assert(reader != null);
            return cancellationToken.IsCancellationRequested ? FromCanceled<int>(cancellationToken) : reader.ReadAsync(buffer, index, count);
        }

        public static bool IsCompletedSucessfully(this Task task)
        {
#if NETCOREAPP2_0
            return task.IsCompletedSucessfully;
#else
            return task.Status == TaskStatus.RanToCompletion;
#endif
        }
    }
}

#endif
