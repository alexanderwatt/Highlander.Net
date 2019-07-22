
using System;

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Partial implementation of Vector.</summary>
	[Serializable]
	public abstract class AbstractVector : IVector
	{
        /// <summary> Size of the vector</summary>
        protected int _length;

	    /// <summary> Constructor for AbstractVector.</summary>
	    protected AbstractVector()
	    {}

	    /// <summary> Constructor for AbstractVector.</summary>
            protected AbstractVector(int length)
		{
			if (length < 0)
				throw new IndexOutOfRangeException("Vector length cannot be negative");
		    _length = length;
		}

		/// <summary> Constructor for AbstractVector, same size as x</summary>
		protected AbstractVector(IVector x)
		{
		    _length = x.Length;
		}

		public virtual int Length => _length;
	}
}