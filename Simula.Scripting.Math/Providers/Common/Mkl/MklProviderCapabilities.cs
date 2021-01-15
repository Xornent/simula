#if NATIVE

namespace Simula.Maths.Providers.Common.Mkl
{
    internal enum ProviderPlatform : int
    {
        x86 = 8,
        x64 = 9,
        ia64 = 10,
    }

    internal enum ProviderConfig : int
    {
        MklMajorVersion = 32,
        MklMinorVersion = 33,
        MklUpdateVersion = 34,
        Revision = 64,
        Precision = 65,
        Threading = 66,
        Memory = 67,
    }

    internal enum ProviderCapability : int
    {
        LinearAlgebraMajor = 128,
        LinearAlgebraMinor = 129,
        VectorFunctionsMajor = 130,
        VectorFunctionsMinor = 131,
        FourierTransformMajor = 384,
        FourierTransformMinor = 385,
        SparseSolverMajor = 512,
        SparseSolverMinor = 513,
    }
}

#endif
