/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Highlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Highlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Using directives

using System.Collections.Generic;

#endregion

namespace FpML.V5r3.Reporting.Helpers
{
    public  class CashflowsFactory
    {
        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods)
        {
            return Create(paymentCalculationPeriods, true);
        }

        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods, bool cashflowsMatchParameters)
        {
            var cashflows = new Cashflows
                                      {
                                          cashflowsMatchParameters = cashflowsMatchParameters,
                                          paymentCalculationPeriod = paymentCalculationPeriods.ToArray()
                                      };

            return cashflows;
        }

        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods, List<PrincipalExchange> principalExchanges)
        {
            return Create(paymentCalculationPeriods, principalExchanges, true);
        }

        public static Cashflows Create(List<PaymentCalculationPeriod> paymentCalculationPeriods, List<PrincipalExchange> principalExchanges, bool cashflowsMatchParameters)
        {
            var cashflows = new Cashflows
                                      {
                                          cashflowsMatchParameters = cashflowsMatchParameters,
                                          paymentCalculationPeriod = paymentCalculationPeriods.ToArray(),
                                          principalExchange = principalExchanges.ToArray()
                                      };

            return cashflows;
        }

    }
}
