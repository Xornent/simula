using System;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core.Handles
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GitBuf : IDisposable
    {
        public IntPtr ptr;
        public UIntPtr asize;
        public UIntPtr size;

        public void Dispose()
        {
            Proxy.git_buf_dispose(this);
        }
    }
}
