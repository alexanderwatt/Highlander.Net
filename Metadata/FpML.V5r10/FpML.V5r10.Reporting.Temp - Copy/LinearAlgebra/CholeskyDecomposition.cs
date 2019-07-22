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

namespace Orion.Analytics.LinearAlgebra
{
    /// <summary>Cholesky Decomposition.</summary>
    /// <remarks>
    /// For a symmetric, positive definite matrix A, the Cholesky decomposition
    /// is an lower triangular matrix L so that A = L*L'.
    /// If the matrix is not symmetric or positive definite, the constructor
    /// returns a partial decomposition and sets an internal flag that may
    /// be queried by the <see cref="CholeskyDecomposition.IsSPD"/> property.
    /// </remarks>
    [Serializable]
    public class CholeskyDecomposition
    {
        #region Class variables
		
        /// <summary>Array for internal storage of decomposition.</summary>
        private double[,] L;
		
        /// <summary>Row and column dimension (square matrix).</summary>
        private int n => L.GetLength(0);

        /// <summary>Symmetric and positive definite flag.</summary>
        private readonly bool _isspd;
		
        #endregion //  Class variables

        #region Constructor
		
        /// <summary>Cholesky algorithm for symmetric and positive definite matrix.</summary>
        /// <param name="arg">Square, symmetric matrix.</param>
        /// <returns>Structure to access L and isspd flag.</returns>
        public CholeskyDecomposition(Matrix arg)
        {
            // Initialize.
            Matrix A = arg;
            L = new double[arg.RowCount, arg.RowCount];

            _isspd = (arg.ColumnCount == n);
            // Main loop.
            for (int j = 0; j < n; j++)
            {
                //double[] Lrowj = L[j];
                double d = 0.0;
                for (int k = 0; k < j; k++)
                {
                    //double[] Lrowk = L[k];
                    double s = 0.0;
                    for (int i = 0; i < k; i++)
                    {
                        s += L[k,i] * L[j,i];
                    }
                    L[j,k] = s = (A[j, k] - s) / L[k, k];
                    d = d + s * s;
                    _isspd = _isspd & (A[k, j] == A[j, k]);
                }
                d = A[j, j] - d;
                _isspd = _isspd & (d > 0.0);
                L[j, j] = Math.Sqrt(Math.Max(d, 0.0));
                for (int k = j + 1; k < n; k++)
                {
                    L[j, k] = 0.0;
                }
            }
        }
		
        #endregion //  Constructor

        /// <summary>Is the matrix symmetric and positive definite?</summary>
        /// <returns><c>true</c> if A is symmetric and positive definite.</returns>
        public bool IsSPD => _isspd;

        #region Public Methods
		
        /// <summary>Return triangular factor.</summary>
        /// <returns>L</returns>
        public Matrix GetL()
        {
            return new Matrix(L);
        }
		
        /// <summary>Solve A*X = B</summary>
        /// <param name="B">  A Matrix with as many rows as A and any number of columns.</param>
        /// <returns>X so that L*L'*X = B</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.SystemException">Matrix is not symmetric positive definite.</exception>
        public Matrix Solve(Matrix B)
        {
            if (B.RowCount != n)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!_isspd)
            {
                throw new SystemException("Matrix is not symmetric positive definite.");
            }
			
            // Copy right hand side.
            Matrix X = B.Clone();
            int nx = B.ColumnCount;
			
            // Solve L*Y = B;
            for (int k = 0; k < n; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j] * L[i, k];
                    }
                }
                for (int j = 0; j < nx; j++)
                {
                    X[k, j] /= L[k, k];
                }
            }
			
            // Solve L'*X = Y;
            for (int k = n - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k, j] /= L[k, k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j] * L[k, i];
                    }
                }
            }
            return X;
        }
        #endregion //  Public Methods
    }
}