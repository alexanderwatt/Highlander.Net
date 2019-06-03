using System;
using nab.QR.Xml;
using nab.QR.PricingModel;

namespace nab.QR.PricingModelNab1
{
  internal static class VanillaExpiryVolatility
  {
    internal static double Calculate(OptionProduct product, Market market, PositionType positionType)
    {
      Xml.Curve frnCurve = market["FXDF", product.LeftAsset, product.LeftAsset];
      Xml.Curve domCurve = market["FXDF", product.RightAsset, product.RightAsset];
      Xml.Curve volCurve = market["FXVOL", product.LeftAsset, product.RightAsset];

      DateTime expiryDate = product.ExpiryDate;
      DateTime horizonDate = product.HorizonDate;
      DateTime spotDate = product.SpotDate;
      DateTime settlementDate = product.SettlementDate;

      int settleDays = (settlementDate - spotDate).Days;
      double r = CurveInterpolationDF.GetFwdCC(domCurve, spotDate, settlementDate)[positionType];
      double q = CurveInterpolationDF.GetFwdCC(frnCurve, spotDate, settlementDate)[positionType.Opposite()];
      double s = product.MarketSpot[positionType];
      double fwd  = s * Math.Exp((r - q) * settleDays / 365.0); 

      double atmVol = 0;
      double[] deltaVols = new double[5];

      PropertyNode volParams = new PropertyNode();
      volParams.SetValue("strike", VolatilityStrikeMappings.Map("RR25"));
      double rr25 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, volParams)[positionType];
      volParams.SetValue("strike", VolatilityStrikeMappings.Map("FLY25"));
      double fly25 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, volParams)[positionType];
      volParams.SetValue("strike", VolatilityStrikeMappings.Map("RR10"));
      double rr10 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, volParams)[positionType];
      volParams.SetValue("strike", VolatilityStrikeMappings.Map("FLY10"));
      double fly10 = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, volParams)[positionType];
      volParams.SetValue("strike", VolatilityStrikeMappings.Map("ATM"));
      atmVol = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, expiryDate, volParams)[positionType];

      deltaVols[0] = atmVol + fly10 + 0.5 * rr10; // 10call
      deltaVols[1] = atmVol + fly25 + 0.5 * rr25; // 25call
      deltaVols[2] = atmVol;
      deltaVols[3] = atmVol + fly25 - 0.5 * rr25; // 25put
      deltaVols[4] = atmVol + fly10 - 0.5 * rr10; // 10put

      double[] deltaArray = { 0.1, 0.25, 0, 0.25, 0.1 };
      int[] deltaPutCallFlags = { 1, 1, 0, -1, -1 };
      int strikeType = 0;  // zero for delta, 1 for strike

      int expiryDays = (expiryDate - horizonDate).Days;
      int deltaType = expiryDays <= 365 ? (int)volCurve.Parameters.GetValue<double>("FwdDeltaTypeNormal")
                                        : (int)volCurve.Parameters.GetValue<double>("FwdDeltaTypeMedium");

      int call = product.OptionType == OptionType.Call ? 1 : -1;
      double strike = product.Rate;

      double numeraireDf = CurveInterpolationDF.GetFwdDF(domCurve, spotDate, settlementDate)[positionType];
      double assetDf = CurveInterpolationDF.GetFwdDF(frnCurve, spotDate, settlementDate)[positionType.Opposite()];

      return SmileCalculation.CalculateVol(deltaType, call, fwd, strike, expiryDays, atmVol, numeraireDf,
                                           assetDf, deltaArray, deltaPutCallFlags, deltaVols, strikeType);

    }
  }
}
