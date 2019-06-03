#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace FpML.V5r10.Reporting.ModelFramework
{
    /// <summary>
    /// Curve id and stress name tuple.
    /// </summary>
    public class CurveStressPair
    {
        /// <summary>
        /// 
        /// </summary>
        public readonly string Curve;

        /// <summary>
        /// 
        /// </summary>
        public readonly string Stress;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="curve"></param>
        /// <param name="stress"></param>
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