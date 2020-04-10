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
	/// <summary> Dense vector.</summary>
	[Serializable]
	public class DenseVector : AbstractVector, IDenseAccessVector, IElementalAccessVector, IVectorAccess
	{
		/// <summary> Internal representation</summary>
		private double[] _data;

		/// <summary> Constructor for DenseVector.</summary>
		public DenseVector(int size) : base(size)
		{
			_data = new double[size];
		}

		/// <summary> Constructor for DenseVector, and copies the contents from the
		/// supplied vector.</summary>
		public DenseVector(IVector x) : this(x.Length)
		{
			Blas.Default.Copy(x, this);
		}

		public virtual double[] Vector
		{
			get => _data;
		    set => _data = value;
		}

		public virtual void AddValue(int index, double val)
		{
			_data[index] += val;
		}

		public virtual void SetValue(int index, double val)
		{
			_data[index] = val;
		}

		public virtual double GetValue(int index)
		{
			return _data[index];
		}

		public virtual void AddValues(int[] index, double[] values)
		{
			for (int i = 0; i < index.Length; ++i)
					_data[index[i]] += values[i];
		}

		public virtual void SetValues(int[] index, double[] values)
		{
			for (int i = 0; i < index.Length; ++i)
					_data[index[i]] = values[i];
		}

		public virtual double[] GetValues(int[] index)
		{
			double[] ret = new double[index.Length];
			for (int i = 0; i < index.Length; ++i)
				ret[i] = _data[index[i]];
			return ret;
		}

		public virtual double[] Data => _data;
	}
}