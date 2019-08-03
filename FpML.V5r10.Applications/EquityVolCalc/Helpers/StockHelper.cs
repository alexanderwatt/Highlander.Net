using System;
/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

using System.Collections.Generic;
using FpML.V5r10.EquityVolatilityCalculator.Exception;

namespace FpML.V5r10.EquityVolatilityCalculator.Helpers
{
    /// <summary>
    /// Helper class for Stocks
    /// </summary>
    ///    

    public static class StockHelper
    {
        const int CExpiryProximityDays = 5;

        /// <summary>
        /// Checks the child expiries well defined. Obsolete.
        /// </summary>
        /// <param name="leadStock">The lead stock.</param>
        /// <param name="childStock">The child stock.</param>
        public static void CheckChildExpiriesWellDefined(IStock leadStock, IStock childStock)
        {

           foreach (ForwardExpiry childExpiry in childStock.VolatilitySurface.Expiries)
           {                       
               var parentExpiries = new List<ForwardExpiry>(leadStock.VolatilitySurface.Expiries);               
               List<ForwardExpiry> matchedExpiries = parentExpiries.FindAll(delegate(ForwardExpiry expiryItem)
                   {
                       // Time between parent and child
                       TimeSpan span = childExpiry.ExpiryDate - expiryItem.ExpiryDate;
                       //For each child we should find a mapping expiry i.e matchesPerExpiry = 1
                       return (Math.Abs(span.Days) <= CExpiryProximityDays);
                   }
               );
               if (matchedExpiries.Count != 1)                    
                   throw new IncompleteLeadSurfaceException("Not all child expiries map 1:1 to a parent");                                    
           }
        }                 
    }
}
