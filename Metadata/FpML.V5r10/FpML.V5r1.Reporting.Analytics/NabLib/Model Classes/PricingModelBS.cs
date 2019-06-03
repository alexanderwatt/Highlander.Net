using System;
using System.Net;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  internal class PricingModelBS
  {
    public PricingResult Price(OptionProduct product, OptionMarket market)
    {
      PricingResult result = new PricingResult();
      result.ValueDate = product.SpotDate;
      Black black = ProductToBlack(product, market);

      double price = black.Price();

      OptionGreeks gks = BlackGreeks.Compute(black);

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

    public static Black ProductToBlack(OptionProduct product, OptionMarket market)
    {
      Black result = null;
      if (OptionMappings.IsPhysical(product.Code))
        result = PhysicalToBlack(product, market);
      else
        result = OptionToBlack(product, market);

      return result;
    }

    public static Black PhysicalToBlack(OptionProduct product, OptionMarket market)
    {
      Black blk = new Black();
      blk.PayStyle = (int)product.Code;

      PositionType spreadPosition = product.SpreadPricing ? product.PositionType : PositionType.None;
      blk.Spot = product.Spot[spreadPosition];
      blk.Spot += market.SpotShift;

      blk.K = product.Rate;
      blk.T = (product.SettlementDate - product.SpotDate).Days / 365.0;

      blk.ForRate = product.LeftCC[spreadPosition.Opposite()];
      blk.DomRate = product.RightCC[spreadPosition];
      blk.Fwd = blk.Spot * Math.Exp((blk.DomRate - blk.ForRate) * blk.T);

      return blk;
    }

    public static Black OptionToBlack(OptionProduct product, OptionMarket market)
    {
      Black blk = new Black();
      blk.PayStyle = (int)product.Code;

      PositionType spreadPosition = product.SpreadPricing ? product.PositionType : PositionType.None;
      blk.Spot = product.Spot[spreadPosition];
      blk.Spot += market.SpotShift;

      blk.K = product.Rate;
      blk.T = (product.ExpiryDate - product.HorizonDate).Days / 365.0;

      double tau = (product.SettlementDate - product.SpotDate).Days / 365.0;
      double compression = blk.T == 0 ?  1 : tau / blk.T;

      blk.ForRate = product.LeftCC[spreadPosition.Opposite()] * compression;
      blk.DomRate = product.RightCC[spreadPosition] * compression;
      blk.Fwd = blk.Spot * Math.Exp((blk.DomRate - blk.ForRate) * blk.T);
      blk.Sig = product.Volatility[spreadPosition];
      blk.Sig += market.VolatilityShift;

      blk.b = blk.Upper = blk.Lower = blk.StepFlag = 0;

      if (product.BarrierType != null)
      {
        if (product.BarrierUpperLevel != 0 && product.BarrierLowerLevel != 0)
        {
          blk.Upper = product.BarrierUpperLevel;
          blk.Lower = product.BarrierLowerLevel;
        }
        else
        {
          blk.b = Math.Max(product.BarrierUpperLevel, product.BarrierLowerLevel);
        }

        // 1 - early finish, 2 - late start, does not support windows
        if (product.BarrierStartDate > product.HorizonDate)
        {
          blk.StepFlag = 2;
          blk.t = (product.BarrierStartDate - product.HorizonDate).Days / 365.0;
        }
        else if (product.BarrierEndDate < product.ExpiryDate)
        {
          blk.t = (product.ExpiryDate - product.BarrierEndDate).Days / 365.0;
          blk.StepFlag = 1;
        }
        else
        {
          blk.StepFlag = 0;
          blk.t = 0;
        }
      }

      blk.m = blk.CPCflag = 0;
      if (product.PremiumMarginAsset != null)
      {
        blk.CPCflag = (product.FixedAsset == product.LeftAsset) ? 1 : 2;
        blk.m = product.PremiumMarginPoints / market.SwapFactor; 
      }

      return blk;
    }
  }
}
