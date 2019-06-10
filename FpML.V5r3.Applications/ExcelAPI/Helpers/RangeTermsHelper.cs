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

using System.Collections.Generic;
using Orion.Util.Helpers;

namespace HLV5r3.Helpers
{
    ///<summary>
    ///</summary>
    public static class RangeTermsHelper
    {
        /// <summary>
        /// Creates the array for item.
        /// </summary>
        /// <typeparam name="TIn">The type of the in.</typeparam>
        /// <typeparam name="TOut">The type of the out.</typeparam>
        /// <param name="itemName">Name of the item.</param>
        /// <param name="terms">The terms.</param>
        /// <returns></returns>
        public static TOut[] CreateArrayForItem<TIn, TOut>(string itemName, TIn[] terms)
        {
            var result = new List<TOut>();
            foreach (TIn streamTerm in terms)
            {
                if (ObjectLookupHelper.ObjectPropertyExists(streamTerm, itemName))
                {
                    result.Add((TOut)ObjectLookupHelper.GetPropertyValue(streamTerm, itemName));
                }
            }
            return result.ToArray();
        }
    }
}