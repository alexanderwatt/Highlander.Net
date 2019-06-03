using System;
using National.QRSC.Numerics.DataStructures;
using National.QRSC.Numerics.FiniteDifferences;
using National.QRSC.Numerics.OptionModels;
using National.QRSC.Numerics.Utilities;

namespace National.QRSC.Analytics.NabLib
{
  ///<summary>
  ///</summary>
  public static class Newton
  {
    private static int Gauss(int n, ref double[,] am, ref double[] bm)
    {
      int[] ipiv = new int[20];
      int[] indxr = new int[20];
      int[] indxc = new int[20];
      int icol = 0, irow = 0, idx, jdx, kdx, ldx, lldx;
      double big, pivinv, dum;


      for (idx = 0; idx < n; idx++) { ipiv[idx] = 0; }

      for (idx = 0; idx < n; idx++)
      {
        big = 0.0;

        for (jdx = 0; jdx < n; jdx++)
        {
          if (ipiv[jdx] != 1)
          {
            for (kdx = 0; kdx < n; kdx++)
            {
              if (ipiv[kdx] == 0.0)
              {
                if (Math.Abs(am[jdx, kdx]) > big)
                {
                  big = Math.Abs(am[jdx, kdx]); irow = jdx; icol = kdx;
                }
              }
              else
              {
                if (ipiv[kdx] > 1)
                {
                  return 90;
                }
              } //endif
            } // next k
          }
        } //next j

        ipiv[icol] += 1;
        if (irow != icol)
        {
          for (ldx = 0; ldx < n; ldx++)
          {
            dum = am[irow, ldx]; am[irow, ldx] = am[icol, ldx]; am[icol, ldx] = dum;
          }
          dum = bm[irow]; bm[irow] = bm[icol]; bm[icol] = dum;
        }
        indxr[idx] = irow;
        indxc[idx] = icol;
        if (am[icol, icol] == 0.0) { return 98; }
        pivinv = 1.0 / am[icol, icol];
        am[icol, icol] = 1.0;
        for (ldx = 0; ldx < n; ldx++) { am[icol, ldx] *= pivinv; }
        bm[icol] *= pivinv;
        for (lldx = 0; lldx < n; lldx++)
        {
          if (lldx != icol)
          {
            dum = am[lldx, icol];
            am[lldx, icol] = 0.0;
            for (ldx = 0; ldx < n; ldx++) { am[lldx, ldx] -= am[icol, ldx] * dum; }
            bm[lldx] -= bm[icol] * dum;
          }
        }
      }  // next idx
      for (ldx = (n - 1); ldx >= 0; ldx--)
      {
        if (indxr[ldx] != indxc[ldx])
        {
          for (kdx = 0; kdx < n; kdx++)
          {
            dum = am[kdx, indxr[ldx]]; am[kdx, indxr[ldx]] = am[kdx, indxc[ldx]]; am[kdx, indxc[ldx]] = dum;
          }
        }
      }  // next ldx
      return 2;
    }

