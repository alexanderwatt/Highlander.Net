using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcPLDeltaNumerairePercent : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double value = CalcDeltaAssetPercent.CalculateValue(result);
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      value = -1 * value * spot / CalcStrike.Calculate(result);
      result.Add(new PricingValue { Name = "PLDelta", Asset = numeraire, Value = new PropertyValue(value), Unit = ValueUnit.Percent });
    }
  }  
}


