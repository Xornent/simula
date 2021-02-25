using System.Runtime.InteropServices;

namespace Simula.Scripting.Git.Core
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct git_certificate
    {
        public GitCertificateType type;
    }
}
