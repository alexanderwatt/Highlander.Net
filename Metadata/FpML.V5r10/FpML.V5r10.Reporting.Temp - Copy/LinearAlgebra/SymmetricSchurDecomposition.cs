/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Analytics.LinearAlgebra.Sparse;

namespace Orion.Analytics.LinearAlgebra
{
    /// <summary>
    /// Symmetric threshold Jacobi algorithm.
    /// </summary>
    /// <remarks>
    /// Given a real symmetric matrix S, the Schur decomposition
    /// finds the eigenvalues and eigenvectors of S. If D is the
    /// diagonal matrix formed by the eigenvalues and U the
    /// unitarian matrix of th eigenvector we can write the
    /// Schur decomposition as
    /// 	\f[ S = U \cdot D \cdot U^T \, ,\f]
    /// where \f$ \cdot \f$ is the standard matrix product
    /// and  \f$ ^T  \f$ is the transpose operator.
    /// 
    /// This class implements the Schur decomposition using the
    /// symmetric threshold Jacobi algorithm. For details on the
    /// different Jacobi transformations you can start from the great book
    /// on matrix computations by Golub and Van Loan: Matrix computation,
    /// second edition The Johns Hopkins University Press
    /// </remarks>
    public class SymmetricSchurDecomposition
    {
        /* \pre s must be symmetric */
        /// <summary>
        /// Constructs a SymmetricSchurDecomposition given
        /// a symmetric <see cref="Matrix"/>.
        /// </summary>
        /// <param name="symmetricMatrix">A symmetric Matrix which is not modified.</param>
        public SymmetricSchurDecomposition(Matrix symmetricMatrix)
        {
            size = symmetricMatrix.RowCount;
            if( size != symmetricMatrix.ColumnCount)
                throw new ArgumentException("TODO: Input matrix must be square.");
            _diagonal = new SparseVector(symmetricMatrix.Diagonal);
            _eigenVectors = Matrix.Identity(size,size);
            Compute(symmetricMatrix);
        }

        private readonly int size;
        private readonly SparseVector _diagonal;
        private readonly Matrix _eigenVectors;

        ///<summary>
        ///</summary>
        public SparseVector Eigenvalues => _diagonal;

        ///<summary>
        ///</summary>
        public Matrix Eigenvectors => _eigenVectors;

        private const int MaxIterations = 100;
        private const double EpsPrec = 1e-15;

        private void Compute(Matrix symmetricMatrix)
        {
            var tmpDiag = new SparseVector(size);
            Blas.Default.Copy(_diagonal, tmpDiag); 
            var tmpAccumulate = new SparseVector(size);
            Matrix s = symmetricMatrix.Clone();

            for( int ite=1; ite<=MaxIterations; ite++)
            { 
                // abs sum of upper triangel
                double sum = 0.0;
                for (int j=0; j < size-1; j++) 
                    for (int k=j+1; k < size; k++)
                        sum += Math.Abs(s[j,k]);

                if (sum == 0.0) return;

                // To speed up computation a threshold is introduced to
                // make sure it is worthy to perform the Jacobi rotation
                double threshold;
                if (ite < 5)
                    threshold = 0.2*sum/(size*size);
                else
                    threshold = 0.0;

                // sweep upper triangle
                for (int j=0; j < size-1; j++) 
                {
                    for (int k=j+1; k < size; k++) 
                    {
                        double smll = Math.Abs(s[j,k]);
                        if( ite > 5 &&
                            smll < EpsPrec * Math.Abs(_diagonal.Data[j]) &&
                            smll < EpsPrec * Math.Abs(_diagonal.Data[k]))
                        {
                            s[j,k] = 0.0;
                        }
                        else if (Math.Abs(s[j,k]) > threshold) 
                        {
                            double heig = _diagonal.Data[k]-_diagonal.Data[j];
                            double tang;
                            if ( smll < EpsPrec * Math.Abs(heig) )
                            {
                                tang = s[j,k]/heig;
                            }
                            else 
                            {
                                double beta = 0.5 * heig / s[j,k];
                                tang = 1.0 / ( Math.Abs(beta) + Math.Sqrt(1.0+beta*beta) );
                                if(beta < 0.0) tang = -tang;
                            }
                            double cosin = 1.0 / Math.Sqrt(1.0+tang*tang);
                            double sine = tang * cosin;
                            double rho = sine / (1.0+cosin);
                            heig  = tang*s[j,k];

                            tmpAccumulate.Data[j] -= heig;
                            tmpAccumulate.Data[k] += heig;
                            _diagonal.Data[j] -= heig;
                            _diagonal.Data[k] += heig;
                            s[j,k] = 0.0;

                            // perform rotation on upper triangle
                            int l=0;
                            for (   ; l < j; l++) 
                                JacobiRotate(s, rho, sine, l, j, l, k);
                            for (++l; l < k; l++) 
                                JacobiRotate(s, rho, sine, j, l, l, k);
                            for (++l; l < size; l++) 
                                JacobiRotate(s, rho, sine, j, l, k, l);

                            for (l = 0; l < size; l++) 
                                JacobiRotate(_eigenVectors, rho, sine, l, j, l, k);
                        }
                    }
                }
                // tmpDiag.Add(tmpAccumulate);
                // diagonal = tmpDiag.Clone();
                // tmpAccumulate.Fill(0.0);
                for (int j=0; j < size; j++) 
                {
                    tmpDiag.Data[j] += tmpAccumulate.Data[j];
                    _diagonal.Data[j] = tmpDiag.Data[j];
                    tmpAccumulate.Data[j] = 0.0;
                }
            }
            throw new ApplicationException("TODO: Too many iterations reached");
        }

        /// <summary>
        /// This routines implements the Jacobi, a.k.a. Givens, rotation.
        /// </summary>
        private static void JacobiRotate(Matrix m, double rot, double dil, int j1, int k1, int j2, int k2)
        {
            double x1 = m[j1,k1];
            double x2 = m[j2,k2];
            m[j1,k1] = x1 - dil*(x2 + x1*rot);
            m[j2,k2] = x2 + dil*(x1 - x2*rot);
        }
    }
}