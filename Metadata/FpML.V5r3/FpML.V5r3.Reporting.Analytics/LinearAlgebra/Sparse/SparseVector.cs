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

#region Using directives

using System;
using System.Collections;
using Orion.Analytics.Solvers;
using Orion.Analytics.Utilities;

#endregion

namespace Orion.Analytics.LinearAlgebra.Sparse
{
	/// <summary> Sparse vector.</summary>
	[Serializable]
    public class SparseVector : AbstractVector, IElementalAccessVector, ISparseAccessVector, IVectorAccess, IEnumerable, IEquatable<SparseVector>
	{
	    /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
	    /// <param name="other">An object to compare with this object.</param>
	    /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
	    public bool Equals(SparseVector other)
	    {
	        if (ReferenceEquals(null, other)) return false;
	        if (ReferenceEquals(this, other)) return true;
	        return Equals(_data, other._data) && Equals(_ind, other._ind) && _used == other._used;
	    }

	    /// <summary>Determines whether the specified object is equal to the current object.</summary>
	    /// <param name="obj">The object to compare with the current object. </param>
	    /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
	    public override bool Equals(object obj)
	    {
	        if (ReferenceEquals(null, obj)) return false;
	        if (ReferenceEquals(this, obj)) return true;
	        if (obj.GetType() != GetType()) return false;
	        return Equals((SparseVector) obj);
	    }

	    /// <summary>Serves as the default hash function. </summary>
	    /// <returns>A hash code for the current object.</returns>
	    public override int GetHashCode()
	    {
	        unchecked
	        {
	            var hashCode = (_data != null ? _data.GetHashCode() : 0);
	            hashCode = (hashCode * 397) ^ (_ind != null ? _ind.GetHashCode() : 0);
	            hashCode = (hashCode * 397) ^ _used;
	            return hashCode;
	        }
	    }

	    /// <summary> Data</summary>
		private double[] _data;

		/// <summary> Indices to data</summary>
		private int[] _ind;

		/// <summary> How much has been used</summary>
		private int _used;

	    /// <summary> Constructor for SparseVector.</summary>
	    /// <param name="size"></param>
	    /// <param name="nz">Initial number of non-zeros.</param>
	    public SparseVector(int size, int nz) : base(size)
		{
			_data = new double[nz];
			_ind = new int[nz];
		}

	    /// <summary> Constructor for SparseVector, and copies the contents from the
	    /// supplied vector.</summary>
	    /// <param name="x"></param>
	    /// <param name="nz">Initial number of non-zeros.</param>
	    public SparseVector(IVector x, int nz) : this(x.Length, nz)
		{
			Blas.Default.Copy(x, this);
		}

		/// <summary> Constructor for SparseVector, and copies the contents from the
		/// supplied vector. Zero initial pre-allocation.</summary>
		public SparseVector(IVector x) : this(x.Length)
		{
			Blas.Default.Copy(x, this);
		}

        /// <summary> Constructor for SparseVector, and copies the contents from the
        /// supplied daouble array. Zero initial pre-allocation.</summary>
        public SparseVector(double[] x, bool copy): this(x.Length)
        {
            if (copy)
            {
                SparseVector temp = new SparseVector(x, false);
                Blas.Default.Copy(temp, this);
            }
            else
            {
                _data = x;
            }
        }
        
        /// <summary> Constructor for SparseVector. Zero initial pre-allocation</summary>
		public SparseVector(int size) : this(size, 0)
		{
		}

        /// <summary>
		/// Creates the array and fills it according to
		/// <code>a[0]=value; a[i]=a[i-1]+increment;</code>.
		/// </summary>
		/// <param name="size"></param>
		/// <param name="value"></param>
		/// <param name="increment"></param>
		public SparseVector(int size, double value, double increment)
			: this(size)
		{
			Fill(value, increment);
		}

	    /// <summary>
	    /// Creates the array and fills it according to
	    /// <code>a[0]=value; a[i]=a[i-1];</code>.
	    /// </summary>
	    /// <param name="size"></param>
	    /// <param name="value"></param>
	    public SparseVector(int size, double value)
            : this(size)
        {
            Fill(value, 0.0);
        }

		public virtual void AddValue(int index, double val)
		{
			int i = GetIndex(index);
			_data[i] += val;
		}

