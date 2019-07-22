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

using System.Collections.Generic;

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// This class contains all the necessary information
    /// for fixed leg of the swap
    /// </summary>
    public class FixedPaymentStream: PaymentStream
    {
        #region Constructor

        /// <summary>
        /// The payment stream is class which encapsulate all the 
        /// necessary information for each leg of the swap
        /// important:
        /// The number of discount factors is more than the number of
        /// notional, accrual factors and cashflows by one
        /// The first discount factor refers to discount factor of
        /// principle exchange date.
        /// If principle exchange amount is zero then the first discount
        /// factor should be set to 1.
        /// </summary>
        /// <param name="paymentDirection">Payer or Receiver</param>
        /// <param name="notional">List of notional amount based on which the coupons are calculated</param>
        /// <param name="accrualFactors">List of coupon periods as fraction of year </param>
        /// <param name="cashflows">List of cashflows</param>
        /// <param name="discountFactors">List of discount factors associated with coupon payment dates</param>
        /// <param name="principleExchange">The amount of principle exchanged at the beginning and end of swap</param>
        public FixedPaymentStream(Direction paymentDirection,
                                  List<decimal> notional,
                                  List<decimal> accrualFactors,
                                  List<decimal> cashflows,
                                  List<decimal> discountFactors,
                                  decimal principleExchange
                                 ) :
                       base(paymentDirection,
                                  notional,
                                  accrualFactors,
                                  cashflows,
                                  discountFactors,
                                  principleExchange
                                  ) { }
        

        #endregion
    }
}
