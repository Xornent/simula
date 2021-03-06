﻿using System;
using Simula.Maths.LinearAlgebra.Factorization;

namespace Simula.Maths.LinearAlgebra.Complex.Factorization
{
    using Complex = System.Numerics.Complex;

    /// <summary>
    /// <para>A class which encapsulates the functionality of the QR decomposition Modified Gram-Schmidt Orthogonalization.</para>
    /// <para>Any real square matrix A may be decomposed as A = QR where Q is an orthogonal mxn matrix and R is an nxn upper triangular matrix.</para>
    /// </summary>
    /// <remarks>
    /// The computation of the QR decomposition is done at construction time by modified Gram-Schmidt Orthogonalization.
    /// </remarks>
    internal abstract class GramSchmidt : GramSchmidt<Complex>
    {
        protected GramSchmidt(Matrix<Complex> q, Matrix<Complex> rFull)
            : base(q, rFull)
        {
        }

        /// <summary>
        /// Gets the absolute determinant value of the matrix for which the QR matrix was computed.
        /// </summary>
        public override Complex Determinant
        {
            get
            {
                if (FullR.RowCount != FullR.ColumnCount)
                {
                    throw new ArgumentException("Matrix must be square.");
                }

                var det = Complex.One;
                for (var i = 0; i < FullR.ColumnCount; i++)
                {
                    det *= FullR.At(i, i);
                    if (FullR.At(i, i).Magnitude.AlmostEqual(0.0))
                    {
                        return 0;
                    }
                }

                return det.Magnitude;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the matrix is full rank or not.
        /// </summary>
        /// <value><c>true</c> if the matrix is full rank; otherwise <c>false</c>.</value>
        public override bool IsFullRank
        {
            get
            {
                for (var i = 0; i < FullR.ColumnCount; i++)
                {
                    if (FullR.At(i, i).Magnitude.AlmostEqual(0.0))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
