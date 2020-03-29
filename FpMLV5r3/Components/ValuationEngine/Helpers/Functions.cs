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
using System.Collections.Generic;
using Highlander.Core.Common;
using Highlander.Reporting.Helpers.V5r3;
using Highlander.CurveEngine.V5r3.PricingStructures;
using Highlander.Utilities.Logging;
using Highlander.Reporting.V5r3;
using Highlander.Reporting.Analytics.V5r3.Solvers;
using Highlander.Reporting.ModelFramework.V5r3.PricingStructures;
using Highlander.ValuationEngine.V5r3.Pricers;

#endregion

namespace Highlander.ValuationEngine.V5r3.Helpers
{
    ///<summary>
    ///</summary>
    public class Functions
    {

        ///<summary>
        /// The accuracy of the solver.
        ///</summary>
        public static double Accuracy {get; set;}

        ///<summary>
        /// The guess of the solver.
        ///</summary>
        public static double Guess { get; set; }

        ///<summary>
        /// The step of the solver.
        ///</summary>
        public static double Step { get; set; }

        ///<summary>
        ///</summary>
        public Functions()
        {
            Accuracy = 10e-8;
            Guess = .05;
            Step = 0.00001;
        }

        /// <summary>
        /// <remarks>
        /// Always: 
        /// pay floating
        /// receive fixed
        /// </remarks>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="valueDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="terminationDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="marginAboveFloatingRate"></param>
        /// <param name="resetRate"></param>
        /// <param name="directionDateGenerationPayLeg"></param>
        /// <param name="cashFlowFrequencyPayLeg"></param>
        /// <param name="accrualMethodPayLeg"></param>
        /// <param name="holidaysPayLeg"></param>
        /// <param name="discountFactorCurvePayLeg"></param>
        /// <param name="directionDateGenerationRecLeg"></param>
        /// <param name="cashFlowFrequencyRecLeg"></param>
        /// <param name="accrualMethodRecLeg"></param>
        /// <param name="holidaysRecLeg"></param>
        /// <param name="discountFactorCurveRecLeg"></param>
        /// <returns></returns>
        public double GetSwapParRateWithoutCurves
        (
        ILogger logger, 
        ICoreCache cache,
        string nameSpace,
        DateTime valueDate,
        DateTime effectiveDate,
        DateTime terminationDate,
        string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
        double marginAboveFloatingRate,// use 0 initially
        double resetRate,
        int directionDateGenerationPayLeg,
        string cashFlowFrequencyPayLeg,
        string accrualMethodPayLeg,
        string holidaysPayLeg,
        IRateCurve discountFactorCurvePayLeg,
        int directionDateGenerationRecLeg,
        string cashFlowFrequencyRecLeg,
        string accrualMethodRecLeg,
        string holidaysRecLeg,
        IRateCurve discountFactorCurveRecLeg
        )
        {
            const decimal dummyNotional = 1000000.0m;
            //  received fixed leg
            //
            var recFixedCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate,
                                                                   interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                   dummyNotional,
                                                                   directionDateGenerationRecLeg,
                                                                   cashFlowFrequencyRecLeg,
                                                                   accrualMethodRecLeg,
                                                                   holidaysRecLeg,
                                                                   discountFactorCurveRecLeg,
                                                                   true);
            recFixedCashflows.PayingFixedRate = false;
            //  pay floating leg
            //
            var payFloatingCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate,
                                                                      interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                      dummyNotional,
                                                                      directionDateGenerationPayLeg,
                                                                      cashFlowFrequencyPayLeg,
                                                                      accrualMethodPayLeg,
                                                                      holidaysPayLeg,
                                                                      discountFactorCurvePayLeg,
                                                                      false);
            payFloatingCashflows.PayingFixedRate = false;
            var objectiveFunction = new InterestRateSwapPricer
                                       {
                                           ReceiveLeg = recFixedCashflows,
                                           PayLeg = payFloatingCashflows
                                       };
            var solver = new Brent
                             {
                                 LowerBound = -100.0,
                                 UpperBound = 100.0
                             };

