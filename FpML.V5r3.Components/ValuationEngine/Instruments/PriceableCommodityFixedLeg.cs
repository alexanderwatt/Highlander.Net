#region Usings

using System;
using System.Collections.Generic;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Instruments;
using Orion.Models.Commodities.CommoditySwapLeg;

#endregion

namespace Orion.ValuationEngine.Instruments
{
    [Serializable]
    public class PriceableCommodityFixedLeg : PriceableCommoditySwapLeg
    {

        #region Public Fields

        public NonPeriodicFixedPriceLeg FixedLeg { get; set; }

        #endregion

        #region Constructors

        protected PriceableCommodityFixedLeg(NonPeriodicFixedPriceLeg fixedLeg)
        {
            Multiplier = 1.0m;
            AnalyticsModel = new CommoditySwapLegAnalytic();
            FixedLeg = fixedLeg;
        }

        #endregion

        #region FpML Representation

        /// <summary>
        /// Builds this instance.
        /// </summary>
        /// <returns></returns>
        public override CommoditySwapLeg Build()
        {
            return new NonPeriodicFixedPriceLeg();
        }
       
        #endregion

        #region Static Helpers


        #endregion

        #region Overrides of InstrumentControllerBase

        /// <summary>
        /// Calculates the specified model data.
        /// </summary>
        /// <param name="modelData">The model data.</param>
        /// <returns></returns>
        public override AssetValuation Calculate(IInstrumentControllerData modelData)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Updates the bucketing interval.
        /// </summary>
        /// <param name="baseDate">The base date for bucketting.</param>
        /// <param name="bucketingInterval">The bucketing interval.</param>
        public override DateTime[] GetBucketingDates(DateTime baseDate, Period bucketingInterval)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Builds the product with the calculated data.
        /// </summary>
        /// <returns></returns>
        public override Product BuildTheProduct()
        {
            throw new NotImplementedException();
        }

        ///<summary>
        /// Gets all the child controllers.
        ///</summary>
        ///<returns></returns>
        public override IList<InstrumentControllerBase> GetChildren()
        {
            return MapPayments(Payments);
        }

        #endregion
    }
}