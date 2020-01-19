using System;
using System.Linq;
using FpML.V5r10.EquityVolatilityCalculator;
using Orion.EquityVolCalcTestData;
using Stock = FpML.V5r10.EquityVolatilityCalculator.Stock;

namespace FpML.V5r10.EquitiesVolCalcTests
{
    public class TestHelper
    {
        public static Stock CreateStock(Orion.EquityVolCalcTestData.Stock stock)
        {
            DateTime today = XmlGetDate(stock.Date);
            decimal spot = Convert.ToDecimal(stock.Spot);
            DateTime baseDate = XmlGetDate(stock.RateCurve.BaseDate);
            DateTime[] rateDates = XmlGetDateArray(stock.RateCurve.DateArray);         
            //Load rate curve
            String tp = stock.RateCurve.RateType;
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
                Decimal fwd = Convert.ToDecimal(exp.FwdPrice);                
                var exp0 = new ForwardExpiry(expDate, fwd);
                // exp0.NodalPoint = System.Convert.ToBoolean(exp.NodalPoint);
                vol0.AddExpiry(exp0);

                foreach (StockVolatilitySurfaceForwardExpiryStrike str in exp.Strikes)
                {
                    var call = new OptionPosition();
                    var put = new OptionPosition();
                    double strikeprice0 = Convert.ToDouble(str.StrikePrice);
                    var str0 = new Strike(strikeprice0,call,put,Units.Cents)
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
        internal static DateTime XmlGetDate(String date)
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
