#region Usings

using System;
using Orion.ModelFramework;
using FpML.V5r3.Reporting;
using Orion.ModelFramework.Assets;
using Orion.Models.Assets;

#endregion

namespace Orion.CurveEngine.Assets
{
    /// <summary>
    /// PriceableBondAssetController
    /// </summary>
    public abstract class PriceableBondAssetController : AssetControllerBase, IPriceableBondAssetController
    {
        public abstract Bond GetBond();

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        public Decimal Multiplier { get; set; }

        ///<summary>
        ///</summary>
        public IBondAssetResults CalculationResults { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IBondAssetParameters AnalyticModelParameters { get; protected set; }

        // Analytics
        public IModelAnalytic<IBondAssetParameters, BondMetrics> AnalyticsModel { get; set; }

        ///<summary>
        ///</summary>
        public DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        public Boolean IsYTMQuote { get; set; }

        ///<summary>
        ///</summary>
        public Decimal QuoteValue { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SettlementDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime MaturityDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextCouponDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastCouponDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextExDivDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastRegCoupDate { get; set; }

        ///<summary>
        ///</summary>
        public int RegCoupsToMaturity { get; set; }

        ///<summary>
        ///</summary>
        public DateTime Next2CoupDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime FirstAccrualDate { get; set; }

        ///<summary>
        ///</summary>
        public int CoupNum { get; set; }

        ///<summary>
        ///</summary>
        public bool IsXD { get; set; }

        ///<summary>
        ///</summary>
        public RelativeDateOffset ExDividendDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public RelativeDateOffset SettlementDateOffset { get; set; }

        ///<summary>
        ///</summary>
        public IBusinessCalendar SettlementDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        public IBusinessCalendar PaymentDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        public DateTime[] UnAdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        public DateTime[] AdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        public Decimal Notional { get; set; }

        ///<summary>
        ///</summary>
        public Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        public int Frequency { get; set; }

        /// <summary>
        /// The purchase price as a dirty price.
        /// </summary>
        public Decimal PurchasePrice { get; set; }

        ///<summary>
        ///</summary>
        public BusinessDayAdjustments PaymentBusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        public abstract decimal[] GetYearFractions();
    }
}