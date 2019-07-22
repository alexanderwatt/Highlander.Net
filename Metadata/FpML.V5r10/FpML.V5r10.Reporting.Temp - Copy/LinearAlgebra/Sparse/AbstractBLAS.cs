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

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Partial implementation of BLAS</summary>
	public abstract class AbstractBLAS : IBLAS
	{
		public virtual IVector MultAdd(double alpha, IMatrix A, IVector x, double beta, IVector y, IVector z)
		{
			CheckMultAddArguments(A, x, y, z);

			// Quick return if possible
			if (alpha == 0.0 && beta == 0.0) SetVector(0.0, z);
			else if (alpha == 0.0) ScaleCopy(beta, y, z);
			else MultAddI(alpha, A, x, beta, y, z);

			return z;
		}

		protected internal abstract IVector MultAddI(double alpha, IMatrix A, IVector x, double beta, IVector y, IVector z);

		private static void CheckMultAddArguments(IMatrix A, IVector x, IVector y, IVector z)
		{
		    if (A.ColumnCount != x.Length)
				throw new IndexOutOfRangeException("A.numColumns() != x.size()");
		    if (A.RowCount != y.Length)
		        throw new IndexOutOfRangeException("A.numRows() != y.size()");
		    if (y.Length != z.Length)
		        throw new IndexOutOfRangeException("y.size() != z.size()");
		    if (x == y)
		        throw new ArgumentException("x == y");
		    if (x == z)
		        throw new ArgumentException("x == z");
		}

	    public virtual double Norm(IMatrix A, NORMS normtype)
		{
			switch (normtype)
			{
				case NORMS.NORM1:
					return Norm1(A);

				case NORMS.NORMF:
					return NormF(A);

				case NORMS.NORMINF:
					return NormInf(A);
			}

			throw new ArgumentException("normtype must be one of BLAS.NORM1, BLAS.NORMF or BLAS.NORMINF");
		}

		protected internal abstract double Norm1(IMatrix A);

		protected internal abstract double NormF(IMatrix A);

		protected internal abstract double NormInf(IMatrix A);

		public virtual IVector MultAdd(double alpha, IMatrix A, IVector x, IVector y, IVector z)
		{
			return MultAdd(alpha, A, x, 1.0, y, z);
		}

		public virtual IVector MultAdd(IMatrix A, IVector x, double beta, IVector y, IVector z)
		{
			return MultAdd(1.0, A, x, beta, y, z);
		}

		public virtual IVector MultAdd(double alpha, IMatrix A, IVector x, double beta, IVector y)
		{
			return MultAdd(alpha, A, x, beta, y, y);
		}

		public virtual IVector MultAdd(double alpha, IMatrix A, IVector x, IVector y)
		{
			return MultAdd(alpha, A, x, 1.0, y, y);
		}

		public virtual IVector MultAdd(IMatrix A, IVector x, double beta, IVector y)
		{
			return MultAdd(1.0, A, x, beta, y, y);
		}

		public virtual IVector MultAdd(IMatrix A, IVector x, IVector y, IVector z)
		{
			return MultAdd(1.0, A, x, 1.0, y, z);
		}

		public virtual IVector MultAdd(IMatrix A, IVector x, IVector y)
		{
			return MultAdd(1.0, A, x, 1.0, y, y);
		}

		public virtual IVector Mult(double alpha, IMatrix A, IVector x, IVector y)
		{
			return MultAdd(alpha, A, x, 0.0, y, y);
		}

		public virtual IVector Mult(IMatrix A, IVector x, IVector y)
		{
			return MultAdd(1.0, A, x, 0.0, y, y);
		}

		public virtual IVector TransMultAdd(double alpha, IMatrix A, IVector x, double beta, IVector y, IVector z)
		{
			checkTransMultAddArguments(A, x, y, z);

			// Quick return if possible
			if (alpha == 0.0 && beta == 0.0)
				SetVector(0.0, z);
			else if (alpha == 0.0)
				ScaleCopy(beta, y, z);
			else
				TransMultAddI(alpha, A, x, beta, y, z);

			return z;
		}

		protected internal abstract IVector TransMultAddI(double alpha, IMatrix A, IVector x, double beta, IVector y, IVector z);

		private static void checkTransMultAddArguments(IMatrix A, IVector x, IVector y, IVector z)
		{
		    if (A.RowCount != x.Length)
				throw new IndexOutOfRangeException("A.numRows() != x.size()");
		    if (A.ColumnCount != y.Length)
		        throw new IndexOutOfRangeException("A.numColumns() != y.size()");
		    if (y.Length != z.Length)
		        throw new IndexOutOfRangeException("y.size() != z.size()");
		    if (x == y)
		        throw new ArgumentException("x == y");
		    if (x == z)
		        throw new ArgumentException("x == z");
		}

	    public virtual IVector TransMult(double alpha, IMatrix A, IVector x, IVector y)
		{
			return TransMultAdd(alpha, A, x, 0.0, y, y);
		}

		public virtual IVector TransMult(IMatrix A, IVector x, IVector y)
		{
			return TransMultAdd(1.0, A, x, 0.0, y, y);
		}

		public virtual IVector TransMultAdd(double alpha, IMatrix A, IVector x, double beta, IVector y)
		{
			return TransMultAdd(alpha, A, x, beta, y, y);
		}

		public virtual IVector TransMultAdd(double alpha, IMatrix A, IVector x, IVector y, IVector z)
		{
			return TransMultAdd(alpha, A, x, 1.0, y, z);
		}

		public virtual IVector TransMultAdd(double alpha, IMatrix A, IVector x, IVector y)
		{
			return TransMultAdd(alpha, A, x, 1.0, y, y);
		}

		public virtual IVector TransMultAdd(IMatrix A, IVector x, double beta, IVector y, IVector z)
		{
			return TransMultAdd(1.0, A, x, beta, y, z);
		}

		public virtual IVector TransMultAdd(IMatrix A, IVector x, double beta, IVector y)
		{
			return TransMultAdd(1.0, A, x, beta, y, y);
		}

		public virtual IVector TransMultAdd(IMatrix A, IVector x, IVector y, IVector z)
		{
			return TransMultAdd(1.0, A, x, 1.0, y, z);
		}

		public virtual IVector TransMultAdd(IMatrix A, IVector x, IVector y)
		{
			return TransMultAdd(1.0, A, x, 1.0, y, y);
		}

		public virtual IMatrix Transpose(IMatrix A, IMatrix B)
		{
			checkTransposeArguments(A, B);
			return TransposeI(A, B);
		}

		private static void checkTransposeArguments(IMatrix A, IMatrix B)
		{
		    if (A.RowCount != B.ColumnCount)
				throw new IndexOutOfRangeException("A.numRows() != B.numColumns()");
		    if (A.ColumnCount != B.RowCount)
		        throw new IndexOutOfRangeException("A.numColumns() != B.numRows()");
		    if (A == B)
		        throw new ArgumentException("A == B");
		}

	    protected internal abstract IMatrix TransposeI(IMatrix A, IMatrix B);

		public virtual IMatrix AddDiagonal(double alpha, IMatrix A)
		{
		    return alpha == 0.0 ? A : AddDiagonalI(alpha, A);
		}

	    protected internal abstract IMatrix AddDiagonalI(double alpha, IMatrix A);

		public virtual IMatrix Scale(double alpha, IMatrix A)
		{
		    if (alpha == 1.0)
				return A;
		    return alpha == 0.0 ? Zero(A) : ScaleI(alpha, A);
		}

	    protected internal abstract IMatrix ScaleI(double alpha, IMatrix A);

		public virtual IMatrix Copy(IMatrix A, IMatrix B)
		{
			checkCopyArguments(A, B);
			return CopyI(A, B);
		}

		private static void checkCopyArguments(IMatrix A, IMatrix B)
		{
		    if (A.RowCount != B.RowCount)
				throw new IndexOutOfRangeException("A.numRows() != B.numRows()");
		    if (A.ColumnCount != B.ColumnCount)
		        throw new IndexOutOfRangeException("A.numColumns() != B.numColumns()");
		    if (A == B)
		        throw new ArgumentException("A == B");
		}

	    public virtual IMatrix Copy(IMatrix A, IMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			checkCopyArguments(A, B, startRow, stopRow, startColumn, stopColumn);
			if (startRow == 0 && stopRow == A.RowCount && startColumn == 0 && stopColumn == A.ColumnCount)
				return CopyI(A, B);
	        return CopyI(A, B, startRow, stopRow, startColumn, stopColumn);
		}

		private static void checkCopyArguments(IMatrix A, IMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
		    if (startRow < 0)
				throw new IndexOutOfRangeException("startRow < 0");
		    if (startColumn < 0)
		        throw new IndexOutOfRangeException("startColumn < 0");
		    if (stopRow > A.RowCount)
		        throw new IndexOutOfRangeException("stopRow > A.numRows()");
		    if (stopColumn > A.ColumnCount)
		        throw new IndexOutOfRangeException("stopColumn > A.numColumns()");
		    if (B.RowCount != stopRow - startRow)
		        throw new IndexOutOfRangeException("B.numRows() != stopRow - startRow");
		    if (B.ColumnCount != stopColumn - startColumn)
		        throw new IndexOutOfRangeException("B.numColumns() != stopColumn - startColumn");
		    if (A == B)
		        throw new ArgumentException("A == B");
		}

	    protected internal abstract IMatrix CopyI(IMatrix A, IMatrix B, int startRow, int stopRow, int startColumn, int stopColumn);

		public virtual IMatrix Rank1(double alpha, IVector x, IVector y, IMatrix A)
		{
			// Quick return if possible
			if (alpha == 0.0)
				return A;

			checkRankArguments(x, y, A);
			return Rank1I(alpha, x, y, A);
		}

		protected internal abstract IMatrix Rank1I(double alpha, IVector x, IVector y, IMatrix A);

		private static void checkRankArguments(IVector x, IVector y, IMatrix A)
		{
			if (x.Length != A.RowCount)
				throw new IndexOutOfRangeException("x.size() != A.numRows()");
			if (y.Length != A.ColumnCount)
				throw new IndexOutOfRangeException("y.size() != A.numColumns()");
		}

		public virtual IMatrix Rank1(IVector x, IVector y, IMatrix A)
		{
			return Rank1(1.0, x, y, A);
		}

		public virtual IMatrix Rank1(double alpha, IVector x, IMatrix A)
		{
			return Rank1(alpha, x, x, A);
		}

		public virtual IMatrix Rank1(IVector x, IMatrix A)
		{
			return Rank1(1.0, x, A);
		}

		public virtual IVector Add(double alpha, IVector x, double beta, IVector y, IVector z)
		{
			checkAddArguments(x, y, z);

			if (alpha == 0.0 && beta == 0.0)
				Zero(z);
			else if (alpha == 0)
				z = y != z ? ScaleCopy(beta, y, z) : Scale(beta, z);
			else if (beta == 0.0)
				z = x != z ? ScaleCopy(alpha, x, z) : Scale(alpha, z);
			else
				AddI(alpha, x, beta, y, z);

			return z;
		}

		private static void checkAddArguments(IVector x, IVector y, IVector z)
		{
		    if (x.Length != y.Length)
				throw new IndexOutOfRangeException("x.size() != y.size()");
		    if (x.Length != z.Length)
		        throw new IndexOutOfRangeException("x.size() != z.size()");
		}

	    protected internal abstract IVector AddI(double alpha, IVector x, double beta, IVector y, IVector z);

		public virtual IVector Add(double alpha, IVector x, double beta, IVector y)
		{
			return Add(alpha, x, beta, y, y);
		}

		public virtual IVector Add(double alpha, IVector x, IVector y, IVector z)
		{
			return Add(alpha, x, 1.0, y, z);
		}

		public virtual IVector Add(IVector x, double beta, IVector y)
		{
			return Add(1.0, x, beta, y, y);
		}

		public virtual IVector Add(IVector x, IVector y, IVector z)
		{
			return Add(1.0, x, 1.0, y, z);
		}

		public virtual IVector Add(IVector x, IVector y)
		{
			return Add(1.0, x, 1.0, y, y);
		}

		public virtual IVector Add(IVector x, double beta, IVector y, IVector z)
		{
			return Add(1.0, x, beta, y, z);
		}

		public virtual IVector Add(double alpha, IVector x, IVector y)
		{
			return Add(alpha, x, 1.0, y, y);
		}

		public virtual double Dot(IVector x, IVector y)
		{
			checkDotArguments(x, y);
			return DotI(x, y);
		}

		protected internal abstract double DotI(IVector x, IVector y);

		private static void checkDotArguments(IVector x, IVector y)
		{
			if (x.Length != y.Length)
				throw new IndexOutOfRangeException("x.size() != y.size()");
		}

		public virtual double Norm(IVector x, NORMS normtype)
		{
			switch (normtype)
			{
				case NORMS.NORM1:
					return Norm1(x);

				case NORMS.NORM2:
					return Norm2(x);

				case NORMS.NORMINF:
					return NormInf(x);
			}

			throw new ArgumentException("normtype must be one of BLAS.NORM1, BLAS.NORM2 or BLAS.NORMINF");
		}

		protected internal abstract double Norm1(IVector x);

		protected internal abstract double Norm2(IVector x);

		protected internal abstract double NormInf(IVector x);

		public virtual IVector ScaleCopy(double alpha, IVector x, IVector y)
		{
			return Scale(alpha, Copy(x, y));
		}

		public virtual IVector Scale(double alpha, IVector x)
		{
		    if (alpha == 1.0)
				return x;
		    if (alpha == 0.0)
		        return Zero(x);
		    return ScaleI(alpha, x);
		}

	    protected internal abstract IVector ScaleI(double alpha, IVector x);

		public virtual IVector SetVector(double alpha, IVector x)
		{
		    return alpha == 0.0 ? Zero(x) : SetI(alpha, x);
		}

	    protected internal abstract IVector SetI(double alpha, IVector x);

		public virtual IVector Copy(IVector x, IVector y)
		{
			checkCopyArguments(x, y);
			return CopyI(x, y);
		}

		protected internal abstract IVector CopyI(IVector x, IVector y);

		private static void checkCopyArguments(IVector x, IVector y)
		{
		    if (x.Length != y.Length)
				throw new IndexOutOfRangeException("x.size() != y.size()");
		    if (x == y)
		        throw new ArgumentException("x == y");
		}

	    public virtual IVector Copy(IVector x, IVector y, int start, int stop)
		{
			checkCopyArguments(x, y, start, stop);
			return CopyI(x, y, start, stop);
		}

		protected internal abstract IVector CopyI(IVector x, IVector y, int start, int stop);

		private static void checkCopyArguments(IVector x, IVector y, int start, int stop)
		{
		    if (start < 0)
				throw new IndexOutOfRangeException("start < 0");
		    if (stop > x.Length)
		        throw new IndexOutOfRangeException("stop > x.size()");
		    if (y.Length != stop - start)
		        throw new IndexOutOfRangeException("y.size() != stop - start");
		    if (x == y)
		        throw new ArgumentException("x == y");
		}

	    protected internal abstract IMatrix CopyI(IMatrix A, IMatrix B);
		public abstract IVector Zero(IVector param1);
		public abstract IntDoubleVectorPair Gather(int[] param1, double[] param2);
		public abstract double[] GetArrayCopy(IVector param1);
		public abstract IVector SetVector(double[] param1, IVector param2);
		public abstract IMatrix Zero(IMatrix param1);
		public abstract double[] Scatter(IntDoubleVectorPair param1, double[] param2);
		public abstract IntDoubleVectorPair Gather(double[] param1);
		public abstract int Cardinality(IVector param1);
		public abstract int Cardinality(IMatrix param1);
		public abstract double[,] GetArrayCopy(IMatrix param1);
	}
}