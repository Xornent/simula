namespace Simula.Maths.Providers.LinearAlgebra.ManagedReference
{
    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    internal partial class ManagedReferenceLinearAlgebraProvider : ILinearAlgebraProvider
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        public virtual bool IsAvailable()
        {
            return true;
        }

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        public virtual void InitializeVerify()
        {
        }

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        public virtual void FreeResources()
        {
        }

        public override string ToString()
        {
            return "Managed";
        }
    }
}