		public virtual void SetValue(int index, double val)
		{
			int i = GetIndex(index);
			_data[i] = val;
		}

		public virtual double GetValue(int index)
		{
			int inRenamed = ArraySupport.BinarySearch(_ind, index, 0, _used);
			if (inRenamed != - 1)
				return _data[inRenamed];
		    if (index < _length && index >= 0)
		        return 0.0;
		    throw new IndexOutOfRangeException("Index " + index);
		}

		public virtual void AddValues(int[] index, double[] values)
		{
			for (int i = 0; i < index.Length; ++i)
				AddValue(index[i], values[i]);
		}

		public virtual void SetValues(int[] index, double[] values)
		{
			for (int i = 0; i < index.Length; ++i)
				SetValue(index[i], values[i]);
		}

		public virtual double[] GetValues(int[] index)
		{
			double[] ret = new double[index.Length];
			for (int i = 0; i < index.Length; ++i)
				ret[i] = GetValue(index[i]);
			return ret;
		}

		/// <summary> Tries to find the index. If it is not found, a reallocation is done, and
		/// a new index is returned.</summary>
		private int GetIndex(int index)
		{
			// Try to find column index
			int i = ArraySupport.BinarySearchGreater(_ind, index, 0, _used);

			// Found
			if (i < _used && _ind[i] == index)
				return i;

			// Not found, try to make room
			if (index < 0 || index >= _length)
				throw new IndexOutOfRangeException("Index " + index);
			_used++;

			// Check available memory
			if (_used > _data.Length)
			{
				// If zero-length, use new length of 1, else double the bandwidth
				int newLength = 1;
				if (_data.Length != 0)
					newLength = 2*_data.Length;

				// Copy existing data into new arrays
				int[] newInd = new int[newLength];
				double[] newDat = new double[newLength];
				Array.Copy(_ind, 0, newInd, 0, _data.Length);
				Array.Copy(_data, 0, newDat, 0, _data.Length);

				// Update pointers
				_ind = newInd;
				_data = newDat;
			}

			// All ok, make room for insertion
			for (int j = _used - 1; j >= i + 1; --j)
			{
				_ind[j] = _ind[j - 1];
				_data[j] = _data[j - 1];
			}

			// Put in new structure
			_ind[i] = index;
			_data[i] = 0.0;
			return i;
		}

		public virtual IntDoubleVectorPair Vector
		{
			get
			{
				Compact();
				return new IntDoubleVectorPair(_ind, _data);
			}
			set => SetValues(value.Indices, value.Data);
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
			if (_used < _ind.Length)
			{
				double[] newData = new double[_used];
				Array.Copy(_data, 0, newData, 0, _used);
				int[] newIndex = new int[_used];
				Array.Copy(_ind, 0, newIndex, 0, _used);
				_data = newData;
				_ind = newIndex;
			}
        }

        #region Apply a Unary function to the vector

        /// <summary>
        /// Apply the given unary function to all elements.
        /// </summary>
        /// <param name="f">The unary function implementing <see cref="IUnaryFunction"/>.</param>
        /// <returns>The resulting vector.</returns>
        /// <seealso cref="IUnaryFunction"/>
        /// <seealso cref="UnaryFunction">UnaryFunction delegate</seealso>
        /// <seealso cref="Functional"/>
        public SparseVector Apply(IUnaryFunction f)
        {
            return Apply(f.Value);
        }

        /// <summary>
        /// Apply the given unary function to all elements.
        /// </summary>
        /// The <see cref="UnaryFunction"/> delegate applied to all
        /// values of x.
        /// <returns>The resulting vector.</returns>
        /// <seealso cref="IUnaryFunction"/>
        /// <seealso cref="UnaryFunction">UnaryFunction delegate</seealso>
        /// <seealso cref="Functional"/>
        public SparseVector Apply(UnaryFunction f)
        {
            SparseVector temp = Clone();
            for (int i = 0; i < Length; i++)
                temp.Data[i] = f(Data[i]);
            return temp;
        }

        #endregion

        /// <summary>
        ///		Creates a shallow copy of the <see cref="Vector"/>.
        /// </summary>
        public virtual SparseVector Clone()
        {
            return new SparseVector(this);
        }

        #region Vector Math functions

