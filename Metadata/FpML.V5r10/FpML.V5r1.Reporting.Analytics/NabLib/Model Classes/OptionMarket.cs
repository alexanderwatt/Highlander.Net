using System;
using nab.QR.Xml;
using System.Collections.Generic;
using nab.QR.PricingModel;

namespace nab.QR.PricingModelNab1
{
  internal class OptionMarket
  {
    // Pricing parameters
    public DateTime HorizonDate;
    public DateTime SpotDate;
    public string LeftAsset;
    public string RightAsset;
    public double VolatilityShift;
    public double SpotShift;
    public double Spot;
    public double SwapFactor;
    public int LeftDayCount;
    public int RightDayCount;
    public bool IsBlackModel;

    // Exotic parameters
    public int GridStep;
    public double TimeStep;
    public bool IsLeftDelta;
    public bool IsStochastic;
    public double DeltaCutTime;
    public bool IsBlackGreeks;
    public double LastCalibratedTenor;

    // Term strucute
    public DateTime[] Expirys;          // expiry date
    public double[] Tenors;             // expiry tenors
    public double[] Fors;               // foreign rates
    public double[] Doms;               // domestic rates
    public double[] RR25s;              // RR 25
    public double[] FLY25s;             // FLY 25
    public double[] RR10s;              // RR 10
    public double[] FLY10s;             // FLY 10
    public double[] ATMs;               // ATM Vol
    public double[] Speeds;             // Speed
    public double[] P1;                 // coefficient 1
    public double[] P2;                 // ...
    public double[] P3;
    public double[] P4;
    public double[] P5;
    public double[] P6;
    public double[] MR;

    public double[] K10c;
    public double[] K25c;
    public double[] K25p;
    public double[] K10p;
    public double[] KATM;

    public double[] FV0;
    public double[] FV1;
    public double[] FV2;
    public double[] FV3;
    public double[] FV4;

    public void InitParams(Market market, PricingParams parameters)
    {
      HorizonDate = parameters.HorizonDate;
      VolatilityShift = parameters.Values.GetValue<double>("VolatilityShift");
      SpotShift = parameters.Values.GetValue<double>("SpotShift");
      LeftAsset = parameters.Values.GetValue<string>("LeftAsset");
      RightAsset = parameters.Values.GetValue<string>("RightAsset");
      Xml.Curve fwdCurve = market["FXFWD", LeftAsset, RightAsset];
      Xml.Rate spotRate = fwdCurve.Rates.Find("SPOT");
      Spot = ExchangeRate.Flip(spotRate.Mid, fwdCurve.LeftAsset, LeftAsset);
      SpotDate = spotRate.Date;
      SwapFactor = fwdCurve.Parameters.GetValue<double>("SwapFactor");

      Xml.Curve dfCurve = market["FXDF", LeftAsset, LeftAsset];
      LeftDayCount = (int) dfCurve.Parameters.GetValue<double>("DayCount");

      dfCurve = market["FXDF", RightAsset, RightAsset];
      RightDayCount = (int) dfCurve.Parameters.GetValue<double>("DayCount");

      IsBlackModel = parameters.Values.GetValue<bool>("BlackModel");
    }

    public bool IsTermChanged(Market market, PricingParams parameters)
    {
      string leftAsset = parameters.Values.GetValue<string>("LeftAsset");
      string rightAsset = parameters.Values.GetValue<string>("RightAsset");

      Xml.Curve volCurve = market["FXVOL_SMILE", LeftAsset, RightAsset];
      DateTime[] expirys = volCurve == null ? null : volCurve.GetTenorDates().ToArray();

      return this.HorizonDate != parameters.HorizonDate
          || this.LeftAsset != leftAsset
          || this.RightAsset != rightAsset
          || this.Expirys == null || expirys == null
          || this.Expirys.Length != expirys.Length;
    }

    public void InitTermParams(Market market, PricingParams parameters)
    {
      GridStep = parameters.Values.GetValue<int>("GridStep");
      TimeStep = parameters.Values.GetValue<double>("TimeStep");
      IsLeftDelta = parameters.Values.GetValue<bool>("LeftDelta");
      IsStochastic = parameters.Values.GetValue<bool>("Stochastic");
      DeltaCutTime = parameters.Values.GetValue<double>("DeltaCutTime");
      IsBlackGreeks = parameters.Values.GetValue<bool>("BlackGreeks");
    }

