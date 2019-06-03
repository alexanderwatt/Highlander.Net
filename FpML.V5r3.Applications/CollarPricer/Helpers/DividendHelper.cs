/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

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
using System.Reflection;

#endregion

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Helper class for working with Dividends
    /// </summary>
    public static class DividendHelper
    {
        /// <summary>
        /// Finds the dividend.
        /// </summary>
        /// <param name="dividend">The dividend.</param>
        /// <param name="dividends">The dividends.</param>
        /// <returns></returns>
        public static Dividend FindDividend(Dividend dividend, DividendList dividends)
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
        public static T[] GetDividendsProperty<T>(List<Dividend> dividends, string propertyName)
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
        public static T GetDividendProperty<T>(Dividend dividend, string propertyName)
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