    private static int FuncVec(int k,
                               double[] p,
                               double[] vec,
                               int strikecount,
                               double[] DataParams,
                               double[] DataPrices,
                               double[] DataStrikes,
                               double[] ImpVols,
                               Curve myDom,
                               Curve myFor,
                               Curve myVol,
                               Curve myP1,
                               Curve myP2,
                               Curve myP3,
                               Curve myP4,
                               Curve myP5,
                               Curve myP6,
                               double[] Speed,
                               Curve myMR,
                               double[] FitPrices,
                               double[] FitVols,
                               long CalSwitch,
                               double SpeedKC,
                               double SpeedKP,
                               double VolP,
                               double VolC,
                               Curve myD0,
                               Curve myD1,
                               Grid myGrid)
    {
      // declare  CN and curve objects
      CN myCN = new CN();
      NabLibBlack myBlack = new NabLibBlack();
      Pricer myPricer = new Pricer();

      myP1.SetRate((int)k, p[0]);
      myP2.SetRate((int)k, p[1]);
      myP3.SetRate((int)k, p[2]);
      myP4.SetRate((int)k, p[3]);
      myP5.SetRate((int)k, p[4]);
      myP6.SetRate((int)k, p[5]);



      // load the option paramaters for the Crank Nicholson call
      myCN.T = DataParams[0];
      myCN.S = Math.Log(DataParams[1]);
      myCN.Anchor = myCN.S;
      myCN.NoAssetSteps2 = (long)DataParams[2];
      myCN.NoTimeSteps2 = (long)DataParams[3];

      //get the rates and set the lower and upper bounds
      double zeroR = myDom.LinInterpolate(myCN.T);
      double zeroQ = myFor.LinInterpolate(myCN.T);
      //myCN.LowerLogAsset2 = myCN.S + (zeroR-zeroQ)*myCN.T - 10.0*DataParams[5]*sqrt(myCN.T);
      //myCN.UpperLogAsset2 = myCN.S + (zeroR-zeroQ)*myCN.T + 10.0*DataParams[5]*sqrt(myCN.T);
      myCN.LowerLogAsset2 = Math.Log(0.5 * DataStrikes[0]);
      myCN.UpperLogAsset2 = Math.Log(2.0 * DataStrikes[4]);

      //load the Black object paramters for impvol calc
      myBlack.T = DataParams[0];
      myBlack.Spot = DataParams[1];
      myBlack.Fwd = myBlack.Spot * Math.Exp((zeroR - zeroQ) * myBlack.T);
      myBlack.ForRate = zeroQ;
      myBlack.DomRate = zeroR;


      //loop over the strikes
      for (int idx = 0; idx < strikecount; idx++)
      {
        myCN.PayStyle = myBlack.PayStyle = (DataStrikes[2] <= DataStrikes[idx]) ?
          (long) OptionCode.VEUC : (long) OptionCode.VEUP;
        myCN.K = myBlack.K = DataStrikes[idx];
        double price = myPricer.CNBarrierFull(myGrid, myCN, myVol, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);
        FitPrices[idx] = price;

        // check the prices, error handling, clean up memory
        if ((price == 0.0) || (price == 99.0))
        {
          return (int)price;
        }


        myBlack.Sig = myVol.Interpolate(myCN.T);
        myBlack.Impvol(price);
        FitVols[idx] = myBlack.Sig;
        vec[idx] = myBlack.Sig - ImpVols[idx];
      }
      //use to calswitch to apply either the Speed or RB conditions

      if (CalSwitch == 1)
      {
        //calculate the binary 
        myCN.LowerLogAsset2 = DataParams[6];
        myCN.UpperLogAsset2 = DataParams[7];
        myCN.NoTimeSteps2 = (long)(myCN.T / 0.0025);
        myCN.PayStyle = (long) OptionCode.RBNP;
        double price = myPricer.CNBarrierFull(myGrid, myCN, myVol, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);

        // check the prices, error handling, clean up memory
        if ((price == 0.0) || (price == 99.0))
        {
          return (int)price;
        }

        //define flat curves
        double temp = myPricer.CNBarrierFull(myGrid, myCN, myVol, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);

        // check the prices, error handling, clean up memory
        if ((temp == 0.0) || (temp == 99.0))
        {
          return (int)temp;
        }

        vec[5] = (price - temp) - Speed[k] / 10000.0;
        FitVols[5] = 0.0;
        FitPrices[5] = (price - temp) * 10000.0;
      }
      else  //apply the speed condition
      {
        double spotshift = 1.01;//exp(0.1 * myVol->GetY(k) * sqrt(myVol->GetX(k)));
        myCN.S = Math.Log(spotshift * DataParams[1]);
        myCN.K = SpeedKC;
        myBlack.K = SpeedKC;
        myCN.PayStyle = (long) OptionCode.VEUC;
        myBlack.PayStyle = (long) OptionCode.VEUC;
        myBlack.Spot *= spotshift;
        myBlack.Fwd *= spotshift;
        double price = myPricer.CNBarrierFull(myGrid, myCN, myVol, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);

        // check the prices, error handling, clean up memory
        if ((price == 0.0) || (price == 99.0))
        {
          return (int)price;
        }

        myBlack.Sig = myVol.Interpolate(myCN.T);
        long j = myBlack.Impvol(price);
        double priceSigC = myBlack.Sig;

        myCN.K = SpeedKP;
        myBlack.K = SpeedKP;
        myCN.PayStyle = (long) OptionCode.VEUP;
        myBlack.PayStyle = (long) OptionCode.VEUP;
        price = myPricer.CNBarrierFull(myGrid, myCN, myVol, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);

        // check the prices, error handling, clean up memory
        if ((price == 0.0) || (price == 99.0))
        {
          return (int)price;
        }

        //apply the speed condition
        j = myBlack.Impvol(price);
        double priceSigP = myBlack.Sig;
        vec[5] = (priceSigC - priceSigP - VolC + VolP);
        FitPrices[5] = 0.0;
        FitVols[5] = (priceSigC - priceSigP - ImpVols[3] + ImpVols[1]) * 10000.0;
      }
      return 1;
    }


