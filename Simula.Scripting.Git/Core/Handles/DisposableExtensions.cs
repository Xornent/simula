using System;

namespace Simula.Scripting.Git.Core.Handles
{
    internal static class DisposableExtensions
    {
        public static void SafeDispose(this IDisposable disposable)
        {
            if (disposable == null)
            {
                return;
            }

            disposable.Dispose();
        }
    }
}
