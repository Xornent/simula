#if NATIVE

using System;
using Simula.Maths.Providers.Common.Cuda;

namespace Simula.Maths.Providers.LinearAlgebra.Cuda
{
    /// <summary>
    /// NVidia's CUDA Toolkit linear algebra provider.
    /// </summary>
    internal partial class CudaLinearAlgebraProvider : Managed.ManagedLinearAlgebraProvider, IDisposable
    {
        const int MinimumCompatibleRevision = 1;

        readonly string _hintPath;
        IntPtr _blasHandle;
        IntPtr _solverHandle;

        /// <param name="hintPath">Hint path where to look for the native binaries</param>
        internal CudaLinearAlgebraProvider(string hintPath)
        {
            _hintPath = hintPath;
        }

        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public override bool IsAvailable()
        {
            return CudaProvider.IsAvailable(hintPath: _hintPath);
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available.
        /// If calling this method fails, consider to fall back to alternatives like the managed provider.
        /// </summary>
        public override void InitializeVerify()
        {
            int revision = CudaProvider.Load(hintPath: _hintPath);
            if (revision < MinimumCompatibleRevision)
            {
                throw new NotSupportedException(FormattableString.Invariant($"Cuda Native Provider revision r{revision} is too old. Consider upgrading to a newer version. Revision r{MinimumCompatibleRevision} and newer are supported."));
            }

            int linearAlgebra = SafeNativeMethods.query_capability((int)ProviderCapability.LinearAlgebraMajor);

            // we only support exactly one major version, since major version changes imply a breaking change.
            if (linearAlgebra != 1)
            {
                throw new NotSupportedException(FormattableString.Invariant($"Cuda Native Provider not compatible. Expecting linear algebra v1 but provider implements v{linearAlgebra}."));
            }

            BLAS(SafeNativeMethods.createBLASHandle(ref _blasHandle));
            Solver(SafeNativeMethods.createSolverHandle(ref _solverHandle));
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public override void FreeResources()
        {
            CudaProvider.FreeResources();
        }

        private void BLAS(int status)
        {
            switch (status)
            {
                case 0:  // CUBLAS_STATUS_SUCCESS
                    return;

                case 1:  // CUBLAS_STATUS_NOT_INITIALIZED
                    throw new Exception("The CUDA Runtime initialization failed");

                case 2:  // CUSOLVER_STATUS_ALLOC_FAILED
                    throw new OutOfMemoryException("The resources could not be allocated");

                case 7:  // CUBLAS_STATUS_INVALID_VALUE
                    throw new ArgumentException("Invalid value");

                case 8:  // CUBLAS_STATUS_ARCH_MISMATCH
                    throw new NotSupportedException("The device does not support this operation.");

                case 11: // CUBLAS_STATUS_MAPPING_ERROR
                    throw new Exception("Mapping error.");

                case 13: // CUBLAS_STATUS_EXECUTION_FAILED
                    throw new Exception("Execution failed");

                case 14: // CUBLAS_STATUS_INTERNAL_ERROR
                    throw new Exception("Internal error");

                case 15: // CUBLAS_STATUS_NOT_SUPPORTED
                    throw new NotSupportedException();

                case 16: // CUBLAS_STATUS_LICENSE_ERROR
                    throw new Exception("License error");

                default:
                    throw new Exception("Unrecognized cuBLAS status code: " + status);
            }
        }

        private void Solver(int status)
        {
            switch (status)
            {
                case 0:  // CUSOLVER_STATUS_SUCCESS
                    return;

                case 1:  // CUSOLVER_STATUS_NOT_INITIALIZED
                    throw new Exception("The library was not initialized");

                case 2: // CUSOLVER_STATUS_ALLOC_FAILED
                    throw new OutOfMemoryException("The resources could not be allocated");

                case 3: // CUSOLVER_STATUS_INVALID_VALUE
                    throw new ArgumentException("Invalid value");

                case 4: // CUSOLVER_STATUS_ARCH_MISMATCH
                    throw new NotSupportedException("The device does not support compute capability 2.0 and above");

                case 5: // CUSOLVER_STATUS_MAPPING_ERROR
                    throw new Exception("Mapping error");

                case 6: // CUSOLVER_STATUS_EXECUTION_FAILED
                    throw new NonConvergenceException("Execution failed");

                case 7: //CUSOLVER_STATUS_INTERNAL_ERROR
                    throw new Exception("Internal error");

                case 8: // CUSOLVER_STATUS_MATRIX_TYPE_NOT_SUPPORTED
                    throw new ArgumentException("Matrix type not supported");

                case 9: // CUSOLVER_STATUS_NOT_SUPPORTED
                    throw new NotSupportedException();

                case 10: // CUSOLVER_STATUS_ZERO_PIVOT
                    throw new Exception("Zero pivot");

                case 11: //CUSOLVER_STATUS_INVALID_LICENSE
                    throw new Exception("Invalid license");

                default:
                    throw new Exception("Unrecognized cuSolverDn status code: " + status);
            }
        }

        public override string ToString()
        {
            return CudaProvider.Describe();
        }

        public void Dispose()
        {
            BLAS(SafeNativeMethods.destroyBLASHandle(_blasHandle));
            Solver(SafeNativeMethods.destroySolverHandle(_solverHandle));
            FreeResources();
        }
    }
}

#endif
