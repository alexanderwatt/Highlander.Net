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
	/// <summary> Sparse matrix stored as one long vector, row major.</summary>
	[Serializable]
	public class SparseRowColumnMatrix : AbstractMatrix, IElementalAccessZeroRowMatrix, 
		ISparseRowColumnAccessMatrix, IVectorAccess
	{
		/// <summary> Matrix data</summary>
		private double[] _data;

		/// <summary> Column indices. These are kept sorted within each row.</summary>
		private int[] _columnIndices;

		/// <summary> Indices to the start of each row</summary>
		private int[] _rowIndices;

		/// <summary> Number of indices in use on each row.</summary>
		private readonly int[] _used;

		/// <summary> Is the matrix compacted?</summary>
		private bool _isCompact;

		/// <summary> Constructor for SparseRowColumnMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="A"></param>
		/// <param name="nz">Maximum number of non-zeros on each row.</param>
		public SparseRowColumnMatrix(IMatrix A, int[] nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseRowColumnMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="A"></param>
		/// <param name="nz">Maximum number of non-zeros on each row</param>
		public SparseRowColumnMatrix(IMatrix A, int nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseRowColumnMatrix.</summary>
		/// <param name="numColumns"></param>
		/// <param name="nz">Maximum number of non-zeros on each row</param>
		/// <param name="numRows"></param>
		public SparseRowColumnMatrix(int numRows, int numColumns, int[] nz) : base(numRows, numColumns)
		{
			int nnz = 0;
			foreach (int t in nz)
			    nnz += t;
		    _rowIndices = new int[numRows + 1];
			_columnIndices = new int[nnz];
			_data = new double[nnz];
			_used = new int[numRows];
			for (int i = 1; i < numRows + 1; ++i)
				_rowIndices[i] = nz[i - 1] + _rowIndices[i - 1];
		}

		/// <summary> Constructor for SparseRowColumnMatrix.</summary>
		/// <param name="numColumns"></param>
		/// <param name="nz">Maximum number of non-zeros on each row</param>
		/// <param name="numRows"></param>
		public SparseRowColumnMatrix(int numRows, int numColumns, int nz) : base(numRows, numColumns)
		{
			int nnz = nz*numRows;

			_rowIndices = new int[numRows + 1];
			_columnIndices = new int[nnz];
			_data = new double[nnz];
			_used = new int[numRows];
			for (int i = 1; i < numRows + 1; ++i)
				_rowIndices[i] = nz + _rowIndices[i - 1];
		}

		public virtual void AddValue(int row, int column, double val)
		{
			int index = GetColumnIndex(column, row);
			_data[_rowIndices[row] + index] += val;
		}

		public virtual void SetValue(int row, int column, double val)
		{
			int index = GetColumnIndex(column, row);
			_data[_rowIndices[row] + index] = val;
		}

		public virtual double GetValue(int row, int column)
		{
			int ind = ArraySupport.BinarySearch(_columnIndices, column, _rowIndices[row], _rowIndices[row] + _used[row]);
			
			if (ind >= 0) return _data[ind];
		    if (column < column_count && column >= 0) return 0;
		    throw new IndexOutOfRangeException("Row " + row + " Column " + column);
		}

		public virtual void AddValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < row.Length; ++i)
				for (int j = 0; j < column.Length; ++j)
				{
					int index = GetColumnIndex(column[j], row[i]);
					_data[_rowIndices[row[i]] + index] += values[i, j];
				}
		}

		public virtual void SetValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < row.Length; ++i)
			{
				for (int j = 0; j < column.Length; ++j)
				{
					int index = GetColumnIndex(column[j], row[i]);
					_data[_rowIndices[row[i]] + index] = values[i, j];
				}
			}
		}

		public virtual double[,] GetValues(int[] row, int[] column)
		{
			var sub = new double[row.Length, column.Length];
			for (int i = 0; i < row.Length; ++i)
				for (int j = 0; j < column.Length; ++j)
					sub[i, j] = GetValue(row[i], column[j]);
			return sub;
		}

		public virtual IntIntDoubleVectorTriple Matrix
		{
			get
			{
				Compact();
				return new IntIntDoubleVectorTriple(_columnIndices, _rowIndices, _data);
			}
			set
			{
				_data = value.Data;
				_columnIndices = value.Major;
				_rowIndices = value.Minor;
			}
		}

		public virtual void Compact()
		{
			if (_isCompact)
				return;

			int nnz = 0;
			for (int i = 0; i < row_count; ++i)
				nnz += _used[i];

			if (nnz < _data.Length)
			{
				var newRowIndex = new int[row_count + 1];
				var newColumnIndex = new int[nnz];
				var newData = new double[nnz];

				newRowIndex[0] = _rowIndices[0];
				for (int i = 0; i < row_count; ++i)
				{
					newRowIndex[i + 1] = newRowIndex[i] + _used[i];
					Array.Copy(_data, _rowIndices[i], newData, newRowIndex[i], _used[i]);
					Array.Copy(_columnIndices, _rowIndices[i], newColumnIndex, newRowIndex[i], _used[i]);
				}

				_rowIndices = newRowIndex;
				_columnIndices = newColumnIndex;
				_data = newData;
			}

			_isCompact = true;
		}


		public virtual void ZeroRows(int[] row, double diagonal)
		{
		    foreach (int rowI in row)
		    {
		        if (rowI < column_count)
		        {
		            _data[_rowIndices[rowI]] = diagonal;
		            _columnIndices[_rowIndices[rowI]] = rowI;
		            _used[rowI] = 1;
		        }
		        else
		            _used[rowI] = 0;
		    }

		    _isCompact = false;
		}

		/// <summary> Tries to find the col-index in the given row. If it is not found,
		/// a reallocation is done, and a new index is returned. If there is no more
		/// space for further allocation, an exception is thrown.</summary>
		private int GetColumnIndex(int col, int row)
		{
			int rowOffset = _rowIndices[row];

			int ind = ArraySupport.BinarySearchGreater(_columnIndices, col, 
				rowOffset, rowOffset + _used[row]) - rowOffset;

			// Found
			if (ind < _used[row] && _columnIndices[rowOffset + ind] == col)
				return ind;

			// Need to insert

			// Check column index
			if (col < 0 || col >= column_count)
				throw new IndexOutOfRangeException("Row " + row + " Column " + col);

			// Is the row full?
			if (rowOffset + _used[row] >= _data.Length || ((row + 1) < _rowIndices.Length 
				&& rowOffset + _used[row] >= _rowIndices[row + 1]))
					throw new IndexOutOfRangeException("Too many non-zeros on row " + row);

			// Make room for insertion
			for (int i = _used[row]; i >= ind + 1; --i)
			{
				_columnIndices[rowOffset + i] = _columnIndices[rowOffset + i - 1];
				_data[rowOffset + i] = _data[rowOffset + i - 1];
			}

			// Put in new structure
			_used[row]++;
			_columnIndices[rowOffset + ind] = col;
			_data[rowOffset + ind] = 0;
			_isCompact = false;

			return ind;
		}

		public virtual double[] Data
		{
			get
			{
				Compact();
				return _data;
			}
		}
	}
}