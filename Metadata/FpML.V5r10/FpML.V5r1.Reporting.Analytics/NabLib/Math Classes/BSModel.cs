using System;
using System.Collections.Generic;
using nab.QR.Xml;
using nab.QR.PricingModel;

namespace nab.QR.PricingModelNab1
{
  public class BSModel
  {
    //pricing parameters
    private string Asset;
    private string Numeraire;
    private DateTime HorizonDate;
    private DateTime SpotDate;

    private double FixedAmount;
    private string FixedAsset;
    private int PositionSign;

    public PricingResult Price(Product product, Market market, PricingParams parameters)
    {
      InitProduct(product, market, parameters);

      Black blk = ToBlack(product, market, parameters);

      PricingResult results = CreateResults();
      results.ProductID = product.UniqueID;
      results.Values = new PricingValue[2];

      GreekCalculation greek = new GreekCalculation();
      greek.Asset = Asset;
      greek.Numeraire = Numeraire;
      greek.HorizonDate = HorizonDate;
      greek.SpotDate = SpotDate;
      greek.FixedAmount = FixedAmount;
      greek.FixedAsset = FixedAsset;
      greek.PositionSign = PositionSign;
      greek.Price = blk.Price();

      results.Values[0] = greek.CreateNumerairePricePoints();
      results.Values[1] = greek.CreateNumeraireValueAbsolute();

      return results;
    }

    public PricingResult CreateResults()
    {
      PricingResult results = new PricingResult();
      results.ValueDate = SpotDate;
      return results;
    }

    private void InitProduct(Product product, Market market, PricingParams parameters)
    {
      Asset = parameters.Values.GetValue<string>("LeftAsset");
      Numeraire = parameters.Values.GetValue<string>("RightAsset");
      HorizonDate = parameters.HorizonDate;

      FixedAmount = product.Exchange.GetFixedAmount();
      FixedAsset = product.Exchange.GetFixedAsset();
      PositionSign = product.Position.PositionType.Sign();
    }

    private Black ToBlack(Product product, Market market, PricingParams parameters)
    {
      HorizonDate = parameters.HorizonDate;
      Asset = parameters.Values.GetValue<string>("LeftAsset");
      Numeraire = parameters.Values.GetValue<string>("RightAsset");

      Xml.Curve fwdCurve = market["FXFWD", Asset, Numeraire];
      Xml.Rate spotRate = fwdCurve.Rates.Find("SPOT");
      SpotDate = spotRate.Date;

      Black blk = new Black();
      blk.PayStyle = (long)MapPayStyle(product);

      PropertyNode propParameters = new PropertyNode();
      string asset = parameters.Values.GetValue<string>("LeftAsset");
      string leftAsset = parameters.Values.GetValue<string>("LeftAsset");
      string rightAsset = parameters.Values.GetValue<string>("RightAsset");
      Xml.Curve frnCurve = market["FXDF", leftAsset, leftAsset];
      Xml.Curve domCurve = market["FXDF", rightAsset, rightAsset];

      blk.Spot = ExchangeRate.Flip(spotRate.Mid, fwdCurve.LeftAsset, Asset);
      blk.Spot += parameters.Values.GetValue<double>("SpotShift");

      if (product.Option != null)
      {
        blk.K = product.Option.Strike.Rate;

        Xml.Curve volCurve = market["FXVOL", leftAsset, rightAsset];

        DateTime expiryDate = product.Option.ExpiryDate;
        blk.T = (expiryDate - HorizonDate).Days / 365.0;

        double compression = 1.0;
        if (expiryDate > HorizonDate) compression = 1.0 / ((expiryDate - HorizonDate).Days / 365.0);
        blk.ForRate = -Math.Log(CurveInterpolationDF.GetFwdDF(frnCurve, SpotDate, product.Settlement.SettlementDate).Mid) * compression;
        blk.DomRate = -Math.Log(CurveInterpolationDF.GetFwdDF(domCurve, SpotDate, product.Settlement.SettlementDate).Mid) * compression;
        blk.Fwd = blk.Spot * Math.Exp((blk.DomRate - blk.ForRate) * blk.T);

        propParameters.SetValue("strike", 25.0);
        double rr25 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, propParameters).Mid;
        propParameters.SetValue("strike", -25.0);
        double fly25 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, propParameters).Mid;
        propParameters.SetValue("strike", 10.0);
        double rr10 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, propParameters).Mid;
        propParameters.SetValue("strike", -10.0);
        double fly10 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, propParameters).Mid;
        propParameters.SetValue("strike", 0.0);
        double atmVol = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, propParameters).Mid;
        atmVol += parameters.Values.GetValue<double>("VolatilityShift");

        double[] deltaVols = new double[5];
        bool isCall = product.Option.Type == OptionType.Call;

        // Determine whether the surface has to be flipped
