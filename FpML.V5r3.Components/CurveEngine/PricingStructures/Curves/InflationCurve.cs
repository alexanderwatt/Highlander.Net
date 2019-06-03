#region Using directives

using System;
using Core.Common;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using Orion.Util.NamedValues;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.CurveEngine.PricingStructures.Curves
{
    ///<summary>
    ///</summary>
    public class InflationCurve : RateCurve, IInflationCurve
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RateCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="properties">The properties.</param>
        /// <param name="instrumetData">The instrument data.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public InflationCurve(ILogger logger, ICoreCache cache, string nameSpace, NamedValueSet properties, 
            QuotedAssetSet instrumetData, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, properties, instrumetData, fixingCalendar, rollCalendar)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="InflationCurve"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="cache">The cache.</param>
        /// <param name="nameSpace">The client namespace</param>
        /// <param name="fpmlData">The FPML data.</param>
        /// <param name="properties">The properties.</param>
        /// <param name="fixingCalendar">The fixingCalendar.</param>
        /// <param name="rollCalendar">The rollCalendar.</param>
        public InflationCurve(ILogger logger, ICoreCache cache, string nameSpace, 
            Pair<PricingStructure, PricingStructureValuation> fpmlData, 
            NamedValueSet properties, IBusinessCalendar fixingCalendar, IBusinessCalendar rollCalendar)
            : base(logger, cache, nameSpace, fpmlData, properties, fixingCalendar, rollCalendar)
        {}

        #region Implementation of IInflationCurve

        ///<summary>
        /// Returns the discoutn factor.
        ///</summary>
        ///<param name="valuationDate"></param>
        ///<param name="targetDate"></param>
        ///<returns></returns>
        public double GetIndexValue(DateTime valuationDate, DateTime targetDate)
        {
            return GetDiscountFactor(valuationDate, targetDate);
        }

        /// <summary>
        /// Gets the discount factor.
        /// </summary>
        /// <param name="targetDate">The target date.</param>
        /// <returns></returns>
        public double GetIndexValue(DateTime targetDate)
        {
            return GetDiscountFactor(PricingStructureValuation.baseDate.Value, targetDate);
        }

        #endregion
    }
}