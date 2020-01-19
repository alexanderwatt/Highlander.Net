using System;
using FpML.V5r10.Reporting;
using FpML.V5r10.Reporting.ModelFramework;
using FpML.V5r10.Reporting.ModelFramework.Assets;
using FpML.V5r10.Reporting.Models.Equity;

namespace Orion.CurveEngine.Assets.Equity
{
    /// <summary>
    /// PriceableBondAssetController
    /// </summary>
    public abstract class PriceableEquityAssetController : AssetControllerBase, IPriceableEquityAssetController
    {
        public abstract EquityAsset GetEquity();

        ///<summary>
        ///</summary>
        public IEquityAssetResults CalculationResults { get; set; }

        /// <summary>
        /// Gets the analytic model parameters.
        /// </summary>
        /// <value>The analytic model parameters.</value>
        public IEquityAssetParameters AnalyticModelParameters { get; protected set; }

        // Analytics
        public IModelAnalytic<IEquityAssetParameters, EquityMetrics> AnalyticsModel { get; set; }

        ///<summary>
        ///</summary>
        public DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        public Decimal QuoteValue { get; set; }

        ///<summary>
        ///</summary>
        public DateTime SettlementDate { get; set; }

        /// <summary>
        /// THe equity valuation curve.
        /// </summary>
        public string EquityCurveName { get; set; }

        /// <summary>
        /// The multiplier to be set
        /// </summary>
        public decimal Multiplier { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextDividendDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastDividendDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime NextExDivDate { get; set; }

        ///<summary>
        ///</summary>
        public DateTime LastRegDividendDate { get; set; }

        ///<summary>
        ///</summary>
        public int RegCoupsToMaturity { get; set; }

        ///<summary>
        ///</summary>
        public DateTime Next2DividendDate { get; set; }

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
        public Decimal Notional { get; set; }

        ///<summary>
        ///</summary>
        public Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        public int Frequency { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal Price { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal IndexAtMaturity { get; set; }
    }
}