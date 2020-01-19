/*
 Copyright (C) 2019 Alex Watt and Simon Dudley (alexwatt@hotmail.com)

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
using System.Linq;
using Highlander.Equities;
using Highlander.EquityCalculator.TestData.V5r3;
using Highlander.Reporting.Analytics.V5r3.Rates;
using Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities;
using OptionPosition = Highlander.Equities.OptionPosition;
using Stock = Highlander.Reporting.Analytics.V5r3.Equities.Stock;
using VolatilityPoint = Highlander.Equities.VolatilityPoint;
using VolatilitySurface = Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities.VolatilitySurface;
using ForwardExpiry = Highlander.Reporting.Analytics.V5r3.Stochastics.Volatilities.ForwardExpiry;

namespace Highlander.EquitiesCalculator.Tests.V5r3
{
    public class TestHelper
    {
        public static Stock CreateStock(EquityCalculator.TestData.V5r3.Stock stock)
        {
            DateTime today = XmlGetDate(stock.Date);
            decimal spot = Convert.ToDecimal(stock.Spot);
            DateTime baseDate = XmlGetDate(stock.RateCurve.BaseDate);
            DateTime[] rateDates = XmlGetDateArray(stock.RateCurve.DateArray);         
            //Load rate curve
            string tp = stock.RateCurve.RateType;
            var rc = new RateCurve(stock.RateCurve.Ccy,tp , baseDate, rateDates, stock.RateCurve.RateArray);                                                
            // Load dividends
            var divs = (from div in stock.Dividends let exDate = XmlGetDate(div.ExDate) select new Dividend(exDate, Convert.ToDecimal(div.Amount))).ToList();
            //Load stock object
            var stock0 = new Stock(today, spot, stock.AssetId, stock.Name, rc, divs);         
            var vol0 = new VolatilitySurface(stock.AssetId, spot, today);           
            //Load vols
            stock0.VolatilitySurface = vol0;
            foreach (StockVolatilitySurfaceForwardExpiry exp in stock.VolatilitySurface.Expiries)
            {
                DateTime expDate = XmlGetDate(exp.ExpiryDate);
                decimal fwd = Convert.ToDecimal(exp.FwdPrice);                
                var exp0 = new ForwardExpiry(expDate, fwd);
                // exp0.NodalPoint = System.Convert.ToBoolean(exp.NodalPoint);
                vol0.AddExpiry(exp0);

                foreach (StockVolatilitySurfaceForwardExpiryStrike str in exp.Strikes)
                {
                    var call = new OptionPosition();
                    var put = new OptionPosition();
                    double strikePrice0 = Convert.ToDouble(str.StrikePrice);
                    var str0 = new EquityStrike(strikePrice0,call,put,Units.Cents)
                        {
                            Moneyness = Convert.ToDouble(str.Moneyness)
                        };
                    exp0.AddStrike(str0, true);
                    var vp = new VolatilityPoint();
                    decimal vol = Convert.ToDecimal(str.Volatility.Value);
                    vp.SetVolatility(vol, VolatilityState.Default());
                    str0.SetVolatility(vp);
                }              
            }
            return stock0;                                                                                             
        }

        /// <summary>
        /// XMLs the get date.
        /// </summary>
        /// <param name="date">The date.</param>
        /// <returns></returns>
        internal static DateTime XmlGetDate(string date)
        {
            DateTime.TryParse(date, out DateTime today);
            if (today != DateTime.MinValue)
                return today;         
            throw new Exception("Test helper: Xml Date could not be read");
        }


        /// <summary>
        /// XMLs the get date array.
        /// </summary>
        /// <param name="dates">The dates.</param>
        /// <returns></returns>
        internal static DateTime[] XmlGetDateArray(string[] dates)
        {
            var dateArray = new DateTime[dates.Length];            
            for (int idx=0; idx< dates.Length;idx++)
            {
                dateArray[idx] = XmlGetDate(dates[idx]);
            }
            return dateArray;
        }
    }
}
