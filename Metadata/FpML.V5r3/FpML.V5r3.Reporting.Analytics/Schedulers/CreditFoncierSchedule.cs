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
using Orion.Util.Helpers;
using Orion.Analytics.DayCounters;
using Orion.Analytics.Solvers;

#endregion

namespace Orion.Analytics.Schedulers
{
    /// <summary>
    /// The credit foncier schedule item.
    /// </summary>
    public  class CreditFoncierScheduleItem
    {
        ///<summary>
        ///</summary>
        public DateTime RollDate;
        ///<summary>
        ///</summary>
        public double NotionalAtStart;
        ///<summary>
        ///</summary>
        public double NotionalAtEnd;
        ///<summary>
        ///</summary>
        public double PrincipalPayment;
        ///<summary>
        ///</summary>
        public double InterestPayment;
        ///<summary>
        ///</summary>
        public double TotalPayment;
    }

    /// <summary>
    /// CreditFoncierSchedule
    /// </summary>
    public class CreditFoncierSchedule
    {
        /// <summary>
        /// Generates the schedule.
        /// </summary>
        /// <param name="rollDates"></param>
        /// <param name="dayCounterAsString"></param>
        /// <param name="initialPrincipal"></param>
        /// <param name="annualRate"></param>
        /// <returns></returns>
        public static List<CreditFoncierScheduleItem> GenerateSchedule(List<DateTime> rollDates, 
                                                                       string dayCounterAsString, 
                                                                       double initialPrincipal, 
                                                                       double annualRate)
        {
            var dayCounter = DayCounterHelper.Parse(dayCounterAsString);
            //  first - calculate the repayment amount (P+I) = C
            //

            // InitialPrincipal = C / df1 + C / df2 + C / df3 ....  + C / dfLast      
            //

            //  List of pairs - start & end of the period
            //
            var periodStartAndEnd = new List<Pair<DateTime, DateTime>>();
            for(var i = 0; i < rollDates.Count - 1; ++i)
            {
                periodStartAndEnd.Add(new Pair<DateTime, DateTime>(rollDates[i], rollDates[i + 1]));
            }
            //  list of discount factors
            //
            var discountFactorsAtTheEndOfPeriods = new List<double>();
            //DateTime startDate = rollDates[0];
            double compoundedDf = 1;
            foreach (var periodStartAndEndItem in periodStartAndEnd)
            {
                var yearFraction = dayCounter.YearFraction(periodStartAndEndItem.First, periodStartAndEndItem.Second);
                var periodDf = 1.0 / (1.0 + annualRate * yearFraction);
                compoundedDf *= periodDf;
                discountFactorsAtTheEndOfPeriods.Add(compoundedDf);                    
            }
            //  apply solver ...
            //
            var function = new CreditFoncierScheduleRepaymentAmountObjectiveFunction(initialPrincipal, discountFactorsAtTheEndOfPeriods);           
            //const double accuracy = 10E-10;
            var guess = initialPrincipal / 2;
            //var derivativeOfTargetFunction = new NumericalDerivative().CreateDerivativeFunctionHandle(function.Value, 1);
            var xmin = 0.00001;
            var xmax = initialPrincipal;
            //var accuracy = 10 ^ -10;
            //var repaymentAmount = MathNet.Numerics.RootFinding.NewtonRaphson.FindRoot(function.Value,
            //    derivativeOfTargetFunction, guess, xmin, xmax);
            var repaymentAmount = new Brent().Solve(function, guess, xmin, xmax);
            //double repaymentAmount = new Newton().Solve(function, accuracy, guess, 1.0);
            var result = new List<CreditFoncierScheduleItem>();
            var notionalPeriod = initialPrincipal;
            foreach (var periodStartAndEndItem in periodStartAndEnd)
            {
                var item = new CreditFoncierScheduleItem
                               {
                                   TotalPayment = repaymentAmount,
                                   RollDate = periodStartAndEndItem.First,
                                   NotionalAtStart = notionalPeriod
                               };
                //  interest 
                //
                var yearFraction = dayCounter.YearFraction(periodStartAndEndItem.First, periodStartAndEndItem.Second);
                item.InterestPayment = notionalPeriod * annualRate * yearFraction;
                item.PrincipalPayment = item.TotalPayment - item.InterestPayment;
                notionalPeriod -= item.PrincipalPayment;
                item.NotionalAtEnd = notionalPeriod;
                result.Add(item);
            }
            return result;
        }


        internal class CreditFoncierScheduleRepaymentAmountObjectiveFunction : IObjectiveFunction
        {
            private readonly double _initialNotional;
            private readonly List<double> _discountFactorsAtTheEndOfPeriods;

            public CreditFoncierScheduleRepaymentAmountObjectiveFunction(double initialNotional, List<double> discountFactorsAtTheEndOfPeriods)
            {
                _initialNotional = initialNotional;
                _discountFactorsAtTheEndOfPeriods = discountFactorsAtTheEndOfPeriods;
            }

            public double Value(double repaymentAmount)
            {
                double calculatedPresentValue = 0;
                foreach (var discountFactor in _discountFactorsAtTheEndOfPeriods)
                {
                    calculatedPresentValue += repaymentAmount * discountFactor;
                }
                return calculatedPresentValue - _initialNotional;
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
        }
    }
}