        /// <summary>
        /// Calculate absolute values of the vector elements.
        /// </summary>
        public void Abs()
        {
            for (int i = 0; i < Length; i++)
                _data[i] = Math.Abs(_data[i]);
        }

        /// <summary> Copies the contents from the
        /// supplied vector. Zero initial pre-allocation.</summary>
        public void Copy(SparseVector x)
        {
            Blas.Default.Copy(x, this);
        }

        /// <summary>
        /// Adds the elements of another vector.
        /// </summary>
        public void Add(SparseVector vectorX)
        {
            int n = Length;
            if (n != vectorX.Length)
                throw new ArgumentException("VectorDimensionsNotEqual");
            for (int i = 0; i < n; i++)
                _data[i] = _data[i]+vectorX.Data[i];
        }

        /// <summary>
        /// Adds the elements of another vector.
        /// </summary>
        public void Add(SparseVector vectorX, double scalar)
        {
            Blas.Default.Add(scalar,vectorX,this);
        }

        /// <summary>
        /// Subtracts the elements of another vector.
        /// </summary>
        public void Subtract(SparseVector vectorX)
        {
            int n = Length;
            if (n != vectorX.Length)
                throw new ArgumentException("VectorDimensionsNotEqual");
            for (int i = 0; i < n; i++)
                _data[i] = _data[i] - vectorX.Data[i];
        }

        #region Vector mult/div with scalar -- prefer in-place

        public void Multiply(double scalar)
        {
            if (scalar == 1.0) return;
/*            if (scalar == 0.0 && inc == 1)
                Array.Clear(this.data, offset, count);*/
            Blas.Default.Scale(scalar, this);
        }

        public static SparseVector operator *(double scalar, SparseVector v)
        {
            SparseVector tmp = v.Clone();
            tmp.Multiply(scalar);
            return tmp;
        }

        public static SparseVector operator *(SparseVector v, double scalar)
        {
            SparseVector tmp = v.Clone();
            tmp.Multiply(scalar);
            return tmp;
        }

        public static SparseVector operator /(SparseVector v, double scalar)
        {
            SparseVector tmp = v.Clone();
            tmp.Multiply(1.0 / scalar);
            return tmp;
        }

        public void Divide(double scalar)
        {
            Multiply(1.0 / scalar);
        }
        // Array operator/(double, const Array&);


        public void Multiply(SparseVector other)
        {
            int n = Length;
            if (n != other.Length)
                throw new ArgumentException("VectorDimensionsNotEqual");
            // TODO: optimize this!!!!!
            for (int i = 0; i < n; i++)
                Data[i] *= other.Data[i];
        }

        public static SparseVector operator *(SparseVector x, SparseVector y)
        {
            SparseVector tmp = x.Clone();
            tmp.Multiply(y);
            return tmp;
        }

        /*        public static Matrix OuterProduct(Vector x, Vector y)
                {
                    Matrix temp = new Matrix(x.Length, y.Length);
                    BLAS.ger(temp.RowCount, temp.ColumnCount, 1.0,
                        ref x.data[x.offset], x.inc,
                        ref y.data[y.offset], y.inc,
                        ref temp.data[0], temp.ld);
                    return temp;
                } */

        #endregion

        // Vector operator*=(Vector v);
        // Vector operator/=(Vector v);
        // public static Vector operator/(Vector x, Vector y);

        public static double DotProduct(SparseVector a, SparseVector b)
        {
            if (a.Length != b.Length)
                throw new ArgumentException("Vectors not the same length.");
            return Blas.Default.Dot(a, b);
        }

        public static SparseVector Abs(SparseVector a)
        {
            int n = a.Length;
            SparseVector temp = new SparseVector(n);
            for (int i = 0; i < n; i++)
                temp._data[i] = Math.Abs(a._data[i]);
            return temp;
        }

        public double Sum()
        {
            int n = Length;
            double temp = 0.0;
            for (int i = 0; i < n; i++)
                temp += Data[i];
            return temp;
        }

        public double Min()
        {
            int n = Length;
            double temp = Data[0];
            for (int i = 1; i < n-1; i++)
            {
                if (Data[i] < temp)
                {
                    temp = Data[i];
                }
            }
            return temp;
        }

        public double Max()
        {
            int n = Length;
            double temp = Data[0];
            for (int i = 1; i < n - 1; i++)
            {
                if (Data[i] > temp)
                {
                    temp = Data[i];
                }
            }
            return temp;
        }

