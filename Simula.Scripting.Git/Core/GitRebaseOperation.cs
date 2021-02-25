using System;
using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct git_rebase_operation
    {
        internal RebaseStepOperation type;
        internal git_oid id;
        internal char* exec;
    }
}
