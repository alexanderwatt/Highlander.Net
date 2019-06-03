using Orion.Models.Rates.Coupons;
using System;

namespace Orion.Models.Rates.TermDeposit
{
    public class TermDepositInstrumentParameters : RateCouponParameters, ITermDepositInstrumentParameters
    {
        public bool IsLenderInd { get; set; }

        public decimal BreakEvenRate { get; set; }

        public decimal BreakEvenSpread { get; set; }
    }
}