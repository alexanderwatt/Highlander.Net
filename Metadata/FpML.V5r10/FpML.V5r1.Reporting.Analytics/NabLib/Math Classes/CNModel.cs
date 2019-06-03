using System;
using System.Collections.Generic;
using nab.QR.Xml;
using nab.QR.PricingModel;

namespace nab.QR.PricingModelNab1
{
  public class CNModel
  {
    //pricing parameters
    private double spot;
    private int gridstep;
    private double timestep;
    private string asset;
    private string numeraire;
    private DateTime horizonDate;
    private DateTime spotDate;

    //contract values
    private double ub;                   //upper barrier
    private double lb;                   //lower barrier
    private bool bVan;                   //true if vanilla type payoff
    private bool bDAE;                   //true for digital on expiry
    private bool bCash;                  //for digitals, cash or asset flag
    private bool bKI;                    //for a KI/KO barrier or OT/NT digital
    private bool bCall;                  //true for a call payoff, false for a put
    private double T;                    //time to Expiry
    private double tb;                   //time to Barrier Start
    private double te;                   //time to Barrier end
    private double K;                    //strike
    private double fixedAmount;
    private string fixedAsset;
    private int positionSign;

    // market rates
    private DateTime[] expirys;          // expiry dates
    private double[] tenors;             // expiry tenors
    private double[] fors;               // foreign rates
    private double[] doms;               // domestic rates
    private double[] rr25s;              // RR 25
    private double[] fly25s;             // FLY 25
    private double[] rr10s;              // RR 10
    private double[] fly10s;             // FLY 10
    private double[] atmvols;            // ATM Vol
    private double[] speeds;             // Speed
    private double[] p1;                 // coefficient 1
    private double[] p2;                 // ...
    private double[] p3;
    private double[] p4;
    private double[] p5;
    private double[] p6;
    private double[] mr;

    private double[] k10c;
    private double[] k25c;
    private double[] k25p;
    private double[] k10p;
    private double[] katm;

    public PricingResult Price(Product product, Market market, PricingParams parameters)
    {
      InitRates(market, parameters);
      InitProduct(product, market, parameters);

      spot += parameters.Values.GetValue<double>("SpotShift");
      atmvols.Add(parameters.Values.GetValue<double>("VolatilityShift"));

      CN cn = ToCN();
      Pricer model = new Pricer();

      PricingResult results = CreateResults();
      results.ProductID = product.UniqueID;
      results.Values = new PricingValue[2];

      GreekCalculation greek = new GreekCalculation();
      greek.Asset = asset;
      greek.Numeraire = numeraire;
      greek.HorizonDate = horizonDate;
      greek.SpotDate = spotDate;
      greek.FixedAmount = fixedAmount;
      greek.FixedAsset = fixedAsset;
      greek.PositionSign = positionSign;
      greek.Price = model.CNBarrier(cn, tenors, atmvols, doms, fors, p1, p2, p3, p4, p5, p6, mr);

      results.Values[0] = greek.CreateNumerairePricePoints();
      results.Values[1] = greek.CreateNumeraireValueAbsolute();

      return results;
    }

    public PricingResult CreateResults()
    {
      PricingResult results = new PricingResult();
      results.ValueDate = spotDate;
      return results;
    }

