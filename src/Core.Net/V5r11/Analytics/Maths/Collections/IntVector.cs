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

namespace Highlander.Reporting.Analytics.V5r3.Maths.Collections
{
    /// <summary>
    ///		A strongly-typed collection of <see cref="int"/> objects.
    /// </summary>
    [Serializable]
    public class IntVector : IList, ICloneable
    {
        #region Interfaces
        /// <summary>
        ///		Supports type-safe iteration over a <see cref="IntVector"/>.
        /// </summary>
        public interface IIntVectorEnumerator
        {
            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            int Current {get;}

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

        private const int DefaultCapacity = 16;

        #region Implementation (data)
        private int[] _mArray;
        private int _mCount;
        [NonSerialized]
        private int _mVersion;
        #endregion
	
        #region Static Wrappers
        /// <summary>
        ///		Creates a synchronized (thread-safe) wrapper for a 
        ///     <c>IntVector</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>IntVector</c> wrapper that is synchronized (thread-safe).
        /// </returns>
        public static IntVector Synchronized(IntVector list)
        {
            if(list==null)
                throw new ArgumentNullException(nameof(list));
            return new SyncIntVector(list);
        }
        
        /// <summary>
        ///		Creates a read-only wrapper for a 
        ///     <c>IntVector</c> instance.
        /// </summary>
        /// <returns>
        ///     An <c>IntVector</c> wrapper that is read-only.
        /// </returns>
        public static IntVector ReadOnly(IntVector list)
        {
            if(list==null)
                throw new ArgumentNullException(nameof(list));
            return new ReadOnlyIntVector(list);
        }
        #endregion

        #region Construction

        /// <summary>
        ///		Initializes a new instance of the <c>IntVector</c> class
        ///		that is empty and has the default initial capacity.
        /// </summary>
        public IntVector()
        {
            _mArray = new int[DefaultCapacity];
        }
		
        /// <summary>
        ///		Initializes a new instance of the <c>IntVector</c> class
        ///		that has the specified initial capacity.
        /// </summary>
        /// <param name="capacity">
        ///		The number of elements that the new <c>IntVector</c> is initially capable of storing.
        ///	</param>
        public IntVector(int capacity)
        {
            _mArray = new int[capacity];
        }

        /// <summary>
        ///		Initializes a new instance of the <c>IntVector</c> class
        ///		that contains elements copied from the specified <c>IntVector</c>.
        /// </summary>
        /// <param name="c">The <c>IntVector</c> whose elements are copied to the new collection.</param>
        public IntVector(IntVector c)
        {
            _mArray = new int[c.Count];
            AddRange(c);
        }

        /// <summary>
        ///		Initializes a new instance of the <c>IntVector</c> class
        ///		that contains elements copied from the specified <see cref="int"/> array.
        /// </summary>
        /// <param name="a">The <see cref="int"/> array whose elements are copied to the new list.</param>
        public IntVector(int[] a)
        {
            _mArray = new int[a.Length];
            AddRange(a);
        }

        /// <summary>
        ///		The mehtod for the IDenseAccessVector interface.
        /// </summary>
        public int[] Vector
        {
            get => _mArray;
            set => _mArray = value;
        }

        #endregion

        #region Operations (type-safe ICollection)
        /// <summary>
        ///		Gets the number of elements actually contained in the <c>IntVector</c>.
        /// </summary>
        public virtual int Count => _mCount;

        /// <summary>
        ///		Copies the entire <c>IntVector</c> to a one-dimensional
        ///		string array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="int"/> array to copy to.</param>
        public virtual void CopyTo(int[] array)
        {
            CopyTo(array, 0);
        }

        /// <summary>
        ///		Copies the entire <c>IntVector</c> to a one-dimensional
        ///		<see cref="int"/> array, starting at the specified index of the target array.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="int"/> array to copy to.</param>
        /// <param name="start">The zero-based index in <paramref name="array"/> at which copying begins.</param>
        public virtual void CopyTo(int[] array, int start)
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
        ///		Gets or sets the <see cref="int"/> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index of the element to get or set.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntVector.Count"/>.</para>
        /// </exception>
        public virtual int this[int index]
        {
            get
            {
                ValidateIndex(index); // throws
                return _mArray[index]; 
            }
            set
            {
                ValidateIndex(index); // throws
                ++_mVersion; 
                _mArray[index] = value; 
            }
        }

        /// <summary>
        ///		Adds a <see cref="int"/> to the end of the <c>IntVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="int"/> to be added to the end of the <c>IntVector</c>.</param>
        /// <returns>The index at which the value has been added.</returns>
        public virtual int Add(int item)
        {
            if (_mCount == _mArray.Length)
                EnsureCapacity(_mCount + 1);
            _mArray[_mCount] = item;
            _mVersion++;
            return _mCount++;
        }
		
        /// <summary>
        ///		Removes all elements from the <c>IntVector</c>.
        /// </summary>
        public virtual void Clear()
        {
            ++_mVersion;
            _mArray = new int[DefaultCapacity];
            _mCount = 0;
        }
		
