using System;
using System.Net;

namespace nab.QR.PricingModelNab1
{
  public class CalcStrike
  {
    public static double Calculate(nab.QR.Xml.PricingResult result)
    {
      string asset = result.Find("", "Asset", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
      double rate = result.Find(asset, "Rate", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
      if (rate == 0)
      {
        string numeraire = result.Find("", "Numeraire", nab.QR.Xml.ValueUnit.Absolute).Value.As<string>();
        double price = result.Find(numeraire, "Price", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        double assetAmount = result.Find(asset, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        double numeraireAmount = result.Find(numeraire, "Amount", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        double fwd = result.Find(asset, "Forward", nab.QR.Xml.ValueUnit.Absolute).Value.As<double>();
        if (assetAmount == 0) assetAmount = numeraireAmount / fwd;
        if (numeraireAmount == 0) numeraireAmount = assetAmount * fwd;
        rate = assetAmount / numeraireAmount;
      }
      return rate;
    }
  }
}