    public void Calibrate(Market market, PricingParams parameters, IProgressSink sink)
    {
      // init market rates
      InitRates(market, parameters);

      // re-use coefficient arrays
      Pricer pricer = new Pricer();
      if (p1[0] != 0)
      {
        pricer.a0 = p1;
        pricer.a1 = p2;
        pricer.a2 = p3;
        pricer.a3 = p4;
        pricer.a4 = p5;
        pricer.a5 = p6;
        pricer.mr = mr;
      }

      // calibrate
      bool leftDelta = parameters.Values.GetValue<bool>("LeftDelta");
      bool Stochastic = parameters.Values.GetValue<bool>("Stochastic");
      if (!pricer.Calibrate(tenors, atmvols, doms, fors, rr25s, fly25s,
                            rr10s, fly10s, speeds, spot, leftDelta, Stochastic, sink))
      {
        if (Stochastic && !pricer.Calibrate(tenors, atmvols, doms, fors, rr25s, fly25s,
                            rr10s, fly10s, speeds, spot, leftDelta, false, sink))
          throw new Exception(Resource.ErrVolCalibrationFailed);
      }

      // save coefficient arrays
      string leftAsset = parameters.Values.GetValue<string>("LeftAsset");
      string rightAsset = parameters.Values.GetValue<string>("RightAsset");
      Xml.Curve volCurve = market["FXVOL_SMILE", leftAsset, rightAsset];

      int length = tenors.Length;
      for (int idx = 0; idx < length; idx++)
      {
        DateTime expiry = expirys[idx];
        volCurve.Rates.Find(expiry, 101).Mid = pricer.a0[idx];
        volCurve.Rates.Find(expiry, 102).Mid = pricer.a1[idx];
        volCurve.Rates.Find(expiry, 103).Mid = pricer.a2[idx];
        volCurve.Rates.Find(expiry, 104).Mid = pricer.a3[idx];
        volCurve.Rates.Find(expiry, 105).Mid = pricer.a4[idx];
        volCurve.Rates.Find(expiry, 106).Mid = pricer.a5[idx];
        volCurve.Rates.Find(expiry, 107).Mid = pricer.mr[idx];

        volCurve.Rates.Find(expiry, 201).Mid = pricer.fv0[idx];
        volCurve.Rates.Find(expiry, 202).Mid = pricer.fv1[idx];
        volCurve.Rates.Find(expiry, 203).Mid = pricer.fv2[idx];
        volCurve.Rates.Find(expiry, 204).Mid = pricer.fv3[idx];
        volCurve.Rates.Find(expiry, 205).Mid = pricer.fv4[idx];
      }
    }

    private void InitProduct(Product product, Market market, PricingParams parameters)
    {
      asset = parameters.Values.GetValue<string>("LeftAsset");
      numeraire = parameters.Values.GetValue<string>("RightAsset");
      horizonDate = parameters.HorizonDate;
      positionSign = product.Position.PositionType.Sign();


      // vanilla or digital
      if (product.Exchange != null)
      {
        bVan = true;
        bCash = false;
        fixedAmount = product.Exchange.GetFixedAmount();
        fixedAsset = product.Exchange.GetFixedAsset();
      }
      else if(product.Cash != null)
      {
        bVan = false;
        bCash = product.Cash.Asset == numeraire;
        fixedAmount = product.Cash.Amount;
        fixedAsset = product.Cash.Asset;

        // at expiry
        bDAE = !bVan && product.Option.Type != OptionType.None;
      }

      // expiry and strike
      T = 0;
      K = 0;

      bCall = product.Option.Type == OptionType.Call;
      T = (product.Option.ExpiryDate - horizonDate).Days / 365.0;
      K = ExchangeRate.Flip(product.Option.Strike.Rate, product.Option.Strike.LeftAsset, asset);

      bKI = false;
      ub = 0;
      lb = 0;
      tb = 0;
      te = 0;
      if (product.Triggers != null && product.Triggers.Length == 1)
      {
        ProductTrigger trigger = product.Triggers[0];
        bKI = trigger.Type == TriggerType.KI;
        tb = (trigger.StartDate - horizonDate).Days / 365.0;
        te = (product.Option.ExpiryDate - horizonDate).Days / 365.0;
        ub = trigger.UpperLevel.Rate;
        lb = trigger.LowerLevel.Rate;
      }
    }