        public static void Abs(double[] a)
        {
            int i = a.GetLowerBound(0);
            for (int last = a.GetUpperBound(0); i < last; i++)
                a[i] = Math.Abs(a[i]);
        }
		
        public static SparseVector Sqrt(SparseVector a)
        {
            int n = a.Length;
            SparseVector temp = new SparseVector(n);
            for (int i = 0; i < n; i++)
                temp.SetValue(i, Math.Sqrt(a._data[i]));
            return temp;
        }

        public static SparseVector Log(SparseVector a)
        {
            int n = a.Length;
            SparseVector temp = new SparseVector(n);
            for (int i = 0; i < n; i++)
                temp.SetValue(i,Math.Log(a._data[i]));
            return temp;
        }

        public static SparseVector Exp(SparseVector a)
        {
            int n = a.Length;
            SparseVector temp = new SparseVector(n);
            for (int i = 0; i < n; i++)
                temp.SetValue(i,Math.Exp(a._data[i]));
            return temp;
        }

        #endregion

        #region Vectorial operators

        public static SparseVector operator +(SparseVector vectorX, SparseVector vectorY)
        {
            SparseVector tmp = vectorX.Clone();
            tmp.Add(vectorY);
            return tmp;
        }

        public static SparseVector operator -(SparseVector v)
        {
            return v * -1.0;	// creates new object
        }

        public static bool operator ==(SparseVector x, SparseVector y)
        {
            int n = x.Length;
            if (n != y.Length)
                throw new ArgumentException("VectorDimensionsNotEqual");
            for (int i = 0; i < n; i++)
                if (x.Data[i] != y.Data[i]) return false;
            return true;
            // creates new object
        }
        public static bool operator !=(SparseVector x, SparseVector y)
        {
            return !(x==y);
        }

	    public static SparseVector operator -(SparseVector vectorX, SparseVector vectorY)
        {
            SparseVector tmp = vectorX.Clone();
            tmp.Subtract(vectorY);
            return tmp;
        }

        #endregion

        #region Fill, Reset & Copy

        public void Reset()
        {
            Fill(0.0);
        }

        public void Fill(double value)
        {
            for (int i = 0; i < Length; i++)
                    _data[i++] = value;

        }

        public void Fill(double value, double increment)
        {
            for (int i = 0; i < Length; value += increment)
                    _data[i++] = value;
        }

        #endregion

        #region IVectorEnumerator Interface
        /// <summary>
        ///		Supports type-safe iteration over a <see cref="Vector"/>.
        /// </summary>
        public interface IVectorEnumerator
        {
            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            double Current { get;}

            /// <summary>
            ///		Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <exception cref="InvalidOperationException">
            ///		The collection was modified after the enumerator was created.
            /// </exception>
            /// <returns>
            ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
            ///		<c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            bool MoveNext();

            /// <summary>
            ///		Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            void Reset();
        }
        #endregion

        #region Operations (type-safe IEnumerable)

        /// <summary>
        ///		Returns an enumerator that can iterate through the <c>Vector</c>.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <c>Vector</c>.</returns>
        public virtual IVectorEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        #endregion

        #region Nested enumerator class

        /// <summary>
        ///		Supports simple iteration over a <see cref="Vector"/>.
        /// </summary>
        private class Enumerator : IEnumerator, IVectorEnumerator
        {
            private readonly SparseVector _v;
            private int _i = -1;

            internal Enumerator(SparseVector tc)
            {
                _v = tc;
            }

            public double Current => _v.Data[_i];

            /// <summary>
            ///		Advances the enumerator to the next element in the collection.
            /// </summary>
            /// <returns>
            ///		<c>true</c> if the enumerator was successfully advanced to the next element; 
            ///		<c>false</c> if the enumerator has passed the end of the collection.
            /// </returns>
            public bool MoveNext()
            {
                return (++_i < _v.Length);
            }

            /// <summary>
            ///		Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _i = -1;
            }

            object IEnumerator.Current => Current;
        }

        #endregion

        #region Implementation (IEnumerable)

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)(GetEnumerator());
        }

        #endregion

	    public double this[int i]
	    {
	        get => Data[i];
	        set => Data[i] = value;
	    }
	}
}