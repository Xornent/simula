#if NATIVE

namespace Simula.Maths.Providers.Common.Cuda
{
    internal enum ProviderPlatform : int
    {
        x86 = 8,
        x64 = 9,
        ia64 = 10,
    }

    internal enum ProviderConfig : int
    {
        Revision = 64,
    }

    internal enum ProviderCapability : int
    {
        LinearAlgebraMajor = 128,
        LinearAlgebraMinor = 129,
    }
}

#endif
