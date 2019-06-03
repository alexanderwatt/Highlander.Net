using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcValueAsset : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double spot = result.Find(asset, "Spot", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double value = CalcValueNumeraire.CalculateValue(result) / spot;
      result.Add(new PricingValue { Name = "Value", Asset = asset, Value = new PropertyValue(value), Unit = ValueUnit.Absolute });
    }
  }
}
