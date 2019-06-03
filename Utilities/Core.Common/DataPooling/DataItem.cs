/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)
 Copyright (C) 2019 Simon Dudley

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

namespace Core.Common.DataPooling
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DataItem<T> : IEquatable<DataItem<T>>
    {
        // static behaviour
        // DefaultIsEmpty causes the "empty" value to be returned when value == default(T)
        // e.g. Guid.Empty becomes the "empty" singleton value, not an actual value.
        private static bool _defaultIsEmpty = DefaultIsEmptyInitialiser();
        
        /// <summary>
        /// 
        /// </summary>
        public static bool DefaultIsEmpty => _defaultIsEmpty;

        private static bool DefaultIsEmptyInitialiser()
        {
            //  typeof(T)           DefaultIsEmpty  Examples
            //  Guid                true
            //  IsValueType         false           int, double, Decimal, DateTime, etc.
            //  otherwise           true            strings and other classes
            if (typeof(T) == typeof(Guid))
                return true;
            if (typeof(T).IsValueType)
                return false;
            return true;
        }

        /// <summary>
        /// Returns a new list of data items containing the values.
        /// </summary>
        /// <param name="values">The input values.</param>
        /// <returns></returns>
        public static List<DataItem<T>> NewList(IEnumerable<T> values)
        {
            return values?.Select(value => new DataItem<T>(value, _defaultIsEmpty, false)).ToList();
        }

        /// <summary>
        /// Returns a new array of data items containing the values.
        /// </summary>
        /// <param name="values">The values.</param>
        /// <returns></returns>
        public static DataItem<T>[] NewArray(IEnumerable<T> values)
        {
            if (values == null)
                return null;
            return NewList(values).ToArray();
        }

        /// <summary>
        /// Clears the pool.
        /// </summary>
        public static void ClearPool()
        {
            PoolValue<T>.ClearPool();
        }

        /// <summary>
        /// Returns the time the pool was last cleared.
        /// </summary>
        /// <value>The time.</value>
        public static DateTimeOffset PoolLastCleared { get { return PoolValue<T>.PoolLastCleared; } }

        // instance behaviour
        private PoolValue<T> _value;

        /// <summary>
        /// 
        /// </summary>
        public PoolValue<T> PoolValue => _value;

        /// <summary>
        /// Gets the value for this instance.
        /// </summary>
        /// <value>The value.</value>
        public T Value => _value.Value;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataItem&lt;T&gt;"/> class with the "empty" value.
        /// </summary>
        public DataItem()
        {
            _value = PoolValue<T>.Empty;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataItem&lt;T&gt;"/> class with a value.
        /// </summary>
        /// <param name="value">The value.</param>
        public DataItem(T value)
        {
            _value = PoolValue<T>.NewValue(value, _defaultIsEmpty, false);
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="DataItem&lt;T&gt;"/> class,
        /// specifying whether the default for (T) results in an "empty" value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultIsEmpty">if set to <c>true</c> [default is empty].</param>
        /// <param name="doNotPool">if set to <c>true</c> [do not pool].</param>
        public DataItem(T value, bool defaultIsEmpty, bool doNotPool)
        {
            _value = PoolValue<T>.NewValue(value, defaultIsEmpty, doNotPool);
        }

        /// <summary>
        /// Ensures the internal value is pooled. Usually called after clearing the pool.
        /// </summary>
        public void Repool()
        {
            _value = PoolValue<T>.NewValue(_value.Value);
        }

        /// <summary>
        /// Gets a value indicating whether this instance is set to Empty (meaning it has no value).
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => _value.IsEmpty;

        /// <summary>
        /// Gets a value indicating whether this instance contains a value.
        /// </summary>
        /// <value><c>true</c> if this instance is not empty; otherwise, <c>false</c>.</value>
        public bool HasValue => _value.HasValue;

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj is DataItem<T>)
                return Equals((DataItem<T>)obj);
            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(DataItem<T> other)
        {
            return _value.Equals(other?.PoolValue);
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            return _value.ToString();
        }
    }

    /// <summary>
    /// Describes a type that may or may not contain a value.
    /// </summary>
    /// <typeparam name="T">The type of the contained value.</typeparam>
    public sealed class PoolValue<T> : IEquatable<PoolValue<T>>
    {
        // static state
        private static readonly PoolValue<T> _Empty = new PoolValue<T>();
        private static readonly object _PoolLock = new object();
        private static readonly Dictionary<PoolValue<T>, PoolValue<T>> PooledItems = new Dictionary<PoolValue<T>, PoolValue<T>>();
        private static DateTimeOffset _poolLastCleared = DateTimeOffset.MinValue;

        /// <summary>
        /// 
        /// </summary>
        public static DateTimeOffset PoolLastCleared { get { lock (_PoolLock) { return _poolLastCleared; } } }
        
        /// <summary>
        /// 
        /// </summary>
        public static void ClearPool()
        {
            lock (_PoolLock)
            {
                PooledItems.Clear();
                _poolLastCleared = DateTimeOffset.Now;
            }
        }

        // instance state
        private readonly T _value;

        private PoolValue() { }
        private PoolValue(T value)
        {
            _value = value;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is set to Empty (meaning it has no value).
        /// </summary>
        /// <value><c>true</c> if this instance is empty; otherwise, <c>false</c>.</value>
        public bool IsEmpty => (this == _Empty);

        /// <summary>
        /// Gets a value indicating whether this instance contains a value.
        /// </summary>
        /// <value><c>true</c> if this instance is not empty; otherwise, <c>false</c>.</value>
        public bool HasValue => (this != _Empty);

        /// <summary>
        /// Gets the value for this instance.
        /// </summary>
        /// <value>The value.</value>
        public T Value
        {
            get
            {
                if (HasValue)
                    return _value;
                throw new InvalidOperationException();
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <param name="obj">The <see cref="T:System.Object"/> to compare with the current <see cref="T:System.Object"/>.</param>
        /// <returns>
        /// true if the specified <see cref="T:System.Object"/> is equal to the current <see cref="T:System.Object"/>; otherwise, false.
        /// </returns>
        /// <exception cref="T:System.NullReferenceException">
        /// The <paramref name="obj"/> parameter is null.
        /// </exception>
        public override bool Equals(object obj)
        {
            if (obj is PoolValue<T>)
                return Equals((PoolValue<T>)obj);

            return false;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(PoolValue<T> other)
        {
            if (this == _Empty)
                return other.IsEmpty;
            return EqualityComparer<T>.Default.Equals(_value, other._value);
        }

        /// <summary>
        /// Returns a <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.String"/> that represents the current <see cref="T:System.Object"/>.
        /// </returns>
        public override string ToString()
        {
            if (this == _Empty)
                return null;
            return _value.ToString();
        }

        /// <summary>
        /// Serves as a hash function for a particular type.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="T:System.Object"/>.
        /// </returns>
        public override int GetHashCode()
        {
            if (this == _Empty)
                return 0;
            return EqualityComparer<T>.Default.GetHashCode(_value);
        }

        /// <summary>
        /// Returns a "Empty" instance for this type.
        /// </summary>
        /// <value>The "Empty" instance.</value>
        public static PoolValue<T> Empty => _Empty;

        /// <summary>
        /// Returns a new PoolValue type containing a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="defaultIsEmpty"> </param>
        /// <param name="doNotPool"> </param>
        /// <returns></returns>
        public static PoolValue<T> NewValue(T value, bool defaultIsEmpty, bool doNotPool)
        {
            if (defaultIsEmpty && value.Equals(default(T)))
                return Empty;
            var newItem = new PoolValue<T>(value);
            if (doNotPool)
                return newItem;
            lock (_PoolLock)
            {
                if (PooledItems.TryGetValue(newItem, out var oldItem))
                    return oldItem;
                PooledItems[newItem] = newItem;
            }
            return newItem;
        }

        /// <summary>
        /// Returns a new PoolValue type containing a value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static PoolValue<T> NewValue(T value)
        {
            return NewValue(value, false, false);
        }
    }

    /// <summary>
    /// Contains extension methods for the DataItem type.
    /// </summary>
    public static class DataItemExtensions
    {
        /// <summary>
        /// If the input has a value, applies the selector function on the value and returns the result.
        /// Otherwise returns Empty.
        /// </summary>
        /// <typeparam name="T">The input type.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input value.</param>
        /// <param name="selector">The selector function.</param>
        /// <returns></returns>
        public static DataItem<TResult> Select<T, TResult>(this DataItem<T> input, Func<T, DataItem<TResult>> selector)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input), "input is null.");
            if (selector == null)
                throw new ArgumentNullException(nameof(selector), "selector is null.");
            return input.IsEmpty ? new DataItem<TResult>() : selector(input.Value);
        }
    }
}
