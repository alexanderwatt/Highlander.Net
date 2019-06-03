using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Helper class for working with class properties
    /// </summary>
    static public class PropertyHelper
    {

        /// <summary>
        /// Gets the properties.
        /// </summary>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="items">The items.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        static public T1[] GetProperties<T1, T2>(List<T2> items, string propertyName)
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
        static public T1 GetProperty<T1, T2>(T2 item, string propertyName)
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
