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
using System.Runtime.InteropServices;
using Highlander.CurveEngine.Helpers.V5r3;
using Highlander.Equities;
using Highlander.Reporting.Analytics.V5r3.Counterparty;
using Highlander.Reporting.Analytics.V5r3.Equities;
using Highlander.Reporting.Analytics.V5r3.Helpers;
using Highlander.Utilities.Helpers;
using HLV5r3.Helpers;
using Microsoft.Win32;
using DivList = Highlander.Equities.DivList;
//using Excel = Microsoft.Office.Interop.Excel;
using ZeroCurve = Highlander.Reporting.Analytics.V5r3.Rates.ZeroCurve;

#endregion

namespace HLV5r3.Analytics
{
    ///<summary>
    ///</summary>
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [Guid("5002A42B-52BF-4BBA-A2AD-A263013120B3")]
    public class Equity
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
            key?.SetValue("", Environment.SystemDirectory + @"\mscoree.dll", RegistryValueKind.String);
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

        #endregion

        #region Portfolio Interface

        //private const int CArraySize = 10000;

        //public object[,] PortfolioOptimizer(object[,] portfolioArray)
        //{
        //    //var portfolioArray = portfolioArray.get_Value(System.Reflection.Missing.Value) as object[,];
        //    if (portfolioArray != null)
        //    {
        //        int iRows = portfolioArray.GetUpperBound(0);
        //        //int iCols = _PortfolioArray.GetUpperBound(1);
        //        var res = new object[iRows, 2];
        //        var portfolio = new List<Stock>();
        //        var lp = new LinearProgramming();
        //        for (int idx = 1; idx <= iRows; idx++)
        //        {
        //            string stockName = Convert.ToString(portfolioArray[idx, 1]);
        //            var stockDollars = (double)portfolioArray[idx, 2];
        //            if (stockDollars > 0)
        //            {
        //                var stock = new Stock(stockName, stockDollars);
        //                portfolio.Add(stock);
        //            }
        //        }
        //        portfolio.Sort();
        //        lp.Portfolio = portfolio;
        //        double[] solutionVector = lp.CalcFinalOptimum();
        //        var stockArray = new Stock[portfolio.Count];
        //        portfolio.CopyTo(stockArray);
        //        // Excel API expects a zero-based array to be returned;
        //        for (int idx = 0; idx < solutionVector.Length; idx++)
        //        {
        //            res[idx, 0] = stockArray[idx].StockName;
        //            res[idx, 1] = solutionVector[idx].ToString(CultureInfo.InvariantCulture);
        //        }
        //        return res;
        //    }
        //    return null;
        //}

        #endregion

        #region Collar Pricer

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="spot"></param>
        /// <param name="callStrike"></param>
        /// <param name="style"></param>
        /// <param name="tradeDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="zeroRange"></param>
        /// <param name="divRange"></param>
        /// <param name="orcParamsRange"></param>
        /// <returns></returns>
        public static double PutCollarPricer(string underlying, double spot, double callStrike,
                                      string style, DateTime tradeDate, DateTime expiryDate,
                                      object[,] zeroRange,
                                      object[,] divRange, object[,] orcParamsRange)
        {
            //var values = ((Excel.Range)orcParamsRange).Value[System.Reflection.Missing.Value] as object[,];
            orcParamsRange = (object[,])DataRangeHelper.TrimNulls(orcParamsRange);
            var orcParams = RangeHelper.Convert2DArrayToClass<WingParamsRange>(ArrayHelper.RangeToMatrix(orcParamsRange));
            //var zeroArray = ((Excel.Range)zeroRange).Value[System.Reflection.Missing.Value] as object[,];
            var zeroes = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ZeroCurveRange>(zeroRange);
            //var divArray = ((Excel.Range)zeroRange).Value[System.Reflection.Missing.Value] as object[,];
            var divs = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DividendRange>(divRange);
            var result = CollarPricer.PutCollarPricer(underlying, spot, callStrike,
                                                                   style, tradeDate, expiryDate,
                                                                   zeroes, divs, orcParams);
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="spot"></param>
        /// <param name="callStrike"></param>
        /// <param name="style"></param>
        /// <param name="tradeDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="zeroRange"></param>
        /// <param name="divRange"></param>
        /// <param name="orcParamsRange"></param>
        /// <returns></returns>
        public static double CallCollarPricer(string underlying, double spot, double callStrike,
                                      string style, DateTime tradeDate, DateTime expiryDate,
                                      object[,] zeroRange,
                                      object[,] divRange, object[,] orcParamsRange)
        {
            //var values = orcParamsRange.Value[System.Reflection.Missing.Value] as object[,];
            orcParamsRange = (object[,])DataRangeHelper.TrimNulls(orcParamsRange);
            var orcParams = RangeHelper.Convert2DArrayToClass<WingParamsRange>(ArrayHelper.RangeToMatrix(orcParamsRange));
            //var zeroArray = zeroRange.Value[System.Reflection.Missing.Value] as object[,];
            var zeroes = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ZeroCurveRange>(zeroRange);
            //var divArray = divRange.Value[System.Reflection.Missing.Value] as object[,];
            var divs = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DividendRange>(divRange);
            var result = CollarPricer.CallCollarPricer(underlying, spot, callStrike,
                                                                   style, tradeDate, expiryDate,
                                                                   zeroes, divs, orcParams);
            return result;
        }

