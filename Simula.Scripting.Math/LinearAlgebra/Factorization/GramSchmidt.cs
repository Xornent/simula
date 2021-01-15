using System;

namespace Simula.Maths.LinearAlgebra.Factorization
{
    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition Modified Gram-Schmidt Orthogonalization.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal mxn matrix and R is an nxn upper triangular matrix.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by modified Gram-Schmidt Orthogonalization.
    /// </remarks>
    /// <typeparam name="T">Supported data types are double, single, <see cref="Complex"/>, and <see cref="Complex32"/>.</typeparam>
    public abstract class GramSchmidt<T> : QR<T>
    where T : struct, IEquatable<T>, IFormattable
    {
        protected GramSchmidt(Matrix<T> q, Matrix<T> rFull)
            : base(q, rFull, QRMethod.Full)
        {
        }
    }
}
