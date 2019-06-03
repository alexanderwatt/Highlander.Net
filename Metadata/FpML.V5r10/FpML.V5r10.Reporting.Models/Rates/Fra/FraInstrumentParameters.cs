using FpML.V5r10.Reporting.Models.Rates.Coupons;

namespace FpML.V5r10.Reporting.Models.Rates.Fra
{
    public class FraInstrumentParameters : RateCouponParameters, IFraInstrumentParameters
    {
        /// <summary>
        /// 
        /// </summary>
        public bool IsBuyerInd { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal BreakEvenRate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public decimal BreakEvenSpread { get; set; }
    }
}