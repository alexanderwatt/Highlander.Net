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
using System.Collections.Generic;

namespace Highlander.Reporting.Analytics.V5r3.Maths.Collections
{

    [Serializable]
    public class Vector<T> : ICloneable, IEnumerable<T>
    {
        //----------------------------------------------------------------------------------------------
        public Vector()
        {
          RemoveAll();
        }

        //----------------------------------------------------------------------------------------------
        public int Count => _list.Count;

          //----------------------------------------------------------------------------------------------
        public int Capacity
        {
          get => _list.Capacity;
            set => _list.Capacity = value;
        }

        //----------------------------------------------------------------------------------------------
        public T this[int idx]
        {
          get => _list[idx];
            set => _list[idx] = value;
        }

        //----------------------------------------------------------------------------------------------
        public void Add(T obj)
        {
          _list.Add(obj);
        }

        //----------------------------------------------------------------------------------------------
        public void Add(Vector<T> rhs)
        {
          _list.AddRange(rhs._list);
        }

        //----------------------------------------------------------------------------------------------
        public void Insert(int idx,
                           T obj)
        {
          _list.Insert(idx, obj);
        }

        //----------------------------------------------------------------------------------------------
        public void Remove(T obj)
        {
          _list.Remove(obj);
        }

        //----------------------------------------------------------------------------------------------
        public void Remove(int idx)
        {
          _list.RemoveAt(idx);
        }

        //----------------------------------------------------------------------------------------------
        public void RemoveAll()
        {
          _list = new List<T>(); 
        }

        //----------------------------------------------------------------------------------------------
        public void Sort(IComparable<T> compare)
        {
          _list.Sort(compare);
        }

        /// <summary>
        /// 
        /// </summary>
        public struct Position : IEquatable<Position>
        {
          public int Index;
          public int Insert;

            public override bool Equals(object obj)
            {
                throw new NotImplementedException();
            }

            public override int GetHashCode()
            {
                throw new NotImplementedException();
            }

            public static bool operator ==(Position left, Position right)
            {
                return left.Equals(right);
            }

            public static bool operator !=(Position left, Position right)
            {
                return !(left == right);
            }

            public bool Equals(Position other)
            {
                throw new NotImplementedException();
            }
        }
        //----------------------------------------------------------------------------------------------
        public Position Find(IComparable<T> func)
        {
          int top    = 0;
          int bottom = Count - 1;
          int sign = func.IsAscending() ? 1 : -1;

          Position position;
          position.Index = -1;

          while (top <= bottom)
          {
            int middle = (top + bottom ) >>1;
            int result = func.Compare(this[middle]);
            int comp = result * sign;

            if (0 == comp)
            {
              position.Index = middle;
              break;
            }
            if (comp > 0)
            {
                bottom = middle - 1;
            }
            else
            {
                top = middle + 1;
            }
          }
          position.Insert = position.Index != -1 ? position.Index : top;

          return position;
        }

        //----------------------------------------------------------------------------------------------
        public Position Find(IComparable<T> func,
                             T rhs)
        {
          int top = 0;
          int bottom = Count - 1;
          int sign = func.IsAscending() ? 1 : -1;

          Position position;
          position.Index = -1;
          while (top <= bottom)
          {
            int middle = (top + bottom ) >> 1;
            int comp = func.Compare(this[middle], rhs) * sign;
            if (0 == comp)
            {
              position.Index = middle;
              break;
            }
            if (comp > 0)
            {
                bottom = middle - 1;
            }
            else
            {
                top = middle + 1;
            }
          }
          position.Insert = position.Index != -1 ? position.Index : top;

          return position;
        }
    
        //----------------------------------------------------------------------------------------------
        public override bool Equals(object obj)
        {
            var vector = obj as Vector<T>;
            return vector != null && this == vector;
        }

        //----------------------------------------------------------------------------------------------
        public override int GetHashCode() => _list != null ? _list.GetHashCode() : base.GetHashCode();

        //----------------------------------------------------------------------------------------------
        public static bool operator==(Vector<T> lhs, Vector<T> rhs)
        {
          bool equal = ReferenceEquals(lhs, rhs);
          if (!equal
           && !ReferenceEquals(lhs, null)
           && !ReferenceEquals(rhs, null))
          {
            equal = lhs.Count == rhs.Count;
            for (int idx = 0; equal && idx < lhs.Count; idx++)
              equal = lhs[idx].Equals(rhs[idx]);
          }
          return equal;
        }

        //----------------------------------------------------------------------------------------------
        public static bool operator!=(Vector<T> lhs, Vector<T> rhs)
        {
          return !(lhs == rhs); 
        }

        //----------------------------------------------------------------------------------------------
        public IEnumerator<T> GetEnumerator()
        {
          return _list.GetEnumerator();
        }

        //----------------------------------------------------------------------------------------------
        IEnumerator IEnumerable.GetEnumerator()
        {
          return GetEnumerator();
        }

        //----------------------------------------------------------------------------------------------
        public object Clone()
        {
          Vector<T> clone = new Vector<T>();
          for (int idx = 0; idx < Count; idx++)
          {
            T item = this[idx];
            if (item != null)
            {
              var cloneable = item.GetType().GetInterface("ICloneable");
              if (cloneable != null)
                item = (T)cloneable.GetMethod("Clone").Invoke(item, null);
            }
            clone.Add(item);
          }
          return clone;
        }

        //----------------------------------------------------------------------------------------------
        public void Copy(Vector<T> rhs)
        {
            RemoveAll();
            foreach (T t in rhs)
                Add(t);
        }
     
        //----------------------------------------------------------------------------------------------
        private List<T> _list;
    }
}