    public void InitTermRates(Market market, PricingParams parameters)
    {
      Xml.Curve volCurve = market["FXVOL_SMILE", LeftAsset, RightAsset];
      Xml.Curve frnCurve = market["FXDF", LeftAsset, LeftAsset];
      Xml.Curve domCurve = market["FXDF", RightAsset, RightAsset];

      Expirys = volCurve.GetTenorDates().ToArray();
      int length = Expirys.Length;

      Tenors = new double[length];
      Fors = new double[length];
      Doms = new double[length];
      RR25s = new double[length];
      FLY25s = new double[length];
      RR10s = new double[length];
      FLY10s = new double[length];
      ATMs = new double[length];
      Speeds = new double[length];
      P1 = new double[length];
      P2 = new double[length];
      P3 = new double[length];
      P4 = new double[length];
      P5 = new double[length];
      P6 = new double[length];
      MR = new double[length];

      K10p = new double[length];
      K25p = new double[length];
      K10c = new double[length];
      K25c = new double[length];
      KATM = new double[length];

      double[] impVols = new double[5];

      double atmStrike = VolatilityStrikeMappings.Map("ATM");
      double rr25Strike = VolatilityStrikeMappings.Map("RR25");
      double rr10Strike = VolatilityStrikeMappings.Map("RR10");
      double fly25Strike = VolatilityStrikeMappings.Map("FLY25");
      double fly10Strike = VolatilityStrikeMappings.Map("FLY10");
      double p1Strike = VolatilityStrikeMappings.Map("P1");
      double p2Strike = VolatilityStrikeMappings.Map("P2");
      double p3Strike = VolatilityStrikeMappings.Map("P3");
      double p4Strike = VolatilityStrikeMappings.Map("P4");
      double p5Strike = VolatilityStrikeMappings.Map("P5");
      double p6Strike = VolatilityStrikeMappings.Map("P6");
      double mrStrike = VolatilityStrikeMappings.Map("MR");
      double speedStrike = VolatilityStrikeMappings.Map("SPEED");

      for (int idx = 0; idx < length; idx++)
      {
        DateTime expiry = Expirys[idx];
        Tenors[idx] = (expiry - HorizonDate).Days / 365.0;

        DateTime maturity = volCurve.Parameters["Maturitys"].GetValue<DateTime>(volCurve.Rates.Find(expiry, 0).Tenor);

        double compression = 1 / Tenors[idx];
        Fors[idx] = -Math.Log(CurveInterpolationDF.GetFwdDF(frnCurve, SpotDate, maturity).Mid) * compression;
        Doms[idx] = -Math.Log(CurveInterpolationDF.GetFwdDF(domCurve, SpotDate, maturity).Mid) * compression;

        ATMs[idx] = volCurve.Rates.Find(expiry, atmStrike).Mid;
        RR25s[idx] = volCurve.Rates.Find(expiry, rr25Strike).Mid;
        FLY25s[idx] = volCurve.Rates.Find(expiry, fly25Strike).Mid;
        RR10s[idx] = volCurve.Rates.Find(expiry, rr10Strike).Mid;
        FLY10s[idx] = volCurve.Rates.Find(expiry, fly10Strike).Mid;
        Speeds[idx] = volCurve.Rates.Find(expiry, speedStrike).Mid;
        P1[idx] = volCurve.Rates.Find(expiry, p1Strike).Mid;
        P2[idx] = volCurve.Rates.Find(expiry, p2Strike).Mid;
        P3[idx] = volCurve.Rates.Find(expiry, p3Strike).Mid;
        P4[idx] = volCurve.Rates.Find(expiry, p4Strike).Mid;
        P5[idx] = volCurve.Rates.Find(expiry, p5Strike).Mid;
        P6[idx] = volCurve.Rates.Find(expiry, p6Strike).Mid;
        MR[idx] = volCurve.Rates.Find(expiry, mrStrike).Mid;

        impVols[0] = ATMs[idx] + FLY10s[idx] - 0.5 * RR10s[idx];
        impVols[1] = ATMs[idx] + FLY25s[idx] - 0.5 * RR25s[idx];
        impVols[2] = ATMs[idx];
        impVols[3] = ATMs[idx] + FLY25s[idx] + 0.5 * RR25s[idx];
        impVols[4] = ATMs[idx] + FLY10s[idx] + 0.5 * RR10s[idx];

        K10p[idx] = Utilities.ComputeStrike(Tenors[idx], Doms[idx], Fors[idx], Spot, impVols[0], -0.10, DeltaCutTime, IsLeftDelta);
        K25p[idx] = Utilities.ComputeStrike(Tenors[idx], Doms[idx], Fors[idx], Spot, impVols[1], -0.25, DeltaCutTime, IsLeftDelta);
        K25c[idx] = Utilities.ComputeStrike(Tenors[idx], Doms[idx], Fors[idx], Spot, impVols[3], 0.25, DeltaCutTime, IsLeftDelta);
        K10c[idx] = Utilities.ComputeStrike(Tenors[idx], Doms[idx], Fors[idx], Spot, impVols[4], 0.10, DeltaCutTime, IsLeftDelta);
        KATM[idx] = Utilities.ComputeATMStrike(Tenors[idx], Doms[idx], Fors[idx], Spot, impVols[2], IsLeftDelta);
      }

      LastCalibratedTenor = 0;
    }

    public void Publish(PricingResult result)
    {
      result.Add(new PricingValue { Asset = LeftAsset, Value = new PropertyValue(SwapFactor), Name = "SwapFactor", Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = LeftAsset, Value = new PropertyValue(LeftDayCount), Name = "DayCount", Unit = ValueUnit.Absolute });
      result.Add(new PricingValue { Asset = RightAsset, Value = new PropertyValue(RightDayCount), Name = "DayCount", Unit = ValueUnit.Absolute });
    }
  }
}
