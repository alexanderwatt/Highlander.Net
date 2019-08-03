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
using System.Collections;
using Orion.Analytics.LinearAlgebra.Sparse;
using Orion.Analytics.Solvers;

namespace Orion.Analytics.Maths.Collections
{
    /// <summary>
    ///		A strongly-typed collection of <see cref="double"/> objects.
    /// </summary>
    [Serializable]
    public class DoubleVector : IDenseAccessVector, IList, ICloneable 
    {
        #region Interfaces

        /// <summary>
        ///		Supports type-safe iteration over a <see cref="DoubleVector"/>.
        /// </summary>
        public interface IDoubleVectorEnumerator
        {
            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            double Current {get;}

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

        #region IDenseAccessVector Interface

        /// <summary>
        ///		The mehtod for the IDenseAccessVector interface.
        /// </summary>
        public virtual double[] Vector
        {
            get => _mArray;
            set => _mArray = value;
        }

        #endregion

        #region IVector Interface

        /// <summary>
        ///		The mehtod for the IDenseAccessVector interface.
        /// </summary>
        public int Length => _mLength;

        #endregion

        private const int DefaultCapacity = 16; 

        #region Implementation (data)

        private double[] _mArray;
        private readonly int _mLength;
        private int _mCount;
        [NonSerialized]
        public int MVersion;

        #endregion

        #region Apply Unary Functions

        /// <summary>
        /// Apply the given unary function to all elements.
        /// </summary>
        /// <param name="f">The unary function implementing <see cref="IUnaryFunction"/>.</param>
        /// <returns>The resulting vector.</returns>
        /// <seealso cref="IUnaryFunction"/>
        /// <seealso cref="UnaryFunction">UnaryFunction delegate</seealso>
        /// <seealso cref="Functional"/>
        public DoubleVector Apply(IUnaryFunction f)
        {
            return Apply(f.Value);
        }

        /// <summary>
        /// Apply the given unary function to all elements.
        /// </summary>
        /// The <see cref="IUnaryFunction"/> delegate applied to all
        /// values of x.
        /// <returns>The resulting vector.</returns>
        /// <seealso cref="UnaryFunction"/>
        /// <seealso cref="Functional">UnaryFunction delegate</seealso>
        /// <seealso cref="UnaryFunction"/>
        public DoubleVector Apply(UnaryFunction f)
        {
            var temp = new DoubleVector(this);
            for (int i = 0; i < temp.Length; i++)
                temp._mArray[i] = f(_mArray[i]);
            return temp;
        }

        /// <summary>
        /// Finds the position of the first element in an ordered range that has a 
        /// value that is greater than a specified value.
        /// </summary>
        /// <param name="val">
        /// The value in the ordered range that needs to be exceeded by the value of the
        /// element addressed by the index returned.
        /// </param>
        /// <returns>
        /// The index addressing the position of the first element in an ordered range
        /// that has a value that is greater than the specified value.
        /// </returns>
        // TODO: implement using binary search
        public int UpperBound(double val)
        {
            for (int i = 0; i < _mArray.Length; i++)
                if (_mArray[i++] > val)
                    return i;
            return 0;
        }

        /// <summary>
        /// Finds the position where the first element in an ordered range is or would be
        /// if it had a value that is less than or equivalent to a specified value.
        /// </summary>
        /// <param name="val">
        /// The value whose first position or possible first position is being searched for
        /// in the ordered range. 
        /// </param>
        /// <returns>
        /// The index addressing the position where the first element in an ordered range
        /// is or would be if it had a value that is less than or equivalent to the
        /// specified value.
        /// </returns>
        // TODO: implement using binary search
        public int LowerBound(double val)
        {
            for (int i = _mArray.Length; i != 0; )
                if (_mArray[--i] <= val)
                    return i;
            return 0;
        }

        /// <summary>Multiplies in place this <c>Vector</c> by a scalar.</summary>
        /// <seealso cref="operator * (double, DoubleVector)"/>
        public virtual void Multiply(double s)
        {
            for (int i = 0; i < Length; i++)
            {
                this[i] *= s; 
            }
        }

