using System;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct GitError
    {
        public char* Message;
        public GitErrorCategory Category;
    }
}
