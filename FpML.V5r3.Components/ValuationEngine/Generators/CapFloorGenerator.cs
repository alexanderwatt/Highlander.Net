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

using System;
using Core.Common;
using FpML.V5r3.Reporting.Helpers;
using Orion.CalendarEngine.Helpers;
using Orion.Constants;
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
    public class CapFloorGenerator
    {
        public static CapFloor GenerateDefinition(CapFloorLegParametersRange capFloorLeg,
                                                 Schedule spreadSchedule,
                                                 Schedule capOrFloorSchedule,
                                                 NonNegativeSchedule notionalSchedule)
        {
            InterestRateStream capFloorStream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(capFloorLeg);
            InterestRateStreamHelper.SetPayerAndReceiver(capFloorStream, capFloorLeg.Payer, capFloorLeg.Receiver);
            var capFloor = new CapFloor { capFloorStream = capFloorStream };
            return capFloor;
        }

        public static CapFloor GenerateDefinition(CapFloorLegParametersRange_Old capFloorLeg,
                                                 Schedule    spreadSchedule,
                                                 Schedule    capOrFloorSchedule,
                                                 NonNegativeSchedule notionalSchedule)
        {
            InterestRateStream capFloorStream = InterestRateStreamParametricDefinitionGenerator.GenerateStreamDefinition(capFloorLeg);
            InterestRateStreamHelper.SetPayerAndReceiver(capFloorStream, capFloorLeg.Payer, capFloorLeg.Receiver);
            var capFloor = new CapFloor {capFloorStream = capFloorStream};
            return capFloor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="capFloorLeg"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="capOrFloorSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <returns></returns>
        public static CapFloor GenerateDefinitionCashflows(ILogger logger, ICoreCache cache,
            string nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            CapFloorLegParametersRange capFloorLeg,
            Schedule spreadSchedule,
            Schedule capOrFloorSchedule,
            NonNegativeAmountSchedule notionalSchedule)
        {
            if (paymentCalendar == null)
            {
                if (!string.IsNullOrEmpty(capFloorLeg.PaymentCalendar))
                {
                    var payCalendar = BusinessCentersHelper.Parse(capFloorLeg.PaymentCalendar);
                    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, payCalendar, nameSpace);
                }
            }
            if (fixingCalendar == null)
            {
                if (!string.IsNullOrEmpty(capFloorLeg.FixingCalendar))
                {
                    var fixCalendar = BusinessCentersHelper.Parse(capFloorLeg.FixingCalendar);
                    fixingCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, fixCalendar, nameSpace);
                }
            }
            CapFloor capFloor = GenerateDefinition(capFloorLeg, spreadSchedule, capOrFloorSchedule, notionalSchedule);
            if (null != spreadSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(capFloor.capFloorStream, spreadSchedule);
            }
            if (null != notionalSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(capFloor.capFloorStream, notionalSchedule);
            }
            if (null != capOrFloorSchedule)
            {
                if (capFloorLeg.CapOrFloor == CapFloorType.Cap)
                {
                    InterestRateStreamParametricDefinitionGenerator.SetCapRateSchedule(capFloor.capFloorStream, capOrFloorSchedule, true);
                }
                else
                {
                    InterestRateStreamParametricDefinitionGenerator.SetFloorRateSchedule(capFloor.capFloorStream, capOrFloorSchedule, true);
                }
            }
            capFloor.capFloorStream.cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(capFloor.capFloorStream, fixingCalendar, paymentCalendar);
            return capFloor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="capFloorLeg"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="capOrFloorSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <returns></returns>
        public static CapFloor GenerateDefinitionCashflows(IBusinessCalendar fixingCalendar,
                                                            IBusinessCalendar paymentCalendar,
                                                            CapFloorLegParametersRange_Old capFloorLeg,                                                          
                                                            Schedule spreadSchedule,
                                                            Schedule capOrFloorSchedule,                                    
                                                            NonNegativeAmountSchedule notionalSchedule)

        {
            CapFloor capFloor = GenerateDefinition(capFloorLeg, spreadSchedule, capOrFloorSchedule, notionalSchedule);
            if (null != spreadSchedule) 
            {
                InterestRateStreamParametricDefinitionGenerator.SetSpreadSchedule(capFloor.capFloorStream, spreadSchedule);
            }
            if (null != notionalSchedule)
            {
                InterestRateStreamParametricDefinitionGenerator.SetNotionalSchedule(capFloor.capFloorStream, notionalSchedule);
            }
            if (null != capOrFloorSchedule)
            {
                if (capFloorLeg.CapOrFloor == CapFloorType.Cap)
                {
                    InterestRateStreamParametricDefinitionGenerator.SetCapRateSchedule(capFloor.capFloorStream, capOrFloorSchedule, true);
                }
                else 
                {
                    InterestRateStreamParametricDefinitionGenerator.SetFloorRateSchedule(capFloor.capFloorStream, capOrFloorSchedule, true);
                }
            }
            capFloor.capFloorStream.cashflows = FixedAndFloatingRateStreamCashflowGenerator.GetCashflows(capFloor.capFloorStream, fixingCalendar, paymentCalendar);
            return capFloor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="capFloorLeg"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="capOrFloorSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <returns></returns>
        public static CapFloor GenerateDefinitionCashflowsAmounts(ILogger logger, 
            ICoreCache cache, String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            CapFloorLegParametersRange capFloorLeg,
            Schedule spreadSchedule,
            Schedule capOrFloorSchedule,
            NonNegativeAmountSchedule notionalSchedule)
        {
            var capFloor = GenerateDefinitionCashflows(logger, cache, nameSpace, fixingCalendar, paymentCalendar, capFloorLeg, spreadSchedule, capOrFloorSchedule, notionalSchedule);
            return capFloor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="capFloorLeg"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="capOrFloorSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <param name="marketEnvironment"></param>
        /// <param name="valuationDate"></param>
        /// <returns></returns>
        public static CapFloor GenerateDefinitionCashflowsAmounts(ILogger logger, 
            ICoreCache cache, String nameSpace,
            IBusinessCalendar fixingCalendar,
            IBusinessCalendar paymentCalendar,
            CapFloorLegParametersRange capFloorLeg,
            Schedule spreadSchedule,
            Schedule capOrFloorSchedule,
            NonNegativeAmountSchedule notionalSchedule,
            ISwapLegEnvironment marketEnvironment,
            DateTime valuationDate)
        {
            CapFloor capFloor = GenerateDefinitionCashflows(logger, cache, nameSpace, fixingCalendar, paymentCalendar, capFloorLeg, spreadSchedule, capOrFloorSchedule, notionalSchedule);
            IRateCurve payStreamDiscountingCurve = marketEnvironment.GetDiscountRateCurve();
            IRateCurve payStreamForecastCurve = marketEnvironment.GetForecastRateCurve();
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(capFloor.capFloorStream, payStreamForecastCurve, payStreamDiscountingCurve, valuationDate);
            return capFloor;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fixingCalendar"></param>
        /// <param name="paymentCalendar"></param>
        /// <param name="capFloorLeg"></param>
        /// <param name="spreadSchedule"></param>
        /// <param name="capOrFloorSchedule"></param>
        /// <param name="notionalSchedule"></param>
        /// <param name="marketEnvironment"></param>
        /// <param name="valuationDate"></param>
        /// <returns></returns>
        public static CapFloor GenerateDefinitionCashflowsAmounts(IBusinessCalendar fixingCalendar,
                                                                    IBusinessCalendar paymentCalendar,
                                                                    CapFloorLegParametersRange_Old capFloorLeg,
                                                                    Schedule spreadSchedule,
                                                                    Schedule capOrFloorSchedule,
                                                                    NonNegativeAmountSchedule notionalSchedule,
                                                                    ISwapLegEnvironment marketEnvironment,
                                                                    DateTime valuationDate)
        {
            CapFloor capFloor = GenerateDefinitionCashflows(fixingCalendar, paymentCalendar, capFloorLeg, spreadSchedule, capOrFloorSchedule, notionalSchedule);
            IRateCurve payStreamDiscountingCurve = marketEnvironment.GetDiscountRateCurve();
            IRateCurve payStreamForecastCurve = marketEnvironment.GetForecastRateCurve();
            FixedAndFloatingRateStreamCashflowGenerator.UpdateCashflowsAmounts(capFloor.capFloorStream, payStreamForecastCurve, payStreamDiscountingCurve, valuationDate);
            return capFloor;
        }

        public static void UpdatePaymentsAmounts(   IBusinessCalendar paymentCalendar,
                                                    CapFloor capFloor,
                                                    CapFloorLegParametersRange capFloorLeg,
                                                    IRateCurve discountCurve,
                                                    DateTime valuationDate)
        {
            foreach (Payment payment in capFloor.additionalPayment)
            {
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
}