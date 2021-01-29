namespace Simula.Maths
{
    /// <summary>
    /// AppContext based switches to disable functionality, controllable through also in the
    /// host application through AppContext or by configuration with AppContextSwitchOverride.
    /// https://docs.microsoft.com/en-us/dotnet/framework/configure-apps/file-schema/runtime/appcontextswitchoverrides-element
    /// </summary>
    /// <remarks>
    /// Since AppContext is not supported on .NET Framework 4.0, a local implementation is used there instead,
    /// which cannot be controlled though configuration or through AppContext.
    /// </remarks>
    public static class AppSwitches
    {
        const string AppSwitchDisableNativeProviderProbing = "Switch.Simula.Maths.Providers.DisableNativeProviderProbing";
        const string AppSwitchDisableNativeProviders = "Switch.Simula.Maths.Providers.DisableNativeProviders";
        const string AppSwitchDisableMklNativeProvider = "Switch.Simula.Maths.Providers.DisableMklNativeProvider";
        const string AppSwitchDisableAcmlNativeProvider = "Switch.Simula.Maths.Providers.DisableAcmlNativeProvider";
        const string AppSwitchDisableCudaNativeProvider = "Switch.Simula.Maths.Providers.DisableCudaNativeProvider";
        const string AppSwitchDisableOpenBlasNativeProvider = "Switch.Simula.Maths.Providers.DisableOpenBlasNativeProvider";

#if NET40
        static readonly System.Collections.Generic.Dictionary<string, bool> Switches = new System.Collections.Generic.Dictionary<string, bool>();
#endif

        static void SetSwitch(string switchName, bool isEnabled)
        {
#if NET40
            Switches[switchName] = isEnabled;
#else
            System.AppContext.SetSwitch(switchName, isEnabled);
#endif
        }

        static bool IsEnabled(string switchName)
        {
#if NET40
            return Switches.TryGetValue(switchName, out bool isEnabled) && isEnabled;
#else
            return System.AppContext.TryGetSwitch(switchName, out bool isEnabled) && isEnabled;
#endif
        }

        public static bool DisableNativeProviderProbing
        {
            get => IsEnabled(AppSwitchDisableNativeProviderProbing);
            set => SetSwitch(AppSwitchDisableNativeProviderProbing, value);
        }

        public static bool DisableNativeProviders
        {
            get => IsEnabled(AppSwitchDisableNativeProviders);
            set => SetSwitch(AppSwitchDisableNativeProviders, value);
        }

        public static bool DisableMklNativeProvider
        {
            get => IsEnabled(AppSwitchDisableMklNativeProvider);
            set => SetSwitch(AppSwitchDisableMklNativeProvider, value);
        }

        public static bool DisableAcmlNativeProvider
        {
            get => IsEnabled(AppSwitchDisableAcmlNativeProvider);
            set => SetSwitch(AppSwitchDisableAcmlNativeProvider, value);
        }

        public static bool DisableCudaNativeProvider
        {
            get => IsEnabled(AppSwitchDisableCudaNativeProvider);
            set => SetSwitch(AppSwitchDisableCudaNativeProvider, value);
        }

        public static bool DisableOpenBlasNativeProvider
        {
            get => IsEnabled(AppSwitchDisableOpenBlasNativeProvider);
            set => SetSwitch(AppSwitchDisableOpenBlasNativeProvider, value);
        }
    }
}