    private void InitRates(Market market, PricingParams parameters)
    {
      horizonDate = parameters.HorizonDate;

      gridstep = parameters.Values.GetValue<int>("GridStep");
      timestep = parameters.Values.GetValue<double>("TimeStep");

      string leftAsset = parameters.Values.GetValue<string>("LeftAsset");
      string rightAsset = parameters.Values.GetValue<string>("RightAsset");
      bool leftDelta = parameters.Values.GetValue<bool>("LeftDelta");
      double deltaCutTime = parameters.Values.GetValue<double>("DeltaCutTime");

      Xml.Curve volCurve = market["FXVOL_SMILE", leftAsset, rightAsset];
      Xml.Curve frnCurve = market["FXDF", leftAsset, leftAsset];
      Xml.Curve domCurve = market["FXDF", rightAsset, rightAsset];
      Xml.Curve fwdCurve = market["FXFWD", leftAsset, rightAsset];

      Xml.Rate spotRate = fwdCurve.Rates.Find("SPOT");

      spot = ExchangeRate.Flip(spotRate.Mid, fwdCurve.LeftAsset, leftAsset);
      spotDate = spotRate.Date;

      expirys = volCurve.GetTenorDates().ToArray();
      int length = expirys.Length;

      tenors = new double[length];
      fors = new double[length];
      doms = new double[length];
      rr25s = new double[length];
      fly25s = new double[length];
      rr10s = new double[length];
      fly10s = new double[length];
      atmvols = new double[length];
      speeds = new double[length];
      p1 = new double[length];
      p2 = new double[length];
      p3 = new double[length];
      p4 = new double[length];
      p5 = new double[length];
      p6 = new double[length];
      mr = new double[length];

      k10p = new double[length];
      k25p = new double[length];
      k10c = new double[length];
      k25c = new double[length];
      katm = new double[length];

      double[] impVols = new double[5];

      List<string> expiryTenors = volCurve.GetTenors();
      for (int idx = 0; idx < length; idx++)
      {
        DateTime expiry = expirys[idx];
        tenors[idx] = (expiry - horizonDate).Days / 365.0;

        DateTime maturity = expiry.AddDays(2); //TODO

        double compression = 1 / tenors[idx];
        fors[idx] = -Math.Log(CurveInterpolationDF.GetFwdDF(frnCurve, spotRate.Date, maturity).Mid) * compression;
        doms[idx] = -Math.Log(CurveInterpolationDF.GetFwdDF(domCurve, spotRate.Date, maturity).Mid) * compression;

        atmvols[idx] = volCurve.Rates.Find(expiry, 0).Mid;
        rr25s[idx] = volCurve.Rates.Find(expiry, 25).Mid;
        fly25s[idx] = volCurve.Rates.Find(expiry, -25).Mid;
        rr10s[idx] = volCurve.Rates.Find(expiry, 10).Mid;
        fly10s[idx] = volCurve.Rates.Find(expiry, -10).Mid;
        speeds[idx] = volCurve.Rates.Find(expiry, 301).Mid;
        p1[idx] = volCurve.Rates.Find(expiry, 101).Mid;
        p2[idx] = volCurve.Rates.Find(expiry, 102).Mid;
        p3[idx] = volCurve.Rates.Find(expiry, 103).Mid;
        p4[idx] = volCurve.Rates.Find(expiry, 104).Mid;
        p5[idx] = volCurve.Rates.Find(expiry, 105).Mid;
        p6[idx] = volCurve.Rates.Find(expiry, 106).Mid;
        mr[idx] = volCurve.Rates.Find(expiry, 107).Mid;

        impVols[0] = atmvols[idx] + fly10s[idx] - 0.5 * rr10s[idx];
        impVols[1] = atmvols[idx] + fly25s[idx] - 0.5 * rr25s[idx];
        impVols[2] = atmvols[idx];
        impVols[3] = atmvols[idx] + fly25s[idx] + 0.5 * rr25s[idx];
        impVols[4] = atmvols[idx] + fly10s[idx] + 0.5 * rr10s[idx];

        k10p[idx] = Utilities.ComputeStrike(tenors[idx], doms[idx], fors[idx], spot, impVols[0], -0.10, deltaCutTime, leftDelta);
        k25p[idx] = Utilities.ComputeStrike(tenors[idx], doms[idx], fors[idx], spot, impVols[1], -0.25, deltaCutTime, leftDelta);
        k25c[idx] = Utilities.ComputeStrike(tenors[idx], doms[idx], fors[idx], spot, impVols[3], 0.25, deltaCutTime, leftDelta);
        k10c[idx] = Utilities.ComputeStrike(tenors[idx], doms[idx], fors[idx], spot, impVols[4], 0.10, deltaCutTime, leftDelta);
        katm[idx] = Utilities.ComputeATMStrike(tenors[idx], doms[idx], fors[idx], spot, impVols[2], leftDelta);
      }
    }