            return solver.Solve(objectiveFunction, Accuracy, Guess, Step);
        }

        /// <summary>
        /// <remarks>
        /// Always: 
        /// pay floating
        /// receive fixed
        /// </remarks>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="valueDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="terminationDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="marginAboveFloatingRate"></param>
        /// <param name="resetRate"></param>
        /// <param name="directionDateGenerationPayLeg"></param>
        /// <param name="cashFlowFrequencyPayLeg"></param>
        /// <param name="accrualMethodPayLeg"></param>
        /// <param name="holidaysPayLeg"></param>
        /// <param name="discountFactorCurvePayLegAsObject"></param>
        /// <param name="directionDateGenerationRecLeg"></param>
        /// <param name="cashFlowFrequencyRecLeg"></param>
        /// <param name="accrualMethodRecLeg"></param>
        /// <param name="holidaysRecLeg"></param>
        /// <param name="discountFactorCurveRecLegAsObject"></param>
        /// <returns></returns>
        public double GetSwapParRate
        (
            ILogger logger,
            ICoreCache cache,
            string nameSpace,
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double marginAboveFloatingRate, // use 0 initially
            double resetRate,
            int directionDateGenerationPayLeg,
            string cashFlowFrequencyPayLeg,
            string accrualMethodPayLeg,
            string holidaysPayLeg,
            object[,] discountFactorCurvePayLegAsObject,
            int directionDateGenerationRecLeg,
            string cashFlowFrequencyRecLeg,
            string accrualMethodRecLeg,
            string holidaysRecLeg,
            object[,] discountFactorCurveRecLegAsObject
            )
        {
            IRateCurve discountFactorCurvePayLeg = GetDiscountFactorCurveFromObject(discountFactorCurvePayLegAsObject,
                "discountFactorCurvePayLegAsObject");
            IRateCurve discountFactorCurveRecLeg = GetDiscountFactorCurveFromObject(discountFactorCurveRecLegAsObject,
                "discountFactorCurveRecLegAsObject");
            return GetSwapParRate(logger, cache, nameSpace, valueDate, effectiveDate, terminationDate,
                interpolationMethod,
                marginAboveFloatingRate, resetRate, directionDateGenerationPayLeg, cashFlowFrequencyPayLeg,
                accrualMethodPayLeg, holidaysPayLeg,
                discountFactorCurvePayLeg, directionDateGenerationRecLeg, cashFlowFrequencyRecLeg,
                accrualMethodRecLeg, holidaysRecLeg,
                discountFactorCurveRecLeg);
        }

        /// <summary>
        /// <remarks>
        /// Always: 
        /// pay floating
        /// receive fixed
        /// </remarks>
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="valueDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="terminationDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="marginAboveFloatingRate"></param>
        /// <param name="resetRate"></param>
        /// <param name="directionDateGenerationPayLeg"></param>
        /// <param name="cashFlowFrequencyPayLeg"></param>
        /// <param name="accrualMethodPayLeg"></param>
        /// <param name="holidaysPayLeg"></param>
        /// <param name="discountFactorCurvePayLeg"></param>
        /// <param name="directionDateGenerationRecLeg"></param>
        /// <param name="cashFlowFrequencyRecLeg"></param>
        /// <param name="accrualMethodRecLeg"></param>
        /// <param name="holidaysRecLeg"></param>
        /// <param name="discountFactorCurveRecLeg"></param>
        /// <returns></returns>
        public double GetSwapParRate
        (
            ILogger logger,
            ICoreCache cache,
            string nameSpace,
            DateTime    valueDate,
            DateTime    effectiveDate,
            DateTime    terminationDate,
            string      interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double      marginAboveFloatingRate,// use 0 initially
            double      resetRate,              
            int         directionDateGenerationPayLeg,
            string      cashFlowFrequencyPayLeg,
            string      accrualMethodPayLeg,
            string      holidaysPayLeg,
            IRateCurve      discountFactorCurvePayLeg,
            int         directionDateGenerationRecLeg,
            string      cashFlowFrequencyRecLeg,
            string      accrualMethodRecLeg,
            string      holidaysRecLeg,
            IRateCurve discountFactorCurveRecLeg
            )
        {
            const decimal dummyNotional = 1000000.0m;
            //  received fixed leg
            //
            var recFixedCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate, 
                                                                   interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                   dummyNotional,
                                                                   directionDateGenerationRecLeg, 
                                                                   cashFlowFrequencyRecLeg,
                                                                   accrualMethodRecLeg, 
                                                                   holidaysRecLeg, 
                                                                   discountFactorCurveRecLeg,
                                                                   true);
            recFixedCashflows.PayingFixedRate = false;
            //  pay floating leg
            //
            var payFloatingCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate, 
                                                                      interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                      dummyNotional,
                                                                      directionDateGenerationPayLeg,
                                                                      cashFlowFrequencyPayLeg,
                                                                      accrualMethodPayLeg,
                                                                      holidaysPayLeg,
                                                                      discountFactorCurvePayLeg,
                                                                      false);
            payFloatingCashflows.PayingFixedRate = false;
            var objectiveFunction = new InterestRateSwapPricer
                                       {
                                           ReceiveLeg = recFixedCashflows,
                                           PayLeg = payFloatingCashflows
                                       };         
            var solver = new Brent
                             {
                                 LowerBound = -100.0,
                                 UpperBound = 100.0
                             };
            return solver.Solve(objectiveFunction, Accuracy, Guess, Step);
        }

        private static IRateCurve GetDiscountFactorCurveFromObject(object discountFactorCurveObject, string paramName)
        {
            IRateCurve curve;
            if (discountFactorCurveObject is IRateCurve o)
            {
                curve = o;
            }
            else
            {
                if (discountFactorCurveObject is object[,] array)
                {
                    //  extract from [ , ]
                    //               [ , ]
                    //               [ , ]
                    //               [ , ] array 
                    var discountFactorCurveObjectAsArray = array;               
                    int dim0LowerBound = discountFactorCurveObjectAsArray.GetLowerBound(0);
                    int dim1LowerBound = discountFactorCurveObjectAsArray.GetLowerBound(1);
                    int dim0UpperBound = discountFactorCurveObjectAsArray.GetUpperBound(0);
                    var listDt = new List<DateTime>();
                    var listDf = new List<double>();
                    for (int i = dim0LowerBound; i <= dim0UpperBound; ++i)
                    {
                        var dt = (DateTime)discountFactorCurveObjectAsArray[i, dim1LowerBound];
                        var df = (double)discountFactorCurveObjectAsArray[i, dim1LowerBound + 1];
                        listDt.Add(dt);
                        listDf.Add(df);
                    }
                    DateTime baseDate = listDt[0];
                    InterpolationMethod interpolation = InterpolationMethodHelper.Parse("LinearInterpolation");
                    //SimpleDFToZeroRateCurve curve = new SimpleDFToZeroRateCurve(baseDate, interpolation, false, dates, dfs);
                    curve = new SimpleDiscountFactorCurve(baseDate, interpolation, false, listDt.ToArray(), listDf.ToArray());
                }
                else
                {
                    var message = $"{paramName} is invalid";
                    throw new ArgumentException(message, paramName);
                }
            }
            return curve;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="valueDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="terminationDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="marginAboveFloatingRate"></param>
        /// <param name="resetRate"></param>
        /// <param name="notional"></param>
        /// <param name="directionDateGenerationPayLeg"></param>
        /// <param name="cashFlowFrequencyPayLeg"></param>
        /// <param name="accrualMethodPayLeg"></param>
        /// <param name="holidaysPayLeg"></param>
        /// <param name="discountFactorCurvePayLegAsObject"></param>
        /// <param name="directionDateGenerationRecLeg"></param>
        /// <param name="cashFlowFrequencyRecLeg"></param>
        /// <param name="accrualMethodRecLeg"></param>
        /// <param name="holidaysRecLeg"></param>
        /// <param name="discountFactorCurveRecLegAsObject"></param>
        /// <param name="layout">
        /// if 0 - cashflows displayed one under another, 
        /// if 1 - cashflows displayed side by side.
        /// </param>
        /// <returns>
        ///  Pay cashflows:   
        ///     
        ///     
        ///     
        ///  Rec cashflows:   
        ///     
        ///     
        /// </returns>
        public object[,] GetSwapCashflows
            (
            ILogger logger,
            ICoreCache cache,
            string nameSpace,
            DateTime    valueDate,
            DateTime    effectiveDate,
            DateTime    terminationDate,
            string      interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double      marginAboveFloatingRate,// use 0 initially
            double      resetRate,
            decimal      notional,             
            int         directionDateGenerationPayLeg,
            string      cashFlowFrequencyPayLeg,
            string      accrualMethodPayLeg,
            string      holidaysPayLeg,
            object[,]      discountFactorCurvePayLegAsObject,
            int         directionDateGenerationRecLeg,
            string      cashFlowFrequencyRecLeg,
            string      accrualMethodRecLeg,
            string      holidaysRecLeg,
            object[,]      discountFactorCurveRecLegAsObject,
            int         layout
            )
        {
            IRateCurve discountFactorCurvePayLeg = GetDiscountFactorCurveFromObject(discountFactorCurvePayLegAsObject, "discountFactorCurvePayLegAsObject");
            IRateCurve discountFactorCurveRecLeg = GetDiscountFactorCurveFromObject(discountFactorCurveRecLegAsObject, "discountFactorCurveRecLegAsObject");
            //  received fixed leg
            //
            var recFixedCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate,
                                                                   interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                   notional,
                                                                   directionDateGenerationRecLeg,
                                                                   cashFlowFrequencyRecLeg,
                                                                   accrualMethodRecLeg,
                                                                   holidaysRecLeg,
                                                                   discountFactorCurveRecLeg,
                                                                   true);
            //  pay floating leg
            //
            var payFloatingCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate,
                                                                      interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                      notional,
                                                                      directionDateGenerationPayLeg,
                                                                      cashFlowFrequencyPayLeg,
                                                                      accrualMethodPayLeg,
                                                                      holidaysPayLeg,
                                                                      discountFactorCurvePayLeg,
                                                                      false);
            var objectiveFunction = new InterestRateSwapPricer
                                       {
                                           ReceiveLeg = recFixedCashflows,
                                           PayLeg = payFloatingCashflows
                                       };
            var solver = new Brent
                             {
                                 LowerBound = -100.0,
                                 UpperBound = 100.0
                             };
            solver.Solve(objectiveFunction, Accuracy, Guess, Step);
