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
	// TODO: current implementation of CoordinateMatrix is very inefficient.
	// Accessing any element costs O(n) where 'n' is the number of non-zero elements.
	// A hashtable should be used to store the non-zero indices.

	/// <summary> Coordinate storage matrix. The data is not kept sorted.</summary>
	[Serializable]
	public class CoordinateMatrix : AbstractMatrix, IElementalAccessMatrix, ICoordinateAccessMatrix
	{
		/// <summary> The rows</summary>
		private int[] _row;

		/// <summary> The columns</summary>
		private int[] _column;

		/// <summary> The data</summary>
		private double[] _data;

		/// <summary> Current insertion offset</summary>
		private int _offset;

		/// <summary> Constructor for CoordinateMatrix. No pre-allocation</summary>
		public CoordinateMatrix(int numRows, int numColumns) : this(numRows, numColumns, 0)
		{
		}

		/// <summary> Constructor for CoordinateMatrix.</summary>
		/// <param name="numColumns"></param>
		/// <param name="numEntries">Initial number of non-zero entries.</param>
		/// <param name="numRows"></param>
		public CoordinateMatrix(int numRows, int numColumns, int numEntries) : base(numRows, numColumns)
		{
			_column = new int[numEntries];
			_row = new int[numEntries];
			_data = new double[numEntries];
		}

		/// <summary> Constructor for CoordinateMatrix.</summary>
		public CoordinateMatrix(int numRows, int numColumns, int[] row, int[] column, double[] data) 
			: base(numRows, numColumns)
		{
			_row = row;
			_column = column;
			_data = data;
			_offset = data.Length;
		}

		public virtual void AddValue(int row, int column, double val)
		{
			int index = GetIndex(row, column);
			_data[index] += val;
		}

		public virtual void SetValue(int row, int column, double val)
		{
			int index = GetIndex(row, column);
			_data[index] = val;
		}

		public virtual double GetValue(int row, int column)
		{
			int index = Search(row, column);
			if (index >= 0)
				return _data[index];
		    if (row >= 0 && row < row_count && column >= 0 && column < column_count)
		        return 0.0;
		    throw new IndexOutOfRangeException("Row " + row + " Column " + column);
		}

		public virtual void AddValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < row.Length; ++i)
				for (int j = 0; j < column.Length; ++j)
					AddValue(row[i], column[j], values[i, j]);
		}

		public virtual void SetValues(int[] row, int[] column, double[,] values)
		{
			for (int i = 0; i < row.Length; ++i)
				for (int j = 0; j < column.Length; ++j)
					SetValue(row[i], column[j], values[i, j]);
		}

		public virtual double[,] GetValues(int[] row, int[] column)
		{
			var sub = new double[row.Length, column.Length];
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

		public virtual int[] RowIndices
		{
			get
			{
				Compact();
				return _row;
			}
			set
			{
				_row = value;
				_offset = value.Length;
			}
		}

		public virtual int[] ColumnIndices
		{
			get
			{
				Compact();
				return _column;
			}
			set
			{
				_column = value;
				_offset = value.Length;
			}
		}

		public virtual void SetData(double[] data)
		{
			this._data = data;
			_offset = data.Length;
		}

		/// <summary> Tries to get the index, or allocates more space if needed</summary>
		private int GetIndex(int rowI, int columnI)
		{
			int index = Search(rowI, columnI);
			if (index >= 0)
				// Found
				return index;
		    // Not found

		    // Need to allocate more space
		    if (_offset >= _data.Length)
		        if (_offset == 0)
		            Realloc(1);
		        else
		            Realloc(2*_offset);

		    // Put in the new indices
		    _row[_offset] = rowI;
		    _column[_offset] = columnI;
		    return _offset++;
		}

		/// <summary> Reallocates the internal arrays to a new size</summary>
		private void Realloc(int newSize)
		{
			int[] newRow = new int[newSize], newColumn = new int[newSize];
			var newData = new double[newSize];

			Array.Copy(_row, 0, newRow, 0, _offset);
			Array.Copy(_column, 0, newColumn, 0, _offset);
			Array.Copy(_data, 0, newData, 0, _offset);

			_row = newRow;
			_column = newColumn;
			_data = newData;
		}

		/// <summary> Searches for the given entry. Returns -1 if it is not found</summary>
		private int Search(int rowI, int columnI)
		{
			for (int i = 0; i < _offset; ++i)
				if (_row[i] == rowI && _column[i] == columnI)
					return i;
			return - 1;
		}

		public virtual void Compact()
		{
			if (_offset < _row.Length) Realloc(_offset);
		}
	}
}