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

using System;
using System.Diagnostics;

namespace Highlander.Models.Tests.V5r3.Helpers
{
    public static class TestHelper
    {
        static readonly string _resourceFile = "Highlander.Models.Tests.V5r3.Data.SwapResources";

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static DateTime[] GetFixedCashflowDates()
        {
            string methodInstance = GetExecutingMethod();
            string cashflowDatesList = ResourceHelper.ReadResourceValue(_resourceFile, methodInstance);
            string[] cashflowDates = cashflowDatesList.Split(';');
            DateTime[] dates = new DateTime[cashflowDates.Length];
            int i = 0;
            foreach (string cashflowDate in cashflowDates)
            {
                dates[i] =  DateTime.Parse(cashflowDate);
                i++;
            }
            return dates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static decimal[] GetFixedCashflows()
        {
            string methodInstance = GetExecutingMethod();
            string cashflowsList = ResourceHelper.ReadResourceValue(_resourceFile, methodInstance);
            // string cashflowsList = "170090.9936;170691.4838;155467.4332;161683.2391;162421.4679;154681.1085;147229.8426;154625.6738;147217.6024;144791.3672;142411.4178;140076.8972;137749.7479;136924.6277;131747.6486;132414.5066;130181.0235;127984.1094;121788.2132;123767.2337;121638.9516;119542.9056;113712.3003;115511.3014;113507.6312;113915.9178;103695.0523;107740.4718;108143.9773;102905.0813;101145.8765;99414.95745;97711.12422;97071.50421;93347.354;93762.44139;91145.56342;90540.55495;86108.19705;87454.71913";
            string[] cashflows = cashflowsList.Split(';');
            var cfs = new decimal[cashflows.Length];
            int i = 0;
            foreach (string cf in cashflows)
            {
                cfs[i] = decimal.Parse(cf);
                ++i;
            }
            return cfs;
        }

        internal static decimal[] GetFloatingCashflows()
        {
            string methodInstance = GetExecutingMethod();
            string cashflowsList = ResourceHelper.ReadResourceValue(_resourceFile, methodInstance);
            //string cashflowsList = "358810.7279;310225.1266;305939.3087;291101.4252;285795.0122;275014.6438;270696.8415;260229.0627;255630.6997;243307.8199;243743.672;232622.5254;231900.1935;216467.0593;213748.0107;203594.5236;198251.7124;190926.5835;185903.3343;178046.2306";
            string[] cashflows = cashflowsList.Split(';');
            decimal[] cfs = new decimal[cashflows.Length];
            int i = 0;
            foreach (string cf in cashflows)
            {
                cfs[i] = decimal.Parse(cf);
                ++i;
            }
            return cfs;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static decimal[] GetFixedCouponYearFractions()
        {
            string methodInstance = GetExecutingMethod();
            string couponYearFractionsList = ResourceHelper.ReadResourceValue(_resourceFile, methodInstance);
            //string couponYearFractionsList = "0.25205479;0.25753425;0.23835616;0.25205479;0.25753425;0.24931507;0.24109589;0.25753425;0.24931507;0.24931507;0.24931507;0.24931507;0.24931507;0.25205479;0.24657534;0.25205479;0.25205479;0.25205479;0.24383562;0.25205479;0.25205479;0.25205479;0.24383562;0.25205479;0.25205479;0.25753425;0.23835616;0.25205479;0.25753425;0.24931507;0.24931507;0.24931507;0.24931507;0.25205479;0.24657534;0.25205479;0.24931507;0.25205479;0.24383562;0.25205479";
            string[] yearFractions = couponYearFractionsList.Split(';');
            var dYearFractions = new decimal[yearFractions.Length];
            int i = 0;
            foreach (string yearFraction in yearFractions)
            {
                dYearFractions[i] = decimal.Parse(yearFraction);
                ++i;
            }
            return dYearFractions;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        internal static decimal[] GetFloatingCouponYearFractions()
        {
            string methodInstance = GetExecutingMethod();
            string couponYearFractionsList = ResourceHelper.ReadResourceValue(_resourceFile, methodInstance);
            //string couponYearFractionsList = "0.25205479;0.25753425;0.23835616;0.25205479;0.25753425;0.24931507;0.24109589;0.25753425;0.24931507;0.24931507;0.24931507;0.24931507;0.24931507;0.25205479;0.24657534;0.25205479;0.25205479;0.25205479;0.24383562;0.25205479;0.25205479;0.25205479;0.24383562;0.25205479;0.25205479;0.25753425;0.23835616;0.25205479;0.25753425;0.24931507;0.24931507;0.24931507;0.24931507;0.25205479;0.24657534;0.25205479;0.24931507;0.25205479;0.24383562;0.25205479";
            string[] yearFractions = couponYearFractionsList.Split(';');
            var dYearFractions = new decimal[yearFractions.Length];
            int i = 0;
            foreach (string yearFraction in yearFractions)
            {
                dYearFractions[i] = decimal.Parse(yearFraction);
                ++i;
            }
            return dYearFractions;
        }

        internal static decimal[] GetFixedCurveYearFractions()
        {
            string methodInstance = GetExecutingMethod();
            string curveYearFractionsList = ResourceHelper.ReadResourceValue(_resourceFile, methodInstance);
            //string curveYearFractionsList = "0.33972603;0.59726027;0.83561644;1.08767123;1.34520548;1.59452055;1.83561644;2.09315068;2.34246575;2.59178082;2.84109589;3.09041096;3.33972603;3.59178082;3.83835616;4.09041096;4.34246575;4.59452055;4.83835616;5.09041096;5.34246575;5.59452055;5.83835616;6.09041096;6.34246575;6.60000000;6.83835616;7.09041096;7.34794521;7.59726027;7.84657534;8.09589041;8.34520548;8.59726027;8.84383562;9.09589041;9.3452054;9.59726027;9.84109589;10.09315068";
            string[] yearFractions = curveYearFractionsList.Split(';');
            decimal[] dYearFractions = new decimal[yearFractions.Length];
            int i = 0;
            foreach (string yearFraction in yearFractions)
            {
                dYearFractions[i] = decimal.Parse(yearFraction);
                ++i;
            }
            return dYearFractions;
        }


        internal static decimal[] GetFloatingCurveYearFractions()
        {
            //string methodInstance = TestHelper.GetExecutingMethod();
            //string curveYearFractionsList = ResourceHelper.ReadResourceValue(TestHelper._resourceFile, methodInstance);
            string curveYearFractionsList = "0.597260274;1.087671233;1.594520548;2.093150685;2.591780822;3.090410959;3.591780822;4.090410959;4.594520548;5.090410959;5.594520548;6.090410959;6.6;7.090410959;7.597260274;8.095890411;8.597260274;9.095890411;9.597260274;10.09315068";
            string[] yearFractions = curveYearFractionsList.Split(';');
            decimal[] dYearFractions = new decimal[yearFractions.Length];
            int i = 0;
            foreach (string yearFraction in yearFractions)
            {
                dYearFractions[i] = decimal.Parse(yearFraction);
                ++i;
            }
            return dYearFractions;
        }


        internal static decimal[] GetFixedDiscountFactors()
        {

            //string methodInstance = TestHelper.GetExecutingMethod();
            //string discountFactorsList  = ResourceHelper.ReadResourceValue(TestHelper._resourceFile, methodInstance);
            string discountFactorsList = "0.98141;0.96392;0.948587;0.932898;0.917218;0.902304;0.888117;0.873194;0.858767;0.844614;0.830731;0.817113;0.803538;0.790043;0.777065;0.76402;0.751133;0.738457;0.726394;0.714126;0.701846;0.689752;0.678226;0.66649;0.654929;0.6433;0.632697;0.621653;0.610705;0.600278;0.590016;0.579919;0.56998;0.560094;0.550575;0.541001;0.531681;0.522411;0.513584;0.504606";
            string[] discountFactors = discountFactorsList.Split(';');
            decimal[] dDiscountFactors = new decimal[discountFactors.Length];
            int i = 0;
            foreach (string df in discountFactors)
            {
                dDiscountFactors[i] = decimal.Parse(df);
                ++i;
            }
            return dDiscountFactors;
        }

        internal static decimal[] GetFloatingDiscountFactors()
        {
            //string methodInstance = TestHelper.GetExecutingMethod();
            //string discountFactorsList  = ResourceHelper.ReadResourceValue(TestHelper._resourceFile, methodInstance);
            string discountFactorsList = "0.96392;0.932898;0.902304;0.873194;0.844614;0.817113;0.790043;0.76402;0.738457;0.714126;0.689752;0.66649;0.6433;0.621653;0.600278;0.579919;0.560094;0.541001;0.522411;0.504606";
            string[] discountFactors = discountFactorsList.Split(';');
            decimal[] dDiscountFactors = new decimal[discountFactors.Length];
            int i = 0;
            foreach (string df in discountFactors)
            {
                dDiscountFactors[i] = decimal.Parse(df);
                ++i;
            }
            return dDiscountFactors;
        }

        internal static decimal[] GetFloatingRates()
        {
            string floatingRatesList = "0.07304733;0.067808267;0.066896521;0.066858259;0.067860624;0.067498664;0.068339883;0.068308154;0.068669335;0.068706134;0.070099581;0.070383753;0.070740395;0.071004131;0.070253957;0.070407712;0.070598878;0.070776619;0.070976837;0.071153237";
            string[] floatingRates = floatingRatesList.Split(';');
            decimal[] dFloatingRates = new decimal[floatingRates.Length];
            int i = 0;
            foreach (string fr in floatingRates)
            {
                dFloatingRates[i] = decimal.Parse(fr);
                ++i;
            }
            return dFloatingRates;
        }

        /// <summary>
        /// Gets the executing class.
        /// </summary>
        /// <returns></returns>
        internal static string GetExecutingClass()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            var declaringType = sf.GetMethod().DeclaringType;
            if (declaringType != null) return declaringType.Name;
            return null;
        }

        /// <summary>
        /// Gets the executing method.
        /// </summary>
        /// <returns></returns>
        internal static string GetExecutingMethod()
        {
            StackTrace st = new StackTrace();
            StackFrame sf = st.GetFrame(1);
            return sf.GetMethod().Name;
        }
    }
}