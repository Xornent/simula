
namespace Simula.Maths.LinearAlgebra
{
    public enum ExistingData
    {
        /// <summary>
        /// Existing data may not be all zeros, so clearing may be necessary
        /// if not all of it will be overwritten anyway.
        /// </summary>
        Clear = 0,

        /// <summary>
        /// If existing data is assumed to be all zeros already,
        /// clearing it may be skipped if applicable.
        /// </summary>
        AssumeZeros = 1
    }

    public enum Zeros
    {
        /// <summary>
        /// Allow skipping zero entries (without enforcing skipping them).
        /// When enumerating sparse matrices this can significantly speed up operations.
        /// </summary>
        AllowSkip = 0,

        /// <summary>
        /// Force applying the operation to all fields even if they are zero.
        /// </summary>
        Include = 1
    }

    public enum Symmetricity
    {
        /// <summary>
        /// It is not known yet whether a matrix is symmetric or not.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// A matrix is symmetric
        /// </summary>
        Symmetric = 1,

        /// <summary>
        /// A matrix is Hermitian (conjugate symmetric).
        /// </summary>
        Hermitian = 2,

        /// <summary>
        /// A matrix is not symmetric
        /// </summary>
        Asymmetric = 3
    }
}
