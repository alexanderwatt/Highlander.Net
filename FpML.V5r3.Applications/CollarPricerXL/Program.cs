using System;
using System.Collections.Generic;
using Orion.EquityCollarPricer;

namespace Orion.CollarPricer
{
    class Program
    {
        static void Main(string[] args)
        {
          

          const double tolerance = 10.0;
          //load the strike
          var st = new Strike(OptionType.Call, 100.0);

          var tr = new TransactionDetail("BHP");
          tr.SetStrike(st);
          tr.CurrentSpot = 100.0;
          tr.PayStyle = PayStyleType.European;
          tr.TradeDate = DateTime.Parse("25-Mar-2008");
          tr.ExpiryDate = tr.TradeDate.AddDays(107) ;


          //set the curvature
          var wc = new[] { new WingCurvature() };
          wc[0].EtoDate = DateTime.Parse("25-Mar-2008"); ;
          wc[0][WingCurvature.WingCurvatureProperty.CallCurvature] = -1.1478;
          wc[0][WingCurvature.WingCurvatureProperty.CurrentVolatility] = 0.2417;
          wc[0][WingCurvature.WingCurvatureProperty.DownCutOff] = -.2871;
          wc[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = 0.5;
          wc[0][WingCurvature.WingCurvatureProperty.PutCurvature] = 0.2283;
          wc[0][WingCurvature.WingCurvatureProperty.ReferenceForward] = 100.0;
          wc[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = 100.0;
          wc[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = 0.0;
          wc[0][WingCurvature.WingCurvatureProperty.SlopeReference] = -0.1234;
          wc[0][WingCurvature.WingCurvatureProperty.UpCutOff] = 0.1327;
          wc[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = 0.50;
          wc[0][WingCurvature.WingCurvatureProperty.VolChangeRate] = 0.0;
          

          DateTime d1 = DateTime.Parse("25-Sep-2008");
          var div1 = new Dividend(d1, d1.AddDays(10), 1.0, "AUD");
          d1 = DateTime.Parse("26-Oct-2009");
          var div2 = new Dividend(d1, d1.AddDays(10), 1.0, "AUD");
          var dList = new DividendList {div1, div2};

            var ist = new Stock("BHP", "BHP", dList, wc) {Transaction = tr};

            //set up the zero curve

          var tDates = new List<DateTime>();
          var tRates = new List<double>();

          tDates.Add(DateTime.Parse("25-Jun-2008"));
          tDates.Add(DateTime.Parse("25-Sep-2008"));

          tRates.Add(0.05);
          tRates.Add(0.05);

          var zc = new ZeroAUDCurve(DateTime.Parse("25-Mar-2008"),tDates, tRates);


          //test the price 
          var col = new Collar();


          //test the pricer
          double price = col.FindPrice(ist, zc);

          //test collar
          var st2 = new Strike(OptionType.Call, 102.0);
          ist.Transaction.SetStrike(st2);
          price = col.FindZeroCostPutStrike(ist, zc);


          var st3 = new Strike(OptionType.Put, 98.0);
          ist.Transaction.SetStrike(st3);
          price = col.FindZeroCostCallStrike(ist, zc);


          //Test against an ETO in Orc;

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
            wc2[0][WingCurvature.WingCurvatureProperty.CallCurvature] = 0.0;
          wc2[0][WingCurvature.WingCurvatureProperty.CurrentVolatility] = 0.7056;
          wc2[0][WingCurvature.WingCurvatureProperty.DownCutOff] = -0.1438;
          wc2[0][WingCurvature.WingCurvatureProperty.DownSmoothingRange] = 0.0;
          wc2[0][WingCurvature.WingCurvatureProperty.PutCurvature] = 16.5346;
          wc2[0][WingCurvature.WingCurvatureProperty.ReferenceForward] = 3440.27;
          wc2[0][WingCurvature.WingCurvatureProperty.SkewSwimmingnessRate] = 92.11;
          wc2[0][WingCurvature.WingCurvatureProperty.SlopeChangeRate] = 0.0;
          wc2[0][WingCurvature.WingCurvatureProperty.SlopeReference] = 4.0573;
          wc2[0][WingCurvature.WingCurvatureProperty.UpCutOff] = 0.0001;
          wc2[0][WingCurvature.WingCurvatureProperty.UpSmoothingRange] = 82.6825;
          wc2[0][WingCurvature.WingCurvatureProperty.VolChangeRate] = 0.0;


          DateTime date = DateTime.Parse("01-Sep-2008");
          var dividend1 = new Dividend(date, DateTime.Parse("25-Sep-2008") , 34.5, "AUD");
          date = DateTime.Parse("23-Feb-2009");
          var dividend2 = new Dividend(date, DateTime.Parse("17-Mar-2009"), 36.5, "AUD");
          var dList2 = new DividendList {dividend1, dividend2};

            var ist2 = new Stock("BHP", "BHP", dList2, wc2) {Transaction = tr2};

            //set up the zero curve

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
          double testPrice = col2.FindPrice(ist2, zc2);

          //test the pricer - call
          var _testSt1 = new Strike(OptionType.Call, 5379.10);
          tr2.SetStrike(_testSt1);
          double _testPrice = col2.FindPrice(ist2, zc2);

          //test collar
          var testSt2 = new Strike(OptionType.Call, 5379.10);
          ist2.Transaction.SetStrike(testSt2);
          price = col2.FindZeroCostPutStrike(ist2, zc2);               

          var testSt3 = new Strike(OptionType.Put, 3900.00);
          ist2.Transaction.SetStrike(testSt3);
          price = col2.FindZeroCostCallStrike(ist2, zc2);

          //Specify downside strike of 3900:
          // Orc put price @ 3900 744.11
          // Orc call price @ 5379.10 748.30                                    
        }
    }
}
