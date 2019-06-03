using System;
using FpML.V5r10.Reporting.Models.Rates.Coupons;

namespace FpML.V5r10.Reporting.Models.Rates.TermDeposit
{
    public interface ITermDepositInstrumentParameters : IRateCouponParameters
    {
        Boolean IsLenderInd { get; set; }

        Decimal BreakEvenRate { get; set; }

        Decimal BreakEvenSpread { get; set; }
    }
}