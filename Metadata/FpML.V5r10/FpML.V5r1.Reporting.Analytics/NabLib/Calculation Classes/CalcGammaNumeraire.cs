using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcGammaNumeraire : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double value = CalcGammaAsset.CalculateValue(result);
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      value = value * spot * -1;
      result.Add(new PricingValue { Name = "GammaPosition", Asset = numeraire, Value = new PropertyValue(value), Unit = ValueUnit.Absolute });
    }
  }
}

