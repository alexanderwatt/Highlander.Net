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

using System;
using System.Collections.Generic;

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// A parent class for FixedPaymentStream and FloatingPaymentStream
    /// </summary>
    public abstract class PaymentStream
    {
        
            #region Enums

            /// <summary>
            /// Payer = -1
            /// Receiver = +1
            /// used to determine the direction of cashflows
            /// </summary>
            public enum Direction
            { Payer = -1, Receiver = 1 };

        #endregion

            #region Constructors
                
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
                /// <param name="principalExchange">The amount of principle exchanged at the beginning and end of swap</param>
                protected PaymentStream(Direction paymentDirection,
                                  ICollection<decimal> notional,
                                  ICollection<decimal> accrualFactors,
                                  ICollection<decimal> cashflows,
                                  IEnumerable<decimal> discountFactors,
                                  decimal principalExchange)
                               
            {

                ValidateInputLists(accrualFactors, cashflows, notional);

                Dir = paymentDirection;

                Notional = new List<decimal>();
                Notional.AddRange(notional);

                AccrualFactors = new List<decimal>();
                AccrualFactors.AddRange(accrualFactors);

                Cashflows = new List<decimal>();
                Cashflows.AddRange(cashflows);

                DiscountFactors = new List<decimal>();
                DiscountFactors.AddRange(discountFactors);

                PrincipleExchange = principalExchange;
               
            }
 

            #endregion

            #region Public Methods

                
               

                /// <summary>
                /// Calculates the value of level function
                /// </summary>
                /// <returns>Level Function value in decimal</returns>
                public  decimal LevelFunction()
                {
                    decimal levelFunctionValue = 0.0m;
                    int len = AccrualFactors.Count;

                    for (int i = 0; i < len; ++i)
                    {
                        levelFunctionValue += AccrualFactors[i] * DiscountFactors[i+1];
                    }

                    return levelFunctionValue * (int) Dir;
                }

                /// <summary>
                /// The net present value of principle exchange on the stream
                /// It is assumed that amount of principle exchange is paid a the
                /// start of the swap and the same amount is received at the final
                /// coupon date.
                /// </summary>
                /// <returns></returns>
                public decimal PrincipleExchangePV()
                {

                    int len = DiscountFactors.Count;
                    return -1.0m * PrincipleExchange * DiscountFactors[0] +
                                        PrincipleExchange * DiscountFactors[len - 1];

                } 


                /// <summary>
                /// Accrued net present value of notional
                /// </summary>
                /// <returns>net present value of notional</returns>
                public decimal AccruedNPVOfNotional()
                {
                    decimal accruedValue = 0.0m;

                    int len = Notional.Count;

                    for (int i = 0; i < len; ++i)
                        accruedValue += Notional[i] * AccrualFactors[i] * DiscountFactors[i+1];

                    
                    return accruedValue * (int)Dir;

                }

                /// <summary>
                /// Net present value of the steam
                /// </summary>
                /// <returns></returns>
                public decimal NPV()
                {
                    return AccruedNPV() + PrincipleExchangePV();
                }

                

                /// <summary>
                /// Accrued net present value of all the coupons
                /// of the stream
                /// </summary>
                /// <returns></returns>
                public decimal AccruedNPV()
                {
                    decimal accruedValue = 0.0m;

                    int len = Cashflows.Count;

                    for (int i = 0; i < len; ++i)
                        accruedValue += Cashflows[i] * DiscountFactors[i + 1];

                    return accruedValue * (int)Dir;

                }
                
                #endregion

            #region Public Accessors

                /// <summary>
                ///Get the type of the stream (Payer or Receiver)
                /// </summary>
                public Direction Dir { get; }

                /// <summary>
                /// Get the list of notional based on which each coupon is
                /// calculated
                /// </summary>
                public List<decimal> Notional { get; }

                /// <summary>
            /// Get the list of coupon periods as a fraction of year
            /// </summary>
            public List<decimal> AccrualFactors { get; }

                /// <summary>
            /// Get the list of cashflows
            /// </summary>
            public List<decimal> Cashflows { get; }

                /// <summary>
            /// Get the list of discount factors for coupon payment days
            /// </summary>
            public List<decimal> DiscountFactors { get; }


                /// <summary>
            /// Get teh principle exchange amount
            /// </summary>
            public decimal PrincipleExchange { get; }

                #endregion

            #region Helper Functions

            private static void ValidateInputLists(ICollection<decimal> list1,
                                           ICollection<decimal> list2,
                                           ICollection<decimal> list3)
            {
                if ((list1.Count != list2.Count) || (list1.Count != list3.Count))
                {
                    throw new Exception("The input data must be of the same length.");
                }
            } 

            #endregion

            #region Private Data Members

            //private List<decimal> _tenors;

            #endregion

    }
}
