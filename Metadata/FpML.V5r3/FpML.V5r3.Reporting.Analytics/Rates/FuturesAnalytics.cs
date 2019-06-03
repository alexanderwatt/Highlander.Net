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
using Orion.Analytics.Dates;

namespace Orion.Analytics.Rates
{
    /// <summary>
    /// A simple model for futures convexity correction.
    /// </summary>
    public static class FuturesAnalytics
    {
        /// <summary>
        /// Evaluates the implied quote.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public static Decimal FuturesMarginConvexityAdjustment(Decimal rate, double timeToExpiry, double volatility)
        {
            if (timeToExpiry == 0 || volatility==0 || rate <= 0)
            {
                return 0.0m;
            }
            try
            {
                var a = (double)rate;
                var x = Math.Pow(volatility, 2);
                var y = Math.Exp(x * timeToExpiry) - 1;
                var factor1 = 2 * y;
                var factor2 = timeToExpiry * a - 1;
                var factor3 = Math.Pow(factor2, 2);
                var factor4 = 4 * a / x * y;
                var adjustedRate = x / factor1 * (factor2 + Math.Sqrt(factor3 + factor4));
                return rate - (decimal)adjustedRate;
            }
            catch (Exception ex)
            {
                throw new Exception("Real solution does not exist", ex);
            }
        }

        /// <summary>
        /// Evaluates the implied futures quote from a provided convexity adjusted forward.
        /// This currently only works for margined futures without an arrears adjustment.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public static Decimal FuturesImpliedQuoteFromMarginAdjusted(Decimal impliedRate, double timeToExpiry, double volatility)
        {
            if (timeToExpiry == 0 || volatility == 0 || impliedRate <= 0)
            {
                return impliedRate;
            }
            try
            {
                //Analytical Solution.
                var vol = volatility;
                var v = Math.Pow(vol, 2);
                var t = timeToExpiry;
                var x = Math.Exp(v * t);
                var a = v / (2 * (x - 1));
                var q = (double)impliedRate / a + 1;
                var b = 4 * (x - 1) / v;
                var rate = (1 - q * q) / (2 * t - 2 * q * t - b);
                return (decimal)rate;
            }
            catch (Exception ex)
            {
                throw new Exception("Real solution does not exist", ex);
            }
        }

        /// <summary>
        /// Evaluates the implied futures quote from a provided convexity adjusted forward.
        /// This currently only works for margined futures without an arrears adjustment.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public static Decimal FuturesImpliedQuoteWithArrears(Decimal impliedRate, double yearFraction, double timeToExpiry, double volatility)
        {
            if (timeToExpiry == 0 || volatility == 0 || impliedRate <= 0)
            {
                return impliedRate;
            }
            try
            {
                //Analytical Solution.
                var vol = volatility;
                var v = Math.Pow(vol, 2);
                var t = timeToExpiry;
                var d = yearFraction;
                var x = Math.Exp(v * t);
                var a = 1 / (2 * d * x);
                var q = (double)impliedRate / a + 1;
                var b = 4 * d * x;
                var rate = (1 - q * q) / (2 * d - 2 * q * d - b);
                return (decimal)rate;
            }
            catch (Exception ex)
            {
                throw new Exception("Real solution does not exist", ex);
            }
        }

        /// <summary>
        /// Evaluates the implied futures quote from a provided convexity adjusted forward.
        /// </summary>
        /// <param name="impliedRate"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="yearFraction"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public static Decimal FuturesImpliedQuoteFromMarginAdjustedWithArrears(Decimal impliedRate, double yearFraction, double timeToExpiry, double volatility)
        {
            if (timeToExpiry == 0 || volatility == 0 || impliedRate <= 0)
            {
                return impliedRate;
            }
            try
            {
                var r = FuturesImpliedQuoteFromMarginAdjusted(impliedRate, timeToExpiry, volatility);
                //Analytical Solution.
                var vol = volatility;
                var v = Math.Pow(vol, 2);
                var t = timeToExpiry;
                var d = yearFraction;
                var x = Math.Exp(v * t);
                var a = 1 / (2 * d * x );
                var q = (double)r / a + 1;
                var b = 4 * d * x;
                var rate = (1 - q * q) / (2 * d - 2 * q * d - b);
                return (decimal)rate;
            }
            catch (Exception ex)
            {
                throw new Exception("Real solution does not exist", ex);
            }
        }

