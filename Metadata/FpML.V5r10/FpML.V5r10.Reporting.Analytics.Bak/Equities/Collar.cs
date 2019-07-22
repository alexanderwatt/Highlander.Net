#region Usings

using System;
using Orion.Analytics.Options;
using Orion.Analytics.Rates;

#endregion

namespace Orion.Analytics.Equities
{
  public class Collar
  {
        public static double FindPrice(ZeroCurve myZero, DivList myDiv, OrcWingVol myVol, double t, 
          double strike, double spot, string style, string paystyle, double gridsteps)
        {
          //get the atfwd
          double atFwd = GetATMfwd(myZero, myDiv, spot, t);
          //set up the tree
          var myTree = new Tree();
          int nGridsteps = (gridsteps < 20.0) ? 20 : Convert.ToInt32(gridsteps);
          myTree.Gridsteps = nGridsteps;
          myTree.Tau = t;
          myTree.Sig = myVol.orcvol(atFwd, strike);
          myTree.Spot = spot;
          myTree.MakeGrid(myZero, myDiv);
          //create pricer
          var myPrice = new Pricer {Strike = strike, Payoff = paystyle, Smoothing = "y", Style = style};
            myPrice.MakeGrid(myTree);
          double pr = myPrice.Price();
          return pr;
        }

        public static double GetATMfwd(ZeroCurve myZero, DivList myDivList, double spot, double t)
        {
          double spotstar = spot;
          //compute the discounted dividends and take off spot
          if ((myDivList != null) && (myZero != null))
          {
            for (int idx = 0; idx < myDivList.Divpoints; idx++)
            {
              if (myDivList.GetT(idx) <= t)
              {
                double d1 = myDivList.GetD(idx);
                double r1 = myZero.LinInterp(myDivList.GetT(idx));
                double t1 = Math.Exp(-r1 * myDivList.GetT(idx));
                spotstar -= d1 * t1;
              }
            }
          }          
          //gross up to expiry to get atfwd
            if (myZero != null)
            {
                double r = myZero.LinInterp(t);
                return spotstar * Math.Exp(r * t);
            }
            return 0;
        }

        public static double FindZeroCostCall(ZeroCurve myZero, DivList myDiv, OrcWingVol myVol, double t,
          double strike, double spot, string style, string paystyle, double gridsteps)
        {
          //compute the cost of the put
          double callPrice = 0.0;
          double putPrice = FindPrice(myZero, myDiv, myVol, t, strike, spot, style, "p", gridsteps);
          //start looking for the call using the ATM strike
          //double callStrike = GetATMfwd(myZero, myDiv, spot, t);
          double callStrike = strike;
          const double tol = 0.0001;

          for (int idx = 0; idx < 1000; idx++)
          {
            callPrice = FindPrice(myZero, myDiv, myVol, t, callStrike, spot, style, "c", gridsteps);

            if (Math.Abs(callPrice - putPrice) < tol) break;

            //apply newton
            double dk = 0.001 * callStrike;
            double callPriceUp = FindPrice(myZero, myDiv, myVol, t, callStrike +dk , spot, style, "c", gridsteps);
            callStrike -= (callPrice-putPrice) * dk / (callPriceUp - callPrice);
        }
        if (Math.Abs(callPrice - putPrice) > tol)
            throw new Exception("Price did not converge");
            return callStrike;
        }

        public static double FindZeroCostPut(ZeroCurve myZero, DivList myDiv, OrcWingVol myVol, double t,
          double strike, double spot, string style, string paystyle, double gridsteps)
        {
          //compute the cost of the put
          double putPrice = 0.0;
          double callPrice = FindPrice(myZero, myDiv, myVol, t, strike, spot, style, "c", gridsteps);
          //start looking for the call using the ATM strike
          //double putStrike = GetATMfwd(myZero, myDiv, spot, t);
          double putStrike = strike;
          const double tol = 0.0001;
          for (int idx = 0; idx < 1000; idx++)
          {
            putPrice = FindPrice(myZero, myDiv, myVol, t, putStrike, spot, style, "p", gridsteps);

            if (Math.Abs(callPrice - putPrice) < tol) break;

            //apply newton
            double dk = 0.001 * putStrike;
            double putPriceUp = FindPrice(myZero, myDiv, myVol, t, putStrike + dk, spot, style, "p", gridsteps);
            putStrike -=  (putPrice-callPrice) * dk / (putPriceUp - putPrice);
          }
          if (Math.Abs(callPrice - putPrice) > tol)
              throw new Exception("Price did not converge");
            return putStrike;
        }
  }
}
