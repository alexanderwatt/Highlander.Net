#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;
using Orion.Constants;
using Orion.ModelFramework.Instruments;

#endregion

namespace Orion.ModelFramework
{
    /// <summary>
    /// Curve id and stress name tuple.
    /// </summary>
    public class CurveStressPair
    {
        public readonly string Curve;
        public readonly string Stress;
        public CurveStressPair(string curve, string stress)
        {
            Curve = curve;
            Stress = stress;
        }
    }

    ///<summary>
    ///</summary>
    public interface IPortfolioPricer
    {
        ///<summary>
        /// Returns the relevant pricing structure ids.
        ///</summary>
        ///<returns></returns>
        IEnumerable<String> GetRequiredPricingStructures();
    }

}