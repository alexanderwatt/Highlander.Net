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
using System;
using System.Linq;
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CalendarEngine.V5r3.Helpers;
using Highlander.Utilities.Helpers;
using Highlander.Utilities.Logging;
using Highlander.Reporting.V5r3;

using Highlander.ValuationEngine.V5r3.Helpers;
using Highlander.ValuationEngine.V5r3.Generators;
using Highlander.CurveEngine.V5r3.PricingStructures.Curves;
using Highlander.Reporting.ModelFramework.V5r3;
using Math = System.Math;

#endregion

namespace Highlander.ValuationEngine.V5r3.Pricers
{
    public class BillSwapPricerCashflowRow
    {
        public DateTime DateTime;
        public double ForwardRate;
        public double MaturityValue;
        public double PurchaseCost;
        public double BillSwapCashflow;
    }

    public class BillSwapPricerDateNotional
    {
        public DateTime DateTime;
        public double   MaturityValue;
    }

    public class BillSwapPricerDatesRange
    {
        public DateTime StartDate;
        public DateTime EndDate;
        public DateTime FirstRegularPeriodStartDate;
        public string   RollConvention;
        public string   RollFrequency;
        public string   Calendar;
        public string   BusinessDayConvention;
        public string InitialStubPeriod;
        public string FinalStubPeriod;
    }

    public class BillSwapPricer
    {

        public List<DateTime> BuildDates(ILogger logger, ICoreCache cache, String nameSpace, BillSwapPricerDatesRange billSwapPricerDatesRange, IBusinessCalendar paymentCalendar)
        {
            CalculationPeriodFrequency frequency = CalculationPeriodFrequencyHelper.Parse(billSwapPricerDatesRange.RollFrequency, billSwapPricerDatesRange.RollConvention);
            StubPeriodTypeEnum? initialStub = null;
            if (!String.IsNullOrEmpty(billSwapPricerDatesRange.InitialStubPeriod))
            {
                initialStub = EnumHelper.Parse<StubPeriodTypeEnum>(billSwapPricerDatesRange.InitialStubPeriod);
            }
            StubPeriodTypeEnum? finalStub = null;
            if (!String.IsNullOrEmpty(billSwapPricerDatesRange.FinalStubPeriod))
            {
                finalStub = EnumHelper.Parse<StubPeriodTypeEnum>(billSwapPricerDatesRange.FinalStubPeriod);
            }
            //BusinessDayAdjustments adjustments = BusinessDayAdjustmentsHelper.Create(BusinessDayConventionEnum.NONE, "");
            //if (paymentCalendar == null)
            //{
            //    paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, adjustments.businessCenters);//Make sure this builds a valid calendar!
            //}
            BusinessDayAdjustments calculationPeriodDayAdjustments = BusinessDayAdjustmentsHelper.Create(billSwapPricerDatesRange.BusinessDayConvention, billSwapPricerDatesRange.Calendar);
            if (paymentCalendar == null)
            {
                paymentCalendar = BusinessCenterHelper.ToBusinessCalendar(cache, calculationPeriodDayAdjustments.businessCenters, nameSpace);
            }
            CalculationPeriodsPrincipalExchangesAndStubs result = CalculationPeriodGenerator.GenerateAdjustedCalculationPeriods(
                billSwapPricerDatesRange.StartDate, 
                billSwapPricerDatesRange.EndDate, 
                billSwapPricerDatesRange.FirstRegularPeriodStartDate, 
                frequency,
                calculationPeriodDayAdjustments,
                initialStub,
                finalStub,
                paymentCalendar
                );          
            foreach (CalculationPeriod regularCalculationPeriod in result.GetRegularAndStubPeriods())
            {
                // Adjust both startDate & endDate of period
                //
                CalculationPeriodHelper.SetAdjustedDates(regularCalculationPeriod,
                                                         AdjustedDateHelper.ToAdjustedDate(paymentCalendar, regularCalculationPeriod.unadjustedStartDate, calculationPeriodDayAdjustments),
                                                         AdjustedDateHelper.ToAdjustedDate(paymentCalendar, regularCalculationPeriod.unadjustedEndDate, calculationPeriodDayAdjustments));

            }
            var listResult = result.GetRegularAndStubPeriods().Select(regularCalculationPeriod => regularCalculationPeriod.adjustedStartDate).ToList();
            listResult.Add(result.GetRegularAndStubPeriods()[result.GetRegularAndStubPeriods().Count - 1].adjustedEndDate);
            return listResult;
        }

        public List<BillSwapPricerCashflowRow> PopulateForwardRates(List<BillSwapPricerDateNotional> input, IDayCounter dayCounter, RateCurve rateCurve)
        {
            var result = new List<BillSwapPricerCashflowRow>();
            for (int i = 0; i < input.Count; ++i)
            {
                BillSwapPricerDateNotional currentItem = input[i];
                double forwardRate = 0;
                if (i !=  input.Count - 1)
                {
                    BillSwapPricerDateNotional nextItem = input[i + 1];
                    forwardRate = rateCurve.GetForwardRate(currentItem.DateTime, nextItem.DateTime, dayCounter);
                }
                var resultItem = new BillSwapPricerCashflowRow
                                     {
                                         DateTime = currentItem.DateTime,
                                         MaturityValue = currentItem.MaturityValue,
                                         ForwardRate = forwardRate
                                     };

                result.Add(resultItem);
            }
            return result;
        }

        public List<BillSwapPricerCashflowRow> PopulatePurchaseCosts(List<BillSwapPricerCashflowRow> input, IDayCounter dayCounter, RateCurve rateCurve)
        {
            for (int i = 0; i < input.Count - 1; ++i)
            {
                BillSwapPricerCashflowRow currentItem = input[i];
                BillSwapPricerCashflowRow nextItem = input[i + 1];
                double yearFraction = dayCounter.YearFraction(currentItem.DateTime, nextItem.DateTime);
                double purchaseCost = currentItem.MaturityValue / (1.0 + currentItem.ForwardRate * yearFraction);
                currentItem.PurchaseCost = purchaseCost;
            }
            input[0].MaturityValue = 0;//no maturity value inflow at the settlement day.
            return input;
        }

        public double GetSimpleYield(List<BillSwapPricerCashflowRow> input, IDayCounter dayCounter, RateCurve rateCurve)
        {
            double yield = 0;
            for (int i = 0; i < input.Count - 1; ++i)
            {
                BillSwapPricerCashflowRow currentItem = input[i];
                BillSwapPricerCashflowRow nextItem = input[i + 1];
                double yearFraction = dayCounter.YearFraction(currentItem.DateTime, nextItem.DateTime);
                if (yield == 0)
                {
                    yield = (1.0 + currentItem.ForwardRate * yearFraction);
                }
                else
                {
                    yield *= (1.0 + currentItem.ForwardRate * yearFraction);
                }
            }
            return yield - 1.0;
        }

        public double GetAnnualYield(List<BillSwapPricerCashflowRow> input, IDayCounter dayCounter, RateCurve rateCurve)
        {
            double simpleYield = GetSimpleYield(input, dayCounter, rateCurve);
            double yearFraction = dayCounter.YearFraction(input[0].DateTime, input[input.Count - 1].DateTime);
            double annuallyCompoundingRate = Math.Pow(1.0 + simpleYield, 1 / yearFraction) - 1.0;
            return annuallyCompoundingRate;
        }
    }
}