        /// <summary>
        /// Evaluates the futures arrears convexity adjustment.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public static Decimal FuturesArrearsConvexityAdjustment(Decimal rate, double yearFraction, double timeToExpiry, double volatility)
        {
            if (timeToExpiry == 0 || volatility == 0 || rate <= 0)
            {
                return 0.0m;
            }
            try
            {
                var a = (double)rate;
                var x = volatility;
                var t = timeToExpiry;
                var y = Math.Exp(Math.Pow(x, 2) * t);
                var z = yearFraction;
                var factor1 = 2 * z * y;
                var factor2 = z * a - 1;
                var factor3 = Math.Pow(factor2, 2);
                var factor4 = factor1 * 2 * a;
                var adjustedRate = (decimal)(1 / factor1 * (factor2 + Math.Sqrt(factor3 + factor4)));
                return rate - adjustedRate ;
            }
            catch (Exception ex)
            {
                throw new Exception("Real solution does not exist", ex);
            }
        }

        /// <summary>
        /// Evaluates the futures arrears convexity adjustment.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="timeToExpiry"></param>
        /// <param name="volatility"></param>
        /// <returns></returns>
        public static Decimal FuturesMarginWithArrearsConvexityAdjustment(Decimal rate, double yearFraction, double timeToExpiry, double volatility)
        {
            if (timeToExpiry == 0 || volatility == 0 || rate <= 0)
            {
                return 0.0m;
            }
            var arrearsAdjustment = FuturesArrearsConvexityAdjustment(rate, yearFraction, timeToExpiry, volatility);
            var marginAdjustment = FuturesMarginConvexityAdjustment(rate - arrearsAdjustment, timeToExpiry, volatility);
            return arrearsAdjustment + marginAdjustment;
        }

        ///<summary>
        ///</summary>
        ///<param name="valueDate"></param>
        ///<param name="effectiveDate"></param>
        ///<param name="terminationDate"></param>
        ///<param name="volatility"></param>
        ///<param name="a"></param>
        ///<returns></returns>
        public static double HullWhiteConvexityAdjustment(DateTime valueDate,
                                                    DateTime effectiveDate,
                                                    DateTime terminationDate,
                                                    double volatility,
                                                    double a)
        {
            double t1 = (effectiveDate - valueDate).TotalDays / 365.00;
            double t2 = (terminationDate - valueDate).TotalDays / 365.00;
            double var1 = B(a, t1, t2) / (t2 - t1);
            double var2 = B(a, t1, t2) * (1.0 - Math.Exp(-2.0 * a * t1));
            double var3 = 2.0 * a * B(a, 0.0, t1) * B(a, 0.0, t1);
            double result = var1 * (var2 + var3) * (volatility * volatility / (4.0 * a));
            return result;
        }

        private static double B(double a, double t, double T)
        {
            return (1.0 - Math.Exp(-a * (T - t))) / a;
        }

