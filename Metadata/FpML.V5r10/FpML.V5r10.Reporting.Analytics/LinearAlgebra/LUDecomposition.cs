using System;

namespace Orion.Analytics.LinearAlgebra
{
    /// <summary>LU Decomposition.</summary>
    /// <remarks>
    /// For an m-by-n matrix A with m >= n, the LU decomposition is an m-by-n
    /// unit lower triangular matrix L, an n-by-n upper triangular matrix U,
    /// and a permutation vector piv of length m so that A(piv,:) = L*U.
    /// <c> If m &lt; n, then L is m-by-m and U is m-by-n. </c>
    /// The LU decompostion with pivoting always exists, even if the matrix is
    /// singular, so the constructor will never fail.  The primary use of the
    /// LU decomposition is in the solution of square systems of simultaneous
    /// linear equations.  This will fail if IsNonSingular() returns false.
    /// </remarks>
    [Serializable]
    public class LUDecomposition
    {
        #region Class variables
		
        /// <summary>Array for internal storage of decomposition.</summary>
        private readonly Matrix _lu;
		
        /// <summary>Row dimensions.</summary>
        private int m => _lu.RowCount;

        /// <summary>Column dimensions.</summary>
        private int n => _lu.ColumnCount;

        /// <summary>Pivot sign.</summary>
        private readonly int _pivsign;
		
        /// <summary>Internal storage of pivot vector.</summary>
        private readonly int[] _piv;
		
        #endregion

        #region Constructor
		
        /// <summary>LU Decomposition</summary>
        /// <param name="A">  Rectangular matrix
        /// </param>
        /// <returns>     Structure to access L, U and piv.
        /// </returns>
		
        public LUDecomposition(Matrix A)
        {
            // Use a "left-looking", dot-product, Crout/Doolittle algorithm.
			
            _lu = A.Clone();
            _piv = new int[m];
            for (int i = 0; i < m; i++)
            {
                _piv[i] = i;
            }
            _pivsign = 1;
            //double[] LUrowi;
            var LUcolj = new double[m];
			
            // Outer loop.
			
            for (int j = 0; j < n; j++)
            {
				
                // Make a copy of the j-th column to localize references.
				
                for (int i = 0; i < m; i++)
                {
                    LUcolj[i] = _lu[i, j];
                }
				
                // Apply previous transformations.
				
                for (int i = 0; i < m; i++)
                {
                    //LUrowi = LU[i];
					
                    // Most of the time is spent in the following dot product.
					
                    int kmax = Math.Min(i, j);
                    double s = 0.0;
                    for (int k = 0; k < kmax; k++)
                    {
                        s += _lu[i,k] * LUcolj[k];
                    }
					
                    _lu[i,j] = LUcolj[i] -= s;
                }
				
                // Find pivot and exchange if necessary.
				
                int p = j;
                for (int i = j + 1; i < m; i++)
                {
                    if (Math.Abs(LUcolj[i]) > Math.Abs(LUcolj[p]))
                    {
                        p = i;
                    }
                }
                if (p != j)
                {
                    for (int k = 0; k < n; k++)
                    {
                        double t = _lu[p, k]; _lu[p, k] = _lu[j, k]; _lu[j, k] = t;
                    }
                    int k2 = _piv[p]; _piv[p] = _piv[j]; _piv[j] = k2;
                    _pivsign = - _pivsign;
                }
				
                // Compute multipliers.
				
                if (j < m & _lu[j, j] != 0.0)
                {
                    for (int i = j + 1; i < m; i++)
                    {
                        _lu[i, j] /= _lu[j, j];
                    }
                }
            }
        }
        #endregion //  Constructor
				
        #region Public Properties
        /// <summary>Indicates whether the matrix is nonsingular.</summary>
        /// <returns><c>true</c> if U, and hence A, is nonsingular.</returns>
        public bool IsNonSingular
        {
            get
            {
                for (int j = 0; j < n; j++)
                    if (_lu[j, j] == 0) return false;
	
                return true;
            }
        }

        /// <summary>Gets lower triangular factor.</summary>
        public Matrix L
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var X = new Matrix(m, n);
                Matrix L = X;
                for (int i = 0; i < m; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        if (i > j)
                        {
                            L[i, j] = _lu[i, j];
                        }
                        else L[i, j] = i == j ? 1.0 : 0.0;
                    }
                }
                return X;
            }
        }

        /// <summary>Gets upper triangular factor.</summary>
        public Matrix U
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var X = new Matrix(n, n);
                Matrix U = X;
                for (int i = 0; i < n; i++)
                {
                    for (int j = 0; j < n; j++)
                    {
                        U[i, j] = i <= j ? _lu[i, j] : 0.0;
                    }
                }
                return X;
            }
        }

        /// <summary>Gets pivot permutation vector</summary>
        public int[] Pivot
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var p = new int[m];
                for (int i = 0; i < m; i++)
                {
                    p[i] = _piv[i];
                }
                return p;
            }
        }

        /// <summary>Returns pivot permutation vector as a one-dimensional double array.</summary>
        public double[] DoublePivot
        {
            get
            {
                // TODO: bad behavior of this property
                // this property does not always return the same matrix

                var vals = new double[m];
                for (int i = 0; i < m; i++)
                {
                    vals[i] = _piv[i];
                }
                return vals;
            }
        }

        #endregion
		
        #region Public Methods
		
        /// <summary>Determinant</summary>
        /// <returns>det(A)</returns>
        /// <exception cref="System.ArgumentException">Matrix must be square</exception>
        public double Determinant()
        {
            if (m != n)
            {
                throw new ArgumentException("Matrix must be square.");
            }
            double d = _pivsign;
            for (int j = 0; j < n; j++)
            {
                d *= _lu[j, j];
            }
            return d;
        }
		
        /// <summary>Solve A*X = B</summary>
        /// <param name="B">A Matrix with as many rows as A and any number of columns.</param>
        /// <returns>X so that L*U*X = B(piv,:)</returns>
        /// <exception cref="System.ArgumentException">Matrix row dimensions must agree.</exception>
        /// <exception cref="System.SystemException">Matrix is singular.</exception>
        public Matrix Solve(Matrix B)
        {
            if (B.RowCount != m)
            {
                throw new ArgumentException("Matrix row dimensions must agree.");
            }
            if (!IsNonSingular)
            {
                throw new SystemException("Matrix is singular.");
            }
			
            // Copy right hand side with pivoting
            int nx = B.ColumnCount;
            Matrix Xmat = B.GetMatrix(_piv, 0, nx - 1);
            Matrix X = Xmat;
			
            // Solve L*Y = B(piv,:)
            for (int k = 0; k < n; k++)
            {
                for (int i = k + 1; i < n; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j] * _lu[i, k];
                    }
                }
            }
            // Solve U*X = Y;
            for (int k = n - 1; k >= 0; k--)
            {
                for (int j = 0; j < nx; j++)
                {
                    X[k, j] /= _lu[k, k];
                }
                for (int i = 0; i < k; i++)
                {
                    for (int j = 0; j < nx; j++)
                    {
                        X[i, j] -= X[k, j] * _lu[i, k];
                    }
                }
            }
            return Xmat;
        }

        #endregion //  Public Methods
    }
}