using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcGammaAsset : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      result.Add(new PricingValue { Name = "GammaPosition", Asset = asset, Value = new PropertyValue(CalculateValue(result)), Unit = ValueUnit.Absolute });
    }

    public static double CalculateValue(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double gamma = result.Find(asset, "Gamma", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double assetAmount = result.Find(asset, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      if (assetAmount == 0)
      {
        string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
        double numeraireAmount = result.Find(numeraire, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        assetAmount = numeraireAmount * CalcStrike.Calculate(result);
      }
      return gamma * assetAmount;
    }
  }
}