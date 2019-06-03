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