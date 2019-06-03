using System;
using System.Linq;
using National.QRSC.Numerics.DataStructures;
using National.QRSC.Numerics.FiniteDifferences;
using National.QRSC.Numerics.OptionModels;
using National.QRSC.Numerics.Utilities;

namespace National.QRSC.Analytics.NabLib
{
  ///<summary>
  ///</summary>
  public class Pricer
  {
    private double[,] _rmat = { { 2.7043269, -2.7692308, 1.6875, -0.9230769, 0.3004808},
	  { -22.8365385, 40.0, -27.75, 15.7948718, -5.2083333}, { 65.3044872, -140.7179487, 127.75, -79.1794872, 26.8429487},
	  {-76.9230769,184.6153846, -200.0,143.5897436, -51.2820513},{32.0512821, -82.0512821, 100.0, -82.0512821, 32.0512821}};

    const int VOLSMILE = 5;
    const int VOLSTATES = 3;
    double DeltaCutTime = 1.05;
    double Toolerance = 0.0002;

    private double[] _ac = new double[VOLSMILE];
    private double[] F = new double[VOLSMILE];
    private double[] Coef = new double[VOLSMILE];

    // coefficient arrays
    public double[] a0, a1, a2, a3, a4, a5, mr;

    // market prices (output)
    public double[] mp0, mp1, mp2, mp3, mp4;

    // fit prices (output)
    public double[] fp0, fp1, fp2, fp3, fp4;

    // fit vols (output)
    public double[] fv0, fv1, fv2, fv3, fv4;

    // coeffiecients for std normal dist
    const double b1 = 0.31938153;
    const double b2 = -0.356563782;
    const double b3 = 1.781477937;
    const double b4 = -1.821255978;
    const double b5 = 1.330274429;
    const double p = 0.2316419;
    const double c2 = 0.3989423;

    //local volatility function
    private double LocalVol(double x)
    {
      F[0] = _ac[0] - _ac[1] + _ac[2] + _ac[3] - _ac[4];
      F[1] = _ac[0] - _ac[1] + _ac[2];
      F[2] = _ac[0]; 
      F[3] = _ac[0] + _ac[1] + _ac[2];
      F[4] = _ac[0] + _ac[1] + _ac[2] + _ac[3] + _ac[4];


      for (int idx = 0; idx < VOLSMILE; idx++)
      {
        Coef[idx] = 0.0;
        for (int jdx = 0; jdx < VOLSMILE; jdx++)
          Coef[idx] += _rmat[idx, jdx] * F[jdx];
      }

      //apply formulas
      double temp = 0.0;
      if ((x >= 0.1) && (x <= 0.9))
      {
        temp = Coef[0] + x * (Coef[1] + x * (Coef[2] + x * (Coef[3] + x * Coef[4])));
      }
      else if (x < 0.1)
      {
        double g1 = Coef[0] + 0.1 * (Coef[1] + 0.1 * (Coef[2] + 0.1 * (Coef[3] + 0.1 * Coef[4])));
        double g1p = Coef[1] + 0.2 * Coef[2] + 0.03 * Coef[3] + 0.004 * Coef[4];
        double cl = g1p * 5.0;
        double al = g1 - 0.01 * cl;
        temp = al + cl * x * x;
      }
      else
      {
        double g9 = Coef[0] + 0.9 * (Coef[1] + 0.9 * (Coef[2] + 0.9 * (Coef[3] + 0.9 * Coef[4])));
        double g9p = Coef[1] + 1.8 * Coef[2] + 2.43 * Coef[3] + 2.916 * Coef[4];
        double cu = -g9p * 5.0;
        double bu = -2.0 * cu;
        double au = g9 - 0.81 * cu - 0.9 * bu;
        temp = au + bu * x + cu * x * x;
      }
      return temp;
    }


