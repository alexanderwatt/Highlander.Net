#region Using directives

using System;
using System.Collections.Generic;

#endregion

namespace Orion.ValuationEngine.Helpers
{
    ///<summary>
    ///</summary>
    public class CashflowCombinedComparer : IComparer<CashflowCombined>
    {
        public int Compare(CashflowCombined x, CashflowCombined y)
        {
            if (
                x.AccrualStartDate == y.AccrualStartDate
                &&
                x.AccrualEndDate == y.AccrualEndDate
                &&
                x.PaymentDate == y.PaymentDate
                &&
                x.YearFraction == y.YearFraction
                )
            {
                return 0;
            }
            if (x.PaymentDate != y.PaymentDate)
            {
                return DateTime.Compare(x.PaymentDate, y.PaymentDate);
            }
            if (x.AccrualEndDate != y.AccrualEndDate)
            {
                return DateTime.Compare(x.AccrualEndDate, y.AccrualEndDate);
            }
            if (x.AccrualStartDate != y.AccrualStartDate)
            {
                return DateTime.Compare(x.AccrualStartDate, y.AccrualStartDate);
            }
            if (x.YearFraction != y.YearFraction)
            {
                return x.YearFraction > y.YearFraction ? 1 : 0;
            }
            throw new NotSupportedException();
        }
    }

    ///<summary>
    ///</summary>
    public class CashflowCombined
    {
        public DateTime AccrualStartDate;
        public DateTime AccrualEndDate;
        public DateTime PaymentDate;

        public decimal YearFraction;

// ReSharper disable InconsistentNaming
        public decimal FixedIntFV;
        public decimal FloatIntFV;
        public decimal SwapIntFV;
        public decimal SwapIntPV;
// ReSharper restore InconsistentNaming

// ReSharper disable InconsistentNaming
        public decimal FixedIntPV;//temp only
        public decimal FloatIntPV;
// ReSharper restore InconsistentNaming


        public decimal FixedRate;
        public decimal ForwardRate;

        public decimal DiscountFactor;
        public decimal Notional;

    }
}