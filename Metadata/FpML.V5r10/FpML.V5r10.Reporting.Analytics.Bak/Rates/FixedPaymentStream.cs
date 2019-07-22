using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// This class contains all the necessary inforamtion
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
        /// principle exhange date.
        /// If principle exchange amount is zero then the first discount
        /// factor should be set to 1.
        /// </summary>
        /// <param name="paymentDirection">Payer or Receiver</param>
        /// <param name="notional">List of notional amount based on which the coupons are calculated</param>
        /// <param name="accrualFactors">List of coupon periods as fraction of year </param>
        /// <param name="cashflows">List of cashflows</param>
        /// <param name="discountFactors">List of discountfactors associated with coupon payment dates</param>
        /// <param name="principleExchange">The amount of principle exchanged at the begining and end of swap</param>
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