        #endregion

        #region Basic Grid Interface

        /// <summary>
        /// 
        /// </summary>
        /// <param name="today"></param>
        /// <param name="spot"></param>
        /// <param name="strike"></param>
        /// <param name="sig"></param>
        /// <param name="dExp"></param>
        /// <param name="sPay"></param>
        /// <param name="sStyle"></param>
        /// <param name="nGrid"></param>
        /// <param name="tStep"></param>
        /// <param name="divAmountsAsArray"></param>
        /// <param name="divDatesAsArray"></param>
        /// <param name="zeroAmountsAsArray"></param>
        /// <param name="zeroDatesAsArray"></param>
        /// <returns></returns>
        public static double GetEquityPrice(DateTime today,
                double spot,
                double strike,
                double sig,
                DateTime dExp,
                string sPay,
                string sStyle,
                int nGrid,
                double tStep,
                object divAmountsAsArray,
                object divDatesAsArray,
                object zeroAmountsAsArray,
                object zeroDatesAsArray)
        {
            //Map the ranges
            var divAmounts = DataRangeHelper.StripDoubleRange(divAmountsAsArray);
            var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
            var zeroAmounts = DataRangeHelper.StripDoubleRange(zeroAmountsAsArray);
            var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
            //set up the DivList
            int nd = divDates.Count;//GetUpperBound(0) + 1;
            var myDiv = new DivList {DivPoints = nd};
            myDiv.MakeArrays();
            for (int idx = 0; idx != nd; idx++)
            {
                double r = divAmounts[idx];
                DateTime dp = divDates[idx];
                TimeSpan ts = dp - today;
                myDiv.SetD(idx, r, ts.Days / 365.0);
            }
            //set up the zero
            int nz = zeroDates.Count;//GetUpperBound(0) + 1;
            var myZero = new ZeroCurve {Ratepoints = nz};
            myZero.MakeArrays();
            for (int idx = 0; idx != nz; idx++)
            {
                double r = zeroAmounts[idx];
                DateTime dp = zeroDates[idx];
                TimeSpan ts = dp - today;
                myZero.SetR(idx, r, ts.Days / 365.0);
            }
            //compute the discounted dividends to  expiry and work out continuous
            double sum = 0.0;
            TimeSpan tsE = dExp - today;
            double texp = tsE.Days / 365.0;
            for (int idx = 0; idx != nd; idx++)
            {
                if (myDiv.GetT(idx) <= texp)
                    sum += myDiv.GetD(idx) * Math.Exp(-myDiv.GetT(idx) * myZero.LinInterp(myDiv.GetT(idx)));
            }
            if (sum >= spot) throw new ApplicationException("Dividend stream greater than spot");
            var myG = new Grid
                {
                    XL = Math.Log(spot) - 8.0*sig*Math.Sqrt(texp),
                    Xu = Math.Log(spot) + 8.0*sig*Math.Sqrt(texp),
                    Steps = nGrid,
                    Strike = strike,
                    Spot = spot,
                    SPay = sPay,
                    SStyle = sStyle,
                    T = texp
                };
            myG.NTsteps = Convert.ToInt32(myG.T / tStep);
            myG.Sig = sig;
            var greeks = new double[4];
            double priceUp = myG.Pricer(myZero, myDiv, ref greeks, false);
            return priceUp;
        }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="pTarget"></param>
            /// <param name="today"></param>
            /// <param name="spot"></param>
            /// <param name="strike"></param>
            /// <param name="sig"></param>
            /// <param name="dExp"></param>
            /// <param name="sPay"></param>
            /// <param name="sStyle"></param>
            /// <param name="nGrid"></param>
            /// <param name="tStep"></param>
            /// <param name="dividendAmountsAsArray"></param>
            /// <param name="dividendDatesAsArray"></param>
            /// <param name="zeroAmountsAsArray"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <returns></returns>
            public static double GetEquityImpliedVol(double pTarget, DateTime today,
                double spot,
                double strike,
                double sig,
                DateTime dExp,
                string sPay,
                string sStyle,
                int nGrid,
                double tStep,
                object dividendAmountsAsArray,
                object dividendDatesAsArray,
                object zeroAmountsAsArray,
                object zeroDatesAsArray)
            {
                double sG = sig;
                for (int idx = 0; idx != 25; idx++)
                {
                    double res = GetEquityPrice(today, spot, strike, sG, dExp, sPay, sStyle, nGrid, tStep, dividendAmountsAsArray, dividendDatesAsArray, zeroAmountsAsArray, zeroDatesAsArray)
                        - pTarget;
                    if (Math.Abs(res) < 0.00001) { return sG; }
                    var resUp = GetEquityPrice(today, spot, strike, sG + 0.001, dExp, sPay, sStyle, nGrid, tStep, dividendAmountsAsArray, dividendDatesAsArray, zeroAmountsAsArray, zeroDatesAsArray)
                        - pTarget;
                    sG -= res * 0.001 / (resUp - res);
                }
                return 0.0;
            }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="today"></param>
        /// <param name="spot"></param>
        /// <param name="strike"></param>
        /// <param name="sig"></param>
        /// <param name="dExp"></param>
        /// <param name="sPay"></param>
        /// <param name="sStyle"></param>
        /// <param name="nGrid"></param>
        /// <param name="tStep"></param>
        /// <param name="iNum"></param>
        /// <param name="dividendAmountsAsArray"></param>
        /// <param name="dividendDatesAsArray"></param>
        /// <param name="zeroAmountsAsArray"></param>
        /// <param name="zeroDatesAsArray"></param>
        /// <returns></returns>
        public static double GetEquityGreeks(DateTime today,
                double spot,
                double strike,
                double sig,
                DateTime dExp,
                string sPay,
                string sStyle,
                int nGrid,
                double tStep,
                int iNum,
                object dividendAmountsAsArray,
                object dividendDatesAsArray,
                object zeroAmountsAsArray,
                object zeroDatesAsArray
              )
            {
                //Map the ranges
                var divAmounts = DataRangeHelper.StripDoubleRange(dividendAmountsAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(dividendDatesAsArray);
                var zeroAmounts = DataRangeHelper.StripDoubleRange(zeroAmountsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                //set up the DivList
                int nd = divDates.Count;//GetUpperBound(0) + 1;
                var myDiv = new DivList {DivPoints = nd};
                myDiv.MakeArrays();
                for (int idx = 0; idx != nd; idx++)
                {
                    double r = divAmounts[idx];
                    DateTime dp = divDates[idx];
                    TimeSpan ts = dp - today;
                    myDiv.SetD(idx, r, ts.Days / 365.0);
                }
                //set up the zero
                int nz = zeroDates.Count;//GetUpperBound(0) + 1;
                var myZero = new ZeroCurve {Ratepoints = nz};
                myZero.MakeArrays();
                for (int idx = 0; idx != nz; idx++)
                {
                    double r = zeroAmounts[idx];
                    DateTime dp = zeroDates[idx];
                    TimeSpan ts = dp - today;
                    myZero.SetR(idx, r, ts.Days / 365.0);
                }
                //compute the discounted dividends to  expiry and work out continuous
                TimeSpan tsE = dExp - today;
                double tExp = tsE.Days / 365.0;
                for (int idx = 0; idx != nd; idx++)
                {
                    if (myDiv.GetT(idx) <= tExp)
                    {
                        double d = myDiv.GetD(idx)*Math.Exp(-myDiv.GetT(idx)*myZero.LinInterp(myDiv.GetT(idx)));
                    }
                }
                //double qc = -Math.Log((spot - sum) / spot) / texp;
                //double rc = myZero.linInterp(texp);
                var myG = new Grid
                    {
                        XL = Math.Log(spot) - 8.0*sig*Math.Sqrt(tExp),
                        Xu = Math.Log(spot) + 8.0*sig*Math.Sqrt(tExp),
                        Steps = nGrid,
                        Strike = strike,
                        Spot = spot,
                        SPay = sPay,
                        SStyle = sStyle,
                        T = tExp
                    };
                myG.NTsteps = Convert.ToInt32(myG.T / tStep);
                myG.Sig = sig;
                var greeks = new double[4];
                double price = myG.Pricer(myZero, myDiv, ref greeks, true);
                myG.Sig += 0.01;
                double priceUp = myG.Pricer(myZero, myDiv, ref greeks, false);
                greeks[3] = priceUp - price;
                return greeks[iNum];
            }

            ///<summary>
            ///</summary>
            ///<param name="rateDaysAsArray"></param>
            ///<param name="rateAmountsAsArray"></param>
            ///<param name="dividendDaysAsArray"></param>
            ///<param name="dividendAmountsAsArray"></param>
            ///<param name="volatilityTimesAsArray"></param>
            ///<param name="volatilitiesAsRange"></param>
            ///<param name="spot"></param>
            ///<param name="callStrike"></param>
            ///<param name="putStrike"></param>
            ///<param name="maturity"></param>
            ///<param name="kappa"></param>
            ///<param name="theta"></param>
            ///<param name="sigma"></param>
            ///<param name="profileTimes"></param>
            ///<param name="confidence"></param>
            ///<param name="tStepSize"></param>
            ///<param name="simulations"></param>
            ///<param name="seed"></param>
            ///<returns></returns>
            public double[,] GetCollarPCE(object rateDaysAsArray,
                                                object rateAmountsAsArray,
                                                object dividendDaysAsArray,
                                                object dividendAmountsAsArray,
                                                object volatilityTimesAsArray,
                                                object[,] volatilitiesAsRange,
                                                double spot,
                                                double callStrike,
                                                double putStrike,
                                                double maturity,
                                                double kappa,
                                                double theta,
                                                double sigma,
                                                object profileTimes, 
                                                double confidence,                                
                                                double tStepSize,
                                                int simulations,
                                                int seed)
            {
                //Map Ranges
                var rateAmounts = DataRangeHelper.StripDoubleRange(rateAmountsAsArray);
                var dividendAmounts = DataRangeHelper.StripDoubleRange(dividendAmountsAsArray);
                var volatilityTimes = DataRangeHelper.StripDoubleRange(volatilityTimesAsArray);
                var dividendDays = DataRangeHelper.StripIntRange(dividendDaysAsArray);
                var rateDays = DataRangeHelper.StripIntRange(rateDaysAsArray);
                var profile = DataRangeHelper.StripDoubleRange(profileTimes);
                var volatilitiesAsDoubles = RangeHelper.RangeToDoubleMatrix(volatilitiesAsRange);
                List<OrcWingParameters> volSurf = UnpackWing(volatilitiesAsDoubles, volatilityTimes.ToArray(), spot, rateDays.ToArray(), rateAmounts.ToArray(), dividendDays.ToArray(), dividendAmounts.ToArray());          
                CleanDivs(ref dividendDays, ref dividendAmounts);
                double[,] results = EquityPCEAnalytics.GetCollarPCE("CollarPCE",
                                                  rateDays.ToArray(),
                                                  rateAmounts.ToArray(),
                                                  dividendDays.ToArray(),
                                                  dividendAmounts.ToArray(),
                                                  volSurf,
                                                  spot,
                                                  callStrike,
                                                  putStrike,
                                                  maturity,
                                                  kappa,
                                                  theta,
                                                  sigma,
                                                  profile.ToArray(),
                                                  confidence,
                                                  tStepSize,
                                                  simulations,
                                                  seed);
                //int n = profiletimes.Length;
                //var lhs = new double[profiletimes.Length];
                //for (int i = 0; i < n; i++)
                //{
                //    lhs[i] = results[i, 1];                
                //}
                return results;
            }

            ///<summary>
            ///</summary>
            ///<param name="volatilities"></param>
            ///<param name="days"></param>
            ///<param name="spot"></param>
            ///<param name="rateDays"></param>
            ///<param name="rateAmounts"></param>
            ///<param name="divDays"></param>
            ///<param name="divAmounts"></param>
            ///<returns></returns>
            private static List<OrcWingParameters> UnpackWing(double[,] volatilities, double[] days, double spot, int[] rateDays, double[] rateAmounts, 
                int[] divDays, double[] divAmounts)
            {
                const double dayBasis = 365.0;
                int rows = volatilities.GetLength(0);
                var opList = new List<OrcWingParameters>();
                for (int idx = 0; idx < rows; idx++)
                {
                    double fwd = EquityAnalytics.GetForwardCCLin365(spot, days[idx]/dayBasis, divDays, divAmounts, rateDays, rateAmounts);
                    var op = new OrcWingParameters
                                 {
                        AtmForward = fwd,
                        CurrentVol = volatilities[idx, 0],
                        SlopeRef = volatilities[idx, 1],
                        PutCurve = volatilities[idx, 2],
                        CallCurve = volatilities[idx, 3],
                        DnCutoff = volatilities[idx, 4],
                        UpCutoff = volatilities[idx, 5],
                        Vcr = volatilities[idx, 6],
                        Scr = volatilities[idx, 7],
                        Ssr = 100*volatilities[idx, 8],
                        Dsr = volatilities[idx, 9],
                        Usr = volatilities[idx, 10],
                        RefVol = volatilities[idx, 11],
                        RefFwd = fwd,
                        TimeToMaturity = days[idx]/dayBasis
                    };
                    opList.Add(op);                                                                 
                }
                return opList;
            }

            private static ZeroCurve UnpackZero(DateTime today, DateTime[] dates, double[] amts)
            {
                int n1 = dates.Length;
                int n2 = dates.Length;
                if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
                var zc = new ZeroCurve {Ratepoints = n1};
                zc.MakeArrays();
                int kdx = 0;
                for (int idx=0; idx<n1;idx++)
                {
                    double time = dates[idx].Subtract(today).Days/365.0;
                    double rate = amts[idx];            
                    zc.SetR(kdx, rate, time);
                    kdx++;          
                }        
                return zc;
            }
            int _kdx;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="dates"></param>
            /// <param name="amounts"></param>
            /// <returns></returns>
            /// <exception cref="Exception"></exception>
            public DivList UnpackDiv(DateTime today, DateTime expiry, DateTime[] dates, double[] amounts)
            {
                int n1 = dates.Length;
                int n2 = dates.Length;
                double timeToExp = expiry.Subtract(today).Days / 365.0;
                if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
                var dl = new DivList {DivPoints = n1};
                dl.MakeArrays();
                for (int idx = 0; idx < n1; idx++)
                {
                    double time = dates[idx].Subtract(today).Days / 365.0;
                    double rate = amounts[idx];
                    if (time > 0 & time <= timeToExp)
                    {
                        dl.SetD(_kdx, rate, time);
                        _kdx++;
                    }
                }
                return dl;
            }

            /// <summary>
            /// Clean up input dividends
            /// </summary>
            /// <param name="dividendDays"></param>
            /// <param name="dividendAmounts"></param>
            private static void CleanDivs(ref List<int> dividendDays, ref List<double> dividendAmounts)
            {
	            var indices = new List<int>();
	            for (int idx =0 ; idx< dividendDays.Count; idx++)
	            {
		            if (dividendDays[idx]>0)
			            indices.Add(idx);
	            }
                var divDaysCopy = new List<int>();
                var divAmountsCopy = new List<double>();        
                foreach (int idx in indices)
                {
                    divDaysCopy.Add(dividendDays[idx]);
                    divAmountsCopy.Add(dividendAmounts[idx]);
                
                }
                dividendDays.AddRange(divDaysCopy);
                dividendAmounts.AddRange(divAmountsCopy);
            }

        #endregion

        #region Equities Add In Interface

            /// <summary>
            /// Prices a vanilla option.
            /// </summary>
            /// <param name="style">The style.</param>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol">The vol.</param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="payStyle">The pay style.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridSteps">The grid steps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <returns></returns>
            public static double BinomialPricer(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
            string payStyle, object zeroDatesAsArray, object zeroRatesAsArray, object divDatesAsArray, object divAmsAsArray, 
            double gridSteps, string smoo, bool flatFlag)
            {
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BinomialPricer(style, spot, strike, vol, today, expiry,
                payStyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridSteps, smoo, flatFlag);
                return pr;
            }

            /// <summary>
            /// Pricers the specified style.
            /// </summary>
            /// <param name="style">The style.</param>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol">The vol.</param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="payStyle">The pay style.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridSteps">The grid steps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <returns></returns>
            public static double BinomialRelativePricer(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
            string payStyle, object zeroDatesAsArray, object zeroRatesAsArray, object divDatesAsArray, object divAmsAsArray, 
            double gridSteps, string smoo, bool flatFlag)
            {
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BinomialRelativePricer(style, spot, strike, vol, today, expiry, payStyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridSteps, smoo, flatFlag);
                return pr;
            }

            /// <summary>
            /// Gets the Black Scholes price.
            /// </summary>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol">The vol.</param>
            /// <param name="payStyle">The pay style.</param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <returns></returns>
            public static double BlackScholesPricer(double spot, double strike, double vol, string payStyle, DateTime today, DateTime expiry,
                object zeroDatesAsArray, object zeroRatesAsArray, object divDatesAsArray, object divAmsAsArray)
            {
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BlackScholesPricer(spot, strike, vol, payStyle, today, expiry, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray());
                return pr;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="spot"></param>
            /// <param name="strike"></param>
            /// <param name="vol"></param>
            /// <param name="payStyle"></param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray"></param>
            /// <param name="q"></param>
            /// <returns></returns>
            public static double BlackScholesPricerContDiv(double spot, double strike, double vol, string payStyle, DateTime today,
            DateTime expiry, object zeroDatesAsArray, object zeroRatesAsArray, double q)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BlackScholesPricerContDiv(spot, strike, vol, payStyle, today, expiry, zeroDates.ToArray(), zeroRates.ToArray(), q);
                return pr;
            }

            /// <summary>
            /// Gets the implied vol, using the discrete dividend pricer;
            /// </summary>
            /// <param name="price">The price.</param>
            /// <param name="style">The style.</param>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol0">The vol0.</param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="payStyle">The pay style.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="tol">The TOL.</param>
            /// <param name="step">The STEP.</param>
            /// <param name="gridSteps">The grid steps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <returns></returns>
            public static double BinomialImpVol(double price, string style, double spot, double strike, double vol0, DateTime today, DateTime expiry,
            string payStyle, object zeroDatesAsArray, object zeroRatesAsArray, object divDatesAsArray, object divAmsAsArray, 
                double tol, double step, double gridSteps, string smoo, bool flatFlag)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                double pr = EquitiesLibrary.BinomialImpVol(price, style, spot, strike, vol0, today, expiry, payStyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), tol, step, gridSteps, smoo, flatFlag);
                return pr;
            }

