using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcValueNumeraire : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double value = CalculateValue(result);
      result.Add(new PricingValue { Name = "Value", Asset = numeraire, Value = new PropertyValue(value), Unit = ValueUnit.Absolute});
    }

    public static double CalculateValue(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double price = result.Find(numeraire, "Price", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double assetAmount = result.Find(asset, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      if (assetAmount == 0)
      {
        double numeraireAmount = result.Find(numeraire, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        assetAmount = numeraireAmount * CalcStrike.Calculate(result);
      }
      return assetAmount * price;
    }
  }
}
