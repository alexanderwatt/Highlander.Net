using System;
using National.QRSC.Numerics.DataStructures;
using National.QRSC.Numerics.Utilities;

namespace National.QRSC.Analytics.NabLib
{
  ///<summary>
  ///</summary>
  public static class CNGreeks
  {
    ///<summary>
    ///</summary>
    ///<param name="myCN"></param>
    ///<param name="tenors"></param>
    ///<param name="vols"></param>
    ///<param name="dom"></param>
    ///<param name="fors"></param>
    ///<param name="p1"></param>
    ///<param name="p2"></param>
    ///<param name="p3"></param>
    ///<param name="p4"></param>
    ///<param name="p5"></param>
    ///<param name="p6"></param>
    ///<param name="mr"></param>
    ///<returns></returns>
    public static OptionGreeks Compute(CN myCN, double[] tenors, double[] vols, double[] dom, double[] fors,
                                       double[] p1, double[] p2, double[] p3, double[] p4, double[] p5, double[] p6,
                                       double[] mr)
    {
      var temp = new OptionGreeks();

      //double[] temp = new double[6]; //delta,gamma,vega,rho,phi,theta

      var myP = new Pricer();

      //compute delta and gamma
      double price = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr);

      double shift = ComputeSpotShift(myCN);

      myCN.S = Math.Log(Math.Exp(myCN.S) + shift);
      double priceUp = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr);
      myCN.S = Math.Log(Math.Exp(myCN.S) - 2.0 * shift);
      double priceDn = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr);
      myCN.S = Math.Log(Math.Exp(myCN.S) + shift);

      temp.Delta = (priceUp - priceDn) / (2.0 * shift);
      temp.Gamma = (priceUp + priceDn - 2.0 * price) / (shift * shift);

      temp.Gamma = 0.01 * myCN.S * temp.Gamma;

      //compute vega
      for (long idx = 0; idx != vols.Length; idx++)
        vols[idx] += 0.01;

      priceUp = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr);
      temp.Vega = priceUp - price;

      for (long idx = 0; idx != vols.Length; idx++)
        vols[idx] -= 0.01;


      //compute rho
      for (long idx = 0; idx != vols.Length; idx++)
        dom[idx] += 0.0001;

      priceUp = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr);
      temp.Rho = priceUp - price;

      for (long idx = 0; idx != vols.Length; idx++)
        dom[idx] -= 0.0001;

      //compute the phi
      for (long idx = 0; idx != vols.Length; idx++)
        fors[idx] += 0.0001;

      priceUp = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr);
      temp.Phi = priceUp - price;

      for (long idx = 0; idx != vols.Length; idx++)
        fors[idx] -= 0.0001;


      //compute theta
      if (myCN.StepFlag == 0)  //non-step barrier
      {
        if (myCN.T > 1.0 / 365.0)  //more than one day to run
        {
          myCN.T -= 1.0 / 365.0;
          temp.Theta = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr) - price;
          myCN.T += 1.0 / 365.0;
        }
        else //use the intrinsic value
        {
          temp.Theta = myCN.IntrinsicValue() - price;
        }
      }
      else if (myCN.StepFlag == 1) //early finish
      {
        if (myCN.T > 1.0 / 365.0)  //more than one day to run
        {
          if (myCN.t > 1.0 / 365.0)  //barrier will still be live
          {
            myCN.T -= 1.0 / 365.0;
            temp.Theta = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr) - price;
            myCN.T += 1.0 / 365.0;
          }
          else //early finish barrier ends tomorrow
          {
            long savePay = myCN.PayStyle;
            //myCN.PayStyle = (long)MapEarlyFinish(myCN);
            temp.Theta = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr) - price;
            myCN.PayStyle = savePay;
          }
        }
        else  //the EF expires tomorrow
        {
          temp.Theta = myCN.IntrinsicValue() - price;
        }
      }
      else if (myCN.StepFlag == 2) //late start
      {
        if (myCN.T > 1.0 / 365.0)  //more than one day to run
        {
          if (myCN.t > 1.0 / 365.0)  //barrier will still be live
          {
            myCN.T -= 1.0 / 365.0;
            temp.Theta = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr) - price;
            myCN.T += 1.0 / 365.0;
          }
          else //late start barrier begins
          {
            long savePay = myCN.PayStyle;
            // myCN.PayStyle = (long)MapLateStart(myCN);
            temp.Theta = myP.CNBarrier(myCN, tenors, vols, dom, fors, p1, p2, p3, p4, p5, p6, mr) - price;
            myCN.PayStyle = savePay;
          }
        }
        else  //the LS expires tomorrow
        {
          temp.Theta = myCN.IntrinsicValue() - price;
        }
      }
      return temp;
    }

    private static double ComputeSpotShift(CN myCN)
    {
      double shift = 0.01 * Math.Exp(myCN.S);

      //for single barriers use this logic
      if ((myCN.PayStyle >= (long)OptionCode.SDOC) && (myCN.PayStyle <= (long)OptionCode.SUIP))
      {
        //if early finish
        if (myCN.StepFlag == 1)
        {
          //lower barriers
          if ((myCN.PayStyle == (long)OptionCode.SDIC) || (myCN.PayStyle == (long)OptionCode.SDIP) ||
            (myCN.PayStyle == (long)OptionCode.SDOC) || (myCN.PayStyle == (long)OptionCode.SDOP))
            shift = Math.Min(shift, 0.9 * Math.Abs(Math.Exp(myCN.S) - Math.Exp(myCN.LowerLogAsset1)));

          //upper barriers
          if ((myCN.PayStyle == (long)OptionCode.SUIC) || (myCN.PayStyle == (long)OptionCode.SUIP) ||
            (myCN.PayStyle == (long)OptionCode.SUOC) || (myCN.PayStyle == (long)OptionCode.SUOP))
            shift = Math.Min(shift, 0.9 * Math.Abs(Math.Exp(myCN.S) - Math.Exp(myCN.UpperLogAsset1)));
        }
      }
      else
      {
        //lower barriers
        if ((myCN.PayStyle == (long)OptionCode.B2IC) || (myCN.PayStyle == (long)OptionCode.B2IP) ||
          (myCN.PayStyle == (long)OptionCode.B2OC) || (myCN.PayStyle == (long)OptionCode.B2OP) ||
          (myCN.PayStyle == (long)OptionCode.BDOC) || (myCN.PayStyle == (long)OptionCode.BDOP) ||
          (myCN.PayStyle == (long)OptionCode.BDIC) || (myCN.PayStyle == (long)OptionCode.BDIP) ||
          (myCN.PayStyle == (long)OptionCode.OTAP) || (myCN.PayStyle == (long)OptionCode.NTAP) ||
          (myCN.PayStyle == (long)OptionCode.OTNP) || (myCN.PayStyle == (long)OptionCode.NTNP) ||
          (myCN.PayStyle == (long)OptionCode.RBAP) || (myCN.PayStyle == (long)OptionCode.RBNP) ||
          (myCN.PayStyle == (long)OptionCode.RIAP) || (myCN.PayStyle == (long)OptionCode.RINP))
          shift = Math.Min(shift, 0.9 * Math.Abs(Math.Exp(myCN.S) - Math.Exp(myCN.LowerLogAsset2)));

        //upperbarriers
        if ((myCN.PayStyle == (long)OptionCode.B2IC) || (myCN.PayStyle == (long)OptionCode.B2IP) ||
         (myCN.PayStyle == (long)OptionCode.B2OC) || (myCN.PayStyle == (long)OptionCode.B2OP) ||
         (myCN.PayStyle == (long)OptionCode.BUOC) || (myCN.PayStyle == (long)OptionCode.BUOP) ||
         (myCN.PayStyle == (long)OptionCode.BUIC) || (myCN.PayStyle == (long)OptionCode.BUIP) ||
         (myCN.PayStyle == (long)OptionCode.OTAC) || (myCN.PayStyle == (long)OptionCode.NTAC) ||
         (myCN.PayStyle == (long)OptionCode.OTNC) || (myCN.PayStyle == (long)OptionCode.NTNC) ||
         (myCN.PayStyle == (long)OptionCode.RBAP) || (myCN.PayStyle == (long)OptionCode.RBNP) ||
         (myCN.PayStyle == (long)OptionCode.RIAP) || (myCN.PayStyle == (long)OptionCode.RINP))
          shift = Math.Min(shift, 0.9 * Math.Abs(Math.Exp(myCN.S) - Math.Exp(myCN.UpperLogAsset2)));
      }
      return shift;
    }
  }
}