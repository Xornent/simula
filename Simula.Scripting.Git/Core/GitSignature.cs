using System;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct git_signature
    {
        public char* name;
        public char* email;
        public git_time when;
    }
}
