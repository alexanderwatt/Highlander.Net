#region Using directives

using System;
using System.Collections.Generic;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;

#endregion

namespace Orion.CurveEngine.Assets.Rates.CapsFloors
{
    /// <summary>
    /// Base class for Libor indexes.
    /// </summary>
    public class PriceableIRFloor : PriceableIRCap
    { 
        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRFloor"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="identifier">The asset identifier.</param>
        /// <param name="rollDates">The roll dates. plus the maturity date. This array is an element longer than 
        /// the notionals and strikes arrays.</param>
        /// <param name="notionals">The notionals for each caplet. There should be ne less than the number of roll dates.</param>
        /// <param name="strikes">The various strikes for each caplet. The same length as the notional list.</param>
        /// <param name="resets">An array of reset rates. This may be null.</param>
        /// <param name="resetOffset">The relative date offset for all the fixings.</param>
        /// <param name="paymentBusinessDayAdjustments">The payment business day adjustments.</param>
        /// <param name="capCalculation">The cap calculation.</param>
        /// <param name="fixingCalendar">The fixing Calendar.</param>
        public PriceableIRFloor(DateTime baseDate, string identifier, List<DateTime> rollDates,
                              List<double> notionals, List<double> strikes, List<double> resets, 
                              RelativeDateOffset resetOffset, BusinessDayAdjustments paymentBusinessDayAdjustments,
                              Calculation capCalculation, IBusinessCalendar fixingCalendar)
            : base(baseDate, identifier, rollDates, notionals, strikes, resets, resetOffset,
            paymentBusinessDayAdjustments, capCalculation, fixingCalendar)
        {
            IsCap = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PriceableIRCap"/> class.
        /// </summary>
        /// <param name="baseDate">The base date.</param>
        /// <param name="effectiveDate">The effective Date.</param>
        /// <param name="term">The cap term.</param>
        /// <param name="strike">The strike for each caplet.</param>
        /// <param name="lastResets">A list of reset rates. This may be null.</param>
        /// <param name="includeStubFlag">A flag: include the first stub periood or not.</param>
        /// <param name="paymentFrequency">The caplet frequency.</param>
        /// <param name="rollBackward">A flag which determines whether to roll 
        /// the dates: Backward or Forward. Currency this is ignored.</param>
        /// <param name="resetOffset">The relative date offset for all the fixings.</param>
        /// <param name="paymentBusinessDayAdjustments">The payment business day adjustments.</param>
        /// <param name="capCalculation">The cap calculation.</param>
        /// <param name="paymentCalendar">The payment Calendar.</param>
        /// <param name="fixingCalendar"></param>
        public PriceableIRFloor(DateTime baseDate, DateTime effectiveDate,
            string term, Double strike, List<double> lastResets, 
            Boolean includeStubFlag, string paymentFrequency, 
            Boolean rollBackward, RelativeDateOffset resetOffset, 
            BusinessDayAdjustments paymentBusinessDayAdjustments,
            Calculation capCalculation, IBusinessCalendar fixingCalendar, IBusinessCalendar paymentCalendar)
            : base(baseDate, effectiveDate, term, strike, lastResets, includeStubFlag,
            paymentFrequency, rollBackward, resetOffset,
            paymentBusinessDayAdjustments, capCalculation, fixingCalendar, paymentCalendar)
        {
            IsCap = false;
        }
    }
}