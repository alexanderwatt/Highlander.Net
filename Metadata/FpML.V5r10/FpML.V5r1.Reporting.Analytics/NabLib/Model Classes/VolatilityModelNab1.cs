using System;
using System.Collections.Generic;
using nab.QR.PricingModel;
using nab.QR.Xml;
using System.Linq;

namespace nab.QR.PricingModelNab1
{
  public class VolatilityModelNab1 : IVolatilityModel 
  {
    public Spread CalculateVolatility(Product product, Market market, PricingParams parameters)
    {
      OptionProduct option = new OptionProduct();
      option.Init(product, market, parameters);
      Spread result = new Spread();

      if (option.SpreadPricing)
      {
        PropertyNode volParams = new PropertyNode();
        volParams.SetValue("strike", VolatilityStrikeMappings.Map("ATM"));
        Xml.Curve volCurve = market["FXVOL", option.LeftAsset, option.RightAsset];
        Xml.Curve fwdCurve = market["FXFWD", option.LeftAsset, option.RightAsset];
        Spread spotRates = fwdCurve.Rates.Find("SPOT").Spread;
        Spread spread = CurveInterpolationLinearVariance.InterpolateCurve(volCurve, option.ExpiryDate, volParams);
        double interpVol = VanillaExpiryVolatility.Calculate(option, market, PositionType.None);
        double atmSpread = (spread.Ask - spread.Bid) * 0.5;

        double vega = BlackScholes.VanillaVega(option.LeftAsset, option.RightAsset, option.ExpiryDate, PositionType.None,
                                               option.SpotDate, option.SettlementDate, option.HorizonDate,
                                               market, option.Rate, interpVol);
        vega /= spotRates[PositionType.None];
        double vSpread = vega == 0.0 ? atmSpread : atmSpread / vega;

        PositionType position = option.PositionType;

        result[position] = interpVol - vSpread * position.Sign();
        result[position.Opposite()] = interpVol + vSpread * position.Sign();

        if (result[position] < 0)
          result[position] = 0;

        if (result[position.Opposite()] < 0)
          result[position.Opposite()] = 0.0;
      }
      else
      {
        result[PositionType.None] = VanillaExpiryVolatility.Calculate(option, market, PositionType.None);
      }
      return result;
    }

    public void CalibrateSurface(Market market, PricingParams parameters, IProgressSink sink)
    {
      OptionMarket om = new OptionMarket();
      om.InitParams(market, parameters);
      om.InitTermParams(market, parameters);
      om.InitTermRates(market, parameters);
      CalibrateSurface(om, sink, double.MaxValue);
      SaveCalibration(om, market); 
    }

    internal void SaveCalibration(OptionMarket om, Market market)
    {
      Xml.Curve volCurve = market["FXVOL_SMILE", om.LeftAsset, om.RightAsset];

      double p1s = VolatilityStrikeMappings.Map("P1");
      double p2s = VolatilityStrikeMappings.Map("P2");
      double p3s = VolatilityStrikeMappings.Map("P3");
      double p4s = VolatilityStrikeMappings.Map("P4");
      double p5s = VolatilityStrikeMappings.Map("P5");
      double p6s = VolatilityStrikeMappings.Map("P6");
      double mrs = VolatilityStrikeMappings.Map("MR");

      double f0s = VolatilityStrikeMappings.Map("F0");
      double f1s = VolatilityStrikeMappings.Map("F1");
      double f2s = VolatilityStrikeMappings.Map("F2");
      double f3s = VolatilityStrikeMappings.Map("F3");
      double f4s = VolatilityStrikeMappings.Map("F4");

      int length = om.Tenors.Length;
      for (int idx = 0; idx < length; idx++)
      {
        DateTime expiry = om.Expirys[idx];
        volCurve.Rates.Find(expiry, p1s).Mid = om.P1[idx];
        volCurve.Rates.Find(expiry, p2s).Mid = om.P2[idx];
        volCurve.Rates.Find(expiry, p3s).Mid = om.P3[idx];
        volCurve.Rates.Find(expiry, p4s).Mid = om.P4[idx];
        volCurve.Rates.Find(expiry, p5s).Mid = om.P5[idx];
        volCurve.Rates.Find(expiry, p6s).Mid = om.P6[idx];
        volCurve.Rates.Find(expiry, mrs).Mid = om.MR[idx];

        volCurve.Rates.Find(expiry, f0s).Mid = om.FV0[idx];
        volCurve.Rates.Find(expiry, f1s).Mid = om.FV1[idx];
        volCurve.Rates.Find(expiry, f2s).Mid = om.FV2[idx];
        volCurve.Rates.Find(expiry, f3s).Mid = om.FV3[idx];
        volCurve.Rates.Find(expiry, f4s).Mid = om.FV4[idx];
      }
    }

    internal void CalibrateSurface(OptionMarket om, IProgressSink sink, double targetTenor)
    {
      // re-use coefficient arrays
      Pricer pricer = new Pricer();
      if (om.P1.Length > 0 && om.P1[0] != 0)
      {
        pricer.a0 = om.P1;
        pricer.a1 = om.P2;
        pricer.a2 = om.P3;
        pricer.a3 = om.P4;
        pricer.a4 = om.P5;
        pricer.a5 = om.P6;
        pricer.mr = om.MR;
      }

      // calibrate
      if (!pricer.Calibrate(om.Tenors, om.ATMs, om.Doms, om.Fors, om.RR25s, om.FLY25s, om.RR10s, om.FLY10s,
                            om.Speeds, om.Spot, om.IsLeftDelta, om.IsStochastic, targetTenor, sink))
      {
        // try non-stochastic
        if (om.IsStochastic && !pricer.Calibrate(om.Tenors, om.ATMs, om.Doms, om.Fors, om.RR25s, om.FLY25s, om.RR10s, om.FLY10s,
                                                 om.Speeds, om.Spot, om.IsLeftDelta, false, targetTenor, sink))
          throw new Exception(Resource.ErrVolCalibrationFailed);
      }

      om.P1 = pricer.a0;
      om.P2 = pricer.a1;
      om.P3 = pricer.a2;
      om.P4 = pricer.a3;
      om.P5 = pricer.a4;
      om.P6 = pricer.a5;
      om.MR = pricer.mr;

      om.FV0 = pricer.fv0;
      om.FV1 = pricer.fv1;
      om.FV2 = pricer.fv2;
      om.FV3 = pricer.fv3;
      om.FV4 = pricer.fv4;
    }
  }
}
