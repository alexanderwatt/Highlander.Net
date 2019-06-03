using System;
using Orion.Models.Rates.Coupons;

namespace Orion.Models.Rates.Fra
{
    public interface IFraInstrumentParameters : IRateCouponParameters
    {
        Boolean IsBuyerInd { get; set; }

        Decimal BreakEvenRate { get; set; }

        Decimal BreakEvenSpread { get; set; }
    }
}