        /// <summary>Multiplication of a vector by a scalar, C = s*A</summary>
        public static DoubleVector operator *(double s, DoubleVector v)
        {
            DoubleVector x = new DoubleVector(v.Length);
            for (int i = 0; i < v.Length; i++)
            {
                x[i] = s * v[i];
            }
            return x;
        }

        #endregion

        #region Type casts

        public static explicit operator double[](DoubleVector v)
        {
            if (v.Count == v._mArray.Length)
                return (double[])v._mArray.Clone();
            double[] data = new double[v.Count];
            v.CopyTo(data, 0);
            return data;
        }

        #endregion

        #region Static Wrappers
        /// <summary>
        ///		Creates a synchronized (thread-safe) wrapper for a 
        ///     <c>DoubleVector</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>DoubleVector</c> wrapper that is synchronized (thread-safe).
        /// </returns>
        public static DoubleVector Synchronized(DoubleVector list)
        {
            if(list==null)
                throw new ArgumentNullException(nameof(list));
            return new SyncDoubleVector(list);
        }
        
        /// <summary>
        ///		Creates a read-only wrapper for a 
        ///     <c>DoubleVector</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>DoubleVector</c> wrapper that is read-only.
        /// </returns>
        public static DoubleVector ReadOnly(DoubleVector list)
        {
            if(list==null)
                throw new ArgumentNullException(nameof(list));
            return new ReadOnlyDoubleVector(list);
        }
        #endregion

        #region Construction

        /// <summary>
        ///		Initializes a new instance of the <c>DoubleVector</c> class
        ///		that is empty and has the default initial capacity.
        /// </summary>
        public DoubleVector()
        {
            _mArray = new double[DefaultCapacity];
            _mLength = DefaultCapacity; 
        }

        /// <summary>
        ///		Initializes a new instance of the <c>DoubleVector</c>
        ///     from a sparsevector type that is empty and has the default initial capacity.
        /// </summary>
        public DoubleVector(SparseVector v)
        {
            for (int i = 0; i < v.Length; i++)
                _mArray[i] = v.Data[i];
            _mLength = v.Length;
        }
       
        /// <summary>
        ///		Initializes a new instance of the <c>DoubleVector</c> class
        ///		that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///		The number of elements that the new <c>DoubleVector</c> is initially capable of storing.
        ///	</param>
        public DoubleVector(int capacity)
        {
            _mArray = new double[capacity];
            _mLength = capacity;
        }

        /// <summary>
        /// Creates the array and fills it according to
        /// <code>a[0]=value; a[i]=a[i-1]+increment;</code>.
        /// </summary>
        /// <param name="size"></param>
        /// <param name="value"></param>
        /// <param name="increment"></param>
        public DoubleVector(int size, double value, double increment)
            : this(size)
        {
            Fill(value, increment);
            _mLength = size;
            _mCount = size;
        }
        
        /// <summary>
        ///		Initializes a new instance of the <c>DoubleVector</c> class
        ///		that contains elements copied from the specified <c>DoubleVector</c>.
        /// </summary>
        /// <param name="c">The <c>DoubleVector</c> whose elements are copied to the new collection.</param>
        public DoubleVector(DoubleVector c)
        {
            _mArray = c._mArray;
            //AddRange(c);
            _mLength = c.Length;
            _mCount = c.Length;
        }

        /// <summary>
        ///		Initializes a new instance of the <c>DoubleVector</c> class
        ///		that contains elements copied from the specified <see cref="double"/> array.
        /// </summary>
        /// <param name="a">The <see cref="double"/> array whose elements are copied to the new list.</param>
        public DoubleVector(double[] a)
        {
            _mArray = a;
            //AddRange(a);
            _mLength = a.Length;
            _mCount = a.Length;
        }
        /// <summary>
        ///		Initializes a new instance of the <c>DoubleVector</c> class
        ///		that contains elements copied from the specified <see cref="double"/> array.
        /// </summary>
        /// <param name="value">The <see cref="double"/> array whose elements are copied to the new list.</param>
        public DoubleVector(double value)
            : this(DefaultCapacity)
        {
            Fill(value);
            _mLength = DefaultCapacity; 
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
                _mArray[i] = value;

        }

