using System;
using nab.QR.PricingModel;
using nab.QR.Xml;

namespace nab.QR.PricingModelNab1
{
  public class CalcPhiNumeraire : ICalculation
  {
    public void Calculate(PricingResult result)
    {
      string asset = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      result.Add(new PricingValue { Name = "PhiPosition", Asset = asset, Value = new PropertyValue(CalculateValue(result)), Unit = ValueUnit.Absolute });
    }

    public static double CalculateValue(PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double phi = result.Find(numeraire, "Phi", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      double assetAmount = result.Find(asset, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      if (assetAmount == 0)
      {
        double numeraireAmount = result.Find(numeraire, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        assetAmount = numeraireAmount * CalcStrike.Calculate(result);
      }
      return phi * assetAmount;
    }
  }
}


