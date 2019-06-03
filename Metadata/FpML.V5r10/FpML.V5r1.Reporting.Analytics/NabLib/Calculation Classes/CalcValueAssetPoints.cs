using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcValueAssetPoints : ICalculation
  {
    public void Calculate(nab.QR.Xml.PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double price = result.Find(numeraire, "Price", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double swap = result.Find(asset, "SwapFactor", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double value = price * swap / spot / CalcStrike.Calculate(result);
      result.Add(new PricingValue { Name = "Value", Asset = asset, Value = new PropertyValue(value), Unit = ValueUnit.Points });
    }
  }
}