    ///<summary>
    ///</summary>
    ///<param name="k"></param>
    ///<param name="p"></param>
    ///<param name="vec"></param>
    ///<param name="Strikecount"></param>
    ///<param name="tol"></param>
    ///<param name="DataParams"></param>
    ///<param name="DataPrices"></param>
    ///<param name="DataStrikes"></param>
    ///<param name="ImpVols"></param>
    ///<param name="n"></param>
    ///<param name="myTenor"></param>
    ///<param name="rawDom"></param>
    ///<param name="rawFor"></param>
    ///<param name="rawVols"></param>
    ///<param name="rawP1"></param>
    ///<param name="rawP2"></param>
    ///<param name="rawP3"></param>
    ///<param name="rawP4"></param>
    ///<param name="rawP5"></param>
    ///<param name="rawP6"></param>
    ///<param name="Speed"></param>
    ///<param name="MR"></param>
    ///<param name="FitPrices"></param>
    ///<param name="FitVols"></param>
    ///<param name="CalSwitch"></param>
    ///<param name="SpeedKC"></param>
    ///<param name="SpeedKP"></param>
    ///<param name="VolP"></param>
    ///<param name="VolC"></param>
    ///<returns></returns>
    public static int newton6(int k,
                              double[] p,
                              double[] vec,
                              int Strikecount,
                              double tol,
                              double[] DataParams,
                              double[] DataPrices,
                              double[] DataStrikes,
                              double[] ImpVols,
                              int n,
                              double[] myTenor,
                              double[] rawDom,
                              double[] rawFor,
                              double[] rawVols,
                              double[] rawP1,
                              double[] rawP2,
                              double[] rawP3,
                              double[] rawP4,
                              double[] rawP5,
                              double[] rawP6,
                              double[] Speed,
                              double[] MR,
                              double[] FitPrices,
                              double[] FitVols,
                              int CalSwitch,
                              double SpeedKC,
                              double SpeedKP,
                              double VolP,
                              double VolC)
    {

      Curve myVol = new Curve(n, myTenor, rawVols);
      Curve myDom = new Curve(n, myTenor, rawDom);
      Curve myFor = new Curve(n, myTenor, rawFor);
      Curve myP1 = new Curve(n, myTenor, rawP1);
      Curve myP2 = new Curve(n, myTenor, rawP2);
      Curve myP3 = new Curve(n, myTenor, rawP3);
      Curve myP4 = new Curve(n, myTenor, rawP4);
      Curve myP5 = new Curve(n, myTenor, rawP5);

      //if the dataparams[8] value is != 0.0 then we are ignoring the stochastic
      //and will overwrite the values
      long mp = 6;
      if (Math.Abs(DataParams[6]) > 1.0e-6)
      {
        for (long idx = 0; idx <= n; idx++)
          rawP6[idx] = 0.0;
        mp = 5;
        p[5] = 0.0; //overwrite any input for the first guess
      }

      Curve myP6 = new Curve(n, myTenor, rawP6);
      Curve myMR = new Curve(n, myTenor, MR);
      Curve myD0 = new Curve(n, myTenor, 0.0);
      Curve myD1 = new Curve(n, myTenor, 1.0);

      //declare arrays
      const long nsize = 6;

      double[] vec1 = new double[nsize];
      double[] vec2 = new double[nsize];
      double[] bm = new double[nsize];
      double[,] am = new double[nsize, nsize];

      bool TempMax, TempError;

      double LogAssetStep = (Math.Log(2.0 * DataStrikes[4]) - Math.Log(0.5 * DataStrikes[0])) / DataParams[2];
      Grid myGrid = new Grid((long) DataParams[2], Math.Log(0.5*DataStrikes[0]), LogAssetStep);

      //iteration for the newton method
      for (int idx = 0; idx < 10; idx++)
      {
        int kj = FuncVec(k, p, vec, Strikecount, DataParams, DataPrices, DataStrikes, ImpVols,
                          myDom, myFor, myVol, myP1, myP2, myP3, myP4, myP5, myP6, Speed, myMR,
                          FitPrices, FitVols, CalSwitch, SpeedKC, SpeedKP, VolP, VolC, myD0, myD1, myGrid);

        // error check, clean memory
        if (kj != 1)
        {
          return kj;
        }
        TempMax = true;

        for (int jdx = 0; jdx < 5; jdx++)
        {
          if (Math.Abs(vec[jdx]) > tol) { TempMax = false; }
        }
        if ((mp == 6) && (Math.Abs(vec[5]) > (tol / 3.0))) { TempMax = false; }

        if (TempMax == true) { break; }

        for (int jdx = 0; jdx < mp; jdx++)
        {
          double dp = 0.005;
          p[jdx] += dp;
          kj = FuncVec(k, p, vec1, Strikecount, DataParams, DataPrices, DataStrikes, ImpVols,
                       myDom, myFor, myVol, myP1, myP2, myP3, myP4, myP5, myP6, Speed, myMR,
                       FitPrices, FitVols, CalSwitch, SpeedKC, SpeedKP, VolP, VolC, myD0, myD1, myGrid);

          // error check, clean memory
          if (kj != 1)
          {
            return kj;
          }

          p[jdx] -= 2.0 * dp;
          kj = FuncVec(k, p, vec2, Strikecount, DataParams, DataPrices, DataStrikes, ImpVols,
                       myDom, myFor, myVol, myP1, myP2, myP3, myP4, myP5, myP6, Speed, myMR,
                       FitPrices, FitVols, CalSwitch, SpeedKC, SpeedKP, VolP, VolC, myD0, myD1, myGrid);

          // error check, clean memory
          if (kj != 1)
          {
            return kj;
          }

          p[jdx] += dp;
          // set up the jacobian matrix
          for (long kdx = 0; kdx < nsize; kdx++)
            am[kdx, jdx] = (vec1[kdx] - vec2[kdx]) / (2.0 * dp);

          bm[jdx] = -vec[jdx];
        }

        Gauss((int)mp, ref am, ref bm);

        TempError = false;
        for (int kdx = 0; kdx < mp; kdx++)
        {
          p[kdx] += bm[kdx];
          if (kdx == 5) { p[5] = Math.Abs(p[5]); }
          if (Math.Abs(bm[kdx]) >= 3.0) { TempError = true; }
        }

        //error check if the increments are large.
        if (TempError)
        {
          return 98;
        }
      }

      rawP1[k] = myP1.GetRate((int)k);
      rawP2[k] = myP2.GetRate((int)k);
      rawP3[k] = myP3.GetRate((int)k);
      rawP4[k] = myP4.GetRate((int)k);
      rawP5[k] = myP5.GetRate((int)k);
      rawP6[k] = myP6.GetRate((int)k);

      //return successful code
      return 2;
    }
  }
}
