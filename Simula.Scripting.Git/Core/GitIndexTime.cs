using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GitIndexTime
    {
        public int seconds;
        public uint nanoseconds;
    }
}
