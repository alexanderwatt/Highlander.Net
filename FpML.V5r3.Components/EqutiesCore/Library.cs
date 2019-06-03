using System;

namespace Orion.EquitiesCore
{

  /// <summary>
  /// 
  /// </summary>
  public class EquitiesLibrary
  {
          /// <summary>
          /// 
          /// </summary>
          /// <param name="style"></param>
          /// <param name="spot"></param>
          /// <param name="strike"></param>
          /// <param name="vol"></param>
          /// <param name="today"></param>
          /// <param name="expiry"></param>
          /// <param name="paystyle"></param>
          /// <param name="zeroDates"></param>
          /// <param name="zeroRates"></param>
          /// <param name="divDates"></param>
          /// <param name="divAms"></param>
          /// <param name="gridsteps"></param>
          /// <param name="smoo"></param>
          /// <param name="flatFlag"></param>
          /// <returns></returns>
      public static double BinomialPricer(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
          string paystyle, DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms, double gridsteps, string smoo, bool flatFlag)
        {      
          gridsteps = (gridsteps == 0.0) ? 20 : gridsteps;
          int nGridsteps = Convert.ToInt32(gridsteps); 
          string smoothing = (smoo.ToLower().Equals("n")) ? "n" : "y";
          double t = expiry.Subtract(today).Days / 365.0;
          var wrapper = new Wrapper();
          ZeroCurve myZero = wrapper.UnpackZero(today, zeroDates, zeroRates) ;    
          DivList myDiv = wrapper.UnpackDiv(today, expiry,  divDates,divAms);
          //create the tree
          var myTree = new DiscreteTree(t, vol, spot,nGridsteps, true );
          myTree.MakeGrid(myZero, myDiv);
          //create pricer
          var myPrice = new Pricer(strike, paystyle, smoothing, style);
          myPrice.MakeGrid(myTree);
          double pr = myPrice.Price();
          return pr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="spot"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="today"></param>
        /// <param name="expiry"></param>
        /// <param name="paystyle"></param>
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <param name="divDates"></param>
        /// <param name="divAms"></param>
        /// <param name="gridsteps"></param>
        /// <param name="smoo"></param>
        /// <param name="flatFlag"></param>
        /// <returns></returns>
        public static double BinomialRelativePricer(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
          string paystyle, DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms, double gridsteps, string smoo, bool flatFlag)
        {
            gridsteps = (gridsteps == 0.0) ? 20 : gridsteps;
            int nGridsteps = Convert.ToInt32(gridsteps);
            string smoothing = (smoo.ToLower().Equals("n")) ? "n" : "y";
            double t = expiry.Subtract(today).Days / 365.0;
            var wrapper = new Wrapper();
            ZeroCurve myZero = wrapper.UnpackZero(today, zeroDates, zeroRates) ;    
            DivList myDiv = wrapper.UnpackDiv(today, expiry, divDates,divAms);
            //create the tree
            var myTree = new PropTree(t, vol, spot, nGridsteps, true);
            myTree.MakeGrid(myZero, myDiv);
            //create pricer
            var myPrice = new Pricer(strike, paystyle, smoothing, style);
            myPrice.MakeGrid(myTree);
            double pr = myPrice.Price();
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
      /// <param name="zeroDates"></param>
      /// <param name="zeroRates"></param>
      /// <param name="divDates"></param>
      /// <param name="divAms"></param>
      /// <returns></returns>
        public static double BlackScholesPricer(double spot, double strike, double vol, string paystyle, DateTime today, DateTime expiry, 
            DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms)
        {
            double t = expiry.Subtract(today).Days / 365.0;
            var wrapper = new Wrapper();
            ZeroCurve myZero = wrapper.UnpackZero(today, zeroDates, zeroRates) ;    
            DivList myDiv = wrapper.UnpackDiv(today, expiry, divDates,divAms);
            double fwd = Collar.GetATMfwd(myZero, myDiv, spot, t);
            double r = myZero.LinInterp(t);
            //create default pricer
            var myPrice = new Pricer {Strike = strike, Payoff = paystyle};
            double pr = myPrice.BSprice(fwd, t, strike, r, vol, paystyle);
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
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <param name="q"></param>
        /// <returns></returns>
        public static double BlackScholesPricerContDiv(double spot, double strike, double vol, string paystyle, DateTime today, DateTime expiry, 
            DateTime[] zeroDates, double[] zeroRates, double q)
        {
            double t = expiry.Subtract(today).Days / 365.0;          
            ZeroCurve myZero = (new Wrapper()).UnpackZero(today, zeroDates, zeroRates);   
            double fwd = Collar.GetATMfwd(myZero, q, spot,t);
            double r = myZero.LinInterp(t);
            //create default pricer
            var myPrice = new Pricer {Strike = strike, Payoff = paystyle};
            double pr = myPrice.BSprice(fwd, t, strike, r, vol, paystyle);
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
      /// <param name="expiry"></param>
      /// <param name="paystyle">The paystyle.</param>
      /// <param name="zeroDates"></param>
      /// <param name="zeroRates">The zero curve.</param>
      /// <param name="divDates"></param>
      /// <param name="divAms">The div curve.</param>
      /// <param name="tol">The TOL.</param>
      /// <param name="step">The STEP.</param>
      /// <param name="gridsteps">The gridsteps.</param>
      /// <param name="smoo">The smoo.</param>
      /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
      /// <param name="today"></param>
      /// <returns></returns>
      public static double BinomialImpVol(double price, string style, double spot, double strike, double vol0, DateTime today, DateTime expiry,
          string paystyle, DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms, 
            double tol, double step, double gridsteps, string smoo, bool flatFlag)
        {
            gridsteps = (gridsteps == 0.0) ? 20 : gridsteps;
            int nGridsteps = Convert.ToInt32(gridsteps);
            string smoothing = (smoo.ToLower().Equals("n")) ? "n" : "y";
            //double t = expiry.Subtract(today).Days / 365.0;
            const int nIterations = 120;
            double f = BinomialPricer(style, spot, strike, vol0, today, expiry, paystyle, zeroDates, zeroRates, divDates, divAms, nGridsteps, smoothing, flatFlag) - price;
            for (int idx = 0; idx < nIterations; idx++)
            {
                // Pricer isn't working;
                if (f == 0)
                {
                    return 0;
                }

                if (Math.Abs(f) < tol)
                {
                    return vol0;
                }
                double f1 = BinomialPricer(style, spot, strike, vol0 + step, today, expiry, paystyle, zeroDates, zeroRates, divDates, divAms, nGridsteps, smoothing, flatFlag) - price;
                // Too far out or in the money
                if (f1 - f == 0)
                {
                    return 0;
                }
                double dvol = -f * step / (f1 - f);
                vol0 = vol0 + dvol;
                f = f1;
                // Failed to converge;
                if (vol0 < 0)
                {
                    return 0;
                }
            }
            return 0;
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
      /// <param name="zeroDates"></param>
      /// <param name="zeroRates">The zero curve.</param>
      /// <param name="divDates"></param>
      /// <param name="divAms">The div curve.</param>
      /// <param name="gridsteps">The gridsteps.</param>
      /// <param name="smoo">The smoo.</param>
      /// <param name="flatFlag">if set to <c>true</c> [flat flag].</param>
      /// <param name="today"></param>
      /// <returns></returns>
      public static double[] BinomialGetGreeks(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
          string paystyle, DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms, double gridsteps, string smoo, bool flatFlag)
        {
            const int arrsize = 4;
            gridsteps = (gridsteps == 0.0) ? 20 : gridsteps;
            int nGridsteps = Convert.ToInt32(gridsteps);
            string smoothing = (smoo.ToLower().Equals("n")) ? "n" : "y";
            double t = expiry.Subtract(today).Days / 365.0;
            ZeroCurve myZero = (new Wrapper()).UnpackZero(today, zeroDates, zeroRates);
            DivList myDiv = (new Wrapper()).UnpackDiv(today, expiry, divDates, divAms);
            var retArray = new double[arrsize];
            //create the tree
            var myTree = new DiscreteTree(t, vol, spot, nGridsteps, true) {Gridsteps = nGridsteps};
            myTree.MakeGrid(myZero, myDiv);
            //create pricer
            var myPrice = new Pricer(strike, paystyle, smoothing, style);
            myPrice.MakeGrid(myTree);
            myPrice.Price();
            var myGreeks = new Greeks();
            myGreeks.MakeDeltaGamma(myTree, myPrice, myZero, myDiv);
            double delta = myGreeks.Delta;
            double gamma = myGreeks.Gamma;
            myGreeks.MakeVega(myTree, myPrice, myZero, myDiv);
            double vega = myGreeks.Vega;
            myGreeks.MakeTheta(myTree, myPrice, myZero, myDiv);
            double theta = myGreeks.Theta;
            retArray[0] = delta;
            retArray[1] = gamma;
            retArray[2] = vega;
            retArray[3] = theta;
            return retArray;
        }



        /// <summary>
        /// 
        /// </summary>
        /// <param name="style"></param>
        /// <param name="spot"></param>
        /// <param name="strike"></param>
        /// <param name="vol"></param>
        /// <param name="today"></param>
        /// <param name="expiry"></param>
        /// <param name="paystyle"></param>
        /// <param name="zeroDates"></param>
        /// <param name="zeroRates"></param>
        /// <param name="divDates"></param>
        /// <param name="divAms"></param>
        /// <param name="gridsteps"></param>
        /// <param name="smoo"></param>
        /// <param name="flatFlag"></param>
        /// <returns></returns>
        public static double[,] BinomialRelativeGetGreeks(string style, double spot, double strike, double vol, DateTime today, DateTime expiry,
          string paystyle, DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms, double gridsteps, string smoo, bool flatFlag)
        {
            const int arrsize = 4;
            gridsteps = (gridsteps == 0.0) ? 20 : gridsteps;
            int nGridsteps = Convert.ToInt32(gridsteps);
            string smoothing = (smoo.ToLower().Equals("n")) ? "n" : "y";
            var retArray = new double[arrsize, arrsize];
            double t = expiry.Subtract(today).Days / 365.0;
            ZeroCurve myZero = (new Wrapper()).UnpackZero(today, zeroDates, zeroRates);
            DivList myDiv = (new Wrapper()).UnpackDiv(today, expiry,  divDates, divAms);
            //create the tree
            var myTree = new PropTree(t, vol, spot, nGridsteps, true) {Gridsteps = nGridsteps};
            myTree.MakeGrid(myZero, myDiv);
            //create pricer
            var myPrice = new Pricer(strike, paystyle, smoothing, style);
            myPrice.MakeGrid(myTree);
            myPrice.Price();
            var myGreeks = new Greeks();
            myGreeks.MakeDeltaGamma(myTree, myPrice, myZero, myDiv);
            double delta = myGreeks.Delta;
            double gamma = myGreeks.Gamma;
            myGreeks.MakeVega(myTree, myPrice, myZero, myDiv);
            double vega = myGreeks.Vega;
            myGreeks.MakeTheta(myTree, myPrice, myZero, myDiv);
            double theta = myGreeks.Theta;
            retArray[0, 0] = delta;
            retArray[0, 1] = gamma;
            retArray[0, 2] = vega;
            retArray[0, 3] = theta;
            return retArray;
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
            var volSurface = new OrcWingVol
                {
                    CurrentVol = currentVol,
                    DnCutoff = dnCutOff,
                    Dsr = dsr,
                    PutCurve = putCurve,
                    CallCurve = callCurve,
                    RefFwd = refFwd
                };
            volSurface.CurrentVol = currentVol;
            volSurface.Scr = scr;
            volSurface.SlopeRef = slopeRef;
            volSurface.Ssr = ssr;
            volSurface.TimeToMaturity = timeToMaturity;
            volSurface.UpCutoff = upCutOff;
            volSurface.Usr = usr;
            volSurface.Vcr = vcr;
            double res = volSurface.Orcvol(atm, k);
            return res;
        }

      /// <summary>
      /// Gets the AT mforward.
      /// </summary>
      /// <param name="zeroDates"></param>
      /// <param name="zeroRates">The zero curve.</param>
      /// <param name="divDates"></param>
      /// <param name="divAms">The div curve.</param>
      /// <param name="expiry"></param>
      /// <param name="spot">The spot.</param>
      /// <param name="today"></param>
      /// <returns></returns>
        public static double GetForward(DateTime today, DateTime expiry, double spot, DateTime[] zeroDates, double[] zeroRates, DateTime[] divDates, double[] divAms)
        {
            var wrapper = new Wrapper();
            ZeroCurve myZero = wrapper.UnpackZero(today, zeroDates, zeroRates);
            DivList myDiv = wrapper.UnpackDiv(today, expiry, divDates, divAms);
            double t = expiry.Subtract(today).Days / 365.0;
            double temp = Collar.GetATMfwd(myZero, myDiv, spot, t);
            return temp;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="valueDate"></param>
        /// <param name="dates"></param>
        /// <param name="amounts"></param>
        /// <param name="interpType"></param>
        /// <returns></returns>
        public static double InterpolateOnDates(DateTime valueDate, DateTime[] dates, double[] amounts, string interpType)
        {
            //At this stage only linear interpolation is supported
            return Utilities.InterpolateDates(valueDate, dates, amounts);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xvalue"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="interpType"></param>
        /// <returns></returns>
        public static double InterpolateOnValues(double xvalue, double[] x, double[] y, string interpType)
        {
            //At this stage only linear interpolation is supported
            return Utilities.InterpolateValues(xvalue, x, y);
        }
  
        /// <summary>
        /// Returns the PV (as at the valueDate) of the payment stream.
        /// Only payments occuring on or between valueDate and finalDate are included in the sum.
        /// All other payments are ignored.
        /// </summary>
        /// <param name="valueDate">The date at which the PV is taken.</param>
        /// <param name="paymentDates">The dates on which payments are made, in ascending order.</param>
        /// <param name="paymentAmounts">The amounds of payments.</param>
        /// <param name="zeroDates">The dates corresponding to the ZCB discount curve, in ascending order.</param>
        /// <param name="zeroRates">The rates corresponding to the ZCB discount curve.</param>
        /// <param name="finalDate">The final date on which payments are to be included.</param>
        /// <returns>A double representing the PV.</returns>
        public static double PVofPaymentStream(DateTime valueDate, DateTime finalDate, DateTime[] paymentDates, double[] paymentAmounts, DateTime[] zeroDates, double[] zeroRates)
        {
            return Utilities.PVofPaymentStream(valueDate, paymentDates, paymentAmounts, zeroDates, zeroRates, finalDate);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rate"></param>
        /// <param name="compoundingFrequency"></param>
        /// <returns></returns>
        public static double[] ConvToContinuousRate(double[] rate, string compoundingFrequency)
        {
            double[] retArray = Utilities.ConvToContinuousRate(rate, compoundingFrequency);
            return retArray;
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
        public static double[] DivYield(double baseAmount, DateTime valueDate, DateTime[] finalDate, DateTime[] paymentDates,
            double[] paymentAmounts, DateTime[] zeroDates,
            double[] zeroRates)
        {
            double[] retArray = Utilities.EquivalentYield(baseAmount, valueDate, finalDate, paymentDates, paymentAmounts, zeroDates, zeroRates);
            return retArray;
        }


      //public double OrcPricer(string style, double spot, double strike, double t, string paystyle,
        // [ExcelZeroLowerBound()]double[,] zeroArray, [ExcelZeroLowerBound()]double[,] divArray, WingParamsRange orcparams, double gridsteps, string smoo, bool flatFlag)
        //{

        //  gridsteps = (gridsteps == 0.0) ? 20 : gridsteps;
        //  int N_GRIDSTEPS = System.Convert.ToInt32(gridsteps);
        //  string SMOOTHING = (smoo.ToLower().Equals("n")) ? "n" : "y";

        //  Wrapper wrapper = new Wrapper();
        //  ZeroCurve myZero = wrapper.UnpackZero(zeroArray);
        //  DivList myDiv = wrapper.UnpackDiv(divArray);
      
      
     
        //  OrcWingVol myvol = new OrcWingVol();

        //  myvol.currentVol = orcparams.CurrentVolatility;
        //  myvol.dnCutoff = orcparams.DownCutOff;
        //  myvol.upCutoff = orcparams.UpCutOff;

        //  myvol.putCurve = orcparams.PutCurvature;
        //  myvol.callCurve = orcparams.CallCurvature;
        //  myvol.refFwd = orcparams.ReferenceForward;
        //  myvol.refVol = orcparams.CurrentVolatility;
        //  myvol.scr = orcparams.SlopeChangeRate;
        //  myvol.slopeRef = orcparams.SlopeReference;

        //  myvol.ssr = orcparams.SkewSwimmingnessRate;
        //  myvol.usr = orcparams.UpSmoothingRange;
        //  myvol.vcr = orcparams.VolChangeRate;


        //  myvol.timeToMaturity = t;

     
        //  //get the atfwd
        //  double atfwd = Collar.GetATMfwd(myZero, myDiv, spot, t );
        //  double vol = myvol.orcvol(atfwd, strike);

        //  //create the tree
        //  Tree myTree = new Tree(t, vol, spot, N_GRIDSTEPS, false);
        //  myTree.MakeGrid(myZero, myDiv);

        //  //create pricer
        //  Pricer myPrice = new Pricer(strike, paystyle, SMOOTHING, style);
        //  myPrice.MakeGrid(myTree);
        //  double pr = myPrice.Price();
      
        //  return pr;
        //}
    }
}