            /// <summary>
            /// Gets the Greeks.
            /// </summary>
            /// <param name="style">The style.</param>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol">The vol.</param>
            /// <param name="expiry"></param>
            /// <param name="payStyle">The pay style.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridSteps">The grid steps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <param name="today"></param>
            /// <returns></returns>
            public static object[,] BinomialGetGreeks(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
            string payStyle, object zeroDatesAsArray, object zeroRatesAsArray, object divDatesAsArray, object divAmsAsArray, double gridSteps, string smoo, bool flatFlag)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var retArray = EquitiesLibrary.BinomialGetGreeks(style, spot, strike, vol, today, expiry, payStyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridSteps, smoo, flatFlag);
                var result = RangeHelper.ConvertArrayToRange(retArray);//TODO This is a matrix
                return result;
            }

            /// <summary>
            /// Gets the greeks.
            /// </summary>
            /// <param name="style">The style.</param>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol">The vol.</param>
            /// <param name="expiry"></param>
            /// <param name="payStyle">The paystyle.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridSteps">The gridsteps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <param name="today"></param>
            /// <returns></returns>
            public static object[,] BinomialRelativeGetGreeks(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
              string payStyle, object zeroDatesAsArray, object zeroRatesAsArray, object divDatesAsArray, object divAmsAsArray, double gridSteps, string smoo, bool flatFlag)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var retArray = EquitiesLibrary.BinomialRelativeGetGreeks(style, spot, strike, vol, today, expiry, payStyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridSteps, smoo, flatFlag);
                var result = RangeHelper.ConvertArrayToRange(retArray);
                return result;
            }

            /// <summary>
            /// Gets the orc vol.
            /// </summary>
            /// <param name="k">The k.</param>
            /// <param name="timeToMaturity">The time to maturity.</param>
            /// <param name="atm">The atm.</param>
            /// <param name="currentVol">The current vol.</param>
            /// <param name="slopeRef">The slope ref.</param>
            /// <param name="putCurve">The put curve.</param>
            /// <param name="callCurve">The call curve.</param>
            /// <param name="dnCutOff">The dn cut off.</param>
            /// <param name="upCutOff">Up cut off.</param>
            /// <param name="vcr">The VCR.</param>
            /// <param name="scr">The SCR.</param>
            /// <param name="ssr">The SSR.</param>
            /// <param name="dsr">The DSR.</param>
            /// <param name="usr">The usr.</param>
            /// <param name="refFwd">The ref FWD.</param>
            /// <returns></returns>
            public static double GetWingVol(double k, double timeToMaturity, double atm, double currentVol, double slopeRef, double putCurve, double callCurve,
                                    double dnCutOff, double upCutOff, double vcr, double scr, double ssr, double dsr, double usr,
                                    double refFwd)
            {
                return EquitiesLibrary.GetWingVol(k, timeToMaturity, atm, currentVol, slopeRef, putCurve, callCurve,
                                    dnCutOff, upCutOff, vcr, scr, ssr, dsr, usr, refFwd);
            }

            /// <summary>
            /// Gets the ATM forward.
            /// </summary>
            /// <param name="spot"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <returns></returns>
            public static double GetForward(DateTime today, DateTime expiry, double spot, object zeroDatesAsArray, object zeroRatesAsArray,
                object divDatesAsArray, object divAmsAsArray)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                return EquitiesLibrary.GetForward(today, expiry, spot, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray());
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="valueDate"></param>
            /// <param name="datesAsArray"></param>
            /// <param name="amountsAsArray"></param>
            /// <param name="interpolationType"></param>
            /// <returns></returns>
            public static double InterpolateOnDates(DateTime valueDate, object datesAsArray, object amountsAsArray, string interpolationType)
            {
                //At this stage only linear interpolation is supported
                var dates = DataRangeHelper.StripDateTimeRange(datesAsArray);
                var amounts = DataRangeHelper.StripDoubleRange(amountsAsArray);
                return EquitiesLibrary.InterpolateOnDates(valueDate, dates.ToArray(), amounts.ToArray(), interpolationType);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="xValue"></param>
            /// <param name="xAsArray"></param>
            /// <param name="yAsArray"></param>
            /// <param name="interpolationType"></param>
            /// <returns></returns>
            public static double InterpolateOnValues(double xValue, object xAsArray, object yAsArray, string interpolationType)
            {
                //At this stage only linear interpolation is supported
                var x = DataRangeHelper.StripDoubleRange(xAsArray);
                var y = DataRangeHelper.StripDoubleRange(yAsArray);
                return EquitiesLibrary.InterpolateOnValues(xValue, x.ToArray(), y.ToArray(), interpolationType);
            }
  
            /// <summary>
            /// Returns the PV (as at the valueDate) of the payment stream.
            /// Only payments occuring on or between valueDate and finalDate are included in the sum.
            /// All other payments are ignored.
            /// </summary>
            /// <param name="valueDate">The date at which the PV is taken.</param>
            /// <param name="paymentDatesAsArray">The dates on which payments are made, in ascending order.</param>
            /// <param name="paymentAmountsAsArray">The amounts of payments.</param>
            /// <param name="zeroDatesAsArray">The dates corresponding to the ZCB discount curve, in ascending order.</param>
            /// <param name="zeroRatesAsArray">The rates corresponding to the ZCB discount curve.</param>
            /// <param name="finalDate">The final date on which payments are to be included.</param>
            /// <returns>A double representing the PV.</returns>
            public static double PVofPaymentStream(DateTime valueDate, DateTime finalDate, object paymentDatesAsArray, object paymentAmountsAsArray,
                object zeroDatesAsArray, object zeroRatesAsArray)
            {
                var paymentDates = DataRangeHelper.StripDateTimeRange(paymentDatesAsArray);
                var paymentAmounts = DataRangeHelper.StripDoubleRange(paymentAmountsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                return EquitiesLibrary.PVofPaymentStream(valueDate, finalDate, paymentDates.ToArray(), paymentAmounts.ToArray(), zeroDates.ToArray(), zeroRates.ToArray());
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="rateAsArray"></param>
            /// <param name="compoundingFrequency"></param>
            /// <returns></returns>
            public static object[,] ConvToContinuousRate(object rateAsArray, string compoundingFrequency)
            {
                var rate = DataRangeHelper.StripDoubleRange(rateAsArray);
                var retArray = EquitiesLibrary.ConvToContinuousRate(rate.ToArray(), compoundingFrequency);
                var result = RangeHelper.ConvertArrayToRange(retArray);
                return result;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="baseAmount"></param>
            /// <param name="valueDate"></param>
            /// <param name="finalDatesAsArray"></param>
            /// <param name="paymentDatesAsArray"></param>
            /// <param name="paymentAmountsAsArray"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray"></param>
            /// <returns></returns>
            public static object[,] DivYield(double baseAmount, DateTime valueDate, object finalDatesAsArray, object paymentDatesAsArray,
                object paymentAmountsAsArray, object zeroDatesAsArray, object zeroRatesAsArray)
            {
                var finalDates = DataRangeHelper.StripDateTimeRange(finalDatesAsArray);
                var paymentDates = DataRangeHelper.StripDateTimeRange(paymentDatesAsArray);
                var paymentAmounts = DataRangeHelper.StripDoubleRange(paymentAmountsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var retArray = EquitiesLibrary.DivYield(baseAmount, valueDate, finalDates.ToArray(), paymentDates.ToArray(), paymentAmounts.ToArray(), zeroDates.ToArray(), zeroRates.ToArray());
                var result = RangeHelper.ConvertArrayToRange(retArray);
                return result;
            }

        #endregion
    }
}
