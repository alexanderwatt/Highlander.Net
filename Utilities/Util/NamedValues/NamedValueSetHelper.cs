/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;
using System.Linq;

namespace Orion.Util.NamedValues
{
    /// <summary>
    /// A (not very) useful helper class.
    /// </summary>
    public static class NamedValueSetHelper
    {
        ///// <summary>
        ///// Converts object[,] to NamedValueSet
        ///// </summary>
        ///// <param name="properties">The list of properties.
        ///// These must be of the form: Name/Value</param>
        ///// <returns></returns>
        //public static NamedValueSet RangeToNamedValueSet(object[,] properties)
        //{
        //    var rows = properties.GetLength(0);
        //    var resultSet = new NamedValueSet();
        //    for(var i = 0; i < rows; i++)
        //    {
        //        if (properties[i, 0] != null)
        //            resultSet.Set((string) properties[i, 0], properties[i, 1]);
        //    }
        //    return resultSet;
        //}

        /// <summary>
        /// Creates a quoted asset set (just joking!)
        /// </summary>
        /// <param name="names">The names of properties.</param>
        /// <param name="values">The values of the properties.
        /// These must be of the form: Name/Value</param>
        /// <returns></returns>
        public static NamedValueSet Build(string[] names, object[] values)
        {
            var length = names.Length;
            var resultSet = new NamedValueSet();
            if (values.Length == length)
            {
                for (var i = 0; i < length; i++)
                {
                    resultSet.Set(names[i], values[i]);
                }
            }
            return resultSet;
        }

        /// <summary>
        /// Extracts and then builds an array from an Excel input range.
        /// </summary>
        /// <param name="namevalues">The values of the properties.
        /// These must be of the form: Name/Value</param>
        /// <returns></returns>
        public static NamedValueSet DistinctInstances(object[,] namevalues)
        {
            var namedValueSet = new NamedValueSet();
            var rows = namevalues.GetLength(0);
            var properties = new string[rows];
            for(var i = 0; i < rows; i++)
            {
                properties[i] = (string)namevalues[i, 0];
            }
            var uniqueproperties = (IEnumerable<string>)properties;
            foreach (var uniqueproperty in uniqueproperties.Distinct())
            {
                var index = 0;
                var values = new List<object>();
                foreach (var property in properties)
                {
                    index++;
                    if (property != uniqueproperty) continue;
                    values.Add(namevalues[index - 1, 1]);
                }
                namedValueSet.Set(uniqueproperty, values.Count == 1 ? values[0] : values.ToArray());
            }
            return namedValueSet;
        }
    }
}