        /// <summary>
        ///		Creates a shallow copy of the <see cref="IntVector"/>.
        /// </summary>
        public virtual object Clone()
        {
            IntVector newColl = new IntVector(_mCount);
            Array.Copy(_mArray, 0, newColl._mArray, 0, _mCount);
            newColl._mCount = _mCount;
            newColl._mVersion = _mVersion;
            return newColl;
        }

        /// <summary>
        ///		Determines whether a given <see cref="int"/> is in the <c>IntVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="int"/> to check for.</param>
        /// <returns><c>true</c> if <paramref name="item"/> is found in the <c>IntVector</c>; otherwise, <c>false</c>.</returns>
        public virtual bool Contains(int item)
        {
            for (int i=0; i != _mCount; ++i)
                if (_mArray[i].Equals(item))
                    return true;
            return false;
        }

        /// <summary>
        ///		Returns the zero-based index of the first occurrence of a <see cref="int"/>
        ///		in the <c>IntVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="int"/> to locate in the <c>IntVector</c>.</param>
        /// <returns>
        ///		The zero-based index of the first occurrence of <paramref name="item"/> 
        ///		in the entire <c>IntVector</c>, if found; otherwise, -1.
        ///	</returns>
        public virtual int IndexOf(int item)
        {
            for (int i=0; i != _mCount; ++i)
                if (_mArray[i].Equals(item))
                    return i;
            return -1;
        }

        /// <summary>
        ///		Inserts an element into the <c>IntVector</c> at the specified index.
        /// </summary>
        /// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param>
        /// <param name="item">The <see cref="int"/> to insert.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntVector.Count"/>.</para>
        /// </exception>
        public virtual void Insert(int index, int item)
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
            _mVersion++;
        }

        /// <summary>
        ///		Removes the first occurrence of a specific <see cref="int"/> from the <c>IntVector</c>.
        /// </summary>
        /// <param name="item">The <see cref="int"/> to remove from the <c>IntVector</c>.</param>
        /// <exception cref="ArgumentException">
        ///		The specified <see cref="int"/> was not found in the <c>IntVector</c>.
        /// </exception>
        public virtual void Remove(int item)
        {		   
            int i = IndexOf(item);
            if (i < 0)
                throw new ArgumentException("Cannot remove the specified item because it was not found in the specified Collection.");			
            ++_mVersion;
            RemoveAt(i);
        }

        /// <summary>
        ///		Removes the element at the specified index of the <c>IntVector</c>.
        /// </summary>
        /// <param name="index">The zero-based index of the element to remove.</param>
        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntVector.Count"/>.</para>
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
            int[] temp = new int[1];
            Array.Copy(temp, 0, _mArray, _mCount, 1);
            _mVersion++;
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
        ///		Returns an enumerator that can iterate through the <c>IntVector</c>.
        /// </summary>
        /// <returns>An <see cref="Enumerator"/> for the entire <c>IntVector</c>.</returns>
        public virtual IIntVectorEnumerator GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

        #region Public helpers (just to mimic some nice features of ArrayList)
		
