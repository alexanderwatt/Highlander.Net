using FpML.V5r10.Reporting.Models.Rates.Coupons;

namespace FpML.V5r10.Reporting.Models.Rates.TermDeposit
{
    public class TermDepositInstrumentParameters : RateCouponParameters, ITermDepositInstrumentParameters
    {
        public bool IsLenderInd { get; set; }

        public decimal BreakEvenRate { get; set; }

        public decimal BreakEvenSpread { get; set; }
    }
}