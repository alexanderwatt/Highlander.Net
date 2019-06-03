using System;
using System.Collections.Generic;
using Orion.Equity.VolatilityCalculator.Exception;

namespace Orion.Equity.VolatilityCalculator.Helpers
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
