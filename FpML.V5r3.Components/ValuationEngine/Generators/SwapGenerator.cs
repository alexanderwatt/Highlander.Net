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
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Constants;
using Orion.Util.Helpers;
using Orion.Util.Logging;
using FpML.V5r3.Reporting;
using Orion.ModelFramework;
using Orion.ModelFramework.MarketEnvironments;
using Orion.ModelFramework.PricingStructures;

#endregion

namespace Orion.ValuationEngine.Generators
{
    /// <summary>
    /// </summary>
    public class SwapGenerator
    {
        #region New Format

        /// <summary>
        /// 
        /// </summary>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg2Parameters"></param>
        /// <returns></returns>
        public static Swap GenerateDefinition(
            SwapLegParametersRange leg1Parameters,

            SwapLegParametersRange leg2Parameters)
        {
            InterestRateStream stream1 = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(leg1Parameters);
            InterestRateStreamHelper.SetPayerAndReceiver(stream1, leg1Parameters.Payer, leg1Parameters.Receiver);
            InterestRateStream stream2 = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(leg2Parameters);
            InterestRateStreamHelper.SetPayerAndReceiver(stream2, leg2Parameters.Payer, leg2Parameters.Receiver);
            return SwapFactory.Create(stream1, stream2);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1Calendars"></param>
        /// <param name="leg2Parameters"></param>
        /// <param name="leg2Calendars"></param>
        /// <param name="fixedRateSchedule"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <returns></returns>
        public static Swap GenerateDefinitionCashflows(ILogger logger, 
            ICoreCache cache, string nameSpace,
            SwapLegParametersRange leg1Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg1Calendars,
            SwapLegParametersRange leg2Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg2Calendars,
            Schedule fixedRateSchedule,
            Schedule    spreadSchedule,
            NonNegativeAmountSchedule notionalSchedule)
        {
            IBusinessCalendar leg1PaymentCalendar = null;
            IBusinessCalendar leg2PaymentCalendar = null;
            IBusinessCalendar leg1FixingCalendar = null;
            IBusinessCalendar leg2FixingCalendar = null;
            if (leg1Calendars != null)
            {
                leg1FixingCalendar = leg1Calendars.First;
                leg1PaymentCalendar = leg1Calendars.Second;                
            }
            else
            {
                if (!string.IsNullOrEmpty(leg1Parameters.PaymentCalendar))
                {
                    var payCalendar = BusinessCentersHelper.Parse(leg1Parameters.PaymentCalendar);
                    leg1PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, payCalendar, nameSpace);
                    leg1FixingCalendar = leg1PaymentCalendar;
                }
                if (!string.IsNullOrEmpty(leg1Parameters.FixingCalendar))
                {
                    var fixingCalendar = BusinessCentersHelper.Parse(leg1Parameters.FixingCalendar);
                    leg1FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixingCalendar, nameSpace);
                }
            }
            if (leg2Calendars != null)
            {
                leg2FixingCalendar = leg2Calendars.First;
                leg2PaymentCalendar = leg2Calendars.Second;
            }
            else
            {
                if (!string.IsNullOrEmpty(leg2Parameters.PaymentCalendar))
                {
                    var payCalendar = BusinessCentersHelper.Parse(leg2Parameters.PaymentCalendar);
                    leg2PaymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, payCalendar, nameSpace);
                    leg2FixingCalendar = leg2PaymentCalendar;
                }
                if (!string.IsNullOrEmpty(leg2Parameters.FixingCalendar))
                {
                    var fixingCalendar = BusinessCentersHelper.Parse(leg2Parameters.FixingCalendar);
                    leg2FixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixingCalendar, nameSpace);
                }
            }
            var swap = GenerateDefinition(leg1Parameters, leg2Parameters);           
            InterestRateStream stream1 = swap.swapStream[0];
            InterestRateStream stream2 = swap.swapStream[1];
            if (null != fixedRateSchedule)
            {
                //  Set FixedRateSchedule (if this is a fixed leg)
                //
                if (leg1Parameters.IsFixedLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetFixedRateSchedule(stream1, fixedRateSchedule);
                }
                //  Set FixedRateSchedule (if this is a fixed leg)
                //
                if (leg2Parameters.IsFixedLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetFixedRateSchedule(stream2, fixedRateSchedule);
                }
            }
            if (null != spreadSchedule) //for float legs only
            {
                if (leg1Parameters.IsFloatingLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(stream1, spreadSchedule);
                }
                if (leg2Parameters.IsFloatingLegType())
                {
                    InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(stream2, spreadSchedule);
                }
            }
            if (null != notionalSchedule)
            {
                //  Set notional schedule
                //
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream1, notionalSchedule);
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(stream2, notionalSchedule);
            }
            stream1.cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream1, leg1FixingCalendar, leg1PaymentCalendar);
            stream2.cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(stream2, leg2FixingCalendar, leg2PaymentCalendar);           
            return swap; 
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1Calendars"></param>
        /// <param name="leg2Parameters"></param>
        /// <param name="leg2Calendars"></param>
        /// <param name="fixedRateSchedule"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <returns></returns>
        public static Swap GenerateDefinitionCashflowsAmounts(ILogger logger, ICoreCache cache,
            string nameSpace,
            SwapLegParametersRange leg1Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg1Calendars,
            SwapLegParametersRange leg2Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg2Calendars,
            Schedule fixedRateSchedule,
            Schedule spreadSchedule,
            NonNegativeAmountSchedule notionalSchedule)
        {
            var swap = GenerateDefinitionCashflows(logger, cache, nameSpace, leg1Parameters, leg1Calendars, leg2Parameters, leg2Calendars, fixedRateSchedule, spreadSchedule, notionalSchedule);
            return swap;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1Calendars"></param>
        /// <param name="leg2Parameters"></param>
        /// <param name="leg2Calendars"></param>
        /// <param name="fixedRateSchedule"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <param name="leg1MarketEnvironment"></param>
        /// <param name="leg2MarketEnvironment"></param>
        /// <param name="valuationDate"></param>
        /// <returns></returns>
        public static Swap GenerateDefinitionCashflowsAmounts(ILogger logger, ICoreCache cache,
            string nameSpace, SwapLegParametersRange leg1Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg1Calendars,
            SwapLegParametersRange leg2Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg2Calendars,
            Schedule fixedRateSchedule,
            Schedule spreadSchedule,
            NonNegativeAmountSchedule notionalSchedule,
            ISwapLegEnvironment leg1MarketEnvironment,
            ISwapLegEnvironment leg2MarketEnvironment,
            DateTime valuationDate)
        {
            var swap = GenerateDefinitionCashflows(logger, cache, nameSpace, leg1Parameters, leg1Calendars, leg2Parameters, leg2Calendars, fixedRateSchedule, spreadSchedule, notionalSchedule);
            InterestRateStream stream1 = swap.swapStream[0];
            InterestRateStream stream2 = swap.swapStream[1];
            UpdateStreamCashflowsAmounts(leg1Parameters, stream1, leg1MarketEnvironment, valuationDate);
            UpdateStreamCashflowsAmounts(leg2Parameters, stream2, leg2MarketEnvironment, valuationDate);
            return swap;
        }

        public static void UpdateStreamCashflowsAmounts(SwapLegParametersRange legParameters,
                                                         InterestRateStream stream,
                                                         ISwapLegEnvironment marketEnvironment,
                                                         DateTime valuationDate)
        {
            IRateCurve forecastCurve = null;
            if (!String.IsNullOrEmpty(legParameters.ForecastCurve))
            {
                forecastCurve = marketEnvironment.GetForecastRateCurve();
            }
            IRateCurve discountingCurve = marketEnvironment.GetDiscountRateCurve();
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, forecastCurve, discountingCurve, valuationDate);
        }

        public static void UpdatePaymentsAmounts(ILogger logger, ICoreCache cache, 
            String nameSpace, Swap swap,
            SwapLegParametersRange leg1Parameters,
            SwapLegParametersRange leg2Parameters,
            IRateCurve leg1DiscountCurve,
            IRateCurve leg2DiscountCurve,
            DateTime valuationDate, IBusinessCalendar paymentCalendar)
        {
            foreach (Payment payment in swap.additionalPayment)
            {
                //  choose correct discount curve
                //
                IRateCurve discountCurve;
                if (payment.payerPartyReference.href == leg1Parameters.Payer)
                {
                    discountCurve = leg1DiscountCurve;
                }
                else if (payment.payerPartyReference.href == leg2Parameters.Payer)
                {
                    discountCurve = leg2DiscountCurve;
                }
                else
                {
                    throw new NotImplementedException();
                }
                if (paymentCalendar == null)
                {
                    var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(payment.paymentDate, ItemsChoiceType.dateAdjustments, out var dateAdjustments);
                    if (containsPaymentDateAdjustments && dateAdjustments != null)
                    {
                        paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ((BusinessDayAdjustments)dateAdjustments).
                                                                                  businessCenters, nameSpace);
                    }
                }
                var date = AdjustedDateHelper.GetAdjustedDate(paymentCalendar, payment.paymentDate);
                if (date == null) continue;
                payment.discountFactor = (decimal) discountCurve.GetDiscountFactor(valuationDate, (DateTime) date);
                payment.discountFactorSpecified = true;
                payment.presentValueAmount = MoneyHelper.Mul(payment.paymentAmount, payment.discountFactor);
            }
        }

        public static void UpdatePaymentsAmounts(ILogger logger, ICoreCache cache, 
            String nameSpace, Swap swap,
        SwapLegParametersRange leg1Parameters,
        IRateCurve leg1DiscountCurve,
        DateTime valuationDate, IBusinessCalendar paymentCalendar)
        {
            if (null != swap.additionalPayment)
            {
                foreach (Payment payment in swap.additionalPayment)
                {
                    //  choose the right discount curve
                    //
                    IRateCurve discountCurve;
                    if (payment.payerPartyReference.href == leg1Parameters.Payer)
                    {
                        discountCurve = leg1DiscountCurve;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    if (paymentCalendar == null)
                    {
                        var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(payment.paymentDate, ItemsChoiceType.dateAdjustments, out var dateAdjustments);
                        if (containsPaymentDateAdjustments && dateAdjustments != null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ((BusinessDayAdjustments)dateAdjustments).
                                                                                      businessCenters, nameSpace);
                        }
                    }
                    var date = AdjustedDateHelper.GetAdjustedDate(paymentCalendar, payment.paymentDate);
                    if (date != null)
                    {
                        payment.discountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, (DateTime)date);
                        payment.discountFactorSpecified = true;
                        payment.presentValueAmount = MoneyHelper.Mul(payment.paymentAmount, payment.discountFactor);
                    }                    
                }
            }
        }

        #endregion

        #region Old Format

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="leg1Parameters"></param>
        /// <param name="leg1Calendars"></param>
        /// <param name="leg2Parameters"></param>
        /// <param name="leg2Calendars"></param>
        /// <param name="fixedRateSchedule"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <param name="marketEnvironment"></param>
        /// <param name="valuationDate"></param>
        /// <returns></returns>
        public static Swap GenerateDefinitionCashflowsAmounts(ILogger logger, ICoreCache cache, 
            string nameSpace, SwapLegParametersRange_Old leg1Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg1Calendars,
            SwapLegParametersRange_Old leg2Parameters,
            Pair<IBusinessCalendar, IBusinessCalendar> leg2Calendars,
            Schedule fixedRateSchedule, 
            Schedule spreadSchedule,
            NonNegativeAmountSchedule notionalSchedule,
            ISwapLegEnvironment marketEnvironment,
            DateTime valuationDate)
        {
            var swap = GenerateDefinitionCashflows(logger, cache, nameSpace, leg1Parameters, leg1Calendars, leg2Parameters, leg2Calendars, fixedRateSchedule, spreadSchedule, notionalSchedule);
            InterestRateStream stream1 = swap.swapStream[0];
            InterestRateStream stream2 = swap.swapStream[1];
            UpdateStreamCashflowsAmounts(leg1Parameters, stream1, marketEnvironment, valuationDate);
            UpdateStreamCashflowsAmounts(leg2Parameters, stream2, marketEnvironment, valuationDate);
            return swap;
        }

        public static void  UpdateStreamCashflowsAmounts(SwapLegParametersRange_Old legParameters,
                                                         InterestRateStream stream,
                                                         ISwapLegEnvironment marketEnvironment,
                                                         DateTime valuationDate)
        {
            IRateCurve forecastCurve = null;           
            if (!String.IsNullOrEmpty(legParameters.ForecastCurve))
            {
                forecastCurve = marketEnvironment.GetForecastRateCurve();
            }
            IRateCurve discountingCurve = marketEnvironment.GetDiscountRateCurve();
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(stream, forecastCurve, discountingCurve, valuationDate);
        }

        public static void UpdatePaymentsAmounts(ILogger logger, ICoreCache cache, 
            String nameSpace, Swap swap, 
            SwapLegParametersRange_Old leg1Parameters,
            SwapLegParametersRange_Old leg2Parameters,
            IRateCurve leg1DiscountCurve,
            IRateCurve leg2DiscountCurve,
            DateTime valuationDate, IBusinessCalendar paymentCalendar)
        {
            foreach (Payment payment in swap.additionalPayment)
            {
                //  choose correct discount curve
                //
                IRateCurve discountCurve;
                if (payment.payerPartyReference.href == leg1Parameters.Payer)
                {
                    discountCurve = leg1DiscountCurve;
                }
                else if (payment.payerPartyReference.href == leg2Parameters.Payer)
                {
                    discountCurve = leg2DiscountCurve;
                }
                else
                {
                    throw new NotImplementedException();
                }
                if (paymentCalendar == null)
                {
                    var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(payment.paymentDate, ItemsChoiceType.dateAdjustments, out var dateAdjustments);
                    if (containsPaymentDateAdjustments && dateAdjustments != null)
                    {
                        paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ((BusinessDayAdjustments)dateAdjustments).
                                                                                  businessCenters, nameSpace);
                    }
                }
                var date = AdjustedDateHelper.GetAdjustedDate(paymentCalendar, payment.paymentDate);
                if (date != null)
                {
                    payment.discountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, (DateTime)date);
                    payment.discountFactorSpecified = true;
                    payment.presentValueAmount = MoneyHelper.Mul(payment.paymentAmount, payment.discountFactor);
                }               
            }
        }

        public static void UpdatePaymentsAmounts(ILogger logger, ICoreCache cache, 
            String nameSpace, Swap swap, 
            SwapLegParametersRange_Old leg1Parameters,
            IRateCurve leg1DiscountCurve,
            DateTime valuationDate, IBusinessCalendar paymentCalendar)
        {
            if (null != swap.additionalPayment)
            {
                foreach (Payment payment in swap.additionalPayment)
                {
                    //  choose the right discount curve
                    //
                    IRateCurve discountCurve;
                    if (payment.payerPartyReference.href == leg1Parameters.Payer)
                    {
                        discountCurve = leg1DiscountCurve;
                    }
                    else
                    {
                        throw new NotImplementedException();
                    }
                    if (paymentCalendar == null)
                    {
                        var containsPaymentDateAdjustments = AdjustableOrAdjustedDateHelper.Contains(payment.paymentDate, ItemsChoiceType.dateAdjustments, out var dateAdjustments);
                        if (containsPaymentDateAdjustments && dateAdjustments != null)
                        {
                            paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, ((BusinessDayAdjustments)dateAdjustments).
                                                                                      businessCenters, nameSpace);
                        }
                    }
                    var date = AdjustedDateHelper.GetAdjustedDate(paymentCalendar, payment.paymentDate);
                    if (date != null)
                    {
                        payment.discountFactor = (decimal)discountCurve.GetDiscountFactor(valuationDate, (DateTime)date);
                        payment.discountFactorSpecified = true;
                        payment.presentValueAmount = MoneyHelper.Mul(payment.paymentAmount, payment.discountFactor);
                    }                   
                }
            }
        }

        #endregion
    }
}