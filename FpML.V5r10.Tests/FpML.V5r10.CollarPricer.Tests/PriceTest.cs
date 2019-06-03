using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Orion.EquityCollarPricer.Tests
{
    [TestClass]
    public class PriceTest
    {
        # region OrcPrices

            const double cPriceTol = 0.5;
            const double cOrcPriceTol = 10.0;
            const double cStrikeTol = 1.0;

            const double cOrcPut = 744.11;
            const double cOrcCall = 748.30;

            const double cOrcOTCPut = 777.25;
            const double cOrcOTCCall = 780.30;

        #endregion

        #region WingParams

            const double cc1 = 0.0;
            const double vc1 = 0.7056;
            const double dc1 = -0.1438;
            const double dsr1 = 0.0;
            const double pc1 = 16.5346;
            const double reffwd1 = 3440.27;
            const double ssr1 = 92.11;
            const double scr1 = 0.0;
            const double sloperef1 = 4.0573;
            const double uc1 = 0.0001;
            const double usr1 = 82.6825;
            const double vcr1 = 0.0;

            const double cc2 = 0.0;
            const double vc2 = 0.8019;
            const double dc2 = -0.1852;
            const double dsr2 = 0.5274;
            const double pc2 = 15.0724;
            const double reffwd2 = 3479.43;
            const double ssr2 = 93.32;
            const double scr2 = 0;
            const double sloperef2 = 4.8195;
            const double uc2 = 0.0001;
            const double usr2 = 0.00;
            const double vcr2 = 0.00;    
                    
        #endregion

        [TestMethod]
        public void OptionPrice()
        {
            //load the strike
            var testSt1 = new Strike(OptionType.Put, 3900.00);

            var tr2 = new TransactionDetail("BHP");
            tr2.SetStrike(testSt1);
            tr2.CurrentSpot = 3900.00;
            tr2.PayStyle = PayStyleType.American;
            tr2.TradeDate = DateTime.Parse("25-Mar-2008");
            tr2.ExpiryDate = DateTime.Parse("26-Mar-2009");


            //set the curvature
            var wc2 = new[] { new WingCurvature() };
            wc2[0].EtoDate = DateTime.Parse("25-Mar-2008");
            wc2[0][WingCurvature.WingCurvatureProperty.CallCurvature] = cc1;
            wc2[0][WingCurvature.WingCurvatureProperty.CurrentVolatility] = vc1;
            wc2[0][WingCurvature.WingCurvatureProperty.DownCutOff] = dc1;
            wc2[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = dsr1;
            wc2[0][WingCurvature.WingCurvatureProperty.PutCurvature] = pc1;
            wc2[0][WingCurvature.WingCurvatureProperty.ReferenceForward] = reffwd1;
            wc2[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = ssr1;
            wc2[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = scr1;
            wc2[0][WingCurvature.WingCurvatureProperty.SlopeReference] = sloperef1;
            wc2[0][WingCurvature.WingCurvatureProperty.UpCutOff] = uc1;
            wc2[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = usr1;
            wc2[0][WingCurvature.WingCurvatureProperty.VolChangeRate] = vcr1;


            DateTime date = DateTime.Parse("01-Sep-2008");
            var dividend1 = new Dividend(date, DateTime.Parse("25-Sep-2008"), 34.5, "AUD");
            date = DateTime.Parse("23-Feb-2009");
            var dividend2 = new Dividend(date, DateTime.Parse("17-Mar-2009"), 36.5, "AUD");
            var dList2 = new DividendList {dividend1, dividend2};

            var ist2 = new Stock("BHP", "BHP", dList2, wc2) {Transaction = tr2};

            //set up the zerocurve

            var tDates2 = new List<DateTime>();
            var tRates2 = new List<double>();

            tDates2.Add(DateTime.Parse("26-Mar-2008"));
            tDates2.Add(DateTime.Parse("26-Apr-2008"));
            tDates2.Add(DateTime.Parse("27-May-2008"));
            tDates2.Add(DateTime.Parse("25-Jun-2008"));
            tDates2.Add(DateTime.Parse("05-Jul-2008"));
            tDates2.Add(DateTime.Parse("04-Oct-2008"));
            tDates2.Add(DateTime.Parse("03-Jan-2009"));
            tDates2.Add(DateTime.Parse("04-Apr-2009"));

            tRates2.Add(0.068646);
            tRates2.Add(0.070701);
            tRates2.Add(0.072238);
            tRates2.Add(0.073009);
            tRates2.Add(0.073086);
            tRates2.Add(0.073792);
            tRates2.Add(0.074177);
            tRates2.Add(0.074375);


            var zc2 = new ZeroAUDCurve(DateTime.Parse("25-Mar-2008"), tDates2, tRates2);

            //test the price 
            var col2 = new Collar();

            //test the pricer - put
            double downsidePutPrice = col2.FindPrice(ist2, zc2);

            //test the pricer - call
            var _testSt1 = new Strike(OptionType.Call, 5379.10);
            tr2.SetStrike(_testSt1);
            double upsideCallPrice = col2.FindPrice(ist2, zc2);

            Assert.IsTrue(upsideCallPrice >= downsidePutPrice - cPriceTol);
            Assert.IsTrue(downsidePutPrice >= upsideCallPrice - cPriceTol);   

           // Orc put price @ 3900 744.11
           // Orc call price @ 5379.10 748.30  
 
            //Test against orc;

            Assert.IsTrue(Math.Abs(upsideCallPrice - cOrcCall) <= cOrcPriceTol);
            Assert.IsTrue(Math.Abs(downsidePutPrice - cOrcPut) <= cOrcPriceTol);
        
        }

        [TestMethod]
        public void CollarPrice()
        {
            //load the strike
            var testSt1 = new Strike(OptionType.Put, 3900.00);

            var tr2 = new TransactionDetail("BHP");
            tr2.SetStrike(testSt1);
            tr2.CurrentSpot = 3900.00;
            tr2.PayStyle = PayStyleType.American;
            tr2.TradeDate = DateTime.Parse("25-Mar-2008");
            tr2.ExpiryDate = DateTime.Parse("26-Mar-2009");


            //set the curvature
            var wc2 = new WingCurvature[1] { new WingCurvature() };
            wc2[0].EtoDate = DateTime.Parse("25-Mar-2008");
            wc2[0][WingCurvature.WingCurvatureProperty.CallCurvature] = cc1;
            wc2[0][WingCurvature.WingCurvatureProperty.CurrentVolatility] = vc1;
            wc2[0][WingCurvature.WingCurvatureProperty.DownCutOff] = dc1;
            wc2[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = dsr1;
            wc2[0][WingCurvature.WingCurvatureProperty.PutCurvature] = pc1;
            wc2[0][WingCurvature.WingCurvatureProperty.ReferenceForward] = reffwd1;
            wc2[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = ssr1;
            wc2[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = scr1;
            wc2[0][WingCurvature.WingCurvatureProperty.SlopeReference] = sloperef1;
            wc2[0][WingCurvature.WingCurvatureProperty.UpCutOff] = uc1;
            wc2[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = usr1;
            wc2[0][WingCurvature.WingCurvatureProperty.VolChangeRate] = vcr1;


            DateTime date = DateTime.Parse("01-Sep-2008");
            var dividend1 = new Dividend(date, DateTime.Parse("25-Sep-2008"), 34.5, "AUD");
            date = DateTime.Parse("23-Feb-2009");
            var dividend2 = new Dividend(date, DateTime.Parse("17-Mar-2009"), 36.5, "AUD");
            var dList2 = new DividendList {dividend1, dividend2};

            var ist2 = new Stock("BHP", "BHP", dList2, wc2) {Transaction = tr2};

            //set up the zerocurve

            var tDates2 = new List<DateTime>();
            var tRates2 = new List<double>();

            tDates2.Add(DateTime.Parse("26-Mar-2008"));
            tDates2.Add(DateTime.Parse("26-Apr-2008"));
            tDates2.Add(DateTime.Parse("27-May-2008"));
            tDates2.Add(DateTime.Parse("25-Jun-2008"));
            tDates2.Add(DateTime.Parse("05-Jul-2008"));
            tDates2.Add(DateTime.Parse("04-Oct-2008"));
            tDates2.Add(DateTime.Parse("03-Jan-2009"));
            tDates2.Add(DateTime.Parse("04-Apr-2009"));

            tRates2.Add(0.068646);
            tRates2.Add(0.070701);
            tRates2.Add(0.072238);
            tRates2.Add(0.073009);
            tRates2.Add(0.073086);
            tRates2.Add(0.073792);
            tRates2.Add(0.074177);
            tRates2.Add(0.074375);

            var zc2 = new ZeroAUDCurve(DateTime.Parse("25-Mar-2008"), tDates2, tRates2);

            //test the price 
            var col2 = new Collar();

            //test collar
            var testSt2 = new Strike(OptionType.Call, 5379.1);
            ist2.Transaction.SetStrike(testSt2);
            double putStrike = col2.FindZeroCostPutStrike(ist2, zc2);

            var testSt3 = new Strike(OptionType.Put, 3900.0);
            ist2.Transaction.SetStrike(testSt3);
            double callStrike = col2.FindZeroCostCallStrike(ist2, zc2);

            Assert.IsTrue(Math.Abs(putStrike - 3900.0) <= cStrikeTol);
            Assert.IsTrue(Math.Abs(callStrike - 5379.1) <= cStrikeTol);


        }

        [TestMethod]
        public void OTCCollar()
        {
            //load the strike
            var testSt1 = new Strike(OptionType.Put, 3900.00);

            //set the curvature
            var wc = new[] { new WingCurvature(), new WingCurvature() };
            wc[0].EtoDate = DateTime.Parse("29-Jan-2009");
            wc[0][WingCurvature.WingCurvatureProperty.CallCurvature] = cc1;
            wc[0][WingCurvature.WingCurvatureProperty.CurrentVolatility] = vc1;
            wc[0][WingCurvature.WingCurvatureProperty.DownCutOff] = dc1;
            wc[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = dsr1;
            wc[0][WingCurvature.WingCurvatureProperty.PutCurvature] = pc1;
            wc[0][WingCurvature.WingCurvatureProperty.ReferenceForward] = reffwd1;
            wc[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = ssr1;
            wc[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = scr1;
            wc[0][WingCurvature.WingCurvatureProperty.SlopeReference] = sloperef1;
            wc[0][WingCurvature.WingCurvatureProperty.UpCutOff] = uc1;
            wc[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = usr1;
            wc[0][WingCurvature.WingCurvatureProperty.VolChangeRate] = vcr1;


            //set the curvature
            wc[1].EtoDate = DateTime.Parse("26-Mar-2009");
            wc[1][WingCurvature.WingCurvatureProperty.CallCurvature] = cc2;
            wc[1][WingCurvature.WingCurvatureProperty.CurrentVolatility] = vc2;
            wc[1][WingCurvature.WingCurvatureProperty.DownCutOff] = dc2;
            wc[1][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = dsr2;
            wc[1][WingCurvature.WingCurvatureProperty.PutCurvature] = pc2;
            wc[1][WingCurvature.WingCurvatureProperty.ReferenceForward] = reffwd2;
            wc[1][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = ssr2;
            wc[1][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = scr2;
            wc[1][WingCurvature.WingCurvatureProperty.SlopeReference] = sloperef2;
            wc[1][WingCurvature.WingCurvatureProperty.UpCutOff] = uc2;
            wc[1][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = usr2;
            wc[1][WingCurvature.WingCurvatureProperty.VolChangeRate] = vcr2;


            var tr2 = new TransactionDetail("BHP");
            tr2.SetStrike(testSt1);           
            tr2.CurrentSpot = 3900.00;
            tr2.PayStyle = PayStyleType.American;
            tr2.TradeDate = DateTime.Parse("25-Mar-2008");
            tr2.ExpiryDate = DateTime.Parse("26-Feb-2009");

            //Interpolate vol params;

            //DateTime eto1 = DateTime.Parse("29-Jan-2009");
            //DateTime eto2 = DateTime.Parse("26-Mar-2009");
            //DateTime otc3 = DateTime.Parse("26-Feb-2009");


            DateTime date = DateTime.Parse("01-Sep-2008");
            var dividend1 = new Dividend(date, DateTime.Parse("25-Sep-2008"), 34.5, "AUD");
            date = DateTime.Parse("23-Feb-2009");
            var dividend2 = new Dividend(date, DateTime.Parse("17-Mar-2009"), 36.5, "AUD");
            var dList2 = new DividendList {dividend1, dividend2};

            var ist2 = new Stock("BHP", "BHP", dList2, wc) {Transaction = tr2};

            //set up the zerocurve

            var tDates2 = new List<DateTime>();
            var tRates2 = new List<double>();

            tDates2.Add(DateTime.Parse("26-Mar-2008"));
            tDates2.Add(DateTime.Parse("26-Apr-2008"));
            tDates2.Add(DateTime.Parse("27-May-2008"));
            tDates2.Add(DateTime.Parse("25-Jun-2008"));
            tDates2.Add(DateTime.Parse("05-Jul-2008"));
            tDates2.Add(DateTime.Parse("04-Oct-2008"));
            tDates2.Add(DateTime.Parse("03-Jan-2009"));
            tDates2.Add(DateTime.Parse("04-Apr-2009"));

            tRates2.Add(0.068646);
            tRates2.Add(0.070701);
            tRates2.Add(0.072238);
            tRates2.Add(0.073009);
            tRates2.Add(0.073086);
            tRates2.Add(0.073792);
            tRates2.Add(0.074177);
            tRates2.Add(0.074375);

            var zc2 = new ZeroAUDCurve(DateTime.Parse("25-Mar-2008"), tDates2, tRates2);

            //test the price 
            var col2 = new Collar();


            //test the pricer - put
            double downsidePutPrice = col2.FindPrice(ist2, zc2);

            //test the pricer - call
            var _testSt1 = new Strike(OptionType.Call, 5299.41);
            tr2.SetStrike(_testSt1);
            double upsideCallPrice = col2.FindPrice(ist2, zc2);

            Assert.IsTrue(Math.Abs(upsideCallPrice - cOrcOTCCall) <= cOrcPriceTol);
            Assert.IsTrue(Math.Abs(downsidePutPrice - cOrcOTCPut) <= cOrcPriceTol);

            //Check the collar price on the above while we are here
             
            var testSt2 = new Strike(OptionType.Call, 5299.41);
            ist2.Transaction.SetStrike(testSt2);
            double putStrike = col2.FindZeroCostPutStrike(ist2, zc2);

            var testSt3 = new Strike(OptionType.Put, 3900.0);
            ist2.Transaction.SetStrike(testSt3);
            double callStrike = col2.FindZeroCostCallStrike(ist2, zc2);

            Assert.IsTrue(Math.Abs(putStrike - 3900.0) <= cStrikeTol);
            Assert.IsTrue(Math.Abs(callStrike - 5299.41) <= cStrikeTol);

        }   

    }
}
