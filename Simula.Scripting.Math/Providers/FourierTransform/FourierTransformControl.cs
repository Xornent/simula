using System;

namespace Simula.Maths.Providers.FourierTransform
{
    public static class FourierTransformControl
    {
        const string EnvVarFFTProvider = "MathNetMaths.FTProvider";
        const string EnvVarFFTProviderPath = "MathNetMaths.FTProviderPath";

        static IFourierTransformProvider _fourierTransformProvider;
        static readonly object StaticLock = new object();

        /// <summary>
        /// Gets or sets the Fourier transform provider. Consider to use UseNativeMKL or UseManaged instead.
        /// </summary>
        /// <value>The linear algebra provider.</value>
        public static IFourierTransformProvider Provider
        {
            get
            {
                if (_fourierTransformProvider == null)
                {
                    lock (StaticLock)
                    {
                        if (_fourierTransformProvider == null)
                        {
                            UseDefault();
                        }
                    }
                }

                return _fourierTransformProvider;
            }
            set
            {
                value.InitializeVerify();

                // only actually set if verification did not throw
                _fourierTransformProvider = value;
            }
        }

        /// <summary>
        /// Optional path to try to load native provider binaries from.
        /// If not set, Maths.will fall back to the environment variable
        /// `MathNetMaths.FTProviderPath` or the default probing paths.
        /// </summary>
        public static string HintPath { get; set; }

        public static IFourierTransformProvider CreateManaged()
        {
            return new Managed.ManagedFourierTransformProvider();
        }

        public static void UseManaged()
        {
            Provider = CreateManaged();
        }

#if NATIVE
        public static IFourierTransformProvider CreateNativeMKL()
        {
            return new Mkl.MklFourierTransformProvider(GetCombinedHintPath());
        }

        public static void UseNativeMKL()
        {
            Provider = CreateNativeMKL();
        }

        public static bool TryUseNativeMKL()
        {
            return TryUse(CreateNativeMKL());
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

            return TryUseNativeMKL();
        }
#endif

        static bool TryUse(IFourierTransformProvider provider)
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
        /// "MathNetMaths.FTProvider" environment variable,
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
            var value = Environment.GetEnvironmentVariable(EnvVarFFTProvider);
            switch (value != null ? value.ToUpperInvariant() : string.Empty)
            {

                case "MKL":
                    UseNativeMKL();
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

            var value = Environment.GetEnvironmentVariable(EnvVarFFTProviderPath);
            if (!String.IsNullOrEmpty(value))
            {
                return value;
            }

            return null;
        }
    }
}
