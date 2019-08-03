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

#region Usings

using System;
using System.Collections.Generic;
using Orion.EquitiesCore;
using DivList = Orion.Analytics.Equities.DivList;
using Wrapper = Orion.EquityCollarPricer.Helpers.Wrapper;
using ZeroCurve = Orion.Analytics.Rates.ZeroCurve;

#endregion

namespace Orion.EquityCollarPricer
{
    /// <summary>
    /// Represents an Equity Collar to be priced
    /// </summary>
    public class Collar: ICollarPricer
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="spot"></param>
        /// <param name="callStrike"></param>
        /// <param name="style"></param>
        /// <param name="tradeDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="zeroArray"></param>
        /// <param name="divArray"></param>
        /// <param name="orcParams"></param>
        /// <returns></returns>
        public static double PutCollarPricer(String underlying, double spot, double callStrike,
                           string style, DateTime tradeDate, DateTime expiryDate, List<ZeroCurveRange> zeroArray,
                           List<DividendRange> divArray, WingParamsRange orcParams)
        {
            // Skew;
            WingCurvature[] wingCurve = Wrapper.UnpackWing(orcParams, tradeDate);
            // Dividends;
            DividendList dList = Wrapper.UnpackDiv(divArray);
            // ZeroCurve;
            ZeroAUDCurve zeroCurve = Wrapper.UnpackZero(zeroArray, tradeDate);
            var ist = new Stock(underlying, underlying, dList, wingCurve);
            //test the price 
            var col = new Collar();
            var tr = new TransactionDetail(underlying) { CurrentSpot = spot };
            if (style == "A")
                tr.PayStyle = PayStyleType.American;
            else if (style == "E")
                tr.PayStyle = PayStyleType.European;
            else tr.PayStyle = PayStyleType.NotSpecified;
            tr.TradeDate = tradeDate;
            tr.ExpiryDate = expiryDate;
            ist.Transaction = tr;
            var testSt = new Strike(OptionType.Call, callStrike);
            ist.Transaction.SetStrike(testSt);
            double putStrike = col.FindZeroCostPutStrike(ist, zeroCurve);
            return putStrike;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="underlying"></param>
        /// <param name="spot"></param>
        /// <param name="putStrike"></param>
        /// <param name="style"></param>
        /// <param name="tradeDate"></param>
        /// <param name="expiryDate"></param>
        /// <param name="zeroArray"></param>
        /// <param name="divArray"></param>
        /// <param name="orcParams"></param>
        /// <returns></returns>
        public static double CallCollarPricer(String underlying, double spot, double putStrike,
                                 string style, DateTime tradeDate, DateTime expiryDate, List<ZeroCurveRange> zeroArray,
                                 List<DividendRange> divArray, WingParamsRange orcParams)
        {
            // Skew;
            WingCurvature[] wingCurve = Wrapper.UnpackWing(orcParams, tradeDate);
            // Dividends;
            DividendList dList = Wrapper.UnpackDiv(divArray);
            // ZeroCurve;
            ZeroAUDCurve zeroCurve = Wrapper.UnpackZero(zeroArray, tradeDate);
            var ist = new Stock(underlying, underlying, dList, wingCurve);
            //test the price 
            var col = new Collar();
            var tr = new TransactionDetail(underlying) { CurrentSpot = spot };
            if (style == "A")
                tr.PayStyle = PayStyleType.American;
            else if (style == "E")
                tr.PayStyle = PayStyleType.European;
            else tr.PayStyle = PayStyleType.NotSpecified;
            tr.TradeDate = tradeDate;
            tr.ExpiryDate = expiryDate;
            ist.Transaction = tr;
            //test collar       
            var testSt = new Strike(OptionType.Put, putStrike);
            ist.Transaction.SetStrike(testSt);
            double callStrike = col.FindZeroCostCallStrike(ist, zeroCurve);
            return callStrike;
        }

        /// <summary>
        /// Finds the price.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        public Double FindPrice(Stock stock, ZeroAUDCurve zeroRateCurve)
        {
            // Unpack objects
            ZeroCurve myZero = Wrapper.UnpackZeroRateCurve(zeroRateCurve);
            DivList myDiv = Wrapper.UnpackDividends(stock, zeroRateCurve);
            var myORC = Wrapper.UnpackOrcWingParameters(stock);
            //
            TimeSpan ts = stock.Transaction.ExpiryDate - stock.Transaction.TradeDate;
            double t = ts.Days / 365.0;
            string payFlag = stock.Transaction.PayStyle.ToString().Substring(0, 1).ToUpper();
            string style = stock.Transaction.Strike.Style.ToString().Substring(0, 1).ToUpper();
            double price = Analytics.Equities.Collar.FindPrice(myZero, myDiv, myORC, t,
                                                               stock.Transaction.Strike.StrikePrice, stock.Transaction.CurrentSpot, payFlag, style, 100.0);        
            return price;
        }

        /// <summary>
        /// Finds the zero cost call strike.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        public Double FindZeroCostCallStrike(Stock stock, ZeroAUDCurve zeroRateCurve)
        {
            // Unpack objects
            ZeroCurve myZero = Wrapper.UnpackZeroRateCurve(zeroRateCurve);
            DivList myDiv = Wrapper.UnpackDividends(stock, zeroRateCurve);
            var myORC = Wrapper.UnpackOrcWingParameters(stock);
            //
            TimeSpan ts = stock.Transaction.ExpiryDate - stock.Transaction.TradeDate;
            double t = ts.Days / 365.0;
            string payFlag = stock.Transaction.PayStyle.ToString().Substring(0, 1).ToUpper();
            string style = stock.Transaction.Strike.Style.ToString().Substring(0, 1).ToUpper();
            double price = Analytics.Equities.Collar.FindZeroCostCall(myZero, myDiv, myORC, t,
                                                                      stock.Transaction.Strike.StrikePrice, stock.Transaction.CurrentSpot, payFlag, style, 100.0);
            return price;
        }

        /// <summary>
        /// Finds the zero cost put strike.
        /// </summary>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        public Double FindZeroCostPutStrike(Stock stock, ZeroAUDCurve zeroRateCurve)
        {
            // Unpack objects
            ZeroCurve myZero = Wrapper.UnpackZeroRateCurve(zeroRateCurve);
            DivList myDiv = Wrapper.UnpackDividends(stock, zeroRateCurve);
            var myORC = Wrapper.UnpackOrcWingParameters(stock);
            //
            TimeSpan ts = stock.Transaction.ExpiryDate - stock.Transaction.TradeDate;
            double t = ts.Days / 365.0;
            string payFlag = stock.Transaction.PayStyle.ToString().Substring(0, 1).ToUpper();
            string style = stock.Transaction.Strike.Style.ToString().Substring(0, 1).ToUpper();
            double price = Analytics.Equities.Collar.FindZeroCostPut(myZero, myDiv, myORC, t,
                                                                     stock.Transaction.Strike.StrikePrice, stock.Transaction.CurrentSpot, payFlag, style, 100.0);
            return price;
        }

        /// <summary>
        /// Finds the zero cost strike.
        /// </summary>
        /// <param name="optionType">Type of the option.</param>
        /// <param name="stock">The stock.</param>
        /// <param name="zeroRateCurve">The zero rate curve.</param>
        /// <returns></returns>
        public Double FindZeroCostStrike(OptionType optionType, Stock stock, ZeroAUDCurve zeroRateCurve)
        {
            Double price;
            switch (optionType)
            {
                case OptionType.Call:
                    price = FindZeroCostCallStrike(stock, zeroRateCurve);
                    break;
                case OptionType.Put:
                    price = FindZeroCostPutStrike(stock, zeroRateCurve);
                    break;
                default:
                    throw new ArgumentException("Option Type must be specified");
            }
            return price;
        }
    }
}
