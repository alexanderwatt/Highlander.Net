
using System;

namespace FpML.V5r10.Reporting.ModelFramework.Assets
{
    /// <summary>
    /// Base rate asset controller interface
    /// </summary>
    public interface IPriceableBondAssetController : IPriceableAssetController
    {
        ///<summary>
        ///</summary>
        Bond GetBond();

        ///<summary>
        ///</summary>
        DateTime BaseDate { get; set; }

        ///<summary>
        ///</summary>
        Boolean IsYTMQuote { get; set; }

        ///<summary>
        ///</summary>
        Decimal QuoteValue { get; set; }

        ///<summary>
        ///</summary>
        DateTime SettlementDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime MaturityDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime NextCouponDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime LastCouponDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime NextExDivDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime LastRegCoupDate { get; set; }

        ///<summary>
        ///</summary>
        int RegCoupsToMaturity { get; set; }

        ///<summary>
        ///</summary>
        DateTime Next2CoupDate { get; set; }

        ///<summary>
        ///</summary>
        DateTime FirstAccrualDate { get; set; }

        ///<summary>
        ///</summary>
        int CoupNum { get; set; }

        ///<summary>
        ///</summary>
        bool IsXD { get; set; }

        ///<summary>
        ///</summary>
        RelativeDateOffset ExDividendDateOffset { get; set; }

        ///<summary>
        ///</summary>
        RelativeDateOffset SettlementDateOffset { get; set; }

        ///<summary>
        ///</summary>
        IBusinessCalendar SettlementDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        IBusinessCalendar PaymentDateCalendar { get; set; }

        ///<summary>
        ///</summary>
        DateTime[] UnAdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        DateTime[] AdjustedPeriodDates { get; set; }

        ///<summary>
        ///</summary>
        Decimal Notional { get; set; }

        ///<summary>
        ///</summary>
        Currency Currency { get; set; }

        ///<summary>
        ///</summary>
        int Frequency { get; set; }

        /// <summary>
        /// The purchase price as a dirty price.
        /// </summary>
        Decimal PurchasePrice { get; set; }

        ///<summary>
        ///</summary>
        BusinessDayAdjustments PaymentBusinessDayAdjustments { get; set; }

        /// <summary>
        /// Gets the year fraction.
        /// </summary>
        /// <returns></returns>
        decimal[] GetYearFractions();

        /// <summary>
        /// Gets or sets the multiplier.
        /// </summary>
        ///  <value>The multiplier.</value>
        Decimal Multiplier { get; set; }
    }
}