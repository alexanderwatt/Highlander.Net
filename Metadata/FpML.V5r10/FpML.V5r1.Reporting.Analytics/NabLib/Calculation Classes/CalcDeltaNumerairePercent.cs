using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcDeltaNumerairePercent : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string asset = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      result.Add(new PricingValue { Name = "DeltaPosition", Asset = asset, Value = new PropertyValue(CalculateValue(result)), Unit = ValueUnit.Percent });
    }

    public static double CalculateValue(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double delta = result.Find(asset, "Delta", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();

      return -1 * delta * spot / CalcStrike.Calculate(result);
    }
  }
}