    public double CNBarrierFull(Grid myGrid, CN myCN, Curve myVol, Curve myDom, Curve myFor, Curve myP1,
      Curve myP2, Curve myP3, Curve myP4, Curve myP5, Curve myP6, Curve myMR)
    {
      myCN.NoAssetSteps = myCN.NoAssetSteps2;
      double LogAssetStep = (myCN.UpperLogAsset2 - myCN.LowerLogAsset2) / (double)myCN.NoAssetSteps;

      long N = myCN.NoAssetSteps;

      if (LogAssetStep < 0.0)
        return 0.0;

      double TimeStep = myCN.T / (double)myCN.NoTimeSteps2;
      double n1 = TimeStep / LogAssetStep;
      double n2 = n1 / LogAssetStep;
      const double theta = 0.5;

      //additional information for CPC
      if ((myCN.PayStyle == (long) OptionCode.CPCC) || (myCN.PayStyle == (long) OptionCode.CPCP))
      {
        myGrid._margin = myCN.margin;
        myGrid._cpcflag = myCN.CPCflag;
      }

      //set the initial boundary conditions
      myGrid.SetIC(myCN.PayStyle, myCN.K, myCN.LowerLogAsset2);

      double atmvol = myVol.Interpolate(myCN.T);
      double rootThree = Math.Sqrt(3.0);

      for (long j = 1; j <= myCN.NoTimeSteps2; j++)
      {
        double tau = j * TimeStep;
        double TauDate = myCN.T - tau;

        // set the  forward interest rates
        double zero2 = myDom.Interpolate(TauDate + TimeStep);
        double zero1 = myDom.Interpolate(TauDate);
        double ForwardRate = (zero2 * (TauDate + TimeStep) - zero1 * TauDate) / TimeStep;


        zero2 = myFor.Interpolate(TauDate + TimeStep);
        zero1 = myFor.Interpolate(TauDate);
        double Dividend = (zero2 * (TauDate + TimeStep) - zero1 * TauDate) / TimeStep;


        // set the  interest rates between t and Expiry Time
        zero2 = myDom.Interpolate(myCN.T);
        zero1 = myDom.Interpolate(TauDate);
        double ForwardTerm = zero2 * myCN.T - zero1 * TauDate;
        double ForwardZero = myDom.Interpolate(TauDate + 0.5 * TimeStep);

        zero2 = myFor.Interpolate(myCN.T);
        zero1 = myFor.Interpolate(TauDate);
        double DividendTerm = zero2 * myCN.T - zero1 * TauDate;
        double DividendZero = myFor.Interpolate(TauDate + 0.5 * TimeStep);


        // set the stochastic scaling
        double volroott = 1.0 / (atmvol * Math.Sqrt(TauDate + 0.5 * TimeStep));
        double arg = TauDate + 0.5 * TimeStep;
        double Vovol = myP6.Interpolate(arg);
        double PL = Math.Exp(-Vovol * rootThree);
        double PU = Math.Exp(Vovol * rootThree);

        // get the local vol paramters

        _ac[0] = myP1.Interpolate(arg);
        _ac[1] = myP2.Interpolate(arg);
        _ac[2] = myP3.Interpolate(arg);
        _ac[3] = myP4.Interpolate(arg);
        _ac[4] = myP5.Interpolate(arg);

        double preFactor = myCN.Anchor + (ForwardZero - DividendZero) * (TauDate + 0.5 * TimeStep);


        //scroll across the three states
        for (int k = 0; k < VOLSTATES; k++)
        {
          // scroll from 1 to N-1 and fill up the matrix

          for (int i = 1; i < myCN.NoAssetSteps; i++)
          {

            double ta = myGrid._X[i] - preFactor;
            double za = ta * volroott;

            if (k == 0)
            {
              double n = 0;
              if (za > 6.0)
              {
                n = 1.0;
              }
              else if (za < -6.0)
              {
                n = 0.0;
              }
              else
              {
                
                double a = Math.Abs(za);
                double t = 1.0 / (1.0 + a * p);
                double b = c2 * Math.Exp((-za) * (za / 2.0));
                n = ((((b5 * t + b4) * t + b3) * t + b2) * t + b1) * t;
                n = 1.0 - b * n;
                if (za < 0.0) n = 1.0 - n;
              }
              double ya = n;

              F[0] = _ac[0] - _ac[1] + _ac[2] + _ac[3] - _ac[4];
              F[1] = _ac[0] - _ac[1] + _ac[2];
              F[2] = _ac[0];
              F[3] = _ac[0] + _ac[1] + _ac[2];
              F[4] = _ac[0] + _ac[1] + _ac[2] + _ac[3] + _ac[4];


              for (int idx = 0; idx < 5; idx++)
              {
                Coef[idx] = 0.0;
                for (int jdx = 0; jdx < 5; jdx++)
                  Coef[idx] += _rmat[idx, jdx] * F[jdx];
              }

              //apply formulas
              double tempy = 0.0;
              double x = ya;
              if ((x >= 0.1) && (x <= 0.9))
              {
                tempy = Coef[0] + x * (Coef[1] + x * (Coef[2] + x * (Coef[3] + x * Coef[4])));
              }
              else if (x < 0.1)
              {
                double g1 = Coef[0] + 0.1 * (Coef[1] + 0.1 * (Coef[2] + 0.1 * (Coef[3] + 0.1 * Coef[4])));
                double g1p = Coef[1] + 0.2 * Coef[2] + 0.03 * Coef[3] + 0.004 * Coef[4];
                double cl = g1p * 5.0;
                double al = g1 - 0.01 * cl;
                tempy = al + cl * x * x;
              }
              else
              {
                double g9 = Coef[0] + 0.9 * (Coef[1] + 0.9 * (Coef[2] + 0.9 * (Coef[3] + 0.9 * Coef[4])));
                double g9p = Coef[1] + 1.8 * Coef[2] + 2.43 * Coef[3] + 2.916 * Coef[4];
                double cu = -g9p * 5.0;
                double bu = -2.0 * cu;
                double au = g9 - 0.81 * cu - 0.9 * bu;
                tempy = au + bu * x + cu * x * x;
              }

              myGrid._VolFactor[i] = Math.Max(atmvol * tempy, 0.0);

            }
            double vol = myGrid._VolFactor[i];

            switch (k)
            {
              case 0:
                vol *= PL;
                break;
              case 2:
                vol *= PU;
                break;
              default:
                break;
            }

            double TempA = vol * vol * 0.5;
            double TempB = ForwardRate - Dividend - TempA;

            double AR = TempA * n2 * (1.0 - theta);
            double BR = TempB * n1 * (1.0 - theta) * 0.5;

            myGrid._DiagR[i] = 1.0 - AR * 2.0;
            myGrid._SubDiagR[i] = AR - BR;
            myGrid._SuperDiagR[i] = AR + BR;

            double AL = AR; //TempA * n2 * Theta;
            double BL = BR; //TempB * n1 * Theta;

            myGrid._DiagL[i] = 1.0 + 2.0 * AL;
            myGrid._SubDiagL[i] = BL - AL;
            myGrid._SuperDiagL[i] = -AL - BL;

          }  // close i loop


          myGrid.LUDecomp();

          myGrid.TriDiagMult(k);

          //set the  boundary conditions
          myGrid.SetBC(myCN.PayStyle, k, myCN.K, ForwardTerm, DividendTerm, myCN.LowerLogAsset1);
          myGrid.SetBarrierBC(myCN.PayStyle, k);

          // take off the R vector

          myGrid._Q[1] -= myGrid._SubDiagL[1] * myGrid._V[k, 0];
          myGrid._Q[N - 1] -= myGrid._SuperDiagL[N - 1] * myGrid._V[k, N];

          // compute the solution vector
          myGrid.LUSolution(k);


        }  //next k

        //stochastic mixing

        double MR = myMR.Interpolate(arg);
        double mix = MR * TimeStep / (TauDate + 1.5 * TimeStep);
        double PrevA = mix * 0.166666667;
        double PrevC = PrevA;
        double PrevB = mix * 0.666666666;

        double B1 = (1.0 - PrevC - PrevB);
        double B2 = (1.0 - PrevA - PrevC);//(1.0 - PrevB - PrevB);
        double B3 = (1.0 - PrevA - PrevB);


        for (long i = 0; i <= N; i++)
        {
          double TempA = myGrid._V[0, i]; double TempB = myGrid._V[1, i]; double TempC = myGrid._V[2, i];
          myGrid._V[0, i] = B1 * TempA + PrevB * TempB + PrevC * TempC;
          myGrid._V[1, i] = PrevA * TempA + B2 * TempB + PrevC * TempC;
          myGrid._V[2, i] = PrevA * TempA + PrevB * TempB + B3 * TempC;
        }

      }//close time step j

      // interpolate to find results

      double T1 = (myCN.S - myCN.LowerLogAsset2) / LogAssetStep;
      long NStrike = (long)(T1);
      if (NStrike < 0 || NStrike >= myCN.NoAssetSteps)
        return 0;

      double alpha = (myCN.S - myGrid._X[NStrike]) / (myGrid._X[NStrike + 1] - myGrid._X[NStrike]);
      double TempA1 = myGrid._V[0, NStrike] * 0.16666667 + myGrid._V[1, NStrike] * 0.66666666 + myGrid._V[2, NStrike] * 0.16666667;
      double TempB1 = myGrid._V[0, NStrike + 1] * 0.16666667 + myGrid._V[1, NStrike + 1] * 0.66666666 + myGrid._V[2, NStrike + 1] * 0.16666667;
      double zeroR = myDom.Interpolate(myCN.T);
      double zeroQ = myFor.Interpolate(myCN.T);

      double temp = Math.Exp(-zeroR * myCN.T) * ((1.0 - alpha) * TempA1 + alpha * TempB1);

      // for KI options compute the vanilla and subtract
      if ((myCN.PayStyle == (long) OptionCode.BUIC) || (myCN.PayStyle == (long) OptionCode.BUIP) ||
        (myCN.PayStyle == (long) OptionCode.BDIC) || (myCN.PayStyle == (long) OptionCode.BDIP) ||
        (myCN.PayStyle == (long) OptionCode.B2IC) || (myCN.PayStyle == (long) OptionCode.B2IP))
      {
        long tPay = myCN.PayStyle;
        if ((myCN.PayStyle == (long) OptionCode.BUIC) || (myCN.PayStyle == (long) OptionCode.BDIC) ||
          (myCN.PayStyle == (long) OptionCode.B2IC))
        {
          myCN.PayStyle = (long) OptionCode.VEUC;
        }
        else
        {
          myCN.PayStyle = (long) OptionCode.VEUP;
        }
        myCN.LowerLogAsset2 = myCN.LowerVanLogAsset2;
        myCN.UpperLogAsset2 = myCN.UpperVanLogAsset2;
        double LogAssetStep2 = (myCN.UpperLogAsset2 - myCN.LowerLogAsset2) / (double) myCN.NoAssetSteps2;

        Grid myGrid2 = new Grid(myCN.NoAssetSteps2, myCN.LowerLogAsset2, LogAssetStep2);

        temp -= CNBarrierFull(myGrid2, myCN, myVol, myDom, myFor, myP1, myP2, myP3,
                            myP4, myP5, myP6, myMR);
        temp *= -1.0;
        myCN.PayStyle = tPay;
      }

      //for one touch options, use parity to compute price; ASSET PAYOUT
      if ((myCN.PayStyle == (long) OptionCode.RIAP) ||
        (myCN.PayStyle == (long) OptionCode.OTAC) ||
        (myCN.PayStyle == (long) OptionCode.OTAP))
      {
        temp -= Math.Exp(myCN.S - zeroQ * myCN.T);
        temp *= -1.0;
      }

      //for one touch options, use parity to compute price; Numeraire PAYOUT
      if ((myCN.PayStyle == (long) OptionCode.RINP) ||
        (myCN.PayStyle == (long) OptionCode.OTNC) ||
        (myCN.PayStyle == (long) OptionCode.OTNP))
      {
        temp -= Math.Exp(-zeroR * myCN.T);
        temp *= -1.0;
      }

      
      // return with error codes if necessary
      if (System.Double.IsNaN(temp))
        throw new Exception(Resource.ErrPricingFailed);
      else if (((temp < -1.0e-4) || (temp > 1.0e10)) && (myCN.PayStyle < (long) OptionCode.FRWD))
        return 0.0;
      else
        return temp;
    }


