using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcValueAssetPercent : ICalculation
  {
    public void Calculate(nab.QR.Xml.PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double price = result.Find(numeraire, "Price", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double value = price / spot;
      result.Add(new PricingValue { Name = "Value", Asset = asset, Value = new PropertyValue(value), Unit = ValueUnit.Percent});
    }
  }
}