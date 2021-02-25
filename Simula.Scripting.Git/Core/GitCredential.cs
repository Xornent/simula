using System;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct GitCredential
    {
        public GitCredentialType credtype;
        public IntPtr free;
    }
}

