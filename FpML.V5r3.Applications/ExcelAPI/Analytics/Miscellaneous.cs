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

#region Using Directives

using System;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.Dates;
using Orion.Analytics.Helpers;
using Orion.Analytics.Interpolations;
using Orion.Analytics.Options;
using Orion.Analytics.Rates;
using Orion.Util.Helpers;
using Excel = Microsoft.Office.Interop.Excel;

#endregion

namespace HLV5r3.Analytics
{
    ///<summary>
    /// A port of Barra's functions. The spreed is impacted by using ExcelAPI.
    ///</summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("45A66C27-90E0-4663-BB71-18A1BFB04935")]
    public class Miscellaneous
    {
        #region Registration

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComRegisterFunction]
        public static void RegisterFunction(Type type)
        {
            Registry.ClassesRoot.CreateSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"));
            RegistryKey key = Registry.ClassesRoot.OpenSubKey(ApplicationHelper.GetSubKeyName(type, "InprocServer32"), true);
            key.SetValue("", Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        [ComUnregisterFunction]
        public static void UnregisterFunction(Type type)
        {
            Registry.ClassesRoot.DeleteSubKey(ApplicationHelper.GetSubKeyName(type, "Programmable"), false);
        }

        #endregion
            
        #region Constructor

        public Miscellaneous()
        {
        }

        #endregion

        #region Functions

        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<param name="min"></param>
        ///<param name="max"></param>
        ///<returns></returns>
        public double DescaleVariable(double value, double min, double max)
        {
            return Misc.DeScaleVariable(value, min, max);
        }

        ///<summary>
        ///</summary>
        ///<param name="value"></param>
        ///<param name="min"></param>
        ///<param name="max"></param>
        ///<returns></returns>
        public double ScaleVariable(double value, double min, double max)
        {
            return Misc.ScaleVariable(value, min, max);
        }

        ///<summary>
        ///</summary>
        ///<param name="f"></param>
        ///<param name="k"></param>
        ///<param name="sigma"></param>
        ///<param name="T"></param>
        ///<param name="lambda"></param>
        ///<returns></returns>
        public object[] BlackFwdPrice(double f, double k, double sigma, double T, int lambda)
        {
            if (f == 0 | k == 0 | sigma == 0 | T == 0 | lambda == 0) return null;
            object[] output = OptionAnalytics.BlackFwdPrice(f, k, sigma, T, lambda);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="f"></param>
        ///<param name="k"></param>
        ///<param name="price"></param>
        ///<param name="T"></param>
        ///<param name="lambda"></param>
        ///<returns></returns>
        public double BlackImpliedVol(double f, double k, double price, double T, int lambda)
        {
            if (f == 0 | k == 0 | price < 0 | T == 0 | lambda == 0) return 0;
            double output = OptionAnalytics.BlackImpliedVol(f, k, price, T, lambda);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="contract"></param>
        ///<param name="futuresPrice"></param>
        ///<returns></returns>
        public double SFEBondPrice(string contract, double futuresPrice)
        {
            contract = contract.ToUpper();
            return FuturesAnalytics.SFEBondPrice(contract, futuresPrice);
        }

        ///<summary>
        ///</summary>
        ///<param name="contract"></param>
        ///<param name="futuresPrice"></param>
        ///<returns></returns>
        public double SFEBondTickValue(string contract, double futuresPrice)
        {
            contract = contract.ToUpper();
            return FuturesAnalytics.SFEBondTickValue(contract, futuresPrice);
        }

        ///<summary>
        ///</summary>
        ///<param name="contract"></param>
        ///<param name="futuresPrice"></param>
        ///<returns></returns>
        public double NZSFEBondPrice(string contract, double futuresPrice)
        {
            contract = contract.ToUpper();
            return FuturesAnalytics.NZSFEBondPrice(contract, futuresPrice);
        }

        ///<summary>
        ///</summary>
        ///<param name="contract"></param>
        ///<param name="futuresPrice"></param>
        ///<returns></returns>
        public double NZSFEBondTickValue(string contract, double futuresPrice)
        {
            contract = contract.ToUpper();
            return FuturesAnalytics.NZSFEBondTickValue(contract, futuresPrice);
        }

        ///<summary>
        ///</summary>
        ///<param name="contract"></param>
        ///<param name="futuresPrice"></param>
        ///<param name="strikePrice"></param>
        ///<param name="volatility"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="putCall"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public object[] SFEBondOptionPrice(string contract, double futuresPrice, double strikePrice, double volatility,
            DateTime valueDate, DateTime expiryMonthYear, string putCall, [Optional] Excel.Range xlHolidays)
        {
            if (contract == null | futuresPrice == 0 | strikePrice == 0 | volatility == 0 | putCall == null) return null;
            contract = contract.ToUpper();
            putCall = putCall.ToUpper();
            putCall = putCall.Trim();
            putCall = putCall.Substring(0, 1);
            object[,] values = xlHolidays.Value[System.Reflection.Missing.Value] as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return OptionAnalytics.SFEBondOptionPrice(contract, futuresPrice, strikePrice, volatility, valueDate, expiryMonthYear, putCall, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="contract"></param>
        ///<param name="futuresPrice"></param>
        ///<param name="strikePrice"></param>
        ///<param name="priceBp"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="putCall"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public double SFEBondOptionImpliedVol(string contract, double futuresPrice, double strikePrice, double priceBp,
            DateTime valueDate, DateTime expiryMonthYear, string putCall, [Optional] Excel.Range xlHolidays)
        {
            if (contract == null | futuresPrice == 0 | strikePrice == 0 | priceBp == 0 | putCall == null) return 0;
            contract = contract.ToUpper();
            putCall = putCall.ToUpper();
            putCall = putCall.Trim();
            putCall = putCall.Substring(0, 1);
            object[,] holidays;
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return OptionAnalytics.SFEBondOptionImpliedVol(contract, futuresPrice, strikePrice, priceBp, valueDate, expiryMonthYear, putCall, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<returns></returns>
        public double SFEBillPrice(double futuresPrice)
        {
            return FuturesAnalytics.SFEBillPrice(futuresPrice);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<returns></returns>
        public double SFEBillTickValue(double futuresPrice)
        {
            return FuturesAnalytics.SFEBillTickValue(futuresPrice);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<param name="strikePrice"></param>
        ///<param name="volatility"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="putCall"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public object[] SFEBillOptionPrice(double futuresPrice, double strikePrice, double volatility, DateTime valueDate,
            DateTime expiryMonthYear, string putCall, [Optional] Excel.Range xlHolidays)
        {
            if (futuresPrice == 0 | strikePrice == 0 | volatility == 0 | putCall == null) return null;
            putCall = putCall.ToUpper();
            putCall = putCall.Trim();
            putCall = putCall.Substring(0, 1);
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return OptionAnalytics.SFEBillOptionPrice(futuresPrice, strikePrice, volatility, valueDate, expiryMonthYear, putCall, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<param name="strikePrice"></param>
        ///<param name="priceBp"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="putCall"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public double SFEBillOptionImpliedVol(double futuresPrice, double strikePrice, double priceBp, DateTime valueDate,
            DateTime expiryMonthYear, string putCall, [Optional] Excel.Range xlHolidays)
        {
            if (futuresPrice == 0 | strikePrice == 0 | priceBp == 0 | putCall == null) return 0;
            putCall = putCall.ToUpper();
            putCall = putCall.Trim();
            putCall = putCall.Substring(0, 1);
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return OptionAnalytics.SFEBillOptionImpliedVol(futuresPrice, strikePrice, priceBp, valueDate, expiryMonthYear, putCall, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<param name="absoluteVol"></param>
        ///<param name="meanReversion"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public double SFEConvexityAdjustment(double futuresPrice, double absoluteVol, double meanReversion, DateTime valueDate,
            DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return FuturesAnalytics.SFEConvexityAdjustment(futuresPrice, absoluteVol, meanReversion, valueDate, expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<param name="strikePrice"></param>
        ///<param name="volatility"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="putCall"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public object[] CMEEuroOptionPrice(double futuresPrice, double strikePrice, double volatility, DateTime valueDate, DateTime expiryMonthYear,
            string putCall, [Optional] Excel.Range xlHolidays)
        {
            if (futuresPrice == 0 | strikePrice == 0 | volatility == 0 | putCall == null) return null;
            putCall = putCall.ToUpper();
            putCall = putCall.Trim();
            putCall = putCall.Substring(0, 1);
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return OptionAnalytics.CMEEuroOptionPrice(futuresPrice, strikePrice, volatility, valueDate, expiryMonthYear, putCall, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<param name="strikePrice"></param>
        ///<param name="priceBp"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="putCall"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public double CMEEuroOptionImpliedVol(double futuresPrice, double strikePrice, double priceBp, DateTime valueDate, DateTime expiryMonthYear,
            string putCall, [Optional] Excel.Range xlHolidays)
        {
            if (futuresPrice == 0 | strikePrice == 0 | priceBp == 0 | putCall == null) return 0;
            putCall = putCall.ToUpper();
            putCall = putCall.Trim();
            putCall = putCall.Substring(0, 1);
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return OptionAnalytics.CMEEuroOptionImpliedVol(futuresPrice, strikePrice, priceBp, valueDate, expiryMonthYear, putCall, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="futuresPrice"></param>
        ///<param name="absoluteVol"></param>
        ///<param name="meanReversion"></param>
        ///<param name="valueDate"></param>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public double CMEConvexityAdjustment(double futuresPrice, double absoluteVol, double meanReversion, DateTime valueDate, DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return FuturesAnalytics.CMEConvexityAdjustment(futuresPrice, absoluteVol, meanReversion, valueDate, expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime[] SFEBillDates(DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.SFEBillDates(expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime[] SFEBondDates(DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.SFEBondDates(expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime[] CMEEuroDates(DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.CMEEuroDates(expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime[] NZBillDates(DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.NZBillDates(expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime[] CBOT5YrNoteDates(DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.CBOT5YrNoteDates(expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="expiryMonthYear"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime[] CBOT10YrNoteDates(DateTime expiryMonthYear, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.CBOT10YrNoteDates(expiryMonthYear, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="startDate"></param>
        ///<param name="numRolls"></param>
        ///<param name="dwmy"></param>
        ///<param name="rollMethod"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime DateRoll(DateTime startDate, int numRolls, string dwmy, string rollMethod, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            dwmy = dwmy.ToUpper();
            rollMethod = rollMethod.ToUpper();
            return DateHelper.DateRoll(startDate, numRolls, dwmy, rollMethod, holidays);
        }

        //BMK
        ///<summary>
        ///</summary>
        ///<param name="startDate"></param>
        ///<param name="numDays"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public DateTime BizDayRoll(DateTime startDate, int numDays, [Optional] Excel.Range xlHolidays)
        {
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            return DateHelper.BizDayRoll(startDate, numDays, holidays);
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="couponFreq"></param>
        ///<returns></returns>
        public DateTime NextCouponDate(DateTime settlement, DateTime maturity, int couponFreq)
        {
            return DateHelper.NextCouponDate(settlement, maturity, couponFreq);
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="couponFreq"></param>
        ///<returns></returns>
        public DateTime LastCouponDate(DateTime settlement, DateTime maturity, int couponFreq)
        {
            return DateHelper.LastCouponDate(settlement, maturity, couponFreq);
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="couponRate"></param>
        ///<param name="yield"></param>
        ///<param name="couponFreq"></param>
        ///<param name="exIntPeriod"></param>
        ///<returns></returns>
        public double BondPrice(DateTime settlement, DateTime maturity, double couponRate, double yield, int couponFreq, int exIntPeriod)
        {
            if (couponRate == 0 | yield == 0 | couponFreq == 0 | exIntPeriod < 0) return 0;
            if (settlement > maturity) return 0;
            Double output = BondAnalytics.BondPrice(settlement, maturity, couponRate, yield, couponFreq, exIntPeriod);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="imBp"></param>
        ///<param name="tmBp"></param>
        ///<param name="pmtFreq"></param>
        ///<param name="rateSet"></param>
        ///<param name="rateToNextCoup"></param>
        ///<param name="swapRate"></param>
        ///<param name="dayCount"></param>
        ///<param name="exInt"></param>
        ///<param name="xlHolidays"></param>
        ///<returns></returns>
        public double FRNPrice(DateTime settlement, DateTime maturity, double imBp, double tmBp, int pmtFreq,
            double rateSet, double rateToNextCoup, double swapRate, int dayCount, int exInt, [Optional] Excel.Range xlHolidays)
        {
            if (pmtFreq < 0 | rateSet < 0 | rateToNextCoup < 0 | swapRate < 0 | exInt < 0) return 0;
            if (settlement > maturity) return 0;
            object[,] values = xlHolidays.get_Value(System.Reflection.Missing.Value) as object[,];
            object[,] holidays;
            if (values.GetType().Name == "Missing")
            {
                holidays = new Object[1, 1];
                holidays[0, 0] = new DateTime(1, 1, 1);
            }
            else
            {
                holidays = ArrayHelper.RangeToMatrix(values);
            }
            Double output = BondAnalytics.FRNPrice(settlement, maturity, imBp, tmBp, pmtFreq, rateSet, rateToNextCoup, swapRate, dayCount, exInt, holidays);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="yield"></param>
        ///<param name="faceValue"></param>
        ///<returns></returns>
        public double BillPrice(DateTime settlement, DateTime maturity, double yield, double faceValue)
        {
            if (yield == 0 | faceValue == 0) return 0;
            if (settlement > maturity) return 0;
            Double output = BondAnalytics.BillPrice(settlement, maturity, yield, faceValue);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="couponRate"></param>
        ///<param name="yield"></param>
        ///<returns></returns>
        public double RBABondPrice(DateTime settlement, DateTime maturity, double couponRate, double yield)
        {
            if (couponRate == 0 | yield == 0) return 0;
            if (settlement > maturity) return 0;
            Double output = BondAnalytics.RBABondPrice(settlement, maturity, couponRate, yield);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="couponRate"></param>
        ///<param name="yield"></param>
        ///<param name="couponFreq"></param>
        ///<param name="kt"></param>
        ///<param name="p"></param>
        ///<param name="exIntPeriod"></param>
        ///<returns></returns>
        public double CIBPrice(DateTime settlement, DateTime maturity, double couponRate, double yield, int couponFreq, double kt, double p, int exIntPeriod)
        {
            if (couponRate < 0 | yield < 0 | couponFreq <= 0 | kt < 0 | exIntPeriod < 0) return 0;
            if (settlement > maturity) return 0;
            Double output = BondAnalytics.CIBPrice(settlement, maturity, couponRate, yield, couponFreq, kt, p, exIntPeriod);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="settlement"></param>
        ///<param name="maturity"></param>
        ///<param name="baseAnnuity"></param>
        ///<param name="yield"></param>
        ///<param name="pmtFreq"></param>
        ///<param name="cpiBase"></param>
        ///<param name="cpiLatest"></param>
        ///<param name="cpiPrevious"></param>
        ///<param name="nextPmtKnownFlag"></param>
        ///<param name="exIntPeriod"></param>
        ///<returns></returns>
        public double IABPrice(DateTime settlement, DateTime maturity, double baseAnnuity, double yield, int pmtFreq, 
            double cpiBase, double cpiLatest, double cpiPrevious, bool nextPmtKnownFlag, int exIntPeriod)
        {
            if (baseAnnuity < 0 | yield < 0 | pmtFreq <= 0 | cpiBase < 0 | exIntPeriod < 0) return 0;
            if (settlement > maturity) return 0;
            Double output = BondAnalytics.IABPrice(settlement, maturity, baseAnnuity, yield, pmtFreq, cpiBase, cpiLatest, cpiPrevious, nextPmtKnownFlag, exIntPeriod);
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="cf"></param>
        ///<param name="cfDays"></param>
        ///<param name="interpSpace"></param>
        ///<param name="zeroCompFreq"></param>
        ///<param name="method"></param>
        ///<param name="seedRates"></param>
        ///<returns></returns>
        public object[,] ZeroCalc(Excel.Range cf, Excel.Range cfDays, string interpSpace, double zeroCompFreq, string method, Excel.Range seedRates)
        {
            object[,] cfs = cf.Value[System.Reflection.Missing.Value] as object[,];
            object[,] cfDay = cfDays.Value[System.Reflection.Missing.Value] as object[,];
            object[,] values = seedRates.Value[System.Reflection.Missing.Value] as object[,];
            interpSpace = interpSpace.ToUpper();
            interpSpace = interpSpace.Trim();
            method = method.ToUpper();
            method = method.Trim();
            return InterpolationFunctions.ZeroCalc(ArrayHelper.RangeToMatrix(cfs), ArrayHelper.RangeToMatrix(cfDay), interpSpace, zeroCompFreq, method, ArrayHelper.RangeToMatrix(values));
        }

        ///<summary>
        ///</summary>
        ///<param name="xyData"></param>
        ///<param name="x"></param>
        ///<param name="col1"></param>
        ///<param name="col2"></param>
        ///<returns></returns>
        public double LinearInterp(Excel.Range xyData, double x, int col1, int col2)
        {
            object[,] values = xyData.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.LinearInterp(Misc.Extract2Columns(ArrayHelper.RangeToMatrix(values), col1, col2), x);
        }

        ///<summary>
        ///</summary>
        ///<param name="xyData"></param>
        ///<param name="x"></param>
        ///<param name="col1"></param>
        ///<param name="col2"></param>
        ///<returns></returns>
        public double PiecewiseLinearInterp(Excel.Range xyData, double x, int col1, int col2)
        {
            object[,] values = xyData.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.PiecewiseLinearInterp(Misc.Extract2Columns(ArrayHelper.RangeToMatrix(values), col1, col2), x);
        }

        ///<summary>
        ///</summary>
        ///<param name="xyData"></param>
        ///<param name="x"></param>
        ///<param name="col1"></param>
        ///<param name="col2"></param>
        ///<returns></returns>
        public double HermiteSplineInterp(Excel.Range xyData, double x, int col1, int col2)
        {
            object[,] values = xyData.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.HSplineInterp(Misc.Extract2Columns(ArrayHelper.RangeToMatrix(values), col1, col2), x);
        }

        ///<summary>
        ///</summary>
        ///<param name="xyData"></param>
        ///<param name="x"></param>
        ///<param name="col1"></param>
        ///<param name="col2"></param>
        ///<returns></returns>
        public double LogLinearInterp(Excel.Range xyData, double x, int col1, int col2)
        {
            object[,] values = xyData.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.LogLinearInterp(Misc.Extract2Columns(ArrayHelper.RangeToMatrix(values), col1, col2), x);
        }

        ///<summary>
        ///</summary>
        ///<param name="xyzData"></param>
        ///<param name="x"></param>
        ///<param name="y"></param>
        ///<returns></returns>
        public double BiLinearInterp(Excel.Range xyzData, double x, double y)
        {
            object[,] values = xyzData.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.BiLinearInterp(ArrayHelper.RangeToMatrix(values), x, y);
        }

        ///<summary>
        ///</summary>
        ///<param name="xyzData"></param>
        ///<param name="x"></param>
        ///<param name="y"></param>
        ///<returns></returns>
        public double BiLinearInterp2(Excel.Range xyzData, double x, double y)
        {
            object[,] values = xyzData.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.BiLinearInterp2(ArrayHelper.RangeToMatrix(values), x, y);
        }

        ///<summary>
        ///</summary>
        ///<param name="xyData"></param>
        ///<param name="interpX"></param>
        ///<param name="interpSpace"></param>
        ///<param name="zeroCompFreq"></param>
        ///<param name="method"></param>
        ///<returns></returns>
        public double GeneralZeroInterp(Excel.Range xyData, double interpX, string interpSpace, double zeroCompFreq, string method)
        {
            object[,] values = xyData.Value[System.Reflection.Missing.Value] as object[,];
            interpSpace = interpSpace.ToUpper();
            interpSpace = interpSpace.Trim();
            method = method.ToUpper();
            method = method.Trim();
            return InterpolationFunctions.GeneralZeroInterp(ArrayHelper.RangeToMatrix(values), interpX, interpSpace, zeroCompFreq, method);
        }

        ///<summary>
        ///</summary>
        ///<param name="cfDays"></param>
        ///<param name="cf"></param>
        ///<param name="curve"></param>
        ///<param name="instrumentNumber"></param>
        ///<param name="interpSpace"></param>
        ///<param name="zeroCompFreq"></param>
        ///<param name="method"></param>
        ///<returns></returns>
        public double PVFromCurve(Excel.Range cfDays, Excel.Range cf, Excel.Range curve, int instrumentNumber, string interpSpace, double zeroCompFreq, string method)
        {
            object[,] cfds = cfDays.Value[System.Reflection.Missing.Value] as object[,];
            object[,] cfs = cf.Value[System.Reflection.Missing.Value] as object[,];
            object[,] values = curve.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.PVFromCurve(ArrayHelper.RangeToMatrix(cfds), ArrayHelper.RangeToMatrix(cfs), ArrayHelper.RangeToMatrix(values)
                                                    , instrumentNumber, interpSpace, zeroCompFreq, method);
        }

        ///<summary>
        ///</summary>
        ///<param name="zero"></param>
        ///<param name="zeroCompFreq"></param>
        ///<param name="T"></param>
        ///<returns></returns>
        public double Zero2DF(double zero, double zeroCompFreq, double T)
        {
            return InterpolationFunctions.Zero2DF(zero, zeroCompFreq, T);
        }

        ///<summary>
        ///</summary>
        ///<param name="df"></param>
        ///<param name="zeroCompFreq"></param>
        ///<param name="T"></param>
        ///<returns></returns>
        public double DF2Zero(double df, double zeroCompFreq, double T)
        {
            return InterpolationFunctions.DF2Zero(df, zeroCompFreq, T);
        }

        ///<summary>
        ///</summary>
        ///<param name="startTime"></param>
        ///<param name="matTime"></param>
        ///<param name="curve"></param>
        ///<param name="interpSpace"></param>
        ///<param name="zeroCompFreq"></param>
        ///<param name="method"></param>
        ///<returns></returns>
        public double FwDfromCurve(double startTime, double matTime, Excel.Range curve, string interpSpace, double zeroCompFreq, string method)
        {
            object[,] values = curve.Value[System.Reflection.Missing.Value] as object[,];
            return InterpolationFunctions.FWDfromCurve(startTime, matTime, ArrayHelper.RangeToMatrix(values), interpSpace, zeroCompFreq, method);
        }

        ///<summary>
        ///</summary>
        ///<param name="f"></param>
        ///<param name="k"></param>
        ///<param name="expiry"></param>
        ///<param name="beta"></param>
        ///<param name="alpha"></param>
        ///<param name="rho"></param>
        ///<param name="nu"></param>
        ///<returns></returns>
        public double BlackVolSABR(double f, double k, double expiry, double beta, double alpha, double rho, double nu)
        {
            return OptionAnalytics.BlackVolSABR(f, k, expiry, beta, alpha, rho, nu);
        }

        ///<summary>
        ///</summary>
        ///<param name="f"></param>
        ///<param name="k"></param>
        ///<param name="expiry"></param>
        ///<param name="xlParams"></param>
        ///<param name="interpMethod"></param>
        ///<returns></returns>
        public double InterpBlackVolSABR(double f, double k, double expiry, Excel.Range xlParams, string interpMethod)
        {
            object[,] values = xlParams.Value[System.Reflection.Missing.Value] as object[,];
            interpMethod = interpMethod.ToUpper();
            interpMethod = interpMethod.Trim();
            interpMethod = interpMethod.Substring(0, 1);
            return OptionAnalytics.InterpBlackVolSABR(f, k, expiry, ArrayHelper.RangeToMatrix(values), interpMethod);
        }

        private object[,] XLMapRangeTo2DObject(object range)
        {
            Excel.Range r = range as Excel.Range;
            var matrix = (Object[,])r.get_Value(System.Type.Missing);
            int rows = matrix.GetUpperBound(0);
            int cols = matrix.GetUpperBound(1);
            object[,] output = new object[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    output[i, j] = matrix[i + 1, j + 1];
                    if (Convert.ToString(output[i, j]) == "") output[i, j] = null;
                }
            return output;
        }

        private static double[,] Object2Double(object[,] input)
        {
            int rows = input.GetUpperBound(0) + 1;
            int cols = input.GetUpperBound(1) + 1;
            var output = new double[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    if (input[i, j] == null) input[i, j] = 0.00;
                    output[i, j] = (double)input[i, j];
                }
            return output;
        }

        private int[,] Object2Int(object[,] input)
        {
            int rows = input.GetUpperBound(0) + 1;
            int cols = input.GetUpperBound(1) + 1;
            var output = new int[rows, cols];
            for (int i = 0; i < rows; i++)
                for (int j = 0; j < cols; j++)
                {
                    if (input[i, j] == null) input[i, j] = 0;
                    output[i, j] = Convert.ToInt32(input[i, j]);
                }
            return output;
        }

        ///<summary>
        ///</summary>
        ///<param name="z"></param>
        ///<returns></returns>
        public double CummNormDist(double z)
        {
            return Misc.CummNormDist(z);
        }

        //public double CummNormDistOld(double z)
        //{
        //    return Misc.CummNormDistOld(z);
        //}

        ///<summary>
        ///</summary>
        ///<param name="num"></param>
        ///<param name="seed"></param>
        ///<returns></returns>
        public double[,] NormVarZ(int num, int seed)
        {
            return Misc.NormVarZ(num, seed);
        }

        ///<summary>
        ///</summary>
        ///<param name="mean1"></param>
        ///<param name="mean2"></param>
        ///<param name="stDev1"></param>
        ///<param name="stDev2"></param>
        ///<param name="correlation"></param>
        ///<returns></returns>
        public double ProductNormalMean(double mean1, double mean2, double stDev1, double stDev2, double correlation)
        {
            return Misc.ProductNormalMean(mean1, mean2, stDev1, stDev2, correlation);
        }

        ///<summary>
        ///</summary>
        ///<param name="mean1"></param>
        ///<param name="mean2"></param>
        ///<param name="stDev1"></param>
        ///<param name="stDev2"></param>
        ///<param name="correlation"></param>
        ///<returns></returns>
        public double ProductNormalStDev(double mean1, double mean2, double stDev1, double stDev2, double correlation)
        {
            return Misc.ProductNormalStDev(mean1, mean2, stDev1, stDev2, correlation);
        }

        ///<summary>
        ///</summary>
        ///<param name="inputArray"></param>
        ///<param name="col1"></param>
        ///<param name="col2"></param>
        ///<returns></returns>
        public object[,] Extract2Columns(Excel.Range inputArray, int col1, int col2)
        {
            object[,] values = inputArray.Value[System.Reflection.Missing.Value] as object[,];
            return Misc.Extract2Columns(ArrayHelper.RangeToMatrix(values), col1, col2);
        }

        ///<summary>
        ///</summary>
        ///<param name="xlInput"></param>
        ///<returns></returns>
        public double[,] CholeskyDecomp(Excel.Range xlInput)
        {
            object[,] values = xlInput.Value[System.Reflection.Missing.Value] as object[,];
            return Misc.CholeskyDecomp(Object2Double(ArrayHelper.RangeToMatrix(values)));
        }

        ///<summary>
        ///</summary>
        ///<param name="xlInput"></param>
        ///<returns></returns>
        public double Determinant(Excel.Range xlInput)
        {
            object[,] values = xlInput.Value[System.Reflection.Missing.Value] as object[,];
            return Misc.Determinant(Object2Double(ArrayHelper.RangeToMatrix(values)));
        }

        ///<summary>
        ///</summary>
        ///<param name="xlInput"></param>
        ///<param name="row"></param>
        ///<param name="col"></param>
        ///<returns></returns>
        public object[,] SquareSubMatrix(Excel.Range xlInput, int row, int col)
        {
            object[,] values = xlInput.Value[System.Reflection.Missing.Value] as object[,];
            return Misc.SquareSubMatrix(ArrayHelper.RangeToMatrix(values), row, col);
        }

        #endregion
    }
}
