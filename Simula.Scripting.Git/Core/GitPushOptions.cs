using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal class GitPushOptions
    {
        public int Version = 1;
        public int PackbuilderDegreeOfParallelism;
        public GitRemoteCallbacks RemoteCallbacks;
        public GitProxyOptions ProxyOptions;
        public GitStrArrayManaged CustomHeaders;
    }
}
