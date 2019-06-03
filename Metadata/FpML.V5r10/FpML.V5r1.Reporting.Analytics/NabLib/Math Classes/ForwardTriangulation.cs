using System;
using System.Collections.Generic;

namespace National.QRSC.Analytics.NabLib
{
  ///<summary>
  ///</summary>
  public class ForwardTriangulation
  {
    public DateTime SpotDate { get; set; }
    public DateTime SettlemenDate { get; set; }
    public int DayCountAsset { get; set; }
    public int DayCountNum { get; set; }
    public double SwapFactor { get; set; }

    private Spread _depoAsset = new Spread();
    private Spread _depoNum = new Spread();
    private Spread _spot = new Spread();
    private Spread _swap = new Spread();
    private Queue<string> _changedNames = new Queue<string>();

    public delegate void SwapDerivedHandler(Spread value);
    public event SwapDerivedHandler SwapDerived;

    public delegate void DepoAssetDerivedHandler(Spread value);
    public event DepoAssetDerivedHandler DepoAssetDerived;

    public delegate void DepoNumDerivedHandler(Spread value);
    public event DepoNumDerivedHandler DepoNumDerived;

    private bool _enabled = false;
    public bool Enabled 
    {
      get
      {
        return _enabled;
      }
      set
      {
        if (value)
        {
          _changedNames.Clear();
          Enqueue("DepoAsset");
          Enqueue("DepoNum");
        }
        _enabled = value;
      }
    }

    ///<summary>
    ///</summary>
    public ForwardTriangulation()
    {
    }

    ///<summary>
    ///</summary>
    public Spread DepoAsset
    {
      get {return _depoAsset;}
      set
      {
        _depoAsset = value;
        Enqueue("DepoAsset");
      }
    }
    ///<summary>
    ///</summary>
    public double DepoAssetBid
    {
      get { return _depoAsset.Bid; }
      set { _depoAsset.Bid = value; Enqueue("DepoAsset"); }
    }
    ///<summary>
    ///</summary>
    public double DepoAssetAsk
    {
      get { return _depoAsset.Ask; }
      set { _depoAsset.Ask = value; Enqueue("DepoAsset"); }
    }

    ///<summary>
    ///</summary>
    public Spread DepoNum
    {
      get { return _depoNum; }
      set
      {
        _depoNum = value;
        Enqueue("DepoNum");
      }
    }
    ///<summary>
    ///</summary>
    public double DepoNumBid
    {
      get { return _depoNum.Bid; }
      set { _depoNum.Bid = value; Enqueue("DepoNum"); }
    }
    ///<summary>
    ///</summary>
    public double DepoNumAsk
    {
      get { return _depoNum.Ask; }
      set { _depoNum.Ask = value; Enqueue("DepoNum"); }
    }

    ///<summary>
    ///</summary>
    public Spread Spot
    {
      get { return _spot; }
      set
      {
        _spot = value;
        Enqueue("Spot");  /*Triangulate();*/
      }
    }
    ///<summary>
    ///</summary>
    public double SpotBid
    {
      get { return _spot.Bid; }
      set { _spot.Bid = value; Enqueue("Spot"); }
    }
    ///<summary>
    ///</summary>
    public double SpotAsk
    {
      get { return _spot.Ask; }
      set { _spot.Ask = value; Enqueue("Spot"); }
    }

    ///<summary>
    ///</summary>
    public Spread Swap
    {
      get { return _swap; }
      set
      {
        _swap = value;
        Enqueue("Swap");
      }
    }
    ///<summary>
    ///</summary>
    public double SwapBid
    {
      get { return _swap.Bid; }
      set { _swap.Bid = value; Enqueue("Swap"); }
    }
    ///<summary>
    ///</summary>
    public double SwapAsk
    {
      get { return _swap.Ask; }
      set { _swap.Ask = value; Enqueue("Swap"); }
    }

    ///<summary>
    ///</summary>
    public void Triangulate()
    {
      if (Enabled && _changedNames.Count >= 2)
      {
        if (!_changedNames.Contains("Swap"))
          TriangulateSwap();
        else if (!_changedNames.Contains("DepoAsset"))
          TriangulateDepoAsset();
        else if (!_changedNames.Contains("DepoNum"))
          TriangulateDepoNum();
      }
    }

    ///<summary>
    ///</summary>
    public void TriangulateSwap()
    {
      int days = (SettlemenDate - SpotDate).Days;
      Spread assetDf = CurveInterpolationDF.ZeroToDF(DepoAsset, days, DayCountAsset);
      Spread numDf = CurveInterpolationDF.ZeroToDF(DepoNum, days, DayCountNum);

      Spread forward = Spot * assetDf.Swap() / numDf;
      _swap = (forward - Spot) * SwapFactor;

      if (SwapDerived != null)
        SwapDerived(_swap);
    }

    ///<summary>
    ///</summary>
    public void TriangulateDepoAsset()
    {
      Spread ccAsset = new Spread();

      int days = (SettlemenDate - SpotDate).Days;
      double dt = days / 365.0;
      Spread numDf = CurveInterpolationDF.ZeroToDF(DepoNum, days, DayCountNum);

      double num = (Spot.Bid + Swap.Bid / SwapFactor) * numDf.Bid;
      double den = Spot.Bid;
      ccAsset.Ask = -Math.Log(num / den) / dt;

      num = (Spot.Ask + Swap.Ask / SwapFactor) * numDf.Ask;
      den = Spot.Ask;
      ccAsset.Bid = -Math.Log(num / den) / dt;

      Spread assetDF = CurveInterpolationDF.CCToDF(ccAsset, days);
      _depoAsset = CurveInterpolationDF.DFToZero(assetDF, days, DayCountNum);

      if (DepoAssetDerived != null)
        DepoAssetDerived(_depoAsset);
    }

    public void TriangulateDepoNum()
    {
      Spread ccNum = new Spread();

      int days = (SettlemenDate - SpotDate).Days;
      double dt = days / 365.0;
      Spread assetDf = CurveInterpolationDF.ZeroToDF(DepoAsset, days, DayCountAsset);

      double num = Spot.Bid + Swap.Bid / SwapFactor;
      double den = Spot.Bid * assetDf.Ask;
      ccNum.Bid = Math.Log(num / den) / dt;

      num = Spot.Ask + Swap.Ask / SwapFactor;
      den = Spot.Ask * assetDf.Bid;
      ccNum.Ask = Math.Log(num / den) / dt;
      Spread numDF = CurveInterpolationDF.CCToDF(ccNum, days);
      _depoNum = CurveInterpolationDF.DFToZero(numDF, days, DayCountNum);

      if (DepoNumDerived != null)
        DepoNumDerived(_depoNum);
    }

    public void TriangulateSpot()
    {
      int days = (SettlemenDate - SpotDate).Days;
      Spread assetDf = CurveInterpolationDF.ZeroToDF(DepoAsset, days, DayCountAsset);
      Spread numDf = CurveInterpolationDF.ZeroToDF(DepoNum, days, DayCountNum);
      _spot = Swap / (assetDf / numDf - 1);
    }

    private void Enqueue(string name)
    {
      if (!_changedNames.Contains(name) || _changedNames.Peek() == name)
        _changedNames.Enqueue(name);

      if (_changedNames.Count > 2)
        _changedNames.Dequeue();
      Triangulate();
    }
  }
}
