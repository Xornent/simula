#if NATIVE

using System;
using Simula.Maths.Providers.Common.Mkl;

namespace Simula.Maths.Providers.LinearAlgebra.Mkl
{
    /// <summary>
    /// Error codes return from the MKL provider.
    /// </summary>
    internal enum MklError : int
    {
        /// <summary>
        /// Unable to allocate memory.
        /// </summary>
        MemoryAllocation = -999999
    }

    /// <summary>
    /// Intel's Math Kernel Library (MKL) linear algebra provider.
    /// </summary>
    internal partial class MklLinearAlgebraProvider : Managed.ManagedLinearAlgebraProvider, IDisposable
    {
        const int MinimumCompatibleRevision = 4;

        readonly string _hintPath;
        readonly MklConsistency _consistency;
        readonly MklPrecision _precision;
        readonly MklAccuracy _accuracy;

        int _linearAlgebraMajor;
        int _linearAlgebraMinor;
        int _vectorFunctionsMajor;
        int _vectorFunctionsMinor;

        /// <param name="hintPath">Hint path where to look for the native binaries</param>
        /// <param name="consistency">
        /// Sets the desired bit consistency on repeated identical computations on varying CPU architectures,
        /// as a trade-off with performance.
        /// </param>
        /// <param name="precision">VML optimal precision and rounding.</param>
        /// <param name="accuracy">VML accuracy mode.</param>
        internal MklLinearAlgebraProvider(string hintPath, MklConsistency consistency, MklPrecision precision, MklAccuracy accuracy)
        {
            _hintPath = hintPath;
            _consistency = consistency;
            _precision = precision;
            _accuracy = accuracy;
        }

        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public override bool IsAvailable()
        {
            return MklProvider.IsAvailable(hintPath: _hintPath);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If calling this method fails, consider to fall back to alternatives like the managed provider.
        /// </summary>
        public override void InitializeVerify()
        {
            int revision = MklProvider.Load(_hintPath, _consistency, _precision, _accuracy);
            if (revision < MinimumCompatibleRevision)
            {
                throw new NotSupportedException(FormattableString.Invariant($"MKL Native Provider revision r{revision} is too old. Consider upgrading to a newer version. Revision r{MinimumCompatibleRevision} and newer are supported."));
            }

            _linearAlgebraMajor = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMajor);
            _linearAlgebraMinor = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMinor);
            _vectorFunctionsMajor = SafeNativeMethods.query_capability((int)ProviderCapability.VectorFunctionsMajor);
            _vectorFunctionsMinor = SafeNativeMethods.query_capability((int)ProviderCapability.VectorFunctionsMinor);

            // we only support exactly one major version, since major version changes imply a breaking change.
            if (_linearAlgebraMajor != 2)
            {
                throw new NotSupportedException(FormattableString.Invariant($"MKL Native Provider not compatible. Expecting linear algebra v2 but provider implements v{_linearAlgebraMajor}."));
            }
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public override void FreeResources()
        {
            MklProvider.FreeResources();
        }

        public override string ToString()
        {
            return MklProvider.Describe();
        }

        public void Dispose()
        {
            FreeResources();
        }
    }
}

#endif
