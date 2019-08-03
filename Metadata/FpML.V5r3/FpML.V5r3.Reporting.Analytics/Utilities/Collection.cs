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

namespace Orion.Analytics.Utilities
{
    /// <summary>
    /// The class <c>Collection</c> contains several utilities performing
    /// some basic collection operations (like union, intersection...).
    /// </summary>
    public static class Collection
    {
        /// <summary>
        /// The class <c>ConcatCollection</c> is used to perform the 
        /// mathematical concatenation between two collections.
        /// </summary>
        /// <seealso cref="Collection.Concat"/>
        public class ConcatCollection : ICollection
        {
            ///<summary>
            ///</summary>
            public class ConcatEnumerator : IEnumerator
            {
                private readonly IEnumerator _enumerator1;
                private readonly IEnumerator _enumerator2;

                private bool _isEnumerator1Current;

                ///<summary>
                ///</summary>
                ///<param name="union"></param>
                public ConcatEnumerator(ConcatCollection union)
                {
                    _enumerator1 = union._c1.GetEnumerator();
                    _enumerator2 = union._c2.GetEnumerator();
                    _isEnumerator1Current = true;
                }

                public void Reset()
                {
                    _enumerator1.Reset();
                    _enumerator2.Reset();
                    _isEnumerator1Current = true;
                }

                public object Current => _isEnumerator1Current ? _enumerator1.Current : _enumerator2.Current;

                public bool MoveNext()
                {
                    if(_isEnumerator1Current && _enumerator1.MoveNext()) return true;
                    _isEnumerator1Current = false;
                    return _enumerator2.MoveNext();
                }
            }

            private readonly ICollection _c1;
            private readonly ICollection _c2;

            ///<summary>
            ///</summary>
            ///<param name="c1"></param>
            ///<param name="c2"></param>
            public ConcatCollection(ICollection c1, ICollection c2)
            {
                _c1 = c1;
                _c2 = c2;
            }

		
            public bool IsSynchronized => _c1.IsSynchronized && _c2.IsSynchronized;

            public int Count => _c1.Count + _c2.Count;

            public void CopyTo(Array array, int index)
            {
                int indexArray = index;
                foreach(object obj in _c1) array.SetValue(obj, indexArray++);
                foreach(object obj in _c2) array.SetValue(obj, indexArray++);
            }

            public object SyncRoot => _c1.SyncRoot;

            public IEnumerator GetEnumerator()
            {
                return new ConcatEnumerator(this);
            }
        }


        /// <summary>
        /// The class <c>InterCollection</c> is used to perform the
        /// mathematical intersection between two collections.
        /// </summary>
        /// <seealso cref="Collection.Inter"/>
        private class InterCollection : ICollection
        {
            private readonly ArrayList _intersection;

            public InterCollection(ICollection c1, ICollection c2)
            {
                // swap in order to have <c>c1.Count <= c2.Count</c>
                if(c1.Count > c2.Count)
                {
                    ICollection c1Bis = c1;
                    c1 = c2;
                    c2 = c1;
                }

                var table = new Hashtable(c1.Count);
                foreach(object obj in c1) 
                    if(!table.Contains(obj)) table.Add(obj, null);

                // building the intersection
                _intersection = new ArrayList();
                foreach(object obj in c2)
                    if(table.Contains(obj)) 
                    {
                        _intersection.Add(obj);
                        table.Remove(obj);
                    }

                _intersection.TrimToSize();
            }

            #region ICollection Members

            public IEnumerator GetEnumerator()
            {
                return _intersection.GetEnumerator();
            }

            public bool IsSynchronized => _intersection.IsSynchronized;

            public int Count => _intersection.Count;

            public void CopyTo(Array array, int index)
            {
                _intersection.CopyTo(array, index);
            }

            public object SyncRoot => _intersection.SyncRoot;

            #endregion
        }

        /// <summary>
        /// The class <c>UnionCollection</c> is used to perform the
        /// mathematical union between two collections.
        /// </summary>
        private class UnionCollection : ICollection
        {
            private readonly ArrayList _union;

            public UnionCollection(ICollection c1, ICollection c2)
            {
                var table1 = new Hashtable(c1.Count);
                foreach(object obj in c1) 
                    if(!table1.Contains(obj)) table1.Add(obj, null);
                var table2 = new Hashtable(c2.Count);
                foreach(object obj in c2) 
                    if(!table2.Contains(obj)) table2.Add(obj, null);
                // building the union
                _union = new ArrayList(Math.Max(table1.Count, table2.Count));
                _union.AddRange(table1.Keys);
                foreach(object obj in c2)
                    if(!table1.Contains(obj)) _union.Add(obj);
                _union.TrimToSize();
            }

            #region ICollection Members

            public bool IsSynchronized => _union.IsSynchronized;

            public int Count => _union.Count;

            public void CopyTo(Array array, int index)
            {
                _union.CopyTo(array, index);	
            }
			
            public IEnumerator GetEnumerator()
            {
                return _union.GetEnumerator();
            }

            public object SyncRoot => _union.SyncRoot;

            #endregion
        }

