#if NATIVE

using System;
using Simula.Maths.Providers.Common.OpenBlas;

namespace Simula.Maths.Providers.LinearAlgebra.OpenBlas
{
    /// <summary>
    /// Error codes return from the native OpenBLAS provider.
    /// </summary>
    public enum NativeError : int
    {
        /// <summary>
        /// Unable to allocate memory.
        /// </summary>
        MemoryAllocation = -999999
    }

    internal enum ParallelType : int
    {
        Sequential = 0,
        Thread = 1,
        OpenMP = 2
    }

    /// <summary>
    /// OpenBLAS linear algebra provider.
    /// </summary>
    internal partial class OpenBlasLinearAlgebraProvider : Managed.ManagedLinearAlgebraProvider, IDisposable
    {
        const int MinimumCompatibleRevision = 1;

        readonly string _hintPath;

        /// <param name="hintPath">Hint path where to look for the native binaries</param>
        internal OpenBlasLinearAlgebraProvider(string hintPath)
        {
            _hintPath = hintPath;
        }

        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public override bool IsAvailable()
        {
            return OpenBlasProvider.IsAvailable(hintPath: _hintPath);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If not, fall back to alternatives like the managed provider
        /// </summary>
        public override void InitializeVerify()
        {
            int revision = OpenBlasProvider.Load(hintPath: _hintPath);
            if (revision < MinimumCompatibleRevision)
            {
                throw new NotSupportedException(FormattableString.Invariant($"OpenBLAS Native Provider revision r{revision} is too old. Consider upgrading to a newer version. Revision r{MinimumCompatibleRevision} and newer are supported."));
            }

            int linearAlgebra = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMajor);

            // we only support exactly one major version, since major version changes imply a breaking change.
            if (linearAlgebra != 1)
            {
                throw new NotSupportedException(FormattableString.Invariant($"OpenBLAS Native Provider not compatible. Expecting linear algebra v1 but provider implements v{linearAlgebra}."));
            }
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public override void FreeResources()
        {
            OpenBlasProvider.FreeResources();
        }

        public override string ToString()
        {
            return OpenBlasProvider.Describe();
        }

        public void Dispose()
        {
            FreeResources();
        }
    }
}

#endif
