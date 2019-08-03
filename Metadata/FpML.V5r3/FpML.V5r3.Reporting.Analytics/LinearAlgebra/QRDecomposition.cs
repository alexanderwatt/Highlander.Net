/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;
using Orion.Analytics.Maths;

namespace Orion.Analytics.LinearAlgebra
{
    /// <summary>QR Decomposition.</summary>
    /// <remarks>
    /// For an m-by-n matrix A with m >= n, the QR decomposition is an m-by-n
    /// orthogonal matrix Q and an n-by-n upper triangular matrix R so that
    /// A = Q*R.<br/>
    /// 
    /// The QR decompostion always exists, even if the matrix does not have
    /// full rank, so the constructor will never fail.  The primary use of the
    /// QR decomposition is in the least squares solution of nonsquare systems
    /// of simultaneous linear equations.  This will fail if <c>IsFullRank()</c>
    /// returns false.
    /// </remarks>
    [Serializable]
    public class QRDecomposition
    {
        #region Class variables
		
        /// <summary>Array for internal storage of decomposition.</summary>
        private readonly double[,] QR;
		
        /// <summary>Row dimensions.</summary>
        private int m => QR.GetLength(0);

        /// <summary>Column dimensions.</summary>
        private int n => QR.GetLength(1);

        /// <summary>Array for internal storage of diagonal of R.</summary>
        private readonly double[] Rdiag;

        #endregion //  Class variables
		
        #region Constructor
		
        /// <summary>QR Decomposition, computed by Householder reflections.</summary>
        /// <remarks>Provides access to R, the Householder vectors and computes Q.</remarks>
        /// <param name="A">Rectangular matrix</param>
        public QRDecomposition(Matrix A)
        {
            // Initialize.
            QR = A.Clone();
            Rdiag = new double[n];
			
            // Main loop.
            for (int k = 0; k < n; k++)
            {
                // Compute 2-norm of k-th column without under/overflow.
                double nrm = 0;
                for (int i = k; i < m; i++)
                {
                    nrm = Fn.Hypot(nrm, QR[i, k]);
                }
				
                if (nrm != 0.0)
                {
                    // Form k-th Householder vector.
                    if (QR[k, k] < 0)
                    {
                        nrm = - nrm;
                    }
                    for (int i = k; i < m; i++)
                    {
                        QR[i, k] /= nrm;
                    }
                    QR[k, k] += 1.0;
					
                    // Apply transformation to remaining columns.
                    for (int j = k + 1; j < n; j++)
                    {
                        double s = 0.0;
                        for (int i = k; i < m; i++)
                        {
                            s += QR[i, k] * QR[i, j];
                        }
                        s = (- s) / QR[k, k];
                        for (int i = k; i < m; i++)
                        {
                            QR[i, j] += s * QR[i, k];
                        }
                    }
                }
                Rdiag[k] = - nrm;
            }
        }

        #endregion //  Constructor
		
        #region Public Properties

        /// <summary>Indicates whether the matrix is full rank.</summary>
        /// <returns><c>true</c> if R, and hence A, has full rank.</returns>
        public bool FullRank
        {
            get
            {
                for (int j = 0; j < n; j++)
                {
                    if (Rdiag[j] == 0)
                        return false;
                }
                return true;
            }
        }

        /// <summary>Gets the Householder vectors.</summary>
        /// <returns>Lower trapezoidal matrix whose columns define the reflections.</returns>
        public Matrix H
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var X = new Matrix(m, n);
                double[,] H = X;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        H[i, j] = i >= j ? QR[i, j] : 0.0;
                    }
                }
                return X;
            }
        }

        /// <summary>Gets the upper triangular factor</summary>
        public Matrix R
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var X = new Matrix(n, n);
                double[,] R = X;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i < j)
                        {
                            R[i, j] = QR[i, j];
                        }
                        else R[i, j] = i == j ? Rdiag[i] : 0.0;
                    }
                }
                return X;
            }
        }

        /// <summary>Gets the (economy-sized) orthogonal factor.</summary>
        public Matrix Q
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var X = new Matrix(m, n);
                double[,] Q = X;
                for (int k = n - 1; k >= 0; k--)
                {
                    for (int i = 0; i < m; i++)
                    {
                        Q[i, k] = 0.0;
                    }
                    Q[k, k] = 1.0;
                    for (int j = k; j < n; j++)
                    {
                        if (QR[k, k] == 0) continue;
                        double s = 0.0;
                        for (int i = k; i < m; i++)
                        {
                            s += QR[i, k] * Q[i, j];
                        }
                        s = (- s) / QR[k, k];
                        for (int i = k; i < m; i++)
                        {
                            Q[i, j] += s * QR[i, k];
                        }
                    }
                }
                return X;
            }
        }
        #endregion //  Public Properties
		
        #region Public Methods
		
        /// <summary>Least squares solution of A*X = B</summary>
        /// <param name="B">A Matrix with as many rows as A and any number of columns.</param>
        /// <returns>X that minimizes the two norm of Q*R*X-B.</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.SystemException"> Matrix is rank deficient.</exception>
        public Matrix Solve(Matrix B)
        {
            if (B.RowCount != m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!FullRank)
            {
                throw new SystemException("Matrix is rank deficient.");
            }
			
            // Copy right hand side
            int nx = B.ColumnCount;
            double[,] X = B.Clone();
			
            // Compute Y = transpose(Q)*B
            for (int k = 0; k < n; k++)
            {
                for (int j = 0; j < nx; j++)
                {
                    double s = 0.0;
                    for (int i = k; i < m; i++)
                    {
                        s += QR[i, k] * X[i, j];
                    }
                    s = (- s) / QR[k, k];
                    for (int i = k; i < m; i++)
                    {
                        X[i, j] += s * QR[i, k];
                    }
                }
            }
            // Solve R*X = Y;
            for (int k = n - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k, j] /= Rdiag[k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j] * QR[i, k];
                    }
                }
            }

            return (new Matrix(X).GetMatrix(0, n - 1, 0, nx - 1));
        }

        #endregion //  Public Methods
    }
}