    public double CNBarrierPartial(Grid myGrid, CN myCN, Curve myVol, Curve myDom, Curve myFor, Curve myP1,
      Curve myP2, Curve myP3, Curve myP4, Curve myP5, Curve myP6, Curve myMR)
    {
      double LogAssetStep = (myCN.UpperLogAsset2 - myCN.LowerLogAsset2) / (double)myCN.NoAssetSteps2;
      if (LogAssetStep <= 0)
        return 0;

      double TimeStep = (myCN.T - myCN.t) / (double)myCN.NoTimeSteps2;
      double n1 = TimeStep / LogAssetStep;
      double n2 = n1 / LogAssetStep;
      double Theta = 0.5;

      //set the number of steps for use in working backward from T to t
      myCN.NoAssetSteps = myCN.NoAssetSteps2;

      //set the initial boundary conditions
      myGrid.SetIC(myCN.PayStyle, myCN.K, myCN.LowerLogAsset2);

      double atmvol = myVol.LinInterpolate(myCN.T);
      double rootThree = Math.Sqrt(3.0);

      for (long j = 1; j <= myCN.NoTimeSteps2; j++)
      {
        double tau = j * TimeStep;
        double TauDate = myCN.T - tau;

        // set the  forward interest rates
        double zero2 = myDom.LinInterpolate(TauDate + TimeStep);
        double zero1 = myDom.LinInterpolate(TauDate);
        double ForwardRate = (zero2 * (TauDate + TimeStep) - zero1 * TauDate) / TimeStep;

        zero2 = myFor.LinInterpolate(TauDate + TimeStep);
        zero1 = myFor.LinInterpolate(TauDate);
        double Dividend = (zero2 * (TauDate + TimeStep) - zero1 * TauDate) / TimeStep;


        // set the  interest rates between t and Expiry Time
        zero2 = myDom.LinInterpolate(myCN.T);
        zero1 = myDom.LinInterpolate(TauDate);
        double ForwardTerm = zero2 * myCN.T - zero1 * TauDate;
        double ForwardZero = myDom.LinInterpolate(TauDate + 0.5 * TimeStep);

        zero2 = myFor.LinInterpolate(myCN.T);
        zero1 = myFor.LinInterpolate(TauDate);
        double DividendTerm = zero2 * myCN.T - zero1 * TauDate;
        double DividendZero = myFor.LinInterpolate(TauDate + 0.5 * TimeStep);


        // set the stochastic scaling
        double volroott = 1.0 / (atmvol * Math.Sqrt(TauDate + 0.5 * TimeStep));
        double arg = TauDate + 0.5 * TimeStep;
        double Vovol = myP6.Interpolate(arg);
        double PL = Math.Exp(-Vovol * rootThree);
        double PU = Math.Exp(Vovol * rootThree);

        // get the local vol paramters

        _ac[0] = myP1.Interpolate(arg);
        _ac[1] = myP2.Interpolate(arg);
        _ac[2] = myP3.Interpolate(arg);
        _ac[3] = myP4.Interpolate(arg);
        _ac[4] = myP5.Interpolate(arg);

        double preFactor = myCN.Anchor + (ForwardZero - DividendZero) * (TauDate + 0.5 * TimeStep);
        //scroll across the three states

        long N = myCN.NoAssetSteps;

        for (int k = 0; k < VOLSTATES; k++)
        {
          // scroll from 1 to N-1 and fill up the matrix

          for (int i = 1; i < myCN.NoAssetSteps; i++)
          {
            double ta = myGrid._X[i] - preFactor;
            double za = ta * volroott;
            if (k == 0)
            {
              double n = 0;
              if (za > 6.0)
              {
                n = 1.0;
              }
              else if (za < -6.0)
              {
                n = 0.0;
              }
              else
              {

                double a = Math.Abs(za);
                double t = 1.0 / (1.0 + a * p);
                double b = c2 * Math.Exp((-za) * (za / 2.0));
                n = ((((b5 * t + b4) * t + b3) * t + b2) * t + b1) * t;
                n = 1.0 - b * n;
                if (za < 0.0) n = 1.0 - n;
              }
              double ya = n;

              F[0] = _ac[0] - _ac[1] + _ac[2] + _ac[3] - _ac[4];
              F[1] = _ac[0] - _ac[1] + _ac[2];
              F[2] = _ac[0];
              F[3] = _ac[0] + _ac[1] + _ac[2];
              F[4] = _ac[0] + _ac[1] + _ac[2] + _ac[3] + _ac[4];


              for (int idx = 0; idx < 5; idx++)
              {
                Coef[idx] = 0.0;
                for (int jdx = 0; jdx < 5; jdx++)
                  Coef[idx] += _rmat[idx, jdx] * F[jdx];
              }

              //apply formulas
              double tempy = 0.0;
              double x = ya;
              if ((x >= 0.1) && (x <= 0.9))
              {
                tempy = Coef[0] + x * (Coef[1] + x * (Coef[2] + x * (Coef[3] + x * Coef[4])));
              }
              else if (x < 0.1)
              {
                double g1 = Coef[0] + 0.1 * (Coef[1] + 0.1 * (Coef[2] + 0.1 * (Coef[3] + 0.1 * Coef[4])));
                double g1p = Coef[1] + 0.2 * Coef[2] + 0.03 * Coef[3] + 0.004 * Coef[4];
                double cl = g1p * 5.0;
                double al = g1 - 0.01 * cl;
                tempy = al + cl * x * x;
              }
              else
              {
                double g9 = Coef[0] + 0.9 * (Coef[1] + 0.9 * (Coef[2] + 0.9 * (Coef[3] + 0.9 * Coef[4])));
                double g9p = Coef[1] + 1.8 * Coef[2] + 2.43 * Coef[3] + 2.916 * Coef[4];
                double cu = -g9p * 5.0;
                double bu = -2.0 * cu;
                double au = g9 - 0.81 * cu - 0.9 * bu;
                tempy = au + bu * x + cu * x * x;
              }

              myGrid._VolFactor[i] = Math.Max(atmvol * tempy, 0.0);

            }

            double vol = myGrid._VolFactor[i];

            switch (k)
            {
              case 0:
                vol *= PL;
                break;
              case 2:
                vol *= PU;
                break;
              default:
                break;
            }

            //construct Temp variables
            double TempA = vol * vol * 0.5;
            double TempB = ForwardRate - Dividend - TempA;

            double AR = TempA * n2 * (1.0 - Theta);
            double BR = TempB * n1 * (1.0 - Theta) * 0.5;

            myGrid._DiagR[i] = 1.0 - AR * 2.0;
            myGrid._SubDiagR[i] = AR - BR;
            myGrid._SuperDiagR[i] = AR + BR;

            double AL = AR; //TempA * n2 * Theta;
            double BL = BR; //TempB * n1 * Theta;

            myGrid._DiagL[i] = 1.0 + 2.0 * AL;
            myGrid._SubDiagL[i] = BL - AL;
            myGrid._SuperDiagL[i] = -AL - BL;

          }  // close i loop


          myGrid.LUDecomp();

          myGrid.TriDiagMult(k);

          //set the  boundary conditions
          N = myCN.NoAssetSteps;

          myGrid.SetBC(myCN.PayStyle, k, myCN.K, ForwardTerm, DividendTerm, myCN.LowerLogAsset1);

          //if a late start barrier, then apply the boundary condition
          if (myCN.StepFlag == 2)
          {
            myGrid.SetPartialBarrierBC(myCN.PayStyle, k);
          }


          // take off the R vector

          myGrid._Q[1] -= myGrid._SubDiagL[1] * myGrid._V[k, 0];
          myGrid._Q[N - 1] -= myGrid._SuperDiagL[N - 1] * myGrid._V[k, N];

          myGrid.LUSolution(k);

        }  //next k

        //stochastic mixing

        double MR = myMR.Interpolate(arg);
        double mix = MR * TimeStep / (TauDate + TimeStep);
        double PrevA = mix * 0.166666667;
        double PrevC = PrevA;
        double PrevB = mix * 0.666666666;

        double B1 = (1.0 - PrevC - PrevB);
        double B2 = (1.0 - PrevA - PrevC);//(1.0 - PrevB - PrevB);
        double B3 = (1.0 - PrevA - PrevB);

        for (long i = 0; i <= N; i++)
        {
          double TempA = myGrid._V[0, i]; double TempB = myGrid._V[1, i]; double TempC = myGrid._V[2, i];
          myGrid._V[0, i] = B1 * TempA + PrevB * TempB + PrevC * TempC;
          myGrid._V[1, i] = PrevA * TempA + B2 * TempB + PrevC * TempC;//PrevB * TempA + B2 * TempB + PrevB * TempC;
          myGrid._V[2, i] = PrevA * TempA + PrevB * TempB + B3 * TempC;
        }
      }  // next j  time step

      //
      //  now work backward from t to 0
      //

      LogAssetStep = (myCN.UpperLogAsset1 - myCN.LowerLogAsset1) / (double)myCN.NoAssetSteps1;
      TimeStep = myCN.t / (double)myCN.NoTimeSteps1;
      n1 = TimeStep / LogAssetStep;
      n2 = n1 / LogAssetStep;

      //set the number of steps for use in working backward from T to t
      myCN.NoAssetSteps = myCN.NoAssetSteps1;


      // make the new grid
      Grid myGrid2 = new Grid(myCN.NoAssetSteps, myCN.LowerLogAsset1, LogAssetStep);

      for (long i = 0; i <= myCN.NoAssetSteps; i++)
      {
        if (myGrid2._X[i] <= myCN.LowerLogAsset2)
        {
          myGrid2._V[0, i] = myGrid2._V[1, i] = myGrid2._V[2, i] = 0.0;
        }
        else if (myGrid2._X[i] >= myCN.UpperLogAsset2)
        {
          myGrid2._V[0, i] = myGrid2._V[1, i] = myGrid2._V[2, i] = 0.0;
        }
        else
        {
          double Ts = (myGrid2._X[i] - myCN.LowerLogAsset2) * myCN.NoAssetSteps2 /
            (myCN.UpperLogAsset2 - myCN.LowerLogAsset2);
          long NStrike = (long)Ts;
          //safety valve
          NStrike = (NStrike >= myCN.NoAssetSteps2) ? myCN.NoAssetSteps2 - 1 : NStrike;
          double alpha = (myGrid2._X[i] - myGrid._X[NStrike]) /
            (myGrid._X[NStrike + 1] - myGrid._X[NStrike]);
          myGrid2._V[0, i] = (1.0 - alpha) * myGrid._V[0, NStrike] + alpha * myGrid._V[0, NStrike + 1];
          myGrid2._V[1, i] = (1.0 - alpha) * myGrid._V[1, NStrike] + alpha * myGrid._V[1, NStrike + 1];
          myGrid2._V[2, i] = (1.0 - alpha) * myGrid._V[2, NStrike] + alpha * myGrid._V[2, NStrike + 1];
        }
      }


      for (long j = 1; j <= myCN.NoTimeSteps1; j++)
      {
        double tau = j * TimeStep;
        double TauDate = myCN.t - tau;

        // set the  forward interest rates
        double zero2 = myDom.LinInterpolate(TauDate + TimeStep);
        double zero1 = myDom.LinInterpolate(TauDate);
        double ForwardRate = (zero2 * (TauDate + TimeStep) - zero1 * TauDate) / TimeStep;

        zero2 = myFor.LinInterpolate(TauDate + TimeStep);
        zero1 = myFor.LinInterpolate(TauDate);
        double Dividend = (zero2 * (TauDate + TimeStep) - zero1 * TauDate) / TimeStep;


        // set the  interest rates between t and Expiry Time
        zero2 = myDom.LinInterpolate(myCN.T);
        zero1 = myDom.LinInterpolate(TauDate);
        double ForwardTerm = zero2 * myCN.T - zero1 * TauDate;
        double ForwardZero = myDom.LinInterpolate(TauDate + 0.5 * TimeStep);

        zero2 = myFor.LinInterpolate(myCN.T);
        zero1 = myFor.LinInterpolate(TauDate);
        double DividendTerm = zero2 * myCN.T - zero1 * TauDate;
        double DividendZero = myFor.LinInterpolate(TauDate + 0.5 * TimeStep);


        // set the stochastic scaling
        double volroott = 1.0 / (atmvol * Math.Sqrt(TauDate + 0.5 * TimeStep));
        double arg = TauDate + 0.5 * TimeStep;
        double Vovol = myP6.Interpolate(arg);
        double PL = Math.Exp(-Vovol * rootThree);
        double PU = Math.Exp(Vovol * rootThree);

        // get the local vol paramters

        _ac[0] = myP1.Interpolate(arg);
        _ac[1] = myP2.Interpolate(arg);
        _ac[2] = myP3.Interpolate(arg);
        _ac[3] = myP4.Interpolate(arg);
        _ac[4] = myP5.Interpolate(arg);

        double preFactor = myCN.Anchor + (ForwardZero - DividendZero) * (TauDate + 0.5 * TimeStep);
        //scroll across the three states

        long N = myCN.NoAssetSteps;

        for (int k = 0; k < VOLSTATES; k++)
        {
          // scroll from 1 to N-1 and fill up the matrix

          for (int i = 1; i < myCN.NoAssetSteps; i++)
          {
            double ta = myGrid2._X[i] - preFactor;
            double za = ta * volroott;
            if (k == 0)
            {
              double n = 0;
              if (za > 6.0)
              {
                n = 1.0;
              }
              else if (za < -6.0)
              {
                n = 0.0;
              }
              else
              {

                double a = Math.Abs(za);
                double t = 1.0 / (1.0 + a * p);
                double b = c2 * Math.Exp((-za) * (za / 2.0));
                n = ((((b5 * t + b4) * t + b3) * t + b2) * t + b1) * t;
                n = 1.0 - b * n;
                if (za < 0.0) n = 1.0 - n;
              }
              double ya = n;

              F[0] = _ac[0] - _ac[1] + _ac[2] + _ac[3] - _ac[4];
              F[1] = _ac[0] - _ac[1] + _ac[2];
              F[2] = _ac[0];
              F[3] = _ac[0] + _ac[1] + _ac[2];
              F[4] = _ac[0] + _ac[1] + _ac[2] + _ac[3] + _ac[4];


              for (int idx = 0; idx < 5; idx++)
              {
                Coef[idx] = 0.0;
                for (int jdx = 0; jdx < 5; jdx++)
                  Coef[idx] += _rmat[idx, jdx] * F[jdx];
              }

              //apply formulas
              double tempy = 0.0;
              double x = ya;
              if ((x >= 0.1) && (x <= 0.9))
              {
                tempy = Coef[0] + x * (Coef[1] + x * (Coef[2] + x * (Coef[3] + x * Coef[4])));
              }
              else if (x < 0.1)
              {
                double g1 = Coef[0] + 0.1 * (Coef[1] + 0.1 * (Coef[2] + 0.1 * (Coef[3] + 0.1 * Coef[4])));
                double g1p = Coef[1] + 0.2 * Coef[2] + 0.03 * Coef[3] + 0.004 * Coef[4];
                double cl = g1p * 5.0;
                double al = g1 - 0.01 * cl;
                tempy = al + cl * x * x;
              }
              else
              {
                double g9 = Coef[0] + 0.9 * (Coef[1] + 0.9 * (Coef[2] + 0.9 * (Coef[3] + 0.9 * Coef[4])));
                double g9p = Coef[1] + 1.8 * Coef[2] + 2.43 * Coef[3] + 2.916 * Coef[4];
                double cu = -g9p * 5.0;
                double bu = -2.0 * cu;
                double au = g9 - 0.81 * cu - 0.9 * bu;
                tempy = au + bu * x + cu * x * x;
              }

              myGrid2._VolFactor[i] = Math.Max(atmvol * tempy, 0.0);

            }

            double vol = myGrid2._VolFactor[i];
            switch (k)
            {
              case 0:
                vol *= PL;
                break;
              case 2:
                vol *= PU;
                break;
              default:
                break;
            }

            //construct Temp variables



            double TempA = vol * vol * 0.5;
            double TempB = ForwardRate - Dividend - TempA;

            double AR = TempA * n2 * (1.0 - Theta);
            double BR = TempB * n1 * (1.0 - Theta) * 0.5;

            myGrid2._DiagR[i] = 1.0 - AR * 2.0;
            myGrid2._SubDiagR[i] = AR - BR;
            myGrid2._SuperDiagR[i] = AR + BR;

            double AL = AR; //TempA * n2 * Theta;
            double BL = BR; //TempB * n1 * Theta;

            myGrid2._DiagL[i] = 1.0 + 2.0 * AL;
            myGrid2._SubDiagL[i] = BL - AL;
            myGrid2._SuperDiagL[i] = -AL - BL;

          }  // close i loop


          myGrid2.LUDecomp();

          myGrid2.TriDiagMult(k);

          //set the  boundary conditions

          myGrid2.SetBC(myCN.PayStyle, k, myCN.K, ForwardTerm, DividendTerm, myCN.LowerLogAsset1);

          //if early start option, apply the BC
          if (myCN.StepFlag == 1)
          {
            myGrid2.SetPartialBarrierBC(myCN.PayStyle, k);
          }


          // take off the R vector

          myGrid2._Q[1] -= myGrid2._SubDiagL[1] * myGrid2._V[k, 0];
          myGrid2._Q[N - 1] -= myGrid2._SuperDiagL[N - 1] * myGrid2._V[k, N];

          myGrid2.LUSolution(k);


        }  //next k

        //stochastic mixing

        double MR = myMR.Interpolate(arg);
        double mix = MR * TimeStep / (TauDate + TimeStep);
        double PrevA = mix * 0.166666667;
        double PrevC = PrevA;
        double PrevB = mix * 0.666666666;

        double B1 = (1.0 - PrevC - PrevB);
        double B2 = (1.0 - PrevA - PrevC);//(1.0 - PrevB - PrevB);
        double B3 = (1.0 - PrevA - PrevB);

        for (long i = 0; i <= N; i++)
        {
          double TempA = myGrid2._V[0, i]; double TempB = myGrid2._V[1, i]; double TempC = myGrid2._V[2, i];
          myGrid2._V[0, i] = B1 * TempA + PrevB * TempB + PrevC * TempC;
          myGrid2._V[1, i] = PrevA * TempA + B2 * TempB + PrevC * TempC;//PrevB * TempA + B2 * TempB + PrevB * TempC;
          myGrid2._V[2, i] = PrevA * TempA + PrevB * TempB + B3 * TempC;
        }
      }  // next j  time step



      // interpolate to find results

      double T1 = (myCN.S - myCN.LowerLogAsset1) / LogAssetStep;
      long NStrike1 = (long)T1;
      if (NStrike1 < 0 || NStrike1 >= myCN.NoAssetSteps)
        return 0;

      double alpha1 = (myCN.S - myGrid2._X[NStrike1]) / (myGrid2._X[NStrike1 + 1] - myGrid2._X[NStrike1]);
      double TempA1 = myGrid2._V[0, NStrike1] * 0.16666667 + myGrid2._V[1, NStrike1] * 0.66666666 + myGrid2._V[2, NStrike1] * 0.16666667;
      double TempB1 = myGrid2._V[0, NStrike1 + 1] * 0.16666667 + myGrid2._V[1, NStrike1 + 1] * 0.66666666 + myGrid2._V[2, NStrike1 + 1] * 0.16666667;
      double zero = myDom.LinInterpolate(myCN.T);


      double temp = Math.Exp(-zero * myCN.T) * ((1.0 - alpha1) * TempA1 + alpha1 * TempB1);


      // for KI options compute the vanilla and subtract
      if ((myCN.PayStyle == (long) OptionCode.SDIC) || (myCN.PayStyle == (long) OptionCode.SDIP) ||
        (myCN.PayStyle == (long) OptionCode.SUIC) || (myCN.PayStyle == (long) OptionCode.SUIP))
      {
        long tPay = myCN.PayStyle;
        if ((myCN.PayStyle == (long) OptionCode.SDIC) || (myCN.PayStyle == (long) OptionCode.SUIC))
        {
          myCN.PayStyle = (long) OptionCode.VEUC;
        }
        else
        {
          myCN.PayStyle = (long) OptionCode.VEUP;
        }
        myCN.LowerLogAsset2 = myCN.LowerVanLogAsset2;
        myCN.UpperLogAsset2 = myCN.UpperVanLogAsset2;

        double LogAssetStep2 = (myCN.UpperLogAsset2 - myCN.LowerLogAsset2) / (double)myCN.NoAssetSteps2;

        Grid myGrid3 = new Grid(myCN.NoAssetSteps2, myCN.LowerLogAsset2, LogAssetStep2);

        temp -= CNBarrierFull(myGrid3, myCN, myVol, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);
        temp *= -1.0;
        myCN.PayStyle = tPay;
      }

      if (System.Double.IsNaN(temp))
        throw new Exception(Resource.ErrPricingFailed);
      else if (((temp < -1.0e-4) || (temp > 1.0e10)) && (myCN.PayStyle < (long) OptionCode.FRWD))
        return 0.0;
      else
        return temp;
    }

