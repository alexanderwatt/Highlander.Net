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
using Orion.Analytics.Utilities;

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Sequential BLAS implementation</summary>
	public class SequentialBLAS : AbstractBLAS
	{
		protected internal override IMatrix AddDiagonalI(double alpha, IMatrix A)
		{
			if (A is IElementalAccessMatrix)
				AddDiagonalI(alpha, (IElementalAccessMatrix) A, 0, A.RowCount);
			else
				throw new NotSupportedException();
			return A;
		}

		protected internal virtual void AddDiagonalI(double alpha, 
			IElementalAccessMatrix A, int start, int stop)
		{
			for (int i = start; i < stop && i < A.ColumnCount; ++i)
				A.AddValue(i, i, alpha);
		}

		protected internal override IVector AddI(double alpha, IVector x, double beta, IVector y, IVector z)
		{
			if (x is IDenseAccessVector && y is IDenseAccessVector && z is IDenseAccessVector)
				AddI(alpha, ((IDenseAccessVector) x).Vector, beta, 
					((IDenseAccessVector) y).Vector, ((IDenseAccessVector) z).Vector, 0, z.Length);
			else if (x is IBlockAccessVector && z is IBlockAccessVector && z is IBlockAccessVector)
				AddI(alpha, (IBlockAccessVector) x, beta, (IBlockAccessVector) y, 
					(IBlockAccessVector) z, 0, ((IBlockAccessVector) x).BlockCount);
			else
				throw new NotSupportedException();

			return z;
		}

		protected internal virtual void AddI(double alpha, IBlockAccessVector x, 
			double beta, IBlockAccessVector y, IBlockAccessVector z, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				Add(alpha, x.GetBlock(i), beta, y.GetBlock(i), z.GetBlock(i));
		}

		protected internal virtual void AddI(double alpha, double[] x, 
			double beta, double[] y, double[] z, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				z[i] = alpha*x[i] + beta*y[i];
		}

		protected internal override IMatrix CopyI(IMatrix A, IMatrix B, 
			int startRow, int stopRow, int startColumn, int stopColumn)
		{
			Zero(B);

			if (A is ISparseRowColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseRowColumnAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is ISparseRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseRowAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is ISparseColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseColumnAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is ISparseColumnRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseColumnRowAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is IDenseRowColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseRowColumnAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is IDenseRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseRowAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is IDenseColumnRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseColumnRowAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is IDenseColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseColumnAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is ICoordinateAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ICoordinateAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			else if (A is IElementalAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IElementalAccessMatrix) A, (IElementalAccessMatrix) B, 
					startRow, stopRow, startColumn, stopColumn);
			
			else throw new NotSupportedException();

			return B;
		}

		protected internal void CopyI(ICoordinateAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			int[] row = A.RowIndices, column = A.ColumnIndices;
			double[] Data = A.Data;

			for (int i = 0; i < Data.Length; ++i)
				if (row[i] >= startRow && row[i] < stopRow 
					&& column[i] >= startColumn && column[i] < stopColumn)
						B.SetValue(row[i] - startRow, column[i] - startColumn, Data[i]);
		}

		protected internal virtual void CopyI(ISparseRowColumnAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = startRow; i < stopRow; ++i)
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
					if (Amat.Major[j] >= startColumn && Amat.Major[j] < stopColumn)
						B.SetValue(i - startRow, Amat.Major[j] - startColumn, Amat.Data[j]);
		}

		protected internal void CopyI(ISparseRowAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			for (int i = startRow; i < stopRow; ++i)
			{
				IntDoubleVectorPair Arow = A.GetRow(i);
				for (int j = 0; j < Arow.Indices.Length; ++j)
					if (Arow.Indices[j] >= startColumn && Arow.Indices[j] < stopColumn)
						B.SetValue(i - startRow, Arow.Indices[j] - startColumn, Arow.Data[j]);
			}
		}

		protected internal virtual void CopyI(ISparseColumnRowAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = startColumn; i < stopColumn; ++i)
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
					if (Amat.Major[j] >= startRow && Amat.Major[j] < stopRow)
						B.SetValue(Amat.Major[j] - startRow, i - startColumn, Amat.Data[j]);
		}

		protected internal virtual void CopyI(ISparseColumnAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			for (int i = startColumn; i < stopColumn; ++i)
			{
				IntDoubleVectorPair Acol = A.GetColumn(i);
				for (int j = 0; j < Acol.Indices.Length; ++j)
					if (Acol.Indices[j] >= startRow && Acol.Indices[j] < stopRow)
						B.SetValue(Acol.Indices[j] - startRow, i - startColumn, Acol.Data[j]);
			}
		}

		protected internal void CopyI(IDenseRowColumnAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = startRow; i < stopRow; ++i)
				for (int j = Amat.Indices[i] + startColumn; j < Amat.Indices[i] + stopColumn; ++j)
					if (Amat.Data[j] != 0.0)
						B.SetValue(i - startRow, j - Amat.Indices[i] - startColumn, Amat.Data[j]);
		}

		protected internal virtual void CopyI(IDenseRowAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			for (int i = startRow; i < stopRow; ++i)
			{
				double[] Arow = A.GetRow(i);
				for (int j = startColumn; j < stopColumn; ++j)
					if (Arow[j] != 0.0)
						B.SetValue(i - startRow, j - startColumn, Arow[j]);
			}
		}

		protected internal void CopyI(IDenseColumnRowAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = startColumn; i < stopColumn; ++i)
				for (int j = Amat.Indices[i] + startRow; j < Amat.Indices[i] + stopRow; ++j)
					if (Amat.Data[j] != 0.0)
						B.SetValue(j - Amat.Indices[j] - startRow, i - startColumn, Amat.Data[j]);
		}

		protected internal void CopyI(IDenseColumnAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			for (int i = startColumn; i < stopColumn; ++i)
			{
				double[] Acol = A.GetColumn(i);
				for (int j = startRow; j < stopRow; ++j)
					if (Acol[j] != 0.0)
						B.SetValue(j - startRow, i - startColumn, Acol[j]);
			}
		}

		protected internal void CopyI(IElementalAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow, int startColumn, int stopColumn)
		{
			for (int i = startRow; i < stopRow; ++i)
				for (int j = startColumn; j < stopColumn; ++j)
				{
					double val = A.GetValue(i, j);
					if (val != 0.0)
						B.SetValue(i - startRow, j - startColumn, val);
				}
		}

		protected internal override IMatrix CopyI(IMatrix A, IMatrix B)
		{
			int stopRow = A.RowCount, stopColumn = A.ColumnCount;
			Zero(B);

			if (A is ISparseRowColumnAccessMatrix && B is ISparseRowColumnAccessMatrix)
				CopyI((ISparseRowColumnAccessMatrix) A, (ISparseRowColumnAccessMatrix) B);
			else if (A is ISparseRowColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseRowColumnAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopRow);
			else if (A is ISparseRowAccessMatrix && B is ISparseRowAccessMatrix)
				CopyI((ISparseRowAccessMatrix) A, (ISparseRowAccessMatrix) B, 0, stopRow);
			else if (A is ISparseRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseRowAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopRow);
			else if (A is ISparseColumnRowAccessMatrix && B is ISparseColumnRowAccessMatrix)
				CopyI((ISparseColumnRowAccessMatrix) A, (ISparseColumnRowAccessMatrix) B);
			else if (A is ISparseColumnRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseColumnRowAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopColumn);
			else if (A is ISparseColumnAccessMatrix && B is ISparseColumnAccessMatrix)
				CopyI((ISparseColumnAccessMatrix) A, (ISparseColumnAccessMatrix) B, 0, stopColumn);
			else if (A is ISparseColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((ISparseColumnAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopColumn);
			else if (A is IDenseRowColumnAccessMatrix && B is IDenseRowColumnAccessMatrix)
				CopyI((IDenseRowColumnAccessMatrix) A, (IDenseRowColumnAccessMatrix) B);
			else if (A is IDenseRowColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseRowColumnAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopRow);
			else if (A is IDenseRowAccessMatrix && B is IDenseRowAccessMatrix)
				CopyI((IDenseRowAccessMatrix) A, (IDenseRowAccessMatrix) B, 0, stopRow);
			else if (A is IDenseRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseRowAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopRow);
			else if (A is IDenseColumnRowAccessMatrix && B is IDenseColumnRowAccessMatrix)
				CopyI((IDenseColumnRowAccessMatrix) A, (IDenseColumnRowAccessMatrix) B);
			else if (A is IDenseColumnRowAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseColumnRowAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopColumn);
			else if (A is IDenseColumnAccessMatrix && B is IDenseColumnAccessMatrix)
				CopyI((IDenseColumnAccessMatrix) A, (IDenseColumnAccessMatrix) B, 0, stopColumn);
			else if (A is IDenseColumnAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IDenseColumnAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopColumn);
			else if (A is ICoordinateAccessMatrix && B is IElementalAccessMatrix)
			{
				CopyI((ICoordinateAccessMatrix) A, (IElementalAccessMatrix) B);
			}
			else if (A is IElementalAccessMatrix && B is IElementalAccessMatrix)
				CopyI((IElementalAccessMatrix) A, (IElementalAccessMatrix) B, 0, stopColumn);
			else
				throw new NotSupportedException();

			return B;
		}

		protected internal void CopyI(ICoordinateAccessMatrix A, IElementalAccessMatrix B)
		{
			int[] row = A.RowIndices, column = A.ColumnIndices;
			double[] Data = A.Data;

			for (int i = 0; i < Data.Length; ++i)
				B.SetValue(row[i], column[i], Data[i]);
		}

		protected internal void CopyI(ISparseRowColumnAccessMatrix A, ISparseRowColumnAccessMatrix B)
		{
			B.Matrix = A.Matrix.Clone();
		}

		protected internal virtual void CopyI(ISparseRowColumnAccessMatrix A, IElementalAccessMatrix B, int startRow, int stopRow)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = startRow; i < stopRow; ++i)
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
					B.SetValue(i, Amat.Major[j], Amat.Data[j]);
		}

		protected internal virtual void CopyI(ISparseRowAccessMatrix A, ISparseRowAccessMatrix B, int startRow, int stopRow)
		{
			for (int i = startRow; i < stopRow; ++i)
				B.SetRow(i, A.GetRow(i).Clone());
		}

		protected internal virtual void CopyI(ISparseRowAccessMatrix A, IElementalAccessMatrix B, int startRow, int stopRow)
		{
			for (int i = startRow; i < stopRow; ++i)
			{
				IntDoubleVectorPair Arow = A.GetRow(i);
				for (int j = 0; j < Arow.Indices.Length; ++j)
					B.SetValue(i, Arow.Indices[j], Arow.Data[j]);
			}
		}

		protected internal virtual void CopyI(ISparseColumnRowAccessMatrix A, ISparseColumnRowAccessMatrix B)
		{
			B.Matrix = A.Matrix.Clone();
		}

		protected internal virtual void CopyI(ISparseColumnRowAccessMatrix A, IElementalAccessMatrix B, int startColumn, int stopColumn)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = startColumn; i < stopColumn; ++i)
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
					B.SetValue(Amat.Major[j], i, Amat.Data[j]);
		}

		protected internal virtual void CopyI(ISparseColumnAccessMatrix A, ISparseColumnAccessMatrix B, int startColumn, int stopColumn)
		{
			for (int i = startColumn; i < stopColumn; ++i)
				B.SetColumn(i, A.GetColumn(i).Clone());
		}

		protected internal virtual void CopyI(ISparseColumnAccessMatrix A, 
			IElementalAccessMatrix B, int startColumn, int stopColumn)
		{
			for (int i = startColumn; i < stopColumn; ++i)
			{
				IntDoubleVectorPair Acol = A.GetColumn(i);
				for (int j = 0; j < Acol.Indices.Length; ++j)
					B.SetValue(Acol.Indices[j], i, Acol.Data[j]);
			}
		}

		protected internal void CopyI(IDenseRowColumnAccessMatrix A, IDenseRowColumnAccessMatrix B)
		{
			B.Matrix = A.Matrix.Clone();
		}

		protected internal virtual void CopyI(IDenseRowColumnAccessMatrix A,
			IElementalAccessMatrix B, int startRow, int stopRow)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = startRow; i < stopRow; ++i)
				for (int j = Amat.Indices[i], k = 0; j < Amat.Indices[i + 1]; ++j, ++k)
					if (Amat.Data[j] != 0.0)
						B.SetValue(i, k, Amat.Data[j]);
		}

		protected internal virtual void CopyI(IDenseRowAccessMatrix A, 
			IDenseRowAccessMatrix B, int startRow, int stopRow)
		{
			for (int i = startRow; i < stopRow; ++i)
				Array.Copy(A.GetRow(i), 0, B.GetRow(i), 0, A.ColumnCount);
		}

		protected internal virtual void CopyI(IDenseRowAccessMatrix A, 
			IElementalAccessMatrix B, int startRow, int stopRow)
		{
			for (int i = startRow; i < stopRow; ++i)
			{
				double[] Arow = A.GetRow(i);
				for (int j = 0; j < Arow.Length; ++j)
					if (Arow[j] != 0.0)
						B.SetValue(i, j, Arow[j]);
			}
		}

		protected internal virtual void CopyI(IDenseColumnRowAccessMatrix A, IDenseColumnRowAccessMatrix B)
		{
			B.Matrix = A.Matrix.Clone();
		}

		protected internal virtual void CopyI(IDenseColumnRowAccessMatrix A, 
			IElementalAccessMatrix B, int startColumn, int stopColumn)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = startColumn; i < stopColumn; ++i)
				for (int j = Amat.Indices[i]; j < Amat.Indices[i + 1]; ++j)
					if (Amat.Data[j] != 0.0)
						B.SetValue(j - Amat.Indices[i], i, Amat.Data[j]);
		}

		protected internal virtual void CopyI(IDenseColumnAccessMatrix A, IDenseColumnAccessMatrix B, int startColumn, int stopColumn)
		{
			for (int i = startColumn; i < stopColumn; ++i)
				Array.Copy(A.GetColumn(i), 0, B.GetColumn(i), 0, A.RowCount);
		}

		protected internal virtual void CopyI(IDenseColumnAccessMatrix A, IElementalAccessMatrix B, int startColumn, int stopColumn)
		{
			for (int i = startColumn; i < stopColumn; ++i)
			{
				double[] Acol = A.GetColumn(i);
				for (int j = 0; j < Acol.Length; ++j)
					if (Acol[j] != 0.0)
						B.SetValue(j, i, Acol[j]);
			}
		}

		protected internal virtual void CopyI(IElementalAccessMatrix A, IElementalAccessMatrix B, int startRow, int stopRow)
		{
			for (int i = startRow; i < stopRow; ++i)
				for (int j = 0; j < A.RowCount; ++j)
				{
					double val = A.GetValue(i, j);
					if (val != 0.0)
						B.SetValue(i, j, val);
				}
		}

		protected internal override IVector CopyI(IVector x, IVector y, int start, int stop)
		{
			Zero(y);

			if (x is IDenseAccessVector && y is IDenseAccessVector)
				CopyI((IDenseAccessVector) x, (IDenseAccessVector) y, start, stop);
			else if (x is IDenseAccessVector && y is IElementalAccessVector)
				CopyI((IDenseAccessVector) x, (IElementalAccessVector) y, start, stop);
			else if (x is ISparseAccessVector && y is IElementalAccessVector)
				CopyI((ISparseAccessVector) x, (IElementalAccessVector) y, start, stop);
			else if (x is IElementalAccessVector && y is IElementalAccessVector)
				CopyI((IElementalAccessVector) x, (IElementalAccessVector) y, start, stop);
			else
				throw new NotSupportedException();

			return y;
		}

		protected internal virtual void CopyI(IDenseAccessVector x, IDenseAccessVector y, int start, int stop)
		{
			double[] yData = new double[stop - start];
			Array.Copy(x.Vector, start, yData, 0, yData.Length);
			y.Vector = yData;
		}

		protected internal void CopyI(IDenseAccessVector x, IElementalAccessVector y, int start, int stop)
		{
			double[] xData = x.Vector;
			for (int i = start; i < stop; ++i)
				if (xData[i] != 0.0)
					y.SetValue(i - start, xData[i]);
		}

		protected internal virtual void CopyI(ISparseAccessVector x, IElementalAccessVector y, int start, int stop)
		{
			IntDoubleVectorPair xData = x.Vector;
			for (int i = 0; i < xData.Indices.Length; ++i)
				if (xData.Indices[i] >= start && xData.Indices[i] < stop)
					y.SetValue(xData.Indices[i] - start, xData.Data[i]);
		}

		protected internal virtual void CopyI(IElementalAccessVector x, IElementalAccessVector y, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				double val = x.GetValue(i);
				if (val != 0.0)
					y.SetValue(i - start, val);
			}
		}

		protected internal override IVector CopyI(IVector x, IVector y)
		{
			Zero(y);

			if (x is IDenseAccessVector && y is IDenseAccessVector)
				CopyI((IDenseAccessVector) x, (IDenseAccessVector) y);
			else if (x is IDenseAccessVector && y is IElementalAccessVector)
				CopyI((IDenseAccessVector) x, (IElementalAccessVector) y);
			else if (x is ISparseAccessVector && y is ISparseAccessVector)
				CopyI((ISparseAccessVector) x, (ISparseAccessVector) y);
			else if (x is ISparseAccessVector && y is IElementalAccessVector)
				CopyI((ISparseAccessVector) x, (IElementalAccessVector) y);
			else if (x is IBlockAccessVector && y is IBlockAccessVector)
				CopyI((IBlockAccessVector) x, (IBlockAccessVector) y, 0, ((IBlockAccessVector) x).BlockCount);
			else if (x is IElementalAccessVector && y is IElementalAccessVector)
				CopyI((IElementalAccessVector) x, (IElementalAccessVector) y);
			else
				throw new NotSupportedException();

			return y;
		}

		protected internal void CopyI(IBlockAccessVector x, IBlockAccessVector y, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				Copy(x.GetBlock(i), y.GetBlock(i));
		}

		protected internal virtual void CopyI(IDenseAccessVector x, IDenseAccessVector y)
		{
			double[] yData = new double[x.Length];
			Array.Copy(x.Vector, 0, yData, 0, x.Length);
			y.Vector = yData;
		}

		protected internal virtual void CopyI(IDenseAccessVector x, IElementalAccessVector y)
		{
			double[] xData = x.Vector;
			for (int i = 0; i < xData.Length; ++i)
				if (xData[i] != 0.0)
					y.SetValue(i, xData[i]);
		}

		protected internal virtual void CopyI(ISparseAccessVector x, ISparseAccessVector y)
		{
			IntDoubleVectorPair xData = x.Vector;
			int[] yindex = new int[xData.Indices.Length];
			double[] yData = new double[xData.Data.Length];
			Array.Copy(xData.Indices, 0, yindex, 0, yindex.Length);
			Array.Copy(xData.Data, 0, yData, 0, yData.Length);
			y.Vector = new IntDoubleVectorPair(yindex, yData);
		}

		protected internal virtual void CopyI(ISparseAccessVector x, IElementalAccessVector y)
		{
			IntDoubleVectorPair xData = x.Vector;
			for (int i = 0; i < xData.Indices.Length; ++i)
				y.SetValue(xData.Indices[i], xData.Data[i]);
		}

		protected internal virtual void CopyI(IElementalAccessVector x, IElementalAccessVector y)
		{
			for (int i = 0; i < x.Length; ++i)
			{
				double val = x.GetValue(i);
				if (val != 0.0)
					y.SetValue(i, val);
			}
		}

		protected internal override double DotI(IVector x, IVector y)
		{
			if (x is IDenseAccessVector && y is IDenseAccessVector)
				return DotI((IDenseAccessVector) x, (IDenseAccessVector) y);
			else if (x is IDenseAccessVector && y is ISparseAccessVector)
				return DotI((IDenseAccessVector) x, (ISparseAccessVector) y);
			else if (x is ISparseAccessVector && y is IDenseAccessVector)
				return DotI((IDenseAccessVector) y, (ISparseAccessVector) x);
			else if (x is IBlockAccessVector && y is IBlockAccessVector)
				return DotI(block2array((IBlockAccessVector) x), block2array((IBlockAccessVector) y));
			else
				throw new NotSupportedException();
		}

		protected internal virtual double DotI(double[] xData, double[] yData)
		{
			double dot = 0.0;
			for (int i = 0; i < xData.Length; ++i)
				dot += xData[i]*yData[i];
			return dot;
		}

		protected internal virtual double DotI(IDenseAccessVector x, IDenseAccessVector y)
		{
			double[] xData = x.Vector, yData = y.Vector;
			return DotI(xData, yData);
		}

		protected internal virtual double DotI(IDenseAccessVector x, ISparseAccessVector y)
		{
			double dot = 0.0;
			double[] xData = x.Vector;
			IntDoubleVectorPair yData = y.Vector;
			for (int i = 0; i < yData.Indices.Length; ++i)
				dot += xData[yData.Indices[i]]*yData.Data[i];
			return dot;
		}

		protected internal override IVector MultAddI(double alpha, 
			IMatrix A, IVector x, double beta, IVector y, IVector z)
		{
			if (A is IBlockAccessMatrix && x is IBlockAccessVector && y is IBlockAccessVector && z is IBlockAccessVector)
			{
				MultAddI(alpha, (IBlockAccessMatrix) A, (IBlockAccessVector) x, beta, (IBlockAccessVector) y, (IBlockAccessVector) z, 0, ((IBlockAccessMatrix) A).BlockCount);
				return z;
			}
			else if (A is IShellMatrix)
				return ((IShellMatrix) A).MultAdd(alpha, x, beta, y, z);

			// The following requires dense access to the vectors
			if (!(x is IDenseAccessVector) || !(y is IDenseAccessVector) || !(z is IDenseAccessVector))
				throw new NotSupportedException();
			double[] xData = ((IDenseAccessVector) x).Vector, yData = ((IDenseAccessVector) y).Vector, zData = ((IDenseAccessVector) z).Vector;
			if (A is ISparseRowColumnAccessMatrix)
				MultAddI(alpha, (ISparseRowColumnAccessMatrix) A, xData, beta, yData, zData, 0, A.RowCount);
			else if (A is ISparseRowAccessMatrix)
				MultAddI(alpha, (ISparseRowAccessMatrix) A, xData, beta, yData, zData, 0, A.RowCount);
			else if (A is IDenseRowColumnAccessMatrix)
				MultAddI(alpha, (IDenseRowColumnAccessMatrix) A, xData, beta, yData, zData, 0, A.RowCount);
			else if (A is IDenseRowAccessMatrix)
				MultAddI(alpha, (IDenseRowAccessMatrix) A, xData, beta, yData, zData, 0, A.RowCount);
			else if (A is IDenseColumnAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);

				MultAddI((IDenseColumnAccessMatrix) A, xData, zData, 0, A.ColumnCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is IDenseColumnRowAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);
				
				MultAddI((IDenseColumnRowAccessMatrix) A, xData, zData, 0, A.ColumnCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is ISparseColumnAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);
				
				MultAddI((ISparseColumnAccessMatrix) A, xData, zData, 0, A.ColumnCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is ISparseColumnRowAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);

				MultAddI((ISparseColumnRowAccessMatrix) A, xData, zData, 0, A.ColumnCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else throw new NotSupportedException();

			return z;
		}

		protected internal virtual void MultAddI(double alpha, IBlockAccessMatrix A, IBlockAccessVector x, double beta, IBlockAccessVector y, IBlockAccessVector z, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				MultAdd(alpha, A.GetBlock(i), x.GetBlock(i), beta, y.GetBlock(i), z.GetBlock(i));
		}

		protected internal virtual void MultAddI(ISparseColumnAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				IntDoubleVectorPair Acol = A.GetColumn(i);
				for (int j = 0; j < Acol.Indices.Length; ++j)
				{
					zData[Acol.Indices[j]] += Acol.Data[j]*xData[i];
				}
			}
		}

		protected internal virtual void MultAddI(ISparseColumnRowAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
				{
					zData[Amat.Major[j]] += Amat.Data[j]*xData[i];
				}
		}

		protected internal virtual void MultAddI(IDenseColumnAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				double[] Acol = A.GetColumn(i);
				for (int j = 0; j < Acol.Length; ++j)
				{
					zData[j] += Acol[j]*xData[i];
				}
			}
		}

		protected internal virtual void MultAddI(IDenseColumnRowAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
				for (int j = Amat.Indices[i], k = 0; j < Amat.Indices[i + 1]; ++j, ++k)
				{
						zData[k] += Amat.Data[j]*xData[i];
				}
		}

		protected internal virtual void MultAddI(double alpha, IDenseRowColumnAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			int columns = A.ColumnCount;
			for (int i = start; i < stop; ++i)
			{
				double val = 0.0;

				int j = Amat.Indices[i], l = 0;
				for (int k = columns%4; --k >= 0; )
					val += Amat.Data[j++]*xData[l++];
				for (int k = columns/4; --k >= 0; )
					val += Amat.Data[j++]*xData[l++] + Amat.Data[j++]*xData[l++] 
						+ Amat.Data[j++]*xData[l++] + Amat.Data[j++]*xData[l++];

				//for (int j = Amat.index[i], k = 0; j < Amat.index[i + 1];)
				//	val += Amat.Data[j++] * xData[k++];

				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void MultAddI(double alpha, IDenseRowAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			int columns = A.ColumnCount;
			for (int i = start; i < stop; ++i)
			{
				double[] Arow = A.GetRow(i);
				double val = 0.0;

				int j = 0, l = 0;
				for (int k = columns%4; --k >= 0; )
					val += Arow[j++]*xData[l++];
				for (int k = columns/4; --k >= 0; )
					val += Arow[j++]*xData[l++] + Arow[j++]*xData[l++] 
						+ Arow[j++]*xData[l++] + Arow[j++]*xData[l++];

				//for (int j = 0; j < Arow.length; ++j)
				//	val += Arow[j] * xData[j];

				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void MultAddI(double alpha, ISparseRowColumnAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
			{
				double val = 0.0;
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
					val += Amat.Data[j]*xData[Amat.Major[j]];
				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void MultAddI(double alpha, ISparseRowAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				IntDoubleVectorPair Amat = A.GetRow(i);
				double val = 0.0;
				for (int j = 0; j < Amat.Indices.Length; ++j)
					val += Amat.Data[j]*xData[Amat.Indices[j]];
				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal override double Norm1(IMatrix A)
		{
			if (A is IDenseRowColumnAccessMatrix)
				return Norm1((IDenseRowColumnAccessMatrix) A);
			else if (A is IDenseRowAccessMatrix)
				return Norm1((IDenseRowAccessMatrix) A);
			else if (A is ISparseRowAccessMatrix)
				return Norm1((ISparseRowAccessMatrix) A);
			else if (A is ISparseRowColumnAccessMatrix)
				return Norm1((ISparseRowColumnAccessMatrix) A);
			else
				throw new NotSupportedException();
		}

		protected internal double Norm1(IDenseRowColumnAccessMatrix A)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			double norm = 0;
			for (int i = 0; i < Amat.Indices.Length; ++i)
				norm = Math.Max(norm, Norm1(Amat.Data, Amat.Indices[i], Amat.Indices[i + 1]));
			return norm;
		}

		protected internal double Norm1(IDenseRowAccessMatrix A)
		{
			double norm = 0;
			for (int i = 0; i < A.RowCount; ++i)
				norm = Math.Max(norm, Norm1(A.GetRow(i)));
			return norm;
		}

		protected internal double Norm1(ISparseRowAccessMatrix A)
		{
			double norm = 0;
			for (int i = 0; i < A.RowCount; ++i)
				norm = Math.Max(norm, Norm1(A.GetRow(i).Data));
			return norm;
		}

		protected internal double Norm1(ISparseRowColumnAccessMatrix A)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			double norm = 0;
			for (int i = 0; i < Amat.Minor.Length; ++i)
				norm = Math.Max(norm, Norm1(Amat.Data, Amat.Minor[i], Amat.Minor[i + 1]));
			return norm;
		}

		protected internal override double Norm1(IVector x)
		{
			if (x is IVectorAccess)
				return Norm1(((IVectorAccess) x).Data);
			else if (x is IBlockAccessVector)
				return Norm1(block2array((IBlockAccessVector) x));
			else
				throw new NotSupportedException();
		}

		protected internal virtual double Norm1(double[] Data)
		{
			return Norm1(Data, 0, Data.Length);
		}

		protected internal virtual double Norm1(double[] Data, int start, int stop)
		{
			double norm = 0.0;
			for (int i = start; i < stop; ++i)
				norm += Math.Abs(Data[i]);
			return norm;
		}

		protected internal override double Norm2(IVector x)
		{
			if (x is IVectorAccess)
				return Norm2((IVectorAccess) x);
			else if (x is IBlockAccessVector)
				return Norm2(block2array((IBlockAccessVector) x));
			else
				throw new NotSupportedException();
		}

		protected internal virtual double Norm2(double[] xData)
		{
			double scale = 0, ssq = 1;
			for (int i = 0; i < xData.Length; ++i)
			{
				if (xData[i] != 0)
				{
					double absxi = Math.Abs(xData[i]);
					if (scale < absxi)
					{
						ssq = 1 + ssq*Math.Pow(scale/absxi, 2);
						scale = absxi;
					}
					else
						ssq = ssq + Math.Pow(absxi/scale, 2);
				}
			}
			return scale*Math.Sqrt(ssq);
		}

		protected internal virtual double Norm2(IVectorAccess x)
		{
			double[] xData = x.Data;
			return Norm2(xData);
		}

		protected internal override double NormF(IMatrix A)
		{
			if (A is IVectorAccess)
				return Norm2((IVectorAccess) A);
			else if (A is IMatrixAccess)
				return NormF((IMatrixAccess) A);
			else
				throw new NotSupportedException();
		}

		protected internal virtual double NormF(IMatrixAccess A)
		{
			double[][] AData = A.Data;
			double scale = 0, ssq = 1;
			for (int i = 0; i < AData.Length; ++i)
				for (int j = 0; j < AData[i].Length; ++j)
				{
					if (AData[i][j] != 0)
					{
						double absxi = Math.Abs(AData[i][j]);
						if (scale < absxi)
						{
							ssq = 1 + ssq*Math.Pow(scale/absxi, 2);
							scale = absxi;
						}
						else
							ssq = ssq + Math.Pow(absxi/scale, 2);
					}
				}
			return scale*Math.Sqrt(ssq);
		}

		protected internal override double NormInf(IMatrix A)
		{
			if (A is IDenseColumnRowAccessMatrix)
				return NormInf((IDenseColumnRowAccessMatrix) A);
			else if (A is IDenseColumnAccessMatrix)
				return NormInf((IDenseColumnAccessMatrix) A);
			else if (A is ISparseColumnRowAccessMatrix)
				return NormInf((ISparseColumnRowAccessMatrix) A);
			else if (A is ISparseColumnAccessMatrix)
				return NormInf((ISparseColumnAccessMatrix) A);
			else
				throw new NotSupportedException();
		}

		protected internal double NormInf(IDenseColumnAccessMatrix A)
		{
			double norm = 0;
			for (int i = 0; i < A.ColumnCount; ++i)
				norm = Math.Max(norm, Norm1(A.GetColumn(i)));
			return norm;
		}

		protected internal virtual double NormInf(IDenseColumnRowAccessMatrix A)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			double norm = 0;
			for (int i = 0; i < Amat.Indices.Length; ++i)
				norm = Math.Max(norm, Norm1(Amat.Data, Amat.Indices[i], Amat.Indices[i + 1]));
			return norm;
		}

		protected internal double Norm1(ISparseColumnAccessMatrix A)
		{
			double norm = 0;
			for (int i = 0; i < A.RowCount; ++i)
				norm = Math.Max(norm, Norm1(A.GetColumn(i).Data));
			return norm;
		}

		protected internal virtual double Norm1(ISparseColumnRowAccessMatrix A)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			double norm = 0;
			for (int i = 0; i < Amat.Minor.Length; ++i)
				norm = Math.Max(norm, Norm1(Amat.Data, Amat.Minor[i], Amat.Minor[i + 1]));
			return norm;
		}

		protected internal override double NormInf(IVector x)
		{
		    if (x is IVectorAccess)
				return NormInf(((IVectorAccess) x).Data);
		    if (x is IBlockAccessVector)
		        return NormInf(block2array((IBlockAccessVector) x));
		    else
		        throw new NotSupportedException();
		}

	    protected internal virtual double NormInf(double[] Data)
		{
			double norm = 0.0;
			for (int i = 0; i < Data.Length; ++i)
				if (Math.Abs(Data[i]) > norm)
					norm = Math.Abs(Data[i]);
			return norm;
		}

		protected internal override IMatrix Rank1I(double alpha, IVector x, IVector y, IMatrix A)
		{
			try
			{
				Rank1I(alpha, (DenseVector) x, (DenseVector) y, (IElementalAccessMatrix) A, 0, x.Length);
			}
			catch (InvalidCastException)
			{
				throw new NotSupportedException();
			}

			return A;
		}

		protected internal virtual void Rank1I(double alpha, DenseVector x, DenseVector y, IElementalAccessMatrix A, int start, int stop)
		{
			double[] xData = x.Data, yData = y.Data;

			for (int i = start; i < stop; ++i)
			{
				double xval = xData[i];
				if (xval != 0.0)
					for (int j = 0; j < yData.Length; ++j)
					{
						double yval = yData[j];
						if (yval != 0.0)
							A.AddValue(i, j, alpha*xval*yval);
					}
			}
		}

		protected internal override IMatrix ScaleI(double alpha, IMatrix A)
		{
			if (A is IVectorAccess)
				ScaleI(alpha, (IVectorAccess) A);
			else if (A is IMatrixAccess)
				ScaleI(alpha, (IMatrixAccess) A, 0, A.RowCount);
			else
				throw new NotSupportedException();
			return A;
		}

		protected internal override IVector ScaleI(double alpha, IVector x)
		{
			if (x is IVectorAccess)
				ScaleI(alpha, (IVectorAccess) x);
			else if (x is IBlockAccessVector)
				ScaleI(alpha, (IBlockAccessVector) x, 0, ((IBlockAccessVector) x).BlockCount);
			else
				throw new NotSupportedException();
			return x;
		}

		protected internal virtual void ScaleI(double alpha, IBlockAccessVector x, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				Scale(alpha, x.GetBlock(i));
		}

		protected internal virtual void ScaleI(double alpha, IVectorAccess x)
		{
			double[] xData = x.Data;
			for (int i = 0; i < xData.Length; ++i)
				xData[i] *= alpha;
		}

		protected internal virtual void ScaleI(double alpha, IMatrixAccess A, int start, int stop)
		{
			double[][] AData = A.Data;
			for (int i = start; i < stop; ++i)
				for (int j = 0; j < AData[i].Length; ++j)
					AData[i][j] *= alpha;
		}

		protected internal override IVector SetI(double alpha, IVector x)
		{
			if (x is IDenseAccessVector)
				return SetI(alpha, (IDenseAccessVector) x);
			else if (x is IElementalAccessMatrix)
				return SetI(alpha, (IElementalAccessVector) x);
			else
				throw new NotSupportedException();
		}

		protected internal IVector SetI(double alpha, IDenseAccessVector x)
		{
			double[] xData = x.Vector;
			ArraySupport.Fill(xData, alpha);
			return x;
		}

		protected internal IVector SetI(double alpha, IElementalAccessVector x)
		{
			for (int i = 0; i < x.Length; ++i)
				x.SetValue(i, alpha);
			return x;
		}

		protected internal override IVector TransMultAddI(double alpha, IMatrix A, IVector x, double beta, IVector y, IVector z)
		{
			if (A is IBlockAccessMatrix && x is IBlockAccessVector && y is IBlockAccessVector && z is IBlockAccessVector)
			{
				TransMultAddI(alpha, (IBlockAccessMatrix) A, (IBlockAccessVector) x, beta, (IBlockAccessVector) y, (IBlockAccessVector) z, 0, ((IBlockAccessMatrix) A).BlockCount);
				return z;
			}
			else if (A is IShellMatrix)
				return ((IShellMatrix) A).TransMultAdd(alpha, x, beta, y, z);

			// The following requires dense access to the vectors
			if (!(x is IDenseAccessVector) || !(y is IDenseAccessVector) || !(z is IDenseAccessVector))
				throw new NotSupportedException();
			double[] xData = ((IDenseAccessVector) x).Vector, yData = ((IDenseAccessVector) y).Vector, zData = ((IDenseAccessVector) z).Vector;
			if (A is ISparseRowColumnAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);
				
				TransMultAddI((ISparseRowColumnAccessMatrix) A, xData, zData, 0, A.RowCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is ISparseRowAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);
				
				TransMultAddI((ISparseRowAccessMatrix) A, xData, zData, 0, A.RowCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is IDenseRowColumnAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);

				TransMultAddI((IDenseRowColumnAccessMatrix) A, xData, zData, 0, A.RowCount);
				
				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is IDenseRowAccessMatrix)
			{
				if (y != z) Zero(z);
				else Scale(beta/alpha, z);

				TransMultAddI((IDenseRowAccessMatrix) A, xData, zData, 0, A.RowCount);

				if (y != z) Add(beta, y, alpha, z);
				else Scale(alpha, z);
			}
			else if (A is ISparseColumnAccessMatrix)
				TransMultAddI(alpha, (ISparseColumnAccessMatrix) A, xData, beta, yData, zData, 0, A.ColumnCount);
			else if (A is ISparseColumnRowAccessMatrix)
				TransMultAddI(alpha, (ISparseColumnRowAccessMatrix) A, xData, beta, yData, zData, 0, A.ColumnCount);
			else if (A is IDenseColumnRowAccessMatrix)
				TransMultAddI(alpha, (IDenseColumnRowAccessMatrix) A, xData, beta, yData, zData, 0, A.ColumnCount);
			else if (A is IDenseColumnAccessMatrix)
				TransMultAddI(alpha, (IDenseColumnAccessMatrix) A, xData, beta, yData, zData, 0, A.ColumnCount);
			else
				throw new NotSupportedException();

			return z;
		}

		protected internal virtual void TransMultAddI(double alpha, IBlockAccessMatrix A, 
			IBlockAccessVector x, double beta, IBlockAccessVector y, IBlockAccessVector z, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				TransMultAdd(alpha, A.GetBlock(i), x.GetBlock(i), beta, y.GetBlock(i), z.GetBlock(i));
		}

		protected internal virtual void TransMultAddI(double alpha, ISparseColumnRowAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
			{
				double val = 0.0;
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
					val += Amat.Data[j]*xData[Amat.Major[j]];
				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void TransMultAddI(double alpha, ISparseColumnAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				IntDoubleVectorPair Acol = A.GetColumn(i);
				double val = 0.0;
				for (int j = 0; j < Acol.Indices.Length; ++j)
					val += Acol.Data[j]*xData[Acol.Indices[j]];
				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void TransMultAddI(double alpha, IDenseColumnRowAccessMatrix A, 
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			int rows = A.RowCount;
			for (int i = start; i < stop; ++i)
			{
				double val = 0.0;

				int j = Amat.Indices[i], l = 0;
				for (int k = rows%4; --k >= 0; )
					val += Amat.Data[j++]*xData[l++];
				for (int k = rows/4; --k >= 0; )
					val += Amat.Data[j++]*xData[l++] + Amat.Data[j++]*xData[l++] 
						+ Amat.Data[j++]*xData[l++] + Amat.Data[j++]*xData[l++];

				//for (int j = Amat.index[i], k = 0; j < Amat.index[i + 1]; ++j, ++k)
				//	val += Amat.Data[j] * xData[k];

				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void TransMultAddI(double alpha, IDenseColumnAccessMatrix A,
			double[] xData, double beta, double[] yData, double[] zData, int start, int stop)
		{
			int rows = A.RowCount;
			for (int i = start; i < stop; ++i)
			{
				double[] Acol = A.GetColumn(i);
				double val = 0.0;

				int j = 0, l = 0;
				for (int k = rows%4; --k >= 0; )
					val += Acol[j++]*xData[l++];
				for (int k = rows/4; --k >= 0; )
					val += Acol[j++]*xData[l++] + Acol[j++]*xData[l++] 
						+ Acol[j++]*xData[l++] + Acol[j++]*xData[l++];

				//for (int j = 0; j < Arow.length; ++j)
				//	val += Arow[j] * xData[j];

				zData[i] = alpha*val + beta*yData[i];
			}
		}

		protected internal virtual void TransMultAddI(ISparseRowColumnAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
			{
				for (int j = Amat.Minor[i]; j < Amat.Minor[i + 1]; ++j)
				{
					zData[Amat.Major[j]] += Amat.Data[j]*xData[i];
				}
			}
		}

		protected internal virtual void TransMultAddI(ISparseRowAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				IntDoubleVectorPair Arow = A.GetRow(i);
				for (int j = 0; j < Arow.Indices.Length; ++j)
				{
					zData[Arow.Indices[j]] += Arow.Data[j]*xData[i];
				}
			}
		}

		protected internal virtual void TransMultAddI(IDenseRowColumnAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
			{
				for (int j = Amat.Indices[i]; j < Amat.Indices[i + 1]; ++j)
				{
					zData[j - Amat.Indices[i]] += Amat.Data[j]*xData[i];
				}
			}
		}

		protected internal virtual void TransMultAddI(IDenseRowAccessMatrix A, 
			double[] xData, double[] zData, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				double[] Arow = A.GetRow(i);
				for (int j = 0; j < Arow.Length; ++j)
				{
					zData[j] += Arow[j]*xData[i];
				}
			}
		}

		protected internal override IMatrix TransposeI(IMatrix A, IMatrix B)
		{
			if (A is ISparseRowAccessMatrix && B is ISparseColumnAccessMatrix)
				TransposeI((ISparseRowAccessMatrix) A, (ISparseColumnAccessMatrix) B, 0, A.RowCount);
			else if (A is ISparseRowColumnAccessMatrix && B is ISparseColumnAccessMatrix)
				TransposeI((ISparseRowColumnAccessMatrix) A, (ISparseColumnAccessMatrix) B, 0, A.RowCount);
			else if (A is ISparseColumnAccessMatrix && B is ISparseRowAccessMatrix)
				TransposeI((ISparseColumnAccessMatrix) A, (ISparseRowAccessMatrix) B, 0, A.ColumnCount);
			else if (A is ISparseColumnRowAccessMatrix && B is ISparseRowAccessMatrix)
				TransposeI((ISparseColumnRowAccessMatrix) A, (ISparseRowAccessMatrix) B, 0, A.ColumnCount);
			else if (A is IDenseRowAccessMatrix && B is IDenseColumnAccessMatrix)
				TransposeI((IDenseRowAccessMatrix) A, (IDenseColumnAccessMatrix) B, 0, A.RowCount);
			else if (A is IDenseRowColumnAccessMatrix && B is IDenseColumnAccessMatrix)
				TransposeI((IDenseRowColumnAccessMatrix) A, (IDenseColumnAccessMatrix) B, 0, A.RowCount);
			else if (A is IDenseColumnAccessMatrix && B is IDenseRowAccessMatrix)
				TransposeI((IDenseColumnAccessMatrix) A, (IDenseRowAccessMatrix) B, 0, A.ColumnCount);
			else if (A is IDenseColumnRowAccessMatrix && B is IDenseRowAccessMatrix)
				TransposeI((IDenseColumnRowAccessMatrix) A, (IDenseRowAccessMatrix) B, 0, A.ColumnCount);
			else if (A is IElementalAccessMatrix && B is IElementalAccessMatrix)
			{
				Zero(B);
				TransposeI((IElementalAccessMatrix) A, (IElementalAccessMatrix) B, 0, A.RowCount);
			}
			else
				throw new NotSupportedException();
			return B;
		}

		protected internal virtual void TransposeI(IElementalAccessMatrix A, 
			IElementalAccessMatrix B, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				for (int j = 0; j < A.ColumnCount; ++j)
				{
					double val = A.GetValue(i, j);
					if (val != 0.0)
						B.SetValue(j, i, val);
				}
		}

		protected internal virtual void TransposeI(ISparseRowAccessMatrix A, 
			ISparseColumnAccessMatrix B, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				B.SetColumn(i, A.GetRow(i).Clone());
		}

		protected internal virtual void TransposeI(ISparseRowColumnAccessMatrix A, 
			ISparseColumnAccessMatrix B, int start, int stop)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
			{
				B.SetColumn(i, Amat.Copy(i));
			}
		}

		protected internal virtual void TransposeI(ISparseColumnAccessMatrix A, 
			ISparseRowAccessMatrix B, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				B.SetRow(i, A.GetColumn(i).Clone());
		}

		protected internal virtual void TransposeI(ISparseColumnRowAccessMatrix A, 
			ISparseRowAccessMatrix B, int start, int stop)
		{
			IntIntDoubleVectorTriple Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
			{
				B.SetRow(i, Amat.Copy(i));
			}
		}

		protected internal virtual void TransposeI(IDenseRowAccessMatrix A, 
			IDenseColumnAccessMatrix B, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				double[] Bcol = B.GetColumn(i);
				Array.Copy(A.GetRow(i), 0, Bcol, 0, Bcol.Length);
			}
		}

		protected internal virtual void TransposeI(IDenseRowColumnAccessMatrix A, 
			IDenseColumnAccessMatrix B, int start, int stop)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
				B.SetColumn(i, Amat.Copy(i));
		}

		protected internal virtual void TransposeI(IDenseColumnAccessMatrix A, 
			IDenseRowAccessMatrix B, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
			{
				double[] Brow = B.GetRow(i);
				Array.Copy(A.GetColumn(i), 0, Brow, 0, Brow.Length);
			}
		}

		protected internal virtual void TransposeI(IDenseColumnRowAccessMatrix A, 
			IDenseRowAccessMatrix B, int start, int stop)
		{
			IntDoubleVectorPair Amat = A.Matrix;
			for (int i = start; i < stop; ++i)
				B.SetRow(i, Amat.Copy(i));
		}

		public override IMatrix Zero(IMatrix A)
		{
			if (A is IVectorAccess)
				ZeroI(((IVectorAccess) A).Data);
			else if (A is IMatrixAccess)
				ZeroI(((IMatrixAccess) A).Data);
			else if (A is IBlockAccessMatrix)
				ZeroI((IBlockAccessMatrix) A, 0, ((IBlockAccessMatrix) A).BlockCount);
			else
				throw new NotSupportedException();
			return A;
		}

		protected internal virtual void ZeroI(IBlockAccessMatrix A, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				Zero(A.GetBlock(i));
		}

//        [CLSCompliant(false)]
        protected internal virtual void ZeroI(double[] Data)
		{
			ArraySupport.Fill(Data, 0.0);
		}


//        [CLSCompliant(false)]
        protected internal virtual void ZeroI(double[][] Data)
        {
            for (int i = 0; i < Data.Length; ++i)
                ArraySupport.Fill(Data[i], 0.0);
        }

		public override IVector Zero(IVector x)
		{
			if (x is IVectorAccess)
				ZeroI(((IVectorAccess) x).Data);
			else if (x is IBlockAccessVector)
				ZeroI((IBlockAccessVector) x, 0, ((IBlockAccessVector) x).BlockCount);
			else
				throw new NotSupportedException();

			return x;
		}

		protected internal virtual void ZeroI(IBlockAccessVector x, int start, int stop)
		{
			for (int i = start; i < stop; ++i)
				Zero(x.GetBlock(i));
		}

		public override int Cardinality(IMatrix A)
		{
			if (A is IVectorAccess)
				return CardinalityI(((IVectorAccess) A).Data);
			else if (A is IMatrixAccess)
				return CardinalityI(((IMatrixAccess) A).Data);
			else
				return A.RowCount*A.ColumnCount;
		}

		protected internal virtual int CardinalityI(double[] Data)
		{
			return Data.Length;
		}

        //IS
//        [CLSCompliant(false)]
        protected internal virtual int CardinalityI(double[][] Data)
        {
            int nnz = 0;
            for (int i = 0; i < Data.Length; ++i)
                nnz += Data[i].Length;
            return nnz;
        }

		public override int Cardinality(IVector x)
		{
			if (x is IVectorAccess)
				return CardinalityI(((IVectorAccess) x).Data);
			else
				return x.Length;
		}

		public override IntDoubleVectorPair Gather(double[] x)
		{
			// Number of non-zeros
			int nnz = 0;
			for (int i = 0; i < x.Length; ++i)
				if (x[i] == 0.0)
					nnz++;

			// Locations of the non-zeros
			int[] nz = new int[nnz];
			int j = 0;
			for (int i = 0; i < x.Length; ++i)
				if (x[i] == 0.0)
					nz[j++] = i;

			return Gather(nz, x);
		}

		public override IntDoubleVectorPair Gather(int[] xIndex, double[] xData)
		{
			int[] index = new int[xIndex.Length];
			Array.Copy(xIndex, 0, index, 0, index.Length);

			double[] Data = new double[xIndex.Length];
			for (int i = 0; i < xIndex.Length; ++i)
				Data[i] = xData[xIndex[i]];

			return new IntDoubleVectorPair(index, Data);
		}

		public override double[] Scatter(IntDoubleVectorPair x, double[] y)
		{
			ArraySupport.Fill(y, 0.0);
			for (int i = 0; i < x.Indices.Length; ++i)
				y[x.Indices[i]] = x.Data[i];
			return y;
		}

		public override double[,] GetArrayCopy(IMatrix A)
		{
			if (A is IElementalAccessMatrix)
			{
				int[] row = new int[A.RowCount], 
					column = new int[A.ColumnCount];

                //for (int i = 0; i < row.Length; row[i] = i++);
                //for (int i = 0; i < column.Length; column[i] = i++);

				return ((IElementalAccessMatrix) A).GetValues(row, column);
			}
			else
				throw new NotSupportedException();
		}

		public override double[] GetArrayCopy(IVector x)
		{
			double[] ret = new double[x.Length];
			if (x is IDenseAccessVector)
				Array.Copy(((IDenseAccessVector) x).Vector, 0, ret, 0, ret.Length);
			else if (x is ISparseAccessVector)
				ret = Scatter(((ISparseAccessVector) x).Vector, ret);
			else if (x is IBlockAccessVector)
				Array.Copy(((IBlockAccessVector) x).Array, 0, ret, 0, ret.Length);
			else
				throw new NotSupportedException();
			return ret;
		}

		public override IVector SetVector(double[] x, IVector y)
		{
			if (y is IDenseAccessVector)
				((IDenseAccessVector) y).Vector = x;
			else if (y is ISparseAccessVector)
				Scatter(((ISparseAccessVector) y).Vector, x);
			else
				throw new NotSupportedException();
			return y;
		}

		/// <summary> Populates the dense array of x with its Data, and returns it</summary>
		protected internal virtual double[] block2array(IBlockAccessVector x)
		{
			double[] y = x.Array;
			for (int i = 0; i < x.BlockCount; ++i)
			{
				int[] index = x.GetBlockIndices(i);
				double[] Data = ((IDenseAccessVector) x.GetBlock(i)).Vector;
				for (int j = 0; j < Data.Length; ++j)
					y[index[j]] += Data[j];
			}
			return y;
		}
	}
}