        public static double SFEConvexityAdjustment(double futuresPrice, double absoluteVol, double meanReversion, DateTime valueDate, DateTime expiryMonthYear, Object[,] holidays)
        {
            DateTime[] dates = DateHelper.SFEBillDates(expiryMonthYear, holidays);
            TimeSpan span = dates[1].Date - valueDate.Date;
            double T = span.Days;
            T = T / 365;
            const double minusT = 0.25;
            //double Lambda, Lambda1, Lambda2, Lambda3;
            //Lambda1 = Math.Pow(AbsoluteVol, 2);
            //Lambda2 = (1 - Math.Exp(-2 * MeanReversion * T)) / (2* MeanReversion);
            //Lambda3 = Math.Pow((1 - Math.Exp(-MeanReversion * T_minus_t)) / MeanReversion, 2);
            //Lambda = Lambda1 * Lambda2 * Lambda3;
            double phi1 = Math.Pow(absoluteVol, 2) / 2;
            double phi2 = (1 - Math.Exp(-meanReversion * minusT)) / meanReversion;
            double phi3 = Math.Pow((1 - Math.Exp(-meanReversion * T)) / meanReversion, 2);
            double phi = phi1 * phi2 * phi3;
            //z = Lambda + Phi;
            double z = phi;
            return (1 - Math.Exp(-z)) * ((100 - futuresPrice) / 100 + 1 / minusT);
        }

        public static double SFEBondPrice(string contract, double futuresPrice)
        {
            double y = (100 - futuresPrice) / 100;
            int n = 0;
            if (contract == "3YR") n = 6;
            if (contract == "10YR") n = 20;
            double price = (Math.Pow(1 + y / 2, -n) + 0.03 * (1 - Math.Pow(1 + y / 2, -n)) / (y / 2)) * 100000;
            return price;
        }

        public static double SFEBondTickValue(string contract, double futuresPrice)
        {
            double tickVal = Math.Abs(SFEBondPrice(contract, futuresPrice - 0.01) - SFEBondPrice(contract, futuresPrice));
            return tickVal;
        }

        public static double NZSFEBondPrice(string contract, double futuresPrice)
        {
            double y = (100 - futuresPrice) / 100;
            int n = 0;
            if (contract == "3YR") n = 6;
            if (contract == "10YR") n = 20;
            double price = (Math.Pow(1 + y / 2, -n) + 0.04 * (1 - Math.Pow(1 + y / 2, -n)) / (y / 2)) * 100000;
            return price;
        }

        public static double NZSFEBondTickValue(string contract, double futuresPrice)
        {
            double tickVal = Math.Abs(NZSFEBondPrice(contract, futuresPrice - 0.01) - NZSFEBondPrice(contract, futuresPrice));
            return tickVal;
        }

        public static double SFEBillPrice(double futuresPrice)
        {
            double y = (100 - futuresPrice) / 100;
            double price = Math.Round(1000000 / (1 + y * 90 / 365), 2);

            return price;
        }

        public static double SFEBillTickValue(double futuresPrice)
        {
            double tickVal = Math.Abs(SFEBillPrice(futuresPrice + 0.01) - SFEBillPrice(futuresPrice));
            return tickVal;
        }

        public static double CMEConvexityAdjustment(double futuresPrice, double absoluteVol, double meanReversion, DateTime valueDate, DateTime expiryMonthYear, Object[,] holidays)
        {
            DateTime[] dates = DateHelper.CMEEuroDates(expiryMonthYear, holidays);
            TimeSpan span = (dates[1].Date - valueDate.Date);
            double T = span.Days;
            T = T / 365;
            const double minusT = 0.25;
            double lambda1 = Math.Pow(absoluteVol, 2);
            double lambda2 = (1 - Math.Exp(-2 * meanReversion * T)) / (2 * meanReversion);
            double lambda3 = Math.Pow((1 - Math.Exp(-meanReversion * minusT)) / meanReversion, 2);
            double lambda = lambda1 * lambda2 * lambda3;
            double phi1 = Math.Pow(absoluteVol, 2) / 2;
            double phi2 = (1 - Math.Exp(-meanReversion * minusT)) / meanReversion;
            double phi3 = Math.Pow((1 - Math.Exp(-meanReversion * T)) / meanReversion, 2);
            double phi = phi1 * phi2 * phi3;
            double z = lambda + phi;
            return (1 - Math.Exp(-z)) * ((100 - futuresPrice) / 100 + 1 / minusT);
        }
    }
}