    //function to be called
    public double CNBarrier(CN myCN, double[] tenors, double[] vols, double[] dom, double[] fors,
                            double[] p1, double[] p2, double[] p3, double[] p4, double[] p5, double[] p6,
                            double[] mr)
    {
      double temp = 0.0;

      Curve myVols = new Curve(tenors.Length - 1, tenors, vols);
      Curve myDom = new Curve(tenors.Length - 1, tenors, dom);
      Curve myFor = new Curve(tenors.Length - 1, tenors, fors);
      Curve myP1 = new Curve(tenors.Length - 1, tenors, p1);
      Curve myP2 = new Curve(tenors.Length - 1, tenors, p2);
      Curve myP3 = new Curve(tenors.Length - 1, tenors, p3);
      Curve myP4 = new Curve(tenors.Length - 1, tenors, p4);
      Curve myP5 = new Curve(tenors.Length - 1, tenors, p5);
      Curve myP6 = new Curve(tenors.Length - 1, tenors, p6);
      Curve myMR = new Curve(tenors.Length - 1, tenors, mr);

      double LogAssetStep = (myCN.UpperLogAsset2 - myCN.LowerLogAsset2) / (double)myCN.NoAssetSteps2;
      Grid myGrid = new Grid(myCN.NoAssetSteps2, myCN.LowerLogAsset2, LogAssetStep);

      if (myCN.T > 0.0001)
      {
          if ((myCN.PayStyle < (long)OptionCode.SDOC) || (myCN.PayStyle > (long)OptionCode.SUIP))
              temp = CNBarrierFull(myGrid, myCN, myVols, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);
          else
              temp = CNBarrierPartial(myGrid, myCN, myVols, myDom, myFor, myP1, myP2, myP3, myP4, myP5, myP6, myMR);
      }
      else  //return intrinsic value
      {
          var myB = new NabLibBlack
                        {
                            K = myCN.K,
                            Spot = Math.Exp(myCN.S),
                            Upper = Math.Exp(myCN.UpperVanLogAsset2),
                            Lower = Math.Exp(myCN.LowerVanLogAsset2),
                            PayStyle = myCN.PayStyle
                        };
          temp = myB.IntrinsicValue();
      }
      return temp;
    }


