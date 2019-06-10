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

#region Using directives

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using HLV5r3.Helpers;
using Microsoft.Win32;
using Orion.Analytics.Counterparty;
using Orion.Analytics.Equities;
using Orion.Analytics.Helpers;
using Orion.CurveEngine.Helpers;
using Orion.EquitiesCore;
using Orion.Util.Helpers;
using DivList = Orion.Analytics.Equities.DivList;
using Excel = Microsoft.Office.Interop.Excel;
using ZeroCurve = Orion.Analytics.Rates.ZeroCurve;

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

        //private const int CArrsize = 10000;

        //public object[,] PortfolioOptimizer(Excel.Range portfolioArray)
        //    {
        //        var _portfolioArray = portfolioArray.get_Value(System.Reflection.Missing.Value) as object[,];
        //        if (_portfolioArray != null)
        //        {
        //            int iRrows = _portfolioArray.GetUpperBound(0);
        //            //int iRcols = _PortfolioArray.GetUpperBound(1);
        //            var res = new object[iRrows, 2];
        //            var portfolio = new List<Stock>();
        //            var lp = new LinearProgramming();
        //            for (int idx = 1; idx <= iRrows; idx++)
        //            {
        //                string stockName = Convert.ToString(_portfolioArray[idx, 1]);
        //                var stockDollars = (double)_portfolioArray[idx, 2];
        //                if (stockDollars > 0)
        //                {
        //                    var stock = new Stock(stockName, stockDollars);
        //                    portfolio.Add(stock);
        //                }
        //            }
        //            portfolio.Sort();
        //            lp.Portfolio = portfolio;
        //            double[] solutionVector = lp.CalcFinalOptimum();
        //            var stockArray = new Stock[portfolio.Count];
        //            portfolio.CopyTo(stockArray);
        //            // Excel API expects a zero-based array to be returned;
        //            for (int idx = 0; idx < solutionVector.Length; idx++)
        //            {
        //                res[idx, 0] = stockArray[idx].StockName;
        //                res[idx, 1] = solutionVector[idx].ToString(CultureInfo.InvariantCulture);
        //            }
        //            return res;
        //        }
        //        return null;
        //    }

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
        public double PutCollarPricer(string underlying, double spot, double callStrike,
                                      string style, DateTime tradeDate, DateTime expiryDate,
                                      Excel.Range zeroRange,
                                      Excel.Range divRange, Excel.Range orcParamsRange)
        {
            var values = orcParamsRange.Value[System.Reflection.Missing.Value] as object[,];
            values = (object[,])DataRangeHelper.TrimNulls(values);
            var orcParams = RangeHelper.Convert2DArrayToClass<WingParamsRange>(ArrayHelper.RangeToMatrix(values));
            var zeroArray = zeroRange.Value[System.Reflection.Missing.Value] as object[,];
            var zeroes = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ZeroCurveRange>(zeroArray);
            var divArray = zeroRange.Value[System.Reflection.Missing.Value] as object[,];
            var divs = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DividendRange>(divArray);
            var result = Orion.EquityCollarPricer.Collar.PutCollarPricer(underlying, spot, callStrike,
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
        public double CallCollarPricer(string underlying, double spot, double callStrike,
                                      string style, DateTime tradeDate, DateTime expiryDate,
                                      Excel.Range zeroRange,
                                      Excel.Range divRange, Excel.Range orcParamsRange)
        {
            var values = orcParamsRange.Value[System.Reflection.Missing.Value] as object[,];
            values = (object[,])DataRangeHelper.TrimNulls(values);
            var orcParams = RangeHelper.Convert2DArrayToClass<WingParamsRange>(ArrayHelper.RangeToMatrix(values));
            var zeroArray = zeroRange.Value[System.Reflection.Missing.Value] as object[,];
            var zeroes = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<ZeroCurveRange>(zeroArray);
            var divArray = zeroRange.Value[System.Reflection.Missing.Value] as object[,];
            var divs = ObjectToArrayOfPropertiesConverter.CreateListFromHorizontalArrayRange<DividendRange>(divArray);
            var result = Orion.EquityCollarPricer.Collar.CallCollarPricer(underlying, spot, callStrike,
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
            /// <param name="dexp"></param>
            /// <param name="sPay"></param>
            /// <param name="sStyle"></param>
            /// <param name="nGrid"></param>
            /// <param name="tStep"></param>
            /// <param name="divamsAsArray"></param>
            /// <param name="divdatesAsArray"></param>
            /// <param name="zeramsAsArray"></param>
            /// <param name="zerdatesAsArray"></param>
            /// <returns></returns>
            public double GetEquityPrice(DateTime today,
                double spot,
                double strike,
                double sig,
                DateTime dexp,
                string sPay,
                string sStyle,
                int nGrid,
                double tStep,
                Excel.Range divamsAsArray,
                Excel.Range divdatesAsArray,
                Excel.Range zeramsAsArray,
                Excel.Range zerdatesAsArray
              )
            {
                //Map the ranges
                var divams = DataRangeHelper.StripDoubleRange(divamsAsArray);
                var divdates = DataRangeHelper.StripDateTimeRange(divamsAsArray);
                var zerams = DataRangeHelper.StripDoubleRange(zeramsAsArray);
                var zerdates = DataRangeHelper.StripDateTimeRange(zerdatesAsArray);
                //set up the DivList
                int nd = divdates.Count;//GetUpperBound(0) + 1;
                var myDiv = new DivList {Divpoints = nd};
                myDiv.MakeArrays();
                for (int idx = 0; idx != nd; idx++)
                {
                    double r = divams[idx];
                    DateTime dp = divdates[idx];
                    TimeSpan ts = dp - today;
                    myDiv.SetD(idx, r, ts.Days / 365.0);
                }
                //set up the zero
                int nz = zerdates.Count;//GetUpperBound(0) + 1;
                var myZero = new ZeroCurve {Ratepoints = nz};
                myZero.MakeArrays();
                for (int idx = 0; idx != nz; idx++)
                {
                    double r = zerams[idx];
                    DateTime dp = zerdates[idx];
                    TimeSpan ts = dp - today;
                    myZero.SetR(idx, r, ts.Days / 365.0);
                }
                //compute the discounted dividends to  expiry and work out continuous
                double sum = 0.0;
                TimeSpan tsE = dexp - today;
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
            /// <param name="dexp"></param>
            /// <param name="sPay"></param>
            /// <param name="sStyle"></param>
            /// <param name="nGrid"></param>
            /// <param name="tStep"></param>
            /// <param name="divamsAsArray"></param>
            /// <param name="divdatesAsArray"></param>
            /// <param name="zeramsAsArray"></param>
            /// <param name="zerdatesAsArray"></param>
            /// <returns></returns>
            public double GetEquityImpliedVol(double pTarget, DateTime today,
                double spot,
                double strike,
                double sig,
                DateTime dexp,
                string sPay,
                string sStyle,
                int nGrid,
                double tStep,
                Excel.Range divamsAsArray,
                Excel.Range divdatesAsArray,
                Excel.Range zeramsAsArray,
                Excel.Range zerdatesAsArray
              )
            {
                double sG = sig;
                for (int idx = 0; idx != 25; idx++)
                {
                    double res = GetEquityPrice(today, spot, strike, sG, dexp, sPay, sStyle, nGrid, tStep, divamsAsArray, divdatesAsArray, zeramsAsArray, zerdatesAsArray)
                        - pTarget;
                    if (Math.Abs(res) < 0.00001) { return sG; }
                    var resUp = GetEquityPrice(today, spot, strike, sG + 0.001, dexp, sPay, sStyle, nGrid, tStep, divamsAsArray, divdatesAsArray, zeramsAsArray, zerdatesAsArray)
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
            /// <param name="dexp"></param>
            /// <param name="sPay"></param>
            /// <param name="sStyle"></param>
            /// <param name="nGrid"></param>
            /// <param name="tStep"></param>
            /// <param name="iNum"></param>
            /// <param name="divamsAsArray"></param>
            /// <param name="divdatesAsArray"></param>
            /// <param name="zeramsAsArray"></param>
            /// <param name="zerdatesAsArray"></param>
            /// <returns></returns>
            public double GetEquityGreeks(DateTime today,
                double spot,
                double strike,
                double sig,
                DateTime dexp,
                string sPay,
                string sStyle,
                int nGrid,
                double tStep,
                int iNum,
                Excel.Range divamsAsArray,
                Excel.Range divdatesAsArray,
                Excel.Range zeramsAsArray,
                Excel.Range zerdatesAsArray
              )
            {
                //Map the ranges
                var divams = DataRangeHelper.StripDoubleRange(divamsAsArray);
                var divdates = DataRangeHelper.StripDateTimeRange(divamsAsArray);
                var zerams = DataRangeHelper.StripDoubleRange(zeramsAsArray);
                var zerdates = DataRangeHelper.StripDateTimeRange(zerdatesAsArray);
                //set up the DivList
                int nd = divdates.Count;//GetUpperBound(0) + 1;
                var myDiv = new DivList {Divpoints = nd};
                myDiv.MakeArrays();
                for (int idx = 0; idx != nd; idx++)
                {
                    double r = divams[idx];
                    DateTime dp = divdates[idx];
                    TimeSpan ts = dp - today;
                    myDiv.SetD(idx, r, ts.Days / 365.0);
                }
                //set up the zero
                int nz = zerdates.Count;//GetUpperBound(0) + 1;
                var myZero = new ZeroCurve {Ratepoints = nz};
                myZero.MakeArrays();
                for (int idx = 0; idx != nz; idx++)
                {
                    double r = zerams[idx];
                    DateTime dp = zerdates[idx];
                    TimeSpan ts = dp - today;
                    myZero.SetR(idx, r, ts.Days / 365.0);
                }
                //compute the discounted dividends to  expiry and work out continuous
                TimeSpan tsE = dexp - today;
                double texp = tsE.Days / 365.0;
                for (int idx = 0; idx != nd; idx++)
                {
                    if (myDiv.GetT(idx) <= texp)
                    {
                        double d = myDiv.GetD(idx)*Math.Exp(-myDiv.GetT(idx)*myZero.LinInterp(myDiv.GetT(idx)));
                    }
                }
                //double qc = -Math.Log((spot - sum) / spot) / texp;
                //double rc = myZero.linInterp(texp);
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
                double price = myG.Pricer(myZero, myDiv, ref greeks, true);
                myG.Sig += 0.01;
                double priceUp = myG.Pricer(myZero, myDiv, ref greeks, false);
                greeks[3] = priceUp - price;
                return greeks[iNum];
            }

            ///<summary>
            ///</summary>
            ///<param name="ratedaysAsArray"></param>
            ///<param name="rateamtsAsArray"></param>
            ///<param name="divdaysAsArray"></param>
            ///<param name="divamtsAsArray"></param>
            ///<param name="voltimesAsArray"></param>
            ///<param name="volatilitiesAsRange"></param>
            ///<param name="spot"></param>
            ///<param name="callstrike"></param>
            ///<param name="putstrike"></param>
            ///<param name="maturity"></param>
            ///<param name="kappa"></param>
            ///<param name="theta"></param>
            ///<param name="sigma"></param>
            ///<param name="profiletimes"></param>
            ///<param name="confidence"></param>
            ///<param name="tstepSize"></param>
            ///<param name="simulations"></param>
            ///<param name="seed"></param>
            ///<returns></returns>
            public double[,] GetCollarPCE(Excel.Range ratedaysAsArray,
                                                Excel.Range rateamtsAsArray,
                                                Excel.Range divdaysAsArray,
                                                Excel.Range divamtsAsArray,
                                                Excel.Range voltimesAsArray,
                                                Excel.Range volatilitiesAsRange,
                                                double spot,
                                                double callstrike,
                                                double putstrike,
                                                double maturity,
                                                double kappa,
                                                double theta,
                                                double sigma,
                                                Excel.Range profiletimes, 
                                                double confidence,                                
                                                double tstepSize,
                                                int simulations,
                                                int seed)
            {
                //Map Ranges
                var rateamts = DataRangeHelper.StripDoubleRange(rateamtsAsArray);
                var divamts = DataRangeHelper.StripDoubleRange(divamtsAsArray);
                var voltimes = DataRangeHelper.StripDoubleRange(voltimesAsArray);
                var divdays = DataRangeHelper.StripIntRange(divdaysAsArray);
                var ratedays = DataRangeHelper.StripIntRange(ratedaysAsArray);
                var profile = DataRangeHelper.StripDoubleRange(profiletimes);
                var volatilities = volatilitiesAsRange.Value[System.Reflection.Missing.Value] as object[,];
                var volatilitiesAsDoubles = RangeHelper.RangeToDoubleMatrix(volatilities);
                List<OrcWingParameters> volSurf = UnpackWing(volatilitiesAsDoubles, voltimes.ToArray(), spot, ratedays.ToArray(), rateamts.ToArray(), divdays.ToArray(), divamts.ToArray());          
                CleanDivs(ref divdays, ref divamts);
                double[,] results = EquityPCEAnalytics.GetCollarPCE("CollarPCE",
                                                  ratedays.ToArray(),
                                                  rateamts.ToArray(),
                                                  divdays.ToArray(),
                                                  divamts.ToArray(),
                                                  volSurf,
                                                  spot,
                                                  callstrike,
                                                  putstrike,
                                                  maturity,
                                                  kappa,
                                                  theta,
                                                  sigma,
                                                  profile.ToArray(),
                                                  confidence,
                                                  tstepSize,
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
            ///<param name="vols"></param>
            ///<param name="days"></param>
            ///<param name="spot"></param>
            ///<param name="ratedays"></param>
            ///<param name="rateamts"></param>
            ///<param name="divdays"></param>
            ///<param name="divamts"></param>
            ///<returns></returns>
            private List<OrcWingParameters> UnpackWing(double[,] vols, double[] days, double spot, int[] ratedays, double[] rateamts, int[] divdays, double[] divamts)
            {
                const double daybasis = 365.0;
                int rows = vols.GetLength(0);
                var opList = new List<OrcWingParameters>();
                for (int idx = 0; idx < rows; idx++)
                {
                    double fwd = EquityAnalytics.GetForwardCCLin365(spot, days[idx]/daybasis, divdays, divamts, ratedays, rateamts);
                    var op = new OrcWingParameters
                                 {
                        AtmForward = fwd,
                        CurrentVol = vols[idx, 0],
                        SlopeRef = vols[idx, 1],
                        PutCurve = vols[idx, 2],
                        CallCurve = vols[idx, 3],
                        DnCutoff = vols[idx, 4],
                        UpCutoff = vols[idx, 5],
                        Vcr = vols[idx, 6],
                        Scr = vols[idx, 7],
                        Ssr = 100*vols[idx, 8],
                        Dsr = vols[idx, 9],
                        Usr = vols[idx, 10],
                        RefVol = vols[idx, 11],
                        RefFwd = fwd,
                        TimeToMaturity = days[idx]/daybasis
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
            public DivList UnpackDiv(DateTime today, DateTime expiry, DateTime[] dates, double[] amts)
            {
                int n1 = dates.Length;
                int n2 = dates.Length;
                double timetoexp = expiry.Subtract(today).Days / 365.0;
                if (n1 != n2) throw new Exception("Rate ranges must be of the same length");
                var dl = new DivList {Divpoints = n1};
                dl.MakeArrays();
 
                for (int idx = 0; idx < n1; idx++)
                {
                    double time = dates[idx].Subtract(today).Days / 365.0;
                    double rate = amts[idx];
                    if (time > 0 & time <= timetoexp)
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
            /// <param name="divdays"></param>
            /// <param name="divamts"></param>
            private static void CleanDivs(ref List<int> divdays, ref List<double> divamts)
            {
	            var indices = new List<int>();
	            for (int idx =0 ; idx< divdays.Count; idx++)
	            {
		            if (divdays[idx]>0)
			            indices.Add(idx);
	            }
                var divDaysCopy = new List<int>();
                var divAmtsCopy = new List<double>();        
                foreach (int idx in indices)
                {
                    divDaysCopy.Add(divdays[idx]);
                    divAmtsCopy.Add(divamts[idx]);
                
                }
                divdays.AddRange(divDaysCopy);
                divamts.AddRange(divAmtsCopy);
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
            /// <param name="paystyle">The paystyle.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridsteps">The gridsteps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <returns></returns>
            public double BinomialPricer(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
            string paystyle, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, Excel.Range divDatesAsArray, Excel.Range divAmsAsArray, double gridsteps, string smoo, bool flatFlag)
            {
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BinomialPricer(style, spot, strike, vol, today, expiry,
                paystyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridsteps, smoo, flatFlag);
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
            /// <param name="paystyle">The paystyle.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridsteps">The gridsteps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <returns></returns>
            public double BinomialRelativePricer(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
            string paystyle, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, Excel.Range divDatesAsArray, Excel.Range divAmsAsArray, double gridsteps, string smoo, bool flatFlag)
            {
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BinomialRelativePricer(style, spot, strike, vol, today, expiry, paystyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridsteps, smoo, flatFlag);
                return pr;
            }

            /// <summary>
            /// Gets the Black Scholes price.
            /// </summary>
            /// <param name="spot">The spot.</param>
            /// <param name="strike">The strike.</param>
            /// <param name="vol">The vol.</param>
            /// <param name="paystyle">The paystyle.</param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <returns></returns>
            public double BlackScholesPricer(double spot, double strike, double vol, string paystyle, DateTime today, DateTime expiry,
            Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, Excel.Range divDatesAsArray, Excel.Range divAmsAsArray)
            {
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BlackScholesPricer(spot, strike, vol, paystyle, today, expiry, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray());
                return pr;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="spot"></param>
            /// <param name="strike"></param>
            /// <param name="vol"></param>
            /// <param name="paystyle"></param>
            /// <param name="today"></param>
            /// <param name="expiry"></param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray"></param>
            /// <param name="q"></param>
            /// <returns></returns>
            public double BlackScholesPricerContDiv(double spot, double strike, double vol, string paystyle, DateTime today,
            DateTime expiry, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, double q)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                double pr = EquitiesLibrary.BlackScholesPricerContDiv(spot, strike, vol, paystyle, today, expiry, zeroDates.ToArray(), zeroRates.ToArray(), q);
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
            /// <param name="paystyle">The paystyle.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="tol">The TOL.</param>
            /// <param name="step">The STEP.</param>
            /// <param name="gridsteps">The gridsteps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <returns></returns>
            public double BinomialImpVol(double price, string style, double spot, double strike, double vol0, DateTime today, DateTime expiry,
            string paystyle, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, Excel.Range divDatesAsArray, Excel.Range divAmsAsArray, 
                double tol, double step, double gridsteps, string smoo, bool flatFlag)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                double pr = EquitiesLibrary.BinomialImpVol(price, style, spot, strike, vol0, today, expiry, paystyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), tol, step, gridsteps, smoo, flatFlag);
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
            /// <param name="paystyle">The paystyle.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridsteps">The gridsteps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <param name="today"></param>
            /// <returns></returns>
            public object[,] BinomialGetGreeks(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
            string paystyle, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, Excel.Range divDatesAsArray, Excel.Range divAmsAsArray, double gridsteps, string smoo, bool flatFlag)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var retArray = EquitiesLibrary.BinomialGetGreeks(style, spot, strike, vol, today, expiry, paystyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridsteps, smoo, flatFlag);
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
            /// <param name="paystyle">The paystyle.</param>
            /// <param name="zeroDatesAsArray"></param>
            /// <param name="zeroRatesAsArray">The zero curve.</param>
            /// <param name="divDatesAsArray"></param>
            /// <param name="divAmsAsArray">The div curve.</param>
            /// <param name="gridsteps">The gridsteps.</param>
            /// <param name="smoo">The smoo.</param>
            /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
            /// <param name="today"></param>
            /// <returns></returns>
            public object[,] BinomialRelativeGetGreeks(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
              string paystyle, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, Excel.Range divDatesAsArray, Excel.Range divAmsAsArray, double gridsteps, string smoo, bool flatFlag)
            {
                var zeroDates = DataRangeHelper.StripDateTimeRange(zeroDatesAsArray);
                var zeroRates = DataRangeHelper.StripDoubleRange(zeroRatesAsArray);
                var divDates = DataRangeHelper.StripDateTimeRange(divDatesAsArray);
                var divAms = DataRangeHelper.StripDoubleRange(divAmsAsArray);
                var retArray = EquitiesLibrary.BinomialRelativeGetGreeks(style, spot, strike, vol, today, expiry, paystyle, zeroDates.ToArray(), zeroRates.ToArray(), divDates.ToArray(), divAms.ToArray(), gridsteps, smoo, flatFlag);
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
            public double GetWingVol(double k, double timeToMaturity, double atm, double currentVol, double slopeRef, double putCurve, double callCurve,
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
            public double GetForward(DateTime today, DateTime expiry, double spot, Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray, 
            Excel.Range divDatesAsArray, Excel.Range divAmsAsArray)
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
            /// <param name="interpType"></param>
            /// <returns></returns>
            public double InterpolateOnDates(DateTime valueDate, Excel.Range datesAsArray, Excel.Range amountsAsArray, string interpType)
            {
                //At this stage only linear interpolation is supported
                var dates = DataRangeHelper.StripDateTimeRange(datesAsArray);
                var amounts = DataRangeHelper.StripDoubleRange(amountsAsArray);
                return EquitiesLibrary.InterpolateOnDates(valueDate, dates.ToArray(), amounts.ToArray(), interpType);
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="xvalue"></param>
            /// <param name="xAsArray"></param>
            /// <param name="yAsArray"></param>
            /// <param name="interpType"></param>
            /// <returns></returns>
            public double InterpolateOnValues(double xvalue, Excel.Range xAsArray, Excel.Range yAsArray, string interpType)
            {
                //At this stage only linear interpolation is supported
                var x = DataRangeHelper.StripDoubleRange(xAsArray);
                var y = DataRangeHelper.StripDoubleRange(yAsArray);
                return EquitiesLibrary.InterpolateOnValues(xvalue, x.ToArray(), y.ToArray(), interpType);
            }
  
            /// <summary>
            /// Returns the PV (as at the valueDate) of the payment stream.
            /// Only payments occuring on or between valueDate and finalDate are included in the sum.
            /// All other payments are ignored.
            /// </summary>
            /// <param name="valueDate">The date at which the PV is taken.</param>
            /// <param name="paymentDatesAsArray">The dates on which payments are made, in ascending order.</param>
            /// <param name="paymentAmountsAsArray">The amounds of payments.</param>
            /// <param name="zeroDatesAsArray">The dates corresponding to the ZCB discount curve, in ascending order.</param>
            /// <param name="zeroRatesAsArray">The rates corresponding to the ZCB discount curve.</param>
            /// <param name="finalDate">The final date on which payments are to be included.</param>
            /// <returns>A double representing the PV.</returns>
            public double PVofPaymentStream(DateTime valueDate, DateTime finalDate, Excel.Range paymentDatesAsArray, Excel.Range paymentAmountsAsArray,
            Excel.Range zeroDatesAsArray, Excel.Range zeroRatesAsArray)
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
            public object[,] ConvToContinuousRate(Excel.Range rateAsArray, string compoundingFrequency)
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
            public object[,] DivYield(double baseAmount, DateTime valueDate, Excel.Range finalDatesAsArray, Excel.Range paymentDatesAsArray,
            Excel.Range paymentAmountsAsArray, Excel.Range zeroDatesAsArray,
            Excel.Range zeroRatesAsArray)
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