    private CN ToCN()
    {
      CN cn = new CN();

      bool bLS = tb != 0;

      cn.T = T;
      cn.t = tb != 0 ? tb : (te < T ? T - te : 0);

      double u, l;
      ComputeGridWidth(T, out u, out l);
      cn.LowerVanLogAsset2 = Math.Log(l);
      cn.UpperVanLogAsset2 = Math.Log(u);
      cn.LowerLogAsset2 = cn.LowerVanLogAsset2;
      cn.UpperLogAsset2 = cn.UpperVanLogAsset2;
      cn.K = K;
      cn.NoAssetSteps2 = (long)gridstep;
      cn.NoTimeSteps2 = (long)Math.Max(25.0, cn.T / timestep);
      cn.S = Math.Log(spot);
      cn.Anchor = Math.Log(spot);
      cn.PayStyle = (long)MapPayStyle();

      if (cn.t > 0.0)
      {
        ComputeGridWidth(cn.t, out u, out l);
        cn.LowerVanLogAsset1 = Math.Log(l);
        cn.UpperVanLogAsset1 = Math.Log(u);
        cn.LowerLogAsset1 = cn.LowerVanLogAsset1;
        cn.UpperLogAsset1 = cn.UpperVanLogAsset1;
        cn.StepFlag = (bLS) ? 2 : 1;
      }

      if (cn.t != 0)
      {
        //partial, map the boundary
        cn.NoAssetSteps1 = (long)gridstep;
        cn.NoTimeSteps1 = (long)Math.Max(25.0, cn.t / timestep);
        cn.NoTimeSteps1 = (long)Math.Max(25.0, (cn.T - cn.t) / timestep);
        if (bLS)
        {
          cn.UpperLogAsset2 = (ub > 0.0) ? Math.Log(ub) : cn.UpperVanLogAsset2;
          cn.LowerLogAsset2 = (lb > 0.0) ? Math.Log(lb) : cn.LowerVanLogAsset2;
        }
        else
        {
          cn.UpperLogAsset1 = (ub > 0.0) ? Math.Log(ub) : cn.UpperVanLogAsset1;
          cn.LowerLogAsset1 = (lb > 0.0) ? Math.Log(lb) : cn.LowerVanLogAsset1;
        }
      }
      else
      {
        //continuous barrier
        cn.UpperLogAsset2 = (ub > 0.0) ? Math.Log(ub) : cn.UpperVanLogAsset2;
        cn.LowerLogAsset2 = (lb > 0.0) ? Math.Log(lb) : cn.LowerVanLogAsset2;
      }
      return cn;
    }

    private void ComputeGridWidth(double tt,
                                  out double u,
                                  out double l)
    {
      u = l = 0;
      if (tt <= tenors[0])
      {
        u = 2.0 * k25c[0]; l = 0.5 * k25p[0];
      }
      else if (tt > tenors[tenors.Length - 1])
      {
        u = 2.0 * k25c[tenors.Length - 1]; l = 0.5 * k25p[tenors.Length - 1];
      }
      else
      {
        for (int idx = 0; idx <= tenors.Length; idx++)
        {
          if ((tt > tenors[idx]) && (tt <= tenors[idx + 1]))
          {
            u = 2.0 * k25c[idx + 1]; l = 0.5 * k25p[idx + 1];
            break;
          }
        }
      }
    }