        public void Fill(double value, double increment)
        {
            for (int i = 0; i < Length; value += increment)
                _mArray[i++] = value;
        }

        #endregion

        #region Operations (type-safe ICollection)
        /// <summary>
        ///		Gets the number of elements actually contained in the <c>DoubleVector</c>.
        /// </summary>
        public virtual int Count => _mCount;

        /*      /// <summary>
        ///		Gets the number of elements actually contained in the <c>DoubleVector</c>.
        /// </summary>
        public virtual int Length
        {
            get { return m_array.Length; }
        }  */

        /// <summary>
        ///		Copies the entire <c>DoubleVector</c> to a one-dimensional
        ///		string array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="double"/> array to copy to.</param>
        public virtual void CopyTo(double[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        ///		Copies the entire <c>DoubleVector</c> to a one-dimensional
        ///		<see cref="double"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="double"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(double[] array, int start)
        {
            if (_mCount > array.GetUpperBound(0) + 1 - start)
                throw new ArgumentException("Destination array was not long enough.");
			
            Array.Copy(_mArray, 0, array, start, _mCount); 
        }

        /// <summary>
        ///		Gets a value indicating whether access to the collection is synchronized (thread-safe).
        /// </summary>
        /// <returns>true if access to the ICollection is synchronized (thread-safe); otherwise, false.</returns>
        public virtual bool IsSynchronized => _mArray.IsSynchronized;

        /// <summary>
        ///		Gets an object that can be used to synchronize access to the collection.
        /// </summary>
        public virtual object SyncRoot => _mArray.SyncRoot;

        #endregion
		
        #region Operations (type-safe IList)

        /// <summary>
        ///		Gets or sets the <see cref="double"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleVector.Count"/>.</para>
        /// </exception>
        public virtual double this[int index]
        {
            get
            {
                ValidateIndex(index); // throws
                return _mArray[index]; 
            }
            set
            {
                ValidateIndex(index); // throws
                ++MVersion; 
                _mArray[index] = value; 
            }
        }

        /// <summary>
        ///	Adds a <see cref="DoubleVector"/> to the base <c>DoubleVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="DoubleVector"/> to be added to the base <c>DoubleVector</c>.</param>
        /// <returns>The DoubleVector.</returns>
        public DoubleVector Add(DoubleVector item)
        {
            if (item.Length != _mArray.Length) return null;
            var result = new DoubleVector(_mArray.Length);
            for(var i=0; i< _mArray.Length; i++)
            {
                result[i] = item[i] + _mArray[i];
            }
            return result;
        }

        /// <summary>
        ///	Adds a <see cref="DoubleVector"/> to the base <c>DoubleVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="DoubleVector"/> to be added to the base <c>DoubleVector</c>.</param>
        /// <returns>The DoubleVector.</returns>
        public DoubleVector Subtract(DoubleVector item)
        {
            if (item.Length != _mArray.Length) return null;
            var result = new DoubleVector(_mArray.Length);
            for (var i = 0; i < _mArray.Length; i++)
            {
                result[i] = _mArray[i] - item[i];
            }
            return result;
        }

        /// <summary>
        ///		Adds a <see cref="double"/> to the end of the <c>DoubleVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to be added to the end of the <c>DoubleVector</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(double item)
        {
            if (_mCount == _mArray.Length)
                EnsureCapacity(_mCount + 1);
            _mArray[_mCount] = item;
            MVersion++;
            return _mCount++;
        }
		
        /// <summary>
        ///		Removes all elements from the <c>DoubleVector</c>.
        /// </summary>
        public virtual void Clear()
        {
            ++MVersion;
            _mArray = new double[DefaultCapacity];
            _mCount = 0;
        }
		
        /// <summary>
        ///		Creates a shallow copy of the <see cref="DoubleVector"/>.
        /// </summary>
        public virtual object Clone()
        {
            DoubleVector newColl = new DoubleVector(_mCount);
            Array.Copy(_mArray, 0, newColl._mArray, 0, _mCount);
            newColl._mCount = _mCount;
            newColl.MVersion = MVersion;

            return newColl;
        }

        /// <summary>
        ///		Determines whether a given <see cref="double"/> is in the <c>DoubleVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>DoubleVector</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(double item)
        {
            for (int i=0; i != _mCount; ++i)
                if (_mArray[i].Equals(item))
                    return true;
            return false;
        }

        /// <summary>
        ///		Returns the zero-based index of the first occurrence of a <see cref="double"/>
        ///		in the <c>DoubleVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to locate in the <c>DoubleVector</c>.</param>
        /// <returns>
        ///		The zero-based index of the first occurrence of <paramref name="item"/> 
        ///		in the entire <c>DoubleVector</c>, if found; otherwise, -1.
        ///	</returns>
        public virtual int IndexOf(double item)
        {
            for (int i=0; i != _mCount; ++i)
                if (_mArray[i].Equals(item))
                    return i;
            return -1;
        }

        /// <summary>
        ///		Inserts an element into the <c>DoubleVector</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="double"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleVector.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, double item)
        {
            ValidateIndex(index, true); // throws
			
            if (_mCount == _mArray.Length)
                EnsureCapacity(_mCount + 1);

            if (index < _mCount)
            {
                Array.Copy(_mArray, index, _mArray, index + 1, _mCount - index);
            }

            _mArray[index] = item;
            _mCount++;
            MVersion++;
        }

        /// <summary>
        ///		Removes the first occurrence of a specific <see cref="double"/> from the <c>DoubleVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="double"/> to remove from the <c>DoubleVector</c>.</param>
        /// <exception cref="ArgumentException">
        ///		The specified <see cref="double"/> was not found in the <c>DoubleVector</c>.
        /// </exception>
        public virtual void Remove(double item)
        {		   
            int i = IndexOf(item);
            if (i < 0)
                throw new ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");
			
            ++MVersion;
            RemoveAt(i);
        }

        /// <summary>
        ///		Removes the element at the specified index of the <c>DoubleVector</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleVector.Count"/>.</para>
        /// </exception>
        public virtual void RemoveAt(int index)
        {
            ValidateIndex(index); // throws		
            _mCount--;
            if (index < _mCount)
            {
                Array.Copy(_mArray, index + 1, _mArray, index, _mCount - index);
            }			
            // We can't set the deleted entry equal to null, because it might be a value type.
            // Instead, we'll create an empty single-element array of the right type and copy it 
            // over the entry we want to erase.
            double[] temp = new double[1];
            Array.Copy(temp, 0, _mArray, _mCount, 1);
            MVersion++;
        }

        /// <summary>
        ///		Gets a value indicating whether the collection has a fixed size.
        /// </summary>
        /// <value>true if the collection has a fixed size; otherwise, false. The default is false</value>
        public virtual bool IsFixedSize => false;

        /// <summary>
        ///		gets a value indicating whether the IList is read-only.
        /// </summary>
        /// <value>true if the collection is read-only; otherwise, false. The default is false</value>
        public virtual bool IsReadOnly => false;

        #endregion

        #region Operations (type-safe IEnumerable)
		
        /// <summary>
        ///		Returns an enumerator that can iterate through the <c>DoubleVector</c>.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <c>DoubleVector</c>.</returns>
        public virtual IDoubleVectorEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

        #region Public helpers (just to mimic some nice features of ArrayList)
		
        /// <summary>
        ///		Gets or sets the number of elements the <c>DoubleVector</c> can contain.
        /// </summary>
        public virtual int Capacity
        {
            get => _mArray.Length;
            set
            {
                if (value < _mCount)
                    value = _mCount;
                if (value != _mArray.Length)
                {
                    if (value > 0)
                    {
                        double[] temp = new double[value];
                        Array.Copy(_mArray, temp, _mCount);
                        _mArray = temp;
                    }
                    else
                    {
                        _mArray = new double[DefaultCapacity];
                    }
                }
            }
        }

        /// <summary>
        ///		Adds the elements of another <c>DoubleVector</c> to the current <c>DoubleVector</c>.
        /// </summary>
        /// <param name="x">The <c>DoubleVector</c> whose elements should be added to the end of the current <c>DoubleVector</c>.</param>
        /// <returns>The new <see cref="DoubleVector.Count"/> of the <c>DoubleVector</c>.</returns>
        public int AddRange(DoubleVector x)
        {
            if (_mCount + x.Count >= _mArray.Length)
                EnsureCapacity(_mCount + x.Count);		
            Array.Copy(x._mArray, 0, _mArray, _mCount, x.Count);
            _mCount += x.Count;
            MVersion++;
            return _mCount;
        }

        /// <summary>
        ///		Adds the elements of a <see cref="double"/> array to the current <c>DoubleVector</c>.
        /// </summary>
        /// <param name="x">The <see cref="double"/> array whose elements should be added to the end of the <c>DoubleVector</c>.</param>
        /// <returns>The new <see cref="DoubleVector.Count"/> of the <c>DoubleVector</c>.</returns>
        public virtual int AddRange(double[] x)
        {
            if (_mCount + x.Length >= _mArray.Length)
                EnsureCapacity(_mCount + x.Length);
            Array.Copy(x, 0, _mArray, _mCount, x.Length);
            _mCount += x.Length;
            MVersion++;
            return _mCount;
        }
		
        /// <summary>
        ///		Sets the capacity to the actual number of elements.
        /// </summary>
        public virtual void TrimToSize()
        {
            Capacity = _mCount;
        }

        #endregion

        #region Implementation (helpers)

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="DoubleVector.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int index, bool allowEqualEnd = false)
        {
            int max = allowEqualEnd?_mCount:_mCount-1;
            if (index < 0 || index > max)
                throw new ArgumentOutOfRangeException(nameof(index), index, "Specified argument was out of the range of valid values.");
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = _mArray.Length == 0 ? DefaultCapacity : _mArray.Length * 2;
            if (newCapacity < min)
                newCapacity = min;
            Capacity = newCapacity;
        }

        #endregion
		
        #region Implementation (ICollection)

        void ICollection.CopyTo(Array array, int start)
        {
            CopyTo((double[])array, start);
        }

        #endregion

        #region Implementation (IList)

        object IList.this[int i]
        {
            get => this[i];
            set => this[i] = (double)value;
        }

        int IList.Add(object x)
        {
            return Add((double)x);
        }

        bool IList.Contains(object x)
        {
            return Contains((double)x);
        }

        int IList.IndexOf(object x)
        {
            return IndexOf((double)x);
        }

        void IList.Insert(int pos, object x)
        {
            Insert(pos, (double)x);
        }

        void IList.Remove(object x)
        {
            Remove((double)x);
        }

        void IList.RemoveAt(int pos)
        {
            RemoveAt(pos);
        }

        #endregion

        #region Implementation (IEnumerable)

        IEnumerator IEnumerable.GetEnumerator()
        {
            return (IEnumerator)(GetEnumerator());
        }

        #endregion

        #region Nested enumerator class
        /// <summary>
        ///		Supports simple iteration over a <see cref="DoubleVector"/>.
        /// </summary>
        private class Enumerator : IEnumerator, IDoubleVectorEnumerator
        {
            #region Implementation (data)
			
            private readonly DoubleVector _mCollection;
            private int _mIndex;
            private readonly int _mVersion;
			
            #endregion
		
            #region Construction
			
            /// <summary>
            ///		Initializes a new instance of the <c>Enumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal Enumerator(DoubleVector tc)
            {
                _mCollection = tc;
                _mIndex = -1;
                _mVersion = tc.MVersion;
            }
			
            #endregion
	
            #region Operations (type-safe IEnumerator)
			
            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            public double Current => _mCollection[_mIndex];

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
            public bool MoveNext()
            {
                if (_mVersion != _mCollection.MVersion)
                    throw new InvalidOperationException("Collection was modified; enumeration operation may not execute.");

                ++_mIndex;
                return (_mIndex < _mCollection.Count);
            }

            /// <summary>
            ///		Sets the enumerator to its initial position, before the first element in the collection.
            /// </summary>
            public void Reset()
            {
                _mIndex = -1;
            }
            #endregion
	
            #region Implementation (IEnumerator)
			
            object IEnumerator.Current => Current;

            #endregion
        }
        #endregion
        
        #region Nested Syncronized Wrapper class

        private class SyncDoubleVector : DoubleVector
        {
            #region Implementation (data)
            private readonly DoubleVector _mCollection;
            private readonly object _mRoot;
            #endregion

            #region Construction
            internal SyncDoubleVector(DoubleVector list)
            {
                _mRoot = list.SyncRoot;
                _mCollection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(double[] array)
            {
                lock(_mRoot)
                    _mCollection.CopyTo(array);
            }

            public override void CopyTo(double[] array, int start)
            {
                lock(_mRoot)
                    _mCollection.CopyTo(array,start);
            }
            public override int Count
            {
                get
                { 
                    lock(_mRoot)
                        return _mCollection.Count;
                }
            }

            public override bool IsSynchronized => true;

            public override object SyncRoot => _mRoot;

            #endregion
            
            #region Type-safe IList
            public override double this[int i]
            {
                get
                {
                    lock(_mRoot)
                        return _mCollection[i];
                }
                set
                {
                    lock(_mRoot)
                        _mCollection[i] = value; 
                }
            }

            public override int Add(double x)
            {
                lock(_mRoot)
                    return _mCollection.Add(x);
            }
            
            public override void Clear()
            {
                lock(_mRoot)
                    _mCollection.Clear();
            }

            public override bool Contains(double x)
            {
                lock(_mRoot)
                    return _mCollection.Contains(x);
            }

            public override int IndexOf(double x)
            {
                lock(_mRoot)
                    return _mCollection.IndexOf(x);
            }

            public override void Insert(int pos, double x)
            {
                lock(_mRoot)
                    _mCollection.Insert(pos,x);
            }

            public override void Remove(double x)
            {           
                lock(_mRoot)
                    _mCollection.Remove(x);
            }

            public override void RemoveAt(int pos)
            {
                lock(_mRoot)
                    _mCollection.RemoveAt(pos);
            }
            
            public override bool IsFixedSize => _mCollection.IsFixedSize;

            public override bool IsReadOnly => _mCollection.IsReadOnly;

            #endregion

            #region Type-safe IEnumerable
            public override IDoubleVectorEnumerator GetEnumerator()
            {
                lock(_mRoot)
                    return _mCollection.GetEnumerator();
            }
            #endregion

            #region Public Helpers

            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get
                {
                    lock(_mRoot)
                        return _mCollection.Capacity;
                }
                
                set
                {
                    lock(_mRoot)
                        _mCollection.Capacity = value;
                }
            }

            //public override int AddRange(DoubleVector x)
            //{
            //    lock(_mRoot)
            //        return _mCollection.AddRange(x);
            //}

            public override int AddRange(double[] x)
            {
                lock(_mRoot)
                    return _mCollection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class

        private class ReadOnlyDoubleVector : DoubleVector
        {
            #region Implementation (data)
            private readonly DoubleVector _mCollection;
            #endregion

            #region Construction
            internal ReadOnlyDoubleVector(DoubleVector list)
            {
                _mCollection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(double[] array)
            {
                _mCollection.CopyTo(array);
            }

            public override void CopyTo(double[] array, int start)
            {
                _mCollection.CopyTo(array,start);
            }
            public override int Count => _mCollection.Count;

            public override bool IsSynchronized => _mCollection.IsSynchronized;

            public override object SyncRoot => _mCollection.SyncRoot;

            #endregion
            
            #region Type-safe IList
            public override double this[int i]
            {
                get => _mCollection[i];
                set => throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int Add(double x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(double x)
            {
                return _mCollection.Contains(x);
            }

            public override int IndexOf(double x)
            {
                return _mCollection.IndexOf(x);
            }

            public override void Insert(int pos, double x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(double x)
            {           
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void RemoveAt(int pos)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override bool IsFixedSize => true;

            public override bool IsReadOnly => true;

            #endregion

            #region Type-safe IEnumerable
            public override IDoubleVectorEnumerator GetEnumerator()
            {
                return _mCollection.GetEnumerator();
            }
            #endregion

            #region Public Helpers

            // (just to mimic some nice features of ArrayList)
            public override int Capacity
            {
                get => _mCollection.Capacity;
                set => throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            //public override int AddRange(DoubleVector x)
            //{
            //    throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            //}

            //public override int AddRange(double[] x)
            //{
            //    throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            //}

            #endregion
        }
        #endregion
    }
}