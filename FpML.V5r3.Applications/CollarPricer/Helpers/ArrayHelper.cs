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

using System.Collections.Generic;

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Helper class for arrays
    /// </summary>
    public static class ArrayHelper
    {
        /// <summary>
        /// Converts a List to a typed array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list">The list.</param>
        /// <returns></returns>
        public static T[] ListToArray<T>(List<T> list)
        {
            T[] array = { };
            if (list.Count > 0)
            {
                array = new T[list.Count];
                list.CopyTo(array);
            }
            return array;
        }

        /// <summary>
        /// Converts an array to a typed List.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="array">The array.</param>
        /// <returns></returns>
        public static List<T> ArrayToList<T>(T[] array)
        {
            List<T> list=null;
            if (array != null && array.Length > 0)
            {
                list = new List<T>(array);
            }
            return list;
        }
    }
}
