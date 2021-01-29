using System;

namespace Simula.Maths.Providers.LinearAlgebra.Managed
{
    /// <summary>
    /// The managed linear algebra provider.
    /// </summary>
    internal partial class ManagedLinearAlgebraProvider : ILinearAlgebraProvider
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

        /// <summary>
        /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
        /// </summary>
        static void GetRow<T>(Transpose transpose, int rowindx, int numRows, int numCols, T[] matrix, T[] row)
        {
            if (transpose == Transpose.DontTranspose)
            {
                for (int i = 0; i < numCols; i++)
                {
                    row[i] = matrix[(i * numRows) + rowindx];
                }
            }
            else
            {
                Array.Copy(matrix, rowindx * numCols, row, 0, numCols);
            }
        }

        /// <summary>
        /// Assumes that <paramref name="numRows"/> and <paramref name="numCols"/> have already been transposed.
        /// </summary>
        static void GetColumn<T>(Transpose transpose, int colindx, int numRows, int numCols, T[] matrix, T[] column)
        {
            if (transpose == Transpose.DontTranspose)
            {
                Array.Copy(matrix, colindx * numRows, column, 0, numRows);
            }
            else
            {
                for (int i = 0; i < numRows; i++)
                {
                    column[i] = matrix[(i * numCols) + colindx];
                }
            }
        }
    }
}
