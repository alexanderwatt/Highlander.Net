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

using System;

namespace Orion.EquitiesCore
{
  /// <summary>
  /// 
  /// </summary>
  public class Collar
  {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDiv"></param>
        /// <param name="orcParams"></param>
        /// <param name="t"></param>
        /// <param name="strike"></param>
        /// <param name="spot"></param>
        /// <param name="style"></param>
        /// <param name="paystyle"></param>
        /// <param name="gridsteps"></param>
        /// <returns></returns>
        public static double FindPrice(ZeroCurve myZero, DivList myDiv, double[] orcParams, double t, 
          double strike, double spot, string style, string paystyle, double gridsteps)
        {
          //get the atfwd
          double atFwd = GetATMfwd(myZero, myDiv, spot, t);
          //unpack the orc paramters
          //double currentVol = OrcParams[0];
          //double slopeRef = OrcParams[1];
          //double putCurve = OrcParams[2];
          //double callCurve = OrcParams[3];
          //double dnCutOff = OrcParams[4];
          //double upCutOff = OrcParams[5];
          //double refFwd = OrcParams[6];
          //double refVol = OrcParams[7];
          //double vcr = OrcParams[8];
          //double scr = OrcParams[9];
          //double ssr = OrcParams[10];
          //double dsr = OrcParams[11];
          //double usr = OrcParams[12];

          //set up the tree
          var myTree = new DiscreteTree();
          var myVol = new OrcWingVol();
          int nGridsteps = (gridsteps < 20.0) ? 20 : Convert.ToInt32(gridsteps);
          myTree.Gridsteps = nGridsteps;
          myTree.Tau = t;
          myTree.Sig = myVol.Orcvol(atFwd, strike);
          myTree.Spot = spot;
          myTree.MakeGrid(myZero, myDiv);
          //create pricer
          var myPrice = new Pricer {Strike = strike, Payoff = paystyle, Smoothing = "y", Style = style};
            myPrice.MakeGrid(myTree);
          double pr = myPrice.Price();
          return pr;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDivList"></param>
        /// <param name="spot"></param>
        /// <param name="t"></param>
        /// <returns></returns>
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="q"></param>
        /// <param name="spot"></param>
        /// <param name="t"></param>
        /// <returns></returns>
        public static double GetATMfwd(ZeroCurve myZero, double q, double spot, double t)
        {
            //compute the discounted dividends and take off spot
            if ((myZero != null))
            {
                double spotstar = spot * Math.Exp(-q * t);
                double r = myZero.LinInterp(t);
                return spotstar * Math.Exp(r * t);
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDiv"></param>
        /// <param name="orcParams"></param>
        /// <param name="t"></param>
        /// <param name="strike"></param>
        /// <param name="spot"></param>
        /// <param name="style"></param>
        /// <param name="paystyle"></param>
        /// <param name="gridsteps"></param>
        /// <returns></returns>
        public static double FindZeroCostCall(ZeroCurve myZero, DivList myDiv, double[] orcParams, double t,
          double strike, double spot, string style, string paystyle, double gridsteps)
        {
          //compute the cost of the put
          double callPrice = 0.0;
          double putPrice = FindPrice(myZero, myDiv, orcParams, t, strike, spot, style, "p", gridsteps);
          //start looking for the call using the ATM strike
          double callStrike = GetATMfwd(myZero, myDiv, spot, t);
          const double tol = 0.0001;
          for (int idx = 0; idx < 20; idx++)
          {
            callPrice = FindPrice(myZero, myDiv, orcParams, t, callStrike, spot, style, "c", gridsteps);
            if (Math.Abs(callPrice - putPrice) < tol) break;
            //apply newton
            double dk = 0.01 * callStrike;
            double callPriceUp = FindPrice(myZero, myDiv, orcParams, t, callStrike +dk , spot, style, "c", gridsteps);
            callStrike += (putPrice - callPrice) * dk / (callPriceUp - callPrice);
          }
          return callPrice;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="myZero"></param>
        /// <param name="myDiv"></param>
        /// <param name="orcParams"></param>
        /// <param name="t"></param>
        /// <param name="strike"></param>
        /// <param name="spot"></param>
        /// <param name="style"></param>
        /// <param name="paystyle"></param>
        /// <param name="gridsteps"></param>
        /// <returns></returns>
        public static double FindZeroCostPut(ZeroCurve myZero, DivList myDiv, double[] orcParams, double t,
          double strike, double spot, string style, string paystyle, double gridsteps)
        {
          //compute the cost of the put
          double putPrice = 0.0;
          double callPrice = FindPrice(myZero, myDiv, orcParams, t, strike, spot, style, "c", gridsteps);
          //start looking for the call using the ATM strike
          double putStrike = GetATMfwd(myZero, myDiv, spot, t);
          const double tol = 0.0001;
          for (int idx = 0; idx < 20; idx++)
          {
            putPrice = FindPrice(myZero, myDiv, orcParams, t, putStrike, spot, style, "p", gridsteps);
            if (Math.Abs(callPrice - putPrice) < tol) break;
            //apply newton
            double dk = 0.01 * putStrike;
            double putPriceUp = FindPrice(myZero, myDiv, orcParams, t, putStrike - dk, spot, style, "p", gridsteps);
            putStrike += (callPrice - putPrice) * dk / (putPriceUp - putPrice);
          }
          return putPrice;
        }
  }
}
