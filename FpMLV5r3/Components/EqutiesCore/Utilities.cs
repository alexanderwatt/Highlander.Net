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
using Highlander.Utilities.Exception;

namespace Highlander.Equities
{
   
    /// <summary>
    /// 
    /// </summary>
    public enum CompoundingFrequency
    {
        /// <summary>
        /// 
        /// </summary>
        Annual = 1,
        /// <summary>
        /// 
        /// </summary>
        Quarterly = 4,
        /// <summary>
        /// 
        /// </summary>
        SemiAnnual = 2,
        /// <summary>
        /// 
        /// </summary>
        Monthly = 12,
        /// <summary>
        /// 
        /// </summary>
        Continuous = -100
    }

    /// <summary>
    /// 
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="dates"></param>
        /// <param name="amounts"></param>
        /// <param name="interpolationType"></param>
        /// <returns></returns>
        /// <exception cref="UnequalArrayLengthsException"></exception>
        /// <exception cref="UnsortedDatesException"></exception>
        /// <exception cref="DateToLargeException"></exception>
        public static double Interpolate(DateTime valueDate, DateTime[] dates, double[] amounts, string interpolationType)
        {
            //Need to check that length of dates is equal to length of amounts

            if (dates.Length != amounts.Length)
            {
                throw new UnequalArrayLengthsException("Dissimilar length: The dates[] and amounts[] must be arrays of the same length, sorted with ascending dates.");
            }
            //Will check for sorting as I pass through the array.
            double tempIntRate = 0;
            bool rateFound = false;
            int n = dates.Length;
            if (dates[0] > valueDate)
            {
                //The date is outside of range - flat line amount
                throw new DateToSmallException();
                //TODO For extrapolation remove comments below
                //return amounts[0];                
            }
            if (dates[n-1] < valueDate)
            {
                //The date is outside of range - flat line amount
                throw new DateToLargeException();
                //TODO For extrapolation remove comments below
                //return amounts[n-1];
            }
            if (dates[0] == valueDate)
            {
                rateFound = true;
                tempIntRate = amounts[0];
            }
            for (long i = 1; i < n; i++) //even if date has been found, still need to check that it's sorted.
            {
                if (dates[i] <= dates[i - 1])
                {//then the list isn't sorted and it is a problem... do something about it
                    throw new UnsortedDatesException();
                }
                if (!rateFound && dates[i] == valueDate)
                { //For the case where there is an exact match in the list
                    tempIntRate = amounts[i];
                    rateFound = true;
                    //don't break as we still have to check that the list is sorted
                }
                else if (!rateFound && dates[i] > valueDate)
                { //If we get here then the current date is the lowest one greater than the valueDate
                    rateFound = true;
                    if (interpolationType == "l")
                    {
                        tempIntRate = amounts[i - 1] + (amounts[i] - amounts[i - 1]) / (dates[i] - dates[i - 1]).TotalDays * (valueDate - dates[i - 1]).TotalDays;
                    }    //don't break as we still have to check that the list is sorted
                    else
                    {
                        return 0.0; //this is where we will put other interpolation techniques
                    }
                }
            }
            if (!rateFound)
            {
                throw new DateToLargeException();
            }
            return tempIntRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="dates"></param>
        /// <param name="amounts"></param>
        /// <returns></returns>
        public static double InterpolateDates(DateTime valueDate, DateTime[] dates, double[] amounts)
        {
            return Interpolate(valueDate, dates, amounts, "l");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public static double InterpolateValues(double xValue, double[] x, double[] y)
        {
            return Interpolate(xValue, x, y, "l");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xValue"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="interpolationType"></param>
        /// <returns></returns>
        /// <exception cref="UnequalArrayLengthsException"></exception>
        /// <exception cref="UnsortedDatesException"></exception>
        /// <exception cref="DateToLargeException"></exception>
        public static double Interpolate(double xValue,  double[] x, double[] y, string interpolationType)
        {
            //Need to check that length of dates is equal to length of amounts
            if (x.Length != y.Length)
            {
                throw new UnequalArrayLengthsException("Dissimilar length: The dates[] and amounts[] must be arrays of the same length, sorted with ascending dates.");
            }
            //Will check for sorting as I pass through the array.
            double tempIntRate = 0;
            bool rateFound = false;
            int n = x.Length;
            if (x[0] > xValue)
            {
                //The date is outside of range - flat line amount
                return y[0];
            }
            if (x[n - 1] < xValue)
            {
                //The date is outside of range - flat line amount
                return x[n - 1];
            }
            if (x[0] == xValue)
            {
                rateFound = true;
                tempIntRate = y[0];
            }
            for (long i = 1; i < n; i++) //even if date has been found, still need to check that it's sorted.
            {
                if (x[i] <= x[i - 1])
                {//then the list isn't sorted and it is a problem... do something about it
                    throw new UnsortedDatesException();
                }
                if (!rateFound && x[i] == xValue)
                { //For the case where there is an exact match in the list
                    tempIntRate = y[i];
                    rateFound = true;
                    //don't break as we still have to check that the list is sorted
                }
                else if (!rateFound && x[i] > xValue)
                { //If we get here then the current date is the lowest one greater than the valueDate
                    rateFound = true;
                    if (interpolationType == "l")
                    {
                        tempIntRate = y[i - 1] + (y[i] - y[i - 1]) / (x[i] - x[i - 1]) * (xValue - x[i - 1]);
                    }    //don't break as we still have to check that the list is sorted
                    else
                    {
                        return 0.0; //this is where we will put other interpolation techniques
                    }
                }
            }
            if (!rateFound)
            {
                throw new DateToLargeException();
            }
            return tempIntRate;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="paymentDates"></param>
        /// <param name="paymentAmounts"></param>
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <returns></returns>
        /// <exception cref="UnequalArrayLengthsException"></exception>
        public static double PVofPaymentStream(DateTime valueDate, DateTime[] paymentDates, double[] paymentAmounts,
            DateTime[] zeroDates, double[] zeroRates)
        {
            if (paymentDates.Length != paymentAmounts.Length)
            {
                throw new UnequalArrayLengthsException("paymentDates[] length is not equal to paymentAmounts[] length");
            }
            if (zeroDates.Length != zeroRates.Length)
            {
                throw new UnequalArrayLengthsException("zeroDates[] length is not equal to zeroRates[] length");
            }
            double tempPV = 0;
            for (long i = 0; i < paymentAmounts.Length; i++)
            {
                double discRate = InterpolateDates(paymentDates[i], zeroDates, zeroRates);
                double period = (paymentDates[i] - valueDate).TotalDays / 365;
                tempPV = tempPV + paymentAmounts[i] * Math.Exp(-discRate * period);
            }
            return tempPV;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="paymentDates"></param>
        /// <param name="paymentAmounts"></param>
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <param name="finalDate"></param>
        /// <returns></returns>
        /// <exception cref="UnequalArrayLengthsException"></exception>
        /// <exception cref="Exception"></exception>
        public static double PVofPaymentStream(DateTime valueDate, DateTime[] paymentDates, double[] paymentAmounts,
            DateTime[] zeroDates, double[] zeroRates, DateTime finalDate)
        {
            if (paymentDates.Length != paymentAmounts.Length)
            {
                throw new UnequalArrayLengthsException("paymentDates[] length is not equal to paymentAmounts[] length");
            }
            if (zeroDates.Length != zeroRates.Length)
            {
                throw new UnequalArrayLengthsException("zeroDates[] length is not equal to zeroRates[] length");
            }
            if (finalDate <= valueDate)
            {
                throw new Exception("finalDate must be greater than valueDate.");
            }
            double tempPV = 0;
            for (long i = 0; i < paymentAmounts.Length; i++)
            {
                if (paymentDates[i] <= finalDate && paymentDates[i] >= valueDate)
                {
                    double discRate = InterpolateDates(paymentDates[i], zeroDates, zeroRates);
                    double period = (paymentDates[i] - valueDate).TotalDays / 365;
                    tempPV = tempPV + paymentAmounts[i] * Math.Exp(-discRate * period);
                }
            }
            return tempPV;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        public static double[] ConvToContinuousRate(double[] rate, CompoundingFrequency freq)
        {
            int lngFreq = (int)freq;
            double[] tempRates = new double[rate.Length];
            if (lngFreq < 0)
            {
                return rate;
            }
            for (long i = 0; i < rate.Length; i++)
            {
                tempRates[i] = lngFreq * Math.Log(1 + rate[i] / lngFreq);
            }
            return tempRates;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="freq"></param>
        /// <returns></returns>
        public static double ConvToContinuousRate(double rate, CompoundingFrequency freq)
        {
            double[] arRate = { rate };
            var tempRate = ConvToContinuousRate(arRate, freq);
            return tempRate[0];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="strFreq"></param>
        /// <returns></returns>
        public static double ConvToContinuousRate(double rate, string strFreq)
        {
            CompoundingFrequency freq = (CompoundingFrequency)Enum.Parse(typeof(CompoundingFrequency), strFreq);
            return ConvToContinuousRate(rate, freq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="strFreq"></param>
        /// <returns></returns>
        public static double[] ConvToContinuousRate(double[] rate, string strFreq)
        {
            CompoundingFrequency freq = (CompoundingFrequency)Enum.Parse(typeof(CompoundingFrequency), strFreq);
            return ConvToContinuousRate(rate, freq);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAmount"></param>
        /// <param name="valueDate"></param>
        /// <param name="finalDate"></param>
        /// <param name="paymentDates"></param>
        /// <param name="paymentAmounts"></param>
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <returns></returns>
        public static double EquivalentYield(double baseAmount, DateTime valueDate, DateTime finalDate, DateTime[] paymentDates, double[] paymentAmounts,
            DateTime[] zeroDates, double[] zeroRates)
        {
            double tempPV = PVofPaymentStream(valueDate, paymentDates, paymentAmounts, zeroDates, zeroRates, finalDate);
            double periodYears = (finalDate - valueDate).TotalDays / 365;
            return -1 / periodYears * Math.Log((baseAmount - tempPV) / baseAmount);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="baseAmount"></param>
        /// <param name="valueDate"></param>
        /// <param name="finalDate"></param>
        /// <param name="paymentDates"></param>
        /// <param name="paymentAmounts"></param>
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <returns></returns>
        public static double[] EquivalentYield(double baseAmount, DateTime valueDate, DateTime[] finalDate, DateTime[] paymentDates, double[] paymentAmounts,
            DateTime[] zeroDates, double[] zeroRates)
        {
            double[] retArray = new double[finalDate.Length];
            for (long i = 0; i < finalDate.Length; i++)
            {
                retArray[i] = EquivalentYield(baseAmount, valueDate, finalDate[i], paymentDates, paymentAmounts, zeroDates, zeroRates);
            }
            return retArray;
        }
    }
}
