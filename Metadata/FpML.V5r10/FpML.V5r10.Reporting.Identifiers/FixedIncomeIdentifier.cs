#region Using directives

using System;
using FpML.V5r10.Reporting.ModelFramework.Identifiers;
using Orion.Constants;

#endregion

namespace FpML.V5r10.Reporting.Identifiers
{
    /// <summary>
    /// The RateCurveIdentifier.
    /// </summary>
    public class FixedIncomeIdentifier : Identifier, IFixedIncomeIdentifier
    {
        /// <summary>
        /// The Source System.
        /// </summary>
        /// <value></value>
        public string SourceSystem {get; set;}

        ///<summary>
        /// The base party.
        ///</summary>
        public string MarketSector { get; set; }

        ///<summary>
        /// An id for a bond.
        ///</summary>
        ///<param name="coupon">The coupon. Prefixed by F if floating and V if variable. </param>
        ///<param name="marketSector">The market sector. This is the Bloomberg designation. </param>
        ///<param name="ticker">The bond ticker. </param>
        ///<param name="maturityDate">The maturity date as a string and formatted as MM/DD/YY </param>
        ///<param name="couponType">The cooupon type: fixed, float or struct. </param>
        public FixedIncomeIdentifier(string ticker, string coupon, string marketSector, DateTime maturityDate, string couponType)
            : base(BuildUniqueId(ticker, coupon, marketSector, maturityDate, couponType))
        {
            MarketSector = marketSector;
            Id = BuildId(ticker, coupon, marketSector, maturityDate, couponType);
        }

        private static string BuildUniqueId(string ticker, string coupon, string marketSector, DateTime maturityDate, string couponType)
        {
            var coup = coupon.Replace('.', ',');
            return FunctionProp.ReferenceData + "." + ReferenceDataProp.FixedIncome + "." + marketSector + "." + ticker + "." + couponType + "." + coup + "." + maturityDate.ToString("d");
        }

        public static string BuildId(string ticker, string coupon, string marketSector, DateTime maturityDate, string couponType)
        {
            var coup = coupon.Replace('.', ',');
            return marketSector + "." + ticker + "." + couponType + "." + coup + "." + maturityDate.ToString("d");
        }
    }
}