using System;

namespace Simula.TeX.Utils
{
    internal static class Result
    {
        public static Result<TValue> Ok<TValue>(TValue value) => new Result<TValue>(value, null);
        public static Result<TValue> Error<TValue>(Exception error) => new Result<TValue>(default!, error); // Nullable: CS8604; can't be avoided with generics without constraints
    }

    internal readonly struct Result<TValue>
    {
        private readonly TValue value;

        public TValue Value => Error == null ? value : throw Error;
        public Exception? Error { get; }

        public bool IsSuccess => Error == null;

        public Result(TValue value, Exception? error)
        {
            if (!Equals(value, default) && error != null) {
                throw new ArgumentException($"Invalid {nameof(Result)} constructor call", nameof(error));
            }

            this.value = value;
            Error = error;
        }

        public Result<TProduct> Map<TProduct>(Func<TValue, TProduct> mapper) => IsSuccess
            ? Result.Ok(mapper(Value))
            : Result.Error<TProduct>(Error!);
    }
}