        /// <summary>
        /// The collection <c>MinusCollection</c> is used to perform
        /// the mathematical subtraction of two collections.
        /// </summary>
        /// <seealso cref="Collection.Minus"/>
        private class MinusCollection : ICollection
        {
            private readonly ArrayList _minus;

            public MinusCollection(ICollection c1, ICollection c2)
            {
                var table1 = new Hashtable(c1.Count);
                foreach(object obj in c1)
                    if(!table1.Contains(obj)) table1.Add(obj, null);
                var table2 = new Hashtable(c2.Count);
                foreach(object obj in c2) 
                    if(!table2.Contains(obj)) table2.Add(obj, null);
                // building minus collection
                _minus = new ArrayList(Math.Max(c1.Count - c2.Count, 10)); 
                foreach(object obj in table1.Keys)
                    if(!table2.Contains(obj)) _minus.Add(obj);

                _minus.TrimToSize();
            }

            #region ICollection Members

            public bool IsSynchronized => _minus.IsSynchronized;

            public int Count => _minus.Count;

            public void CopyTo(Array array, int index)
            {
                _minus.CopyTo(array, index);	
            }

            public IEnumerator GetEnumerator()
            {
                return _minus.GetEnumerator();
            }

            public object SyncRoot => _minus.SyncRoot;

            #endregion
        }

        /// <summary>
        /// Returns a collection resulting from the concatenation from
        /// <c>c1</c> and <c>c2</c>.
        /// </summary>
        /// <param name="c1">Should not be null.</param>
        /// <param name="c2">Should not be null.</param>
        /// <remarks>The call is performed in <c>O(1)</c> computational time, the
        /// concatenated collection is not built explicitly.</remarks>
        public static ICollection Concat(ICollection c1, ICollection c2)
        {
            if(c1 == null) throw new ArgumentNullException(nameof(c1),
                                                           "#E00 Concatenated collections should not be null.");
            if(c2 == null) throw new ArgumentNullException(nameof(c2),
                                                           "#E01 Concatenated collections should not be null.");
            return new ConcatCollection(c1, c2);
        }

        /// <summary>
        /// Returns a collection resulting from the mathematical intersection
        /// of <c>c1</c> and <c>c2</c>.
        /// </summary>
        /// <param name="c1">Should not be null.</param>
        /// <param name="c2">Should not be null.</param>
        /// <remarks>
        /// <p>The call is performed in <c>O(c1.Count+c2.Count)</c> and
        /// the intersection is built explicitly.</p>
        /// <p>The resulting collection will not contain several identical elements.</p>
        /// <p>Example: Inter({1;1;2;3},{0;1;1;3;4}) = {1;3}.</p>
        /// </remarks>
        public static ICollection Inter(ICollection c1, ICollection c2)
        {
            if(c1 == null) throw new ArgumentNullException(nameof(c1),
                                                           "#E00 Intersected collections should not be null.");
            if(c2 == null) throw new ArgumentNullException(nameof(c2),
                                                           "#E01 Intersected collections should not be null.");
            return new InterCollection(c1, c2);
        }

        /// <summary>
        /// Returns a collection resulting from the subtraction of
        /// the items of <c>c2</c> to the collection <c>c1</c>. 
        /// </summary>
        /// <param name="c1">Should not be null.</param>
        /// <param name="c2">Should not be null.</param>
        /// <remarks>The call is performed in <c>O(c1.Count+c2.Count)</c></remarks>
        public static ICollection Minus(ICollection c1, ICollection c2)
        {
            if(c1 == null) throw new ArgumentNullException(nameof(c1),
                                                           "#E00 Base collection should not be null.");
            if(c2 == null) throw new ArgumentNullException(nameof(c2),
                                                           "#E01 Subtracted collection should not be null.");
            return new MinusCollection(c1, c2);
        }

        /// <summary>
        /// Returns the cartesian product of the two collections <c>c1</c>
        /// and <c>c2</c>.
        /// </summary>
        /// <param name="c1">Should not be null.</param>
        /// <param name="c2">Should not be null.</param>
        public static ICollection Product(ICollection c1, ICollection c2)
        {
            if(c1 == null) throw new ArgumentNullException(nameof(c1),
                                                           "#E00 Product collection should not be null.");
            if(c2 == null) throw new ArgumentNullException(nameof(c2),
                                                           "#E01 Product collection should not be null.");
            return null;
        }

        /// <summary>
        /// Returns a collection resulting from the union of the items
        /// of <c>c1</c> and <c>c2</c>.
        /// </summary>
        /// <param name="c1">Should not be null.</param>
        /// <param name="c2">Should not be null.</param>
        /// <remarks>
        /// <p>The call is performed in <c>O(c1.Count+c2.Count)</c>
        /// computational time.</p>
        /// <p>The resulting collection will not contain several identical elements.</p>
        /// <p>Example: Union({1;1;3},{0;1;2;3}) = {0;1;2;3}</p>
        /// </remarks>
        public static ICollection Union(ICollection c1, ICollection c2)
        {
            if(c1 == null) throw new ArgumentNullException(nameof(c1),
                                                           "#E00 Union collections should not be null.");
            if(c2 == null) throw new ArgumentNullException(nameof(c2),
                                                           "#E01 Union collections should not be null.");
            return new UnionCollection(c1, c2);
        }
    }
}