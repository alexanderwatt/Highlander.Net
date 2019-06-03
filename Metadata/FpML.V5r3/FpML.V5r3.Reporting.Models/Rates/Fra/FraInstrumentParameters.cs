using Orion.Models.Rates.Coupons;
using System;

namespace Orion.Models.Rates.Fra
{
    public class FraInstrumentParameters : RateCouponParameters, IFraInstrumentParameters
    {
        public bool IsBuyerInd { get; set; }

        public decimal BreakEvenRate { get; set; }

        public decimal BreakEvenSpread { get; set; }
    }
}