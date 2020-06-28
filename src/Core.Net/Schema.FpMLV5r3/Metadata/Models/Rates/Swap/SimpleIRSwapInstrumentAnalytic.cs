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

#region Usings

using System;
using Highlander.Reporting.Analytics.V5r3.Solvers;
using Highlander.Reporting.ModelFramework.V5r3;

#endregion

namespace Highlander.Reporting.Models.V5r3.Rates.Swap
{
    public class SimpleIRSwapInstrumentAnalytic : ModelAnalyticBase<IIRSwapInstrumentParameters, SwapInstrumentMetrics>, IIRSwapInstrumentResults, IObjectiveFunction
    {
        /// <summary>
        /// Gets the break even rate.
        /// </summary>
        /// <value>The break even rate.</value>
        public decimal BreakEvenRate => EvaluateBreakEvenRate();

        /// <summary>
        /// Gets the break even spread.
        /// </summary>
        /// <value>The break even spread.</value>
        public decimal BreakEvenSpread => EvaluateBreakEvenSpread();

        /// <summary>
        /// Gets the PCE.
        /// </summary>
        /// <value>The PCE.</value>
        public decimal[] PCE => Array.Empty<decimal>();

        /// <summary>
        /// Gets the PCE term.
        /// </summary>
        /// <value>The PCE term.</value>
        public decimal[] PCETerm => Array.Empty<decimal>();

        /// <summary>
        /// Evaluates the break even rate.
        /// </summary>
        /// <returns></returns>
        public virtual decimal EvaluateBreakEvenRate()
        {
            var result = 0.0m;
            //TODO need to handle the case of a discount swap. There is no parameter flag yet for this.
            //if (!AnalyticParameters.IsDiscounted)
            //{
                if (AnalyticParameters.IsPayFixedInd)
                {
                    if (AnalyticParameters.PayStreamAccrualFactor != 0)
                    {
                        result = AnalyticParameters.ReceiveStreamFloatingNPV / AnalyticParameters.PayStreamAccrualFactor / 10000;
                    }
                }
                else
                {
                    if (AnalyticParameters.ReceiveStreamAccrualFactor != 0)
                    {
                        result = AnalyticParameters.PayStreamFloatingNPV / AnalyticParameters.ReceiveStreamAccrualFactor / 10000;
                    }
                }
                return -result;
            //}
        }


        //virtual public Decimal EvaluateBreakEvenRate2()
        //{
        //    var result = 0.0m;

        //    //PaymentStream payerStream;
        //    //PaymentStream recStream;

        //    //PaymentStream.Direction direction = PaymentStream.Direction.Payer;
        //    //if (AnalyticParameters.IsPayFixedInd)
        //    //{
        //    //    payerStream = new FixedPaymentStream(PaymentStream.Direction.Payer, new List<Decimal>(AnalyticParameters.PayerCouponNotionals), new List<Decimal>(AnalyticParameters.PayerCouponYearFractions), new List<Decimal>(AnalyticParameters.PayerPresentValues), new List<Decimal>(AnalyticParameters.PayerPaymentDiscountFactors), AnalyticParameters.PayerPrincipalExchange);
        //    //    recStream = new FloatingPaymentStream(PaymentStream.Direction.Receiver, new List<Decimal>(AnalyticParameters.ReceiverCouponNotionals), new List<Decimal>(AnalyticParameters.ReceiverCouponYearFractions), new List<Decimal>(AnalyticParameters.ReceiverPresentValues), new List<Decimal>(AnalyticParameters.ReceiverPaymentDiscountFactors), AnalyticParameters.ReceiverPrincipalExchange);
        //    //}
        //    //else
        //    //{
        //    //    direction = PaymentStream.Direction.Receiver;
        //    //    payerStream = new FloatingPaymentStream(PaymentStream.Direction.Payer, new List<Decimal>(AnalyticParameters.PayerCouponNotionals), new List<Decimal>(AnalyticParameters.PayerCouponYearFractions), new List<Decimal>(AnalyticParameters.PayerPresentValues), new List<Decimal>(AnalyticParameters.PayerPaymentDiscountFactors), AnalyticParameters.ReceiverPrincipalExchange);
        //    //    recStream = new FixedPaymentStream(PaymentStream.Direction.Receiver, new List<Decimal>(AnalyticParameters.ReceiverCouponNotionals), new List<Decimal>(AnalyticParameters.ReceiverCouponYearFractions), new List<Decimal>(AnalyticParameters.ReceiverPresentValues), new List<Decimal>(AnalyticParameters.ReceiverPaymentDiscountFactors), AnalyticParameters.ReceiverPrincipalExchange);
        //    //}
        //    //var swap = new CrossCurrencyIRSwap(payerStream, recStream, AnalyticParameters.ReceiverToPayerSpotRate, direction);
        //    //decimal result = swap.BreakEvenRate(direction);

        //    return result;
        //}

        /// <summary>
        /// Evaluates the break even spread.
        /// </summary>
        /// <returns></returns>
        public virtual decimal EvaluateBreakEvenSpread()
        {
            var result = 0.0m;
            if (!AnalyticParameters.IsPayFixedInd)
            {
                if (AnalyticParameters.PayStreamAccrualFactor != 0)
                {
                    result = AnalyticParameters.NPV / AnalyticParameters.PayStreamAccrualFactor / 10000;
                }
            }
            else
            {
                if (AnalyticParameters.ReceiveStreamAccrualFactor != 0)
                {
                    result = AnalyticParameters.NPV / AnalyticParameters.ReceiveStreamAccrualFactor / 10000;
                }
            }
            return -result;
        }

        /// <summary>
        /// Gets the implied quote.
        /// </summary>
        /// <value>The implied quote.</value>
        public decimal ImpliedQuote => EvaluateBreakEvenRate();

        /// <summary>
        /// Gets the market quote.
        /// </summary>
        /// <value>The market quote.</value>
        public decimal MarketQuote => AnalyticParameters.MarketQuote;

        #region Implementation of IObjectiveFunction

        /// <summary>
        /// Definiton of the objective function.
        /// </summary>
        /// <param name="fixedRate">Argument to the objective function.</param>
        /// <returns>The value of the objective function, <i>f(x)</i>.</returns>
        public double Value(double fixedRate)
        {
            var result = AnalyticParameters.TargetNPV;
            //// update fixed leg
            var accruals = AnalyticParameters.PayerCouponYearFractions.Length;
            var discounts = AnalyticParameters.PayerPaymentDiscountFactors.Length;
            if (accruals == discounts)
            {
                for (var i = 0; i < accruals; i++)
                {
                    result += AnalyticParameters.PayerCouponYearFractions[i] * AnalyticParameters.PayerPaymentDiscountFactors[i] * (decimal)fixedRate;
                }
            }
            return (double)result;
        }

        /// <summary>
        /// Derivative of the objective function.
        /// </summary>
        /// <param name="x">Argument to the objective function.</param>
        /// <returns>The value of the derivative, <i>f'(x)</i>.</returns>
        /// <exception cref="NotImplementedException">
        /// Thrown when the function's derivative has not been implemented.
        /// </exception>
        public double Derivative(double x)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}