        /// <summary>
        ///		Gets or sets the number of elements the <c>IntVector</c> can contain.
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
                        int[] temp = new int[value];
                        Array.Copy(_mArray, temp, _mCount);
                        _mArray = temp;
                    }
                    else
                    {
                        _mArray = new int[DefaultCapacity];
                    }
                }
            }
        }

        /// <summary>
        ///		Adds the elements of another <c>IntVector</c> to the current <c>IntVector</c>.
        /// </summary>
        /// <param name="x">The <c>IntVector</c> whose elements should be added to the end of the current <c>IntVector</c>.</param>
        /// <returns>The new <see cref="IntVector.Count"/> of the <c>IntVector</c>.</returns>
        public virtual int AddRange(IntVector x)
        {
            if (_mCount + x.Count >= _mArray.Length)
                EnsureCapacity(_mCount + x.Count);
			
            Array.Copy(x._mArray, 0, _mArray, _mCount, x.Count);
            _mCount += x.Count;
            _mVersion++;

            return _mCount;
        }

        /// <summary>
        ///		Adds the elements of a <see cref="int"/> array to the current <c>IntVector</c>.
        /// </summary>
        /// <param name="x">The <see cref="int"/> array whose elements should be added to the end of the <c>IntVector</c>.</param>
        /// <returns>The new <see cref="IntVector.Count"/> of the <c>IntVector</c>.</returns>
        public virtual int AddRange(int[] x)
        {
            if (_mCount + x.Length >= _mArray.Length)
                EnsureCapacity(_mCount + x.Length);

            Array.Copy(x, 0, _mArray, _mCount, x.Length);
            _mCount += x.Length;
            _mVersion++;

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
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntVector.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int index)
        {
            ValidateIndex(index, false);
        }

        /// <exception cref="ArgumentOutOfRangeException">
        ///		<para><paramref name="index"/> is less than zero</para>
        ///		<para>-or-</para>
        ///		<para><paramref name="index"/> is equal to or greater than <see cref="IntVector.Count"/>.</para>
        /// </exception>
        private void ValidateIndex(int index, bool allowEqualEnd)
        {
            int max = (allowEqualEnd)?(_mCount):(_mCount-1);
            if (index < 0 || index > max)
                throw new ArgumentOutOfRangeException(
                    $"Index was out of range.  Must be non-negative and less than the size of the collection.", index, "Specified argument was out of the range of valid values.");
        }

        private void EnsureCapacity(int min)
        {
            int newCapacity = ((_mArray.Length == 0) ? DefaultCapacity : _mArray.Length * 2);
            if (newCapacity < min)
                newCapacity = min;

            Capacity = newCapacity;
        }

        #endregion
		
        #region Implementation (ICollection)

        void ICollection.CopyTo(Array array, int start)
        {
            CopyTo((int[])array, start);
        }

        #endregion

        #region Implementation (IList)

        object IList.this[int i]
        {
            get => this[i];
            set => this[i] = (int)value;
        }

        int IList.Add(object x)
        {
            return Add((int)x);
        }

        bool IList.Contains(object x)
        {
            return Contains((int)x);
        }

        int IList.IndexOf(object x)
        {
            return IndexOf((int)x);
        }

        void IList.Insert(int pos, object x)
        {
            Insert(pos, (int)x);
        }

        void IList.Remove(object x)
        {
            Remove((int)x);
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
        ///		Supports simple iteration over a <see cref="IntVector"/>.
        /// </summary>
        private class Enumerator : IEnumerator, IIntVectorEnumerator
        {
            #region Implementation (data)
			
            private readonly IntVector _mCollection;
            private int _mIndex;
            private readonly int m_version;
			
            #endregion
		
            #region Construction
			
            /// <summary>
            ///		Initializes a new instance of the <c>Enumerator</c> class.
            /// </summary>
            /// <param name="tc"></param>
            internal Enumerator(IntVector tc)
            {
                _mCollection = tc;
                _mIndex = -1;
                m_version = tc._mVersion;
            }
			
            #endregion
	
            #region Operations (type-safe IEnumerator)
			
            /// <summary>
            ///		Gets the current element in the collection.
            /// </summary>
            public int Current => _mCollection[_mIndex];

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
                if (m_version != _mCollection._mVersion)
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
        private class SyncIntVector : IntVector
        {
            #region Implementation (data)
            private readonly IntVector _mCollection;
            private readonly object _mRoot;
            #endregion

            #region Construction
            internal SyncIntVector(IntVector list)
            {
                _mRoot = list.SyncRoot;
                _mCollection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(int[] array)
            {
                lock(_mRoot)
                    _mCollection.CopyTo(array);
            }

            public override void CopyTo(int[] array, int start)
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
            public override int this[int i]
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

            public override int Add(int x)
            {
                lock(_mRoot)
                    return _mCollection.Add(x);
            }
            
            public override void Clear()
            {
                lock(_mRoot)
                    _mCollection.Clear();
            }

            public override bool Contains(int x)
            {
                lock(_mRoot)
                    return _mCollection.Contains(x);
            }

            public override int IndexOf(int x)
            {
                lock(_mRoot)
                    return _mCollection.IndexOf(x);
            }

            public override void Insert(int pos, int x)
            {
                lock(_mRoot)
                    _mCollection.Insert(pos,x);
            }

            public override void Remove(int x)
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
            public override IIntVectorEnumerator GetEnumerator()
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

            public override int AddRange(IntVector x)
            {
                lock(_mRoot)
                    return _mCollection.AddRange(x);
            }

            public override int AddRange(int[] x)
            {
                lock(_mRoot)
                    return _mCollection.AddRange(x);
            }
            #endregion
        }
        #endregion

        #region Nested Read Only Wrapper class
        private class ReadOnlyIntVector : IntVector
        {
            #region Implementation (data)
            private readonly IntVector _mCollection;
            #endregion

            #region Construction
            internal ReadOnlyIntVector(IntVector list)
            {
                _mCollection = list;
            }
            #endregion
            
            #region Type-safe ICollection
            public override void CopyTo(int[] array)
            {
                _mCollection.CopyTo(array);
            }

            public override void CopyTo(int[] array, int start)
            {
                _mCollection.CopyTo(array,start);
            }
            public override int Count => _mCollection.Count;

            public override bool IsSynchronized => _mCollection.IsSynchronized;

            public override object SyncRoot => _mCollection.SyncRoot;

            #endregion
            
            #region Type-safe IList
            public override int this[int i]
            {
                get => _mCollection[i];
                set => throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int Add(int x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }
            
            public override void Clear()
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override bool Contains(int x)
            {
                return _mCollection.Contains(x);
            }

            public override int IndexOf(int x)
            {
                return _mCollection.IndexOf(x);
            }

            public override void Insert(int pos, int x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override void Remove(int x)
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
            public override IIntVectorEnumerator GetEnumerator()
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

            public override int AddRange(IntVector x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            public override int AddRange(int[] x)
            {
                throw new NotSupportedException("This is a Read Only Collection and can not be modified");
            }

            #endregion
        }
        #endregion
    }
}