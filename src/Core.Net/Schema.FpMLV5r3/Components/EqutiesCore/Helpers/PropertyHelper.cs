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

#region Usings

using System;
using System.Collections.Generic;
using System.Reflection;

#endregion

namespace Highlander.Utilities.Helpers
{
    /// <summary>
    /// Helper class for working with class properties
    /// </summary>
    public static class PropertyHelper
    {

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static T1[] GetProperties<T1, T2>(List<T2> items, string propertyName)
        {
            var values = new T1[items.Count];
            int index = 0;
            foreach (var item in items)
            {
                values[index] = GetProperty<T1, T2>(item, propertyName);
                index++;//TODO Added later - looks like a bug but not sure!
            }
            return values;
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="item">The item.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        public static T1 GetProperty<T1, T2>(T2 item, string propertyName)
        {
            var value = default(T1);
            Type itemType = item.GetType();
            PropertyInfo p = itemType.GetProperty(propertyName);
            if (p != null)
            {
                value = (T1)p.GetValue(item, null);
            }
            return value;
        }
    }
}