    //calibration routine
    ///<summary>
    ///</summary>
    ///<param name="tenors"></param>
    ///<param name="vols"></param>
    ///<param name="dom"></param>
    ///<param name="fors"></param>
    ///<param name="rr25"></param>
    ///<param name="fly25"></param>
    ///<param name="rr10"></param>
    ///<param name="fly10"></param>
    ///<param name="speed"></param>
    ///<param name="spot"></param>
    ///<param name="bLHS"></param>
    ///<param name="bStoch"></param>
    ///<param name="targetTenor"></param>
    ///<param name="sink"></param>
    ///<returns></returns>
    public bool Calibrate(double[] tenors, double[] vols, double[] dom, double[] fors,
                          double[] rr25, double[] fly25, double[] rr10, double[] fly10, double[] speed,
                          double spot, bool bLHS, bool bStoch, double targetTenor, IProgressSink sink)
    {
      //data check
      int n = tenors.Length;
      int Nstrikes = 5;

      double[] DataStrikes = new double[Nstrikes];
      double[] DataPrices = new double[Nstrikes];
      double[] ImpVols = new double[Nstrikes];
      double[] p = new double[6];
      double[] vec = new double[6];
      double[] fitPrices = new double[6];
      double[] fitVols = new double[6];
      double[] DataParams = new double[10];
      double VolP, VolC, SpeedKC, SpeedKP;

      mp0 = new double[n];
      mp1 = new double[n];
      mp2 = new double[n];
      mp3 = new double[n];
      mp4 = new double[n];

      fp0 = new double[n];
      fp1 = new double[n];
      fp2 = new double[n];
      fp3 = new double[n];
      fp4 = new double[n];

      fv0 = new double[n];
      fv1 = new double[n];
      fv2 = new double[n];
      fv3 = new double[n];
      fv4 = new double[n];

      var myB = new NabLibBlack();

      // check if the coefficient arrays exist
      if (a0 == null)
      {
        a0 = new double[n];
        a1 = new double[n];
        a2 = new double[n];
        a3 = new double[n];
        a4 = new double[n];
        a5 = new double[n];
        mr = new double[n];
        for (int idx = 0; idx != n; idx++)
        {
          a0[idx] = 0.9;
          a1[idx] = 0.2;
          a2[idx] = 0.0;
          a3[idx] = 0.0;
          a4[idx] = 0.0;
          a5[idx] = 0.2;
          mr[idx] = 1.0;
        }
      }

      // actual tenors to calibrate to
      int targetN = tenors.TakeWhile((x) => x <= targetTenor).ToArray().Length;
      Progress sinkProxy = new Progress { CurrentStep = 0, MinStep = 0, MaxStep = targetN - 1, Sink = sink };

      int istatus = 2;
      for (int k = 0; k != targetN; k++)
      {
        if (sinkProxy.Cancelled)
          break;

        sinkProxy.StartStep(k);

        //compute the implied vols and strikes
        ImpVols[0] = vols[k] + fly10[k] - 0.5 * rr10[k];
        ImpVols[1] = vols[k] + fly25[k] - 0.5 * rr25[k];
        ImpVols[2] = vols[k];
        ImpVols[3] = vols[k] + fly25[k] + 0.5 * rr25[k];
        ImpVols[4] = vols[k] + fly10[k] + 0.5 * rr10[k];

        DataStrikes[0] = NabLibUtilities.ComputeStrike(tenors[k], dom[k], fors[k], spot, ImpVols[0], -0.10, DeltaCutTime, bLHS);
        DataStrikes[1] = NabLibUtilities.ComputeStrike(tenors[k], dom[k], fors[k], spot, ImpVols[1], -0.25, DeltaCutTime, bLHS);
        DataStrikes[2] = NabLibUtilities.ComputeATMStrike(tenors[k], dom[k], fors[k], spot, ImpVols[2], bLHS);
        DataStrikes[3] = NabLibUtilities.ComputeStrike(tenors[k], dom[k], fors[k], spot, ImpVols[3], 0.25, DeltaCutTime, bLHS);
        DataStrikes[4] = NabLibUtilities.ComputeStrike(tenors[k], dom[k], fors[k], spot, ImpVols[4], 0.10, DeltaCutTime, bLHS);

        //use the black shcole price to get the option prices for P,P,C,C,C
        myB.T = tenors[k];
        myB.DomRate = dom[k];
        myB.ForRate = fors[k];
        myB.Spot = spot;
        myB.Fwd = spot * Math.Exp((dom[k] - fors[k]) * tenors[k]);

        myB.K = DataStrikes[0];
        myB.Sig = ImpVols[0];
        myB.PayStyle = (long) OptionCode.VEUP;
        DataPrices[0] = myB.Price();

        myB.K = DataStrikes[1];
        myB.Sig = ImpVols[1];
        myB.PayStyle = (long) OptionCode.VEUP;
        DataPrices[1] = myB.Price();

        myB.K = DataStrikes[2];
        myB.Sig = ImpVols[2];
        myB.PayStyle = (long) OptionCode.VEUC;
        DataPrices[2] = myB.Price();

        myB.K = DataStrikes[3];
        myB.Sig = ImpVols[3];
        myB.PayStyle = (long) OptionCode.VEUC;
        DataPrices[3] = myB.Price();

        myB.K = DataStrikes[4];
        myB.Sig = ImpVols[4];
        myB.PayStyle = (long) OptionCode.VEUC;
        DataPrices[4] = myB.Price();

        //set up the speed
        VolP = ImpVols[1] - 0.5 * speed[k];
        VolC = ImpVols[3] + 0.5 * speed[k];
        SpeedKP = NabLibUtilities.ComputeStrike(tenors[k], dom[k], fors[k], spot * 1.01, VolP, -0.25, DeltaCutTime, bLHS);
        SpeedKC = NabLibUtilities.ComputeStrike(tenors[k], dom[k], fors[k], spot * 1.01, VolC, 0.25, DeltaCutTime, bLHS);

        DataParams[0] = tenors[k];
        DataParams[1] = spot;
        DataParams[2] = 200.0;
        int nstep = (int)(tenors[k] / 0.025);
        nstep = (nstep < 25) ? 25 : nstep;
        nstep = (nstep > 200) ? 200 : nstep;
        DataParams[3] = (double)nstep;

        if (bStoch)
          DataParams[6] = 0.0;
        else
          DataParams[6] = 1.0;  //turn off the stochastic

        //load the guesses
        p[0] = a0[k];
        p[1] = a1[k];
        p[2] = a2[k];
        p[3] = a3[k];
        p[4] = a4[k];
        p[5] = a5[k];

        istatus = Newton.newton6(k, p, vec, Nstrikes, Toolerance, DataParams, DataPrices, DataStrikes,
                                 ImpVols, n - 1, tenors, dom, fors, vols, a0, a1, a2, a3, a4, a5, speed, mr, fitPrices, fitVols,
                                 2, SpeedKC, SpeedKP, VolP, VolC);

        if (istatus != 2) break;

        // store market prices
        mp0[k] = DataPrices[0];
        mp1[k] = DataPrices[1];
        mp2[k] = DataPrices[2];
        mp3[k] = DataPrices[3];
        mp4[k] = DataPrices[4];

        // store fit prices;
        fp0[k] = fitPrices[0];
        fp1[k] = fitPrices[1];
        fp2[k] = fitPrices[2];
        fp3[k] = fitPrices[3];
        fp4[k] = fitPrices[4];

        // store fit vols;
        fv0[k] = fitVols[0];
        fv1[k] = fitVols[1];
        fv2[k] = fitVols[2];
        fv3[k] = fitVols[3];
        fv4[k] = fitVols[4];

        sinkProxy.EndStep(k);
      }

      return istatus == 2;
    }
  }
}
