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
    ///<summary>
    ///</summary>
    public class FloatingPaymentStream: PaymentStream
    {
        #region Constructor

        ///<summary>
        ///</summary>
        ///<param name="paymentDirection"></param>
        ///<param name="notional"></param>
        ///<param name="accrualFactors"></param>
        ///<param name="cashflows"></param>
        ///<param name="discountFactors"></param>
        ///<param name="principleExchange"></param>
        public FloatingPaymentStream(Direction paymentDirection,
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
                                  )
        {

            ForwardRates = CalculateForwardRates(discountFactors, accrualFactors);
        }
        

        #endregion

        #region Helper Methods

        /// <summary>
        /// Calculates the forward rates
        /// </summary>
        /// <param name="discountFactors">List of discount factors</param>
        /// <param name=" accrualFactors">List of year fractions</param>
        /// <param name="accrualFactors"></param>
        /// <returns>A list of forward rates</returns>
        static List<decimal> CalculateForwardRates(List<decimal> discountFactors, List<decimal> accrualFactors)
        {
            int len = discountFactors.Count -1; 
            var forwardRates = new List<decimal>(len);
            //forwardRates.Add((1 - discountFactors[0]) / (discountFactors[0] * accrualFactors[0]));
            for(int i = 0; i < len ; ++i )
            { 
                forwardRates.Add( (discountFactors[i] - discountFactors[i+1]) /(discountFactors[i+1] * accrualFactors[i]));
            }
            return forwardRates;
        }

        #endregion

        #region Public Accessors

        public List<decimal> ForwardRates { get; }

        #endregion

        #region Private Data Members

        #endregion
    }
}