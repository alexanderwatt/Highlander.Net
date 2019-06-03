using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;

namespace Orion.ModelFramework.PricingStructures
{
    /// <summary>
    /// The Pricing Structure Interface
    /// </summary>
    public interface ICurve : IPricingStructure
    {
        ///// <summary>
        ///// Updates a basic quotation value and then perturbs and rebuilds the curve. 
        ///// Uses the measuretype to determine which one.
        ///// </summary>
        ///// <param name="values">The perturbation value array. This must be the same length as the number of assets in the QuotedAssetSet,
        ///// or it will not work.</param>
        ///// <param name="measureType">The measureType of the quotation required.</param>
        ///// <returns></returns>
        //Boolean PerturbCurve(Decimal[] values, String measureType);

        /// <summary>
        /// Gets the quoted asset set.
        /// </summary>
        /// <returns>The FpML QuotedAssetSet.</returns>
        QuotedAssetSet GetQuotedAssetSet();

        /// <summary>
        /// Gets the TermCurve.
        /// </summary>
        /// <returns>The FpML TermCurve.</returns>
        TermCurve GetTermCurve();

        IDictionary<string, Decimal> GetInputs();
    }
}