//        if (!isCall)
//        {
//          rr10 = -rr10;
//          rr25 = -rr25;
//        }

        deltaVols[0] = atmVol + fly10 - 0.5 * rr10;
        deltaVols[1] = atmVol + fly25 - 0.5 * rr25;
        deltaVols[2] = atmVol;
        deltaVols[3] = atmVol + fly25 + 0.5 * rr25;
        deltaVols[4] = atmVol + fly10 + 0.5 * rr10;

        // delta type
        int deltaType = blk.T <= 1 ? (int)volCurve.Parameters.GetValue<double>("FwdDeltaTypeNormal")
                                   : (int)volCurve.Parameters.GetValue<double>("FwdDeltaTypeMedium");

        double deltaOrStrike = blk.K;
        double numeraireDf = CurveInterpolationDF.GetFwdDF(domCurve, SpotDate, expiryDate).Mid;
        double assetDf = CurveInterpolationDF.GetFwdDF(frnCurve, SpotDate, expiryDate).Mid;
        double[] deltaArray = { 0.1, 0.25, 0, 0.25, 0.1 };
        int[] deltaPutCallFlags = { 1, 1, 0, -1, -1 };
        int typeSupplied = 0;  // set to zero for delta, 1 for strike

        blk.Sig = SmileCalculation.CalculateVol(deltaType, isCall ? 1 : -1, blk.Fwd, blk.K,
                                               (expiryDate - HorizonDate).Days, atmVol, numeraireDf,
                                                assetDf, deltaArray, deltaPutCallFlags, deltaVols,
                                                typeSupplied);
      }
      else
      {
        // FXSP & FRWD
        blk.K = product.Exchange.Rate.Rate;
        blk.T = (product.Settlement.SettlementDate - SpotDate).Days;
        blk.ForRate = -Math.Log(CurveInterpolationDF.GetFwdDF(frnCurve, SpotDate, product.Settlement.SettlementDate).Mid);
        blk.DomRate = -Math.Log(CurveInterpolationDF.GetFwdDF(domCurve, SpotDate, product.Settlement.SettlementDate).Mid);
        blk.Fwd = blk.Spot * Math.Exp((blk.DomRate - blk.ForRate) * blk.T);
      }

      return blk;
    }

    private static OptionCode MapPayStyle(Product product)
    {
      OptionCode result =  OptionCode.ZZZZ;
      if (product.Type.CompareNoCase("VEUC") == 0)
        result = OptionCode.VEUC;
      else if (product.Type.CompareNoCase("VEUP") == 0)
        result = OptionCode.VEUP;
      else if (product.Type.CompareNoCase("CPCC") == 0)
        result = OptionCode.CPCC;
      else if (product.Type.CompareNoCase("CPCP") == 0)
        result = OptionCode.CPCP;
      else if (product.Type.CompareNoCase("FXSP") == 0)
        result = OptionCode.FXSP;
      else if (product.Type.CompareNoCase("FRWD") == 0)
        result = OptionCode.FRWD;
      return result;
    }

    public static bool IsBlackProduct(Product product)
    {
      return product.Type.CompareNoCase("VEUC") == 0
          || product.Type.CompareNoCase("VEUP") == 0
          || product.Type.CompareNoCase("CPCC") == 0
          || product.Type.CompareNoCase("CPCP") == 0
          || product.Type.CompareNoCase("FXSP") == 0
          || product.Type.CompareNoCase("FRWD") == 0;
    }
  }
}
