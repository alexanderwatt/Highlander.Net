/*
 Copyright (C) 2019 Alex Watt (alexwatt@hotmail.com)

 This file is part of Highlander Project https://github.com/alexanderwatt/Hghlander.Net

 Highlander is free software: you can redistribute it and/or modify it
 under the terms of the Highlander license.  You should have received a
 copy of the license along with this program; if not, license is
 available at <https://github.com/alexanderwatt/Hghlander.Net/blob/develop/LICENSE>.

 This program is distributed in the hope that it will be useful, but WITHOUT
 ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
 FOR A PARTICULAR PURPOSE.  See the license for more details.
*/

#region Usings

using System;
using FpML.V5r10.Codes;
using Math=System.Math;

#endregion

namespace FpML.V5r10.Reporting.Analytics.Rates
{
    /// <summary>
    /// Rate analytics functions.
    /// </summary>
    public static class RateAnalytics
    {
        private static double GetCompoundingPeriod(CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double frequency;
            switch (compoundingFrequencyEnum)
            {
                case CompoundingFrequencyEnum.Continuous:
                    frequency = 0;
                    break;
                case CompoundingFrequencyEnum.Daily:
                    frequency = 1 / 365d;
                    break;
                case CompoundingFrequencyEnum.Weekly:
                    frequency = 1 / 52d;
                    break;
                case CompoundingFrequencyEnum.Monthly:
                    frequency = 1 / 12d;
                    break;
                case CompoundingFrequencyEnum.Quarterly:
                    frequency = 0.25;
                    break;
                case CompoundingFrequencyEnum.SemiAnnual:
                    frequency = 0.5;
                    break;
                case CompoundingFrequencyEnum.Annual:
                    frequency = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(compoundingFrequencyEnum), "CompoundingFrequency is of an invalid type " + compoundingFrequencyEnum);
            }
            return frequency;
        }

        ///<summary>
        ///</summary>
        ///<param name="zeroRate"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingFrequency"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static double ZeroRateToDiscountFactor(double zeroRate, double yearFraction, string compoundingFrequency)
        {
            return ZeroRateToDiscountFactor(zeroRate, yearFraction, EnumParse.ToCompoundingFrequencyEnum(compoundingFrequency));
        }

        ///<summary>
        ///</summary>
        ///<param name="zeroRate"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingFrequencyEnum"></param>
        ///<returns></returns>
        ///<exception cref="NotImplementedException"></exception>
        public static double ZeroRateToDiscountFactor(double zeroRate, double yearFraction,
                                                      CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double compoundingPeriod = GetCompoundingPeriod(compoundingFrequencyEnum);
            return Highlander.Numerics.Rates.RateAnalytics.ZeroRateToDiscountFactor(zeroRate, yearFraction, compoundingPeriod);
        }

        ///<summary>
        /// Converts a discount factor to a compounding zero rate.
        ///</summary>
        ///<param name="yearFraction">The year Fraction.</param>
        ///<param name="discountFactor">The discount factor.</param>
        ///<param name="frequency">The compounding frequency. Can take: Continuous, Daily, Quarterly,
        /// Semi-Annual,SemiAnnual,Semi and Annual</param>
        ///<returns>The compounded zero rate requested.</returns>
        public static double DiscountFactorToZeroRate(double discountFactor, double yearFraction, string frequency)
        {
            var compounding = EnumParse.ToCompoundingFrequencyEnum(frequency);
            double result = DiscountFactorToZeroRate(discountFactor, yearFraction, compounding);
            return result;
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        /// <param name="startDiscountFactor"></param>
        /// <param name="endDiscountFactor"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingPeriod"></param>
        /// <returns></returns>
        public static decimal DiscountFactorToZeroRate(decimal startDiscountFactor, decimal endDiscountFactor,
            decimal yearFraction, decimal compoundingPeriod)
        {
            double discountFactor = Convert.ToDouble(endDiscountFactor / startDiscountFactor);
            double result = DiscountFactorToZeroRate(discountFactor, (double)yearFraction, (double)compoundingPeriod);
            return Convert.ToDecimal(result);
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        ///<param name="discountFactor"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingFrequencyEnum"></param>
        ///<returns></returns>
        public static double DiscountFactorToZeroRate(double discountFactor, double yearFraction,
                                                      CompoundingFrequencyEnum compoundingFrequencyEnum)
        {
            double compoundingPeriod = GetCompoundingPeriod(compoundingFrequencyEnum);
            return DiscountFactorToZeroRate(discountFactor, yearFraction, compoundingPeriod);
        }

        /// <summary>
        /// Converts a discount factor to a compounding zero rate.
        /// </summary>
        ///<param name="discountFactor"></param>
        ///<param name="yearFraction"></param>
        ///<param name="compoundingPeriod"></param>
        ///<returns></returns>
        public static double DiscountFactorToZeroRate(double discountFactor, double yearFraction,
                                                      double compoundingPeriod)
        {
            double result;
            if (compoundingPeriod == 0)
            {
                result = -Math.Log(discountFactor) / yearFraction;
            }
            else
            {
                double power = -compoundingPeriod / yearFraction;
                result = (Math.Pow(discountFactor, power) - 1.0) / compoundingPeriod;
            }
            return result;
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public static decimal TerminalWealthFromZeroRate(decimal rate, decimal yearFraction,
            string compoundingFrequency)
        {
            return TerminalWealthFromZeroRate(rate, yearFraction,
                                              EnumParse.ToCompoundingFrequencyEnum(compoundingFrequency));
        }

        /// <summary>
        /// Converts from the zero rate to a terminal wealth.
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="yearFraction"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public static decimal TerminalWealthFromZeroRate(decimal rate, decimal yearFraction,
            CompoundingFrequencyEnum compoundingFrequency)
        {
            double compoundingPeriod = GetCompoundingPeriod(compoundingFrequency);
            decimal result;
            if (compoundingPeriod == 0)
            {
                result = (decimal)Math.Exp(Convert.ToDouble(-rate * yearFraction));
            }
            else
            {
                decimal df;

                if ((double)yearFraction > compoundingPeriod)
                {
                    df = (decimal)Math.Pow(1 + compoundingPeriod * (double)rate, (double)yearFraction);
                }
                else
                {
                    df = 1 + yearFraction * rate;
                }
                result = 1 / df;
            }
            return result;
        }
    }
}
