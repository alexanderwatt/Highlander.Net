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
using Highlander.Reporting.Analytics.V5r3.Rates;
using Microsoft.VisualStudio.TestTools.UnitTesting;

#endregion

namespace Highlander.Analytics.Tests.V5r3.Bonds
{
    /// <summary>
    /// 
    /// </summary>
    [TestClass]
    public class BondTests 
    {

        [TestMethod]
        public void CalculateYTM()
        {
            var ytm = BondAnalytics.CalculateBondYTM(DateTime.Today.AddMonths(9), DateTime.Today.AddMonths(-3), 0.06, 2, 100, 104);
            Debug.WriteLine($"ytm : {100 * ytm}");
        }
        
// inputs, (15/05/13, 15/11/08, .065, 2, 100, 113.27) output is 3.8% and should be around 3.75
// sorry should be around 3.7%
//
// again, 15/04/12,15/10/08, .0575, 2 ,100, 109.52 and output is 3.44%, shoudl be closer to 3.4%
// and thirdly, 15/06/11, 15/12/08, .0575, 100, 2, 107.36, output is 3.49% but should be around 3.11%

        [TestMethod]
        public void CalculateYTM1()
        {
            var ytm = BondAnalytics.CalculateBondYTM(DateTime.Parse("15/05/13"), DateTime.Parse("15/11/08"), .065, 2, 100, 113.27);

            Debug.WriteLine($"ytm : {100 * ytm}");
        }

        [TestMethod]
        public void CalculateYTM12()
        {
            var ytm = BondAnalytics.CalculateBondYTM(DateTime.Parse("15/04/12"), DateTime.Parse("15/10/08"), .0575, 2, 100, 109.52);

            Debug.WriteLine($"ytm : {100 * ytm}");
        }

        [TestMethod]
        public void CalculateYTM13()
        {
            var ytm = BondAnalytics.CalculateBondYTM(DateTime.Parse("15/06/11"), DateTime.Parse("15/12/08"), .0575, 2, 100, 107.36);

            Debug.WriteLine($"ytm : {100 * ytm}");
        }


        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestISMADirtyPrice0()
        {
            //var ytm = BondAnalytics.CalculateBondYTM(DateTime.Parse("15/05/13"), DateTime.Parse("15/11/08"), 
            //.065, 2, 100, 113.27); 3.75-TYM
            //sorry should be around 3.7%
            DateTime matDate = DateTime.Parse("15/05/13");
            DateTime lastCouponDate = DateTime.Parse("15/11/08");
            var couponRate = .065;
            double accrualYearFraction = (DateTime.Today - lastCouponDate).TotalDays / 365;
            //int finalAccrualYearFraction = 0;
            double finalAccrualYearFraction = 0.5 - accrualYearFraction;
            int numberOfCouponPeriodsInAYear = 2;
            //int i = (int)((matDate - lastCouponDate).TotalDays / 365) * numberOfCouponPeriodsInAYear;
            int i = 8;
            double nextCoupon = couponRate * 100 / numberOfCouponPeriodsInAYear;
            //double nextCoupon = 0;
            double next2Coupon = couponRate * 100 / numberOfCouponPeriodsInAYear;
            //double annualCoupon = couponRate * 100 / numberOfCouponPeriodsInAYear;
            double annualCoupon = couponRate * 100;
            //double finalCoupon = 0.0;
            double finalCoupon = couponRate * 100 * finalAccrualYearFraction;
            //double ytm = .037;
            double ytm = .037;
            double discountFactorForOnePeriod = 1 / (1 + ytm / numberOfCouponPeriodsInAYear);
            var result = BondAnalytics.ISMADirtyPrice(accrualYearFraction, 
                finalAccrualYearFraction, 
                i, 
                nextCoupon, 
                next2Coupon, 
                annualCoupon, finalCoupon, numberOfCouponPeriodsInAYear, discountFactorForOnePeriod, ytm);
            Debug.WriteLine($"dirty price : {result}");
        }

        [TestMethod]
        public void TestISMADirtyPrice11()
        {
            double accrualYearFraction = 297.0 / 365.0;
            int finalAccrualYearFraction = 0;
            int i = 8;
            double nextCoupon = 7.0;
            double next2Coupon = 7.0;
            double annualCoupon = 7.0;
            double finalCoupon = 0.0;
            int numberOfCouponPeriodsInAYear = 1;
            double ytm = .06;
            double discountFactorForOnePeriod = 1 / (1 + ytm / numberOfCouponPeriodsInAYear);
            var result = BondAnalytics.ISMADirtyPrice(accrualYearFraction, 
                finalAccrualYearFraction, 
                i, 
                nextCoupon, 
                next2Coupon, 
                annualCoupon, finalCoupon, numberOfCouponPeriodsInAYear, discountFactorForOnePeriod, ytm);
            Debug.WriteLine($"dirty price : {result}");
        }

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestISMADirtyPrice1()
        {
            var result = BondAnalytics.ISMADirtyPrice(297.0/365.0, 0, 8, 7.0, 7.0, 7.0, 0.0, 1, .9434, .06 );
            Debug.WriteLine($"dirty price : {result}");
        }

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestISMADirtyPrice2()//On coupon date, but ex div.
        {
            var result = BondAnalytics.ISMADirtyPrice(0.0, 0, 8, 0.0, 7.0, 7.0, 0.0, 1, 1/(1.07), .07);
            Debug.WriteLine($"dirty price : {result}");
        }

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestISMADirtyPrice3()
        {
            var result = BondAnalytics.ISMADirtyPrice(2.0 / 365.0, 0, 8, 7.0, 7.0, 7.0, 0.0, 1, 1 / (1.07), .07);
            Debug.WriteLine($"dirty price : {result}");
        }

        /// <summary>
        /// Testing the zero rates.
        /// </summary>
        [TestMethod]
        public void TestISMADP()
        {
            var result = BondAnalytics.ISMADP(297.0 / 365.0, 0, 8, .07, .07, .07, 0.0, 1, .9434, .06);
            Debug.WriteLine($"dirty price : {result}");
        } 
    }
}