    private OptionCode MapPayStyle()
    {
      OptionCode result = OptionCode.ZZZZ;
      if (tb != 0 || te < T)
        result = MapPartial();
      else if (bVan)
        result = MapVanilla();
      else
        result = MapDigital();
      return result;
    }

    private OptionCode MapPartial()
    {
      OptionCode result = OptionCode.ZZZZ;

      if (bKI)
      {
        if (bCall)
        {
          result = (ub == 0.0) ? OptionCode.SDIC : OptionCode.SUIC;
        }
        else
        {
          result = (ub == 0.0) ? OptionCode.SDIP : OptionCode.SUIP;
        }
      }
      else
      {
        if (bCall)
        {
          result = (ub == 0.0) ? OptionCode.SDOC : OptionCode.SUOC;
        }
        else
        {
          result = (ub == 0.0) ? OptionCode.SDOP : OptionCode.SUOP;
        }
      }
      return result;
    }

    private OptionCode MapVanilla()
    {
      OptionCode result = bCall ? OptionCode.VEUC : OptionCode.VEUP;

      if (bKI)
      {
        if ((ub == 0.0) && (lb > 0.0) && (lb < spot))
        {
          result = (bCall) ? OptionCode.BDIC : OptionCode.BDIP;
        }
        else if ((ub > 0.0) && (lb == 0.0) && (ub > spot))
        {
          result = (bCall) ? OptionCode.BUIC : OptionCode.BUIP;
        }
        else if ((ub > lb) && (lb > 0.0) && (ub > spot) && (lb < spot))
        {
          result = (bCall) ? OptionCode.B2IC : OptionCode.B2IP;
        }
      }
      else
      {
        if ((ub == 0.0) && (lb > 0.0) && (lb < spot))
        {
          result = (bCall) ? OptionCode.BDOC : OptionCode.BDOP;
        }
        else if ((ub > 0.0) && (lb == 0.0) && (ub > spot))
        {
          result = (bCall) ? OptionCode.BUOC : OptionCode.BUOP;
        }
        else if ((ub > lb) && (lb > 0.0) && (ub > spot) && (lb < spot))
        {
          result = (bCall) ? OptionCode.B2OC : OptionCode.B2OP;
        }
      }
      return result;
    }

    private OptionCode MapDigital()
    {
      OptionCode result = OptionCode.ZZZZ;

      if (bDAE)
      {
        // digital at expiry
        if (bCash)
        {
          result = (bCall) ? OptionCode.EDNC : OptionCode.EDNP;
        }
        else  //aset payout digital at expiry
        {
          result = (bCall) ? OptionCode.EDAC : OptionCode.EDAP;
        }
      }
      else if (bKI)
      {
        // digital One-Touch
        if ((ub == 0.0) && (lb > 0.0) && (lb < spot))
        {
          result = (bCash) ? OptionCode.OTNP : OptionCode.OTAP;
        }
        else if ((ub > 0.0) && (lb == 0.0) && (ub > spot))
        {
          result = (bCash) ? OptionCode.OTNC : OptionCode.OTAC;
        }
        else if ((ub > lb) && (lb > 0.0) && (lb < spot))
        {
          result = (bCash) ? OptionCode.RINP : OptionCode.RIAP;
        }
      }
      else
      {
        // digital No-Touch
        if ((ub == 0.0) && (lb > 0.0) && (lb < spot))
        {
          result = (bCash) ? OptionCode.NTNP : OptionCode.NTAP;
        }
        else if ((ub > 0.0) && (lb == 0.0) && (ub > spot))
        {
          result = (bCash) ? OptionCode.NTNC : OptionCode.NTAC;
        }
        else if ((ub > lb) && (lb > 0.0) && (lb < spot) && (ub > spot))
        {
          result = (bCash) ? OptionCode.RBNP : OptionCode.RBAP;
        }
      }
      return result;
    }
  }
}
