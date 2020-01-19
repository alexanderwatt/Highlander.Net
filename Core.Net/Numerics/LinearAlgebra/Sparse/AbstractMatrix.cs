/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System;

namespace Highlander.Numerics.LinearAlgebra.Sparse
{
	/// <summary> Partial implementation of Matrix</summary>
	[Serializable]
	public abstract class AbstractMatrix : IMatrix
	{
		public virtual bool Square => row_count == column_count;

	    /// <summary> Matrix dimensions</summary>
		protected internal int row_count;

	    /// <summary> Matrix dimensions</summary>
	    protected internal int column_count;

	    /// <summary> Constructor for AbstractMatrix.</summary>
		protected AbstractMatrix(int numRows, int numColumns)
		{
			if (numRows < 0 || numColumns < 0)
				throw new IndexOutOfRangeException("Matrix size cannot be negative");
            row_count = numRows;
			column_count = numColumns;
		}

		/// <summary> Constructor for AbstractMatrix, same size as A.</summary>
		protected AbstractMatrix(IMatrix A) : this(A.RowCount, A.ColumnCount)
		{
		}

		public virtual int RowCount => row_count;

	    public virtual int ColumnCount => column_count;
	}
}