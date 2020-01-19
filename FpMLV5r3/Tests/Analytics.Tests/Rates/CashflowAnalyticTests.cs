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

#region Using Directives

using System;
using System.Diagnostics;
using System.Linq;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Rates
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class CashflowAnalyticTests 
    {
        /// <summary>
        /// Testing the npv.
        /// </summary>
        [TestMethod]
        public void TestCashflowNPV()
        {
            for (var i = 1; i <= 10; i++)
            {
                var npv = CashflowAnalytics.CashflowNPV(10000000.0m, (Decimal)Math.Exp(-0.05 * i));
                Debug.WriteLine($"npv : {npv} Time: {i}");
            }
        }

        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestCashflowDelta1()
        {
            for (var i = 1; i <= 10; i++)
            {
                var delta1 = CashflowAnalytics.CashflowDelta1(10000000.0m, 0.25m, i, 0.05m, (Decimal)Math.Exp(-0.05 * i));
                Debug.WriteLine($"Delta1 : {delta1} Time: {i}");
            }
        }

        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestCashflowDelta12()
        {
            var delta1 = CashflowAnalytics.CashflowBucketDelta12(10000000.0m, 0.25m, 10.0m, 0.05m, (Decimal)Math.Exp(-0.05 * 10.0));
            var index = 0;
            foreach(var delta in delta1)
            {
                Debug.WriteLine($"Delta1 : {delta} Time: {index * 0.25m}");
                index++;
            }
            Debug.WriteLine($"TotalDelta1 : {delta1.Sum()}");
        }

        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestCashflowDelta1CCR()
        {
            for (var i = 1; i <= 10; i++)
            {
                var delta1 = CashflowAnalytics.CashflowDelta1CCR(10000000.0m, i, (Decimal)Math.Exp(-0.05 * i));
                Debug.WriteLine($"Delta1 : {delta1} Time: {i}");
            }
        }

        /// <summary>
        /// Testing the gamma1.
        /// </summary>
        [TestMethod]
        public void TestCashflowGamma1CCR()
        {
            for (var i = 1; i <= 10; i++)
            {
                var delta1 = CashflowAnalytics.CashflowGamma1CCR(10000000.0m, i, (Decimal)Math.Exp(-0.05 * i));
                Debug.WriteLine($"Delta1 : {delta1} Time: {i}");
            }
        }

        /// <summary>
        /// Testing the npv.
        /// </summary>
        [TestMethod]
        public void TestInArrearsCouponNPV()
        {
            for (var i = 1; i <= 10; i++)
            {
                var df = (decimal) Math.Exp(-0.05*i/2);
                var npv = CashflowAnalytics.InArrearsCouponNPV(1000000.0m, 0.5m, 0.05m, df);
                Debug.WriteLine($"npv : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the deltar.
        /// </summary>
        [TestMethod]
        public void TestInArrearsCouponDeltaR()
        {
            for (var i = 1; i <= 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.InArrearsCouponDeltaR(1000000.0m, 0.5m, df);
                Debug.WriteLine($"deltar : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the deltar.
        /// </summary>
        [TestMethod]
        public void TestInArrearsCouponDelta0()
        {
            for (var i = 1; i <= 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.InArrearsCouponDelta0(1000000.0m, 0.5m, df);
                Debug.WriteLine($"deltar : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestInArrearsCouponAccrualFactor()
        {
            for (var i = 1; i <= 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.InArrearsCouponAccrualFactor(1000000.0m, 0.5m, df);
                Debug.WriteLine($"deltar : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestInArrearsCouponDelta1CCR()
        {
            for (var i = 1; i <= 10; i++)
            {
                var df = (Decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.InArrearsCouponDelta1CCR(1000000.0m, 0.5m, 0.05m, i / 2, df);
                Debug.WriteLine($"deltar : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestInArrearsCouponDelta12()
        {
            var delta1 = CashflowAnalytics.InArrearsCouponBucketDelta12(1000000.0m, 0.5m, 0.05m, 0.5m, 5.0m, 0.05m, (Decimal)Math.Exp(-0.05 * 5.0));
            var index = 0;
            foreach (var delta in delta1)
            {
                Debug.WriteLine($"Delta1 : {delta} Time: {index * 0.5m}");
                index++;
            }
            Debug.WriteLine($"TotalDelta1 : {delta1.Sum()}");
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFloatingCouponNPV()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFloatingCouponNPV(1000000.0m, 0.5m, 0.05m, df);
                Debug.WriteLine($"deltar : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFloatingCouponDelta0()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFloatingCouponDelta0(1000000.0m, 0.5m, 0.05m, df);
                Debug.WriteLine($"delta0 : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta1.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFloatingCouponDelta1()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFloatingCouponDelta1(1000000.0m, 0.5m, 0.05m, i / 2, df);
                Debug.WriteLine($"delta0 : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFixedCouponNPV()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFixedCouponNPV(1000000.0m, 0.5m, 0.05m, 0.05m, df);
                Debug.WriteLine($"deltar : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFixedCouponDelta0()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFixedCouponDelta0(1000000.0m, 0.5m, 0.05m, 0.05m, df);
                Debug.WriteLine($"delta0 : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFixedCouponAccrualFactor()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFixedCouponAccrualFactor(1000000.0m, 0.5m, 0.05m, df);
                Debug.WriteLine($"delta0 : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFixedCouponDelta1()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFixedCouponDelta1(1000000.0m, 0.5m, 0.05m, 0.05m, i / 2, df);
                Debug.WriteLine($"delta0 : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }

        /// <summary>
        /// Testing the delta0.
        /// </summary>
        [TestMethod]
        public void TestISDADiscountedFixedCouponDeltaR()
        {
            for (var i = 0; i < 10; i++)
            {
                var df = (decimal)Math.Exp(-0.05 * i / 2);
                var npv = CashflowAnalytics.ISDADiscountedFixedCouponDeltaR(1000000.0m, 0.5m, 0.05m, df);
                Debug.WriteLine($"delta0 : {npv} Time: {i} Df: {df} YearFraction: {0.5m}");
            }
        }
    }
}