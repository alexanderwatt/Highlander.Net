using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.Analytics.Stochastics.Volatilities
{
    /// <summary>
    /// Base class for PointsPerDay (PPD) Grids.
    /// Child classes will need to implement their own internal structure based around the FpML MultiDimensionalPricingData class
    /// </summary>
    [Serializable()]
    public class PPDGrid : MultiDimensionalPricingData
    {
        // Default constructor
        ///<summary>
        ///</summary>
        public PPDGrid()
        {
            VolsPerExpiry = null;
        }

        ///<summary>
        ///</summary>
        public Dictionary<string, Dictionary<string, decimal>> VolsPerExpiry { get; private set; }

        /// <summary>
        /// This method returns the expiries (rows) for this PPD grid
        /// </summary>
        /// <returns></returns>
        public string[] GetExpiries()
        {
            return null;
        }
    }
}