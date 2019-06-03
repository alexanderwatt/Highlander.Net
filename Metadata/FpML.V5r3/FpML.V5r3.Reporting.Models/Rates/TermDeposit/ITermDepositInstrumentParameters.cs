using System;
using Orion.Models.Rates.Coupons;


namespace Orion.Models.Rates.TermDeposit
{
    public interface ITermDepositInstrumentParameters : IRateCouponParameters
    {
        Boolean IsLenderInd { get; set; }

        Decimal BreakEvenRate { get; set; }

        Decimal BreakEvenSpread { get; set; }
    }
}