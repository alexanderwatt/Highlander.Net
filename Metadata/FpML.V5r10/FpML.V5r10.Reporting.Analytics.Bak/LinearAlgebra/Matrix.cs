using System;
using System.Text;
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Maths.Collections;

namespace Orion.Analytics.LinearAlgebra
{
	internal class Maths 
	{
		/// <summary>sqrt(a^2 + b^2) without under/overflow.</summary>
		public static double Hypot(double a, double b) 
		{
			double r;
			if (Math.Abs(a) > Math.Abs(b)) 
			{
				r = b/a;
				r = Math.Abs(a) * Math.Sqrt(1 + r * r);
			} 
			else if (b != 0) 
			{
				r = a/b;
				r = Math.Abs(b) * Math.Sqrt(1 + r * r);
			} 
			else 
			{
				r = 0.0;
			}
			return r;
		}
	}

	/// <summary>Real matrix.</summary>
	/// <remarks>
	/// The class <c>Matrix</c> provides the elementary operations
	/// on matrices (addition, multiplication, inversion, transposition, ...).
	/// Helpers to handle sub-matrices are also provided.
	/// </remarks>
	[Serializable]
	public class Matrix : ICloneable , IFormattable
	{
	    /// <summary>Array for internal storage of elements.</summary>
	    public double[,] Data { get; private set; }

	    #region Constructors and static construtive methods
		
		/// <summary>Construct an m-by-n matrix of zeros. </summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		public Matrix(int m, int n)
		{
		    Data = new double[m, n];
		}
		
