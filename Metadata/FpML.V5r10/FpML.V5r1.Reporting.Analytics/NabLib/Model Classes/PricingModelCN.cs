using System;
using System.Net;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  internal class PricingModelCN
  {
    public PricingResult Price(OptionProduct product, OptionMarket market)
    {
      PricingResult result = new PricingResult();
      result.ValueDate = product.SpotDate;
      CN cn = ProductToCN(product, market);

      double price = cn.Price(market.Tenors, market.ATMs, market.Doms, market.Fors, market.P1,
                              market.P2, market.P3, market.P4, market.P5, market.P6, market.MR);

      OptionGreeks gks = null;
      if (market.IsBlackGreeks)
      {
        Black black = PricingModelBS.ProductToBlack(product, market);
        gks = BlackGreeks.Compute(black);
      }
      else
      {
        gks = CNGreeks.Compute(cn, market.Tenors, market.ATMs, market.Doms, market.Fors, market.P1,
                               market.P2, market.P3, market.P4, market.P5, market.P6, market.MR);
      }

      int sign = product.PositionType.Sign();
      result.Add(new PricingValue { Name = "Price", Asset = product.RightAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(price * sign) });
      result.Add(new PricingValue { Name = "Delta", Asset = product.LeftAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(gks.Delta * sign) });
      result.Add(new PricingValue { Name = "Gamma", Asset = product.LeftAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(gks.Gamma * sign) });
      result.Add(new PricingValue { Name = "Vega", Asset = product.RightAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(gks.Vega * sign) });
      result.Add(new PricingValue { Name = "Theta", Asset = product.RightAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(gks.Theta * sign) });
      result.Add(new PricingValue { Name = "Rho", Asset = product.RightAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(gks.Rho * sign) });
      result.Add(new PricingValue { Name = "Phi", Asset = product.RightAsset, Unit = ValueUnit.Absolute, Value = new PropertyValue(gks.Phi * sign) });
      return result;
    }

    private static CN ProductToCN(OptionProduct product, OptionMarket market)
    {
      CN cn = new CN();
      cn.PayStyle = (long) product.Code;

      cn.T = (product.ExpiryDate - product.HorizonDate).Days / 365.0;
      
      cn.K = product.Rate;
      cn.NoAssetSteps2 = (long)market.GridStep;
      cn.NoTimeSteps2 = (long)Math.Max(25.0, cn.T / market.TimeStep);
      cn.S = Math.Log(product.Spot[PositionType.None]);
      cn.Anchor = cn.S;

      //compute grid width for a vanilla
      double u, l;
      ComputeGridWidth(market, cn.T, out u, out l);
      cn.LowerVanLogAsset2 = Math.Log(l);
      cn.UpperVanLogAsset2 = Math.Log(u);
      cn.LowerLogAsset2 = cn.LowerVanLogAsset2;
      cn.UpperLogAsset2 = cn.UpperVanLogAsset2;

      if (product.BarrierType != null)
      {
        double ub = product.BarrierUpperLevel;
        double lb = product.BarrierLowerLevel;

        if (product.BarrierStartDate > product.HorizonDate
         && product.BarrierEndDate < product.ExpiryDate)
        {
          // windows barrier
          throw new Exception(Resource.ErrWindowsBarrierNotSupported);
        }
        else if (product.BarrierStartDate > product.HorizonDate)
        {
          // late start
          cn.t = (product.BarrierStartDate - product.HorizonDate).Days / 365.0;
          cn.StepFlag = 2;
        }
        else if(product.BarrierEndDate < product.ExpiryDate)
        {
          // early finish
          cn.t = (product.BarrierEndDate - product.HorizonDate).Days / 365.0;
          cn.StepFlag = 1;
        }
        else
        {
          cn.t = 0.0;
          cn.StepFlag = 0;
        }

        // recompute a vanilla expiring at t
        if (cn.StepFlag != 0)
        {
          ComputeGridWidth(market, cn.t, out u, out l);
          cn.LowerVanLogAsset1 = Math.Log(l);
          cn.UpperVanLogAsset1 = Math.Log(u);
          cn.LowerLogAsset1 = cn.LowerVanLogAsset1;
          cn.UpperLogAsset1 = cn.UpperVanLogAsset1;

          cn.NoAssetSteps1 = (long)market.GridStep;
          cn.NoTimeSteps1 = (long)Math.Max(25.0, cn.t / market.TimeStep);
          cn.NoTimeSteps1 = (long)Math.Max(25.0, (cn.T - cn.t) / market.TimeStep);

          if (cn.StepFlag == 2)
          {
            // late start barrier in period 2
            cn.UpperLogAsset2 = (ub > 0.0) ? Math.Log(ub) : cn.UpperVanLogAsset2;
            cn.LowerLogAsset2 = (lb > 0.0) ? Math.Log(lb) : cn.LowerVanLogAsset2;
          }
          else if (cn.StepFlag == 1)
          {
            // early finish barrier in period 1
            cn.UpperLogAsset1 = (ub > 0.0) ? Math.Log(ub) : cn.UpperVanLogAsset1;
            cn.LowerLogAsset1 = (lb > 0.0) ? Math.Log(lb) : cn.LowerVanLogAsset1;
          }
        }
        else
        {
          // continuous barrier
          cn.UpperLogAsset2 = (ub > 0.0) ? Math.Log(ub) : cn.UpperVanLogAsset2;
          cn.LowerLogAsset2 = (lb > 0.0) ? Math.Log(lb) : cn.LowerVanLogAsset2;
        }
      }

      cn.margin = cn.CPCflag = 0;
      if (product.PremiumMarginAsset != null)
      {
        cn.CPCflag = (product.FixedAsset == product.LeftAsset) ? 1 : 2;
        cn.margin = product.PremiumMarginPoints / market.SwapFactor;
      }
      return cn;
    }

    private static void ComputeGridWidth(OptionMarket market,
                                         double tt,
                                         out double u,
                                         out double l)
    {
      double[] tenors = market.Tenors;
      double[] k25c = market.K25c;
      double[] k25p = market.K25p;

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
  }
}
