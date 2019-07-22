using System;

namespace Orion.Analytics.LinearAlgebra.Sparse
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