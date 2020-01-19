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
using Highlander.Reporting.Analytics.V5r3.Utilities;

namespace Highlander.Reporting.Analytics.V5r3.LinearAlgebra.Sparse
{
	/// <summary> Sparse matrix stored as 2D ragged rows. For best performance during
	/// assembly, ensure that enough memory is allocated at construction time,
	/// as re-allocation may be time-consuming.
	/// </summary>
	[Serializable]
	public class SparseRowMatrix : AbstractMatrix, ISparseRowAccessMatrix, 
		IElementalAccessZeroRowMatrix, IMatrixAccess
	{
		/// <summary> Matrix data</summary>
		private readonly double[][] _data;

		/// <summary> Column indices. These are kept sorted within each row.</summary>
		private readonly int[][] _columnIndex;

		/// <summary> Number of indices in use on each row.</summary>
		private readonly int[] _used;

		/// <summary> Is the matrix compacted?</summary>
		private bool _isCompact;

		/// <summary> Constructor for SparseRowMatrix, and copies the contents from the
		/// supplied matrix.
		/// </summary>
		/// <param name="nz">Initial number of non-zeros on each row.</param>
		public SparseRowMatrix(IMatrix A, int[] nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseRowMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="nz">Initial number of non-zeros on each row.</param>
		public SparseRowMatrix(IMatrix A, int nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseRowMatrix, and copies the contents from the
		/// supplied matrix. Zero initial pre-allocation.</summary>
		public SparseRowMatrix(IMatrix A) : this(A, 0)
		{
		}

	    /// <summary> Constructor for SparseRowMatrix.</summary>
	    /// <param name="numColumns"></param>
	    /// <param name="nz">Initial number of non-zeros on each row.</param>
	    /// <param name="numRows"></param>
	    public SparseRowMatrix(int numRows, int numColumns, int[] nz) : base(numRows, numColumns)
		{
			_data = new double[numRows][];
			_columnIndex = new int[numRows][];
			_used = new int[numRows];
			for (int i = 0; i < numRows; ++i)
			{
				_data[i] = new double[nz[i]];
				_columnIndex[i] = new int[nz[i]];
			}
		}

	    /// <summary> Constructor for SparseRowMatrix.</summary>
	    /// <param name="numColumns"></param>
	    /// <param name="nz">Initial number of non-zeros on each row.</param>
	    /// <param name="numRows"></param>
	    public SparseRowMatrix(int numRows, int numColumns, int nz) : base(numRows, numColumns)
		{
			_data = new double[numRows][];
			for (int i = 0; i < numRows; i++)
			{
				_data[i] = new double[nz];
			}
			_columnIndex = new int[numRows][];
			for (int i2 = 0; i2 < numRows; i2++)
			{
				_columnIndex[i2] = new int[nz];
			}
			_used = new int[numRows];
		}

		/// <summary> Constructor for SparseRowMatrix. Zero initial pre-allocation</summary>
		public SparseRowMatrix(int numRows, int numColumns) : this(numRows, numColumns, 0)
		{
		}

		public virtual double[][] Data
		{
			get
			{
				Compact();
				return _data;
			}
		}

		public virtual void ZeroRows(int[] row, double diagonal)
		{
		    foreach (int rowI in row)
		    {
		        if (rowI < column_count)
		        {
		            _data[rowI][0] = diagonal;
		            _columnIndex[rowI][0] = rowI;
		            _used[rowI] = 1;
		        }
		        else
		            _used[rowI] = 0;
		    }
		    _isCompact = false;
		}

		public virtual void AddValue(int i, int j, double val)
		{
			int index = GetColumnIndex(j, i);
			_data[i][index] += val;
		}

		public virtual void SetValue(int i, int j, double val)
		{
			int index = GetColumnIndex(j, i);
			_data[i][index] = val;
		}

		public virtual double GetValue(int row, int column)
		{
			int ind = ArraySupport.BinarySearch(_columnIndex[row], column, 0, _used[row]);
			
			if (ind != - 1) return _data[row][ind];
		    if (column < column_count && column >= 0) return 0.0;
		    throw new IndexOutOfRangeException("Row " + row + " Column " + column);
		}

		public virtual void AddValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < row.Length; ++i)
			{
				for (int j = 0; j < column.Length; ++j)
				{
					int index = GetColumnIndex(column[j], row[i]);
					_data[row[i]][index] += values[i, j];
				}
			}
		}

		public virtual void SetValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < row.Length; ++i)
			{
				for (int j = 0; j < column.Length; ++j)
				{
					int index = GetColumnIndex(column[j], row[i]);
					_data[row[i]][index] = values[i, j];
				}
			}
		}

		public virtual double[,] GetValues(int[] row, int[] column)
		{
			double[,] sub = new double[row.Length, column.Length];
			for (int i = 0; i < row.Length; ++i)
				for (int j = 0; j < column.Length; ++j)
					sub[i, j] = GetValue(row[i], column[j]);
			return sub;
		}

		/// <summary> Tries to find the col-index in the given row. If it is not found,
		/// a reallocation is done, and a new index is returned.</summary>
		private int GetColumnIndex(int col, int row)
		{
			int[] curCol = _columnIndex[row];
			double[] curDat = _data[row];

			// Try to find column index
			int ind = ArraySupport.BinarySearchGreater(curCol, col, 0, _used[row]);

			// Found
			if (ind < _used[row] && curCol[ind] == col)
				return ind;

			// Not found, try to make room
			if (col < 0 || col >= column_count)
				throw new IndexOutOfRangeException("Row " + row + " Column " + col);
			_used[row]++;

			// Check available memory
			if (_used[row] > curDat.Length)
			{
				// If zero-length, use new length of 1, else double the bandwidth
				int newLength = 1;
				if (curDat.Length != 0)
					newLength = 2*curDat.Length;

				// Copy existing data into new arrays
				int[] newCol = new int[newLength];
				double[] newDat = new double[newLength];
				Array.Copy(curCol, 0, newCol, 0, curDat.Length);
				Array.Copy(curDat, 0, newDat, 0, curDat.Length);

				// Update pointers
				_columnIndex[row] = newCol;
				_data[row] = newDat;
				curCol = newCol;
				curDat = newDat;
			}

			// All ok, make room for insertion
			for (int i = _used[row] - 1; i >= ind + 1; --i)
			{
				curCol[i] = curCol[i - 1];
				curDat[i] = curDat[i - 1];
			}

			// Put in new structure
			curCol[ind] = col;
			curDat[ind] = 0.0;
			return ind;
		}

		public virtual void SetRow(int i, IntDoubleVectorPair x)
		{
			_columnIndex[i] = x.Indices;
			_data[i] = x.Data;
			_used[i] = x.Data.Length;
			_isCompact = false;
		}

		public virtual IntDoubleVectorPair GetRow(int i)
		{
			Compact(i);
			return new IntDoubleVectorPair(_columnIndex[i], _data[i]);
		}

		/// <summary> Compacts the row-indices and entries.</summary>
		private void Compact(int row)
		{
			if (_used[row] < _data[row].Length)
			{
				double[] newDat = new double[_used[row]];
				Array.Copy(_data[row], 0, newDat, 0, _used[row]);
				int[] newInd = new int[_used[row]];
				Array.Copy(_columnIndex[row], 0, newInd, 0, _used[row]);
				_data[row] = newDat;
				_columnIndex[row] = newInd;
			}
		}

		public virtual void Compact()
		{
			if (!_isCompact)
			{
				for (int i = 0; i < row_count; ++i) Compact(i);
				_isCompact = true;
			}
		}
	}
}