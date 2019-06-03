using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcRhoAsset : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      result.Add(new PricingValue { Name = "RhoPosition", Asset = asset, Value = new PropertyValue(CalculateValue(result)), Unit = ValueUnit.Absolute });
    }

    public static double CalculateValue(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double value = CalcRhoNumeraire.CalculateValue(result);
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      return value / spot;
    }
  }
}