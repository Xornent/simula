using Complex = System.Numerics.Complex;

namespace Simula.Maths.Providers.FourierTransform
{
    public enum FourierTransformScaling : int
    {
        NoScaling = 0,
        SymmetricScaling = 1,
        BackwardScaling = 2,
        ForwardScaling = 3
    }

    public interface IFourierTransformProvider
    {
        /// <summary>
        /// Try to find out whether the provider is available, at least in principle.
        /// Verification may still fail if available, but it will certainly fail if unavailable.
        /// </summary>
        bool IsAvailable();

        /// <summary>
        /// Initialize and verify that the provided is indeed available. If not, fall back to alternatives like the managed provider
        /// </summary>
        void InitializeVerify();

        /// <summary>
        /// Frees memory buffers, caches and handles allocated in or to the provider.
        /// Does not unload the provider itself, it is still usable afterwards.
        /// </summary>
        void FreeResources();

        void Forward(Complex32[] samples, FourierTransformScaling scaling);
        void Forward(Complex[] samples, FourierTransformScaling scaling);
        void Backward(Complex32[] spectrum, FourierTransformScaling scaling);
        void Backward(Complex[] spectrum, FourierTransformScaling scaling);

        void ForwardReal(float[] samples, int n, FourierTransformScaling scaling);
        void ForwardReal(double[] samples, int n, FourierTransformScaling scaling);
        void BackwardReal(float[] spectrum, int n, FourierTransformScaling scaling);
        void BackwardReal(double[] spectrum, int n, FourierTransformScaling scaling);

        void ForwardMultidim(Complex32[] samples, int[] dimensions, FourierTransformScaling scaling);
        void ForwardMultidim(Complex[] samples, int[] dimensions, FourierTransformScaling scaling);
        void BackwardMultidim(Complex32[] spectrum, int[] dimensions, FourierTransformScaling scaling);
        void BackwardMultidim(Complex[] spectrum, int[] dimensions, FourierTransformScaling scaling);
    }
}
