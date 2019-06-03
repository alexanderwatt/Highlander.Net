using System;
using Orion.Analytics.Utilities;

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Sparse matrix stored as a vector, column major</summary>
	[Serializable]
	public class SparseColumnRowMatrix : AbstractMatrix, 
		ISparseColumnRowAccessMatrix, IElementalAccessZeroColumnMatrix, IVectorAccess
	{
		/// <summary> Matrix data</summary>
		private double[] _data;

		/// <summary> Row indices. These are kept sorted within each column.</summary>
		private int[] _rowIndex;

		/// <summary> Indices to the start of each column</summary>
		private int[] _columnIndex;

		/// <summary> Number of indices in use on each column.</summary>
		private readonly int[] _used;

		/// <summary> Is the matrix compacted?</summary>
		private bool _isCompact;

		/// <summary> Constructor for SparseColumnRowMatrix.</summary>
		/// <param name="numColumns"></param>
		/// <param name="nz">Maximum number of non-zeros on each row.</param>
		/// <param name="numRows"></param>
		public SparseColumnRowMatrix(int numRows, int numColumns, int[] nz) : base(numRows, numColumns)
		{
			int nnz = 0;
			foreach (int t in nz)
			    nnz += t;
		    _data = new double[nnz];
			_rowIndex = new int[nnz];
			_columnIndex = new int[numColumns + 1];
			_used = new int[numColumns];
			for (int i = 1; i < numColumns + 1; ++i)
				_columnIndex[i] = nz[i - 1] + _columnIndex[i - 1];
		}

		/// <summary> Constructor for SparseColumnRowMatrix.</summary>
		/// <param name="numColumns"></param>
		/// <param name="nz">Maximum number of non-zeros on each row.</param>
		/// <param name="numRows"></param>
		public SparseColumnRowMatrix(int numRows, int numColumns, int nz) : base(numRows, numColumns)
		{
			int nnz = nz*numColumns;

			_data = new double[nnz];
			_rowIndex = new int[nnz];
			_columnIndex = new int[numColumns + 1];
			_used = new int[numColumns];
			for (int i = 1; i < numColumns + 1; ++i)
				_columnIndex[i] = nz + _columnIndex[i - 1];
		}

		/// <summary> Constructor for SparseColumnRowMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="A"></param>
		/// <param name="nz">Maximum number of non-zeros on each row.</param>
		public SparseColumnRowMatrix(IMatrix A, int nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		/// <summary> Constructor for SparseColumnRowMatrix, and copies the contents from the
		/// supplied matrix.</summary>
		/// <param name="nz">Maximum number of non-zeros on each row.</param>
		public SparseColumnRowMatrix(IMatrix A, int[] nz) : this(A.RowCount, A.ColumnCount, nz)
		{
			Blas.Default.Copy(A, this);
		}

		public virtual IntIntDoubleVectorTriple Matrix
		{
			get
			{
				Compact();
				return new IntIntDoubleVectorTriple(_rowIndex, _columnIndex, _data);
			}
			set
			{
				_data = value.Data;
				_rowIndex = value.Major;
				_columnIndex = value.Minor;
			}
		}

		public virtual void ZeroColumns(int[] column, double diagonal)
		{
		    foreach (int col in column)
		    {
		        if (col < row_count)
		        {
		            _data[_columnIndex[col]] = diagonal;
		            _rowIndex[_columnIndex[col]] = col;
		            _used[col] = 1;
		        }
		        else
		            _used[col] = 0;
		    }
		    _isCompact = false;
		}

		public virtual void AddValue(int row, int column, double val)
		{
			int index = GetRowIndex(row, column);
			_data[_columnIndex[column] + index] += val;
		}

		public virtual void SetValue(int row, int column, double val)
		{
			int index = GetRowIndex(row, column);
			_data[_columnIndex[column] + index] = val;
		}

		public virtual double GetValue(int row, int column)
		{
			int ind = ArraySupport.BinarySearch(_rowIndex, row, 
				_columnIndex[column], _columnIndex[column] + _used[column]);

			if (ind >= 0) return _data[ind];
		    if (row < row_count && row >= 0) return 0;
		    throw new IndexOutOfRangeException("Row " + row + " Column " + column);
		}

		public virtual void AddValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < column.Length; ++i)
			{
				for (int j = 0; j < row.Length; ++j)
				{
					int index = GetRowIndex(row[j], column[i]);
					_data[_columnIndex[column[i]] + index] += values[j, i];
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
					_data[_columnIndex[column[i]] + index] = values[j, i];
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

		public virtual double[] Data
		{
			get
			{
				Compact();
				return _data;
			}
		}

		public virtual void Compact()
		{
			if (_isCompact) return;

			int nnz = 0;
			for (int i = 0; i < column_count; ++i) nnz += _used[i];

			if (nnz < _data.Length)
			{
				double[] newData = new double[nnz];
				int[] newRowIndex = new int[nnz];
				int[] newColumnIndex = new int[column_count + 1];

				newColumnIndex[0] = _columnIndex[0];
				for (int i = 0; i < column_count; ++i)
				{
					newColumnIndex[i + 1] = newColumnIndex[i] + _used[i];
					Array.Copy(_data, _columnIndex[i], newData, newColumnIndex[i], _used[i]);
					Array.Copy(_rowIndex, _columnIndex[i], newRowIndex, newColumnIndex[i], _used[i]);
				}

				_data = newData;
				_rowIndex = newRowIndex;
				_columnIndex = newColumnIndex;
			}
			_isCompact = true;
		}

		/// <summary> Tries to find the col-index in the given row. If it is not found,
		/// a reallocation is done, and a new index is returned. If there is no more
		/// space for further allocation, an exception is thrown.
		/// </summary>
		private int GetRowIndex(int row, int col)
		{
			int columnOffset = _columnIndex[col];

			int ind = ArraySupport.BinarySearchGreater(_rowIndex, row, columnOffset, columnOffset + _used[col]) - columnOffset;

			// Found
			if (ind < _used[col] && _rowIndex[columnOffset + ind] == row)
				return ind;

			// Need to insert

			// Check row index
			if (row < 0 || row >= row_count)
				throw new IndexOutOfRangeException("Row " + row + " Column " + col);

			// Is the column full?
			if (columnOffset + _used[col] >= _data.Length || ((col + 1) < _columnIndex.Length && columnOffset + _used[col] >= _columnIndex[col + 1]))
				throw new IndexOutOfRangeException("Too many non-zeros on column " + col);

			// Make room for insertion
			for (int i = _used[col]; i >= ind + 1; --i)
			{
				_rowIndex[columnOffset + i] = _rowIndex[columnOffset + i - 1];
				_data[columnOffset + i] = _data[columnOffset + i - 1];
			}

			// Put in new structure
			_used[col]++;
			_rowIndex[columnOffset + ind] = row;
			_data[columnOffset + ind] = 0;
			_isCompact = false;

			return ind;
		}
	}
}