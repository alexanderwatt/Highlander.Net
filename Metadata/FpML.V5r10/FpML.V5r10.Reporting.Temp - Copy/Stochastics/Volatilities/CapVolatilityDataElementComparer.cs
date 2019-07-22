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

#region Using Directives

using System.Collections.Generic;

#endregion

namespace Orion.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// Class that implements the IComparer interface for the class
    /// CapVolatilityDataElement.
    /// </summary>
    public class CapVolatilityDataElementComparer:
        IComparer<CapVolatilityDataElement<int>>
    {
        #region IComparer<CapVolatilityDataElement> Implementation

        /// <summary>
        /// Implementation of the IComparer interface.
        /// Compares the current object with another object of the same type.
        /// Precedence level is:
        /// Level 1 (top): Cap/Floor volatility data > ETO volatility data;
        /// Level 2: Ascending expiry time.
        /// </summary>
        /// <param name="obj1">First object in the comparison.</param>
        /// <param name="obj2">Second object in the comparison.</param>
        /// <returns>
        /// A 32-bit signed integer that indicates the relative order of the
        /// objects being compared. The return value has the following meanings:
        /// Value Meaning Less than zero This object is less than the
        /// <paramref name="obj1"/> parameter.Zero This object is equal to
        /// <paramref name="obj1"/>. Greater than zero This object is greater
        /// than <paramref name="obj1"/>.
        /// </returns>
        public int Compare
            (CapVolatilityDataElement<int> obj1, CapVolatilityDataElement<int> obj2)
        {
            // Check for null arguments.
            if (obj1 == null && obj2 == null)
            {
                return 0;
            }
            if (obj1 == null)
            {
                return -1;
            }
            if (obj2 == null)
            {
                return 1;
            }

            // Define and initialise the return variable.
            int flag;

            var type1 = obj1.VolatilityType;
            var type2 = obj2.VolatilityType;

            if (type1 == VolatilityDataType.ETO &&
                type2 == VolatilityDataType.CapFloor)
            {
                // LESS THAN ZERO CASE.
                flag = -1;
            }
            else if (type1 == VolatilityDataType.CapFloor &&
                     type2 == VolatilityDataType.ETO)
            {
                // GREATER THAN ZERO CASE.
                flag = 1;
            }
            else
            {
                // Same volatility data type: sort into ascending expiry.
                int expiry1 = obj1.Expiry;
                int expiry2 = obj2.Expiry;

                flag = expiry1.CompareTo(expiry2);
            }

            return flag;
        }

        #endregion

    }
}