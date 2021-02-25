using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GitSubmoduleUpdateOptions
    {
        public uint Version;

        public GitCheckoutOpts CheckoutOptions;

        public GitFetchOptions FetchOptions;

        public CheckoutStrategy CloneCheckoutStrategy;

        public int AllowFetch;
    }
}
