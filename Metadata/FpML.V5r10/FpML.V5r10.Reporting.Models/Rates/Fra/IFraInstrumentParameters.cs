using System;
using FpML.V5r10.Reporting.Models.Rates.Coupons;

namespace FpML.V5r10.Reporting.Models.Rates.Fra
{
    public interface IFraInstrumentParameters : IRateCouponParameters
    {
        /// <summary>
        /// 
        /// </summary>
        Boolean IsBuyerInd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Decimal BreakEvenRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        Decimal BreakEvenSpread { get; set; }
    }
}