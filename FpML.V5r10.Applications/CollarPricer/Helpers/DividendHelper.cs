using System;
using System.Collections.Generic;
using System.Reflection;

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Helper class for working with Dividends
    /// </summary>
    static public class DividendHelper
    {
        /// <summary>
        /// Finds the dividend.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="dividends">The dividends.</param>
        /// <returns></returns>
        static public Dividend FindDividend(Dividend dividend, DividendList dividends)
        {
            return dividends.Find(dividendItem => (DateTime.Compare(dividendItem.ExDivDate, dividend.ExDivDate) == 0)
                );
        }


        /// <summary>
        /// Gets the dividends property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dividends">The dividends.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        static public T[] GetDividendsProperty<T>(List<Dividend> dividends, string propertyName)
        {
            var values = new T[dividends.Count];
            int index = 0;
            foreach (Dividend dividend in dividends)
            {
                values[index] = GetDividendProperty<T>(dividend, propertyName);
                index++;
            }
            return values;
        }

        /// <summary>
        /// Gets the calculation period property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dividend">The dividend.</param>
        /// <param name="propertyName">Name of the property.</param>
        /// <returns></returns>
        static public T GetDividendProperty<T>(Dividend dividend, string propertyName)
        {
            T value = default(T);
            Type dividendType = dividend.GetType();

            PropertyInfo p = dividendType.GetProperty(propertyName);

            if (p != null)
            {
                value = (T)p.GetValue(dividend, null);
            }
            return value;
        }
    }
}