//            bool bothLegsHaveTheSameStructure =
//                directionDateGenerationPayLeg == directionDateGenerationRecLeg &&
//                cashFlowFrequencyPayLeg == cashFlowFrequencyRecLeg &&
//                holidaysPayLeg == holidaysRecLeg;

            //if 0 - cashflows displayed one under another, 
            //if 1 - cashflows displayed side by side.
            var result = 1 == layout ? Utilities.GetCashflowsSideBySideExcelRange(payFloatingCashflows.Coupons, recFixedCashflows.Coupons) : Utilities.GetCashflowsOneUnderAnotherExcelRange(payFloatingCashflows.Coupons, recFixedCashflows.Coupons);           
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="cache"></param>
        /// <param name="nameSpace"></param>
        /// <param name="valueDate"></param>
        /// <param name="effectiveDate"></param>
        /// <param name="terminationDate"></param>
        /// <param name="interpolationMethod"></param>
        /// <param name="marginAboveFloatingRate"></param>
        /// <param name="resetRate"></param>
        /// <param name="notional"></param>
        /// <param name="directionDateGenerationPayLeg"></param>
        /// <param name="cashFlowFrequencyPayLeg"></param>
        /// <param name="accrualMethodPayLeg"></param>
        /// <param name="holidaysPayLeg"></param>
        /// <param name="rateCurvePayLeg"></param>
        /// <param name="directionDateGenerationRecLeg"></param>
        /// <param name="cashFlowFrequencyRecLeg"></param>
        /// <param name="accrualMethodRecLeg"></param>
        /// <param name="holidaysRecLeg"></param>
        /// <param name="rateCurveRecLeg"></param>
        /// <param name="layout">
        /// if 0 - cashflows displayed one under another, 
        /// if 1 - cashflows displayed side by side.
        /// </param>
        /// <returns>
        ///  Pay cashflows:   
        ///     
        ///     
        ///     
        ///  Rec cashflows:   
        ///     
        ///     
        /// </returns>
        public object[,] GetSwapCashflowsWithoutCurves
            (
            ILogger logger,
            ICoreCache cache,
            string nameSpace,
            DateTime valueDate,
            DateTime effectiveDate,
            DateTime terminationDate,
            string interpolationMethod, //1 is linear on forward rates => make sure that the right curve is provided ...
            double marginAboveFloatingRate,// use 0 initially
            double resetRate,
            decimal notional,
            int directionDateGenerationPayLeg,
            string cashFlowFrequencyPayLeg,
            string accrualMethodPayLeg,
            string holidaysPayLeg,
            IRateCurve rateCurvePayLeg,
            int directionDateGenerationRecLeg,
            string cashFlowFrequencyRecLeg,
            string accrualMethodRecLeg,
            string holidaysRecLeg,
            IRateCurve rateCurveRecLeg,
            int layout
            )
        {
            //  received fixed leg
            //
            var recFixedCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate,
                                                                   interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                   notional,
                                                                   directionDateGenerationRecLeg,
                                                                   cashFlowFrequencyRecLeg,
                                                                   accrualMethodRecLeg,
                                                                   holidaysRecLeg,
                                                                   rateCurvePayLeg,
                                                                   true);
            //  pay floating leg
            //
            var payFloatingCashflows = InterestRateSwapPricer.GenerateCashflows(logger, cache, nameSpace, null, valueDate, effectiveDate, terminationDate,
                                                                      interpolationMethod, marginAboveFloatingRate, resetRate,
                                                                      notional,
                                                                      directionDateGenerationPayLeg,
                                                                      cashFlowFrequencyPayLeg,
                                                                      accrualMethodPayLeg,
                                                                      holidaysPayLeg,
                                                                      rateCurveRecLeg,
                                                                      false);
            var objectiveFunction = new InterestRateSwapPricer
                                       {
                                           ReceiveLeg = recFixedCashflows,
                                           PayLeg = payFloatingCashflows
                                       };
            var solver = new Brent
                             {
                                 LowerBound = -100.0,
                                 UpperBound = 100.0
                             };
            solver.Solve(objectiveFunction, Accuracy, Guess, Step);
            //            bool bothLegsHaveTheSameStructure =
            //                directionDateGenerationPayLeg == directionDateGenerationRecLeg &&
            //                cashFlowFrequencyPayLeg == cashFlowFrequencyRecLeg &&
            //                holidaysPayLeg == holidaysRecLeg;

            // if 0 - cashflows displayed one under another, 
            // if 1 - cashflows displayed side by side.

            var result = 1 == layout ? Utilities.GetCashflowsSideBySideExcelRange(payFloatingCashflows.Coupons, recFixedCashflows.Coupons) : Utilities.GetCashflowsOneUnderAnotherExcelRange(payFloatingCashflows.Coupons, recFixedCashflows.Coupons);
            return result;
        }
//d_v	date	value (settlement) date	14-Feb-2000	
//d_e	date	effective date	01-Dec-2001	
//d_t	date	terminating date	01-Feb-2002	
//vlt	rate	volatility	.008	
//a	rate	mean reversion constant	.05      
    }
}