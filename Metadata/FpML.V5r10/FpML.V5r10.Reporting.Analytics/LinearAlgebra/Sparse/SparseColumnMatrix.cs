using System;
using Orion.Analytics.Utilities;

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Sparse matrix stored as 2D ragged columns. For best performance during
	/// assembly, ensure that enough memory is allocated at construction time,
	/// as re-allocation may be time-consuming.
	/// </summary>
	[Serializable]
	public class SparseColumnMatrix : AbstractMatrix, ISparseColumnAccessMatrix, 
		IElementalAccessZeroColumnMatrix, IMatrixAccess
	{
		/// <summary> Matrix data</summary>
		private readonly double[][] _data;

		/// <summary> Row indices. These are kept sorted within each column.</summary>
		private readonly int[][] _rowIndex;

		/// <summary> Number of indices in use on each column.</summary>
		private readonly int[] _used;

		/// <summary> Is the matrix compacted?</summary>
		private bool _isCompact;

	    /// <summary> Constructor for SparseColumnMatrix.</summary>
	    /// <param name="numColumns"></param>
	    /// <param name="nz">Initial number of non-zeros on each column</param>
	    /// <param name="numRows"></param>
	    public SparseColumnMatrix(int numRows, int numColumns, int[] nz) : base(numRows, numColumns)
		{
			_data = new double[numColumns][];
			_rowIndex = new int[numColumns][];
			for (int i = 0; i < numColumns; ++i)
			{
				_data[i] = new double[nz[i]];
				_rowIndex[i] = new int[nz[i]];
			}
			_used = new int[numColumns];
		}

	    /// <summary> Constructor for SparseColumnMatrix.</summary>
	    /// <param name="numColumns"></param>
	    /// <param name="nz">Initial number of non-zeros on each column</param>
	    /// <param name="numRows"></param>
	    public SparseColumnMatrix(int numRows, int numColumns, int nz) : base(numRows, numColumns)
		{
			_data = new double[numColumns][];
			_rowIndex = new int[numColumns][];
			for (int i = 0; i < numColumns; ++i)
			{
				_data[i] = new double[nz];
				_rowIndex[i] = new int[nz];
			}
			_used = new int[numColumns];
		}

		/// <summary> Constructor for SparseColumnMatrix. Zero initial pre-allocation</summary>
		public SparseColumnMatrix(int numRows, int numColumns) : this(numRows, numColumns, 0)
		{
		}

		/// <summary> Constructor for SparseColumnMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="nz">Initial number of non-zeros on each column.</param>
		public SparseColumnMatrix(IMatrix A, int[] nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseColumnMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="nz">Initial number of non-zeros on each column.</param>
		public SparseColumnMatrix(IMatrix A, int nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseColumnMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		public SparseColumnMatrix(IMatrix A) : this(A, 0)
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

		public virtual IntDoubleVectorPair GetColumn(int i)
		{
			Compact(i);
			return new IntDoubleVectorPair(_rowIndex[i], _data[i]);
		}

		public virtual void SetColumn(int i, IntDoubleVectorPair x)
		{
			_rowIndex[i] = x.Indices;
			_data[i] = x.Data;
			_used[i] = x.Data.Length;
			_isCompact = false;
		}

		public virtual void ZeroColumns(int[] column, double diagonal)
		{
		    foreach (int col in column)
		    {
		        if (col < row_count)
		        {
		            _data[col][0] = diagonal;
		            _rowIndex[col][0] = col;
		            _used[col] = 1;
		        }
		        else _used[col] = 0;
		    }

		    _isCompact = false;
		}

		public virtual void AddValue(int row, int column, double val)
		{
			int index = GetRowIndex(row, column);
			_data[column][index] += val;
		}

		public virtual void SetValue(int row, int column, double val)
		{
			int index = GetRowIndex(row, column);
			_data[column][index] = val;
		}

		public virtual double GetValue(int row, int column)
		{
			int ind = ArraySupport.BinarySearch(_rowIndex[column], row, 0, _used[column]);
			
			if (ind != - 1) return _data[column][ind];
		    if (row < row_count && row >= 0) return 0.0;
		    throw new IndexOutOfRangeException("Row " + row + " Column " + column);
		}

		public virtual void AddValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < column.Length; ++i)
			{
				for (int j = 0; j < row.Length; ++j)
				{
					int index = GetRowIndex(row[j], column[i]);
					_data[column[i]][index] += values[j, i];
				}
			}
		}

		public virtual void SetValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < column.Length; ++i)
			{
				for (int j = 0; j < row.Length; ++j)
				{
					int index = GetRowIndex(row[j], column[i]);
					_data[column[i]][index] = values[j, i];
				}
			}
		}

		public virtual double[,] GetValues(int[] row, int[] column)
		{
			double[,] sub = new double[row.Length, column.Length];
//			for (int i = 0; i < row.Length; i++)
//			{
//				sub[i] = new double[column.Length];
//			}
			for (int i = 0; i < row.Length; ++i)
				for (int j = 0; j < column.Length; ++j)
					sub[i, j] = GetValue(row[i], column[j]);
			return sub;
		}

		public virtual void Compact()
		{
			if (!_isCompact)
			{
				for (int i = 0; i < column_count; ++i) Compact(i);
				_isCompact = true;
			}
		}

		/// <summary> Compacts the column-indices and entries.</summary>
		private void Compact(int column)
		{
			if (_used[column] < _data[column].Length)
			{
				double[] newData = new double[_used[column]];
				Array.Copy(_data[column], 0, newData, 0, _used[column]);
				int[] newInd = new int[_used[column]];
				Array.Copy(_rowIndex[column], 0, newInd, 0, _used[column]);
				_data[column] = newData;
				_rowIndex[column] = newInd;
			}
		}

		/// <summary> Tries to find the row-index in the given column. If it is not found,
		/// a reallocation is done, and a new index is returned.</summary>
		private int GetRowIndex(int row, int col)
		{
			int[] curRow = _rowIndex[col];
			double[] curDat = _data[col];

			// Try to find column index
			int ind = ArraySupport.BinarySearchGreater(curRow, row, 0, _used[col]);

			// Found
			if (ind < _used[col] && curRow[ind] == row)
				return ind;

			// Not found, try to make room
			if (row < 0 || row >= row_count)
				throw new IndexOutOfRangeException(" Row " + row + " Column " + col);
			_used[col]++;

			// Check available memory
			if (_used[col] > curDat.Length)
			{
				// If zero-length, use new length of 1, else double the bandwidth
				int newLength = 1;
				if (curDat.Length != 0)
					newLength = 2*curDat.Length;

				// Copy existing data into new arrays
				int[] newRow = new int[newLength];
				double[] newDat = new double[newLength];
				Array.Copy(curRow, 0, newRow, 0, curDat.Length);
				Array.Copy(curDat, 0, newDat, 0, curDat.Length);

				// Update pointers
				_rowIndex[col] = newRow;
				_data[col] = newDat;
				curRow = newRow;
				curDat = newDat;
			}

			// All ok, make room for insertion
			for (int i = _used[col] - 1; i >= ind + 1; --i)
			{
				curRow[i] = curRow[i - 1];
				curDat[i] = curDat[i - 1];
			}

			// Put in new structure
			curRow[ind] = row;
			curDat[ind] = 0.0;
			_isCompact = false;

			return ind;
		}
	}
}