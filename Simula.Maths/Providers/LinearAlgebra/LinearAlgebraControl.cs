using System;

namespace Simula.Maths.Providers.LinearAlgebra
{
    public static class LinearAlgebraControl
    {
        const string EnvVarLAProvider = "MathNetMaths.AProvider";
        const string EnvVarLAProviderPath = "MathNetMaths.AProviderPath";

        static ILinearAlgebraProvider _linearAlgebraProvider;
        static readonly object StaticLock = new object();

        /// <summary>
        /// Gets or sets the linear algebra provider.
        /// Consider to use UseNativeMKL or UseManaged instead.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static ILinearAlgebraProvider Provider
        {
            get
            {
                if (_linearAlgebraProvider == null)
                {
                    lock (StaticLock)
                    {
                        if (_linearAlgebraProvider == null)
                        {
                            UseDefault();
                        }
                    }
                }

                return _linearAlgebraProvider;
            }
            set
            {
                value.InitializeVerify();

                // only actually set if verification did not throw
                _linearAlgebraProvider = value;
            }
        }

        /// <summary>
        /// Optional path to try to load native provider binaries from.
        /// If not set, Maths.will fall back to the environment variable
        /// `MathNetMaths.AProviderPath` or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

        public static ILinearAlgebraProvider CreateManaged()
        {
            return new Managed.ManagedLinearAlgebraProvider();
        }

        public static void UseManaged()
        {
            Provider = CreateManaged();
        }

        internal static ILinearAlgebraProvider CreateManagedReference()
        {
            return new ManagedReference.ManagedReferenceLinearAlgebraProvider();
        }

        internal static void UseManagedReference()
        {
            Provider = CreateManagedReference();
        }

#if NATIVE
        [CLSCompliant(false)]
        public static ILinearAlgebraProvider CreateNativeMKL(
            Common.Mkl.MklConsistency consistency = Common.Mkl.MklConsistency.Auto,
            Common.Mkl.MklPrecision precision = Common.Mkl.MklPrecision.Double,
            Common.Mkl.MklAccuracy accuracy = Common.Mkl.MklAccuracy.High)
        {
            return new Mkl.MklLinearAlgebraProvider(GetCombinedHintPath(), consistency, precision, accuracy);
        }

        [CLSCompliant(false)]
        public static void UseNativeMKL(
            Common.Mkl.MklConsistency consistency = Common.Mkl.MklConsistency.Auto,
            Common.Mkl.MklPrecision precision = Common.Mkl.MklPrecision.Double,
            Common.Mkl.MklAccuracy accuracy = Common.Mkl.MklAccuracy.High)
        {
            Provider = CreateNativeMKL(consistency, precision, accuracy);
        }

        [CLSCompliant(false)]
        public static bool TryUseNativeMKL(
            Common.Mkl.MklConsistency consistency = Common.Mkl.MklConsistency.Auto,
            Common.Mkl.MklPrecision precision = Common.Mkl.MklPrecision.Double,
            Common.Mkl.MklAccuracy accuracy = Common.Mkl.MklAccuracy.High)
        {
            return TryUse(CreateNativeMKL(consistency, precision, accuracy));
        }

        public static ILinearAlgebraProvider CreateNativeCUDA()
        {
            return new Cuda.CudaLinearAlgebraProvider(GetCombinedHintPath());
        }

        public static void UseNativeCUDA()
        {
            Provider = CreateNativeCUDA();
        }

        public static bool TryUseNativeCUDA()
        {
            return TryUse(CreateNativeCUDA());
        }

        public static ILinearAlgebraProvider CreateNativeOpenBLAS()
        {
            return new OpenBlas.OpenBlasLinearAlgebraProvider(GetCombinedHintPath());
        }

        public static void UseNativeOpenBLAS()
        {
            Provider = CreateNativeOpenBLAS();
        }

        public static bool TryUseNativeOpenBLAS()
        {
            return TryUse(CreateNativeOpenBLAS());
        }

        /// <summary>
        /// Try to use a native provider, if available.
        /// </summary>
        public static bool TryUseNative()
        {
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                return false;
            }

            return TryUseNativeMKL() || TryUseNativeOpenBLAS() || TryUseNativeCUDA();
        }
#endif

        static bool TryUse(ILinearAlgebraProvider provider)
        {
            try
            {
                if (!provider.IsAvailable())
                {
                    return false;
                }

                Provider = provider;
                return true;
            }
            catch
            {
                // intentionally swallow exceptions here - use the explicit variants if you're interested in why
                return false;
            }
        }

        /// <summary>
        /// Use the best provider available.
        /// </summary>
        public static void UseBest()
        {
            if (AppSwitches.DisableNativeProviders || AppSwitches.DisableNativeProviderProbing)
            {
                UseManaged();
                return;
            }

#if NATIVE
            if (!TryUseNative())
            {
                UseManaged();
            }
#else
            UseManaged();
#endif
        }

        /// <summary>
        /// Use a specific provider if configured, e.g. using the
        /// "MathNetMaths.AProvider" environment variable,
        /// or fall back to the best provider.
        /// </summary>
        public static void UseDefault()
        {
            if (AppSwitches.DisableNativeProviders)
            {
                UseManaged();
                return;
            }

#if NATIVE
            var value = Environment.GetEnvironmentVariable(EnvVarLAProvider);
            switch (value != null ? value.ToUpperInvariant() : string.Empty)
            {
                case "MKL":
                    UseNativeMKL();
                    break;

                case "CUDA":
                    UseNativeCUDA();
                    break;

                case "OPENBLAS":
                    UseNativeOpenBLAS();
                    break;

                default:
                    UseBest();
                    break;
            }
#else
            UseBest();
#endif
        }

        public static void FreeResources()
        {
            Provider.FreeResources();
        }

        static string GetCombinedHintPath()
        {
            if (!String.IsNullOrEmpty(HintPath))
            {
                return HintPath;
            }

            var value = Environment.GetEnvironmentVariable(EnvVarLAProviderPath);
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }
    }
}
