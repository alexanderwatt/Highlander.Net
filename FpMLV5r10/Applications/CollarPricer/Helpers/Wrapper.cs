#region Usings

using System;
using System.Collections.Generic;
using Orion.EquitiesCore;
using DivList = Orion.Analytics.Equities.DivList;
using OrcWingVol = Orion.Analytics.Options.OrcWingVol;
using ZeroCurve = Orion.Analytics.Rates.ZeroCurve;

#endregion

namespace Orion.EquityCollarPricer.Helpers
{
    /// <summary>
    /// Core Analytics Wrapper - Abstraction layer between the external Collar Pricer interfaces and the internal analytics
    /// </summary>
    public class Wrapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="orcParams"></param>
        /// <param name="tradeDate"></param>
        /// <returns></returns>
        public static WingCurvature[] UnpackWing(WingParamsRange orcParams, DateTime tradeDate)
        {
            var wc = new[] { new WingCurvature() };
            //set the curvature
            wc[0].EtoDate = tradeDate;
            wc[0][WingCurvature.WingCurvatureProperty.CurrentVolatility] = orcParams.CurrentVolatility;
            wc[0][WingCurvature.WingCurvatureProperty.SlopeReference] = orcParams.SlopeReference;
            wc[0][WingCurvature.WingCurvatureProperty.PutCurvature] = orcParams.PutCurvature;
            wc[0][WingCurvature.WingCurvatureProperty.CallCurvature] = orcParams.CallCurvature;
            wc[0][WingCurvature.WingCurvatureProperty.DownCutOff] = orcParams.DownCutOff;
            wc[0][WingCurvature.WingCurvatureProperty.UpCutOff] = orcParams.UpCutOff;
            wc[0][WingCurvature.WingCurvatureProperty.VolChangeRate] = orcParams.VolChangeRate;
            wc[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = orcParams.SlopeChangeRate;
            wc[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = orcParams.SkewSwimmingnessRate;
            wc[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = orcParams.DownSmoothingRange;
            wc[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = orcParams.UpSmoothingRange;
            wc[0][WingCurvature.WingCurvatureProperty.ReferenceForward] = orcParams.ReferenceForward;
            return wc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="divArray"></param>
        /// <returns></returns>
        public static DividendList UnpackDiv(List<DividendRange> divArray)
        {
            //map in the dividend curve
            var dList = new DividendList();
            foreach (DividendRange divRange in divArray)
            {
                DateTime date = divRange.DivDate;
                var dividend = new Dividend(date, date, divRange.DivAmt, "AUD");
                dList.Add(dividend);
            }
            return dList;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="zeroArray"></param>
        /// <param name="tradeDate"></param>
        /// <returns></returns>
        public static ZeroAUDCurve UnpackZero(List<ZeroCurveRange> zeroArray, DateTime tradeDate)
        {
            var tDates = new List<DateTime>();
            var tRates = new List<double>();
            foreach (ZeroCurveRange zeroCurveRange in zeroArray)
            {
                DateTime date = zeroCurveRange.RateDate;
                double rate = zeroCurveRange.Rate;
                tDates.Add(date);
                tRates.Add(rate);
            }
            var zc = new ZeroAUDCurve(tradeDate, tDates, tRates);
            return zc;
        }

        /// <summary>
        /// Unpacks the zero rate curve.
        /// </summary>
        /// <param name="inZero">The in zero.</param>
        /// <returns></returns>
        public static ZeroCurve UnpackZeroRateCurve(ZeroAUDCurve inZero)
        {
            var myZero = new ZeroCurve {Ratepoints = inZero.Rates.Length};
            myZero.MakeArrays();
            int idx = 0;
            foreach(ZeroRate dt in inZero.ZeroRates)
            {
                TimeSpan ts = dt.TenorDate - inZero.CurveDate;
                myZero.SetR(idx, dt.Rate, (ts.Days / 365.0));
                idx++;
            }
            return myZero;
        }


        /// <summary>
        /// Unpacks the dividends.
        /// </summary>
        /// <param name="inStock">The in stock.</param>
        /// <param name="inZero">The in zero.</param>
        /// <returns></returns>
        public static DivList UnpackDividends(Stock inStock, ZeroAUDCurve inZero)
        {
            var myDiv = new DivList {Divpoints = inStock.Dividends.Count};
            myDiv.MakeArrays();
            int idx = 0;
            foreach (Dividend dt in inStock.Dividends)
            {
                TimeSpan ts = dt.ExDivDate - inZero.CurveDate;
                myDiv.SetD(idx, dt.PaymentAmountInCents, (ts.Days / 365.0));
                idx++;
            }
            return myDiv;
        }


        /// <summary>
        /// Unpacks the orc wing parameters.
        /// </summary>
        /// <param name="inStock">The in stock.</param>
        /// <returns></returns>
        public static OrcWingVol UnpackOrcWingParameters(Stock inStock)
        {
            var myORC = new OrcWingVol();
            WingCurvature [] wn = inStock.WingCurvature;
            if (wn.Length == 2)
            {
                //  compute the fraction
                TimeSpan ts = inStock.Transaction.ExpiryDate - wn[0].EtoDate;
                TimeSpan ts2 = wn[1].EtoDate - wn[0].EtoDate;
                double al = ts.Days / ((double)ts2.Days);
                myORC.callCurve = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.CallCurvature]
                                   + al * wn[1][WingCurvature.WingCurvatureProperty.CallCurvature];
                myORC.putCurve = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.PutCurvature]
                                   + al * wn[1][WingCurvature.WingCurvatureProperty.PutCurvature];
                myORC.refVol = myORC.currentVol = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.CurrentVolatility]
                                   + al * wn[1][WingCurvature.WingCurvatureProperty.CurrentVolatility];
                myORC.dsc = myORC.dnCutoff = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.DownCutOff]
                                  + al *  wn[1][WingCurvature.WingCurvatureProperty.DownCutOff];
                myORC.dsr = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange]
                                  + al *  wn[1][WingCurvature.WingCurvatureProperty.DownSmoothingRange];
                myORC.usr = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange]
                                 + al * wn[1][WingCurvature.WingCurvatureProperty.UpSmoothingRange];
                myORC.usc = myORC.upCutoff = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.UpCutOff]
                                 + al *  wn[1][WingCurvature.WingCurvatureProperty.UpCutOff];
                myORC.scr = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate]
                                 + al *  wn[1][WingCurvature.WingCurvatureProperty.SlopeChangeRate];
                myORC.slopeRef = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.SlopeReference]
                                 + al * wn[1][WingCurvature.WingCurvatureProperty.SlopeReference];
                myORC.ssr = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate]
                                 + al * wn[1][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate];
                myORC.refFwd = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.ReferenceForward]
                                 + al * wn[1][WingCurvature.WingCurvatureProperty.ReferenceForward];
                myORC.vcr = (1.0 - al) * wn[0][WingCurvature.WingCurvatureProperty.VolChangeRate]
                                 + al * wn[1][WingCurvature.WingCurvatureProperty.VolChangeRate];
            }
            else
            {
                myORC.callCurve =wn[0][WingCurvature.WingCurvatureProperty.CallCurvature];
                myORC.putCurve = wn[0][WingCurvature.WingCurvatureProperty.PutCurvature];
                myORC.refVol = myORC.currentVol = wn[0][WingCurvature.WingCurvatureProperty.CurrentVolatility];
                myORC.dsc = myORC.dnCutoff = wn[0][WingCurvature.WingCurvatureProperty.DownCutOff];
                myORC.dsr = wn[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange];
                myORC.usr = wn[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange];
                myORC.usc = myORC.upCutoff = wn[0][WingCurvature.WingCurvatureProperty.UpCutOff];
                myORC.scr = wn[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate];
                myORC.slopeRef = wn[0][WingCurvature.WingCurvatureProperty.SlopeReference];
                myORC.ssr = wn[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate];
                myORC.refFwd = wn[0][WingCurvature.WingCurvatureProperty.ReferenceForward];
                myORC.vcr = wn[0][WingCurvature.WingCurvatureProperty.VolChangeRate];
            }
            return myORC;
        }
    }
}
