/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/awatt/highlander

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/awatt/highlander/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

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
                y != null && x != null && (x.AccrualStartDate == y.AccrualStartDate
                                           &&
                                           x.AccrualEndDate == y.AccrualEndDate
                                           &&
                                           x.PaymentDate == y.PaymentDate
                                           &&
                                           x.YearFraction == y.YearFraction)
                )
            {
                return 0;
            }
            if (y != null && x != null && x.PaymentDate != y.PaymentDate)
            {
                return DateTime.Compare(x.PaymentDate, y.PaymentDate);
            }
            if (y != null && x != null && x.AccrualEndDate != y.AccrualEndDate)
            {
                return DateTime.Compare(x.AccrualEndDate, y.AccrualEndDate);
            }
            if (y != null && x != null && x.AccrualStartDate != y.AccrualStartDate)
            {
                return DateTime.Compare(x.AccrualStartDate, y.AccrualStartDate);
            }
            if (y != null && (x != null && x.YearFraction != y.YearFraction))
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