		/// <summary>Construct an m-by-n constant matrix.</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <param name="s">Fill the matrix with this scalar value.</param>
		public Matrix(int m, int n, double s)
		{
		    Data = new double[m, n];
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
				    Data[i, j] = s;
				}
			}
		}

        /// <summary>Constructs a matrix from a 1-D array.</summary>
        /// <param name="a">One-dimensional array of doubles.</param>
        /// <exception cref="System.ArgumentException">All rows must have the same length.</exception>
        /// <seealso cref="Create"/>
        public Matrix(double[] a)
        {
            double[,] matrix = new double[a.Length,1];
            Data = matrix;
        }
        
        /// <summary>Constructs a matrix from a 2-D array.</summary>
		/// <param name="a">Two-dimensional array of doubles.</param>
		/// <exception cref="System.ArgumentException">All rows must have the same length.</exception>
		/// <seealso cref="Create"/>
		public Matrix(double[,] a)
		{
		    Data = a;
		}
		
		/// <summary>Construct a matrix from a one-dimensional packed array</summary>
		/// <param name="vals">One-dimensional array of doubles, packed by columns (ala Fortran).</param>
		/// <param name="m">Number of rows.</param>
		/// <exception cref="System.ArgumentException">Array length must be a multiple of m.</exception>
		public Matrix(double[] vals, int m)
		{
			int n = (m != 0?vals.Length / m:0);
			if (m * n != vals.Length)
			{
				throw new ArgumentException("Array length must be a multiple of m.");
			}
		    Data = new double[m, n];
			for (int i = 0; i < m; i++)
			{
				for (int j = 0; j < n; j++)
				{
				    Data[i, j] = vals[i + j * m];
				}
			}
		}
		
		/// <summary>Constructs a matrix from a copy of a 2-D array.</summary>
		/// <param name="a">Two-dimensional array of doubles.</param>
		public static Matrix Create(double[,] a)
		{
			return new Matrix(a).Clone();
		}

		/// <summary>Generates identity matrix</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <returns>An m-by-n matrix with ones on the diagonal and zeros elsewhere.</returns>
		public static Matrix Identity(int m, int n)
		{
			var x = new Matrix(m, n);
			for (int i = 0; i < m; i++)
				for (int j = 0; j < n; j++)
					x[i, j] = (i == j ? 1.0 : 0.0);				
			return x;
		}

		/// <summary>Generates a diagonal matrix with a specifies diagonal value.</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <param name="diagonalValue">The diagonal value.</param>
		/// <returns>An m-by-n matrix with ones on the diagonal and zeros elsewhere.</returns>
		public static Matrix DiagonalMatrix(int m, int n, double diagonalValue)
		{
			var x = new Matrix(m, n);
			for (int i = 0; i < m; i++)
				for (int j = 0; j < n; j++)
					x[i, j] = (i == j ? diagonalValue : 0.0);
			return x;
		}

		/// <summary>Generates matrix with random elements</summary>
		/// <param name="m">Number of rows.</param>
		/// <param name="n">Number of colums.</param>
		/// <returns>An m-by-n matrix with uniformly distributed
		/// random elements in <c>[0, 1)</c> interval.</returns>
		public static Matrix Random(int m, int n)
		{
			var random = new Random();

			var x = new Matrix(m, n);
			for (int i = 0; i < m; i++)
				for (int j = 0; j < n; j++)
					x[i, j] = random.NextDouble();
			return x;
		}

		#endregion //  Constructors

		#region Columns, Rows and Diagonal
		
		///<summary>
		///</summary>
		///<param name="i"></param>
		///<returns></returns>
		public SparseVector Row(int i)
		{
			var sp = new SparseVector(ColumnCount);
			for (int j = 0; j < ColumnCount; j++)
			{
				sp.AddValue(j,this[i, j]);
			}
			return sp;
		}

		///<summary>
		///</summary>
		///<param name="i"></param>
		///<returns></returns>
		public SparseVector Column(int i)
		{
			var sp = new SparseVector(RowCount);
			for (int j = 0; j < RowCount; j++)
				sp.AddValue(j,this[j, i]);
			return sp;
		}
		
		///<summary>
		///</summary>
		public SparseVector Diagonal
		{
			get
			{
				int size = Math.Min(RowCount, ColumnCount);
				var sp = new SparseVector(size);
				for (int i = 0; i < size; i++)
				{
					sp.AddValue(i, Data[i, i]);
				}
				return sp;
			}
		}	

		/// <summary>Gets the number of rows.</summary>
		public int RowCount => Data.GetLength(0);

	    /// <summary>Gets the number of columns.</summary>
		public int ColumnCount => Data.GetLength(1);
		

		/// <summary>
		/// Sets the vlaue in the matrix.
		/// </summary>
		/// <param name="value">The value to set.</param>
		/// <param name="row">The row.</param>
		/// <param name="column">The column.</param>
		public void SetValue(double value, int row, int column)
		{
			this[row, column] = value;
		}

		#endregion

		#region	 Public Methods

		#region Sub-matrices operation

		/// <summary>Gets a submatrix.</summary>
		/// <param name="i0">Initial row index.</param>
		/// <param name="i1">Final row index.</param>
		/// <param name="j0">Initial column index.</param>
		/// <param name="j1">Final column index.</param>
		/// <returns>A(i0:i1,j0:j1)</returns>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
		public virtual Matrix GetMatrix(int i0, int i1, int j0, int j1)
		{
			var x = new Matrix(i1 - i0 + 1, j1 - j0 + 1);
			double[,] b = x;
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
						b[i - i0, j - j0] = Data[i, j];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
			return x;
		}
		
		/// <summary>Gets a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="c">Array of column indices.</param>
		/// <returns>A(r(:),c(:))</returns>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices.</exception>
		public virtual Matrix GetMatrix(int[] r, int[] c)
		{
			var x = new Matrix(r.Length, c.Length);
			double[,] b = x;
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						b[i, j] = Data[r[i], c[j]];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
			return x;
		}
		
		/// <summary>Get a submatrix.</summary>
		/// <param name="i0">Initial row index.</param>
		/// <param name="i1">Final row index.</param>
		/// <param name="c">Array of column indices.</param>
		/// <returns>A(i0:i1,c(:))</returns>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices.</exception>
		public virtual Matrix GetMatrix(int i0, int i1, int[] c)
		{
			var x = new Matrix(i1 - i0 + 1, c.Length);
			double[,] b = x;
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
						b[i - i0, j] = Data[i, c[j]];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
			return x;
		}
		
		/// <summary>Get a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="j0">Initial column index.</param>
		/// <param name="j1">Final column index.</param>
		/// <returns>A(r(:),j0:j1)</returns>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices.</exception>
		public virtual Matrix GetMatrix(int[] r, int j0, int j1)
		{
			var x = new Matrix(r.Length, j1 - j0 + 1);
			double[,] b = x;
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
						b[i, j - j0] = Data[r[i], j];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
			return x;
		}
		
		/// <summary>Set a submatrix.</summary>
		/// <param name="i0">Initial row index.</param>
		/// <param name="i1">Final row index.</param>
		/// <param name="j0">Initial column index.</param>
		/// <param name="j1">Final column index.</param>
		/// <param name="x">A(i0:i1,j0:j1)</param>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices.</exception>
		public virtual void  SetMatrix(int i0, int i1, int j0, int j1, Matrix x)
		{
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
					    Data[i, j] = x[i - i0, j - j0];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
		}
		
		/// <summary>Sets a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="c">Array of column indices.</param>
		/// <param name="x">A(r(:),c(:))</param>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
		public virtual void  SetMatrix(int[] r, int[] c, Matrix x)
		{
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
					    Data[r[i], c[j]] = x[i, j];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
		}
		
		/// <summary>Sets a submatrix.</summary>
		/// <param name="r">Array of row indices.</param>
		/// <param name="j0">Initial column index.</param>
		/// <param name="j1">Final column index.</param>
		/// <param name="x">A(r(:),j0:j1)</param>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices</exception>
		public virtual void  SetMatrix(int[] r, int j0, int j1, Matrix x)
		{
			try
			{
				for (int i = 0; i < r.Length; i++)
				{
					for (int j = j0; j <= j1; j++)
					{
					    Data[r[i], j] = x[i, j - j0];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
		}
		
		/// <summary>Set a submatrix.</summary>
		/// <param name="i0">Initial row index.</param>
		/// <param name="i1">Final row index.</param>
		/// <param name="c">Array of column indices.</param>
		/// <param name="x">A(i0:i1,c(:))</param>
		/// <exception cref="System.IndexOutOfRangeException">Submatrix indices.</exception>
		public virtual void  SetMatrix(int i0, int i1, int[] c, Matrix x)
		{
			try
			{
				for (int i = i0; i <= i1; i++)
				{
					for (int j = 0; j < c.Length; j++)
					{
					    Data[i, c[j]] = x[i - i0, j];
					}
				}
			}
			catch (IndexOutOfRangeException e)
			{
				throw new IndexOutOfRangeException("Submatrix indices", e);
			}
		}
		

		#endregion
		
		#region Norms computations

		/// <summary>One norm</summary>
		/// <returns>Maximum column sum.</returns>
		public double Norm1()
		{
			double f = 0;
			for (int j = 0; j < ColumnCount; j++)
			{
				double s = 0;
				for (int i = 0; i < RowCount; i++)
				{
					s += Math.Abs(Data[i, j]);
				}
				f = Math.Max(f, s);
			}
			return f;
		}
		
		/// <summary>Two norm</summary>
		/// <returns>Maximum singular value.</returns>
		public double Norm2()
		{
			return (new SingularValueDecomposition(this).Norm2());
		}
		
		/// <summary>Infinity norm</summary>
		/// <returns>Maximum row sum.</returns>
		public double NormInf()
		{
			double f = 0;
			for (int i = 0; i < RowCount; i++)
			{
				double s = 0;
				for (int j = 0; j < ColumnCount; j++)
				{
					s += Math.Abs(Data[i, j]);
				}
				f = Math.Max(f, s);
			}
			return f;
		}
		
		/// <summary>Frobenius norm</summary>
		/// <returns>Sqrt of sum of squares of all elements.</returns>
		public double NormF()
		{
			double f = 0;
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColumnCount; j++)
				{
					f = Maths.Hypot(f, Data[i, j]);
				}
			}
			return f;
		}
		

		#endregion
		
		#region Elementary linear operations

		/// <summary>In place addition of <c>m</c> to this <c>Matrix</c>.</summary>
		/// <seealso cref="operator + (Matrix, Matrix)"/>
		public virtual void Add(Matrix m)
		{
			CheckMatrixDimensions(m);
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColumnCount; j++)
				{
					this[i, j] += m[i, j];
				}
			}
		}

		/// <summary>Multiplies in place this <c>Matrix</c> by a scalar.</summary>
		/// <seealso cref="operator * (double, Matrix)"/>
		public virtual void Multiply(double s)
		{
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColumnCount; j++)
				{
					this[i, j] *= s;
				}
			}
		}
		
		/// <summary>In place substraction of <c>m</c> to this <c>Matrix</c>.</summary>
		/// <seealso cref="operator - (Matrix, Matrix)"/>
		public virtual void Subtract(Matrix m)
		{
			CheckMatrixDimensions(m);
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColumnCount; j++)
				{
					this[i, j] -= m[i, j];
				}
			}
		}

		/// <summary>In place division <c>double</c> to this <c>Matrix</c>.</summary>
		/// <seealso cref="operator / (Matrix, double)"/>
		public virtual void Divide(double scalar)
		{
			if (scalar != 0.0)
			{
				Multiply(1.0 / scalar);
			}
			throw new ArgumentException("Can not divide by zero");
		}

		/// <summary>In place transposition of this <c>Matrix</c>.</summary>
		/// <seealso cref="Transpose(Matrix)"/>
		public virtual void Transpose()
		{
			if(RowCount == ColumnCount)
			{
				// No need for array copy
				for(int i = 0; i < RowCount; i++)
					for(int j = i + 1; j < ColumnCount; j++)
					{
						double thisIJ = this[i, j];
						this[i, j] = this[j, i];
						this[j, i] = thisIJ;
					}
			}
			else
			{
				var X = new Matrix(ColumnCount, RowCount);
				for (int i = 0; i < RowCount; i++)
					for (int j = 0; j < ColumnCount; j++)
						X[j, i] = this[i, j];

			    Data = X.Data;
			}
		}

		/// <summary>Gets the transposition of the provided <c>Matrix</c>.</summary>
		public static Matrix Transpose(Matrix m)
		{
			var X = new Matrix(m.ColumnCount, m.RowCount);
			for (int i = 0; i < m.RowCount; i++)
			{
				for (int j = 0; j < m.ColumnCount; j++)
				{
					X[j, i] = m[i, j];
				}
			}
			return X;
		}

		/// <summary>In place unary minus of the <c>Matrix</c>.</summary>
		public virtual void UnaryMinus()
		{
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColumnCount; j++)
				{
					this[i, j] = - this[i, j];
				}
			}
		}

		#endregion

		#region Array operation on matrices
		
		/// <summary>In place element-by-element multiplication.</summary>
		/// <remarks>This instance and <c>m</c> must have the same dimensions.</remarks>
		/// <seealso cref="ArrayMultiply(Matrix, Matrix)"/>
		public void ArrayMultiply(Matrix m)
		{
			CheckMatrixDimensions(m);

			for (int i = 0; i < RowCount; i++)
				for (int j = 0; j < ColumnCount; j++)
					this[i, j] *= m[i, j];
		}

		/// <summary>Element-by-element multiplication.</summary>
		/// <remarks><c>m1</c> and <c>m2</c> must have the same dimensions.</remarks>
		/// <seealso cref="ArrayMultiply(Matrix )"/>
		public static Matrix ArrayMultiply(Matrix m1, Matrix m2)
		{
			m1.CheckMatrixDimensions(m2);

			var X = new Matrix(m1.RowCount, m1.ColumnCount);
			for (int i = 0; i < m1.RowCount; i++)
				for (int j = 0; j < m1.ColumnCount; j++)
					X[i, j] = m1[i, j] * m2[i, j];

			return X;
		}
		
		/// <summary>In place element-by-element right division, <c>A ./= B</c>.</summary>
		public void ArrayDivide(Matrix m)
		{
			CheckMatrixDimensions(m);

			for (int i = 0; i < RowCount; i++)
				for (int j = 0; j < ColumnCount; j++)
					this[i, j] /= m[i, j];
		}

		/// <summary>Element-by-element right division, <c>C = A./B</c>.</summary>
		public static Matrix ArrayDivide(Matrix m1, Matrix m2)
		{
			m1.CheckMatrixDimensions(m2);

			var X = new Matrix(m1.RowCount, m1.ColumnCount);
			for (int i = 0; i < m1.RowCount; i++)
				for (int j = 0; j < m1.ColumnCount; j++)
					X[i, j] = m1[i, j] / m2[i, j];

			return X;
		}

		#endregion
		
		#region Decompositions

		/// <summary>LU Decomposition</summary>
		/// <seealso cref="LUDecomposition"/>
		public virtual LUDecomposition LUD()
		{
			return new LUDecomposition(this);
		}
		
		/// <summary>QR Decomposition</summary>
		/// <returns>QRDecomposition</returns>
		/// <seealso cref="QRDecomposition"/>
		public virtual QRDecomposition QRD()
		{
			return new QRDecomposition(this);
		}
		
		/// <summary>Cholesky Decomposition</summary>
		/// <seealso cref="CholeskyDecomposition"/>
		public virtual CholeskyDecomposition chol()
		{
			return new CholeskyDecomposition(this);
		}
		
		/// <summary>Singular Value Decomposition</summary>
		/// <seealso cref="SingularValueDecomposition"/>
		public virtual SingularValueDecomposition SVD()
		{
			return new SingularValueDecomposition(this);
		}
		
		/// <summary>Eigenvalue Decomposition</summary>
		/// <seealso cref="EigenvalueDecomposition"/>
		public virtual EigenvalueDecomposition Eigen()
		{
			return new EigenvalueDecomposition(this);
		}

		#endregion

		#region Matrix Operations

		/// <summary>Solve A*X = B</summary>
		/// <param name="B">right hand side</param>
		/// <returns>solution if A is square, least squares solution otherwise.</returns>
		public virtual Matrix Solve(Matrix B)
		{
			return (RowCount == ColumnCount ? (new LUDecomposition(this)).Solve(B):(new QRDecomposition(this)).Solve(B));
		}
		
		/// <summary>Solve X*A = B, which is also A'*X' = B'</summary>
		/// <param name="B">right hand side</param>
		/// <returns>solution if A is square, least squares solution otherwise.</returns>
		public virtual Matrix SolveTranspose(Matrix B)
		{
			return Transpose(this).Solve(Transpose(B));
		}
		
		/// <summary>Matrix inverse or pseudoinverse.</summary>
		/// <returns> inverse(A) if A is square, pseudoinverse otherwise.</returns>
		public virtual Matrix Inverse()
		{
			return Solve(Identity(RowCount, RowCount));
		}
		
		/// <summary>Matrix determinant</summary>
		public virtual double Determinant()
		{
			return new LUDecomposition(this).Determinant();
		}
		
		/// <summary>Matrix rank</summary>
		/// <returns>effective numerical rank, obtained from SVD.</returns>
		public virtual int Rank()
		{
			return new SingularValueDecomposition(this).Rank();
		}
		
		/// <summary>Matrix condition (2 norm)</summary>
		/// <returns>ratio of largest to smallest singular value.</returns>
		public virtual double Condition()
		{
			return new SingularValueDecomposition(this).Condition();
		}
		
		/// <summary>Matrix trace.</summary>
		/// <returns>sum of the diagonal elements.</returns>
		public virtual double Trace()
		{
			double t = 0;
			for (int i = 0; i < Math.Min((sbyte) RowCount, (sbyte) ColumnCount); i++)
			{
				t += Data[i, i];
			}
			return t;
		}
		#endregion // Linear Algebra

		#region vectorial products -- prefer operator

		///<summary>
		///</summary>
		///<param name="vectorX"></param>
		///<param name="matrixA"></param>
		///<returns></returns>
		///<exception cref="ArgumentException"></exception>
		public static IVector operator *(IVector vectorX, Matrix matrixA)
		{
			if (matrixA.RowCount != vectorX.Length)
				throw new ArgumentException("MatDimNeq");
			IVector ytemp = new DoubleVector();
			Blas.Default.Mult((IMatrix) matrixA, vectorX, ytemp);
			return ytemp;
		}

		///<summary>
		///</summary>
		///<param name="matrixA"></param>
		///<param name="vectorX"></param>
		///<returns></returns>
		///<exception cref="ArgumentException"></exception>
		public static IVector operator *(Matrix matrixA, IVector vectorX)
		{
			if (matrixA.ColumnCount != vectorX.Length)
				throw new ArgumentException("MatDimNeq");
			IVector ytemp = new DoubleVector();
			Blas.Default.Mult((IMatrix)matrixA, vectorX, ytemp);
			return ytemp;
		}

		///<summary>
		///</summary>
		///<param name="matrixA"></param>
		///<param name="vectorX"></param>
		///<returns></returns>
		///<exception cref="ArgumentException"></exception>
		public static SparseVector operator *(Matrix matrixA, SparseVector vectorX)
		{
			if (matrixA.ColumnCount != vectorX.Length)
				throw new ArgumentException("MatDimNeq");
			IVector ytemp = new DoubleVector();
			Blas.Default.Mult((IMatrix)matrixA, vectorX, ytemp);
			return (SparseVector) ytemp;
		}
		#endregion

		#endregion //  Public Methods

		#region Operator Overloading

		/// <summary>Gets or set the element indexed by <c>(i, j)</c>
		/// in the <c>Matrix</c>.</summary>
		/// <param name="i">Row index.</param>
		/// <param name="j">Column index.</param>
		public double this [int i, int j]
		{
			get => Data[i, j];
		    set => Data[i, j] = value;
		}

		/// <summary>Addition of matrices</summary>
		public static Matrix operator +(Matrix m1, Matrix m2) 
		{
			m1.CheckMatrixDimensions(m2);
			var X = new Matrix(m1.RowCount, m1.ColumnCount);
			for (int i = 0; i < m1.RowCount; i++)
			{
				for (int j = 0; j < m1.ColumnCount; j++)
				{
					X[i, j] = m1[i, j] + m2[i, j];
				}
			}
			return X;
		} 

		/// <summary>Subtraction of matrices</summary>
		public static Matrix operator -(Matrix m1, Matrix m2) 
		{ 
			m1.CheckMatrixDimensions(m2);

			var X = new Matrix(m1.RowCount, m1.ColumnCount);
			for (int i = 0; i < m1.RowCount; i++)
			{
				for (int j = 0; j < m1.ColumnCount; j++)
				{
					X[i, j] = m1[i, j] - m2[i, j];
				}
			}
			return X;
		} 

		/// <summary>Linear algebraic matrix multiplication.</summary>
		/// <exception cref="System.ArgumentException">Matrix inner dimensions must agree.</exception>
		public static Matrix operator *(Matrix m1, Matrix m2) 
		{ 
			if (m2.RowCount != m1.ColumnCount)
			{
				throw new ArgumentException("Matrix inner dimensions must agree.");
			}

			var X = new Matrix(m1.RowCount, m2.ColumnCount);
			for (int j = 0; j < m2.ColumnCount; j++)
			{
				for (int i = 0; i < m1.RowCount; i++)
				{
					double s = 0;
					for (int k = 0; k < m1.ColumnCount; k++)
					{
						s += m1[i, k] * m2[k, j];
					}
					X[i, j] = s;
				}
			}
			return X;
		}

		/// <summary>Multiplication of a matrix by a scalar, C = s*A</summary>
		public static Matrix operator *(double s, Matrix m)
		{
			var X = new Matrix(m.RowCount, m.ColumnCount);
			for (int i = 0; i < m.RowCount; i++)
			{
				for (int j = 0; j < m.ColumnCount; j++)
				{
					X[i, j] = s * m[i, j];
				}
			}
			return X;
		}

		/// <summary>Multiplication of a matrix by a scalar, C = A*s</summary>
		public static Matrix operator *(Matrix m, double s)
		{
			var X = new Matrix(m.RowCount, m.ColumnCount);
			for (int i = 0; i < m.RowCount; i++)
			{
				for (int j = 0; j < m.ColumnCount; j++)
				{
					X[i, j] = s * m[i, j];
				}
			}
			return X;
		}

		/// <summary>Division of a matrix by a scalar, C = A/s</summary>
		public static Matrix operator /(Matrix m, double s)
		{
			if (s != 0)
			{
				var X = new Matrix(m.RowCount, m.ColumnCount);
				for (int i = 0; i < m.RowCount; i++)
				{
					for (int j = 0; j < m.ColumnCount; j++)
					{
						X[i, j] = 1 / s * m[i, j];
					}
				}
				return X;
			}			
			throw new ArgumentException("Can not divide by zero.");  
		}
		
		/// <summary>Implicit convertion to a <c>double[,]</c> array.</summary>
		public static implicit operator double[,] (Matrix m)
		{
			return m.Data;
		}

		/// <summary>
		/// Explicit convertion to a <c>double[]</c> array of a single column matrix.
		/// </summary>
		/// <param name="m">Exactly one column expected.</param>
		public static explicit operator double[] (Matrix m)
		{
			if(m.ColumnCount != 1) throw new InvalidOperationException(
				"Bad dimensions for conversion to double array");

			var array = new double[m.RowCount];
			for(int i = 0; i < m.RowCount; i++)
				array[i] = m[i, 0];
			return array;
		}

		#endregion   //Operator Overloading

		#region	 Private Methods
		
		/// <summary>Check if size(A) == size(B) *</summary>
		private void  CheckMatrixDimensions(Matrix B)
		{
			if (B.RowCount != RowCount || B.ColumnCount != ColumnCount)
			{
				throw new ArgumentException("Matrix dimensions must agree.");
			}
		}
		#endregion //  Private Methods

		#region Implementation (ICloneable)

		/// <summary>Returns a deep copy of this instance.</summary>
		public Matrix Clone()
		{
			var X = new Matrix(RowCount, ColumnCount);
			for (int i = 0; i < RowCount; i++)
			{
				for (int j = 0; j < ColumnCount; j++)
				{
					X[i, j] = Data[i, j];
				}
			}
			return X;
		}

		object ICloneable.Clone()
		{
			return Clone();
		}

		#endregion

		#region Utilities
		/*       public static Matrix Transpose2(Matrix m)
		{
			int cols = m.ColumnCount;
			int rows = m.RowCount;
			Matrix temp = new Matrix(cols, rows);
			unsafe
			{
				fixed (double* t = &temp.data[0])
				{
					double* tp = t;
					for (int i = 0; i < cols; i++)
						for (int j = 0; j < rows; j++)
							*tp++ = m[j, i];	// TODO: = m.data[m.offset+row*m.ld+column]
				}

			}
			return temp;
		}   */

		/// <summary>
		/// Returns the square root of a real symmetric matrix.
		/// </summary>
		/// <param name="realSymmetricMatrix"></param>
		/// <returns></returns>
		public static Matrix Sqrt(Matrix realSymmetricMatrix)
		{
			//! eigenvalues smaller than tolerance are considered zero
			const double tolerance = 1e-15;
			var jd =
				new SymmetricSchurDecomposition(realSymmetricMatrix);
			Matrix evectors = jd.Eigenvectors;	// do not copy -- consume!
			SparseVector evalues = jd.Eigenvalues;
			int size = realSymmetricMatrix.RowCount;

			const double maxEvTolerance = tolerance;
			/*           for (int i = 0; i < size; i++)
			{    
				if evalues.GetValue(i) * tolerance;

			}   */

			var D = new Matrix(size, size);
			var diagonal = D.Diagonal;
			for (int i = 0; i < size; i++)
			{
				if (Math.Abs(evalues.GetValue(i)) <= maxEvTolerance)
					evalues.SetValue(i,0.0);
				else if (evalues.GetValue(i) < 0.0)
					throw new ApplicationException("TODO: Some eigenvalues are negative.");
				diagonal.SetValue(i,Math.Sqrt(evalues.GetValue(i)));
			}

			// return evectors*D*Matrix.Transpose(evectors)
			//  - use gemm direct so we can skip transpose()
			//  - use D to buffer return matrix
			/*       BLAS.gemm('N', 'T', size, size, size,
				1.0, ref temp.data[temp.offset], temp.ld,
				ref evectors.data[evectors.offset], evectors.ld,
				0.0, ref D.data[D.offset], D.ld);*/
			return D;
		}
		#endregion

		#region Implementation (IFormattable and ToString)

		public override string ToString()
		{
			var sb = new StringBuilder();
			for(int i=0;i<RowCount;i++)
			{
			    sb.Append(i == 0 ? "[[" : " [");
			    for(int j=0;j<ColumnCount;j++)
				{
					if(j != 0)
						sb.Append(',');
					sb.Append(this[i,j]);
				}
			    sb.Append(i == RowCount - 1 ? "]]" : "]\n");
			}
			return sb.ToString();
		}

		public string ToString(string format, IFormatProvider formatProvider)
		{
			var s = new StringBuilder(RowCount * ColumnCount * 15);
			s.AppendFormat("{{ // {0}x{1} {2}\n",
						   RowCount, ColumnCount, base.ToString());

			for (int i = 0; i < RowCount; )
			{
				s.Append("  { ");
				for (int j = 0; j < ColumnCount; )
				{
					s.Append(this[i, j].ToString(format, formatProvider));
					if (++j < ColumnCount)
						s.Append(", ");
				}
			    s.Append(++i < RowCount ? " },\n" : " }\n");
			}

			s.Append("}\n");
			return s.ToString();
		}

		#endregion
	}
}