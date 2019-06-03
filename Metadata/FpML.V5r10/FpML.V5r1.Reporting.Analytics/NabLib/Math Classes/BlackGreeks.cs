using System;
using Orion.Numerics.OptionModels;
using Orion.Numerics.Utilities;

namespace Orion.Analytics.NabLib
{
  ///<summary>
  ///</summary>
    public static class BlackGreeks
  {
      ///<summary>
      ///</summary>
      ///<param name="myB"></param>
      ///<returns></returns>
      public static OptionGreeks Compute(NabLibBlack myB)
    {
      var result = new OptionGreeks();

      //get the delta & gamma
      double save = myB.Fwd;
      double saveSpot = myB.Spot;
      double price = myB.Price();
      double shift = ComputeSpotShift(myB);

      myB.Spot += shift;
      myB.Fwd = myB.Spot * Math.Exp((myB.DomRate - myB.ForRate) * myB.T);
      double priceUp = myB.Price();

      myB.Spot -= 2.0 * shift;
      myB.Fwd = myB.Spot * Math.Exp((myB.DomRate - myB.ForRate) * myB.T);
      double priceDn = myB.Price();

      myB.Fwd = save;
      myB.Spot = saveSpot;

      result.Delta = (priceUp - priceDn) / (2.0 * shift);
      result.Gamma = (priceUp + priceDn - 2.0 * price) / (shift * shift);

      // compute 1% spot * gamma
      result.Gamma = 0.01 * myB.Spot * result.Gamma;

      //compute the 1% vega
      myB.Sig += 0.01;
      priceUp = myB.Price();
      myB.Sig -= 0.01;
      result.Vega = priceUp - price;

      //rho: 1 bp in dom rates
      save = myB.DomRate;
      myB.DomRate += 0.0001;
      myB.Fwd = myB.Spot * Math.Exp((myB.DomRate - myB.ForRate) * myB.T);
      priceUp = myB.Price();
      result.Rho = (priceUp - price) / 0.0001;
      result.Rho *= 0.01;
      myB.DomRate -= 0.0001;

      //phi: 1 bp in for rates
      save = myB.ForRate;
      myB.ForRate += 0.0001;
      myB.Fwd = myB.Spot * Math.Exp((myB.DomRate - myB.ForRate) * myB.T);
      priceUp = myB.Price();
      result.Phi = (priceUp - price) / 0.0001;
      result.Phi *= 0.01;
      myB.ForRate -= 0.0001;

      //compute theta
      if (myB.StepFlag == 0)  //non-step barrier
      {
        if (myB.T > 1.0 / 365.0)  //more than one day to run
        {
          myB.T -= 1.0 / 365.0;
          result.Theta = myB.Price() - price;
          myB.T += 1.0 / 365.0;
        }
        else //use the intrinsic value
        {
          result.Theta = myB.IntrinsicValue() - price;
        }
      }
      else if (myB.StepFlag == 1) //early finish
      {
        if (myB.T > 1.0 / 365.0)  //more than one day to run
        {
          if (myB.t > 1.0 / 365.0)  //barrier will still be live
          {
            myB.T -= 1.0 / 365.0;
            result.Theta = myB.Price() - price;
            myB.T += 1.0 / 365.0;
          }
          else //early finish barrier ends tomorrow
          {
            long savePay = myB.PayStyle;
            myB.PayStyle = (long)MapEarlyFinish(myB);
            result.Theta = myB.Price() - price;
            myB.PayStyle = savePay;
          }
        }
        else  //the EF expires tomorrow
        {
          result.Theta = myB.IntrinsicValue() - price;
        }
      }
      else if (myB.StepFlag == 2) //late start
      {
        if (myB.T > 1.0 / 365.0)  //more than one day to run
        {
          if (myB.t > 1.0 / 365.0)  //barrier will still be live
          {
            myB.T -= 1.0 / 365.0;
            result.Theta = myB.Price() - price;
            myB.T += 1.0 / 365.0;
          }
          else //late start barrier begins
          {
            long savePay = myB.PayStyle;
            myB.PayStyle = (long)MapLateStart(myB);
            result.Theta = myB.Price() - price;
            myB.PayStyle = savePay;
          }
        }
        else  //the LS expires tomorrow
        {
          result.Theta = myB.IntrinsicValue() - price;
        }
      }
      return result;
    }

      private static OptionCode MapEarlyFinish(NabLibBlack myB)
    {
      OptionCode code = OptionCode.VEUC;
      if (myB.t < 1.0e-6)
      {
        switch (myB.PayStyle)
        {
          case (long)OptionCode.SDIC:
          case (long)OptionCode.SUIC:
          case (long)OptionCode.SUOC:
          case (long)OptionCode.SDOC:
            code = OptionCode.VEUC;
            break;

          case (long)OptionCode.SDIP:
          case (long)OptionCode.SUIP:
          case (long)OptionCode.SUOP:
          case (long)OptionCode.SDOP:
            code = OptionCode.VEUP;
            break;
        }
      }
      return code;
    }

      private static OptionCode MapLateStart(NabLibBlack myB)
    {
      OptionCode code = OptionCode.VEUC;
      if (myB.t < 1.0e-6)
      {
        switch (myB.PayStyle)
        {
          case (long)OptionCode.SDOC:
            code = OptionCode.BDOC;
            break;

          case (long)OptionCode.SUOC:
            code = OptionCode.BUOC;
            break;

          case (long)OptionCode.SDIC:
            code = OptionCode.BDIC;
            break;

          case (long)OptionCode.SUIC:
            code = OptionCode.BUIC;
            break;

          case (long)OptionCode.SDOP:
            code = OptionCode.BDOP;
            break;

          case (long)OptionCode.SUOP:
            code = OptionCode.BUOP;
            break;

          case (long)OptionCode.SDIP:
            code = OptionCode.BDIP;
            break;

          case (long)OptionCode.SUIP:
            code = OptionCode.BUIP;
            break;
        }
      }
      return code;
    }

    private static double ComputeSpotShift(NabLibBlack myB)
    {
      double shift = 0.01 * myB.Spot;

      if (myB.Lower > 1.0e-6)
        shift = Math.Min(shift, 0.9 * Math.Abs(myB.Spot - myB.Lower));

      if (myB.Upper > 1.0e-6)
        shift = Math.Min(shift, 0.9 * Math.Abs(myB.Spot - myB.Upper));

      return shift;
    }
  }
}
